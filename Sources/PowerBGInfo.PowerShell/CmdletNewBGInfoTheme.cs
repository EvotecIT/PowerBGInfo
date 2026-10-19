using ChartForgeX.Themes;
using PowerBGInfo;

namespace PowerBGInfo.PowerShell;

/// <summary>Creates a wallpaper theme: text, panel, status and backdrop colors plus spacing and type scale.</summary>
/// <para>Pass the result, or just a preset name, to New-BGInfo -Theme. Every cmdlet inside the script block then uses the theme for parameters you do not set.</para>
/// <example>
/// <code>
/// $theme = New-BGInfoTheme -Preset Aurora -Mode Light -Accent '#0EA5E9' -Scale 1.33
/// New-BGInfo -Theme $theme -Target File {
///     New-BGInfoBackground -Width 2560 -Height 1440
///     New-BGInfoValue -Name 'Host' -Value $env:COMPUTERNAME
///     New-BGInfoChart -Region Bottom -Title 'CPU' -Kind Area -Values 12, 30, 22
/// } -ConfigurationDirectory $PSScriptRoot -OutputFileName 'themed.png'
/// </code>
/// </example>
[Cmdlet(VerbsCommon.New, "BGInfoTheme")]
[OutputType(typeof(BgInfoTheme))]
public sealed class CmdletNewBGInfoTheme : PSCmdlet {
    /// <para>Named look: Graphite, Midnight, Aurora, Ember, Orchid or Corporate.</para>
    [Parameter(Position = 0)]
    public BgInfoThemePreset Preset { get; set; } = BgInfoThemePreset.Graphite;

    /// <para>Light or Dark variant.</para>
    [Parameter]
    [Alias("ThemeMode")]
    public VisualThemeMode Mode { get; set; } = VisualThemeMode.Dark;

    /// <para>Accent that replaces the preset accent.</para>
    [Parameter]
    public object? Accent { get; set; }

    /// <para>Second accent for contrasting series.</para>
    [Parameter]
    public object? SecondaryAccent { get; set; }

    /// <para>Multiplier for type, padding, radius and margins, from 0.5 to 4. Use about 1.33 for 2560x1440 and 2 for 3840x2160.</para>
    [Parameter]
    [ValidateRange(0.5d, 4d)]
    public double Scale { get; set; } = 1d;

    /// <para>Font family for text, panel headers and widgets.</para>
    [Parameter]
    public string FontFamilyName { get; set; } = string.Empty;

    /// <para>Panel fill that replaces the preset glass.</para>
    [Parameter]
    public object? Surface { get; set; }

    /// <para>Panel corner radius before scaling.</para>
    [Parameter]
    [ValidateRange(0d, 200d)]
    public double CornerRadius { get; set; }

    /// <summary>Emits the theme.</summary>
    protected override void EndProcessing() {
        var theme = BgInfoTheme.Create(Preset, Mode, PowerShellColorConverter.ConvertOptional(Accent, nameof(Accent)), Scale);
        var second = PowerShellColorConverter.ConvertOptional(SecondaryAccent, nameof(SecondaryAccent));
        if (second.HasValue) theme.SecondaryAccent = second.Value;
        var surface = PowerShellColorConverter.ConvertOptional(Surface, nameof(Surface));
        if (surface.HasValue) theme.Surface = surface.Value;
        if (!string.IsNullOrWhiteSpace(FontFamilyName)) theme.FontFamilyName = FontFamilyName;
        if (MyInvocation.BoundParameters.ContainsKey(nameof(CornerRadius))) theme.BaseCornerRadius = CornerRadius;
        WriteObject(theme);
    }
}
