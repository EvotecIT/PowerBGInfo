using System;
using System.Drawing;
using System.Globalization;

namespace PowerBGInfo;

/// <summary>
/// Provides color parsing helpers for BGInfo configuration.
/// </summary>
public static class BgInfoColorParser {
    /// <summary>
    /// Attempts to parse a color from a string representation.
    /// </summary>
    /// <param name="text">Color string (name or hex).</param>
    /// <param name="color">Parsed color.</param>
    /// <returns>True when parsing succeeds.</returns>
    public static bool TryParse(string text, out Color color) {
        color = default;
        if (string.IsNullOrWhiteSpace(text)) {
            return false;
        }

        var trimmed = text.Trim();
        if (TryParseHexColor(trimmed, out color)) {
            return true;
        }

        var named = Color.FromName(trimmed);
        if (named.IsKnownColor || named.IsNamedColor || named.A != 0 ||
            trimmed.Equals("Transparent", StringComparison.OrdinalIgnoreCase)) {
            color = named;
            return true;
        }

        return false;
    }

    /// <summary>
    /// Formats a color as #AARRGGBB.
    /// </summary>
    /// <param name="color">Color value.</param>
    /// <returns>Formatted color string.</returns>
    public static string ToHex(Color color) {
        return $"#{color.A:X2}{color.R:X2}{color.G:X2}{color.B:X2}";
    }

    private static bool TryParseHexColor(string text, out Color color) {
        color = default;
        var trimmed = text.Trim();
        var hasPrefix = trimmed.StartsWith("#", StringComparison.Ordinal);
        if (hasPrefix) {
            trimmed = trimmed.Substring(1);
        }

        if (trimmed.Length != 6 && trimmed.Length != 8) {
            return false;
        }

        if (!byte.TryParse(trimmed.Substring(0, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var first) ||
            !byte.TryParse(trimmed.Substring(2, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var second) ||
            !byte.TryParse(trimmed.Substring(4, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var third)) {
            return false;
        }

        if (trimmed.Length == 6) {
            color = Color.FromArgb(255, first, second, third);
            return true;
        }

        if (!byte.TryParse(trimmed.Substring(6, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var fourth)) {
            return false;
        }

        if (hasPrefix) {
            color = Color.FromArgb(first, second, third, fourth);
            return true;
        }

        color = Color.FromArgb(fourth, first, second, third);
        return true;
    }
}
