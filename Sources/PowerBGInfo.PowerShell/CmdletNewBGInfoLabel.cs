using System.Management.Automation;
using ChartForgeX.Typography;
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
    public object? Color { get; set; }

    /// <para>Label font size override.</para>
    [Parameter]
    public float FontSize { get; set; }

    /// <para>Label font family override.</para>
    [Parameter]
    public string FontFamilyName { get; set; } = string.Empty;

    /// <para>Render the label with a bold font weight.</para>
    [Parameter]
    public SwitchParameter Bold { get; set; }

    /// <para>Numeric label font weight from 100 through 900.</para>
    [Parameter]
    [ValidateRange(100, 900)]
    public int FontWeight { get; set; }

    /// <para>Render the label with italic text.</para>
    [Parameter]
    public SwitchParameter Italic { get; set; }

    /// <para>Underline the label.</para>
    [Parameter]
    public SwitchParameter Underline { get; set; }

    /// <para>Underline pattern for the label.</para>
    [Parameter]
    public TextDecorationStyle UnderlineStyle { get; set; }

    /// <para>Strikethrough pattern for the label.</para>
    [Parameter]
    public TextDecorationStyle StrikethroughStyle { get; set; }

    /// <para>Subscript or superscript placement for the label.</para>
    [Parameter]
    public TextBaseline Baseline { get; set; }

    /// <para>Display-time casing transform for the label.</para>
    [Parameter]
    public TextCaseTransform TextCase { get; set; }

    /// <summary>Emits a BGInfo label entry.</summary>
    protected override void EndProcessing() {
        var entry = new BgInfoEntry
        {
            Type = BgInfoEntryType.Label,
            Name = Name,
            ForEach = string.IsNullOrWhiteSpace(ForEach) ? null : ForEach,
            Color = IsParameterBound(nameof(Color)) ? PowerShellColorConverter.ConvertRequired(Color, nameof(Color)) : null,
            FontSize = IsParameterBound(nameof(FontSize)) ? FontSize : null,
            FontFamilyName = IsParameterBound(nameof(FontFamilyName)) ? FontFamilyName : null,
            Bold = IsParameterBound(nameof(Bold)) ? Bold.IsPresent : null,
            FontWeight = IsParameterBound(nameof(FontWeight)) ? PowerShellTextStyleValidator.ValidateFontWeight(FontWeight, nameof(FontWeight)) : null,
            Italic = IsParameterBound(nameof(Italic)) ? Italic.IsPresent : null,
            Underline = IsParameterBound(nameof(Underline)) ? Underline.IsPresent : null,
            UnderlineStyle = IsParameterBound(nameof(UnderlineStyle)) ? UnderlineStyle : null,
            StrikethroughStyle = IsParameterBound(nameof(StrikethroughStyle)) ? StrikethroughStyle : null,
            Baseline = IsParameterBound(nameof(Baseline)) ? Baseline : null,
            TextCase = IsParameterBound(nameof(TextCase)) ? TextCase : null
        };
        WriteObject(entry);
    }

    private bool IsParameterBound(string name)
    {
        return MyInvocation.BoundParameters.ContainsKey(name);
    }
}
