using ChartForgeX.Topology;
using PowerBGInfo;
using System.Management.Automation;

namespace PowerBGInfo.PowerShell;

/// <summary>Creates a BGInfo topology group definition.</summary>
[Cmdlet(VerbsCommon.New, "BGInfoTopologyGroup")]
[OutputType(typeof(TopologyGroup))]
public sealed class CmdletNewBGInfoTopologyGroup : PSCmdlet {
    /// <para>Stable group identifier.</para>
    [Parameter(Mandatory = true, Position = 0)]
    public string Id { get; set; } = string.Empty;

    /// <para>Group label rendered in the topology.</para>
    [Parameter(Mandatory = true, Position = 1)]
    public string Label { get; set; } = string.Empty;

    /// <para>Optional group subtitle.</para>
    [Parameter]
    public string Subtitle { get; set; } = string.Empty;

    /// <para>Group health or state.</para>
    [Parameter]
    public TopologyHealthStatus Status { get; set; } = TopologyHealthStatus.Unknown;

    /// <para>Short symbol rendered near the group header.</para>
    [Parameter]
    public string Symbol { get; set; } = string.Empty;

    /// <para>Optional group accent color as CSS hex.</para>
    [Parameter]
    public string Color { get; set; } = string.Empty;

    /// <summary>Emits a topology group definition.</summary>
    protected override void EndProcessing() {
        var group = new TopologyGroup {
            Id = Id,
            Label = Label,
            Status = Status,
            Width = 320,
            Height = 220
        };

        if (!string.IsNullOrWhiteSpace(Subtitle)) {
            group.Subtitle = Subtitle;
        }
        if (!string.IsNullOrWhiteSpace(Symbol)) {
            group.Symbol = Symbol;
        }
        if (!string.IsNullOrWhiteSpace(Color)) {
            group.Color = Color;
        }

        WriteObject(group);
    }
}
