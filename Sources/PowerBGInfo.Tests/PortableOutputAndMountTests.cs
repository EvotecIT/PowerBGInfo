using ChartForgeX.Primitives;
using DesktopManager;

namespace PowerBGInfo.Tests;

public sealed class PortableOutputAndMountTests {
    [Theory]
    [InlineData("nested\\render.png", "nested/render.png")]
    [InlineData("nested/render.png", "nested/render.png")]
    [InlineData("..\\render.png", "../render.png")]
    public void JsonOutputUsesTheSamePortableRelativePathRulesAsInputs(string output, string expectedRelativePath) {
        using var folder = new FixtureFolder();
        var configurationDirectory = Path.Combine(folder.Path, "configuration");
        Directory.CreateDirectory(configurationDirectory);
        var configurationPath = Path.Combine(folder.Path, "configuration.json");
        BgInfoConfigurationJson.Save(new BgInfoConfiguration {
            ConfigurationDirectory = configurationDirectory,
            OutputFileName = output,
            Target = BgInfoTarget.File,
            Background = new BgInfoBackground { Width = 16, Height = 12, Color = ChartColors.DarkBlue }
        }, configurationPath);

        var configuration = BgInfoConfigurationJson.Load(configurationPath);
        var result = new BgInfoGenerator(new ImageService(), new FakeWallpaperService()).Generate(configuration);

        var expected = Path.GetFullPath(Path.Combine(configurationDirectory,
            expectedRelativePath.Replace('/', Path.DirectorySeparatorChar)));
        Assert.Equal(expected, result);
        using var image = BgInfoRasterImage.Load(expected);
        Assert.Equal(16, image.Width);
        Assert.Equal(12, image.Height);
    }

    [Theory]
    [InlineData("C:\\portable-output\\render.png")]
    [InlineData("\\\\server\\share\\render.png")]
    public void JsonOutputRejectsForeignWindowsRootsBeforeWritingOnUnix(string output) {
        if (OperatingSystem.IsWindows()) return;
        using var folder = new FixtureFolder();
        var configurationPath = Path.Combine(folder.Path, "configuration.json");
        var outputDirectory = Path.Combine(folder.Path, "output");
        BgInfoConfigurationJson.Save(new BgInfoConfiguration {
            ConfigurationDirectory = outputDirectory,
            OutputFileName = output,
            Target = BgInfoTarget.File,
            Background = new BgInfoBackground { Width = 16, Height = 12 }
        }, configurationPath);

        Assert.Throws<ArgumentException>(() => {
            var configuration = BgInfoConfigurationJson.Load(configurationPath);
            new BgInfoGenerator(new ImageService(), new FakeWallpaperService()).Generate(configuration);
        });
        Assert.False(Directory.Exists(outputDirectory));
    }

    [Fact]
    public void DirectConfigurationOutputUsesPortableSeparatorsAndPreservesParentRelativeSelection() {
        using var folder = new FixtureFolder();
        var configurationDirectory = Path.Combine(folder.Path, "configuration");
        var configuration = new BgInfoConfiguration {
            ConfigurationDirectory = configurationDirectory,
            OutputFileName = "..\\direct.png",
            Target = BgInfoTarget.File,
            Background = new BgInfoBackground { Width = 16, Height = 12, Color = ChartColors.DarkBlue }
        };

        var result = new BgInfoGenerator(new ImageService(), new FakeWallpaperService()).Generate(configuration);

        Assert.Equal(Path.Combine(folder.Path, "direct.png"), result);
        using var image = BgInfoRasterImage.Load(result);
        Assert.Equal(16, image.Width);
        Assert.Equal(12, image.Height);
    }

