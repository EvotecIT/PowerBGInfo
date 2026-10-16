using Microsoft.Win32;

namespace PowerBGInfo;

/// <summary>
/// The fallback DesktopManager uses for the lock screen when the Windows lock screen API is not reachable, which is always the
/// case under PowerShell 7: it writes the value of the "Force a specific default lock screen and logon image" policy.
/// Windows applies that policy only on Enterprise, Education and Server editions.
/// </summary>
internal static class LockScreenPolicy {
    public const string KeyPath = @"SOFTWARE\Policies\Microsoft\Windows\Personalization";
    public const string ValueName = "LockScreenImage";

    private static readonly string[] PolicyEditionPrefixes = { "Enterprise", "Education", "Server", "IoTEnterprise" };

    /// <summary>True when the EditionID is one the lock screen policy applies to. Multi-session hosts report ServerRdsh.</summary>
    public static bool IsPolicyEdition(string? editionId) {
        if (string.IsNullOrWhiteSpace(editionId)) return false;
        var edition = editionId!.Trim();
        return PolicyEditionPrefixes.Any(prefix => edition.StartsWith(prefix, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>Reads EditionID, for example Professional or Enterprise; null when it cannot be read.</summary>
    public static string? ReadEditionId() {
        try {
            using var key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion");
            return key?.GetValue("EditionID") as string;
        } catch {
            return null;
        }
    }

    /// <summary>The warning for the fallback on an edition the policy does not apply to; null when the policy applies.</summary>
    public static string? GetEditionWarning(string? editionId) {
        if (IsPolicyEdition(editionId)) return null;
        var edition = string.IsNullOrWhiteSpace(editionId) ? "an edition that could not be read" : $"'{editionId!.Trim()}'";
        return "The Windows lock screen API is not available in this PowerShell host, so PowerBGInfo wrote the image path to "
            + $@"HKLM\{KeyPath}\{ValueName}, the value of the policy 'Force a specific default lock screen and logon image'. "
            + $"Windows applies that policy only on Enterprise, Education and Server editions, and this is {edition}, so the lock screen "
            + "probably does not change. Run -Target LogonScreen from Windows PowerShell 5.1, which uses the lock screen API, or render "
            + "with -Target File and deliver the image with your management tool.";
    }

    /// <summary>
    /// True when the WinRT lock screen API can be reached from this process, which is the check DesktopManager makes before it
    /// falls back to the policy value. Windows PowerShell 5.1 (.NET Framework) can; PowerShell 7 (.NET 8) cannot.
    /// </summary>
    public static bool IsLockScreenApiAvailable() {
        try {
            return Type.GetType("Windows.System.UserProfile.LockScreen, Windows, ContentType=WindowsRuntime") != null
                && Type.GetType("Windows.Storage.StorageFile, Windows, ContentType=WindowsRuntime") != null;
        } catch {
            return false;
        }
    }
}
