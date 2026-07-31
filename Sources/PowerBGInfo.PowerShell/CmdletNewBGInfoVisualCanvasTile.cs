using System;
using System.Management.Automation;
using PowerBGInfo;

namespace PowerBGInfo.PowerShell;

/// <summary>Creates a BGInfo visual canvas tile definition.</summary>
/// <para>Tiles are the readable lane-based information boxes used by New-BGInfoVisualCanvas.</para>
/// <example>
/// <code>
/// New-BGInfoVisualCanvasTile -Side Left -IconKind Computer -SurfaceStyle Raised -Label HOSTNAME -Value '{{HostName}}' -Detail 'production desktop' -Accent '#0F766EFF'
/// </code>
/// </example>
/// <example>
/// <code>
/// New-BGInfoVisualCanvasTile -Side Right -IconKind Cpu -SurfaceStyle Raised -Label 'CPU LOAD' -Value '31% active' -MiniChartKind Area -MiniChartValues 22,28,25,36,31 -MiniChartMaximum 100
/// </code>
/// </example>
[Cmdlet(VerbsCommon.New, "BGInfoVisualCanvasTile")]
[OutputType(typeof(BgInfoVisualCanvasTile))]
public class CmdletNewBGInfoVisualCanvasTile : PSCmdlet {
    /// <para>Tile lane placement.</para>
    [Parameter]
    [Alias("Lane")]
    public BgInfoVisualCanvasSide Side { get; set; }

    /// <para>Compact tile icon or symbol.</para>
    [Parameter]
    public string Icon { get; set; } = string.Empty;

    /// <para>Tile label. Templates such as {{HostName}} are resolved at render time.</para>
    [Parameter(Mandatory = true)]
    public string Label { get; set; } = string.Empty;

    /// <para>Primary tile value. Templates such as {{HostName}} are resolved at render time.</para>
    [Parameter(Mandatory = true)]
    public string Value { get; set; } = string.Empty;

    /// <para>Optional detail text. Templates are resolved at render time.</para>
    [Parameter]
    public string Detail { get; set; } = string.Empty;

    /// <para>Optional tile width in pixels. Zero uses the visual canvas default or template default.</para>
    [Parameter]
    public int Width { get; set; }

    /// <para>Optional tile height in pixels. Zero uses the visual canvas default or template default.</para>
    [Parameter]
    public int Height { get; set; }

    /// <para>Optional accent color.</para>
    [Parameter]
    public object? Accent { get; set; }

    /// <para>Optional progress value from zero to one.</para>
    [Parameter]
    public double? Progress { get; set; }

    /// <para>Tile surface style.</para>
    [Parameter]
    public BgInfoVisualCanvasTileSurfaceStyle SurfaceStyle { get; set; }

    /// <para>Built-in icon to render instead of the Icon text.</para>
    [Parameter]
    public BgInfoVisualCanvasTileIconKind IconKind { get; set; }

    /// <para>Compact chart kind rendered inside the tile.</para>
    [Parameter]
    public BgInfoVisualCanvasTileMiniChartKind MiniChartKind { get; set; }

    /// <para>Tile-specific text fitting policy. Auto inherits the visual canvas setting.</para>
    [Parameter]
    public BgInfoVisualCanvasTileTextFitPolicy TextFitPolicy { get; set; }

    /// <para>Compact chart values rendered inside the tile.</para>
    [Parameter]
    public double[] MiniChartValues { get; set; } = Array.Empty<double>();

    /// <para>Optional compact chart maximum.</para>
    [Parameter]
    public double? MiniChartMaximum { get; set; }

    /// <summary>Emits a visual canvas tile definition.</summary>
    protected override void EndProcessing() {
        WriteObject(new BgInfoVisualCanvasTile {
            Side = Side,
            Icon = Icon,
            Label = Label,
            Value = Value,
            Detail = Detail,
            Width = Width,
            Height = Height,
            Accent = PowerShellColorConverter.ConvertOptional(Accent, nameof(Accent)),
            Progress = Progress,
            SurfaceStyle = SurfaceStyle,
            IconKind = IconKind,
            MiniChartKind = MiniChartKind,
            TextFitPolicy = TextFitPolicy,
            MiniChartValues = MiniChartValues ?? Array.Empty<double>(),
            MiniChartMaximum = MiniChartMaximum
        });
    }
}
