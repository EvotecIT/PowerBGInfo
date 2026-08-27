using System.Collections;
using Color = ChartForgeX.Primitives.ChartColor;
using ChartForgeX.Typography;
using System.Globalization;
using System.IO;
using System.Management.Automation;
using DesktopManager;
using PowerBGInfo;

namespace PowerBGInfo.PowerShell;

/// <summary>Creates a BGInfo overlay image and optionally applies it as wallpaper.</summary>
/// <para>Use the script block to emit label/value entries.</para>
[Cmdlet(VerbsCommon.New, "BGInfo")]
[OutputType(typeof(string), typeof(BgInfoConfiguration))]
public class CmdletNewBGInfo : PSCmdlet {
    /// <para>Script block that outputs BGInfo entries.</para>
    [Parameter(Mandatory = true, Position = 0)]
    public ScriptBlock BGInfoContent { get; set; } = null!;

    /// <para>Optional base wallpaper file path. When omitted, current wallpaper is used.</para>
    [Parameter]
    public string FilePath { get; set; } = string.Empty;
    /// <para>Optional output file name for the generated image.</para>
    [Parameter]
    public string OutputFileName { get; set; } = string.Empty;

    /// <para>Output directory for generated BGInfo images.</para>
    [Parameter(Mandatory = true)]
    public string ConfigurationDirectory { get; set; } = string.Empty;

    /// <para>Default label font family.</para>
    [Parameter]
    public string FontFamilyName { get; set; } = "Calibri";

    /// <para>Default label color.</para>
    [Parameter]
    public object Color { get; set; } = "Black";
    /// <para>Background color to use when no wallpaper image is available.</para>
    [Parameter]
    public object? BackgroundColor { get; set; }

    /// <para>Default label font size.</para>
    [Parameter]
    public int FontSize { get; set; } = 16;

    /// <para>Render labels with a bold font weight by default.</para>
    [Parameter]
    public SwitchParameter Bold { get; set; }

    /// <para>Default numeric label font weight from 100 through 900.</para>
    [Parameter]
    [ValidateRange(100, 900)]
    public int FontWeight { get; set; } = 400;

    /// <para>Render labels with italic text by default.</para>
    [Parameter]
    public SwitchParameter Italic { get; set; }

    /// <para>Underline labels by default.</para>
    [Parameter]
    public SwitchParameter Underline { get; set; }

    /// <para>Default label underline pattern.</para>
    [Parameter]
    public TextDecorationStyle UnderlineStyle { get; set; }

    /// <para>Default label strikethrough pattern.</para>
    [Parameter]
    public TextDecorationStyle StrikethroughStyle { get; set; }

    /// <para>Default label subscript or superscript placement.</para>
    [Parameter]
    public TextBaseline Baseline { get; set; }

    /// <para>Default display-time label casing transform.</para>
    [Parameter]
    public TextCaseTransform TextCase { get; set; }

    /// <para>Default value color.</para>
    [Parameter]
    public object ValueColor { get; set; } = "Black";

    /// <para>Default value font size.</para>
    [Parameter]
    public float ValueFontSize { get; set; } = 16;

    /// <para>Default value font family.</para>
    [Parameter]
    public string ValueFontFamilyName { get; set; } = "Calibri";

    /// <para>Render values with a bold font weight by default.</para>
    [Parameter]
    public SwitchParameter ValueBold { get; set; }

    /// <para>Default numeric value font weight from 100 through 900.</para>
    [Parameter]
    [ValidateRange(100, 900)]
    public int ValueFontWeight { get; set; } = 400;

    /// <para>Render values with italic text by default.</para>
    [Parameter]
    public SwitchParameter ValueItalic { get; set; }

    /// <para>Underline values by default.</para>
    [Parameter]
    public SwitchParameter ValueUnderline { get; set; }

    /// <para>Default value underline pattern.</para>
    [Parameter]
    public TextDecorationStyle ValueUnderlineStyle { get; set; }

    /// <para>Default value strikethrough pattern.</para>
    [Parameter]
    public TextDecorationStyle ValueStrikethroughStyle { get; set; }

