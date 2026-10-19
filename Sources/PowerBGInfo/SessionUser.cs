using DesktopManager;
using System.Diagnostics;

namespace PowerBGInfo;

/// <summary>
/// Resolves the signed-in session user through DesktopManager, with PowerBGInfo's WMI fallback and display placeholder.
/// </summary>
internal static class SessionUser {
    /// <summary>Reads the session account, then the console user from WMI in session zero; otherwise returns Unknown.</summary>
    /// <param name="consoleUserFromWmi">Last resort, for example Win32_ComputerSystem.UserName.</param>
    public static string Resolve(Func<string?> consoleUserFromWmi) {
        var sessionId = -1;
        try {
            using var process = Process.GetCurrentProcess();
            sessionId = process.SessionId;
        } catch { /* An unreadable session must not fall back to another user's account. */ }
        return Resolve(() => new DesktopSessionService().GetSessionUser()?.AccountName, consoleUserFromWmi, sessionId);
    }

    /// <summary>Applies source precedence and presentation at the session and WMI read boundaries.</summary>
    internal static string Resolve(Func<string?> sessionUser, Func<string?>? consoleUserFromWmi, int processSessionId = 0) {
        var user = Try(sessionUser);
        if (!string.IsNullOrWhiteSpace(user)) {
            return user!;
        }

        if (processSessionId != 0) return SystemInfoProvider.Unknown;
        var wmi = consoleUserFromWmi == null ? null : Try(consoleUserFromWmi);
        return string.IsNullOrWhiteSpace(wmi) ? SystemInfoProvider.Unknown : wmi!.Trim();
    }

    private static string? Try(Func<string?> read) {
        try {
            return read();
        } catch {
            return null;
        }
    }
}
