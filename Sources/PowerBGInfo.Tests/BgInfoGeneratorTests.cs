using DesktopManager;
using PowerBGInfo;
using ChartForgeX.Composition;
using ChartForgeX.Primitives;
using Color = ChartForgeX.Primitives.ChartColors;
using System.IO;
using Xunit;

namespace PowerBGInfo.Tests;

public class BgInfoGeneratorTests
{
    [Fact]
    public void GenerateCreatesFile()
    {
        if (!OperatingSystem.IsWindows())
        {
            return;
        }
        var imageService = new ImageService();
        var wallpaperService = new FakeWallpaperService();
        var generator = new BgInfoGenerator(imageService, wallpaperService);
        var config = new BgInfoConfiguration
        {
            FilePath = Path.Combine(AppContext.BaseDirectory, "TapC-Evotec-2560x1080.jpg"),
            ConfigurationDirectory = Path.Combine(Path.GetTempPath(), "bginfo" + Path.GetRandomFileName())
        };
        config.Entries.Add(new BgInfoEntry{Type=BgInfoEntryType.Value, Name="Test", Value="1"});
        var path = generator.Generate(config);
        Assert.True(File.Exists(path));
    }

    [Fact]
    public void GenerateUsesSolidColorWhenWallpaperMissing()
    {
        if (!OperatingSystem.IsWindows())
        {
            return;
        }
        var imageService = new ImageService();
        var wallpaperService = new FakeWallpaperService();
        var generator = new BgInfoGenerator(imageService, wallpaperService);
        var config = new BgInfoConfiguration
        {
            FilePath = Path.Combine(Path.GetTempPath(), "missing-wallpaper.png"),
            OutputFileName = "solid-wallpaper.png",
            BackgroundColor = Color.DarkSlateBlue,
            ForceWallpaperRefresh = false,
            ConfigurationDirectory = Path.Combine(Path.GetTempPath(), "bginfo" + Path.GetRandomFileName())
        };
        config.Entries.Add(new BgInfoEntry { Type = BgInfoEntryType.Value, Name = "Test", Value = "1" });
        var path = generator.Generate(config);
        Assert.True(File.Exists(path));
    }

    [Fact]
    public void ResolveBaseImagePathReturnsNullWhenWallpaperLookupThrows()
    {
        if (!OperatingSystem.IsWindows())
        {
            return;
        }

        var config = new BgInfoConfiguration();
        var path = BgInfoGenerator.ResolveBaseImagePath(config, _ => throw new IOException("No wallpaper configured."));

        Assert.Null(path);
    }

    [Fact]
    public void GenerateCallsAllUsersWhenRequested()
    {
        if (!OperatingSystem.IsWindows())
        {
            return;
        }
        var imageService = new ImageService();
        var wallpaperService = new FakeWallpaperService();
        var generator = new BgInfoGenerator(imageService, wallpaperService);
        var config = new BgInfoConfiguration
        {
            FilePath = Path.Combine(AppContext.BaseDirectory, "TapC-Evotec-2560x1080.jpg"),
            ConfigurationDirectory = Path.Combine(Path.GetTempPath(), "bginfo" + Path.GetRandomFileName()),
            ApplyToAllUsers = true,
            ForceWallpaperRefresh = false
        };
        config.Entries.Add(new BgInfoEntry { Type = BgInfoEntryType.Value, Name = "Test", Value = "1" });
        generator.Generate(config);
        Assert.Equal(1, wallpaperService.AllUsersCalls);
        Assert.Equal(1, wallpaperService.Calls);
    }

