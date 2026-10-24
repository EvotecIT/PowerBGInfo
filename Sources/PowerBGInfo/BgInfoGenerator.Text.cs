using System;
using System.Linq;
using System.Collections.Generic;
using ChartForgeX.Primitives;
using ChartForgeX.Typography;

namespace PowerBGInfo;

/// <summary>
/// Generates BGInfo overlays and applies them to the configured target.
/// </summary>
public partial class BgInfoGenerator
{
    internal static IReadOnlyList<string> WrapTextLines(BgInfoRasterImage image, string? text, double wrapWidth, double fontSize, string fontFamilyName, bool bold = false, bool underline = false)
    {
        var style = BgInfoRasterImage.CreateTextStyle(fontSize, fontFamilyName, ChartColor.Black, bold ? 700 : 400, false, underline ? TextDecorationStyle.Single : TextDecorationStyle.None, TextDecorationStyle.None, TextBaseline.Normal, TextCaseTransform.None);
        return WrapTextLines(image, text, wrapWidth, style);
    }

    internal static IReadOnlyList<string> WrapTextLines(BgInfoRasterImage image, string? text, double wrapWidth, TextStyle style)
    {
        if (image == null) throw new ArgumentNullException(nameof(image));
        if (style == null) throw new ArgumentNullException(nameof(style));

        var paragraphs = NormalizeLineEndings(text).Split('\n');
        var lines = new List<string>();
        var mode = wrapWidth > 0 ? TextWrapMode.Word : TextWrapMode.NoWrap;
        var maximumWidth = wrapWidth > 0 ? wrapWidth : double.MaxValue;
        // The entry pipeline prepares display casing once before wrapping. Keep this helper's
        // string output uncased so callers can also pass its lines to the ordinary styled painter.
        var layoutStyle = style.Clone();
        layoutStyle.TextCase = TextCaseTransform.None;
        // Keep explicit blank paragraphs, including whitespace-only paragraphs. The shared owner
        // supplies one empty line for each standalone paragraph and keeps whole Unicode clusters.
        foreach (var paragraph in paragraphs)
        {
            var layout = TextLayoutEngine.Layout(paragraph, maximumWidth, layoutStyle, mode,
                maximumLines: null, trimming: TextTrimming.None);
            lines.AddRange(layout.Lines.Select(line => line.Text));
        }
        return lines;
    }

    private static List<EntryLayout> BuildEntryLayouts(BgInfoRasterImage image, BgInfoConfiguration config, IReadOnlyList<BgInfoEntry> entries)
    {
        var layouts = new List<EntryLayout>();
        foreach (var entry in entries)
        {
            ApplyEntryTextDefaults(entry, config);

            var labelStyle = CreateLabelTextStyle(entry);
            var valueStyle = entry.Type == BgInfoEntryType.Label ? labelStyle.Clone() : CreateValueTextStyle(entry);
            var labelSize = image.GetTextSize(entry.Name, labelStyle);
            var resolvedValue = ResolveEntryValue(entry);
            var transformedValue = TextCaseTransformer.Apply(resolvedValue ?? string.Empty, valueStyle.TextCase);
            valueStyle.TextCase = TextCaseTransform.None;
            var valueLines = entry.Type == BgInfoEntryType.Label
                ? Array.Empty<string>()
                : WrapTextLines(image, transformedValue, config.ValueWrapWidth, valueStyle).ToArray();
            var valueLineHeight = entry.Type == BgInfoEntryType.Label
                ? 0f
                : GetLineHeight(image, valueStyle);
            var valueWidth = valueLines.Length == 0
                ? 0f
                : valueLines.Max(line => image.GetTextSize(line, valueStyle).Width);
            var valueHeight = valueLines.Length == 0 ? 0f : valueLineHeight * valueLines.Length;
            layouts.Add(new EntryLayout(
                entry,
                labelStyle,
                valueStyle,
                labelSize.Width,
                labelSize.Height,
                valueLines,
                valueWidth,
                valueLineHeight,
                Math.Max(labelSize.Height, valueHeight)));
        }

        return layouts;
    }

    internal static void ApplyEntryTextDefaults(BgInfoEntry entry, BgInfoConfiguration config)
    {
        if (entry == null) throw new ArgumentNullException(nameof(entry));
        if (config == null) throw new ArgumentNullException(nameof(config));

        if (entry.Type != BgInfoEntryType.Label)
        {
            entry.ValueColor ??= entry.Color ?? config.ValueColor;
            entry.ValueFontSize ??= entry.FontSize ?? config.ValueFontSize;
            entry.ValueFontFamilyName ??= entry.FontFamilyName ?? config.ValueFontFamilyName;
            entry.ValueFontWeight ??= ResolveWeight(entry.ValueBold) ?? entry.FontWeight ?? ResolveWeight(entry.Bold) ?? config.ValueFontWeight;
            entry.ValueBold ??= entry.ValueFontWeight >= 600;
            entry.ValueItalic ??= entry.Italic ?? config.ValueItalic;
            entry.ValueUnderlineStyle ??= ResolveUnderline(entry.ValueUnderline) ?? entry.UnderlineStyle ?? ResolveUnderline(entry.Underline) ?? config.ValueUnderlineStyle;
            entry.ValueUnderline ??= entry.ValueUnderlineStyle != TextDecorationStyle.None;
            entry.ValueStrikethroughStyle ??= entry.StrikethroughStyle ?? config.ValueStrikethroughStyle;
            entry.ValueBaseline ??= entry.Baseline ?? config.ValueBaseline;
            entry.ValueTextCase ??= entry.TextCase ?? config.ValueTextCase;
        }

        entry.Color ??= config.Color;
        entry.FontSize ??= config.FontSize;
        entry.FontFamilyName ??= config.FontFamilyName;
        entry.FontWeight ??= ResolveWeight(entry.Bold) ?? config.FontWeight;
        entry.Bold ??= entry.FontWeight >= 600;
        entry.Italic ??= config.Italic;
        entry.UnderlineStyle ??= ResolveUnderline(entry.Underline) ?? config.UnderlineStyle;
        entry.Underline ??= entry.UnderlineStyle != TextDecorationStyle.None;
        entry.StrikethroughStyle ??= config.StrikethroughStyle;
        entry.Baseline ??= config.Baseline;
        entry.TextCase ??= config.TextCase;
    }

