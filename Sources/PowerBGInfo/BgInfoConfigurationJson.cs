using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using ChartForgeX.Primitives;
using ChartForgeX.Topology;
using ChartForgeX.Typography;
using ChartForgeX.Themes;

#if NET472
using System.Web.Script.Serialization;
#else
using System.Text.Json;
using System.Text.Json.Serialization;
#endif

namespace PowerBGInfo;

/// <summary>
/// Loads and saves BGInfo configurations from JSON files.
/// </summary>
public static partial class BgInfoConfigurationJson {
    /// <summary>
    /// Loads a BGInfo configuration from JSON.
    /// </summary>
    /// <param name="path">Path to the JSON configuration file.</param>
    /// <param name="baseDirectoryOverride">Optional directory used to resolve relative paths instead of the JSON file location.</param>
    /// <returns>Configured BGInfo settings.</returns>
    public static BgInfoConfiguration Load(string path, string? baseDirectoryOverride = null) {
        if (string.IsNullOrWhiteSpace(path)) {
            throw new ArgumentException("Configuration path is required.", nameof(path));
        }

        var resolvedPath = Path.GetFullPath(path);
        var json = File.ReadAllText(resolvedPath);
        var model = Deserialize(json);
        return MapToConfiguration(model, resolvedPath, baseDirectoryOverride);
    }

    /// <summary>
    /// Saves a BGInfo configuration to JSON.
    /// </summary>
    /// <param name="configuration">BGInfo configuration instance.</param>
    /// <param name="path">Output path for the JSON file.</param>
    public static void Save(BgInfoConfiguration configuration, string path) {
        if (configuration == null) throw new ArgumentNullException(nameof(configuration));
        if (string.IsNullOrWhiteSpace(path)) {
            throw new ArgumentException("Configuration path is required.", nameof(path));
        }

        var model = MapFromConfiguration(configuration);
        var json = Serialize(model);
        File.WriteAllText(path, json);
    }

    private static BgInfoConfigurationFile Deserialize(string json) {
#if NET472
        var serializer = new JavaScriptSerializer { MaxJsonLength = int.MaxValue };
        var model = serializer.Deserialize<BgInfoConfigurationFile>(json);
        return model ?? new BgInfoConfigurationFile();
#else
        var model = JsonSerializer.Deserialize(json, ReadContext.BgInfoConfigurationFile);
        return model ?? new BgInfoConfigurationFile();
#endif
    }

    private static string Serialize(BgInfoConfigurationFile model) {
#if NET472
        var serializer = new JavaScriptSerializer { MaxJsonLength = int.MaxValue };
        return serializer.Serialize(model);
#else
        return JsonSerializer.Serialize(model, WriteContext.BgInfoConfigurationFile);
#endif
    }

#if !NET472
    private static JsonSerializerOptions CreateOptions() {
        return new JsonSerializerOptions {
            PropertyNameCaseInsensitive = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };
    }

    private static JsonSerializerOptions CreateWriteOptions() {
        var options = CreateOptions();
        options.WriteIndented = true;
        return options;
    }

    private static readonly BgInfoConfigurationJsonSerializerContext ReadContext = new(CreateOptions());
    private static readonly BgInfoConfigurationJsonSerializerContext WriteContext = new(CreateWriteOptions());
#endif

    private static BgInfoConfiguration MapToConfiguration(BgInfoConfigurationFile model, string sourcePath, string? baseDirectoryOverride = null) {
        var configuration = new BgInfoConfiguration();
        var baseDirectory = string.IsNullOrWhiteSpace(baseDirectoryOverride)
            ? Path.GetDirectoryName(sourcePath) ?? string.Empty
            : Path.GetFullPath(baseDirectoryOverride);

        var configurationDirectory = ResolvePath(model.ConfigurationDirectory, baseDirectory);
        if (string.IsNullOrWhiteSpace(configurationDirectory)) {
            configurationDirectory = baseDirectory;
        }
        configuration.ConfigurationDirectory = configurationDirectory;

        if (!string.IsNullOrWhiteSpace(model.FilePath)) {
            configuration.FilePath = ResolvePath(model.FilePath, baseDirectory);
        }
        if (!string.IsNullOrWhiteSpace(model.OutputFileName)) {
            configuration.OutputFileName = ResolvePath(model.OutputFileName, configurationDirectory);
        }
        if (!string.IsNullOrWhiteSpace(model.FontFamilyName)) {
            configuration.FontFamilyName = model.FontFamilyName!;
        }
        if (!string.IsNullOrWhiteSpace(model.ValueFontFamilyName)) {
            configuration.ValueFontFamilyName = model.ValueFontFamilyName!;
        }

        ApplyColor(model.Color, value => configuration.Color = value);
        ApplyColor(model.ValueColor, value => configuration.ValueColor = value);
        ApplyColor(model.BackgroundColor, value => configuration.BackgroundColor = value);

        if (model.FontSize.HasValue) configuration.FontSize = model.FontSize.Value;
        if (model.Bold.HasValue) configuration.Bold = model.Bold.Value;
        if (model.FontWeight.HasValue) configuration.FontWeight = model.FontWeight.Value;
        if (model.Italic.HasValue) configuration.Italic = model.Italic.Value;
        if (model.Underline.HasValue) configuration.Underline = model.Underline.Value;
        ApplyEnum<TextDecorationStyle>(model.UnderlineStyle, value => configuration.UnderlineStyle = value);
        ApplyEnum<TextDecorationStyle>(model.StrikethroughStyle, value => configuration.StrikethroughStyle = value);
        ApplyEnum<TextBaseline>(model.Baseline, value => configuration.Baseline = value);
        ApplyEnum<TextCaseTransform>(model.TextCase, value => configuration.TextCase = value);
        if (model.ValueFontSize.HasValue) configuration.ValueFontSize = model.ValueFontSize.Value;
        if (model.ValueBold.HasValue) configuration.ValueBold = model.ValueBold.Value;
        if (model.ValueFontWeight.HasValue) configuration.ValueFontWeight = model.ValueFontWeight.Value;
        if (model.ValueItalic.HasValue) configuration.ValueItalic = model.ValueItalic.Value;
        if (model.ValueUnderline.HasValue) configuration.ValueUnderline = model.ValueUnderline.Value;
        ApplyEnum<TextDecorationStyle>(model.ValueUnderlineStyle, value => configuration.ValueUnderlineStyle = value);
        ApplyEnum<TextDecorationStyle>(model.ValueStrikethroughStyle, value => configuration.ValueStrikethroughStyle = value);
        ApplyEnum<TextBaseline>(model.ValueBaseline, value => configuration.ValueBaseline = value);
        ApplyEnum<TextCaseTransform>(model.ValueTextCase, value => configuration.ValueTextCase = value);
        if (model.ValueWrapWidth.HasValue) configuration.ValueWrapWidth = model.ValueWrapWidth.Value;
        if (model.SpaceBetweenLines.HasValue) configuration.SpaceBetweenLines = model.SpaceBetweenLines.Value;
        if (model.SpaceBetweenColumns.HasValue) configuration.SpaceBetweenColumns = model.SpaceBetweenColumns.Value;
        if (model.PositionX.HasValue) configuration.PositionX = model.PositionX.Value;
        if (model.PositionY.HasValue) configuration.PositionY = model.PositionY.Value;
        if (model.MonitorIndex.HasValue) configuration.MonitorIndex = model.MonitorIndex.Value;
        if (model.SpaceX.HasValue) configuration.SpaceX = model.SpaceX.Value;
        if (model.SpaceY.HasValue) configuration.SpaceY = model.SpaceY.Value;
        if (model.UseScreenCoordinates.HasValue) configuration.UseScreenCoordinates = model.UseScreenCoordinates.Value;
        if (model.ForceWallpaperRefresh.HasValue) configuration.ForceWallpaperRefresh = model.ForceWallpaperRefresh.Value;
        if (model.PreserveWallpaperSlideshow.HasValue) configuration.PreserveWallpaperSlideshow = model.PreserveWallpaperSlideshow.Value;
        if (model.ApplyToAllUsers.HasValue) configuration.ApplyToAllUsers = model.ApplyToAllUsers.Value;
        if (model.IncludeDefaultUserProfile.HasValue) configuration.IncludeDefaultUserProfile = model.IncludeDefaultUserProfile.Value;

        ApplyEnum<DesktopManager.DesktopWallpaperPosition>(model.WallpaperFit, value => configuration.WallpaperFit = value);
        ApplyEnum<BgInfoTextPosition>(model.TextPosition, value => configuration.TextPosition = value);
        ApplyEnum<BgInfoTarget>(model.Target, value => configuration.Target = value);
        ApplyEnum<BgInfoChartLayoutMode>(model.ChartLayout, value => configuration.ChartLayout = value);
        ApplyEnum<BgInfoTextPosition>(model.ChartStackAnchor, value => configuration.ChartStackAnchor = value);
        ApplyEnum<BgInfoChartStackDirection>(model.ChartStackDirection, value => configuration.ChartStackDirection = value);
        if (model.ChartStackSpacing.HasValue) configuration.ChartStackSpacing = model.ChartStackSpacing.Value;
        if (model.ChartStackOffsetX.HasValue) configuration.ChartStackOffsetX = model.ChartStackOffsetX.Value;
        if (model.ChartStackOffsetY.HasValue) configuration.ChartStackOffsetY = model.ChartStackOffsetY.Value;
        if (model.ChartStackAlignToTextBlock.HasValue) configuration.ChartStackAlignToTextBlock = model.ChartStackAlignToTextBlock.Value;
        if (model.ChartStackOutsideTextBlock.HasValue) configuration.ChartStackOutsideTextBlock = model.ChartStackOutsideTextBlock.Value;

        if (model.Entries != null) {
            foreach (var entryModel in model.Entries) {
                var entry = MapEntry(entryModel);
                if (entry != null) {
                    configuration.Entries.Add(entry);
                }
            }
        }

        if (model.Variables != null) {
            foreach (var variableModel in model.Variables) {
                var variable = MapVariable(variableModel);
                if (variable != null) {
                    configuration.Variables.Add(variable);
                }
            }
        }

        if (model.Charts != null) {
            foreach (var chartModel in model.Charts) {
                var chart = MapChart(chartModel);
                if (chart != null) {
                    configuration.Charts.Add(chart);
                }
            }
        }

        if (model.Topologies != null) {
            foreach (var topologyModel in model.Topologies) {
                var topology = MapTopology(topologyModel);
                if (topology != null) {
                    configuration.Topologies.Add(topology);
                }
            }
        }

        if (model.VisualCanvases != null) {
            foreach (var visualModel in model.VisualCanvases) {
                var visual = MapVisualCanvas(visualModel, baseDirectory);
                if (visual != null) {
                    configuration.VisualCanvases.Add(visual);
                }
            }
        }

        if (model.Images != null) {
            foreach (var imageModel in model.Images) {
                var image = MapImage(imageModel, baseDirectory);
                if (image != null) {
                    configuration.Images.Add(image);
                }
            }
        }

        configuration.Background = MapBackground(model.Background, baseDirectory);
        MapExtensions(model, configuration);
        return configuration;
    }