    [Fact]
    public void GeneratePreservesWallpaperSlideshowWhenNoFilePathIsConfigured()
    {
        if (!OperatingSystem.IsWindows())
        {
            return;
        }

        var tempDirectory = Path.Combine(Path.GetTempPath(), "bginfo" + Path.GetRandomFileName());
        Directory.CreateDirectory(tempDirectory);
        var sourceOne = Path.Combine(tempDirectory, "slide-one.jpg");
        var sourceTwo = Path.Combine(tempDirectory, "slide-two.jpg");
        File.Copy(Path.Combine(AppContext.BaseDirectory, "TapC-Evotec-2560x1080.jpg"), sourceOne);
        File.Copy(Path.Combine(AppContext.BaseDirectory, "TapC-Evotec-2560x1080.jpg"), sourceTwo);

        var imageService = new ImageService();
        var wallpaperService = new FakeWallpaperService {
            Slideshow = new DesktopWallpaperSlideshow {
                ImagePaths = new[] { sourceOne, sourceTwo },
                State = DesktopSlideshowState.Enabled | DesktopSlideshowState.Slideshow,
                Options = DesktopSlideshowOptions.ShuffleImages,
                SlideshowTick = 600000
            }
        };
        var generator = new BgInfoGenerator(imageService, wallpaperService);
        var config = new BgInfoConfiguration
        {
            OutputFileName = "generated.png",
            ConfigurationDirectory = tempDirectory,
            WallpaperFit = DesktopWallpaperPosition.Fill
        };
        config.Entries.Add(new BgInfoEntry { Type = BgInfoEntryType.Value, Name = "Test", Value = "1" });

        var path = generator.Generate(config);

        Assert.True(File.Exists(path));
        Assert.Equal(1, wallpaperService.SlideshowCalls);
        Assert.Equal(0, wallpaperService.Calls);
        Assert.Equal(DesktopWallpaperPosition.Fill, wallpaperService.SlideshowPosition);
        Assert.Equal(DesktopSlideshowOptions.ShuffleImages, wallpaperService.SlideshowOptions);
        Assert.Equal(600000u, wallpaperService.SlideshowTick);
        Assert.Equal(2, wallpaperService.SlideshowPaths.Count);
        Assert.All(wallpaperService.SlideshowPaths, generated => Assert.True(File.Exists(generated)));
        Assert.EndsWith("generated_001.png", wallpaperService.SlideshowPaths[0]);
        Assert.EndsWith("generated_002.png", wallpaperService.SlideshowPaths[1]);
    }

    [Fact]
    public void GeneratePreservesWallpaperSlideshowFromDirectorySources()
    {
        if (!OperatingSystem.IsWindows())
        {
            return;
        }

        var tempDirectory = Path.Combine(Path.GetTempPath(), "bginfo" + Path.GetRandomFileName());
        var sourceDirectory = Path.Combine(tempDirectory, "slides");
        Directory.CreateDirectory(sourceDirectory);
        File.Copy(Path.Combine(AppContext.BaseDirectory, "TapC-Evotec-2560x1080.jpg"), Path.Combine(sourceDirectory, "slide-one.jpg"));
        File.Copy(Path.Combine(AppContext.BaseDirectory, "TapC-Evotec-2560x1080.jpg"), Path.Combine(sourceDirectory, "slide-two.png"));
        File.WriteAllText(Path.Combine(sourceDirectory, "notes.txt"), "not an image");

        var imageService = new ImageService();
        var wallpaperService = new FakeWallpaperService {
            Slideshow = new DesktopWallpaperSlideshow {
                ImagePaths = new[] { sourceDirectory },
                State = DesktopSlideshowState.Enabled | DesktopSlideshowState.Slideshow
            }
        };
        var generator = new BgInfoGenerator(imageService, wallpaperService);
        var config = new BgInfoConfiguration
        {
            OutputFileName = "directory-slides.png",
            ConfigurationDirectory = tempDirectory
        };
        config.Entries.Add(new BgInfoEntry { Type = BgInfoEntryType.Value, Name = "Test", Value = "1" });

        generator.Generate(config);

        Assert.Equal(1, wallpaperService.SlideshowCalls);
        Assert.Equal(2, wallpaperService.SlideshowPaths.Count);
        Assert.All(wallpaperService.SlideshowPaths, generated => Assert.True(File.Exists(generated)));
    }

    [Fact]
    public void GenerateDoesNotPreserveWallpaperSlideshowWhenItIsNotRunning()
    {
        if (!OperatingSystem.IsWindows())
        {
            return;
        }

        var tempDirectory = Path.Combine(Path.GetTempPath(), "bginfo" + Path.GetRandomFileName());
        Directory.CreateDirectory(tempDirectory);
        var source = Path.Combine(tempDirectory, "slide-one.jpg");
        File.Copy(Path.Combine(AppContext.BaseDirectory, "TapC-Evotec-2560x1080.jpg"), source);

        var imageService = new ImageService();
        var wallpaperService = new FakeWallpaperService {
            Slideshow = new DesktopWallpaperSlideshow {
                ImagePaths = new[] { source },
                State = DesktopSlideshowState.Enabled
            }
        };
        var generator = new BgInfoGenerator(imageService, wallpaperService);
        var config = new BgInfoConfiguration
        {
            OutputFileName = "static-fallback.png",
            ConfigurationDirectory = tempDirectory,
            BackgroundColor = Color.Black,
            ForceWallpaperRefresh = false
        };
        config.Entries.Add(new BgInfoEntry { Type = BgInfoEntryType.Value, Name = "Test", Value = "1" });

        var path = generator.Generate(config);

        Assert.True(File.Exists(path));
        Assert.Equal(0, wallpaperService.SlideshowCalls);
    }

