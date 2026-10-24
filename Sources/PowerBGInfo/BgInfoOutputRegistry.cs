using System.Text.RegularExpressions;
using System.Diagnostics;
using System.Text;

namespace PowerBGInfo;

/// <summary>
/// Remembers which images PowerBGInfo wrote and which image each one was drawn on. A later run uses that
/// record to go back to the original instead of drawing on its own output, which would stack the text.
/// </summary>
/// <remarks>The versioned record stores one image per line, with UTF-8 Base64 fields so all native filenames round-trip.
/// Unescaped legacy records are migrated only on Windows after validating the Windows filename grammar.
/// Unix legacy records cannot distinguish embedded newlines from additional records and are never trusted.
/// When earlier origins are unavailable, v2 retains that uncertainty for unmapped images across later writes.
/// A companion lock file serializes writers across processes; readers see an atomic snapshot.</remarks>
internal static class BgInfoOutputRegistry {
    /// <summary>File name of the record, written next to the generated images.</summary>
    public const string FileName = "PowerBgInfo.outputs.txt";

    private const char Separator = '|';
    private const string LegacyHeader = "# Images written by PowerBGInfo and the image each was drawn on (empty: a generated or plain background).";
    private const string VersionPrefix = "# PowerBGInfo output registry ";
    private const string Header = VersionPrefix + "v2 (UTF-8 Base64 fields).";
    private const string UnavailableOriginsMarker = "# Earlier output origins unavailable.";
    private static readonly Encoding FieldEncoding = new UTF8Encoding(false, true);

    // Default output names: <name>_PowerBgInfo.<ext>, slideshow copies <name>_PowerBgInfo_001.<ext>,
    // and the temporary refresh copy <name>_refresh_<guid>.<ext>.
    private static readonly Regex OwnName = new(@"^(?:PowerBgInfo$|.*(?:_PowerBgInfo(?:_\d{3})?|_refresh_[0-9a-f]{32})$)",
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Singleline);

    /// <summary>Records that <paramref name="outputPath"/> was drawn on <paramref name="sourcePath"/>. Never throws.</summary>
    public static void Record(string outputPath, string? sourcePath) => Update(outputPath, sourcePath, remove: false);

    /// <summary>Forgets an owned transient output after it is no longer referenced or retained. Never throws.</summary>
    internal static void Forget(string outputPath) => Update(outputPath, null, remove: true);

    private static void Update(string outputPath, string? sourcePath, bool remove) {
        string? temporaryPath = null;
        var temporaryOwned = false;
        try {
            var fullPath = Path.GetFullPath(outputPath);
            var directory = Path.GetDirectoryName(fullPath);
            if (string.IsNullOrWhiteSpace(directory)) return;
            var registryPath = Path.Combine(directory, FileName);
            using var updateLock = AcquireLock(registryPath, readOnly: false);
            var entries = Load(registryPath, out var earlierOriginsUnavailable);
            var key = Path.GetFileName(fullPath);
            var removed = entries.RemoveAll(entry => BgInfoPaths.AreSame(Path.Combine(directory, entry.Key), fullPath));
            if (remove && removed == 0) return;
            if (!remove) {
                var source = string.IsNullOrWhiteSpace(sourcePath) ? string.Empty : Path.GetFullPath(sourcePath!);
                entries.Add(new KeyValuePair<string, string>(key, source));
            }
            var lines = new List<string>(entries.Count + 2) { Header };
            if (earlierOriginsUnavailable) lines.Add(UnavailableOriginsMarker);
            lines.AddRange(entries.Select(entry => Encode(entry.Key) + Separator + Encode(entry.Value)));
            temporaryPath = registryPath + "." + Guid.NewGuid().ToString("N") + ".tmp";
            using (var snapshot = new FileStream(temporaryPath, FileMode.CreateNew, FileAccess.Write, FileShare.None)) {
                temporaryOwned = true;
                using var writer = new StreamWriter(snapshot, FieldEncoding);
                foreach (var line in lines) writer.WriteLine(line);
            }
            if (File.Exists(registryPath)) File.Replace(temporaryPath, registryPath, null);
            else File.Move(temporaryPath, registryPath);
            temporaryOwned = false;
        } catch {
            // The record only prevents stacked text on later runs; it must never fail a render.
        } finally {
            if (temporaryPath != null && temporaryOwned) {
                try { File.Delete(temporaryPath); } catch { /* Best-effort removal after a failed update. */ }
            }
        }
    }

    private static FileStream AcquireLock(string registryPath, bool readOnly) {
        var wait = Stopwatch.StartNew();
        while (true) {
            try {
                return readOnly
                    ? new FileStream(registryPath + ".lock", FileMode.Open, FileAccess.Read, FileShare.Read)
                    : new FileStream(registryPath + ".lock", FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None);
            } catch (IOException) when (wait.Elapsed < TimeSpan.FromSeconds(5)) {
                System.Threading.Thread.Sleep(10);
            }
        }
    }

    /// <summary>Returns true when <paramref name="imagePath"/> is recorded as PowerBGInfo output, with its original (null when it had none).</summary>
    public static bool TryGetSource(string imagePath, out string? sourcePath) => TryGetSource(imagePath, out sourcePath, out _);

