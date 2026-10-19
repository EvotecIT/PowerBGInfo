namespace PowerBGInfo.Tests;

public sealed class BgInfoScaledRailTests {
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
