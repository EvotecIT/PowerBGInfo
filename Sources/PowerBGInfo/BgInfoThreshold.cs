using System;
using ChartForgeX.Themes;
using ChartForgeX.VisualBlocks;
using Color = ChartForgeX.Primitives.ChartColor;

namespace PowerBGInfo;

/// <summary>Semantic state of a value.</summary>
public enum BgInfoStatus {
    /// <summary>No status; the item keeps its own color.</summary>
    None,
    /// <summary>Healthy.</summary>
    Ok,
    /// <summary>Needs attention.</summary>
    Warning,
    /// <summary>Needs action now.</summary>
    Critical,
    /// <summary>Informational.</summary>
    Info
}

/// <summary>Which side of a threshold is worse.</summary>
public enum BgInfoThresholdDirection {
    /// <summary>Values at or above the threshold are worse, for example CPU or disk used.</summary>
    Above,
    /// <summary>Values at or below the threshold are worse, for example free space or battery.</summary>
    Below
}

/// <summary>Colors used for each status.</summary>
public sealed class BgInfoStatusColors {
    /// <summary>Healthy color.</summary>
    public Color Ok { get; set; }
    /// <summary>Warning color.</summary>
    public Color Warning { get; set; }
    /// <summary>Critical color.</summary>
    public Color Critical { get; set; }
    /// <summary>Informational color.</summary>
    public Color Info { get; set; }

    /// <summary>Default status colors tuned for a light or dark backdrop.</summary>
    public static BgInfoStatusColors Default(VisualThemeMode mode) {
        var dark = mode == VisualThemeMode.Dark;
        return new BgInfoStatusColors {
            Ok = Color.FromHex(dark ? "#34D399" : "#059669"),
            Warning = Color.FromHex(dark ? "#FBBF24" : "#D97706"),
            Critical = Color.FromHex(dark ? "#F87171" : "#DC2626"),
            Info = Color.FromHex(dark ? "#60A5FA" : "#2563EB")
        };
    }

    /// <summary>Returns the color for a status, or null for <see cref="BgInfoStatus.None"/>.</summary>
    public Color? Resolve(BgInfoStatus status) => status switch {
        BgInfoStatus.Ok => Ok,
        BgInfoStatus.Warning => Warning,
        BgInfoStatus.Critical => Critical,
        BgInfoStatus.Info => Info,
        _ => null
    };

    internal BgInfoStatusColors Clone() => new() { Ok = Ok, Warning = Warning, Critical = Critical, Info = Info };
}

/// <summary>Warning and critical thresholds that turn a value amber or red.</summary>
public sealed class BgInfoThreshold {
    /// <summary>Value at which the status becomes <see cref="BgInfoStatus.Warning"/>.</summary>
    public double? Warning { get; set; }
    /// <summary>Value at which the status becomes <see cref="BgInfoStatus.Critical"/>.</summary>
    public double? Critical { get; set; }
    /// <summary>Which side of the thresholds is worse.</summary>
    public BgInfoThresholdDirection Direction { get; set; } = BgInfoThresholdDirection.Above;
    /// <summary>Optional colors. Unset colors use the theme or the defaults for the item's mode.</summary>
    public BgInfoStatusColors? Colors { get; set; }

    /// <summary>Whether at least one threshold is set.</summary>
    public bool IsDefined => Warning.HasValue || Critical.HasValue;

    /// <summary>Evaluates a value. Values that cross no threshold are <see cref="BgInfoStatus.Ok"/>.</summary>
    public BgInfoStatus Evaluate(double value) {
        if (!IsDefined || double.IsNaN(value)) return BgInfoStatus.None;
        if (Critical.HasValue && Crosses(value, Critical.Value)) return BgInfoStatus.Critical;
        if (Warning.HasValue && Crosses(value, Warning.Value)) return BgInfoStatus.Warning;
        return BgInfoStatus.Ok;
    }

    /// <summary>Evaluates a value and returns its status color, or null when no threshold is set.</summary>
    public Color? ResolveColor(double value, VisualThemeMode mode) {
        var status = Evaluate(value);
        return (Colors ?? BgInfoStatusColors.Default(mode)).Resolve(status);
    }

    /// <summary>Validates that warning and critical are ordered for the direction.</summary>
    public void Validate() {
        if (!Enum.IsDefined(typeof(BgInfoThresholdDirection), Direction)) throw new ArgumentOutOfRangeException(nameof(Direction), Direction, "Threshold direction must be Above or Below.");
        if (Warning.HasValue && !IsFinite(Warning.Value)) throw new ArgumentOutOfRangeException(nameof(Warning), Warning, "Warning threshold must be finite.");
        if (Critical.HasValue && !IsFinite(Critical.Value)) throw new ArgumentOutOfRangeException(nameof(Critical), Critical, "Critical threshold must be finite.");
        if (!Warning.HasValue || !Critical.HasValue) return;
        var ordered = Direction == BgInfoThresholdDirection.Above ? Critical.Value >= Warning.Value : Critical.Value <= Warning.Value;
        if (!ordered) {
            throw new ArgumentException(Direction == BgInfoThresholdDirection.Above
                ? "The critical threshold must be at or above the warning threshold when higher values are worse."
                : "The critical threshold must be at or below the warning threshold when lower values are worse.");
        }
    }

    internal BgInfoThreshold Clone() => new() { Warning = Warning, Critical = Critical, Direction = Direction, Colors = Colors?.Clone() };

    internal static VisualStatus ToVisualStatus(BgInfoStatus status) => status switch {
        BgInfoStatus.Ok => VisualStatus.Positive,
        BgInfoStatus.Warning => VisualStatus.Warning,
        BgInfoStatus.Critical => VisualStatus.Negative,
        BgInfoStatus.Info => VisualStatus.Info,
        _ => VisualStatus.None
    };

    private bool Crosses(double value, double threshold) =>
        Direction == BgInfoThresholdDirection.Above ? value >= threshold : value <= threshold;

    private static bool IsFinite(double value) => !double.IsNaN(value) && !double.IsInfinity(value);
}
