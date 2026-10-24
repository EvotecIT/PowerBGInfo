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
    private static IReadOnlyList<double> ResolveChartValues(BgInfoConfiguration config, BgInfoChart chart, DateTimeOffset now, int index, Action<string>? warning = null)
    {
        var values = ResolveChartInputValues(chart);
        if (values.Count == 0 && chart.Metric != BgInfoChartMetric.None) {
            warning?.Invoke($"The metric for chart '{chart.Title}' is unavailable. Historical values are retained but are not displayed as a current reading.");
            return values;
        }
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

    private IReadOnlyList<IReadOnlyList<double>> ResolveAllChartValues(BgInfoConfiguration config, DateTimeOffset now)
    {
        var values = new IReadOnlyList<double>[config.Charts.Count];
        for (var i = 0; i < config.Charts.Count; i++)
        {
            var chart = config.Charts[i];
            values[i] = ResolveChartValues(config, chart, now, i, Warning);
        }
        return values;
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

}
