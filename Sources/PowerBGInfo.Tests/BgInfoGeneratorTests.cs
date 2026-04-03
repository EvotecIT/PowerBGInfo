using DesktopManager;
using PowerBGInfo;
using System.Drawing;
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
