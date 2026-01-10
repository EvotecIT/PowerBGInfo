using System.Management.Automation;
using DesktopManager;
using ImagePlayground;
using SixLabors.ImageSharp;
using PowerBGInfo;

namespace PowerBGInfo.PowerShell;

/// <summary>Creates a BGInfo overlay image and optionally applies it as wallpaper.</summary>
/// <para>Use the script block to emit label/value entries.</para>
[Cmdlet(VerbsCommon.New, "BGInfo")]
[OutputType(typeof(string))]
public class CmdletNewBGInfo : PSCmdlet {
    /// <para>Script block that outputs BGInfo entries.</para>
    [Parameter(Mandatory = true)]
    public ScriptBlock BGInfoContent { get; set; } = null!;

    /// <para>Optional base wallpaper file path. When omitted, current wallpaper is used.</para>
    [Parameter]
    public string FilePath { get; set; } = string.Empty;

    /// <para>Output directory for generated BGInfo images.</para>
    [Parameter(Mandatory = true)]
    public string ConfigurationDirectory { get; set; } = string.Empty;

    /// <para>Default label font family.</para>
    [Parameter]
    public string FontFamilyName { get; set; } = "Calibri";

    /// <para>Default label color.</para>
    [Parameter]
    public Color Color { get; set; } = Color.Black;

    /// <para>Default label font size.</para>
    [Parameter]
    public int FontSize { get; set; } = 16;

    /// <para>Default value color.</para>
    [Parameter]
    public Color ValueColor { get; set; } = Color.Black;

    /// <para>Default value font size.</para>
    [Parameter]
    public float ValueFontSize { get; set; } = 16;

    /// <para>Default value font family.</para>
    [Parameter]
    public string ValueFontFamilyName { get; set; } = "Calibri";

    /// <para>Vertical spacing between rows.</para>
    [Parameter]
    public int SpaceBetweenLines { get; set; } = 10;

    /// <para>Spacing between label and value columns.</para>
    [Parameter]
    public int SpaceBetweenColumns { get; set; } = 30;

    /// <para>Legacy position X placeholder (reserved for future layout strategies).</para>
    [Parameter]
    public float PositionX { get; set; } = 10;

    /// <para>Legacy position Y placeholder (reserved for future layout strategies).</para>
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
    public DesktopWallpaperPosition WallpaperFit { get; set; } = DesktopWallpaperPosition.Center;

    /// <para>Layout anchor position (for example TopLeft, TopCenter, BottomRight).</para>
    [Parameter]
    public BgInfoTextPosition TextPosition { get; set; } = BgInfoTextPosition.TopLeft;

    /// <para>Output target (Wallpaper, File, or Both).</para>
    [Parameter]
    public BgInfoTarget Target { get; set; } = BgInfoTarget.Wallpaper;

    /// <para>Use screen coordinates for placement calculations.</para>
    [Parameter]
    public SwitchParameter UseScreenCoordinates { get; set; }

    /// <summary>Processes the BGInfo script block and generates the image.</summary>
    protected override void ProcessRecord() {
        var config = new BgInfoConfiguration
        {
            FilePath = FilePath,
            ConfigurationDirectory = ConfigurationDirectory,
            FontFamilyName = FontFamilyName,
            Color = Color,
            FontSize = FontSize,
            ValueColor = ValueColor,
            ValueFontFamilyName = ValueFontFamilyName,
            ValueFontSize = ValueFontSize,
            SpaceBetweenLines = SpaceBetweenLines,
            SpaceBetweenColumns = SpaceBetweenColumns,
            PositionX = PositionX,
            PositionY = PositionY,
            MonitorIndex = MonitorIndex,
            SpaceX = SpaceX,
            SpaceY = SpaceY,
            WallpaperFit = WallpaperFit,
            TextPosition = TextPosition,
            Target = Target,
            UseScreenCoordinates = UseScreenCoordinates.IsPresent
        };

        var results = BGInfoContent.Invoke();
        foreach (var item in results)
        {
            if (item.BaseObject is BgInfoEntry entry)
            {
                config.Entries.Add(entry);
            }
        }

        var generator = new BgInfoGenerator(new ImageService(), new WallpaperService());
        var path = generator.Generate(config);
        WriteObject(path);
    }
}