    /// <para>Default value subscript or superscript placement.</para>
    [Parameter]
    public TextBaseline ValueBaseline { get; set; }

    /// <para>Default display-time value casing transform.</para>
    [Parameter]
    public TextCaseTransform ValueTextCase { get; set; }

    /// <para>Maximum width used when wrapping value text. Set to 0 to disable wrapping.</para>
    [Parameter]
    public int ValueWrapWidth { get; set; }

    /// <para>Vertical spacing between rows.</para>
    [Parameter]
    public int SpaceBetweenLines { get; set; } = 10;

    /// <para>Spacing between label and value columns.</para>
    [Parameter]
    public int SpaceBetweenColumns { get; set; } = 30;

    /// <para>Legacy position X placeholder (reserved for future layout strategies).</para>
    [Parameter]
    public float PositionX { get; set; } = 10;

    /// <para>Legacy position Y placeholder (reserved for future layout strategies).</para>
    [Parameter]
    public float PositionY { get; set; } = 10;

    /// <para>Monitor index to target for wallpaper operations.</para>
    [Parameter]
    public int MonitorIndex { get; set; }

    /// <para>X padding used for layout positioning.</para>
    [Parameter]
    public int SpaceX { get; set; } = 10;

    /// <para>Y padding used for layout positioning.</para>
    [Parameter]
    public int SpaceY { get; set; } = 10;

    /// <para>Wallpaper fit mode used after generation.</para>
    [Parameter]
    public DesktopWallpaperPosition WallpaperFit { get; set; } = DesktopWallpaperPosition.Center;

    /// <para>Layout anchor position (for example TopLeft, TopCenter, BottomRight).</para>
    [Parameter]
    public BgInfoTextPosition TextPosition { get; set; } = BgInfoTextPosition.TopLeft;

    /// <para>Output target (Wallpaper, File, LogonScreen, or Both).</para>     
    [Parameter]
    public BgInfoTarget Target { get; set; } = BgInfoTarget.Wallpaper;

    /// <para>Chart layout mode.</para>
    [Parameter]
    public BgInfoChartLayoutMode ChartLayout { get; set; } = BgInfoChartLayoutMode.Manual;

    /// <para>Anchor used when stacking charts.</para>
    [Parameter]
    public BgInfoTextPosition ChartStackAnchor { get; set; } = BgInfoTextPosition.BottomLeft;

    /// <para>Direction used when stacking charts.</para>
    [Parameter]
    public BgInfoChartStackDirection ChartStackDirection { get; set; } = BgInfoChartStackDirection.Vertical;

    /// <para>Spacing between stacked charts.</para>
    [Parameter]
    public int ChartStackSpacing { get; set; } = 12;

    /// <para>Horizontal offset for stacked charts.</para>
    [Parameter]
    public int ChartStackOffsetX { get; set; } = 10;

    /// <para>Vertical offset for stacked charts.</para>
    [Parameter]
    public int ChartStackOffsetY { get; set; } = 10;

    /// <para>Align stacked charts to the text block.</para>
    [Parameter]
    public SwitchParameter ChartStackAlignToTextBlock { get; set; }

    /// <para>Place stacked charts outside the text block.</para>
    [Parameter]
    public SwitchParameter ChartStackOutsideTextBlock { get; set; }

    /// <para>Apply wallpaper for all user profiles.</para>
    [Parameter]
    public SwitchParameter AllUsers { get; set; }
    /// <para>Exclude the default user profile when applying to all users.</para>
    [Parameter]
    public SwitchParameter ExcludeDefaultUserProfile { get; set; }
    /// <para>Disable the forced wallpaper refresh after generation.</para>
    [Parameter]
    public SwitchParameter DisableWallpaperRefresh { get; set; }

    /// <para>Disable automatic preservation of the current Windows wallpaper slideshow.</para>
    [Parameter]
    public SwitchParameter DisableWallpaperSlideshow { get; set; }

    /// <para>Use screen coordinates for placement calculations.</para>
    [Parameter]
    public SwitchParameter UseScreenCoordinates { get; set; }

    /// <para>Optional path where the generated configuration JSON should be saved.</para>
    [Parameter]
    public string JsonPath { get; set; } = string.Empty;

