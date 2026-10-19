using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using ChartForgeX.Themes;
using ChartForgeX.Typography;

namespace PowerBGInfo;

public static partial class BgInfoConfigurationJson {
    private static void MapExtensions(BgInfoConfigurationFile model, BgInfoConfiguration configuration) {
        configuration.Layout = MapLayout(model.Layout);
        if (model.Widgets == null) return;
        foreach (var widgetModel in model.Widgets) {
            var widget = widgetModel == null ? null : MapWidget(widgetModel);
            if (widget != null) configuration.Widgets.Add(widget);
        }
    }

    private static void WriteExtensions(BgInfoConfiguration configuration, BgInfoConfigurationFile model) {
        model.Layout = MapLayoutFile(configuration.Layout);
        if (model.Charts != null) {
            for (var i = 0; i < configuration.Charts.Count && i < model.Charts.Count; i++) {
                var chart = configuration.Charts[i];
                model.Charts[i].Region = chart.Region == BgInfoRegion.None ? null : chart.Region.ToString();
                model.Charts[i].RegionOrder = chart.RegionOrder == 0 ? null : chart.RegionOrder;
                model.Charts[i].Threshold = MapThresholdFile(chart.Threshold);
            }
        }
        if (model.Topologies != null) {
            for (var i = 0; i < configuration.Topologies.Count && i < model.Topologies.Count; i++) {
                var topology = configuration.Topologies[i];
                model.Topologies[i].Region = topology.Region == BgInfoRegion.None ? null : topology.Region.ToString();
                model.Topologies[i].RegionOrder = topology.RegionOrder == 0 ? null : topology.RegionOrder;
            }
        }
        if (model.VisualCanvases != null) {
            var visuals = configuration.VisualCanvases.Where(visual => visual != null).ToList();
            for (var i = 0; i < visuals.Count && i < model.VisualCanvases.Count; i++) {
                model.VisualCanvases[i].Region = visuals[i].Region == BgInfoRegion.None ? null : visuals[i].Region.ToString();
                model.VisualCanvases[i].RegionOrder = visuals[i].RegionOrder == 0 ? null : visuals[i].RegionOrder;
                model.VisualCanvases[i].RailMarginX = visuals[i].RailMarginX;
                model.VisualCanvases[i].RailMarginY = visuals[i].RailMarginY;
                model.VisualCanvases[i].FontFamilyName = visuals[i].FontFamilyName;
            }
        }
        if (configuration.Widgets.Count > 0) {
            model.Widgets = configuration.Widgets.Where(widget => widget != null).Select(MapWidgetFile).ToList();
        }
    }

    private static void MapPlacement(BgInfoChartFile model, BgInfoChart chart) {
        chart.Region = ParseRegion(model.Region);
        chart.RegionOrder = model.RegionOrder ?? 0;
        chart.Threshold = MapThreshold(model.Threshold, chart.ThemeMode);
    }

    private static BgInfoRegion ParseRegion(string? text) {
        if (string.IsNullOrWhiteSpace(text)) return BgInfoRegion.None;
        if (TryParseDefinedEnumName<BgInfoRegion>(text, out var region)) return region;
        throw new InvalidDataException($"Unknown region '{text}'. Expected None, Left, Right, Top, Bottom or Center.");
    }

    private static BgInfoLayout? MapLayout(BgInfoLayoutFile? model) {
        if (model == null) return null;
        var layout = new BgInfoLayout();
        if (model.MarginX.HasValue) layout.MarginX = Math.Max(0, model.MarginX.Value);
        if (model.MarginY.HasValue) layout.MarginY = Math.Max(0, model.MarginY.Value);
        if (model.Gap.HasValue) layout.Gap = Math.Max(0, model.Gap.Value);
        if (model.ColumnWidth.HasValue) layout.ColumnWidth = Math.Max(0, model.ColumnWidth.Value);
        if (model.BandHeight.HasValue) layout.BandHeight = Math.Max(0, model.BandHeight.Value);
        if (model.TaskbarReserve.HasValue) layout.TaskbarReserve = model.TaskbarReserve.Value;
        if (model.IconAreaWidth.HasValue) layout.IconAreaWidth = Math.Max(0, model.IconAreaWidth.Value);
        if (model.MaximumPanelHeight.HasValue) layout.MaximumPanelHeight = Math.Max(0, model.MaximumPanelHeight.Value);
        ApplyEnum<BgInfoColumnAlignment>(model.ColumnAlignment, value => layout.ColumnAlignment = value);
        return layout;
    }

