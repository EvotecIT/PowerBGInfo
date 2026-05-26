using System.IO;
using System.Drawing;
using ChartForgeX.Topology;
using Xunit;

namespace PowerBGInfo.Tests;

public class BgInfoConfigurationJsonTests
{
    [Fact]
    public void SaveAndLoadPreservesBuiltinValueEntries()
    {
        var tempDirectory = Path.Combine(Path.GetTempPath(), "bginfo-json-" + Path.GetRandomFileName());
        Directory.CreateDirectory(tempDirectory);
        var path = Path.Combine(tempDirectory, "config.json");

        var configuration = new BgInfoConfiguration
        {
            ConfigurationDirectory = tempDirectory
        };
        configuration.Entries.Add(new BgInfoEntry
        {
            Type = BgInfoEntryType.Value,
            Name = "Host",
            BuiltinValue = "HostName",
            Value = "stale-value"
        });

        BgInfoConfigurationJson.Save(configuration, path);

        var json = File.ReadAllText(path);
        Assert.Contains("\"BuiltinValue\"", json);
        Assert.DoesNotContain("stale-value", json);

        var roundTripped = BgInfoConfigurationJson.Load(path);
        var entry = Assert.Single(roundTripped.Entries);
        Assert.Equal("HostName", entry.BuiltinValue);
        Assert.Null(entry.Value);
    }

    [Fact]
    public void SaveAndLoadPreservesVariablesAndLoopEntries()
    {
        var tempDirectory = Path.Combine(Path.GetTempPath(), "bginfo-json-" + Path.GetRandomFileName());
        Directory.CreateDirectory(tempDirectory);
        var path = Path.Combine(tempDirectory, "config.json");

        var configuration = new BgInfoConfiguration {
            ConfigurationDirectory = tempDirectory
        };
        configuration.Variables.Add(new BgInfoVariable {
            Name = "Volumes",
            Provider = BgInfoVariableProvider.Volumes
        });
        configuration.Entries.Add(new BgInfoEntry {
            Type = BgInfoEntryType.Value,
            ForEach = "Volumes",
            Name = "Drive {{DriveLetter}}",
            Value = "{{SizeRemaining}}"
        });

        BgInfoConfigurationJson.Save(configuration, path);

        var roundTripped = BgInfoConfigurationJson.Load(path);
        var variable = Assert.Single(roundTripped.Variables);
        Assert.Equal("Volumes", variable.Name);
        Assert.Equal(BgInfoVariableProvider.Volumes, variable.Provider);

        var entry = Assert.Single(roundTripped.Entries);
        Assert.Equal("Volumes", entry.ForEach);
        Assert.Equal("Drive {{DriveLetter}}", entry.Name);
        Assert.Equal("{{SizeRemaining}}", entry.Value);
    }

    [Fact]
    public void LoadCanResolveRelativePathsAgainstAnOverrideDirectory()
    {
        var tempDirectory = Path.Combine(Path.GetTempPath(), "bginfo-json-" + Path.GetRandomFileName());
        var scriptDirectory = Path.Combine(tempDirectory, "scripts");
        Directory.CreateDirectory(scriptDirectory);

        var path = Path.Combine(tempDirectory, "config.json");
        File.WriteAllText(path, """
{
  "FilePath": "..\\Samples\\wallpaper.jpg",
  "ConfigurationDirectory": "..\\Output"
}
""");

        var configuration = BgInfoConfigurationJson.Load(path, scriptDirectory);

        Assert.Equal(Path.GetFullPath(Path.Combine(scriptDirectory, "..\\Samples\\wallpaper.jpg")), configuration.FilePath);
        Assert.Equal(Path.GetFullPath(Path.Combine(scriptDirectory, "..\\Output")), configuration.ConfigurationDirectory);
    }

