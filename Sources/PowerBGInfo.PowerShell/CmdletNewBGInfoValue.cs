using System.Management.Automation;
using DesktopManager;
using ImagePlayground;
using SixLabors.ImageSharp;
using PowerBGInfo;

namespace PowerBGInfo.PowerShell;
/// <summary>Creates a BGInfo value entry.</summary>

[Cmdlet(VerbsCommon.New, "BGInfoValue")]
[OutputType(typeof(BgInfoEntry))]
public class CmdletNewBGInfoValue : PSCmdlet {
    /// <para>Label text to render.</para>
    [Parameter(ParameterSetName = "Values")]
    [Parameter(ParameterSetName = "Builtin")]
    public string Name { get; set; } = string.Empty;

    /// <para>Explicit value to render.</para>
    [Parameter(ParameterSetName = "Values")]
    public string Value { get; set; } = string.Empty;

    /// <para>Built-in token to resolve to a value.</para>
    [Parameter(ParameterSetName = "Builtin")]
    public string BuiltinValue { get; set; } = string.Empty;

    /// <para>Label color override.</para>
    [Parameter]
    public Color Color { get; set; } = Color.Black;

    /// <para>Label font size override.</para>
    [Parameter]
    public float FontSize { get; set; } = 16;

    /// <para>Label font family override.</para>
    [Parameter]
    public string FontFamilyName { get; set; } = "Calibri";

    /// <para>Value color override.</para>
    [Parameter]
    public Color ValueColor { get; set; } = Color.Black;

    /// <para>Value font size override.</para>
    [Parameter]
    public float ValueFontSize { get; set; } = 16;

    /// <para>Value font family override.</para>
    [Parameter]
    public string ValueFontFamilyName { get; set; } = "Calibri";

    /// <summary>Emits a BGInfo value entry.</summary>
    protected override void EndProcessing() {
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
