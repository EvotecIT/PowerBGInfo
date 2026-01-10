namespace PowerBGInfo;

/// <summary>
/// Defines where generated BGInfo output is applied.
/// </summary>
public enum BgInfoTarget {
    /// <summary>Apply as wallpaper only.</summary>
    Wallpaper,
    /// <summary>Write output file only.</summary>
    File,
    /// <summary>Write output file and apply as wallpaper.</summary>
    Both
}