    private static BgInfoConfigurationFile MapFromConfiguration(BgInfoConfiguration configuration) {
        var model = new BgInfoConfigurationFile {
            FilePath = configuration.FilePath,
            OutputFileName = configuration.OutputFileName,
            ConfigurationDirectory = configuration.ConfigurationDirectory,
            FontFamilyName = configuration.CustomFontFamilyName,
            Color = configuration.CustomColor.HasValue ? BgInfoColorParser.ToHex(configuration.CustomColor.Value) : null,
            FontSize = configuration.FontSize,
            Bold = configuration.Bold,
            FontWeight = configuration.FontWeight,
            Italic = configuration.Italic,
            Underline = configuration.Underline,
            UnderlineStyle = configuration.UnderlineStyle.ToString(),
            StrikethroughStyle = configuration.StrikethroughStyle.ToString(),
            Baseline = configuration.Baseline.ToString(),
            TextCase = configuration.TextCase.ToString(),
            ValueColor = configuration.CustomValueColor.HasValue ? BgInfoColorParser.ToHex(configuration.CustomValueColor.Value) : null,
            ValueFontSize = configuration.ValueFontSize,
            ValueFontFamilyName = configuration.CustomValueFontFamilyName,
            ValueBold = configuration.ValueBold,
            ValueFontWeight = configuration.ValueFontWeight,
            ValueItalic = configuration.ValueItalic,
            ValueUnderline = configuration.ValueUnderline,
            ValueUnderlineStyle = configuration.ValueUnderlineStyle.ToString(),
            ValueStrikethroughStyle = configuration.ValueStrikethroughStyle.ToString(),
            ValueBaseline = configuration.ValueBaseline.ToString(),
            ValueTextCase = configuration.ValueTextCase.ToString(),
            ValueWrapWidth = configuration.ValueWrapWidth,
            BackgroundColor = configuration.BackgroundColor.HasValue ? BgInfoColorParser.ToHex(configuration.BackgroundColor.Value) : null,
            SpaceBetweenLines = configuration.SpaceBetweenLines,
            SpaceBetweenColumns = configuration.SpaceBetweenColumns,
            PositionX = configuration.PositionX,
            PositionY = configuration.PositionY,
            MonitorIndex = configuration.MonitorIndex,
            SpaceX = configuration.SpaceX,
            SpaceY = configuration.SpaceY,
            WallpaperFit = configuration.HasExplicitWallpaperFit ? configuration.WallpaperFit.ToString() : null,
            TextPosition = configuration.TextPosition.ToString(),
            Target = configuration.Target.ToString(),
            ForceWallpaperRefresh = configuration.ForceWallpaperRefresh,
            PreserveWallpaperSlideshow = configuration.PreserveWallpaperSlideshow,
            ApplyToAllUsers = configuration.ApplyToAllUsers,
            IncludeDefaultUserProfile = configuration.IncludeDefaultUserProfile,
            UseScreenCoordinates = configuration.UseScreenCoordinates,
            ChartLayout = configuration.ChartLayout.ToString(),
            ChartStackAnchor = configuration.ChartStackAnchor.ToString(),
            ChartStackDirection = configuration.ChartStackDirection.ToString(),
            ChartStackSpacing = configuration.ChartStackSpacing,
            ChartStackOffsetX = configuration.ChartStackOffsetX,
            ChartStackOffsetY = configuration.ChartStackOffsetY,
            ChartStackAlignToTextBlock = configuration.ChartStackAlignToTextBlock,
            ChartStackOutsideTextBlock = configuration.ChartStackOutsideTextBlock
        };

        if (configuration.Entries.Count > 0) {
            model.Entries = new List<BgInfoEntryFile>();
            foreach (var entry in configuration.Entries) {
                model.Entries.Add(new BgInfoEntryFile {
                    Type = entry.Type.ToString(),
                    Name = entry.Name,
                    Value = string.IsNullOrWhiteSpace(entry.BuiltinValue) ? entry.Value : null,
                    BuiltinValue = entry.BuiltinValue,
                    ForEach = entry.ForEach,
                    Color = entry.Color.HasValue ? BgInfoColorParser.ToHex(entry.Color.Value) : null,
                    FontSize = entry.FontSize,
                    FontFamilyName = entry.FontFamilyName,
                    Bold = entry.Bold,
                    FontWeight = entry.FontWeight,
                    Italic = entry.Italic,
                    Underline = entry.Underline,
                    UnderlineStyle = entry.UnderlineStyle?.ToString(),
                    StrikethroughStyle = entry.StrikethroughStyle?.ToString(),
                    Baseline = entry.Baseline?.ToString(),
                    TextCase = entry.TextCase?.ToString(),
                    ValueColor = entry.ValueColor.HasValue ? BgInfoColorParser.ToHex(entry.ValueColor.Value) : null,
                    ValueFontSize = entry.ValueFontSize,
                    ValueFontFamilyName = entry.ValueFontFamilyName,
                    ValueBold = entry.ValueBold,
                    ValueFontWeight = entry.ValueFontWeight,
                    ValueItalic = entry.ValueItalic,
                    ValueUnderline = entry.ValueUnderline,
                    ValueUnderlineStyle = entry.ValueUnderlineStyle?.ToString(),
                    ValueStrikethroughStyle = entry.ValueStrikethroughStyle?.ToString(),
                    ValueBaseline = entry.ValueBaseline?.ToString(),
                    ValueTextCase = entry.ValueTextCase?.ToString()
                });
            }
        }

        if (configuration.Variables.Count > 0) {
            model.Variables = new List<BgInfoVariableFile>();
            foreach (var variable in configuration.Variables) {
                model.Variables.Add(new BgInfoVariableFile {
                    Name = variable.Name,
                    Provider = variable.Provider.ToString(),
                    Argument = variable.Argument
                });
            }
        }

        if (configuration.Charts.Count > 0) {
            model.Charts = new List<BgInfoChartFile>();
            foreach (var chart in configuration.Charts) {
                model.Charts.Add(new BgInfoChartFile {
                    Id = chart.Id,
                    Title = chart.Title,
                    Kind = chart.Kind.ToString(),
                    ThemeMode = chart.ThemeMode.ToString(),
                    Width = chart.Width,
                    Height = chart.Height,
                    Anchor = chart.Anchor.ToString(),
                    OffsetX = chart.OffsetX,
                    OffsetY = chart.OffsetY,
                    PositionX = chart.PositionX,
                    PositionY = chart.PositionY,
                    Values = chart.Values is null ? null : new List<double>(chart.Values).ToArray(),
                    Labels = chart.Labels is null ? null : new List<string>(chart.Labels).ToArray(),
                    Target = chart.Target,
                    RangeEnds = chart.RangeEnds is null ? null : new List<double>(chart.RangeEnds).ToArray(),
                    MaxPoints = chart.MaxPoints,
                    UseHistory = chart.UseHistory,
                    AppendValues = chart.AppendValues,
                    BackgroundColor = chart.BackgroundColor.HasValue ? BgInfoColorParser.ToHex(chart.BackgroundColor.Value) : null,
                    CornerRadius = chart.CornerRadius > 0 ? chart.CornerRadius : null,
                    BorderColor = chart.BorderColor.HasValue ? BgInfoColorParser.ToHex(chart.BorderColor.Value) : null,
                    LineColor = chart.LineColor.HasValue ? BgInfoColorParser.ToHex(chart.LineColor.Value) : null,
                    FillColor = chart.FillColor.HasValue ? BgInfoColorParser.ToHex(chart.FillColor.Value) : null,
                    Palette = chart.Palette is null ? null : chart.Palette.Select(BgInfoColorParser.ToHex).ToArray(),
                    TextColor = chart.TextColor.HasValue ? BgInfoColorParser.ToHex(chart.TextColor.Value) : null,
                    TitleColor = chart.TitleColor.HasValue ? BgInfoColorParser.ToHex(chart.TitleColor.Value) : null,
                    ValueColor = chart.ValueColor.HasValue ? BgInfoColorParser.ToHex(chart.ValueColor.Value) : null,
                    FontFamilyName = chart.FontFamilyName,
                    TitleFontSize = chart.TitleFontSize,
                    ValueFontSize = chart.ValueFontSize,
                    TitleBold = chart.TitleBold,
                    TitleFontWeight = chart.TitleFontWeight,
                    TitleItalic = chart.TitleItalic,
                    TitleUnderline = chart.TitleUnderline,
                    TitleUnderlineStyle = chart.TitleUnderlineStyle?.ToString(),
                    TitleStrikethroughStyle = chart.TitleStrikethroughStyle?.ToString(),
                    TitleBaseline = chart.TitleBaseline?.ToString(),
                    TitleTextCase = chart.TitleTextCase?.ToString(),
                    ValueBold = chart.ValueBold,
                    ValueFontWeight = chart.ValueFontWeight,
                    ValueItalic = chart.ValueItalic,
                    ValueUnderline = chart.ValueUnderline,
                    ValueUnderlineStyle = chart.ValueUnderlineStyle?.ToString(),
                    ValueStrikethroughStyle = chart.ValueStrikethroughStyle?.ToString(),
                    ValueBaseline = chart.ValueBaseline?.ToString(),
                    ValueTextCase = chart.ValueTextCase?.ToString(),
                    ShowLatestValue = chart.ShowLatestValue,
                    ValueFormat = chart.ValueFormat,
                    ValueSuffix = chart.ValueSuffix,
                    BarGap = chart.BarGap,
                    Padding = chart.Padding,
                    ShowGrid = chart.ShowGrid,
                    GridColor = chart.GridColor.HasValue ? BgInfoColorParser.ToHex(chart.GridColor.Value) : null,
                    GridLineCount = chart.GridLineCount,
                    ShowLegend = chart.ShowLegend,
                    ShowPointLegend = chart.ShowPointLegend,
                    LegendPosition = chart.LegendPosition.ToString(),
                    ShowDataLabels = chart.ShowDataLabels,
                    Minimum = chart.Minimum,
                    Maximum = chart.Maximum,
                    ShowDonutCenterLabel = chart.ShowDonutCenterLabel,
                    DonutInnerRadiusRatio = chart.DonutInnerRadiusRatio,
                    DonutCenterValue = chart.DonutCenterValue,
                    DonutCenterLabel = chart.DonutCenterLabel,
                    ShowRadialBarCenterLabel = chart.ShowRadialBarCenterLabel,
                    ShowCircleStatusLabel = chart.ShowCircleStatusLabel,
                    ShowProgressValues = chart.ShowProgressValues,
                    ShowProgressHandles = chart.ShowProgressHandles,
                    ProgressBarThicknessRatio = chart.ProgressBarThicknessRatio,
                    PictorialSymbol = chart.PictorialSymbol.ToString(),
                    PictorialColumns = chart.PictorialColumns,
                    Metric = chart.Metric.ToString(),
                    MetricArgument = chart.MetricArgument
                });
            }
        }

        if (configuration.Topologies.Count > 0) {
            model.Topologies = new List<BgInfoTopologyFile>();
            foreach (var topology in configuration.Topologies) {
                model.Topologies.Add(new BgInfoTopologyFile {
                    Title = topology.Title,
                    Subtitle = topology.Subtitle,
                    Width = topology.Width,
                    Height = topology.Height,
                    Anchor = topology.Anchor.ToString(),
                    OffsetX = topology.OffsetX,
                    OffsetY = topology.OffsetY,
                    PositionX = topology.PositionX,
                    PositionY = topology.PositionY,
                    Layout = topology.Layout.ToString(),
                    Direction = topology.Direction.ToString(),
                    NodeDisplayMode = topology.NodeDisplayMode.ToString(),
                    VisualStyle = topology.VisualStyle.ToString(),
                    CanvasSurfaceStyle = topology.CanvasSurfaceStyle.ToString(),
                    Theme = topology.Theme,
                    Accent = topology.Accent.HasValue ? BgInfoColorParser.ToHex(topology.Accent.Value) : null,
                    FontFamilyName = topology.FontFamilyName,
                    Transparent = topology.Transparent,
                    ShowTitle = topology.ShowTitle,
                    ShowLegend = topology.ShowLegend,
                    ShowGroups = topology.ShowGroups,
                    ShowEdgeLabels = topology.ShowEdgeLabels,
                    ShowStatusBadges = topology.ShowStatusBadges,
                    FitContentToViewport = topology.FitContentToViewport,
                    Scale = topology.Scale == 1 ? null : topology.Scale,
                    BackgroundColor = topology.BackgroundColor.HasValue ? BgInfoColorParser.ToHex(topology.BackgroundColor.Value) : null,
                    BorderColor = topology.BorderColor.HasValue ? BgInfoColorParser.ToHex(topology.BorderColor.Value) : null,
                    CornerRadius = topology.CornerRadius > 0 ? topology.CornerRadius : null,
                    Padding = topology.Padding == 20 ? null : topology.Padding,
                    TitleColor = topology.TitleColor.HasValue ? BgInfoColorParser.ToHex(topology.TitleColor.Value) : null,
                    TitleFontSize = topology.TitleFontSize,
                    TitleFontWeight = topology.TitleFontWeight,
                    TitleTextCase = topology.TitleTextCase?.ToString(),
                    Groups = topology.Groups.Select(MapGroupFile).ToList(),
                    Nodes = topology.Nodes.Select(MapNodeFile).ToList(),
                    Edges = topology.Edges.Select(MapEdgeFile).ToList()
                });
            }
        }

        if (configuration.VisualCanvases.Count > 0) {
            model.VisualCanvases = new List<BgInfoVisualCanvasFile>();
            foreach (var visual in configuration.VisualCanvases) {
                if (visual == null) {
                    continue;
                }
                model.VisualCanvases.Add(new BgInfoVisualCanvasFile {
                    Template = visual.Template.ToString(),
                    ThemeMode = visual.ThemeMode.ToString(),
                    LayoutPreset = visual.LayoutPreset == BgInfoVisualCanvasLayoutPreset.Default ? null : visual.LayoutPreset.ToString(),
                    Title = visual.Title,
                    Subtitle = visual.Subtitle,
                    Width = visual.Width,
                    Height = visual.Height,
                    PositionX = visual.PositionX,
                    PositionY = visual.PositionY,
                    BackgroundTop = visual.CustomBackgroundTop.HasValue ? BgInfoColorParser.ToHex(visual.CustomBackgroundTop.Value) : null,
                    BackgroundBottom = visual.CustomBackgroundBottom.HasValue ? BgInfoColorParser.ToHex(visual.CustomBackgroundBottom.Value) : null,
                    Accent = visual.CustomAccent.HasValue ? BgInfoColorParser.ToHex(visual.CustomAccent.Value) : null,
                    SecondaryAccent = visual.SecondaryAccent.HasValue ? BgInfoColorParser.ToHex(visual.SecondaryAccent.Value) : null,
                    StatusOk = visual.StatusColors == null ? null : BgInfoColorParser.ToHex(visual.StatusColors.Ok),
                    StatusWarning = visual.StatusColors == null ? null : BgInfoColorParser.ToHex(visual.StatusColors.Warning),
                    StatusCritical = visual.StatusColors == null ? null : BgInfoColorParser.ToHex(visual.StatusColors.Critical),
                    StatusInfo = visual.StatusColors == null ? null : BgInfoColorParser.ToHex(visual.StatusColors.Info),
                    TitleColor = visual.TitleColor.HasValue ? BgInfoColorParser.ToHex(visual.TitleColor.Value) : null,
                    TitleAccentColor = visual.TitleAccentColor.HasValue ? BgInfoColorParser.ToHex(visual.TitleAccentColor.Value) : null,
                    SubtitleColor = visual.SubtitleColor.HasValue ? BgInfoColorParser.ToHex(visual.SubtitleColor.Value) : null,
                    TileGlassTop = visual.TileGlassTop.HasValue ? BgInfoColorParser.ToHex(visual.TileGlassTop.Value) : null,
                    TileGlassBottom = visual.TileGlassBottom.HasValue ? BgInfoColorParser.ToHex(visual.TileGlassBottom.Value) : null,
                    TileLabelColor = visual.TileLabelColor.HasValue ? BgInfoColorParser.ToHex(visual.TileLabelColor.Value) : null,
                    TileValueColor = visual.TileValueColor.HasValue ? BgInfoColorParser.ToHex(visual.TileValueColor.Value) : null,
                    TileDetailColor = visual.TileDetailColor.HasValue ? BgInfoColorParser.ToHex(visual.TileDetailColor.Value) : null,
                    TileProgressTrackColor = visual.TileProgressTrackColor.HasValue ? BgInfoColorParser.ToHex(visual.TileProgressTrackColor.Value) : null,
                    HeroBadgeTop = visual.HeroBadgeTop.HasValue ? BgInfoColorParser.ToHex(visual.HeroBadgeTop.Value) : null,
                    HeroBadgeBottom = visual.HeroBadgeBottom.HasValue ? BgInfoColorParser.ToHex(visual.HeroBadgeBottom.Value) : null,
                    HeroBadgeTextColor = visual.HeroBadgeTextColor.HasValue ? BgInfoColorParser.ToHex(visual.HeroBadgeTextColor.Value) : null,
                    HeroBadgeVisible = visual.HeroBadgeVisible,
                    HeroContentVisible = visual.HeroContentVisible ? null : false,
                    HeroBadgeText = string.Equals(visual.HeroBadgeText, ">_", StringComparison.Ordinal) ? null : visual.HeroBadgeText,
                    HeroBadgeImagePath = string.IsNullOrWhiteSpace(visual.HeroBadgeImagePath) ? null : visual.HeroBadgeImagePath,
                    HeroBadgeImageFit = visual.HeroBadgeImageFit == BgInfoImageFit.Contain ? null : visual.HeroBadgeImageFit.ToString(),
                    HeroBadgeImagePadding = visual.HeroBadgeImagePadding == 10 ? null : visual.HeroBadgeImagePadding,
                    HeroBadgeImageOpacity = visual.HeroBadgeImageOpacity == 1d ? null : ValidateImageOpacity(visual.HeroBadgeImageOpacity),
                    FeatureAnchor = visual.FeatureAnchor?.ToString(),
                    FeatureWidth = visual.FeatureWidth == 0 ? null : visual.FeatureWidth,
                    FeatureHeight = visual.FeatureHeight == 0 ? null : visual.FeatureHeight,
                    TileWidth = visual.TileWidth == 0 ? null : visual.TileWidth,
                    TileHeight = visual.TileHeight == 0 ? null : visual.TileHeight,
                    TileGap = visual.TileGap == 0 ? null : visual.TileGap,
                    LeftTileWidth = visual.LeftTileWidth == 0 ? null : visual.LeftTileWidth,
                    RightTileWidth = visual.RightTileWidth == 0 ? null : visual.RightTileWidth,
                    CenterTileWidth = visual.CenterTileWidth == 0 ? null : visual.CenterTileWidth,
                    LeftTileOffsetX = visual.LeftTileOffsetX == 0 ? null : visual.LeftTileOffsetX,
                    LeftTileOffsetY = visual.LeftTileOffsetY == 0 ? null : visual.LeftTileOffsetY,
                    RightTileOffsetX = visual.RightTileOffsetX == 0 ? null : visual.RightTileOffsetX,
                    RightTileOffsetY = visual.RightTileOffsetY == 0 ? null : visual.RightTileOffsetY,
                    CenterTileOffsetX = visual.CenterTileOffsetX == 0 ? null : visual.CenterTileOffsetX,
                    CenterTileOffsetY = visual.CenterTileOffsetY == 0 ? null : visual.CenterTileOffsetY,
                    TileTextFitPolicy = visual.TileTextFitPolicy == BgInfoVisualCanvasTileTextFitPolicy.Auto ? null : visual.TileTextFitPolicy.ToString(),
                    TileTextScale = visual.TileTextScale == 1 ? null : ValidateTileTextScale(visual.TileTextScale),
                    FeatureOffsetX = visual.FeatureOffsetX == 0 ? null : visual.FeatureOffsetX,
                    FeatureOffsetY = visual.FeatureOffsetY == 0 ? null : visual.FeatureOffsetY,
                    Transparent = visual.Transparent,
                    TechBackdrop = visual.TechBackdrop,
                    Tiles = visual.Tiles.Where(tile => tile != null).Select(MapVisualCanvasTileFile).ToList(),
                    Features = visual.Features.Where(feature => feature != null).Select(MapVisualCanvasFeatureFile).ToList()
                });
            }
        }

        if (configuration.Images.Count > 0) {
            model.Images = new List<BgInfoImageFile>();
            foreach (var image in configuration.Images) {
                if (image == null) {
                    continue;
                }
                model.Images.Add(new BgInfoImageFile {
                    Path = image.Path,
                    Width = image.Width,
                    Height = image.Height,
                    Anchor = image.Anchor.ToString(),
                    OffsetX = image.OffsetX,
                    OffsetY = image.OffsetY,
                    PositionX = image.PositionX,
                    PositionY = image.PositionY,
                    Opacity = ValidateImageOpacity(image.Opacity),
                    Fit = image.Fit == BgInfoImageFit.Stretch ? null : image.Fit.ToString()
                });
            }
        }

        model.Background = MapBackgroundFile(configuration.Background);
        WriteExtensions(configuration, model);
        return model;
    }

