namespace PowerBGInfo;

/// <summary>Controls how an image is fitted inside its destination rectangle.</summary>
public enum BgInfoImageFit {
    /// <summary>Scale the image to fill the destination rectangle exactly.</summary>
    Stretch,
    /// <summary>Scale the whole image into the rectangle while preserving aspect ratio.</summary>
    Contain,
    /// <summary>Scale the image to cover the rectangle while preserving aspect ratio.</summary>
    Cover,
    /// <summary>Place the image at its natural size in the center of the rectangle.</summary>
    Center,
    /// <summary>Repeat the image to fill the destination rectangle.</summary>
    Tile
}
