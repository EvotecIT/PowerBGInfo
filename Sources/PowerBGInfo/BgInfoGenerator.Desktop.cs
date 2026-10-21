using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using ChartForgeX.Primitives;
using ChartForgeX.Typography;
using DesktopManager;

namespace PowerBGInfo;

/// <summary>
/// Generates BGInfo overlays and applies them to the configured target.
/// </summary>
public partial class BgInfoGenerator
{
    private BgInfoRasterImage LoadBaseImage(string imagePath, bool chosenByUser, BgInfoConfiguration config,
        Func<IReadOnlyList<BgInfoMonitor>?> getMonitors, string outputPath, out bool loaded)
    {
        try {
            var image = _imageService.Load(imagePath);
            loaded = true;
            return image;
        } catch (NotSupportedException) when (!chosenByUser) {
            loaded = false;
            WriteWarning($"The current wallpaper '{imagePath}' uses an unsupported raster format. A plain background is used; choose a PNG, JPEG, GIF, BMP, PPM, or supported TIFF image to preserve a photo background.");
            return CreateBaseImage(config, getMonitors(), outputPath);
        }
    }

    private static BgInfoRasterImage CreateBaseImage(BgInfoConfiguration config, IReadOnlyList<BgInfoMonitor>? monitors, string outputPath)
    {
        var (width, height) = GetMonitorSize(monitors, config.MonitorIndex);
        var background = ResolveBackgroundColor(config, monitors);
        var image = new BgInfoRasterImage();
        image.Create(outputPath, width, height, background);
        return image;
    }

    private static BgInfoRasterImage CreateBackgroundImage(BgInfoBackground background, string? imagePath, BgInfoConfiguration config, Func<IReadOnlyList<BgInfoMonitor>?> getMonitors)
    {
        var width = background.Width;
        var height = background.Height;
        if (width <= 0 || height <= 0)
        {
            var monitorSize = GetMonitorSize(getMonitors(), config.MonitorIndex);
            if (width <= 0) width = monitorSize.Width;
            if (height <= 0) height = monitorSize.Height;
        }

        return BgInfoBackgroundRenderer.Render(background, width, height, imagePath);
    }

    internal static string? ResolveBaseImagePath(BgInfoConfiguration config, Func<int, string?> getWallpaper)
    {
        if (!string.IsNullOrWhiteSpace(config.FilePath))
        {
            return config.FilePath;
        }

        try
        {
            return getWallpaper(config.MonitorIndex);
        }
        catch
        {
            return null;
        }
    }

    private static (int Width, int Height) GetMonitorSize(IReadOnlyList<BgInfoMonitor>? monitors, int monitorIndex)
    {
        if (monitors == null || monitors.Count == 0)
        {
            return (1920, 1080);
        }

        return ResolveMonitorSize(monitors, monitorIndex);
    }

    internal static (int Width, int Height) ResolveMonitorSize(IReadOnlyList<BgInfoMonitor> monitors, int monitorIndex)
    {
        BgInfoMonitor? monitor = monitors.FirstOrDefault(item => item.Index == monitorIndex)
            ?? monitors.FirstOrDefault(item => item.Primary)
            ?? monitors.FirstOrDefault();
        if (monitor == null)
        {
            return (1920, 1080);
        }
        return (monitor.Width, monitor.Height);
    }

    private static ChartColor ResolveBackgroundColor(BgInfoConfiguration config, IReadOnlyList<BgInfoMonitor>? monitors)
    {
        if (config.BackgroundColor.HasValue)
        {
            return config.BackgroundColor.Value;
        }

        if (monitors == null || monitors.Count == 0)
        {
            return ChartColor.Black;
        }

        uint rgb;
        try
        {
            rgb = WallpaperService.GetCurrentBackgroundColor();
        }
        catch
        {
            return ChartColor.Black;
        }

        byte r = (byte)(rgb & 0xFF);
        byte g = (byte)((rgb >> 8) & 0xFF);
        byte b = (byte)((rgb >> 16) & 0xFF);
        return ChartColor.FromRgb(r, g, b);
    }

    private void ApplyWallpaper(BgInfoConfiguration config, string outputPath)
    {
        if (!config.ForceWallpaperRefresh)
        {
            _wallpaperService.SetWallpaper(config.MonitorIndex, outputPath, config.WallpaperFit);
            return;
        }

        var refreshPath = BuildRefreshPath(outputPath);
        try
        {
            File.Copy(outputPath, refreshPath, true);
            _wallpaperService.SetWallpaper(config.MonitorIndex, refreshPath, config.WallpaperFit);
        }
        catch
        {
            _wallpaperService.SetWallpaper(config.MonitorIndex, outputPath, config.WallpaperFit);
            TryDelete(refreshPath);
            return;
        }

        _wallpaperService.SetWallpaper(config.MonitorIndex, outputPath, config.WallpaperFit);
        TryDelete(refreshPath);
    }

    private static string BuildRefreshPath(string outputPath)
    {
        var directory = Path.GetDirectoryName(outputPath) ?? string.Empty;
        var name = Path.GetFileNameWithoutExtension(outputPath);
        var extension = Path.GetExtension(outputPath);
        var refreshName = $"{name}_refresh_{Guid.NewGuid():N}{extension}";
        return Path.Combine(directory, refreshName);
    }

    private static void TryDelete(string path)
    {
        try
        {
            if (!string.IsNullOrWhiteSpace(path) && File.Exists(path))
            {
                File.Delete(path);
            }
        }
        catch
        {
        }
    }

}
