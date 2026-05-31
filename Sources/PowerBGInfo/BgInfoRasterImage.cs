using System;
using System.Drawing;
using ChartForgeX;
using ChartForgeX.Composition;
using ChartForgeX.Core;
using ChartForgeX.Primitives;
using ChartForgeX.Raster;

namespace PowerBGInfo;

/// <summary>
/// Dependency-free image surface used by PowerBGInfo rendering.
/// </summary>
public sealed class BgInfoRasterImage : IDisposable {
    private ImageComposition _composition = ImageComposition.CreateTransparent(1, 1);
    private readonly Dictionary<string, SizeF> _textSizeCache = new(StringComparer.Ordinal);

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
        _composition = ImageComposition.Create(width, height, ToChartColor(background));
    }

    /// <summary>Saves the image to disk using the output extension to choose the raster format.</summary>
    public void Save(string filePath) {
        if (filePath == null) throw new ArgumentNullException(nameof(filePath));
        _composition.Save(filePath, new RasterImageOptions { Background = ChartColors.Black, JpegQuality = 95 });
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
    public void DrawImage(BgInfoRasterImage image, float x, float y) {
        if (image == null) throw new ArgumentNullException(nameof(image));
        DrawImage(image.ToRgbaImage(), x, y, image.Width, image.Height);
    }

    /// <summary>Draws an RGBA image at its native size.</summary>
    public void DrawImage(RgbaImage image, float x, float y) => DrawImage(image, x, y, image.Width, image.Height);

    /// <summary>Draws an RGBA image into the destination rectangle.</summary>
    public void DrawImage(RgbaImage image, float x, float y, float width, float height, double opacity = 1d) {
        _composition.DrawImage(image, x, y, Math.Max(1f, width), Math.Max(1f, height), VisualCanvasImageFit.Stretch, opacity);
    }

    /// <summary>Draws text using the shared ChartForgeX raster text path.</summary>
    public void AddText(float x, float y, string text, Color color, float fontSize, string fontFamilyName) {
        if (string.IsNullOrEmpty(text) || color.A == 0) return;
        var size = GetTextSize(text, fontSize, fontFamilyName);
        _composition.DrawText(x, y, Math.Max(1, size.Width + 2), text, Math.Max(1, fontSize), ToChartColor(color));
    }

    /// <summary>Measures text for layout and wrapping.</summary>
    public SizeF GetTextSize(string? text, float fontSize, string fontFamilyName) {
        var normalized = text ?? string.Empty;
        if (normalized.Length == 0) return SizeF.Empty;
        var cacheKey = fontSize.ToString("R", System.Globalization.CultureInfo.InvariantCulture) + "\u001f" + (fontFamilyName ?? string.Empty) + "\u001f" + normalized;
        if (_textSizeCache.TryGetValue(cacheKey, out var cached)) return cached;

        var lines = normalized.Replace("\r\n", "\n").Replace('\r', '\n').Split('\n');
        var width = 0f;
        var height = 0f;
        var fallbackLineHeight = GetFallbackLineHeight(fontSize);
        foreach (var line in lines) {
            var lineSize = MeasureRenderedLine(line, fontSize);
            width = Math.Max(width, lineSize.Width);
            height += Math.Max(fallbackLineHeight, lineSize.Height);
        }

        var size = new SizeF(width, Math.Max(fallbackLineHeight, height));
        _textSizeCache[cacheKey] = size;
        return size;
    }

    /// <summary>Returns a copy of the current pixels.</summary>
    public RgbaImage ToRgbaImage() => _composition.ToImage();

    /// <inheritdoc />
    public void Dispose() {
    }

    private static float MeasureTextWidth(string text, float fontSize) {
        var width = 0f;
        foreach (var ch in text) {
            width += CharacterWidth(ch, fontSize);
        }

        return width;
    }

    private static float CharacterWidth(char ch, float fontSize) {
        if (char.IsWhiteSpace(ch)) return fontSize * 0.34f;
        if ("il.,'`:;!|".IndexOf(ch) >= 0) return fontSize * 0.28f;
        if ("mwMW@#%&".IndexOf(ch) >= 0) return fontSize * 0.88f;
        if (char.IsDigit(ch)) return fontSize * 0.56f;
        if (char.IsUpper(ch)) return fontSize * 0.64f;
        return fontSize * 0.54f;
    }

    private static SizeF MeasureRenderedLine(string text, float fontSize) {
        if (text.Length == 0) return new SizeF(0, GetFallbackLineHeight(fontSize));

        var fallbackWidth = Math.Max(1f, MeasureTextWidth(text, fontSize));
        var canvasWidth = Math.Max(16, (int)Math.Ceiling(Math.Max(fallbackWidth + fontSize * 4, text.Length * fontSize * 1.3f + 16)));
        var canvasHeight = Math.Max(16, (int)Math.Ceiling(fontSize * 3.2f));
        var probe = ImageComposition.CreateTransparent(canvasWidth, canvasHeight)
            .DrawText(0, 0, canvasWidth, text, Math.Max(1, fontSize), ChartColors.White);
        var image = probe.ToImage();

        var left = image.Width;
        var top = image.Height;
        var right = -1;
        var bottom = -1;
        for (var y = 0; y < image.Height; y++) {
            var row = y * image.Width * 4;
            for (var x = 0; x < image.Width; x++) {
                if (image.Pixels[row + x * 4 + 3] == 0) continue;
                if (x < left) left = x;
                if (x > right) right = x;
                if (y < top) top = y;
                if (y > bottom) bottom = y;
            }
        }

        if (right < left || bottom < top) return new SizeF(fallbackWidth, GetFallbackLineHeight(fontSize));
        return new SizeF(Math.Max(1, right - left + 1), Math.Max(1, bottom - top + 1));
    }

    private static float GetFallbackLineHeight(float fontSize) => Math.Max(1f, fontSize * 1.24f);

    private static ChartColor ToChartColor(Color color) => ChartColor.FromRgba(color.R, color.G, color.B, color.A);
}