    [Fact]
    public void GeneratePreservesWallpaperSlideshowWithoutDuplicatingChartHistory()
    {
        if (!OperatingSystem.IsWindows())
        {
            return;
        }

        var tempDirectory = Path.Combine(Path.GetTempPath(), "bginfo" + Path.GetRandomFileName());
        Directory.CreateDirectory(tempDirectory);
        var sourceOne = Path.Combine(tempDirectory, "slide-one.jpg");
        var sourceTwo = Path.Combine(tempDirectory, "slide-two.jpg");
        File.Copy(Path.Combine(AppContext.BaseDirectory, "TapC-Evotec-2560x1080.jpg"), sourceOne);
        File.Copy(Path.Combine(AppContext.BaseDirectory, "TapC-Evotec-2560x1080.jpg"), sourceTwo);

        var imageService = new ImageService();
        var wallpaperService = new FakeWallpaperService {
            Slideshow = new DesktopWallpaperSlideshow {
                ImagePaths = new[] { sourceOne, sourceTwo },
                State = DesktopSlideshowState.Enabled | DesktopSlideshowState.Slideshow
            }
        };
        var generator = new BgInfoGenerator(imageService, wallpaperService);
        var config = new BgInfoConfiguration
        {
            OutputFileName = "chart-slideshow.png",
            ConfigurationDirectory = tempDirectory
        };
        config.Entries.Add(new BgInfoEntry { Type = BgInfoEntryType.Value, Name = "Test", Value = "1" });
        config.Charts.Add(new BgInfoChart
        {
            Id = "cpu",
            Title = "CPU",
            Kind = BgInfoChartKind.Sparkline,
            Values = new[] { 10d },
            MaxPoints = 10
        });

        generator.Generate(config);

        var historyPath = Path.Combine(tempDirectory, "Charts", "cpu.txt");
        Assert.True(File.Exists(historyPath));
        Assert.Single(File.ReadAllLines(historyPath));
        Assert.Equal(2, wallpaperService.SlideshowPaths.Count);
    }

    [Fact]
    public void GenerateUsesStaticWallpaperWhenFilePathIsConfigured()
    {
        if (!OperatingSystem.IsWindows())
        {
            return;
        }

        var imageService = new ImageService();
        var wallpaperService = new FakeWallpaperService {
            Slideshow = new DesktopWallpaperSlideshow {
                ImagePaths = new[] { Path.Combine(AppContext.BaseDirectory, "TapC-Evotec-2560x1080.jpg") },
                State = DesktopSlideshowState.Enabled | DesktopSlideshowState.Slideshow,
                Options = DesktopSlideshowOptions.ShuffleImages,
                SlideshowTick = 600000
            }
        };
        var generator = new BgInfoGenerator(imageService, wallpaperService);
        var config = new BgInfoConfiguration
        {
            FilePath = Path.Combine(AppContext.BaseDirectory, "TapC-Evotec-2560x1080.jpg"),
            ConfigurationDirectory = Path.Combine(Path.GetTempPath(), "bginfo" + Path.GetRandomFileName()),
            ForceWallpaperRefresh = false
        };
        config.Entries.Add(new BgInfoEntry { Type = BgInfoEntryType.Value, Name = "Test", Value = "1" });

        generator.Generate(config);

        Assert.Equal(1, wallpaperService.Calls);
        Assert.Equal(0, wallpaperService.SlideshowCalls);
    }

