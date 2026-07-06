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

    private static int CountOpaquePixels(ChartForgeX.Raster.RgbaImage image, int x, int y, int width, int height)
    {
        var count = 0;
        var right = Math.Min(image.Width, x + width);
        var bottom = Math.Min(image.Height, y + height);
        for (var row = Math.Max(0, y); row < bottom; row++)
        {
            var rowStart = row * image.Width * 4;
            for (var column = Math.Max(0, x); column < right; column++)
            {
                if (image.Pixels[rowStart + column * 4 + 3] > 0)
                {
                    count++;
                }
            }
        }

        return count;
    }
}
