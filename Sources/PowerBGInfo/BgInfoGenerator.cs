using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
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

        var imagePath = ResolveBaseImagePath(config, index => GetWallpaper(GetMonitors(), index));

        bool hasBaseImage = !string.IsNullOrEmpty(imagePath) && File.Exists(imagePath);
        var outputPath = BuildOutputPath(config, imagePath, hasBaseImage);
        var outputDirectory = Path.GetDirectoryName(outputPath);
        if (!string.IsNullOrWhiteSpace(outputDirectory))
        {
            Directory.CreateDirectory(outputDirectory);
        }

        using var image = hasBaseImage
            ? LoadBaseImage(imagePath!, outputPath)
            : CreateBaseImage(config, GetMonitors(), outputPath);

        var expandedEntries = BgInfoVariableResolver.ExpandEntries(config);
        var entryLayouts = BuildEntryLayouts(image, config, expandedEntries);
        float highestWidth = entryLayouts.Count == 0 ? 0f : entryLayouts.Max(layout => layout.LabelWidth);
        float highestValueWidth = entryLayouts.Count == 0 ? 0f : entryLayouts.Max(layout => layout.ValueWidth);
        bool hasValueEntries = entryLayouts.Any(layout => layout.Entry.Type != BgInfoEntryType.Label);
        var totalWidth = highestWidth + (hasValueEntries ? config.SpaceBetweenColumns + highestValueWidth : 0f);
        var textBlockHeight = GetTextBlockHeight(config, entryLayouts);

        float posX;
        float posY;
        float textStartX;
        float textStartY;

        if (config.UseScreenCoordinates)
        {
            var (screenWidth, screenHeight) = GetMonitorSize(GetMonitors(), config.MonitorIndex);

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

    private static Image CreateBaseImage(BgInfoConfiguration config, Monitors? monitors, string outputPath)
    {
        var (width, height) = GetMonitorSize(monitors, config.MonitorIndex);
        var background = ResolveBackgroundColor(config, monitors);
        var image = new Image();
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

    private static System.Drawing.Color ResolveBackgroundColor(BgInfoConfiguration config, Monitors? monitors)
    {
        if (config.BackgroundColor.HasValue)
        {
            return config.BackgroundColor.Value;
        }

        if (monitors == null)
        {
            return System.Drawing.Color.Black;
        }

        uint rgb;
        try
        {
            rgb = monitors.GetBackgroundColor();
        }
        catch
        {
            return System.Drawing.Color.Black;
        }

        byte r = (byte)(rgb & 0xFF);
        byte g = (byte)((rgb >> 8) & 0xFF);
        byte b = (byte)((rgb >> 16) & 0xFF);
        return System.Drawing.Color.FromArgb(255, r, g, b);
    }

    private static string BuildOutputPath(BgInfoConfiguration config, string? imagePath, bool hasBaseImage)
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
        bool outside = config.ChartStackAlignToTextBlock && config.ChartStackOutsideTextBlock && textBlock.Width > 0 && textBlock.Height > 0;
        float? cursorX = null;
        float? cursorY = null;

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

            float positionX = cursorX ?? basePosition.X;
            float positionY = cursorY ?? basePosition.Y;
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

            var position = new System.Drawing.PointF(positionX, positionY);
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

        return new System.Drawing.PointF(x, y);
    }

    internal static IReadOnlyList<string> WrapTextLines(Image image, string? text, float wrapWidth, float fontSize, string fontFamilyName)
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

    internal static System.Drawing.PointF ResolveChartPosition(System.Drawing.RectangleF area, int chartWidth, int chartHeight, BgInfoTextPosition anchor, int offsetX, int offsetY)
    {
        float x = area.X + offsetX;
        float y = area.Y + offsetY;
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

        return new System.Drawing.PointF(x, y);
    }

    private static List<EntryLayout> BuildEntryLayouts(Image image, BgInfoConfiguration config, IReadOnlyList<BgInfoEntry> entries)
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

    private static float GetTextBlockHeight(BgInfoConfiguration config, IReadOnlyList<EntryLayout> entryLayouts)
    {
        if (entryLayouts.Count == 0)
        {
            return 0f;
        }

        return entryLayouts.Sum(layout => layout.RowHeight) + (entryLayouts.Count - 1) * config.SpaceBetweenLines;
    }

    private static float GetLineHeight(Image image, float fontSize, string fontFamilyName)
    {
        return image.GetTextSize("Ag", fontSize, fontFamilyName).Height;
    }

    private static string NormalizeLineEndings(string? text)
    {
        return (text ?? string.Empty).Replace("\r\n", "\n").Replace('\r', '\n');
    }

    private static void AddWrappedParagraph(List<string> lines, Image image, string paragraph, float wrapWidth, float fontSize, string fontFamilyName)
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

    private static IEnumerable<string> WrapLongWord(Image image, string word, float wrapWidth, float fontSize, string fontFamilyName)
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
            float labelWidth,
            float labelHeight,
            string[] valueLines,
            float valueWidth,
            float valueLineHeight,
            float rowHeight)
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
        public float LabelWidth { get; }
        public float LabelHeight { get; }
        public string[] ValueLines { get; }
        public float ValueWidth { get; }
        public float ValueLineHeight { get; }
        public float RowHeight { get; }
    }
}
