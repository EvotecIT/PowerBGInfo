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
    private readonly ImageService _imageService;
    private readonly IWallpaperService _wallpaperService;
    private static readonly HashSet<string> SlideshowImageExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".bmp",
        ".dib",
        ".gif",
        ".jfif",
        ".jpe",
        ".jpeg",
        ".jpg",
        ".png",
        ".tif",
        ".tiff",
        ".wdp"
    };

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

    private string GenerateCore(BgInfoConfiguration config)
    {
        var applyCurrentWallpaper = _wallpaperService is not WallpaperService desktop || desktop.ValidateTarget(config);
        Directory.CreateDirectory(config.ConfigurationDirectory);

        IReadOnlyList<BgInfoMonitor>? monitors = null;
        IReadOnlyList<BgInfoMonitor>? GetMonitors()
        {
            if (monitors != null)
            {
                return monitors;
            }

            try
            {
                monitors = config.Target == BgInfoTarget.File
                    ? Array.Empty<BgInfoMonitor>() : BgInfoMonitorProvider.GetMonitors();
            }
            catch
            {
                monitors = null;
            }

            return monitors;
        }

        if (TryGenerateWallpaperSlideshow(config, GetMonitors, applyCurrentWallpaper, out var slideshowOutputPath))
        {
            return slideshowOutputPath;
        }

        return GenerateImage(config, CurrentWallpaper ?? (index => config.Target == BgInfoTarget.File
            ? null : WallpaperService.GetCurrentWallpaper(index)), GetMonitors, applyTargets: true, applyCurrentWallpaper);
    }

    private string GenerateImage(BgInfoConfiguration config, Func<int, string?> getWallpaperPath, Func<IReadOnlyList<BgInfoMonitor>?> getMonitors, bool applyTargets, bool applyCurrentWallpaper = true)
    {
        // Never draw on PowerBGInfo's own earlier output: that would stack the text on every run.
        var plannedOutputPath = string.IsNullOrWhiteSpace(config.OutputFileName) ? null : BuildOutputPath(config, null, false);
        var chosenBase = !string.IsNullOrWhiteSpace(config.FilePath);
        var imagePath = config.Background == null
            ? ResolveOriginalImage(ResolveBaseImagePath(config, getWallpaperPath), plannedOutputPath,
                chosenBase ? "-FilePath" : "The current wallpaper", chosenByUser: chosenBase)
            : null;
        var backgroundImagePath = config.Background == null
            ? null
            : ResolveOriginalImage(config.Background.ImagePath, plannedOutputPath, "The New-BGInfoBackground image", chosenByUser: true);

        bool hasBaseImage = !string.IsNullOrEmpty(imagePath) && File.Exists(imagePath);
        var outputPath = BuildOutputPath(config, imagePath, hasBaseImage);
        if (!string.IsNullOrWhiteSpace(backgroundImagePath) && IsSamePath(backgroundImagePath!, outputPath)) {
            backgroundImagePath = PreserveInPlaceBaseImage(backgroundImagePath!, outputPath);
        } else if (chosenBase && hasBaseImage && IsSamePath(imagePath!, outputPath)) {
            imagePath = PreserveInPlaceBaseImage(imagePath!, outputPath);
        }
        var outputDirectory = Path.GetDirectoryName(outputPath);
        if (!string.IsNullOrWhiteSpace(outputDirectory))
        {
            Directory.CreateDirectory(outputDirectory);
        }

        using var image = config.Background != null
            ? CreateBackgroundImage(config.Background, backgroundImagePath, config, getMonitors)
            : hasBaseImage
                ? LoadBaseImage(imagePath!, chosenBase, config, getMonitors, outputPath, out hasBaseImage)
                : CreateBaseImage(config, getMonitors(), outputPath);

        var expandedEntries = BgInfoVariableResolver.ExpandEntries(config);
        var entryLayouts = BuildEntryLayouts(image, config, expandedEntries);
        var (highestWidth, totalWidth) = MeasureTextColumns(
            entryLayouts.Select(layout => (layout.Entry.Type == BgInfoEntryType.Label, layout.LabelWidth, layout.ValueWidth)),
            config.SpaceBetweenColumns);
        var textBlockHeight = GetTextBlockHeight(config, entryLayouts);

        double posX;
        double posY;
        double textStartX;
        double textStartY;

        if (config.UseScreenCoordinates)
        {
            var (screenWidth, screenHeight) = config.Target == BgInfoTarget.File
                ? (image.Width, image.Height) : GetMonitorSize(getMonitors(), config.MonitorIndex);

            double scaleX;
            double scaleY;

            if (config.WallpaperFit is DesktopWallpaperPosition.Fill or DesktopWallpaperPosition.Stretch)
            {
                image.Resize(screenWidth, screenHeight);
                scaleX = 1;
                scaleY = 1;
            }
            else
            {
                scaleX = (double)screenWidth / image.Width;
                scaleY = (double)screenHeight / image.Height;
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
                    posY = (screenHeight / 2f - textBlockHeight * scaleY / 2f) / scaleY;
                    break;
                case BgInfoTextPosition.MiddleCenter:
                    posX = (screenWidth / 2f - totalWidth * scaleX / 2f) / scaleX;
                    posY = (screenHeight / 2f - textBlockHeight * scaleY / 2f) / scaleY;
                    break;
                case BgInfoTextPosition.MiddleRight:
                    posX = (screenWidth - totalWidth * scaleX - config.SpaceX) / scaleX;
                    posY = (screenHeight / 2f - textBlockHeight * scaleY / 2f) / scaleY;
                    break;
                case BgInfoTextPosition.BottomLeft:
                    posY = (screenHeight - textBlockHeight * scaleY - config.SpaceY) / scaleY;
                    break;
                case BgInfoTextPosition.BottomCenter:
                    posX = (screenWidth / 2f - totalWidth * scaleX / 2f) / scaleX;
                    posY = (screenHeight - textBlockHeight * scaleY - config.SpaceY) / scaleY;
                    break;
                case BgInfoTextPosition.BottomRight:
                    posX = (screenWidth - totalWidth * scaleX - config.SpaceX) / scaleX;
                    posY = (screenHeight - textBlockHeight * scaleY - config.SpaceY) / scaleY;
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
                    posY = (image.Height / 2f) - (textBlockHeight / 2f);
                    break;
                case BgInfoTextPosition.MiddleCenter:
                    posX = (image.Width / 2f) - (totalWidth / 2f);
                    posY = (image.Height / 2f) - (textBlockHeight / 2f);
                    break;
                case BgInfoTextPosition.MiddleRight:
                    posX = image.Width - totalWidth - config.SpaceX;
                    posY = (image.Height / 2f) - (textBlockHeight / 2f);
                    break;
                case BgInfoTextPosition.BottomLeft:
                    posY = image.Height - textBlockHeight - config.SpaceY;
                    break;
                case BgInfoTextPosition.BottomCenter:
                    posX = (image.Width / 2f) - (totalWidth / 2f);
                    posY = image.Height - textBlockHeight - config.SpaceY;
                    break;
                case BgInfoTextPosition.BottomRight:
                    posX = image.Width - totalWidth - config.SpaceX;
                    posY = image.Height - textBlockHeight - config.SpaceY;
                    break;
            }
        }

        // The desktop-icon area of a layout also keeps a left-aligned text block clear.
        if (config.Layout is { IconAreaWidth: > 0 } iconLayout && config.TextPosition is BgInfoTextPosition.TopLeft or BgInfoTextPosition.MiddleLeft or BgInfoTextPosition.BottomLeft)
        {
            posX += Math.Min(iconLayout.IconAreaWidth, Math.Max(0, image.Width - totalWidth - posX));
        }

        textStartX = posX;
        textStartY = posY;

        foreach (var layout in entryLayouts)
        {
            var entry = layout.Entry;
            if (entry.Type == BgInfoEntryType.Label)
            {
                image.AddText(posX, posY, entry.Name, layout.LabelStyle);
            }
            else
            {
                image.AddText(posX, posY, entry.Name, layout.LabelStyle);
                var valueY = posY;
                foreach (var line in layout.ValueLines)
                {
                    image.AddText(posX + highestWidth + config.SpaceBetweenColumns, valueY, line, layout.ValueStyle);
                    valueY += layout.ValueLineHeight;
                }
            }
            posY += layout.RowHeight + config.SpaceBetweenLines;
        }

        var textBlock = new ChartRect(textStartX, textStartY, totalWidth, Math.Max(0f, textBlockHeight));

        var chartValues = ResolveAllChartValues(config, DateTimeOffset.UtcNow);
        var placements = ResolveRegionPlacements(image.Width, image.Height, config, textBlock, chartValues);
        RenderCharts(image, config, textBlock, placements, chartValues);
        RenderTopologies(image, config, placements);
        RenderVisualCanvases(image, config, placements);
        RenderWidgets(image, config, placements);
        RenderImages(image, config);

        _imageService.Save(image, outputPath);
        BgInfoOutputRegistry.Record(outputPath, config.Background != null ? backgroundImagePath : hasBaseImage ? imagePath : null);

        if (applyTargets && config.Target.HasFlag(BgInfoTarget.Wallpaper))
        {
            if (config.ApplyToAllUsers)
            {
                _wallpaperService.SetWallpaperForAllUsers(outputPath, config.WallpaperFit, config.IncludeDefaultUserProfile);
            }

            if (applyCurrentWallpaper) ApplyWallpaper(config, outputPath);
            else WriteWarning("All-user wallpaper profiles were updated. The current session has no available desktop and was not changed.");
        }

        if (applyTargets && config.Target.HasFlag(BgInfoTarget.LogonScreen))
        {
            _wallpaperService.SetLogonWallpaper(outputPath);
        }

        return outputPath;
    }

}
