using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using ChartForgeX.Primitives;
using DesktopManager;

namespace PowerBGInfo;

/// <summary>
/// Generates BGInfo overlays and applies them to the configured target.
/// </summary>
public class BgInfoGenerator
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

    /// <summary>
    /// Generates the BGInfo image and returns the output file path.
    /// </summary>
    /// <param name="config">Configuration controlling layout and output.</param>
    /// <returns>Path to the generated image.</returns>
    public string Generate(BgInfoConfiguration config)
    {
        Directory.CreateDirectory(config.ConfigurationDirectory);

        Monitors? monitors = null;
        Monitors? GetMonitors()
        {
            if (monitors != null)
            {
                return monitors;
            }

            try
            {
                monitors = new Monitors();
            }
            catch
            {
                monitors = null;
            }

            return monitors;
        }

        if (TryGenerateWallpaperSlideshow(config, out var slideshowOutputPath))
        {
            return slideshowOutputPath;
        }

        return GenerateImage(config, index => GetWallpaper(GetMonitors(), index), GetMonitors, applyTargets: true);
    }

    private string GenerateImage(BgInfoConfiguration config, Func<int, string?> getWallpaperPath, Func<Monitors?> getMonitors, bool applyTargets)
    {
        var imagePath = ResolveBaseImagePath(config, getWallpaperPath);

        bool hasBaseImage = !string.IsNullOrEmpty(imagePath) && File.Exists(imagePath);
        var outputPath = BuildOutputPath(config, imagePath, hasBaseImage);
        var outputDirectory = Path.GetDirectoryName(outputPath);
        if (!string.IsNullOrWhiteSpace(outputDirectory))
        {
            Directory.CreateDirectory(outputDirectory);
        }

        using var image = hasBaseImage
            ? LoadBaseImage(imagePath!)
            : CreateBaseImage(config, getMonitors(), outputPath);

        var expandedEntries = BgInfoVariableResolver.ExpandEntries(config);
        var entryLayouts = BuildEntryLayouts(image, config, expandedEntries);
        double highestWidth = entryLayouts.Count == 0 ? 0f : entryLayouts.Max(layout => layout.LabelWidth);
        double highestValueWidth = entryLayouts.Count == 0 ? 0f : entryLayouts.Max(layout => layout.ValueWidth);
        bool hasValueEntries = entryLayouts.Any(layout => layout.Entry.Type != BgInfoEntryType.Label);
        var totalWidth = highestWidth + (hasValueEntries ? config.SpaceBetweenColumns + highestValueWidth : 0f);
        var textBlockHeight = GetTextBlockHeight(config, entryLayouts);

        double posX;
        double posY;
        double textStartX;
        double textStartY;

        if (config.UseScreenCoordinates)
        {
            var (screenWidth, screenHeight) = GetMonitorSize(getMonitors(), config.MonitorIndex);

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

        textStartX = posX;
        textStartY = posY;

        foreach (var layout in entryLayouts)
        {
            var entry = layout.Entry;
            if (entry.Type == BgInfoEntryType.Label)
            {
                image.AddText(posX, posY, entry.Name, entry.Color!.Value, entry.FontSize!.Value, entry.FontFamilyName!);
            }
            else
            {
                image.AddText(posX, posY, entry.Name, entry.Color!.Value, entry.FontSize!.Value, entry.FontFamilyName!);
                var valueY = posY;
                foreach (var line in layout.ValueLines)
                {
                    image.AddText(posX + highestWidth + config.SpaceBetweenColumns, valueY, line, entry.ValueColor!.Value, entry.ValueFontSize!.Value, entry.ValueFontFamilyName!);
                    valueY += layout.ValueLineHeight;
                }
            }
            posY += layout.RowHeight + config.SpaceBetweenLines;
        }

        var textBlock = new ChartRect(textStartX, textStartY, totalWidth, Math.Max(0f, textBlockHeight));

        RenderCharts(image, config, textBlock);
        RenderTopologies(image, config);
        RenderVisualCanvases(image, config);
        RenderImages(image, config);

        _imageService.Save(image, outputPath);

        if (applyTargets && config.Target.HasFlag(BgInfoTarget.Wallpaper))
        {
            if (config.ApplyToAllUsers)
            {
                _wallpaperService.SetWallpaperForAllUsers(outputPath, config.WallpaperFit, config.IncludeDefaultUserProfile);
            }

            ApplyWallpaper(config, outputPath);
        }

        if (applyTargets && config.Target.HasFlag(BgInfoTarget.LogonScreen))
        {
            _wallpaperService.SetLogonWallpaper(outputPath);
        }

        return outputPath;
    }

    private bool TryGenerateWallpaperSlideshow(BgInfoConfiguration config, out string outputPath)
    {
        outputPath = string.Empty;
        if (!ShouldPreserveWallpaperSlideshow(config))
        {
            return false;
        }

        DesktopWallpaperSlideshow slideshow;
        try
        {
            slideshow = _wallpaperService.GetWallpaperSlideshow();
        }
        catch
        {
            return false;
        }

        if (!slideshow.IsRunning || slideshow.IsDisabledByRemoteSession)
        {
            return false;
        }

        var sourcePaths = ResolveSlideshowSourcePaths(slideshow.ImagePaths);
        if (sourcePaths.Length == 0)
        {
            return false;
        }

        var generatedPaths = new List<string>(sourcePaths.Length);
        var slideshowCharts = BuildSlideshowCharts(config);
        for (int i = 0; i < sourcePaths.Length; i++)
        {
            var itemConfig = CloneForSlideshowItem(config, sourcePaths[i], BuildSlideshowOutputPath(config, sourcePaths[i], i), slideshowCharts);
            generatedPaths.Add(GenerateImage(itemConfig, _ => null, () => null, applyTargets: false));
        }

        if (generatedPaths.Count == 0)
        {
            return false;
        }

        outputPath = generatedPaths[0];
        if (config.Target.HasFlag(BgInfoTarget.Wallpaper))
        {
            if (config.ApplyToAllUsers)
            {
                _wallpaperService.SetWallpaperForAllUsers(outputPath, config.WallpaperFit, config.IncludeDefaultUserProfile);
            }

            _wallpaperService.StartWallpaperSlideshow(generatedPaths, config.WallpaperFit, slideshow.Options, slideshow.SlideshowTick);
        }

        if (config.Target.HasFlag(BgInfoTarget.LogonScreen))
        {
            _wallpaperService.SetLogonWallpaper(outputPath);
        }

        return true;
    }

    private static bool ShouldPreserveWallpaperSlideshow(BgInfoConfiguration config)
    {
        return config.PreserveWallpaperSlideshow
            && config.Target.HasFlag(BgInfoTarget.Wallpaper)
            && string.IsNullOrWhiteSpace(config.FilePath);
    }

    private static string[] ResolveSlideshowSourcePaths(IEnumerable<string> paths)
    {
        var sourcePaths = new List<string>();
        foreach (var path in paths ?? Array.Empty<string>())
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                continue;
            }

            if (File.Exists(path))
            {
                if (IsSupportedSlideshowImage(path))
                {
                    sourcePaths.Add(path);
                }
                continue;
            }

            if (Directory.Exists(path))
            {
                sourcePaths.AddRange(EnumerateSlideshowImages(path));
            }
        }

        return sourcePaths
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }

    private static IEnumerable<string> EnumerateSlideshowImages(string directory)
    {
        try
        {
            return Directory.EnumerateFiles(directory, "*", SearchOption.TopDirectoryOnly)
                .Where(IsSupportedSlideshowImage)
                .ToArray();
        }
        catch
        {
            return Array.Empty<string>();
        }
    }

    private static bool IsSupportedSlideshowImage(string path)
    {
        return SlideshowImageExtensions.Contains(Path.GetExtension(path));
    }

    private static IReadOnlyList<BgInfoChart> BuildSlideshowCharts(BgInfoConfiguration source)
    {
        if (source.Charts.Count == 0)
        {
            return Array.Empty<BgInfoChart>();
        }

        var now = DateTimeOffset.UtcNow;
        var charts = new List<BgInfoChart>(source.Charts.Count);
        for (int i = 0; i < source.Charts.Count; i++)
        {
            var sourceChart = source.Charts[i];
            var values = ResolveChartValues(source, sourceChart, now, i);
            charts.Add(CloneChartForSlideshow(sourceChart, values));
        }

        return charts;
    }

    private static BgInfoChart CloneChartForSlideshow(BgInfoChart source, IReadOnlyList<double> values)
    {
        return new BgInfoChart
        {
            Id = source.Id,
            Title = source.Title,
            Kind = source.Kind,
            Width = source.Width,
            Height = source.Height,
            Anchor = source.Anchor,
            OffsetX = source.OffsetX,
            OffsetY = source.OffsetY,
            PositionX = source.PositionX,
            PositionY = source.PositionY,
            Values = values.ToArray(),
            Labels = source.Labels.ToArray(),
            Target = source.Target,
            RangeEnds = source.RangeEnds.ToArray(),
            Metric = BgInfoChartMetric.None,
            MetricArgument = source.MetricArgument,
            MaxPoints = source.MaxPoints,
            UseHistory = false,
            AppendValues = false,
            BackgroundColor = source.BackgroundColor,
            LineColor = source.LineColor,
            FillColor = source.FillColor,
            Palette = source.Palette.ToArray(),
            TextColor = source.TextColor,
            FontFamilyName = source.FontFamilyName,
            TitleFontSize = source.TitleFontSize,
            ValueFontSize = source.ValueFontSize,
            ShowLatestValue = source.ShowLatestValue,
            ValueFormat = source.ValueFormat,
            ValueSuffix = source.ValueSuffix,
            BarGap = source.BarGap,
            Padding = source.Padding,
            ShowGrid = source.ShowGrid,
            GridColor = source.GridColor,
            GridLineCount = source.GridLineCount,
            ShowLegend = source.ShowLegend,
            ShowPointLegend = source.ShowPointLegend,
            LegendPosition = source.LegendPosition,
            ShowDataLabels = source.ShowDataLabels,
            Minimum = source.Minimum,
            Maximum = source.Maximum,
            ShowDonutCenterLabel = source.ShowDonutCenterLabel,
            DonutInnerRadiusRatio = source.DonutInnerRadiusRatio,
            DonutCenterValue = source.DonutCenterValue,
            DonutCenterLabel = source.DonutCenterLabel,
            ShowRadialBarCenterLabel = source.ShowRadialBarCenterLabel,
            ShowCircleStatusLabel = source.ShowCircleStatusLabel,
            ShowProgressValues = source.ShowProgressValues,
            ShowProgressHandles = source.ShowProgressHandles,
            ProgressBarThicknessRatio = source.ProgressBarThicknessRatio,
            PictorialSymbol = source.PictorialSymbol,
            PictorialColumns = source.PictorialColumns
        };
    }

    private static BgInfoConfiguration CloneForSlideshowItem(BgInfoConfiguration source, string filePath, string outputPath, IReadOnlyList<BgInfoChart> charts)
    {
        var clone = new BgInfoConfiguration {
            ChartLayout = source.ChartLayout,
            ChartStackAnchor = source.ChartStackAnchor,
            ChartStackDirection = source.ChartStackDirection,
            ChartStackSpacing = source.ChartStackSpacing,
            ChartStackOffsetX = source.ChartStackOffsetX,
            ChartStackOffsetY = source.ChartStackOffsetY,
            ChartStackAlignToTextBlock = source.ChartStackAlignToTextBlock,
            ChartStackOutsideTextBlock = source.ChartStackOutsideTextBlock,
            FilePath = filePath,
            OutputFileName = outputPath,
            ConfigurationDirectory = source.ConfigurationDirectory,
            FontFamilyName = source.FontFamilyName,
            Color = source.Color,
            FontSize = source.FontSize,
            ValueColor = source.ValueColor,
            ValueFontSize = source.ValueFontSize,
            ValueFontFamilyName = source.ValueFontFamilyName,
            ValueWrapWidth = source.ValueWrapWidth,
            BackgroundColor = source.BackgroundColor,
            SpaceBetweenLines = source.SpaceBetweenLines,
            SpaceBetweenColumns = source.SpaceBetweenColumns,
            PositionX = source.PositionX,
            PositionY = source.PositionY,
            MonitorIndex = source.MonitorIndex,
            SpaceX = source.SpaceX,
            SpaceY = source.SpaceY,
            WallpaperFit = source.WallpaperFit,
            TextPosition = source.TextPosition,
            Target = BgInfoTarget.File,
            ForceWallpaperRefresh = false,
            PreserveWallpaperSlideshow = false,
            ApplyToAllUsers = false,
            IncludeDefaultUserProfile = source.IncludeDefaultUserProfile,
            UseScreenCoordinates = source.UseScreenCoordinates
        };

        clone.Variables.AddRange(source.Variables);
        clone.Entries.AddRange(source.Entries);
        clone.Charts.AddRange(charts);
        clone.Topologies.AddRange(source.Topologies);
        clone.VisualCanvases.AddRange(source.VisualCanvases);
        clone.Images.AddRange(source.Images);
        return clone;
    }

    private BgInfoRasterImage LoadBaseImage(string imagePath)
    {
        return _imageService.Load(imagePath);
    }

    private static BgInfoRasterImage CreateBaseImage(BgInfoConfiguration config, Monitors? monitors, string outputPath)
    {
        var (width, height) = GetMonitorSize(monitors, config.MonitorIndex);
        var background = ResolveBackgroundColor(config, monitors);
        var image = new BgInfoRasterImage();
        image.Create(outputPath, width, height, background);
        return image;
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

    private static string? GetWallpaper(Monitors? monitors, int monitorIndex)
    {
        if (monitors == null)
        {
            return null;
        }

        try
        {
            return monitors.GetWallpaper(monitorIndex);
        }
        catch
        {
            return null;
        }
    }

    private static (int Width, int Height) GetMonitorSize(Monitors? monitors, int monitorIndex)
    {
        if (monitors == null)
        {
            return (1920, 1080);
        }

        DesktopManager.Monitor? monitor;
        try
        {
            monitor = monitors.GetMonitors(index: monitorIndex).FirstOrDefault()
                ?? monitors.GetMonitors(primaryOnly: true).FirstOrDefault()
                ?? monitors.GetMonitors().FirstOrDefault();
        }
        catch
        {
            return (1920, 1080);
        }

        if (monitor == null)
        {
            return (1920, 1080);
        }
        int width = Math.Max(1, monitor.PositionRight - monitor.PositionLeft);
        int height = Math.Max(1, monitor.PositionBottom - monitor.PositionTop);
        return (width, height);
    }

    private static ChartColor ResolveBackgroundColor(BgInfoConfiguration config, Monitors? monitors)
    {
        if (config.BackgroundColor.HasValue)
        {
            return config.BackgroundColor.Value;
        }

        if (monitors == null)
        {
            return ChartColor.Black;
        }

        uint rgb;
        try
        {
            rgb = monitors.GetBackgroundColor();
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

    private static string BuildOutputPath(BgInfoConfiguration config, string? imagePath, bool hasBaseImage)
    {
        if (!string.IsNullOrWhiteSpace(config.OutputFileName))
        {
            var configuredPath = Path.IsPathRooted(config.OutputFileName)
                ? config.OutputFileName
                : Path.Combine(config.ConfigurationDirectory, config.OutputFileName);
            return NormalizeOutputPathExtension(configuredPath);
        }

        if (hasBaseImage)
        {
            var fileName = Path.GetFileNameWithoutExtension(imagePath) + "_PowerBgInfo" + NormalizeOutputImageExtension(Path.GetExtension(imagePath));
            return Path.Combine(config.ConfigurationDirectory, fileName);
        }

        return Path.Combine(config.ConfigurationDirectory, "PowerBgInfo.png");
    }

    private static string BuildSlideshowOutputPath(BgInfoConfiguration config, string sourcePath, int index)
    {
        var sourceExtension = NormalizeOutputImageExtension(Path.GetExtension(sourcePath));

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
                extension = NormalizeOutputImageExtension(extension);
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
            ".gif" or ".dib" or ".wdp" => ".png",
            _ => ".png"
        };
    }

    private static string NormalizeOutputPathExtension(string path)
    {
        var directory = Path.GetDirectoryName(path) ?? string.Empty;
        var name = Path.GetFileNameWithoutExtension(path);
        if (string.IsNullOrWhiteSpace(name))
        {
            name = "PowerBgInfo";
        }

        var extension = NormalizeOutputImageExtension(Path.GetExtension(path));
        return string.IsNullOrWhiteSpace(directory)
            ? name + extension
            : Path.Combine(directory, name + extension);
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

    private static void RenderCharts(BgInfoRasterImage image, BgInfoConfiguration config, ChartRect textBlock)
    {
        if (config.Charts.Count == 0)
        {
            return;
        }

        var now = DateTimeOffset.UtcNow;
        if (config.ChartLayout == BgInfoChartLayoutMode.Stack)
        {
            RenderStackedCharts(image, config, textBlock, now);
            return;
        }

        for (int i = 0; i < config.Charts.Count; i++)
        {
            var chart = config.Charts[i];
            var values = ResolveChartValues(config, chart, now, i);
            if (values.Count == 0)
            {
                continue;
            }

            using var chartImage = BgInfoChartRenderer.Render(chart, values, config);
            var position = ResolveChartPosition(image, chart);
            image.DrawImage(chartImage, position.X, position.Y);
        }
    }

    private static void RenderTopologies(BgInfoRasterImage image, BgInfoConfiguration config)
    {
        if (config.Topologies.Count == 0)
        {
            return;
        }

        for (int i = 0; i < config.Topologies.Count; i++)
        {
            var topology = config.Topologies[i];
            if (topology.Nodes.Count == 0)
            {
                continue;
            }

            using var topologyImage = BgInfoTopologyRenderer.Render(topology, config);
            var position = ResolveTopologyPosition(image, topology);
            image.DrawImage(topologyImage, position.X, position.Y);
        }
    }

    private static void RenderVisualCanvases(BgInfoRasterImage image, BgInfoConfiguration config)
    {
        if (config.VisualCanvases.Count == 0)
        {
            return;
        }

        for (int i = 0; i < config.VisualCanvases.Count; i++)
        {
            var visual = config.VisualCanvases[i];
            using var visualImage = BgInfoVisualCanvasRenderer.Render(visual, config, image.Width, image.Height);
            image.DrawImage(visualImage, visual.PositionX, visual.PositionY);
        }
    }

    private static void RenderImages(BgInfoRasterImage image, BgInfoConfiguration config)
    {
        if (config.Images.Count == 0)
        {
            return;
        }

        foreach (var overlay in config.Images)
        {
            if (overlay == null || string.IsNullOrWhiteSpace(overlay.Path))
            {
                continue;
            }
            if (!File.Exists(overlay.Path))
            {
                throw new FileNotFoundException("BGInfo image overlay file was not found.", overlay.Path);
            }

            using var overlayImage = BgInfoRasterImage.Load(overlay.Path);
            var (width, height) = ResolveImageSize(overlay, overlayImage.Width, overlayImage.Height);
            var position = ResolveImagePosition(image, overlay, width, height);
            image.DrawImage(overlayImage.ToRgbaImage(), position.X, position.Y, width, height, overlay.Opacity, BgInfoImageFitMapper.ToVisualCanvasFit(overlay.Fit));
        }
    }

    private static void RenderStackedCharts(BgInfoRasterImage image, BgInfoConfiguration config, ChartRect textBlock, DateTimeOffset now)
    {
        var area = ResolveChartStackArea(image, config, textBlock);
        if (area.Width <= 0 || area.Height <= 0)
        {
            area = new ChartRect(0, 0, image.Width, image.Height);
        }

        int offsetX = config.ChartStackOffsetX;
        int offsetY = config.ChartStackOffsetY;
        double spacing = Math.Max(0, config.ChartStackSpacing);
        bool outside = config.ChartStackAlignToTextBlock && config.ChartStackOutsideTextBlock && textBlock.Width > 0 && textBlock.Height > 0;
        double? cursorX = null;
        double? cursorY = null;

        for (int i = 0; i < config.Charts.Count; i++)
        {
            var chart = config.Charts[i];
            var values = ResolveChartValues(config, chart, now, i);
            if (values.Count == 0)
            {
                continue;
            }

            int width = Math.Max(1, chart.Width);
            int height = Math.Max(1, chart.Height);
            var basePosition = outside
                ? ResolveChartPositionOutsideTextBlock(textBlock, width, height, config.ChartStackAnchor, offsetX, offsetY)
                : ResolveChartPosition(area, width, height, config.ChartStackAnchor, offsetX, offsetY);

            double positionX = cursorX ?? basePosition.X;
            double positionY = cursorY ?? basePosition.Y;
            if (config.ChartStackDirection == BgInfoChartStackDirection.Vertical)
            {
                var direction = ResolveStackDirection(config.ChartStackAnchor, vertical: true);
                if (outside) direction *= -1;
                positionX = basePosition.X;
                cursorY = positionY + (height + spacing) * direction;
            }
            else
            {
                var direction = ResolveStackDirection(config.ChartStackAnchor, vertical: false);
                if (outside) direction *= -1;
                positionY = basePosition.Y;
                cursorX = positionX + (width + spacing) * direction;
            }

            var position = new ChartPoint(positionX, positionY);
            using var chartImage = BgInfoChartRenderer.Render(chart, values, config);
            image.DrawImage(chartImage, position.X, position.Y);
        }
    }

    private static ChartRect ResolveChartStackArea(BgInfoRasterImage image, BgInfoConfiguration config, ChartRect textBlock)
    {
        if (!config.ChartStackAlignToTextBlock)
        {
            return new ChartRect(0, 0, image.Width, image.Height);
        }

        if (textBlock.Width <= 0 || textBlock.Height <= 0)
        {
            return new ChartRect(0, 0, image.Width, image.Height);
        }

        return textBlock;
    }

    private static ChartPoint ResolveChartPositionOutsideTextBlock(ChartRect textBlock, int chartWidth, int chartHeight, BgInfoTextPosition anchor, int offsetX, int offsetY)
    {
        double x;
        double y;
        switch (anchor)
        {
            case BgInfoTextPosition.TopLeft:
                x = textBlock.X + offsetX;
                y = textBlock.Y - chartHeight - offsetY;
                break;
            case BgInfoTextPosition.TopCenter:
                x = textBlock.X + (textBlock.Width - chartWidth) / 2f + offsetX;
                y = textBlock.Y - chartHeight - offsetY;
                break;
            case BgInfoTextPosition.TopRight:
                x = textBlock.X + textBlock.Width - chartWidth - offsetX;
                y = textBlock.Y - chartHeight - offsetY;
                break;
            case BgInfoTextPosition.MiddleLeft:
                x = textBlock.X - chartWidth - offsetX;
                y = textBlock.Y + (textBlock.Height - chartHeight) / 2f + offsetY;
                break;
            case BgInfoTextPosition.MiddleCenter:
                x = textBlock.X + (textBlock.Width - chartWidth) / 2f + offsetX;
                y = textBlock.Y + textBlock.Height + offsetY;
                break;
            case BgInfoTextPosition.MiddleRight:
                x = textBlock.X + textBlock.Width + offsetX;
                y = textBlock.Y + (textBlock.Height - chartHeight) / 2f + offsetY;
                break;
            case BgInfoTextPosition.BottomLeft:
                x = textBlock.X + offsetX;
                y = textBlock.Y + textBlock.Height + offsetY;
                break;
            case BgInfoTextPosition.BottomCenter:
                x = textBlock.X + (textBlock.Width - chartWidth) / 2f + offsetX;
                y = textBlock.Y + textBlock.Height + offsetY;
                break;
            case BgInfoTextPosition.BottomRight:
                x = textBlock.X + textBlock.Width - chartWidth - offsetX;
                y = textBlock.Y + textBlock.Height + offsetY;
                break;
            default:
                x = textBlock.X + offsetX;
                y = textBlock.Y + textBlock.Height + offsetY;
                break;
        }

        return new ChartPoint(x, y);
    }

    internal static IReadOnlyList<string> WrapTextLines(BgInfoRasterImage image, string? text, double wrapWidth, double fontSize, string fontFamilyName)
    {
        if (image == null) throw new ArgumentNullException(nameof(image));

        var normalized = NormalizeLineEndings(text);
        if (normalized.Length == 0)
        {
            return new[] { string.Empty };
        }

        var paragraphs = normalized.Split('\n');
        if (wrapWidth <= 0)
        {
            return paragraphs.Length == 0 ? new[] { string.Empty } : paragraphs;
        }

        var lines = new List<string>();
        foreach (var paragraph in paragraphs)
        {
            if (paragraph.Length == 0)
            {
                lines.Add(string.Empty);
                continue;
            }

            AddWrappedParagraph(lines, image, paragraph, wrapWidth, fontSize, fontFamilyName);
        }

        return lines.Count == 0 ? new[] { string.Empty } : lines;
    }

    private static int ResolveStackDirection(BgInfoTextPosition anchor, bool vertical)
    {
        if (vertical)
        {
            return anchor switch
            {
                BgInfoTextPosition.BottomLeft => -1,
                BgInfoTextPosition.BottomCenter => -1,
                BgInfoTextPosition.BottomRight => -1,
                _ => 1
            };
        }

        return anchor switch
        {
            BgInfoTextPosition.TopRight => -1,
            BgInfoTextPosition.MiddleRight => -1,
            BgInfoTextPosition.BottomRight => -1,
            _ => 1
        };
    }

    private static IReadOnlyList<double> ResolveChartValues(BgInfoConfiguration config, BgInfoChart chart, DateTimeOffset now, int index)
    {
        var values = ResolveChartInputValues(chart);
        if (!chart.UseHistory)
        {
            return values;
        }

        var historyPath = GetChartHistoryPath(config, chart, index);
        var samples = ChartHistoryStore.Load(historyPath);
        if (!chart.AppendValues)
        {
            samples.Clear();
        }

        if (values.Count > 0)
        {
            foreach (var value in values)
            {
                samples.Add(new ChartSample(now, value));
            }
        }

        if (chart.MaxPoints > 0 && samples.Count > chart.MaxPoints)
        {
            samples = samples.Skip(samples.Count - chart.MaxPoints).ToList();
        }

        ChartHistoryStore.Save(historyPath, samples);
        return samples.Select(s => s.Value).ToList();
    }

    private static IReadOnlyList<double> ResolveChartInputValues(BgInfoChart chart)
    {
        var values = chart.Values ?? Array.Empty<double>();
        if (values.Count > 0)
        {
            return values;
        }

        if (chart.Metric == BgInfoChartMetric.None)
        {
            return Array.Empty<double>();
        }

        if (SystemInfoProvider.TryGetNumericValue(chart.Metric, chart.MetricArgument, out var value))
        {
            return new[] { value };
        }

        return Array.Empty<double>();
    }

    private static string GetChartHistoryPath(BgInfoConfiguration config, BgInfoChart chart, int index)
    {
        var key = GetChartHistoryKey(chart, index);
        var folder = Path.Combine(config.ConfigurationDirectory, "Charts");
        return Path.Combine(folder, $"{key}.txt");
    }

    private static string GetChartHistoryKey(BgInfoChart chart, int index)
    {
        if (!string.IsNullOrWhiteSpace(chart.Id))
        {
            return SanitizeFileName(chart.Id);
        }

        var key = string.IsNullOrWhiteSpace(chart.Title)
            ? "chart"
            : chart.Title;
        return SanitizeFileName($"{key}_{index}");
    }

    private static string SanitizeFileName(string name)
    {
        var invalid = Path.GetInvalidFileNameChars();
        var buffer = name.ToCharArray();
        for (int i = 0; i < buffer.Length; i++)
        {
            if (invalid.Contains(buffer[i]))
            {
                buffer[i] = '_';
            }
        }
        return new string(buffer);
    }

    private static ChartPoint ResolveChartPosition(BgInfoRasterImage image, BgInfoChart chart)
    {
        if (chart.PositionX.HasValue && chart.PositionY.HasValue)
        {
            return new ChartPoint(chart.PositionX.Value, chart.PositionY.Value);
        }

        double chartWidth = Math.Max(1, chart.Width);
        double chartHeight = Math.Max(1, chart.Height);
        return ResolveChartPosition(new ChartRect(0, 0, image.Width, image.Height),
            (int)chartWidth, (int)chartHeight, chart.Anchor, chart.OffsetX, chart.OffsetY);
    }

    private static ChartPoint ResolveTopologyPosition(BgInfoRasterImage image, BgInfoTopology topology)
    {
        if (topology.PositionX.HasValue && topology.PositionY.HasValue)
        {
            return new ChartPoint(topology.PositionX.Value, topology.PositionY.Value);
        }

        return ResolveChartPosition(new ChartRect(0, 0, image.Width, image.Height),
            Math.Max(1, topology.Width), Math.Max(1, topology.Height), topology.Anchor, topology.OffsetX, topology.OffsetY);
    }

    private static ChartPoint ResolveImagePosition(BgInfoRasterImage image, BgInfoImage overlay, int width, int height)
    {
        var anchored = ResolveChartPosition(new ChartRect(0, 0, image.Width, image.Height),
            width, height, overlay.Anchor, overlay.OffsetX, overlay.OffsetY);
        return new ChartPoint(overlay.PositionX ?? anchored.X, overlay.PositionY ?? anchored.Y);
    }

    private static (int Width, int Height) ResolveImageSize(BgInfoImage overlay, int sourceWidth, int sourceHeight)
    {
        var width = overlay.Width;
        var height = overlay.Height;
        if (width <= 0 && height <= 0)
        {
            return (Math.Max(1, sourceWidth), Math.Max(1, sourceHeight));
        }
        if (width > 0 && height <= 0)
        {
            height = (int)Math.Round(sourceHeight * (width / (double)Math.Max(1, sourceWidth)));
        }
        if (height > 0 && width <= 0)
        {
            width = (int)Math.Round(sourceWidth * (height / (double)Math.Max(1, sourceHeight)));
        }

        return (Math.Max(1, width), Math.Max(1, height));
    }

    internal static ChartPoint ResolveChartPosition(ChartRect area, int chartWidth, int chartHeight, BgInfoTextPosition anchor, int offsetX, int offsetY)
    {
        double x = area.X + offsetX;
        double y = area.Y + offsetY;
        switch (anchor)
        {
            case BgInfoTextPosition.TopCenter:
                x = area.X + (area.Width - chartWidth) / 2f + offsetX;
                break;
            case BgInfoTextPosition.TopRight:
                x = area.X + area.Width - chartWidth - offsetX;
                break;
            case BgInfoTextPosition.MiddleLeft:
                y = area.Y + (area.Height - chartHeight) / 2f + offsetY;
                break;
            case BgInfoTextPosition.MiddleCenter:
                x = area.X + (area.Width - chartWidth) / 2f + offsetX;
                y = area.Y + (area.Height - chartHeight) / 2f + offsetY;
                break;
            case BgInfoTextPosition.MiddleRight:
                x = area.X + area.Width - chartWidth - offsetX;
                y = area.Y + (area.Height - chartHeight) / 2f + offsetY;
                break;
            case BgInfoTextPosition.BottomLeft:
                y = area.Y + area.Height - chartHeight - offsetY;
                break;
            case BgInfoTextPosition.BottomCenter:
                x = area.X + (area.Width - chartWidth) / 2f + offsetX;
                y = area.Y + area.Height - chartHeight - offsetY;
                break;
            case BgInfoTextPosition.BottomRight:
                x = area.X + area.Width - chartWidth - offsetX;
                y = area.Y + area.Height - chartHeight - offsetY;
                break;
            case BgInfoTextPosition.TopLeft:
            default:
                break;
        }

        return new ChartPoint(x, y);
    }

    private static List<EntryLayout> BuildEntryLayouts(BgInfoRasterImage image, BgInfoConfiguration config, IReadOnlyList<BgInfoEntry> entries)
    {
        var layouts = new List<EntryLayout>();
        foreach (var entry in entries)
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

            var labelSize = image.GetTextSize(entry.Name, entry.FontSize!.Value, entry.FontFamilyName!);
            var resolvedValue = ResolveEntryValue(entry);
            var valueLines = entry.Type == BgInfoEntryType.Label
                ? Array.Empty<string>()
                : WrapTextLines(image, resolvedValue, config.ValueWrapWidth, entry.ValueFontSize!.Value, entry.ValueFontFamilyName!).ToArray();
            var valueLineHeight = entry.Type == BgInfoEntryType.Label
                ? 0f
                : GetLineHeight(image, entry.ValueFontSize!.Value, entry.ValueFontFamilyName!);
            var valueWidth = valueLines.Length == 0
                ? 0f
                : valueLines.Max(line => image.GetTextSize(line, entry.ValueFontSize!.Value, entry.ValueFontFamilyName!).Width);
            var valueHeight = valueLines.Length == 0 ? 0f : valueLineHeight * valueLines.Length;
            layouts.Add(new EntryLayout(
                entry,
                labelSize.Width,
                labelSize.Height,
                valueLines,
                valueWidth,
                valueLineHeight,
                Math.Max(labelSize.Height, valueHeight)));
        }

        return layouts;
    }

    private static string? ResolveEntryValue(BgInfoEntry entry)
    {
        if (!string.IsNullOrWhiteSpace(entry.BuiltinValue))
        {
            return SystemInfoProvider.GetValue(entry.BuiltinValue!);
        }

        return entry.Value;
    }

    private static double GetTextBlockHeight(BgInfoConfiguration config, IReadOnlyList<EntryLayout> entryLayouts)
    {
        if (entryLayouts.Count == 0)
        {
            return 0f;
        }

        return entryLayouts.Sum(layout => layout.RowHeight) + (entryLayouts.Count - 1) * config.SpaceBetweenLines;
    }

    private static double GetLineHeight(BgInfoRasterImage image, double fontSize, string fontFamilyName)
    {
        return image.GetTextSize("Ag", fontSize, fontFamilyName).Height;
    }

    private static string NormalizeLineEndings(string? text)
    {
        return (text ?? string.Empty).Replace("\r\n", "\n").Replace('\r', '\n');
    }

    private static void AddWrappedParagraph(List<string> lines, BgInfoRasterImage image, string paragraph, double wrapWidth, double fontSize, string fontFamilyName)
    {
        var words = paragraph.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
        if (words.Length == 0)
        {
            lines.Add(string.Empty);
            return;
        }

        var current = string.Empty;
        foreach (var word in words)
        {
            var candidate = string.IsNullOrEmpty(current) ? word : current + " " + word;
            if (image.GetTextSize(candidate, fontSize, fontFamilyName).Width <= wrapWidth)
            {
                current = candidate;
                continue;
            }

            if (!string.IsNullOrEmpty(current))
            {
                lines.Add(current);
            }

            if (image.GetTextSize(word, fontSize, fontFamilyName).Width <= wrapWidth)
            {
                current = word;
                continue;
            }

            foreach (var fragment in WrapLongWord(image, word, wrapWidth, fontSize, fontFamilyName))
            {
                if (image.GetTextSize(fragment, fontSize, fontFamilyName).Width <= wrapWidth)
                {
                    lines.Add(fragment);
                }
            }
            current = string.Empty;
        }

        if (!string.IsNullOrEmpty(current))
        {
            lines.Add(current);
        }
    }

    private static IEnumerable<string> WrapLongWord(BgInfoRasterImage image, string word, double wrapWidth, double fontSize, string fontFamilyName)
    {
        var current = string.Empty;
        foreach (var character in word)
        {
            var candidate = current + character;
            if (!string.IsNullOrEmpty(current) && image.GetTextSize(candidate, fontSize, fontFamilyName).Width > wrapWidth)
            {
                yield return current;
                current = character.ToString();
                continue;
            }

            current = candidate;
        }

        if (!string.IsNullOrEmpty(current))
        {
            yield return current;
        }
    }

    private sealed class EntryLayout
    {
        public EntryLayout(
            BgInfoEntry entry,
            double labelWidth,
            double labelHeight,
            string[] valueLines,
            double valueWidth,
            double valueLineHeight,
            double rowHeight)
        {
            Entry = entry;
            LabelWidth = labelWidth;
            LabelHeight = labelHeight;
            ValueLines = valueLines;
            ValueWidth = valueWidth;
            ValueLineHeight = valueLineHeight;
            RowHeight = rowHeight;
        }

        public BgInfoEntry Entry { get; }
        public double LabelWidth { get; }
        public double LabelHeight { get; }
        public string[] ValueLines { get; }
        public double ValueWidth { get; }
        public double ValueLineHeight { get; }
        public double RowHeight { get; }
    }
}
