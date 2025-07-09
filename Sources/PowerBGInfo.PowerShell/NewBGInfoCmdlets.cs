using System.Management.Automation;
using DesktopManager;
using ImagePlayground;
using SixLabors.ImageSharp;
using PowerBGInfo;

namespace PowerBGInfo.PowerShell;

[Cmdlet(VerbsCommon.New, "BGInfo")]
[OutputType(typeof(string))]
public class NewBGInfoCommand : PSCmdlet
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
            Target = Target
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

[Cmdlet(VerbsCommon.New, "BGInfoLabel")]
[OutputType(typeof(BgInfoEntry))]
public class NewBGInfoLabelCommand : PSCmdlet
{
    [Parameter(Mandatory = true)]
    public string Name { get; set; } = string.Empty;

    [Parameter]
    public Color Color { get; set; } = Color.Black;

    [Parameter]
    public float FontSize { get; set; } = 16;

    [Parameter]
    public string FontFamilyName { get; set; } = "Calibri";

    protected override void EndProcessing()
    {
        var entry = new BgInfoEntry
        {
            Type = BgInfoEntryType.Label,
            Name = Name,
            Color = Color,
            FontSize = FontSize,
            FontFamilyName = FontFamilyName
        };
        WriteObject(entry);
    }
}

[Cmdlet(VerbsCommon.New, "BGInfoValue")]
[OutputType(typeof(BgInfoEntry))]
public class NewBGInfoValueCommand : PSCmdlet
{
    [Parameter(ParameterSetName = "Values")]
    [Parameter(ParameterSetName = "Builtin")]
    public string Name { get; set; } = string.Empty;

    [Parameter(ParameterSetName = "Values")]
    public string Value { get; set; } = string.Empty;

    [Parameter(ParameterSetName = "Builtin")]
    public string BuiltinValue { get; set; } = string.Empty;

    [Parameter]
    public Color Color { get; set; } = Color.Black;

    [Parameter]
    public float FontSize { get; set; } = 16;

    [Parameter]
    public string FontFamilyName { get; set; } = "Calibri";

    [Parameter]
    public Color ValueColor { get; set; } = Color.Black;

    [Parameter]
    public float ValueFontSize { get; set; } = 16;

    [Parameter]
    public string ValueFontFamilyName { get; set; } = "Calibri";

    protected override void EndProcessing()
    {
        string finalValue = string.IsNullOrEmpty(BuiltinValue) ? Value : SystemInfoProvider.GetValue(BuiltinValue);
        var entry = new BgInfoEntry
        {
            Type = BgInfoEntryType.Value,
            Name = string.IsNullOrEmpty(Name) ? BuiltinValue : Name,
            Value = finalValue,
            Color = Color,
            FontSize = FontSize,
            FontFamilyName = FontFamilyName,
            ValueColor = ValueColor,
            ValueFontSize = ValueFontSize,
            ValueFontFamilyName = ValueFontFamilyName
        };
        WriteObject(entry);
    }
}

[Cmdlet(VerbsCommon.Set, "LogonScreen")]
public class SetLogonScreenCommand : PSCmdlet
{
    [Parameter(Mandatory = true)]
    public string FilePath { get; set; } = string.Empty;

    protected override void EndProcessing()
    {
        var path = System.IO.Path.GetFullPath(FilePath);
        const string regPath = "HKLM:SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\PersonalizationCSP";
        using var key = Microsoft.Win32.Registry.LocalMachine.CreateSubKey(regPath);
        key.SetValue("LockScreenImagePath", path, Microsoft.Win32.RegistryValueKind.String);
        key.SetValue("LockScreenImageUrl", path, Microsoft.Win32.RegistryValueKind.String);
        key.SetValue("LockScreenImageStatus", 1, Microsoft.Win32.RegistryValueKind.DWord);
    }
}
