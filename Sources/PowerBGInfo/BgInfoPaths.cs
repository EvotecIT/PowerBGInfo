using System.Runtime.InteropServices;

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
    /// This resolves case aliases and symbolic links. An unreadable existing component
    /// fails closed: guessing that two paths differ could permit overwriting the only original image.
    /// </remarks>
    internal static string GetIdentity(string path) => NormalizeIdentity(ResolveIdentity(path, 0));

    private static string ResolveIdentity(string path, int linkDepth) {
        if (linkDepth > 64) throw new IOException($"The filesystem link chain for '{path}' is too deep to establish a safe image identity.");
        var fullPath = Path.GetFullPath(path);
        var root = Path.GetPathRoot(fullPath)!;
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows) && root.Length >= 2 && root[1] == ':') {
            root = char.ToUpperInvariant(root[0]) + root.Substring(1);
        }
        var current = root;
        var parts = fullPath.Substring(root.Length).Split(new[] { Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar }, StringSplitOptions.RemoveEmptyEntries);
        for (var index = 0; index < parts.Length; index++) {
            var candidate = Path.Combine(current, parts[index]);
            try {
                File.GetAttributes(candidate);
            } catch (FileNotFoundException) {
                return AppendRemaining(current, parts, index);
            } catch (DirectoryNotFoundException) {
                return AppendRemaining(current, parts, index);
            } catch (Exception error) when (error is IOException or UnauthorizedAccessException) {
                throw new IOException($"Could not establish the filesystem identity of '{candidate}'. No image should overwrite an uncertain base path.", error);
            }

            if ((File.GetAttributes(candidate) & FileAttributes.ReparsePoint) != 0 && ResolveLink(candidate) is { } linkTarget) {
                // ResolveLinkTarget follows the leaf link chain, but its target can
                // still contain symbolic-link parents (for example /tmp on macOS).
                current = ResolveIdentity(linkTarget, linkDepth + 1);
                continue;
            }

            string? alias = null;
            try {
                foreach (var entry in Directory.EnumerateFileSystemEntries(current)) {
                    var name = Path.GetFileName(entry);
                    if (string.Equals(name, parts[index], StringComparison.Ordinal)) {
                        alias = name;
                        break;
                    }
                    if (string.Equals(name, parts[index], StringComparison.OrdinalIgnoreCase)) {
                        // Existence above was checked with the real filesystem's case rules.
                        // On a case-sensitive volume a differently cased missing name never reaches here.
                        alias = name;
                    }
                }
            } catch (Exception error) when (error is IOException or UnauthorizedAccessException) {
                throw new IOException($"Could not read the filesystem spelling of '{candidate}'. No image should overwrite an uncertain base path.", error);
            }
            if (alias == null) throw new IOException($"The filesystem identity of '{candidate}' changed or could not be resolved. Run again before writing the image.");
            current = Path.Combine(current, alias);
        }
        return current;
    }

    internal static bool AreSame(string path, string other) {
        var left = Path.GetFullPath(path);
        var right = Path.GetFullPath(other);
        if (string.Equals(left, right, StringComparison.Ordinal)) return true;
        // Most registry rows have unrelated leaf names. Only a final symbolic link can
        // alias another leaf; directory aliases still retain the same leaf spelling.
        if (!string.Equals(Path.GetFileName(left), Path.GetFileName(right), StringComparison.OrdinalIgnoreCase)
            && !IsLink(left) && !IsLink(right)) return false;
        return string.Equals(GetIdentity(left), GetIdentity(right), StringComparison.Ordinal);
    }

    private static bool IsLink(string path) {
        try { return (File.GetAttributes(path) & FileAttributes.ReparsePoint) != 0 && ResolveLink(path) != null; }
        catch (FileNotFoundException) { return false; }
        catch (DirectoryNotFoundException) { return false; }
    }

    private static string? ResolveLink(string path) {
#if NET472
        return WindowsPathIdentity.ResolveLink(path);
#else
        FileSystemInfo item = Directory.Exists(path) ? new DirectoryInfo(path) : new FileInfo(path);
        // Reparse points also represent ordinary files such as cloud placeholders.
        // The platform API returns null for those rather than a link target.
        return item.ResolveLinkTarget(returnFinalTarget: true)?.FullName;
#endif
    }

    // Canonicalize links without enumerating directory contents for every hash operation.
    private static string ResolveLinks(string path) {
        var fullPath = Path.GetFullPath(path);
        var root = Path.GetPathRoot(fullPath)!;
        var current = root;
        var parts = fullPath.Substring(root.Length).Split(new[] { Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar }, StringSplitOptions.RemoveEmptyEntries);
        for (var index = 0; index < parts.Length; index++) {
            var candidate = Path.Combine(current, parts[index]);
            try {
                current = (File.GetAttributes(candidate) & FileAttributes.ReparsePoint) != 0 && ResolveLink(candidate) is { } target
                    ? ResolveIdentity(target, 0) : candidate;
            } catch (FileNotFoundException) {
                return AppendRemaining(current, parts, index);
            } catch (DirectoryNotFoundException) {
                return AppendRemaining(current, parts, index);
            }
        }
        return current;
    }

    private static string AppendRemaining(string current, IReadOnlyList<string> parts, int index) {
        for (; index < parts.Count; index++) current = Path.Combine(current, parts[index]);
        return current;
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

        // Stable across creation: distinct case-sensitive names may share a bucket, while case
        // aliases must always share one even when a planned output did not exist when it was added.
        public int GetHashCode(string path) => StringComparer.OrdinalIgnoreCase.GetHashCode(NormalizeIdentity(ResolveLinks(path)));
    }
}
