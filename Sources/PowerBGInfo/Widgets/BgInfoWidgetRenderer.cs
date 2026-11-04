using System;
using ChartForgeX.Composition;
using ChartForgeX.Primitives;
using ChartForgeX.Raster;
using ChartForgeX.Themes;
using ChartForgeX.Typography;

namespace PowerBGInfo;

/// <summary>Colors and fonts a widget body renders with.</summary>
internal sealed class BgInfoWidgetStyle {
    public BgInfoWidgetStyle(BgInfoWidget widget, BgInfoConfiguration config) {
        Mode = widget.ThemeMode;
        var tokens = Mode == VisualThemeMode.Dark ? VisualDesignTokens.GraphiteDark() : VisualDesignTokens.GraphiteLight();
        Ink = widget.TextColor ?? tokens.Foreground;
        Muted = widget.MutedColor ?? tokens.MutedForeground;
        Accent = widget.Accent ?? tokens.Accent;
        SecondaryAccent = widget.SecondaryAccent ?? tokens.SecondaryAccent;
        Status = widget is BgInfoMetricWidget metric && metric.NumericValue.HasValue
            && metric.Threshold is { IsDefined: true, Colors: { } thresholdColors }
                ? thresholdColors
                : widget.ResolveStatusColors();
        Grid = widget.BorderColor.HasValue ? WithAlpha(widget.BorderColor.Value, 255) : tokens.Border;
        FontFamily = widget.FontFamilyName ?? config.CustomFontFamilyName ?? VisualTheme.Graphite().Typography.Family;
    }

    public VisualThemeMode Mode { get; }
    public ChartColor Ink { get; }
    public ChartColor Muted { get; }
    public ChartColor Accent { get; }
    public ChartColor SecondaryAccent { get; }
    public BgInfoStatusColors Status { get; }
    public ChartColor Grid { get; }
    public string FontFamily { get; }

    public ChartColor[] Palette => new[] { Accent, SecondaryAccent, Status.Info, Status.Ok, Status.Warning, Status.Critical, Muted };

    /// <summary>
    /// Palette for metric cards, tables and lists. These blocks color the Info status with the first palette entry and
    /// list markers by cycling the palette, so an Info row under a red accent read as an alert and a checklist came out
    /// in rainbow colors. One entry fixes both: the info color when an Info status is shown, otherwise the accent.
    /// </summary>
    public ChartColor[] StatusBlockPalette(bool showsInfo) => new[] { showsInfo ? Status.Info : Accent };

    /// <summary>A transparent ChartForgeX theme that inherits the widget colors.</summary>
    /// <param name="surface">Table header and striping surface.</param>
    /// <param name="palette">Series palette; defaults to <see cref="Palette"/>. See <see cref="StatusBlockPalette"/>.</param>
    public VisualTheme CreateTheme(ChartColor? surface = null, ChartColor[]? palette = null) => VisualTheme.Graphite().WithFontFamily(FontFamily).Configure(builder => {
        var colors = builder.Colors(Mode);
        colors.Background = ChartColor.Transparent;
        // Table headers and striping read the surface; a mid-tone keeps both subtle on glass panels.
        colors.Surface = surface ?? ChartColor.FromHex(Mode == VisualThemeMode.Dark ? "#1E293B" : "#E2E8F0");
        colors.ElevatedSurface = ChartColor.Transparent;
        colors.Foreground = Ink;
        colors.MutedForeground = Muted;
        colors.Accent = Accent;
        colors.SecondaryAccent = SecondaryAccent;
        colors.Positive = Status.Ok;
        colors.Info = Status.Info;
        colors.Warning = Status.Warning;
        colors.Negative = Status.Critical;
        colors.Border = WithAlpha(Grid, 90);
        colors.PlotBorder = ChartColor.Transparent;
        colors.Grid = WithAlpha(Grid, 70);
        colors.Axis = WithAlpha(Grid, 110);
        colors.Palette = palette ?? Palette;
        builder.CardShadowOpacity = 0;
    });

    public static ChartColor WithAlpha(ChartColor color, byte alpha) => ChartColor.FromRgba(color.R, color.G, color.B, alpha);
}

