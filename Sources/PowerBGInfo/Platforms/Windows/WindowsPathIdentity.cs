#if NET472
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Text;
using Microsoft.Win32.SafeHandles;

namespace PowerBGInfo;

/// <summary>Resolves redirected paths on .NET Framework; non-link reparse points have no target.</summary>
internal static class WindowsPathIdentity {
    internal static string? ResolveLink(string path) {
        using var handle = CreateFile(path, 0, 7, IntPtr.Zero, 3, 0x02000000, IntPtr.Zero);
        if (handle.IsInvalid) throw new IOException($"Could not open the filesystem link '{path}'.", new Win32Exception(Marshal.GetLastWin32Error()));
        var value = new StringBuilder(512);
        var count = GetFinalPathNameByHandle(handle, value, (uint)value.Capacity, 0);
        if (count >= value.Capacity) {
            value.Capacity = checked((int)count + 1);
            count = GetFinalPathNameByHandle(handle, value, (uint)value.Capacity, 0);
        }
        if (count == 0 || count >= value.Capacity) throw new IOException($"Could not resolve the filesystem link '{path}'.", new Win32Exception(Marshal.GetLastWin32Error()));
        var resolved = value.ToString();
        // Cloud and deduplication reparse points can retain their original path.
        // Recursing on that same component would never establish an identity.
        // Exact spelling matters on Windows directories with case sensitivity enabled:
        // a real link can redirect 'images' to a distinct sibling named 'Images'.
        return string.Equals(BgInfoPaths.NormalizeWindowsIdentity(resolved), BgInfoPaths.NormalizeWindowsIdentity(Path.GetFullPath(path)),
            StringComparison.Ordinal) ? null : resolved;
    }

    [DllImport("kernel32.dll", EntryPoint = "CreateFileW", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern SafeFileHandle CreateFile(string path, uint access, uint share, IntPtr security, uint disposition, uint flags, IntPtr template);

    [DllImport("kernel32.dll", EntryPoint = "GetFinalPathNameByHandleW", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern uint GetFinalPathNameByHandle(SafeFileHandle handle, StringBuilder path, uint capacity, uint flags);
}
#endif
