using Color = ChartForgeX.Primitives.ChartColor;

namespace PowerBGInfo;

/// <summary>
/// Describes the kind of BGInfo entry being rendered.
/// </summary>
public enum BgInfoEntryType {
    /// <summary>Renders a label-only entry.</summary>
    Label,
    /// <summary>Renders a label and value entry.</summary>
    Value
}

/// <summary>
/// Defines a single BGInfo label/value entry.
/// </summary>
public class BgInfoEntry {
    /// <summary>
    /// Gets or sets the entry type.
    /// </summary>
    public BgInfoEntryType Type { get; set; }
    /// <summary>
    /// Gets or sets the label text.
    /// </summary>
    public string Name { get; set; } = string.Empty;
    /// <summary>
    /// Gets or sets the value text.
    /// </summary>
    public string? Value { get; set; }
    /// <summary>
    /// Gets or sets the built-in value token resolved at render time.
    /// </summary>
    public string? BuiltinValue { get; set; }
    /// <summary>
    /// Gets or sets the variable name that should be expanded for this entry.
    /// When set, <see cref="Name"/> and <see cref="Value"/> are treated as templates.
    /// </summary>
    public string? ForEach { get; set; }
    /// <summary>
    /// Gets or sets the label color.
    /// </summary>
    public Color? Color { get; set; }
    /// <summary>
    /// Gets or sets the label font size.
    /// </summary>
    public float? FontSize { get; set; }
    /// <summary>
    /// Gets or sets the label font family.
    /// </summary>
    public string? FontFamilyName { get; set; }
    /// <summary>
    /// Gets or sets whether the label uses a bold font weight.
    /// </summary>
    public bool? Bold { get; set; }
    /// <summary>
    /// Gets or sets whether the label is underlined.
    /// </summary>
    public bool? Underline { get; set; }
    /// <summary>
    /// Gets or sets the value color.
    /// </summary>
    public Color? ValueColor { get; set; }
    /// <summary>
    /// Gets or sets the value font size.
    /// </summary>
    public float? ValueFontSize { get; set; }
    /// <summary>
    /// Gets or sets the value font family.
    /// </summary>
    public string? ValueFontFamilyName { get; set; }
    /// <summary>
    /// Gets or sets whether the value uses a bold font weight.
    /// </summary>
    public bool? ValueBold { get; set; }
    /// <summary>
    /// Gets or sets whether the value is underlined.
    /// </summary>
    public bool? ValueUnderline { get; set; }
}