    /// <para>Export JSON only and skip image generation/application. Requires JsonPath.</para>
    [Parameter]
    public SwitchParameter ExportOnly { get; set; }

    /// <para>Return the generated configuration object instead of rendering the image.</para>
    [Parameter]
    public SwitchParameter PassThru { get; set; }

    /// <summary>Processes the BGInfo script block and generates the image.</summary>
    protected override void ProcessRecord() {
        if (PassThru.IsPresent && ExportOnly.IsPresent) {
            ThrowTerminatingError(new ErrorRecord(
                new ArgumentException("PassThru cannot be used together with ExportOnly.", nameof(PassThru)),
                "BGInfoPassThruExportOnlyConflict",
                ErrorCategory.InvalidArgument,
                PassThru));
            return;
        }

        if (ExportOnly.IsPresent && string.IsNullOrWhiteSpace(JsonPath)) {
            ThrowTerminatingError(new ErrorRecord(
                new ArgumentException("JsonPath is required when using ExportOnly.", nameof(JsonPath)),
                "BGInfoJsonPathRequired",
                ErrorCategory.InvalidArgument,
                JsonPath));
            return;
        }

        var config = new BgInfoConfiguration
        {
            FilePath = FilePath,
            OutputFileName = OutputFileName,
            ConfigurationDirectory = ConfigurationDirectory,
            FontFamilyName = FontFamilyName,
            Color = PowerShellColorConverter.ConvertRequired(Color, nameof(Color)),
            BackgroundColor = PowerShellColorConverter.ConvertOptional(BackgroundColor, nameof(BackgroundColor)),
            FontSize = FontSize,
            FontWeight = MyInvocation.BoundParameters.ContainsKey(nameof(FontWeight)) ? PowerShellTextStyleValidator.ValidateFontWeight(FontWeight, nameof(FontWeight)) : Bold.IsPresent ? 700 : 400,
            Italic = Italic.IsPresent,
            UnderlineStyle = MyInvocation.BoundParameters.ContainsKey(nameof(UnderlineStyle)) ? UnderlineStyle : Underline.IsPresent ? TextDecorationStyle.Single : TextDecorationStyle.None,
            StrikethroughStyle = StrikethroughStyle,
            Baseline = Baseline,
            TextCase = TextCase,
            ValueColor = PowerShellColorConverter.ConvertRequired(ValueColor, nameof(ValueColor)),
            ValueFontFamilyName = ValueFontFamilyName,
            ValueFontSize = ValueFontSize,
            ValueFontWeight = MyInvocation.BoundParameters.ContainsKey(nameof(ValueFontWeight)) ? PowerShellTextStyleValidator.ValidateFontWeight(ValueFontWeight, nameof(ValueFontWeight)) : ValueBold.IsPresent ? 700 : 400,
            ValueItalic = ValueItalic.IsPresent,
            ValueUnderlineStyle = MyInvocation.BoundParameters.ContainsKey(nameof(ValueUnderlineStyle)) ? ValueUnderlineStyle : ValueUnderline.IsPresent ? TextDecorationStyle.Single : TextDecorationStyle.None,
            ValueStrikethroughStyle = ValueStrikethroughStyle,
            ValueBaseline = ValueBaseline,
            ValueTextCase = ValueTextCase,
            ValueWrapWidth = ValueWrapWidth,
            SpaceBetweenLines = SpaceBetweenLines,
            SpaceBetweenColumns = SpaceBetweenColumns,
            PositionX = PositionX,
            PositionY = PositionY,
            MonitorIndex = MonitorIndex,
            SpaceX = SpaceX,
            SpaceY = SpaceY,
            WallpaperFit = WallpaperFit,
            TextPosition = TextPosition,
            Target = Target,
            ChartLayout = ChartLayout,
            ChartStackAnchor = ChartStackAnchor,
            ChartStackDirection = ChartStackDirection,
            ChartStackSpacing = ChartStackSpacing,
            ChartStackOffsetX = ChartStackOffsetX,
            ChartStackOffsetY = ChartStackOffsetY,
            ChartStackAlignToTextBlock = ChartStackAlignToTextBlock.IsPresent,
            ChartStackOutsideTextBlock = ChartStackOutsideTextBlock.IsPresent,
            UseScreenCoordinates = UseScreenCoordinates.IsPresent,
            ForceWallpaperRefresh = !DisableWallpaperRefresh.IsPresent,
            PreserveWallpaperSlideshow = !DisableWallpaperSlideshow.IsPresent,
            ApplyToAllUsers = AllUsers.IsPresent,
            IncludeDefaultUserProfile = !ExcludeDefaultUserProfile.IsPresent
        };

        var results = BGInfoContent.Invoke();
        foreach (var item in results)
        {
            if (item?.BaseObject is BgInfoEntry entry)
            {
                config.Entries.Add(entry);
                continue;
            }

            if (item?.BaseObject is BgInfoVariable variable)
            {
                config.Variables.Add(variable);
                continue;
            }

            if (item?.BaseObject is BgInfoChart chart)
            {
                config.Charts.Add(chart);
                continue;
            }

            if (item?.BaseObject is BgInfoTopology topology)
            {
                config.Topologies.Add(topology);
                continue;
            }

            if (item?.BaseObject is BgInfoVisualCanvas visualCanvas)
            {
                config.VisualCanvases.Add(visualCanvas);
                continue;
            }

            if (item?.BaseObject is BgInfoImage image)
            {
                config.Images.Add(image);
                continue;
            }

            if (item != null && TryConvertLegacyEntry(item, out var legacyEntry))
            {
                config.Entries.Add(legacyEntry);
            }
        }

        if (!string.IsNullOrWhiteSpace(JsonPath)) {
            var fullJsonPath = SessionState.Path.GetUnresolvedProviderPathFromPSPath(JsonPath);
            var directory = Path.GetDirectoryName(fullJsonPath);
            if (!string.IsNullOrWhiteSpace(directory)) {
                Directory.CreateDirectory(directory);
            }
            BgInfoConfigurationJson.Save(config, fullJsonPath);

            if (ExportOnly.IsPresent) {
                WriteObject(fullJsonPath);
                return;
            }
        }

        if (PassThru.IsPresent) {
            WriteObject(config);
            return;
        }

        var path = BgInfoRunner.Run(config);
        WriteObject(path);
    }

