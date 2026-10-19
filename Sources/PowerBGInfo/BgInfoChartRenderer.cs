using System;
using System.Collections.Generic;
using Color = ChartForgeX.Primitives.ChartColor;
using System.Linq;
using ChartForgeX;
using ChartForgeX.Core;
using ChartForgeX.Primitives;
using ChartForgeX.Themes;
using ChartForgeX.Typography;

namespace PowerBGInfo;

internal static class BgInfoChartRenderer {
    public static BgInfoRasterImage Render(BgInfoChart chart, IReadOnlyList<double> values, BgInfoConfiguration config) {
        if (chart == null) throw new ArgumentNullException(nameof(chart));
        return Render(chart, values, config, chart.Width, chart.Height);
    }

    /// <summary>Renders the chart panel at a resolved size, for example a layout region slot.</summary>
    public static BgInfoRasterImage Render(BgInfoChart chart, IReadOnlyList<double> values, BgInfoConfiguration config, int requestedWidth, int requestedHeight) {
        if (chart == null) throw new ArgumentNullException(nameof(chart));
        if (config == null) throw new ArgumentNullException(nameof(config));
        chart.Threshold?.Validate();

        var width = Math.Max(1, requestedWidth);
        var height = Math.Max(1, requestedHeight);
        var image = BgInfoPanel.Create(width, height, chart.BackgroundColor, chart.BorderColor, chart.CornerRadius);

        var padding = Math.Max(0, chart.Padding);
        var plotLeft = (double)padding;
        var plotTop = (double)padding;
        var plotWidth = Math.Max(1d, width - padding * 2d);
        var plotHeight = Math.Max(1d, height - padding * 2d);

        var title = chart.Title ?? string.Empty;
        var showValue = chart.ShowLatestValue && values.Count > 0;
        var latestValueText = showValue ? FormatValue(values[values.Count - 1], chart) : string.Empty;

        var titleColor = ResolveTitleColor(chart, config);
        var valueColor = ResolveValueColor(chart, config);
        var titleFontFamily = ResolveTitleFontFamily(chart, config);
        var valueFontFamily = ResolveValueFontFamily(chart, config);
        var titleStyle = BgInfoRasterImage.CreateTextStyle(
            chart.TitleFontSize ?? config.FontSize,
            titleFontFamily,
            titleColor,
            chart.TitleFontWeight ?? ResolveWeight(chart.TitleBold) ?? config.FontWeight,
            chart.TitleItalic ?? config.Italic,
            chart.TitleUnderlineStyle ?? ResolveUnderline(chart.TitleUnderline) ?? config.UnderlineStyle,
            chart.TitleStrikethroughStyle ?? config.StrikethroughStyle,
            chart.TitleBaseline ?? config.Baseline,
            chart.TitleTextCase ?? config.TextCase);
        var valueStyle = BgInfoRasterImage.CreateTextStyle(
            chart.ValueFontSize ?? config.ValueFontSize,
            valueFontFamily,
            valueColor,
            chart.ValueFontWeight ?? ResolveWeight(chart.ValueBold) ?? config.ValueFontWeight,
            chart.ValueItalic ?? config.ValueItalic,
            chart.ValueUnderlineStyle ?? ResolveUnderline(chart.ValueUnderline) ?? config.ValueUnderlineStyle,
            chart.ValueStrikethroughStyle ?? config.ValueStrikethroughStyle,
            chart.ValueBaseline ?? config.ValueBaseline,
            chart.ValueTextCase ?? config.ValueTextCase);
        var titleSize = !string.IsNullOrWhiteSpace(title)
            ? image.GetTextSize(title, titleStyle)
            : new TextMetrics(0, 0, 0);
        var valueSize = !string.IsNullOrWhiteSpace(latestValueText)
            ? image.GetTextSize(latestValueText, valueStyle)
            : new TextMetrics(0, 0, 0);
        var headerWidth = Math.Max(0, width - padding * 2d);
        var titleLayout = headerWidth > 0 && titleSize.Width > 0
            ? TextLayoutEngine.Layout(title, headerWidth, titleStyle, TextWrapMode.NoWrap, 1, TextTrimming.Ellipsis)
            : null;
        var valueLayout = headerWidth > 0 && valueSize.Width > 0
            ? TextLayoutEngine.Layout(latestValueText, headerWidth, valueStyle, TextWrapMode.Word, null, TextTrimming.None)
            : null;
        const double headerGap = 12;
        var separateValueRow = titleSize.Width > 0 && valueSize.Width > 0 &&
            titleSize.Width + valueSize.Width + headerGap > headerWidth;
        var titleHeight = titleLayout?.Metrics.Height ?? 0;
        var valueHeight = valueLayout?.Metrics.Height ?? 0;
        var headerHeight = separateValueRow
            ? titleHeight + valueHeight + 4
            : Math.Max(titleHeight, valueHeight);
        if (headerHeight > 0) {
            plotTop += headerHeight + 4;
            plotHeight = Math.Max(1, height - plotTop - padding);
            if (titleLayout != null) {
                DrawHeaderLayout(image, titleLayout, titleStyle, padding, padding, headerWidth, false);
            }

            if (valueLayout != null) {
                var valueY = separateValueRow ? padding + titleHeight + 4 : padding;
                DrawHeaderLayout(image, valueLayout, valueStyle, padding, valueY, headerWidth, true);
            }
        }

        if (values.Count == 0) {
            return image;
        }

        var plot = BuildChartForgeXChart(chart, values, config, (int)Math.Round(plotWidth), (int)Math.Round(plotHeight));
        image.DrawImage(plot.ToRgbaImage(), plotLeft, plotTop, plotWidth, plotHeight);
        return image;
    }

