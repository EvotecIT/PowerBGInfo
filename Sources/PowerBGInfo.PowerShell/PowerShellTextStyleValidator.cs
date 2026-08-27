using System;

namespace PowerBGInfo.PowerShell;

internal static class PowerShellTextStyleValidator {
    internal static int ValidateFontWeight(int value, string parameterName) {
        if (value < 100 || value > 900) {
            throw new ArgumentOutOfRangeException(parameterName, value, "Font weight must be from 100 through 900.");
        }
        return value;
    }
}
