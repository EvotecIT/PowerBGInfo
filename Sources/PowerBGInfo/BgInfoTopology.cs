using System.Collections.Generic;
using ChartForgeX.Topology;

namespace PowerBGInfo;

/// <summary>Defines a topology diagram block rendered onto the BGInfo output.</summary>
public sealed class BgInfoTopology {
    /// <summary>Topology title.</summary>
    public string Title { get; set; } = string.Empty;
    /// <summary>Optional topology subtitle.</summary>
    public string Subtitle { get; set; } = string.Empty;
    /// <summary>Topology width in pixels.</summary>
    public int Width { get; set; } = 520;
    /// <summary>Topology height in pixels.</summary>
    public int Height { get; set; } = 300;
    /// <summary>Anchor position used for placement.</summary>
    public BgInfoTextPosition Anchor { get; set; } = BgInfoTextPosition.BottomRight;
    /// <summary>Horizontal offset from the anchor.</summary>
    public int OffsetX { get; set; } = 32;
    /// <summary>Vertical offset from the anchor.</summary>
    public int OffsetY { get; set; } = 32;
    /// <summary>Explicit X position override.</summary>
    public float? PositionX { get; set; }
    /// <summary>Explicit Y position override.</summary>
    public float? PositionY { get; set; }
    /// <summary>Topology layout mode.</summary>
    public TopologyLayoutMode Layout { get; set; } = TopologyLayoutMode.Layered;
    /// <summary>Topology layout direction.</summary>
    public TopologyLayoutDirection Direction { get; set; } = TopologyLayoutDirection.LeftToRight;
    /// <summary>Node presentation mode.</summary>
    public TopologyNodeDisplayMode NodeDisplayMode { get; set; } = TopologyNodeDisplayMode.CompactCard;
    /// <summary>Reusable visual style.</summary>
    public TopologyVisualStyle VisualStyle { get; set; } = TopologyVisualStyle.MonitoringDashboard;
    /// <summary>Canvas surface style.</summary>
    public TopologyCanvasSurfaceStyle CanvasSurfaceStyle { get; set; } = TopologyCanvasSurfaceStyle.Plain;
    /// <summary>Theme name.</summary>
    public string Theme { get; set; } = "Dark";
    /// <summary>Whether to render a transparent canvas.</summary>
    public bool Transparent { get; set; } = true;
    /// <summary>Whether to show the topology title.</summary>
    public bool ShowTitle { get; set; } = true;
    /// <summary>Whether to show the topology legend.</summary>
    public bool ShowLegend { get; set; }
    /// <summary>Whether to show group containers.</summary>
    public bool ShowGroups { get; set; } = true;
    /// <summary>Whether to show edge labels.</summary>
    public bool ShowEdgeLabels { get; set; } = true;
    /// <summary>Whether to show status badges.</summary>
    public bool ShowStatusBadges { get; set; } = true;
    /// <summary>Whether to fit content into the requested viewport.</summary>
    public bool FitContentToViewport { get; set; } = true;
    /// <summary>Topology groups.</summary>
    public List<TopologyGroup> Groups { get; } = new();
    /// <summary>Topology nodes.</summary>
    public List<TopologyNode> Nodes { get; } = new();
    /// <summary>Topology edges.</summary>
    public List<TopologyEdge> Edges { get; } = new();
}
