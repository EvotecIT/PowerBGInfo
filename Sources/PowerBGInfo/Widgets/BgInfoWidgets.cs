using System;
using System.Collections.Generic;
using System.Linq;
using Color = ChartForgeX.Primitives.ChartColor;

namespace PowerBGInfo;

/// <summary>History drawing for a metric widget.</summary>
public enum BgInfoMetricHistoryKind {
    /// <summary>A line with a soft fill.</summary>
    Sparkline,
    /// <summary>Short columns.</summary>
    Bars
}

/// <summary>One KPI rendered as a ChartForgeX metric card.</summary>
public sealed class BgInfoMetricWidget : BgInfoWidget {
    /// <summary>Creates a metric widget with a compact default size.</summary>
    public BgInfoMetricWidget() {
        Width = 320;
        Height = 150;
    }

    /// <inheritdoc />
    public override BgInfoWidgetKind Kind => BgInfoWidgetKind.Metric;
    /// <summary>Metric label, for example CPU.</summary>
    public string Label { get; set; } = string.Empty;
    /// <summary>Displayed value.</summary>
    public string Value { get; set; } = string.Empty;
    /// <summary>Optional unit shown after the value.</summary>
    public string Unit { get; set; } = string.Empty;
    /// <summary>Short change text, for example +4 %.</summary>
    public string Trend { get; set; } = string.Empty;
    /// <summary>Supporting text under the value.</summary>
    public string Caption { get; set; } = string.Empty;
    /// <summary>Short badge text, for example CPU or RAM.</summary>
    public string Symbol { get; set; } = string.Empty;
    /// <summary>Explicit status. Thresholds take over when <see cref="NumericValue"/> and thresholds are set.</summary>
    public BgInfoStatus Status { get; set; }
    /// <summary>Numeric value evaluated against <see cref="Threshold"/>.</summary>
    public double? NumericValue { get; set; }
    /// <summary>Optional warning and critical thresholds.</summary>
    public BgInfoThreshold? Threshold { get; set; }
    /// <summary>Recent values drawn beside the metric.</summary>
    public IReadOnlyList<double> History { get; set; } = Array.Empty<double>();
    /// <summary>How the history is drawn.</summary>
    public BgInfoMetricHistoryKind HistoryKind { get; set; }
    /// <summary>Optional history scale maximum.</summary>
    public double? HistoryMaximum { get; set; }

    /// <inheritdoc />
    public override void Validate() {
        base.Validate();
        if (History == null) throw new ArgumentNullException(nameof(History));
        foreach (var value in History) {
            if (double.IsNaN(value) || double.IsInfinity(value)) throw new ArgumentOutOfRangeException(nameof(History), "Metric history must contain finite values.");
        }
        if (HistoryKind == BgInfoMetricHistoryKind.Sparkline && History.Count == 1) {
            throw new ArgumentException("Metric sparkline history requires at least two values; use Bars for a single sample.", nameof(History));
        }
        if (HistoryMaximum.HasValue && (double.IsNaN(HistoryMaximum.Value) || double.IsInfinity(HistoryMaximum.Value))) {
            throw new ArgumentOutOfRangeException(nameof(HistoryMaximum), "Metric history maximum must be finite.");
        }
    }

    /// <summary>The status after thresholds are applied.</summary>
    public BgInfoStatus ResolveStatus() {
        if (Threshold is { IsDefined: true } threshold && NumericValue.HasValue) return threshold.Evaluate(NumericValue.Value);
        return Status;
    }
}

/// <summary>Rows and columns rendered as a ChartForgeX table.</summary>
public sealed class BgInfoTableWidget : BgInfoWidget {
    /// <summary>Creates a table widget.</summary>
    public BgInfoTableWidget() {
        Width = 520;
        Height = 260;
    }

    /// <inheritdoc />
    public override BgInfoWidgetKind Kind => BgInfoWidgetKind.Table;
    /// <summary>Column headers.</summary>
    public List<string> Columns { get; } = new();
    /// <summary>Cell text per row. Each row has one value per column.</summary>
    public List<string[]> Rows { get; } = new();
    /// <summary>Optional column whose values (OK, Warning, Failed...) get a status dot.</summary>
    public string? StatusColumn { get; set; }
    /// <summary>Use compact rows.</summary>
    public bool Dense { get; set; }
    /// <summary>Show the column header row.</summary>
    public bool ShowHeader { get; set; } = true;
    /// <summary>Shade every other row.</summary>
    public bool RowStriping { get; set; } = true;

    // ChartForgeX table rows are 31 px (24 dense) under a 32 px (26 dense) header.
    internal override double NaturalBodyHeight => (ShowHeader ? (Dense ? 30 : 36) : 0) + Rows.Count * (Dense ? 24d : 31d) + 16;

