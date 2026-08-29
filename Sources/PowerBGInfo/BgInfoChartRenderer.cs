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
        if (config == null) throw new ArgumentNullException(nameof(config));

        var width = Math.Max(1, chart.Width);
        var height = Math.Max(1, chart.Height);
        var image = new BgInfoRasterImage();
        var background = chart.BackgroundColor ?? Color.Transparent;
        image.Create(string.Empty, width, height, background);

        var padding = Math.Max(0, chart.Padding);
        var plotLeft = (double)padding;
        var plotTop = (double)padding;
        var plotWidth = Math.Max(1d, width - padding * 2d);
        var plotHeight = Math.Max(1d, height - padding * 2d);

        var title = chart.Title ?? string.Empty;
        var showValue = chart.ShowLatestValue && values.Count > 0;
        var latestValueText = showValue ? FormatValue(values[values.Count - 1], chart) : string.Empty;

        var titleColor = chart.TitleColor ?? chart.TextColor ?? config.Color;
        var valueColor = chart.ValueColor ?? chart.TextColor ?? config.ValueColor;
        var titleFontFamily = chart.FontFamilyName ?? config.FontFamilyName;
        var valueFontFamily = chart.FontFamilyName ?? config.ValueFontFamilyName;
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
        var headerHeight = Math.Max(titleSize.Height, valueSize.Height);
        if (headerHeight > 0) {
            plotTop += headerHeight + 4;
            plotHeight = Math.Max(1, height - plotTop - padding);
            if (!string.IsNullOrWhiteSpace(title)) {
                image.AddText(padding, padding, title, titleStyle);
            }

            if (!string.IsNullOrWhiteSpace(latestValueText)) {
                var valueX = Math.Max(padding, width - padding - valueSize.Width);
                image.AddText(valueX, padding, latestValueText, valueStyle);
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
        var accent = chart.LineColor ?? config.ValueColor;
        var plot = Chart.Create()
            .WithSize(Math.Max(1, width), Math.Max(1, height))
            .WithTheme(CreateOverlayTheme(chart, config, accent))
            .WithTransparentBackground()
            .WithHeader(false)
            .WithLegend(false)
            .WithAxes(false)
            .WithXAxisVisible(false)
            .WithYAxisVisible(false)
            .WithAxisLines(false)
            .WithCard(false)
            .WithPlotBackground(false)
            .WithPadding(8, 8, 8, 8)
            .WithPngSupersampling(2)
            .WithValueFormatter(value => FormatValue(value, chart));

        ApplyTextStyle(plot.Options.TitleStyle, ResolveChartTitleStyle(chart, config), chart.TitleFontSize);
        var valueStyle = ResolveChartValueStyle(chart, config);
        ApplyTextStyle(plot.Options.DataLabelStyle, valueStyle, chart.ValueFontSize);
        ApplyTextStyle(plot.Options.LegendStyle, valueStyle, chart.ValueFontSize);
        ApplyTextStyle(plot.Options.TickLabelStyle, valueStyle, chart.ValueFontSize);
        ApplyTextStyle(plot.Options.AxisTitleStyle, ResolveChartTitleStyle(chart, config), chart.TitleFontSize);

        if (chart.ShowGrid && chart.GridLineCount > 0) {
            plot.WithGrid();
        }

        ApplyChartOptions(plot, chart);

        switch (chart.Kind) {
            case BgInfoChartKind.Bar:
                plot.AddBar(chart.Title, BuildIndexedPoints(values), accent);
                break;
            case BgInfoChartKind.HorizontalBar:
                plot.AddHorizontalBar(chart.Title, BuildIndexedPoints(values), accent);
                break;
            case BgInfoChartKind.Line:
                AddTrendSeries(plot, chart.Title, values, width, accent, smooth: false, area: false);
                break;
            case BgInfoChartKind.Area:
                AddTrendSeries(plot, chart.Title, values, width, chart.FillColor ?? chart.LineColor ?? config.ValueColor, smooth: true, area: true);
                break;
            case BgInfoChartKind.Gauge:
                AddGauge(plot, chart, values, accent);
                break;
            case BgInfoChartKind.Circle:
                AddCircle(plot, chart, values, accent);
                break;
            case BgInfoChartKind.RadialBar:
                plot.AddRadialBar(chart.Title, BuildPercentPoints(values), accent);
                break;
            case BgInfoChartKind.Bullet:
                AddBulletRows(plot, chart, values, accent);
                break;
            case BgInfoChartKind.Pie:
                plot.WithXLabels(BuildLabels(chart, values.Count)).AddPie(chart.Title, BuildIndexedPoints(values));
                ApplyPointColors(plot, chart);
                break;
            case BgInfoChartKind.Donut:
                plot.WithXLabels(BuildLabels(chart, values.Count)).AddDonut(chart.Title, BuildIndexedPoints(values));
                ApplyPointColors(plot, chart);
                break;
            case BgInfoChartKind.ProgressBar:
                plot.AddProgressBars(chart.Title, BuildProgressItems(chart, values), ResolveMaximum(chart, values, 100), accent);
                break;
            case BgInfoChartKind.Pictorial:
                plot.AddPictorial(chart.Title, BuildPictorialItems(chart, values), ResolvePictorialShape(chart.PictorialSymbol), accent);
                break;
            default:
                AddTrendSeries(plot, chart.Title, values, width, accent, smooth: true, area: false);
                break;
        }

        return plot;
    }

    private static void AddTrendSeries(Chart plot, string name, IReadOnlyList<double> values, int width, ChartColor color, bool smooth, bool area) {
        var points = BuildIndexedPoints(values);
        if (area) {
            plot.AddAdaptiveArea(name, points, width, ChartResolutionPolicy.Trend(), color);
        } else {
            plot.AddAdaptiveLine(name, points, width, ChartResolutionPolicy.Trend(), color);
        }
        plot.Series[plot.Series.Count - 1].WithSmooth(smooth);
    }

    private static void ApplyChartOptions(Chart plot, BgInfoChart chart) {
        plot.WithLegend(chart.ShowLegend)
            .WithPointLegend(chart.ShowPointLegend)
            .WithLegendPosition(ResolveLegendPosition(chart.LegendPosition))
            .WithDataLabels(chart.ShowDataLabels)
            .WithDonutCenterLabel(chart.ShowDonutCenterLabel)
            .WithRadialBarCenterLabel(chart.ShowRadialBarCenterLabel)
            .WithCircleStatusLabel(chart.ShowCircleStatusLabel)
            .WithProgressValues(chart.ShowProgressValues)
            .WithProgressHandles(chart.ShowProgressHandles)
            .WithPictorialShape(ResolvePictorialShape(chart.PictorialSymbol));

        if (chart.Palette.Count > 0) {
            plot.WithPalette(chart.Palette.ToArray());
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

    private static ChartTheme CreateOverlayTheme(BgInfoChart chart, BgInfoConfiguration config, ChartColor accent) {
        var text = chart.TextColor ?? config.Color;
        var grid = chart.GridColor ?? chart.TextColor ?? config.ValueColor;
        return ChartTheme.Minimal()
            .WithSurfaceColors(ChartColor.Transparent, ChartColor.Transparent, ChartColor.Transparent, ChartColor.Transparent, ChartColor.Transparent)
            .WithTextColors(text, text)
            .WithGuideColors(WithAlpha(grid, 90), WithAlpha(grid, 120))
            .WithPalette(accent, WithAlpha(accent, 190), config.Color, config.ValueColor)
            .WithTypography(18, 11, 10, 9, 9, 9)
            .WithStrokeWidth(2.2)
            .WithMarkerRadius(2.4);
    }

    private static void AddGauge(Chart plot, BgInfoChart chart, IReadOnlyList<double> values, ChartColor accent) {
        var latest = values[values.Count - 1];
        plot.AddGauge(chart.Title, latest, chart.Minimum ?? 0, ResolveMaximum(chart, values, 100), accent);
    }

    private static void AddCircle(Chart plot, BgInfoChart chart, IReadOnlyList<double> values, ChartColor accent) {
        var latest = values[values.Count - 1];
        plot.AddCircle(chart.Title, latest, chart.Minimum ?? 0, ResolveMaximum(chart, values, 100), accent);
    }

    private static void AddBulletRows(Chart plot, BgInfoChart chart, IReadOnlyList<double> values, ChartColor accent) {
        var maximum = ResolveMaximum(chart, values, 100);
        var minimum = chart.Minimum ?? 0;
        var target = chart.Target ?? maximum;
        var ranges = chart.RangeEnds.Count > 0 ? chart.RangeEnds : null;

        for (var i = 0; i < values.Count; i++) {
            plot.AddBullet(LabelAt(chart, i), values[i], target, minimum, maximum, ranges, ColorAt(chart, i) ?? accent);
            plot.Series[plot.Series.Count - 1].WithDataLabels(chart.ShowDataLabels);
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

    private static ChartProgressItem[] BuildProgressItems(BgInfoChart chart, IReadOnlyList<double> values) {
        var items = new ChartProgressItem[values.Count];
        for (var i = 0; i < values.Count; i++) {
            items[i] = new ChartProgressItem(LabelAt(chart, i), values[i], ColorAt(chart, i));
        }

        return items;
    }

    private static ChartPictorialItem[] BuildPictorialItems(BgInfoChart chart, IReadOnlyList<double> values) {
        var items = new ChartPictorialItem[values.Count];
        for (var i = 0; i < values.Count; i++) {
            items[i] = new ChartPictorialItem(LabelAt(chart, i), Math.Max(0, values[i]), ColorAt(chart, i));
        }

        return items;
    }

    private static string LabelAt(BgInfoChart chart, int index) =>
        index < chart.Labels.Count && !string.IsNullOrWhiteSpace(chart.Labels[index])
            ? chart.Labels[index]
            : (index + 1).ToString(System.Globalization.CultureInfo.InvariantCulture);

    private static ChartColor? ColorAt(BgInfoChart chart, int index) =>
        index < chart.Palette.Count ? chart.Palette[index] : null;

    private static void ApplyPointColors(Chart plot, BgInfoChart chart) {
        if (plot.Series.Count == 0 || chart.Palette.Count == 0) {
            return;
        }

        for (var i = 0; i < chart.Palette.Count; i++) {
            plot.Series[0].WithPointColor(i, chart.Palette[i]);
        }
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

    private static TextStyle ResolveChartTitleStyle(BgInfoChart chart, BgInfoConfiguration config) => BgInfoRasterImage.CreateTextStyle(
        chart.TitleFontSize ?? config.FontSize,
        chart.FontFamilyName ?? config.FontFamilyName,
        chart.TitleColor ?? chart.TextColor ?? config.Color,
        chart.TitleFontWeight ?? ResolveWeight(chart.TitleBold) ?? config.FontWeight,
        chart.TitleItalic ?? config.Italic,
        chart.TitleUnderlineStyle ?? ResolveUnderline(chart.TitleUnderline) ?? config.UnderlineStyle,
        chart.TitleStrikethroughStyle ?? config.StrikethroughStyle,
        chart.TitleBaseline ?? config.Baseline,
        chart.TitleTextCase ?? config.TextCase);

    private static TextStyle ResolveChartValueStyle(BgInfoChart chart, BgInfoConfiguration config) => BgInfoRasterImage.CreateTextStyle(
        chart.ValueFontSize ?? config.ValueFontSize,
        chart.FontFamilyName ?? config.ValueFontFamilyName,
        chart.ValueColor ?? chart.TextColor ?? config.ValueColor,
        chart.ValueFontWeight ?? ResolveWeight(chart.ValueBold) ?? config.ValueFontWeight,
        chart.ValueItalic ?? config.ValueItalic,
        chart.ValueUnderlineStyle ?? ResolveUnderline(chart.ValueUnderline) ?? config.ValueUnderlineStyle,
        chart.ValueStrikethroughStyle ?? config.ValueStrikethroughStyle,
        chart.ValueBaseline ?? config.ValueBaseline,
        chart.ValueTextCase ?? config.ValueTextCase);

    private static int? ResolveWeight(bool? bold) => bold.HasValue ? (bold.Value ? 700 : 400) : null;

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
