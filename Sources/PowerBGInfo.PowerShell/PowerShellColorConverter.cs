using System;
using System.Collections;
using System.Collections.Generic;
using Color = ChartForgeX.Primitives.ChartColor;
using System.Management.Automation;
using ChartForgeX.Primitives;
using PowerBGInfo;

namespace PowerBGInfo.PowerShell;

/// <summary>Converts PowerShell-friendly color values into BGInfo colors.</summary>
internal static class PowerShellColorConverter {
    public static Color ConvertRequired(object? value, string parameterName) {
        var color = ConvertOptional(value, parameterName);
        if (!color.HasValue) {
            throw new ArgumentException("Color must be a ChartForgeX color name, token, or hex string (#RGB, #RGBA, #RRGGBB, or #RRGGBBAA).", parameterName);
        }

        return color.Value;
    }

    public static Color? ConvertOptional(object? value, string parameterName) {
        value = Unwrap(value);
        if (value == null) {
            return null;
        }

        if (value is ChartColor chartColor) {
            return chartColor;
        }

        if (value is string text && BgInfoColorParser.TryParse(text, out var parsed)) {
            return parsed;
        }

        throw new ArgumentException("Color must be a ChartForgeX color name, token, or hex string (#RGB, #RGBA, #RRGGBB, or #RRGGBBAA).", parameterName);
    }

    public static IReadOnlyList<Color> ConvertPalette(object[]? values, string parameterName) {
        if (values == null || values.Length == 0) {
            return Array.Empty<Color>();
        }

        var colors = new List<Color>();
        foreach (var value in values) {
            foreach (var item in Expand(value)) {
                colors.Add(ConvertRequired(item, parameterName));
            }
        }

        return colors;
    }

    private static IEnumerable<object?> Expand(object? value) {
        value = Unwrap(value);
        if (value == null || value is string || value is ChartColor) {
            yield return value;
            yield break;
        }

        if (value is IEnumerable enumerable) {
            foreach (var item in enumerable) {
                yield return Unwrap(item);
            }
            yield break;
        }

        yield return value;
    }

    private static object? Unwrap(object? value) => value is PSObject psObject ? psObject.BaseObject : value;
}
