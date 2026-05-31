using ChartForgeX;
using ChartForgeX.Raster;

namespace PowerBGInfo.QualityGate;

public static class ImageQualityComparer {
    private static readonly string[] SupportedExtensions = { ".png", ".jpg", ".jpeg", ".bmp", ".ppm", ".pnm", ".tif", ".tiff" };

    public static ImageComparisonReport Compare(ImageComparisonOptions options) {
        ValidateOptions(options);
        Directory.CreateDirectory(options.OutputDirectory);
        var diffDirectory = Path.Combine(options.OutputDirectory, "diffs");
        Directory.CreateDirectory(diffDirectory);

        var report = new ImageComparisonReport {
            BaselineDirectory = Path.GetFullPath(options.BaselineDirectory),
            CandidateDirectory = Path.GetFullPath(options.CandidateDirectory),
            OutputDirectory = Path.GetFullPath(options.OutputDirectory),
            MeanThreshold = options.MeanThreshold,
            RmseThreshold = options.RmseThreshold,
            MaxChannelThreshold = options.MaxChannelThreshold,
            ChangedPixelPercentThreshold = options.ChangedPixelPercentThreshold,
            PerceptualMeanThreshold = options.PerceptualMeanThreshold,
            PerceptualRmseThreshold = options.PerceptualRmseThreshold,
            StructuralSimilarityThreshold = options.StructuralSimilarityThreshold
        };

        foreach (var baselinePath in EnumerateImages(options.BaselineDirectory, options.Recursive)) {
            var relativePath = Path.GetRelativePath(options.BaselineDirectory, baselinePath);
            var candidatePath = Path.Combine(options.CandidateDirectory, relativePath);
            var diffPath = Path.Combine(diffDirectory, Path.ChangeExtension(relativePath, ".diff.png"));
            var result = CompareOne(relativePath, baselinePath, candidatePath, diffPath, options);
            report.Results.Add(result);
        }

        report.Compared = report.Results.Count(r => !r.MissingCandidate);
        report.Missing = report.Results.Count(r => r.MissingCandidate);
        report.Passed = report.Results.Count(r => r.Passed);
        report.Failed = report.Results.Count(r => !r.Passed);
        return report;
    }

    private static ImageComparisonResult CompareOne(string relativePath, string baselinePath, string candidatePath, string diffPath, ImageComparisonOptions options) {
        var result = new ImageComparisonResult {
            RelativePath = NormalizePath(relativePath),
            BaselinePath = Path.GetFullPath(baselinePath),
            CandidatePath = Path.GetFullPath(candidatePath),
            DiffPath = Path.GetFullPath(diffPath)
        };

        if (!File.Exists(candidatePath)) {
            result.MissingCandidate = true;
            result.Passed = !options.FailOnMissing;
            result.Message = "Candidate image is missing.";
            return result;
        }

        var baseline = RasterImageDecoder.Read(baselinePath);
        var candidate = RasterImageDecoder.Read(candidatePath);
        result.BaselineWidth = baseline.Width;
        result.BaselineHeight = baseline.Height;
        result.CandidateWidth = candidate.Width;
        result.CandidateHeight = candidate.Height;
        result.DimensionsMatch = baseline.Width == candidate.Width && baseline.Height == candidate.Height;

        if (!result.DimensionsMatch) {
            result.Passed = false;
            result.Message = $"Dimensions differ: baseline {baseline.Width}x{baseline.Height}, candidate {candidate.Width}x{candidate.Height}.";
            return result;
        }

        ComputeMetrics(baseline, candidate, result, diffPath, options.DiffScale);
        result.StrictPixelMatch = result.MeanAbsoluteChannelError <= options.MeanThreshold &&
            result.RmseChannelError <= options.RmseThreshold &&
            result.MaxChannelError <= options.MaxChannelThreshold &&
            result.ChangedPixelPercent <= options.ChangedPixelPercentThreshold;
        result.PerceptualMatch = result.MeanAbsoluteChannelError <= options.PerceptualMeanThreshold &&
            (result.RmseChannelError <= options.PerceptualRmseThreshold ||
             result.StructuralSimilarity >= options.StructuralSimilarityThreshold);
        result.Passed = result.StrictPixelMatch || result.PerceptualMatch;
        result.Message = result.Passed
            ? (result.StrictPixelMatch ? "Image difference is within strict pixel thresholds." : "Image difference is within perceptual quality thresholds.")
            : "Image difference exceeds quality thresholds.";
        return result;
    }

