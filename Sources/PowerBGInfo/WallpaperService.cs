using DesktopManager;

namespace PowerBGInfo;

public interface IWallpaperService
{
    void SetWallpaper(int monitorIndex, string filePath, DesktopWallpaperPosition position);
}

public class WallpaperService : IWallpaperService
{
    private readonly Monitors _monitors = new();

    public void SetWallpaper(int monitorIndex, string filePath, DesktopWallpaperPosition position)
    {
        _monitors.SetWallpaperPosition(position);
        _monitors.SetWallpaper(monitorIndex, filePath);
    }
}