    [Fact]
    public void SaveAndLoadPreservesWallpaperSlideshowPreference()
    {
        var tempDirectory = Path.Combine(Path.GetTempPath(), "bginfo-json-" + Path.GetRandomFileName());
        Directory.CreateDirectory(tempDirectory);
        var path = Path.Combine(tempDirectory, "config.json");

        var configuration = new BgInfoConfiguration {
            ConfigurationDirectory = tempDirectory,
            PreserveWallpaperSlideshow = false
        };

        BgInfoConfigurationJson.Save(configuration, path);

        var json = File.ReadAllText(path);
        Assert.Contains("\"PreserveWallpaperSlideshow\"", json);

        var roundTripped = BgInfoConfigurationJson.Load(path);
        Assert.False(roundTripped.PreserveWallpaperSlideshow);
    }

    [Fact]
    public void LoadUsesWallpaperSlideshowPreservationByDefault()
    {
        var tempDirectory = Path.Combine(Path.GetTempPath(), "bginfo-json-" + Path.GetRandomFileName());
        Directory.CreateDirectory(tempDirectory);
        var path = Path.Combine(tempDirectory, "config.json");
        File.WriteAllText(path, "{}");

        var configuration = BgInfoConfigurationJson.Load(path);

        Assert.True(configuration.PreserveWallpaperSlideshow);
    }

    [Fact]
    public void SaveAndLoadPreservesChartForgeXChartOptions() {
        var tempDirectory = Path.Combine(Path.GetTempPath(), "bginfo-json-" + Path.GetRandomFileName());
        Directory.CreateDirectory(tempDirectory);
        var path = Path.Combine(tempDirectory, "config.json");

        var configuration = new BgInfoConfiguration {
            ConfigurationDirectory = tempDirectory
        };
        configuration.Charts.Add(new BgInfoChart {
            Id = "usage",
            Title = "Usage",
            Kind = BgInfoChartKind.Donut,
            Values = new[] { 72d, 28d },
            Labels = new[] { "Used", "Free" },
            Palette = new[] { Color.Red, Color.Green },
            ShowLegend = true,
            ShowPointLegend = true,
            LegendPosition = BgInfoChartLegendPosition.Right,
            ShowDataLabels = true,
            Minimum = 0,
            Maximum = 100,
            Target = 95,
            RangeEnds = new[] { 70d, 85d },
            DonutInnerRadiusRatio = 0.64,
            DonutCenterValue = "72%",
            DonutCenterLabel = "Used",
            ProgressBarThicknessRatio = 0.4,
            PictorialSymbol = BgInfoChartPictorialSymbol.Person,
            PictorialColumns = 8
        });

        BgInfoConfigurationJson.Save(configuration, path);

        var roundTripped = BgInfoConfigurationJson.Load(path);
        var chart = Assert.Single(roundTripped.Charts);
        Assert.Equal(BgInfoChartKind.Donut, chart.Kind);
        Assert.Equal(new[] { "Used", "Free" }, chart.Labels);
        Assert.Equal(2, chart.Palette.Count);
        Assert.True(chart.ShowLegend);
        Assert.True(chart.ShowPointLegend);
        Assert.Equal(BgInfoChartLegendPosition.Right, chart.LegendPosition);
        Assert.True(chart.ShowDataLabels);
        Assert.Equal(0, chart.Minimum);
        Assert.Equal(100, chart.Maximum);
        Assert.Equal(95, chart.Target);
        Assert.Equal(new[] { 70d, 85d }, chart.RangeEnds);
        Assert.Equal(0.64, chart.DonutInnerRadiusRatio);
        Assert.Equal("72%", chart.DonutCenterValue);
        Assert.Equal("Used", chart.DonutCenterLabel);
        Assert.Equal(0.4, chart.ProgressBarThicknessRatio);
        Assert.Equal(BgInfoChartPictorialSymbol.Person, chart.PictorialSymbol);
        Assert.Equal(8, chart.PictorialColumns);
    }

