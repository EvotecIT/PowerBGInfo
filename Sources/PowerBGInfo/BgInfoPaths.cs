using System.Runtime.InteropServices;
using System.Text;

namespace PowerBGInfo;

/// <summary>Filesystem identity rules shared by output ownership and volume bindings.</summary>
internal static class BgInfoPaths {
    // Lexical Windows values such as drive letters retain their existing comparison rule.
    // File identity uses the spelling returned by the filesystem instead of this OS-wide rule.
    internal static StringComparison Comparison => RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
        ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;

    internal static IEqualityComparer<string> Comparer { get; } = new FileSystemPathComparer();

    /// <summary>
    /// Resolves the actual spelling of existing path components. Missing components retain their
    /// authored spelling, so two new names on a case-sensitive volume are never collapsed.
    /// </summary>
    /// <remarks>
    /// Native filesystem resolution handles aliases and symbolic links; Linux also verifies stored
    /// spelling. An unreadable or uncertain existing component fails closed: guessing that two paths
    /// differ could permit overwriting the only original image.
    /// </remarks>
    internal static string GetIdentity(string path) => NormalizeIdentity(ResolveIdentity(path));

    private static string ResolveIdentity(string path) {
        var current = Path.GetFullPath(path);
        var missing = new Stack<string>();
        while (true) {
            try {
                File.GetAttributes(current);
            } catch (FileNotFoundException) {
                MoveToParent();
                continue;
            } catch (DirectoryNotFoundException) {
                MoveToParent();
                continue;
            } catch (Exception error) when (error is IOException or UnauthorizedAccessException) {
                throw new IOException($"Could not establish the filesystem identity of '{current}'. No image should overwrite an uncertain base path.", error);
            }
            // The operating system owns alias and link semantics. Resolve the deepest existing
            // ancestor once; authored missing suffixes must remain distinct until they exist.
            current = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
                ? WindowsPathIdentity.Resolve(current) : UnixPathIdentity.Resolve(current);
            while (missing.Count > 0) current = Path.Combine(current, missing.Pop());
            return current;
        }

        void MoveToParent() {
            var parent = Path.GetDirectoryName(current);
            if (string.IsNullOrEmpty(parent) || string.Equals(parent, current, StringComparison.Ordinal)) {
                throw new IOException($"Could not establish an existing filesystem root for '{path}'. No image should overwrite an uncertain base path.");
            }
            missing.Push(Path.GetFileName(current));
            current = parent;
        }
    }

    internal static bool AreSame(string path, string other) {
        var left = Path.GetFullPath(path);
        var right = Path.GetFullPath(other);
        if (string.Equals(left, right, StringComparison.Ordinal)) return true;
        // Leaf spelling alone cannot exclude aliases (8.3 names and Unicode variants differ).
        return string.Equals(GetIdentity(left), GetIdentity(right), StringComparison.Ordinal);
    }

    private static string NormalizeIdentity(string path) => RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
        ? NormalizeWindowsIdentity(path) : path;

    /// <summary>Normalizes Windows comparison roots without changing physical lookup paths or suffix case.</summary>
    internal static string NormalizeWindowsIdentity(string path) {
        var identity = path;
        var devicePrefix = identity.StartsWith(@"\\?\", StringComparison.Ordinal) || identity.StartsWith(@"\\.\", StringComparison.Ordinal);
        if (devicePrefix && identity.Substring(4).StartsWith("UNC\\", StringComparison.OrdinalIgnoreCase)) {
            identity = @"\\" + identity.Substring(8);
        } else if (devicePrefix && identity.Length >= 7 && char.IsLetter(identity[4]) && identity[5] == ':' && identity[6] == '\\') {
            identity = identity.Substring(4);
        }
        if (identity.Length >= 3 && char.IsLetter(identity[0]) && identity[1] == ':' && identity[2] == '\\') {
            return char.ToUpperInvariant(identity[0]) + identity.Substring(1);
        }
        if (identity.StartsWith(@"\\", StringComparison.Ordinal) && !identity.StartsWith(@"\\?\", StringComparison.Ordinal)
            && !identity.StartsWith(@"\\.\", StringComparison.Ordinal)) {
            var serverEnd = identity.IndexOf('\\', 2);
            if (serverEnd >= 0) {
                var shareEnd = identity.IndexOf('\\', serverEnd + 1);
                var rootEnd = shareEnd < 0 ? identity.Length : shareEnd;
                return identity.Substring(0, rootEnd).ToUpperInvariant() + identity.Substring(rootEnd);
            }
        }
        return identity;
    }

    private sealed class FileSystemPathComparer : IEqualityComparer<string> {
        public bool Equals(string? left, string? right) => left == null || right == null
            ? left == right : AreSame(left, right);

        // Stable across creation: folding only the hash permits collisions for distinct names,
        // while aliases must share a bucket even when a planned output did not exist when added.
        // Equality always uses native identities; Unicode is never collapsed on Linux or in equality.
        public int GetHashCode(string path) {
            var identity = GetIdentity(path);
            if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX)) identity = identity.Normalize(NormalizationForm.FormD);
            return StringComparer.OrdinalIgnoreCase.GetHashCode(identity);
        }
    }
}
