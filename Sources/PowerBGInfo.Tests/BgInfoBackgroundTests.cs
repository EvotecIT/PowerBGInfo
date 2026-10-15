using ChartForgeX.Primitives;
using ChartForgeX.Raster;
using DesktopManager;

namespace PowerBGInfo.Tests;

public class BgInfoBackgroundTests
{
    private static readonly ChartColor Navy = ChartColor.FromRgb(10, 20, 60);
    private static readonly ChartColor Teal = ChartColor.FromRgb(20, 160, 150);

    [Fact]
    public void GradientRunsFromStartToEndColor()
    {
        var background = new BgInfoBackground {
            GradientStart = Navy,
            GradientEnd = Teal,
            GradientAngle = 0
        };

        using var image = BgInfoBackgroundRenderer.Render(background, 400, 100);
        var pixels = image.ToRgbaImage();

        AssertNear(Navy, Pixel(pixels, 1, 50));
        AssertNear(Teal, Pixel(pixels, 398, 50));
    }

    [Fact]
    public void GlowBrightensItsCenterAndFadesToTheBase()
    {
        var background = new BgInfoBackground { Color = Navy };
        background.Glows.Add(new BgInfoBackgroundGlow { X = 0.25, Y = 0.5, Radius = 0.2, Color = ChartColor.FromRgba(255, 255, 255, 200) });

        using var image = BgInfoBackgroundRenderer.Render(background, 400, 200);
        var pixels = image.ToRgbaImage();

        Assert.True(Pixel(pixels, 100, 100).R > 150, "The glow center must be lit.");
        AssertNear(Navy, Pixel(pixels, 390, 100));
    }

    [Theory]
    [InlineData(BgInfoBackgroundPattern.Dots)]
    [InlineData(BgInfoBackgroundPattern.Grid)]
    public void PatternInkLandsOnTheGridAndLeavesTheGapsClear(BgInfoBackgroundPattern pattern)
    {
        var background = new BgInfoBackground {
            Color = ChartColor.Black,
            Pattern = pattern,
            PatternColor = ChartColor.White,
            PatternSpacing = 40
        };

        using var image = BgInfoBackgroundRenderer.Render(background, 200, 200);
        var pixels = image.ToRgbaImage();

        // The first dot or grid crossing sits half a spacing in from the edge.
        Assert.True(Pixel(pixels, 20, 20).R > 100, $"{pattern} must paint at the first crossing.");
        AssertNear(ChartColor.Black, Pixel(pixels, 30, 30));
    }

    [Fact]
    public void ImageCoversTheBackdropAndTranslucentGradientTintsIt()
    {
        var source = Path.Combine(Path.GetTempPath(), "bginfo-background-" + Path.GetRandomFileName() + ".png");
        try
        {
            File.WriteAllBytes(source, ChartForgeX.Composition.ImageComposition.Create(4, 4, ChartColor.FromRgb(200, 0, 0)).ToPng());
            var background = new BgInfoBackground {
                Color = ChartColor.Black,
                ImagePath = source,
                GradientStart = ChartColor.FromRgba(0, 0, 0, 128),
                GradientEnd = ChartColor.FromRgba(0, 0, 0, 128)
            };

            using var image = BgInfoBackgroundRenderer.Render(background, 320, 180);
            var corner = Pixel(image.ToRgbaImage(), 319, 179);

            Assert.InRange(corner.R, 80, 120);
            Assert.True(corner.G < 10 && corner.B < 10);
        }
        finally
        {
            File.Delete(source);
        }
    }

