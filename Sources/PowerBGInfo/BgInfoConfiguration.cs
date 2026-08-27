using System.Collections.Generic;
using Color = ChartForgeX.Primitives.ChartColor;
using ChartForgeX.Typography;
using DesktopManager;

namespace PowerBGInfo;

/// <summary>
/// Defines the configuration used to generate a BGInfo overlay image.
/// </summary>
public class BgInfoConfiguration {
    private int _fontWeight = 400;
    private int _valueFontWeight = 400;
    private TextDecorationStyle _underlineStyle;
    private TextDecorationStyle _strikethroughStyle;
    private TextBaseline _baseline;
    private TextCaseTransform _textCase;
    private TextDecorationStyle _valueUnderlineStyle;
    private TextDecorationStyle _valueStrikethroughStyle;
    private TextBaseline _valueBaseline;
    private TextCaseTransform _valueTextCase;
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
    /// Gets or sets whether labels use a bold font weight by default.
    /// </summary>
    public bool Bold {
        get => FontWeight >= 600;
        set => FontWeight = value ? 700 : 400;
    }
    /// <summary>
    /// Gets or sets the default numeric label font weight from 100 through 900.
    /// </summary>
    public int FontWeight {
        get => _fontWeight;
        set => _fontWeight = BgInfoTextStyleValidation.ValidateFontWeight(value, nameof(value));
    }
    /// <summary>
    /// Gets or sets whether labels use italic text by default.
    /// </summary>
    public bool Italic { get; set; }
    /// <summary>
    /// Gets or sets whether labels are underlined by default.
    /// </summary>
    public bool Underline {
        get => UnderlineStyle != TextDecorationStyle.None;
        set => UnderlineStyle = value ? TextDecorationStyle.Single : TextDecorationStyle.None;
    }
    /// <summary>
    /// Gets or sets the default label underline pattern.
    /// </summary>
    public TextDecorationStyle UnderlineStyle {
        get => _underlineStyle;
        set => _underlineStyle = BgInfoTextStyleValidation.ValidateEnum(value, nameof(value));
    }
    /// <summary>
    /// Gets or sets the default label strikethrough pattern.
    /// </summary>
    public TextDecorationStyle StrikethroughStyle {
        get => _strikethroughStyle;
        set => _strikethroughStyle = BgInfoTextStyleValidation.ValidateEnum(value, nameof(value));
    }
    /// <summary>
    /// Gets or sets the default label baseline placement.
    /// </summary>
    public TextBaseline Baseline {
        get => _baseline;
        set => _baseline = BgInfoTextStyleValidation.ValidateEnum(value, nameof(value));
    }
    /// <summary>
    /// Gets or sets the default display-time label casing transform.
    /// </summary>
    public TextCaseTransform TextCase {
        get => _textCase;
        set => _textCase = BgInfoTextStyleValidation.ValidateEnum(value, nameof(value));
    }
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
    /// Gets or sets whether values use a bold font weight by default.
    /// </summary>
    public bool ValueBold {
        get => ValueFontWeight >= 600;
        set => ValueFontWeight = value ? 700 : 400;
    }
    /// <summary>
    /// Gets or sets the default numeric value font weight from 100 through 900.
    /// </summary>
    public int ValueFontWeight {
        get => _valueFontWeight;
        set => _valueFontWeight = BgInfoTextStyleValidation.ValidateFontWeight(value, nameof(value));
    }
    /// <summary>
    /// Gets or sets whether values use italic text by default.
    /// </summary>
    public bool ValueItalic { get; set; }
    /// <summary>
    /// Gets or sets whether values are underlined by default.
    /// </summary>
    public bool ValueUnderline {
        get => ValueUnderlineStyle != TextDecorationStyle.None;
        set => ValueUnderlineStyle = value ? TextDecorationStyle.Single : TextDecorationStyle.None;
    }
    /// <summary>
    /// Gets or sets the default value underline pattern.
    /// </summary>
    public TextDecorationStyle ValueUnderlineStyle {
        get => _valueUnderlineStyle;
        set => _valueUnderlineStyle = BgInfoTextStyleValidation.ValidateEnum(value, nameof(value));
    }
    /// <summary>
    /// Gets or sets the default value strikethrough pattern.
    /// </summary>
    public TextDecorationStyle ValueStrikethroughStyle {
        get => _valueStrikethroughStyle;
        set => _valueStrikethroughStyle = BgInfoTextStyleValidation.ValidateEnum(value, nameof(value));
    }
    /// <summary>
    /// Gets or sets the default value baseline placement.
    /// </summary>
    public TextBaseline ValueBaseline {
        get => _valueBaseline;
        set => _valueBaseline = BgInfoTextStyleValidation.ValidateEnum(value, nameof(value));
    }
    /// <summary>
    /// Gets or sets the default display-time value casing transform.
    /// </summary>
    public TextCaseTransform ValueTextCase {
        get => _valueTextCase;
        set => _valueTextCase = BgInfoTextStyleValidation.ValidateEnum(value, nameof(value));
    }
    /// <summary>
    /// Gets or sets the maximum width used when wrapping value text. Set to 0 to disable wrapping.
    /// </summary>
    public int ValueWrapWidth { get; set; }
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
    /// Gets or sets a value indicating whether Windows wallpaper slideshows are preserved when no explicit base image is configured.
    /// </summary>
    public bool PreserveWallpaperSlideshow { get; set; } = true;
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
    /// Gets the collection of variables available to loop/template entries.
    /// </summary>
    public List<BgInfoVariable> Variables { get; } = new();
    /// <summary>
    /// Gets the collection of BGInfo entries to render.
    /// </summary>
    public List<BgInfoEntry> Entries { get; } = new();
    /// <summary>
    /// Gets the collection of charts to render.
    /// </summary>
    public List<BgInfoChart> Charts { get; } = new();
    /// <summary>
    /// Gets the collection of topology diagrams to render.
    /// </summary>
    public List<BgInfoTopology> Topologies { get; } = new();
    /// <summary>
    /// Gets the collection of ChartForgeX visual canvases to render.
    /// </summary>
    public List<BgInfoVisualCanvas> VisualCanvases { get; } = new();
    /// <summary>
    /// Gets the collection of image overlays to render.
    /// </summary>
    public List<BgInfoImage> Images { get; } = new();

}