    private static bool TryConvertLegacyEntry(PSObject item, out BgInfoEntry entry)
    {
        entry = null!;
        var typeValue = GetPropertyValue(item, "Type");
        if (typeValue == null)
        {
            return false;
        }

        var typeText = Convert.ToString(typeValue, CultureInfo.CurrentCulture);
        if (string.IsNullOrWhiteSpace(typeText))
        {
            return false;
        }

        var entryType = ParseEntryType(typeText);
        if (entryType == null)
        {
            return false;
        }

        var name = GetString(item, "Name");
        if (string.IsNullOrWhiteSpace(name))
        {
            return false;
        }

        var value = GetString(item, "Value") ?? string.Empty;
        entry = new BgInfoEntry
        {
            Type = entryType.Value,
            Name = name!,
            Value = entryType.Value == BgInfoEntryType.Value ? value : null,
            Color = GetColor(item, "Color"),
            FontSize = GetSingle(item, "FontSize"),
            FontFamilyName = GetString(item, "FontFamilyName"),
            Bold = GetBoolean(item, "Bold"),
            FontWeight = GetFontWeight(item, "FontWeight"),
            Italic = GetBoolean(item, "Italic"),
            Underline = GetBoolean(item, "Underline"),
            UnderlineStyle = GetEnum<TextDecorationStyle>(item, "UnderlineStyle"),
            StrikethroughStyle = GetEnum<TextDecorationStyle>(item, "StrikethroughStyle"),
            Baseline = GetEnum<TextBaseline>(item, "Baseline"),
            TextCase = GetEnum<TextCaseTransform>(item, "TextCase"),
            ValueColor = GetColor(item, "ValueColor"),
            ValueFontSize = GetSingle(item, "ValueFontSize"),
            ValueFontFamilyName = GetString(item, "ValueFontFamilyName"),
            ValueBold = GetBoolean(item, "ValueBold"),
            ValueFontWeight = GetFontWeight(item, "ValueFontWeight"),
            ValueItalic = GetBoolean(item, "ValueItalic"),
            ValueUnderline = GetBoolean(item, "ValueUnderline"),
            ValueUnderlineStyle = GetEnum<TextDecorationStyle>(item, "ValueUnderlineStyle"),
            ValueStrikethroughStyle = GetEnum<TextDecorationStyle>(item, "ValueStrikethroughStyle"),
            ValueBaseline = GetEnum<TextBaseline>(item, "ValueBaseline"),
            ValueTextCase = GetEnum<TextCaseTransform>(item, "ValueTextCase")
        };

        return true;
    }

