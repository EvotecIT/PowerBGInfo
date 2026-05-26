using DesktopManager;
using Microsoft.Win32;

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
    /// <summary>
    /// Gets the current desktop wallpaper slideshow configuration.
    /// </summary>
    /// <returns>Current slideshow state, options, tick interval, and source image paths.</returns>
    DesktopWallpaperSlideshow GetWallpaperSlideshow();
    /// <summary>
    /// Starts a wallpaper slideshow with generated images while preserving slideshow options.
    /// </summary>
    /// <param name="filePaths">Generated slideshow image paths.</param>
    /// <param name="position">Wallpaper fit mode.</param>
    /// <param name="options">Slideshow options to apply.</param>
    /// <param name="slideshowTick">Slideshow interval in milliseconds.</param>
    void StartWallpaperSlideshow(IEnumerable<string> filePaths, DesktopWallpaperPosition position, DesktopSlideshowOptions options, uint slideshowTick);
}

/// <summary>
/// Windows wallpaper service implementation using DesktopManager.
/// </summary>
public class WallpaperService : IWallpaperService
{
    private Monitors? _monitors;

    private Monitors Monitors => _monitors ??= new Monitors();

    /// <summary>
    /// Sets the wallpaper for a monitor with the specified position mode.
    /// </summary>
    /// <param name="monitorIndex">Monitor index to update.</param>
    /// <param name="filePath">Path to the image.</param>
    /// <param name="position">Wallpaper fit mode.</param>
    public void SetWallpaper(int monitorIndex, string filePath, DesktopWallpaperPosition position)
    {
        Monitors.SetWallpaperPosition(position);
        Monitors.SetWallpaper(monitorIndex, filePath);
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
        try {
            Monitors.SetLogonWallpaper(filePath);
        } catch (InvalidOperationException ex) when (IsWinRtMissing(ex)) {
            SetLogonWallpaperFallback(filePath);
        }
    }

    /// <summary>
    /// Sets the wallpaper for all user profiles.
    /// </summary>
    /// <param name="filePath">Path to the image.</param>
    /// <param name="position">Wallpaper fit mode.</param>
    /// <param name="includeDefaultUserProfile">Whether to include the default user profile.</param>
    public void SetWallpaperForAllUsers(string filePath, DesktopWallpaperPosition position, bool includeDefaultUserProfile)
    {
        Monitors.SetWallpaperForAllUsers(filePath, position, includeDefaultUserProfile);
    }

    /// <summary>
    /// Gets the current desktop wallpaper slideshow configuration.
    /// </summary>
    /// <returns>Current slideshow state, options, tick interval, and source image paths.</returns>
    public DesktopWallpaperSlideshow GetWallpaperSlideshow()
    {
        return Monitors.GetWallpaperSlideshow();
    }

    /// <summary>
    /// Starts a wallpaper slideshow with generated images while preserving slideshow options.
    /// </summary>
    /// <param name="filePaths">Generated slideshow image paths.</param>
    /// <param name="position">Wallpaper fit mode.</param>
    /// <param name="options">Slideshow options to apply.</param>
    /// <param name="slideshowTick">Slideshow interval in milliseconds.</param>
    public void StartWallpaperSlideshow(IEnumerable<string> filePaths, DesktopWallpaperPosition position, DesktopSlideshowOptions options, uint slideshowTick)
    {
        Monitors.SetWallpaperPosition(position);
        Monitors.StartWallpaperSlideshow(filePaths, options, slideshowTick);
    }

    private static bool IsWinRtMissing(InvalidOperationException ex)
    {
        return ex.Message.Contains("LockScreen type not found", StringComparison.OrdinalIgnoreCase)
            || ex.Message.Contains("StorageFile type not found", StringComparison.OrdinalIgnoreCase)
            || ex.Message.Contains("GetFileFromPathAsync method not found", StringComparison.OrdinalIgnoreCase)
            || ex.Message.Contains("SetImageFileAsync method not found", StringComparison.OrdinalIgnoreCase)
            || ex.Message.Contains("AsTask method not found", StringComparison.OrdinalIgnoreCase)
            || ex.Message.Contains("GetImageStream method not found", StringComparison.OrdinalIgnoreCase)
            || ex.Message.Contains("AsStreamForRead method not found", StringComparison.OrdinalIgnoreCase);
    }

    private static void SetLogonWallpaperFallback(string filePath)
    {
        using RegistryKey? key = Registry.LocalMachine.CreateSubKey(
            @"SOFTWARE\Policies\Microsoft\Windows\Personalization");
        key?.SetValue("LockScreenImage", filePath, RegistryValueKind.String);
    }
}
