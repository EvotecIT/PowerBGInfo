using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Text;

namespace PowerBGInfo;

/// <summary>Queries Darwin's canonical path, including firmlink volume views and stored Unicode spelling.</summary>
internal static class MacOSPathIdentity {
    internal static string Resolve(string path) {
        // sys/attr.h: ATTR_BIT_MAP_COUNT = 5, ATTR_CMN_FULLPATH = 0x08000000.
        // One requested variable-length attribute is returned as length + attrreference + UTF-8.
        var attributes = new AttributeList { BitmapCount = 5, CommonAttributes = 0x08000000 };
        var buffer = new byte[4096]; // Darwin PATH_MAX is 1024 bytes; includes room for attribute headers.
        if (GetAttributeList(path, ref attributes, buffer, (UIntPtr)buffer.Length, UIntPtr.Zero) != 0) {
            throw new IOException($"Could not resolve the canonical macOS path for '{path}'. No image should overwrite an uncertain base path.",
                new Win32Exception(Marshal.GetLastWin32Error()));
        }
        var returned = BitConverter.ToUInt32(buffer, 0);
        var offset = BitConverter.ToInt32(buffer, 4);
        var length = BitConverter.ToUInt32(buffer, 8);
        var start = 4L + offset; // attr_dataoffset is relative to the attribute reference.
        if (returned < 12 || returned > buffer.Length || start < 12 || length < 2
            || start + length > returned || buffer[(int)(start + length - 1)] != 0 || buffer[(int)start] != '/') {
            throw new IOException($"macOS returned an invalid canonical path for '{path}'. No image should overwrite an uncertain base path.");
        }
        return Encoding.UTF8.GetString(buffer, (int)start, checked((int)length - 1));
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct AttributeList {
        internal ushort BitmapCount;
        internal ushort Reserved;
        internal uint CommonAttributes;
        internal uint VolumeAttributes;
        internal uint DirectoryAttributes;
        internal uint FileAttributes;
        internal uint ForkAttributes;
    }

    [DllImport("libc", EntryPoint = "getattrlist", SetLastError = true)]
    private static extern int GetAttributeList([MarshalAs(UnmanagedType.LPUTF8Str)] string path,
        ref AttributeList attributes, [Out] byte[] buffer, UIntPtr size, UIntPtr options);
}
