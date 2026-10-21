using System.Globalization;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Runtime.InteropServices;

namespace PowerBGInfo;

/// <summary>Reads portable facts and the native Unix facts available without additional programs or packages.</summary>
internal sealed class PortableSystemInfoProvider : IBgInfoSystemInfoProvider {
    public BgInfoSystemInfo GetSnapshot() {
        var uptime = PortableMachineFacts.ReadUptime();
        var snapshot = new BgInfoSystemInfo {
            ComputerName = Environment.MachineName,
            UserName = Environment.UserName,
            LoggedOnUser = SessionUser.Resolve(() => null),
            Domain = SystemInfoProvider.OrUnknown(ReadDomain()),
            OSName = PortableMachineFacts.ReadOsName(),
            OSVersion = PortableMachineFacts.ReadOsVersion(),
            OSBuild = PortableMachineFacts.ReadOsBuild(),
            LastBootTime = uptime.HasValue ? DateTime.Now - uptime.Value : null,
            Uptime = uptime,
            UptimeText = SystemInfoProvider.FormatUptime(uptime),
            IPv4Address = SystemInfoProvider.OrUnknown(ReadAddress(AddressFamily.InterNetwork)),
            Disks = ReadDisks()
        };
        snapshot.FactStates = BgInfoFactStates.FromSnapshot(snapshot, windowsFacts: false);
        return snapshot;
    }

    public string GetValue(string builtin) {
        if (string.IsNullOrWhiteSpace(builtin)) return string.Empty;
        switch (builtin.Trim().ToUpperInvariant()) {
            case "USERNAME": return Environment.UserName;
            case "HOSTNAME": return Environment.MachineName;
            case "FULLUSERNAME": return RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
                ? Environment.UserDomainName + "\\" + Environment.UserName : Environment.UserName;
            case "CPUNAME": return PortableMachineFacts.ReadCpuName();
            case "CPUMAXCLOCKSPEED": return Format(PortableMachineFacts.ReadCpuClockMhz());
            case "CPUCORES": return Format(PortableMachineFacts.ReadPhysicalCores());
            case "CPULOGICALCORES": return PortableMachineFacts.ReadLogicalCores().ToString(CultureInfo.CurrentCulture);
            case "RAMSIZE":
                var bytes = PortableMachineFacts.ReadTotalMemory();
                return bytes.HasValue ? Math.Round(bytes.Value / (1024d * 1024d * 1024d)).ToString("N0", CultureInfo.CurrentCulture) + "GB" : string.Empty;
            case "RAMSPEED": case "RAMPARTNUMBER": case "BIOSVERSION": case "BIOSMANUFACTURER": case "BIOSRELEASEDATE":
            case "OSINSTALLDATE": return string.Empty;
            case "OSNAME": return PortableMachineFacts.ReadOsName();
            case "OSVERSION": return PortableMachineFacts.ReadOsVersion();
            case "OSARCHITECTURE": return RuntimeInformation.OSArchitecture.ToString();
            case "OSBUILD": return PortableMachineFacts.ReadOsBuild();
            case "OSLASTBOOTUPTIME":
                var uptime = PortableMachineFacts.ReadUptime();
                return uptime.HasValue ? (DateTime.Now - uptime.Value).ToString("G", CultureInfo.CurrentCulture) : string.Empty;
            case "USERDNSDOMAIN": case "DOMAIN": return SystemInfoProvider.OrUnknown(ReadDomain());
            case "FQDN":
                var domain = ReadDomain();
                var host = Environment.MachineName;
                return host.Contains(".") ? host : string.IsNullOrWhiteSpace(domain) ? string.Empty : host + "." + domain;
            case "IPV4ADDRESS": return ReadAddress(AddressFamily.InterNetwork);
            case "IPV6ADDRESS": return ReadAddress(AddressFamily.InterNetworkV6);
            case "UPTIME": return SystemInfoProvider.FormatUptime(PortableMachineFacts.ReadUptime());
            case "LASTPATCH": case "DEFENDERSTATUS": case "BITLOCKERSTATUS": return SystemInfoProvider.Unknown;
            case "LOGGEDONUSER": return SessionUser.Resolve(() => null);
            case "DISKSUMMARY": return SystemInfoProvider.FormatDiskSummary(ReadDisks());
            default: return string.Empty;
        }
    }

