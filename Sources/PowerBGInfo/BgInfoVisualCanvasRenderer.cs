using System;
using System.Collections.Generic;
using Color = ChartForgeX.Primitives.ChartColor;
using System.IO;
using ChartForgeX;
using ChartForgeX.Composition;
using ChartForgeX.Primitives;
using ChartForgeX.Typography;
using ChartForgeX.Themes;

namespace PowerBGInfo;

internal static partial class BgInfoVisualCanvasRenderer {
    public static BgInfoRasterImage Render(BgInfoVisualCanvas visual, BgInfoConfiguration config, int targetWidth, int targetHeight) {
        if (visual == null) throw new ArgumentNullException(nameof(visual));
        var width = visual.Width > 0 ? visual.Width : Math.Max(1, targetWidth);
        var height = visual.Height > 0 ? visual.Height : Math.Max(1, targetHeight);
        var chartCanvas = BuildCanvas(visual, width, height);
        var image = new BgInfoRasterImage();
        image.Create(string.Empty, width, height, Color.Transparent);
        image.DrawImage(chartCanvas.ToRgbaImage(), 0, 0, width, height);
        return image;
    }

    private static VisualCanvas BuildCanvas(BgInfoVisualCanvas visual, int width, int height) {
        var canvas = VisualCanvas.Create(width, height)
            .WithTitle(Resolve(visual.Title))
            .WithTheme(BuildTheme(visual), visual.ThemeMode)
            .ConfigureStyle(style => ApplyStyle(style, visual))
            .WithBackground(visual.BackgroundTop, visual.BackgroundBottom)
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
        var accent = visual.Accent;
        var defaultTileWidth = ApplyTileWidthPreset(visual.LayoutPreset, 300 * scaleX);
        var leftDefaultTileWidth = ResolveSideTileWidth(visual, BgInfoVisualCanvasSide.Left, defaultTileWidth);
        var rightDefaultTileWidth = ResolveSideTileWidth(visual, BgInfoVisualCanvasSide.Right, defaultTileWidth);
        var centerDefaultTileWidth = ResolveSideTileWidth(visual, BgInfoVisualCanvasSide.Center, defaultTileWidth);
        var leftTileWidth = ResolveRailWidth(visual, BgInfoVisualCanvasSide.Left, defaultTileWidth);
        var rightTileWidth = ResolveRailWidth(visual, BgInfoVisualCanvasSide.Right, defaultTileWidth);
        var centerTileWidth = ResolveRailWidth(visual, BgInfoVisualCanvasSide.Center, defaultTileWidth);
        var laneScale = ResolveCenteredLaneWidthScale(visual, width, 48 * scaleX, 48 * scaleX, leftTileWidth, centerTileWidth, rightTileWidth);
        leftDefaultTileWidth *= laneScale;
        rightDefaultTileWidth *= laneScale;
        centerDefaultTileWidth *= laneScale;
        leftTileWidth *= laneScale;
        rightTileWidth *= laneScale;
        centerTileWidth *= laneScale;
        var leftTileHeight = ApplyTileHeightPreset(visual.LayoutPreset, 82 * scaleY);
        var rightTileHeight = ApplyTileHeightPreset(visual.LayoutPreset, 96 * scaleY);
        var leftGap = ResolveTileGap(visual, ApplyTileGapPreset(visual.LayoutPreset, 16 * scaleY));
        var rightGap = ResolveTileGap(visual, ApplyTileGapPreset(visual.LayoutPreset, 18 * scaleY));
        var leftRailX = 48 * scaleX + visual.LeftTileOffsetX;
        var rightRailX = width - 48 * scaleX - rightTileWidth - visual.RightTileOffsetX;
        AddTiles(canvas, visual, BgInfoVisualCanvasSide.Left, leftRailX, 92 * scaleY + visual.LeftTileOffsetY, leftTileWidth, leftDefaultTileWidth, leftTileHeight, leftGap);
        AddTiles(canvas, visual, BgInfoVisualCanvasSide.Right, rightRailX, 70 * scaleY + visual.RightTileOffsetY, rightTileWidth, rightDefaultTileWidth, rightTileHeight, rightGap);
        AddTiles(canvas, visual, BgInfoVisualCanvasSide.Center, ResolveCenteredLaneX(visual, width, 48 * scaleX, 48 * scaleX, centerTileWidth, leftRailX, leftTileWidth, rightRailX), 70 * scaleY + visual.CenterTileOffsetY, centerTileWidth, centerDefaultTileWidth, rightTileHeight, rightGap);
        if (visual.HeroContentVisible) {
            AddHeroBadge(canvas, visual, 538 * scaleX, 157 * scaleY, 124 * scaleX, 88 * scaleY, accent);
            canvas
                .AddHeroTitle(312 * scaleX, 296 * scaleY, 576 * scaleX, 82 * scaleY, SplitTitle(Resolve(visual.Title), visual, canvas.Theme))
                .AddText(240 * scaleX, 402 * scaleY, 720 * scaleX, Resolve(visual.Subtitle), 24 * scaleY, visual.SubtitleColor ?? canvas.Theme.Resolve(visual.ThemeMode).MutedForeground, TextAlignment.Center);
        }
        if (visual.Features.Count > 0) {
            var stripWidth = ResolveFeatureWidth(visual, 620 * scaleX);
            var stripHeight = ResolveFeatureHeight(visual, 62 * scaleY);
            if (visual.FeatureAnchor.HasValue) {
                canvas.AddFeatureStrip(ToPlacement(visual.FeatureAnchor.Value, visual.FeatureOffsetX, visual.FeatureOffsetY), stripWidth, stripHeight, BuildFeatureItems(visual.Features));
            } else {
                var bounds = ResolveDefaultOpaqueFeatureStripBounds(visual, width, height, scaleX, scaleY);
                canvas.AddFeatureStrip(bounds.X, bounds.Y, bounds.Width, bounds.Height, BuildFeatureItems(visual.Features));
            }
        }
    }

