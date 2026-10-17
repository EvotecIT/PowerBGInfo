using System.Globalization;
using System.Management;

namespace PowerBGInfo;

/// <summary>Read-only facts about the local machine. Fields that could not be read hold <see cref="SystemInfoProvider.Unknown"/> or null.</summary>
/// <remarks>Scripts can replace <c>Get-BGInfoSystemInfo</c> with their own function that returns an object of the same shape.</remarks>
public sealed class BgInfoSystemInfo {
    /// <summary>Computer name.</summary>
    public string ComputerName { get; set; } = string.Empty;
    /// <summary>User running the script.</summary>
    public string UserName { get; set; } = string.Empty;
    /// <summary>User signed in at the console, which differs from <see cref="UserName"/> when the script runs as SYSTEM.</summary>
    public string LoggedOnUser { get; set; } = SystemInfoProvider.Unknown;
    /// <summary>DNS domain or workgroup.</summary>
    public string Domain { get; set; } = SystemInfoProvider.Unknown;
    /// <summary>Operating system name.</summary>
    public string OSName { get; set; } = SystemInfoProvider.Unknown;
    /// <summary>Operating system version.</summary>
    public string OSVersion { get; set; } = SystemInfoProvider.Unknown;
    /// <summary>Operating system build number.</summary>
    public string OSBuild { get; set; } = SystemInfoProvider.Unknown;
    /// <summary>Last boot time, local.</summary>
    public DateTime? LastBootTime { get; set; }
    /// <summary>Time since the last boot.</summary>
    public TimeSpan? Uptime { get; set; }
    /// <summary>Uptime as short text, for example 41 days 6 h.</summary>
    public string UptimeText { get; set; } = SystemInfoProvider.Unknown;
    /// <summary>IPv4 address of the adapter with a default gateway.</summary>
    public string IPv4Address { get; set; } = SystemInfoProvider.Unknown;
    /// <summary>Fixed disks.</summary>
    public BgInfoDiskInfo[] Disks { get; set; } = Array.Empty<BgInfoDiskInfo>();
    /// <summary>Most recently installed update identifier, for example KB5044284.</summary>
    public string LastPatchId { get; set; } = SystemInfoProvider.Unknown;
    /// <summary>Install date of the most recent update.</summary>
    public DateTime? LastPatchDate { get; set; }
    /// <summary>Microsoft Defender state: Protected, Disabled or Unknown.</summary>
    public string DefenderStatus { get; set; } = SystemInfoProvider.Unknown;
    /// <summary>Age of the Defender signatures.</summary>
    public TimeSpan? DefenderSignatureAge { get; set; }
    /// <summary>BitLocker state of the system drive: On, Off, or Unknown (reading it usually needs elevation).</summary>
    public string BitLockerStatus { get; set; } = SystemInfoProvider.Unknown;
    /// <summary>When the snapshot was taken.</summary>
    public DateTime CollectedAt { get; set; } = DateTime.Now;
}

/// <summary>One fixed disk.</summary>
public sealed class BgInfoDiskInfo {
    /// <summary>Drive letter with colon.</summary>
    public string Drive { get; set; } = string.Empty;
    /// <summary>Volume label.</summary>
    public string Label { get; set; } = string.Empty;
    /// <summary>Size in gigabytes.</summary>
    public double SizeGB { get; set; }
    /// <summary>Free space in gigabytes.</summary>
    public double FreeGB { get; set; }
    /// <summary>Used space in percent.</summary>
    public double UsedPercent { get; set; }
}

public static partial class SystemInfoProvider {
    /// <summary>Text used when a fact cannot be read.</summary>
    public const string Unknown = "Unknown";

    private static readonly TimeSpan QueryTimeout = TimeSpan.FromSeconds(5);
    private static (string Id, DateTime? Date) GetLastPatch() => LoadLastPatch();
    private static (string Status, TimeSpan? SignatureAge) GetDefender() => LoadDefender();
    private static string GetBitLockerStatus() => LoadBitLocker();
    private static string GetLoggedOnUser() => LoadLoggedOnUser();

