using System.Globalization;
using System.Management;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Runtime.InteropServices;

namespace PowerBGInfo;

/// <summary>
/// Provides built-in system values for BGInfo rendering.
/// </summary>
public static class SystemInfoProvider
{
    private static readonly Lazy<CpuInfo> CpuInfoCache = new(LoadCpuInfo, true);
    private static readonly Lazy<MemoryInfo> MemoryInfoCache = new(LoadMemoryInfo, true);
    private static readonly Lazy<BiosInfo> BiosInfoCache = new(LoadBiosInfo, true);
    private static readonly Lazy<OsInfo> OsInfoCache = new(LoadOsInfo, true);
    private static readonly Lazy<NetworkInfo> NetworkInfoCache = new(LoadNetworkInfo, true);
    private static readonly Lazy<string> FqdnCache = new(LoadFqdn, true);

    private static readonly Dictionary<string, Func<string>> Handlers = new(StringComparer.OrdinalIgnoreCase)
    {
        ["UserName"] = () => Environment.UserName,
        ["HostName"] = () => Environment.MachineName,
        ["FullUserName"] = () => $"{Environment.UserDomainName}\\{Environment.UserName}",
        ["CpuName"] = () => CpuInfoCache.Value.Name,
        ["CpuMaxClockSpeed"] = () => CpuInfoCache.Value.MaxClockSpeed?.ToString(CultureInfo.CurrentCulture) ?? string.Empty,
        ["CpuCores"] = () => CpuInfoCache.Value.Cores?.ToString(CultureInfo.CurrentCulture) ?? string.Empty,
        ["CpuLogicalCores"] = () => CpuInfoCache.Value.LogicalCores?.ToString(CultureInfo.CurrentCulture) ?? Environment.ProcessorCount.ToString(CultureInfo.CurrentCulture),
        ["RAMSize"] = () => MemoryInfoCache.Value.Size,
        ["RAMSpeed"] = () => MemoryInfoCache.Value.Speed,
        ["RAMPartNumber"] = () => MemoryInfoCache.Value.PartNumber,
        ["BiosVersion"] = () => BiosInfoCache.Value.Version,
        ["BiosManufacturer"] = () => BiosInfoCache.Value.Manufacturer,
        ["BiosReleaseDate"] = () => FormatDateTime(BiosInfoCache.Value.ReleaseDate),
        ["OSName"] = () => OsInfoCache.Value.Name,
        ["OSVersion"] = () => OsInfoCache.Value.Version,
        ["OSArchitecture"] = () => OsInfoCache.Value.Architecture,
        ["OSBuild"] = () => OsInfoCache.Value.Build,
        ["OSInstallDate"] = () => FormatDateTime(OsInfoCache.Value.InstallDate),
        ["OSLastBootUpTime"] = () => FormatDateTime(OsInfoCache.Value.LastBootUpTime),
        ["UserDNSDomain"] = () => Environment.GetEnvironmentVariable("USERDNSDOMAIN") ?? Environment.UserDomainName,
        ["FQDN"] = () => FqdnCache.Value,
        ["IPv4Address"] = () => NetworkInfoCache.Value.IPv4,
        ["IPv6Address"] = () => NetworkInfoCache.Value.IPv6
    };

    /// <summary>
    /// Resolves a built-in token to its system value.
    /// </summary>
    /// <param name="builtin">Built-in key name (for example UserName, HostName, OSVersion).</param>
    /// <returns>The resolved value, or an empty string when unknown.</returns>
    public static string GetValue(string builtin)
    {
        if (string.IsNullOrWhiteSpace(builtin))
        {
            return string.Empty;
        }

        var key = builtin.Trim();
        if (Handlers.TryGetValue(key, out var handler))
        {
            return handler();
        }

        return string.Empty;
    }

