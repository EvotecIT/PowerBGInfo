using System.Collections.Generic;
using Color = ChartForgeX.Primitives.ChartColor;

namespace PowerBGInfo;

/// <summary>
/// Describes a generated wallpaper backdrop that replaces the current wallpaper or <see cref="BgInfoConfiguration.FilePath"/>.
/// </summary>
/// <remarks>
/// Layers are painted in a fixed order: base color, optional image, optional linear gradient,
/// glows and finally the pattern. A translucent gradient over an image therefore acts as a scrim.
/// </remarks>
public sealed class BgInfoBackground {
    /// <summary>Output width in pixels. Zero uses the target monitor width.</summary>
    public int Width { get; set; }
    /// <summary>Output height in pixels. Zero uses the target monitor height.</summary>
    public int Height { get; set; }
    /// <summary>Base fill painted before any other layer.</summary>
    public Color Color { get; set; } = Color.Black;
    /// <summary>Optional image painted over the base fill.</summary>
    public string? ImagePath { get; set; }
    /// <summary>How the image fills the backdrop.</summary>
    public BgInfoImageFit ImageFit { get; set; } = BgInfoImageFit.Cover;
    /// <summary>Image opacity from zero to one.</summary>
    public double ImageOpacity { get; set; } = 1d;
    /// <summary>Gradient start color. The gradient is drawn only when both ends are set.</summary>
    public Color? GradientStart { get; set; }
    /// <summary>Gradient end color. The gradient is drawn only when both ends are set.</summary>
    public Color? GradientEnd { get; set; }
    /// <summary>Gradient direction in degrees, measured like a CSS gradient line from the start color.</summary>
    public double GradientAngle { get; set; } = 135d;
    /// <summary>Soft radial lights painted over the gradient.</summary>
    public List<BgInfoBackgroundGlow> Glows { get; } = new();
    /// <summary>Optional repeating pattern painted last.</summary>
    public BgInfoBackgroundPattern Pattern { get; set; }
    /// <summary>Pattern ink. Use a low alpha so the pattern stays behind the content.</summary>
    public Color PatternColor { get; set; } = Color.FromRgba(255, 255, 255, 20);
    /// <summary>Distance between pattern dots or lines in pixels.</summary>
    public int PatternSpacing { get; set; } = 48;
}

/// <summary>Describes a soft radial light on a generated background.</summary>
public sealed class BgInfoBackgroundGlow {
    /// <summary>Horizontal center as a fraction of the backdrop width (0 is left, 1 is right).</summary>
    public double X { get; set; } = 0.5d;
    /// <summary>Vertical center as a fraction of the backdrop height (0 is top, 1 is bottom).</summary>
    public double Y { get; set; } = 0.5d;
    /// <summary>Radius as a fraction of the longest backdrop side.</summary>
    public double Radius { get; set; } = 0.5d;
    /// <summary>Glow color at the center. It fades to transparent at the radius.</summary>
    public Color Color { get; set; } = Color.FromRgba(255, 255, 255, 64);
}

/// <summary>Repeating patterns available for generated backgrounds.</summary>
public enum BgInfoBackgroundPattern {
    /// <summary>No pattern.</summary>
    None,
    /// <summary>A regular grid of small dots.</summary>
    Dots,
    /// <summary>Thin horizontal and vertical grid lines.</summary>
    Grid
}
