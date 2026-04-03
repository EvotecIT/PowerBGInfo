using System;
using System.IO;
using System.Linq;
using ImagePlayground.Gdi;
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

        bool hasBaseImage = !string.IsNullOrEmpty(imagePath) && File.Exists(imagePath);
        var outputPath = BuildOutputPath(config, imagePath, hasBaseImage);
        var outputDirectory = Path.GetDirectoryName(outputPath);
        if (!string.IsNullOrWhiteSpace(outputDirectory))
        {
            Directory.CreateDirectory(outputDirectory);
        }

        using var image = hasBaseImage
            ? LoadBaseImage(imagePath, outputPath)
            : CreateBaseImage(config, monitors, outputPath);

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
        float textStartX;
        float textStartY;

        if (config.UseScreenCoordinates)
        {
            var (screenWidth, screenHeight) = GetMonitorSize(monitors, config.MonitorIndex);

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

        textStartX = posX;
        textStartY = posY;

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

        var textBlockHeight = config.Entries.Count == 0
            ? 0f
            : (config.Entries.Count * (highestHeight + config.SpaceBetweenLines)) - config.SpaceBetweenLines;
        var textBlock = new System.Drawing.RectangleF(textStartX, textStartY, totalWidth, Math.Max(0f, textBlockHeight));

        RenderCharts(image, config, textBlock);

        _imageService.Save(image, outputPath);

        if (config.Target.HasFlag(BgInfoTarget.Wallpaper))
        {
            if (config.ApplyToAllUsers)
            {
                _wallpaperService.SetWallpaperForAllUsers(outputPath, config.WallpaperFit, config.IncludeDefaultUserProfile);
            }

            ApplyWallpaper(config, outputPath);
        }

        if (config.Target.HasFlag(BgInfoTarget.LogonScreen))
        {
            _wallpaperService.SetLogonWallpaper(outputPath);
        }

        return outputPath;
    }

    private Image LoadBaseImage(string imagePath, string outputPath)
    {
        if (!PathsEqual(imagePath, outputPath))
        {
            File.Copy(imagePath, outputPath, true);
        }
        return _imageService.Load(outputPath);
    }

    private static Image CreateBaseImage(BgInfoConfiguration config, Monitors monitors, string outputPath)
    {
        var (width, height) = GetMonitorSize(monitors, config.MonitorIndex);
        var background = ResolveBackgroundColor(config, monitors);
        var image = new Image();
        image.Create(outputPath, width, height, background);
        return image;
    }

    private static (int Width, int Height) GetMonitorSize(Monitors monitors, int monitorIndex)
    {
        var monitor = monitors.GetMonitors(index: monitorIndex).FirstOrDefault()
            ?? monitors.GetMonitors(primaryOnly: true).FirstOrDefault()
            ?? monitors.GetMonitors().FirstOrDefault();
        if (monitor == null)
        {
            return (1920, 1080);
        }
        int width = Math.Max(1, monitor.PositionRight - monitor.PositionLeft);
        int height = Math.Max(1, monitor.PositionBottom - monitor.PositionTop);
        return (width, height);
    }

    private static System.Drawing.Color ResolveBackgroundColor(BgInfoConfiguration config, Monitors monitors)
    {
        if (config.BackgroundColor.HasValue)
        {
            return config.BackgroundColor.Value;
        }

        uint rgb = monitors.GetBackgroundColor();
        byte r = (byte)(rgb & 0xFF);
        byte g = (byte)((rgb >> 8) & 0xFF);
        byte b = (byte)((rgb >> 16) & 0xFF);
        return System.Drawing.Color.FromArgb(255, r, g, b);
    }

    private static string BuildOutputPath(BgInfoConfiguration config, string imagePath, bool hasBaseImage)
    {
        if (!string.IsNullOrWhiteSpace(config.OutputFileName))
        {
            return Path.IsPathRooted(config.OutputFileName)
                ? config.OutputFileName
                : Path.Combine(config.ConfigurationDirectory, config.OutputFileName);
        }

        if (hasBaseImage)
        {
            var fileName = Path.GetFileNameWithoutExtension(imagePath) + "_PowerBgInfo" + Path.GetExtension(imagePath);
            return Path.Combine(config.ConfigurationDirectory, fileName);
        }

        return Path.Combine(config.ConfigurationDirectory, "PowerBgInfo.png");
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

    private static bool PathsEqual(string left, string right)
    {
        if (string.IsNullOrWhiteSpace(left) || string.IsNullOrWhiteSpace(right))
        {
            return false;
        }
        return string.Equals(Path.GetFullPath(left), Path.GetFullPath(right), StringComparison.OrdinalIgnoreCase);
    }

    private static void RenderCharts(Image image, BgInfoConfiguration config, System.Drawing.RectangleF textBlock)
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

    private static void RenderStackedCharts(Image image, BgInfoConfiguration config, System.Drawing.RectangleF textBlock, DateTimeOffset now)
    {
        var area = ResolveChartStackArea(image, config, textBlock);
        if (area.Width <= 0 || area.Height <= 0)
        {
            area = new System.Drawing.RectangleF(0, 0, image.Width, image.Height);
        }

        int offsetX = config.ChartStackOffsetX;
        int offsetY = config.ChartStackOffsetY;
        float spacing = Math.Max(0, config.ChartStackSpacing);

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
            var basePosition = config.ChartStackAlignToTextBlock && config.ChartStackOutsideTextBlock && textBlock.Width > 0 && textBlock.Height > 0
                ? ResolveChartPositionOutsideTextBlock(textBlock, width, height, config.ChartStackAnchor, offsetX, offsetY)
                : ResolveChartPosition(area, width, height, config.ChartStackAnchor, offsetX, offsetY);

            float stepX = 0;
            float stepY = 0;
            bool outside = config.ChartStackAlignToTextBlock && config.ChartStackOutsideTextBlock && textBlock.Width > 0 && textBlock.Height > 0;
            if (config.ChartStackDirection == BgInfoChartStackDirection.Vertical)
            {
                var direction = ResolveStackDirection(config.ChartStackAnchor, vertical: true);
                if (outside) direction *= -1;
                stepY = (height + spacing) * direction;
            }
            else
            {
                var direction = ResolveStackDirection(config.ChartStackAnchor, vertical: false);
                if (outside) direction *= -1;
                stepX = (width + spacing) * direction;
            }

            var position = new System.Drawing.PointF(basePosition.X + stepX * i, basePosition.Y + stepY * i);
            using var chartImage = BgInfoChartRenderer.Render(chart, values, config);
            image.DrawImage(chartImage, position.X, position.Y);
        }
    }

    private static System.Drawing.RectangleF ResolveChartStackArea(Image image, BgInfoConfiguration config, System.Drawing.RectangleF textBlock)
    {
        if (!config.ChartStackAlignToTextBlock)
        {
            return new System.Drawing.RectangleF(0, 0, image.Width, image.Height);
        }

        if (textBlock.Width <= 0 || textBlock.Height <= 0)
        {
            return new System.Drawing.RectangleF(0, 0, image.Width, image.Height);
        }

        return textBlock;
    }

    private static System.Drawing.PointF ResolveChartPositionOutsideTextBlock(System.Drawing.RectangleF textBlock, int chartWidth, int chartHeight, BgInfoTextPosition anchor, int offsetX, int offsetY)
    {
        float x;
        float y;
        switch (anchor)
        {
            case BgInfoTextPosition.TopLeft:
                x = textBlock.X + offsetX;
                y = textBlock.Y - chartHeight - offsetY;
                break;
            case BgInfoTextPosition.TopCenter:
                x = textBlock.X + (textBlock.Width - chartWidth) / 2f;
                y = textBlock.Y - chartHeight - offsetY;
                break;
            case BgInfoTextPosition.TopRight:
                x = textBlock.X + textBlock.Width - chartWidth - offsetX;
                y = textBlock.Y - chartHeight - offsetY;
                break;
            case BgInfoTextPosition.MiddleLeft:
                x = textBlock.X - chartWidth - offsetX;
                y = textBlock.Y + (textBlock.Height - chartHeight) / 2f;
                break;
            case BgInfoTextPosition.MiddleCenter:
                x = textBlock.X + (textBlock.Width - chartWidth) / 2f;
                y = textBlock.Y + textBlock.Height + offsetY;
                break;
            case BgInfoTextPosition.MiddleRight:
                x = textBlock.X + textBlock.Width + offsetX;
                y = textBlock.Y + (textBlock.Height - chartHeight) / 2f;
                break;
            case BgInfoTextPosition.BottomLeft:
                x = textBlock.X + offsetX;
                y = textBlock.Y + textBlock.Height + offsetY;
                break;
            case BgInfoTextPosition.BottomCenter:
                x = textBlock.X + (textBlock.Width - chartWidth) / 2f;
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

        return new System.Drawing.PointF(x, y);
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
        var key = string.IsNullOrWhiteSpace(chart.Id) ? chart.Title : chart.Id;
        if (string.IsNullOrWhiteSpace(key))
        {
            key = $"chart_{index}";
        }
        return SanitizeFileName(key);
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

    private static System.Drawing.PointF ResolveChartPosition(Image image, BgInfoChart chart)
    {
        if (chart.PositionX.HasValue && chart.PositionY.HasValue)
        {
            return new System.Drawing.PointF(chart.PositionX.Value, chart.PositionY.Value);
        }

        float chartWidth = Math.Max(1, chart.Width);
        float chartHeight = Math.Max(1, chart.Height);
        return ResolveChartPosition(new System.Drawing.RectangleF(0, 0, image.Width, image.Height),
            (int)chartWidth, (int)chartHeight, chart.Anchor, chart.OffsetX, chart.OffsetY);
    }

    private static System.Drawing.PointF ResolveChartPosition(System.Drawing.RectangleF area, int chartWidth, int chartHeight, BgInfoTextPosition anchor, int offsetX, int offsetY)
    {
        float x = area.X + offsetX;
        float y = area.Y + offsetY;
        switch (anchor)
        {
            case BgInfoTextPosition.TopCenter:
                x = area.X + (area.Width - chartWidth) / 2f;
                break;
            case BgInfoTextPosition.TopRight:
                x = area.X + area.Width - chartWidth - offsetX;
                break;
            case BgInfoTextPosition.MiddleLeft:
                y = area.Y + (area.Height - chartHeight) / 2f;
                break;
            case BgInfoTextPosition.MiddleCenter:
                x = area.X + (area.Width - chartWidth) / 2f;
                y = area.Y + (area.Height - chartHeight) / 2f;
                break;
            case BgInfoTextPosition.MiddleRight:
                x = area.X + area.Width - chartWidth - offsetX;
                y = area.Y + (area.Height - chartHeight) / 2f;
                break;
            case BgInfoTextPosition.BottomLeft:
                y = area.Y + area.Height - chartHeight - offsetY;
                break;
            case BgInfoTextPosition.BottomCenter:
                x = area.X + (area.Width - chartWidth) / 2f;
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

        return new System.Drawing.PointF(x, y);
    }
}
