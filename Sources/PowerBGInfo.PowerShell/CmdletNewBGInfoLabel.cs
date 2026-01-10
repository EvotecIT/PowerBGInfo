using System.Management.Automation;
using DesktopManager;
using ImagePlayground;
using SixLabors.ImageSharp;
using PowerBGInfo;

namespace PowerBGInfo.PowerShell;
/// <summary>Creates a BGInfo label entry.</summary>
[Cmdlet(VerbsCommon.New, "BGInfoLabel")]
[OutputType(typeof(BgInfoEntry))]
public class CmdletNewBGInfoLabel : PSCmdlet {
    /// <para>Label text to render.</para>
    [Parameter(Mandatory = true)]
    public string Name { get; set; } = string.Empty;

    /// <para>Label color override.</para>
    [Parameter]
    public Color Color { get; set; } = Color.Black;

    /// <para>Label font size override.</para>
    [Parameter]
    public float FontSize { get; set; } = 16;

    /// <para>Label font family override.</para>
    [Parameter]
    public string FontFamilyName { get; set; } = "Calibri";

    /// <summary>Emits a BGInfo label entry.</summary>
    protected override void EndProcessing() {
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
