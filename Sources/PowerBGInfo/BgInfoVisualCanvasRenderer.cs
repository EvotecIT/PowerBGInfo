using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using ChartForgeX;
using ChartForgeX.Composition;
using ChartForgeX.Primitives;
using ImagePlayground.Gdi;
using GdiImage = ImagePlayground.Gdi.Image;

namespace PowerBGInfo;

internal static class BgInfoVisualCanvasRenderer {
    public static GdiImage Render(BgInfoVisualCanvas visual, BgInfoConfiguration config, int targetWidth, int targetHeight) {
        if (visual == null) throw new ArgumentNullException(nameof(visual));
        var width = visual.Width > 0 ? visual.Width : Math.Max(1, targetWidth);
        var height = visual.Height > 0 ? visual.Height : Math.Max(1, targetHeight);
        var chartCanvas = BuildCanvas(visual, width, height);
        var image = new GdiImage();
        image.Create(string.Empty, width, height, Color.Transparent);
        DrawPng(image, chartCanvas.ToPng(), 0, 0, width, height);
        return image;
    }

    private static VisualCanvas BuildCanvas(BgInfoVisualCanvas visual, int width, int height) {
        var canvas = VisualCanvas.Create(width, height)
            .WithTitle(Resolve(visual.Title))
            .WithTheme(BuildTheme(visual))
            .WithBackground(ToChartColor(visual.BackgroundTop), ToChartColor(visual.BackgroundBottom))
            .WithBackdrop(visual.Transparent ? VisualCanvasBackdropStyle.Transparent : (visual.TechBackdrop ? VisualCanvasBackdropStyle.TechHorizon : VisualCanvasBackdropStyle.Plain));

        switch (visual.Template) {
            case BgInfoVisualCanvasTemplate.PowerBgInfoHero:
                BuildPowerBgInfoHero(canvas, visual, width, height);
                return canvas;
            default:
                throw new NotSupportedException("Unsupported visual canvas template: " + visual.Template);
        }
    }

    private static void BuildPowerBgInfoHero(VisualCanvas canvas, BgInfoVisualCanvas visual, int width, int height) {
        if (visual.Transparent) {
            BuildPowerBgInfoOverlay(canvas, visual, width, height);
            return;
        }

        var scaleX = width / 1200.0;
        var scaleY = height / 630.0;
        var accent = ToChartColor(visual.Accent);
        AddTiles(canvas, visual.Tiles, BgInfoVisualCanvasSide.Left, 48 * scaleX, 92 * scaleY, 300 * scaleX, 82 * scaleY, 16 * scaleY);
        AddTiles(canvas, visual.Tiles, BgInfoVisualCanvasSide.Right, 852 * scaleX, 70 * scaleY, 300 * scaleX, 96 * scaleY, 18 * scaleY);
        canvas
            .AddHeroBadge(538 * scaleX, 157 * scaleY, 124 * scaleX, 88 * scaleY, ">_", accent)
            .AddHeroTitle(312 * scaleX, 296 * scaleY, 576 * scaleX, 82 * scaleY, SplitTitle(Resolve(visual.Title), canvas.Theme))
            .AddText(240 * scaleX, 402 * scaleY, 720 * scaleX, Resolve(visual.Subtitle), 24 * scaleY, canvas.Theme.SubtitleColor, VisualCanvasTextAlignment.Center);
        if (visual.Features.Count > 0) {
            var stripWidth = ResolveFeatureWidth(visual, 620 * scaleX);
            var stripHeight = ResolveFeatureHeight(visual, 62 * scaleY);
            if (visual.FeatureAnchor.HasValue) {
                canvas.AddFeatureStrip(ToPlacement(visual.FeatureAnchor.Value, visual.FeatureOffsetX, visual.FeatureOffsetY), stripWidth, stripHeight, BuildFeatureItems(visual.Features));
            } else {
                canvas.AddFeatureStrip(290 * scaleX, 522 * scaleY, stripWidth, stripHeight, BuildFeatureItems(visual.Features));
            }
        }
    }

