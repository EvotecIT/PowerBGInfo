using System.Globalization;

namespace PowerBGInfo;

internal static partial class PortableMachineFacts {
    private static string? ReadLinuxRelease(string key) => ParseLinuxRelease(ReadTextFile("/etc/os-release"), key);

    /// <summary>Reads os-release as data; shell interpolation and command execution are never performed.</summary>
    internal static string? ParseLinuxRelease(string? text, string key) {
        if (text == null) return null;
        foreach (var line in text.Split('\n')) {
            var pair = line.Trim();
            var separator = pair.IndexOf('=');
            if (separator <= 0 || pair.Substring(0, separator) != key) continue;
            var value = pair.Substring(separator + 1).Trim();
            if (value.Length >= 2 && (value[0] == '"' && value[value.Length - 1] == '"' || value[0] == '\'' && value[value.Length - 1] == '\'')) {
                var quote = value[0];
                value = value.Substring(1, value.Length - 2);
                if (quote == '"') value = value.Replace("\\\"", "\"").Replace("\\$", "$").Replace("\\`", "`").Replace("\\\\", "\\");
            }
            return string.IsNullOrWhiteSpace(value) ? null : value;
        }
        return null;
    }

    private static string? ReadLinuxCpuValue(string key) {
        var text = ReadTextFile("/proc/cpuinfo");
        if (text == null) return null;
        foreach (var line in text.Split('\n')) {
            var separator = line.IndexOf(':');
            if (separator > 0 && string.Equals(line.Substring(0, separator).Trim(), key, StringComparison.OrdinalIgnoreCase)) return line.Substring(separator + 1).Trim();
        }
        return null;
    }

    private static int? ReadLinuxPhysicalCores() {
        var text = ReadTextFile("/proc/cpuinfo");
        if (text == null) return null;
        var cores = new HashSet<string>(StringComparer.Ordinal);
        string? socket = null, core = null;
        foreach (var line in (text + "\n\n").Split('\n')) {
            if (string.IsNullOrWhiteSpace(line)) {
                if (core != null) cores.Add((socket ?? "0") + ":" + core);
                socket = core = null;
                continue;
            }
            var separator = line.IndexOf(':');
            if (separator <= 0) continue;
            var key = line.Substring(0, separator).Trim();
            var value = line.Substring(separator + 1).Trim();
            if (key == "physical id") socket = value;
            if (key == "core id") core = value;
        }
        return cores.Count > 0 ? cores.Count : null;
    }

    /// <summary>Counts Linux logical processor records rather than the current process's CPU allocation.</summary>
    internal static int? ParseLinuxLogicalCores(string? text) {
        if (text == null) return null;
        var processors = new HashSet<int>();
        foreach (var line in text.Split('\n')) {
            var separator = line.IndexOf(':');
            if (separator <= 0 || line.Substring(0, separator).Trim() != "processor") continue;
            if (int.TryParse(line.Substring(separator + 1).Trim(), NumberStyles.None, CultureInfo.InvariantCulture, out var processor) && processor >= 0) processors.Add(processor);
        }
        return processors.Count > 0 ? processors.Count : null;
    }

    private static ulong? ReadLinuxMemoryValue(string key) => ParseLinuxMemoryValue(ReadTextFile("/proc/meminfo"), key);

    /// <summary>Reads Linux's explicitly kilobyte-valued memory counters and rejects malformed or overflowing input.</summary>
    internal static ulong? ParseLinuxMemoryValue(string? text, string key) {
        if (text == null) return null;
        foreach (var line in text.Split('\n')) {
            var separator = line.IndexOf(':');
            if (separator <= 0 || line.Substring(0, separator) != key) continue;
            var parts = line.Substring(separator + 1).Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length != 2 || parts[1] != "kB" || !ulong.TryParse(parts[0], NumberStyles.None, CultureInfo.InvariantCulture, out var kb) || kb > ulong.MaxValue / 1024) return null;
            return kb * 1024;
        }
        return null;
    }

    private static (ulong Total, ulong Idle)? ReadLinuxCpuCounters() => ParseLinuxCpuCounters(ReadTextFile("/proc/stat"));

    /// <summary>Excludes guest counters, which Linux already includes in user and nice time.</summary>
    internal static (ulong Total, ulong Idle)? ParseLinuxCpuCounters(string? text) {
        if (text == null) return null;
        var line = text.Split('\n').FirstOrDefault(value => value.StartsWith("cpu ", StringComparison.Ordinal));
        if (line == null) return null;
        var parts = line.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length < 5) return null;
        ulong total = 0, idle = 0;
        try {
            for (var index = 1; index < Math.Min(parts.Length, 9); index++) {
                if (!ulong.TryParse(parts[index], NumberStyles.None, CultureInfo.InvariantCulture, out var ticks)) return null;
                total = checked(total + ticks);
                if (index is 4 or 5) idle = checked(idle + ticks);
            }
        } catch (OverflowException) { return null; }
        return (total, idle);
    }
}
