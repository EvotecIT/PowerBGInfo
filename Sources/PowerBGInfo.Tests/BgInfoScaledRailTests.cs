namespace PowerBGInfo.Tests;

public sealed class BgInfoScaledRailTests {
    [Theory]
    [InlineData(2.5)]
    [InlineData(3)]
    [InlineData(4)]
    public void ImplicitScaledOpposingRailsLeaveTheCenterAtTheCanvasMidpoint(double scale) {
        var visual = new BgInfoVisualCanvas { Transparent = true, HeroContentVisible = false, TileTextScale = scale };
        visual.Tiles.Add(new BgInfoVisualCanvasTile { Side = BgInfoVisualCanvasSide.Left, Label = "Left", Value = "42" });
        visual.Tiles.Add(new BgInfoVisualCanvasTile { Side = BgInfoVisualCanvasSide.Right, Label = "Right", Value = "73" });

        var center = BgInfoVisualCanvasRenderer.ResolveDefaultTransparentCenterBounds(visual, 1920, 1080);

        Assert.InRange(center.X, 0, 960);
    }

    [Theory]
    [InlineData(3)]
    [InlineData(4)]
    public void ImplicitThreeRailOverlayKeepsTheMeasuredGapsTransparent(double scale) {
        var visual = new BgInfoVisualCanvas { Transparent = true, HeroContentVisible = false, TileTextScale = scale };
        foreach (var side in new[] { BgInfoVisualCanvasSide.Left, BgInfoVisualCanvasSide.Center, BgInfoVisualCanvasSide.Right }) {
            visual.Tiles.Add(new BgInfoVisualCanvasTile { Side = side, Label = side.ToString(), Value = "42" });
        }

        var center = BgInfoVisualCanvasRenderer.ResolveDefaultTransparentCenterBounds(visual, 1920, 1080);
        using var image = BgInfoVisualCanvasRenderer.Render(visual, new BgInfoConfiguration(), 1920, 1080);
        var pixels = image.ToRgbaImage().Pixels;
        var y = (int)center.Y + 100;
        byte Alpha(int x) => pixels[(y * 1920 + x) * 4 + 3];

        Assert.True(Alpha(300) > 0);
        Assert.True(Alpha(960) > 0);
        Assert.True(Alpha(1600) > 0);
        Assert.Equal((byte)0, Alpha((int)center.X - 24));
        Assert.Equal((byte)0, Alpha((int)(center.X + center.Width) + 24));
    }

    [Fact]
    public void ExplicitRailWidthRemainsAnAuthoredLayoutChoice() {
        var visual = new BgInfoVisualCanvas { Transparent = true, HeroContentVisible = false, TileTextScale = 3, LeftTileWidth = 1100 };
        visual.Tiles.Add(new BgInfoVisualCanvasTile { Side = BgInfoVisualCanvasSide.Left, Label = "Wide", Value = "42" });
        Assert.True(BgInfoVisualCanvasRenderer.ResolveDefaultTransparentCenterBounds(visual, 1920, 1080).X > 1100);
    }

    [Theory]
    [InlineData(BgInfoVisualCanvasSide.Left, false, false)]
    [InlineData(BgInfoVisualCanvasSide.Right, true, false)]
    [InlineData(BgInfoVisualCanvasSide.Center, false, true)]
    [InlineData(BgInfoVisualCanvasSide.Left, true, true)]
    public void FittingScaledTileStacksStayInsideTheOverlay(
        BgInfoVisualCanvasSide side, bool mixedScales, bool explicitGap) {
        var visual = new BgInfoVisualCanvas {
            Transparent = true,
            HeroContentVisible = false,
            TileTextScale = mixedScales ? 1 : 1.5,
            TileGap = explicitGap ? 20 : 0
        };
        var scales = new[] { 1d, 1.5, 1, 2, 1.5, 1.5 };
        for (var index = 0; index < scales.Length; index++) {
            visual.Tiles.Add(new BgInfoVisualCanvasTile {
                Side = side,
                Label = "Tile " + index,
                Value = "Fact " + index,
                TextScale = mixedScales ? scales[index] : null
            });
        }

        var requiredHeight = BgInfoVisualCanvasRenderer.MeasureRegionHeight(visual, 1080);
        Assert.InRange(requiredHeight, 1, 1080);
        var placement = BgInfoVisualCanvasRenderer.ResolveDefaultTransparentCenterBounds(visual, 1920, 1080);
        Assert.True(placement.Y + requiredHeight <= 1081,
            $"The fitting stack ends at {placement.Y + requiredHeight}, outside the 1080px canvas.");
    }
}
