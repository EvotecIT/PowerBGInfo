using System.Text.RegularExpressions;
using System.Diagnostics;

namespace PowerBGInfo;

/// <summary>
/// Remembers which images PowerBGInfo wrote and which image each one was drawn on. A later run uses that
/// record to go back to the original instead of drawing on its own output, which would stack the text.
/// </summary>
/// <remarks>The record is a small text file next to the images: one line per image, <c>file name|original path</c>.
/// A companion lock file serializes writers across processes; readers see an atomic snapshot.</remarks>
internal static class BgInfoOutputRegistry {
    /// <summary>File name of the record, written next to the generated images.</summary>
    public const string FileName = "PowerBgInfo.outputs.txt";

    private const char Separator = '|';
    private const string Header = "# Images written by PowerBGInfo and the image each was drawn on (empty: a generated or plain background).";

    // Default output names: <name>_PowerBgInfo.<ext>, slideshow copies <name>_PowerBgInfo_001.<ext>,
    // and the temporary refresh copy <name>_refresh_<guid>.<ext>.
    private static readonly Regex OwnName = new(@"^(?:PowerBgInfo$|.*(?:_PowerBgInfo(?:_\d{3})?|_refresh_[0-9a-f]{32})$)",
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

    /// <summary>Records that <paramref name="outputPath"/> was drawn on <paramref name="sourcePath"/>. Never throws.</summary>
    public static void Record(string outputPath, string? sourcePath) {
        string? temporaryPath = null;
        try {
            var directory = Path.GetDirectoryName(Path.GetFullPath(outputPath));
            if (string.IsNullOrWhiteSpace(directory)) return;
            var registryPath = Path.Combine(directory, FileName);
            using var updateLock = AcquireLock(registryPath, readOnly: false);
            var entries = Load(registryPath);
            var key = Path.GetFileName(outputPath);
            entries.RemoveAll(entry => string.Equals(entry.Key, key, StringComparison.OrdinalIgnoreCase));
            var source = string.IsNullOrWhiteSpace(sourcePath) ? string.Empty : Path.GetFullPath(sourcePath);
            entries.Add(new KeyValuePair<string, string>(key, source));
            var lines = new List<string>(entries.Count + 1) { Header };
            lines.AddRange(entries.Select(entry => entry.Key + Separator + entry.Value));
            temporaryPath = registryPath + "." + Guid.NewGuid().ToString("N") + ".tmp";
            File.WriteAllLines(temporaryPath, lines);
            if (File.Exists(registryPath)) File.Replace(temporaryPath, registryPath, null);
            else File.Move(temporaryPath, registryPath);
        } catch {
            // The record only prevents stacked text on later runs; it must never fail a render.
        } finally {
            if (temporaryPath != null) {
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
    public static bool TryGetSource(string imagePath, out string? sourcePath) {
        sourcePath = null;
        try {
            var fullPath = Path.GetFullPath(imagePath);
            var directory = Path.GetDirectoryName(fullPath);
            if (string.IsNullOrWhiteSpace(directory)) return false;
            var key = Path.GetFileName(fullPath);
            var registryPath = Path.Combine(directory, FileName);
            // Legacy records have no lock. Reading them must not require write access to the image directory.
            using var readLock = File.Exists(registryPath + ".lock") ? AcquireLock(registryPath, readOnly: true) : null;
            foreach (var entry in Load(registryPath)) {
                if (!string.Equals(entry.Key, key, StringComparison.OrdinalIgnoreCase)) continue;
                sourcePath = string.IsNullOrWhiteSpace(entry.Value) ? null : entry.Value;
                return true;
            }
        } catch {
            return false;
        }
        return false;
    }

    /// <summary>True when the file name follows the names PowerBGInfo gives its own images.</summary>
    public static bool HasOwnOutputName(string imagePath) {
        var name = Path.GetFileNameWithoutExtension(imagePath);
        return !string.IsNullOrEmpty(name) && OwnName.IsMatch(name);
    }

    private static List<KeyValuePair<string, string>> Load(string registryPath) {
        var entries = new List<KeyValuePair<string, string>>();
        if (!File.Exists(registryPath)) return entries;
        using var stream = new FileStream(registryPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete);
        using var reader = new StreamReader(stream);
        string? line;
        while ((line = reader.ReadLine()) != null) {
            if (string.IsNullOrWhiteSpace(line) || line.StartsWith("#", StringComparison.Ordinal)) continue;
            var separator = line.IndexOf(Separator);
            if (separator <= 0) continue;
            entries.Add(new KeyValuePair<string, string>(line.Substring(0, separator), line.Substring(separator + 1)));
        }
        return entries;
    }
}
