using DesktopManager;

namespace PowerBGInfo;

public partial class BgInfoConfiguration {
    private DesktopWallpaperPosition _wallpaperFit = DesktopWallpaperPosition.Center;
    internal bool HasExplicitWallpaperFit { get; private set; }

    /// <summary>Copies per-run desktop defaults without changing the caller's configuration or its explicit overrides.</summary>
    internal BgInfoConfiguration WithDesktopDefaults(DesktopWallpaperPosition? position, BgInfoBackground? background) {
        var effective = (BgInfoConfiguration)MemberwiseClone();
        if (!HasExplicitWallpaperFit && position.HasValue) effective._wallpaperFit = position.Value;
        if (background != null) effective.Background = background;
        return effective;
    }
}
