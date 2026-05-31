namespace PowerBGInfo.QualityGate;

public sealed class ImageComparisonOptions {
    public string BaselineDirectory { get; set; } = string.Empty;
    public string CandidateDirectory { get; set; } = string.Empty;
    public string OutputDirectory { get; set; } = string.Empty;
    public bool Recursive { get; set; }
    public bool FailOnMissing { get; set; } = true;
    public double MeanThreshold { get; set; } = 1.25;
    public double RmseThreshold { get; set; } = 3.0;
    public int MaxChannelThreshold { get; set; } = 48;
    public double ChangedPixelPercentThreshold { get; set; } = 2.0;
    public double PerceptualMeanThreshold { get; set; } = 3.0;
    public double PerceptualRmseThreshold { get; set; } = 12.0;
    public double StructuralSimilarityThreshold { get; set; } = 0.995;
    public int DiffScale { get; set; } = 4;
}
