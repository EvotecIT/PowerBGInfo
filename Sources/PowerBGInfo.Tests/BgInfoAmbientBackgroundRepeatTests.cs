using System.Reflection;
using ChartForgeX.Primitives;
using DesktopManager;

namespace PowerBGInfo.Tests;

public sealed class BgInfoAmbientBackgroundRepeatTests {
    [Theory]
    [InlineData(DesktopBackgroundShading.Solid, true)]
    [InlineData(DesktopBackgroundShading.Horizontal, true)]
    [InlineData(DesktopBackgroundShading.Vertical, true)]
    [InlineData(DesktopBackgroundShading.Solid, false)]
    public void NativeColorOrGradientRepeatsFromOnePristineBaseWithoutStackingTheOverlay(DesktopBackgroundShading shading, bool explicitOutputName) {
        using var fixture = new AmbientFixture();
        var configuration = fixture.Configuration;
        if (!explicitOutputName) configuration.OutputFileName = string.Empty;
        var target = BgInfoDesktopDefaultsTests.QualifiedTarget(targetIndex: 0);
        var nativeBackground = Background("#12243c", shading, "#c48e20");
        var initialState = State(background: nativeBackground, imageOpacity: 0);
        var initialConfiguration = BgInfoGenerator.ResolveDesktopDefaults(configuration, initialState, target);
        var background = Assert.IsType<BgInfoBackground>(initialConfiguration.Background);
        using var pristine = BgInfoBackgroundRenderer.Render(background, background.Width, background.Height);

        var first = new BgInfoGenerator(new ImageService(), new FakeWallpaperService()).Generate(initialConfiguration);
        using var firstImage = BgInfoRasterImage.Load(first);
        var firstPixels = firstImage.ToRgbaImage().Pixels;
        Assert.False(pristine.ToRgbaImage().Pixels.SequenceEqual(firstPixels));
        // After application the native backend reports the generated PNG as an opaque image.
        var imageState = State(first, DesktopWallpaperPosition.Stretch, imageOpacity: 1);
        var repeatConfiguration = BgInfoGenerator.ResolveDesktopDefaults(configuration, imageState, target);
        var repeatGenerator = new BgInfoGenerator(new ImageService(), new FakeWallpaperService()) { CurrentWallpaper = _ => first };

        var repeat = repeatGenerator.Generate(repeatConfiguration);

        Assert.Equal(first, repeat);
        using var repeatedImage = BgInfoRasterImage.Load(repeat);
        Assert.Equal(firstPixels, repeatedImage.ToRgbaImage().Pixels);
        Assert.True(BgInfoOutputRegistry.TryGetSource(repeat, out var original));
        Assert.NotNull(original);
        Assert.NotEqual(repeat, original);
        Assert.Equal(original, Assert.Single(Directory.GetFiles(fixture.Directory, "*PowerBGInfo-original-*.png")));
        using var recordedBase = BgInfoRasterImage.Load(original!);
        Assert.Equal(pristine.ToRgbaImage().Pixels, recordedBase.ToRgbaImage().Pixels);
        Assert.Null(configuration.Background);
    }

    [Fact]
    public void FailedOutputSaveCreatesNoPristineBaseAndMakesNoNativeWrite() {
        using var fixture = new AmbientFixture();
        var configuration = NativeConfiguration(fixture);
        Directory.CreateDirectory(Path.Combine(fixture.Directory, configuration.OutputFileName!));
        var service = new FakeWallpaperService();

        Assert.Throws<IOException>(() => new BgInfoGenerator(new ImageService(), service).Generate(configuration));

        Assert.Empty(Directory.GetFiles(fixture.Directory, "*PowerBGInfo-original-*.png"));
        Assert.Equal(0, service.Calls);
    }

    [Fact]
    public void FailedProvenancePublicationRemovesItsOwnedBaseBeforeAnyNativeWrite() {
        using var fixture = new AmbientFixture();
        var configuration = NativeConfiguration(fixture);
        // A directory at the registry destination prevents the real atomic snapshot publication.
        Directory.CreateDirectory(Path.Combine(fixture.Directory, BgInfoOutputRegistry.FileName));
        var previousBase = Path.Combine(fixture.Directory, ".PowerBGInfo-original-desktop-" + Guid.NewGuid().ToString("N") + ".png");
        var previousBytes = new byte[] { 17, 34, 51 };
        File.WriteAllBytes(previousBase, previousBytes);
        var service = new FakeWallpaperService();

        var error = Assert.Throws<IOException>(() => new BgInfoGenerator(new ImageService(), service).Generate(configuration));

        Assert.Contains("background could not be recorded", error.Message, StringComparison.Ordinal);
        Assert.Equal(previousBase, Assert.Single(Directory.GetFiles(fixture.Directory, "*PowerBGInfo-original-*.png")));
        Assert.Equal(previousBytes, File.ReadAllBytes(previousBase));
        Assert.Equal(0, service.Calls);
    }

