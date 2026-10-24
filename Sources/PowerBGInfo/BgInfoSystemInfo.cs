using System.Globalization;

namespace PowerBGInfo;

/// <summary>Read-only facts about the local machine. Fields that could not be read hold <see cref="SystemInfoProvider.Unknown"/> or null.</summary>
/// <remarks>Scripts can replace <c>Get-BGInfoSystemInfo</c> with their own function that returns an object of the same shape.</remarks>
public sealed class BgInfoSystemInfo {
    /// <summary>Computer name.</summary>
    public string ComputerName { get; set; } = string.Empty;
    /// <summary>User running the script.</summary>
    public string UserName { get; set; } = string.Empty;
    /// <summary>
    /// User signed in to the Windows session this process runs in, as DOMAIN\user. In session 0 (a SYSTEM task or a service) it is the
    /// user of the active console session, so it differs from <see cref="UserName"/> there; Unknown when nobody is signed in.
    /// </summary>
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
    /// <summary>
    /// Availability by snapshot property name. CollectedAt describes this snapshot;
    /// it does not claim that cached hardware metadata was freshly probed.
    /// </summary>
    public IReadOnlyDictionary<string, BgInfoFactState> FactStates { get; internal set; } =
        new System.Collections.ObjectModel.ReadOnlyDictionary<string, BgInfoFactState>(new Dictionary<string, BgInfoFactState>());
}

/// <summary>One fixed disk.</summary>
public sealed class BgInfoDiskInfo {
    /// <summary>Drive letter with colon on Windows, or an absolute mount path on Linux and macOS.</summary>
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
