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
    private static string BuildOutputPath(BgInfoConfiguration config, string? imagePath, bool hasBaseImage)
    {
        if (!string.IsNullOrWhiteSpace(config.OutputFileName))
        {
            var configuredPath = Path.IsPathRooted(config.OutputFileName)
                ? config.OutputFileName
                : Path.Combine(config.ConfigurationDirectory, config.OutputFileName);
            return NormalizeOutputPathExtension(configuredPath, config.Target);
        }

        if (hasBaseImage)
        {
            var fileName = Path.GetFileNameWithoutExtension(imagePath) + "_PowerBgInfo" + NormalizeOutputImageExtension(Path.GetExtension(imagePath), config.Target);
            return Path.Combine(config.ConfigurationDirectory, fileName);
        }

        return Path.Combine(config.ConfigurationDirectory, "PowerBgInfo.png");
    }

    private static string BuildSlideshowOutputPath(BgInfoConfiguration config, string sourcePath, int index)
    {
        var sourceExtension = NormalizeOutputImageExtension(Path.GetExtension(sourcePath), BgInfoTarget.Wallpaper);

        if (!string.IsNullOrWhiteSpace(config.OutputFileName))
        {
            var configuredPath = Path.IsPathRooted(config.OutputFileName)
                ? config.OutputFileName
                : Path.Combine(config.ConfigurationDirectory, config.OutputFileName);
            var directory = Path.GetDirectoryName(configuredPath) ?? config.ConfigurationDirectory;
            var name = Path.GetFileNameWithoutExtension(configuredPath);
            if (string.IsNullOrWhiteSpace(name))
            {
                name = "PowerBgInfo";
            }

            var extension = Path.GetExtension(configuredPath);
            if (string.IsNullOrWhiteSpace(extension))
            {
                extension = sourceExtension;
            }
            else
            {
                extension = NormalizeOutputImageExtension(extension, BgInfoTarget.Wallpaper);
            }

            return Path.Combine(directory, $"{name}_{index + 1:D3}{extension}");
        }

        var sourceName = Path.GetFileNameWithoutExtension(sourcePath);
        if (string.IsNullOrWhiteSpace(sourceName))
        {
            sourceName = "PowerBgInfo";
        }

        return Path.Combine(config.ConfigurationDirectory, $"{sourceName}_PowerBgInfo_{index + 1:D3}{sourceExtension}");
    }

    internal static string NormalizeOutputImageExtension(string? extension)
    {
        if (string.IsNullOrWhiteSpace(extension))
        {
            return ".png";
        }

        return extension!.ToLowerInvariant() switch
        {
            ".jpg" or ".jpeg" or ".jpe" or ".jfif" => ".jpg",
            ".png" => ".png",
            ".bmp" => ".bmp",
            ".ppm" or ".pnm" => ".ppm",
            ".tif" or ".tiff" => ".tiff",
            ".gif" => ".gif",
            ".dib" or ".wdp" => ".png",
            _ => ".png"
        };
    }

    internal static string NormalizeOutputImageExtension(string? extension, BgInfoTarget target)
    {
        var normalized = NormalizeOutputImageExtension(extension);
        var desktopTarget = (target & (BgInfoTarget.Wallpaper | BgInfoTarget.LogonScreen)) != 0;
        return desktopTarget && normalized is not ".png" and not ".jpg" and not ".bmp"
            ? ".png"
            : normalized;
    }

    private static string NormalizeOutputPathExtension(string path, BgInfoTarget target)
    {
        var directory = Path.GetDirectoryName(path) ?? string.Empty;
        var name = Path.GetFileNameWithoutExtension(path);
        if (string.IsNullOrWhiteSpace(name))
        {
            name = "PowerBgInfo";
        }

        var extension = NormalizeOutputImageExtension(Path.GetExtension(path), target);
        return string.IsNullOrWhiteSpace(directory)
            ? name + extension
            : Path.Combine(directory, name + extension);
    }

}
