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
    public static string Run(BgInfoConfiguration configuration) => Run(configuration, null);

    /// <summary>
    /// Generates the BGInfo image and reports warnings, for example a base image that is PowerBGInfo's own earlier output.
    /// </summary>
    /// <param name="configuration">BGInfo configuration.</param>
    /// <param name="warning">Receives each warning; null ignores them.</param>
    /// <returns>Path to the generated image.</returns>
    public static string Run(BgInfoConfiguration configuration, Action<string>? warning) {
        if (configuration == null) throw new ArgumentNullException(nameof(configuration));

        var imageService = new ImageService();
        var wallpaperService = new WallpaperService();
        var generator = new BgInfoGenerator(imageService, wallpaperService) { Warning = warning };
        return generator.Generate(configuration);
    }
}