    private static BgInfoEntry? MapEntry(BgInfoEntryFile model) {
        if (model == null) {
            return null;
        }

        var entry = new BgInfoEntry();
        entry.Type = !string.IsNullOrWhiteSpace(model.Value) || !string.IsNullOrWhiteSpace(model.BuiltinValue)
            ? BgInfoEntryType.Value : BgInfoEntryType.Label;
        ApplyEnum<BgInfoEntryType>(model.Type, value => entry.Type = value);

        entry.Name = model.Name ?? string.Empty;
        if (!string.IsNullOrWhiteSpace(model.BuiltinValue)) {
            entry.BuiltinValue = model.BuiltinValue;
        }
        if (!string.IsNullOrWhiteSpace(model.ForEach)) {
            entry.ForEach = model.ForEach;
        }

        if (!string.IsNullOrWhiteSpace(model.BuiltinValue) && string.IsNullOrWhiteSpace(model.Value)) {
            if (string.IsNullOrWhiteSpace(entry.Name)) {
                entry.Name = model.BuiltinValue!;
            }
        }

        entry.Value = model.Value;

        ApplyColor(model.Color, value => entry.Color = value);
        ApplyColor(model.ValueColor, value => entry.ValueColor = value);
        if (model.FontSize.HasValue) entry.FontSize = model.FontSize.Value;
        if (!string.IsNullOrWhiteSpace(model.FontFamilyName)) entry.FontFamilyName = model.FontFamilyName;
        if (model.Bold.HasValue) entry.Bold = model.Bold.Value;
        if (model.FontWeight.HasValue) entry.FontWeight = model.FontWeight.Value;
        if (model.Italic.HasValue) entry.Italic = model.Italic.Value;
        if (model.Underline.HasValue) entry.Underline = model.Underline.Value;
        ApplyEnum<TextDecorationStyle>(model.UnderlineStyle, value => entry.UnderlineStyle = value);
        ApplyEnum<TextDecorationStyle>(model.StrikethroughStyle, value => entry.StrikethroughStyle = value);
        ApplyEnum<TextBaseline>(model.Baseline, value => entry.Baseline = value);
        ApplyEnum<TextCaseTransform>(model.TextCase, value => entry.TextCase = value);
        if (model.ValueFontSize.HasValue) entry.ValueFontSize = model.ValueFontSize.Value;
        if (!string.IsNullOrWhiteSpace(model.ValueFontFamilyName)) entry.ValueFontFamilyName = model.ValueFontFamilyName;
        if (model.ValueBold.HasValue) entry.ValueBold = model.ValueBold.Value;
        if (model.ValueFontWeight.HasValue) entry.ValueFontWeight = model.ValueFontWeight.Value;
        if (model.ValueItalic.HasValue) entry.ValueItalic = model.ValueItalic.Value;
        if (model.ValueUnderline.HasValue) entry.ValueUnderline = model.ValueUnderline.Value;
        ApplyEnum<TextDecorationStyle>(model.ValueUnderlineStyle, value => entry.ValueUnderlineStyle = value);
        ApplyEnum<TextDecorationStyle>(model.ValueStrikethroughStyle, value => entry.ValueStrikethroughStyle = value);
        ApplyEnum<TextBaseline>(model.ValueBaseline, value => entry.ValueBaseline = value);
        ApplyEnum<TextCaseTransform>(model.ValueTextCase, value => entry.ValueTextCase = value);

        return entry;
    }

