using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Management.Automation;
using PowerBGInfo;

namespace PowerBGInfo.PowerShell;

/// <summary>Converts PowerShell-friendly color values into BGInfo colors.</summary>
internal static class PowerShellColorConverter {
    public static Color ConvertRequired(object? value, string parameterName) {
        var color = ConvertOptional(value, parameterName);
        if (!color.HasValue) {
            throw new ArgumentException("Color must be a System.Drawing.Color, known color name, #RRGGBB, #AARRGGBB, RGB integer, or ARGB integer.", parameterName);
        }

        return color.Value;
    }

    public static Color? ConvertOptional(object? value, string parameterName) {
        value = Unwrap(value);
        if (value == null) {
            return null;
        }

        if (value is Color color) {
            return color;
        }

        if (value is int argb) {
            return Color.FromArgb(argb);
        }

        if (value is uint argbUnsigned) {
            return Color.FromArgb(unchecked((int)argbUnsigned));
        }

        var text = value as string ?? System.Convert.ToString(value, CultureInfo.InvariantCulture);
        if (!string.IsNullOrWhiteSpace(text) && BgInfoColorParser.TryParse(text, out var parsed)) {
            return parsed;
        }

        throw new ArgumentException("Color must be a System.Drawing.Color, known color name, #RRGGBB, #AARRGGBB, RGB integer, or ARGB integer.", parameterName);
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
        if (value == null || value is string || value is Color) {
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
