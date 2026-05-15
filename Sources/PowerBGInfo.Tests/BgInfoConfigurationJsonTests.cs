using System.IO;
using System.Drawing;
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
}
