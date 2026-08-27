using Color = ChartForgeX.Primitives.ChartColor;
using ChartForgeX.Typography;
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
    public object? Color { get; set; }

    /// <para>Background color to use when no wallpaper image is available.</para>
    [Parameter]
    public object? BackgroundColor { get; set; }

    /// <para>Default label font size.</para>
    [Parameter]
    public float FontSize { get; set; } = 16;

    /// <para>Render labels with a bold font weight by default.</para>
    [Parameter]
    public SwitchParameter Bold { get; set; }

    /// <para>Default numeric label font weight from 100 through 900.</para>
    [Parameter]
    [ValidateRange(100, 900)]
    public int FontWeight { get; set; } = 400;

    /// <para>Render labels with italic text by default.</para>
    [Parameter]
    public SwitchParameter Italic { get; set; }

    /// <para>Underline labels by default.</para>
    [Parameter]
    public SwitchParameter Underline { get; set; }

    /// <para>Default label underline pattern.</para>
    [Parameter]
    public TextDecorationStyle UnderlineStyle { get; set; }

    /// <para>Default label strikethrough pattern.</para>
    [Parameter]
    public TextDecorationStyle StrikethroughStyle { get; set; }

    /// <para>Default label subscript or superscript placement.</para>
    [Parameter]
    public TextBaseline Baseline { get; set; }

    /// <para>Default display-time label casing transform.</para>
    [Parameter]
    public TextCaseTransform TextCase { get; set; }

    /// <para>Default value color.</para>
    [Parameter]
    public object? ValueColor { get; set; }

    /// <para>Default value font size.</para>
    [Parameter]
    public float ValueFontSize { get; set; } = 16;

    /// <para>Default value font family.</para>
    [Parameter]
    public string ValueFontFamilyName { get; set; } = "Calibri";

    /// <para>Render values with a bold font weight by default.</para>
    [Parameter]
    public SwitchParameter ValueBold { get; set; }

    /// <para>Default numeric value font weight from 100 through 900.</para>
    [Parameter]
    [ValidateRange(100, 900)]
    public int ValueFontWeight { get; set; } = 400;

    /// <para>Render values with italic text by default.</para>
    [Parameter]
    public SwitchParameter ValueItalic { get; set; }

    /// <para>Underline values by default.</para>
    [Parameter]
    public SwitchParameter ValueUnderline { get; set; }

    /// <para>Default value underline pattern.</para>
    [Parameter]
    public TextDecorationStyle ValueUnderlineStyle { get; set; }

    /// <para>Default value strikethrough pattern.</para>
    [Parameter]
    public TextDecorationStyle ValueStrikethroughStyle { get; set; }

    /// <para>Default value subscript or superscript placement.</para>
    [Parameter]
    public TextBaseline ValueBaseline { get; set; }

    /// <para>Default display-time value casing transform.</para>
    [Parameter]
    public TextCaseTransform ValueTextCase { get; set; }

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

    /// <para>Disable automatic preservation of the current Windows wallpaper slideshow.</para>
    [Parameter]
    public SwitchParameter DisableWallpaperSlideshow { get; set; }

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

    /// <para>Topology diagrams to include in the configuration.</para>
    [Parameter]
    public BgInfoTopology[] Topologies { get; set; } = System.Array.Empty<BgInfoTopology>();

    /// <para>Image overlays to include in the configuration.</para>
    [Parameter]
    public BgInfoImage[] Images { get; set; } = System.Array.Empty<BgInfoImage>();

    /// <para>Visual canvas overlays to include in the configuration.</para>
    [Parameter]
    [Alias("VisualCanvas")]
    public BgInfoVisualCanvas[] VisualCanvases { get; set; } = System.Array.Empty<BgInfoVisualCanvas>();

    /// <summary>Creates the configuration object.</summary>
    protected override void EndProcessing() {
        var config = new BgInfoConfiguration();

        if (IsParameterBound(nameof(FilePath))) config.FilePath = FilePath;
        if (IsParameterBound(nameof(OutputFileName))) config.OutputFileName = OutputFileName;
        if (IsParameterBound(nameof(ConfigurationDirectory))) config.ConfigurationDirectory = ConfigurationDirectory;
        if (IsParameterBound(nameof(FontFamilyName))) config.FontFamilyName = FontFamilyName;
        if (IsParameterBound(nameof(Color))) config.Color = PowerShellColorConverter.ConvertRequired(Color, nameof(Color));
        if (IsParameterBound(nameof(BackgroundColor))) config.BackgroundColor = PowerShellColorConverter.ConvertRequired(BackgroundColor, nameof(BackgroundColor));
        if (IsParameterBound(nameof(FontSize))) config.FontSize = FontSize;
        if (IsParameterBound(nameof(Bold))) config.Bold = Bold.IsPresent;
        if (IsParameterBound(nameof(FontWeight))) config.FontWeight = PowerShellTextStyleValidator.ValidateFontWeight(FontWeight, nameof(FontWeight));
        if (IsParameterBound(nameof(Italic))) config.Italic = Italic.IsPresent;
        if (IsParameterBound(nameof(Underline))) config.Underline = Underline.IsPresent;
        if (IsParameterBound(nameof(UnderlineStyle))) config.UnderlineStyle = UnderlineStyle;
        if (IsParameterBound(nameof(StrikethroughStyle))) config.StrikethroughStyle = StrikethroughStyle;
        if (IsParameterBound(nameof(Baseline))) config.Baseline = Baseline;
        if (IsParameterBound(nameof(TextCase))) config.TextCase = TextCase;
        if (IsParameterBound(nameof(ValueColor))) config.ValueColor = PowerShellColorConverter.ConvertRequired(ValueColor, nameof(ValueColor));
        if (IsParameterBound(nameof(ValueFontSize))) config.ValueFontSize = ValueFontSize;
        if (IsParameterBound(nameof(ValueFontFamilyName))) config.ValueFontFamilyName = ValueFontFamilyName;
        if (IsParameterBound(nameof(ValueBold))) config.ValueBold = ValueBold.IsPresent;
        if (IsParameterBound(nameof(ValueFontWeight))) config.ValueFontWeight = PowerShellTextStyleValidator.ValidateFontWeight(ValueFontWeight, nameof(ValueFontWeight));
        if (IsParameterBound(nameof(ValueItalic))) config.ValueItalic = ValueItalic.IsPresent;
        if (IsParameterBound(nameof(ValueUnderline))) config.ValueUnderline = ValueUnderline.IsPresent;
        if (IsParameterBound(nameof(ValueUnderlineStyle))) config.ValueUnderlineStyle = ValueUnderlineStyle;
        if (IsParameterBound(nameof(ValueStrikethroughStyle))) config.ValueStrikethroughStyle = ValueStrikethroughStyle;
        if (IsParameterBound(nameof(ValueBaseline))) config.ValueBaseline = ValueBaseline;
        if (IsParameterBound(nameof(ValueTextCase))) config.ValueTextCase = ValueTextCase;
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
        if (IsParameterBound(nameof(DisableWallpaperSlideshow))) config.PreserveWallpaperSlideshow = !DisableWallpaperSlideshow.IsPresent;
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
        if (IsParameterBound(nameof(Topologies)) && Topologies.Length > 0) {
            config.Topologies.AddRange(Topologies);
        }
        if (IsParameterBound(nameof(Images)) && Images.Length > 0) {
            foreach (var image in Images) if (image != null) config.Images.Add(image);
        }
        if (IsParameterBound(nameof(VisualCanvases)) && VisualCanvases.Length > 0) {
            foreach (var visual in VisualCanvases) if (visual != null) config.VisualCanvases.Add(visual);
        }

        WriteObject(config);
    }

    private bool IsParameterBound(string name) {
        return MyInvocation.BoundParameters.ContainsKey(name);
    }
}
