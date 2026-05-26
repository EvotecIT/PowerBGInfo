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
}
