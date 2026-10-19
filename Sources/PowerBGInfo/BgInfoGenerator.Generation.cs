namespace PowerBGInfo;

public partial class BgInfoGenerator {
    /// <summary>
    /// Generates the BGInfo image and returns its path. Runs for the same Windows
    /// user serialize across processes and sessions through rendering and application.
    /// </summary>
    /// <param name="config">Configuration controlling layout and output.</param>
    /// <returns>Path to the generated image.</returns>
    /// <exception cref="TimeoutException">Another run for this user has not finished within five minutes.</exception>
    public string Generate(BgInfoConfiguration config) {
        if (config == null) throw new ArgumentNullException(nameof(config));
        using var generation = BgInfoGenerationLease.Acquire();
        return GenerateCore(config);
    }
}
