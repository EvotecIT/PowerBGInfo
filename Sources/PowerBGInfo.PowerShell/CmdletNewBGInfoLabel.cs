using System.Management.Automation;
using DesktopManager;
using ImagePlayground;
using SixLabors.ImageSharp;
using PowerBGInfo;

namespace PowerBGInfo.PowerShell;
[Cmdlet(VerbsCommon.New, "BGInfoLabel")]
[OutputType(typeof(BgInfoEntry))]
public class CmdletNewBGInfoLabel : PSCmdlet
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