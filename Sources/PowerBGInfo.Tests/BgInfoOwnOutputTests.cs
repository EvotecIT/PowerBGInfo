using ChartForgeX.Primitives;
using DesktopManager;

namespace PowerBGInfo.Tests;

public class BgInfoOwnOutputTests
{
    [Fact]
    public void RunOnItsOwnOutputDrawsOnTheOriginalAgain()
    {
        using var folder = new TempFolder();
        var basePath = folder.CreateImage("corporate.png", ChartColors.Navy);
        var first = Render(folder, basePath, "wallpaper.png", out _);
        var firstPixels = Pixels(first);

        // After the first run Windows reports the output as the current wallpaper.
        var second = Render(folder, first, "wallpaper.png", out var warnings);

        Assert.Equal(first, second);
        Assert.Equal(firstPixels, Pixels(second));
        Assert.Empty(warnings);
    }

    [Fact]
    public void DefaultOutputNameDoesNotGrowOnRepeatedRuns()
    {
        using var folder = new TempFolder();
        var basePath = folder.CreateImage("corporate.png", ChartColors.Navy);
        var first = Render(folder, basePath, null, out _);
        var second = Render(folder, first, null, out _);

        Assert.Equal(Path.Combine(folder.Path, "corporate_PowerBgInfo.png"), first);
        Assert.Equal(first, second);
        Assert.Equal(basePath, ReadRecordedSource(first));
    }

    [Fact]
    public void UnrecordedOwnOutputIsNotUsedAndWarns()
    {
        using var folder = new TempFolder();
        var basePath = folder.CreateImage("corporate.png", ChartColors.Navy);
        var first = Render(folder, basePath, null, out _);
        File.Delete(Path.Combine(folder.Path, BgInfoOutputRegistry.FileName));

        var second = Render(folder, first, "second.png", out var warnings, ChartColors.DarkRed);

        var warning = Assert.Single(warnings);
        Assert.Contains("drawn twice", warning);
        using var image = BgInfoRasterImage.Load(second);
        var pixel = image.ToRgbaImage().Pixels;
        Assert.Equal(new[] { ChartColors.DarkRed.R, ChartColors.DarkRed.G, ChartColors.DarkRed.B }, new[] { pixel[0], pixel[1], pixel[2] });
    }

    [Fact]
    public void SlideshowOfRenderedCopiesIsRenderedFromTheOriginals()
    {
        using var folder = new TempFolder();
        var one = folder.CreateImage("slide-one.png", ChartColors.Navy);
        var two = folder.CreateImage("slide-two.png", ChartColors.DarkGreen);
        var wallpaper = new FakeWallpaperService {
            Slideshow = new DesktopWallpaperSlideshow {
                ImagePaths = new[] { one, two },
                State = DesktopSlideshowState.Enabled | DesktopSlideshowState.Slideshow
            }
        };
        new BgInfoGenerator(new ImageService(), wallpaper).Generate(SlideshowConfiguration(folder));
        var firstRun = wallpaper.SlideshowPaths.ToArray();
        var firstPixels = firstRun.Select(Pixels).ToArray();

        // Windows now reports the rendered copies as the slideshow images.
        wallpaper.Slideshow = new DesktopWallpaperSlideshow {
            ImagePaths = firstRun,
            State = DesktopSlideshowState.Enabled | DesktopSlideshowState.Slideshow
        };
        new BgInfoGenerator(new ImageService(), wallpaper).Generate(SlideshowConfiguration(folder));

        Assert.Equal(firstRun, wallpaper.SlideshowPaths);
        Assert.Equal(firstPixels, wallpaper.SlideshowPaths.Select(Pixels).ToArray());
    }

    [Fact]
    public void BackgroundImageThatIsOwnOutputUsesTheOriginal()
    {
        using var folder = new TempFolder();
        var photo = folder.CreateImage("photo.png", ChartColors.Navy);
        string RenderBackground(string imagePath) {
            var config = Configuration(folder, null, "backdrop.png");
            config.Background = new BgInfoBackground { Width = 320, Height = 180, ImagePath = imagePath };
            return new BgInfoGenerator(new ImageService(), new FakeWallpaperService()).Generate(config);
        }

        var first = RenderBackground(photo);
        var firstPixels = Pixels(first);
        var second = RenderBackground(first);

        Assert.Equal(firstPixels, Pixels(second));
    }

    [Fact]
    public void ChosenFilePathIsUsedAsGivenEvenWhenPowerBGInfoRenderedIt()
    {
        using var folder = new TempFolder();
        var photo = folder.CreateImage("photo.png", ChartColors.Navy);
        var stage = Render(folder, photo, "stage.png", out _, filePath: photo);

        var final = Render(folder, null, "final.png", out var warnings, filePath: stage);

        Assert.Empty(warnings);
        Assert.Equal(stage, ReadRecordedSource(final));
    }

    [Fact]
    public void ChosenFilePathThatIsTheOutputItselfUsesTheOriginal()
    {
        using var folder = new TempFolder();
        var photo = folder.CreateImage("photo.png", ChartColors.Navy);
        var first = Render(folder, null, "wallpaper.png", out _, filePath: photo);
        var firstPixels = Pixels(first);

        var second = Render(folder, null, "wallpaper.png", out _, filePath: first);

        Assert.Equal(firstPixels, Pixels(second));
        Assert.Equal(photo, ReadRecordedSource(second));
    }

