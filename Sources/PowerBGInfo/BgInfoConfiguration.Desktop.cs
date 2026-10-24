using DesktopManager;

namespace PowerBGInfo;

public partial class BgInfoConfiguration {
    private DesktopWallpaperPosition _wallpaperFit = DesktopWallpaperPosition.Center;
    internal bool HasExplicitWallpaperFit { get; private set; }
    /// <summary>Marks a per-run native color/shading canvas whose pristine pixels must survive the first application.</summary>
    internal bool HasSynthesizedDesktopBackground { get; private set; }

    /// <summary>Copies per-run desktop defaults without changing the caller's configuration or its explicit overrides.</summary>
    internal BgInfoConfiguration WithDesktopDefaults(DesktopWallpaperPosition? position, BgInfoBackground? background) {
        var effective = (BgInfoConfiguration)MemberwiseClone();
        effective.HasSynthesizedDesktopBackground = background != null;
        if (!HasExplicitWallpaperFit && position.HasValue) effective._wallpaperFit = position.Value;
        if (background != null) effective.Background = background;
        return effective;
    }
}