    private static void BuildPowerBgInfoOverlay(VisualCanvas canvas, BgInfoVisualCanvas visual, int width, int height) {
        var accent = ToChartColor(visual.Accent);
        var marginX = Clamp(width * 0.045, Math.Min(24, width * 0.04), Math.Min(132, width * 0.08));
        var railBudget = Math.Max(1, (width - (marginX * 2) - 96) / 2);
        var tileWidth = Math.Min(Clamp(width * 0.18, Math.Min(180, railBudget), Math.Min(470, railBudget)), railBudget);
        var tileHeight = Clamp(height * 0.092, Math.Min(56, height * 0.18), Math.Min(106, height * 0.22));
        var tileGap = Clamp(height * 0.018, Math.Min(8, height * 0.02), Math.Min(24, height * 0.04));
        var railY = Clamp(height * 0.16, Math.Min(32, height * 0.08), Math.Min(190, height * 0.25));
        AddTiles(canvas, visual.Tiles, BgInfoVisualCanvasSide.Left, marginX, railY, tileWidth, tileHeight, tileGap);
        AddTiles(canvas, visual.Tiles, BgInfoVisualCanvasSide.Right, width - marginX - tileWidth, railY, tileWidth, tileHeight, tileGap);

        var centerLeft = marginX + tileWidth + 48;
        var centerRight = width - marginX - tileWidth - 48;
        var centerWidth = Math.Max(1, centerRight - centerLeft);
        var badgeWidth = Math.Min(centerWidth, Clamp(width * 0.065, Math.Min(64, centerWidth), Math.Min(144, centerWidth)));
        var badgeHeight = Clamp(height * 0.085, Math.Min(42, height * 0.16), Math.Min(100, height * 0.22));
        var badgeY = Clamp(height * 0.23, Math.Min(24, height * 0.08), Math.Max(24, height - badgeHeight - 24));
        var titleFont = Clamp(width * 0.042, Math.Min(28, height * 0.09), Math.Min(96, height * 0.16));
        var subtitleFont = Clamp(width * 0.014, Math.Min(12, height * 0.035), Math.Min(30, height * 0.07));
        var subtitleGap = Clamp(height * 0.035, 8, 32);
        var titleTopMin = badgeY + badgeHeight + Math.Min(12, height * 0.025);
        var titleTopMax = Math.Max(titleTopMin, height - titleFont - subtitleFont - subtitleGap - Math.Min(24, height * 0.06));
        var titleY = Clamp(height * 0.39, titleTopMin, titleTopMax);
        canvas
            .AddHeroBadge(centerLeft + (centerWidth - badgeWidth) / 2, badgeY, badgeWidth, badgeHeight, ">_", accent)
            .AddHeroTitle(centerLeft, titleY, centerWidth, titleFont, SplitTitle(Resolve(visual.Title), canvas.Theme))
            .AddText(centerLeft, titleY + titleFont + subtitleGap, centerWidth, Resolve(visual.Subtitle), subtitleFont, canvas.Theme.SubtitleColor, VisualCanvasTextAlignment.Center);
        if (visual.Features.Count > 0) {
            var stripHeight = Clamp(height * 0.075, Math.Min(36, height * 0.1), Math.Min(64, height * 0.14));
            var stripWidth = Clamp(centerWidth * 0.72, Math.Min(120, centerWidth), Math.Min(760, centerWidth));
            stripWidth = ResolveFeatureWidth(visual, stripWidth);
            stripHeight = ResolveFeatureHeight(visual, stripHeight);
            if (visual.FeatureAnchor.HasValue) {
                canvas.AddFeatureStrip(ToPlacement(visual.FeatureAnchor.Value, visual.FeatureOffsetX, visual.FeatureOffsetY), stripWidth, stripHeight, BuildFeatureItems(visual.Features));
            } else {
                var stripY = Math.Max(0, height - Clamp(height * 0.18, stripHeight + 8, Math.Min(190, height * 0.28)));
                canvas.AddFeatureStrip(centerLeft + (centerWidth - stripWidth) / 2, stripY, stripWidth, stripHeight, BuildFeatureItems(visual.Features));
            }
        }
    }