    internal static Chart BuildChartForgeXChart(BgInfoChart chart, IReadOnlyList<double> values, BgInfoConfiguration config, int width, int height) {
        var accent = chart.LineColor ?? (values.Count > 0 ? ResolveThresholdColor(chart, values[values.Count - 1]) : null) ?? config.CustomValueColor ?? ResolveTokens(chart).Accent;
        var palette = ResolvePalette(chart, values);
        // Keep a positive native chart interior even when a tall header leaves a short plot.
        // The host still owns the requested tile dimensions and clips its composed content.
        var plotPadding = Math.Min(8d, (Math.Max(1, Math.Min(width, height)) - 1d) / 2d);
        var plot = Chart.Create()
            .WithSize(Math.Max(1, width), Math.Max(1, height))
            .WithTheme(CreateOverlayTheme(chart, config), chart.ThemeMode)
            .WithTransparentBackground()
            .WithHeader(false)
            .WithLegend(false)
            .WithAxes(chart.Kind == BgInfoChartKind.HorizontalBar)
            .WithXAxisVisible(false)
            .WithYAxisVisible(chart.Kind == BgInfoChartKind.HorizontalBar)
            .WithAxisLines(false)
            .WithCard(false)
            .WithPlotBackground(false)
            .WithPadding(plotPadding, plotPadding, plotPadding, plotPadding)
            .WithPngSupersampling(2)
            .WithValueFormatter(value => FormatValue(value, chart));

        plot.ConfigureTitleStyle(style => ApplyTextStyle(style, ResolveChartTitleStyle(chart, config), chart.TitleFontSize));
        var valueStyle = ResolveChartValueStyle(chart, config);
        plot.ConfigureDataLabelStyle(style => {
            ApplyTextStyle(style, valueStyle, chart.ValueFontSize);
            // An inherited foreground is not an authored override: let the chart owner choose
            // contrasting ink when a data label is placed inside a painted mark.
            style.Color = chart.ValueColor ?? chart.TextColor ?? config.CustomValueColor;
        });
        plot.ConfigureLegendStyle(style => ApplyTextStyle(style, valueStyle, chart.ValueFontSize));
        plot.ConfigureTickLabelStyle(style => ApplyTextStyle(style, valueStyle, chart.ValueFontSize));
        plot.ConfigureAxisTitleStyle(style => ApplyTextStyle(style, ResolveChartTitleStyle(chart, config), chart.TitleFontSize));

        var showGrid = chart.ShowGrid && chart.GridLineCount > 0;
        plot.WithGrid(showGrid);
        if (showGrid) {
            var horizontal = chart.Kind == BgInfoChartKind.HorizontalBar;
            plot.ConfigureGridStyle(style => {
                style.ShowHorizontalLines = !horizontal;
                style.ShowVerticalLines = horizontal;
            });
            // The shared axes choose readable nice-number steps from a preferred density.
            var gridTicks = Math.Max(2, Math.Min(100, chart.GridLineCount));
            if (horizontal) {
                plot.ConfigureXAxis(axis => axis.TickCount = gridTicks);
            } else {
                plot.ConfigureYAxis(axis => axis.TickCount = gridTicks);
            }
        }

        ApplyChartOptions(plot, chart);

        switch (chart.Kind) {
            case BgInfoChartKind.Bar:
                plot.AddSeries(WithPointColors(ChartSeries.CreateBar(SeriesId(chart), chart.Title, BuildIndexedPoints(values), accent), palette, values.Count));
                break;
            case BgInfoChartKind.HorizontalBar:
                var labels = BuildLabels(chart, values.Count);
                plot.ConfigureYAxis(axis => {
                    axis.TickCount = Math.Max(2, labels.Length);
                    axis.LabelDensity = ChartLabelDensity.All;
                    axis.Labels.Clear();
                    for (var index = 0; index < labels.Length; index++) axis.Labels.Add(new ChartAxisLabel(index + 1, labels[index]));
                });
                plot.AddSeries(WithPointColors(new ChartSeries(SeriesId(chart), chart.Title, ChartSeriesKind.HorizontalBar, BuildIndexedPoints(values)) { Color = accent }, palette, values.Count));
                break;
            case BgInfoChartKind.Line:
                AddTrendSeries(plot, chart, values, width, accent, smooth: false, area: false);
                break;
            case BgInfoChartKind.Area:
                AddTrendSeries(plot, chart, values, width, chart.FillColor ?? accent, smooth: true, area: true);
                break;
            case BgInfoChartKind.Gauge:
                AddGauge(plot, chart, values, accent);
                break;
            case BgInfoChartKind.Circle:
                AddCircle(plot, chart, values, accent);
                break;
            case BgInfoChartKind.RadialBar:
                plot.AddSeries(new ChartSeries(SeriesId(chart), chart.Title, ChartSeriesKind.ProgressRing, BuildPercentPoints(values)) { Color = accent });
                break;
            case BgInfoChartKind.Bullet:
                AddBulletRows(plot, chart, values, accent, palette);
                break;
            case BgInfoChartKind.Pie:
                AddCategoricalSeries(plot, chart, values, ChartSeriesKind.Pie, palette);
                break;
            case BgInfoChartKind.Donut:
                AddCategoricalSeries(plot, chart, values, ChartSeriesKind.Donut, palette);
                break;
            case BgInfoChartKind.ProgressBar:
                plot.WithProgressMaximum(ResolveMaximum(chart, values, 100));
                AddCategoricalSeries(plot, chart, values, ChartSeriesKind.ProgressBar, palette, accent);
                break;
            case BgInfoChartKind.Pictorial:
                AddCategoricalSeries(plot, chart, values.Select(value => Math.Max(0, value)).ToArray(), ChartSeriesKind.Pictorial, palette, accent);
                break;
            default:
                AddTrendSeries(plot, chart, values, width, accent, smooth: true, area: false);
                break;
        }

        return plot;
    }

