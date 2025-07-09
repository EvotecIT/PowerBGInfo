using DesktopManager;
using PowerBGInfo;
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
            FilePath = Path.Combine("Examples","Samples","TapC-Evotec-2560x1080.jpg"),
            ConfigurationDirectory = Path.Combine(Path.GetTempPath(), "bginfo" + Path.GetRandomFileName())
        };
        config.Entries.Add(new BgInfoEntry{Type=BgInfoEntryType.Value, Name="Test", Value="1"});
        var path = generator.Generate(config);
        Assert.True(File.Exists(path));
    }
}

internal class FakeWallpaperService : IWallpaperService
{
    public int Calls { get; private set; }
    public void SetWallpaper(int monitorIndex, string filePath, DesktopManager.DesktopWallpaperPosition position)
    {
        Calls++;
    }
}