    private static BgInfoVariable? MapVariable(BgInfoVariableFile model) {
        if (model == null || string.IsNullOrWhiteSpace(model.Name) || string.IsNullOrWhiteSpace(model.Provider)) {
            return null;
        }

        if (!TryParseDefinedEnumName(model.Provider, out BgInfoVariableProvider provider)) {
            throw new InvalidDataException("Unknown variable provider '" + model.Provider + "'.");
        }

        return new BgInfoVariable {
            Name = model.Name!,
            Provider = provider,
            Argument = model.Argument
        };
    }

    private static BgInfoChart? MapChart(BgInfoChartFile model) {
        if (model == null) {
            return null;
        }

        var chart = new BgInfoChart {
            Id = model.Id ?? string.Empty,
            Title = model.Title ?? string.Empty
        };
        ApplyEnum<VisualThemeMode>(model.ThemeMode, value => chart.ThemeMode = value);

        if (!string.IsNullOrWhiteSpace(model.Kind)) {
            if (!TryParseDefinedEnumName(model.Kind, out BgInfoChartKind kind)) {
                throw new InvalidDataException("Unknown chart kind '" + model.Kind + "'.");
            }
            chart.Kind = kind;
        }
        ApplyEnum<BgInfoTextPosition>(model.Anchor, value => chart.Anchor = value);
        ApplyEnum<BgInfoChartMetric>(model.Metric, value => chart.Metric = value);

        if (model.Width.HasValue) chart.Width = ImportRasterDimension(model.Width.Value);
        if (model.Height.HasValue) chart.Height = ImportRasterDimension(model.Height.Value);
        if (model.OffsetX.HasValue) chart.OffsetX = model.OffsetX.Value;
        if (model.OffsetY.HasValue) chart.OffsetY = model.OffsetY.Value;

        if (model.PositionX.HasValue && model.PositionY.HasValue) {
            chart.PositionX = model.PositionX.Value;
            chart.PositionY = model.PositionY.Value;
        }

        if (model.Values != null) chart.Values = model.Values;
        if (model.Labels != null) chart.Labels = model.Labels;
        if (model.Target.HasValue) chart.Target = model.Target.Value;
        if (model.RangeEnds != null) chart.RangeEnds = model.RangeEnds;
        if (model.MaxPoints.HasValue) chart.MaxPoints = model.MaxPoints.Value;
        if (model.UseHistory.HasValue) chart.UseHistory = model.UseHistory.Value;
        if (model.AppendValues.HasValue) chart.AppendValues = model.AppendValues.Value;

        ApplyColor(model.BackgroundColor, value => chart.BackgroundColor = value);
        if (model.CornerRadius.HasValue) chart.CornerRadius = Math.Max(0, model.CornerRadius.Value);
        ApplyColor(model.BorderColor, value => chart.BorderColor = value);
        ApplyColor(model.LineColor, value => chart.LineColor = value);
        ApplyColor(model.FillColor, value => chart.FillColor = value);
        if (model.Palette != null) {
            var colors = new List<ChartForgeX.Primitives.ChartColor>();
            foreach (var item in model.Palette) {
                if (BgInfoColorParser.TryParse(item, out var color)) {
                    colors.Add(color);
                }
            }

            chart.Palette = colors;
        }
        ApplyColor(model.TextColor, value => chart.TextColor = value);
        ApplyColor(model.TitleColor, value => chart.TitleColor = value);
        ApplyColor(model.ValueColor, value => chart.ValueColor = value);

        if (!string.IsNullOrWhiteSpace(model.FontFamilyName)) chart.FontFamilyName = model.FontFamilyName;
        if (model.TitleFontSize.HasValue) chart.TitleFontSize = model.TitleFontSize.Value;
        if (model.ValueFontSize.HasValue) chart.ValueFontSize = model.ValueFontSize.Value;
        if (model.TitleBold.HasValue) chart.TitleBold = model.TitleBold.Value;
        if (model.TitleFontWeight.HasValue) chart.TitleFontWeight = model.TitleFontWeight.Value;
        if (model.TitleItalic.HasValue) chart.TitleItalic = model.TitleItalic.Value;
        if (model.TitleUnderline.HasValue) chart.TitleUnderline = model.TitleUnderline.Value;
        ApplyEnum<TextDecorationStyle>(model.TitleUnderlineStyle, value => chart.TitleUnderlineStyle = value);
        ApplyEnum<TextDecorationStyle>(model.TitleStrikethroughStyle, value => chart.TitleStrikethroughStyle = value);
        ApplyEnum<TextBaseline>(model.TitleBaseline, value => chart.TitleBaseline = value);
        ApplyEnum<TextCaseTransform>(model.TitleTextCase, value => chart.TitleTextCase = value);
        if (model.ValueBold.HasValue) chart.ValueBold = model.ValueBold.Value;
        if (model.ValueFontWeight.HasValue) chart.ValueFontWeight = model.ValueFontWeight.Value;
        if (model.ValueItalic.HasValue) chart.ValueItalic = model.ValueItalic.Value;
        if (model.ValueUnderline.HasValue) chart.ValueUnderline = model.ValueUnderline.Value;
        ApplyEnum<TextDecorationStyle>(model.ValueUnderlineStyle, value => chart.ValueUnderlineStyle = value);
        ApplyEnum<TextDecorationStyle>(model.ValueStrikethroughStyle, value => chart.ValueStrikethroughStyle = value);
        ApplyEnum<TextBaseline>(model.ValueBaseline, value => chart.ValueBaseline = value);
        ApplyEnum<TextCaseTransform>(model.ValueTextCase, value => chart.ValueTextCase = value);
        if (model.ShowLatestValue.HasValue) chart.ShowLatestValue = model.ShowLatestValue.Value;
        if (!string.IsNullOrWhiteSpace(model.ValueFormat)) chart.ValueFormat = model.ValueFormat!;
        if (!string.IsNullOrWhiteSpace(model.ValueSuffix)) chart.ValueSuffix = model.ValueSuffix!;
        if (model.BarGap.HasValue) chart.BarGap = model.BarGap.Value;
        if (model.Padding.HasValue) chart.Padding = model.Padding.Value;
        if (model.ShowGrid.HasValue) chart.ShowGrid = model.ShowGrid.Value;
        if (model.GridLineCount.HasValue) chart.GridLineCount = model.GridLineCount.Value;
        if (!string.IsNullOrWhiteSpace(model.GridColor)) {
            ApplyColor(model.GridColor, value => chart.GridColor = value);
        }
        if (model.ShowLegend.HasValue) chart.ShowLegend = model.ShowLegend.Value;
        if (model.ShowPointLegend.HasValue) chart.ShowPointLegend = model.ShowPointLegend.Value;
        ApplyEnum<BgInfoChartLegendPosition>(model.LegendPosition, value => chart.LegendPosition = value);
        if (model.ShowDataLabels.HasValue) chart.ShowDataLabels = model.ShowDataLabels.Value;
        if (model.Minimum.HasValue) chart.Minimum = model.Minimum.Value;
        if (model.Maximum.HasValue) chart.Maximum = model.Maximum.Value;
        if (model.ShowDonutCenterLabel.HasValue) chart.ShowDonutCenterLabel = model.ShowDonutCenterLabel.Value;
        if (model.DonutInnerRadiusRatio.HasValue) chart.DonutInnerRadiusRatio = model.DonutInnerRadiusRatio.Value;
        if (!string.IsNullOrWhiteSpace(model.DonutCenterValue)) chart.DonutCenterValue = model.DonutCenterValue;
        if (!string.IsNullOrWhiteSpace(model.DonutCenterLabel)) chart.DonutCenterLabel = model.DonutCenterLabel;
        if (model.ShowRadialBarCenterLabel.HasValue) chart.ShowRadialBarCenterLabel = model.ShowRadialBarCenterLabel.Value;
        if (model.ShowCircleStatusLabel.HasValue) chart.ShowCircleStatusLabel = model.ShowCircleStatusLabel.Value;
        if (model.ShowProgressValues.HasValue) chart.ShowProgressValues = model.ShowProgressValues.Value;
        if (model.ShowProgressHandles.HasValue) chart.ShowProgressHandles = model.ShowProgressHandles.Value;
        if (model.ProgressBarThicknessRatio.HasValue) chart.ProgressBarThicknessRatio = model.ProgressBarThicknessRatio.Value;
        ApplyEnum<BgInfoChartPictorialSymbol>(model.PictorialSymbol, value => chart.PictorialSymbol = value);
        if (model.PictorialColumns.HasValue) chart.PictorialColumns = model.PictorialColumns.Value;
        if (!string.IsNullOrWhiteSpace(model.MetricArgument)) chart.MetricArgument = model.MetricArgument;
        MapPlacement(model, chart);

        return chart;
    }

