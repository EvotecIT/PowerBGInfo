using System.Collections.Generic;
using SixLabors.ImageSharp;
using DesktopManager;

namespace PowerBGInfo;

/// <summary>
/// Defines the configuration used to generate a BGInfo overlay image.
/// </summary>
public class BgInfoConfiguration {
    /// <summary>
    /// Gets or sets the base image path. When empty, the current wallpaper for the monitor is used.
    /// </summary>
    public string FilePath { get; set; } = string.Empty;
    /// <summary>
    /// Gets or sets the output directory for generated BGInfo images.
    /// </summary>
    public string ConfigurationDirectory { get; set; } = string.Empty;
    /// <summary>
    /// Gets or sets the default label font family.
    /// </summary>
    public string FontFamilyName { get; set; } = "Calibri";
    /// <summary>
    /// Gets or sets the default label color.
    /// </summary>
    public Color Color { get; set; } = Color.Black;
    /// <summary>
    /// Gets or sets the default label font size.
    /// </summary>
    public float FontSize { get; set; } = 16f;
    /// <summary>
    /// Gets or sets the default value color.
    /// </summary>
    public Color ValueColor { get; set; } = Color.Black;
    /// <summary>
    /// Gets or sets the default value font size.
    /// </summary>
    public float ValueFontSize { get; set; } = 16f;
    /// <summary>
    /// Gets or sets the default value font family.
    /// </summary>
    public string ValueFontFamilyName { get; set; } = "Calibri";
    /// <summary>
    /// Gets or sets the vertical spacing between rows.
    /// </summary>
    public int SpaceBetweenLines { get; set; } = 10;
    /// <summary>
    /// Gets or sets the spacing between label and value columns.
    /// </summary>
    public int SpaceBetweenColumns { get; set; } = 30;
    /// <summary>
    /// Gets or sets the X position placeholder (reserved for future layout strategies).
    /// </summary>
    public float PositionX { get; set; } = 10;
    /// <summary>
    /// Gets or sets the Y position placeholder (reserved for future layout strategies).
    /// </summary>
    public float PositionY { get; set; } = 10;
    /// <summary>
    /// Gets or sets the target monitor index for wallpaper operations.
    /// </summary>
    public int MonitorIndex { get; set; }
    /// <summary>
    /// Gets or sets the X padding used for layout positioning.
    /// </summary>
    public int SpaceX { get; set; } = 10;
    /// <summary>
    /// Gets or sets the Y padding used for layout positioning.
    /// </summary>
    public int SpaceY { get; set; } = 10;
    /// <summary>
    /// Gets or sets the wallpaper fit mode applied after generation.
    /// </summary>
    public DesktopWallpaperPosition WallpaperFit { get; set; } = DesktopWallpaperPosition.Center;
    /// <summary>
    /// Gets or sets the layout anchor position.
    /// </summary>
    public BgInfoTextPosition TextPosition { get; set; } = BgInfoTextPosition.TopLeft;
    /// <summary>
    /// Gets or sets the output target.
    /// </summary>
    public BgInfoTarget Target { get; set; } = BgInfoTarget.Wallpaper;
    /// <summary>
    /// Gets or sets a value indicating whether coordinates are calculated in screen space.
    /// </summary>
    public bool UseScreenCoordinates { get; set; }
    /// <summary>
    /// Gets the collection of BGInfo entries to render.
    /// </summary>
    public List<BgInfoEntry> Entries { get; } = new();
}
