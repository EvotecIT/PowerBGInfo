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