    private static BgInfoTopology? MapTopology(BgInfoTopologyFile model) {
        if (model == null) {
            return null;
        }

        var topology = new BgInfoTopology {
            Title = model.Title ?? string.Empty,
            Subtitle = model.Subtitle ?? string.Empty
        };

        if (model.Width.HasValue) {
            topology.Width = ImportRasterDimension(model.Width.Value);
        }
        if (model.Height.HasValue) {
            topology.Height = ImportRasterDimension(model.Height.Value);
        }
        ApplyEnum<BgInfoTextPosition>(model.Anchor, value => topology.Anchor = value);
        if (model.OffsetX.HasValue) {
            topology.OffsetX = model.OffsetX.Value;
        }
        if (model.OffsetY.HasValue) {
            topology.OffsetY = model.OffsetY.Value;
        }
        if (model.PositionX.HasValue && model.PositionY.HasValue) {
            topology.PositionX = model.PositionX.Value;
            topology.PositionY = model.PositionY.Value;
        }
        ApplyEnum<TopologyLayoutMode>(model.Layout, value => topology.Layout = value);
        ApplyEnum<TopologyLayoutDirection>(model.Direction, value => topology.Direction = value);
        ApplyEnum<TopologyNodeDisplayMode>(model.NodeDisplayMode, value => topology.NodeDisplayMode = value);
        ApplyEnum<TopologyVisualStyle>(model.VisualStyle, value => topology.VisualStyle = value);
        ApplyEnum<TopologyCanvasSurfaceStyle>(model.CanvasSurfaceStyle, value => topology.CanvasSurfaceStyle = value);
        ApplyEnum<VisualThemeMode>(model.Theme, value => topology.Theme = value.ToString());
        if (!string.IsNullOrWhiteSpace(model.Accent) && BgInfoColorParser.TryParse(model.Accent!, out var accent)) topology.Accent = accent;
        if (!string.IsNullOrWhiteSpace(model.FontFamilyName)) topology.FontFamilyName = model.FontFamilyName;
        if (model.Transparent.HasValue) {
            topology.Transparent = model.Transparent.Value;
        }
        if (model.ShowTitle.HasValue) {
            topology.ShowTitle = model.ShowTitle.Value;
        }
        if (model.ShowLegend.HasValue) {
            topology.ShowLegend = model.ShowLegend.Value;
        }
        if (model.ShowGroups.HasValue) {
            topology.ShowGroups = model.ShowGroups.Value;
        }
        if (model.ShowEdgeLabels.HasValue) {
            topology.ShowEdgeLabels = model.ShowEdgeLabels.Value;
        }
        if (model.ShowStatusBadges.HasValue) {
            topology.ShowStatusBadges = model.ShowStatusBadges.Value;
        }
        if (model.FitContentToViewport.HasValue) {
            topology.FitContentToViewport = model.FitContentToViewport.Value;
        }
        if (model.Scale.HasValue) {
            topology.Scale = BgInfoTopology.ValidateScale(model.Scale.Value);
        }
        ApplyColor(model.BackgroundColor, value => topology.BackgroundColor = value);
        ApplyColor(model.BorderColor, value => topology.BorderColor = value);
        if (model.CornerRadius.HasValue) topology.CornerRadius = Math.Max(0, model.CornerRadius.Value);
        if (model.Padding.HasValue) topology.Padding = Math.Max(0, model.Padding.Value);
        ApplyColor(model.TitleColor, value => topology.TitleColor = value);
        if (model.TitleFontSize.HasValue) topology.TitleFontSize = model.TitleFontSize.Value;
        if (model.TitleFontWeight.HasValue) topology.TitleFontWeight = model.TitleFontWeight.Value;
        ApplyEnum<TextCaseTransform>(model.TitleTextCase, value => topology.TitleTextCase = value);

        if (model.Groups != null) {
            foreach (var item in model.Groups) {
                topology.Groups.Add(MapGroup(item));
            }
        }
        if (model.Nodes != null) {
            foreach (var item in model.Nodes) {
                topology.Nodes.Add(MapNode(item));
            }
        }
        if (model.Edges != null) {
            foreach (var item in model.Edges) {
                topology.Edges.Add(MapEdge(item));
            }
        }
        topology.Region = ParseRegion(model.Region);
        topology.RegionOrder = model.RegionOrder ?? 0;

        return topology;
    }

    private static TopologyGroup MapGroup(BgInfoTopologyGroupFile model) {
        var group = new TopologyGroup {
            Id = model.Id ?? string.Empty,
            Label = model.Label ?? string.Empty,
            Width = model.Width ?? 0,
            Height = model.Height ?? 0
        };
        ApplyEnum<TopologyHealthStatus>(model.Status, value => group.Status = value);
        if (!string.IsNullOrWhiteSpace(model.Subtitle)) {
            group.Subtitle = model.Subtitle;
        }
        if (!string.IsNullOrWhiteSpace(model.Symbol)) {
            group.Symbol = model.Symbol;
        }
        if (!string.IsNullOrWhiteSpace(model.Color)) {
            group.Color = model.Color;
        }
        return group;
    }

    private static TopologyNode MapNode(BgInfoTopologyNodeFile model) {
        var node = new TopologyNode { Id = model.Id ?? string.Empty, Label = model.Label ?? string.Empty };
        ApplyEnum<TopologyNodeKind>(model.Kind, value => node.Kind = value);
        ApplyEnum<TopologyHealthStatus>(model.Status, value => node.Status = value);
        ApplyEnum<TopologyNodeDisplayMode>(model.DisplayMode, value => node.DisplayMode = value);
        if (!string.IsNullOrWhiteSpace(model.Subtitle)) {
            node.Subtitle = model.Subtitle;
        }
        if (!string.IsNullOrWhiteSpace(model.GroupId)) {
            node.GroupId = model.GroupId;
        }
        if (!string.IsNullOrWhiteSpace(model.Symbol)) {
            node.Symbol = model.Symbol;
        }
        if (!string.IsNullOrWhiteSpace(model.Badge)) {
            node.Badge = model.Badge;
        }
        if (!string.IsNullOrWhiteSpace(model.Color)) {
            node.Color = model.Color;
        }
        return node;
    }

    private static TopologyEdge MapEdge(BgInfoTopologyEdgeFile model) {
        var edge = new TopologyEdge {
            Id = model.Id ?? ((model.SourceNodeId ?? string.Empty) + "-" + (model.TargetNodeId ?? string.Empty)),
            SourceNodeId = model.SourceNodeId ?? string.Empty,
            TargetNodeId = model.TargetNodeId ?? string.Empty,
            IsMuted = model.Muted ?? false
        };
        ApplyEnum<TopologyEdgeKind>(model.Kind, value => edge.Kind = value);
        ApplyEnum<TopologyHealthStatus>(model.Status, value => edge.Status = value);
        ApplyEnum<VisualLinkDirection>(model.Direction, value => edge.Direction = value);
        ApplyEnum<TopologyEdgeRouting>(model.Routing, value => edge.Routing = value);
        if (!string.IsNullOrWhiteSpace(model.Label)) {
            edge.Label = model.Label;
        }
        if (!string.IsNullOrWhiteSpace(model.Color)) {
            edge.Color = model.Color;
        }
        return edge;
    }

    private static BgInfoTopologyGroupFile MapGroupFile(TopologyGroup group) => new() {
        Id = group.Id,
        Label = group.Label,
        Subtitle = group.Subtitle,
        Status = group.Status.ToString(),
        Width = group.Width,
        Height = group.Height,
        Symbol = group.Symbol,
        Color = group.Color
    };

    private static BgInfoTopologyNodeFile MapNodeFile(TopologyNode node) => new() {
        Id = node.Id,
        Label = node.Label,
        Subtitle = node.Subtitle,
        Kind = node.Kind.ToString(),
        Status = node.Status.ToString(),
        GroupId = node.GroupId,
        Symbol = node.Symbol,
        DisplayMode = node.DisplayMode?.ToString(),
        Badge = node.Badge,
        Color = node.Color
    };

