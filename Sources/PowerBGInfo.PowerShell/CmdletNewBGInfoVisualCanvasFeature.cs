using System.Management.Automation;
using PowerBGInfo;

namespace PowerBGInfo.PowerShell;

/// <summary>Creates a BGInfo visual canvas feature-strip item.</summary>
/// <para>Feature-strip items are compact labels shown in the optional visual canvas footer strip.</para>
/// <example>
/// <code>
/// $features = @(
///     New-BGInfoVisualCanvasFeature -Icon 'A+' -Label 'light contrast boxes'
///     New-BGInfoVisualCanvasFeature -Icon 'JSON' -Label 'portable config'
/// )
/// </code>
/// </example>
[Cmdlet(VerbsCommon.New, "BGInfoVisualCanvasFeature")]
[OutputType(typeof(BgInfoVisualCanvasFeature))]
public class CmdletNewBGInfoVisualCanvasFeature : PSCmdlet {
    /// <para>Compact item icon or symbol.</para>
    [Parameter]
    public string Icon { get; set; } = string.Empty;

    /// <para>Feature label.</para>
    [Parameter(Mandatory = true)]
    public string Label { get; set; } = string.Empty;

    /// <summary>Emits a visual canvas feature-strip item.</summary>
    protected override void EndProcessing() {
        WriteObject(new BgInfoVisualCanvasFeature {
            Icon = Icon,
            Label = Label
        });
    }
}
