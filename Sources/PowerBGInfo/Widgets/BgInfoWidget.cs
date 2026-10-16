using System;
using ChartForgeX.Themes;
using ChartForgeX.Typography;
using Color = ChartForgeX.Primitives.ChartColor;

namespace PowerBGInfo;

/// <summary>Kinds of widget panels.</summary>
public enum BgInfoWidgetKind {
    /// <summary>One KPI with trend, status and optional history.</summary>
    Metric,
    /// <summary>Rows and columns.</summary>
    Table,
    /// <summary>Bullets, numbers, checklist or status list.</summary>
    List,
    /// <summary>Dated values on a week grid.</summary>
    CalendarHeatmap,
    /// <summary>Time ranges on named lanes.</summary>
    Timeline,
    /// <summary>Sizes as nested rectangles.</summary>
    Treemap,
    /// <summary>A diagram described in Mermaid text.</summary>
    Mermaid,
    /// <summary>A scannable QR code.</summary>
    QrCode
}

/// <summary>
/// Base for widget panels: a framed surface with an optional header and a ChartForgeX body.
/// </summary>
public abstract class BgInfoWidget {
    private double _scale = 1d;
    private VisualThemeMode _themeMode = VisualThemeMode.Dark;

    /// <summary>Widget kind.</summary>
    public abstract BgInfoWidgetKind Kind { get; }
    /// <summary>Optional identifier.</summary>
    public string Id { get; set; } = string.Empty;
    /// <summary>Panel header text.</summary>
    public string Title { get; set; } = string.Empty;
    /// <summary>Short right-aligned header note, for example a time range.</summary>
    public string Note { get; set; } = string.Empty;
    /// <summary>Width in pixels. Zero fills the region width.</summary>
    public int Width { get; set; } = 360;
    /// <summary>Height in pixels. Zero shares the region height.</summary>
    public int Height { get; set; } = 220;
    /// <summary>Region the widget flows into. <see cref="BgInfoRegion.None"/> uses the anchor.</summary>
    public BgInfoRegion Region { get; set; }
    /// <summary>Position in the region's flow. New-BGInfo numbers items in the order they are emitted; equal values keep chart, diagram, canvas, widget order.</summary>
    public int RegionOrder { get; set; }
    /// <summary>Anchor used without a region.</summary>
    public BgInfoTextPosition Anchor { get; set; } = BgInfoTextPosition.TopRight;
    /// <summary>Horizontal offset from the anchor.</summary>
    public int OffsetX { get; set; } = 32;
    /// <summary>Vertical offset from the anchor.</summary>
    public int OffsetY { get; set; } = 32;
    /// <summary>Explicit X position.</summary>
    public float? PositionX { get; set; }
    /// <summary>Explicit Y position.</summary>
    public float? PositionY { get; set; }
    /// <summary>Light or dark content.</summary>
    public VisualThemeMode ThemeMode {
        get => _themeMode;
        set {
            if (!Enum.IsDefined(typeof(VisualThemeMode), value)) throw new ArgumentOutOfRangeException(nameof(value), value, "Unknown theme mode.");
            _themeMode = value;
        }
    }
    /// <summary>Panel fill. Unset leaves the panel transparent.</summary>
    public Color? BackgroundColor { get; set; }
    /// <summary>Panel outline.</summary>
    public Color? BorderColor { get; set; }
    /// <summary>Panel corner radius.</summary>
    public double CornerRadius { get; set; }
    /// <summary>Space between the panel edge and its content.</summary>
    public int Padding { get; set; } = 20;
    /// <summary>Header color.</summary>
    public Color? TitleColor { get; set; }
    /// <summary>Header font size.</summary>
    public float? TitleFontSize { get; set; }
    /// <summary>Header font weight.</summary>
    public int? TitleFontWeight { get; set; }
    /// <summary>Header casing.</summary>
    public TextCaseTransform? TitleTextCase { get; set; }
    /// <summary>Primary content color.</summary>
    public Color? TextColor { get; set; }
    /// <summary>Secondary content color.</summary>
    public Color? MutedColor { get; set; }
    /// <summary>Accent for series and highlights.</summary>
    public Color? Accent { get; set; }
    /// <summary>Second accent for contrasting series.</summary>
    public Color? SecondaryAccent { get; set; }
    /// <summary>Font family for content and header.</summary>
    public string? FontFamilyName { get; set; }
    /// <summary>Status colors. Unset uses the defaults for <see cref="ThemeMode"/>.</summary>
    public BgInfoStatusColors? StatusColors { get; set; }
    /// <summary>Content density from 1 to 4. Larger values enlarge text and marks inside the same panel.</summary>
    public double Scale {
        get => _scale;
        set {
            if (double.IsNaN(value) || double.IsInfinity(value) || value < 1 || value > 4) throw new ArgumentOutOfRangeException(nameof(value), value, "Widget scale must be between 1 and 4.");
            _scale = value;
        }
    }

    /// <summary>Height of the body content in logical pixels when it has a natural size, or zero when the widget fills any height.</summary>
    /// <remarks>Region layout uses it for unsized widgets in columns, so a short table does not stretch to half the screen.</remarks>
    internal virtual double NaturalBodyHeight => 0;

    /// <summary>Resolved status colors.</summary>
    public BgInfoStatusColors ResolveStatusColors() => StatusColors ?? BgInfoStatusColors.Default(ThemeMode);

    /// <summary>Checks the widget data before rendering.</summary>
    public virtual void Validate() {
        if (Width < 0) throw new ArgumentOutOfRangeException(nameof(Width), Width, "Widget width cannot be negative.");
        if (Height < 0) throw new ArgumentOutOfRangeException(nameof(Height), Height, "Widget height cannot be negative.");
        if (Padding < 0) throw new ArgumentOutOfRangeException(nameof(Padding), Padding, "Widget padding cannot be negative.");
    }
}
