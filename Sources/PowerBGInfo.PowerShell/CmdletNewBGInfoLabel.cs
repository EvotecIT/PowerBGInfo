using System.Drawing;
using System.Management.Automation;
using PowerBGInfo;

namespace PowerBGInfo.PowerShell;
/// <summary>Creates a BGInfo label entry.</summary>
[Cmdlet(VerbsCommon.New, "BGInfoLabel")]
[OutputType(typeof(BgInfoEntry))]
public class CmdletNewBGInfoLabel : PSCmdlet {
    /// <para>Label text to render.</para>
    [Parameter(Mandatory = true)]
    public string Name { get; set; } = string.Empty;

    /// <para>Variable name used to expand this label multiple times.</para>
    [Parameter]
    public string ForEach { get; set; } = string.Empty;

    /// <para>Label color override.</para>
    [Parameter]
    public Color Color { get; set; }

    /// <para>Label font size override.</para>
    [Parameter]
    public float FontSize { get; set; }

    /// <para>Label font family override.</para>
    [Parameter]
    public string FontFamilyName { get; set; } = string.Empty;

    /// <summary>Emits a BGInfo label entry.</summary>
    protected override void EndProcessing() {
        var entry = new BgInfoEntry
        {
            Type = BgInfoEntryType.Label,
            Name = Name,
            ForEach = string.IsNullOrWhiteSpace(ForEach) ? null : ForEach,
            Color = IsParameterBound(nameof(Color)) ? Color : null,
            FontSize = IsParameterBound(nameof(FontSize)) ? FontSize : null,
            FontFamilyName = IsParameterBound(nameof(FontFamilyName)) ? FontFamilyName : null
        };
        WriteObject(entry);
    }

    private bool IsParameterBound(string name)
    {
        return MyInvocation.BoundParameters.ContainsKey(name);
    }
}
