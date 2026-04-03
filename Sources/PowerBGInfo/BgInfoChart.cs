using System.Collections.Generic;
using System.Drawing;

namespace PowerBGInfo;

/// <summary>Supported chart kinds for BGInfo rendering.</summary>
public enum BgInfoChartKind {
    /// <summary>Renders a minimal line sparkline.</summary>
    Sparkline,
    /// <summary>Renders a minimal bar chart.</summary>
    Bar
}

/// <summary>Chart layout strategy.</summary>
public enum BgInfoChartLayoutMode {
    /// <summary>Use per-chart positions.</summary>
    Manual,
    /// <summary>Stack charts from a shared anchor.</summary>
    Stack
}

/// <summary>Direction for stacked charts.</summary>
public enum BgInfoChartStackDirection {
    /// <summary>Stack charts vertically.</summary>
    Vertical,
    /// <summary>Stack charts horizontally.</summary>
    Horizontal
}

/// <summary>Supported chart metric sources.</summary>
public enum BgInfoChartMetric {
    /// <summary>No metric source; use explicit values.</summary>
    None,
    /// <summary>Total CPU usage percent.</summary>
    CpuPercent,
    /// <summary>Memory usage percent.</summary>
    MemoryPercent,
    /// <summary>Disk free space percent.</summary>
    DiskFreePercent,
    /// <summary>Disk used space percent.</summary>
    DiskUsedPercent,
    /// <summary>Disk free space in gigabytes.</summary>
    DiskFreeGb,
    /// <summary>System uptime in hours.</summary>
    UptimeHours,
    /// <summary>System uptime in days.</summary>
    UptimeDays
}

/// <summary>Defines a chart block rendered onto the BGInfo output.</summary>
public sealed class BgInfoChart {
    /// <summary>Chart identifier used for history storage.</summary>
    public string Id { get; set; } = string.Empty;
    /// <summary>Title displayed above the chart.</summary>
    public string Title { get; set; } = string.Empty;
    /// <summary>Chart kind.</summary>
    public BgInfoChartKind Kind { get; set; } = BgInfoChartKind.Sparkline;
    /// <summary>Chart width in pixels.</summary>
    public int Width { get; set; } = 240;
    /// <summary>Chart height in pixels.</summary>
    public int Height { get; set; } = 90;
    /// <summary>Anchor position used for placement.</summary>
    public BgInfoTextPosition Anchor { get; set; } = BgInfoTextPosition.BottomLeft;
    /// <summary>Horizontal offset from the anchor.</summary>
    public int OffsetX { get; set; } = 10;
    /// <summary>Vertical offset from the anchor.</summary>
    public int OffsetY { get; set; } = 10;
    /// <summary>Explicit X position override.</summary>
    public float? PositionX { get; set; }
    /// <summary>Explicit Y position override.</summary>
    public float? PositionY { get; set; }
    /// <summary>Values to plot for this run.</summary>
    public IReadOnlyList<double> Values { get; set; } = Array.Empty<double>();
    /// <summary>Metric source used when no explicit values are provided.</summary>
    public BgInfoChartMetric Metric { get; set; } = BgInfoChartMetric.None;
    /// <summary>Optional argument for the metric source (for example drive letter).</summary>
    public string? MetricArgument { get; set; }
    /// <summary>Maximum number of samples to keep in history.</summary>
    public int MaxPoints { get; set; } = 60;
    /// <summary>Whether to use and update history.</summary>
    public bool UseHistory { get; set; } = true;
    /// <summary>Whether to append to history (true) or replace it.</summary>
    public bool AppendValues { get; set; } = true;
    /// <summary>Optional background color override.</summary>
    public Color? BackgroundColor { get; set; }
    /// <summary>Line or bar color.</summary>
    public Color? LineColor { get; set; }
    /// <summary>Optional fill color for sparklines.</summary>
    public Color? FillColor { get; set; }
    /// <summary>Text color for title and value.</summary>
    public Color? TextColor { get; set; }
    /// <summary>Font family for title and value.</summary>
    public string? FontFamilyName { get; set; }
    /// <summary>Title font size.</summary>
    public float? TitleFontSize { get; set; }
    /// <summary>Value font size.</summary>
    public float? ValueFontSize { get; set; }
    /// <summary>Whether to show the latest value.</summary>
    public bool ShowLatestValue { get; set; } = true;
    /// <summary>Format string used for latest value.</summary>
    public string ValueFormat { get; set; } = "0.##";
    /// <summary>Optional suffix appended to the value.</summary>
    public string ValueSuffix { get; set; } = string.Empty;
    /// <summary>Gap between bars (0-1).</summary>
    public float BarGap { get; set; } = 0.2f;
    /// <summary>Padding inside the chart.</summary>
    public int Padding { get; set; } = 6;
    /// <summary>Whether to show grid lines.</summary>
    public bool ShowGrid { get; set; }
    /// <summary>Grid line color.</summary>
    public Color? GridColor { get; set; }
    /// <summary>Number of horizontal grid lines.</summary>
    public int GridLineCount { get; set; } = 4;
}
