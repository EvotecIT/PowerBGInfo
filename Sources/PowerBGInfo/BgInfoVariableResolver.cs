using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;

namespace PowerBGInfo;

internal static partial class BgInfoVariableResolver {
    private static readonly Regex TemplatePattern = new(@"\{\{\s*(?<name>[A-Za-z0-9_]+)\s*\}\}", RegexOptions.Compiled);

    public static IReadOnlyList<BgInfoEntry> ExpandEntries(BgInfoConfiguration configuration) {
        if (configuration == null) throw new ArgumentNullException(nameof(configuration));

        var variableValues = ResolveVariables(configuration.Variables);
        return ExpandEntries(configuration, variableValues);
    }

    internal static IReadOnlyList<BgInfoEntry> ExpandEntries(BgInfoConfiguration configuration, IReadOnlyDictionary<string, IReadOnlyList<IReadOnlyDictionary<string, string>>> variableValues) {
        if (configuration == null) throw new ArgumentNullException(nameof(configuration));

        var entries = new List<BgInfoEntry>();
        foreach (var entry in configuration.Entries) {
            if (string.IsNullOrWhiteSpace(entry.ForEach)) {
                entries.Add(CloneEntry(entry));
                continue;
            }

            if (!variableValues.TryGetValue(entry.ForEach!, out var items)) {
                throw new InvalidOperationException($"Unknown BGInfo variable '{entry.ForEach}'.");
            }

            foreach (var item in items) {
                entries.Add(ExpandEntry(entry, item));
            }
        }

        return entries;
    }

    internal static Dictionary<string, IReadOnlyList<IReadOnlyDictionary<string, string>>> ResolveVariables(IEnumerable<BgInfoVariable> variables) {
        var resolved = new Dictionary<string, IReadOnlyList<IReadOnlyDictionary<string, string>>>(StringComparer.OrdinalIgnoreCase);
        foreach (var variable in variables) {
            if (string.IsNullOrWhiteSpace(variable.Name)) {
                continue;
            }

            resolved[variable.Name] = ResolveVariable(variable);
        }

        return resolved;
    }

    internal static IReadOnlyList<IReadOnlyDictionary<string, string>> ResolveVariable(BgInfoVariable variable) {
        if (variable == null) throw new ArgumentNullException(nameof(variable));

        return variable.Provider switch {
            BgInfoVariableProvider.Volumes => ResolveVolumes(variable.Argument),
            _ => Array.Empty<IReadOnlyDictionary<string, string>>()
        };
    }

    internal static string RenderTemplate(string? template, IReadOnlyDictionary<string, string> context) {
        if (string.IsNullOrWhiteSpace(template)) {
            return string.Empty;
        }

        return TemplatePattern.Replace(template, match => {
            var key = match.Groups["name"].Value;
            if (context.TryGetValue(key, out var value)) {
                return value;
            }

            return SystemInfoProvider.GetValue(key);
        });
    }

    private static BgInfoEntry ExpandEntry(BgInfoEntry entry, IReadOnlyDictionary<string, string> context) {
        var expanded = CloneEntry(entry);
        expanded.ForEach = null;
        expanded.Name = RenderTemplate(entry.Name, context);
        if (entry.Type == BgInfoEntryType.Value) {
            expanded.Value = RenderTemplate(entry.Value, context);
            expanded.BuiltinValue = null;
        }
        return expanded;
    }

    private static BgInfoEntry CloneEntry(BgInfoEntry entry) {
        return new BgInfoEntry {
            Type = entry.Type,
            Name = entry.Name,
            Value = entry.Value,
            BuiltinValue = entry.BuiltinValue,
            ForEach = entry.ForEach,
            Color = entry.Color,
            FontSize = entry.FontSize,
            FontFamilyName = entry.FontFamilyName,
            Bold = entry.Bold,
            Underline = entry.Underline,
            ValueColor = entry.ValueColor,
            ValueFontSize = entry.ValueFontSize,
            ValueFontFamilyName = entry.ValueFontFamilyName,
            ValueBold = entry.ValueBold,
            ValueUnderline = entry.ValueUnderline
        };
    }

    private static IReadOnlyList<IReadOnlyDictionary<string, string>> ResolveVolumes(string? argument) {
        var values = new List<IReadOnlyDictionary<string, string>>();
        IEnumerable<DriveInfo> drives = DriveInfo.GetDrives().Where(drive => drive.IsReady);
        var driveFilter = NormalizeDriveLetter(argument);
        if (!string.IsNullOrWhiteSpace(driveFilter)) {
            drives = drives.Where(drive => string.Equals(NormalizeDriveLetter(drive.Name), driveFilter, StringComparison.OrdinalIgnoreCase));
        }

        int index = 0;
        foreach (var drive in drives) {
            ulong totalSize;
            ulong freeSpace;
            try {
                totalSize = (ulong)drive.TotalSize;
                freeSpace = (ulong)drive.AvailableFreeSpace;
            } catch {
                continue;
            }

            ulong usedSpace = totalSize > freeSpace ? totalSize - freeSpace : 0;
            double freePercent = totalSize > 0 ? (double)freeSpace / totalSize * 100d : 0d;
            double usedPercent = totalSize > 0 ? (double)usedSpace / totalSize * 100d : 0d;
            var context = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase) {
                ["Index"] = index.ToString(CultureInfo.CurrentCulture),
                ["DriveLetter"] = NormalizeDriveLetter(drive.Name) ?? string.Empty,
                ["DriveRoot"] = drive.Name,
                ["Name"] = drive.Name,
                ["VolumeLabel"] = drive.VolumeLabel ?? string.Empty,
                ["DriveFormat"] = drive.DriveFormat ?? string.Empty,
                ["DriveType"] = drive.DriveType.ToString(),
                ["TotalSize"] = FormatBytes(totalSize),
                ["TotalSizeBytes"] = totalSize.ToString(CultureInfo.CurrentCulture),
                ["SizeRemaining"] = FormatBytes(freeSpace),
                ["SizeRemainingBytes"] = freeSpace.ToString(CultureInfo.CurrentCulture),
                ["FreeSpace"] = FormatBytes(freeSpace),
                ["FreeSpaceBytes"] = freeSpace.ToString(CultureInfo.CurrentCulture),
                ["UsedSpace"] = FormatBytes(usedSpace),
                ["UsedSpaceBytes"] = usedSpace.ToString(CultureInfo.CurrentCulture),
                ["FreePercent"] = freePercent.ToString("0.##", CultureInfo.CurrentCulture),
                ["UsedPercent"] = usedPercent.ToString("0.##", CultureInfo.CurrentCulture)
            };
            values.Add(context);
            index++;
        }

        return values;
    }

    private static string FormatBytes(ulong value) {
        string[] units = ["B", "KB", "MB", "GB", "TB", "PB"];
        double size = value;
        int unitIndex = 0;
        while (size >= 1024d && unitIndex < units.Length - 1) {
            size /= 1024d;
            unitIndex++;
        }

        return $"{size.ToString(size >= 100d || unitIndex == 0 ? "0" : "0.##", CultureInfo.CurrentCulture)} {units[unitIndex]}";
    }

    private static string? NormalizeDriveLetter(string? drive) {
        if (drive == null) {
            return null;
        }

        if (string.IsNullOrWhiteSpace(drive)) {
            return null;
        }

        var safeDrive = drive.Trim();
        if (safeDrive.Length == 0) {
            return null;
        }

        return safeDrive[0].ToString().ToUpperInvariant();
    }
}
