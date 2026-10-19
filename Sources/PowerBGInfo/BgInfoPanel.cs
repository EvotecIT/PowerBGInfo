using System;
using Color = ChartForgeX.Primitives.ChartColor;
using ChartForgeX.Typography;

namespace PowerBGInfo;

/// <summary>Shared panel surface for chart and topology overlays.</summary>
internal static class BgInfoPanel {
    public static BgInfoRasterImage Create(int width, int height, Color? background, Color? border, double cornerRadius) {
        var image = new BgInfoRasterImage();
        var fill = background ?? Color.Transparent;
        var radius = Math.Min(Math.Max(0, cornerRadius), Math.Min(width, height) / 2d);
        if (radius <= 0 && !border.HasValue) {
            image.Create(string.Empty, width, height, fill);
            return image;
        }

        image.Create(string.Empty, width, height, Color.Transparent);
        if (fill.A > 0) {
            image.Composition.FillRoundedRectangle(0, 0, width, height, radius, fill);
        }
        if (border is { A: > 0 } stroke) {
            // The stroke is painted inside the rectangle, so the full bounds keep the outline on whole pixels.
            image.Composition.StrokeRoundedRectangle(0, 0, width, height, radius, stroke);
        }

        return image;
    }

    /// <summary>Draws a one-row header with a left title and an optional right-aligned note.</summary>
    /// <returns>The header height in pixels, or zero when nothing was drawn.</returns>
    public static double DrawHeader(BgInfoRasterImage image, string? title, string? note, TextStyle style, double x, double y, double width) {
        if (width <= 0) return 0;
        var noteWidth = 0d;
        var height = 0d;
        if (!string.IsNullOrWhiteSpace(note)) {
            var noteStyle = style.Clone();
            noteStyle.TextCase = TextCaseTransform.None;
            var noteLayout = TextLayoutEngine.Layout(note!, width, noteStyle, TextWrapMode.NoWrap, 1, TextTrimming.Ellipsis);
            foreach (var line in noteLayout.Lines) {
                noteWidth = Math.Max(noteWidth, line.Width);
                image.AddText(x + Math.Max(0, width - line.Width), y, line.Text, noteStyle);
            }
            height = noteLayout.Metrics.Height;
        }

        var titleWidth = Math.Max(0, width - (noteWidth > 0 ? noteWidth + 12 : 0));
        if (!string.IsNullOrWhiteSpace(title) && titleWidth > 0) {
            var titleLayout = TextLayoutEngine.Layout(title!, titleWidth, style, TextWrapMode.NoWrap, 1, TextTrimming.Ellipsis);
            var titleStyle = style.Clone();
            // The layout already applied casing; applying ToggleCase twice would undo it.
            titleStyle.TextCase = TextCaseTransform.None;
            foreach (var line in titleLayout.Lines) {
                image.AddText(x, y, line.Text, titleStyle);
            }
            height = Math.Max(height, titleLayout.Metrics.Height);
        }

        return height;
    }
}
