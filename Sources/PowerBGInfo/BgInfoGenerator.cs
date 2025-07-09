using System.IO;
using ImagePlayground;
using SixLabors.ImageSharp;
using DesktopManager;

namespace PowerBGInfo;

public class BgInfoGenerator
{
    private readonly ImageService _imageService;
    private readonly IWallpaperService _wallpaperService;

    public BgInfoGenerator(ImageService imageService, IWallpaperService wallpaperService)
    {
        _imageService = imageService;
        _wallpaperService = wallpaperService;
    }

    public string Generate(BgInfoConfiguration config)
    {
        Directory.CreateDirectory(config.ConfigurationDirectory);

        var monitors = new Monitors();
        var imagePath = string.IsNullOrEmpty(config.FilePath)
            ? monitors.GetWallpaper(config.MonitorIndex)
            : config.FilePath;

        if (string.IsNullOrEmpty(imagePath) || !File.Exists(imagePath))
            throw new FileNotFoundException("Wallpaper not found", imagePath);

        var fileName = Path.GetFileNameWithoutExtension(imagePath) + "_PowerBgInfo" + Path.GetExtension(imagePath);
        var outputPath = Path.Combine(config.ConfigurationDirectory, fileName);
        File.Copy(imagePath, outputPath, true);

        using var image = _imageService.Load(outputPath);

        float highestWidth = 0;
        float highestHeight = 0;
        float highestValueWidth = 0;
        foreach (var entry in config.Entries)
        {
            entry.Color ??= config.Color;
            entry.FontSize ??= config.FontSize;
            entry.FontFamilyName ??= config.FontFamilyName;
            if (entry.Type != BgInfoEntryType.Label)
            {
                entry.ValueColor ??= entry.Color ?? config.ValueColor;
                entry.ValueFontSize ??= entry.FontSize ?? config.ValueFontSize;
                entry.ValueFontFamilyName ??= entry.FontFamilyName ?? config.ValueFontFamilyName;
            }
            var size = image.GetTextSize(entry.Name, entry.FontSize!.Value, entry.FontFamilyName!);
            if (size.Width > highestWidth) highestWidth = size.Width;
            if (size.Height > highestHeight) highestHeight = size.Height;
            if (entry.Type != BgInfoEntryType.Label && entry.Value != null)
            {
                var valSize = image.GetTextSize(entry.Value, entry.ValueFontSize!.Value, entry.ValueFontFamilyName!);
                if (valSize.Width > highestValueWidth) highestValueWidth = valSize.Width;
            }
        }

        float posX = config.PositionX;
        float posY = config.PositionY;
        foreach (var entry in config.Entries)
        {
            if (entry.Type == BgInfoEntryType.Label)
            {
                image.AddText(posX, posY, entry.Name, entry.Color!.Value, entry.FontSize!.Value, entry.FontFamilyName!);
            }
            else
            {
                image.AddText(posX, posY, entry.Name, entry.Color!.Value, entry.FontSize!.Value, entry.FontFamilyName!);
                image.AddText(posX + highestWidth + config.SpaceBetweenColumns, posY, entry.Value!, entry.ValueColor!.Value, entry.ValueFontSize!.Value, entry.ValueFontFamilyName!);
            }
            posY += highestHeight + config.SpaceBetweenLines;
        }

        _imageService.Save(image, outputPath);

        if (config.Target is "Wallpaper" or "Both")
        {
            _wallpaperService.SetWallpaper(config.MonitorIndex, outputPath, config.WallpaperFit);
        }

        return outputPath;
    }
}
