using System.Management.Automation;
using PowerBGInfo;

namespace PowerBGInfo.PowerShell;

/// <summary>Creates a BGInfo visual canvas definition backed by ChartForgeX.</summary>
/// <para>Visual canvases render a reusable HUD-style overlay with a central title, side information boxes, and an optional feature strip.</para>
/// <example>
/// <code>
/// $tiles = @(
///     New-BGInfoVisualCanvasTile -Side Left -IconKind Computer -SurfaceStyle Raised -Label HOSTNAME -Value '{{HostName}}'
///     New-BGInfoVisualCanvasTile -Side Right -IconKind Cpu -SurfaceStyle Raised -Label 'CPU LOAD' -Value '31% active' -MiniChartKind Area -MiniChartValues 22,28,25,36,31 -MiniChartMaximum 100
/// )
///
/// New-BGInfo -Target File {
///     New-BGInfoVisualCanvas -Title 'PowerBGInfo' -Subtitle 'High-contrast information boxes' -Tile $tiles -TileGlassTop '#FFF7EDD9' -TileGlassBottom '#DBEAFECC' -TileValueColor '#0F172AFF'
/// } -FilePath .\Examples\Samples\TapC-Evotec-2560x1080.jpg -ConfigurationDirectory .\Examples\Output -OutputFileName 'PowerBGInfo.VisualCanvas.ContrastBox.jpg' -WallpaperFit Fill
/// </code>
/// </example>
/// <example>
/// <code>
/// New-BGInfoVisualCanvas -Title 'PowerBGInfo' -Feature $features -FeatureAnchor BottomRight -FeatureWidth 610 -FeatureOffsetX 165 -FeatureOffsetY 120
/// </code>
/// </example>
[Cmdlet(VerbsCommon.New, "BGInfoVisualCanvas")]
[OutputType(typeof(BgInfoVisualCanvas))]
public class CmdletNewBGInfoVisualCanvas : PSCmdlet {
    /// <para>Visual canvas template.</para>
    [Parameter]
    public BgInfoVisualCanvasTemplate Template { get; set; } = BgInfoVisualCanvasTemplate.PowerBgInfoHero;

    /// <para>Responsive side-rail sizing preset.</para>
    [Parameter]
    public BgInfoVisualCanvasLayoutPreset LayoutPreset { get; set; }

    /// <para>Canvas title or brand text.</para>
    [Parameter]
    public string Title { get; set; } = "PowerBGInfo";

    /// <para>Canvas subtitle text.</para>
    [Parameter]
    public string Subtitle { get; set; } = "Desktop background insights for Windows and PowerShell";

    /// <para>Canvas width in pixels. Zero uses the target wallpaper width.</para>
    [Parameter]
    public int Width { get; set; }

    /// <para>Canvas height in pixels. Zero uses the target wallpaper height.</para>
    [Parameter]
    public int Height { get; set; }

    /// <para>Explicit X position on the generated wallpaper.</para>
    [Parameter]
    public int PositionX { get; set; }

    /// <para>Explicit Y position on the generated wallpaper.</para>
    [Parameter]
    public int PositionY { get; set; }

    /// <para>Top background color.</para>
    [Parameter]
    public object? BackgroundTop { get; set; }

    /// <para>Bottom background color.</para>
    [Parameter]
    public object? BackgroundBottom { get; set; }

    /// <para>Primary accent color.</para>
    [Parameter]
    public object? Accent { get; set; }

    /// <para>Secondary accent color for badge and backdrop highlights.</para>
    [Parameter]
    public object? SecondaryAccent { get; set; }

    /// <para>Primary hero title color.</para>
    [Parameter]
    public object? TitleColor { get; set; }

    /// <para>Accent hero title color.</para>
    [Parameter]
    public object? TitleAccentColor { get; set; }

    /// <para>Subtitle text color.</para>
    [Parameter]
    public object? SubtitleColor { get; set; }

    /// <para>Glass tile top color.</para>
    [Parameter]
    public object? TileGlassTop { get; set; }

    /// <para>Glass tile bottom color.</para>
    [Parameter]
    public object? TileGlassBottom { get; set; }

    /// <para>Tile label text color.</para>
    [Parameter]
    public object? TileLabelColor { get; set; }

    /// <para>Tile value text color.</para>
    [Parameter]
    public object? TileValueColor { get; set; }

    /// <para>Tile detail text color.</para>
    [Parameter]
    public object? TileDetailColor { get; set; }

    /// <para>Tile progress track color.</para>
    [Parameter]
    public object? TileProgressTrackColor { get; set; }

    /// <para>Hero badge top fill color.</para>
    [Parameter]
    public object? HeroBadgeTop { get; set; }

    /// <para>Hero badge bottom fill color.</para>
    [Parameter]
    public object? HeroBadgeBottom { get; set; }

    /// <para>Hero badge symbol color.</para>
    [Parameter]
    public object? HeroBadgeTextColor { get; set; }

