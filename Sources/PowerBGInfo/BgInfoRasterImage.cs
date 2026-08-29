using System;
using Color = ChartForgeX.Primitives.ChartColor;
using System.IO;
using ChartForgeX;
using ChartForgeX.Composition;
using ChartForgeX.Core;
using ChartForgeX.Primitives;
using ChartForgeX.Raster;
using ChartForgeX.Typography;

namespace PowerBGInfo;

/// <summary>
/// Dependency-free image surface used by PowerBGInfo rendering.
/// </summary>
public sealed class BgInfoRasterImage : IDisposable {
    private ImageComposition _composition = ImageComposition.CreateTransparent(1, 1);
    private readonly Dictionary<string, TextMetrics> _textSizeCache = new(StringComparer.Ordinal);

    /// <summary>Gets the image width in pixels.</summary>
    public int Width => _composition.Width;

    /// <summary>Gets the image height in pixels.</summary>
    public int Height => _composition.Height;

    /// <summary>Loads a raster image from disk.</summary>
    public static BgInfoRasterImage Load(string filePath) {
        if (filePath == null) throw new ArgumentNullException(nameof(filePath));
        return new BgInfoRasterImage {
            _composition = ImageComposition.FromFile(filePath)
        };
    }

    /// <summary>Creates a new image surface.</summary>
    public void Create(string filePath, int width, int height, Color background) {
        if (width <= 0) throw new ArgumentOutOfRangeException(nameof(width), width, "Image width must be positive.");
        if (height <= 0) throw new ArgumentOutOfRangeException(nameof(height), height, "Image height must be positive.");
        _composition = background.A == 0
            ? ImageComposition.CreateTransparent(width, height)
            : ImageComposition.Create(width, height, background);
    }

    /// <summary>Saves the image to disk using the output extension to choose the raster format.</summary>
    public void Save(string filePath) {
        if (filePath == null) throw new ArgumentNullException(nameof(filePath));
        var format = ResolveOutputFormat(filePath);
        File.WriteAllBytes(filePath, _composition.ToRasterImage(format, new RasterImageOptions { Background = ChartColors.Black, JpegQuality = 95 }));
    }

    /// <summary>Resizes the image in place.</summary>
    public void Resize(int width, int height) {
        if (width <= 0) throw new ArgumentOutOfRangeException(nameof(width), width, "Image width must be positive.");
        if (height <= 0) throw new ArgumentOutOfRangeException(nameof(height), height, "Image height must be positive.");
        var source = _composition.ToImage();
        _composition = ImageComposition.CreateTransparent(width, height)
            .DrawImage(source, 0, 0, width, height);
    }

    /// <summary>Draws another image at its native size.</summary>
    public void DrawImage(BgInfoRasterImage image, double x, double y) {
        if (image == null) throw new ArgumentNullException(nameof(image));
        DrawImage(image.ToRgbaImage(), x, y, image.Width, image.Height);
    }

    /// <summary>Draws an RGBA image at its native size.</summary>
    public void DrawImage(RgbaImage image, double x, double y) => DrawImage(image, x, y, image.Width, image.Height);

    /// <summary>Draws an RGBA image into the destination rectangle.</summary>
    public void DrawImage(RgbaImage image, double x, double y, double width, double height, double opacity = 1d) {
        DrawImage(image, x, y, width, height, opacity, VisualCanvasImageFit.Stretch);
    }

    /// <summary>Draws an RGBA image into the destination rectangle with a fit mode.</summary>
    public void DrawImage(RgbaImage image, double x, double y, double width, double height, double opacity, VisualCanvasImageFit fit) {
        _composition.DrawImage(image, x, y, Math.Max(1d, width), Math.Max(1d, height), fit, opacity);
    }

    /// <summary>Draws text using the shared ChartForgeX raster text path.</summary>
    public void AddText(double x, double y, string text, Color color, double fontSize, string fontFamilyName) {
        AddText(x, y, text, color, fontSize, fontFamilyName, false, false);
    }

    /// <summary>Draws text using the requested font traits through the shared ChartForgeX raster text path.</summary>
    public void AddText(double x, double y, string text, Color color, double fontSize, string fontFamilyName, bool bold, bool underline) {
        var style = CreateTextStyle(fontSize, fontFamilyName, color, bold ? 700 : 400, false, underline ? TextDecorationStyle.Single : TextDecorationStyle.None, TextDecorationStyle.None, TextBaseline.Normal, TextCaseTransform.None);
        AddText(x, y, text, style);
    }

