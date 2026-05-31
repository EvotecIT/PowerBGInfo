using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using ChartForgeX;
using ChartForgeX.Core;
using ChartForgeX.Primitives;
using ChartForgeX.Raster;
using ChartForgeX.Themes;

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
        var plotLeft = (float)padding;
        var plotTop = (float)padding;
        var plotWidth = Math.Max(1f, width - padding * 2f);
        var plotHeight = Math.Max(1f, height - padding * 2f);

        var title = chart.Title ?? string.Empty;
        var showValue = chart.ShowLatestValue && values.Count > 0;
        var latestValueText = showValue ? FormatValue(values[values.Count - 1], chart) : string.Empty;

        var titleColor = chart.TextColor ?? config.Color;
        var valueColor = chart.TextColor ?? config.ValueColor;
        var fontFamily = chart.FontFamilyName ?? config.FontFamilyName;
        var titleSize = !string.IsNullOrWhiteSpace(title)
            ? image.GetTextSize(title, chart.TitleFontSize ?? config.FontSize, fontFamily)
            : SizeF.Empty;
        var valueSize = !string.IsNullOrWhiteSpace(latestValueText)
            ? image.GetTextSize(latestValueText, chart.ValueFontSize ?? config.ValueFontSize, fontFamily)
            : SizeF.Empty;
        var headerHeight = Math.Max(titleSize.Height, valueSize.Height);
        if (headerHeight > 0) {
            plotTop += headerHeight + 4;
            plotHeight = Math.Max(1, height - plotTop - padding);
            if (!string.IsNullOrWhiteSpace(title)) {
                image.AddText(padding, padding, title, titleColor, chart.TitleFontSize ?? config.FontSize, fontFamily);
            }

            if (!string.IsNullOrWhiteSpace(latestValueText)) {
                var valueX = Math.Max(padding, width - padding - valueSize.Width);
                image.AddText(valueX, padding, latestValueText, valueColor, chart.ValueFontSize ?? config.ValueFontSize, fontFamily);
            }
        }

        if (values.Count == 0) {
            return image;
        }

        var plot = BuildChartForgeXChart(chart, values, config, (int)Math.Round(plotWidth), (int)Math.Round(plotHeight));
        DrawPng(image, plot.ToPng(), plotLeft, plotTop, plotWidth, plotHeight);
        return image;
    }

    private static Chart BuildChartForgeXChart(BgInfoChart chart, IReadOnlyList<double> values, BgInfoConfiguration config, int width, int height) {
        var accent = ToChartColor(chart.LineColor ?? config.ValueColor);
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
                plot.AddLine(chart.Title, BuildIndexedPoints(values), accent);
                break;
            case BgInfoChartKind.Area:
                plot.AddSmoothArea(chart.Title, BuildIndexedPoints(values), ToChartColor(chart.FillColor ?? chart.LineColor ?? config.ValueColor));
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
                plot.AddSmoothLine(chart.Title, BuildIndexedPoints(values), accent);
                break;
        }

        return plot;
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
            plot.WithPalette(chart.Palette.Select(ToChartColor).ToArray());
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
        var text = ToChartColor(chart.TextColor ?? config.Color);
        var grid = ToChartColor(chart.GridColor ?? chart.TextColor ?? config.ValueColor);
        return ChartTheme.Minimal()
            .WithSurfaceColors(ChartColor.Transparent, ChartColor.Transparent, ChartColor.Transparent, ChartColor.Transparent, ChartColor.Transparent)
            .WithTextColors(text, text)
            .WithGuideColors(WithAlpha(grid, 90), WithAlpha(grid, 120))
            .WithPalette(accent, WithAlpha(accent, 190), ToChartColor(config.Color), ToChartColor(config.ValueColor))
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
        index < chart.Palette.Count ? ToChartColor(chart.Palette[index]) : null;

    private static void ApplyPointColors(Chart plot, BgInfoChart chart) {
        if (plot.Series.Count == 0 || chart.Palette.Count == 0) {
            return;
        }

        for (var i = 0; i < chart.Palette.Count; i++) {
            plot.Series[0].WithPointColor(i, ToChartColor(chart.Palette[i]));
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

    private static void DrawPng(BgInfoRasterImage image, byte[] png, float x, float y, float width, float height) {
        image.DrawImage(RasterImageDecoder.Decode(png), x, y, width, height);
    }

    private static ChartColor ToChartColor(Color color) => ChartColor.FromRgba(color.R, color.G, color.B, color.A);

    private static ChartColor WithAlpha(ChartColor color, byte alpha) => ChartColor.FromRgba(color.R, color.G, color.B, alpha);

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
