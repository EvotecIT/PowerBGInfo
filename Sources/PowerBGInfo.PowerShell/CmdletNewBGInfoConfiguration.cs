using System.Drawing;
using System.Management.Automation;

namespace PowerBGInfo.PowerShell;

/// <summary>Creates a BGInfo configuration object.</summary>
/// <para>Use this to build reusable configurations that can be exported to JSON.</para>
[Cmdlet(VerbsCommon.New, "BGInfoConfiguration")]
[OutputType(typeof(BgInfoConfiguration))]
public sealed class CmdletNewBGInfoConfiguration : PSCmdlet {
    /// <para>Optional base wallpaper file path.</para>
    [Parameter]
    public string FilePath { get; set; } = string.Empty;

    /// <para>Optional output file name for the generated image.</para>
    [Parameter]
    public string OutputFileName { get; set; } = string.Empty;

    /// <para>Output directory for generated BGInfo images.</para>
    [Parameter]
    public string ConfigurationDirectory { get; set; } = string.Empty;

    /// <para>Default label font family.</para>
    [Parameter]
    public string FontFamilyName { get; set; } = "Calibri";

    /// <para>Default label color.</para>
    [Parameter]
    public Color Color { get; set; }

    /// <para>Background color to use when no wallpaper image is available.</para>
    [Parameter]
    public Color? BackgroundColor { get; set; }

    /// <para>Default label font size.</para>
    [Parameter]
    public float FontSize { get; set; } = 16;

    /// <para>Default value color.</para>
    [Parameter]
    public Color ValueColor { get; set; }

    /// <para>Default value font size.</para>
    [Parameter]
    public float ValueFontSize { get; set; } = 16;

    /// <para>Default value font family.</para>
    [Parameter]
    public string ValueFontFamilyName { get; set; } = "Calibri";

    /// <para>Maximum width used when wrapping value text. Set to 0 to disable wrapping.</para>
    [Parameter]
    public int ValueWrapWidth { get; set; }

    /// <para>Vertical spacing between rows.</para>
    [Parameter]
    public int SpaceBetweenLines { get; set; } = 10;

    /// <para>Spacing between label and value columns.</para>
    [Parameter]
    public int SpaceBetweenColumns { get; set; } = 30;

    /// <para>Legacy position X placeholder.</para>
    [Parameter]
    public float PositionX { get; set; } = 10;

    /// <para>Legacy position Y placeholder.</para>
    [Parameter]
    public float PositionY { get; set; } = 10;

    /// <para>Monitor index to target for wallpaper operations.</para>
    [Parameter]
    public int MonitorIndex { get; set; }

    /// <para>X padding used for layout positioning.</para>
    [Parameter]
    public int SpaceX { get; set; } = 10;

    /// <para>Y padding used for layout positioning.</para>
    [Parameter]
    public int SpaceY { get; set; } = 10;

    /// <para>Wallpaper fit mode used after generation.</para>
    [Parameter]
    public DesktopManager.DesktopWallpaperPosition WallpaperFit { get; set; } = DesktopManager.DesktopWallpaperPosition.Center;

    /// <para>Layout anchor position.</para>
    [Parameter]
    public BgInfoTextPosition TextPosition { get; set; } = BgInfoTextPosition.TopLeft;

    /// <para>Output target (Wallpaper, File, LogonScreen, or Both).</para>
    [Parameter]
    public BgInfoTarget Target { get; set; } = BgInfoTarget.Wallpaper;

    /// <para>Chart layout mode.</para>
    [Parameter]
    public BgInfoChartLayoutMode ChartLayout { get; set; } = BgInfoChartLayoutMode.Manual;

    /// <para>Anchor used when stacking charts.</para>
    [Parameter]
    public BgInfoTextPosition ChartStackAnchor { get; set; } = BgInfoTextPosition.BottomLeft;

    /// <para>Direction used when stacking charts.</para>
    [Parameter]
    public BgInfoChartStackDirection ChartStackDirection { get; set; } = BgInfoChartStackDirection.Vertical;

    /// <para>Spacing between stacked charts.</para>
    [Parameter]
    public int ChartStackSpacing { get; set; } = 12;

    /// <para>Horizontal offset for stacked charts.</para>
    [Parameter]
    public int ChartStackOffsetX { get; set; } = 10;

    /// <para>Vertical offset for stacked charts.</para>
    [Parameter]
    public int ChartStackOffsetY { get; set; } = 10;

    /// <para>Align stacked charts to the text block.</para>
    [Parameter]
    public SwitchParameter ChartStackAlignToTextBlock { get; set; }

    /// <para>Place stacked charts outside of the text block.</para>
    [Parameter]
    public SwitchParameter ChartStackOutsideTextBlock { get; set; }

    /// <para>Apply wallpaper for all user profiles.</para>
    [Parameter]
    public SwitchParameter AllUsers { get; set; }

    /// <para>Exclude the default user profile when applying to all users.</para>
    [Parameter]
    public SwitchParameter ExcludeDefaultUserProfile { get; set; }

    /// <para>Disable wallpaper refresh (keep old behavior).</para>
    [Parameter]
    public SwitchParameter DisableWallpaperRefresh { get; set; }