    [Fact]
    public void FailedNativeApplicationRetainsThePublishedBaseForRecoveryAndRepeat() {
        using var fixture = new AmbientFixture();
        var configuration = NativeConfiguration(fixture);
        var service = new FailingWallpaperService();
        var output = Path.Combine(fixture.Directory, configuration.OutputFileName!);

        Assert.Throws<InvalidOperationException>(() => new BgInfoGenerator(new ImageService(), service).Generate(configuration));

        Assert.Equal(1, service.Attempts);
        Assert.True(BgInfoOutputRegistry.TryGetSource(output, out var original));
        Assert.Equal(original, Assert.Single(Directory.GetFiles(fixture.Directory, "*PowerBGInfo-original-*.png")));
        Assert.True(File.Exists(original));
        var repeatConfiguration = BgInfoGenerator.ResolveDesktopDefaults(fixture.Configuration,
            State(output, DesktopWallpaperPosition.Stretch, imageOpacity: 1),
            BgInfoDesktopDefaultsTests.QualifiedTarget(targetIndex: 0));
        var repeat = new BgInfoGenerator(new ImageService(), new FakeWallpaperService()) { CurrentWallpaper = _ => output }
            .Generate(repeatConfiguration);

        Assert.Equal(output, repeat);
        Assert.Single(Directory.GetFiles(fixture.Directory, "*PowerBGInfo-original-*.png"));
    }

    [Fact]
    public void ExplicitCallerBackgroundKeepsItsExistingRepeatBehaviorWithoutNativeBaseFiles() {
        using var fixture = new AmbientFixture();
        var configuration = fixture.Configuration;
        configuration.Background = new BgInfoBackground { Width = 160, Height = 120, Color = ChartColors.Navy };
        var generator = new BgInfoGenerator(new ImageService(), new FakeWallpaperService());
        var first = generator.Generate(configuration);
        using var firstImage = BgInfoRasterImage.Load(first);
        var firstPixels = firstImage.ToRgbaImage().Pixels;
        generator.CurrentWallpaper = _ => first;

        var repeat = generator.Generate(configuration);

        using var repeatImage = BgInfoRasterImage.Load(repeat);
        Assert.Equal(firstPixels, repeatImage.ToRgbaImage().Pixels);
        Assert.Empty(Directory.GetFiles(fixture.Directory, "*PowerBGInfo-original-*.png"));
        Assert.True(BgInfoOutputRegistry.TryGetSource(repeat, out var original));
        Assert.Null(original);
        Assert.False(configuration.HasSynthesizedDesktopBackground);
    }

    private static BgInfoConfiguration NativeConfiguration(AmbientFixture fixture) =>
        BgInfoGenerator.ResolveDesktopDefaults(fixture.Configuration,
            State(background: Background("#12243c", DesktopBackgroundShading.Solid, "#c48e20"), imageOpacity: 0),
            BgInfoDesktopDefaultsTests.QualifiedTarget(targetIndex: 0));

    private static DesktopWallpaperState State(string? path = null, DesktopWallpaperPosition? position = null,
        DesktopWallpaperBackground? background = null, double? imageOpacity = null) =>
        (DesktopWallpaperState)typeof(DesktopWallpaperState)
            .GetConstructors(BindingFlags.Instance | BindingFlags.NonPublic).Single()
            .Invoke(new object?[] { DesktopOperationStatus.Success, "Native background fixture.", path, position, background, imageOpacity });

    private static DesktopWallpaperBackground Background(string primary, DesktopBackgroundShading shading, string secondary) =>
        (DesktopWallpaperBackground)typeof(DesktopWallpaperBackground)
            .GetConstructors(BindingFlags.Instance | BindingFlags.NonPublic).Single()
            .Invoke(new object?[] { primary, shading, secondary });

    private sealed class FailingWallpaperService : FakeWallpaperService, IWallpaperService {
        internal int Attempts { get; private set; }

        public new void SetWallpaper(int monitorIndex, string filePath, DesktopWallpaperPosition position) {
            Attempts++;
            throw new InvalidOperationException("Controlled native apply failure.");
        }
    }

    private sealed class AmbientFixture : IDisposable {
        internal AmbientFixture() {
            Directory = Path.Combine(Path.GetTempPath(), "bginfo-native-background-repeat-" + Guid.NewGuid().ToString("N"));
            System.IO.Directory.CreateDirectory(Directory);
        }

        internal string Directory { get; }
        internal BgInfoConfiguration Configuration {
            get {
                var configuration = new BgInfoConfiguration {
                    ConfigurationDirectory = Directory,
                    OutputFileName = "ambient.png",
                    Target = BgInfoTarget.Wallpaper,
                    ForceWallpaperRefresh = false,
                    UseScreenCoordinates = false,
                    Color = ChartColors.White,
                    ValueColor = ChartColors.White,
                    FontSize = 14,
                    ValueFontSize = 14,
                    SpaceX = 8,
                    SpaceY = 8
                };
                configuration.Entries.Add(new BgInfoEntry { Type = BgInfoEntryType.Value, Name = "Host", Value = "TEST" });
                return configuration;
            }
        }

        public void Dispose() => System.IO.Directory.Delete(Directory, recursive: true);
    }
}
