namespace PowerBGInfo;

/// <summary>
/// Built-in variable providers that can be resolved without PowerShell.
/// </summary>
public enum BgInfoVariableProvider {
    /// <summary>
    /// Enumerates ready local/system volumes.
    /// </summary>
    Volumes
}

/// <summary>
/// Defines a named variable backed by a built-in provider.
/// </summary>
public sealed class BgInfoVariable {
    /// <summary>
    /// Variable name referenced by entry templates.
    /// </summary>
    public string Name { get; set; } = string.Empty;
    /// <summary>
    /// Built-in provider used to populate the variable.
    /// </summary>
    public BgInfoVariableProvider Provider { get; set; }
    /// <summary>
    /// Optional provider argument for future filtering/customization.
    /// </summary>
    public string? Argument { get; set; }
}