    [Fact]
    public void SaveAndLoadPreservesTopologyOptions() {
        var tempDirectory = Path.Combine(Path.GetTempPath(), "bginfo-json-" + Path.GetRandomFileName());
        Directory.CreateDirectory(tempDirectory);
        var path = Path.Combine(tempDirectory, "config.json");

        var configuration = new BgInfoConfiguration {
            ConfigurationDirectory = tempDirectory
        };
        var topology = new BgInfoTopology {
            Title = "Lab topology",
            Subtitle = "Gateway, API, SQL",
            Width = 560,
            Height = 310,
            Anchor = BgInfoTextPosition.BottomRight,
            OffsetX = 34,
            OffsetY = 42,
            Layout = TopologyLayoutMode.Layered,
            Direction = TopologyLayoutDirection.LeftToRight,
            NodeDisplayMode = TopologyNodeDisplayMode.CompactCard,
            Theme = "Dark",
            Transparent = true,
            ShowLegend = true
        };
        topology.Groups.Add(new TopologyGroup {
            Id = "lab",
            Label = "Lab Site",
            Status = TopologyHealthStatus.Healthy,
            Symbol = "region"
        });
        topology.Nodes.Add(new TopologyNode {
            Id = "gateway",
            Label = "Gateway",
            Kind = TopologyNodeKind.Network,
            Status = TopologyHealthStatus.Healthy,
            GroupId = "lab",
            Symbol = "GW"
        });
        topology.Nodes.Add(new TopologyNode {
            Id = "api",
            Label = "API",
            Kind = TopologyNodeKind.Service,
            Status = TopologyHealthStatus.Warning,
            GroupId = "lab",
            Symbol = "API"
        });
        topology.Edges.Add(new TopologyEdge {
            Id = "gateway-api",
            SourceNodeId = "gateway",
            TargetNodeId = "api",
            Label = "HTTPS",
            Kind = TopologyEdgeKind.Connectivity,
            Status = TopologyHealthStatus.Healthy,
            Direction = TopologyDirection.Forward
        });
        configuration.Topologies.Add(topology);

        BgInfoConfigurationJson.Save(configuration, path);

        var roundTripped = BgInfoConfigurationJson.Load(path);
        var loaded = Assert.Single(roundTripped.Topologies);
        Assert.Equal("Lab topology", loaded.Title);
        Assert.Equal("Gateway, API, SQL", loaded.Subtitle);
        Assert.Equal(560, loaded.Width);
        Assert.Equal(310, loaded.Height);
        Assert.Equal(BgInfoTextPosition.BottomRight, loaded.Anchor);
        Assert.Equal(34, loaded.OffsetX);
        Assert.Equal(42, loaded.OffsetY);
        Assert.Equal(TopologyLayoutMode.Layered, loaded.Layout);
        Assert.Equal(TopologyLayoutDirection.LeftToRight, loaded.Direction);
        Assert.Equal(TopologyNodeDisplayMode.CompactCard, loaded.NodeDisplayMode);
        Assert.True(loaded.Transparent);
        Assert.True(loaded.ShowLegend);
        Assert.Single(loaded.Groups);
        Assert.Equal(2, loaded.Nodes.Count);
        Assert.Single(loaded.Edges);
        Assert.Equal("gateway-api", loaded.Edges[0].Id);
        Assert.Equal(TopologyEdgeKind.Connectivity, loaded.Edges[0].Kind);
    }