    private static BgInfoLayoutFile? MapLayoutFile(BgInfoLayout? layout) => layout == null ? null : new BgInfoLayoutFile {
        MarginX = layout.MarginX, MarginY = layout.MarginY, Gap = layout.Gap,
        ColumnWidth = layout.ColumnWidth > 0 ? layout.ColumnWidth : null,
        BandHeight = layout.BandHeight > 0 ? layout.BandHeight : null,
        TaskbarReserve = layout.TaskbarReserve >= 0 ? layout.TaskbarReserve : null,
        IconAreaWidth = layout.IconAreaWidth > 0 ? layout.IconAreaWidth : null,
        MaximumPanelHeight = layout.MaximumPanelHeight > 0 ? layout.MaximumPanelHeight : null,
        ColumnAlignment = layout.ColumnAlignment == BgInfoColumnAlignment.Top ? null : layout.ColumnAlignment.ToString()
    };

    private static BgInfoThreshold? MapThreshold(BgInfoThresholdFile? model, VisualThemeMode mode) {
        if (model == null || (!model.Warning.HasValue && !model.Critical.HasValue)) return null;
        var threshold = new BgInfoThreshold { Warning = model.Warning, Critical = model.Critical };
        ApplyEnum<BgInfoThresholdDirection>(model.Direction, value => threshold.Direction = value);
        threshold.Colors = MapStatusColors(model.Ok, model.WarningColor, model.CriticalColor, model.Info, mode);
        threshold.Validate();
        return threshold;
    }

    private static BgInfoThresholdFile? MapThresholdFile(BgInfoThreshold? threshold) => threshold is not { IsDefined: true } ? null : new BgInfoThresholdFile {
        Warning = threshold.Warning,
        Critical = threshold.Critical,
        Direction = threshold.Direction == BgInfoThresholdDirection.Above ? null : threshold.Direction.ToString(),
        Ok = threshold.Colors == null ? null : BgInfoColorParser.ToHex(threshold.Colors.Ok),
        WarningColor = threshold.Colors == null ? null : BgInfoColorParser.ToHex(threshold.Colors.Warning),
        CriticalColor = threshold.Colors == null ? null : BgInfoColorParser.ToHex(threshold.Colors.Critical),
        Info = threshold.Colors == null ? null : BgInfoColorParser.ToHex(threshold.Colors.Info)
    };

    private static BgInfoStatusColors? MapStatusColors(string? ok, string? warning, string? critical, string? info, VisualThemeMode mode) {
        if (ok == null && warning == null && critical == null && info == null) return null;
        var colors = BgInfoStatusColors.Default(mode);
        ApplyColor(ok, value => colors.Ok = value);
        ApplyColor(warning, value => colors.Warning = value);
        ApplyColor(critical, value => colors.Critical = value);
        ApplyColor(info, value => colors.Info = value);
        return colors;
    }