    /// <para>Hide the central hero badge while keeping the title, subtitle, tiles, and feature strip.</para>
    [Parameter]
    public SwitchParameter NoHeroBadge { get; set; }

    /// <para>Text rendered in the central hero badge when no image is configured.</para>
    [Parameter]
    [Alias("HeroBadgeSymbol")]
    public string HeroBadgeText { get; set; } = ">_";

    /// <para>Optional image path rendered inside the central hero badge.</para>
    [Parameter]
    public string HeroBadgeImagePath { get; set; } = string.Empty;

    /// <para>How the hero badge image is fitted inside the badge.</para>
    [Parameter]
    public BgInfoImageFit HeroBadgeImageFit { get; set; } = BgInfoImageFit.Contain;

    /// <para>Padding inside the hero badge image area.</para>
    [Parameter]
    public int HeroBadgeImagePadding { get; set; } = 10;

    /// <para>Hero badge image opacity from zero to one.</para>
    [Parameter]
    public double HeroBadgeImageOpacity { get; set; } = 1d;

    /// <para>Optional feature-strip anchor. When omitted, the template keeps its default centered strip placement.</para>
    [Parameter]
    public BgInfoTextPosition FeatureAnchor { get; set; } = BgInfoTextPosition.BottomCenter;

    /// <para>Optional feature-strip width in pixels. Zero uses the template default width.</para>
    [Parameter]
    public int FeatureWidth { get; set; }

    /// <para>Optional feature-strip height in pixels. Zero uses the template default height.</para>
    [Parameter]
    public int FeatureHeight { get; set; }

    /// <para>Default side-rail tile width in pixels. Zero uses the template default width.</para>
    [Parameter]
    public int TileWidth { get; set; }

    /// <para>Default side-rail tile height in pixels. Zero uses the template default height.</para>
    [Parameter]
    public int TileHeight { get; set; }

    /// <para>Default vertical gap between side-rail tiles in pixels. Zero uses the template default gap.</para>
    [Parameter]
    public int TileGap { get; set; }

    /// <para>Default left side-rail tile width in pixels. Zero uses TileWidth or the template default.</para>
    [Parameter]
    public int LeftTileWidth { get; set; }

    /// <para>Default right side-rail tile width in pixels. Zero uses TileWidth or the template default.</para>
    [Parameter]
    public int RightTileWidth { get; set; }

    /// <para>Horizontal left side-rail offset in pixels.</para>
    [Parameter]
    public int LeftTileOffsetX { get; set; }

    /// <para>Vertical left side-rail offset in pixels.</para>
    [Parameter]
    public int LeftTileOffsetY { get; set; }

    /// <para>Horizontal right side-rail inset in pixels.</para>
    [Parameter]
    public int RightTileOffsetX { get; set; }

    /// <para>Vertical right side-rail offset in pixels.</para>
    [Parameter]
    public int RightTileOffsetY { get; set; }

    /// <para>Default side-rail tile text fitting policy.</para>
    [Parameter]
    public BgInfoVisualCanvasTileTextFitPolicy TileTextFitPolicy { get; set; }

    /// <para>Horizontal feature-strip offset. For right anchors, positive values inset from the right edge.</para>
    [Parameter]
    public int FeatureOffsetX { get; set; }

    /// <para>Vertical feature-strip offset. For bottom anchors, positive values inset from the bottom edge.</para>
    [Parameter]
    public int FeatureOffsetY { get; set; }

    /// <para>Disable the built-in technology backdrop.</para>
    [Parameter]
    public SwitchParameter NoTechBackdrop { get; set; }

    /// <para>Render a full ChartForgeX background instead of floating over the wallpaper.</para>
    [Parameter]
    public SwitchParameter Opaque { get; set; }

    /// <para>Side rail tile definitions.</para>
    [Parameter]
    public BgInfoVisualCanvasTile[] Tile { get; set; } = Array.Empty<BgInfoVisualCanvasTile>();

    /// <para>Feature strip item definitions.</para>
    [Parameter]
    public BgInfoVisualCanvasFeature[] Feature { get; set; } = Array.Empty<BgInfoVisualCanvasFeature>();

