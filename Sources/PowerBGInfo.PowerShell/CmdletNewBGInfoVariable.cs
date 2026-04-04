using System.Management.Automation;

namespace PowerBGInfo.PowerShell;

/// <summary>Creates a reusable BGInfo variable backed by a built-in provider.</summary>
[Cmdlet(VerbsCommon.New, "BGInfoVariable")]
[OutputType(typeof(BgInfoVariable))]
public sealed class CmdletNewBGInfoVariable : PSCmdlet {
    /// <para>Name used by -ForEach references.</para>
    [Parameter(Mandatory = true)]
    public string Name { get; set; } = string.Empty;

    /// <para>Built-in provider used to populate the variable.</para>
    [Parameter(Mandatory = true)]
    public BgInfoVariableProvider Provider { get; set; }

    /// <para>Optional provider argument for filtering/customization.</para>
    [Parameter]
    public string Argument { get; set; } = string.Empty;

    /// <summary>Emits the BGInfo variable definition.</summary>
    protected override void EndProcessing() {
        WriteObject(new BgInfoVariable {
            Name = Name,
            Provider = Provider,
            Argument = string.IsNullOrWhiteSpace(Argument) ? null : Argument
        });
    }
}
