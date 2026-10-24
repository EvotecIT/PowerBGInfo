using System.Globalization;
using System.Runtime.InteropServices;

namespace PowerBGInfo;

/// <summary>Dispatches Unix probes without loading a native library on another operating system.</summary>
internal static partial class PortableMachineFacts {
    private static bool IsLinux => RuntimeInformation.IsOSPlatform(OSPlatform.Linux);
    private static bool IsMac => RuntimeInformation.IsOSPlatform(OSPlatform.OSX);

    internal static string ReadOsName() {
        if (IsLinux) return SystemInfoProvider.OrUnknown(ReadLinuxRelease("PRETTY_NAME") ?? RuntimeInformation.OSDescription);
        if (IsMac) return "macOS";
        return SystemInfoProvider.OrUnknown(RuntimeInformation.OSDescription);
    }

    internal static string ReadOsVersion() => SystemInfoProvider.OrUnknown(IsLinux
        ? ReadLinuxRelease("VERSION_ID") ?? RuntimeInformation.OSDescription
        : IsMac ? ReadMacText("kern.osproductversion") ?? RuntimeInformation.OSDescription : Environment.OSVersion.Version.ToString());

    internal static string ReadOsBuild() => SystemInfoProvider.OrUnknown(IsLinux
        ? ReadTextFile("/proc/sys/kernel/osrelease")?.Trim()
        : IsMac ? ReadMacText("kern.osversion") : Environment.OSVersion.Version.Build.ToString(CultureInfo.InvariantCulture));

    internal static string ReadCpuName() => SystemInfoProvider.OrUnknown(IsLinux
        ? ReadLinuxCpuValue("model name") ?? ReadLinuxCpuValue("Hardware")
        : IsMac ? ReadMacText("machdep.cpu.brand_string") ?? ReadMacText("hw.model") : Environment.GetEnvironmentVariable("PROCESSOR_IDENTIFIER"));

    internal static int? ReadCpuClockMhz() {
        if (IsMac) {
            var frequency = ReadMacInteger("hw.cpufrequency_max") ?? ReadMacInteger("hw.cpufrequency");
            return frequency.HasValue && frequency.Value >= 1000000 && frequency.Value / 1000000 <= int.MaxValue ? (int)(frequency.Value / 1000000) : null;
        }
        if (IsLinux && ulong.TryParse(ReadTextFile("/sys/devices/system/cpu/cpu0/cpufreq/cpuinfo_max_freq")?.Trim(), NumberStyles.None, CultureInfo.InvariantCulture, out var khz) && khz >= 1000 && khz / 1000 <= int.MaxValue) return (int)(khz / 1000);
        return null;
    }

    internal static int? ReadPhysicalCores() {
        if (IsMac) {
            var cores = ReadMacInteger("hw.physicalcpu");
            return cores is > 0 and <= int.MaxValue ? (int)cores.Value : null;
        }
        return IsLinux ? ReadLinuxPhysicalCores() : null;
    }

    /// <summary>Reads machine logical CPUs before falling back to the runtime's affinity or configuration-limited process count.</summary>
    internal static int ReadLogicalCores() {
        if (IsMac) {
            var cores = ReadMacInteger("hw.logicalcpu");
            if (cores is > 0 and <= int.MaxValue) return (int)cores.Value;
        }
        if (IsLinux) {
            var cores = ParseLinuxLogicalCores(ReadTextFile("/proc/cpuinfo"));
            if (cores.HasValue) return cores.Value;
        }
        return Environment.ProcessorCount;
    }

    internal static ulong? ReadTotalMemory() => IsLinux ? ReadLinuxMemoryValue("MemTotal") : IsMac ? ReadMacInteger("hw.memsize") : null;

    internal static TimeSpan? ReadUptime() {
        if (IsLinux) {
            var first = ReadTextFile("/proc/uptime")?.Split(' ')[0];
            if (double.TryParse(first, NumberStyles.Float, CultureInfo.InvariantCulture, out var seconds) && seconds >= 0 && seconds <= TimeSpan.MaxValue.TotalSeconds) return TimeSpan.FromSeconds(seconds);
        }
        if (IsMac) {
            var boot = ReadMacBootTime();
            if (boot.HasValue) {
                var elapsed = DateTimeOffset.UtcNow - boot.Value;
                if (elapsed >= TimeSpan.Zero) return elapsed;
            }
        }
#if !NET472
        try { return TimeSpan.FromMilliseconds(Environment.TickCount64); }
        catch { /* An unsupported clock leaves uptime unavailable. */ }
#endif
        return null;
    }

    internal static bool TryReadMemoryPercent(out double value) {
        value = 0;
        var total = ReadTotalMemory();
        if (!total.HasValue || total.Value == 0) return false;
        ulong? available = null;
        if (IsLinux) available = ReadLinuxMemoryValue("MemAvailable");
        else if (IsMac) available = ReadMacAvailableMemory();
        if (!available.HasValue) return false;
        value = Math.Max(0, Math.Min(100, (1d - available.Value / (double)total.Value) * 100d));
        return true;
    }

    /// <summary>Samples cumulative CPU counters over a short interval instead of presenting load average as CPU utilization.</summary>
    internal static bool TryReadCpuPercent(out double value) {
        value = 0;
        var first = ReadCpuCounters();
        if (!first.HasValue) return false;
        System.Threading.Thread.Sleep(80);
        var last = ReadCpuCounters();
        if (!last.HasValue || last.Value.Total <= first.Value.Total || last.Value.Idle < first.Value.Idle) return false;
        var total = last.Value.Total - first.Value.Total;
        var idle = last.Value.Idle - first.Value.Idle;
        value = Math.Max(0, Math.Min(100, (1d - idle / (double)total) * 100d));
        return true;
    }

    private static (ulong Total, ulong Idle)? ReadCpuCounters() => IsLinux ? ReadLinuxCpuCounters() : IsMac ? ReadMacCpuCounters() : null;

    private static string? ReadTextFile(string path) {
        try { return File.ReadAllText(path); }
        catch { return null; }
    }
}