    private static void AddTiles(VisualCanvas canvas, IReadOnlyList<BgInfoVisualCanvasTile> tiles, BgInfoVisualCanvasSide side, double x, double y, double width, double height, double gap) {
        var index = 0;
        foreach (var tile in tiles) {
            if (tile.Side != side) continue;
            canvas.AddInfoTile(
                x,
                y + index * (height + gap),
                width,
                height,
                Resolve(tile.Icon),
                Resolve(tile.Label),
                Resolve(tile.Value),
                Resolve(tile.Detail),
                tile.Accent.HasValue ? ToChartColor(tile.Accent.Value) : (ChartColor?)null,
                tile.Progress,
                MapSurfaceStyle(tile.SurfaceStyle),
                MapIconKind(tile.IconKind),
                MapMiniChartKind(tile.MiniChartKind),
                tile.MiniChartValues,
                tile.MiniChartMaximum);
            index++;
        }
    }

    private static IEnumerable<VisualCanvasTextRun> SplitTitle(string title, VisualCanvasTheme theme) {
        if (title.EndsWith("BGInfo", StringComparison.OrdinalIgnoreCase) && title.Length > "BGInfo".Length) {
            yield return new VisualCanvasTextRun(title.Substring(0, title.Length - "BGInfo".Length), theme.HeroTitleColor);
            yield return new VisualCanvasTextRun(title.Substring(title.Length - "BGInfo".Length), theme.HeroTitleAccentColor);
            yield break;
        }

        yield return new VisualCanvasTextRun(title, theme.HeroTitleColor);
    }

    private static IEnumerable<VisualCanvasFeatureItem> BuildFeatureItems(IEnumerable<BgInfoVisualCanvasFeature> features) {
        foreach (var feature in features) yield return new VisualCanvasFeatureItem(Resolve(feature.Icon), Resolve(feature.Label));
    }

