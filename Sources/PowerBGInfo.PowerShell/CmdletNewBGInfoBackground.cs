using PowerBGInfo;

namespace PowerBGInfo.PowerShell;

/// <summary>Creates a generated backdrop that replaces the wallpaper image.</summary>
/// <para>Emit the result inside the New-BGInfo script block. Layers paint in order: color, image, gradient, glows, pattern.</para>
/// <example>
/// <code>
/// New-BGInfo -Target File {
///     New-BGInfoBackground -Width 1920 -Height 1080 -Gradient '#0B1022', '#05070F' -Glow @(
///         New-BGInfoBackgroundGlow -X 0.15 -Y 0.1 -Radius 0.55 -Color '#4F46E566'
///     ) -Pattern Dots
///     New-BGInfoValue -Name 'Host' -Value $env:COMPUTERNAME
/// } -ConfigurationDirectory $PSScriptRoot -OutputFileName 'Generated.png'
/// </code>
/// </example>
[Cmdlet(VerbsCommon.New, "BGInfoBackground")]
[OutputType(typeof(BgInfoBackground))]
public sealed class CmdletNewBGInfoBackground : PSCmdlet {
    /// <para>Output width in pixels. Zero uses the target monitor width.</para>
    [Parameter]
    [ValidateRange(0, 16384)]
    public int Width { get; set; }

    /// <para>Output height in pixels. Zero uses the target monitor height.</para>
    [Parameter]
    [ValidateRange(0, 16384)]
    public int Height { get; set; }

    /// <para>Base fill painted before every other layer. Defaults to black.</para>
    [Parameter]
    public object? Color { get; set; }

    /// <para>Two gradient colors: start and end. Translucent colors tint an image underneath.</para>
    [Parameter]
    [ValidateCount(2, 2)]
    public object[] Gradient { get; set; } = Array.Empty<object>();

    /// <para>Gradient direction in degrees clockwise from the rightward horizontal axis. 0 paints left to right; 90 paints top to bottom.</para>
    [Parameter]
    public double GradientAngle { get; set; } = 135d;

    /// <para>Optional image painted over the base color, for example a company wallpaper.</para>
    [Parameter]
    public string ImagePath { get; set; } = string.Empty;

    /// <para>How the image fills the backdrop. Cover fills the whole area and crops the overflow.</para>
    [Parameter]
    public BgInfoImageFit ImageFit { get; set; } = BgInfoImageFit.Cover;

    /// <para>Image opacity from zero to one.</para>
    [Parameter]
    [ValidateRange(0d, 1d)]
    public double ImageOpacity { get; set; } = 1d;

    /// <para>Soft radial lights created with New-BGInfoBackgroundGlow.</para>
    [Parameter]
    public BgInfoBackgroundGlow[] Glow { get; set; } = Array.Empty<BgInfoBackgroundGlow>();

    /// <para>Optional repeating pattern painted last.</para>
    [Parameter]
    public BgInfoBackgroundPattern Pattern { get; set; }

    /// <para>Pattern ink. Keep the alpha low so the pattern stays behind the content.</para>
    [Parameter]
    public object? PatternColor { get; set; }

    /// <para>Distance between pattern dots or lines in pixels.</para>
    [Parameter]
    [ValidateRange(8, 1024)]
    public int PatternSpacing { get; set; } = 48;

    /// <summary>Emits the background definition.</summary>
    protected override void EndProcessing() {
        var background = new BgInfoBackground {
            Width = Width,
            Height = Height,
            GradientAngle = GradientAngle,
            ImageFit = ImageFit,
            ImageOpacity = ImageOpacity,
            Pattern = Pattern,
            PatternSpacing = PatternSpacing
        };

        if (MyInvocation.BoundParameters.ContainsKey(nameof(Color))) {
            background.Color = PowerShellColorConverter.ConvertRequired(Color, nameof(Color));
        }
        if (Gradient.Length == 2) {
            background.GradientStart = PowerShellColorConverter.ConvertRequired(Gradient[0], nameof(Gradient));
            background.GradientEnd = PowerShellColorConverter.ConvertRequired(Gradient[1], nameof(Gradient));
        }
        if (!string.IsNullOrWhiteSpace(ImagePath)) {
            background.ImagePath = SessionState.Path.GetUnresolvedProviderPathFromPSPath(ImagePath);
        }
        if (MyInvocation.BoundParameters.ContainsKey(nameof(PatternColor))) {
            background.PatternColor = PowerShellColorConverter.ConvertRequired(PatternColor, nameof(PatternColor));
        }
        foreach (var glow in Glow) {
            if (glow != null) background.Glows.Add(glow);
        }

        WriteObject(background);
    }
}
