using System.Management.Automation;
using DesktopManager;
using ImagePlayground;
using SixLabors.ImageSharp;
using PowerBGInfo;

namespace PowerBGInfo.PowerShell;

[Cmdlet(VerbsCommon.New, "BGInfo")]
[OutputType(typeof(string))]
public class CmdletNewBGInfo : PSCmdlet
{
    [Parameter(Mandatory = true)]
    public ScriptBlock BGInfoContent { get; set; } = null!;

    [Parameter]
    public string FilePath { get; set; } = string.Empty;

    [Parameter(Mandatory = true)]
    public string ConfigurationDirectory { get; set; } = string.Empty;

    [Parameter]
    public string FontFamilyName { get; set; } = "Calibri";

    [Parameter]
    public Color Color { get; set; } = Color.Black;

    [Parameter]
    public int FontSize { get; set; } = 16;

    [Parameter]
    public Color ValueColor { get; set; } = Color.Black;

    [Parameter]
    public float ValueFontSize { get; set; } = 16;

    [Parameter]
    public string ValueFontFamilyName { get; set; } = "Calibri";

    [Parameter]
    public int SpaceBetweenLines { get; set; } = 10;

    [Parameter]
    public int SpaceBetweenColumns { get; set; } = 30;

    [Parameter]
    public float PositionX { get; set; } = 10;

    [Parameter]
    public float PositionY { get; set; } = 10;

    [Parameter]
    public int MonitorIndex { get; set; }

    [Parameter]
    public int SpaceX { get; set; } = 10;

    [Parameter]
    public int SpaceY { get; set; } = 10;

    [Parameter]
    public DesktopWallpaperPosition WallpaperFit { get; set; } = DesktopWallpaperPosition.Center;

    [Parameter]
    public string TextPosition { get; set; } = "TopLeft";

    [Parameter]
    public string Target { get; set; } = "Wallpaper";

    [Parameter]
    public SwitchParameter UseScreenCoordinates { get; set; }

    protected override void ProcessRecord()
    {
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
    }}