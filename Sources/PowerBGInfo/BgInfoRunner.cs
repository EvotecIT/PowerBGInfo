using System;

namespace PowerBGInfo;

/// <summary>
/// Executes BGInfo generation using default services.
/// </summary>
public static class BgInfoRunner {
    /// <summary>
    /// Generates the BGInfo image based on the provided configuration.
    /// </summary>
    /// <param name="configuration">BGInfo configuration.</param>
    /// <returns>Path to the generated image.</returns>
    public static string Run(BgInfoConfiguration configuration) {
        if (configuration == null) throw new ArgumentNullException(nameof(configuration));

        var imageService = new ImageService();
        var wallpaperService = new WallpaperService();
        var generator = new BgInfoGenerator(imageService, wallpaperService);
        return generator.Generate(configuration);
    }
}
