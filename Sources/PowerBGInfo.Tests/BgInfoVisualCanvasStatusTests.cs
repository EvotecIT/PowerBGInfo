using ChartForgeX.Primitives;
using ChartForgeX.Themes;

namespace PowerBGInfo.Tests;

public sealed class BgInfoVisualCanvasStatusTests {
    [Theory]
    [InlineData(VisualThemeMode.Light, false)]
    [InlineData(VisualThemeMode.Dark, false)]
    [InlineData(VisualThemeMode.Light, true)]
    [InlineData(VisualThemeMode.Dark, true)]
    public void TileStatusUsesTheOwningCanvasPaletteAndExplicitAccentStillWins(VisualThemeMode mode, bool region) {
        var tile = new BgInfoVisualCanvasTile { Label = "DISK", Value = "91%", Status = BgInfoStatus.Critical };
        var reference = new BgInfoVisualCanvasTile { Label = "DISK", Value = "91%", Accent = BgInfoStatusColors.Default(mode).Critical };
        var actual = Canvas(mode, tile);
        var expected = Canvas(mode, reference);
        void Compare() {
            using var painted = region ? BgInfoVisualCanvasRenderer.RenderRegion(actual, 640, 240, 240)
                : BgInfoVisualCanvasRenderer.Render(actual, new BgInfoConfiguration(), 640, 240);
            using var control = region ? BgInfoVisualCanvasRenderer.RenderRegion(expected, 640, 240, 240)
                : BgInfoVisualCanvasRenderer.Render(expected, new BgInfoConfiguration(), 640, 240);
            Assert.Equal(control.ToRgbaImage().Pixels, painted.ToRgbaImage().Pixels);
        }
        Compare();
        Assert.Null(tile.Accent); // Rendering must not bake a theme into this reusable definition.
        actual.StatusColors = BgInfoStatusColors.Default(mode);
        actual.StatusColors.Critical = ChartColor.FromHex("#019977");
        reference.Accent = actual.StatusColors.Critical;
        Compare();
        tile.Accent = ChartColor.FromHex("#123456");
        reference.Accent = tile.Accent;
        Compare();
    }

    [Fact]
    public void TileStatusAndCanvasPaletteSurvivePublicConfigurationRoundTrip() {
        var colors = BgInfoStatusColors.Default(VisualThemeMode.Light);
        colors.Critical = ChartColor.FromHex("#019977");
        var canvas = Canvas(VisualThemeMode.Light, new BgInfoVisualCanvasTile { Label = "DISK", Value = "91%", Status = BgInfoStatus.Critical });
        canvas.StatusColors = colors;
        var config = new BgInfoConfiguration();
        config.VisualCanvases.Add(canvas);
        var path = Path.Combine(Path.GetTempPath(), "bginfo-status-" + Path.GetRandomFileName() + ".json");
        try {
            BgInfoConfigurationJson.Save(config, path);
            var loaded = Assert.Single(BgInfoConfigurationJson.Load(path).VisualCanvases);
            Assert.Equal(BgInfoStatus.Critical, Assert.Single(loaded.Tiles).Status);
            Assert.Null(loaded.Tiles[0].Accent);
            Assert.Equal(colors.Critical, loaded.StatusColors!.Critical);
            Assert.Equal(colors.Info, loaded.StatusColors.Info);
            using var first = BgInfoVisualCanvasRenderer.Render(canvas, config, 640, 240);
            using var roundTrip = BgInfoVisualCanvasRenderer.Render(loaded, config, 640, 240);
            Assert.Equal(first.ToRgbaImage().Pixels, roundTrip.ToRgbaImage().Pixels);
        } finally { File.Delete(path); }
    }

    [Theory]
    [InlineData("Typo")]
    [InlineData("2")]
    public void PersistedTileStatusRejectsUnknownNames(string status) {
        var path = Path.Combine(Path.GetTempPath(), "bginfo-status-" + Path.GetRandomFileName() + ".json");
        File.WriteAllText(path, "{\"VisualCanvases\":[{\"Tiles\":[{\"Label\":\"DISK\",\"Value\":\"91%\",\"Status\":\"" + status + "\"}]}]}");
        try { Assert.Throws<InvalidDataException>(() => BgInfoConfigurationJson.Load(path)); }
        finally { File.Delete(path); }
    }

    private static BgInfoVisualCanvas Canvas(VisualThemeMode mode, BgInfoVisualCanvasTile tile) {
        var canvas = new BgInfoVisualCanvas { Width = 640, Height = 240, ThemeMode = mode, FontFamilyName = "Carlito",
            HeroContentVisible = false, TechBackdrop = false, TileWidth = 400, TileHeight = 160, RailMarginX = 20, RailMarginY = 20 };
        canvas.Tiles.Add(tile);
        return canvas;
    }
}
