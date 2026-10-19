using ChartForgeX.Themes;
using ChartForgeX.Typography;
using PowerBGInfo;

namespace PowerBGInfo.PowerShell;

/// <summary>Parameters shared by every widget cmdlet: panel, header, placement and theme.</summary>
public abstract class BgInfoWidgetCmdlet : PSCmdlet {
    /// <summary>Content density used when -Scale is not set. Visual blocks draw 11-12 px body text, which is small on a wallpaper.</summary>
    internal const double DefaultContentScale = 1.3d;

    /// <para>Panel header text.</para>
    [Parameter]
    public string Title { get; set; } = string.Empty;

    /// <para>Short right-aligned header note, for example a date range or a count.</para>
    [Parameter]
    public string Note { get; set; } = string.Empty;

    /// <para>Optional identifier.</para>
    [Parameter]
    public string Id { get; set; } = string.Empty;

    /// <para>Width in pixels. With -Region and no width, the widget fills the region width or shares a band.</para>
    [Parameter]
    [ValidateRange(0, 16384)]
    public int Width { get; set; }

    /// <para>Height in pixels. With -Region and no height, the widget shares the column height or takes the band height.</para>
    [Parameter]
    [ValidateRange(0, 16384)]
    public int Height { get; set; }

    /// <para>Region to flow into: Left, Right, Top, Bottom or Center.</para>
    [Parameter]
    public BgInfoRegion Region { get; set; }

    /// <para>Anchor used without -Region.</para>
    [Parameter]
    public BgInfoTextPosition Anchor { get; set; } = BgInfoTextPosition.TopRight;

    /// <para>Horizontal offset from the anchor.</para>
    [Parameter]
    public int OffsetX { get; set; } = 32;

    /// <para>Vertical offset from the anchor.</para>
    [Parameter]
    public int OffsetY { get; set; } = 32;

    /// <para>Absolute X position; use together with -PositionY.</para>
    [Parameter]
    public int PositionX { get; set; }

    /// <para>Absolute Y position; use together with -PositionX.</para>
    [Parameter]
    public int PositionY { get; set; }

    /// <para>Light or Dark content.</para>
    [Parameter]
    public VisualThemeMode ThemeMode { get; set; } = VisualThemeMode.Dark;

    /// <para>Panel fill.</para>
    [Parameter]
    public object? BackgroundColor { get; set; }

    /// <para>Panel outline.</para>
    [Parameter]
    public object? BorderColor { get; set; }

    /// <para>Panel corner radius.</para>
    [Parameter]
    [ValidateRange(0d, 200d)]
    public double CornerRadius { get; set; }

    /// <para>Space between the panel edge and its content.</para>
    [Parameter]
    [ValidateRange(0, 512)]
    public int Padding { get; set; } = 20;

    /// <para>Header color.</para>
    [Parameter]
    public object? TitleColor { get; set; }

    /// <para>Header font size.</para>
    [Parameter]
    public float TitleFontSize { get; set; }

    /// <para>Header font weight from 100 through 900.</para>
    [Parameter]
    [ValidateRange(100, 900)]
    public int TitleFontWeight { get; set; } = 600;

    /// <para>Header casing.</para>
    [Parameter]
    public TextCaseTransform TitleTextCase { get; set; }

    /// <para>Primary content color.</para>
    [Parameter]
    public object? TextColor { get; set; }

    /// <para>Secondary content color.</para>
    [Parameter]
    public object? MutedColor { get; set; }

    /// <para>Accent for series and highlights.</para>
    [Parameter]
    public object? Accent { get; set; }

    /// <para>Font family for content and header.</para>
    [Parameter]
    public string FontFamilyName { get; set; } = string.Empty;

    /// <para>Content density from 1 to 4. Larger values enlarge text and marks inside the same panel. Defaults to 1.3 times the theme scale.</para>
    [Parameter]
    [ValidateRange(1d, 4d)]
    public double Scale { get; set; }

    /// <summary>Whether a parameter was passed.</summary>
    protected bool IsBound(string name) => MyInvocation.BoundParameters.ContainsKey(name);