    /// <summary>
    /// Resolves numeric values for chart metrics.
    /// </summary>
    /// <param name="metric">Metric source.</param>
    /// <param name="argument">Optional metric argument (for example drive letter).</param>
    /// <param name="value">Resolved value.</param>
    /// <returns>True when the metric value could be resolved.</returns>
    public static bool TryGetNumericValue(BgInfoChartMetric metric, string? argument, out double value) {
        value = 0;
        switch (metric) {
            case BgInfoChartMetric.CpuPercent:
                return TryGetCpuPercent(out value);
            case BgInfoChartMetric.MemoryPercent:
                return TryGetMemoryPercent(out value);
            case BgInfoChartMetric.DiskFreePercent:
                return TryGetDiskFreePercent(argument, out value);
            case BgInfoChartMetric.DiskUsedPercent:
                if (TryGetDiskFreePercent(argument, out var freePercent)) {
                    value = ClampPercent(100d - freePercent);
                    return true;
                }
                return false;
            case BgInfoChartMetric.DiskFreeGb:
                return TryGetDiskFreeGb(argument, out value);
            case BgInfoChartMetric.UptimeHours:
                return TryGetUptimeHours(out value);
            case BgInfoChartMetric.UptimeDays:
                if (TryGetUptimeHours(out var hours)) {
                    value = hours / 24d;
                    return true;
                }
                return false;
            default:
                return false;
        }
    }

    private static bool IsWindows()
    {
        return RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
    }

    private static string FormatDateTime(DateTime? value)
    {
        return value.HasValue ? value.Value.ToString("G", CultureInfo.CurrentCulture) : string.Empty;
    }

    private static CpuInfo LoadCpuInfo()
    {
        var info = new CpuInfo();
        if (!IsWindows())
        {
            return info;
        }

        try
        {
            using var searcher = new ManagementObjectSearcher("SELECT Name, MaxClockSpeed, NumberOfCores, NumberOfLogicalProcessors, NumberOfEnabledCore FROM Win32_Processor");
            foreach (ManagementObject obj in searcher.Get())
            {
                var name = obj["Name"]?.ToString();
                var trimmedName = name?.Trim();
                if (!string.IsNullOrWhiteSpace(trimmedName) && string.IsNullOrWhiteSpace(info.Name))
                {
                    info.Name = trimmedName!;
                }

                var cores = GetInt32(obj["NumberOfCores"]) ?? GetInt32(obj["NumberOfEnabledCore"]);
                if (cores.HasValue)
                {
                    info.Cores = (info.Cores ?? 0) + cores.Value;
                }

                var logical = GetInt32(obj["NumberOfLogicalProcessors"]);
                if (logical.HasValue)
                {
                    info.LogicalCores = (info.LogicalCores ?? 0) + logical.Value;
                }

                var maxClock = GetInt32(obj["MaxClockSpeed"]);
                if (maxClock.HasValue)
                {
                    info.MaxClockSpeed = Math.Max(info.MaxClockSpeed ?? 0, maxClock.Value);
                }
            }
        }
        catch
        {
            return info;
        }

        return info;
    }

    private static MemoryInfo LoadMemoryInfo()
    {
        var info = new MemoryInfo();
        if (!IsWindows())
        {
            return info;
        }

        try
        {
            using var searcher = new ManagementObjectSearcher("SELECT Capacity, Speed, PartNumber FROM Win32_PhysicalMemory");
            foreach (ManagementObject obj in searcher.Get())
            {
                var capacity = GetUInt64(obj["Capacity"]);
                if (capacity.HasValue)
                {
                    info.Sizes.Add(capacity.Value);
                }

                var speed = GetInt32(obj["Speed"]);
                if (speed.HasValue)
                {
                    info.Speeds.Add(speed.Value);
                }

                var part = obj["PartNumber"]?.ToString();
                var trimmedPart = part?.Trim();
                if (!string.IsNullOrWhiteSpace(trimmedPart))
                {
                    info.PartNumbers.Add(trimmedPart!);
                }
            }
        }
        catch
        {
            return info;
        }

        info.Size = string.Join(" / ", info.Sizes.Select(size => $"{Math.Round(size / (1024d * 1024d * 1024d), 0).ToString("N0", CultureInfo.CurrentCulture)}GB"));
        info.Speed = string.Join(" / ", info.Speeds.Select(speed => $"{speed.ToString("N0", CultureInfo.CurrentCulture)}MHz"));
        info.PartNumber = string.Join(", ", info.PartNumbers);
        return info;
    }