    [Fact]
    public void SaveAndLoadPreservesVisualCanvasOptions() {
        var tempDirectory = Path.Combine(Path.GetTempPath(), "bginfo-json-" + Path.GetRandomFileName());
        Directory.CreateDirectory(tempDirectory);
        var path = Path.Combine(tempDirectory, "config.json");

        var configuration = new BgInfoConfiguration {
            ConfigurationDirectory = tempDirectory
        };
        var visual = new BgInfoVisualCanvas {
            Title = "PowerBGInfo",
            Subtitle = "Desktop background insights",
            Width = 1200,
            Height = 630,
            PositionX = 12,
            PositionY = 34,
            BackgroundTop = Color.FromArgb(255, 2, 7, 19),
            BackgroundBottom = Color.FromArgb(255, 7, 26, 53),
            Accent = Color.DeepSkyBlue,
            SecondaryAccent = Color.Cyan,
            TitleColor = Color.White,
            TitleAccentColor = Color.DeepSkyBlue,
            SubtitleColor = Color.LightSteelBlue,
            TileGlassTop = Color.FromArgb(230, 10, 20, 30),
            TileGlassBottom = Color.FromArgb(220, 5, 10, 15),
            TileLabelColor = Color.LightBlue,
            TileValueColor = Color.WhiteSmoke,
            TileDetailColor = Color.SlateGray,
            TileProgressTrackColor = Color.DarkSlateBlue,
            HeroBadgeTop = Color.Navy,
            HeroBadgeBottom = Color.Black,
            HeroBadgeTextColor = Color.AliceBlue,
            FeatureAnchor = BgInfoTextPosition.BottomRight,
            FeatureWidth = 610,
            FeatureHeight = 52,
            FeatureOffsetX = 165,
            FeatureOffsetY = 120,
            TechBackdrop = false
        };
        visual.Tiles.Add(new BgInfoVisualCanvasTile {
            Side = BgInfoVisualCanvasSide.Left,
            Icon = "PC",
            Label = "HOSTNAME",
            Value = "{{HostName}}",
            Detail = "{{OSName}}",
            Accent = Color.DodgerBlue,
            Progress = 0.42,
            SurfaceStyle = BgInfoVisualCanvasTileSurfaceStyle.Raised,
            IconKind = BgInfoVisualCanvasTileIconKind.Computer,
            MiniChartKind = BgInfoVisualCanvasTileMiniChartKind.Area,
            MiniChartValues = new[] { 18d, 26d, 22d, 37d },
            MiniChartMaximum = 100
        });
        visual.Features.Add(new BgInfoVisualCanvasFeature {
            Icon = "PS",
            Label = "LIGHTWEIGHT"
        });
        configuration.VisualCanvases.Add(visual);

        BgInfoConfigurationJson.Save(configuration, path);

        var roundTripped = BgInfoConfigurationJson.Load(path);
        var loaded = Assert.Single(roundTripped.VisualCanvases);
        Assert.Equal("PowerBGInfo", loaded.Title);
        Assert.Equal("Desktop background insights", loaded.Subtitle);
        Assert.Equal(1200, loaded.Width);
        Assert.Equal(630, loaded.Height);
        Assert.Equal(12, loaded.PositionX);
        Assert.Equal(34, loaded.PositionY);
        Assert.Equal(Color.DeepSkyBlue.ToArgb(), loaded.Accent.ToArgb());
        Assert.Equal(Color.Cyan.ToArgb(), loaded.SecondaryAccent!.Value.ToArgb());
        Assert.Equal(Color.White.ToArgb(), loaded.TitleColor!.Value.ToArgb());
        Assert.Equal(Color.DeepSkyBlue.ToArgb(), loaded.TitleAccentColor!.Value.ToArgb());
        Assert.Equal(Color.LightSteelBlue.ToArgb(), loaded.SubtitleColor!.Value.ToArgb());
        Assert.Equal(Color.FromArgb(230, 10, 20, 30).ToArgb(), loaded.TileGlassTop!.Value.ToArgb());
        Assert.Equal(Color.FromArgb(220, 5, 10, 15).ToArgb(), loaded.TileGlassBottom!.Value.ToArgb());
        Assert.Equal(Color.LightBlue.ToArgb(), loaded.TileLabelColor!.Value.ToArgb());
        Assert.Equal(Color.WhiteSmoke.ToArgb(), loaded.TileValueColor!.Value.ToArgb());
        Assert.Equal(Color.SlateGray.ToArgb(), loaded.TileDetailColor!.Value.ToArgb());
        Assert.Equal(Color.DarkSlateBlue.ToArgb(), loaded.TileProgressTrackColor!.Value.ToArgb());
        Assert.Equal(Color.Navy.ToArgb(), loaded.HeroBadgeTop!.Value.ToArgb());
        Assert.Equal(Color.Black.ToArgb(), loaded.HeroBadgeBottom!.Value.ToArgb());
        Assert.Equal(Color.AliceBlue.ToArgb(), loaded.HeroBadgeTextColor!.Value.ToArgb());
        Assert.Equal(BgInfoTextPosition.BottomRight, loaded.FeatureAnchor);
        Assert.Equal(610, loaded.FeatureWidth);
        Assert.Equal(52, loaded.FeatureHeight);
        Assert.Equal(165, loaded.FeatureOffsetX);
        Assert.Equal(120, loaded.FeatureOffsetY);
        Assert.False(loaded.TechBackdrop);

        var tile = Assert.Single(loaded.Tiles);
        Assert.Equal(BgInfoVisualCanvasSide.Left, tile.Side);
        Assert.Equal("PC", tile.Icon);
        Assert.Equal("HOSTNAME", tile.Label);
        Assert.Equal("{{HostName}}", tile.Value);
        Assert.Equal("{{OSName}}", tile.Detail);
        Assert.Equal(Color.DodgerBlue.ToArgb(), tile.Accent!.Value.ToArgb());
        Assert.Equal(0.42, tile.Progress);
        Assert.Equal(BgInfoVisualCanvasTileSurfaceStyle.Raised, tile.SurfaceStyle);
        Assert.Equal(BgInfoVisualCanvasTileIconKind.Computer, tile.IconKind);
        Assert.Equal(BgInfoVisualCanvasTileMiniChartKind.Area, tile.MiniChartKind);
        Assert.Equal(new[] { 18d, 26d, 22d, 37d }, tile.MiniChartValues);
        Assert.Equal(100, tile.MiniChartMaximum);

        var feature = Assert.Single(loaded.Features);
        Assert.Equal("PS", feature.Icon);
        Assert.Equal("LIGHTWEIGHT", feature.Label);
    }