    /// <para>Use screen coordinates for layout positioning.</para>
    [Parameter]
    public SwitchParameter UseScreenCoordinates { get; set; }

    /// <para>Entries to include in the configuration.</para>
    [Parameter]
    public BgInfoEntry[] Entries { get; set; } = System.Array.Empty<BgInfoEntry>();

    /// <para>Variables to include in the configuration.</para>
    [Parameter]
    public BgInfoVariable[] Variables { get; set; } = System.Array.Empty<BgInfoVariable>();

    /// <para>Charts to include in the configuration.</para>
    [Parameter]
    public BgInfoChart[] Charts { get; set; } = System.Array.Empty<BgInfoChart>();

    /// <summary>Creates the configuration object.</summary>
    protected override void EndProcessing() {
        var config = new BgInfoConfiguration();

        if (IsParameterBound(nameof(FilePath))) config.FilePath = FilePath;
        if (IsParameterBound(nameof(OutputFileName))) config.OutputFileName = OutputFileName;
        if (IsParameterBound(nameof(ConfigurationDirectory))) config.ConfigurationDirectory = ConfigurationDirectory;
        if (IsParameterBound(nameof(FontFamilyName))) config.FontFamilyName = FontFamilyName;
        if (IsParameterBound(nameof(Color))) config.Color = Color;
        if (IsParameterBound(nameof(BackgroundColor))) config.BackgroundColor = BackgroundColor;
        if (IsParameterBound(nameof(FontSize))) config.FontSize = FontSize;
        if (IsParameterBound(nameof(ValueColor))) config.ValueColor = ValueColor;
        if (IsParameterBound(nameof(ValueFontSize))) config.ValueFontSize = ValueFontSize;
        if (IsParameterBound(nameof(ValueFontFamilyName))) config.ValueFontFamilyName = ValueFontFamilyName;
        if (IsParameterBound(nameof(ValueWrapWidth))) config.ValueWrapWidth = ValueWrapWidth;
        if (IsParameterBound(nameof(SpaceBetweenLines))) config.SpaceBetweenLines = SpaceBetweenLines;
        if (IsParameterBound(nameof(SpaceBetweenColumns))) config.SpaceBetweenColumns = SpaceBetweenColumns;
        if (IsParameterBound(nameof(PositionX))) config.PositionX = PositionX;
        if (IsParameterBound(nameof(PositionY))) config.PositionY = PositionY;
        if (IsParameterBound(nameof(MonitorIndex))) config.MonitorIndex = MonitorIndex;
        if (IsParameterBound(nameof(SpaceX))) config.SpaceX = SpaceX;
        if (IsParameterBound(nameof(SpaceY))) config.SpaceY = SpaceY;
        if (IsParameterBound(nameof(WallpaperFit))) config.WallpaperFit = WallpaperFit;
        if (IsParameterBound(nameof(TextPosition))) config.TextPosition = TextPosition;
        if (IsParameterBound(nameof(Target))) config.Target = Target;
        if (IsParameterBound(nameof(ChartLayout))) config.ChartLayout = ChartLayout;
        if (IsParameterBound(nameof(ChartStackAnchor))) config.ChartStackAnchor = ChartStackAnchor;
        if (IsParameterBound(nameof(ChartStackDirection))) config.ChartStackDirection = ChartStackDirection;
        if (IsParameterBound(nameof(ChartStackSpacing))) config.ChartStackSpacing = ChartStackSpacing;
        if (IsParameterBound(nameof(ChartStackOffsetX))) config.ChartStackOffsetX = ChartStackOffsetX;
        if (IsParameterBound(nameof(ChartStackOffsetY))) config.ChartStackOffsetY = ChartStackOffsetY;
        if (IsParameterBound(nameof(ChartStackAlignToTextBlock))) config.ChartStackAlignToTextBlock = ChartStackAlignToTextBlock.IsPresent;
        if (IsParameterBound(nameof(ChartStackOutsideTextBlock))) config.ChartStackOutsideTextBlock = ChartStackOutsideTextBlock.IsPresent;
        if (IsParameterBound(nameof(DisableWallpaperRefresh))) config.ForceWallpaperRefresh = !DisableWallpaperRefresh.IsPresent;
        if (IsParameterBound(nameof(AllUsers))) config.ApplyToAllUsers = AllUsers.IsPresent;
        if (IsParameterBound(nameof(ExcludeDefaultUserProfile))) config.IncludeDefaultUserProfile = !ExcludeDefaultUserProfile.IsPresent;
        if (IsParameterBound(nameof(UseScreenCoordinates))) config.UseScreenCoordinates = UseScreenCoordinates.IsPresent;

        if (IsParameterBound(nameof(Entries)) && Entries.Length > 0) {
            config.Entries.AddRange(Entries);
        }
        if (IsParameterBound(nameof(Variables)) && Variables.Length > 0) {
            config.Variables.AddRange(Variables);
        }
        if (IsParameterBound(nameof(Charts)) && Charts.Length > 0) {
            config.Charts.AddRange(Charts);
        }

        WriteObject(config);
    }

    private bool IsParameterBound(string name) {
        return MyInvocation.BoundParameters.ContainsKey(name);
    }
}
