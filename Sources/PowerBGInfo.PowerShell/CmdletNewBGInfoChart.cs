using System.Drawing;
using System.Management.Automation;
using PowerBGInfo;

namespace PowerBGInfo.PowerShell;

/// <summary>Creates a BGInfo chart definition.</summary>
[Cmdlet(VerbsCommon.New, "BGInfoChart", DefaultParameterSetName = "Single")]
[OutputType(typeof(BgInfoChart))]
public class CmdletNewBGInfoChart : PSCmdlet {
    /// <para>Chart title displayed above the plot.</para>
    [Parameter]
    public string Title { get; set; } = string.Empty;

    /// <para>Chart identifier used for history storage.</para>
    [Parameter]
    public string Id { get; set; } = string.Empty;

    /// <para>Chart kind (Sparkline or Bar).</para>
    [Parameter]
    public BgInfoChartKind Kind { get; set; } = BgInfoChartKind.Sparkline;

    /// <para>Single value to append.</para>
    [Parameter(ParameterSetName = "Single")]
    public double Value { get; set; }

    /// <para>Multiple values to append or replace.</para>
    [Parameter(ParameterSetName = "Multiple")]
    public double[] Values { get; set; } = Array.Empty<double>();

    /// <para>Built-in metric source used when no explicit values are provided.</para>
    [Parameter]
    public BgInfoChartMetric Metric { get; set; } = BgInfoChartMetric.None;

    /// <para>Optional metric argument (for example drive letter).</para>
    [Parameter]
    public string MetricArgument { get; set; } = string.Empty;

    /// <para>Chart width in pixels.</para>
    [Parameter]
    public int Width { get; set; } = 240;

    /// <para>Chart height in pixels.</para>
    [Parameter]
    public int Height { get; set; } = 90;

    /// <para>Anchor position for placement.</para>
    [Parameter]
    public BgInfoTextPosition Anchor { get; set; } = BgInfoTextPosition.BottomLeft;

    /// <para>Horizontal offset from the anchor.</para>
    [Parameter]
    public int OffsetX { get; set; } = 10;

    /// <para>Vertical offset from the anchor.</para>
    [Parameter]
    public int OffsetY { get; set; } = 10;

    /// <para>Absolute X position for placement.</para>
    [Parameter]
    public int PositionX { get; set; }

    /// <para>Absolute Y position for placement.</para>
    [Parameter]
    public int PositionY { get; set; }

    /// <para>Maximum number of samples to keep in history.</para>
    [Parameter]
    public int MaxPoints { get; set; } = 60;

    /// <para>Disable history storage and render only provided values.</para>
    [Parameter]
    public SwitchParameter NoHistory { get; set; }

    /// <para>Replace history instead of appending values.</para>
    [Parameter]
    public SwitchParameter ReplaceHistory { get; set; }

    /// <para>Line or bar color.</para>
    [Parameter]
    public Color LineColor { get; set; }

    /// <para>Fill color for sparklines.</para>
    [Parameter]
    public Color FillColor { get; set; }

    /// <para>Background color for the chart block.</para>
    [Parameter]
    public Color BackgroundColor { get; set; }

    /// <para>Text color for title/value.</para>
    [Parameter]
    public Color TextColor { get; set; }

    /// <para>Font family for title and value.</para>
    [Parameter]
    public string FontFamilyName { get; set; } = string.Empty;

    /// <para>Title font size.</para>
    [Parameter]
    public float TitleFontSize { get; set; }

    /// <para>Value font size.</para>
    [Parameter]
    public float ValueFontSize { get; set; }

    /// <para>Show the latest value text.</para>
    [Parameter]
    public SwitchParameter ShowLatestValue { get; set; }

    /// <para>Format string for the latest value.</para>
    [Parameter]
    public string ValueFormat { get; set; } = "0.##";

    /// <para>Suffix appended to the latest value.</para>
    [Parameter]
    public string ValueSuffix { get; set; } = string.Empty;

    /// <para>Gap between bars (0-1).</para>
    [Parameter]
    public float BarGap { get; set; } = 0.2f;

    /// <para>Padding inside the chart.</para>
    [Parameter]
    public int Padding { get; set; } = 6;

    /// <para>Show chart grid lines.</para>
    [Parameter]
    public SwitchParameter ShowGrid { get; set; }

    /// <para>Grid line color.</para>
    [Parameter]
    public Color GridColor { get; set; }

    /// <para>Number of horizontal grid lines.</para>
    [Parameter]
    public int GridLineCount { get; set; } = 4;

    /// <summary>Emits a BGInfo chart definition.</summary>
    protected override void EndProcessing() {
        var chart = new BgInfoChart {
            Title = Title,
            Id = Id,
            Kind = Kind,
            Width = Width,
            Height = Height,
            Anchor = Anchor,
            OffsetX = OffsetX,
            OffsetY = OffsetY,
            MaxPoints = MaxPoints,
            UseHistory = !NoHistory.IsPresent,
            AppendValues = !ReplaceHistory.IsPresent,
            ShowLatestValue = !MyInvocation.BoundParameters.ContainsKey(nameof(ShowLatestValue)) || ShowLatestValue.IsPresent,
            ValueFormat = ValueFormat,
            ValueSuffix = ValueSuffix,
            BarGap = BarGap,
            Padding = Padding,
            ShowGrid = ShowGrid.IsPresent,
            GridLineCount = GridLineCount
        };

        if (MyInvocation.BoundParameters.ContainsKey(nameof(PositionX)) &&
            MyInvocation.BoundParameters.ContainsKey(nameof(PositionY))) {
            chart.PositionX = PositionX;
            chart.PositionY = PositionY;
        }

        if (MyInvocation.BoundParameters.ContainsKey(nameof(LineColor))) {
            chart.LineColor = LineColor;
        }
        if (MyInvocation.BoundParameters.ContainsKey(nameof(FillColor))) {
            chart.FillColor = FillColor;
        }
        if (MyInvocation.BoundParameters.ContainsKey(nameof(BackgroundColor))) {
            chart.BackgroundColor = BackgroundColor;
        }
        if (MyInvocation.BoundParameters.ContainsKey(nameof(TextColor))) {
            chart.TextColor = TextColor;
        }
        if (MyInvocation.BoundParameters.ContainsKey(nameof(GridColor))) {
            chart.GridColor = GridColor;
        }
        if (!string.IsNullOrWhiteSpace(FontFamilyName)) {
            chart.FontFamilyName = FontFamilyName;
        }
        if (MyInvocation.BoundParameters.ContainsKey(nameof(TitleFontSize))) {
            chart.TitleFontSize = TitleFontSize;
        }
        if (MyInvocation.BoundParameters.ContainsKey(nameof(ValueFontSize))) {
            chart.ValueFontSize = ValueFontSize;
        }
        if (MyInvocation.BoundParameters.ContainsKey(nameof(ShowGrid))) {
            chart.ShowGrid = ShowGrid.IsPresent;
        }
        if (MyInvocation.BoundParameters.ContainsKey(nameof(GridLineCount))) {
            chart.GridLineCount = GridLineCount;
        }

        if (MyInvocation.BoundParameters.ContainsKey(nameof(Values))) {
            chart.Values = Values ?? Array.Empty<double>();
        } else if (MyInvocation.BoundParameters.ContainsKey(nameof(Value))) {
            chart.Values = new[] { Value };
        } else {
            chart.Values = Array.Empty<double>();
        }

        chart.Metric = Metric;
        if (!string.IsNullOrWhiteSpace(MetricArgument)) {
            chart.MetricArgument = MetricArgument;
        }

        WriteObject(chart);
    }
}
