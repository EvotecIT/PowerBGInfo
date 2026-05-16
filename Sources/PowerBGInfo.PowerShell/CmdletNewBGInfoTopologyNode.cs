using ChartForgeX.Topology;
using PowerBGInfo;
using System.Management.Automation;

namespace PowerBGInfo.PowerShell;

/// <summary>Creates a BGInfo topology node definition.</summary>
[Cmdlet(VerbsCommon.New, "BGInfoTopologyNode")]
[OutputType(typeof(TopologyNode))]
public sealed class CmdletNewBGInfoTopologyNode : PSCmdlet {
    /// <para>Stable node identifier used by topology edges.</para>
    [Parameter(Mandatory = true, Position = 0)]
    public string Id { get; set; } = string.Empty;

    /// <para>Node label rendered in the topology.</para>
    [Parameter(Mandatory = true, Position = 1)]
    public string Label { get; set; } = string.Empty;

    /// <para>Optional node subtitle.</para>
    [Parameter]
    public string Subtitle { get; set; } = string.Empty;

    /// <para>Node kind used for icon and legend selection.</para>
    [Parameter]
    public TopologyNodeKind Kind { get; set; } = TopologyNodeKind.Generic;

    /// <para>Node health or state.</para>
    [Parameter]
    public TopologyHealthStatus Status { get; set; } = TopologyHealthStatus.Unknown;

    /// <para>Optional parent group identifier.</para>
    [Parameter]
    public string GroupId { get; set; } = string.Empty;

    /// <para>Short symbol rendered inside or near the node icon.</para>
    [Parameter]
    public string Symbol { get; set; } = string.Empty;

    /// <para>Optional badge text.</para>
    [Parameter]
    public string Badge { get; set; } = string.Empty;

    /// <para>Optional node accent color as CSS hex.</para>
    [Parameter]
    public string Color { get; set; } = string.Empty;

    /// <para>Optional node display mode override.</para>
    [Parameter]
    public TopologyNodeDisplayMode DisplayMode { get; set; } = TopologyNodeDisplayMode.CompactCard;

    /// <summary>Emits a topology node definition.</summary>
    protected override void EndProcessing() {
        var node = new TopologyNode {
            Id = Id,
            Label = Label,
            Kind = Kind,
            Status = Status
        };

        if (!string.IsNullOrWhiteSpace(Subtitle)) {
            node.Subtitle = Subtitle;
        }
        if (!string.IsNullOrWhiteSpace(GroupId)) {
            node.GroupId = GroupId;
        }
        if (!string.IsNullOrWhiteSpace(Symbol)) {
            node.Symbol = Symbol;
        }
        if (!string.IsNullOrWhiteSpace(Badge)) {
            node.Badge = Badge;
        }
        if (!string.IsNullOrWhiteSpace(Color)) {
            node.Color = Color;
        }
        if (MyInvocation.BoundParameters.ContainsKey(nameof(DisplayMode))) {
            node.DisplayMode = DisplayMode;
        }

        WriteObject(node);
    }
}