    private static void BuildPowerBgInfoOverlay(VisualCanvas canvas, BgInfoVisualCanvas visual, int width, int height) {
        var accent = visual.Accent;
        var layout = ResolveOverlayRailLayout(visual, width, height);
        AddTiles(canvas, visual, BgInfoVisualCanvasSide.Left, layout.LeftRailX, layout.RailY + visual.LeftTileOffsetY, layout.LeftRailWidth, layout.LeftTileWidth, layout.TileHeight, layout.TileGap);
        AddTiles(canvas, visual, BgInfoVisualCanvasSide.Right, layout.RightRailX, layout.RailY + visual.RightTileOffsetY, layout.RightRailWidth, layout.RightTileWidth, layout.TileHeight, layout.TileGap);
        var centerTileWidth = layout.CenterRailWidth;
        var centerDefaultTileWidth = ResolveSideTileWidth(visual, BgInfoVisualCanvasSide.Center, layout.TemplateTileWidth) * layout.LaneWidthScale;
        AddTiles(canvas, visual, BgInfoVisualCanvasSide.Center, ResolveCenteredLaneX(visual, width, layout.MarginX, layout.LaneGap, centerTileWidth, layout.LeftRailX, layout.LeftRailWidth, layout.RightRailX), layout.RailY + visual.CenterTileOffsetY, centerTileWidth, centerDefaultTileWidth, layout.TileHeight, layout.TileGap);

        var centerLeft = layout.CenterLeft;
        var centerWidth = layout.CenterWidth;
        if (visual.HeroContentVisible) {
            var badgeWidth = Math.Min(centerWidth, Clamp(width * 0.065, Math.Min(64, centerWidth), Math.Min(144, centerWidth)));
            var badgeHeight = Clamp(height * 0.085, Math.Min(42, height * 0.16), Math.Min(100, height * 0.22));
            var badgeY = Clamp(height * 0.23, Math.Min(24, height * 0.08), Math.Max(24, height - badgeHeight - 24));
            var titleFont = Clamp(width * 0.042, Math.Min(28, height * 0.09), Math.Min(96, height * 0.16));
            var subtitleFont = Clamp(width * 0.014, Math.Min(12, height * 0.035), Math.Min(30, height * 0.07));
            var subtitleGap = Clamp(height * 0.035, 8, 32);
            var titleTopMin = badgeY + badgeHeight + Math.Min(12, height * 0.025);
            var titleTopMax = Math.Max(titleTopMin, height - titleFont - subtitleFont - subtitleGap - Math.Min(24, height * 0.06));
            var titleY = Clamp(height * 0.39, titleTopMin, titleTopMax);
            AddHeroBadge(canvas, visual, centerLeft + (centerWidth - badgeWidth) / 2, badgeY, badgeWidth, badgeHeight, accent);
            canvas
                .AddHeroTitle(centerLeft, titleY, centerWidth, titleFont, SplitTitle(Resolve(visual.Title), visual, canvas.Theme))
                .AddText(centerLeft, titleY + titleFont + subtitleGap, centerWidth, Resolve(visual.Subtitle), subtitleFont, visual.SubtitleColor ?? canvas.Theme.Resolve(visual.ThemeMode).MutedForeground, TextAlignment.Center);
        }
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

    private static Color? ResolveTileAccent(BgInfoVisualCanvas visual, BgInfoVisualCanvasTile tile) {
        if (tile.Accent.HasValue || tile.Status == BgInfoStatus.None) return tile.Accent;
        return (visual.StatusColors ?? BgInfoStatusColors.Default(visual.ThemeMode)).Resolve(tile.Status);
    }

    private static void AddTiles(VisualCanvas canvas, BgInfoVisualCanvas visual, BgInfoVisualCanvasSide side, double x, double y, double width, double defaultTileWidth, double height, double gap) {
        var cursorY = y;
        foreach (var tile in visual.Tiles) {
            if (tile.Side != side) continue;
            var tileWidth = Math.Min(ResolveTileWidth(visual, tile, defaultTileWidth), width);
            var tileHeight = ResolveTileHeight(visual, tile, height);
            double tileX;
            switch (side) {
                case BgInfoVisualCanvasSide.Right:
                    tileX = x + width - tileWidth;
                    break;
                case BgInfoVisualCanvasSide.Center:
                    tileX = x + (width - tileWidth) / 2;
                    break;
                default:
                    tileX = x;
                    break;
            }
            canvas.AddInfoTileWithTextOptions(
                tileX,
                cursorY,
                tileWidth,
                tileHeight,
                Resolve(tile.Icon),
                Resolve(tile.Label),
                Resolve(tile.Value),
                new VisualCanvasInfoTileTextOptions(
                    textScale: tile.TextScale ?? visual.TileTextScale,
                    labelFontSize: tile.LabelFontSize,
                    valueFontSize: tile.ValueFontSize,
                    detailFontSize: tile.DetailFontSize),
                Resolve(tile.Detail),
                ResolveTileAccent(visual, tile),
                tile.Progress,
                MapSurfaceStyle(tile.SurfaceStyle),
                MapIconKind(tile.IconKind),
                MapMiniChartKind(tile.MiniChartKind),
                tile.MiniChartValues,
                tile.MiniChartMaximum,
                ResolveTextFitPolicy(visual, tile));
            cursorY += tileHeight + ResolveTileGap(visual, tile, gap);
        }
    }

    private static double ResolveRailWidth(BgInfoVisualCanvas visual, BgInfoVisualCanvasSide side, double defaultWidth, double implicitMaximum = double.MaxValue) {
        var width = ResolveSideTileWidth(visual, side, defaultWidth);
        foreach (var tile in visual.Tiles) {
            if (tile.Side != side) continue;
            var tileWidth = ResolveTileWidth(visual, tile, defaultWidth);
            if (tile.Width <= 0 && !HasExplicitSideTileWidth(visual, side)) tileWidth = Math.Min(tileWidth, implicitMaximum);
            if (tileWidth > width) width = tileWidth;
        }

        return width;
    }

    private static double ResolveTileWidth(BgInfoVisualCanvas visual, BgInfoVisualCanvasTile tile, double defaultWidth) {
        if (tile.Width > 0) return tile.Width;
        if (HasExplicitSideTileWidth(visual, tile.Side)) {
            return ResolveSideTileWidth(visual, tile.Side, defaultWidth);
        }

        return defaultWidth * ResolveTileTextScale(visual, tile);
    }

    private static double ResolveTileHeight(BgInfoVisualCanvas visual, BgInfoVisualCanvasTile tile, double defaultHeight) {
        if (tile.Height > 0) return tile.Height;
        return visual.TileHeight > 0 ? visual.TileHeight : defaultHeight * ResolveTileTextScale(visual, tile);
    }

    private static bool HasExplicitSideTileWidth(BgInfoVisualCanvas visual, BgInfoVisualCanvasSide side) =>
        visual.TileWidth > 0 ||
        (side == BgInfoVisualCanvasSide.Left && visual.LeftTileWidth > 0) ||
        (side == BgInfoVisualCanvasSide.Right && visual.RightTileWidth > 0) ||
        (side == BgInfoVisualCanvasSide.Center && visual.CenterTileWidth > 0);

    private static double ResolveTileTextScale(BgInfoVisualCanvas visual, BgInfoVisualCanvasTile tile) =>
        tile.TextScale ?? visual.TileTextScale;

    private static double ResolveTileGap(BgInfoVisualCanvas visual, BgInfoVisualCanvasTile tile, double defaultGap) =>
        visual.TileGap > 0 ? visual.TileGap : defaultGap * ResolveTileTextScale(visual, tile);

    private static double ResolveSideTileWidth(BgInfoVisualCanvas visual, BgInfoVisualCanvasSide side, double defaultWidth) {
        if (side == BgInfoVisualCanvasSide.Left && visual.LeftTileWidth > 0) return visual.LeftTileWidth;
        if (side == BgInfoVisualCanvasSide.Right && visual.RightTileWidth > 0) return visual.RightTileWidth;
        if (side == BgInfoVisualCanvasSide.Center && visual.CenterTileWidth > 0) return visual.CenterTileWidth;
        return visual.TileWidth > 0 ? visual.TileWidth : defaultWidth;
    }

    private static double ResolveTileGap(BgInfoVisualCanvas visual, double defaultGap) => visual.TileGap > 0 ? visual.TileGap : defaultGap;

    /// <summary>Measures the same tile heights and preceding-tile gaps used when drawing a stack.</summary>
    private static double MeasureTileStackHeight(BgInfoVisualCanvas visual, IEnumerable<BgInfoVisualCanvasTile> tiles, double defaultHeight, double defaultGap) {
        double total = 0;
        BgInfoVisualCanvasTile? previous = null;
        foreach (var tile in tiles) {
            if (previous != null) total += ResolveTileGap(visual, previous, defaultGap);
            total += ResolveTileHeight(visual, tile, defaultHeight);
            previous = tile;
        }
        return total;
    }

    private static bool HasTiles(BgInfoVisualCanvas visual, BgInfoVisualCanvasSide side) {
        foreach (var tile in visual.Tiles) {
            if (tile.Side == side) return true;
        }

        return false;
    }

    private static double ResolveCenteredLaneWidthScale(BgInfoVisualCanvas visual, double width, double marginX, double laneGap, double leftWidth, double centerWidth, double rightWidth) {
        if (!HasTiles(visual, BgInfoVisualCanvasSide.Center)) return 1;

        var usableWidth = Math.Max(1, width - 2 * marginX);
        var scale = centerWidth > 0 ? Math.Min(1, usableWidth / centerWidth) : 1;
        if (HasTiles(visual, BgInfoVisualCanvasSide.Left)) {
            var available = width - 2 * marginX - 2 * laneGap - 2 * visual.LeftTileOffsetX;
            var required = 2 * leftWidth + centerWidth;
            if (required > 0) scale = Math.Min(scale, Math.Max(1, available) / required);
        }
        if (HasTiles(visual, BgInfoVisualCanvasSide.Right)) {
            var available = width - 2 * marginX - 2 * laneGap - 2 * visual.RightTileOffsetX;
            var required = centerWidth + 2 * rightWidth;
            if (required > 0) scale = Math.Min(scale, Math.Max(1, available) / required);
        }

        return Math.Min(1, scale);
    }

    private static double ResolveCenteredLaneX(BgInfoVisualCanvas visual, double width, double marginX, double laneGap, double centerWidth, double leftRailX, double leftRailWidth, double rightRailX) {
        var requested = (width - centerWidth) / 2 + visual.CenterTileOffsetX;
        var minimum = marginX;
        var maximum = Math.Max(marginX, width - marginX - centerWidth);
        if (HasTiles(visual, BgInfoVisualCanvasSide.Left)) {
            minimum = Math.Max(minimum, leftRailX + leftRailWidth + laneGap);
        }
        if (HasTiles(visual, BgInfoVisualCanvasSide.Right)) {
            maximum = Math.Min(maximum, rightRailX - laneGap - centerWidth);
        }

        return Clamp(requested, Math.Min(minimum, maximum), maximum);
    }

    internal static (double X, double Y, double Width, double Height) ResolveDefaultTransparentCenterBounds(BgInfoVisualCanvas visual, int width, int height) {
        var layout = ResolveOverlayRailLayout(visual, width, height);
        return (layout.CenterLeft, layout.RailY, layout.CenterWidth, height - layout.RailY);
    }

    private static OverlayRailLayout ResolveOverlayRailLayout(BgInfoVisualCanvas visual, int width, int height) {
        var requestedMarginX = visual.RailMarginX.HasValue ? Math.Max(0, visual.RailMarginX.Value) : Clamp(width * 0.045, Math.Min(24, width * 0.04), Math.Min(132, width * 0.08));
        // Reserve usable rails as well as their centre gap; merely leaving one
        // pixel per rail keeps geometry on-canvas but removes all tile facts.
        var minimumRailWidth = Math.Min(180, width / 4d);
        var marginX = Math.Min(requestedMarginX, Math.Max(0, (width - minimumRailWidth * 2 - 96) / 2));
        var laneGap = Math.Min(48, Math.Max(0, (width - marginX * 2 - 2) / 2));
        var railBudget = Math.Max(1, (width - (marginX * 2) - laneGap * 2) / 2);
        var templateTileWidth = Math.Min(Clamp(ApplyTileWidthPreset(visual.LayoutPreset, width * 0.18), Math.Min(180, railBudget), Math.Min(560, railBudget)), railBudget);
        var tileHeight = Clamp(ApplyTileHeightPreset(visual.LayoutPreset, height * 0.092), Math.Min(56, height * 0.18), Math.Min(140, height * 0.26));
        var tileGap = ResolveTileGap(visual, Clamp(ApplyTileGapPreset(visual.LayoutPreset, height * 0.018), Math.Min(8, height * 0.02), Math.Min(28, height * 0.045)));
        var requestedRailY = visual.RailMarginY.HasValue ? Math.Max(0, visual.RailMarginY.Value) : Clamp(height * 0.16, Math.Min(32, height * 0.08), Math.Min(190, height * 0.25));
        var laneHeight = visual.Tiles.GroupBy(tile => tile.Side)
            .Select(group => MeasureTileStackHeight(visual, group, tileHeight, tileGap))
            .DefaultIfEmpty(tileHeight).Max();
        var railY = Math.Min(requestedRailY, Math.Max(0, height - laneHeight));
        var leftTileWidth = ResolveSideTileWidth(visual, BgInfoVisualCanvasSide.Left, templateTileWidth);
        var rightTileWidth = ResolveSideTileWidth(visual, BgInfoVisualCanvasSide.Right, templateTileWidth);
        var leftRailWidth = ResolveRailWidth(visual, BgInfoVisualCanvasSide.Left, templateTileWidth, railBudget);
        var rightRailWidth = ResolveRailWidth(visual, BgInfoVisualCanvasSide.Right, templateTileWidth, railBudget);
        var centerRailWidth = ResolveRailWidth(visual, BgInfoVisualCanvasSide.Center, templateTileWidth, railBudget);
        var laneWidthScale = ResolveCenteredLaneWidthScale(visual, width, marginX, laneGap, leftRailWidth, centerRailWidth, rightRailWidth);
        leftTileWidth *= laneWidthScale;
        rightTileWidth *= laneWidthScale;
        leftRailWidth *= laneWidthScale;
        rightRailWidth *= laneWidthScale;
        var leftRailX = marginX + visual.LeftTileOffsetX;
        var rightRailX = width - marginX - rightRailWidth - visual.RightTileOffsetX;
        var centerLeft = leftRailX + leftRailWidth + laneGap;
        var centerRight = rightRailX - laneGap;
        return new OverlayRailLayout(leftRailX, rightRailX, railY, leftRailWidth, rightRailWidth, leftTileWidth, rightTileWidth, templateTileWidth, laneWidthScale, centerRailWidth * laneWidthScale, marginX, tileHeight, tileGap, centerLeft, Math.Max(1, centerRight - centerLeft), laneGap);
    }

    private readonly struct OverlayRailLayout {
        public OverlayRailLayout(double leftRailX, double rightRailX, double railY, double leftRailWidth, double rightRailWidth, double leftTileWidth, double rightTileWidth, double templateTileWidth, double laneWidthScale, double centerRailWidth, double marginX, double tileHeight, double tileGap, double centerLeft, double centerWidth, double laneGap) {
            LeftRailX = leftRailX;
            RightRailX = rightRailX;
            RailY = railY;
            LeftRailWidth = leftRailWidth;
            RightRailWidth = rightRailWidth;
            LeftTileWidth = leftTileWidth;
            RightTileWidth = rightTileWidth;
            TemplateTileWidth = templateTileWidth;
            LaneWidthScale = laneWidthScale;
            CenterRailWidth = centerRailWidth;
            MarginX = marginX;
            TileHeight = tileHeight;
            TileGap = tileGap;
            LaneGap = laneGap;
            CenterLeft = centerLeft;
            CenterWidth = centerWidth;
        }

        public double LeftRailX { get; }
        public double RightRailX { get; }
        public double RailY { get; }
        public double LeftRailWidth { get; }
        public double RightRailWidth { get; }
        public double LeftTileWidth { get; }
        public double RightTileWidth { get; }
        public double TemplateTileWidth { get; }
        public double LaneWidthScale { get; }
        public double CenterRailWidth { get; }
        public double MarginX { get; }
        public double TileHeight { get; }
        public double TileGap { get; }
        public double LaneGap { get; }
        public double CenterLeft { get; }
        public double CenterWidth { get; }
    }

    private static double ApplyTileWidthPreset(BgInfoVisualCanvasLayoutPreset preset, double value) {
        switch (preset) {
            case BgInfoVisualCanvasLayoutPreset.Compact: return value * 0.86;
            case BgInfoVisualCanvasLayoutPreset.Comfortable: return value * 1.12;
            case BgInfoVisualCanvasLayoutPreset.WideRails: return value * 1.28;
            case BgInfoVisualCanvasLayoutPreset.Dense: return value * 0.92;
            default: return value;
        }
    }

    private static double ApplyTileHeightPreset(BgInfoVisualCanvasLayoutPreset preset, double value) {
        switch (preset) {
            case BgInfoVisualCanvasLayoutPreset.Compact: return value * 0.88;
            case BgInfoVisualCanvasLayoutPreset.Comfortable: return value * 1.18;
            case BgInfoVisualCanvasLayoutPreset.WideRails: return value * 1.08;
            case BgInfoVisualCanvasLayoutPreset.Dense: return value * 0.76;
            default: return value;
        }
    }

    private static double ApplyTileGapPreset(BgInfoVisualCanvasLayoutPreset preset, double value) {
        switch (preset) {
            case BgInfoVisualCanvasLayoutPreset.Compact: return value * 0.72;
            case BgInfoVisualCanvasLayoutPreset.Comfortable: return value * 1.22;
            case BgInfoVisualCanvasLayoutPreset.Dense: return value * 0.55;
            default: return value;
        }
    }

    private static IEnumerable<VisualCanvasTextRun> SplitTitle(string title, BgInfoVisualCanvas visual, VisualTheme theme) {
        var titleColor = visual.TitleColor ?? theme.Resolve(visual.ThemeMode).Foreground;
        if (title.EndsWith("BGInfo", StringComparison.OrdinalIgnoreCase) && title.Length > "BGInfo".Length) {
            yield return new VisualCanvasTextRun(title.Substring(0, title.Length - "BGInfo".Length), titleColor);
            yield return new VisualCanvasTextRun(title.Substring(title.Length - "BGInfo".Length), visual.TitleAccentColor ?? visual.Accent);
            yield break;
        }

        yield return new VisualCanvasTextRun(title, titleColor);
    }

    private static IEnumerable<VisualCanvasFeatureItem> BuildFeatureItems(IEnumerable<BgInfoVisualCanvasFeature> features) {
        foreach (var feature in features) yield return new VisualCanvasFeatureItem(Resolve(feature.Icon), Resolve(feature.Label));
    }

    private static void AddHeroBadge(VisualCanvas canvas, BgInfoVisualCanvas visual, double x, double y, double width, double height, ChartColor accent) {
        if (!visual.HeroBadgeVisible) return;
        var text = Resolve(visual.HeroBadgeText);
        if (string.IsNullOrWhiteSpace(visual.HeroBadgeImagePath)) {
            canvas.AddHeroBadge(x, y, width, height, text, accent);
            return;
        }

        if (!File.Exists(visual.HeroBadgeImagePath)) {
            throw new FileNotFoundException("BGInfo hero badge image file was not found.", visual.HeroBadgeImagePath);
        }

        using var badgeImage = BgInfoRasterImage.Load(visual.HeroBadgeImagePath);
        var rgba = badgeImage.ToRgbaImage();
        canvas.AddHeroBadge(x, y, width, height, string.Empty, accent);

        var padding = Math.Max(0, visual.HeroBadgeImagePadding);
        canvas.AddImage(
            x + padding,
            y + padding,
            Math.Max(1, width - padding * 2),
            Math.Max(1, height - padding * 2),
            rgba: rgba.Pixels,
            sourceWidth: rgba.Width,
            sourceHeight: rgba.Height,
            opacity: visual.HeroBadgeImageOpacity,
            fit: BgInfoImageFitMapper.ToVisualCanvasFit(visual.HeroBadgeImageFit));
    }

    private static string Resolve(string? value) => BgInfoVariableResolver.RenderTemplate(value, new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase));

