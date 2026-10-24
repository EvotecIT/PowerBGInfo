using DesktopManager;

namespace PowerBGInfo;

/// <summary>One monitor as PowerBGInfo sees it. <see cref="Index"/> is the value <c>-MonitorIndex</c> takes.</summary>
public sealed class BgInfoMonitor {
    /// <summary>Monitor index used by <c>New-BGInfo -MonitorIndex</c>.</summary>
    public int Index { get; set; }
    /// <summary>Backend display identity for the current topology.</summary>
    public string Id { get; set; } = string.Empty;
    /// <summary>Display device name, for example \\.\DISPLAY1. Empty when Windows reports none, as on some session hosts.</summary>
    public string DeviceName { get; set; } = string.Empty;
    /// <summary>Whether this is the primary monitor.</summary>
    public bool Primary { get; set; }
    /// <summary>Left edge in desktop coordinates.</summary>
    public int Left { get; set; }
    /// <summary>Top edge in desktop coordinates.</summary>
    public int Top { get; set; }
    /// <summary>Width in pixels; the size New-BGInfoBackground renders at for this monitor.</summary>
    public int Width { get; set; }
    /// <summary>Height in pixels.</summary>
    public int Height { get; set; }
    /// <summary>Display scale set in Windows for this monitor, for example 1.5 for 150 %. Null when it cannot be read.</summary>
    public double? Scale { get; set; }
}

/// <summary>Reads the displays that PowerBGInfo renders for.</summary>
public static class BgInfoMonitorProvider {
    /// <summary>Returns current wallpaper targets with qualified physical dimensions, ordered by the index accepted by MonitorIndex. Empty when none can be read.</summary>
    public static BgInfoMonitor[] GetMonitors() {
        try {
            return FromWallpaperTargets(new PortableDesktopService().GetWallpaperTargets());
        } catch {
            return Array.Empty<BgInfoMonitor>();
        }
    }

    /// <summary>Maps canonical wallpaper indices to their own qualified geometry without joining or renumbering physical display indices.</summary>
    internal static BgInfoMonitor[] FromWallpaperTargets(IEnumerable<DesktopWallpaperTarget> targets) => targets
        .Where(target => target.Display != null && target.Display.Width > 0 && target.Display.Height > 0)
        .Select(target => {
            var display = target.Display!;
            return new BgInfoMonitor {
                Index = target.Index, Id = display.Id, DeviceName = display.DeviceName, Primary = display.IsPrimary,
                Left = display.Left, Top = display.Top, Width = display.Width, Height = display.Height, Scale = display.Scale
            };
        })
        .OrderBy(monitor => monitor.Index).ToArray();

    internal static BgInfoMonitor Create(int index, string? deviceName, bool primary, int left, int top, int right, int bottom, double? scale) {
        return new BgInfoMonitor {
            Index = index,
            DeviceName = deviceName ?? string.Empty,
            Primary = primary,
            Left = left,
            Top = top,
            Width = Math.Max(0, right - left),
            Height = Math.Max(0, bottom - top),
            Scale = scale is > 0 ? Math.Round(scale.Value, 2) : null
        };
    }

}
