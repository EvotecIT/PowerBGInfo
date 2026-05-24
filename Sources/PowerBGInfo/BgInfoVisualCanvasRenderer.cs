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
        var width = visual.Width > 0 ? visual.Width : Math.Max(1, targetWidth - visual.PositionX);
        var height = visual.Height > 0 ? visual.Height : Math.Max(1, targetHeight - visual.PositionY);
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
            canvas.AddFeatureStrip(290 * scaleX, 522 * scaleY, 620 * scaleX, 62 * scaleY, BuildFeatureItems(visual.Features));
        }
    }

    private static void BuildPowerBgInfoOverlay(VisualCanvas canvas, BgInfoVisualCanvas visual, int width, int height) {
        var accent = ToChartColor(visual.Accent);
        var marginX = Clamp(width * 0.045, 72, 132);
        var tileWidth = Clamp(width * 0.18, 380, 470);
        var tileHeight = Clamp(height * 0.092, 92, 106);
        var tileGap = Clamp(height * 0.018, 16, 24);
        var railY = Clamp(height * 0.16, 92, 190);
        AddTiles(canvas, visual.Tiles, BgInfoVisualCanvasSide.Left, marginX, railY, tileWidth, tileHeight, tileGap);
        AddTiles(canvas, visual.Tiles, BgInfoVisualCanvasSide.Right, width - marginX - tileWidth, railY, tileWidth, tileHeight, tileGap);

        var centerLeft = marginX + tileWidth + 48;
        var centerRight = width - marginX - tileWidth - 48;
        var centerWidth = Math.Max(360, centerRight - centerLeft);
        var badgeWidth = Clamp(width * 0.065, 104, 144);
        var badgeHeight = Clamp(height * 0.085, 76, 100);
        var badgeY = Clamp(height * 0.23, 156, 260);
        var titleFont = Clamp(width * 0.042, 62, 96);
        var titleY = Clamp(height * 0.39, 300, 450);
        var subtitleFont = Clamp(width * 0.014, 20, 30);
        canvas
            .AddHeroBadge(centerLeft + (centerWidth - badgeWidth) / 2, badgeY, badgeWidth, badgeHeight, ">_", accent)
            .AddHeroTitle(centerLeft, titleY, centerWidth, titleFont, SplitTitle(Resolve(visual.Title), canvas.Theme))
            .AddText(centerLeft, titleY + titleFont + 32, centerWidth, Resolve(visual.Subtitle), subtitleFont, canvas.Theme.SubtitleColor, VisualCanvasTextAlignment.Center);
        if (visual.Features.Count > 0) {
            var stripWidth = Clamp(centerWidth * 0.72, 520, 760);
            canvas.AddFeatureStrip(centerLeft + (centerWidth - stripWidth) / 2, height - Clamp(height * 0.18, 132, 190), stripWidth, 64, BuildFeatureItems(visual.Features));
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

    private static double Clamp(double value, double min, double max) => Math.Max(min, Math.Min(max, value));

    private static void DrawPng(GdiImage image, byte[] png, float x, float y, float width, float height) {
        image.WithGraphics(graphics => {
            using var stream = new MemoryStream(png);
            using var bitmap = System.Drawing.Image.FromStream(stream);
            graphics.DrawImage(bitmap, x, y, width, height);
        });
    }
}