    [Fact]
    public void GenerateCallsLogonWallpaperWhenRequested()
    {
        if (!OperatingSystem.IsWindows())
        {
            return;
        }

        var imageService = new ImageService();
        var wallpaperService = new FakeWallpaperService();
        var generator = new BgInfoGenerator(imageService, wallpaperService);
        var config = new BgInfoConfiguration
        {
            FilePath = Path.Combine(AppContext.BaseDirectory, "TapC-Evotec-2560x1080.jpg"),
            ConfigurationDirectory = Path.Combine(Path.GetTempPath(), "bginfo" + Path.GetRandomFileName()),
            Target = BgInfoTarget.LogonScreen
        };
        config.Entries.Add(new BgInfoEntry { Type = BgInfoEntryType.Value, Name = "Test", Value = "1" });

        generator.Generate(config);

        Assert.Equal(1, wallpaperService.LogonCalls);
        Assert.Equal(0, wallpaperService.Calls);
    }

    [Fact]
    public void GenerateRendersChartAndStoresHistory()
    {
        if (!OperatingSystem.IsWindows())
        {
            return;
        }
        var imageService = new ImageService();
        var wallpaperService = new FakeWallpaperService();
        var generator = new BgInfoGenerator(imageService, wallpaperService);
        var configDir = Path.Combine(Path.GetTempPath(), "bginfo" + Path.GetRandomFileName());
        var config = new BgInfoConfiguration
        {
            FilePath = Path.Combine(Path.GetTempPath(), "missing-wallpaper.png"),
            OutputFileName = "chart-wallpaper.png",
            BackgroundColor = Color.Black,
            ForceWallpaperRefresh = false,
            ConfigurationDirectory = configDir
        };
        config.Entries.Add(new BgInfoEntry { Type = BgInfoEntryType.Value, Name = "Test", Value = "1" });
        config.Charts.Add(new BgInfoChart
        {
            Id = "cpu",
            Title = "CPU",
            Kind = BgInfoChartKind.Sparkline,
            Values = new[] { 10d, 20d, 30d },
            MaxPoints = 5,
            OffsetX = 20,
            OffsetY = 20,
            Anchor = BgInfoTextPosition.BottomLeft
        });

        var path = generator.Generate(config);
        Assert.True(File.Exists(path));
        var historyPath = Path.Combine(configDir, "Charts", "cpu.txt");
        Assert.True(File.Exists(historyPath));
    }

    [Fact]
    public void GenerateUsesDistinctHistoryFilesForChartsWithSameTitle()
    {
        if (!OperatingSystem.IsWindows())
        {
            return;
        }

        var imageService = new ImageService();
        var wallpaperService = new FakeWallpaperService();
        var generator = new BgInfoGenerator(imageService, wallpaperService);
        var configDir = Path.Combine(Path.GetTempPath(), "bginfo" + Path.GetRandomFileName());
        var config = new BgInfoConfiguration
        {
            FilePath = Path.Combine(Path.GetTempPath(), "missing-wallpaper.png"),
            OutputFileName = "chart-history.png",
            BackgroundColor = Color.Black,
            ForceWallpaperRefresh = false,
            ConfigurationDirectory = configDir
        };
        config.Entries.Add(new BgInfoEntry { Type = BgInfoEntryType.Value, Name = "Test", Value = "1" });
        config.Charts.Add(new BgInfoChart
        {
            Title = "CPU",
            Kind = BgInfoChartKind.Sparkline,
            Values = new[] { 10d, 20d },
            MaxPoints = 5
        });
        config.Charts.Add(new BgInfoChart
        {
            Title = "CPU",
            Kind = BgInfoChartKind.Bar,
            Values = new[] { 30d, 40d },
            MaxPoints = 5
        });

        generator.Generate(config);

        Assert.True(File.Exists(Path.Combine(configDir, "Charts", "CPU_0.txt")));
        Assert.True(File.Exists(Path.Combine(configDir, "Charts", "CPU_1.txt")));
    }

    [Fact]
    public void GenerateUsesFallbackMonitorSizeForScreenCoordinates()
    {
        if (!OperatingSystem.IsWindows())
        {
            return;
        }
        var imageService = new ImageService();
        var wallpaperService = new FakeWallpaperService();
        var generator = new BgInfoGenerator(imageService, wallpaperService);
        var config = new BgInfoConfiguration
        {
            FilePath = Path.Combine(Path.GetTempPath(), "missing-wallpaper.png"),
            OutputFileName = "screen-coordinates.png",
            BackgroundColor = Color.Black,
            ForceWallpaperRefresh = false,
            UseScreenCoordinates = true,
            MonitorIndex = 999,
            ConfigurationDirectory = Path.Combine(Path.GetTempPath(), "bginfo" + Path.GetRandomFileName())
        };
        config.Entries.Add(new BgInfoEntry { Type = BgInfoEntryType.Value, Name = "Test", Value = "1" });

        var path = generator.Generate(config);
        Assert.True(File.Exists(path));
    }

