using System;
using System.Collections.Generic;
using System.Globalization;
using ChartForgeX;
using ChartForgeX.Core;
using ChartForgeX.Primitives;
using ChartForgeX.Raster;
using ChartForgeX.Themes;

namespace PowerBGInfo;

internal static partial class BgInfoWidgetRenderer {
    private static RgbaImage RenderTreemap(BgInfoTreemapWidget widget, BgInfoWidgetStyle style, int width, int height) {
        var items = new List<ChartHierarchyItem>();
        var groups = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        var index = 0;
        foreach (var item in widget.Items) {
            string? parent = null;
            if (!string.IsNullOrWhiteSpace(item.Group)) {
                var groupName = item.Group!.Trim();
                if (!groups.TryGetValue(groupName, out parent)) {
                    parent = "group:" + groupName;
                    groups.Add(groupName, parent);
                    items.Add(new ChartHierarchyItem(parent, groupName));
                }
            }
            items.Add(new ChartHierarchyItem("item:" + (index++).ToString(CultureInfo.InvariantCulture), item.Name, parent, item.Value, item.ColorValue));
        }

        var suffix = widget.ValueSuffix ?? string.Empty;
        var scale = widget.HasColorValues ? CreateTreemapColorScale(widget, style) : null;
        var chart = CreateChart(widget, style, width, height, axes: false)
            .WithLegend(false)
            .WithDataLabels(true)
            .WithValueFormatter(value => value.ToString("0.#", CultureInfo.CurrentCulture) + suffix)
            .ConfigureTreemap(options => {
                options.ShowValues = widget.ShowValues;
                options.ShowGroupLabels = groups.Count > 0;
                if (scale != null) options.ColorScale = scale;
            })
            .AddTreemap(widget.Title.Length > 0 ? widget.Title : "Treemap", items);
        return chart.ToRgbaImage();
    }

    /// <summary>
    /// Colors rectangles by their second measure: status bands when thresholds are set (healthy ones muted), otherwise a
    /// ramp from a faint accent to the full accent. Groups carry no color value and keep a quiet neutral fill.
    /// </summary>
    internal static ChartColorScale CreateTreemapColorScale(BgInfoTreemapWidget widget, BgInfoWidgetStyle style) {
        var backdrop = ChartColor.FromHex(style.Mode == VisualThemeMode.Dark ? "#0F172A" : "#FFFFFF");
        var neutral = Mix(style.Muted, backdrop, 0.72);
        if (widget.Threshold is not { IsDefined: true } threshold) {
            return ChartColorScale.Sequential(Mix(style.Accent, backdrop, 0.7), style.Accent).WithNoDataColor(neutral);
        }

        var colors = threshold.Colors ?? style.Status;
        // Healthy rectangles stay quiet so the warning and critical ones stand out.
        var ok = Mix(colors.Ok, backdrop, 0.45);
        // A warning equal to the critical threshold adds no band of its own.
        var warning = threshold.Warning == threshold.Critical ? null : threshold.Warning;
        var critical = threshold.Critical;
        var bands = new List<ChartColorBand>();
        if (threshold.Direction == BgInfoThresholdDirection.Above) {
            // Bands end before their upper bound, which matches "at or above the threshold is worse".
            var first = warning ?? critical!.Value;
            bands.Add(new ChartColorBand(first, ok, "ok"));
            if (warning.HasValue && critical.HasValue) bands.Add(new ChartColorBand(critical.Value, colors.Warning, "warning"));
            bands.Add(new ChartColorBand(null, critical.HasValue ? colors.Critical : colors.Warning, critical.HasValue ? "critical" : "warning"));
        } else {
            // Lower is worse: a value at the threshold belongs to the worse band, so each bound sits just above it.
            if (critical.HasValue) bands.Add(new ChartColorBand(JustAbove(critical.Value), colors.Critical, "critical"));
            if (warning.HasValue) bands.Add(new ChartColorBand(JustAbove(warning.Value), colors.Warning, "warning"));
            bands.Add(new ChartColorBand(null, ok, "ok"));
        }
        return ChartColorScale.Discrete(bands).WithNoDataColor(neutral);
    }

    private static double JustAbove(double value) => value + Math.Max(1e-9, Math.Abs(value) * 1e-9);

    private static ChartColor Mix(ChartColor color, ChartColor toward, double amount) {
        byte Channel(byte from, byte to) => (byte)Math.Round(from + (to - from) * amount);
        return ChartColor.FromRgba(Channel(color.R, toward.R), Channel(color.G, toward.G), Channel(color.B, toward.B), 255);
    }
}
