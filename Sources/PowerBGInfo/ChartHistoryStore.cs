using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

namespace PowerBGInfo;

internal static class ChartHistoryStore {
    private const char Separator = '|';

    public static List<ChartSample> Load(string path) {
        var samples = new List<ChartSample>();
        if (!File.Exists(path)) {
            return samples;
        }

        foreach (var line in File.ReadAllLines(path)) {
            if (string.IsNullOrWhiteSpace(line)) {
                continue;
            }
            var parts = line.Split(Separator);
            if (parts.Length != 2) {
                continue;
            }
            if (!DateTimeOffset.TryParse(parts[0], CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var ts)) {
                continue;
            }
            if (!double.TryParse(parts[1], NumberStyles.Float | NumberStyles.AllowThousands, CultureInfo.InvariantCulture, out var value)) {
                continue;
            }
            samples.Add(new ChartSample(ts, value));
        }

        return samples;
    }

    public static void Save(string path, IReadOnlyList<ChartSample> samples) {
        var directory = Path.GetDirectoryName(path);
        if (!string.IsNullOrWhiteSpace(directory)) {
            Directory.CreateDirectory(directory);
        }

        using var writer = new StreamWriter(path, false);
        foreach (var sample in samples) {
            writer.Write(sample.Timestamp.ToString("O", CultureInfo.InvariantCulture));
            writer.Write(Separator);
            writer.WriteLine(sample.Value.ToString("G", CultureInfo.InvariantCulture));
        }
    }
}

internal readonly struct ChartSample {
    public ChartSample(DateTimeOffset timestamp, double value) {
        Timestamp = timestamp;
        Value = value;
    }

    public DateTimeOffset Timestamp { get; }
    public double Value { get; }
}