    private static BiosInfo LoadBiosInfo()
    {
        var info = new BiosInfo();
        if (!IsWindows())
        {
            return info;
        }

        try
        {
            using var searcher = new ManagementObjectSearcher("SELECT Manufacturer, SMBIOSBIOSVersion, Version, ReleaseDate FROM Win32_BIOS");
            foreach (ManagementObject obj in searcher.Get())
            {
                info.Manufacturer = obj["Manufacturer"]?.ToString() ?? string.Empty;
                info.Version = obj["SMBIOSBIOSVersion"]?.ToString() ?? obj["Version"]?.ToString() ?? string.Empty;
                info.ReleaseDate = GetDateTime(obj["ReleaseDate"]);
                break;
            }
        }
        catch
        {
            return info;
        }

        return info;
    }

    private static OsInfo LoadOsInfo()
    {
        var info = new OsInfo();
        if (!IsWindows())
        {
            info.Architecture = RuntimeInformation.OSArchitecture.ToString();
            info.Version = RuntimeInformation.OSDescription;
            return info;
        }

        try
        {
            using var searcher = new ManagementObjectSearcher("SELECT Caption, Version, OSArchitecture, BuildNumber, InstallDate, LastBootUpTime FROM Win32_OperatingSystem");
            foreach (ManagementObject obj in searcher.Get())
            {
                info.Name = obj["Caption"]?.ToString() ?? string.Empty;
                info.Version = obj["Version"]?.ToString() ?? RuntimeInformation.OSDescription;
                info.Architecture = obj["OSArchitecture"]?.ToString() ?? RuntimeInformation.OSArchitecture.ToString();
                info.Build = obj["BuildNumber"]?.ToString() ?? string.Empty;
                info.InstallDate = GetDateTime(obj["InstallDate"]);
                info.LastBootUpTime = GetDateTime(obj["LastBootUpTime"]);
                break;
            }
        }
        catch
        {
            info.Architecture = RuntimeInformation.OSArchitecture.ToString();
            info.Version = RuntimeInformation.OSDescription;
            return info;
        }

        return info;
    }

    private static NetworkInfo LoadNetworkInfo()
    {
        var info = new NetworkInfo();
        try
        {
            var interfaces = NetworkInterface.GetAllNetworkInterfaces()
                .Where(nic => nic.OperationalStatus == OperationalStatus.Up && nic.NetworkInterfaceType != NetworkInterfaceType.Loopback)
                .ToList();

            var preferred = interfaces
                .Where(nic => nic.GetIPProperties().GatewayAddresses.Count > 0)
                .ToList();

            if (preferred.Count == 0)
            {
                preferred = interfaces;
            }

            foreach (var nic in preferred)
            {
                var props = nic.GetIPProperties();
                foreach (var address in props.UnicastAddresses)
                {
                    if (address.Address.AddressFamily == AddressFamily.InterNetwork)
                    {
                        info.IPv4Addresses.Add(address.Address.ToString());
                    }
                    else if (address.Address.AddressFamily == AddressFamily.InterNetworkV6 && !address.Address.IsIPv6LinkLocal)
                    {
                        info.IPv6Addresses.Add(address.Address.ToString());
                    }
                }

                if (info.IPv4Addresses.Count > 0 || info.IPv6Addresses.Count > 0)
                {
                    break;
                }
            }
        }
        catch
        {
            return info;
        }

        info.IPv4 = string.Join(" / ", info.IPv4Addresses.Distinct());
        info.IPv6 = string.Join(" / ", info.IPv6Addresses.Distinct());
        return info;
    }

