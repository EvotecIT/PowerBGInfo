using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.InteropServices;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Text;

namespace PowerBGInfo.Cli;

/// <summary>
/// Owns the wrapper and configuration handoff inside one atomically created,
/// private directory. Cleanup removes only the known files and empty directories;
/// files an author's script creates are never recursively removed.
/// </summary>
internal sealed class PowerShellConfigurationTemporaryDirectory : IDisposable {
    internal string DirectoryPath { get; }
    internal string WrapperPath => Path.Combine(DirectoryPath, "wrapper.ps1");
    internal string OutputPath => Path.Combine(DirectoryPath, "configuration.json");

    private PowerShellConfigurationTemporaryDirectory(string directoryPath) => DirectoryPath = directoryPath;

    internal static PowerShellConfigurationTemporaryDirectory Create() {
#if NETFRAMEWORK
        if (Path.DirectorySeparatorChar == '\\') return new PowerShellConfigurationTemporaryDirectory(WindowsDirectory.Create());
        throw new PlatformNotSupportedException("The .NET Framework configuration host requires Windows.");
#else
        if (OperatingSystem.IsWindows()) return new PowerShellConfigurationTemporaryDirectory(WindowsDirectory.Create());
        // CreateTempSubdirectory performs the atomic Unix mkdir with mode 0700,
        // rather than opening a predictable shared application directory.
        var directory = Directory.CreateTempSubdirectory("PowerBGInfo-");
        var mode = File.GetUnixFileMode(directory.FullName);
        const UnixFileMode privateMode = UnixFileMode.UserRead | UnixFileMode.UserWrite | UnixFileMode.UserExecute;
        const UnixFileMode otherAccess = UnixFileMode.GroupRead | UnixFileMode.GroupWrite | UnixFileMode.GroupExecute |
            UnixFileMode.OtherRead | UnixFileMode.OtherWrite | UnixFileMode.OtherExecute;
        if ((mode & privateMode) != privateMode || (mode & otherAccess) != 0 || (directory.Attributes & FileAttributes.ReparsePoint) != 0) {
            try { directory.Delete(); } catch (IOException) { } catch (UnauthorizedAccessException) { }
            throw new UnauthorizedAccessException("The configuration temporary directory is not private to the current user.");
        }
        return new PowerShellConfigurationTemporaryDirectory(directory.FullName);
#endif
    }

    internal void WriteWrapper(string script) {
        using var stream = new FileStream(WrapperPath, FileMode.CreateNew, FileAccess.Write, FileShare.Read);
        using var writer = new StreamWriter(stream, Encoding.UTF8);
        writer.Write(script);
    }

    public void Dispose() {
        TryDeleteFile(WrapperPath);
        TryDeleteFile(OutputPath);
        try { Directory.Delete(DirectoryPath); } catch (IOException) { } catch (UnauthorizedAccessException) { }
    }

    private static void TryDeleteFile(string path) {
        try { File.Delete(path); } catch (IOException) { } catch (UnauthorizedAccessException) { }
    }

#if !NETFRAMEWORK
    [System.Runtime.Versioning.SupportedOSPlatform("windows")]
#endif
    private static class WindowsDirectory {
        internal static string Create() {
            using var identity = WindowsIdentity.GetCurrent();
            var user = identity.User ?? throw new UnauthorizedAccessException("The current Windows user has no security identifier.");
            var security = new DirectorySecurity();
            security.SetOwner(user);
            security.SetAccessRuleProtection(isProtected: true, preserveInheritance: false);
            security.AddAccessRule(new FileSystemAccessRule(user, FileSystemRights.FullControl,
                InheritanceFlags.ContainerInherit | InheritanceFlags.ObjectInherit, PropagationFlags.None, AccessControlType.Allow));
            var descriptor = GCHandle.Alloc(security.GetSecurityDescriptorBinaryForm(), GCHandleType.Pinned);
            try {
                var attributes = new SecurityAttributes {
                    Length = Marshal.SizeOf<SecurityAttributes>(),
                    Descriptor = descriptor.AddrOfPinnedObject()
                };
                for (var attempt = 0; attempt < 10; attempt++) {
                    var path = Path.Combine(Path.GetTempPath(), "PowerBGInfo-" + Guid.NewGuid().ToString("N"));
                    if (!CreateDirectory(path, ref attributes)) {
                        var error = Marshal.GetLastWin32Error();
                        if (error == 183) continue; // A collision never reuses or changes an existing directory.
                        throw new Win32Exception(error, "Unable to create the private configuration directory.");
                    }
                    try {
                        var created = new DirectoryInfo(path);
                        var actual = created.GetAccessControl(AccessControlSections.Owner | AccessControlSections.Access);
                        if ((created.Attributes & FileAttributes.ReparsePoint) != 0 || !actual.AreAccessRulesProtected ||
                            !user.Equals(actual.GetOwner(typeof(SecurityIdentifier)))) {
                            throw new UnauthorizedAccessException("The configuration temporary directory has an unexpected owner or access policy.");
                        }
                        foreach (FileSystemAccessRule rule in actual.GetAccessRules(true, true, typeof(SecurityIdentifier))) {
                            if (rule.AccessControlType == AccessControlType.Allow && !user.Equals(rule.IdentityReference)) {
                                throw new UnauthorizedAccessException("The configuration temporary directory grants access to another user.");
                            }
                        }
                        return path;
                    } catch {
                        try { Directory.Delete(path); } catch (IOException) { } catch (UnauthorizedAccessException) { }
                        throw;
                    }
                }
                throw new IOException("Unable to allocate a unique configuration temporary directory.");
            } finally {
                descriptor.Free();
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct SecurityAttributes {
            internal int Length;
            internal IntPtr Descriptor;
            [MarshalAs(UnmanagedType.Bool)] internal bool InheritHandle;
        }

        [DllImport("kernel32.dll", EntryPoint = "CreateDirectoryW", CharSet = CharSet.Unicode, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool CreateDirectory(string path, ref SecurityAttributes attributes);
    }
}
