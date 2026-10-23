using DesktopManager;

namespace PowerBGInfo;

/// <summary>Preserves the desktop backend's unsuccessful wallpaper-read status and diagnostic.</summary>
public sealed class BgInfoWallpaperReadException : InvalidOperationException {
    /// <summary>Creates a wallpaper-read failure with the original native outcome and diagnostic.</summary>
    /// <param name="status">The unsuccessful desktop operation status.</param>
    /// <param name="diagnostic">The diagnostic supplied by the desktop backend.</param>
    public BgInfoWallpaperReadException(DesktopOperationStatus status, string diagnostic)
        : base($"The current desktop wallpaper could not be read ({status}): {diagnostic}") {
        Status = status;
        Diagnostic = diagnostic;
    }

    /// <summary>Gets the backend's operation status without translating unsupported reads into a missing image.</summary>
    public DesktopOperationStatus Status { get; }

    /// <summary>Gets the original backend diagnostic.</summary>
    public string Diagnostic { get; }
}
