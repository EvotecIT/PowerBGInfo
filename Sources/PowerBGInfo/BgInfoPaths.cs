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
    /// This resolves case aliases, not symbolic links or hard links. An unreadable existing component
    /// fails closed: guessing that two paths differ could permit overwriting the only original image.
    /// </remarks>
    internal static string GetIdentity(string path) {
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
        // Different names cannot be case aliases. Avoid filesystem work for every unrelated registry row.
        if (!string.Equals(left, right, StringComparison.OrdinalIgnoreCase)) return false;
        return string.Equals(GetIdentity(left), GetIdentity(right), StringComparison.Ordinal);
    }

    private static string AppendRemaining(string current, IReadOnlyList<string> parts, int index) {
        for (; index < parts.Count; index++) current = Path.Combine(current, parts[index]);
        return current;
    }

    private sealed class FileSystemPathComparer : IEqualityComparer<string> {
        public bool Equals(string? left, string? right) => left == null || right == null
            ? left == right : AreSame(left, right);

        // Stable across creation: distinct case-sensitive names may share a bucket, while case
        // aliases must always share one even when a planned output did not exist when it was added.
        public int GetHashCode(string path) => StringComparer.OrdinalIgnoreCase.GetHashCode(Path.GetFullPath(path));
    }
}