    public bool TryGetNumericValue(BgInfoChartMetric metric, string? argument, out double value) {
        value = 0;
        if (metric == BgInfoChartMetric.CpuPercent) return PortableMachineFacts.TryReadCpuPercent(out value);
        if (metric == BgInfoChartMetric.MemoryPercent) return PortableMachineFacts.TryReadMemoryPercent(out value);
        if (metric is BgInfoChartMetric.UptimeHours or BgInfoChartMetric.UptimeDays) {
            var uptime = PortableMachineFacts.ReadUptime();
            if (!uptime.HasValue) return false;
            value = metric == BgInfoChartMetric.UptimeDays ? uptime.Value.TotalDays : uptime.Value.TotalHours;
            return true;
        }
        if (metric is not (BgInfoChartMetric.DiskFreePercent or BgInfoChartMetric.DiskUsedPercent or BgInfoChartMetric.DiskFreeGb)) return false;
        if (!TryReadDiskSpace(argument, out var total, out var free) || total <= 0) return false;
        value = metric switch {
            BgInfoChartMetric.DiskFreeGb => free / (1024d * 1024d * 1024d),
            BgInfoChartMetric.DiskUsedPercent => 100d * (1d - free / (double)total),
            _ => 100d * free / total
        };
        if (metric != BgInfoChartMetric.DiskFreeGb) value = Math.Max(0, Math.Min(100, value));
        return true;
    }

    private static string Format(int? value) => value?.ToString(CultureInfo.CurrentCulture) ?? string.Empty;

    private static string ReadDomain() {
        try { return IPGlobalProperties.GetIPGlobalProperties().DomainName; }
        catch { return string.Empty; }
    }

    private static string ReadAddress(AddressFamily family) {
        try {
            var interfaces = NetworkInterface.GetAllNetworkInterfaces()
                .Where(nic => nic.OperationalStatus == OperationalStatus.Up && nic.NetworkInterfaceType != NetworkInterfaceType.Loopback)
                .OrderByDescending(HasGateway);
            foreach (var nic in interfaces) {
                try {
                    var addresses = nic.GetIPProperties().UnicastAddresses.Select(value => value.Address)
                        .Where(value => value.AddressFamily == family && !value.IsIPv6LinkLocal)
                        .Select(value => value.ToString()).Distinct().ToArray();
                    if (addresses.Length > 0) return string.Join(" / ", addresses);
                } catch { /* One unavailable interface does not discard the other interfaces. */ }
            }
        } catch { /* Network facts are optional. */ }
        return string.Empty;
    }

    private static bool HasGateway(NetworkInterface nic) {
        try { return nic.GetIPProperties().GatewayAddresses.Count > 0; }
        catch { return false; }
    }

    internal static BgInfoDiskInfo[] ReadDisks() {
        var disks = new List<BgInfoDiskInfo>();
        try {
            foreach (var drive in DriveInfo.GetDrives()) {
                try {
                    if (!drive.IsReady || drive.DriveType != DriveType.Fixed || drive.TotalSize <= 0) continue;
                    var total = drive.TotalSize;
                    var free = drive.AvailableFreeSpace;
                    if (free < 0) continue;
                    const double gb = 1024d * 1024d * 1024d;
                    disks.Add(new BgInfoDiskInfo {
                        Drive = drive.Name, Label = drive.VolumeLabel,
                        SizeGB = Math.Round(total / gb, 1), FreeGB = Math.Round(free / gb, 1),
                        UsedPercent = Math.Round(Math.Max(0, Math.Min(100, (1d - free / (double)total) * 100d)), 1)
                    });
                } catch { /* Unmounted or inaccessible filesystems remain absent. */ }
            }
        } catch { /* An unavailable mount enumeration leaves an empty result. */ }
        return disks.OrderBy(value => value.Drive, StringComparer.Ordinal).ToArray();
    }

    private static bool TryReadDiskSpace(string? argument, out long total, out long free) {
        total = free = 0;
        try {
            var root = Path.GetPathRoot(Environment.CurrentDirectory) ?? "/";
            var path = string.IsNullOrWhiteSpace(argument) ? root : Path.GetFullPath(argument!);
            var drive = DriveInfo.GetDrives()
                .Where(candidate => ContainsPath(candidate.Name, path))
                .OrderByDescending(candidate => candidate.Name.Length).FirstOrDefault();
            if (drive == null || !drive.IsReady) return false;
            // A misspelled/nonexistent requested mount must not silently report another filesystem's free space.
            if (!string.IsNullOrWhiteSpace(argument) && !Directory.Exists(path) && !File.Exists(path)) return false;
            total = drive.TotalSize;
            free = drive.AvailableFreeSpace;
            return total > 0 && free >= 0;
        } catch { return false; }
    }

    private static bool ContainsPath(string root, string path) {
        var comparison = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
        var normalized = root.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        return path.Equals(root, comparison) || path.Equals(normalized, comparison) ||
            path.StartsWith(normalized + Path.DirectorySeparatorChar, comparison);
    }
}