    private static string Resolve(string? value) => BgInfoVariableResolver.RenderTemplate(value, new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase));

    private static ChartColor ToChartColor(Color color) => ChartColor.FromRgba(color.R, color.G, color.B, color.A);

    private static VisualCanvasTheme BuildTheme(BgInfoVisualCanvas visual) {
        var theme = new VisualCanvasTheme {
            Accent = ToChartColor(visual.Accent),
            HeroTitleAccentColor = ToChartColor(visual.Accent),
            ImagePlaceholderStroke = ToChartColor(visual.Accent).WithOpacity(0.34)
        };
        ApplyColor(visual.SecondaryAccent, value => theme.SecondaryAccent = value);
        ApplyColor(visual.TitleColor, value => theme.HeroTitleColor = value);
        ApplyColor(visual.TitleAccentColor, value => theme.HeroTitleAccentColor = value);
        ApplyColor(visual.SubtitleColor, value => theme.SubtitleColor = value);
        ApplyColor(visual.TileGlassTop, value => theme.TileGlassTop = value);
        ApplyColor(visual.TileGlassBottom, value => theme.TileGlassBottom = value);
        ApplyColor(visual.TileLabelColor, value => theme.TileLabelColor = value);
        ApplyColor(visual.TileValueColor, value => theme.TileValueColor = value);
        ApplyColor(visual.TileDetailColor, value => theme.TileDetailColor = value);
        ApplyColor(visual.TileProgressTrackColor, value => theme.TileProgressTrackColor = value);
        ApplyColor(visual.HeroBadgeTop, value => theme.HeroBadgeTop = value);
        ApplyColor(visual.HeroBadgeBottom, value => theme.HeroBadgeBottom = value);
        ApplyColor(visual.HeroBadgeTextColor, value => theme.HeroBadgeTextColor = value);
        return theme;
    }

    private static void ApplyColor(Color? color, Action<ChartColor> setter) {
        if (color.HasValue) setter(ToChartColor(color.Value));
    }

    private static VisualCanvasInfoTileSurfaceStyle MapSurfaceStyle(BgInfoVisualCanvasTileSurfaceStyle style) {
        switch (style) {
            case BgInfoVisualCanvasTileSurfaceStyle.Outline: return VisualCanvasInfoTileSurfaceStyle.Outline;
            case BgInfoVisualCanvasTileSurfaceStyle.Raised: return VisualCanvasInfoTileSurfaceStyle.Raised;
            default: return VisualCanvasInfoTileSurfaceStyle.Glass;
        }
    }

    private static VisualCanvasInfoTileMiniChartKind MapMiniChartKind(BgInfoVisualCanvasTileMiniChartKind kind) {
        switch (kind) {
            case BgInfoVisualCanvasTileMiniChartKind.Sparkline: return VisualCanvasInfoTileMiniChartKind.Sparkline;
            case BgInfoVisualCanvasTileMiniChartKind.Area: return VisualCanvasInfoTileMiniChartKind.Area;
            case BgInfoVisualCanvasTileMiniChartKind.Bars: return VisualCanvasInfoTileMiniChartKind.Bars;
            default: return VisualCanvasInfoTileMiniChartKind.None;
        }
    }

    private static VisualCanvasInfoTileIconKind MapIconKind(BgInfoVisualCanvasTileIconKind kind) {
        switch (kind) {
            case BgInfoVisualCanvasTileIconKind.Computer: return VisualCanvasInfoTileIconKind.Computer;
            case BgInfoVisualCanvasTileIconKind.Network: return VisualCanvasInfoTileIconKind.Network;
            case BgInfoVisualCanvasTileIconKind.OperatingSystem: return VisualCanvasInfoTileIconKind.OperatingSystem;
            case BgInfoVisualCanvasTileIconKind.Cpu: return VisualCanvasInfoTileIconKind.Cpu;
            case BgInfoVisualCanvasTileIconKind.Memory: return VisualCanvasInfoTileIconKind.Memory;
            case BgInfoVisualCanvasTileIconKind.User: return VisualCanvasInfoTileIconKind.User;
            case BgInfoVisualCanvasTileIconKind.Domain: return VisualCanvasInfoTileIconKind.Domain;
            case BgInfoVisualCanvasTileIconKind.Terminal: return VisualCanvasInfoTileIconKind.Terminal;
            case BgInfoVisualCanvasTileIconKind.Storage: return VisualCanvasInfoTileIconKind.Storage;
            case BgInfoVisualCanvasTileIconKind.Shield: return VisualCanvasInfoTileIconKind.Shield;
            default: return VisualCanvasInfoTileIconKind.Text;
        }
    }

    private static double ResolveFeatureWidth(BgInfoVisualCanvas visual, double defaultWidth) => visual.FeatureWidth > 0 ? visual.FeatureWidth : defaultWidth;

    private static double ResolveFeatureHeight(BgInfoVisualCanvas visual, double defaultHeight) => visual.FeatureHeight > 0 ? visual.FeatureHeight : defaultHeight;

    private static VisualCanvasPlacement ToPlacement(BgInfoTextPosition anchor, double offsetX, double offsetY) {
        switch (anchor) {
            case BgInfoTextPosition.TopCenter: return VisualCanvasPlacement.At(VisualCanvasAnchor.TopCenter, offsetX, offsetY);
            case BgInfoTextPosition.TopRight: return VisualCanvasPlacement.At(VisualCanvasAnchor.TopRight, offsetX, offsetY);
            case BgInfoTextPosition.MiddleLeft: return VisualCanvasPlacement.At(VisualCanvasAnchor.MiddleLeft, offsetX, offsetY);
            case BgInfoTextPosition.MiddleCenter: return VisualCanvasPlacement.At(VisualCanvasAnchor.Center, offsetX, offsetY);
            case BgInfoTextPosition.MiddleRight: return VisualCanvasPlacement.At(VisualCanvasAnchor.MiddleRight, offsetX, offsetY);
            case BgInfoTextPosition.BottomLeft: return VisualCanvasPlacement.At(VisualCanvasAnchor.BottomLeft, offsetX, offsetY);
            case BgInfoTextPosition.BottomCenter: return VisualCanvasPlacement.At(VisualCanvasAnchor.BottomCenter, offsetX, offsetY);
            case BgInfoTextPosition.BottomRight: return VisualCanvasPlacement.At(VisualCanvasAnchor.BottomRight, offsetX, offsetY);
            default: return VisualCanvasPlacement.At(VisualCanvasAnchor.TopLeft, offsetX, offsetY);
        }
    }

    private static double Clamp(double value, double min, double max) {
        if (max < min) max = min;
        return Math.Max(min, Math.Min(max, value));
    }

    private static void DrawPng(GdiImage image, byte[] png, float x, float y, float width, float height) {
        image.WithGraphics(graphics => {
            using var stream = new MemoryStream(png);
            using var bitmap = System.Drawing.Image.FromStream(stream);
            graphics.DrawImage(bitmap, x, y, width, height);
        });
    }
}
