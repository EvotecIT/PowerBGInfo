using System.Diagnostics;
using System.Runtime.InteropServices;

namespace PowerBGInfo;

/// <summary>
/// Finds the user signed in to the Windows session a wallpaper is rendered for, through the Remote Desktop Services
/// session API. Read-only; querying the caller's own session needs no elevation.
/// </summary>
internal static class SessionUser {
    /// <summary>
    /// Resolves the user: the user of the process's own session; for a process in session 0 (a SYSTEM task or a
    /// service), the user of the active console session; then the console user from WMI; otherwise Unknown.
    /// </summary>
    /// <param name="consoleUserFromWmi">Last resort, for example Win32_ComputerSystem.UserName.</param>
    public static string Resolve(Func<string?> consoleUserFromWmi) =>
        Resolve(GetProcessSessionId(), QuerySessionUser, GetActiveConsoleSessionId, consoleUserFromWmi);

    internal static string Resolve(int processSessionId, Func<int, string?> querySessionUser, Func<int> activeConsoleSession, Func<string?>? consoleUserFromWmi) {
        if (processSessionId > 0) {
            var own = Try(() => querySessionUser(processSessionId));
            if (!string.IsNullOrWhiteSpace(own)) return own!;
        }

        var console = Try(activeConsoleSession, -1);
        if (console > 0 && console != processSessionId) {
            var user = Try(() => querySessionUser(console));
            if (!string.IsNullOrWhiteSpace(user)) return user!;
        }

        var wmi = consoleUserFromWmi == null ? null : Try(consoleUserFromWmi);
        return string.IsNullOrWhiteSpace(wmi) ? SystemInfoProvider.Unknown : wmi!.Trim();
    }

    /// <summary>Returns DOMAIN\user for a session, or null when nobody is signed in to it or it cannot be read.</summary>
    internal static string? QuerySessionUser(int sessionId) {
        var user = QueryString(sessionId, WtsUserName);
        if (string.IsNullOrWhiteSpace(user)) return null;
        var domain = QueryString(sessionId, WtsDomainName);
        return string.IsNullOrWhiteSpace(domain) ? user : domain + "\\" + user;
    }

    internal static int GetProcessSessionId() {
        try {
            using var process = Process.GetCurrentProcess();
            return process.SessionId;
        } catch {
            return -1;
        }
    }

    private static int GetActiveConsoleSessionId() {
        var id = WTSGetActiveConsoleSessionId();
        return id == uint.MaxValue ? -1 : (int)id;
    }

    private static string? QueryString(int sessionId, int infoClass) {
        IntPtr buffer = IntPtr.Zero;
        try {
            if (!WTSQuerySessionInformation(IntPtr.Zero, sessionId, infoClass, out buffer, out _) || buffer == IntPtr.Zero) {
                return null;
            }
            return Marshal.PtrToStringUni(buffer);
        } catch (Exception exception) when (exception is DllNotFoundException or EntryPointNotFoundException) {
            return null;
        } finally {
            if (buffer != IntPtr.Zero) WTSFreeMemory(buffer);
        }
    }

    private static T Try<T>(Func<T> read, T fallback = default!) {
        try {
            return read();
        } catch {
            return fallback;
        }
    }

    private const int WtsUserName = 5;
    private const int WtsDomainName = 7;

    [DllImport("wtsapi32.dll", EntryPoint = "WTSQuerySessionInformationW", CharSet = CharSet.Unicode, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool WTSQuerySessionInformation(IntPtr server, int sessionId, int infoClass, out IntPtr buffer, out int bytesReturned);

    [DllImport("wtsapi32.dll")]
    private static extern void WTSFreeMemory(IntPtr memory);

    [DllImport("kernel32.dll")]
    private static extern uint WTSGetActiveConsoleSessionId();
}