    [Fact]
    public void LoadSkipsNullVisualCanvasChildren() {
        var tempDirectory = Path.Combine(Path.GetTempPath(), "bginfo-json-" + Path.GetRandomFileName());
        Directory.CreateDirectory(tempDirectory);
        var path = Path.Combine(tempDirectory, "config.json");

        File.WriteAllText(path, """
{
  "VisualCanvases": [
    {
      "Tiles": [
        null,
        {
          "Side": "Left",
          "Icon": "PC",
          "Label": "HOSTNAME",
          "Value": "{{HostName}}"
        }
      ],
      "Features": [
        null,
        {
          "Icon": "PS",
          "Label": "LIGHTWEIGHT"
        }
      ]
    }
  ]
}
""");

        var configuration = BgInfoConfigurationJson.Load(path);

        var visual = Assert.Single(configuration.VisualCanvases);
        Assert.Single(visual.Tiles);
        Assert.Single(visual.Features);
    }

    [Fact]
    public void SaveAndLoadPreservesImageOverlays() {
        var tempDirectory = Path.Combine(Path.GetTempPath(), "bginfo-json-" + Path.GetRandomFileName());
        Directory.CreateDirectory(tempDirectory);
        var path = Path.Combine(tempDirectory, "config.json");

        var configuration = new BgInfoConfiguration {
            ConfigurationDirectory = tempDirectory
        };
        configuration.Images.Add(new BgInfoImage {
            Path = @"Images\PowerBGInfo.png",
            Width = 180,
            Height = 64,
            Anchor = BgInfoTextPosition.BottomRight,
            OffsetX = 72,
            OffsetY = 54,
            Opacity = 0.85
        });
        configuration.Images.Add(null!);

        BgInfoConfigurationJson.Save(configuration, path);

        var roundTripped = BgInfoConfigurationJson.Load(path);
        var image = Assert.Single(roundTripped.Images);
        Assert.EndsWith(Path.Combine("Images", "PowerBGInfo.png"), image.Path);
        Assert.Equal(180, image.Width);
        Assert.Equal(64, image.Height);
        Assert.Equal(BgInfoTextPosition.BottomRight, image.Anchor);
        Assert.Equal(72, image.OffsetX);
        Assert.Equal(54, image.OffsetY);
        Assert.Equal(0.85, image.Opacity);
    }

    [Fact]
    public void LoadRejectsInvalidImageOpacity() {
        var tempDirectory = Path.Combine(Path.GetTempPath(), "bginfo-json-" + Path.GetRandomFileName());
        Directory.CreateDirectory(tempDirectory);
        var path = Path.Combine(tempDirectory, "config.json");
        File.WriteAllText(path, """
{
  "Images": [
    {
      "Path": "logo.png",
      "Opacity": 1.2
    }
  ]
}
""");

        Assert.Throws<InvalidDataException>(() => BgInfoConfigurationJson.Load(path));
    }
}