    private static int? ResolveWeight(bool? bold) => bold.HasValue ? (bold.Value ? 700 : 400) : null;

    private static TextDecorationStyle? ResolveUnderline(bool? underline) => underline.HasValue ? (underline.Value ? TextDecorationStyle.Single : TextDecorationStyle.None) : null;

    private static TextStyle CreateLabelTextStyle(BgInfoEntry entry) => BgInfoRasterImage.CreateTextStyle(
        entry.FontSize!.Value,
        entry.FontFamilyName,
        entry.Color!.Value,
        entry.FontWeight!.Value,
        entry.Italic!.Value,
        entry.UnderlineStyle!.Value,
        entry.StrikethroughStyle!.Value,
        entry.Baseline!.Value,
        entry.TextCase!.Value);

    private static TextStyle CreateValueTextStyle(BgInfoEntry entry) => BgInfoRasterImage.CreateTextStyle(
        entry.ValueFontSize!.Value,
        entry.ValueFontFamilyName,
        entry.ValueColor!.Value,
        entry.ValueFontWeight!.Value,
        entry.ValueItalic!.Value,
        entry.ValueUnderlineStyle!.Value,
        entry.ValueStrikethroughStyle!.Value,
        entry.ValueBaseline!.Value,
        entry.ValueTextCase!.Value);

    private static string? ResolveEntryValue(BgInfoEntry entry)
    {
        if (!string.IsNullOrWhiteSpace(entry.BuiltinValue))
        {
            return SystemInfoProvider.GetValue(entry.BuiltinValue!);
        }

        return entry.Value;
    }

    /// <summary>
    /// Section labels without values span both columns, so a large heading does not push the value column away.
    /// </summary>
    internal static (double LabelColumnWidth, double TotalWidth) MeasureTextColumns(
        IEnumerable<(bool IsHeading, double LabelWidth, double ValueWidth)> rows, double columnGap)
    {
        double labelColumn = 0;
        double valueColumn = 0;
        double heading = 0;
        bool hasValueEntries = false;
        foreach (var row in rows)
        {
            if (row.IsHeading)
            {
                heading = Math.Max(heading, row.LabelWidth);
                continue;
            }

            hasValueEntries = true;
            labelColumn = Math.Max(labelColumn, row.LabelWidth);
            valueColumn = Math.Max(valueColumn, row.ValueWidth);
        }

        var columns = hasValueEntries ? labelColumn + columnGap + valueColumn : 0;
        return (labelColumn, Math.Max(columns, heading));
    }

    private static double GetTextBlockHeight(BgInfoConfiguration config, IReadOnlyList<EntryLayout> entryLayouts)
    {
        if (entryLayouts.Count == 0)
        {
            return 0f;
        }

        return entryLayouts.Sum(layout => layout.RowHeight) + (entryLayouts.Count - 1) * config.SpaceBetweenLines;
    }

    private static double GetLineHeight(BgInfoRasterImage image, TextStyle style)
    {
        return image.GetTextSize("Ag", style).Height;
    }

    private static string NormalizeLineEndings(string? text)
    {
        return (text ?? string.Empty).Replace("\r\n", "\n").Replace('\r', '\n');
    }

    private sealed class EntryLayout
    {
        public EntryLayout(
            BgInfoEntry entry,
            TextStyle labelStyle,
            TextStyle valueStyle,
            double labelWidth,
            double labelHeight,
            string[] valueLines,
            double valueWidth,
            double valueLineHeight,
            double rowHeight)
        {
            Entry = entry;
            LabelStyle = labelStyle;
            ValueStyle = valueStyle;
            LabelWidth = labelWidth;
            LabelHeight = labelHeight;
            ValueLines = valueLines;
            ValueWidth = valueWidth;
            ValueLineHeight = valueLineHeight;
            RowHeight = rowHeight;
        }

        public BgInfoEntry Entry { get; }
        public TextStyle LabelStyle { get; }
        public TextStyle ValueStyle { get; }
        public double LabelWidth { get; }
        public double LabelHeight { get; }
        public string[] ValueLines { get; }
        public double ValueWidth { get; }
        public double ValueLineHeight { get; }
        public double RowHeight { get; }
    }
}
