using DesktopManager;
using PowerBGInfo;
using System.Drawing;
using System.IO;
using GdiImage = ImagePlayground.Gdi.Image;
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

        using var image = new GdiImage();
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
        var area = new RectangleF(0, 0, 400, 200);

        var point = BgInfoGenerator.ResolveChartPosition(area, 100, 50, BgInfoTextPosition.MiddleCenter, 15, 20);

        Assert.Equal(165f, point.X);
        Assert.Equal(95f, point.Y);
    }
}

internal class FakeWallpaperService : IWallpaperService
{
    public int Calls { get; private set; }
    public int LogonCalls { get; private set; }
    public int AllUsersCalls { get; private set; }
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
}
