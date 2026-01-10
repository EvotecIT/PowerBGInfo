using System.IO;
using System.Linq;
using ImagePlayground;
using SixLabors.ImageSharp;
using DesktopManager;

namespace PowerBGInfo;

/// <summary>
/// Generates BGInfo overlays and applies them to the configured target.
/// </summary>
public class BgInfoGenerator
{
    private readonly ImageService _imageService;
    private readonly IWallpaperService _wallpaperService;

    /// <summary>
    /// Initializes a new instance of the <see cref="BgInfoGenerator"/> class.
    /// </summary>
    /// <param name="imageService">Image service used to load and save images.</param>
    /// <param name="wallpaperService">Wallpaper service used for applying output.</param>
    public BgInfoGenerator(ImageService imageService, IWallpaperService wallpaperService)
    {
        _imageService = imageService;
        _wallpaperService = wallpaperService;
    }

    /// <summary>
    /// Generates the BGInfo image and returns the output file path.
    /// </summary>
    /// <param name="config">Configuration controlling layout and output.</param>
    /// <returns>Path to the generated image.</returns>
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
        var totalWidth = highestWidth + config.SpaceBetweenColumns + highestValueWidth;

        float posX;
        float posY;

        if (config.UseScreenCoordinates)
        {
            var monitor = new Monitors().GetMonitors(index: config.MonitorIndex).First();
            var screenWidth = monitor.PositionRight - monitor.PositionLeft;
            var screenHeight = monitor.PositionBottom - monitor.PositionTop;

            float scaleX;
            float scaleY;

            if (config.WallpaperFit is DesktopWallpaperPosition.Fill or DesktopWallpaperPosition.Stretch)
            {
                image.Resize(screenWidth, screenHeight);
                scaleX = 1;
                scaleY = 1;
            }
            else
            {
                scaleX = (float)screenWidth / image.Width;
                scaleY = (float)screenHeight / image.Height;
            }

            posX = config.SpaceX / scaleX;
            posY = config.SpaceY / scaleY;

            switch (config.TextPosition) {
                case BgInfoTextPosition.TopCenter:
                    posX = (screenWidth / 2f - totalWidth * scaleX / 2f) / scaleX;
                    break;
                case BgInfoTextPosition.TopRight:
                    posX = (screenWidth - totalWidth * scaleX - config.SpaceX) / scaleX;
                    break;
                case BgInfoTextPosition.MiddleLeft:
                    posY = (screenHeight / 2f - (config.Entries.Count * (highestHeight + config.SpaceBetweenLines)) * scaleY / 2f) / scaleY;
                    break;
                case BgInfoTextPosition.MiddleCenter:
                    posX = (screenWidth / 2f - totalWidth * scaleX / 2f) / scaleX;
                    posY = (screenHeight / 2f - (config.Entries.Count * (highestHeight + config.SpaceBetweenLines)) * scaleY / 2f) / scaleY;
                    break;
                case BgInfoTextPosition.MiddleRight:
                    posX = (screenWidth - totalWidth * scaleX - config.SpaceX) / scaleX;
                    posY = (screenHeight / 2f - (config.Entries.Count * (highestHeight + config.SpaceBetweenLines)) * scaleY / 2f) / scaleY;
                    break;
                case BgInfoTextPosition.BottomLeft:
                    posY = (screenHeight - (config.Entries.Count * (highestHeight + config.SpaceBetweenLines)) * scaleY - config.SpaceY) / scaleY;
                    break;
                case BgInfoTextPosition.BottomCenter:
                    posX = (screenWidth / 2f - totalWidth * scaleX / 2f) / scaleX;
                    posY = (screenHeight - (config.Entries.Count * (highestHeight + config.SpaceBetweenLines)) * scaleY - config.SpaceY) / scaleY;
                    break;
                case BgInfoTextPosition.BottomRight:
                    posX = (screenWidth - totalWidth * scaleX - config.SpaceX) / scaleX;
                    posY = (screenHeight - (config.Entries.Count * (highestHeight + config.SpaceBetweenLines)) * scaleY - config.SpaceY) / scaleY;
                    break;
            }
        }
        else
        {
            posX = config.SpaceX;
            posY = config.SpaceY;

            switch (config.TextPosition) {
                case BgInfoTextPosition.TopCenter:
                    posX = (image.Width / 2f) - (totalWidth / 2f);
                    break;
                case BgInfoTextPosition.TopRight:
                    posX = image.Width - totalWidth - config.SpaceX;
                    break;
                case BgInfoTextPosition.MiddleLeft:
                    posY = (image.Height / 2f) - ((config.Entries.Count * (highestHeight + config.SpaceBetweenLines)) / 2f);
                    break;
                case BgInfoTextPosition.MiddleCenter:
                    posX = (image.Width / 2f) - (totalWidth / 2f);
                    posY = (image.Height / 2f) - ((config.Entries.Count * (highestHeight + config.SpaceBetweenLines)) / 2f);
                    break;
                case BgInfoTextPosition.MiddleRight:
                    posX = image.Width - totalWidth - config.SpaceX;
                    posY = (image.Height / 2f) - ((config.Entries.Count * (highestHeight + config.SpaceBetweenLines)) / 2f);
                    break;
                case BgInfoTextPosition.BottomLeft:
                    posY = image.Height - (config.Entries.Count * (highestHeight + config.SpaceBetweenLines)) - config.SpaceY;
                    break;
                case BgInfoTextPosition.BottomCenter:
                    posX = (image.Width / 2f) - (totalWidth / 2f);
                    posY = image.Height - (config.Entries.Count * (highestHeight + config.SpaceBetweenLines)) - config.SpaceY;
                    break;
                case BgInfoTextPosition.BottomRight:
                    posX = image.Width - totalWidth - config.SpaceX;
                    posY = image.Height - (config.Entries.Count * (highestHeight + config.SpaceBetweenLines)) - config.SpaceY;
                    break;
            }
        }

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

        if (config.Target is BgInfoTarget.Wallpaper or BgInfoTarget.Both) {
            _wallpaperService.SetWallpaper(config.MonitorIndex, outputPath, config.WallpaperFit);
        }

        return outputPath;
    }
}
