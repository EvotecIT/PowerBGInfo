using System.Runtime.InteropServices;
using System.Text;

namespace PowerBGInfo;

internal static partial class PortableMachineFacts {
    private const string MacSystemLibrary = "/usr/lib/libSystem.B.dylib";
    private static readonly object MacHostGate = new();
    private static uint _macHost;

    private static byte[]? ReadMacValue(string name) {
        try {
            var length = UIntPtr.Zero;
            if (SysctlByName(name, null, ref length, IntPtr.Zero, UIntPtr.Zero) != 0) return null;
            var bytes = length.ToUInt64();
            if (bytes == 0 || bytes > 65536) return null;
            var buffer = new byte[(int)bytes];
            if (SysctlByName(name, buffer, ref length, IntPtr.Zero, UIntPtr.Zero) != 0 || length.ToUInt64() > bytes) return null;
            if (length.ToUInt64() != bytes) Array.Resize(ref buffer, (int)length.ToUInt64());
            return buffer;
        } catch { return null; }
    }

    private static string? ReadMacText(string name) {
        var data = ReadMacValue(name);
        if (data == null) return null;
        var text = Encoding.UTF8.GetString(data).TrimEnd('\0').Trim();
        return text.Length > 0 ? text : null;
    }

    private static ulong? ReadMacInteger(string name) {
        var data = ReadMacValue(name);
        if (data?.Length == 8) return BitConverter.ToUInt64(data, 0);
        if (data?.Length == 4) return BitConverter.ToUInt32(data, 0);
        return null;
    }

    private static DateTimeOffset? ReadMacBootTime() {
        var data = ReadMacValue("kern.boottime");
        if (data == null || data.Length < 8) return null;
        try { return DateTimeOffset.FromUnixTimeSeconds(BitConverter.ToInt64(data, 0)); }
        catch (ArgumentOutOfRangeException) { return null; }
    }

    /// <summary>Memory not in free or inactive pages; this is a physical utilization metric, not Activity Monitor's pressure score.</summary>
    private static ulong? ReadMacAvailableMemory() {
        try {
            // host_statistics64 uses a count of 32-bit integers. Reserve room for later structure revisions;
            // the stable first four fields are free, active, inactive and wired page counts.
            var counters = new uint[256];
            uint count = (uint)counters.Length;
            var host = GetMacHost();
            if (host == 0 || HostStatistics64(host, 4, counters, ref count) != 0 || count < 4) return null;
            var pageSize = ReadMacInteger("hw.pagesize");
            if (!pageSize.HasValue || pageSize.Value == 0) return null;
            return checked(((ulong)counters[0] + counters[2]) * pageSize.Value);
        } catch { return null; }
    }

    private static (ulong Total, ulong Idle)? ReadMacCpuCounters() {
        try {
            var counters = new uint[4];
            uint count = 4;
            var host = GetMacHost();
            if (host == 0 || HostStatistics(host, 3, counters, ref count) != 0 || count < 4) return null;
            // HOST_CPU_LOAD_INFO: user, system, idle and nice ticks.
            return ((ulong)counters[0] + counters[1] + counters[2] + counters[3], counters[2]);
        } catch { return null; }
    }

    private static uint GetMacHost() {
        lock (MacHostGate) {
            // The process-wide provider retains one host send right for its lifetime rather than
            // acquiring another right on every wallpaper refresh or metric sample.
            return _macHost != 0 ? _macHost : _macHost = MachHostSelf();
        }
    }

    [DllImport(MacSystemLibrary, EntryPoint = "sysctlbyname", CharSet = CharSet.Ansi, SetLastError = true)]
    private static extern int SysctlByName(string name, [Out] byte[]? oldValue, ref UIntPtr oldLength, IntPtr newValue, UIntPtr newLength);

    [DllImport(MacSystemLibrary, EntryPoint = "mach_host_self")]
    private static extern uint MachHostSelf();

    [DllImport(MacSystemLibrary, EntryPoint = "host_statistics64")]
    private static extern int HostStatistics64(uint host, int flavor, [Out] uint[] information, ref uint count);

    [DllImport(MacSystemLibrary, EntryPoint = "host_statistics")]
    private static extern int HostStatistics(uint host, int flavor, [Out] uint[] information, ref uint count);
}
