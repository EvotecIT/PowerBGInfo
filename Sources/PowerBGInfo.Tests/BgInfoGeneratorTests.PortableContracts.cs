using ChartForgeX.Composition;
using ChartForgeX.Primitives;
using Color = ChartForgeX.Primitives.ChartColors;

namespace PowerBGInfo.Tests;

public partial class BgInfoGeneratorTests {
    [Fact]
    public void GenerateRejectsAnExplicitMissingWallpaperBeforeApplyingAnything() {
        using var folder = new PortableContractFolder();
        var wallpaper = new FakeWallpaperService();
        var generator = new BgInfoGenerator(new ImageService(), wallpaper);
        var config = folder.Configuration("missing-output.png");
        config.Background = null;
        config.FilePath = Path.Combine(folder.Path, "missing-source.png");
        config.Target = BgInfoTarget.Wallpaper;
        config.BackgroundColor = Color.DarkSlateBlue;

        Assert.Throws<FileNotFoundException>(() => generator.Generate(config));

        Assert.False(File.Exists(Path.Combine(folder.Path, "missing-output.png")));
        Assert.Equal(0, wallpaper.Calls);
        Assert.Equal(0, wallpaper.AllUsersCalls);
        Assert.Equal(0, wallpaper.SlideshowCalls);
        Assert.Equal(0, wallpaper.LogonCalls);
    }

    [Fact]
    public void ResolveBaseImagePathPreservesWallpaperReadFailure() {
        var failure = new IOException("The current wallpaper could not be read.");

        var actual = Assert.Throws<IOException>(() =>
            BgInfoGenerator.ResolveBaseImagePath(new BgInfoConfiguration(), _ => throw failure));

        Assert.Same(failure, actual);
    }

    [Fact]
    public void GenerateRendersChartAndStoresHistory() {
        using var folder = new PortableContractFolder();
        var wallpaper = new FakeWallpaperService();
        var config = folder.Configuration("chart-wallpaper.png");
        config.Charts.Add(new BgInfoChart {
            Id = "cpu", Title = "CPU", Kind = BgInfoChartKind.Sparkline,
            Values = new[] { 10d, 20d, 30d }, MaxPoints = 5,
            OffsetX = 20, OffsetY = 20, Anchor = BgInfoTextPosition.BottomLeft
        });

        var path = new BgInfoGenerator(new ImageService(), wallpaper).Generate(config);

        Assert.True(File.Exists(path));
        Assert.Equal(new[] { 10d, 20d, 30d },
            ChartHistoryStore.Load(Path.Combine(folder.Path, "Charts", "cpu.txt")).Select(sample => sample.Value));
        Assert.Equal(0, wallpaper.Calls);
    }

    [Fact]
    public void GenerateUsesDistinctHistoryFilesForChartsWithSameTitle() {
        using var folder = new PortableContractFolder();
        var config = folder.Configuration("chart-history.png");
        config.Charts.Add(new BgInfoChart {
            Title = "CPU", Kind = BgInfoChartKind.Sparkline, Values = new[] { 10d, 20d }, MaxPoints = 5
        });
        config.Charts.Add(new BgInfoChart {
            Title = "CPU", Kind = BgInfoChartKind.Bar, Values = new[] { 30d, 40d }, MaxPoints = 5
        });

        new BgInfoGenerator(new ImageService(), new FakeWallpaperService()).Generate(config);

        Assert.Equal(new[] { 10d, 20d },
            ChartHistoryStore.Load(Path.Combine(folder.Path, "Charts", "CPU_0.txt")).Select(sample => sample.Value));
        Assert.Equal(new[] { 30d, 40d },
            ChartHistoryStore.Load(Path.Combine(folder.Path, "Charts", "CPU_1.txt")).Select(sample => sample.Value));
    }

    [Fact]
    public void GenerateUsesFallbackMonitorSizeForFileOnlyScreenCoordinates() {
        using var folder = new PortableContractFolder();
        var wallpaper = new FakeWallpaperService();
        var config = folder.Configuration("screen-coordinates.png");
        config.Background = new BgInfoBackground { Color = Color.Black };
        config.UseScreenCoordinates = true;
        config.MonitorIndex = 999;

        var path = new BgInfoGenerator(new ImageService(), wallpaper).Generate(config);
        using var image = BgInfoRasterImage.Load(path);

        Assert.Equal(1920, image.Width);
        Assert.Equal(1080, image.Height);
        Assert.Equal(0, wallpaper.Calls);
    }

    [Fact]
    public void GenerateNormalizesExplicitLegacyOutputFileName() {
        using var folder = new PortableContractFolder();
        var config = folder.Configuration("wallpaper.wdp");

        var path = new BgInfoGenerator(new ImageService(), new FakeWallpaperService()).Generate(config);

        Assert.Equal(Path.Combine(folder.Path, "wallpaper.png"), path);
        using var image = BgInfoRasterImage.Load(path);
        Assert.Equal(640, image.Width);
        Assert.Equal(360, image.Height);
    }

    private sealed class PortableContractFolder : IDisposable {
        internal PortableContractFolder() {
            Path = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "bginfo-portable-contract-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(Path);
        }

        internal string Path { get; }

        internal BgInfoConfiguration Configuration(string outputName) {
            var config = new BgInfoConfiguration {
                ConfigurationDirectory = Path, OutputFileName = outputName, Target = BgInfoTarget.File,
                ForceWallpaperRefresh = false,
                Background = new BgInfoBackground { Width = 640, Height = 360, Color = Color.Black }
            };
            config.Entries.Add(new BgInfoEntry { Type = BgInfoEntryType.Value, Name = "Test", Value = "1" });
            return config;
        }

        public void Dispose() => Directory.Delete(Path, recursive: true);
    }
}
