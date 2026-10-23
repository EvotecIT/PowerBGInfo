using DesktopManager;

namespace PowerBGInfo;

public partial class BgInfoGenerator {
    /// <summary>Uses only DesktopManager's effective source metadata; unknown state never becomes an invented backdrop.</summary>
    internal static BgInfoConfiguration ResolveDesktopDefaults(BgInfoConfiguration config, DesktopWallpaperState state,
        IReadOnlyList<BgInfoMonitor>? monitors) {
        WallpaperService.RequireSuccessfulRead(state);
        if (config.Background != null || !string.IsNullOrWhiteSpace(config.FilePath)) return config;

        if (!string.IsNullOrWhiteSpace(state.FilePath)) {
            if (!File.Exists(state.FilePath)) {
                throw new BgInfoWallpaperReadException(DesktopOperationStatus.Failed, "The effective desktop image is not an existing local file.");
            }
            if (state.ImageOpacity != 1) {
                throw UnsupportedDesktopDefault("The effective desktop image opacity is unknown or translucent. Choose an explicit FilePath or Background.");
            }
            if (!config.HasExplicitWallpaperFit && (!state.Position.HasValue || state.Position == DesktopWallpaperPosition.Span)) {
                throw UnsupportedDesktopDefault("The effective desktop fit cannot be preserved for this target. Choose an explicit WallpaperFit.");
            }
            if (config.UseScreenCoordinates && !HasQualifiedMonitor(monitors, config.MonitorIndex)) {
                throw UnsupportedDesktopDefault("Screen-coordinate layout requires qualified physical target dimensions. Use an explicit Background or disable screen-coordinate layout.");
            }
            return config.WithDesktopDefaults(state.Position, null);
        }

        if (state.ImageOpacity != 0 || state.Background == null) {
            throw UnsupportedDesktopDefault("The desktop did not expose effective image pixels or a disabled-image background. Choose an explicit Background.");
        }
        var monitor = monitors?.FirstOrDefault(item => item.Index == config.MonitorIndex && item.Width > 0 && item.Height > 0);
        if (monitor == null) throw UnsupportedDesktopDefault("The desktop background has no qualified physical target dimensions. Choose an explicit Background with Width and Height.");
        if (!BgInfoColorParser.TryParse(state.Background.PrimaryColor, out var primary) && !config.BackgroundColor.HasValue) {
            throw UnsupportedDesktopDefault("The desktop background color syntax is unsupported. Choose an explicit Background.");
        }
        primary = config.BackgroundColor ?? primary;
        var background = new BgInfoBackground { Width = monitor.Width, Height = monitor.Height, Color = primary };
        if (!config.BackgroundColor.HasValue && state.Background.Shading != DesktopBackgroundShading.Solid) {
            if ((state.Background.Shading != DesktopBackgroundShading.Horizontal && state.Background.Shading != DesktopBackgroundShading.Vertical) ||
                !BgInfoColorParser.TryParse(state.Background.SecondaryColor ?? string.Empty, out var secondary)) {
                throw UnsupportedDesktopDefault("The desktop background shading cannot be reproduced. Choose an explicit Background.");
            }
            background.GradientStart = primary;
            background.GradientEnd = secondary;
            background.GradientAngle = state.Background.Shading == DesktopBackgroundShading.Horizontal ? 0 : 90;
        }
        return config.WithDesktopDefaults(state.Position ?? DesktopWallpaperPosition.Stretch, background);
    }

    private static BgInfoWallpaperReadException UnsupportedDesktopDefault(string message) =>
        new(DesktopOperationStatus.Unsupported, message);

    private static bool HasQualifiedMonitor(IReadOnlyList<BgInfoMonitor>? monitors, int index) =>
        monitors?.Any(item => item.Index == index && item.Width > 0 && item.Height > 0) == true;
}