    private static string LoadFqdn()
    {
        if (!IsWindows())
        {
            return string.Empty;
        }

        try
        {
            using var searcher = new ManagementObjectSearcher("SELECT Name, Domain FROM Win32_ComputerSystem");
            foreach (ManagementObject obj in searcher.Get())
            {
                var name = obj["Name"]?.ToString();
                var domain = obj["Domain"]?.ToString();
                if (!string.IsNullOrWhiteSpace(name) && !string.IsNullOrWhiteSpace(domain))
                {
                    return $"{name}.{domain}".ToLowerInvariant();
                }
            }
        }
        catch
        {
            return string.Empty;
        }

        return string.Empty;
    }

    private static int? GetInt32(object? value)
    {
        if (value == null)
        {
            return null;
        }

        try
        {
            return Convert.ToInt32(value, CultureInfo.InvariantCulture);
        }
        catch
        {
            return null;
        }
    }

    private static ulong? GetUInt64(object? value)
    {
        if (value == null)
        {
            return null;
        }

        try
        {
            return Convert.ToUInt64(value, CultureInfo.InvariantCulture);
        }
        catch
        {
            return null;
        }
    }

    private static DateTime? GetDateTime(object? value)
    {
        if (value == null)
        {
            return null;
        }

        var dateString = value.ToString();
        if (string.IsNullOrWhiteSpace(dateString))
        {
            return null;
        }

        try
        {
            return ManagementDateTimeConverter.ToDateTime(dateString);
        }
        catch
        {
            return null;
        }
    }

    private static bool TryGetCpuPercent(out double value) {
        value = 0;
        if (!IsWindows()) {
            return false;
        }

        try {
            using var searcher = new ManagementObjectSearcher("SELECT PercentProcessorTime, Name FROM Win32_PerfFormattedData_PerfOS_Processor WHERE Name='_Total'");
            foreach (ManagementObject obj in searcher.Get()) {
                if (TryGetDouble(obj["PercentProcessorTime"], out value)) {
                    value = ClampPercent(value);
                    return true;
                }
            }
        } catch {
            return false;
        }

        return false;
    }

    private static bool TryGetMemoryPercent(out double value) {
        value = 0;
        if (!IsWindows()) {
            return false;
        }

        try {
            using var searcher = new ManagementObjectSearcher("SELECT TotalVisibleMemorySize, FreePhysicalMemory FROM Win32_OperatingSystem");
            foreach (ManagementObject obj in searcher.Get()) {
                var total = GetUInt64(obj["TotalVisibleMemorySize"]);
                var free = GetUInt64(obj["FreePhysicalMemory"]);
                if (total.HasValue && total.Value > 0 && free.HasValue) {
                    value = ClampPercent((1d - (double)free.Value / total.Value) * 100d);
                    return true;
                }
            }
        } catch {
            return false;
        }

        return false;
    }

    private static bool TryGetDiskFreePercent(string? argument, out double value) {
        value = 0;
        if (!TryGetDiskSpace(argument, out var size, out var free)) {
            return false;
        }
        if (size <= 0) {
            return false;
        }
        value = ClampPercent((double)free / size * 100d);
        return true;
    }

    private static bool TryGetDiskFreeGb(string? argument, out double value) {
        value = 0;
        if (!TryGetDiskSpace(argument, out var size, out var free)) {
            return false;
        }
        if (size <= 0) {
            return false;
        }
        value = free / (1024d * 1024d * 1024d);
        return true;
    }

    private static bool TryGetDiskSpace(string? argument, out ulong size, out ulong free) {
        size = 0;
        free = 0;
        if (!IsWindows()) {
            return false;
        }

        var targetDrive = NormalizeDrive(argument);
        ulong? fallbackSize = null;
        ulong? fallbackFree = null;

        try {
            using var searcher = new ManagementObjectSearcher("SELECT DeviceID, Size, FreeSpace, DriveType FROM Win32_LogicalDisk WHERE DriveType=3");
            foreach (ManagementObject obj in searcher.Get()) {
                var device = obj["DeviceID"]?.ToString();
                var sizeValue = GetUInt64(obj["Size"]);
                var freeValue = GetUInt64(obj["FreeSpace"]);
                if (!sizeValue.HasValue || !freeValue.HasValue) {
                    continue;
                }

                if (string.IsNullOrWhiteSpace(targetDrive) || string.Equals(device, targetDrive, StringComparison.OrdinalIgnoreCase)) {
                    size = sizeValue.Value;
                    free = freeValue.Value;
                    return true;
                }

                if (!fallbackSize.HasValue) {
                    fallbackSize = sizeValue;
                    fallbackFree = freeValue;
                }
            }
        } catch {
            return false;
        }

        if (fallbackSize.HasValue && fallbackFree.HasValue) {
            size = fallbackSize.Value;
            free = fallbackFree.Value;
            return true;
        }

        return false;
    }

