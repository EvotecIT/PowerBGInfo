using System.Management.Automation;
using ChartForgeX.Themes;
using PowerBGInfo;

namespace PowerBGInfo.PowerShell;

/// <summary>The theme of the New-BGInfo script block that is currently running.</summary>
/// <remarks>
/// New-BGInfo runs its script block on the pipeline thread, so a thread-local value reaches every
/// PowerBGInfo cmdlet inside it, including those in nested script blocks and helper functions.
/// </remarks>
internal static class BgInfoThemeScope {
    [ThreadStatic]
    private static BgInfoTheme? _current;

    public static BgInfoTheme? Current => _current;

    public static IDisposable Enter(BgInfoTheme? theme) {
        var previous = _current;
        _current = theme;
        return new Restore(previous);
    }

    private sealed class Restore : IDisposable {
        private readonly BgInfoTheme? _previous;
        private bool _disposed;
        public Restore(BgInfoTheme? previous) => _previous = previous;
        public void Dispose() {
            if (_disposed) return;
            _disposed = true;
            _current = _previous;
        }
    }
}

/// <summary>Accepts a theme object or a preset name such as Aurora for -Theme parameters.</summary>
public sealed class BgInfoThemeTransformationAttribute : ArgumentTransformationAttribute {
    /// <inheritdoc />
    public override object? Transform(EngineIntrinsics engineIntrinsics, object? inputData) {
        var value = inputData is PSObject wrapped ? wrapped.BaseObject : inputData;
        switch (value) {
            case null:
                return null;
            case BgInfoTheme theme:
                return theme;
            case BgInfoThemePreset preset:
                return Plain(preset);
            case string text when Enum.TryParse<BgInfoThemePreset>(text.Trim(), true, out var named) && Enum.IsDefined(typeof(BgInfoThemePreset), named):
                return Plain(named);
            default:
                throw new ArgumentTransformationMetadataException(
                    "Theme must be a theme from New-BGInfoTheme or one of: " + string.Join(", ", Enum.GetNames(typeof(BgInfoThemePreset))) + ".");
        }
    }

    private static BgInfoTheme Plain(BgInfoThemePreset preset) {
        var theme = BgInfoTheme.Create(preset);
        theme.IsPlainPreset = true;
        return theme;
    }
}

/// <summary>Tab completion for theme preset names.</summary>
public sealed class BgInfoThemeCompleter : IArgumentCompleter {
    /// <inheritdoc />
    public IEnumerable<CompletionResult> CompleteArgument(string commandName, string parameterName, string wordToComplete,
        System.Management.Automation.Language.CommandAst commandAst, System.Collections.IDictionary fakeBoundParameters) {
        foreach (var name in Enum.GetNames(typeof(BgInfoThemePreset))) {
            if (name.StartsWith(wordToComplete ?? string.Empty, StringComparison.OrdinalIgnoreCase)) {
                yield return new CompletionResult(name, name, CompletionResultType.ParameterValue, name);
            }
        }
    }
}

/// <summary>Theme helpers shared by the cmdlets.</summary>
internal static class BgInfoThemeDefaults {
    /// <summary>Status colors from the active theme, or null so renderers use their mode defaults.</summary>
    public static BgInfoStatusColors? StatusColors => BgInfoThemeScope.Current?.StatusColors;

    /// <summary>Theme mode: explicit value, then the active theme, then the fallback.</summary>
    public static VisualThemeMode Mode(bool bound, VisualThemeMode value, VisualThemeMode fallback) =>
        bound ? value : BgInfoThemeScope.Current?.Mode ?? fallback;

    /// <summary>Builds a threshold from cmdlet parameters, or null when neither threshold is bound.</summary>
    public static BgInfoThreshold? Threshold(PSCmdlet cmdlet, double warning, double critical, BgInfoThresholdDirection direction) {
        var bound = cmdlet.MyInvocation.BoundParameters;
        var hasWarning = bound.ContainsKey("WarningThreshold");
        var hasCritical = bound.ContainsKey("CriticalThreshold");
        if (!hasWarning && !hasCritical) return null;
        var threshold = new BgInfoThreshold {
            Warning = hasWarning ? warning : null,
            Critical = hasCritical ? critical : null,
            Direction = direction,
            Colors = StatusColors
        };
        threshold.Validate();
        return threshold;
    }
}