    [Fact]
    public void UnrecordedChosenFileThatIsTheOutputItselfRemainsTheBase()
    {
        using var folder = new TempFolder();
        var photo = folder.CreateImage("corporate.png", ChartColors.Navy);
        var first = Render(folder, null, "wallpaper.png", out _, filePath: photo);
        var firstPixels = Pixels(first);
        File.Delete(Path.Combine(folder.Path, BgInfoOutputRegistry.FileName));

        var second = Render(folder, null, "wallpaper.png", out _, filePath: first);

        Assert.Equal(firstPixels, Pixels(second));
    }

    [Fact]
    public void UnrecordedCustomSlideshowOutputIsRecognizedFromItsPlannedPath()
    {
        using var folder = new TempFolder();
        var output = folder.CreateImage("wallpaper_001.png", ChartColors.Navy);
        var warnings = new List<string>();
        var generator = new BgInfoGenerator(new ImageService(), new FakeWallpaperService()) {
            Warning = warnings.Add
        };
        var knownOutputPaths = new HashSet<string>(StringComparer.OrdinalIgnoreCase) {
            Path.GetFullPath(output)
        };

        var original = generator.ResolveOriginalImage(output, null, "The slideshow image", chosenByUser: false, knownOutputPaths: knownOutputPaths);

        Assert.Null(original);
        Assert.Single(warnings);
    }

    [Fact]
    public void RegistryRetainsMappingsForLargeSlideshows()
    {
        using var folder = new TempFolder();
        for (var index = 0; index < 300; index++)
        {
            BgInfoOutputRegistry.Record(
                Path.Combine(folder.Path, $"slide-{index:D3}.png"),
                Path.Combine(folder.Path, $"source-{index:D3}.png"));
        }

        Assert.True(BgInfoOutputRegistry.TryGetSource(
            Path.Combine(folder.Path, "slide-000.png"), out var source));
        Assert.Equal(Path.Combine(folder.Path, "source-000.png"), source);
    }

    [Theory]
    [InlineData("img0_PowerBgInfo.jpg", true)]
    [InlineData("img0_powerbginfo_003.png", true)]
    [InlineData("wallpaper_refresh_0123456789abcdef0123456789abcdef.png", true)]
    [InlineData("PowerBgInfo.png", true)]
    [InlineData("corporate.jpg", false)]
    public void RecognizesTheDefaultOutputNames(string fileName, bool expected)
    {
        Assert.Equal(expected, BgInfoOutputRegistry.HasOwnOutputName(Path.Combine(Path.GetTempPath(), fileName)));
    }

    private static string Render(TempFolder folder, string? currentWallpaper, string? outputFileName, out List<string> warnings,
        ChartColor? background = null, string? filePath = null)
    {
        var messages = new List<string>();
        var config = Configuration(folder, filePath, outputFileName);
        config.BackgroundColor = background;
        var generator = new BgInfoGenerator(new ImageService(), new FakeWallpaperService()) {
            Warning = messages.Add,
            CurrentWallpaper = _ => currentWallpaper
        };
        var path = generator.Generate(config);
        warnings = messages;
        return path;
    }

    private static BgInfoConfiguration Configuration(TempFolder folder, string? filePath, string? outputFileName)
    {
        var config = new BgInfoConfiguration {
            FilePath = filePath ?? string.Empty,
            OutputFileName = outputFileName ?? string.Empty,
            ConfigurationDirectory = folder.Path,
            Target = BgInfoTarget.File,
            Color = ChartColors.White,
            ValueColor = ChartColors.White,
            FontSize = 18,
            ValueFontSize = 18,
            PositionX = 10,
            PositionY = 10
        };
        config.Entries.Add(new BgInfoEntry { Type = BgInfoEntryType.Value, Name = "Host", Value = "LAB-01" });
        return config;
    }

    private static BgInfoConfiguration SlideshowConfiguration(TempFolder folder)
    {
        var config = Configuration(folder, null, "generated.png");
        config.Target = BgInfoTarget.Wallpaper;
        return config;
    }

    private static string? ReadRecordedSource(string outputPath) =>
        BgInfoOutputRegistry.TryGetSource(outputPath, out var source) ? source : null;

    private static byte[] Pixels(string path)
    {
        using var image = BgInfoRasterImage.Load(path);
        return image.ToRgbaImage().Pixels;
    }

    private sealed class TempFolder : IDisposable
    {
        public TempFolder()
        {
            Path = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "bginfo-own-" + System.IO.Path.GetRandomFileName());
            Directory.CreateDirectory(Path);
        }

        public string Path { get; }

        public string CreateImage(string name, ChartColor color)
        {
            var path = System.IO.Path.Combine(Path, name);
            using var image = new BgInfoRasterImage();
            image.Create(path, 320, 180, color);
            image.Save(path);
            return path;
        }

        public void Dispose()
        {
            try { Directory.Delete(Path, recursive: true); } catch (IOException) { }
        }
    }
}