    private static void AddTrendSeries(Chart plot, BgInfoChart chart, IReadOnlyList<double> values, int width, ChartColor color, bool smooth, bool area) {
        var series = ChartSeries.CreateAdaptive(SeriesId(chart), chart.Title, area ? ChartSeriesKind.Area : ChartSeriesKind.Line,
            BuildIndexedPoints(values), width, ChartResolutionPolicy.Trend(), color).WithSmooth(smooth);
        plot.AddSeries(series);
    }

    private static void ApplyChartOptions(Chart plot, BgInfoChart chart) {
        plot.WithLegend(chart.ShowLegend)
            .WithPointLegend(chart.ShowPointLegend)
            .WithLegendPosition(ResolveLegendPosition(chart.LegendPosition))
            .WithDataLabels(chart.ShowDataLabels)
            .WithDonutCenterLabel(chart.ShowDonutCenterLabel)
            .WithProgressRingCenterLabel(chart.ShowRadialBarCenterLabel)
            .WithCircleStatusLabel(chart.ShowCircleStatusLabel)
            .WithProgressValues(chart.ShowProgressValues)
            .WithProgressHandles(chart.ShowProgressHandles)
            .WithPictorialShape(ResolvePictorialShape(chart.PictorialSymbol));

        if (chart.Palette.Count > 0) {
            plot.WithTheme(plot.Options.Theme.WithPalette(chart.Palette.ToArray()), chart.ThemeMode);
        }
        if (chart.DonutInnerRadiusRatio.HasValue) {
            plot.WithDonutInnerRadiusRatio(chart.DonutInnerRadiusRatio.Value);
        }
        if (!string.IsNullOrWhiteSpace(chart.DonutCenterValue) || !string.IsNullOrWhiteSpace(chart.DonutCenterLabel)) {
            plot.WithDonutCenterText(chart.DonutCenterValue, chart.DonutCenterLabel);
        }
        if (chart.ProgressBarThicknessRatio.HasValue) {
            plot.WithProgressBarThickness(chart.ProgressBarThicknessRatio.Value);
        }
        if (chart.Maximum.HasValue) {
            plot.WithProgressMaximum(chart.Maximum.Value);
            plot.WithPictorialMaximum(chart.Maximum.Value);
        }
        if (chart.PictorialColumns.HasValue) {
            plot.WithPictorialColumns(chart.PictorialColumns.Value);
        }
    }