    [Fact]
    public void WrapTextLinesSplitsLongValueIntoMultipleLines()
    {
        if (!OperatingSystem.IsWindows())
        {
            return;
        }

        using var image = new BgInfoRasterImage();
        image.Create(Path.Combine(Path.GetTempPath(), Path.GetRandomFileName() + ".png"), 800, 600, Color.Black);

        var lines = BgInfoGenerator.WrapTextLines(
            image,
            "This is a very long description value that should wrap across multiple lines.",
            120,
            16f,
            "Calibri");

        Assert.True(lines.Count > 1);
        foreach (var line in lines)
        {
            Assert.True(image.GetTextSize(line, 16f, "Calibri").Width <= 120.5f);
        }
    }

    [Fact]
    public void ResolveChartPositionAppliesOffsetsForCenteredAnchors()
    {
        var area = new ChartRect(0, 0, 400, 200);

        var point = BgInfoGenerator.ResolveChartPosition(area, 100, 50, BgInfoTextPosition.MiddleCenter, 15, 20);

        Assert.Equal(165f, point.X);
        Assert.Equal(95f, point.Y);
    }

    [Theory]
    [InlineData(null, ".png")]
    [InlineData("", ".png")]
    [InlineData(".jpg", ".jpg")]
    [InlineData(".jpeg", ".jpg")]
    [InlineData(".jpe", ".jpg")]
    [InlineData(".jfif", ".jpg")]
    [InlineData(".gif", ".png")]
    [InlineData(".dib", ".png")]
    [InlineData(".wdp", ".png")]
    [InlineData(".pnm", ".ppm")]
    [InlineData(".tif", ".tiff")]
    public void NormalizeOutputImageExtensionReturnsChartForgeXWritableExtension(string? extension, string expected)
    {
        Assert.Equal(expected, BgInfoGenerator.NormalizeOutputImageExtension(extension));
    }

    [Fact]
    public void GenerateNormalizesExplicitLegacyOutputFileName()
    {
        if (!OperatingSystem.IsWindows())
        {
            return;
        }

        var tempDirectory = Path.Combine(Path.GetTempPath(), "bginfo" + Path.GetRandomFileName());
        var imageService = new ImageService();
        var wallpaperService = new FakeWallpaperService();
        var generator = new BgInfoGenerator(imageService, wallpaperService);
        var config = new BgInfoConfiguration
        {
            FilePath = Path.Combine(Path.GetTempPath(), "missing-wallpaper.png"),
            OutputFileName = "wallpaper.gif",
            BackgroundColor = Color.Black,
            ForceWallpaperRefresh = false,
            ConfigurationDirectory = tempDirectory
        };
        config.Entries.Add(new BgInfoEntry { Type = BgInfoEntryType.Value, Name = "Test", Value = "1" });

        var path = generator.Generate(config);

        Assert.True(File.Exists(path));
        Assert.Equal(Path.Combine(tempDirectory, "wallpaper.png"), path);
    }

    [Fact]
    public void GeneratePreservesWallpaperSlideshowNormalizesExplicitLegacyOutputFileName()
    {
        if (!OperatingSystem.IsWindows())
        {
            return;
        }

        var tempDirectory = Path.Combine(Path.GetTempPath(), "bginfo" + Path.GetRandomFileName());
        Directory.CreateDirectory(tempDirectory);
        var sourceOne = Path.Combine(tempDirectory, "slide-one.jpg");
        var sourceTwo = Path.Combine(tempDirectory, "slide-two.jpg");
        File.Copy(Path.Combine(AppContext.BaseDirectory, "TapC-Evotec-2560x1080.jpg"), sourceOne);
        File.Copy(Path.Combine(AppContext.BaseDirectory, "TapC-Evotec-2560x1080.jpg"), sourceTwo);

        var imageService = new ImageService();
        var wallpaperService = new FakeWallpaperService {
            Slideshow = new DesktopWallpaperSlideshow {
                ImagePaths = new[] { sourceOne, sourceTwo },
                State = DesktopSlideshowState.Enabled | DesktopSlideshowState.Slideshow
            }
        };
        var generator = new BgInfoGenerator(imageService, wallpaperService);
        var config = new BgInfoConfiguration
        {
            OutputFileName = "slides.gif",
            ConfigurationDirectory = tempDirectory
        };
        config.Entries.Add(new BgInfoEntry { Type = BgInfoEntryType.Value, Name = "Test", Value = "1" });

        generator.Generate(config);

        Assert.Equal(2, wallpaperService.SlideshowPaths.Count);
        Assert.EndsWith("slides_001.png", wallpaperService.SlideshowPaths[0]);
        Assert.EndsWith("slides_002.png", wallpaperService.SlideshowPaths[1]);
        Assert.All(wallpaperService.SlideshowPaths, generated => Assert.True(File.Exists(generated)));
    }

