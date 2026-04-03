using System.Collections.Generic;
using System.Drawing;
using DesktopManager;

namespace PowerBGInfo;

/// <summary>
/// Defines the configuration used to generate a BGInfo overlay image.
/// </summary>
public class BgInfoConfiguration {
    /// <summary>
    /// Specifies how charts are positioned.
    /// </summary>
    public BgInfoChartLayoutMode ChartLayout { get; set; } = BgInfoChartLayoutMode.Manual;
    /// <summary>
    /// Anchor used when stacking charts.
    /// </summary>
    public BgInfoTextPosition ChartStackAnchor { get; set; } = BgInfoTextPosition.BottomLeft;
    /// <summary>
    /// Stack direction used when stacking charts.
    /// </summary>
    public BgInfoChartStackDirection ChartStackDirection { get; set; } = BgInfoChartStackDirection.Vertical;
    /// <summary>
    /// Spacing between stacked charts.
    /// </summary>
    public int ChartStackSpacing { get; set; } = 12;
    /// <summary>
    /// Horizontal offset for stacked charts.
    /// </summary>
    public int ChartStackOffsetX { get; set; } = 10;
    /// <summary>
    /// Vertical offset for stacked charts.
    /// </summary>
    public int ChartStackOffsetY { get; set; } = 10;
    /// <summary>
    /// When true, stack charts relative to the text block instead of the full image.
    /// </summary>
    public bool ChartStackAlignToTextBlock { get; set; }
    /// <summary>
    /// When true and aligned to the text block, place charts outside the text block.
    /// </summary>
    public bool ChartStackOutsideTextBlock { get; set; }
    /// <summary>
    /// Gets or sets the base image path. When empty, the current wallpaper for the monitor is used.
    /// </summary>
    public string FilePath { get; set; } = string.Empty;
    /// <summary>
    /// Gets or sets the output file name. When empty, a name is derived from the base image.
    /// </summary>
    public string OutputFileName { get; set; } = string.Empty;
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
    /// Gets or sets the background color to use when no wallpaper image is available.
    /// </summary>
    public Color? BackgroundColor { get; set; }
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
    /// Gets or sets a value indicating whether to refresh the wallpaper even when the output path is unchanged.
    /// </summary>
    public bool ForceWallpaperRefresh { get; set; } = true;
    /// <summary>
    /// Gets or sets a value indicating whether to apply the wallpaper to all user profiles.
    /// </summary>
    public bool ApplyToAllUsers { get; set; }
    /// <summary>
    /// Gets or sets a value indicating whether to update the default user profile when applying to all users.
    /// </summary>
    public bool IncludeDefaultUserProfile { get; set; } = true;
    /// <summary>
    /// Gets or sets a value indicating whether coordinates are calculated in screen space.
    /// </summary>
    public bool UseScreenCoordinates { get; set; }
    /// <summary>
    /// Gets the collection of BGInfo entries to render.
    /// </summary>
    public List<BgInfoEntry> Entries { get; } = new();
    /// <summary>
    /// Gets the collection of charts to render.
    /// </summary>
    public List<BgInfoChart> Charts { get; } = new();
}
