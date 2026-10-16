using System;
using System.Collections.Generic;
using ChartForgeX.Themes;
using ChartForgeX.Typography;
using Color = ChartForgeX.Primitives.ChartColor;

namespace PowerBGInfo;

/// <summary>Named wallpaper looks. Every preset has a light and a dark variant.</summary>
public enum BgInfoThemePreset {
    /// <summary>Neutral slate with a blue accent and a faint grid.</summary>
    Graphite,
    /// <summary>Deep indigo for dense administrator screens.</summary>
    Midnight,
    /// <summary>Teal glows on a dark ocean backdrop, suited to server status.</summary>
    Aurora,
    /// <summary>Warm orange and pink, suited to classrooms and kiosks.</summary>
    Ember,
    /// <summary>Violet with dots, suited to help-desk and front-desk screens.</summary>
    Orchid,
    /// <summary>Calm sky blue without a pattern, suited to corporate desktops.</summary>
    Corporate
}

/// <summary>
/// A resolved wallpaper look: text, panel, status and backdrop colors plus spacing and type scale.
/// </summary>
/// <remarks>
/// Pass a theme to <c>New-BGInfo -Theme</c>. Cmdlets inside the script block use it for every parameter the
/// caller does not bind. The theme resolves into ordinary chart, topology, canvas and widget properties.
/// </remarks>
public sealed class BgInfoTheme {
    private double _scale = 1d;

    /// <summary>True while the theme is exactly a preset, so its light or dark variant can be swapped.</summary>
    internal bool IsPlainPreset { get; set; }

    /// <summary>The preset this theme was created from.</summary>
    public BgInfoThemePreset Preset { get; set; }
    /// <summary>Light or dark variant.</summary>
    public VisualThemeMode Mode { get; set; } = VisualThemeMode.Dark;
    /// <summary>Multiplier for type, padding, radius and margins. Use larger values on high-DPI screens.</summary>
    public double Scale {
        get => _scale;
        set {
            if (double.IsNaN(value) || double.IsInfinity(value) || value < 0.5 || value > 4) throw new ArgumentOutOfRangeException(nameof(value), value, "Theme scale must be between 0.5 and 4.");
            _scale = value;
        }
    }

    /// <summary>Primary text color.</summary>
    public Color Ink { get; set; }
    /// <summary>Secondary text color for labels, captions and panel headers.</summary>
    public Color Muted { get; set; }
    /// <summary>Accent used for series, tiles and highlighted labels.</summary>
    public Color Accent { get; set; }
    /// <summary>Second accent used for contrasting series.</summary>
    public Color SecondaryAccent { get; set; }
    /// <summary>Healthy status color.</summary>
    public Color Ok { get; set; }
    /// <summary>Warning status color.</summary>
    public Color Warning { get; set; }
    /// <summary>Critical status color.</summary>
    public Color Critical { get; set; }
    /// <summary>Informational status color.</summary>
    public Color Info { get; set; }

    /// <summary>Panel fill behind charts, diagrams and widgets.</summary>
    public Color Surface { get; set; }
    /// <summary>One-pixel panel outline.</summary>
    public Color SurfaceBorder { get; set; }
    /// <summary>Grid and track ink inside panels.</summary>
    public Color GridColor { get; set; }
    /// <summary>Upper glass color of visual canvas tiles.</summary>
    public Color TileGlassTop { get; set; }
    /// <summary>Lower glass color of visual canvas tiles.</summary>
    public Color TileGlassBottom { get; set; }

