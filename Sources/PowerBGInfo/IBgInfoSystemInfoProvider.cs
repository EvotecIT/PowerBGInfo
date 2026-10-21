namespace PowerBGInfo;

/// <summary>Reads machine facts without coupling consumers to a particular operating-system probe.</summary>
public interface IBgInfoSystemInfoProvider {
    /// <summary>Collects the supported facts, retaining Unknown or null for unavailable values.</summary>
    /// <returns>A snapshot with explicit availability for its facts.</returns>
    BgInfoSystemInfo GetSnapshot();

    /// <summary>Resolves a recognized built-in token; unrecognized tokens return an empty string.</summary>
    /// <param name="builtin">Built-in token name.</param>
    /// <returns>The resolved display value.</returns>
    string GetValue(string builtin);

    /// <summary>Reads a numeric metric without converting unavailable measurements to zero.</summary>
    /// <param name="metric">Metric to read.</param>
    /// <param name="argument">Optional drive letter or mount path.</param>
    /// <param name="value">Measurement when available.</param>
    /// <returns>Whether the measurement was available.</returns>
    bool TryGetNumericValue(BgInfoChartMetric metric, string? argument, out double value);
}

/// <summary>Availability of a fact in the current snapshot.</summary>
public enum BgInfoFactState {
    /// <summary>The provider returned a value.</summary>
    Available,
    /// <summary>The provider supports the fact but could not read it.</summary>
    Unavailable,
    /// <summary>The operating system or selected build does not support this fact.</summary>
    Unsupported
}
