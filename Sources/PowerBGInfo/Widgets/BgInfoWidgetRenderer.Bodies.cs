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

    private static RgbaImage RenderTable(BgInfoTableWidget widget, BgInfoWidgetStyle style, int width, int height, int? outputScale = null) {
        if (!CanRenderRowsDirectly(widget, width, height, outputScale)) return RenderBatchedRows(widget, style, width, height);
        var (w, h, scale) = Density(widget, width, height);
        var table = ChartTable.Create()
            .WithTheme(style.CreateTheme(palette: style.StatusBlockPalette(showsInfo: true)), style.Mode)
            .WithTransparentBackground()
            .WithCard(false)
            .WithSize(w, h)
            .WithPadding(BlockInset)
            .WithPngOutputScale(outputScale ?? scale)
            .WithDenseMode(widget.Dense)
            .WithHeader(widget.ShowHeader)
            .WithRowStriping(widget.RowStriping);
        AddTableColumns(widget, table, w);
        foreach (var row in widget.Rows) table.AddRow(row.Cast<object?>().ToArray());
        if (!string.IsNullOrWhiteSpace(widget.StatusColumn)) {
            table.WithStatusColumn(widget.StatusColumn!);
            ApplyStatusMap(widget, table);
        }
        return table.RenderRgba();
    }

    // Equal columns suit plain tables. A status column also carries a dot, so tables with one size each
    // column by its longest text and give the status column room for the dot; long words then fit.
    private static void AddTableColumns(BgInfoTableWidget widget, ChartTable table, int width) {
        var status = string.IsNullOrWhiteSpace(widget.StatusColumn)
            ? -1
            : widget.Columns.FindIndex(name => string.Equals(name, widget.StatusColumn, StringComparison.OrdinalIgnoreCase));
        if (status < 0) {
            table.WithColumns(widget.Columns.ToArray());
            return;
        }
        var weights = new double[widget.Columns.Count];
        for (var column = 0; column < weights.Length; column++) {
            var longest = widget.Columns[column].Length;
            foreach (var row in widget.Rows) longest = Math.Max(longest, row[column]?.Length ?? 0);
            weights[column] = Math.Max(4, Math.Min(32, longest)) + (column == status ? 3 : 0) + 2;
        }
        var available = Math.Max(weights.Length * 24d, width - 2 * BlockInset);
        var total = weights.Sum();
        for (var column = 0; column < weights.Length; column++) {
            table.AddColumn(widget.Columns[column], width: Math.Floor(available * weights[column] / total));
        }
    }

    // An explicit cell status takes precedence over ChartForgeX's built-in status words.
    private static void ApplyStatusMap(BgInfoTableWidget widget, ChartTable table) {
        if (widget.StatusMap.Count == 0) return;
        var column = widget.Columns.FindIndex(name => string.Equals(name, widget.StatusColumn, StringComparison.OrdinalIgnoreCase));
        if (column < 0) return;
        for (var index = 0; index < widget.Rows.Count; index++) {
            if (!widget.StatusMap.TryGetValue(widget.Rows[index][column]?.Trim() ?? string.Empty, out var status)) continue;
            var visual = BgInfoThreshold.ToVisualStatus(status);
            table.ConfigureRow(index, row => row.Cells[column].Status = visual);
        }
    }

    private static RgbaImage RenderList(BgInfoListWidget widget, BgInfoWidgetStyle style, int width, int height, int? outputScale = null) {
        if (!CanRenderRowsDirectly(widget, width, height, outputScale)) return RenderBatchedRows(widget, style, width, height);
        var (w, h, scale) = Density(widget, width, height);
        var list = ChartList.Create()
            .WithTheme(style.CreateTheme(palette: style.StatusBlockPalette(widget.Items.Any(item => item.Status == BgInfoStatus.Info))), style.Mode)
            .WithTransparentBackground()
            .WithCard(false)
            .WithSize(w, h)
            .WithPadding(BlockInset)
            .WithPngOutputScale(outputScale ?? scale)
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

    private static Chart CreateChart(BgInfoWidget widget, BgInfoWidgetStyle style, int width, int height, bool axes, double? renderScale = null) {
        var (w, h, scale) = renderScale.HasValue ? Density(width, height, renderScale.Value) : Density(widget, width, height);
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
            .WithHeatmapColorScale(CalendarScale(widget, style))
            .AddCalendarHeatmap(widget.Title.Length > 0 ? widget.Title : "Calendar",
                widget.Items.Select(item => new ChartCalendarHeatmapItem(item.Date.Date, item.Value)),
                widget.Color ?? style.Accent);
        var grid = chart.ToRgbaImage();
        if (gridPixels >= width) return grid;
        // A wide panel centres the grid instead of leaving the space to its right empty.
        var output = ImageComposition.CreateTransparent(width * 2, height * 2);
        var gridWidth2 = Math.Min(width, gridPixels) * 2;
        output.DrawImage(grid, (width * 2 - gridWidth2) / 2d, 0, gridWidth2, height * 2);
        return output.ToImage();
    }

    // Zero days get a visible neutral cell, distinct from both the lowest value and days with no data
    // (which stay empty), in light and dark. Values ramp from a faint tint at the lowest value through a
    // clear tint at the median to the full color at the highest, so everyday variation and spikes both read.
    internal static ChartColorScale CalendarScale(BgInfoCalendarHeatmapWidget widget, BgInfoWidgetStyle style) {
        var color = widget.Color ?? style.Accent;
        ChartColor Tint(byte alpha) => ChartColor.FromRgba(color.R, color.G, color.B, alpha);
        var track = ChartColor.FromRgba(style.Muted.R, style.Muted.G, style.Muted.B, style.Mode == VisualThemeMode.Dark ? (byte)56 : (byte)44);
        var values = widget.Items.Select(item => item.Value).Where(value => value > 0).OrderBy(value => value).ToList();
        var stops = new List<ChartColorStop> { new(0, track) };
        if (values.Count == 0) {
            stops.Add(new ChartColorStop(1, color));
            return ChartColorScale.Sequential(stops);
        }
        var lowest = values[0];
        var highest = values[values.Count - 1];
        var median = values[values.Count / 2];
        if (lowest < highest) {
            stops.Add(new ChartColorStop(lowest, Tint(48)));
            if (median > lowest && median < highest) stops.Add(new ChartColorStop(median, Tint(150)));
        }
        stops.Add(new ChartColorStop(highest, color));
        return ChartColorScale.Sequential(stops);
    }

    private static RgbaImage RenderTimeline(BgInfoTimelineWidget widget, BgInfoWidgetStyle style, int width, int height) {
        var categories = ResolveTimelineCategories(widget.Items, style);
        var keys = categories.ToDictionary(category => category.Label, category => category.Key, StringComparer.OrdinalIgnoreCase);

        // The lane axis defaults to ISO dates (2026-10-14); a wallpaper reads better with "Wed 14" or "14:00".
        var first = widget.Items.Min(item => item.Start);
        var last = widget.Items.Max(item => item.End ?? item.Start);
        if (widget.Now is { } now) {
            if (now < first) first = now;
            if (now > last) last = now;
        }
        var multiDay = first.Date != last.Date;
        // A taller panel gets thicker lanes and larger lane labels, up to half again the requested scale.
        var lanes = widget.Items.Select(item => item.Lane.Trim()).Distinct(StringComparer.OrdinalIgnoreCase).Count();
        var natural = lanes * 30d + 44 + (widget.ShowLegend ? 34 : 0);
        var grow = Math.Max(1, Math.Min(1.5, height / (natural * widget.Scale)));
        var renderScale = widget.Scale * grow;
        var chart = CreateChart(widget, style, width, height, axes: true, renderScale)
            .WithLegend(widget.ShowLegend)
            .WithDataLabels(true)
            .WithStateCategories(categories)
            .ConfigureXAxis(axis => axis.LabelFormatter = value => FormatTimelineTick(value, multiDay));
        if (multiDay) {
            var ticks = TimelineDayTicks(first, last, width / renderScale);
            chart.ConfigureXAxis(axis => {
                axis.LabelDensity = ChartLabelDensity.All;
                axis.Labels.Clear();
                foreach (var tick in ticks) axis.Labels.Add(new ChartAxisLabel(tick.ToOADate(), FormatTimelineDay(tick)));
            });
        }
        if (widget.Now.HasValue) chart.WithGanttLaneNow(widget.Now.Value);
        foreach (var lane in widget.Items.GroupBy(item => item.Lane.Trim(), StringComparer.OrdinalIgnoreCase)) {
            chart.AddGanttLane(lane.Key, lane.Select(item => new ChartGanttLaneItem(item.Start, item.End,
                keys[NormalizeTimelineCategory(item.Category)],
                string.IsNullOrWhiteSpace(item.Label) ? null : item.Label)));
        }

        return chart.ToRgbaImage();
    }

    // Day ticks from the first day of the range, every 1, 2 or 3 days, or weekly from a Monday, so that
    // the labels fit the lane axis. The first tick is the range's first day (or its first Monday).
    internal static List<DateTime> TimelineDayTicks(DateTime first, DateTime last, double logicalWidth) {
        var start = first.Date;
        var end = last.Date;
        var span = Math.Max(1, (end - start).TotalDays);
        var slots = Math.Max(2, (int)Math.Floor((logicalWidth - 140) / 72));
        var ticks = new List<DateTime>();
        foreach (var step in new[] { 1, 2, 3 }) {
            if (span / step + 1 > slots) continue;
            for (var day = start; day <= end; day = day.AddDays(step)) ticks.Add(day);
            return ticks;
        }
        var weeks = 1;
        while (span / (7d * weeks) + 1 > slots) weeks++;
        var monday = start.AddDays(((int)DayOfWeek.Monday - (int)start.DayOfWeek + 7) % 7);
        for (var day = monday; day <= end; day = day.AddDays(7 * weeks)) ticks.Add(day);
        if (ticks.Count == 0 || (ticks[0] - start).TotalDays >= 2) ticks.Insert(0, start);
        return ticks;
    }

    internal static string FormatTimelineDay(DateTime day) => day.ToString("ddd d", CultureInfo.CurrentCulture);

    internal static List<ChartStateCategory> ResolveTimelineCategories(IEnumerable<BgInfoTimelineItem> items, BgInfoWidgetStyle style) {
        var categories = new List<ChartStateCategory>();
        var palette = style.Palette;
        var index = 0;
        foreach (var group in items.GroupBy(item => NormalizeTimelineCategory(item.Category), StringComparer.OrdinalIgnoreCase)) {
            ChartColor? explicitColor = null;
            var explicitStatus = BgInfoStatus.None;
            foreach (var item in group) {
                if (!explicitColor.HasValue && item.Color.HasValue) explicitColor = item.Color.Value;
                if (explicitStatus == BgInfoStatus.None && item.Status != BgInfoStatus.None) explicitStatus = item.Status;
            }
            var color = explicitColor ?? style.Status.Resolve(explicitStatus) ?? palette[index % palette.Length];
            var key = "c" + index.ToString(CultureInfo.InvariantCulture);
            categories.Add(new ChartStateCategory(key, group.Key, color));
            index++;
        }
        return categories;
    }

    private static string NormalizeTimelineCategory(string? category) =>
        string.IsNullOrWhiteSpace(category) ? "Scheduled" : category!.Trim();

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
