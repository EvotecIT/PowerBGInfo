using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Text;
using Microsoft.Win32.SafeHandles;

namespace PowerBGInfo;

/// <summary>Uses Windows filesystem handles to resolve existing aliases on every supported runtime.</summary>
internal static class WindowsPathIdentity {
    internal static string Resolve(string path) {
        using var handle = CreateFile(path, 0, 7, IntPtr.Zero, 3, 0x02000000, IntPtr.Zero);
        if (handle.IsInvalid) throw new IOException($"Could not open the filesystem path '{path}'.", new Win32Exception(Marshal.GetLastWin32Error()));
        var value = new StringBuilder(512);
        var count = GetFinalPathNameByHandle(handle, value, (uint)value.Capacity, 0);
        if (count >= value.Capacity) {
            value.Capacity = checked((int)count + 1);
            count = GetFinalPathNameByHandle(handle, value, (uint)value.Capacity, 0);
        }
        if (count == 0 || count >= value.Capacity) throw new IOException($"Could not resolve the filesystem path '{path}'.", new Win32Exception(Marshal.GetLastWin32Error()));
        return value.ToString();
    }

    [DllImport("kernel32.dll", EntryPoint = "CreateFileW", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern SafeFileHandle CreateFile(string path, uint access, uint share, IntPtr security, uint disposition, uint flags, IntPtr template);

    [DllImport("kernel32.dll", EntryPoint = "GetFinalPathNameByHandleW", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern uint GetFinalPathNameByHandle(SafeFileHandle handle, StringBuilder path, uint capacity, uint flags);
}