    private static BgInfoTopologyEdgeFile MapEdgeFile(TopologyEdge edge) => new() {
        Id = edge.Id,
        SourceNodeId = edge.SourceNodeId,
        TargetNodeId = edge.TargetNodeId,
        Label = edge.Label,
        Kind = edge.Kind.ToString(),
        Status = edge.Status.ToString(),
        Direction = edge.Direction.ToString(),
        Routing = edge.Routing.ToString(),
        Color = edge.Color,
        Muted = edge.IsMuted
    };

    private static BgInfoVisualCanvas? MapVisualCanvas(BgInfoVisualCanvasFile model, string baseDirectory) {
        if (model == null) {
            return null;
        }

        var visual = new BgInfoVisualCanvas {
            Title = model.Title ?? "PowerBGInfo",
            Subtitle = model.Subtitle ?? "Desktop background insights for Windows and PowerShell"
        };
        ApplyEnum<VisualThemeMode>(model.ThemeMode, value => visual.ThemeMode = value);
        visual.StatusColors = MapStatusColors(model.StatusOk, model.StatusWarning, model.StatusCritical, model.StatusInfo, visual.ThemeMode);

        ApplyEnum<BgInfoVisualCanvasTemplate>(model.Template, value => visual.Template = value);
        ApplyEnum<BgInfoVisualCanvasLayoutPreset>(model.LayoutPreset, value => visual.LayoutPreset = value);
        if (model.Width.HasValue) visual.Width = ImportRasterDimension(model.Width.Value);
        if (model.Height.HasValue) visual.Height = ImportRasterDimension(model.Height.Value);
        if (model.PositionX.HasValue) visual.PositionX = model.PositionX.Value;
        if (model.PositionY.HasValue) visual.PositionY = model.PositionY.Value;
        if (model.Transparent.HasValue) visual.Transparent = model.Transparent.Value;
        if (model.TechBackdrop.HasValue) visual.TechBackdrop = model.TechBackdrop.Value;
        if (model.HeroBadgeVisible.HasValue) visual.HeroBadgeVisible = model.HeroBadgeVisible.Value;
        if (model.HeroContentVisible.HasValue) visual.HeroContentVisible = model.HeroContentVisible.Value;
        if (model.HeroBadgeText != null) visual.HeroBadgeText = model.HeroBadgeText;
        if (!string.IsNullOrWhiteSpace(model.HeroBadgeImagePath)) visual.HeroBadgeImagePath = ResolvePath(model.HeroBadgeImagePath, baseDirectory);
        ApplyEnum<BgInfoImageFit>(model.HeroBadgeImageFit, value => visual.HeroBadgeImageFit = value);
        if (model.HeroBadgeImagePadding.HasValue) visual.HeroBadgeImagePadding = ValidateImagePadding(model.HeroBadgeImagePadding.Value, "Hero badge image padding");
        if (model.HeroBadgeImageOpacity.HasValue) visual.HeroBadgeImageOpacity = ValidateImageOpacity(model.HeroBadgeImageOpacity.Value);
        ApplyEnum<BgInfoTextPosition>(model.FeatureAnchor, value => visual.FeatureAnchor = value);
        if (model.FeatureWidth.HasValue) visual.FeatureWidth = model.FeatureWidth.Value;
        if (model.FeatureHeight.HasValue) visual.FeatureHeight = model.FeatureHeight.Value;
        if (model.TileWidth.HasValue) visual.TileWidth = model.TileWidth.Value;
        if (model.TileHeight.HasValue) visual.TileHeight = model.TileHeight.Value;
        if (model.TileGap.HasValue) visual.TileGap = model.TileGap.Value;
        if (model.LeftTileWidth.HasValue) visual.LeftTileWidth = model.LeftTileWidth.Value;
        if (model.RightTileWidth.HasValue) visual.RightTileWidth = model.RightTileWidth.Value;
        if (model.CenterTileWidth.HasValue) visual.CenterTileWidth = model.CenterTileWidth.Value;
        if (model.LeftTileOffsetX.HasValue) visual.LeftTileOffsetX = model.LeftTileOffsetX.Value;
        if (model.LeftTileOffsetY.HasValue) visual.LeftTileOffsetY = model.LeftTileOffsetY.Value;
        if (model.RightTileOffsetX.HasValue) visual.RightTileOffsetX = model.RightTileOffsetX.Value;
        if (model.RightTileOffsetY.HasValue) visual.RightTileOffsetY = model.RightTileOffsetY.Value;
        if (model.CenterTileOffsetX.HasValue) visual.CenterTileOffsetX = model.CenterTileOffsetX.Value;
        if (model.CenterTileOffsetY.HasValue) visual.CenterTileOffsetY = model.CenterTileOffsetY.Value;
        ApplyEnum<BgInfoVisualCanvasTileTextFitPolicy>(model.TileTextFitPolicy, value => visual.TileTextFitPolicy = value);
        if (model.TileTextScale.HasValue) visual.TileTextScale = ValidateTileTextScale(model.TileTextScale.Value);
        if (model.FeatureOffsetX.HasValue) visual.FeatureOffsetX = model.FeatureOffsetX.Value;
        if (model.FeatureOffsetY.HasValue) visual.FeatureOffsetY = model.FeatureOffsetY.Value;
        ApplyColor(model.BackgroundTop, value => visual.BackgroundTop = value);
        ApplyColor(model.BackgroundBottom, value => visual.BackgroundBottom = value);
        ApplyColor(model.Accent, value => visual.Accent = value);
        ApplyColor(model.SecondaryAccent, value => visual.SecondaryAccent = value);
        ApplyColor(model.TitleColor, value => visual.TitleColor = value);
        ApplyColor(model.TitleAccentColor, value => visual.TitleAccentColor = value);
        ApplyColor(model.SubtitleColor, value => visual.SubtitleColor = value);
        ApplyColor(model.TileGlassTop, value => visual.TileGlassTop = value);
        ApplyColor(model.TileGlassBottom, value => visual.TileGlassBottom = value);
        ApplyColor(model.TileLabelColor, value => visual.TileLabelColor = value);
        ApplyColor(model.TileValueColor, value => visual.TileValueColor = value);
        ApplyColor(model.TileDetailColor, value => visual.TileDetailColor = value);
        ApplyColor(model.TileProgressTrackColor, value => visual.TileProgressTrackColor = value);
        ApplyColor(model.HeroBadgeTop, value => visual.HeroBadgeTop = value);
        ApplyColor(model.HeroBadgeBottom, value => visual.HeroBadgeBottom = value);
        ApplyColor(model.HeroBadgeTextColor, value => visual.HeroBadgeTextColor = value);

        if (model.Tiles != null) {
            foreach (var item in model.Tiles) {
                if (item != null) visual.Tiles.Add(MapVisualCanvasTile(item));
            }
        }
        if (model.Features != null) {
            foreach (var item in model.Features) {
                if (item != null) visual.Features.Add(MapVisualCanvasFeature(item));
            }
        }
        visual.Region = ParseRegion(model.Region);
        visual.RegionOrder = model.RegionOrder ?? 0;
        visual.RailMarginX = model.RailMarginX;
        visual.RailMarginY = model.RailMarginY;
        visual.FontFamilyName = model.FontFamilyName;

        return visual;
    }

    private static BgInfoVisualCanvasTile MapVisualCanvasTile(BgInfoVisualCanvasTileFile model) {
        var tile = new BgInfoVisualCanvasTile {
            Icon = model.Icon ?? string.Empty,
            Label = model.Label ?? string.Empty,
            Value = model.Value ?? string.Empty,
            Detail = model.Detail ?? string.Empty,
            Width = model.Width ?? 0,
            Height = model.Height ?? 0
        };
        var sideValue = model.Side;
        if (!string.IsNullOrWhiteSpace(sideValue)) {
            if (!TryParseDefinedEnumName(sideValue, out BgInfoVisualCanvasSide side)) {
                throw new InvalidDataException("Unknown visual canvas tile side '" + sideValue + "'. Expected Left, Center, or Right.");
            }
            tile.Side = side;
        }
        ApplyEnum<BgInfoVisualCanvasTileSurfaceStyle>(model.SurfaceStyle, value => tile.SurfaceStyle = value);
        ApplyEnum<BgInfoStatus>(model.Status, value => tile.Status = value);
        ApplyEnum<BgInfoVisualCanvasTileIconKind>(model.IconKind, value => tile.IconKind = value);
        ApplyEnum<BgInfoVisualCanvasTileMiniChartKind>(model.MiniChartKind, value => tile.MiniChartKind = value);
        ApplyEnum<BgInfoVisualCanvasTileTextFitPolicy>(model.TextFitPolicy, value => tile.TextFitPolicy = value);
        if (model.TextScale.HasValue) tile.TextScale = ValidateTileTextScale(model.TextScale.Value);
        if (model.LabelFontSize.HasValue) tile.LabelFontSize = ValidateTileFontSize(model.LabelFontSize.Value);
        if (model.ValueFontSize.HasValue) tile.ValueFontSize = ValidateTileFontSize(model.ValueFontSize.Value);
        if (model.DetailFontSize.HasValue) tile.DetailFontSize = ValidateTileFontSize(model.DetailFontSize.Value);
        ApplyColor(model.Accent, value => tile.Accent = value);
        if (model.Progress.HasValue) tile.Progress = model.Progress.Value;
        if (model.MiniChartValues != null) tile.MiniChartValues = model.MiniChartValues;
        if (model.MiniChartMaximum.HasValue) tile.MiniChartMaximum = model.MiniChartMaximum.Value;
        return tile;
    }

    private static bool TryParseDefinedEnumName<TEnum>(string? value, out TEnum parsed) where TEnum : struct, Enum {
        if (value == null) {
            parsed = default;
            return false;
        }

        var candidate = value.Trim();
        var names = Enum.GetNames(typeof(TEnum));
        var tokens = typeof(TEnum).IsDefined(typeof(FlagsAttribute), false)
            ? candidate.Split(',')
            : new[] { candidate };
        foreach (var token in tokens) {
            if (names.Any(name => string.Equals(token.Trim(), name, StringComparison.OrdinalIgnoreCase))) continue;
            parsed = default;
            return false;
        }

        return Enum.TryParse(candidate, true, out parsed);
    }

    private static BgInfoVisualCanvasFeature MapVisualCanvasFeature(BgInfoVisualCanvasFeatureFile model) => new() {
        Icon = model.Icon ?? string.Empty,
        Label = model.Label ?? string.Empty
    };

