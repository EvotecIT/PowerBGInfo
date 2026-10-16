using System.Runtime.InteropServices;
using DesktopManager;

namespace PowerBGInfo;

/// <summary>One monitor as PowerBGInfo sees it. <see cref="Index"/> is the value <c>-MonitorIndex</c> takes.</summary>
public sealed class BgInfoMonitor {
    /// <summary>Monitor index used by <c>New-BGInfo -MonitorIndex</c>.</summary>
    public int Index { get; set; }
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

/// <summary>Reads the monitors that PowerBGInfo renders for.</summary>
public static class BgInfoMonitorProvider {
    /// <summary>Returns the monitors Windows reports with a size, ordered by index. Empty when none can be read, for example without a desktop.</summary>
    public static BgInfoMonitor[] GetMonitors() {
        List<DesktopManager.Monitor> monitors;
        try {
            monitors = new Monitors().GetMonitors();
        } catch {
            return Array.Empty<BgInfoMonitor>();
        }

        return monitors
            .Select(monitor => Create(monitor.Index, monitor.DeviceName, monitor.IsPrimary,
                monitor.PositionLeft, monitor.PositionTop, monitor.PositionRight, monitor.PositionBottom,
                ReadScale(monitor.PositionLeft, monitor.PositionTop, monitor.PositionRight, monitor.PositionBottom)))
            .Where(monitor => monitor.Width > 0 && monitor.Height > 0)
            .OrderBy(monitor => monitor.Index)
            .ToArray();
    }

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

    private static double? ReadScale(int left, int top, int right, int bottom) {
        if (right <= left || bottom <= top) return null;
        // The bounds are physical pixels. A PowerShell host is usually not DPI aware, and Windows then reports
        // 96 DPI for every monitor, so the query runs per-monitor aware on this thread only and is restored.
        var previous = TrySetThreadDpiAwareness(PerMonitorAwareV2);
        try {
            var rect = new NativeRect { Left = left, Top = top, Right = right, Bottom = bottom };
            var handle = MonitorFromRect(ref rect, MonitorDefaultToNull);
            if (handle == IntPtr.Zero) return null;
            return GetDpiForMonitor(handle, MonitorDpiTypeEffective, out var dpiX, out _) == 0 && dpiX > 0
                ? dpiX / 96d
                : null;
        } catch (Exception exception) when (exception is DllNotFoundException or EntryPointNotFoundException) {
            // GetDpiForMonitor needs Windows 8.1 or later.
            return null;
        } finally {
            if (previous != IntPtr.Zero) TrySetThreadDpiAwareness(previous);
        }
    }

    private static IntPtr TrySetThreadDpiAwareness(IntPtr context) {
        try {
            return SetThreadDpiAwarenessContext(context);
        } catch (EntryPointNotFoundException) {
            // Windows 10 1607 and later only; older systems keep the process awareness.
            return IntPtr.Zero;
        }
    }

    private const uint MonitorDefaultToNull = 0;
    private const int MonitorDpiTypeEffective = 0;
    private static readonly IntPtr PerMonitorAwareV2 = new(-4);

    [StructLayout(LayoutKind.Sequential)]
    private struct NativeRect {
        public int Left;
        public int Top;
        public int Right;
        public int Bottom;
    }

    [DllImport("user32.dll")]
    private static extern IntPtr MonitorFromRect(ref NativeRect rect, uint flags);

    [DllImport("user32.dll")]
    private static extern IntPtr SetThreadDpiAwarenessContext(IntPtr context);

    [DllImport("shcore.dll")]
    private static extern int GetDpiForMonitor(IntPtr monitor, int dpiType, out uint dpiX, out uint dpiY);
}
