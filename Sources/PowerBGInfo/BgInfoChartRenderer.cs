using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using ImagePlayground.Gdi;
using GdiImage = ImagePlayground.Gdi.Image;

namespace PowerBGInfo;

internal static class BgInfoChartRenderer {
    public static GdiImage Render(BgInfoChart chart, IReadOnlyList<double> values, BgInfoConfiguration config) {
        if (chart == null) throw new ArgumentNullException(nameof(chart));
        if (config == null) throw new ArgumentNullException(nameof(config));

        int width = Math.Max(1, chart.Width);
        int height = Math.Max(1, chart.Height);
        var image = new GdiImage();
        var background = chart.BackgroundColor ?? Color.Transparent;
        image.Create(string.Empty, width, height, background);

        int padding = Math.Max(0, chart.Padding);
        float plotLeft = padding;
        float plotTop = padding;
        float plotWidth = Math.Max(1, width - padding * 2);
        float plotHeight = Math.Max(1, height - padding * 2);

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
                float valueX = Math.Max(padding, width - padding - valueSize.Width);
                image.AddText(valueX, padding, latestValueText, valueColor, chart.ValueFontSize ?? config.ValueFontSize, fontFamily);
            }
        }

        if (values.Count == 0) {
            return image;
        }

        if (chart.ShowGrid && chart.GridLineCount > 0) {
            RenderGrid(image, chart, config, plotLeft, plotTop, plotWidth, plotHeight);
        }

        switch (chart.Kind) {
            case BgInfoChartKind.Bar:
                RenderBars(image, chart, values, plotLeft, plotTop, plotWidth, plotHeight, config);
                break;
            default:
                RenderSparkline(image, chart, values, plotLeft, plotTop, plotWidth, plotHeight, config);
                break;
        }

        return image;
    }

    private static void RenderGrid(GdiImage image, BgInfoChart chart, BgInfoConfiguration config, float left, float top, float width, float height) {
        int lines = Math.Max(1, chart.GridLineCount);
        var baseColor = chart.GridColor ?? chart.TextColor ?? config.ValueColor;
        var gridColor = Color.FromArgb(90, baseColor.R, baseColor.G, baseColor.B);
        image.WithGraphics(graphics => {
            using var pen = new Pen(gridColor, 1f);
            float step = height / (lines + 1);
            for (int i = 1; i <= lines; i++) {
                float y = top + step * i;
                graphics.DrawLine(pen, left, y, left + width, y);
            }
        });
    }

    private static void RenderSparkline(GdiImage image, BgInfoChart chart, IReadOnlyList<double> values, float left, float top, float width, float height, BgInfoConfiguration config) {
        if (values.Count == 0) {
            return;
        }

        double min = values.Min();
        double max = values.Max();
        double range = max - min;
        if (Math.Abs(range) < double.Epsilon) {
            range = 1;
            min -= 0.5;
            max += 0.5;
        }

        var points = new PointF[values.Count];
        float step = values.Count > 1 ? width / (values.Count - 1) : 0f;
        for (int i = 0; i < values.Count; i++) {
            double normalized = (values[i] - min) / range;
            float x = left + step * i;
            float y = top + (float)((1 - normalized) * height);
            points[i] = new PointF(x, y);
        }

        var lineColor = chart.LineColor ?? config.ValueColor;
        image.WithGraphics(graphics => {
            if (chart.FillColor.HasValue && points.Length > 1) {
                using var brush = new SolidBrush(chart.FillColor.Value);
                var poly = new List<PointF>(points.Length + 2) {
                    new PointF(points[0].X, top + height)
                };
                poly.AddRange(points);
                poly.Add(new PointF(points[points.Length - 1].X, top + height));
                graphics.FillPolygon(brush, poly.ToArray());
            }
            using var pen = new Pen(lineColor, 2f);
            if (points.Length == 1) {
                graphics.DrawLine(pen, left, points[0].Y, left + width, points[0].Y);
                using var dotBrush = new SolidBrush(lineColor);
                graphics.FillEllipse(dotBrush, points[0].X - 3, points[0].Y - 3, 6, 6);
            } else {
                graphics.DrawLines(pen, points);
            }
        });
    }

    private static void RenderBars(GdiImage image, BgInfoChart chart, IReadOnlyList<double> values, float left, float top, float width, float height, BgInfoConfiguration config) {
        if (values.Count == 0) {
            return;
        }

        double max = values.Max();
        if (max <= 0) {
            max = 1;
        }

        float gap = Clamp(chart.BarGap, 0f, 0.9f);
        float groupWidth = width / values.Count;
        float barWidth = Math.Max(1f, groupWidth * (1 - gap));
        var barColor = chart.LineColor ?? config.ValueColor;

        image.WithGraphics(graphics => {
            using var brush = new SolidBrush(barColor);
            for (int i = 0; i < values.Count; i++) {
                double value = Math.Max(0, values[i]);
                float barHeight = (float)(value / max * height);
                float x = left + i * groupWidth + (groupWidth - barWidth) / 2f;
                float y = top + height - barHeight;
                graphics.FillRectangle(brush, x, y, barWidth, barHeight);
            }
        });
    }

    private static string FormatValue(double value, BgInfoChart chart) {
        var format = string.IsNullOrWhiteSpace(chart.ValueFormat) ? "0.##" : chart.ValueFormat;
        var text = value.ToString(format, System.Globalization.CultureInfo.InvariantCulture);
        return string.IsNullOrWhiteSpace(chart.ValueSuffix) ? text : text + chart.ValueSuffix;
    }

    private static float Clamp(float value, float min, float max) {
        if (value < min) return min;
        if (value > max) return max;
        return value;
    }
}