    /// <summary>Font family for text, panel headers and widgets.</summary>
    public string FontFamilyName { get; set; } = "Segoe UI";
    /// <summary>Unscaled label and value size of the text block.</summary>
    public float BaseFontSize { get; set; } = 17;
    /// <summary>Unscaled panel header size.</summary>
    public float BaseTitleFontSize { get; set; } = 15;
    /// <summary>Panel header weight.</summary>
    public int TitleFontWeight { get; set; } = 600;
    /// <summary>Panel header casing.</summary>
    public TextCaseTransform TitleTextCase { get; set; } = TextCaseTransform.Uppercase;
    /// <summary>Unscaled panel corner radius.</summary>
    public double BaseCornerRadius { get; set; } = 18;
    /// <summary>Unscaled panel padding.</summary>
    public int BasePadding { get; set; } = 20;
    /// <summary>Unscaled horizontal distance from the screen edge.</summary>
    public int BaseMarginX { get; set; } = 64;
    /// <summary>Unscaled vertical distance from the screen edge.</summary>
    public int BaseMarginY { get; set; } = 56;
    /// <summary>Unscaled gap between panels.</summary>
    public int BaseGap { get; set; } = 24;
    /// <summary>Unscaled visual canvas tile height.</summary>
    public int BaseTileHeight { get; set; } = 116;

    /// <summary>Backdrop base fill.</summary>
    public Color BackdropColor { get; set; }
    /// <summary>Backdrop gradient start.</summary>
    public Color BackdropGradientStart { get; set; }
    /// <summary>Backdrop gradient end.</summary>
    public Color BackdropGradientEnd { get; set; }
    /// <summary>Backdrop gradient angle in degrees.</summary>
    public double BackdropGradientAngle { get; set; } = 135;
    /// <summary>Backdrop pattern.</summary>
    public BgInfoBackgroundPattern BackdropPattern { get; set; }
    /// <summary>Backdrop pattern ink.</summary>
    public Color BackdropPatternColor { get; set; } = Color.Transparent;
    /// <summary>Unscaled pattern spacing.</summary>
    public int BackdropPatternSpacing { get; set; } = 48;
    /// <summary>Soft lights painted on the backdrop.</summary>
    public List<BgInfoBackgroundGlow> BackdropGlows { get; } = new();

    /// <summary>Scaled label and value size.</summary>
    public float FontSize => (float)Math.Round(BaseFontSize * Scale, 1);
    /// <summary>Scaled panel header size.</summary>
    public float TitleFontSize => (float)Math.Round(BaseTitleFontSize * Scale, 1);
    /// <summary>Scaled corner radius.</summary>
    public double CornerRadius => Math.Round(BaseCornerRadius * Scale, 1);
    /// <summary>Scaled panel padding.</summary>
    public int Padding => ScaleInt(BasePadding);
    /// <summary>Scaled horizontal margin.</summary>
    public int MarginX => ScaleInt(BaseMarginX);
    /// <summary>Scaled vertical margin.</summary>
    public int MarginY => ScaleInt(BaseMarginY);
    /// <summary>Scaled panel gap.</summary>
    public int Gap => ScaleInt(BaseGap);
    /// <summary>Scaled tile height.</summary>
    public int TileHeight => ScaleInt(BaseTileHeight);
    /// <summary>Scaled gap between visual canvas tiles.</summary>
    public int TileGap => ScaleInt(16);
    /// <summary>Scaled space between text rows.</summary>
    public int LineSpacing => ScaleInt(8);
    /// <summary>Scaled space between label and value columns.</summary>
    public int ColumnSpacing => ScaleInt(28);

    /// <summary>Status colors for thresholds.</summary>
    public BgInfoStatusColors StatusColors => new() { Ok = Ok, Warning = Warning, Critical = Critical, Info = Info };