    private static BgInfoWidget? MapWidget(BgInfoWidgetFile model) {
        if (!TryParseDefinedEnumName<BgInfoWidgetKind>(model.Kind, out var kind)) {
            throw new InvalidDataException("Unknown widget kind '" + (model.Kind ?? "<missing>") + "'.");
        }
        var themeMode = TryParseDefinedEnumName<VisualThemeMode>(model.ThemeMode, out var configuredMode)
            ? configuredMode
            : VisualThemeMode.Dark;
        BgInfoWidget widget = kind switch {
            BgInfoWidgetKind.Metric => MapMetric(model, themeMode),
            BgInfoWidgetKind.Table => MapTable(model),
            BgInfoWidgetKind.List => MapList(model),
            BgInfoWidgetKind.CalendarHeatmap => MapCalendar(model),
            BgInfoWidgetKind.Timeline => MapTimeline(model),
            BgInfoWidgetKind.Treemap => MapTreemap(model),
            BgInfoWidgetKind.Mermaid => new BgInfoMermaidWidget { Definition = model.Definition ?? string.Empty },
            _ => MapQrCode(model)
        };

        widget.Id = model.Id ?? string.Empty;
        widget.Title = model.Title ?? string.Empty;
        widget.Note = model.Note ?? string.Empty;
        if (model.Width.HasValue) widget.Width = Math.Max(0, model.Width.Value);
        if (model.Height.HasValue) widget.Height = Math.Max(0, model.Height.Value);
        widget.Region = ParseRegion(model.Region);
        widget.RegionOrder = model.RegionOrder ?? 0;
        ApplyEnum<BgInfoTextPosition>(model.Anchor, value => widget.Anchor = value);
        if (model.OffsetX.HasValue) widget.OffsetX = model.OffsetX.Value;
        if (model.OffsetY.HasValue) widget.OffsetY = model.OffsetY.Value;
        widget.PositionX = model.PositionX;
        widget.PositionY = model.PositionY;
        widget.ThemeMode = themeMode;
        ApplyColor(model.BackgroundColor, value => widget.BackgroundColor = value);
        ApplyColor(model.BorderColor, value => widget.BorderColor = value);
        if (model.CornerRadius.HasValue) widget.CornerRadius = Math.Max(0, model.CornerRadius.Value);
        if (model.Padding.HasValue) widget.Padding = Math.Max(0, model.Padding.Value);
        ApplyColor(model.TitleColor, value => widget.TitleColor = value);
        widget.TitleFontSize = model.TitleFontSize;
        widget.TitleFontWeight = model.TitleFontWeight;
        if (TryParseDefinedEnumName<TextCaseTransform>(model.TitleTextCase, out var textCase)) widget.TitleTextCase = textCase;
        ApplyColor(model.TextColor, value => widget.TextColor = value);
        ApplyColor(model.MutedColor, value => widget.MutedColor = value);
        ApplyColor(model.Accent, value => widget.Accent = value);
        ApplyColor(model.SecondaryAccent, value => widget.SecondaryAccent = value);
        widget.FontFamilyName = string.IsNullOrWhiteSpace(model.FontFamilyName) ? null : model.FontFamilyName;
        if (model.Scale.HasValue) widget.Scale = Math.Max(1, Math.Min(4, model.Scale.Value));
        widget.StatusColors = MapStatusColors(model.StatusOk, model.StatusWarning, model.StatusCritical, model.StatusInfo, themeMode);
        return widget;
    }

    private static BgInfoMetricWidget MapMetric(BgInfoWidgetFile model, VisualThemeMode themeMode) {
        var metric = new BgInfoMetricWidget {
            Label = model.Label ?? string.Empty,
            Value = model.Value ?? string.Empty,
            Unit = model.Unit ?? string.Empty,
            Trend = model.Trend ?? string.Empty,
            Caption = model.Caption ?? string.Empty,
            Symbol = model.Symbol ?? string.Empty,
            NumericValue = model.NumericValue,
            Threshold = MapThreshold(model.Threshold, themeMode),
            History = model.History ?? Array.Empty<double>(),
            HistoryMaximum = model.HistoryMaximum
        };
        ApplyEnum<BgInfoStatus>(model.Status, value => metric.Status = value);
        ApplyEnum<BgInfoMetricHistoryKind>(model.HistoryKind, value => metric.HistoryKind = value);
        return metric;
    }

    private static BgInfoTableWidget MapTable(BgInfoWidgetFile model) {
        var table = new BgInfoTableWidget {
            StatusColumn = string.IsNullOrWhiteSpace(model.StatusColumn) ? null : model.StatusColumn,
            Dense = model.Dense ?? false,
            ShowHeader = model.ShowHeader ?? true,
            RowStriping = model.RowStriping ?? true
        };
        if (model.Columns != null) table.Columns.AddRange(model.Columns);
        if (model.Rows != null) foreach (var row in model.Rows) if (row != null) table.Rows.Add(row);
        return table;
    }

    private static BgInfoListWidget MapList(BgInfoWidgetFile model) {
        var list = new BgInfoListWidget { Dense = model.Dense ?? false };
        ApplyEnum<BgInfoListMarker>(model.Marker, value => list.Marker = value);
        foreach (var item in model.Items ?? new List<BgInfoListItemFile>()) {
            if (item == null) continue;
            var entry = new BgInfoListItem { Text = item.Text ?? string.Empty, Value = item.Value, Checked = item.Checked };
            ApplyEnum<BgInfoStatus>(item.Status, value => entry.Status = value);
            list.Items.Add(entry);
        }
        return list;
    }

    private static BgInfoCalendarHeatmapWidget MapCalendar(BgInfoWidgetFile model) {
        var calendar = new BgInfoCalendarHeatmapWidget { ShowLegend = model.ShowLegend ?? false };
        ApplyColor(model.Color, value => calendar.Color = value);
        ApplyEnum<DayOfWeek>(model.FirstDayOfWeek, value => calendar.FirstDayOfWeek = value);
        foreach (var item in model.Dates ?? new List<BgInfoDatedValueFile>()) {
            var date = ParseDate(item?.Date);
            if (date.HasValue) calendar.Items.Add(new BgInfoDatedValue { Date = date.Value, Value = item!.Value ?? 0 });
        }
        return calendar;
    }

