using System.ComponentModel;
using System.Runtime.InteropServices;

namespace PowerBGInfo;

/// <summary>Delegates existing Unix path spelling and link resolution to the filesystem.</summary>
internal static class UnixPathIdentity {
    internal static string Resolve(string path) {
        var resolved = RealPath(path, IntPtr.Zero);
        if (resolved == IntPtr.Zero) {
            throw new IOException($"Could not resolve the filesystem path '{path}'. No image should overwrite an uncertain base path.",
                new Win32Exception(Marshal.GetLastWin32Error()));
        }
        try {
            // Unix ANSI marshalling is UTF-8. PtrToStringAnsi also compiles on .NET Framework,
            // where this Unix-only branch is never executed.
            var identity = Marshal.PtrToStringAnsi(resolved) ?? throw new IOException($"No filesystem identity was returned for '{path}'.");
            // Darwin realpath retains firmlink volume views; getattrlist returns the canonical
            // user-facing path. glibc can retain caller casing on case-insensitive Linux mounts.
            if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX)) return MacOSPathIdentity.Resolve(identity);
            return ResolveLinuxSpelling(identity);
        } finally {
            Free(resolved);
        }
    }

    private static string ResolveLinuxSpelling(string path) {
        var current = Path.GetPathRoot(path)!;
        foreach (var part in path.Substring(current.Length).Split(Path.DirectorySeparatorChar)) {
            if (part.Length == 0) continue;
            string? alias = null;
            var ambiguous = false;
            try {
                foreach (var entry in Directory.EnumerateFileSystemEntries(current)) {
                    var name = Path.GetFileName(entry);
                    if (string.Equals(name, part, StringComparison.Ordinal)) {
                        alias = name;
                        ambiguous = false;
                        break;
                    }
                    if (string.Equals(name, part, StringComparison.OrdinalIgnoreCase)) {
                        if (alias != null) ambiguous = true;
                        alias = name;
                    }
                }
            } catch (Exception error) when (error is IOException or UnauthorizedAccessException) {
                throw new IOException($"Could not read the filesystem spelling of '{path}'. No image should overwrite an uncertain base path.", error);
            }
            if (alias == null || ambiguous) {
                throw new IOException($"Could not establish the filesystem spelling of '{path}'. Use its stored path spelling before writing an image.");
            }
            current = Path.Combine(current, alias);
        }
        return current;
    }

    [DllImport("libc", EntryPoint = "realpath", SetLastError = true)]
    private static extern IntPtr RealPath([MarshalAs(UnmanagedType.LPUTF8Str)] string path, IntPtr buffer);

    [DllImport("libc", EntryPoint = "free")]
    private static extern void Free(IntPtr value);
}
