using ChartForgeX;
using ChartForgeX.Primitives;
using ChartForgeX.Themes;
using ChartForgeX.Core;

namespace PowerBGInfo.Tests;

public sealed class BgInfoSharedThemeMigrationTests {
    [Fact]
    public void RadialBarWallpaperKeepsPercentRingsAndItsCenterLabelOption() {
        var chart = new BgInfoChart { Kind = BgInfoChartKind.RadialBar, ShowRadialBarCenterLabel = false };
        var plot = BgInfoChartRenderer.BuildChartForgeXChart(chart, new[] { -5d, 45d, 130d }, new BgInfoConfiguration(), 320, 180);
        var series = Assert.Single(plot.Series);
        Assert.Equal(ChartSeriesKind.ProgressRing, series.Kind);
        Assert.Equal(new[] { 0d, 45d, 100d }, series.Points.Select(point => point.Y));
        Assert.False(plot.Options.ShowProgressRingCenterLabel);
        Assert.Contains("progress-ring", plot.ToSvg());
    }

    [Fact]
    public void EveryWallpaperChartKeepsItsSeriesIdentityWhenDisplayNamesChange() {
        var configuration = new BgInfoConfiguration();
        foreach (BgInfoChartKind kind in Enum.GetValues(typeof(BgInfoChartKind))) {
            var chart = new BgInfoChart {
                Id = "machine:cpu", Title = "CPU", Kind = kind,
                Values = new[] { 35d, 65d }, Labels = new[] { "Before", "After" }, UseHistory = false
            };
            var original = BgInfoChartRenderer.BuildChartForgeXChart(chart, chart.Values, configuration, 320, 180);
            chart.Title = "Processor utilization";
            chart.Labels = new[] { "Renamed", "Renamed" };
            var renamed = BgInfoChartRenderer.BuildChartForgeXChart(chart, chart.Values, configuration, 320, 180);

            Assert.Equal(kind == BgInfoChartKind.Bullet ? 2 : 1, original.Series.Count);
            Assert.Equal(original.Series.Select(series => series.Id), renamed.Series.Select(series => series.Id));
            Assert.Equal(original.Series.Count, original.Series.Select(series => series.Id).Distinct().Count());
            Assert.All(original.Series, series => {
                Assert.False(series.Id.IsAutomatic);
                Assert.StartsWith("bginfo:machine:cpu:", series.Id.Value);
            });
        }
    }

    [Theory]
    [InlineData(VisualThemeMode.Light)]
    [InlineData(VisualThemeMode.Dark)]
    public void IndependentCanvasTextPaintsSurviveJsonAndReachWallpaperPixels(VisualThemeMode mode) {
        var directory = Path.Combine(Path.GetTempPath(), "bginfo-role-paints-" + Path.GetRandomFileName());
        Directory.CreateDirectory(directory);
        try {
            var configuration = new BgInfoConfiguration();
            var visual = new BgInfoVisualCanvas {
                ThemeMode = mode, Title = "PowerBGInfo", Subtitle = "Independent text colors",
                Transparent = false, Width = 1200, Height = 630,
                TitleColor = ChartColors.Gold, TitleAccentColor = ChartColors.Cyan,
                SubtitleColor = ChartColors.White,
                TileLabelColor = ChartColor.FromHex("#D01327"),
                TileValueColor = ChartColor.FromHex("#19A64E"),
                TileDetailColor = ChartColor.FromHex("#2163D8"),
                HeroBadgeTextColor = ChartColor.FromHex("#A21FC3")
            };
            visual.Tiles.Add(new BgInfoVisualCanvasTile {
                Label = "Capacity", Value = "256 GB", Detail = "Calibrated role paints", Width = 330, Height = 150
            });
            configuration.VisualCanvases.Add(visual);
            var path = Path.Combine(directory, "wallpaper.json");
            BgInfoConfigurationJson.Save(configuration, path);
            var loaded = BgInfoConfigurationJson.Load(path);
            var retained = Assert.Single(loaded.VisualCanvases);
            using var image = BgInfoVisualCanvasRenderer.Render(retained, loaded, 1200, 630);
            var rgba = image.ToRgbaImage();

            Assert.Equal(mode, retained.ThemeMode);
            Assert.Equal(1200, rgba.Width);
            Assert.Equal(630, rgba.Height);
            foreach (var color in new[] { visual.TileLabelColor, visual.TileValueColor, visual.TileDetailColor,
                visual.HeroBadgeTextColor, visual.TitleColor, visual.TitleAccentColor, visual.SubtitleColor }) {
                Assert.True(ContainsOpaqueColor(rgba.Pixels, color!.Value), "The native wallpaper omitted an independent text paint " + color.Value.ToHexRgba());
            }
        } finally {
            Directory.Delete(directory, recursive: true);
        }
    }

    private static bool ContainsOpaqueColor(byte[] pixels, ChartColor color) {
        for (var index = 0; index < pixels.Length; index += 4)
            if (pixels[index] == color.R && pixels[index + 1] == color.G && pixels[index + 2] == color.B && pixels[index + 3] == 255) return true;
        return false;
    }
}