    [Fact]
    public void BackgroundReplacesTheBaseImageAndSkipsTheSlideshow()
    {
        if (!OperatingSystem.IsWindows())
        {
            return;
        }

        var directory = Path.Combine(Path.GetTempPath(), "bginfo" + Path.GetRandomFileName());
        var wallpaper = new FakeWallpaperService {
            Slideshow = new DesktopWallpaperSlideshow {
                ImagePaths = new[] { Path.Combine(AppContext.BaseDirectory, "TapC-Evotec-2560x1080.jpg") },
                State = DesktopSlideshowState.Enabled | DesktopSlideshowState.Slideshow
            }
        };
        var config = new BgInfoConfiguration {
            ConfigurationDirectory = directory,
            OutputFileName = "generated.png",
            Target = BgInfoTarget.Wallpaper,
            ForceWallpaperRefresh = false,
            Background = new BgInfoBackground { Width = 640, Height = 360, Color = Navy }
        };

        try
        {
            var path = new BgInfoGenerator(new ImageService(), wallpaper).Generate(config);
            using var output = BgInfoRasterImage.Load(path);

            Assert.Equal(640, output.Width);
            Assert.Equal(360, output.Height);
            AssertNear(Navy, Pixel(output.ToRgbaImage(), 600, 340));
            Assert.Equal(0, wallpaper.SlideshowCalls);
            Assert.Equal(1, wallpaper.Calls);
        }
        finally
        {
            if (Directory.Exists(directory)) Directory.Delete(directory, true);
        }
    }

    [Fact]
    public void BackgroundRoundTripsThroughJson()
    {
        var directory = Path.Combine(Path.GetTempPath(), "bginfo-json-" + Path.GetRandomFileName());
        Directory.CreateDirectory(directory);
        var path = Path.Combine(directory, "background.json");
        try
        {
            var configuration = new BgInfoConfiguration {
                Background = new BgInfoBackground {
                    Width = 1920,
                    Height = 1080,
                    Color = Navy,
                    ImagePath = Path.Combine(directory, "wallpaper.jpg"),
                    ImageFit = BgInfoImageFit.Contain,
                    ImageOpacity = 0.4,
                    GradientStart = Navy,
                    GradientEnd = Teal,
                    GradientAngle = 90,
                    Pattern = BgInfoBackgroundPattern.Grid,
                    PatternColor = ChartColor.FromRgba(255, 255, 255, 16),
                    PatternSpacing = 64
                }
            };
            configuration.Background.Glows.Add(new BgInfoBackgroundGlow { X = 0.1, Y = 0.9, Radius = 0.6, Color = Teal });

            BgInfoConfigurationJson.Save(configuration, path);
            var loaded = BgInfoConfigurationJson.Load(path).Background;

            Assert.NotNull(loaded);
            Assert.Equal(1920, loaded!.Width);
            Assert.Equal(1080, loaded.Height);
            Assert.Equal(Navy.ToHexRgba(), loaded.Color.ToHexRgba());
            Assert.Equal(configuration.Background.ImagePath, loaded.ImagePath);
            Assert.Equal(BgInfoImageFit.Contain, loaded.ImageFit);
            Assert.Equal(0.4, loaded.ImageOpacity);
            Assert.Equal(Teal.ToHexRgba(), loaded.GradientEnd!.Value.ToHexRgba());
            Assert.Equal(90, loaded.GradientAngle);
            Assert.Equal(BgInfoBackgroundPattern.Grid, loaded.Pattern);
            Assert.Equal(64, loaded.PatternSpacing);
            var glow = Assert.Single(loaded.Glows);
            Assert.Equal(0.1, glow.X);
            Assert.Equal(0.9, glow.Y);
            Assert.Equal(0.6, glow.Radius);
            Assert.Equal(Teal.ToHexRgba(), glow.Color.ToHexRgba());
        }
        finally
        {
            Directory.Delete(directory, true);
        }
    }

    internal static ChartColor Pixel(RgbaImage image, int x, int y)
    {
        var offset = (y * image.Width + x) * 4;
        return ChartColor.FromRgba(image.Pixels[offset], image.Pixels[offset + 1], image.Pixels[offset + 2], image.Pixels[offset + 3]);
    }

    internal static void AssertNear(ChartColor expected, ChartColor actual, int tolerance = 6)
    {
        Assert.True(Math.Abs(expected.R - actual.R) <= tolerance &&
            Math.Abs(expected.G - actual.G) <= tolerance &&
            Math.Abs(expected.B - actual.B) <= tolerance,
            $"Expected about {expected.ToHexRgba()} but found {actual.ToHexRgba()}.");
    }
}
