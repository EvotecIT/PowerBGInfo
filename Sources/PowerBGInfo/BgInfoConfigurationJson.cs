using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

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
public static class BgInfoConfigurationJson {
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
        var serializer = new JavaScriptSerializer();
        var model = serializer.Deserialize<BgInfoConfigurationFile>(json);
        return model ?? new BgInfoConfigurationFile();
#else
        var model = JsonSerializer.Deserialize(json, ReadContext.BgInfoConfigurationFile);
        return model ?? new BgInfoConfigurationFile();
#endif
    }

    private static string Serialize(BgInfoConfigurationFile model) {
#if NET472
        var serializer = new JavaScriptSerializer();
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
            configuration.OutputFileName = model.OutputFileName!;
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
        if (model.ValueFontSize.HasValue) configuration.ValueFontSize = model.ValueFontSize.Value;
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
        if (model.ApplyToAllUsers.HasValue) configuration.ApplyToAllUsers = model.ApplyToAllUsers.Value;
        if (model.IncludeDefaultUserProfile.HasValue) configuration.IncludeDefaultUserProfile = model.IncludeDefaultUserProfile.Value;

        if (!string.IsNullOrWhiteSpace(model.WallpaperFit) &&
            Enum.TryParse(model.WallpaperFit, true, out DesktopManager.DesktopWallpaperPosition fit)) {
            configuration.WallpaperFit = fit;
        }
        if (!string.IsNullOrWhiteSpace(model.TextPosition) &&
            Enum.TryParse(model.TextPosition, true, out BgInfoTextPosition position)) {
            configuration.TextPosition = position;
        }
        if (!string.IsNullOrWhiteSpace(model.Target) &&
            Enum.TryParse(model.Target, true, out BgInfoTarget target)) {
            configuration.Target = target;
        }
        if (!string.IsNullOrWhiteSpace(model.ChartLayout) &&
            Enum.TryParse(model.ChartLayout, true, out BgInfoChartLayoutMode chartLayout)) {
            configuration.ChartLayout = chartLayout;
        }
        if (!string.IsNullOrWhiteSpace(model.ChartStackAnchor) &&
            Enum.TryParse(model.ChartStackAnchor, true, out BgInfoTextPosition chartAnchor)) {
            configuration.ChartStackAnchor = chartAnchor;
        }
        if (!string.IsNullOrWhiteSpace(model.ChartStackDirection) &&
            Enum.TryParse(model.ChartStackDirection, true, out BgInfoChartStackDirection chartDirection)) {
            configuration.ChartStackDirection = chartDirection;
        }
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

        return configuration;
    }

    private static BgInfoConfigurationFile MapFromConfiguration(BgInfoConfiguration configuration) {
        var model = new BgInfoConfigurationFile {
            FilePath = configuration.FilePath,
            OutputFileName = configuration.OutputFileName,
            ConfigurationDirectory = configuration.ConfigurationDirectory,
            FontFamilyName = configuration.FontFamilyName,
            Color = BgInfoColorParser.ToHex(configuration.Color),
            FontSize = configuration.FontSize,
            ValueColor = BgInfoColorParser.ToHex(configuration.ValueColor),
            ValueFontSize = configuration.ValueFontSize,
            ValueFontFamilyName = configuration.ValueFontFamilyName,
            ValueWrapWidth = configuration.ValueWrapWidth,
            BackgroundColor = configuration.BackgroundColor.HasValue ? BgInfoColorParser.ToHex(configuration.BackgroundColor.Value) : null,
            SpaceBetweenLines = configuration.SpaceBetweenLines,
            SpaceBetweenColumns = configuration.SpaceBetweenColumns,
            PositionX = configuration.PositionX,
            PositionY = configuration.PositionY,
            MonitorIndex = configuration.MonitorIndex,
            SpaceX = configuration.SpaceX,
            SpaceY = configuration.SpaceY,
            WallpaperFit = configuration.WallpaperFit.ToString(),
            TextPosition = configuration.TextPosition.ToString(),
            Target = configuration.Target.ToString(),
            ForceWallpaperRefresh = configuration.ForceWallpaperRefresh,
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
                    ValueColor = entry.ValueColor.HasValue ? BgInfoColorParser.ToHex(entry.ValueColor.Value) : null,
                    ValueFontSize = entry.ValueFontSize,
                    ValueFontFamilyName = entry.ValueFontFamilyName
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
                    LineColor = chart.LineColor.HasValue ? BgInfoColorParser.ToHex(chart.LineColor.Value) : null,
                    FillColor = chart.FillColor.HasValue ? BgInfoColorParser.ToHex(chart.FillColor.Value) : null,
                    Palette = chart.Palette is null ? null : chart.Palette.Select(BgInfoColorParser.ToHex).ToArray(),
                    TextColor = chart.TextColor.HasValue ? BgInfoColorParser.ToHex(chart.TextColor.Value) : null,
                    FontFamilyName = chart.FontFamilyName,
                    TitleFontSize = chart.TitleFontSize,
                    ValueFontSize = chart.ValueFontSize,
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

        return model;
    }

    private static BgInfoEntry? MapEntry(BgInfoEntryFile model) {
        if (model == null) {
            return null;
        }

        var entry = new BgInfoEntry();
        if (!string.IsNullOrWhiteSpace(model.Type) &&
            Enum.TryParse(model.Type, true, out BgInfoEntryType entryType)) {
            entry.Type = entryType;
        } else if (!string.IsNullOrWhiteSpace(model.Value) || !string.IsNullOrWhiteSpace(model.BuiltinValue)) {
            entry.Type = BgInfoEntryType.Value;
        } else {
            entry.Type = BgInfoEntryType.Label;
        }

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
        if (model.ValueFontSize.HasValue) entry.ValueFontSize = model.ValueFontSize.Value;
        if (!string.IsNullOrWhiteSpace(model.ValueFontFamilyName)) entry.ValueFontFamilyName = model.ValueFontFamilyName;

        return entry;
    }

    private static BgInfoVariable? MapVariable(BgInfoVariableFile model) {
        if (model == null || string.IsNullOrWhiteSpace(model.Name) || string.IsNullOrWhiteSpace(model.Provider)) {
            return null;
        }

        if (!Enum.TryParse(model.Provider, true, out BgInfoVariableProvider provider)) {
            return null;
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

        if (!string.IsNullOrWhiteSpace(model.Kind) &&
            Enum.TryParse(model.Kind, true, out BgInfoChartKind kind)) {
            chart.Kind = kind;
        }
        if (!string.IsNullOrWhiteSpace(model.Anchor) &&
            Enum.TryParse(model.Anchor, true, out BgInfoTextPosition anchor)) {
            chart.Anchor = anchor;
        }
        if (!string.IsNullOrWhiteSpace(model.Metric) &&
            Enum.TryParse(model.Metric, true, out BgInfoChartMetric metric)) {
            chart.Metric = metric;
        }

        if (model.Width.HasValue) chart.Width = model.Width.Value;
        if (model.Height.HasValue) chart.Height = model.Height.Value;
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
        ApplyColor(model.LineColor, value => chart.LineColor = value);
        ApplyColor(model.FillColor, value => chart.FillColor = value);
        if (model.Palette != null) {
            var colors = new List<System.Drawing.Color>();
            foreach (var item in model.Palette) {
                if (BgInfoColorParser.TryParse(item, out var color)) {
                    colors.Add(color);
                }
            }

            chart.Palette = colors;
        }
        ApplyColor(model.TextColor, value => chart.TextColor = value);

        if (!string.IsNullOrWhiteSpace(model.FontFamilyName)) chart.FontFamilyName = model.FontFamilyName;
        if (model.TitleFontSize.HasValue) chart.TitleFontSize = model.TitleFontSize.Value;
        if (model.ValueFontSize.HasValue) chart.ValueFontSize = model.ValueFontSize.Value;
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
        if (!string.IsNullOrWhiteSpace(model.LegendPosition) &&
            Enum.TryParse(model.LegendPosition, true, out BgInfoChartLegendPosition legendPosition)) {
            chart.LegendPosition = legendPosition;
        }
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
        if (!string.IsNullOrWhiteSpace(model.PictorialSymbol) &&
            Enum.TryParse(model.PictorialSymbol, true, out BgInfoChartPictorialSymbol pictorialSymbol)) {
            chart.PictorialSymbol = pictorialSymbol;
        }
        if (model.PictorialColumns.HasValue) chart.PictorialColumns = model.PictorialColumns.Value;
        if (!string.IsNullOrWhiteSpace(model.MetricArgument)) chart.MetricArgument = model.MetricArgument;

        return chart;
    }

    private static void ApplyColor(string? text, Action<System.Drawing.Color> setter) {
        if (string.IsNullOrWhiteSpace(text)) {
            return;
        }

        if (BgInfoColorParser.TryParse(text!, out var color)) {
            setter(color);
        }
    }

    private static string ResolvePath(string? path, string baseDirectory) {
        if (string.IsNullOrWhiteSpace(path)) {
            return string.Empty;
        }

        var safePath = path!;
        if (Path.IsPathRooted(safePath)) {
            return safePath;
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
        public string? ValueColor { get; set; }
        public float? ValueFontSize { get; set; }
        public string? ValueFontFamilyName { get; set; }
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
        public string? ValueColor { get; set; }
        public float? ValueFontSize { get; set; }
        public string? ValueFontFamilyName { get; set; }
    }

    internal sealed class BgInfoChartFile {
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
        public string? FontFamilyName { get; set; }
        public float? TitleFontSize { get; set; }
        public float? ValueFontSize { get; set; }
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
