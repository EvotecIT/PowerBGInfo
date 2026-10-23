using ChartForgeX.Primitives;
using DesktopManager;
using System.Reflection;

namespace PowerBGInfo.Tests;

public sealed class BgInfoDesktopDefaultsTests {
    [Fact]
    public void OmittedFitRemainsImplicitThroughJsonAndPreservesNativeFitWithoutChangingTheCaller() {
        using var folder = new FixtureFolder();
        var original = new BgInfoConfiguration { ConfigurationDirectory = folder.Path };
        var configurationPath = Path.Combine(folder.Path, "implicit.json");
        BgInfoConfigurationJson.Save(original, configurationPath);
        var configuration = BgInfoConfigurationJson.Load(configurationPath);
        var state = WallpaperState(folder.CreateImage(), DesktopWallpaperPosition.Fit, imageOpacity: 1);

        var effective = BgInfoGenerator.ResolveDesktopDefaults(configuration, state, null);

        Assert.Equal(DesktopWallpaperPosition.Fit, effective.WallpaperFit);
        Assert.False(effective.HasExplicitWallpaperFit);
        Assert.NotSame(configuration, effective);
        Assert.False(configuration.HasExplicitWallpaperFit);
        Assert.Equal(DesktopWallpaperPosition.Center, configuration.WallpaperFit);
        Assert.Null(configuration.Background);
        Assert.False(original.HasExplicitWallpaperFit);
    }

    [Theory]
    [InlineData(DesktopWallpaperPosition.Center)]
    [InlineData(DesktopWallpaperPosition.Fill)]
    public void ExplicitFitWinsOverTheNativeFitAndSurvivesJson(DesktopWallpaperPosition fit) {
        using var folder = new FixtureFolder();
        var configuration = new BgInfoConfiguration {
            ConfigurationDirectory = folder.Path,
            WallpaperFit = fit
        };
        var configurationPath = Path.Combine(folder.Path, "explicit.json");
        BgInfoConfigurationJson.Save(configuration, configurationPath);
        var loaded = BgInfoConfigurationJson.Load(configurationPath);
        var state = WallpaperState(folder.CreateImage(), DesktopWallpaperPosition.Fit, imageOpacity: 1);

        var effective = BgInfoGenerator.ResolveDesktopDefaults(loaded, state, null);

        Assert.True(loaded.HasExplicitWallpaperFit);
        Assert.True(effective.HasExplicitWallpaperFit);
        Assert.Equal(fit, effective.WallpaperFit);
        Assert.Equal(fit, loaded.WallpaperFit);
    }

    [Theory]
    [InlineData(DesktopBackgroundShading.Solid)]
    [InlineData(DesktopBackgroundShading.Horizontal)]
    [InlineData(DesktopBackgroundShading.Vertical)]
    public void DisabledImageBackgroundRendersNativeColorsAndShadingAtTheQualifiedTargetSize(DesktopBackgroundShading shading) {
        var primary = ChartColor.FromHex("#12243c");
        var secondary = ChartColor.FromHex("#c48e20");
        var configuration = new BgInfoConfiguration { MonitorIndex = 2 };
        var state = WallpaperState(background: Background("#12243c", shading, "#c48e20"), imageOpacity: 0);

        var effective = BgInfoGenerator.ResolveDesktopDefaults(configuration, state, QualifiedMonitors());

        var background = Assert.IsType<BgInfoBackground>(effective.Background);
        Assert.Equal(160, background.Width);
        Assert.Equal(120, background.Height);
        Assert.Null(configuration.Background);
        using var rendered = BgInfoBackgroundRenderer.Render(background, background.Width, background.Height);
        var pixels = rendered.ToRgbaImage();
        if (shading == DesktopBackgroundShading.Solid) {
            BgInfoBackgroundTests.AssertNear(primary, BgInfoBackgroundTests.Pixel(pixels, 1, 60));
            BgInfoBackgroundTests.AssertNear(primary, BgInfoBackgroundTests.Pixel(pixels, 158, 60));
        } else if (shading == DesktopBackgroundShading.Horizontal) {
            BgInfoBackgroundTests.AssertNear(primary, BgInfoBackgroundTests.Pixel(pixels, 1, 60));
            BgInfoBackgroundTests.AssertNear(secondary, BgInfoBackgroundTests.Pixel(pixels, 158, 60));
            BgInfoBackgroundTests.AssertNear(BgInfoBackgroundTests.Pixel(pixels, 80, 1), BgInfoBackgroundTests.Pixel(pixels, 80, 118));
        } else {
            BgInfoBackgroundTests.AssertNear(primary, BgInfoBackgroundTests.Pixel(pixels, 80, 1));
            BgInfoBackgroundTests.AssertNear(secondary, BgInfoBackgroundTests.Pixel(pixels, 80, 118));
            BgInfoBackgroundTests.AssertNear(BgInfoBackgroundTests.Pixel(pixels, 1, 60), BgInfoBackgroundTests.Pixel(pixels, 158, 60));
        }
    }

