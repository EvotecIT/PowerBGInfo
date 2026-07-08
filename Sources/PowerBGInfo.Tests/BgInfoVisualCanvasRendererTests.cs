namespace PowerBGInfo.Tests;

public class BgInfoVisualCanvasRendererTests
{
    [Fact]
    public void DefaultOpaqueFeatureStripPlacementPreservesTemplateCenterAndBottomMargin()
    {
        var visual = new BgInfoVisualCanvas {
            FeatureWidth = 800,
            FeatureHeight = 100
        };

        var bounds = BgInfoVisualCanvasRenderer.ResolveDefaultOpaqueFeatureStripBounds(visual, 1200, 630, 1, 1);

        Assert.Equal(200, bounds.X);
        Assert.Equal(484, bounds.Y);
        Assert.Equal(800, bounds.Width);
        Assert.Equal(100, bounds.Height);
    }

    [Fact]
    public void RenderSkipsHeroBadgeWhenDisabled()
    {
        var visual = new BgInfoVisualCanvas {
            Width = 1200,
            Height = 630,
            Title = string.Empty,
            Subtitle = string.Empty,
            HeroBadgeVisible = false
        };

        using var image = BgInfoVisualCanvasRenderer.Render(visual, new BgInfoConfiguration(), 1200, 630);
        var pixels = image.ToRgbaImage();

        Assert.Equal(0, CountOpaquePixels(pixels, 538, 157, 124, 88));
    }

    [Fact]
    public void RenderUsesHeroBadgeImageWhenConfigured()
    {
        var logoPath = Path.Combine(Path.GetTempPath(), "powerbginfo-badge-" + Path.GetRandomFileName() + ".png");
        try
        {
            using (var logo = new BgInfoRasterImage())
            {
                logo.Create(logoPath, 12, 12, System.Drawing.Color.Red);
                logo.Save(logoPath);
            }

            var visual = new BgInfoVisualCanvas {
                Width = 1200,
                Height = 630,
                Title = string.Empty,
                Subtitle = string.Empty,
                HeroBadgeText = string.Empty,
                HeroBadgeImagePath = logoPath,
                HeroBadgeImageFit = BgInfoImageFit.Contain,
                HeroBadgeImagePadding = 8
            };

            using var image = BgInfoVisualCanvasRenderer.Render(visual, new BgInfoConfiguration(), 1200, 630);
            var pixels = image.ToRgbaImage();

            Assert.True(CountPixels(pixels, 538, 157, 124, 88, (r, g, b, a) => a > 0 && r > 200 && g < 40 && b < 40) > 1000);
        }
        finally
        {
            if (File.Exists(logoPath)) File.Delete(logoPath);
        }
    }

    [Fact]
    public void RenderUsesConfiguredVisualCanvasTileSize()
    {
        var visual = new BgInfoVisualCanvas {
            Width = 1000,
            Height = 520,
            Title = string.Empty,
            Subtitle = string.Empty,
            TileWidth = 360,
            TileHeight = 132,
            HeroBadgeVisible = false
        };
        visual.Tiles.Add(new BgInfoVisualCanvasTile {
            Side = BgInfoVisualCanvasSide.Left,
            IconKind = BgInfoVisualCanvasTileIconKind.Computer,
            SurfaceStyle = BgInfoVisualCanvasTileSurfaceStyle.Raised,
            Label = "OPERATING SYSTEM",
            Value = "Windows 11 Enterprise with compliance data",
            Detail = "cross-site monitoring window"
        });

        using var image = BgInfoVisualCanvasRenderer.Render(visual, new BgInfoConfiguration(), 1000, 520);
        var pixels = image.ToRgbaImage();

        Assert.True(CountOpaquePixels(pixels, 360, 88, 28, 72) > 0);
    }

