using PowerBGInfo;

namespace PowerBGInfo.PowerShell;

/// <summary>Sets the safe area and region sizes that -Region panels flow into.</summary>
/// <para>Emit it inside the New-BGInfo script block. Without it, regions use the theme margins or the defaults.</para>
/// <para>Regions: Top and Bottom are full-width bands that flow left to right. Left and Right are columns between the bands. Center fills the space between the columns. A panel without -Width or -Height shares the space left in its region.</para>
/// <example>
/// <code>
/// New-BGInfo -Theme Graphite -Target File {
///     New-BGInfoLayout -Margin 48 -ColumnWidth 480 -IconAreaWidth 120
///     New-BGInfoChart -Region Right -Title 'CPU' -Kind Area -Values 12, 30, 22
///     New-BGInfoChart -Region Right -Title 'Memory' -Kind Gauge -Values 64
/// } -ConfigurationDirectory $PSScriptRoot -OutputFileName 'regions.png'
/// </code>
/// </example>
[Cmdlet(VerbsCommon.New, "BGInfoLayout")]
[OutputType(typeof(BgInfoLayout))]
public sealed class CmdletNewBGInfoLayout : PSCmdlet {
    /// <para>Distance from every screen edge. -MarginX and -MarginY override one axis.</para>
    [Parameter]
    [ValidateRange(0, 4096)]
    public int Margin { get; set; }

    /// <para>Distance from the left and right edges.</para>
    [Parameter]
    [ValidateRange(0, 4096)]
    public int MarginX { get; set; }

    /// <para>Distance from the top and bottom edges.</para>
    [Parameter]
    [ValidateRange(0, 4096)]
    public int MarginY { get; set; }

    /// <para>Gap between panels and regions.</para>
    [Parameter]
    [ValidateRange(0, 1024)]
    public int Gap { get; set; }

    /// <para>Width of the Left and Right columns. Zero uses 24 % of the width, clamped to 340-620 pixels.</para>
    [Parameter]
    [ValidateRange(0, 16384)]
    public int ColumnWidth { get; set; }

    /// <para>Height of the Top and Bottom bands for panels without -Height. Zero uses 26 % of the height, clamped to 200-340 pixels.</para>
    [Parameter]
    [ValidateRange(0, 16384)]
    public int BandHeight { get; set; }

    /// <para>Height kept free at the bottom for the taskbar. Unset estimates it from the image height (48 px at 1080, 64 px at 1440). Use 0 when the taskbar auto-hides or sits elsewhere.</para>
    [Parameter]
    [ValidateRange(0, 4096)]
    public int TaskbarReserve { get; set; }

    /// <para>Width kept free on the left for desktop icons.</para>
    [Parameter]
    [ValidateRange(0, 4096)]
    public int IconAreaWidth { get; set; }

    /// <summary>Emits the layout.</summary>
    protected override void EndProcessing() {
        var layout = BgInfoThemeScope.Current?.CreateLayout() ?? new BgInfoLayout();
        if (IsBound(nameof(Margin))) {
            layout.MarginX = Margin;
            layout.MarginY = Margin;
        }
        if (IsBound(nameof(MarginX))) layout.MarginX = MarginX;
        if (IsBound(nameof(MarginY))) layout.MarginY = MarginY;
        if (IsBound(nameof(Gap))) layout.Gap = Gap;
        if (IsBound(nameof(ColumnWidth))) layout.ColumnWidth = ColumnWidth;
        if (IsBound(nameof(BandHeight))) layout.BandHeight = BandHeight;
        if (IsBound(nameof(TaskbarReserve))) layout.TaskbarReserve = TaskbarReserve;
        if (IsBound(nameof(IconAreaWidth))) layout.IconAreaWidth = IconAreaWidth;
        WriteObject(layout);
    }

    private bool IsBound(string name) => MyInvocation.BoundParameters.ContainsKey(name);
}
