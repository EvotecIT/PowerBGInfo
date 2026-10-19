using ChartForgeX.Primitives;

namespace PowerBGInfo.Tests;

public sealed class BgInfoImportAndLayoutBoundaryTests {
    [Theory]
    [InlineData(100, false, 200)]
    [InlineData(100, true, 200)]
    [InlineData(3, true, 200)]
    [InlineData(100, true, 1)]
    public void ShortSafeAreasRetainEveryRequestedVerticalRegion(int height, bool bottom, int topHeight) {
        var requests = new List<BgInfoLayoutRequest> { new(BgInfoRegion.Top, 0, topHeight), new(BgInfoRegion.Center, 0, 200) };
        if (bottom) requests.Add(new(BgInfoRegion.Bottom, 0, 200));
        var slots = BgInfoLayoutEngine.Resolve(640, height,
            new BgInfoLayout { MarginX = 0, MarginY = 0, TaskbarReserve = 0 }, null, requests);
        Assert.All(slots, slot => { Assert.True(slot.Height >= 1); Assert.InRange(slot.Y, 0, height - slot.Height); });
        Assert.True(slots[0].Y + slots[0].Height <= slots[1].Y);
        if (bottom) Assert.True(slots[1].Y + slots[1].Height <= slots[2].Y);
    }