    [Theory]
    [InlineData(".jpe")]
    [InlineData(".jfif")]
    public void RasterImageSaveSupportsJpegAliasExtensions(string extension)
    {
        var directory = Path.Combine(Path.GetTempPath(), "bginfo" + Path.GetRandomFileName());
        Directory.CreateDirectory(directory);
        var path = Path.Combine(directory, "wallpaper" + extension);

        using var image = new BgInfoRasterImage();
        image.Create(path, 8, 8, Color.Navy);
        image.Save(path);

        Assert.True(File.Exists(path));
        Assert.True(new FileInfo(path).Length > 0);
    }

    [Fact]
    public void RasterImageLoadSupportsGifWithoutPlatformDrawing()
    {
        var directory = Path.Combine(Path.GetTempPath(), "bginfo" + Path.GetRandomFileName());
        Directory.CreateDirectory(directory);
        var path = Path.Combine(directory, "legacy.gif");
        File.WriteAllBytes(path, ImageComposition.Create(6, 4, Color.DarkGreen).ToGif());

        using var image = BgInfoRasterImage.Load(path);

        Assert.Equal(6, image.Width);
        Assert.Equal(4, image.Height);
    }

    [Fact]
    public void GenerateLoadsLegacyBaseImageBeforeNormalizingOutputExtension()
    {
        var tempDirectory = Path.Combine(Path.GetTempPath(), "bginfo" + Path.GetRandomFileName());
        Directory.CreateDirectory(tempDirectory);
        var sourcePath = Path.Combine(tempDirectory, "legacy.gif");
        File.WriteAllBytes(sourcePath, ImageComposition.Create(12, 8, Color.MidnightBlue).ToGif());

        var imageService = new ImageService();
        var wallpaperService = new FakeWallpaperService();
        var generator = new BgInfoGenerator(imageService, wallpaperService);
        var config = new BgInfoConfiguration
        {
            FilePath = sourcePath,
            ConfigurationDirectory = tempDirectory,
            ForceWallpaperRefresh = false
        };

        var path = generator.Generate(config);

        Assert.True(File.Exists(path));
        Assert.EndsWith("_PowerBgInfo.png", path);
    }
}

internal class FakeWallpaperService : IWallpaperService
{
    public int Calls { get; private set; }
    public int LogonCalls { get; private set; }
    public int AllUsersCalls { get; private set; }
    public int SlideshowCalls { get; private set; }
    public DesktopWallpaperSlideshow Slideshow { get; set; } = new();
    public List<string> SlideshowPaths { get; } = new();
    public DesktopWallpaperPosition SlideshowPosition { get; private set; }
    public DesktopSlideshowOptions SlideshowOptions { get; private set; }
    public uint SlideshowTick { get; private set; }

    public void SetWallpaper(int monitorIndex, string filePath, DesktopManager.DesktopWallpaperPosition position)
    {
        Calls++;
    }

    public void SetLogonWallpaper(string filePath)
    {
        LogonCalls++;
    }

    public void SetWallpaperForAllUsers(string filePath, DesktopWallpaperPosition position, bool includeDefaultUserProfile)
    {
        AllUsersCalls++;
    }

    public DesktopWallpaperSlideshow GetWallpaperSlideshow()
    {
        return Slideshow;
    }

    public void StartWallpaperSlideshow(IEnumerable<string> filePaths, DesktopWallpaperPosition position, DesktopSlideshowOptions options, uint slideshowTick)
    {
        SlideshowCalls++;
        SlideshowPaths.Clear();
        SlideshowPaths.AddRange(filePaths);
        SlideshowPosition = position;
        SlideshowOptions = options;
        SlideshowTick = slideshowTick;
    }
}
