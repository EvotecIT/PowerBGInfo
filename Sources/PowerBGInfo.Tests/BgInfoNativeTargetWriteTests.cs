using System.Reflection;
using ChartForgeX.Primitives;
using DesktopManager;

namespace PowerBGInfo.Tests;

public sealed class BgInfoNativeTargetWriteTests {
    [Fact]
    public void GenerateUsesSelectedWallpaperGeometryWhenPhysicalAndTargetIndicesDiffer() {
        using var fixture = new WallpaperFixture();
        var config = fixture.Configuration;
        config.FilePath = fixture.Source;
        config.UseScreenCoordinates = true;
        config.WallpaperFit = DesktopWallpaperPosition.Fill;
        var service = new GuardedWallpaperService {
            SelectedWallpaperTarget = BgInfoDesktopDefaultsTests.QualifiedTarget(targetIndex: 0, physicalIndex: 2)
        };

        var output = new BgInfoGenerator(new ImageService(), service).Generate(config);

        using var image = BgInfoRasterImage.Load(output);
        Assert.Equal(160, image.Width);
        Assert.Equal(120, image.Height);
        Assert.Equal(new[] { "wallpaper" }, service.Writes);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void UnknownSelectedGeometryRejectsScreenCoordinatesWhileExplicitCanvasRenderingRemainsUsable(bool explicitBackground) {
        using var fixture = new WallpaperFixture();
        var config = fixture.Configuration;
        if (explicitBackground) config.Background = new BgInfoBackground { Width = 16, Height = 12, Color = ChartColors.Navy };
        else config.FilePath = fixture.Source;
        config.UseScreenCoordinates = true;
        var service = new GuardedWallpaperService {
            SelectedWallpaperTarget = BgInfoDesktopDefaultsTests.QualifiedTarget(targetIndex: 0, hasGeometry: false)
        };
        var generator = new BgInfoGenerator(new ImageService(), service);

        var error = Assert.Throws<BgInfoWallpaperReadException>(() => generator.Generate(config));

        Assert.Equal(DesktopOperationStatus.Unsupported, error.Status);
        Assert.Contains("Screen-coordinate layout", error.Diagnostic, StringComparison.Ordinal);
        Assert.False(File.Exists(Path.Combine(config.ConfigurationDirectory, config.OutputFileName!)));
        Assert.Empty(service.Writes);
        config.UseScreenCoordinates = false;
        var output = generator.Generate(config);
        using var image = BgInfoRasterImage.Load(output);
        Assert.Equal(16, image.Width);
        Assert.Equal(12, image.Height);
        Assert.Equal(new[] { "wallpaper" }, service.Writes);
    }

    [Theory]
    [InlineData("all-users")]
    [InlineData("slideshow")]
    [InlineData("slideshow-all-users")]
    [InlineData("wallpaper-refresh")]
    public void ChangedSelectedTargetRejectsBeforeTheFirstDependentNativeWrite(string workflow) {
        using var fixture = new WallpaperFixture();
        var slideshow = workflow.StartsWith("slideshow", StringComparison.Ordinal);
        var service = new GuardedWallpaperService {
            TargetChanged = true,
            Slideshow = slideshow ? new DesktopWallpaperSlideshow {
                ImagePaths = new[] { fixture.Source },
                State = DesktopSlideshowState.Enabled | DesktopSlideshowState.Slideshow
            } : new DesktopWallpaperSlideshow()
        };
        var config = fixture.Configuration;
        config.FilePath = slideshow ? string.Empty : fixture.Source;
        config.ApplyToAllUsers = workflow.Contains("all-users", StringComparison.Ordinal);
        config.ForceWallpaperRefresh = workflow == "wallpaper-refresh";
        var generator = new BgInfoGenerator(new ImageService(), service) { CurrentWallpaper = _ => fixture.Source };

        var failure = Assert.Throws<InvalidOperationException>(() => generator.Generate(config));

        Assert.Equal("The selected native target changed.", failure.Message);
        Assert.Empty(service.Writes);
        Assert.Equal(1, service.GuardCalls);
        Assert.Empty(Directory.EnumerateFiles(config.ConfigurationDirectory, "*_refresh_*.png"));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void TargetChangeAfterCurrentWallpaperWriteRejectsBeforeFurtherDependentNativeWrites(bool refresh) {
        using var fixture = new WallpaperFixture();
        var config = fixture.Configuration;
        config.FilePath = fixture.Source;
        config.Target = BgInfoTarget.Wallpaper | BgInfoTarget.LogonScreen;
        config.ForceWallpaperRefresh = refresh;
        var service = new GuardedWallpaperService { ChangeTargetAfterWrite = true };
        var warnings = new List<string>();
        var generator = new BgInfoGenerator(new ImageService(), service) { Warning = warnings.Add };

        Assert.Throws<InvalidOperationException>(() => generator.Generate(config));

        Assert.Equal(new[] { "wallpaper" }, service.Writes);
        if (refresh) {
            var retained = Assert.Single(Directory.EnumerateFiles(config.ConfigurationDirectory, "*_refresh_*.png"));
            Assert.Equal(retained, Assert.Single(service.AssignedWallpaperPaths));
            Assert.Contains(warnings, warning => warning.Contains(retained, StringComparison.Ordinal));
        } else {
            Assert.Empty(Directory.EnumerateFiles(config.ConfigurationDirectory, "*_refresh_*.png"));
        }
    }

    [Fact]
    public void NoninteractiveAllUserPolicyDoesNotRequireACurrentDesktopSelection() {
        using var fixture = new WallpaperFixture();
        var config = fixture.Configuration;
        config.FilePath = fixture.Source;
        config.ApplyToAllUsers = true;
        var service = new GuardedWallpaperService { HasSelectedTarget = false, TargetChanged = true };
        var generator = new BgInfoGenerator(new ImageService(), service);
        // Exercise the production policy path selected when Windows has no interactive desktop.
        var generate = typeof(BgInfoGenerator).GetMethod("GenerateImage", BindingFlags.Instance | BindingFlags.NonPublic)!;

        var output = (string)generate.Invoke(generator, new object[] {
            config,
            new Func<int, string?>(_ => null),
            new Func<IReadOnlyList<BgInfoMonitor>?>(() => Array.Empty<BgInfoMonitor>()),
            true,
            false
        })!;

        Assert.True(File.Exists(output));
        Assert.Equal(new[] { "all-users" }, service.Writes);
    }

    private sealed class GuardedWallpaperService : IWallpaperService, IWallpaperTargetGuard {
        public DesktopWallpaperTarget? SelectedWallpaperTarget { get; set; }
        internal bool HasSelectedTarget { get; set; } = true;
        internal bool TargetChanged { get; set; }
        internal bool ChangeTargetAfterWrite { get; set; }
        internal int GuardCalls { get; private set; }
        internal List<string> Writes { get; } = new();
        internal List<string> AssignedWallpaperPaths { get; } = new();
        internal DesktopWallpaperSlideshow Slideshow { get; set; } = new();

        public void RecheckSelectedTarget() {
            GuardCalls++;
            if (HasSelectedTarget && TargetChanged) throw new InvalidOperationException("The selected native target changed.");
        }

        public void SetWallpaper(int monitorIndex, string filePath, DesktopWallpaperPosition position) {
            AssignedWallpaperPaths.Add(filePath);
            Record("wallpaper");
        }
        public void SetLogonWallpaper(string filePath) => Record("lock-screen");
        public void SetWallpaperForAllUsers(string filePath, DesktopWallpaperPosition position, bool includeDefaultUserProfile) => Record("all-users");
        public DesktopWallpaperSlideshow GetWallpaperSlideshow() => Slideshow;
        public void StartWallpaperSlideshow(IEnumerable<string> filePaths, DesktopWallpaperPosition position, DesktopSlideshowOptions options, uint slideshowTick) => Record("slideshow");

        private void Record(string write) {
            Writes.Add(write);
            if (ChangeTargetAfterWrite) TargetChanged = true;
        }
    }

    private sealed class WallpaperFixture : IDisposable {
        private readonly string _directory = Path.Combine(Path.GetTempPath(), "bginfo-native-target-write-" + Guid.NewGuid().ToString("N"));

        internal WallpaperFixture() {
            Directory.CreateDirectory(_directory);
            Source = Path.Combine(_directory, "source.png");
            using var image = new BgInfoRasterImage();
            image.Create(Source, 16, 12, ChartColors.Navy);
            image.Save(Source);
        }

        internal string Source { get; }
        internal BgInfoConfiguration Configuration => new() {
            ConfigurationDirectory = _directory,
            OutputFileName = "generated.png",
            Target = BgInfoTarget.Wallpaper,
            UseScreenCoordinates = false,
            ForceWallpaperRefresh = false
        };

        public void Dispose() => Directory.Delete(_directory, recursive: true);
    }
}