    /// <summary>Collects every fact. Each probe fails on its own; the snapshot is always returned.</summary>
    public static BgInfoSystemInfo GetSnapshot() {
        var uptime = GetUptime();
        var lastBoot = OsInfoCache.Value.LastBootUpTime;
        var patch = GetLastPatch();
        var defender = GetDefender();
        return new BgInfoSystemInfo {
            ComputerName = Environment.MachineName,
            UserName = Environment.UserName,
            LoggedOnUser = GetLoggedOnUser(),
            Domain = OrUnknown(Environment.GetEnvironmentVariable("USERDNSDOMAIN") ?? Environment.UserDomainName),
            OSName = OrUnknown(OsInfoCache.Value.Name),
            OSVersion = OrUnknown(OsInfoCache.Value.Version),
            OSBuild = OrUnknown(OsInfoCache.Value.Build),
            LastBootTime = lastBoot,
            Uptime = uptime,
            UptimeText = FormatUptime(uptime),
            IPv4Address = OrUnknown(LoadNetworkInfo().IPv4),
            Disks = GetDisks(),
            LastPatchId = patch.Id,
            LastPatchDate = patch.Date,
            DefenderStatus = defender.Status,
            DefenderSignatureAge = defender.SignatureAge,
            BitLockerStatus = GetBitLockerStatus()
        };
    }

    /// <summary>Formats an uptime as days and hours, or hours and minutes under a day.</summary>
    public static string FormatUptime(TimeSpan? uptime) {
        if (!uptime.HasValue || uptime.Value < TimeSpan.Zero) return Unknown;
        var value = uptime.Value;
        if (value.TotalDays >= 1) {
            var days = (int)value.TotalDays;
            return $"{days} {(days == 1 ? "day" : "days")} {value.Hours} h";
        }
        return value.TotalHours >= 1 ? $"{(int)value.TotalHours} h {value.Minutes} min" : $"{Math.Max(0, value.Minutes)} min";
    }

    /// <summary>Summarizes fixed disks as used percentages, for example C: 61 % · D: 88 %.</summary>
    public static string FormatDiskSummary(IEnumerable<BgInfoDiskInfo> disks) {
        var parts = disks.Select(disk => $"{disk.Drive} {Math.Round(disk.UsedPercent).ToString(CultureInfo.CurrentCulture)} %").ToArray();
        return parts.Length == 0 ? Unknown : string.Join(" · ", parts);
    }

    internal static string FormatPatch((string Id, DateTime? Date) patch) {
        if (!patch.Date.HasValue) return patch.Id;
        var date = patch.Date.Value.ToString("d", CultureInfo.CurrentCulture);
        return patch.Id == Unknown ? date : $"{patch.Id} · {date}";
    }

    /// <summary>Parses the locale-independent forms Win32_QuickFixEngineering uses for InstalledOn.</summary>
    internal static DateTime? ParsePatchDate(string? text) {
        if (string.IsNullOrWhiteSpace(text)) return null;
        var value = text!.Trim();
        var invariantFormats = new[] { "d-M-yyyy", "dd-MM-yyyy", "yyyyMMdd", "yyyy-MM-dd" };
        if (DateTime.TryParseExact(value, invariantFormats, CultureInfo.InvariantCulture, DateTimeStyles.None, out var exact)) return exact;
        if (DateTime.TryParseExact(value, "d", CultureInfo.CurrentCulture, DateTimeStyles.None, out var localized)) return localized;
        var legacyFormats = new[] { "M/d/yyyy", "MM/dd/yyyy" };
        if (DateTime.TryParseExact(value, legacyFormats, CultureInfo.InvariantCulture, DateTimeStyles.None, out exact)) return exact;
        // Some systems store a FILETIME as hexadecimal text.
        if (long.TryParse(value, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var fileTime) && fileTime > 0) {
            try { return DateTime.FromFileTime(fileTime); } catch (ArgumentOutOfRangeException) { return null; }
        }
        return null;
    }