    private static BgInfoEntryType? ParseEntryType(string typeText)
    {
        if (typeText.Equals("Label", StringComparison.OrdinalIgnoreCase))
        {
            return BgInfoEntryType.Label;
        }

        if (typeText.Equals("Values", StringComparison.OrdinalIgnoreCase) || typeText.Equals("Value", StringComparison.OrdinalIgnoreCase))
        {
            return BgInfoEntryType.Value;
        }

        return null;
    }

    private static object? GetPropertyValue(PSObject item, string name)
    {
        var property = item.Properties[name];
        if (property != null)
        {
            return property.Value;
        }

        if (item.BaseObject is IDictionary dictionary)
        {
            if (dictionary.Contains(name))
            {
                return dictionary[name];
            }

            foreach (DictionaryEntry entry in dictionary)
            {
                if (entry.Key is string key && key.Equals(name, StringComparison.OrdinalIgnoreCase))
                {
                    return entry.Value;
                }
            }
        }

        return null;
    }

    private static string? GetString(PSObject item, string name)
    {
        var value = GetPropertyValue(item, name);
        if (value == null)
        {
            return null;
        }

        var text = Convert.ToString(value, CultureInfo.CurrentCulture);
        return string.IsNullOrWhiteSpace(text) ? null : text;
    }

    private static float? GetSingle(PSObject item, string name)
    {
        return ConvertToSingle(GetPropertyValue(item, name));
    }

    private static bool? GetBoolean(PSObject item, string name)
    {
        var value = GetPropertyValue(item, name);
        if (value == null)
        {
            return null;
        }

        return LanguagePrimitives.TryConvertTo<bool>(value, out var result) ? result : null;
    }

    private static int? GetFontWeight(PSObject item, string name)
    {
        var value = GetPropertyValue(item, name);
        if (value == null || !LanguagePrimitives.TryConvertTo<int>(value, out var result)) return null;
        return PowerShellTextStyleValidator.ValidateFontWeight(result, name);
    }

    private static TEnum? GetEnum<TEnum>(PSObject item, string name) where TEnum : struct
    {
        var value = GetPropertyValue(item, name);
        if (value == null || !LanguagePrimitives.TryConvertTo<TEnum>(value, out var result) || !Enum.IsDefined(typeof(TEnum), result)) return null;
        return result;
    }

    private static float? ConvertToSingle(object? value)
    {
        if (value == null)
        {
            return null;
        }

        if (value is float floatValue)
        {
            return floatValue;
        }

        if (value is double doubleValue)
        {
            return (float)doubleValue;
        }

        if (value is decimal decimalValue)
        {
            return (float)decimalValue;
        }

        if (value is int intValue)
        {
            return intValue;
        }

        var text = Convert.ToString(value, CultureInfo.InvariantCulture);
        if (float.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out var result))
        {
            return result;
        }

        text = Convert.ToString(value, CultureInfo.CurrentCulture);
        if (float.TryParse(text, NumberStyles.Float, CultureInfo.CurrentCulture, out result))
        {
            return result;
        }

        return null;
    }

    private static Color? GetColor(PSObject item, string name)
    {
        return ConvertToColor(GetPropertyValue(item, name));
    }

    private static Color? ConvertToColor(object? value)
    {
        if (value == null)
        {
            return null;
        }

        return PowerShellColorConverter.ConvertOptional(value, "Color");
    }
}
