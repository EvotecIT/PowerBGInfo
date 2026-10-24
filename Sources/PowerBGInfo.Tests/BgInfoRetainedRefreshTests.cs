using System.Reflection;
using ChartForgeX.Primitives;
using DesktopManager;

namespace PowerBGInfo.Tests;

public sealed class BgInfoRetainedRefreshTests {
    [Theory]
    [InlineData(true, "custom", false)]
    [InlineData(true, "default", true)]
    [InlineData(false, "custom", false)]
    [InlineData(false, "default", true)]
    public void RetainedRefreshKeepsItsOriginalAndRepeatsWithoutStackingOrDestinationDrift(bool nativeBackground, string outputMode, bool guardFailure) {
        AssertRetainedRefreshRepeat(nativeBackground, outputMode, guardFailure);
    }

    [UnixFileNameFact]
    public void NativeBackgroundWithLineFeedOutputRecoversAfterSecondAssignmentFailure() =>
        AssertRetainedRefreshRepeat(nativeBackground: true, "line-feed", guardFailure: false);

    [UnixFileNameFact]
    public void ImageWithLineFeedOutputRecoversAfterTargetGuardFailure() =>
        AssertRetainedRefreshRepeat(nativeBackground: false, "line-feed", guardFailure: true);

    private static void AssertRetainedRefreshRepeat(bool nativeBackground, string outputMode, bool guardFailure) {
        using var fixture = new RefreshFixture();
        var target = BgInfoDesktopDefaultsTests.QualifiedTarget(targetIndex: 0);
        var configuration = fixture.Configuration(outputMode);
        if (nativeBackground) {
            configuration = BgInfoGenerator.ResolveDesktopDefaults(configuration,
                State(background: Background(), imageOpacity: 0), target);
        } else {
            configuration.FilePath = fixture.Original;
        }
        var failingService = new RetainedRefreshService(target, guardFailure);
        var generator = new BgInfoGenerator(new ImageService(), failingService);

        var error = Assert.Throws<InvalidOperationException>(() => generator.Generate(configuration));

        Assert.Equal(guardFailure ? "Controlled target change after refresh assignment." : "Controlled second assignment failure.", error.Message);
        Assert.Equal(guardFailure ? 1 : 2, failingService.NativeCalls);
        var retained = Assert.IsType<string>(failingService.AssignedRefresh);
        Assert.True(File.Exists(retained));
        var destination = fixture.Destination(nativeBackground, outputMode);
        using var firstImage = BgInfoRasterImage.Load(destination);
        var firstPixels = firstImage.ToRgbaImage().Pixels;
        Assert.True(BgInfoOutputRegistry.TryGetSource(destination, out var original));
        Assert.NotNull(original);
        var refreshHasOrigin = BgInfoOutputRegistry.TryGetSource(retained, out var refreshOrigin);
        var repeatConfiguration = BgInfoGenerator.ResolveDesktopDefaults(fixture.Configuration(outputMode),
            State(retained, DesktopWallpaperPosition.Stretch, imageOpacity: 1), target);
        Assert.True(string.IsNullOrWhiteSpace(repeatConfiguration.FilePath));
        var repeatService = new RecordingWallpaperService();
        var repeatGenerator = new BgInfoGenerator(new ImageService(), repeatService) { CurrentWallpaper = _ => retained };

        var repeated = repeatGenerator.Generate(repeatConfiguration);

        Assert.Equal(destination, repeated);
        using var repeatedImage = BgInfoRasterImage.Load(repeated);
        Assert.Equal(firstPixels, repeatedImage.ToRgbaImage().Pixels);
        Assert.True(refreshHasOrigin, "The retained native wallpaper must have recoverable provenance before it can become the next input.");
        Assert.Equal(original, refreshOrigin);
        Assert.True(BgInfoOutputRegistry.TryGetSource(repeated, out var repeatedOrigin));
        Assert.Equal(original, repeatedOrigin);
        Assert.Equal(2, repeatService.Paths.Count);
        Assert.Equal(repeated, repeatService.Paths[1]);
        Assert.False(File.Exists(repeatService.Paths[0]));
        Assert.False(BgInfoOutputRegistry.TryGetSource(repeatService.Paths[0], out _));
        Assert.True(BgInfoOutputRegistry.TryGetSource(retained, out refreshOrigin));
        Assert.Equal(original, refreshOrigin);
        var pristineBases = Directory.GetFiles(fixture.Directory, "*PowerBGInfo-original-*.png");
        if (nativeBackground) Assert.Equal(original, Assert.Single(pristineBases));
        else {
            Assert.Empty(pristineBases);
            Assert.Equal(fixture.Original, original);
        }
    }

    [Fact]
    public void RefreshProvenancePublicationFailureStopsBeforeTheFirstNativeAssignment() {
        using var fixture = new RefreshFixture();
        var configuration = fixture.Configuration("custom");
        configuration.FilePath = fixture.Original;
        var sourceBytes = File.ReadAllBytes(fixture.Original);
        // This real filesystem boundary blocks atomic registry publication while image output remains writable.
        Directory.CreateDirectory(Path.Combine(fixture.Directory, BgInfoOutputRegistry.FileName));
        var service = new RetainedRefreshService(BgInfoDesktopDefaultsTests.QualifiedTarget(targetIndex: 0), guardFailure: false);

        Assert.Throws<IOException>(() => new BgInfoGenerator(new ImageService(), service).Generate(configuration));

        Assert.Equal(0, service.NativeCalls);
        Assert.Null(service.AssignedRefresh);
        Assert.Empty(Directory.GetFiles(fixture.Directory, "*_refresh_*.png"));
        Assert.Equal(sourceBytes, File.ReadAllBytes(fixture.Original));
    }