    /// <summary>Creates a theme from a preset.</summary>
    /// <param name="preset">Named look.</param>
    /// <param name="mode">Light or dark variant.</param>
    /// <param name="accent">Optional accent that replaces the preset accent.</param>
    /// <param name="scale">Multiplier for type and spacing.</param>
    public static BgInfoTheme Create(BgInfoThemePreset preset, VisualThemeMode mode = VisualThemeMode.Dark, Color? accent = null, double scale = 1d) {
        if (!Enum.IsDefined(typeof(BgInfoThemePreset), preset)) throw new ArgumentOutOfRangeException(nameof(preset), preset, "Unknown theme preset.");
        if (!Enum.IsDefined(typeof(VisualThemeMode), mode)) throw new ArgumentOutOfRangeException(nameof(mode), mode, "Unknown theme mode.");
        var dark = mode == VisualThemeMode.Dark;
        var theme = new BgInfoTheme {
            Preset = preset,
            Mode = mode,
            Scale = scale,
            Ink = Hex(dark ? "#F1F5F9" : "#0F172A"),
            Muted = Hex(dark ? "#94A3B8" : "#475569"),
            Ok = Hex(dark ? "#34D399" : "#059669"),
            Warning = Hex(dark ? "#FBBF24" : "#D97706"),
            Critical = Hex(dark ? "#F87171" : "#DC2626"),
            Info = Hex(dark ? "#60A5FA" : "#2563EB"),
            Surface = Hex(dark ? "#0B1222C2" : "#FFFFFFC7"),
            SurfaceBorder = Hex(dark ? "#FFFFFF1F" : "#0F172A1A"),
            GridColor = Hex(dark ? "#FFFFFF33" : "#0F172A26"),
            TileGlassTop = Hex(dark ? "#16213ACC" : "#FFFFFFEB"),
            TileGlassBottom = Hex(dark ? "#0B1222E6" : "#F1F5F9E0")
        };

        BgInfoThemePresets.Apply(theme, preset, dark);
        if (accent.HasValue) theme.Accent = accent.Value;
        return theme;
    }

    /// <summary>Creates the theme backdrop at the given size. Zero uses the monitor size.</summary>
    public BgInfoBackground CreateBackground(int width = 0, int height = 0) {
        var background = new BgInfoBackground {
            Width = width,
            Height = height,
            Color = BackdropColor,
            GradientStart = BackdropGradientStart,
            GradientEnd = BackdropGradientEnd,
            GradientAngle = BackdropGradientAngle,
            Pattern = BackdropPattern,
            PatternColor = BackdropPatternColor,
            PatternSpacing = Math.Max(8, ScaleInt(BackdropPatternSpacing))
        };
        foreach (var glow in BackdropGlows) {
            background.Glows.Add(new BgInfoBackgroundGlow { X = glow.X, Y = glow.Y, Radius = glow.Radius, Color = glow.Color });
        }
        return background;
    }

    /// <summary>Creates the default layout for this theme: its margins and gap.</summary>
    public BgInfoLayout CreateLayout() => new() { MarginX = MarginX, MarginY = MarginY, Gap = Gap };

    private int ScaleInt(int value) => (int)Math.Round(value * Scale);

    internal static Color Hex(string value) => Color.FromHex(value);
}

