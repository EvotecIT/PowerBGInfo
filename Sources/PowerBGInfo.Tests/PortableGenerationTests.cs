using ChartForgeX.Primitives;

namespace PowerBGInfo.Tests;

public sealed class PortableGenerationTests {
    [Fact]
    public void FilePreviewIgnoresInactiveWindowsPolicyOptions() {
        var directory = Path.Combine(Path.GetTempPath(), "bginfo-preview-" + Guid.NewGuid().ToString("N"));
        var config = new BgInfoConfiguration {
            ConfigurationDirectory = directory, OutputFileName = "preview.png", Target = BgInfoTarget.File,
            ApplyToAllUsers = true, IncludeDefaultUserProfile = true,
            Background = new BgInfoBackground { Width = 64, Height = 48, Color = ChartColors.Black }
        };
        try {
            var path = new BgInfoGenerator(new ImageService(), new WallpaperService()).Generate(config);
            using var image = BgInfoRasterImage.Load(path);
            Assert.Equal(64, image.Width);
            Assert.Equal(48, image.Height);
        } finally {
            if (Directory.Exists(directory)) Directory.Delete(directory, recursive: true);
        }
    }

    [Fact]
    public void UnavailableMetricRetainsHistoryWithoutDisplayingItAsCurrent() {
        var directory = Path.Combine(Path.GetTempPath(), "bginfo-stale-" + Guid.NewGuid().ToString("N"));
        var history = Path.Combine(directory, "Charts", "disk.txt");
        ChartHistoryStore.Save(history, new[] { new ChartSample(DateTimeOffset.UtcNow.AddHours(-1), 42) });
        var warnings = new List<string>();
        var generator = new BgInfoGenerator(new ImageService(), new FakeWallpaperService()) { Warning = warnings.Add };
        var config = new BgInfoConfiguration {
            ConfigurationDirectory = directory, OutputFileName = "metric.png", Target = BgInfoTarget.File,
            Background = new BgInfoBackground { Width = 128, Height = 96, Color = ChartColors.Black }
        };
        config.Charts.Add(new BgInfoChart {
            Id = "disk", Title = "Disk", Kind = BgInfoChartKind.Bar,
            Metric = BgInfoChartMetric.DiskFreeGb, MetricArgument = Path.Combine(directory, "missing-" + Guid.NewGuid().ToString("N")),
            BackgroundColor = ChartColors.White
        });
        try {
            var path = generator.Generate(config);
            using var image = BgInfoRasterImage.Load(path);
            var pixels = image.ToRgbaImage().Pixels;
            for (var index = 0; index < pixels.Length; index += 4) {
                Assert.Equal(0, pixels[index]);
                Assert.Equal(0, pixels[index + 1]);
                Assert.Equal(0, pixels[index + 2]);
            }
            Assert.Equal(42, Assert.Single(ChartHistoryStore.Load(history)).Value);
            Assert.Contains(warnings, warning => warning.Contains("not displayed as a current reading"));
        } finally {
            Directory.Delete(directory, recursive: true);
        }
    }

    [Fact]
    public void FileRenderingUsesExplicitDimensionsWithScreenCoordinates() {
        var directory = Path.Combine(Path.GetTempPath(), "bginfo-portable-" + Guid.NewGuid().ToString("N"));
        var wallpaper = new FakeWallpaperService();
        var generator = new BgInfoGenerator(new ImageService(), wallpaper) {
            CurrentWallpaper = _ => throw new InvalidOperationException("File rendering must not read the desktop.")
        };
        var config = new BgInfoConfiguration {
            ConfigurationDirectory = directory, OutputFileName = "portable.png",
            Target = BgInfoTarget.File, UseScreenCoordinates = true, MonitorIndex = int.MaxValue,
            Background = new BgInfoBackground { Width = 360, Height = 220, Color = ChartColors.Black }
        };
        config.Entries.Add(new BgInfoEntry { Name = "Portable", Value = "Windows / macOS / Linux" });

        try {
            var path = generator.Generate(config);
            using var image = BgInfoRasterImage.Load(path);
            Assert.Equal(360, image.Width);
            Assert.Equal(220, image.Height);
            Assert.Equal(0, wallpaper.Calls);
        } finally {
            if (Directory.Exists(directory)) Directory.Delete(directory, recursive: true);
        }
    }

    [Fact]
    public void NonWindowsAdministrativeTargetsFailBeforeRendering() {
        if (OperatingSystem.IsWindows()) return;
        var directory = Path.Combine(Path.GetTempPath(), "bginfo-policy-" + Guid.NewGuid().ToString("N"));
        var config = new BgInfoConfiguration {
            ConfigurationDirectory = directory, Target = BgInfoTarget.LogonScreen,
            Background = new BgInfoBackground { Width = 100, Height = 100 }
        };

        Assert.Throws<PlatformNotSupportedException>(() => BgInfoRunner.Run(config));
        Assert.False(Directory.Exists(directory));
    }

    [Fact]
    public void UnixVolumeIdentityKeepsTheMountPath() {
        if (OperatingSystem.IsWindows()) return;

        Assert.Equal("/", BgInfoVariableResolver.NormalizeVolumeIdentity("/"));
        Assert.Equal("/mnt/data", BgInfoVariableResolver.NormalizeVolumeIdentity("/mnt/data/"));
        Assert.NotEqual(BgInfoVariableResolver.NormalizeVolumeIdentity("/mnt/A"), BgInfoVariableResolver.NormalizeVolumeIdentity("/mnt/a"));
    }

    [Fact]
    public void UnixOutputNamesKeepIndependentOriginalImages() {
        if (OperatingSystem.IsWindows()) return;
        var directory = Path.Combine(Path.GetTempPath(), "bginfo-paths-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        try {
            var lower = Path.Combine(directory, "wallpaper.png");
            var upper = Path.Combine(directory, "Wallpaper.png");
            var firstSource = Path.Combine(directory, "first.png");
            var secondSource = Path.Combine(directory, "second.png");
            BgInfoOutputRegistry.Record(lower, firstSource);
            BgInfoOutputRegistry.Record(upper, secondSource);

            Assert.True(BgInfoOutputRegistry.TryGetSource(lower, out var first));
            Assert.True(BgInfoOutputRegistry.TryGetSource(upper, out var second));
            Assert.Equal(firstSource, first);
            Assert.Equal(secondSource, second);
        } finally {
            Directory.Delete(directory, recursive: true);
        }
    }
}