    private static bool TryGetUptimeHours(out double value) {
        value = 0;
        var uptime = GetUptime();
        if (!uptime.HasValue) {
            return false;
        }

        value = uptime.Value.TotalHours;
        return true;
    }

    private static TimeSpan? GetUptime() {
        if (IsWindows()) {
            var lastBoot = OsInfoCache.Value.LastBootUpTime;
            if (lastBoot.HasValue) {
                var lastBootUtc = lastBoot.Value.Kind == DateTimeKind.Unspecified
                    ? DateTime.SpecifyKind(lastBoot.Value, DateTimeKind.Local).ToUniversalTime()
                    : lastBoot.Value.ToUniversalTime();
                return DateTime.UtcNow - lastBootUtc;
            }
        }

        try {
#if NET472
            var ms = unchecked((uint)Environment.TickCount);
#else
            var ms = Environment.TickCount64;
#endif
            if (ms >= 0) {
                return TimeSpan.FromMilliseconds(ms);
            }
        } catch {
            return null;
        }

        return null;
    }

    private static bool TryGetDouble(object? value, out double result) {
        result = 0;
        if (value == null) {
            return false;
        }

        try {
            result = Convert.ToDouble(value, CultureInfo.InvariantCulture);
            return true;
        } catch {
            return false;
        }
    }

    private static double ClampPercent(double value) {
        if (value < 0) return 0;
        if (value > 100) return 100;
        return value;
    }

    private static string? NormalizeDrive(string? argument) {
        var drive = string.IsNullOrWhiteSpace(argument)
            ? Environment.GetEnvironmentVariable("SystemDrive")
            : argument;
        if (string.IsNullOrWhiteSpace(drive)) {
            return null;
        }

        if (drive == null) {
            return null;
        }

        drive = drive.Trim();
        if (drive.Length == 0) {
            return null;
        }
        if (drive.Length >= 2) {
            drive = drive.Substring(0, 2);
        } else if (drive.Length == 1) {
            drive = drive + ":";
        }

        return drive.ToUpperInvariant();
    }

    private sealed class CpuInfo
    {
        public string Name { get; set; } = string.Empty;
        public int? MaxClockSpeed { get; set; }
        public int? Cores { get; set; }
        public int? LogicalCores { get; set; }
    }

    private sealed class MemoryInfo
    {
        public List<ulong> Sizes { get; } = new();
        public List<int> Speeds { get; } = new();
        public List<string> PartNumbers { get; } = new();
        public string Size { get; set; } = string.Empty;
        public string Speed { get; set; } = string.Empty;
        public string PartNumber { get; set; } = string.Empty;
    }

    private sealed class BiosInfo
    {
        public string Version { get; set; } = string.Empty;
        public string Manufacturer { get; set; } = string.Empty;
        public DateTime? ReleaseDate { get; set; }
    }

    private sealed class OsInfo
    {
        public string Name { get; set; } = string.Empty;
        public string Version { get; set; } = string.Empty;
        public string Architecture { get; set; } = string.Empty;
        public string Build { get; set; } = string.Empty;
        public DateTime? InstallDate { get; set; }
        public DateTime? LastBootUpTime { get; set; }
    }

    private sealed class NetworkInfo
    {
        public List<string> IPv4Addresses { get; } = new();
        public List<string> IPv6Addresses { get; } = new();
        public string IPv4 { get; set; } = string.Empty;
        public string IPv6 { get; set; } = string.Empty;
    }
}