    [UnixFileNameFact]
    public void UnixLineFeedRefreshWithoutProvenanceIsRejectedInsteadOfDrawingOnTheOverlay() {
        using var fixture = new RefreshFixture();
        var configuration = fixture.Configuration("line-feed");
        configuration.FilePath = fixture.Original;
        var firstService = new RetainedRefreshService(BgInfoDesktopDefaultsTests.QualifiedTarget(targetIndex: 0), guardFailure: false);
        Assert.Throws<InvalidOperationException>(() => new BgInfoGenerator(new ImageService(), firstService).Generate(configuration));
        var retained = Assert.IsType<string>(firstService.AssignedRefresh);
        Assert.True(File.Exists(retained));
        var retainedBytes = File.ReadAllBytes(retained);
        // Missing metadata can occur after manual cleanup or when recovering an older retained refresh.
        File.Delete(Path.Combine(fixture.Directory, BgInfoOutputRegistry.FileName));
        var repeatConfiguration = fixture.Configuration("line-feed");
        repeatConfiguration.ForceWallpaperRefresh = false;
        var repeatService = new FakeWallpaperService();
        var repeatGenerator = new BgInfoGenerator(new ImageService(), repeatService) { CurrentWallpaper = _ => retained };

        var error = Assert.Throws<BgInfoWallpaperReadException>(() => repeatGenerator.Generate(repeatConfiguration));

        Assert.Equal(DesktopOperationStatus.Unsupported, error.Status);
        Assert.Equal(0, repeatService.Calls);
        Assert.Equal(retainedBytes, File.ReadAllBytes(retained));
    }

    private static DesktopWallpaperState State(string? path = null, DesktopWallpaperPosition? position = null,
        DesktopWallpaperBackground? background = null, double? imageOpacity = null) =>
        (DesktopWallpaperState)typeof(DesktopWallpaperState)
            .GetConstructors(BindingFlags.Instance | BindingFlags.NonPublic).Single()
            .Invoke(new object?[] { DesktopOperationStatus.Success, "Retained refresh fixture.", path, position, background, imageOpacity });

    private static DesktopWallpaperBackground Background() =>
        (DesktopWallpaperBackground)typeof(DesktopWallpaperBackground)
            .GetConstructors(BindingFlags.Instance | BindingFlags.NonPublic).Single()
            .Invoke(new object?[] { "#12243c", DesktopBackgroundShading.Solid, null });

    private sealed class RetainedRefreshService : FakeWallpaperService, IWallpaperService, IWallpaperTargetGuard {
        private readonly bool _guardFailure;

        internal RetainedRefreshService(DesktopWallpaperTarget target, bool guardFailure) {
            SelectedWallpaperTarget = target;
            _guardFailure = guardFailure;
        }

        public DesktopWallpaperTarget SelectedWallpaperTarget { get; }
        internal int NativeCalls { get; private set; }
        internal string? AssignedRefresh { get; private set; }

        public void RecheckSelectedTarget() {
            if (_guardFailure && NativeCalls > 0) throw new InvalidOperationException("Controlled target change after refresh assignment.");
        }

        public new void SetWallpaper(int monitorIndex, string filePath, DesktopWallpaperPosition position) {
            NativeCalls++;
            if (NativeCalls == 1) {
                AssignedRefresh = filePath;
                return;
            }
            throw new InvalidOperationException("Controlled second assignment failure.");
        }
    }

    private sealed class RecordingWallpaperService : FakeWallpaperService, IWallpaperService {
        internal List<string> Paths { get; } = new();
        public new void SetWallpaper(int monitorIndex, string filePath, DesktopWallpaperPosition position) => Paths.Add(filePath);
    }

    private sealed class RefreshFixture : IDisposable {
        internal RefreshFixture() {
            Directory = Path.Combine(Path.GetTempPath(), "bginfo-retained-refresh-" + Guid.NewGuid().ToString("N"));
            System.IO.Directory.CreateDirectory(Directory);
            Original = Path.Combine(Directory, "photo.png");
            using var image = new BgInfoRasterImage();
            image.Create(Original, 160, 120, ChartColors.Navy);
            image.Save(Original);
        }

        internal string Directory { get; }
        internal string Original { get; }

        internal BgInfoConfiguration Configuration(string outputMode) {
            var configuration = new BgInfoConfiguration {
                ConfigurationDirectory = Directory,
                OutputFileName = outputMode == "default" ? string.Empty : outputMode == "line-feed" ? "wall\npaper.png" : "custom.png",
                Target = BgInfoTarget.Wallpaper,
                UseScreenCoordinates = false,
                Color = ChartColors.White,
                ValueColor = ChartColors.White,
                FontSize = 14,
                ValueFontSize = 14,
                SpaceX = 8,
                SpaceY = 8
            };
            configuration.Entries.Add(new BgInfoEntry { Type = BgInfoEntryType.Value, Name = "Host", Value = "TEST" });
            Assert.True(configuration.ForceWallpaperRefresh);
            return configuration;
        }

        internal string Destination(bool nativeBackground, string outputMode) => Path.Combine(Directory,
            outputMode == "default" ? nativeBackground ? "PowerBgInfo.png" : "photo_PowerBgInfo.png"
                : outputMode == "line-feed" ? "wall\npaper.png" : "custom.png");

        public void Dispose() => System.IO.Directory.Delete(Directory, recursive: true);
    }
}

/// <summary>Real filename workflows require a filesystem where LF is a supported filename character.</summary>
internal sealed class UnixFileNameFactAttribute : FactAttribute {
    public UnixFileNameFactAttribute() {
        if (OperatingSystem.IsWindows()) Skip = "The native Windows filename grammar rejects line-feed characters.";
    }
}