    /// <summary>Emits a visual canvas definition.</summary>
    protected override void EndProcessing() {
        if (HeroBadgeImagePadding < 0) {
            ThrowTerminatingError(new ErrorRecord(new ArgumentOutOfRangeException(nameof(HeroBadgeImagePadding), HeroBadgeImagePadding, "Hero badge image padding cannot be negative."), "BGInfoVisualCanvasInvalidHeroBadgeImagePadding", ErrorCategory.InvalidArgument, HeroBadgeImagePadding));
            return;
        }
        if (double.IsNaN(HeroBadgeImageOpacity) || double.IsInfinity(HeroBadgeImageOpacity) || HeroBadgeImageOpacity < 0d || HeroBadgeImageOpacity > 1d) {
            ThrowTerminatingError(new ErrorRecord(new ArgumentOutOfRangeException(nameof(HeroBadgeImageOpacity), HeroBadgeImageOpacity, "Hero badge image opacity must be between 0 and 1."), "BGInfoVisualCanvasInvalidHeroBadgeImageOpacity", ErrorCategory.InvalidArgument, HeroBadgeImageOpacity));
            return;
        }

        var visual = new BgInfoVisualCanvas {
            Template = Template,
            LayoutPreset = LayoutPreset,
            Title = Title,
            Subtitle = Subtitle,
            Width = Width,
            Height = Height,
            PositionX = PositionX,
            PositionY = PositionY,
            BackgroundTop = PowerShellColorConverter.ConvertOptional(BackgroundTop, nameof(BackgroundTop)) ?? System.Drawing.Color.FromArgb(255, 2, 7, 19),
            BackgroundBottom = PowerShellColorConverter.ConvertOptional(BackgroundBottom, nameof(BackgroundBottom)) ?? System.Drawing.Color.FromArgb(255, 7, 26, 53),
            Accent = PowerShellColorConverter.ConvertOptional(Accent, nameof(Accent)) ?? System.Drawing.Color.FromArgb(255, 47, 128, 255),
            SecondaryAccent = PowerShellColorConverter.ConvertOptional(SecondaryAccent, nameof(SecondaryAccent)),
            TitleColor = PowerShellColorConverter.ConvertOptional(TitleColor, nameof(TitleColor)),
            TitleAccentColor = PowerShellColorConverter.ConvertOptional(TitleAccentColor, nameof(TitleAccentColor)),
            SubtitleColor = PowerShellColorConverter.ConvertOptional(SubtitleColor, nameof(SubtitleColor)),
            TileGlassTop = PowerShellColorConverter.ConvertOptional(TileGlassTop, nameof(TileGlassTop)),
            TileGlassBottom = PowerShellColorConverter.ConvertOptional(TileGlassBottom, nameof(TileGlassBottom)),
            TileLabelColor = PowerShellColorConverter.ConvertOptional(TileLabelColor, nameof(TileLabelColor)),
            TileValueColor = PowerShellColorConverter.ConvertOptional(TileValueColor, nameof(TileValueColor)),
            TileDetailColor = PowerShellColorConverter.ConvertOptional(TileDetailColor, nameof(TileDetailColor)),
            TileProgressTrackColor = PowerShellColorConverter.ConvertOptional(TileProgressTrackColor, nameof(TileProgressTrackColor)),
            HeroBadgeTop = PowerShellColorConverter.ConvertOptional(HeroBadgeTop, nameof(HeroBadgeTop)),
            HeroBadgeBottom = PowerShellColorConverter.ConvertOptional(HeroBadgeBottom, nameof(HeroBadgeBottom)),
            HeroBadgeTextColor = PowerShellColorConverter.ConvertOptional(HeroBadgeTextColor, nameof(HeroBadgeTextColor)),
            HeroBadgeVisible = !NoHeroBadge.IsPresent,
            HeroBadgeText = HeroBadgeText,
            HeroBadgeImagePath = string.IsNullOrWhiteSpace(HeroBadgeImagePath) ? string.Empty : SessionState.Path.GetUnresolvedProviderPathFromPSPath(HeroBadgeImagePath),
            HeroBadgeImageFit = HeroBadgeImageFit,
            HeroBadgeImagePadding = HeroBadgeImagePadding,
            HeroBadgeImageOpacity = HeroBadgeImageOpacity,
            FeatureAnchor = MyInvocation.BoundParameters.ContainsKey(nameof(FeatureAnchor)) ? FeatureAnchor : null,
            FeatureWidth = FeatureWidth,
            FeatureHeight = FeatureHeight,
            TileWidth = TileWidth,
            TileHeight = TileHeight,
            TileGap = TileGap,
            LeftTileWidth = LeftTileWidth,
            RightTileWidth = RightTileWidth,
            LeftTileOffsetX = LeftTileOffsetX,
            LeftTileOffsetY = LeftTileOffsetY,
            RightTileOffsetX = RightTileOffsetX,
            RightTileOffsetY = RightTileOffsetY,
            TileTextFitPolicy = TileTextFitPolicy,
            FeatureOffsetX = FeatureOffsetX,
            FeatureOffsetY = FeatureOffsetY,
            Transparent = !Opaque.IsPresent,
            TechBackdrop = !NoTechBackdrop.IsPresent
        };
        foreach (var tile in Tile ?? Array.Empty<BgInfoVisualCanvasTile>()) if (tile != null) visual.Tiles.Add(tile);
        foreach (var feature in Feature ?? Array.Empty<BgInfoVisualCanvasFeature>()) if (feature != null) visual.Features.Add(feature);
        WriteObject(visual);
    }

}