    /// <summary>Copies the shared parameters, then fills unbound settings from the New-BGInfo theme.</summary>
    protected T Configure<T>(T widget) where T : BgInfoWidget {
        widget.Title = Title ?? string.Empty;
        widget.Note = Note ?? string.Empty;
        widget.Id = Id ?? string.Empty;
        widget.Region = Region;
        if (IsBound(nameof(Width))) widget.Width = Width;
        else if (Region != BgInfoRegion.None) widget.Width = 0;
        if (IsBound(nameof(Height))) widget.Height = Height;
        else if (Region != BgInfoRegion.None) widget.Height = 0;
        widget.Anchor = Anchor;
        widget.OffsetX = OffsetX;
        widget.OffsetY = OffsetY;
        if (IsBound(nameof(PositionX)) && IsBound(nameof(PositionY))) {
            widget.PositionX = PositionX;
            widget.PositionY = PositionY;
        }
        widget.ThemeMode = ThemeMode;
        widget.BackgroundColor = PowerShellColorConverter.ConvertOptional(BackgroundColor, nameof(BackgroundColor));
        widget.BorderColor = PowerShellColorConverter.ConvertOptional(BorderColor, nameof(BorderColor));
        widget.CornerRadius = CornerRadius;
        widget.Padding = Padding;
        widget.TitleColor = PowerShellColorConverter.ConvertOptional(TitleColor, nameof(TitleColor));
        if (IsBound(nameof(TitleFontSize))) widget.TitleFontSize = TitleFontSize;
        widget.TitleFontWeight = TitleFontWeight;
        if (IsBound(nameof(TitleTextCase))) widget.TitleTextCase = TitleTextCase;
        widget.TextColor = PowerShellColorConverter.ConvertOptional(TextColor, nameof(TextColor));
        widget.MutedColor = PowerShellColorConverter.ConvertOptional(MutedColor, nameof(MutedColor));
        widget.Accent = PowerShellColorConverter.ConvertOptional(Accent, nameof(Accent));
        if (!string.IsNullOrWhiteSpace(FontFamilyName)) widget.FontFamilyName = FontFamilyName;

        var theme = BgInfoThemeScope.Current;
        widget.Scale = IsBound(nameof(Scale)) ? Scale : Math.Max(1d, Math.Min(4d, DefaultContentScale * (theme?.Scale ?? 1d)));
        if (theme != null) {
            if (!IsBound(nameof(ThemeMode))) widget.ThemeMode = theme.Mode;
            if (!IsBound(nameof(BackgroundColor))) widget.BackgroundColor = theme.Surface;
            if (!IsBound(nameof(BorderColor))) widget.BorderColor = theme.SurfaceBorder;
            if (!IsBound(nameof(CornerRadius))) widget.CornerRadius = theme.CornerRadius;
            if (!IsBound(nameof(Padding))) widget.Padding = theme.Padding;
            if (!IsBound(nameof(TitleColor))) widget.TitleColor = theme.Muted;
            if (!IsBound(nameof(TitleFontSize))) widget.TitleFontSize = theme.TitleFontSize;
            if (!IsBound(nameof(TitleFontWeight))) widget.TitleFontWeight = theme.TitleFontWeight;
            if (!IsBound(nameof(TitleTextCase))) widget.TitleTextCase = theme.TitleTextCase;
            if (!IsBound(nameof(TextColor))) widget.TextColor = theme.Ink;
            if (!IsBound(nameof(MutedColor))) widget.MutedColor = theme.Muted;
            if (!IsBound(nameof(Accent))) widget.Accent = theme.Accent;
            widget.SecondaryAccent = theme.SecondaryAccent;
            if (!IsBound(nameof(FontFamilyName))) widget.FontFamilyName = theme.FontFamilyName;
            widget.StatusColors = theme.StatusColors;
        }

        return widget;
    }

    /// <summary>Validates and emits the widget, turning data problems into a terminating error that names the cmdlet.</summary>
    protected void Emit(BgInfoWidget widget) {
        try {
            widget.Validate();
        } catch (Exception exception) when (exception is InvalidOperationException || exception is ArgumentException) {
            ThrowTerminatingError(new ErrorRecord(exception, "BGInfoWidgetInvalid", ErrorCategory.InvalidData, widget));
            return;
        }
        WriteObject(widget);
    }
}

/// <summary>Reads values from hashtables, PSCustomObjects and .NET objects for the widget cmdlets.</summary>
internal static class BgInfoValueReader {
    /// <summary>Reads a named property from a hashtable, PSCustomObject or .NET object.</summary>
    public static object? GetProperty(object? item, params string[] names) =>
        TryGetProperty(item, out var result, names) ? result : null;

    /// <summary>Distinguishes a missing property from a present null or malformed value.</summary>
    public static bool TryGetProperty(object? item, out object? result, params string[] names) {
        result = null;
        if (item == null) return false;
        var value = Unwrap(item);
        if (value is System.Collections.IDictionary dictionary) {
            foreach (var name in names) {
                foreach (System.Collections.DictionaryEntry entry in dictionary) {
                    if (entry.Key is string key && string.Equals(name, key, StringComparison.OrdinalIgnoreCase)) {
                        result = Unwrap(entry.Value);
                        return true;
                    }
                }
            }
            return false;
        }
        var psObject = item as PSObject ?? PSObject.AsPSObject(value);
        foreach (var name in names) {
            var property = psObject.Properties[name];
            if (property != null) {
                result = Unwrap(property.Value);
                return true;
            }
        }
        return false;
    }

    /// <summary>Returns the base object, except for PSCustomObject, whose properties live on the PSObject wrapper.</summary>
    public static object? Unwrap(object? value) => value is PSObject wrapped && wrapped.BaseObject is not PSCustomObject ? wrapped.BaseObject : value;

    public static string Text(object? value) => value == null ? string.Empty : Convert.ToString(Unwrap(value), System.Globalization.CultureInfo.CurrentCulture) ?? string.Empty;

    public static double? Number(object? value) {
        value = Unwrap(value);
        if (value == null) return null;
        if (value is string text) {
            return double.TryParse(text, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var invariant) ? invariant
                : double.TryParse(text, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.CurrentCulture, out var local) ? local : null;
        }
        return LanguagePrimitives.TryConvertTo<double>(value, out var number) ? number : null;
    }

    public static BgInfoStatus Status(object? value) {
        value = Unwrap(value);
        if (value is BgInfoStatus status) return status;
        var text = Text(value).Trim();
        if (text.Length == 0) return BgInfoStatus.None;
        if (Enum.TryParse<BgInfoStatus>(text, true, out var parsed) && Enum.IsDefined(typeof(BgInfoStatus), parsed)) return parsed;
        switch (text.ToLowerInvariant()) {
            case "healthy": case "pass": case "passed": case "running": case "online": case "success": case "good": return BgInfoStatus.Ok;
            case "warn": case "degraded": case "attention": return BgInfoStatus.Warning;
            case "error": case "failed": case "fail": case "stopped": case "down": case "offline": case "bad": return BgInfoStatus.Critical;
            default: return BgInfoStatus.Info;
        }
    }
}
