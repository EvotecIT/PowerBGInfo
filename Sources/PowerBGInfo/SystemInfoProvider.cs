using System.Globalization;
using System.Runtime.InteropServices;

namespace PowerBGInfo;

/// <summary>Provides built-in system values through the provider supported by this build and operating system.</summary>
public static class SystemInfoProvider {
    /// <summary>Text used when a fact cannot be read.</summary>
    public const string Unknown = "Unknown";

    /// <summary>
    /// The provider selected for this process. Windows retains WMI facts; Linux and macOS
    /// use their native and platform-neutral facts without invoking Windows APIs.
    /// </summary>
    public static IBgInfoSystemInfoProvider DefaultProvider { get; } = CreateProvider();

    private static IBgInfoSystemInfoProvider CreateProvider() {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) return new WindowsSystemInfoProvider();
        return new PortableSystemInfoProvider();
    }

    /// <summary>Resolves a built-in key, or an empty string for an unrecognized key.</summary>
    /// <param name="builtin">Built-in key such as HostName, OSName or Uptime.</param>
    /// <returns>The fact value; unavailable recognized values use Unknown where the existing contract requires it.</returns>
    public static string GetValue(string builtin) => DefaultProvider.GetValue(builtin);

    /// <summary>Collects a snapshot. Unavailable facts remain Unknown or null and are identified in FactStates.</summary>
    /// <returns>Facts collected for this machine and process.</returns>
    public static BgInfoSystemInfo GetSnapshot() => DefaultProvider.GetSnapshot();

    /// <summary>Reads a numeric chart metric without substituting zero for an unavailable measurement.</summary>
    /// <param name="metric">Metric to read.</param>
    /// <param name="argument">Optional drive letter or mount path.</param>
    /// <param name="value">Measurement when the method returns true.</param>
    /// <returns>Whether a finite measurement was available.</returns>
    public static bool TryGetNumericValue(BgInfoChartMetric metric, string? argument, out double value) =>
        DefaultProvider.TryGetNumericValue(metric, argument, out value);

    internal static string ResolveMachineDomain(string? machineDomain) => OrUnknown(machineDomain);

    /// <summary>Formats an uptime as days and hours, or hours and minutes under a day.</summary>
    public static string FormatUptime(TimeSpan? uptime) {
        if (!uptime.HasValue || uptime.Value < TimeSpan.Zero) return Unknown;
        var value = uptime.Value;
        if (value.TotalDays >= 1) {
            var days = (int)value.TotalDays;
            return $"{days} {(days == 1 ? "day" : "days")} {value.Hours} h";
        }
        return value.TotalHours >= 1 ? $"{(int)value.TotalHours} h {value.Minutes} min" : $"{Math.Max(0, value.Minutes)} min";
    }

    /// <summary>Summarizes fixed disks as used percentages, for example C: 61 % · D: 88 %.</summary>
    public static string FormatDiskSummary(IEnumerable<BgInfoDiskInfo> disks) {
        var parts = disks.Select(disk => $"{disk.Drive} {Math.Round(disk.UsedPercent).ToString(CultureInfo.CurrentCulture)} %").ToArray();
        return parts.Length == 0 ? Unknown : string.Join(" · ", parts);
    }

    internal static string FormatPatch((string Id, DateTime? Date) patch) {
        if (!patch.Date.HasValue) return patch.Id;
        var date = patch.Date.Value.ToString("d", CultureInfo.CurrentCulture);
        return patch.Id == Unknown ? date : $"{patch.Id} · {date}";
    }

    /// <summary>Parses the locale-independent forms Win32_QuickFixEngineering uses for InstalledOn.</summary>
    internal static DateTime? ParsePatchDate(string? text) {
        if (string.IsNullOrWhiteSpace(text)) return null;
        var value = text!.Trim();
        var invariantFormats = new[] { "d-M-yyyy", "dd-MM-yyyy", "yyyyMMdd", "yyyy-MM-dd" };
        if (DateTime.TryParseExact(value, invariantFormats, CultureInfo.InvariantCulture, DateTimeStyles.None, out var exact)) return exact;
        if (DateTime.TryParseExact(value, "d", CultureInfo.CurrentCulture, DateTimeStyles.None, out var localized)) return localized;
        var legacyFormats = new[] { "M/d/yyyy", "MM/dd/yyyy" };
        if (DateTime.TryParseExact(value, legacyFormats, CultureInfo.InvariantCulture, DateTimeStyles.None, out exact)) return exact;
        // Some systems store a FILETIME as hexadecimal text.
        if (long.TryParse(value, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var fileTime) && fileTime > 0) {
            try { return DateTime.FromFileTime(fileTime); } catch (ArgumentOutOfRangeException) { return null; }
        }
        return null;
    }

    internal static string OrUnknown(string? value) => string.IsNullOrWhiteSpace(value) ? Unknown : value!.Trim();
}