    private static VisualTheme BuildTheme(BgInfoVisualCanvas visual) =>
        (string.IsNullOrWhiteSpace(visual.FontFamilyName) ? VisualTheme.Graphite() : VisualTheme.Graphite().WithFontFamily(visual.FontFamilyName!)).Configure(builder => {
        var colors = builder.Colors(visual.ThemeMode);
        colors.Accent = visual.Accent;
        if (visual.SecondaryAccent.HasValue) colors.SecondaryAccent = visual.SecondaryAccent.Value;
    });

    private static void ApplyStyle(VisualCanvasStyleOptions style, BgInfoVisualCanvas visual) {
        style.TileTextScale = visual.TileTextScale;
        style.ImagePlaceholderStroke = visual.Accent.WithOpacity(0.34);
        style.TileGlassTop = visual.TileGlassTop;
        style.TileGlassBottom = visual.TileGlassBottom;
        style.TileLabelColor = visual.TileLabelColor;
        style.TileValueColor = visual.TileValueColor;
        style.TileDetailColor = visual.TileDetailColor;
        style.TileProgressTrackColor = visual.TileProgressTrackColor;
        style.HeroBadgeTop = visual.HeroBadgeTop;
        style.HeroBadgeBottom = visual.HeroBadgeBottom;
        style.HeroBadgeTextColor = visual.HeroBadgeTextColor;
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

    private static VisualCanvasTextFitPolicy ResolveTextFitPolicy(BgInfoVisualCanvas visual, BgInfoVisualCanvasTile tile) {
        var policy = tile.TextFitPolicy == BgInfoVisualCanvasTileTextFitPolicy.Auto ? visual.TileTextFitPolicy : tile.TextFitPolicy;
        switch (policy) {
            case BgInfoVisualCanvasTileTextFitPolicy.SingleLineEllipsis: return VisualCanvasTextFitPolicy.SingleLineEllipsis;
            case BgInfoVisualCanvasTileTextFitPolicy.Wrap: return VisualCanvasTextFitPolicy.Wrap;
            case BgInfoVisualCanvasTileTextFitPolicy.ShrinkToFit: return VisualCanvasTextFitPolicy.ShrinkToFit;
            case BgInfoVisualCanvasTileTextFitPolicy.WrapThenShrink: return VisualCanvasTextFitPolicy.WrapThenShrink;
            default: return VisualCanvasTextFitPolicy.Auto;
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

    internal static (double X, double Y, double Width, double Height) ResolveDefaultOpaqueFeatureStripBounds(BgInfoVisualCanvas visual, int width, int height, double scaleX, double scaleY) {
        var stripWidth = ResolveFeatureWidth(visual, 620 * scaleX);
        var stripHeight = ResolveFeatureHeight(visual, 62 * scaleY);
        var x = Math.Max(0, (width - stripWidth) / 2);
        var y = Math.Max(0, height - stripHeight - 46 * scaleY);
        return (x, y, stripWidth, stripHeight);
    }

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

}