    private static BgInfoVisualCanvasTileFile MapVisualCanvasTileFile(BgInfoVisualCanvasTile tile) => new() {
        Side = tile.Side.ToString(),
        Icon = tile.Icon,
        Label = tile.Label,
        Value = tile.Value,
        Detail = tile.Detail,
        Width = tile.Width == 0 ? null : tile.Width,
        Height = tile.Height == 0 ? null : tile.Height,
        Accent = tile.Accent.HasValue ? BgInfoColorParser.ToHex(tile.Accent.Value) : null,
        Status = tile.Status == BgInfoStatus.None ? null : tile.Status.ToString(),
        Progress = tile.Progress,
        SurfaceStyle = tile.SurfaceStyle.ToString(),
        IconKind = tile.IconKind.ToString(),
        MiniChartKind = tile.MiniChartKind.ToString(),
        TextFitPolicy = tile.TextFitPolicy == BgInfoVisualCanvasTileTextFitPolicy.Auto ? null : tile.TextFitPolicy.ToString(),
        TextScale = tile.TextScale.HasValue ? ValidateTileTextScale(tile.TextScale.Value) : null,
        LabelFontSize = tile.LabelFontSize.HasValue ? ValidateTileFontSize(tile.LabelFontSize.Value) : null,
        ValueFontSize = tile.ValueFontSize.HasValue ? ValidateTileFontSize(tile.ValueFontSize.Value) : null,
        DetailFontSize = tile.DetailFontSize.HasValue ? ValidateTileFontSize(tile.DetailFontSize.Value) : null,
        MiniChartValues = tile.MiniChartValues is null ? null : new List<double>(tile.MiniChartValues).ToArray(),
        MiniChartMaximum = tile.MiniChartMaximum
    };

    private static BgInfoVisualCanvasFeatureFile MapVisualCanvasFeatureFile(BgInfoVisualCanvasFeature feature) => new() {
        Icon = feature.Icon,
        Label = feature.Label
    };

    private static BgInfoImage? MapImage(BgInfoImageFile model, string baseDirectory) {
        if (model == null) {
            return null;
        }

        var image = new BgInfoImage {
            Path = ResolvePath(model.Path, baseDirectory)
        };
        if (model.Width.HasValue) image.Width = ImportRasterDimension(model.Width.Value);
        if (model.Height.HasValue) image.Height = ImportRasterDimension(model.Height.Value);
        ApplyEnum<BgInfoTextPosition>(model.Anchor, value => image.Anchor = value);
        if (model.OffsetX.HasValue) image.OffsetX = model.OffsetX.Value;
        if (model.OffsetY.HasValue) image.OffsetY = model.OffsetY.Value;
        if (model.PositionX.HasValue) image.PositionX = model.PositionX.Value;
        if (model.PositionY.HasValue) image.PositionY = model.PositionY.Value;
        if (model.Opacity.HasValue) image.Opacity = ValidateImageOpacity(model.Opacity.Value);
        ApplyEnum<BgInfoImageFit>(model.Fit, value => image.Fit = value);
        return image;
    }

    private static int ValidateImagePadding(int padding, string name) {
        if (padding < 0) {
            throw new InvalidDataException(name + " cannot be negative.");
        }

        return padding;
    }

    private static double ValidateImageOpacity(double opacity) {
        if (double.IsNaN(opacity) || double.IsInfinity(opacity) || opacity < 0d || opacity > 1d) {
            throw new InvalidDataException("Image opacity must be a finite value between 0 and 1.");
        }

        return opacity;
    }

    private static double ValidateTileTextScale(double scale) {
        if (double.IsNaN(scale) || double.IsInfinity(scale) || scale < 0.5 || scale > 4)
            throw new InvalidDataException("Visual canvas tile text scale must be between 0.5 and 4.");
        return scale;
    }

    private static double ValidateTileFontSize(double size) {
        if (double.IsNaN(size) || double.IsInfinity(size) || size < 1 || size > 256)
            throw new InvalidDataException("Visual canvas tile font sizes must be between 1 and 256.");
        return size;
    }

    private static void ApplyColor(string? text, Action<ChartForgeX.Primitives.ChartColor> setter) {
        if (string.IsNullOrWhiteSpace(text)) {
            return;
        }

        if (BgInfoColorParser.TryParse(text!, out var color)) {
            setter(color);
        }
    }

    private static void ApplyEnum<TEnum>(string? text, Action<TEnum> setter) where TEnum : struct, Enum {
        if (string.IsNullOrWhiteSpace(text)) return;
        if (!TryParseDefinedEnumName(text, out TEnum parsed)) {
            throw new InvalidDataException("Unknown " + typeof(TEnum).Name + " value '" + text + "'.");
        }
        setter(parsed);
    }

    /// <summary>
    /// Resolves authored configuration paths using the host's filesystem syntax. Relative Windows
    /// separators are portable; foreign Windows roots are rejected on Unix rather than renamed.
    /// </summary>
    internal static string ResolvePath(string? path, string baseDirectory) {
        if (string.IsNullOrWhiteSpace(path)) {
            return string.Empty;
        }

        var safePath = path!;
        if (Path.IsPathRooted(safePath)) {
            return safePath;
        }
        if (!System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Windows)) {
            if ((safePath.Length > 1 && char.IsLetter(safePath[0]) && safePath[1] == ':') || safePath.StartsWith("\\", StringComparison.Ordinal)) {
                throw new ArgumentException("A Windows-rooted path cannot be resolved on this platform. Use a relative configuration path or a local absolute path.", nameof(path));
            }
            safePath = safePath.Replace('\\', Path.DirectorySeparatorChar);
        }

        if (string.IsNullOrWhiteSpace(baseDirectory)) {
            return safePath;
        }

