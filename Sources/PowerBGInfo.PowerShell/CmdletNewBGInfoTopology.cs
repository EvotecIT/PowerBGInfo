using ChartForgeX.Topology;
using PowerBGInfo;
using System.Management.Automation;

namespace PowerBGInfo.PowerShell;

/// <summary>Creates a BGInfo topology overlay definition.</summary>
[Cmdlet(VerbsCommon.New, "BGInfoTopology")]
[OutputType(typeof(BgInfoTopology))]
public sealed class CmdletNewBGInfoTopology : PSCmdlet {
    /// <para>Script block that emits topology groups, nodes, and edges.</para>
    [Parameter(Mandatory = true, Position = 0)]
    public ScriptBlock TopologyDefinition { get; set; } = null!;

    /// <para>Topology title.</para>
    [Parameter]
    public string Title { get; set; } = string.Empty;

    /// <para>Topology subtitle.</para>
    [Parameter]
    public string Subtitle { get; set; } = string.Empty;

    /// <para>Topology width in pixels.</para>
    [Parameter]
    public int Width { get; set; } = 520;

    /// <para>Topology height in pixels.</para>
    [Parameter]
    public int Height { get; set; } = 300;

    /// <para>Anchor position for placement.</para>
    [Parameter]
    public BgInfoTextPosition Anchor { get; set; } = BgInfoTextPosition.BottomRight;

    /// <para>Horizontal offset from the anchor.</para>
    [Parameter]
    public int OffsetX { get; set; } = 32;

    /// <para>Vertical offset from the anchor.</para>
    [Parameter]
    public int OffsetY { get; set; } = 32;

    /// <para>Absolute X position.</para>
    [Parameter]
    public int PositionX { get; set; }

    /// <para>Absolute Y position.</para>
    [Parameter]
    public int PositionY { get; set; }

    /// <para>Topology layout mode.</para>
    [Parameter]
    public TopologyLayoutMode Layout { get; set; } = TopologyLayoutMode.Layered;

    /// <para>Topology layout direction.</para>
    [Parameter]
    public TopologyLayoutDirection Direction { get; set; } = TopologyLayoutDirection.LeftToRight;

    /// <para>Node presentation mode.</para>
    [Parameter]
    public TopologyNodeDisplayMode NodeDisplayMode { get; set; } = TopologyNodeDisplayMode.CompactCard;

    /// <para>Theme name.</para>
    [Parameter]
    [ValidateSet("Light", "Dark")]
    public string Theme { get; set; } = "Dark";

    /// <para>Use an opaque topology canvas.</para>
    [Parameter]
    public SwitchParameter Opaque { get; set; }

    /// <para>Hide the topology title.</para>
    [Parameter]
    public SwitchParameter NoTitle { get; set; }

    /// <para>Show the topology legend.</para>
    [Parameter]
    public SwitchParameter ShowLegend { get; set; }

    /// <para>Hide group containers.</para>
    [Parameter]
    public SwitchParameter NoGroups { get; set; }

    /// <para>Hide edge labels.</para>
    [Parameter]
    public SwitchParameter NoEdgeLabels { get; set; }

    /// <summary>Emits a BGInfo topology overlay definition.</summary>
    protected override void EndProcessing() {
        var topology = new BgInfoTopology {
            Title = Title,
            Subtitle = Subtitle,
            Width = Width,
            Height = Height,
            Anchor = Anchor,
            OffsetX = OffsetX,
            OffsetY = OffsetY,
            Layout = Layout,
            Direction = Direction,
            NodeDisplayMode = NodeDisplayMode,
            Theme = Theme,
            Transparent = !Opaque.IsPresent,
            ShowTitle = !NoTitle.IsPresent,
            ShowLegend = ShowLegend.IsPresent,
            ShowGroups = !NoGroups.IsPresent,
            ShowEdgeLabels = !NoEdgeLabels.IsPresent
        };

        if (MyInvocation.BoundParameters.ContainsKey(nameof(PositionX)) &&
            MyInvocation.BoundParameters.ContainsKey(nameof(PositionY))) {
            topology.PositionX = PositionX;
            topology.PositionY = PositionY;
        }

        foreach (var item in TopologyDefinition.Invoke()) {
            var value = item is PSObject psObject ? psObject.BaseObject : item;
            if (value is TopologyGroup group) {
                topology.Groups.Add(group);
            } else if (value is TopologyNode node) {
                topology.Nodes.Add(node);
            } else if (value is TopologyEdge edge) {
                topology.Edges.Add(edge);
            }
        }

        WriteObject(topology);
    }
}