    internal static BgInfoDiskInfo[] GetDisks() {
        if (!IsWindows()) return Array.Empty<BgInfoDiskInfo>();
        var disks = new List<BgInfoDiskInfo>();
        try {
            foreach (var obj in Query(@"root\cimv2", "SELECT DeviceID, VolumeName, Size, FreeSpace FROM Win32_LogicalDisk WHERE DriveType=3")) {
                var size = GetUInt64(obj["Size"]);
                var free = GetUInt64(obj["FreeSpace"]);
                if (!size.HasValue || size.Value == 0 || !free.HasValue) continue;
                const double gb = 1024d * 1024d * 1024d;
                disks.Add(new BgInfoDiskInfo {
                    Drive = obj["DeviceID"]?.ToString() ?? string.Empty,
                    Label = obj["VolumeName"]?.ToString() ?? string.Empty,
                    SizeGB = Math.Round(size.Value / gb, 1),
                    FreeGB = Math.Round(free.Value / gb, 1),
                    UsedPercent = Math.Round(ClampPercent((1d - (double)free.Value / size.Value) * 100d), 1)
                });
            }
        } catch {
            return disks.ToArray();
        }
        return disks.OrderBy(disk => disk.Drive, StringComparer.OrdinalIgnoreCase).ToArray();
    }

    private static (string Id, DateTime? Date) LoadLastPatch() {
        if (!IsWindows()) return (Unknown, null);
        try {
            string? id = null;
            DateTime? date = null;
            foreach (var obj in Query(@"root\cimv2", "SELECT HotFixID, InstalledOn FROM Win32_QuickFixEngineering")) {
                var installed = ParsePatchDate(obj["InstalledOn"]?.ToString());
                if (!installed.HasValue || (date.HasValue && installed.Value <= date.Value)) continue;
                date = installed;
                id = obj["HotFixID"]?.ToString();
            }
            return (OrUnknown(id), date);
        } catch {
            return (Unknown, null);
        }
    }

    private static (string Status, TimeSpan? SignatureAge) LoadDefender() {
        if (!IsWindows()) return (Unknown, null);
        try {
            foreach (var obj in Query(@"root\Microsoft\Windows\Defender", "SELECT AntivirusEnabled, RealTimeProtectionEnabled, AntivirusSignatureLastUpdated FROM MSFT_MpComputerStatus")) {
                var enabled = obj["AntivirusEnabled"] as bool? ?? false;
                var realTime = obj["RealTimeProtectionEnabled"] as bool? ?? false;
                var updated = GetDateTime(obj["AntivirusSignatureLastUpdated"]);
                TimeSpan? age = updated.HasValue ? DateTime.Now - updated.Value : null;
                return (enabled && realTime ? "Protected" : enabled ? "Real-time protection off" : "Disabled", age);
            }
        } catch {
            return (Unknown, null);
        }
        return (Unknown, null);
    }

    private static string LoadBitLocker() {
        if (!IsWindows()) return Unknown;
        var drive = Environment.GetEnvironmentVariable("SystemDrive") ?? "C:";
        try {
            foreach (var obj in Query(@"root\CIMV2\Security\MicrosoftVolumeEncryption", $"SELECT DriveLetter, ProtectionStatus FROM Win32_EncryptableVolume WHERE DriveLetter='{drive.Replace("'", string.Empty)}'")) {
                return GetInt32(obj["ProtectionStatus"]) switch { 1 => "On", 0 => "Off", _ => Unknown };
            }
        } catch {
            // Win32_EncryptableVolume normally requires elevation; the field stays Unknown.
            return Unknown;
        }
        return Unknown;
    }

    private static string LoadLoggedOnUser() {
        if (!IsWindows()) return Unknown;
        try {
            foreach (var obj in Query(@"root\cimv2", "SELECT UserName FROM Win32_ComputerSystem")) {
                return OrUnknown(obj["UserName"]?.ToString());
            }
        } catch {
            return Unknown;
        }
        return Unknown;
    }

    private static List<ManagementBaseObject> Query(string scope, string query) {
        var options = new System.Management.EnumerationOptions { Timeout = QueryTimeout, ReturnImmediately = true, Rewindable = false };
        using var searcher = new ManagementObjectSearcher(new ManagementScope(scope), new ObjectQuery(query), options);
        var results = new List<ManagementBaseObject>();
        foreach (var obj in searcher.Get()) results.Add(obj);
        return results;
    }

    private static string OrUnknown(string? value) => string.IsNullOrWhiteSpace(value) ? Unknown : value!.Trim();
}
