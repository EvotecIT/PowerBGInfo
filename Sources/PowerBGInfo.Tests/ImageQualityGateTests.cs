using ChartForgeX.Composition;
using ChartForgeX.Primitives;
using PowerBGInfo.QualityGate;

namespace PowerBGInfo.Tests;

public class ImageQualityGateTests {
    [Fact]
    public void ComparePassesIdenticalImagesAndWritesReports() {
        var root = CreateTempRoot();
        try {
            var baseline = Path.Combine(root, "baseline");
            var candidate = Path.Combine(root, "candidate");
            var output = Path.Combine(root, "report");
            Directory.CreateDirectory(baseline);
            Directory.CreateDirectory(candidate);
            WriteSample(Path.Combine(baseline, "wallpaper.png"), ChartColors.Emerald400);
            WriteSample(Path.Combine(candidate, "wallpaper.png"), ChartColors.Emerald400);

            var report = ImageQualityComparer.Compare(new ImageComparisonOptions {
                BaselineDirectory = baseline,
                CandidateDirectory = candidate,
                OutputDirectory = output
            });

            Assert.Equal(1, report.Compared);
            Assert.Equal(1, report.Passed);
            Assert.Equal(0, report.Failed);
            Assert.True(File.Exists(Path.Combine(output, "diffs", "wallpaper.diff.png")));
        } finally {
            DeleteTempRoot(root);
        }
    }

    [Fact]
    public void CompareFailsWhenImageDifferenceExceedsThresholds() {
        var root = CreateTempRoot();
        try {
            var baseline = Path.Combine(root, "baseline");
            var candidate = Path.Combine(root, "candidate");
            var output = Path.Combine(root, "report");
            Directory.CreateDirectory(baseline);
            Directory.CreateDirectory(candidate);
            WriteSample(Path.Combine(baseline, "wallpaper.png"), ChartColors.Emerald400);
            WriteSample(Path.Combine(candidate, "wallpaper.png"), ChartColor.FromHex("#F43F5E"));

            var report = ImageQualityComparer.Compare(new ImageComparisonOptions {
                BaselineDirectory = baseline,
                CandidateDirectory = candidate,
                OutputDirectory = output,
                MeanThreshold = 0,
                RmseThreshold = 0,
                MaxChannelThreshold = 0,
                ChangedPixelPercentThreshold = 0
            });

            Assert.Equal(1, report.Compared);
            Assert.Equal(0, report.Passed);
            Assert.Equal(1, report.Failed);
            Assert.True(report.Results[0].MaxChannelError > 0);
        } finally {
            DeleteTempRoot(root);
        }
    }

    private static void WriteSample(string path, ChartColor color) {
        ImageComposition.Create(16, 10, color)
            .FillRectangle(2, 2, 6, 4, ChartColors.White.WithOpacity(0.6))
            .Save(path);
    }

    private static string CreateTempRoot() {
        var root = Path.Combine(Path.GetTempPath(), "PowerBGInfo.QualityGate." + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        return root;
    }

    private static void DeleteTempRoot(string root) {
        if (Directory.Exists(root)) Directory.Delete(root, recursive: true);
    }
}