    [Fact]
    public void NarrowSafeAreasRetainNonoverlappingSideAndCenterRegions() {
        var slots = BgInfoLayoutEngine.Resolve(100, 240,
            new BgInfoLayout { MarginX = 0, MarginY = 0, TaskbarReserve = 0 }, null,
            new[] { new BgInfoLayoutRequest(BgInfoRegion.Left, 0, 0), new(BgInfoRegion.Center, 0, 0), new(BgInfoRegion.Right, 0, 0) });
        Assert.All(slots, slot => { Assert.True(slot.Width >= 1); Assert.InRange(slot.X, 0, 100 - slot.Width); });
        Assert.True(slots[0].X + slots[0].Width <= slots[1].X);
        Assert.True(slots[1].X + slots[1].Width <= slots[2].X);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void FixedPanelCompressionReservesPixelsBeforeRoundingNeighbors(bool row) {
        var requests = row
            ? new[] { new BgInfoLayoutRequest(BgInfoRegion.Top, 1, 100), new(BgInfoRegion.Top, 2000, 100) }
            : new[] { new BgInfoLayoutRequest(BgInfoRegion.Top, 0, 200), new(BgInfoRegion.Center, 0, 20), new(BgInfoRegion.Center, 0, 2000) };
        var slots = BgInfoLayoutEngine.Resolve(row ? 50 : 640, 100,
            new BgInfoLayout { MarginX = 0, MarginY = 0, TaskbarReserve = 0, Gap = 0 }, null, requests);
        var first = slots[row ? 0 : 1];
        var next = slots[row ? 1 : 2];
        Assert.True(first.Width >= 1 && first.Height >= 1);
        Assert.True(next.Width >= 1 && next.Height >= 1);
        Assert.True(row ? first.X + first.Width <= next.X : first.Y + first.Height <= next.Y);
        Assert.True(next.X + next.Width <= (row ? 50 : 640) && next.Y + next.Height <= 100);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void TooFewPixelsDoNotProduceOverlappingInventedSlots(bool row) {
        var region = row ? BgInfoRegion.Top : BgInfoRegion.Center;
        var slots = BgInfoLayoutEngine.Resolve(row ? 1 : 640, row ? 100 : 1,
            new BgInfoLayout { MarginX = 0, MarginY = 0, TaskbarReserve = 0, Gap = 0 }, null,
            new[] { new BgInfoLayoutRequest(region, 0, 0), new(region, 0, 0) });
        Assert.All(slots, slot => Assert.True(slot.Width == 0 || slot.Height == 0));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void HalfPixelOriginsKeepAdjacentOnePixelPanelsDistinct(bool row) {
        var requests = row
            ? new[] { new BgInfoLayoutRequest(BgInfoRegion.Top, 1, 100), new(BgInfoRegion.Top, 1, 100), new(BgInfoRegion.Top, 1, 100) }
            : new[] { new BgInfoLayoutRequest(BgInfoRegion.Top, 0, 200), new(BgInfoRegion.Center, 0, 20), new(BgInfoRegion.Center, 0, 20), new(BgInfoRegion.Center, 0, 2000) };
        var slots = BgInfoLayoutEngine.Resolve(row ? 4 : 640, 101,
            new BgInfoLayout { MarginX = 0, MarginY = 0, TaskbarReserve = 0, Gap = 0 }, null, requests);
        Assert.All(slots, slot => Assert.True(slot.Width >= 1 && slot.Height >= 1));
        for (var i = 1; i < slots.Length; i++) {
            Assert.True(row ? slots[i - 1].X + slots[i - 1].Width <= slots[i].X : slots[i - 1].Y + slots[i - 1].Height <= slots[i].Y);
        }
        Assert.True(slots[^1].X + slots[^1].Width <= (row ? 4 : 640));
        Assert.True(slots[^1].Y + slots[^1].Height <= 101);
    }

    [Theory]
    [InlineData("Metric")]
    [InlineData("Table")]
    [InlineData("List")]
    [InlineData("CalendarHeatmap")]
    [InlineData("Timeline")]
    [InlineData("Treemap")]
    [InlineData("Mermaid")]
    [InlineData("QRCode")]
    public void PersistedWidgetDimensionsRespectThePublicPixelLimits(string kind) {
        var path = Path.Combine(Path.GetTempPath(), "bginfo-widget-limits-" + Path.GetRandomFileName() + ".json");
        File.WriteAllText(path, "{\"Widgets\":[{\"Kind\":\"" + kind + "\",\"Width\":2147483647,\"Height\":-5}]}");
        try {
            var widget = Assert.Single(BgInfoConfigurationJson.Load(path).Widgets);
            Assert.Equal(16384, widget.Width);
            Assert.Equal(0, widget.Height);
        } finally { File.Delete(path); }
    }

    [Theory]
    [InlineData(2147483647, 16384)]
    [InlineData(-5, 0)]
    public void PersistedOuterRasterSiblingsUseTheSameDimensionBounds(int input, int expected) {
        var path = Path.Combine(Path.GetTempPath(), "bginfo-raster-limits-" + Path.GetRandomFileName() + ".json");
        var size = new { Width = input, Height = input };
        File.WriteAllText(path, System.Text.Json.JsonSerializer.Serialize(new {
            Background = size, Charts = new[] { size }, Topologies = new[] { size }, VisualCanvases = new[] { size }, Images = new[] { size }
        }));
        try {
            var config = BgInfoConfigurationJson.Load(path);
            var dimensions = new[] { config.Background!.Width, config.Background.Height,
                config.Charts[0].Width, config.Charts[0].Height, config.Topologies[0].Width, config.Topologies[0].Height,
                config.VisualCanvases[0].Width, config.VisualCanvases[0].Height, config.Images[0].Width, config.Images[0].Height };
            Assert.All(dimensions, dimension => Assert.Equal(expected, dimension));
        } finally { File.Delete(path); }
    }

    [Theory]
    [InlineData(BgInfoTextPosition.TopLeft)]
    [InlineData(BgInfoTextPosition.MiddleLeft)]
    [InlineData(BgInfoTextPosition.BottomLeft)]
    public void ExcessIconReservationKeepsTheTextBlockVisible(BgInfoTextPosition anchor) {
        var directory = Path.Combine(Path.GetTempPath(), "bginfo-icon-limit-" + Path.GetRandomFileName());
        var config = new BgInfoConfiguration {
            Target = BgInfoTarget.File, ConfigurationDirectory = directory, OutputFileName = "icon.png",
            TextPosition = anchor, SpaceX = 12, SpaceY = 12, FontFamilyName = "Carlito", FontSize = 22,
            Color = ChartColors.White, Background = new BgInfoBackground { Width = 640, Height = 240, Color = ChartColors.Black },
            Layout = new BgInfoLayout { IconAreaWidth = 4096 }
        };
        config.Entries.Add(new BgInfoEntry { Type = BgInfoEntryType.Label, Name = "VISIBLE LABEL" });
        try {
            var wallpaper = new FakeWallpaperService();
            var path = new BgInfoGenerator(new ImageService(), wallpaper).Generate(config);
            using var image = BgInfoRasterImage.Load(path);
            var pixels = image.ToRgbaImage().Pixels;
            var bright = 0;
            for (var i = 0; i < pixels.Length; i += 4) if (pixels[i] > 200 && pixels[i + 1] > 200 && pixels[i + 2] > 200) bright++;
            Assert.True(bright > 40, "The authored text must remain painted within the canvas.");
            Assert.Equal(0, wallpaper.Calls);
            Assert.Equal(0, wallpaper.LogonCalls);
        } finally { if (Directory.Exists(directory)) Directory.Delete(directory, true); }
    }
}
