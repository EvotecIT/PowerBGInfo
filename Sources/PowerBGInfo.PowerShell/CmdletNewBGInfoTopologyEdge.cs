using ChartForgeX.Primitives;
using ChartForgeX.Topology;
using PowerBGInfo;
using System.Management.Automation;

namespace PowerBGInfo.PowerShell;

/// <summary>Creates a BGInfo topology edge definition.</summary>
[Cmdlet(VerbsCommon.New, "BGInfoTopologyEdge")]
[OutputType(typeof(TopologyEdge))]
public sealed class CmdletNewBGInfoTopologyEdge : PSCmdlet {
    /// <para>Stable edge identifier. When omitted, one is derived from source and target ids.</para>
    [Parameter]
    public string Id { get; set; } = string.Empty;

    /// <para>Source node identifier.</para>
    [Parameter(Mandatory = true, Position = 0)]
    public string SourceNodeId { get; set; } = string.Empty;

    /// <para>Target node identifier.</para>
    [Parameter(Mandatory = true, Position = 1)]
    public string TargetNodeId { get; set; } = string.Empty;

    /// <para>Primary edge label.</para>
    [Parameter(Position = 2)]
    public string Label { get; set; } = string.Empty;

    /// <para>Relationship kind.</para>
    [Parameter]
    public TopologyEdgeKind Kind { get; set; } = TopologyEdgeKind.Generic;

    /// <para>Relationship health or state.</para>
    [Parameter]
    public TopologyHealthStatus Status { get; set; } = TopologyHealthStatus.Unknown;

    /// <para>Direction marker behavior.</para>
    [Parameter]
    public VisualLinkDirection Direction { get; set; } = VisualLinkDirection.None;

    /// <para>Edge routing mode.</para>
    [Parameter]
    public TopologyEdgeRouting Routing { get; set; } = TopologyEdgeRouting.Orthogonal;

    /// <para>Optional edge color as CSS hex.</para>
    [Parameter]
    public string Color { get; set; } = string.Empty;

    /// <para>Render the edge as a quiet structural relationship.</para>
    [Parameter]
    public SwitchParameter Muted { get; set; }

    /// <summary>Emits a topology edge definition.</summary>
    protected override void EndProcessing() {
        var edge = new TopologyEdge {
            Id = string.IsNullOrWhiteSpace(Id) ? SourceNodeId + "-" + TargetNodeId : Id,
            SourceNodeId = SourceNodeId,
            TargetNodeId = TargetNodeId,
            Kind = Kind,
            Status = Status,
            Direction = Direction,
            Routing = Routing,
            IsMuted = Muted.IsPresent
        };

        if (!string.IsNullOrWhiteSpace(Label)) {
            edge.Label = Label;
        }
        if (!string.IsNullOrWhiteSpace(Color)) {
            edge.Color = Color;
        }

        WriteObject(edge);
    }
}
