using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using ChartForgeX;
using ChartForgeX.Composition;
using ChartForgeX.Core;
using ChartForgeX.Primitives;
using ChartForgeX.Raster;
using ChartForgeX.Themes;
using ChartForgeX.Typography;
using ChartForgeX.VisualBlocks;

namespace PowerBGInfo;

internal static partial class BgInfoWidgetRenderer {
    /// <summary>
    /// Inner padding of table and list blocks. With zero padding the static label-placement pass moves
    /// edge labels inward and draws leader lines (see design.md), so the blocks keep a small inset.
    /// </summary>
    private const double BlockInset = 6;

    private static RgbaImage RenderMetric(BgInfoMetricWidget widget, BgInfoWidgetStyle style, int width, int height, double padding) {
        var (w, h, scale) = Density(widget, width, height);
        var status = widget.ResolveStatus();
        var statusColor = style.Status.Resolve(status);
        // The panel owns the frame. Without a card, the status bar runs the full body height
        // (the carded status bar is shorter than the card, see design.md), and the left inset clears it.
        var card = MetricCard.Create()
            .WithTheme(style.CreateTheme(palette: style.StatusBlockPalette(status == BgInfoStatus.Info)), style.Mode)
            .WithTransparentBackground()
            .WithCard(false)
            .WithSize(w, h)
            .WithPadding(padding / widget.Scale + (status == BgInfoStatus.None ? 0 : 12), padding / widget.Scale, padding / widget.Scale, padding / widget.Scale)
            .WithPngOutputScale(scale)
            .WithMetric(widget.Label, widget.Value);
        if (!string.IsNullOrWhiteSpace(widget.Unit)) card.WithUnit(widget.Unit);
        if (!string.IsNullOrWhiteSpace(widget.Trend)) card.WithTrend(widget.Trend);
        if (!string.IsNullOrWhiteSpace(widget.Caption)) card.WithCaption(widget.Caption);
        // The top-right badge overlaps the inline history chart and the top-left badge triggers
        // label leader lines in compact cards (see design.md), so the badge is shown without history only.
        if (!string.IsNullOrWhiteSpace(widget.Symbol) && widget.History.Count == 0) card.WithSymbol(widget.Symbol);
        if (status != BgInfoStatus.None) card.WithStatus(BgInfoThreshold.ToVisualStatus(status));
        if (widget.History.Count > 0) {
            var color = statusColor ?? style.Accent;
            if (widget.HistoryKind == BgInfoMetricHistoryKind.Bars) {
                card.WithMiniBars(widget.History, minimum: 0, maximum: widget.HistoryMaximum, color: color, mutedColor: BgInfoWidgetStyle.WithAlpha(style.Muted, 70));
            } else {
                card.WithMiniSparkline(widget.History, minimum: 0, maximum: widget.HistoryMaximum, color: color, fillColor: BgInfoWidgetStyle.WithAlpha(color, 46));
            }
        }

        return card.RenderRgba();
    }

    private static RgbaImage RenderTable(BgInfoTableWidget widget, BgInfoWidgetStyle style, int width, int height) {
        var (w, h, scale) = Density(widget, width, height);
        var table = ChartTable.Create()
            .WithTheme(style.CreateTheme(palette: style.StatusBlockPalette(showsInfo: true)), style.Mode)
            .WithTransparentBackground()
            .WithCard(false)
            .WithSize(w, h)
            .WithPadding(BlockInset)
            .WithPngOutputScale(scale)
            .WithDenseMode(widget.Dense)
            .WithHeader(widget.ShowHeader)
            .WithRowStriping(widget.RowStriping)
            .WithColumns(widget.Columns.ToArray());
        foreach (var row in widget.Rows) table.AddRow(row.Cast<object?>().ToArray());
        if (!string.IsNullOrWhiteSpace(widget.StatusColumn)) table.WithStatusColumn(widget.StatusColumn!);
        return table.RenderRgba();
    }

    private static RgbaImage RenderList(BgInfoListWidget widget, BgInfoWidgetStyle style, int width, int height) {
        var (w, h, scale) = Density(widget, width, height);
        var list = ChartList.Create()
            .WithTheme(style.CreateTheme(palette: style.StatusBlockPalette(widget.Items.Any(item => item.Status == BgInfoStatus.Info))), style.Mode)
            .WithTransparentBackground()
            .WithCard(false)
            .WithSize(w, h)
            .WithPadding(BlockInset)
            .WithPngOutputScale(scale)
            .WithDenseMode(widget.Dense);
        foreach (var item in widget.Items) {
            if (item.Checked.HasValue) {
                list.AddCheckItem(item.Text, item.Checked.Value, item.Value);
            } else if (item.Status != BgInfoStatus.None) {
                list.AddStatusItem(item.Text, BgInfoThreshold.ToVisualStatus(item.Status), item.Value);
            } else {
                list.AddItem(item.Text, item.Value);
            }
        }

        // Adding check items switches the marker, so the requested marker is applied last.
        list.WithMarker(widget.Marker switch {
            BgInfoListMarker.Number => VisualListMarker.Number,
            BgInfoListMarker.Check => VisualListMarker.Check,
            BgInfoListMarker.Status => VisualListMarker.Status,
            BgInfoListMarker.None => VisualListMarker.None,
            BgInfoListMarker.Bullet => VisualListMarker.Bullet,
            _ => widget.Items.Any(item => item.Checked.HasValue) ? VisualListMarker.Check
                : widget.Items.Any(item => item.Status != BgInfoStatus.None) ? VisualListMarker.Status
                : VisualListMarker.Bullet
        });
        return list.RenderRgba();
    }