    [Fact]
    public void RenderUsesVisualCanvasRailControls()
    {
        var visual = new BgInfoVisualCanvas {
            Width = 1000,
            Height = 520,
            Title = string.Empty,
            Subtitle = string.Empty,
            LayoutPreset = BgInfoVisualCanvasLayoutPreset.WideRails,
            TileHeight = 112,
            TileGap = 36,
            LeftTileWidth = 420,
            RightTileWidth = 310,
            LeftTileOffsetX = 40,
            LeftTileOffsetY = 20,
            RightTileOffsetX = 30,
            RightTileOffsetY = 12,
            TileTextFitPolicy = BgInfoVisualCanvasTileTextFitPolicy.WrapThenShrink,
            HeroBadgeVisible = false
        };
        visual.Tiles.Add(new BgInfoVisualCanvasTile {
            Side = BgInfoVisualCanvasSide.Left,
            IconKind = BgInfoVisualCanvasTileIconKind.OperatingSystem,
            SurfaceStyle = BgInfoVisualCanvasTileSurfaceStyle.Raised,
            Label = "OPERATING SYSTEM",
            Value = "Windows 11 Enterprise with a very long compliance value",
            Detail = "tenant status and policy drift"
        });
        visual.Tiles.Add(new BgInfoVisualCanvasTile {
            Side = BgInfoVisualCanvasSide.Right,
            IconKind = BgInfoVisualCanvasTileIconKind.Cpu,
            SurfaceStyle = BgInfoVisualCanvasTileSurfaceStyle.Raised,
            Label = "CPU",
            Value = "31% active",
            TextFitPolicy = BgInfoVisualCanvasTileTextFitPolicy.SingleLineEllipsis
        });

        using var image = BgInfoVisualCanvasRenderer.Render(visual, new BgInfoConfiguration(), 1000, 520);
        var pixels = image.ToRgbaImage();

        Assert.True(CountOpaquePixels(pixels, 450, 108, 24, 72) > 0);
        Assert.True(CountOpaquePixels(pixels, 640, 100, 24, 72) > 0);
    }

    [Fact]
    public void TransparentCenterBoundsAccountForRailOffsets()
    {
        var visual = new BgInfoVisualCanvas {
            LeftTileWidth = 300,
            RightTileWidth = 300,
            LeftTileOffsetX = 80,
            RightTileOffsetX = 64
        };

        var bounds = BgInfoVisualCanvasRenderer.ResolveDefaultTransparentCenterBounds(visual, 1000, 520);

        Assert.Equal(473, Math.Round(bounds.X));
        Assert.Equal(70, Math.Round(bounds.Width));
    }

    [Fact]
    public void PerTileWidthDoesNotResizeSiblingTiles()
    {
        var visual = new BgInfoVisualCanvas {
            Width = 1000,
            Height = 520,
            Title = string.Empty,
            Subtitle = string.Empty,
            HeroBadgeVisible = false
        };
        visual.Tiles.Add(new BgInfoVisualCanvasTile {
            Side = BgInfoVisualCanvasSide.Left,
            Width = 460,
            Label = "WIDE",
            Value = "wide tile",
            SurfaceStyle = BgInfoVisualCanvasTileSurfaceStyle.Raised
        });
        visual.Tiles.Add(new BgInfoVisualCanvasTile {
            Side = BgInfoVisualCanvasSide.Left,
            Label = "DEFAULT",
            Value = "default tile",
            SurfaceStyle = BgInfoVisualCanvasTileSurfaceStyle.Raised
        });

        using var image = BgInfoVisualCanvasRenderer.Render(visual, new BgInfoConfiguration(), 1000, 520);
        var pixels = image.ToRgbaImage();

        Assert.True(CountOpaquePixels(pixels, 430, 95, 24, 40) > 0);
        Assert.Equal(0, CountOpaquePixels(pixels, 430, 210, 24, 40));
    }

    private static int CountOpaquePixels(ChartForgeX.Raster.RgbaImage image, int x, int y, int width, int height)
    {
        return CountPixels(image, x, y, width, height, (_, _, _, a) => a > 0);
    }

    private static int CountPixels(ChartForgeX.Raster.RgbaImage image, int x, int y, int width, int height, Func<byte, byte, byte, byte, bool> predicate)
    {
        var count = 0;
        var right = Math.Min(image.Width, x + width);
        var bottom = Math.Min(image.Height, y + height);
        for (var row = Math.Max(0, y); row < bottom; row++)
        {
            var rowStart = row * image.Width * 4;
            for (var column = Math.Max(0, x); column < right; column++)
            {
                var index = rowStart + column * 4;
                if (predicate(image.Pixels[index], image.Pixels[index + 1], image.Pixels[index + 2], image.Pixels[index + 3]))
                {
                    count++;
                }
            }
        }

        return count;
    }
}
