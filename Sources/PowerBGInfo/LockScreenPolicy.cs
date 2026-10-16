using Microsoft.Win32;
using DesktopManager;

namespace PowerBGInfo;

/// <summary>
/// Describes the policy fallback DesktopManager reports when its per-user lock screen operation fails.
/// It writes the value of the "Force a specific default lock screen and logon image" policy.
/// Windows applies that policy only on Enterprise, Education and Server editions.
/// </summary>
internal static class LockScreenPolicy {
    public const string KeyPath = @"SOFTWARE\Policies\Microsoft\Windows\Personalization";
    public const string ValueName = "LockScreenImage";

    private static readonly string[] PolicyEditionPrefixes = { "Enterprise", "Education", "Server", "IoTEnterprise" };

    /// <summary>Reads edition policy limits only when DesktopManager reports a completed machine policy write.</summary>
    internal static string? GetFallbackWarning(LogonWallpaperMechanism mechanism, Func<string?> readEditionId) {
        return mechanism == LogonWallpaperMechanism.PersonalizationPolicy
            ? GetEditionWarning(readEditionId())
            : null;
    }

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
        return "The per-user Windows lock screen API did not complete, so PowerBGInfo wrote the image path to "
            + $@"HKLM\{KeyPath}\{ValueName}, the value of the policy 'Force a specific default lock screen and logon image'. "
            + $"Windows applies that policy only on Enterprise, Education and Server editions, and this is {edition}, so the lock screen "
            + "probably does not change. Run in the intended user's session and check that account's profile and applicable policies, "
            + "or render with -Target File and deliver the image with your management tool.";
    }
}