    /// <summary>Draws text using a complete shared ChartForgeX text style.</summary>
    public void AddText(double x, double y, string text, TextStyle style) {
        if (style == null) throw new ArgumentNullException(nameof(style));
        if (string.IsNullOrEmpty(text) || style.Color.A == 0) return;
        var size = TextLayoutEngine.Measure(text, style);
        _composition.DrawText(x, y, Math.Max(1, size.Width + 2), text, style, TextWrapMode.NoWrap, null, TextTrimming.None);
    }

    /// <summary>Measures text for layout and wrapping.</summary>
    public TextMetrics GetTextSize(string? text, double fontSize, string fontFamilyName) {
        return GetTextSize(text, fontSize, fontFamilyName, false, false);
    }

    /// <summary>Measures styled text for layout and wrapping.</summary>
    public TextMetrics GetTextSize(string? text, double fontSize, string fontFamilyName, bool bold, bool underline) {
        var style = CreateTextStyle(fontSize, fontFamilyName, Color.Black, bold ? 700 : 400, false, underline ? TextDecorationStyle.Single : TextDecorationStyle.None, TextDecorationStyle.None, TextBaseline.Normal, TextCaseTransform.None);
        return GetTextSize(text, style);
    }

    /// <summary>Measures text using a complete shared ChartForgeX text style.</summary>
    public TextMetrics GetTextSize(string? text, TextStyle style) {
        if (style == null) throw new ArgumentNullException(nameof(style));
        var normalized = text ?? string.Empty;
        if (normalized.Length == 0) return new TextMetrics(0, 0, 0);
        var cacheKey = CreateTextCacheKey(normalized, style);
        if (_textSizeCache.TryGetValue(cacheKey, out var cached)) return cached;

        var size = TextLayoutEngine.Measure(normalized, style);
        _textSizeCache[cacheKey] = size;
        return size;
    }

    /// <summary>Returns a copy of the current pixels.</summary>
    public RgbaImage ToRgbaImage() => _composition.ToImage();

    /// <inheritdoc />
    public void Dispose() {
    }

    private static BgInfoRasterImage FromRgbaImage(RgbaImage image) => new() {
        _composition = ImageComposition.FromImage(image)
    };

    internal static TextStyle CreateTextStyle(double fontSize, string? fontFamilyName, Color color, int fontWeight, bool italic, TextDecorationStyle underlineStyle, TextDecorationStyle strikethroughStyle, TextBaseline baseline, TextCaseTransform textCase) {
        var style = TextStyle.Create(Math.Max(1, fontSize), color);
        style.Font = FontSpec.FromFamily(string.IsNullOrWhiteSpace(fontFamilyName) ? "Segoe UI, Arial, sans-serif" : fontFamilyName!);
        style.Font.Weight = fontWeight;
        style.Font.Italic = italic;
        style.UnderlineStyle = underlineStyle;
        style.StrikethroughStyle = strikethroughStyle;
        style.Baseline = baseline;
        style.TextCase = textCase;
        return style;
    }

    private static string CreateTextCacheKey(string text, TextStyle style) {
        var culture = System.Globalization.CultureInfo.InvariantCulture;
        return System.Globalization.CultureInfo.CurrentCulture.Name + "\u001f" +
               style.FontSize.ToString("R", culture) + "\u001f" +
               style.LineHeight.ToString("R", culture) + "\u001f" +
               style.Font.Family + "\u001f" + (style.Font.FilePath ?? string.Empty) + "\u001f" +
               (style.Font.CollectionIndex?.ToString(culture) ?? string.Empty) + "\u001f" + (style.Font.FaceName ?? string.Empty) + "\u001f" +
               style.Font.Weight.ToString(culture) + "\u001f" + (style.Font.Italic ? "1" : "0") + "\u001f" +
               ((int)style.UnderlineStyle).ToString(culture) + "\u001f" + ((int)style.StrikethroughStyle).ToString(culture) + "\u001f" +
               ((int)style.Baseline).ToString(culture) + "\u001f" + ((int)style.TextCase).ToString(culture) + "\u001f" + text;
    }

    private static RasterImageFormat ResolveOutputFormat(string filePath) {
        var extension = Path.GetExtension(filePath);
        if (extension.Equals(".jpe", StringComparison.OrdinalIgnoreCase) ||
            extension.Equals(".jfif", StringComparison.OrdinalIgnoreCase)) {
            return RasterImageFormat.Jpeg;
        }

        if (extension.Equals(".pnm", StringComparison.OrdinalIgnoreCase)) {
            return RasterImageFormat.Ppm;
        }

        return RasterImageFormatExtensions.FromFileExtension(filePath);
    }
}
