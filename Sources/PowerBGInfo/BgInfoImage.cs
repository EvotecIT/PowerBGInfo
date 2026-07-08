namespace PowerBGInfo;

/// <summary>Defines an image overlay rendered on top of the generated wallpaper.</summary>
public sealed class BgInfoImage {
    /// <summary>Path to the image file.</summary>
    public string Path { get; set; } = string.Empty;
    /// <summary>Target image width in pixels. Zero preserves the source width or derives it from Height.</summary>
    public int Width { get; set; }
    /// <summary>Target image height in pixels. Zero preserves the source height or derives it from Width.</summary>
    public int Height { get; set; }
    /// <summary>Anchor position used for placement.</summary>
    public BgInfoTextPosition Anchor { get; set; } = BgInfoTextPosition.BottomRight;
    /// <summary>Horizontal offset from the anchor.</summary>
    public int OffsetX { get; set; } = 32;
    /// <summary>Vertical offset from the anchor.</summary>
    public int OffsetY { get; set; } = 32;
    /// <summary>Explicit X position override.</summary>
    public int? PositionX { get; set; }
    /// <summary>Explicit Y position override.</summary>
    public int? PositionY { get; set; }
    /// <summary>Image opacity from zero to one.</summary>
    public double Opacity { get; set; } = 1d;
    /// <summary>How the image is fitted inside the destination rectangle.</summary>
    public BgInfoImageFit Fit { get; set; } = BgInfoImageFit.Stretch;
}