    /// <inheritdoc />
    public override void Validate() {
        base.Validate();
        if (Columns.Count == 0) throw new InvalidOperationException("A table widget needs at least one column.");
        for (var i = 0; i < Rows.Count; i++) {
            if (Rows[i] == null || Rows[i].Length != Columns.Count) throw new InvalidOperationException($"Table row {i + 1} must have {Columns.Count} values.");
        }
        if (!string.IsNullOrWhiteSpace(StatusColumn) && !Columns.Any(column => string.Equals(column, StatusColumn, StringComparison.OrdinalIgnoreCase))) {
            throw new InvalidOperationException($"Status column '{StatusColumn}' is not one of the table columns.");
        }
    }
}

/// <summary>Marker drawn before list items.</summary>
public enum BgInfoListMarker {
    /// <summary>Check for checklist items, Status for status items, otherwise Bullet.</summary>
    Auto,
    /// <summary>A small dot.</summary>
    Bullet,
    /// <summary>One-based numbers.</summary>
    Number,
    /// <summary>Check or empty boxes.</summary>
    Check,
    /// <summary>Status-colored dots.</summary>
    Status,
    /// <summary>No marker.</summary>
    None
}

/// <summary>One list row.</summary>
public sealed class BgInfoListItem {
    /// <summary>Item text.</summary>
    public string Text { get; set; } = string.Empty;
    /// <summary>Optional right-aligned value.</summary>
    public string? Value { get; set; }
    /// <summary>Optional status used by the status marker.</summary>
    public BgInfoStatus Status { get; set; }
    /// <summary>Optional checklist state.</summary>
    public bool? Checked { get; set; }
}

/// <summary>Bullets, numbers, checklist or status rows rendered as a ChartForgeX list.</summary>
public sealed class BgInfoListWidget : BgInfoWidget {
    /// <summary>Creates a list widget.</summary>
    public BgInfoListWidget() {
        Width = 420;
        Height = 240;
    }

    /// <inheritdoc />
    public override BgInfoWidgetKind Kind => BgInfoWidgetKind.List;
    /// <summary>List rows.</summary>
    public List<BgInfoListItem> Items { get; } = new();
    /// <summary>Marker style.</summary>
    public BgInfoListMarker Marker { get; set; }
    /// <summary>Use compact rows.</summary>
    public bool Dense { get; set; }

    // ChartForgeX list rows are 33 px (25 dense).
    internal override double NaturalBodyHeight => Items.Count * (Dense ? 25d : 33d) + 16;
}

/// <summary>One dated value.</summary>
public sealed class BgInfoDatedValue {
    /// <summary>Day of the value.</summary>
    public DateTime Date { get; set; }
    /// <summary>Value for that day. Duplicate days are summed.</summary>
    public double Value { get; set; }
}

/// <summary>Dated values on a week grid, rendered as a ChartForgeX calendar heatmap.</summary>
public sealed class BgInfoCalendarHeatmapWidget : BgInfoWidget {
    /// <summary>Creates a calendar widget.</summary>
    public BgInfoCalendarHeatmapWidget() {
        Width = 560;
        Height = 240;
    }

    /// <inheritdoc />
    public override BgInfoWidgetKind Kind => BgInfoWidgetKind.CalendarHeatmap;
    /// <summary>Dated values.</summary>
    public List<BgInfoDatedValue> Items { get; } = new();
    /// <summary>High-intensity cell color. Unset uses the accent.</summary>
    public Color? Color { get; set; }
    /// <summary>First day of the week rows.</summary>
    public DayOfWeek FirstDayOfWeek { get; set; } = DayOfWeek.Monday;
    /// <summary>Show the less-to-more color key.</summary>
    public bool ShowLegend { get; set; }

    /// <inheritdoc />
    public override void Validate() {
        base.Validate();
        if (Items.Count == 0) throw new InvalidOperationException("A calendar heatmap needs at least one dated value.");
        if (Items.Any(item => item.Value < 0 || double.IsNaN(item.Value) || double.IsInfinity(item.Value))) {
            throw new InvalidOperationException("Calendar heatmap values must be finite and not negative.");
        }
    }
}

/// <summary>One range on a timeline lane.</summary>
public sealed class BgInfoTimelineItem {
    /// <summary>Lane name, for example a server, a team or On call.</summary>
    public string Lane { get; set; } = string.Empty;
    /// <summary>Text drawn on the bar.</summary>
    public string Label { get; set; } = string.Empty;
    /// <summary>Range start.</summary>
    public DateTime Start { get; set; }
    /// <summary>Range end. Unset keeps the item open until now.</summary>
    public DateTime? End { get; set; }
    /// <summary>Legend category, for example Maintenance or Change freeze.</summary>
    public string Category { get; set; } = "Scheduled";
    /// <summary>Optional status that colors the category.</summary>
    public BgInfoStatus Status { get; set; }
    /// <summary>Optional explicit category color.</summary>
    public Color? Color { get; set; }