    private static Chart CreateChart(BgInfoWidget widget, BgInfoWidgetStyle style, int width, int height, bool axes) {
        var (w, h, scale) = Density(widget, width, height);
        var chart = Chart.Create()
            .WithSize(w, h)
            .WithTheme(style.CreateTheme(ChartColor.Transparent), style.Mode)
            .WithTransparentBackground()
            .WithHeader(false)
            .WithCard(false)
            .WithPlotBackground(false)
            .WithAxisLines(false)
            .WithPadding(2, 2, 2, 2)
            .WithPngOutputScale(scale);
        if (!axes) chart.WithAxes(false);
        return chart;
    }

    private static RgbaImage RenderCalendar(BgInfoCalendarHeatmapWidget widget, BgInfoWidgetStyle style, int width, int height) {
        // Size cells from the panel so the grid fills it; the chart default keeps a compact cell.
        var (logicalWidth, logicalHeight, _) = Density(widget, width, height);
        var first = widget.Items.Min(item => item.Date.Date);
        var last = widget.Items.Max(item => item.Date.Date);
        var weeks = Math.Ceiling(((last - first).TotalDays + 1) / 7d) + 1;
        var cell = Math.Max(8, Math.Min(40, Math.Min((logicalWidth - 48) / weeks, (logicalHeight - (widget.ShowLegend ? 64 : 36)) / 7.6)));
        // The week grid is centered while weekday labels stay at the left edge (see design.md),
        // so the chart gets only the width its grid needs and sits at the left of the panel.
        var gridWidth = Math.Min(logicalWidth, (int)Math.Ceiling(56 + weeks * (Math.Floor(cell) + 4)));
        var gridPixels = (int)Math.Round(gridWidth * widget.Scale);
        var chart = CreateChart(widget, style, Math.Min(width, gridPixels), height, axes: true)
            .WithLegend(widget.ShowLegend)
            .ConfigureCalendar(calendar => {
                calendar.FirstDayOfWeek = widget.FirstDayOfWeek;
                calendar.CellSize = Math.Floor(cell);
                calendar.MaximumCellSize = Math.Floor(cell);
            })
            .AddCalendarHeatmap(widget.Title.Length > 0 ? widget.Title : "Calendar",
                widget.Items.Select(item => new ChartCalendarHeatmapItem(item.Date.Date, item.Value)),
                widget.Color ?? style.Accent);
        var grid = chart.ToRgbaImage();
        if (gridPixels >= width) return grid;
        var output = ImageComposition.CreateTransparent(width * 2, height * 2);
        output.DrawImage(grid, 0, 0, Math.Min(width, gridPixels) * 2, height * 2);
        return output.ToImage();
    }

    private static RgbaImage RenderTimeline(BgInfoTimelineWidget widget, BgInfoWidgetStyle style, int width, int height) {
        var categories = new List<ChartStateCategory>();
        var keys = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        var palette = style.Palette;
        foreach (var item in widget.Items) {
            var label = string.IsNullOrWhiteSpace(item.Category) ? "Scheduled" : item.Category.Trim();
            if (keys.ContainsKey(label)) continue;
            var key = "c" + keys.Count.ToString(CultureInfo.InvariantCulture);
            keys[label] = key;
            var color = item.Color ?? style.Status.Resolve(item.Status) ?? palette[(keys.Count - 1) % palette.Length];
            categories.Add(new ChartStateCategory(key, label, color));
        }

        // The lane axis defaults to ISO dates (2026-10-14); a wallpaper reads better with "Wed 14" or "14:00".
        var first = widget.Items.Min(item => item.Start);
        var last = widget.Items.Max(item => item.End ?? item.Start);
        if (widget.Now is { } now) {
            if (now < first) first = now;
            if (now > last) last = now;
        }
        var multiDay = first.Date != last.Date && last - first > TimeSpan.FromDays(1);
        var chart = CreateChart(widget, style, width, height, axes: true)
            .WithLegend(widget.ShowLegend)
            .WithDataLabels(true)
            .WithStateCategories(categories)
            .ConfigureXAxis(axis => axis.LabelFormatter = value => FormatTimelineTick(value, multiDay));
        if (widget.Now.HasValue) chart.WithGanttLaneNow(widget.Now.Value);
        foreach (var lane in widget.Items.GroupBy(item => item.Lane.Trim(), StringComparer.OrdinalIgnoreCase)) {
            chart.AddGanttLane(lane.Key, lane.Select(item => new ChartGanttLaneItem(item.Start, item.End,
                keys[string.IsNullOrWhiteSpace(item.Category) ? "Scheduled" : item.Category.Trim()],
                string.IsNullOrWhiteSpace(item.Label) ? null : item.Label)));
        }

        return chart.ToRgbaImage();
    }

    /// <summary>Formats a lane-axis tick: the day at midnight of a multi-day range, otherwise the 24-hour time of day.</summary>
    internal static string FormatTimelineTick(double value, bool multiDay) {
        if (double.IsNaN(value) || double.IsInfinity(value) || value < -657435 || value > 2958465) return string.Empty;
        var time = DateTime.FromOADate(value);
        // Ticks are computed in floating point; snap to the minute so 13:59:59.999 reads 14:00.
        time = new DateTime((long)Math.Round(time.Ticks / (double)TimeSpan.TicksPerMinute) * TimeSpan.TicksPerMinute);
        var culture = CultureInfo.CurrentCulture;
        if (!multiDay) return time.ToString("HH:mm", culture);
        return time.TimeOfDay == TimeSpan.Zero ? time.ToString("ddd d", culture) : time.ToString("ddd HH:mm", culture);
    }
}