    [Fact]
    public void ExplicitBackgroundColorOverridesTheNativeSolidColor() {
        var configuration = new BgInfoConfiguration { MonitorIndex = 2, BackgroundColor = ChartColors.DarkRed };
        var state = WallpaperState(background: Background("#12243c"), imageOpacity: 0);

        var effective = BgInfoGenerator.ResolveDesktopDefaults(configuration, state, QualifiedMonitors());

        var background = Assert.IsType<BgInfoBackground>(effective.Background);
        using var rendered = BgInfoBackgroundRenderer.Render(background, background.Width, background.Height);
        BgInfoBackgroundTests.AssertNear(ChartColors.DarkRed, BgInfoBackgroundTests.Pixel(rendered.ToRgbaImage(), 80, 60));
    }

    [Theory]
    [InlineData("image-opacity")]
    [InlineData("background-color")]
    [InlineData("physical-geometry")]
    public void IncompleteEffectiveSourceIsTypedUnsupportedBeforeAnyOutput(string missingFact) {
        using var folder = new FixtureFolder();
        var output = Path.Combine(folder.Path, "generated.png");
        var configuration = new BgInfoConfiguration {
            ConfigurationDirectory = folder.Path,
            OutputFileName = output,
            MonitorIndex = 2
        };
        var state = missingFact == "image-opacity"
            ? WallpaperState(folder.CreateImage(), DesktopWallpaperPosition.Fit)
            : WallpaperState(background: missingFact == "background-color" ? null : Background("#12243c"), imageOpacity: 0);
        var monitors = missingFact == "physical-geometry" ? null : QualifiedMonitors();

        var error = Assert.Throws<BgInfoWallpaperReadException>(() =>
            BgInfoGenerator.ResolveDesktopDefaults(configuration, state, monitors));

        Assert.Equal(DesktopOperationStatus.Unsupported, error.Status);
        Assert.False(string.IsNullOrWhiteSpace(error.Diagnostic));
        Assert.False(File.Exists(output));
        Assert.Null(configuration.Background);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void ExplicitImageOrBackgroundDoesNotRequireUnknownNativeSourceDefaults(bool useBackground) {
        using var folder = new FixtureFolder();
        var configuration = new BgInfoConfiguration();
        if (useBackground) configuration.Background = new BgInfoBackground { Width = 16, Height = 12, Color = ChartColors.DarkBlue };
        else configuration.FilePath = folder.CreateImage();
        var unknownSource = WallpaperState();

        var effective = BgInfoGenerator.ResolveDesktopDefaults(configuration, unknownSource, null);

        Assert.Same(configuration, effective);
    }

    private static BgInfoMonitor[] QualifiedMonitors() => new[] {
        new BgInfoMonitor { Index = 2, Id = "qualified-display", Width = 160, Height = 120, Scale = 1 }
    };

    // Native discovery is outside this consumer policy test. DesktopManager intentionally
    // exposes read-only DTOs, so construct the external boundary state without adding hooks.
    private static DesktopWallpaperState WallpaperState(string? path = null, DesktopWallpaperPosition? position = null,
        DesktopWallpaperBackground? background = null, double? imageOpacity = null) =>
        (DesktopWallpaperState)typeof(DesktopWallpaperState)
            .GetConstructors(BindingFlags.Instance | BindingFlags.NonPublic).Single()
            .Invoke(new object?[] { DesktopOperationStatus.Success, "Qualified external state fixture.", path, position, background, imageOpacity });

    private static DesktopWallpaperBackground Background(string primary, DesktopBackgroundShading shading = DesktopBackgroundShading.Solid,
        string? secondary = null) =>
        (DesktopWallpaperBackground)typeof(DesktopWallpaperBackground)
            .GetConstructors(BindingFlags.Instance | BindingFlags.NonPublic).Single()
            .Invoke(new object?[] { primary, shading, secondary });

    private sealed class FixtureFolder : IDisposable {
        internal FixtureFolder() {
            Path = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "bginfo-desktop-defaults-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(Path);
        }

        internal string Path { get; }

        internal string CreateImage() {
            var path = System.IO.Path.Combine(Path, "source.png");
            using var image = new BgInfoRasterImage();
            image.Create(path, 16, 12, ChartColors.DarkBlue);
            image.Save(path);
            return path;
        }

        public void Dispose() => Directory.Delete(Path, recursive: true);
    }
}