    internal string? ValidationError() {
        if (string.IsNullOrWhiteSpace(Lane)) return "Every timeline item needs a lane.";
        try {
            // Use the renderer owner's UTC, epoch and display-resolution contract unchanged.
            _ = new ChartForgeX.Core.ChartGanttLaneItem(Start, End, "range");
            return null;
        } catch (ArgumentException error) {
            return $"Timeline item '{Label}' has an invalid range: {error.Message}";
        }
    }
}

/// <summary>Time ranges on named lanes, rendered as ChartForgeX Gantt lanes.</summary>
public sealed class BgInfoTimelineWidget : BgInfoWidget {
    /// <summary>Creates a timeline widget.</summary>
    public BgInfoTimelineWidget() {
        Width = 640;
        Height = 260;
    }

    /// <inheritdoc />
    public override BgInfoWidgetKind Kind => BgInfoWidgetKind.Timeline;
    /// <summary>Timeline ranges.</summary>
    public List<BgInfoTimelineItem> Items { get; } = new();
    /// <summary>Instant of the Now marker. Unset hides it.</summary>
    public DateTime? Now { get; set; }
    /// <summary>Show the category legend.</summary>
    public bool ShowLegend { get; set; } = true;

    /// <inheritdoc />
    public override void Validate() {
        base.Validate();
        if (Items.Count == 0) throw new InvalidOperationException("A timeline needs at least one item.");
        foreach (var item in Items) {
            if (item.ValidationError() is { } error) throw new InvalidOperationException(error);
        }
    }
}

/// <summary>One treemap rectangle.</summary>
public sealed class BgInfoTreemapItem {
    /// <summary>Rectangle label.</summary>
    public string Name { get; set; } = string.Empty;
    /// <summary>Rectangle size.</summary>
    public double Value { get; set; }
    /// <summary>Optional group that nests the rectangle.</summary>
    public string? Group { get; set; }
}

/// <summary>Sizes as nested rectangles, rendered as a ChartForgeX treemap.</summary>
public sealed class BgInfoTreemapWidget : BgInfoWidget {
    /// <summary>Creates a treemap widget.</summary>
    public BgInfoTreemapWidget() {
        Width = 520;
        Height = 300;
    }

    /// <inheritdoc />
    public override BgInfoWidgetKind Kind => BgInfoWidgetKind.Treemap;
    /// <summary>Rectangles.</summary>
    public List<BgInfoTreemapItem> Items { get; } = new();
    /// <summary>Show values in the rectangles.</summary>
    public bool ShowValues { get; set; } = true;
    /// <summary>Optional suffix after values, for example GB.</summary>
    public string ValueSuffix { get; set; } = string.Empty;

    /// <inheritdoc />
    public override void Validate() {
        base.Validate();
        if (Items.Count == 0) throw new InvalidOperationException("A treemap needs at least one item.");
        if (Items.Any(item => item.Value < 0 || double.IsNaN(item.Value) || double.IsInfinity(item.Value))) throw new InvalidOperationException("Treemap values must be finite and not negative.");
    }
}

/// <summary>A diagram described in Mermaid text, rendered by ChartForgeX.Mermaid.</summary>
public sealed class BgInfoMermaidWidget : BgInfoWidget {
    /// <summary>Creates a Mermaid widget.</summary>
    public BgInfoMermaidWidget() {
        Width = 560;
        Height = 320;
    }

    /// <inheritdoc />
    public override BgInfoWidgetKind Kind => BgInfoWidgetKind.Mermaid;
    /// <summary>Mermaid source, for example a flowchart.</summary>
    public string Definition { get; set; } = string.Empty;

    /// <inheritdoc />
    public override void Validate() {
        base.Validate();
        if (string.IsNullOrWhiteSpace(Definition)) throw new InvalidOperationException("A Mermaid widget needs a definition.");
    }
}

/// <summary>A QR code, encoded by CodeGlyphX and drawn by ChartForgeX.</summary>
public sealed class BgInfoQrCodeWidget : BgInfoWidget {
    /// <summary>Creates a QR code widget.</summary>
    public BgInfoQrCodeWidget() {
        Width = 240;
        Height = 290;
    }

    /// <inheritdoc />
    public override BgInfoWidgetKind Kind => BgInfoWidgetKind.QrCode;
    /// <summary>Encoded text, usually a URL.</summary>
    public string Text { get; set; } = string.Empty;
    /// <summary>Optional text under the code, for example the short URL.</summary>
    public string Caption { get; set; } = string.Empty;
    /// <summary>Module color. Keep it dark: most scanners expect dark modules on a light field.</summary>
    public Color? ModuleColor { get; set; }
    /// <summary>Field and quiet-zone color.</summary>
    public Color? FieldColor { get; set; }

    /// <inheritdoc />
    public override void Validate() {
        base.Validate();
        if (string.IsNullOrWhiteSpace(Text)) throw new InvalidOperationException("A QR code needs text to encode.");
    }
}
