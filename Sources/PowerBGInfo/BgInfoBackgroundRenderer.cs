using System;
using System.IO;
using ChartForgeX.Composition;
using ChartForgeX.Primitives;

namespace PowerBGInfo;

internal static class BgInfoBackgroundRenderer {
    public static BgInfoRasterImage Render(BgInfoBackground background, int width, int height) =>
        Render(background, width, height, background?.ImagePath);

    /// <summary>Renders the backdrop with <paramref name="imagePath"/> in place of the background's own image (null: no image layer).</summary>
    public static BgInfoRasterImage Render(BgInfoBackground background, int width, int height, string? imagePath) {
        if (background == null) throw new ArgumentNullException(nameof(background));
        if (width <= 0) throw new ArgumentOutOfRangeException(nameof(width), width, "Background width must be positive.");
        if (height <= 0) throw new ArgumentOutOfRangeException(nameof(height), height, "Background height must be positive.");

        var image = new BgInfoRasterImage();
        image.Create(string.Empty, width, height, background.Color);
        var composition = image.Composition;

        if (!string.IsNullOrWhiteSpace(imagePath)) {
            if (!File.Exists(imagePath)) {
                throw new FileNotFoundException("BGInfo background image file was not found.", imagePath);
            }

            using var backgroundImage = BgInfoRasterImage.Load(imagePath!);
            composition.DrawImage(backgroundImage.ToRgbaImage(), 0, 0, width, height,
                BgInfoImageFitMapper.ToVisualCanvasFit(background.ImageFit), Clamp01(background.ImageOpacity));
        }

        if (background.GradientStart.HasValue && background.GradientEnd.HasValue) {
            composition.FillRectangleLinearGradient(0, 0, width, height,
                background.GradientStart.Value, background.GradientEnd.Value, background.GradientAngle);
        }

        var longestSide = Math.Max(width, height);
        foreach (var glow in background.Glows) {
            if (glow == null || glow.Radius <= 0 || glow.Color.A == 0) continue;
            var outer = ChartColor.FromRgba(glow.Color.R, glow.Color.G, glow.Color.B, 0);
            composition.FillRectangleRadialGradient(0, 0, width, height,
                glow.X * width, glow.Y * height, glow.Radius * longestSide, glow.Color, outer);
        }

        DrawPattern(composition, background);
        return image;
    }

    private static void DrawPattern(ImageComposition composition, BgInfoBackground background) {
        if (background.Pattern == BgInfoBackgroundPattern.None || background.PatternColor.A == 0) return;
        var spacing = Math.Max(8, background.PatternSpacing);
        if (background.Pattern == BgInfoBackgroundPattern.Grid) {
            composition.FillGridPattern(background.PatternColor, spacing);
            return;
        }
        composition.FillDotPattern(background.PatternColor, spacing, Math.Max(0.8, spacing / 40d));
    }

    private static double Clamp01(double value) => double.IsNaN(value) ? 1d : Math.Max(0d, Math.Min(1d, value));
}
