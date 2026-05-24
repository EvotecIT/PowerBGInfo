using System.Collections.Generic;
using System.Drawing;

namespace PowerBGInfo;

/// <summary>Built-in visual canvas template.</summary>
public enum BgInfoVisualCanvasTemplate {
    /// <summary>PowerBGInfo-style hero wallpaper/social preview with side rails and central title.</summary>
    PowerBgInfoHero
}

/// <summary>Visual canvas side rail placement.</summary>
public enum BgInfoVisualCanvasSide {
    /// <summary>Place the tile on the left side rail.</summary>
    Left,
    /// <summary>Place the tile on the right side rail.</summary>
    Right
}

/// <summary>Visual canvas tile surface treatment.</summary>
public enum BgInfoVisualCanvasTileSurfaceStyle {
    /// <summary>Translucent filled panel.</summary>
    Glass,
    /// <summary>Border-only panel without a background fill.</summary>
    Outline
}

/// <summary>Built-in visual canvas tile icon.</summary>
public enum BgInfoVisualCanvasTileIconKind {
    /// <summary>Render the Icon text as the tile symbol.</summary>
    Text,
    /// <summary>Computer monitor icon.</summary>
    Computer,
    /// <summary>Network/globe icon.</summary>
    Network,
    /// <summary>Operating system/window icon.</summary>
    OperatingSystem,
    /// <summary>Processor icon.</summary>
    Cpu,
    /// <summary>Memory module icon.</summary>
    Memory,
    /// <summary>User icon.</summary>
    User,
    /// <summary>Domain/building icon.</summary>
    Domain,
    /// <summary>Terminal prompt icon.</summary>
    Terminal,
    /// <summary>Storage icon.</summary>
    Storage,
    /// <summary>Shield icon.</summary>
    Shield
}

/// <summary>Defines a reusable ChartForgeX visual canvas overlay.</summary>
public sealed class BgInfoVisualCanvas {
    /// <summary>Template used to build the canvas.</summary>
    public BgInfoVisualCanvasTemplate Template { get; set; } = BgInfoVisualCanvasTemplate.PowerBgInfoHero;
    /// <summary>Canvas title or brand text.</summary>
    public string Title { get; set; } = "PowerBGInfo";
    /// <summary>Canvas subtitle text.</summary>
    public string Subtitle { get; set; } = "Desktop background insights for Windows and PowerShell";
    /// <summary>Canvas width in pixels. A value of zero uses the target wallpaper width.</summary>
    public int Width { get; set; }
    /// <summary>Canvas height in pixels. A value of zero uses the target wallpaper height.</summary>
    public int Height { get; set; }
    /// <summary>Explicit X position on the generated wallpaper.</summary>
    public int PositionX { get; set; }
    /// <summary>Explicit Y position on the generated wallpaper.</summary>
    public int PositionY { get; set; }
    /// <summary>Top background color.</summary>
    public Color BackgroundTop { get; set; } = Color.FromArgb(255, 2, 7, 19);
    /// <summary>Bottom background color.</summary>
    public Color BackgroundBottom { get; set; } = Color.FromArgb(255, 7, 26, 53);
    /// <summary>Primary accent color.</summary>
    public Color Accent { get; set; } = Color.FromArgb(255, 47, 128, 255);
    /// <summary>Secondary accent color for badge and backdrop highlights.</summary>
    public Color? SecondaryAccent { get; set; }
    /// <summary>Primary hero title color.</summary>
    public Color? TitleColor { get; set; }
    /// <summary>Accent hero title color.</summary>
    public Color? TitleAccentColor { get; set; }
    /// <summary>Subtitle text color.</summary>
    public Color? SubtitleColor { get; set; }
    /// <summary>Glass tile top color.</summary>
    public Color? TileGlassTop { get; set; }
    /// <summary>Glass tile bottom color.</summary>
    public Color? TileGlassBottom { get; set; }
    /// <summary>Tile label text color.</summary>
    public Color? TileLabelColor { get; set; }
    /// <summary>Tile value text color.</summary>
    public Color? TileValueColor { get; set; }
    /// <summary>Tile detail text color.</summary>
    public Color? TileDetailColor { get; set; }
    /// <summary>Tile progress track color.</summary>
    public Color? TileProgressTrackColor { get; set; }
    /// <summary>Hero badge top fill color.</summary>
    public Color? HeroBadgeTop { get; set; }
    /// <summary>Hero badge bottom fill color.</summary>
    public Color? HeroBadgeBottom { get; set; }
    /// <summary>Hero badge symbol color.</summary>
    public Color? HeroBadgeTextColor { get; set; }
    /// <summary>Render only floating HUD layers without a full canvas background.</summary>
    public bool Transparent { get; set; } = true;
    /// <summary>Render ChartForgeX's built-in technology backdrop when the canvas is not transparent.</summary>
    public bool TechBackdrop { get; set; }
    /// <summary>Gets side rail tiles.</summary>
    public List<BgInfoVisualCanvasTile> Tiles { get; } = new();
    /// <summary>Gets bottom feature strip items.</summary>
    public List<BgInfoVisualCanvasFeature> Features { get; } = new();
}

/// <summary>Defines one visual canvas side-rail tile.</summary>
public sealed class BgInfoVisualCanvasTile {
    /// <summary>Side rail placement.</summary>
    public BgInfoVisualCanvasSide Side { get; set; }
    /// <summary>Compact tile icon or symbol.</summary>
    public string Icon { get; set; } = string.Empty;
    /// <summary>Tile label. Templates such as {{HostName}} are resolved at render time.</summary>
    public string Label { get; set; } = string.Empty;
    /// <summary>Tile value. Templates such as {{HostName}} are resolved at render time.</summary>
    public string Value { get; set; } = string.Empty;
    /// <summary>Optional detail text. Templates are resolved at render time.</summary>
    public string Detail { get; set; } = string.Empty;
    /// <summary>Optional tile accent color.</summary>
    public Color? Accent { get; set; }
    /// <summary>Optional progress value from zero to one.</summary>
    public double? Progress { get; set; }
    /// <summary>Tile surface style.</summary>
    public BgInfoVisualCanvasTileSurfaceStyle SurfaceStyle { get; set; }
    /// <summary>Tile icon kind.</summary>
    public BgInfoVisualCanvasTileIconKind IconKind { get; set; }
}

/// <summary>Defines one visual canvas feature-strip item.</summary>
public sealed class BgInfoVisualCanvasFeature {
    /// <summary>Compact item icon or symbol.</summary>
    public string Icon { get; set; } = string.Empty;
    /// <summary>Feature label.</summary>
    public string Label { get; set; } = string.Empty;
}