    /// <summary>
    /// Reads provenance and its trust state from one locked snapshot. A found
    /// mapping is trusted. An absent mapping with an untrusted snapshot cannot
    /// prove that an automatically selected image is an original.
    /// </summary>
    internal static bool TryGetSource(string imagePath, out string? sourcePath, out bool untrustedSnapshot) {
        sourcePath = null;
        untrustedSnapshot = false;
        try {
            var fullPath = Path.GetFullPath(imagePath);
            var directory = Path.GetDirectoryName(fullPath);
            if (string.IsNullOrWhiteSpace(directory)) return false;
            var registryPath = Path.Combine(directory, FileName);
            // Legacy records have no lock. Reading them must not require write access to the image directory.
            using var readLock = File.Exists(registryPath + ".lock") ? AcquireLock(registryPath, readOnly: true) : null;
            foreach (var entry in Load(registryPath, out untrustedSnapshot)) {
                if (!BgInfoPaths.AreSame(Path.Combine(directory, entry.Key), fullPath)) continue;
                sourcePath = string.IsNullOrWhiteSpace(entry.Value) ? null : entry.Value;
                untrustedSnapshot = false;
                return true;
            }
        } catch {
            sourcePath = null;
            untrustedSnapshot = true;
            return false;
        }
        return false;
    }

    /// <summary>True when the file name follows the names PowerBGInfo gives its own images.</summary>
    public static bool HasOwnOutputName(string imagePath) {
        var name = Path.GetFileNameWithoutExtension(imagePath);
        return !string.IsNullOrEmpty(name) && OwnName.IsMatch(name);
    }

    private static List<KeyValuePair<string, string>> Load(string registryPath, out bool untrustedSnapshot) {
        var entries = new List<KeyValuePair<string, string>>();
        untrustedSnapshot = false;
        if (!File.Exists(registryPath)) return entries;
        // Remain untrusted until the whole existing snapshot has validated. In
        // particular, an empty/corrupt file must not authorize in-place stacking.
        untrustedSnapshot = true;
        using var stream = new FileStream(registryPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete);
        using var reader = new StreamReader(stream);
        var firstLine = reader.ReadLine();
        var encoded = firstLine == Header;
        var earlierOriginsUnavailable = false;
        if (firstLine == null || (!encoded && Path.DirectorySeparatorChar != '\\')) return entries;
        if (firstLine.StartsWith(VersionPrefix, StringComparison.Ordinal) && !encoded) return entries;
        string? line;
        // Windows forbids the delimiters inside filename components, so its
        // validated native records can migrate. Unix syntax offers no equivalent
        // proof: a real newline path may look exactly like two valid records.
        line = encoded ? reader.ReadLine() : firstLine;
        while (line != null) {
            if (encoded && line == UnavailableOriginsMarker) {
                earlierOriginsUnavailable = true;
                line = reader.ReadLine();
                continue;
            }
            if (string.IsNullOrWhiteSpace(line) || (!encoded && line == LegacyHeader)) {
                line = reader.ReadLine();
                continue;
            }
            var separator = line.IndexOf(Separator);
            if (separator <= 0 || separator != line.LastIndexOf(Separator)) return new List<KeyValuePair<string, string>>();
            var key = line.Substring(0, separator);
            var source = line.Substring(separator + 1);
            if (encoded) {
                try {
                    key = Decode(key);
                    source = Decode(source);
                } catch (FormatException) {
                    return new List<KeyValuePair<string, string>>();
                } catch (DecoderFallbackException) {
                    return new List<KeyValuePair<string, string>>();
                }
            }
            if (!IsLeafName(key) || (!encoded && key.StartsWith("#", StringComparison.Ordinal)) ||
                source.IndexOf('\0') >= 0 || (source.Length != 0 && !Path.IsPathRooted(source))) {
                return new List<KeyValuePair<string, string>>();
            }
            if (!encoded && (!IsLegacyWindowsFileName(key) || !IsLegacyWindowsSource(source))) {
                return new List<KeyValuePair<string, string>>();
            }
            entries.Add(new KeyValuePair<string, string>(key, source));
            line = reader.ReadLine();
        }
        // Never erase the uncertainty of discarded older records merely because
        // a later explicit render added a trustworthy mapping for another image.
        untrustedSnapshot = earlierOriginsUnavailable;
        return entries;
    }

    private static bool IsLeafName(string value) => value.Length != 0 && value != "." && value != ".." &&
        !Path.IsPathRooted(value) && Path.GetFileName(value) == value && value.IndexOf('\0') < 0;

    private static bool IsLegacyWindowsFileName(string value) => value.Length != 0 && value != "." && value != ".." &&
        !value.EndsWith(".", StringComparison.Ordinal) && !value.EndsWith(" ", StringComparison.Ordinal) &&
        !value.Any(character => character < ' ' || "<>:\"/\\|?*".IndexOf(character) >= 0);

    /// <summary>Limits legacy migration to ordinary fully qualified drive/UNC paths, excluding device paths and streams.</summary>
    private static bool IsLegacyWindowsSource(string value) {
        if (value.Length == 0) return true;
        var drivePath = value.Length >= 3 && ((value[0] >= 'A' && value[0] <= 'Z') || (value[0] >= 'a' && value[0] <= 'z')) &&
            value[1] == ':' && value[2] == '\\';
        var uncPath = value.StartsWith("\\\\", StringComparison.Ordinal);
        if (!drivePath && !uncPath) return false;
        var parts = value.Substring(drivePath ? 3 : 2).Split('\\');
        if (parts.Length < (drivePath ? 1 : 3)) return false;
        return parts.All(IsLegacyWindowsFileName);
    }

    private static string Encode(string value) => Convert.ToBase64String(FieldEncoding.GetBytes(value));
    private static string Decode(string value) => FieldEncoding.GetString(Convert.FromBase64String(value));
}
