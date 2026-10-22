using DesktopManager;
using System.Runtime.InteropServices;

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
/// Applies wallpaper through DesktopManager and preserves its Windows-only policy operations.
/// </summary>
public class WallpaperService : IWallpaperService
{
    private Monitors? _monitors;

    private Monitors Monitors {
        get {
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) throw new PlatformNotSupportedException("This operation requires the Windows desktop backend.");
            return _monitors ??= new Monitors();
        }
    }
    private readonly PortableDesktopService _desktop = new();

    internal bool ValidateTarget(BgInfoConfiguration config) {
        if (config.Target == BgInfoTarget.File) return false;
        var capabilities = _desktop.GetCapabilities();
        return ValidateTarget(config, RuntimeInformation.IsOSPlatform(OSPlatform.Windows), capabilities.CanSetWallpaper, capabilities.Message);
    }

    /// <summary>Separates Windows profile policy from the current session's interactive desktop.</summary>
    internal static bool ValidateTarget(BgInfoConfiguration config, bool isWindows, bool canSetWallpaper, string unavailableReason) {
        // File previews may retain inactive desktop options from a saved Windows
        // configuration, including when the CLI or Invoke-BGInfo uses NoApply.
        if (config.Target == BgInfoTarget.File) return false;
        if (!isWindows &&
            (config.Target.HasFlag(BgInfoTarget.LogonScreen) || config.ApplyToAllUsers)) {
            throw new PlatformNotSupportedException("Lock-screen and all-user wallpaper targets are supported only on Windows. Use Wallpaper for the current desktop or File for image generation.");
        }
        if (config.Target.HasFlag(BgInfoTarget.Wallpaper)) {
            if (!canSetWallpaper && !(isWindows && config.ApplyToAllUsers)) {
                throw new PlatformNotSupportedException($"Desktop wallpaper is unavailable: {unavailableReason} Use Target File to render without a desktop session.");
            }
        }
        return config.Target.HasFlag(BgInfoTarget.Wallpaper) && canSetWallpaper;
    }

    internal static string? GetCurrentWallpaper(int monitorIndex) {
        var desktop = new PortableDesktopService();
        var capabilities = desktop.GetCapabilities();
        if (!capabilities.CanReadWallpaper) return null;
        var id = ResolveDisplayId(monitorIndex, capabilities.SupportsPerDisplayWallpaper,
            ReadDisplayIds(desktop, capabilities.SupportsPerDisplayWallpaper));
        var state = desktop.GetWallpaper(id);
        return state.Succeeded ? state.FilePath : null;
    }

    internal static uint GetCurrentBackgroundColor() => RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
        ? new Monitors().GetBackgroundColor() : 0;

    // Readback and application use the same mapping: a missing explicit monitor
    // must never become a request for the backend's default or entire desktop.
    internal static string? ResolveDisplayId(int monitorIndex, bool supportsPerDisplay,
        IReadOnlyDictionary<int, string> displayIds) {
        if (supportsPerDisplay) {
            if (displayIds.TryGetValue(monitorIndex, out var id)) return id;
            if (displayIds.Count > 0) throw new ArgumentOutOfRangeException(nameof(monitorIndex), "The requested display is not attached.");
            if (monitorIndex != 0) throw new PlatformNotSupportedException("This backend cannot map monitor indices to display identities. Use MonitorIndex 0 for its current desktop target.");
        } else if (monitorIndex != 0) {
            throw new PlatformNotSupportedException("This desktop supports one wallpaper for the desktop, rather than individual monitor indices. Use MonitorIndex 0.");
        }
        return null;
    }

    private static IReadOnlyDictionary<int, string> ReadDisplayIds(PortableDesktopService desktop, bool supportsPerDisplay) =>
        supportsPerDisplay ? desktop.GetDisplays().ToDictionary(display => display.Index, display => display.Id)
            : new Dictionary<int, string>();

    /// <summary>Receives warnings, for example when the lock screen fallback is used on an edition that ignores it.</summary>
    public Action<string>? Warning { get; set; }

    /// <summary>
    /// Sets the wallpaper for a monitor with the specified position mode.
    /// </summary>
    /// <param name="monitorIndex">Monitor index to update.</param>
    /// <param name="filePath">Path to the image.</param>
    /// <param name="position">Wallpaper fit mode.</param>
    public void SetWallpaper(int monitorIndex, string filePath, DesktopWallpaperPosition position)
    {
        var capabilities = _desktop.GetCapabilities();
        var id = ResolveDisplayId(monitorIndex, capabilities.SupportsPerDisplayWallpaper,
            ReadDisplayIds(_desktop, capabilities.SupportsPerDisplayWallpaper));
        var result = _desktop.ApplyWallpaper(new DesktopWallpaperRequest { FilePath = filePath, DisplayId = id, Position = position });
        if (!result.Succeeded) throw new InvalidOperationException($"The wallpaper could not be set ({result.Status}): {result.Message}");
    }

    /// <summary>
    /// Sets the logon/lock screen wallpaper.
    /// </summary>
    /// <param name="filePath">Path to the image.</param>
    public void SetLogonWallpaper(string filePath)
    {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) throw new PlatformNotSupportedException("Lock-screen wallpaper is supported only on Windows.");
#if NET5_0_OR_GREATER
        if (!OperatingSystem.IsWindowsVersionAtLeast(10, 0, 10240)) throw new PlatformNotSupportedException("Lock-screen wallpaper requires Windows 10 or later.");
#endif
        var result = Monitors.SetLogonWallpaperWithResult(filePath, allowPolicyFallback: true);
        if (!result.Succeeded) {
            throw new InvalidOperationException($"The lock screen image could not be set: {result.ErrorMessage}");
        }
        var warning = LockScreenPolicy.GetFallbackWarning(result.Mechanism, LockScreenPolicy.ReadEditionId);
        if (warning != null) Warning?.Invoke(warning);
    }

    /// <summary>
    /// Sets the wallpaper for all user profiles.
    /// </summary>
    /// <param name="filePath">Path to the image.</param>
    /// <param name="position">Wallpaper fit mode.</param>
    /// <param name="includeDefaultUserProfile">Whether to include the default user profile.</param>
    public void SetWallpaperForAllUsers(string filePath, DesktopWallpaperPosition position, bool includeDefaultUserProfile)
    {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) throw new PlatformNotSupportedException("All-user wallpaper is supported only on Windows.");
        Monitors.SetWallpaperForAllUsers(filePath, position, includeDefaultUserProfile);
    }

    /// <summary>
    /// Gets the current desktop wallpaper slideshow configuration.
    /// </summary>
    /// <returns>Current slideshow state, options, tick interval, and source image paths.</returns>
    public DesktopWallpaperSlideshow GetWallpaperSlideshow()
    {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) return new DesktopWallpaperSlideshow();
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
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) throw new PlatformNotSupportedException("Wallpaper slideshows are supported only on Windows.");
        Monitors.SetWallpaperPosition(position);
        Monitors.StartWallpaperSlideshow(filePaths, options, slideshowTick);
    }


}