    private static VisualTheme CreateOverlayTheme(BgInfoChart chart, BgInfoConfiguration config) {
        var text = ResolveTitleColor(chart, config);
        var grid = chart.GridColor ?? ResolveTokens(chart).Border;
        return VisualTheme.Graphite().Configure(builder => {
            var colors = builder.Colors(chart.ThemeMode);
            colors.Background = ChartColor.Transparent;
            colors.Surface = ChartColor.Transparent;
            colors.ElevatedSurface = ChartColor.Transparent;
            // The panel owns the frame. Border still paints progress, gauge, circle and pictorial
            // tracks, so keep it as translucent guide ink instead of hiding those tracks.
            colors.Border = WithAlpha(grid, 84);
            colors.PlotBorder = ChartColor.Transparent;
            colors.Foreground = text;
            colors.MutedForeground = text;
            colors.Grid = WithAlpha(grid, 90);
            colors.Axis = WithAlpha(grid, 120);
            builder.SeriesStrokeWidth = 2.2;
            builder.MarkerRadius = 2.4;
        });
    }

    private static void AddGauge(Chart plot, BgInfoChart chart, IReadOnlyList<double> values, ChartColor accent) {
        var latest = values[values.Count - 1];
        plot.AddSeries(ChartSeries.CreateGauge(SeriesId(chart), chart.Title,
            new ChartGaugeValue(latest, chart.Minimum ?? 0, ResolveMaximum(chart, values, 100)), accent));
    }

    private static void AddCircle(Chart plot, BgInfoChart chart, IReadOnlyList<double> values, ChartColor accent) {
        var latest = values[values.Count - 1];
        plot.AddSeries(ChartSeries.CreateCircle(SeriesId(chart), chart.Title,
            new ChartGaugeValue(latest, chart.Minimum ?? 0, ResolveMaximum(chart, values, 100)), accent));
    }

    /// <summary>Explicit palette first; otherwise per-point threshold colors for row-based kinds.</summary>
    internal static IReadOnlyList<ChartColor> ResolvePalette(BgInfoChart chart, IReadOnlyList<double> values) {
        if (chart.Palette.Count > 0 || chart.LineColor.HasValue || chart.Threshold is not { IsDefined: true } || !IsPerPointKind(chart.Kind)) return chart.Palette;
        var colors = new ChartColor[values.Count];
        for (var i = 0; i < values.Count; i++) {
            colors[i] = ResolveThresholdColor(chart, values[i]) ?? ResolveTokens(chart).Accent;
        }
        return colors;
    }

    internal static ChartColor? ResolveThresholdColor(BgInfoChart chart, double value) =>
        chart.Threshold is { IsDefined: true } threshold ? threshold.ResolveColor(value, chart.ThemeMode) : null;

    private static bool IsPerPointKind(BgInfoChartKind kind) => kind is BgInfoChartKind.Bar or BgInfoChartKind.HorizontalBar
        or BgInfoChartKind.ProgressBar or BgInfoChartKind.Bullet or BgInfoChartKind.Pictorial;

    private static ChartSeries WithPointColors(ChartSeries series, IReadOnlyList<ChartColor> palette, int count) {
        for (var i = 0; i < Math.Min(palette.Count, count); i++) series.WithPointColor(i, palette[i]);
        return series;
    }

    private static void AddBulletRows(Chart plot, BgInfoChart chart, IReadOnlyList<double> values, ChartColor accent, IReadOnlyList<ChartColor> palette) {
        var maximum = ResolveMaximum(chart, values, 100);
        var minimum = chart.Minimum ?? 0;
        var target = chart.Target ?? maximum;
        var ranges = chart.RangeEnds.Count > 0 ? chart.RangeEnds : null;

        for (var i = 0; i < values.Count; i++) {
            var series = ChartSeries.CreateBullet(SeriesId(chart, "row:" + i.ToString(System.Globalization.CultureInfo.InvariantCulture)), LabelAt(chart, i),
                new ChartBulletValue(values[i], target, minimum, maximum, ranges), (i < palette.Count ? palette[i] : (ChartColor?)null) ?? accent)
                .WithDataLabels(chart.ShowDataLabels);
            plot.AddSeries(series);
        }
    }