    private static BgInfoTimelineWidget MapTimeline(BgInfoWidgetFile model) {
        var timeline = new BgInfoTimelineWidget { Now = ParseDate(model.Now), ShowLegend = model.ShowLegend ?? true };
        foreach (var item in model.TimelineItems ?? new List<BgInfoTimelineItemFile>()) {
            var start = ParseDate(item?.Start);
            if (item == null || !start.HasValue) continue;
            var entry = new BgInfoTimelineItem {
                Lane = item.Lane ?? string.Empty,
                Label = item.Label ?? string.Empty,
                Start = start.Value,
                End = ParseDate(item.End),
                Category = string.IsNullOrWhiteSpace(item.Category) ? "Scheduled" : item.Category!
            };
            if (entry.ValidationError() is { } error) throw new InvalidDataException(error);
            ApplyEnum<BgInfoStatus>(item.Status, value => entry.Status = value);
            ApplyColor(item.Color, value => entry.Color = value);
            timeline.Items.Add(entry);
        }
        return timeline;
    }

    private static BgInfoTreemapWidget MapTreemap(BgInfoWidgetFile model) {
        var treemap = new BgInfoTreemapWidget { ShowValues = model.ShowValues ?? true, ValueSuffix = model.ValueSuffix ?? string.Empty };
        foreach (var item in model.TreemapItems ?? new List<BgInfoTreemapItemFile>()) {
            if (item == null) continue;
            treemap.Items.Add(new BgInfoTreemapItem { Name = item.Name ?? string.Empty, Value = item.Value ?? 0, Group = item.Group });
        }
        return treemap;
    }

    private static BgInfoQrCodeWidget MapQrCode(BgInfoWidgetFile model) {
        var qr = new BgInfoQrCodeWidget { Text = model.Text ?? string.Empty, Caption = model.Caption ?? string.Empty };
        ApplyColor(model.ModuleColor, value => qr.ModuleColor = value);
        ApplyColor(model.FieldColor, value => qr.FieldColor = value);
        return qr;
    }

