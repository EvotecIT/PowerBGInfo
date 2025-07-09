using System.Management.Automation;
using DesktopManager;
using ImagePlayground;
using SixLabors.ImageSharp;
using PowerBGInfo;

namespace PowerBGInfo.PowerShell;

[Cmdlet(VerbsCommon.New, "BGInfoValue")]
[OutputType(typeof(BgInfoEntry))]
public class CmdletNewBGInfoValue : PSCmdlet
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