using SixLabors.ImageSharp;

namespace PowerBGInfo;

public enum BgInfoEntryType
{
    Label,
    Value
}

public class BgInfoEntry
{
    public BgInfoEntryType Type { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Value { get; set; }
    public Color? Color { get; set; }
    public float? FontSize { get; set; }
    public string? FontFamilyName { get; set; }
    public Color? ValueColor { get; set; }
    public float? ValueFontSize { get; set; }
    public string? ValueFontFamilyName { get; set; }
}