        return Path.GetFullPath(Path.Combine(baseDirectory, safePath));
    }

    internal sealed class BgInfoConfigurationFile {
        public string? FilePath { get; set; }
        public string? OutputFileName { get; set; }
        public string? ConfigurationDirectory { get; set; }
        public string? FontFamilyName { get; set; }
        public string? Color { get; set; }
        public float? FontSize { get; set; }
        public bool? Bold { get; set; }
        public int? FontWeight { get; set; }
        public bool? Italic { get; set; }
        public bool? Underline { get; set; }
        public string? UnderlineStyle { get; set; }
        public string? StrikethroughStyle { get; set; }
        public string? Baseline { get; set; }
        public string? TextCase { get; set; }
        public string? ValueColor { get; set; }
        public float? ValueFontSize { get; set; }
        public string? ValueFontFamilyName { get; set; }
        public bool? ValueBold { get; set; }
        public int? ValueFontWeight { get; set; }
        public bool? ValueItalic { get; set; }
        public bool? ValueUnderline { get; set; }
        public string? ValueUnderlineStyle { get; set; }
        public string? ValueStrikethroughStyle { get; set; }
        public string? ValueBaseline { get; set; }
        public string? ValueTextCase { get; set; }
        public int? ValueWrapWidth { get; set; }
        public string? BackgroundColor { get; set; }
        public int? SpaceBetweenLines { get; set; }
        public int? SpaceBetweenColumns { get; set; }
        public float? PositionX { get; set; }
        public float? PositionY { get; set; }
        public int? MonitorIndex { get; set; }
        public int? SpaceX { get; set; }
        public int? SpaceY { get; set; }
        public string? WallpaperFit { get; set; }
        public string? TextPosition { get; set; }
        public string? Target { get; set; }
        public bool? ForceWallpaperRefresh { get; set; }
        public bool? PreserveWallpaperSlideshow { get; set; }
        public bool? ApplyToAllUsers { get; set; }
        public bool? IncludeDefaultUserProfile { get; set; }
        public bool? UseScreenCoordinates { get; set; }
        public string? ChartLayout { get; set; }
        public string? ChartStackAnchor { get; set; }
        public string? ChartStackDirection { get; set; }
        public int? ChartStackSpacing { get; set; }
        public int? ChartStackOffsetX { get; set; }
        public int? ChartStackOffsetY { get; set; }
        public bool? ChartStackAlignToTextBlock { get; set; }
        public bool? ChartStackOutsideTextBlock { get; set; }
        public List<BgInfoVariableFile>? Variables { get; set; }
        public List<BgInfoEntryFile>? Entries { get; set; }
        public List<BgInfoChartFile>? Charts { get; set; }
        public List<BgInfoTopologyFile>? Topologies { get; set; }
        public List<BgInfoVisualCanvasFile>? VisualCanvases { get; set; }
        public List<BgInfoImageFile>? Images { get; set; }
        public BgInfoBackgroundFile? Background { get; set; }
        public BgInfoLayoutFile? Layout { get; set; }
        public List<BgInfoWidgetFile>? Widgets { get; set; }
    }

    internal sealed class BgInfoVariableFile {
        public string? Name { get; set; }
        public string? Provider { get; set; }
        public string? Argument { get; set; }
    }

    internal sealed class BgInfoEntryFile {
        public string? Type { get; set; }
        public string? Name { get; set; }
        public string? Value { get; set; }
        public string? BuiltinValue { get; set; }
        public string? ForEach { get; set; }
        public string? Color { get; set; }
        public float? FontSize { get; set; }
        public string? FontFamilyName { get; set; }
        public bool? Bold { get; set; }
        public int? FontWeight { get; set; }
        public bool? Italic { get; set; }
        public bool? Underline { get; set; }
        public string? UnderlineStyle { get; set; }
        public string? StrikethroughStyle { get; set; }
        public string? Baseline { get; set; }
        public string? TextCase { get; set; }
        public string? ValueColor { get; set; }
        public float? ValueFontSize { get; set; }
        public string? ValueFontFamilyName { get; set; }
        public bool? ValueBold { get; set; }
        public int? ValueFontWeight { get; set; }
        public bool? ValueItalic { get; set; }
        public bool? ValueUnderline { get; set; }
        public string? ValueUnderlineStyle { get; set; }
        public string? ValueStrikethroughStyle { get; set; }
        public string? ValueBaseline { get; set; }
        public string? ValueTextCase { get; set; }
    }

    internal sealed class BgInfoChartFile {
        public string? Region { get; set; }
        public int? RegionOrder { get; set; }
        public BgInfoThresholdFile? Threshold { get; set; }
        public double? CornerRadius { get; set; }
        public string? BorderColor { get; set; }
        public string? ThemeMode { get; set; }
        public string? Id { get; set; }
        public string? Title { get; set; }
        public string? Kind { get; set; }
        public int? Width { get; set; }
        public int? Height { get; set; }
        public string? Anchor { get; set; }
        public int? OffsetX { get; set; }
        public int? OffsetY { get; set; }
        public float? PositionX { get; set; }
        public float? PositionY { get; set; }
        public double[]? Values { get; set; }
        public string[]? Labels { get; set; }
        public double? Target { get; set; }
        public double[]? RangeEnds { get; set; }
        public int? MaxPoints { get; set; }
        public bool? UseHistory { get; set; }
        public bool? AppendValues { get; set; }
        public string? BackgroundColor { get; set; }
        public string? LineColor { get; set; }
        public string? FillColor { get; set; }
        public string[]? Palette { get; set; }
        public string? TextColor { get; set; }
        public string? TitleColor { get; set; }
        public string? ValueColor { get; set; }
        public string? FontFamilyName { get; set; }
        public float? TitleFontSize { get; set; }
        public float? ValueFontSize { get; set; }
        public bool? TitleBold { get; set; }
        public int? TitleFontWeight { get; set; }
        public bool? TitleItalic { get; set; }
        public bool? TitleUnderline { get; set; }
        public string? TitleUnderlineStyle { get; set; }
        public string? TitleStrikethroughStyle { get; set; }
        public string? TitleBaseline { get; set; }
        public string? TitleTextCase { get; set; }
        public bool? ValueBold { get; set; }
        public int? ValueFontWeight { get; set; }
        public bool? ValueItalic { get; set; }
        public bool? ValueUnderline { get; set; }
        public string? ValueUnderlineStyle { get; set; }
        public string? ValueStrikethroughStyle { get; set; }
        public string? ValueBaseline { get; set; }
        public string? ValueTextCase { get; set; }
        public bool? ShowLatestValue { get; set; }
        public string? ValueFormat { get; set; }
        public string? ValueSuffix { get; set; }
        public float? BarGap { get; set; }
        public int? Padding { get; set; }
        public bool? ShowGrid { get; set; }
        public string? GridColor { get; set; }
        public int? GridLineCount { get; set; }
        public bool? ShowLegend { get; set; }
        public bool? ShowPointLegend { get; set; }
        public string? LegendPosition { get; set; }
        public bool? ShowDataLabels { get; set; }
        public double? Minimum { get; set; }
        public double? Maximum { get; set; }
        public bool? ShowDonutCenterLabel { get; set; }
        public double? DonutInnerRadiusRatio { get; set; }
        public string? DonutCenterValue { get; set; }
        public string? DonutCenterLabel { get; set; }
        public bool? ShowRadialBarCenterLabel { get; set; }
        public bool? ShowCircleStatusLabel { get; set; }
        public bool? ShowProgressValues { get; set; }
        public bool? ShowProgressHandles { get; set; }
        public double? ProgressBarThicknessRatio { get; set; }
        public string? PictorialSymbol { get; set; }
        public int? PictorialColumns { get; set; }
        public string? Metric { get; set; }
        public string? MetricArgument { get; set; }
    }

    internal sealed class BgInfoTopologyFile {
        public string? Region { get; set; }
        public int? RegionOrder { get; set; }
        public string? Title { get; set; }
        public string? Subtitle { get; set; }
        public int? Width { get; set; }
        public int? Height { get; set; }
        public string? Anchor { get; set; }
        public int? OffsetX { get; set; }
        public int? OffsetY { get; set; }
        public float? PositionX { get; set; }
        public float? PositionY { get; set; }
        public string? Layout { get; set; }
        public string? Direction { get; set; }
        public string? NodeDisplayMode { get; set; }
        public string? VisualStyle { get; set; }
        public string? CanvasSurfaceStyle { get; set; }
        public string? Theme { get; set; }
        public string? Accent { get; set; }
        public string? FontFamilyName { get; set; }
        public bool? Transparent { get; set; }
        public bool? ShowTitle { get; set; }
        public bool? ShowLegend { get; set; }
        public bool? ShowGroups { get; set; }
        public bool? ShowEdgeLabels { get; set; }
        public bool? ShowStatusBadges { get; set; }
        public bool? FitContentToViewport { get; set; }
        public int? Scale { get; set; }
        public string? BackgroundColor { get; set; }
        public string? BorderColor { get; set; }
        public double? CornerRadius { get; set; }
        public int? Padding { get; set; }
        public string? TitleColor { get; set; }
        public float? TitleFontSize { get; set; }
        public int? TitleFontWeight { get; set; }
        public string? TitleTextCase { get; set; }
        public List<BgInfoTopologyGroupFile>? Groups { get; set; }
        public List<BgInfoTopologyNodeFile>? Nodes { get; set; }
        public List<BgInfoTopologyEdgeFile>? Edges { get; set; }
    }

    internal sealed class BgInfoTopologyGroupFile {
        public string? Id { get; set; }
        public string? Label { get; set; }
        public string? Subtitle { get; set; }
        public string? Status { get; set; }
        public double? Width { get; set; }
        public double? Height { get; set; }
        public string? Symbol { get; set; }
        public string? Color { get; set; }
    }

    internal sealed class BgInfoTopologyNodeFile {
        public string? Id { get; set; }
        public string? Label { get; set; }
        public string? Subtitle { get; set; }
        public string? Kind { get; set; }
        public string? Status { get; set; }
        public string? GroupId { get; set; }
        public string? Symbol { get; set; }
        public string? DisplayMode { get; set; }
        public string? Badge { get; set; }
        public string? Color { get; set; }
    }

    internal sealed class BgInfoTopologyEdgeFile {
        public string? Id { get; set; }
        public string? SourceNodeId { get; set; }
        public string? TargetNodeId { get; set; }
        public string? Label { get; set; }
        public string? Kind { get; set; }
        public string? Status { get; set; }
        public string? Direction { get; set; }
        public string? Routing { get; set; }
        public string? Color { get; set; }
        public bool? Muted { get; set; }
    }

    internal sealed class BgInfoVisualCanvasFile {
        public string? StatusOk { get; set; }
        public string? StatusWarning { get; set; }
        public string? StatusCritical { get; set; }
        public string? StatusInfo { get; set; }
        public string? FontFamilyName { get; set; }
        public string? Region { get; set; }
        public int? RegionOrder { get; set; }
        public int? RailMarginX { get; set; }
        public int? RailMarginY { get; set; }
        public string? ThemeMode { get; set; }
        public string? Template { get; set; }
        public string? LayoutPreset { get; set; }
        public string? Title { get; set; }
        public string? Subtitle { get; set; }
        public int? Width { get; set; }
        public int? Height { get; set; }
        public int? PositionX { get; set; }
        public int? PositionY { get; set; }
        public string? BackgroundTop { get; set; }
        public string? BackgroundBottom { get; set; }
        public string? Accent { get; set; }
        public string? SecondaryAccent { get; set; }
        public string? TitleColor { get; set; }
        public string? TitleAccentColor { get; set; }
        public string? SubtitleColor { get; set; }
        public string? TileGlassTop { get; set; }
        public string? TileGlassBottom { get; set; }
        public string? TileLabelColor { get; set; }
        public string? TileValueColor { get; set; }
        public string? TileDetailColor { get; set; }
        public string? TileProgressTrackColor { get; set; }
        public string? HeroBadgeTop { get; set; }
        public string? HeroBadgeBottom { get; set; }
        public string? HeroBadgeTextColor { get; set; }
        public bool? HeroBadgeVisible { get; set; }
        public bool? HeroContentVisible { get; set; }
        public string? HeroBadgeText { get; set; }
        public string? HeroBadgeImagePath { get; set; }
        public string? HeroBadgeImageFit { get; set; }
        public int? HeroBadgeImagePadding { get; set; }
        public double? HeroBadgeImageOpacity { get; set; }
        public string? FeatureAnchor { get; set; }
        public int? FeatureWidth { get; set; }
        public int? FeatureHeight { get; set; }
        public int? TileWidth { get; set; }
        public int? TileHeight { get; set; }
        public int? TileGap { get; set; }
        public int? LeftTileWidth { get; set; }
        public int? RightTileWidth { get; set; }
        public int? CenterTileWidth { get; set; }
        public int? LeftTileOffsetX { get; set; }
        public int? LeftTileOffsetY { get; set; }
        public int? RightTileOffsetX { get; set; }
        public int? RightTileOffsetY { get; set; }
        public int? CenterTileOffsetX { get; set; }
        public int? CenterTileOffsetY { get; set; }
        public string? TileTextFitPolicy { get; set; }
        public double? TileTextScale { get; set; }
        public int? FeatureOffsetX { get; set; }
        public int? FeatureOffsetY { get; set; }
        public bool? Transparent { get; set; }
        public bool? TechBackdrop { get; set; }
        public List<BgInfoVisualCanvasTileFile>? Tiles { get; set; }
        public List<BgInfoVisualCanvasFeatureFile>? Features { get; set; }
    }

    internal sealed class BgInfoVisualCanvasTileFile {
        public string? Status { get; set; }
        public string? Side { get; set; }
        public string? Icon { get; set; }
        public string? Label { get; set; }
        public string? Value { get; set; }
        public string? Detail { get; set; }
        public int? Width { get; set; }
        public int? Height { get; set; }
        public string? Accent { get; set; }
        public double? Progress { get; set; }
        public string? SurfaceStyle { get; set; }
        public string? IconKind { get; set; }
        public string? MiniChartKind { get; set; }
        public string? TextFitPolicy { get; set; }
        public double? TextScale { get; set; }
        public double? LabelFontSize { get; set; }
        public double? ValueFontSize { get; set; }
        public double? DetailFontSize { get; set; }
        public double[]? MiniChartValues { get; set; }
        public double? MiniChartMaximum { get; set; }
    }

    internal sealed class BgInfoVisualCanvasFeatureFile {
        public string? Icon { get; set; }
        public string? Label { get; set; }
    }

    internal sealed class BgInfoImageFile {
        public string? Path { get; set; }
        public int? Width { get; set; }
        public int? Height { get; set; }
        public string? Anchor { get; set; }
        public int? OffsetX { get; set; }
        public int? OffsetY { get; set; }
        public int? PositionX { get; set; }
        public int? PositionY { get; set; }
        public double? Opacity { get; set; }
        public string? Fit { get; set; }
    }
}

#if !NET472
[JsonSourceGenerationOptions(
    PropertyNameCaseInsensitive = true,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    WriteIndented = true)]
[JsonSerializable(typeof(BgInfoConfigurationJson.BgInfoConfigurationFile))]
internal sealed partial class BgInfoConfigurationJsonSerializerContext : JsonSerializerContext {
}
#endif