    private static ChartPoint[] BuildIndexedPoints(IReadOnlyList<double> values) {
        var points = new ChartPoint[values.Count];
        for (var i = 0; i < values.Count; i++) {
            points[i] = new ChartPoint(i + 1, values[i]);
        }

        return points;
    }

    private static ChartPoint[] BuildPercentPoints(IReadOnlyList<double> values) {
        var points = new ChartPoint[values.Count];
        for (var i = 0; i < values.Count; i++) {
            points[i] = new ChartPoint(i + 1, Clamp(values[i], 0, 100));
        }

        return points;
    }

    private static string[] BuildLabels(BgInfoChart chart, int count) {
        var labels = new string[count];
        for (var i = 0; i < count; i++) {
            labels[i] = i < chart.Labels.Count && !string.IsNullOrWhiteSpace(chart.Labels[i])
                ? chart.Labels[i]
                : (i + 1).ToString(System.Globalization.CultureInfo.InvariantCulture);
        }

        return labels;
    }

    private static string LabelAt(BgInfoChart chart, int index) =>
        index < chart.Labels.Count && !string.IsNullOrWhiteSpace(chart.Labels[index])
            ? chart.Labels[index]
            : (index + 1).ToString(System.Globalization.CultureInfo.InvariantCulture);


    private static ChartSeriesId SeriesId(BgInfoChart chart, string suffix = "values") =>
        new("bginfo:" + (string.IsNullOrWhiteSpace(chart.Id) ? "chart" : chart.Id.Trim()) + ":" + suffix);

    private static void AddCategoricalSeries(Chart plot, BgInfoChart chart, IReadOnlyList<double> values, ChartSeriesKind kind, IReadOnlyList<ChartColor> palette, ChartColor? color = null) {
        var series = WithPointColors(new ChartSeries(SeriesId(chart), chart.Title, kind, BuildIndexedPoints(values)) { Color = color }, palette, values.Count);
        plot.WithXLabels(BuildLabels(chart, values.Count)).AddSeries(series);
    }

    private static double ResolveMaximum(BgInfoChart chart, IReadOnlyList<double> values, double fallback) {
        if (chart.Maximum.HasValue) {
            return chart.Maximum.Value;
        }

        var maxValue = fallback;
        for (var i = 0; i < values.Count; i++) {
            maxValue = Math.Max(maxValue, Math.Ceiling(values[i] / 10d) * 10d);
        }

        return maxValue <= 0 ? fallback : maxValue;
    }

    private static ChartLegendPosition ResolveLegendPosition(BgInfoChartLegendPosition position) {
        switch (position) {
            case BgInfoChartLegendPosition.Top:
                return ChartLegendPosition.Top;
            case BgInfoChartLegendPosition.Left:
                return ChartLegendPosition.Left;
            case BgInfoChartLegendPosition.Right:
                return ChartLegendPosition.Right;
            default:
                return ChartLegendPosition.Bottom;
        }
    }

    private static ChartPictorialShape ResolvePictorialShape(BgInfoChartPictorialSymbol shape) {
        switch (shape) {
            case BgInfoChartPictorialSymbol.Square:
                return ChartPictorialShape.Square;
            case BgInfoChartPictorialSymbol.Diamond:
                return ChartPictorialShape.Diamond;
            case BgInfoChartPictorialSymbol.Triangle:
                return ChartPictorialShape.Triangle;
            case BgInfoChartPictorialSymbol.Star:
                return ChartPictorialShape.Star;
            case BgInfoChartPictorialSymbol.Person:
                return ChartPictorialShape.Person;
            default:
                return ChartPictorialShape.Circle;
        }
    }

    private static ChartColor WithAlpha(ChartColor color, byte alpha) => ChartColor.FromRgba(color.R, color.G, color.B, alpha);

    private static void DrawHeaderLayout(BgInfoRasterImage image, TextLayout layout, TextStyle style, double x, double y, double width, bool alignRight) {
        // The shared layout already applied casing; applying ToggleCase twice would undo it.
        var resolvedStyle = style.Clone();
        resolvedStyle.TextCase = TextCaseTransform.None;
        foreach (var line in layout.Lines) {
            image.AddText(alignRight ? x + Math.Max(0, width - line.Width) : x, y, line.Text, resolvedStyle);
            y += layout.Metrics.LineHeight;
        }
    }