    private static BgInfoWidgetFile MapWidgetFile(BgInfoWidget widget) {
        string? Hex(ChartForgeX.Primitives.ChartColor? color) => color.HasValue ? BgInfoColorParser.ToHex(color.Value) : null;
        var model = new BgInfoWidgetFile {
            Kind = widget.Kind.ToString(),
            Id = string.IsNullOrWhiteSpace(widget.Id) ? null : widget.Id,
            Title = string.IsNullOrWhiteSpace(widget.Title) ? null : widget.Title,
            Note = string.IsNullOrWhiteSpace(widget.Note) ? null : widget.Note,
            Width = widget.Width,
            Height = widget.Height,
            Region = widget.Region == BgInfoRegion.None ? null : widget.Region.ToString(),
            RegionOrder = widget.RegionOrder == 0 ? null : widget.RegionOrder,
            Anchor = widget.Anchor.ToString(),
            OffsetX = widget.OffsetX,
            OffsetY = widget.OffsetY,
            PositionX = widget.PositionX,
            PositionY = widget.PositionY,
            ThemeMode = widget.ThemeMode.ToString(),
            BackgroundColor = Hex(widget.BackgroundColor),
            BorderColor = Hex(widget.BorderColor),
            CornerRadius = widget.CornerRadius > 0 ? widget.CornerRadius : null,
            Padding = widget.Padding,
            TitleColor = Hex(widget.TitleColor),
            TitleFontSize = widget.TitleFontSize,
            TitleFontWeight = widget.TitleFontWeight,
            TitleTextCase = widget.TitleTextCase?.ToString(),
            TextColor = Hex(widget.TextColor),
            MutedColor = Hex(widget.MutedColor),
            Accent = Hex(widget.Accent),
            SecondaryAccent = Hex(widget.SecondaryAccent),
            FontFamilyName = widget.FontFamilyName,
            Scale = widget.Scale == 1 ? null : widget.Scale,
            StatusOk = Hex(widget.StatusColors?.Ok),
            StatusWarning = Hex(widget.StatusColors?.Warning),
            StatusCritical = Hex(widget.StatusColors?.Critical),
            StatusInfo = Hex(widget.StatusColors?.Info)
        };

        switch (widget) {
            case BgInfoMetricWidget metric:
                model.Label = metric.Label;
                model.Value = metric.Value;
                model.Unit = NullIfEmpty(metric.Unit);
                model.Trend = NullIfEmpty(metric.Trend);
                model.Caption = NullIfEmpty(metric.Caption);
                model.Symbol = NullIfEmpty(metric.Symbol);
                model.Status = metric.Status == BgInfoStatus.None ? null : metric.Status.ToString();
                model.NumericValue = metric.NumericValue;
                model.Threshold = MapThresholdFile(metric.Threshold);
                model.History = metric.History.Count == 0 ? null : metric.History.ToArray();
                model.HistoryKind = metric.HistoryKind == BgInfoMetricHistoryKind.Sparkline ? null : metric.HistoryKind.ToString();
                model.HistoryMaximum = metric.HistoryMaximum;
                break;
            case BgInfoTableWidget table:
                model.Columns = table.Columns.ToArray();
                model.Rows = table.Rows.Select(row => row.ToArray()).ToList();
                model.StatusColumn = table.StatusColumn;
                model.Dense = table.Dense ? true : null;
                model.ShowHeader = table.ShowHeader ? null : false;
                model.RowStriping = table.RowStriping ? null : false;
                break;
            case BgInfoListWidget list:
                model.Marker = list.Marker == BgInfoListMarker.Auto ? null : list.Marker.ToString();
                model.Dense = list.Dense ? true : null;
                model.Items = list.Items.Select(item => new BgInfoListItemFile {
                    Text = item.Text, Value = item.Value, Checked = item.Checked,
                    Status = item.Status == BgInfoStatus.None ? null : item.Status.ToString()
                }).ToList();
                break;
            case BgInfoCalendarHeatmapWidget calendar:
                model.Color = Hex(calendar.Color);
                model.FirstDayOfWeek = calendar.FirstDayOfWeek == DayOfWeek.Monday ? null : calendar.FirstDayOfWeek.ToString();
                model.ShowLegend = calendar.ShowLegend ? true : null;
                model.Dates = calendar.Items.Select(item => new BgInfoDatedValueFile { Date = item.Date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture), Value = item.Value }).ToList();
                break;
            case BgInfoTimelineWidget timeline:
                model.Now = timeline.Now.HasValue ? FormatDate(timeline.Now.Value) : null;
                model.ShowLegend = timeline.ShowLegend ? null : false;
                model.TimelineItems = timeline.Items.Select(item => new BgInfoTimelineItemFile {
                    Lane = item.Lane, Label = NullIfEmpty(item.Label), Start = FormatDate(item.Start),
                    End = item.End.HasValue ? FormatDate(item.End.Value) : null, Category = item.Category,
                    Status = item.Status == BgInfoStatus.None ? null : item.Status.ToString(), Color = Hex(item.Color)
                }).ToList();
                break;
            case BgInfoTreemapWidget treemap:
                model.ShowValues = treemap.ShowValues ? null : false;
                model.ValueSuffix = NullIfEmpty(treemap.ValueSuffix);
                model.TreemapItems = treemap.Items.Select(item => new BgInfoTreemapItemFile { Name = item.Name, Value = item.Value, Group = item.Group }).ToList();
                break;
            case BgInfoMermaidWidget mermaid:
                model.Definition = mermaid.Definition;
                break;
            case BgInfoQrCodeWidget qr:
                model.Text = qr.Text;
                model.Caption = NullIfEmpty(qr.Caption);
                model.ModuleColor = Hex(qr.ModuleColor);
                model.FieldColor = Hex(qr.FieldColor);
                break;
        }
        return model;
    }

    private static string? NullIfEmpty(string? value) => string.IsNullOrWhiteSpace(value) ? null : value;

    private static string FormatDate(DateTime value) => value.ToString("O", CultureInfo.InvariantCulture);

    private static DateTime? ParseDate(string? text) {
        if (string.IsNullOrWhiteSpace(text)) return null;
        const DateTimeStyles styles = DateTimeStyles.AllowWhiteSpaces | DateTimeStyles.RoundtripKind;
        string[] invariantFormats = { "yyyy-MM-dd", "yyyy-MM-dd'T'HH:mm:ss", "yyyy-MM-dd'T'HH:mm:ss.FFFFFFFK" };
        if (DateTime.TryParseExact(text, invariantFormats, CultureInfo.InvariantCulture, styles, out var invariant)) return invariant;
        if (DateTime.TryParse(text, CultureInfo.CurrentCulture, styles, out var localized)) return localized;
        throw new InvalidDataException($"Invalid widget date '{text}'. Use an ISO date or a valid date in the current culture.");
    }

    internal sealed class BgInfoLayoutFile {
        public int? MarginX { get; set; }
        public int? MarginY { get; set; }
        public int? Gap { get; set; }
        public int? ColumnWidth { get; set; }
        public int? BandHeight { get; set; }
        public int? TaskbarReserve { get; set; }
        public int? IconAreaWidth { get; set; }
        public int? MaximumPanelHeight { get; set; }
        public string? ColumnAlignment { get; set; }
    }

    internal sealed class BgInfoThresholdFile {
        public double? Warning { get; set; }
        public double? Critical { get; set; }
        public string? Direction { get; set; }
        public string? Ok { get; set; }
        public string? WarningColor { get; set; }
        public string? CriticalColor { get; set; }
        public string? Info { get; set; }
    }

    internal sealed class BgInfoWidgetFile {
        public string? Kind { get; set; }
        public string? Id { get; set; }
        public string? Title { get; set; }
        public string? Note { get; set; }
        public int? Width { get; set; }
        public int? Height { get; set; }
        public string? Region { get; set; }
        public int? RegionOrder { get; set; }
        public string? Anchor { get; set; }
        public int? OffsetX { get; set; }
        public int? OffsetY { get; set; }
        public float? PositionX { get; set; }
        public float? PositionY { get; set; }
        public string? ThemeMode { get; set; }
        public string? BackgroundColor { get; set; }
        public string? BorderColor { get; set; }
        public double? CornerRadius { get; set; }
        public int? Padding { get; set; }
        public string? TitleColor { get; set; }
        public float? TitleFontSize { get; set; }
        public int? TitleFontWeight { get; set; }
        public string? TitleTextCase { get; set; }
        public string? TextColor { get; set; }
        public string? MutedColor { get; set; }
        public string? Accent { get; set; }
        public string? SecondaryAccent { get; set; }
        public string? FontFamilyName { get; set; }
        public double? Scale { get; set; }
        public string? StatusOk { get; set; }
        public string? StatusWarning { get; set; }
        public string? StatusCritical { get; set; }
        public string? StatusInfo { get; set; }
        public string? Label { get; set; }
        public string? Value { get; set; }
        public string? Unit { get; set; }
        public string? Trend { get; set; }
        public string? Caption { get; set; }
        public string? Symbol { get; set; }
        public string? Status { get; set; }
        public double? NumericValue { get; set; }
        public BgInfoThresholdFile? Threshold { get; set; }
        public double[]? History { get; set; }
        public string? HistoryKind { get; set; }
        public double? HistoryMaximum { get; set; }
        public string[]? Columns { get; set; }
        public List<string[]>? Rows { get; set; }
        public string? StatusColumn { get; set; }
        public bool? Dense { get; set; }
        public bool? ShowHeader { get; set; }
        public bool? RowStriping { get; set; }
        public string? Marker { get; set; }
        public List<BgInfoListItemFile>? Items { get; set; }
        public string? Color { get; set; }
        public string? FirstDayOfWeek { get; set; }
        public bool? ShowLegend { get; set; }
        public List<BgInfoDatedValueFile>? Dates { get; set; }
        public string? Now { get; set; }
        public List<BgInfoTimelineItemFile>? TimelineItems { get; set; }
        public bool? ShowValues { get; set; }
        public string? ValueSuffix { get; set; }
        public List<BgInfoTreemapItemFile>? TreemapItems { get; set; }
        public string? Definition { get; set; }
        public string? Text { get; set; }
        public string? ModuleColor { get; set; }
        public string? FieldColor { get; set; }
    }

    internal sealed class BgInfoListItemFile {
        public string? Text { get; set; }
        public string? Value { get; set; }
        public string? Status { get; set; }
        public bool? Checked { get; set; }
    }

    internal sealed class BgInfoDatedValueFile {
        public string? Date { get; set; }
        public double? Value { get; set; }
    }

    internal sealed class BgInfoTimelineItemFile {
        public string? Lane { get; set; }
        public string? Label { get; set; }
        public string? Start { get; set; }
        public string? End { get; set; }
        public string? Category { get; set; }
        public string? Status { get; set; }
        public string? Color { get; set; }
    }

    internal sealed class BgInfoTreemapItemFile {
        public string? Name { get; set; }
        public double? Value { get; set; }
        public string? Group { get; set; }
    }
}
