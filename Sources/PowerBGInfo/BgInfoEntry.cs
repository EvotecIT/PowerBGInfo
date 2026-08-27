using Color = ChartForgeX.Primitives.ChartColor;
using ChartForgeX.Typography;

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
    private int? _fontWeight;
    private TextDecorationStyle? _underlineStyle;
    private TextDecorationStyle? _strikethroughStyle;
    private TextBaseline? _baseline;
    private TextCaseTransform? _textCase;
    private int? _valueFontWeight;
    private TextDecorationStyle? _valueUnderlineStyle;
    private TextDecorationStyle? _valueStrikethroughStyle;
    private TextBaseline? _valueBaseline;
    private TextCaseTransform? _valueTextCase;
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
    /// Gets or sets the numeric label font weight from 100 through 900.
    /// </summary>
    public int? FontWeight {
        get => _fontWeight;
        set => _fontWeight = BgInfoTextStyleValidation.ValidateFontWeight(value, nameof(value));
    }
    /// <summary>
    /// Gets or sets whether the label uses italic text.
    /// </summary>
    public bool? Italic { get; set; }
    /// <summary>
    /// Gets or sets whether the label is underlined.
    /// </summary>
    public bool? Underline { get; set; }
    /// <summary>
    /// Gets or sets the label underline pattern.
    /// </summary>
    public TextDecorationStyle? UnderlineStyle {
        get => _underlineStyle;
        set => _underlineStyle = BgInfoTextStyleValidation.ValidateEnum(value, nameof(value));
    }
    /// <summary>
    /// Gets or sets the label strikethrough pattern.
    /// </summary>
    public TextDecorationStyle? StrikethroughStyle {
        get => _strikethroughStyle;
        set => _strikethroughStyle = BgInfoTextStyleValidation.ValidateEnum(value, nameof(value));
    }
    /// <summary>
    /// Gets or sets label subscript or superscript placement.
    /// </summary>
    public TextBaseline? Baseline {
        get => _baseline;
        set => _baseline = BgInfoTextStyleValidation.ValidateEnum(value, nameof(value));
    }
    /// <summary>
    /// Gets or sets the display-time label casing transform.
    /// </summary>
    public TextCaseTransform? TextCase {
        get => _textCase;
        set => _textCase = BgInfoTextStyleValidation.ValidateEnum(value, nameof(value));
    }
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
    /// Gets or sets the numeric value font weight from 100 through 900.
    /// </summary>
    public int? ValueFontWeight {
        get => _valueFontWeight;
        set => _valueFontWeight = BgInfoTextStyleValidation.ValidateFontWeight(value, nameof(value));
    }
    /// <summary>
    /// Gets or sets whether the value uses italic text.
    /// </summary>
    public bool? ValueItalic { get; set; }
    /// <summary>
    /// Gets or sets whether the value is underlined.
    /// </summary>
    public bool? ValueUnderline { get; set; }
    /// <summary>
    /// Gets or sets the value underline pattern.
    /// </summary>
    public TextDecorationStyle? ValueUnderlineStyle {
        get => _valueUnderlineStyle;
        set => _valueUnderlineStyle = BgInfoTextStyleValidation.ValidateEnum(value, nameof(value));
    }
    /// <summary>
    /// Gets or sets the value strikethrough pattern.
    /// </summary>
    public TextDecorationStyle? ValueStrikethroughStyle {
        get => _valueStrikethroughStyle;
        set => _valueStrikethroughStyle = BgInfoTextStyleValidation.ValidateEnum(value, nameof(value));
    }
    /// <summary>
    /// Gets or sets value subscript or superscript placement.
    /// </summary>
    public TextBaseline? ValueBaseline {
        get => _valueBaseline;
        set => _valueBaseline = BgInfoTextStyleValidation.ValidateEnum(value, nameof(value));
    }
    /// <summary>
    /// Gets or sets the display-time value casing transform.
    /// </summary>
    public TextCaseTransform? ValueTextCase {
        get => _valueTextCase;
        set => _valueTextCase = BgInfoTextStyleValidation.ValidateEnum(value, nameof(value));
    }
}