/// <summary>Renders widget panels: frame, header, then the ChartForgeX body.</summary>
internal static partial class BgInfoWidgetRenderer {
    public static BgInfoRasterImage Render(BgInfoWidget widget, BgInfoConfiguration config, int width, int height) {
        if (widget == null) throw new ArgumentNullException(nameof(widget));
        if (config == null) throw new ArgumentNullException(nameof(config));
        widget.Validate();
        width = Math.Max(1, width);
        height = Math.Max(1, height);
        var style = new BgInfoWidgetStyle(widget, config);

        var image = BgInfoPanel.Create(width, height, widget.BackgroundColor, widget.BorderColor, widget.CornerRadius);

        var padding = Math.Min(Math.Max(0, widget.Padding), Math.Min(width, height) / 3);
        double left = padding, top = padding, bodyWidth = Math.Max(1, width - padding * 2d);
        if (!string.IsNullOrWhiteSpace(widget.Title) || !string.IsNullOrWhiteSpace(widget.Note)) {
            var header = BgInfoPanel.DrawHeader(image, widget.Title, widget.Note, CreateHeaderStyle(widget, style, config), left, top, bodyWidth);
            if (header > 0) top += header + Math.Max(6, padding / 2d);
        }

        var bodyHeight = Math.Max(1, height - top - padding);
        if (widget is BgInfoMetricWidget metric) {
            // Keep the metric's inset inside the remaining body area after its shared panel header.
            var inset = padding / 2d;
            var cardWidth = (int)Math.Round(Math.Max(1, bodyWidth - inset * 2));
            var cardHeight = (int)Math.Round(Math.Max(1, bodyHeight - inset * 2));
            image.DrawImage(RenderMetric(metric, style, cardWidth, cardHeight, inset), left + inset, top + inset, cardWidth, cardHeight);
            return image;
        }
        var body = RenderBody(widget, style, (int)Math.Round(bodyWidth), (int)Math.Round(bodyHeight), image, left, top);
        if (body is { } rendered) image.DrawImage(rendered, left, top, bodyWidth, bodyHeight);
        return image;
    }

    internal static TextStyle CreateHeaderStyle(BgInfoWidget widget, BgInfoWidgetStyle style, BgInfoConfiguration config) =>
        BgInfoRasterImage.CreateTextStyle(
            widget.TitleFontSize ?? config.FontSize,
            style.FontFamily,
            widget.TitleColor ?? style.Muted,
            widget.TitleFontWeight ?? 600,
            false,
            TextDecorationStyle.None,
            TextDecorationStyle.None,
            TextBaseline.Normal,
            widget.TitleTextCase ?? TextCaseTransform.None);

    /// <summary>Logical size and output density for a body rendered at <see cref="BgInfoWidget.Scale"/>.</summary>
    internal static (int Width, int Height, int OutputScale) Density(BgInfoWidget widget, int width, int height) {
        var scale = widget.Scale;
        if (widget.NaturalBodyHeight > 0) scale = Math.Min(scale, height / widget.NaturalBodyHeight);
        return Density(width, height, scale);
    }

    internal static (int Width, int Height, int OutputScale) Density(int width, int height, double scale) {
        var logicalWidth = Math.Max(1, (int)Math.Min(int.MaxValue, Math.Round(width / scale)));
        var logicalHeight = Math.Max(1, (int)Math.Min(int.MaxValue, Math.Round(height / scale)));
        var output = Math.Max(1, Math.Min(4, (int)Math.Ceiling(scale)));
        // Small panels still need a second sample per pixel to keep thin strokes smooth.
        if (output == 1) output = 2;
        return (logicalWidth, logicalHeight, output);
    }

    private static RgbaImage? RenderBody(BgInfoWidget widget, BgInfoWidgetStyle style, int width, int height, BgInfoRasterImage panel, double x, double y) {
        switch (widget) {

            case BgInfoMetricWidget metric: return RenderMetric(metric, style, width, height, 0);
            case BgInfoTableWidget table: return RenderTable(table, style, width, height);
            case BgInfoListWidget list: return RenderList(list, style, width, height);
            case BgInfoCalendarHeatmapWidget calendar: return RenderCalendar(calendar, style, width, height);
            case BgInfoTimelineWidget timeline: return RenderTimeline(timeline, style, width, height);
            case BgInfoTreemapWidget treemap: return RenderTreemap(treemap, style, width, height);
            case BgInfoMermaidWidget mermaid: return RenderMermaid(mermaid, style, width, height);
            case BgInfoQrCodeWidget qr:
                DrawQrCode(qr, style, panel, x, y, width, height);
                return null;
            default:
                throw new NotSupportedException("Unsupported widget kind: " + widget.Kind);
        }
    }

    /// <summary>Fits an image of any aspect ratio inside a body without stretching it.</summary>
    internal static RgbaImage Contain(RgbaImage source, int width, int height) {
        var composition = ImageComposition.CreateTransparent(Math.Max(1, width), Math.Max(1, height));
        composition.DrawImage(source, 0, 0, width, height, VisualCanvasImageFit.Contain);
        return composition.ToImage();
    }
}