internal static class BgInfoThemePresets {
    public static void Apply(BgInfoTheme theme, BgInfoThemePreset preset, bool dark) {
        switch (preset) {
            case BgInfoThemePreset.Midnight:
                Accent(theme, dark, "#818CF8", "#4F46E5", "#F472B6", "#DB2777");
                Backdrop(theme, dark, "#05060B", "#0D1020", "#05060B", "#F3F4F8", "#FAFAFD", "#E4E7F1", 100);
                Pattern(theme, BgInfoBackgroundPattern.Grid, dark ? "#FFFFFF08" : "#1E1B4B0C", 32);
                Glow(theme, 0.65, 0.25, 0.6, dark ? "#4F46E544" : "#A5B4FC80");
                Glow(theme, 1, 0, 0.3, dark ? "#DC262622" : "#FCA5A555");
                break;
            case BgInfoThemePreset.Aurora:
                Accent(theme, dark, "#2DD4BF", "#0D9488", "#60A5FA", "#2563EB");
                Backdrop(theme, dark, "#04101A", "#062A33", "#050A14", "#EAF4F4", "#F7FBFB", "#DCEBEF", 135);
                Pattern(theme, BgInfoBackgroundPattern.Grid, dark ? "#FFFFFF0A" : "#0F172A0D", 64);
                Glow(theme, 0.9, 0.05, 0.55, dark ? "#14B8A655" : "#5EEAD480");
                Glow(theme, 0.05, 1, 0.5, dark ? "#14B8A655" : "#5EEAD480");
                break;
            case BgInfoThemePreset.Ember:
                Accent(theme, dark, "#FB923C", "#EA580C", "#F472B6", "#DB2777");
                Backdrop(theme, dark, "#120A0A", "#2A1208", "#0B0710", "#FFF7F0", "#FFFBF5", "#FFE4E1", 120);
                Glow(theme, 0.1, 0.1, 0.5, dark ? "#F9731655" : "#FDBA7499");
                Glow(theme, 0.95, 0.3, 0.45, dark ? "#DB277740" : "#F9A8D488");
                Glow(theme, 0.5, 1.1, 0.6, dark ? "#7C3AED33" : "#C4B5FD66");
                break;
            case BgInfoThemePreset.Orchid:
                Accent(theme, dark, "#A78BFA", "#7C3AED", "#F472B6", "#DB2777");
                Backdrop(theme, dark, "#0E0A1A", "#1E1438", "#0A0712", "#FBF7FF", "#FFFFFF", "#F1E9FF", 160);
                Pattern(theme, BgInfoBackgroundPattern.Dots, dark ? "#FFFFFF12" : "#4C1D9512", 36);
                Glow(theme, 0, 0, 0.6, dark ? "#7C3AED55" : "#C4B5FD99");
                Glow(theme, 0.75, 1.05, 0.55, dark ? "#EC489940" : "#FBCFE899");
                break;
            case BgInfoThemePreset.Corporate:
                Accent(theme, dark, "#38BDF8", "#0369A1", "#A3E635", "#4D7C0F");
                Backdrop(theme, dark, "#0A1626", "#0F2238", "#081120", "#F5F8FC", "#FFFFFF", "#E8EEF6", 135);
                Glow(theme, 0.8, 0.15, 0.6, dark ? "#0EA5E933" : "#BAE6FD80");
                break;
            default:
                Accent(theme, dark, "#60A5FA", "#2563EB", "#2DD4BF", "#0D9488");
                Backdrop(theme, dark, "#0B0F17", "#141A26", "#07090E", "#F4F6FA", "#FFFFFF", "#E6EAF1", 135);
                Pattern(theme, BgInfoBackgroundPattern.Grid, dark ? "#FFFFFF08" : "#0F172A0A", 64);
                Glow(theme, 0.85, 0.1, 0.55, dark ? "#3B82F633" : "#93C5FD66");
                break;
        }
    }

    private static void Accent(BgInfoTheme theme, bool dark, string darkAccent, string lightAccent, string darkSecond, string lightSecond) {
        theme.Accent = BgInfoTheme.Hex(dark ? darkAccent : lightAccent);
        theme.SecondaryAccent = BgInfoTheme.Hex(dark ? darkSecond : lightSecond);
    }

    private static void Backdrop(BgInfoTheme theme, bool dark, string darkBase, string darkStart, string darkEnd, string lightBase, string lightStart, string lightEnd, double angle) {
        theme.BackdropColor = BgInfoTheme.Hex(dark ? darkBase : lightBase);
        theme.BackdropGradientStart = BgInfoTheme.Hex(dark ? darkStart : lightStart);
        theme.BackdropGradientEnd = BgInfoTheme.Hex(dark ? darkEnd : lightEnd);
        theme.BackdropGradientAngle = angle;
    }

    private static void Pattern(BgInfoTheme theme, BgInfoBackgroundPattern pattern, string color, int spacing) {
        theme.BackdropPattern = pattern;
        theme.BackdropPatternColor = BgInfoTheme.Hex(color);
        theme.BackdropPatternSpacing = spacing;
    }

    private static void Glow(BgInfoTheme theme, double x, double y, double radius, string color) =>
        theme.BackdropGlows.Add(new BgInfoBackgroundGlow { X = x, Y = y, Radius = radius, Color = BgInfoTheme.Hex(color) });
}
