namespace PowerBGInfo.QualityGate;

public sealed class ImageComparisonReport {
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public string BaselineDirectory { get; set; } = string.Empty;
    public string CandidateDirectory { get; set; } = string.Empty;
    public string OutputDirectory { get; set; } = string.Empty;
    public double MeanThreshold { get; set; }
    public double RmseThreshold { get; set; }
    public int MaxChannelThreshold { get; set; }
    public double ChangedPixelPercentThreshold { get; set; }
    public int Compared { get; set; }
    public int Passed { get; set; }
    public int Failed { get; set; }
    public int Missing { get; set; }
    public List<ImageComparisonResult> Results { get; set; } = new();
}

public sealed class ImageComparisonResult {
    public string RelativePath { get; set; } = string.Empty;
    public string BaselinePath { get; set; } = string.Empty;
    public string CandidatePath { get; set; } = string.Empty;
    public string DiffPath { get; set; } = string.Empty;
    public int BaselineWidth { get; set; }
    public int BaselineHeight { get; set; }
    public int CandidateWidth { get; set; }
    public int CandidateHeight { get; set; }
    public bool DimensionsMatch { get; set; }
    public bool MissingCandidate { get; set; }
    public bool Passed { get; set; }
    public string Message { get; set; } = string.Empty;
    public double MeanAbsoluteChannelError { get; set; }
    public double RmseChannelError { get; set; }
    public int MaxChannelError { get; set; }
    public double ChangedPixelPercent { get; set; }
}
