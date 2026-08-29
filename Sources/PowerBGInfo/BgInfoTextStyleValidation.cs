namespace PowerBGInfo;

internal static class BgInfoTextStyleValidation {
    internal static int ValidateFontWeight(int value, string parameterName) {
        if (value < 100 || value > 900) {
            throw new System.ArgumentOutOfRangeException(parameterName, value, "Font weight must be from 100 through 900.");
        }

        return value;
    }

    internal static int? ValidateFontWeight(int? value, string parameterName) =>
        value.HasValue ? ValidateFontWeight(value.Value, parameterName) : null;

    internal static T ValidateEnum<T>(T value, string parameterName) where T : struct {
        if (!System.Enum.IsDefined(typeof(T), value)) {
            throw new System.ArgumentOutOfRangeException(parameterName, value, "Unknown text formatting value.");
        }

        return value;
    }

    internal static T? ValidateEnum<T>(T? value, string parameterName) where T : struct =>
        value.HasValue ? ValidateEnum(value.Value, parameterName) : null;
}
