using System.Collections.Generic;
using Color = ChartForgeX.Primitives.ChartColor;

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
    Outline,
    /// <summary>Raised panel with stronger depth and edge highlights.</summary>
    Raised
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

/// <summary>Compact chart treatment for visual canvas tiles.</summary>
public enum BgInfoVisualCanvasTileMiniChartKind {
    /// <summary>Render no mini chart.</summary>
    None,
    /// <summary>Render a compact sparkline inside the tile.</summary>
    Sparkline,
    /// <summary>Render a compact area sparkline inside the tile.</summary>
    Area,
    /// <summary>Render compact bars inside the tile.</summary>
    Bars
}

/// <summary>Built-in sizing density for visual canvas side rails.</summary>
public enum BgInfoVisualCanvasLayoutPreset {
    /// <summary>Use the default responsive PowerBGInfo template sizing.</summary>
    Default,
    /// <summary>Use smaller tiles and tighter vertical spacing.</summary>
    Compact,
    /// <summary>Use larger tiles and more breathing room.</summary>
    Comfortable,
    /// <summary>Favor wider side rails for longer values.</summary>
    WideRails,
    /// <summary>Favor fitting more tiles vertically.</summary>
    Dense
}

/// <summary>Text fitting policy for visual canvas tiles.</summary>
public enum BgInfoVisualCanvasTileTextFitPolicy {
    /// <summary>Use the balanced ChartForgeX default.</summary>
    Auto,
    /// <summary>Keep each text role on one line and trim overflowing text.</summary>
    SingleLineEllipsis,
    /// <summary>Wrap text across available tile lines.</summary>
    Wrap,
    /// <summary>Shrink one-line text before trimming.</summary>
    ShrinkToFit,
    /// <summary>Wrap first, then shrink before trimming.</summary>
    WrapThenShrink
}

/// <summary>Defines a reusable ChartForgeX visual canvas overlay.</summary>
public sealed class BgInfoVisualCanvas {
    /// <summary>Template used to build the canvas.</summary>
    public BgInfoVisualCanvasTemplate Template { get; set; } = BgInfoVisualCanvasTemplate.PowerBgInfoHero;
    /// <summary>Responsive side-rail sizing preset.</summary>
    public BgInfoVisualCanvasLayoutPreset LayoutPreset { get; set; }
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
    /// <summary>Render the central hero badge.</summary>
    public bool HeroBadgeVisible { get; set; } = true;
    /// <summary>Text rendered in the central hero badge when no image is configured.</summary>
    public string HeroBadgeText { get; set; } = ">_";
    /// <summary>Optional image path rendered inside the central hero badge.</summary>
    public string HeroBadgeImagePath { get; set; } = string.Empty;
    /// <summary>How the hero badge image is fitted inside the badge.</summary>
    public BgInfoImageFit HeroBadgeImageFit { get; set; } = BgInfoImageFit.Contain;
    /// <summary>Padding inside the hero badge image area.</summary>
    public int HeroBadgeImagePadding { get; set; } = 10;
    /// <summary>Hero badge image opacity from zero to one.</summary>
    public double HeroBadgeImageOpacity { get; set; } = 1d;
    /// <summary>Optional feature-strip anchor. Null uses the template's default centered placement.</summary>
    public BgInfoTextPosition? FeatureAnchor { get; set; }
    /// <summary>Optional feature-strip width in pixels. Zero uses the template default width.</summary>
    public int FeatureWidth { get; set; }
    /// <summary>Optional feature-strip height in pixels. Zero uses the template default height.</summary>
    public int FeatureHeight { get; set; }
    /// <summary>Default tile width in pixels. Zero uses the template default width.</summary>
    public int TileWidth { get; set; }
    /// <summary>Default tile height in pixels. Zero uses the template default height.</summary>
    public int TileHeight { get; set; }
    /// <summary>Default vertical gap between tiles in pixels. Zero uses the template default gap.</summary>
    public int TileGap { get; set; }
    /// <summary>Default left side-rail tile width in pixels. Zero uses TileWidth or the template default.</summary>
    public int LeftTileWidth { get; set; }
    /// <summary>Default right side-rail tile width in pixels. Zero uses TileWidth or the template default.</summary>
    public int RightTileWidth { get; set; }
    /// <summary>Horizontal left side-rail offset in pixels.</summary>
    public int LeftTileOffsetX { get; set; }
    /// <summary>Vertical left side-rail offset in pixels.</summary>
    public int LeftTileOffsetY { get; set; }
    /// <summary>Horizontal right side-rail inset in pixels.</summary>
    public int RightTileOffsetX { get; set; }
    /// <summary>Vertical right side-rail offset in pixels.</summary>
    public int RightTileOffsetY { get; set; }
    /// <summary>Default tile text fitting policy.</summary>
    public BgInfoVisualCanvasTileTextFitPolicy TileTextFitPolicy { get; set; }
    /// <summary>Horizontal feature-strip offset. For right anchors, positive values inset from the right edge.</summary>
    public int FeatureOffsetX { get; set; }
    /// <summary>Vertical feature-strip offset. For bottom anchors, positive values inset from the bottom edge.</summary>
    public int FeatureOffsetY { get; set; }
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
    /// <summary>Optional tile width in pixels. Zero uses the visual canvas default or template default.</summary>
    public int Width { get; set; }
    /// <summary>Optional tile height in pixels. Zero uses the visual canvas default or template default.</summary>
    public int Height { get; set; }
    /// <summary>Optional tile accent color.</summary>
    public Color? Accent { get; set; }
    /// <summary>Optional progress value from zero to one.</summary>
    public double? Progress { get; set; }
    /// <summary>Tile surface style.</summary>
    public BgInfoVisualCanvasTileSurfaceStyle SurfaceStyle { get; set; }
    /// <summary>Tile icon kind.</summary>
    public BgInfoVisualCanvasTileIconKind IconKind { get; set; }
    /// <summary>Compact tile chart kind.</summary>
    public BgInfoVisualCanvasTileMiniChartKind MiniChartKind { get; set; }
    /// <summary>Tile-specific text fitting policy. Auto inherits the visual canvas setting.</summary>
    public BgInfoVisualCanvasTileTextFitPolicy TextFitPolicy { get; set; }
    /// <summary>Compact tile chart values.</summary>
    public IReadOnlyList<double> MiniChartValues { get; set; } = System.Array.Empty<double>();
    /// <summary>Optional compact tile chart maximum.</summary>
    public double? MiniChartMaximum { get; set; }
}

/// <summary>Defines one visual canvas feature-strip item.</summary>
public sealed class BgInfoVisualCanvasFeature {
    /// <summary>Compact item icon or symbol.</summary>
    public string Icon { get; set; } = string.Empty;
    /// <summary>Feature label.</summary>
    public string Label { get; set; } = string.Empty;
}