    private static void ComputeMetrics(RgbaImage baseline, RgbaImage candidate, ImageComparisonResult result, string diffPath, int diffScale) {
        var totalAbs = 0d;
        var totalSquared = 0d;
        var baselineLuma = 0d;
        var candidateLuma = 0d;
        var baselineLumaSquared = 0d;
        var candidateLumaSquared = 0d;
        var lumaProduct = 0d;
        var max = 0;
        var changedPixels = 0;
        var diffPixels = new byte[baseline.Width * baseline.Height * 4];
        var channelCount = baseline.Width * baseline.Height * 4d;
        var pixelCount = baseline.Width * baseline.Height;

        for (var i = 0; i < baseline.Pixels.Length; i += 4) {
            var pixelMax = 0;
            for (var c = 0; c < 4; c++) {
                var delta = Math.Abs(baseline.Pixels[i + c] - candidate.Pixels[i + c]);
                totalAbs += delta;
                totalSquared += delta * delta;
                if (delta > max) max = delta;
                if (delta > pixelMax) pixelMax = delta;
            }

            if (pixelMax > 0) changedPixels++;
            var heat = ClampByte(pixelMax * Math.Max(1, diffScale));
            diffPixels[i] = heat;
            diffPixels[i + 1] = (byte)(heat == 0 ? 0 : Math.Max(0, 96 - heat / 3));
            diffPixels[i + 2] = (byte)(heat == 0 ? 0 : Math.Max(0, 255 - heat));
            diffPixels[i + 3] = 255;

            var y1 = Luminance(baseline.Pixels[i], baseline.Pixels[i + 1], baseline.Pixels[i + 2]);
            var y2 = Luminance(candidate.Pixels[i], candidate.Pixels[i + 1], candidate.Pixels[i + 2]);
            baselineLuma += y1;
            candidateLuma += y2;
            baselineLumaSquared += y1 * y1;
            candidateLumaSquared += y2 * y2;
            lumaProduct += y1 * y2;
        }

        result.MeanAbsoluteChannelError = totalAbs / channelCount;
        result.RmseChannelError = Math.Sqrt(totalSquared / channelCount);
        result.MaxChannelError = max;
        result.ChangedPixelPercent = changedPixels * 100d / pixelCount;
        result.StructuralSimilarity = StructuralSimilarity(
            baselineLuma / pixelCount,
            candidateLuma / pixelCount,
            baselineLumaSquared / pixelCount,
            candidateLumaSquared / pixelCount,
            lumaProduct / pixelCount);
        Directory.CreateDirectory(Path.GetDirectoryName(diffPath) ?? ".");
        File.WriteAllBytes(diffPath, new RgbaImage(baseline.Width, baseline.Height, diffPixels).ToPng());
    }

    private static IEnumerable<string> EnumerateImages(string directory, bool recursive) {
        var option = recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
        return Directory.EnumerateFiles(directory, "*.*", option)
            .Where(path => SupportedExtensions.Contains(Path.GetExtension(path), StringComparer.OrdinalIgnoreCase))
            .OrderBy(path => path, StringComparer.OrdinalIgnoreCase);
    }

    private static void ValidateOptions(ImageComparisonOptions options) {
        if (string.IsNullOrWhiteSpace(options.BaselineDirectory)) throw new ArgumentException("Baseline directory is required.");
        if (string.IsNullOrWhiteSpace(options.CandidateDirectory)) throw new ArgumentException("Candidate directory is required.");
        if (string.IsNullOrWhiteSpace(options.OutputDirectory)) throw new ArgumentException("Output directory is required.");
        if (!Directory.Exists(options.BaselineDirectory)) throw new DirectoryNotFoundException("Baseline directory was not found: " + options.BaselineDirectory);
        if (!Directory.Exists(options.CandidateDirectory)) throw new DirectoryNotFoundException("Candidate directory was not found: " + options.CandidateDirectory);
        if (options.MeanThreshold < 0) throw new ArgumentOutOfRangeException(nameof(options.MeanThreshold));
        if (options.RmseThreshold < 0) throw new ArgumentOutOfRangeException(nameof(options.RmseThreshold));
        if (options.MaxChannelThreshold < 0 || options.MaxChannelThreshold > 255) throw new ArgumentOutOfRangeException(nameof(options.MaxChannelThreshold));
        if (options.ChangedPixelPercentThreshold < 0 || options.ChangedPixelPercentThreshold > 100) throw new ArgumentOutOfRangeException(nameof(options.ChangedPixelPercentThreshold));
        if (options.PerceptualMeanThreshold < 0) throw new ArgumentOutOfRangeException(nameof(options.PerceptualMeanThreshold));
        if (options.PerceptualRmseThreshold < 0) throw new ArgumentOutOfRangeException(nameof(options.PerceptualRmseThreshold));
        if (options.StructuralSimilarityThreshold < -1 || options.StructuralSimilarityThreshold > 1) throw new ArgumentOutOfRangeException(nameof(options.StructuralSimilarityThreshold));
    }

    private static string NormalizePath(string path) => path.Replace('\\', '/');

    private static byte ClampByte(int value) => (byte)Math.Max(0, Math.Min(255, value));

    private static double Luminance(byte red, byte green, byte blue) => 0.2126d * red + 0.7152d * green + 0.0722d * blue;

    private static double StructuralSimilarity(double meanA, double meanB, double meanSquareA, double meanSquareB, double meanProduct) {
        var varianceA = Math.Max(0, meanSquareA - meanA * meanA);
        var varianceB = Math.Max(0, meanSquareB - meanB * meanB);
        var covariance = meanProduct - meanA * meanB;
        const double c1 = 6.5025d;
        const double c2 = 58.5225d;
        var denominator = (meanA * meanA + meanB * meanB + c1) * (varianceA + varianceB + c2);
        if (denominator == 0) return 1;
        return Math.Max(-1, Math.Min(1, ((2 * meanA * meanB + c1) * (2 * covariance + c2)) / denominator));
    }
}
