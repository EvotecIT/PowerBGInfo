using System;
using System.Drawing;
using ChartForgeX.Primitives;

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
        if (ChartColor.TryParse(trimmed, out var chartColor)) {
            color = Color.FromArgb(chartColor.A, chartColor.R, chartColor.G, chartColor.B);
            return true;
        }

        return false;
    }

    /// <summary>
    /// Formats a color as #RRGGBBAA.
    /// </summary>
    /// <param name="color">Color value.</param>
    /// <returns>Formatted color string.</returns>
    public static string ToHex(Color color) {
        return ChartColor.FromRgba(color.R, color.G, color.B, color.A).ToHexRgba();
    }
}
