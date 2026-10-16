using ChartForgeX.Topology;
using ChartForgeX.Typography;
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

    /// <para>Theme name. ThemeMode is accepted as an alias to match charts and visual canvases.</para>
    [Parameter]
    [Alias("ThemeMode")]
    [ValidateSet("Light", "Dark")]
    public string Theme { get; set; } = "Dark";

    /// <para>Whole-number render scale from 1 to 4. Use 2 to enlarge nodes and text on high-resolution wallpapers.</para>
    [Parameter]
    [ValidateRange(1, 4)]
    public int Scale { get; set; } = 1;

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

    /// <para>Panel fill. A fill or border draws the diagram in the same panel style as New-BGInfoChart, with a matching header.</para>
    [Parameter]
    public object? BackgroundColor { get; set; }

    /// <para>Optional one-pixel panel outline.</para>
    [Parameter]
    public object? BorderColor { get; set; }

    /// <para>Panel corner radius in pixels.</para>
    [Parameter]
    [ValidateRange(0, 200)]
    public double CornerRadius { get; set; }

    /// <para>Space between the panel edge and its content.</para>
    [Parameter]
    [ValidateRange(0, 200)]
    public int Padding { get; set; } = 20;

    /// <para>Panel header color.</para>
    [Parameter]
    public object? TitleColor { get; set; }

    /// <para>Panel header font size.</para>
    [Parameter]
    public float TitleFontSize { get; set; }

    /// <para>Panel header font weight from 100 through 900.</para>
    [Parameter]
    [ValidateRange(100, 900)]
    public int TitleFontWeight { get; set; } = 600;

    /// <para>Panel header casing.</para>
    [Parameter]
    public TextCaseTransform TitleTextCase { get; set; }

    /// <para>Region to flow into: Left, Right, Top, Bottom or Center. Without -Width or -Height the diagram shares the region.</para>
    [Parameter]
    public BgInfoRegion Region { get; set; }

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
            ShowEdgeLabels = !NoEdgeLabels.IsPresent,
            Scale = Scale,
            CornerRadius = CornerRadius,
            Padding = Padding
        };

        if (MyInvocation.BoundParameters.ContainsKey(nameof(BackgroundColor))) {
            topology.BackgroundColor = PowerShellColorConverter.ConvertRequired(BackgroundColor, nameof(BackgroundColor));
        }
        if (MyInvocation.BoundParameters.ContainsKey(nameof(BorderColor))) {
            topology.BorderColor = PowerShellColorConverter.ConvertRequired(BorderColor, nameof(BorderColor));
        }
        if (MyInvocation.BoundParameters.ContainsKey(nameof(TitleColor))) {
            topology.TitleColor = PowerShellColorConverter.ConvertRequired(TitleColor, nameof(TitleColor));
        }
        if (MyInvocation.BoundParameters.ContainsKey(nameof(TitleFontSize))) {
            topology.TitleFontSize = TitleFontSize;
        }
        if (MyInvocation.BoundParameters.ContainsKey(nameof(TitleFontWeight))) {
            topology.TitleFontWeight = TitleFontWeight;
        }
        if (MyInvocation.BoundParameters.ContainsKey(nameof(TitleTextCase))) {
            topology.TitleTextCase = TitleTextCase;
        }

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

        topology.Region = Region;
        if (Region != BgInfoRegion.None) {
            if (!IsBound(nameof(Width))) topology.Width = 0;
            if (!IsBound(nameof(Height))) topology.Height = 0;
        }
        ApplyTheme(topology);
        WriteObject(topology);
    }

    private bool IsBound(string name) => MyInvocation.BoundParameters.ContainsKey(name);

    /// <summary>Fills unbound panel settings from the New-BGInfo theme.</summary>
    private void ApplyTheme(BgInfoTopology topology) {
        var theme = BgInfoThemeScope.Current;
        if (theme == null) return;
        if (!IsBound(nameof(Theme))) topology.Theme = theme.Mode.ToString();
        if (!IsBound(nameof(BackgroundColor))) topology.BackgroundColor = theme.Surface;
        if (!IsBound(nameof(BorderColor))) topology.BorderColor = theme.SurfaceBorder;
        if (!IsBound(nameof(CornerRadius))) topology.CornerRadius = theme.CornerRadius;
        if (!IsBound(nameof(Padding))) topology.Padding = theme.Padding;
        if (!IsBound(nameof(TitleColor))) topology.TitleColor = theme.Muted;
        if (!IsBound(nameof(TitleFontSize))) topology.TitleFontSize = theme.TitleFontSize;
        if (!IsBound(nameof(TitleFontWeight))) topology.TitleFontWeight = theme.TitleFontWeight;
        if (!IsBound(nameof(TitleTextCase))) topology.TitleTextCase = theme.TitleTextCase;
    }
}
