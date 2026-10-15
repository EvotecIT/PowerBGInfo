using PowerBGInfo;

namespace PowerBGInfo.PowerShell;

/// <summary>Creates a soft radial light for New-BGInfoBackground.</summary>
/// <para>Positions are fractions of the backdrop, so the same glow works at any resolution.</para>
/// <example>
/// <code>
/// New-BGInfoBackgroundGlow -X 0.85 -Y 0.9 -Radius 0.6 -Color '#06B6D455'
/// </code>
/// </example>
[Cmdlet(VerbsCommon.New, "BGInfoBackgroundGlow")]
[OutputType(typeof(BgInfoBackgroundGlow))]
public sealed class CmdletNewBGInfoBackgroundGlow : PSCmdlet {
    /// <para>Horizontal center from 0 (left edge) to 1 (right edge).</para>
    [Parameter]
    [ValidateRange(-1d, 2d)]
    public double X { get; set; } = 0.5d;

    /// <para>Vertical center from 0 (top edge) to 1 (bottom edge).</para>
    [Parameter]
    [ValidateRange(-1d, 2d)]
    public double Y { get; set; } = 0.5d;

    /// <para>Radius as a fraction of the longest backdrop side.</para>
    [Parameter]
    [ValidateRange(0.01d, 4d)]
    public double Radius { get; set; } = 0.5d;

    /// <para>Center color. The glow fades to transparent at the radius, so use alpha to set its strength.</para>
    [Parameter(Mandatory = true)]
    public object Color { get; set; } = null!;

    /// <summary>Emits the glow definition.</summary>
    protected override void EndProcessing() {
        WriteObject(new BgInfoBackgroundGlow {
            X = X,
            Y = Y,
            Radius = Radius,
            Color = PowerShellColorConverter.ConvertRequired(Color, nameof(Color))
        });
    }
}