    [Fact]
    public void CliDirectoryOverrideRebasesTheRelativeJsonOutputBeforeRendering() {
        using var folder = new FixtureFolder();
        var originalDirectory = Path.Combine(folder.Path, "original-output");
        var overriddenDirectory = Path.Combine(folder.Path, "overridden-output");
        var configurationPath = Path.Combine(folder.Path, "configuration.json");
        BgInfoConfigurationJson.Save(new BgInfoConfiguration {
            ConfigurationDirectory = originalDirectory,
            OutputFileName = "nested\\render.png",
            Target = BgInfoTarget.File,
            Background = new BgInfoBackground { Width = 16, Height = 12, Color = ChartColors.DarkBlue }
        }, configurationPath);

        var exitCode = PowerBGInfo.Cli.Program.Main(new[] {
            "--config", configurationPath, "--directory", overriddenDirectory, "--no-apply"
        });

        Assert.Equal(0, exitCode);
        var expected = Path.Combine(overriddenDirectory, "nested", "render.png");
        using var image = BgInfoRasterImage.Load(expected);
        Assert.Equal(16, image.Width);
        Assert.Equal(12, image.Height);
        Assert.False(Directory.Exists(originalDirectory));
    }

    [Fact]
    public void SlideshowOutputUsesPortableSeparatorsBeforeAddingItsSequenceNumber() {
        using var folder = new FixtureFolder();
        var source = Path.Combine(folder.Path, "source.png");
        using (var image = new BgInfoRasterImage()) {
            image.Create(source, 16, 12, ChartColors.DarkBlue);
            image.Save(source);
        }
        var wallpaper = new FakeWallpaperService {
            Slideshow = new DesktopWallpaperSlideshow {
                ImagePaths = new[] { source },
                State = DesktopSlideshowState.Enabled | DesktopSlideshowState.Slideshow
            }
        };
        var configuration = new BgInfoConfiguration {
            ConfigurationDirectory = folder.Path,
            OutputFileName = "nested\\slides.png",
            Target = BgInfoTarget.Wallpaper,
            ForceWallpaperRefresh = false
        };

        var result = new BgInfoGenerator(new ImageService(), wallpaper).Generate(configuration);

        Assert.Equal(Path.Combine(folder.Path, "nested", "slides_001.png"), result);
        Assert.Equal(result, Assert.Single(wallpaper.SlideshowPaths));
        using var rendered = BgInfoRasterImage.Load(result);
        Assert.NotEmpty(rendered.ToRgbaImage().Pixels);
    }

    [Fact]
    public void DiskSpaceMetricFollowsExistingSymlinkToAnotherMountedFilesystem() {
        if (OperatingSystem.IsWindows()) return;
        using var folder = new FixtureFolder();
        var provider = new PortableSystemInfoProvider();
        Assert.True(provider.TryGetNumericValue(BgInfoChartMetric.DiskFreeGb, folder.Path, out var localFree));
        const double gigabyte = 1024d * 1024d * 1024d;
        DriveInfo? target = null;
        foreach (var drive in DriveInfo.GetDrives()) {
            try {
                if (drive.IsReady && drive.TotalSize > 0 &&
                    Math.Abs(drive.AvailableFreeSpace / gigabyte - localFree) > 1d) {
                    target = drive;
                    break;
                }
            } catch (IOException) { /* An unavailable mount is not a fixture. */ }
            catch (UnauthorizedAccessException) { /* An inaccessible mount is not a fixture. */ }
        }
        // This workflow needs two actual filesystems. A one-filesystem CI host
        // cannot establish cross-mount correctness by using a second directory.
        if (target == null) return;
        var link = Path.Combine(folder.Path, "mounted-filesystem");
        Directory.CreateSymbolicLink(link, target.RootDirectory.FullName);
        try {
            var expected = target.AvailableFreeSpace / gigabyte;
            Assert.True(provider.TryGetNumericValue(BgInfoChartMetric.DiskFreeGb, link, out var actual));
            Assert.InRange(actual, Math.Max(0, expected - 0.25d), expected + 0.25d);
        } finally {
            Directory.Delete(link);
        }
    }

    private sealed class FixtureFolder : IDisposable {
        internal FixtureFolder() {
            Path = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "bginfo-output-mount-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(Path);
        }

        internal string Path { get; }

        public void Dispose() => Directory.Delete(Path, recursive: true);
    }
}
