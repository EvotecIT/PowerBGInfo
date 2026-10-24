using System.Globalization;

namespace PowerBGInfo;

internal sealed partial class WindowsSystemInfoProvider {
#if NET472
    // The 32-bit Environment.TickCount wraps after49.7 days. This native clock is
    // available on supported Windows hosts and includes the full boot duration.
    [System.Runtime.InteropServices.DllImport("kernel32.dll", ExactSpelling = true)]
    private static extern ulong GetTickCount64();
#endif
    /// <summary>Text used when a fact cannot be read.</summary>
    private const string Unknown = SystemInfoProvider.Unknown;

    private static readonly TimeSpan QueryTimeout = TimeSpan.FromSeconds(5);
    private static (string Id, DateTime? Date) GetLastPatch() => LoadLastPatch();
    private static (string Status, TimeSpan? SignatureAge) GetDefender() => LoadDefender();
    private static string GetBitLockerStatus() => LoadBitLocker();
    private static string GetLoggedOnUser() => LoadLoggedOnUser();

    /// <summary>Collects every fact. Each probe fails on its own; the snapshot is always returned.</summary>
    public BgInfoSystemInfo GetSnapshot() {
        var osInfo = OsInfoCache.Value;
        var uptime = GetUptime(osInfo);
        var lastBoot = osInfo.LastBootUpTime;
        var patch = GetLastPatch();
        var defender = GetDefender();
        var snapshot = new BgInfoSystemInfo {
            ComputerName = Environment.MachineName,
            UserName = Environment.UserName,
            LoggedOnUser = GetLoggedOnUser(),
            Domain = MachineDomainCache.Value,
            OSName = SystemInfoProvider.OrUnknown(osInfo.Name),
            OSVersion = SystemInfoProvider.OrUnknown(osInfo.Version),
            OSBuild = SystemInfoProvider.OrUnknown(osInfo.Build),
            LastBootTime = lastBoot,
            Uptime = uptime,
            UptimeText = SystemInfoProvider.FormatUptime(uptime),
            IPv4Address = SystemInfoProvider.OrUnknown(LoadNetworkInfo().IPv4),
            Disks = GetDisks(),
            LastPatchId = patch.Id,
            LastPatchDate = patch.Date,
            DefenderStatus = defender.Status,
            DefenderSignatureAge = defender.SignatureAge,
            BitLockerStatus = GetBitLockerStatus()
        };
        snapshot.FactStates = BgInfoFactStates.FromSnapshot(snapshot, windowsFacts: true);
        return snapshot;
    }

    private static string LoadMachineDomain() {
        if (IsWindows()) {
            try {
                foreach (var computer in Query(@"root\cimv2", "SELECT Domain FROM Win32_ComputerSystem")) {
                    var domain = computer["Domain"]?.ToString();
                    if (!string.IsNullOrWhiteSpace(domain)) return SystemInfoProvider.ResolveMachineDomain(domain);
                }
            } catch {
                // Machine membership is unavailable; the process account is not a reliable substitute.
            }
        }

        return Unknown;
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
                var installed = SystemInfoProvider.ParsePatchDate(obj["InstalledOn"]?.ToString());
                if (!installed.HasValue || (date.HasValue && installed.Value <= date.Value)) continue;
                date = installed;
                id = obj["HotFixID"]?.ToString();
            }
            return (SystemInfoProvider.OrUnknown(id), date);
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
        // The user of the session this process renders for, not only the console user: on a session host the
        // console is usually empty or someone else. Win32_ComputerSystem.UserName stays as the last resort.
        return SessionUser.Resolve(LoadConsoleUserFromWmi);
    }

    private static string? LoadConsoleUserFromWmi() {
        foreach (var obj in Query(@"root\cimv2", "SELECT UserName FROM Win32_ComputerSystem")) {
            return obj["UserName"]?.ToString();
        }
        return null;
    }


}
