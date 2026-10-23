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
    private bool TryGenerateWallpaperSlideshow(BgInfoConfiguration config, Func<IReadOnlyList<BgInfoMonitor>?> getMonitors, bool applyCurrentWallpaper, out string outputPath)
    {
        outputPath = string.Empty;
        if (!applyCurrentWallpaper || !ShouldPreserveWallpaperSlideshow(config))
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

        // After the first run the slideshow shows the rendered copies; go back to the images they were drawn on.
        var slideshowImagePaths = ResolveSlideshowSourcePaths(slideshow.ImagePaths);
        var knownOutputPaths = new HashSet<string>(BgInfoPaths.Comparer);
        for (var index = 0; index < slideshowImagePaths.Length; index++)
        {
            knownOutputPaths.Add(NormalizePath(BuildSlideshowOutputPath(config, slideshowImagePaths[index], index)));
        }

        var sourcePaths = slideshowImagePaths
            .Select(path => ResolveOriginalImage(path, null, "The slideshow image", chosenByUser: false, knownOutputPaths))
            .Where(path => !string.IsNullOrWhiteSpace(path) && File.Exists(path))
            .Select(path => path!)
            .Distinct(BgInfoPaths.Comparer)
            .ToArray();
        if (sourcePaths.Length == 0)
        {
            return false;
        }

        var generatedPaths = new List<string>(sourcePaths.Length);
        var slideshowCharts = BuildSlideshowCharts(config);
        for (int i = 0; i < sourcePaths.Length; i++)
        {
            var itemConfig = CloneForSlideshowItem(config, sourcePaths[i], BuildSlideshowOutputPath(config, sourcePaths[i], i), slideshowCharts);
            generatedPaths.Add(GenerateImage(itemConfig, _ => null, getMonitors, applyTargets: false));
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
                RecheckSelectedNativeTarget();
                _wallpaperService.SetWallpaperForAllUsers(outputPath, config.WallpaperFit, config.IncludeDefaultUserProfile);
            }

            RecheckSelectedNativeTarget();
            _wallpaperService.StartWallpaperSlideshow(generatedPaths, config.WallpaperFit, slideshow.Options, slideshow.SlideshowTick);
        }

        if (config.Target.HasFlag(BgInfoTarget.LogonScreen))
        {
            RecheckSelectedNativeTarget();
            _wallpaperService.SetLogonWallpaper(outputPath);
        }

        return true;
    }

    private static bool ShouldPreserveWallpaperSlideshow(BgInfoConfiguration config)
    {
        return config.PreserveWallpaperSlideshow
            && config.Background == null
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
            .Distinct(BgInfoPaths.Comparer)
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

    private IReadOnlyList<BgInfoChart> BuildSlideshowCharts(BgInfoConfiguration source)
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
            var values = ResolveChartValues(source, sourceChart, now, i, Warning);
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
            ThemeMode = source.ThemeMode,
            Width = source.Width,
            Height = source.Height,
            Anchor = source.Anchor,
            Region = source.Region,
            RegionOrder = source.RegionOrder,
            Threshold = source.Threshold,
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
            CornerRadius = source.CornerRadius,
            BorderColor = source.BorderColor,
            LineColor = source.LineColor,
            FillColor = source.FillColor,
            Palette = source.Palette.ToArray(),
            TextColor = source.TextColor,
            TitleColor = source.TitleColor,
            ValueColor = source.ValueColor,
            FontFamilyName = source.FontFamilyName,
            TitleFontSize = source.TitleFontSize,
            ValueFontSize = source.ValueFontSize,
            TitleBold = source.TitleBold,
            TitleFontWeight = source.TitleFontWeight,
            TitleItalic = source.TitleItalic,
            TitleUnderline = source.TitleUnderline,
            TitleUnderlineStyle = source.TitleUnderlineStyle,
            TitleStrikethroughStyle = source.TitleStrikethroughStyle,
            TitleBaseline = source.TitleBaseline,
            TitleTextCase = source.TitleTextCase,
            ValueBold = source.ValueBold,
            ValueFontWeight = source.ValueFontWeight,
            ValueItalic = source.ValueItalic,
            ValueUnderline = source.ValueUnderline,
            ValueUnderlineStyle = source.ValueUnderlineStyle,
            ValueStrikethroughStyle = source.ValueStrikethroughStyle,
            ValueBaseline = source.ValueBaseline,
            ValueTextCase = source.ValueTextCase,
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
            FontSize = source.FontSize,
            Bold = source.Bold,
            FontWeight = source.FontWeight,
            Italic = source.Italic,
            Underline = source.Underline,
            UnderlineStyle = source.UnderlineStyle,
            StrikethroughStyle = source.StrikethroughStyle,
            Baseline = source.Baseline,
            TextCase = source.TextCase,
            ValueFontSize = source.ValueFontSize,
            ValueBold = source.ValueBold,
            ValueFontWeight = source.ValueFontWeight,
            ValueItalic = source.ValueItalic,
            ValueUnderline = source.ValueUnderline,
            ValueUnderlineStyle = source.ValueUnderlineStyle,
            ValueStrikethroughStyle = source.ValueStrikethroughStyle,
            ValueBaseline = source.ValueBaseline,
            ValueTextCase = source.ValueTextCase,
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
            Target = source.Target,
            ForceWallpaperRefresh = false,
            PreserveWallpaperSlideshow = false,
            ApplyToAllUsers = false,
            IncludeDefaultUserProfile = source.IncludeDefaultUserProfile,
            UseScreenCoordinates = source.UseScreenCoordinates
        };

        if (source.CustomColor.HasValue) clone.Color = source.CustomColor.Value;
        if (source.CustomValueColor.HasValue) clone.ValueColor = source.CustomValueColor.Value;
        if (source.CustomFontFamilyName != null) clone.FontFamilyName = source.CustomFontFamilyName;
        if (source.CustomValueFontFamilyName != null) clone.ValueFontFamilyName = source.CustomValueFontFamilyName;
        clone.Variables.AddRange(source.Variables);
        clone.Entries.AddRange(source.Entries);
        clone.Charts.AddRange(charts);
        clone.Topologies.AddRange(source.Topologies);
        clone.VisualCanvases.AddRange(source.VisualCanvases);
        clone.Images.AddRange(source.Images);
        clone.Widgets.AddRange(source.Widgets);
        clone.Layout = source.Layout;
        return clone;
    }

}
