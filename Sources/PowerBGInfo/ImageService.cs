namespace PowerBGInfo;

/// <summary>
/// Provides image loading and saving operations for BGInfo generation.
/// </summary>
public class ImageService
{
    /// <summary>
    /// Loads an image from the specified path.
    /// </summary>
    /// <param name="filePath">Path to the image.</param>
    /// <returns>The loaded image.</returns>
    public BgInfoRasterImage Load(string filePath) => BgInfoRasterImage.Load(filePath);

    /// <summary>
    /// Saves the image to the specified path.
    /// </summary>
    /// <param name="image">The image to save.</param>
    /// <param name="filePath">Destination path.</param>
    public void Save(BgInfoRasterImage image, string filePath) => image.Save(filePath);
}