    private static TextStyle ResolveChartTitleStyle(BgInfoChart chart, BgInfoConfiguration config) => BgInfoRasterImage.CreateTextStyle(
        chart.TitleFontSize ?? config.FontSize,
        ResolveTitleFontFamily(chart, config),
        ResolveTitleColor(chart, config),
        chart.TitleFontWeight ?? ResolveWeight(chart.TitleBold) ?? config.FontWeight,
        chart.TitleItalic ?? config.Italic,
        chart.TitleUnderlineStyle ?? ResolveUnderline(chart.TitleUnderline) ?? config.UnderlineStyle,
        chart.TitleStrikethroughStyle ?? config.StrikethroughStyle,
        chart.TitleBaseline ?? config.Baseline,
        chart.TitleTextCase ?? config.TextCase);

    private static TextStyle ResolveChartValueStyle(BgInfoChart chart, BgInfoConfiguration config) => BgInfoRasterImage.CreateTextStyle(
        chart.ValueFontSize ?? config.ValueFontSize,
        ResolveValueFontFamily(chart, config),
        ResolveValueColor(chart, config),
        chart.ValueFontWeight ?? ResolveWeight(chart.ValueBold) ?? config.ValueFontWeight,
        chart.ValueItalic ?? config.ValueItalic,
        chart.ValueUnderlineStyle ?? ResolveUnderline(chart.ValueUnderline) ?? config.ValueUnderlineStyle,
        chart.ValueStrikethroughStyle ?? config.ValueStrikethroughStyle,
        chart.ValueBaseline ?? config.ValueBaseline,
        chart.ValueTextCase ?? config.ValueTextCase);

    private static int? ResolveWeight(bool? bold) => bold.HasValue ? (bold.Value ? 700 : 400) : null;

    private static VisualDesignTokens ResolveTokens(BgInfoChart chart) =>
        chart.ThemeMode == VisualThemeMode.Dark ? VisualDesignTokens.GraphiteDark() : VisualDesignTokens.GraphiteLight();

    private static string ResolveTitleFontFamily(BgInfoChart chart, BgInfoConfiguration config) =>
        chart.FontFamilyName ?? config.CustomFontFamilyName ?? VisualTheme.Graphite().Typography.Family;

    private static string ResolveValueFontFamily(BgInfoChart chart, BgInfoConfiguration config) =>
        chart.FontFamilyName ?? config.CustomValueFontFamilyName ?? VisualTheme.Graphite().Typography.Family;

    private static ChartColor ResolveTitleColor(BgInfoChart chart, BgInfoConfiguration config) =>
        chart.TitleColor ?? chart.TextColor ?? config.CustomColor ?? ResolveTokens(chart).Foreground;

    private static ChartColor ResolveValueColor(BgInfoChart chart, BgInfoConfiguration config) =>
        chart.ValueColor ?? chart.TextColor ?? config.CustomValueColor ?? ResolveTokens(chart).Foreground;

    private static TextDecorationStyle? ResolveUnderline(bool? underline) => underline.HasValue ? (underline.Value ? TextDecorationStyle.Single : TextDecorationStyle.None) : null;

    private static void ApplyTextStyle(TextStyleOverride target, TextStyle source, float? explicitFontSize) {
        target.Color = source.Color;
        target.FontFamily = source.Font.Family;
        if (explicitFontSize.HasValue) target.FontSize = explicitFontSize.Value;
        target.FontWeight = source.Font.Weight.ToString(System.Globalization.CultureInfo.InvariantCulture);
        target.Italic = source.Font.Italic;
        target.UnderlineStyle = source.UnderlineStyle;
        target.StrikethroughStyle = source.StrikethroughStyle;
        target.Baseline = source.Baseline;
        target.TextCase = source.TextCase;
    }

    private static string FormatValue(double value, BgInfoChart chart) {
        var format = string.IsNullOrWhiteSpace(chart.ValueFormat) ? "0.##" : chart.ValueFormat;
        var text = value.ToString(format, System.Globalization.CultureInfo.InvariantCulture);
        return string.IsNullOrWhiteSpace(chart.ValueSuffix) ? text : text + chart.ValueSuffix;
    }

    private static double Clamp(double value, double min, double max) {
        if (value < min) return min;
        if (value > max) return max;
        return value;
    }
}
