using DesktopManager;

#if NET5_0_OR_GREATER
using System.Runtime.Versioning;
#endif

namespace PowerBGInfo;

/// <summary>
/// Defines operations for setting wallpapers on a specific monitor.
/// </summary>
public interface IWallpaperService
{
    /// <summary>
    /// Sets the wallpaper for a monitor with the specified position mode.
    /// </summary>
    /// <param name="monitorIndex">Monitor index to update.</param>
    /// <param name="filePath">Path to the image.</param>
    /// <param name="position">Wallpaper fit mode.</param>
    void SetWallpaper(int monitorIndex, string filePath, DesktopWallpaperPosition position);
    /// <summary>
    /// Sets the logon/lock screen wallpaper.
    /// </summary>
    /// <param name="filePath">Path to the image.</param>
    void SetLogonWallpaper(string filePath);
    /// <summary>
    /// Sets the wallpaper for all user profiles.
    /// </summary>
    /// <param name="filePath">Path to the image.</param>
    /// <param name="position">Wallpaper fit mode.</param>
    /// <param name="includeDefaultUserProfile">Whether to include the default user profile.</param>
    void SetWallpaperForAllUsers(string filePath, DesktopWallpaperPosition position, bool includeDefaultUserProfile);
}

/// <summary>
/// Windows wallpaper service implementation using DesktopManager.
/// </summary>
public class WallpaperService : IWallpaperService
{
    private readonly Monitors _monitors = new();

    /// <summary>
    /// Sets the wallpaper for a monitor with the specified position mode.
    /// </summary>
    /// <param name="monitorIndex">Monitor index to update.</param>
    /// <param name="filePath">Path to the image.</param>
    /// <param name="position">Wallpaper fit mode.</param>
    public void SetWallpaper(int monitorIndex, string filePath, DesktopWallpaperPosition position)
    {
        _monitors.SetWallpaperPosition(position);
        _monitors.SetWallpaper(monitorIndex, filePath);
    }

    /// <summary>
    /// Sets the logon/lock screen wallpaper.
    /// </summary>
    /// <param name="filePath">Path to the image.</param>
#if NET5_0_OR_GREATER
    [SupportedOSPlatform("windows10.0.10240.0")]
#endif
    public void SetLogonWallpaper(string filePath)
    {
        _monitors.SetLogonWallpaper(filePath);
    }

    /// <summary>
    /// Sets the wallpaper for all user profiles.
    /// </summary>
    /// <param name="filePath">Path to the image.</param>
    /// <param name="position">Wallpaper fit mode.</param>
    /// <param name="includeDefaultUserProfile">Whether to include the default user profile.</param>
    public void SetWallpaperForAllUsers(string filePath, DesktopWallpaperPosition position, bool includeDefaultUserProfile)
    {
        _monitors.SetWallpaperForAllUsers(filePath, position, includeDefaultUserProfile);
    }
}
