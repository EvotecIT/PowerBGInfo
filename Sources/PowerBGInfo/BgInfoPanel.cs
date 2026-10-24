using System;
using Color = ChartForgeX.Primitives.ChartColor;
using ChartForgeX.Composition;
using ChartForgeX.Typography;

namespace PowerBGInfo;

/// <summary>Shared panel surface for chart and topology overlays.</summary>
internal static class BgInfoPanel {
    public static BgInfoRasterImage Create(int width, int height, Color? background, Color? border, double cornerRadius) {
        return new BgInfoRasterImage(ImageComposition.CreatePanel(width, height, background ?? Color.Transparent,
            border, Math.Max(0, cornerRadius)));
    }

    /// <summary>Draws a one-row header with a left title and an optional right-aligned note.</summary>
    /// <returns>The header height in pixels, or zero when nothing was drawn.</returns>
    public static double DrawHeader(BgInfoRasterImage image, string? title, string? note, TextStyle style, double x, double y, double width) {
        if (width <= 0) return 0;
        return image.Composition.DrawTextHeader(x, y, width, title, note, style);
    }
}
