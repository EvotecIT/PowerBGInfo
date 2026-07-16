using System.Collections.Generic;
using Color = ChartForgeX.Primitives.ChartColor;

namespace PowerBGInfo;

/// <summary>Supported chart kinds for BGInfo rendering.</summary>
public enum BgInfoChartKind {
    /// <summary>Renders a minimal line sparkline.</summary>
    Sparkline,
    /// <summary>Renders a connected line chart.</summary>
    Line,
    /// <summary>Renders a filled area chart.</summary>
    Area,
    /// <summary>Renders a minimal bar chart.</summary>
    Bar,
    /// <summary>Renders horizontal bars.</summary>
    HorizontalBar,
    /// <summary>Renders the latest value as a gauge.</summary>
    Gauge,
    /// <summary>Renders the latest value as a circular status chart.</summary>
    Circle,
    /// <summary>Renders values as radial progress rings.</summary>
    RadialBar,
    /// <summary>Renders value, target, and qualitative ranges.</summary>
    Bullet,
    /// <summary>Renders values as a pie chart.</summary>
    Pie,
    /// <summary>Renders values as a donut chart.</summary>
    Donut,
    /// <summary>Renders values as progress bars.</summary>
    ProgressBar,
    /// <summary>Renders values as pictorial rows.</summary>
    Pictorial
}

/// <summary>Chart legend placement.</summary>
public enum BgInfoChartLegendPosition {
    /// <summary>Place the legend below the chart.</summary>
    Bottom,
    /// <summary>Place the legend above the chart.</summary>
    Top,
    /// <summary>Place the legend on the left.</summary>
    Left,
    /// <summary>Place the legend on the right.</summary>
    Right
}

/// <summary>Built-in pictorial chart symbol.</summary>
public enum BgInfoChartPictorialSymbol {
    /// <summary>Circle symbol.</summary>
    Circle,
    /// <summary>Square symbol.</summary>
    Square,
    /// <summary>Diamond symbol.</summary>
    Diamond,
    /// <summary>Triangle symbol.</summary>
    Triangle,
    /// <summary>Star symbol.</summary>
    Star,
    /// <summary>Person symbol.</summary>
    Person
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
    /// <summary>Optional labels used by point-based chart kinds.</summary>
    public IReadOnlyList<string> Labels { get; set; } = Array.Empty<string>();
    /// <summary>Optional target value used by bullet charts.</summary>
    public double? Target { get; set; }
    /// <summary>Optional qualitative range ends used by bullet charts.</summary>
    public IReadOnlyList<double> RangeEnds { get; set; } = Array.Empty<double>();
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
    /// <summary>Optional palette for point-based charts.</summary>
    public IReadOnlyList<Color> Palette { get; set; } = Array.Empty<Color>();
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
    /// <summary>Whether to show the chart legend.</summary>
    public bool ShowLegend { get; set; }
    /// <summary>Whether point-based charts should use point legends.</summary>
    public bool ShowPointLegend { get; set; }
    /// <summary>Chart legend position.</summary>
    public BgInfoChartLegendPosition LegendPosition { get; set; } = BgInfoChartLegendPosition.Bottom;
    /// <summary>Whether to show supported data labels.</summary>
    public bool ShowDataLabels { get; set; }
    /// <summary>Optional minimum scale value.</summary>
    public double? Minimum { get; set; }
    /// <summary>Optional maximum scale value.</summary>
    public double? Maximum { get; set; }
    /// <summary>Whether donut center label is visible.</summary>
    public bool ShowDonutCenterLabel { get; set; } = true;
    /// <summary>Donut inner radius ratio.</summary>
    public double? DonutInnerRadiusRatio { get; set; }
    /// <summary>Optional donut center value text.</summary>
    public string? DonutCenterValue { get; set; }
    /// <summary>Optional donut center label text.</summary>
    public string? DonutCenterLabel { get; set; }
    /// <summary>Whether radial-bar center label is visible.</summary>
    public bool ShowRadialBarCenterLabel { get; set; } = true;
    /// <summary>Whether circle status label is visible.</summary>
    public bool ShowCircleStatusLabel { get; set; } = true;
    /// <summary>Whether progress values are visible.</summary>
    public bool ShowProgressValues { get; set; } = true;
    /// <summary>Whether progress handles are visible.</summary>
    public bool ShowProgressHandles { get; set; } = true;
    /// <summary>Progress-bar thickness ratio.</summary>
    public double? ProgressBarThicknessRatio { get; set; }
    /// <summary>Pictorial chart symbol.</summary>
    public BgInfoChartPictorialSymbol PictorialSymbol { get; set; } = BgInfoChartPictorialSymbol.Circle;
    /// <summary>Pictorial symbols per row.</summary>
    public int? PictorialColumns { get; set; }
}
