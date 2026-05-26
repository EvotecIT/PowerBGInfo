using System.Management.Automation;
using PowerBGInfo;

namespace PowerBGInfo.PowerShell;

/// <summary>Creates a BGInfo visual canvas definition backed by ChartForgeX.</summary>
[Cmdlet(VerbsCommon.New, "BGInfoVisualCanvas")]
[OutputType(typeof(BgInfoVisualCanvas))]
public class CmdletNewBGInfoVisualCanvas : PSCmdlet {
    /// <para>Visual canvas template.</para>
    [Parameter]
    public BgInfoVisualCanvasTemplate Template { get; set; } = BgInfoVisualCanvasTemplate.PowerBgInfoHero;

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
        var visual = new BgInfoVisualCanvas {
            Template = Template,
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
            Transparent = !Opaque.IsPresent,
            TechBackdrop = !NoTechBackdrop.IsPresent
        };
        foreach (var tile in Tile ?? Array.Empty<BgInfoVisualCanvasTile>()) if (tile != null) visual.Tiles.Add(tile);
        foreach (var feature in Feature ?? Array.Empty<BgInfoVisualCanvasFeature>()) if (feature != null) visual.Features.Add(feature);
        WriteObject(visual);
    }
}
