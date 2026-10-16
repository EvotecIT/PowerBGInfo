using System;
using ChartForgeX;
using ChartForgeX.Composition;
using Color = ChartForgeX.Primitives.ChartColor;

namespace PowerBGInfo;

internal static partial class BgInfoVisualCanvasRenderer {
    /// <summary>Tile height and gap for region stacks. They follow the rail defaults for the image height.</summary>
    internal static (double TileHeight, double Gap) ResolveRegionTileMetrics(BgInfoVisualCanvas visual, int imageHeight) {
        var tileHeight = visual.TileHeight > 0
            ? visual.TileHeight
            : Clamp(ApplyTileHeightPreset(visual.LayoutPreset, imageHeight * 0.092), Math.Min(56, imageHeight * 0.18), Math.Min(140, imageHeight * 0.26));
        var gap = ResolveTileGap(visual, Clamp(ApplyTileGapPreset(visual.LayoutPreset, imageHeight * 0.018), Math.Min(8, imageHeight * 0.02), Math.Min(28, imageHeight * 0.045)));
        return (tileHeight, gap);
    }

    /// <summary>Natural height of a region stack: every tile plus the gaps between them.</summary>
    internal static int MeasureRegionHeight(BgInfoVisualCanvas visual, int imageHeight) {
        var (tileHeight, gap) = ResolveRegionTileMetrics(visual, imageHeight);
        double total = 0;
        var count = 0;
        foreach (var tile in visual.Tiles) {
            total += tile.Height > 0 ? tile.Height : tileHeight;
            count++;
        }
        if (count == 0) return 1;
        return (int)Math.Ceiling(total + gap * (count - 1));
    }

    /// <summary>Renders the tiles as one column that fills the region slot, ignoring tile sides and rail offsets.</summary>
    public static BgInfoRasterImage RenderRegion(BgInfoVisualCanvas visual, int width, int height, int imageHeight) {
        if (visual == null) throw new ArgumentNullException(nameof(visual));
        width = Math.Max(1, width);
        height = Math.Max(1, height);
        var (tileHeight, gap) = ResolveRegionTileMetrics(visual, imageHeight);
        var canvas = VisualCanvas.Create(width, height)
            .WithTitle(string.Empty)
            .WithTheme(BuildTheme(visual), visual.ThemeMode)
            .ConfigureStyle(style => ApplyStyle(style, visual))
            .WithBackdrop(VisualCanvasBackdropStyle.Transparent);

        var y = 0d;
        foreach (var tile in visual.Tiles) {
            var itemHeight = tile.Height > 0 ? tile.Height : tileHeight;
            var itemWidth = tile.Width > 0 ? Math.Min(tile.Width, width) : width;
            canvas.AddInfoTileWithTextOptions(0, y, itemWidth, itemHeight,
                Resolve(tile.Icon), Resolve(tile.Label), Resolve(tile.Value),
                new VisualCanvasInfoTileTextOptions(
                    textScale: tile.TextScale ?? visual.TileTextScale,
                    labelFontSize: tile.LabelFontSize,
                    valueFontSize: tile.ValueFontSize,
                    detailFontSize: tile.DetailFontSize),
                Resolve(tile.Detail), tile.Accent, tile.Progress,
                MapSurfaceStyle(tile.SurfaceStyle), MapIconKind(tile.IconKind),
                MapMiniChartKind(tile.MiniChartKind), tile.MiniChartValues,
                tile.MiniChartMaximum, ResolveTextFitPolicy(visual, tile));
            y += itemHeight + gap;
        }

        var image = new BgInfoRasterImage();
        image.Create(string.Empty, width, height, Color.Transparent);
        image.DrawImage(canvas.ToRgbaImage(), 0, 0, width, height);
        return image;
    }
}
