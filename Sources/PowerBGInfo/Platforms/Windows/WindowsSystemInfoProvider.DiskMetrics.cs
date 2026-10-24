using System.Runtime.InteropServices;

namespace PowerBGInfo;

internal sealed partial class WindowsSystemInfoProvider {
    private static bool TryGetDiskFreePercent(string? argument, out double value) {
        value = 0;
        if (!TryGetDiskSpace(argument, out var size, out var free)) return false;
        value = ClampPercent((double)free / size * 100d);
        return true;
    }

    private static bool TryGetDiskFreeGb(string? argument, out double value) {
        value = 0;
        if (!TryGetDiskSpace(argument, out _, out var free)) return false;
        value = free / (1024d * 1024d * 1024d);
        return true;
    }

    /// <summary>
    /// Reads available space on the actual requested filesystem. Missing or uncertain targets
    /// remain unavailable instead of falling back to the drive containing an authored alias.
    /// </summary>
    private static bool TryGetDiskSpace(string? argument, out ulong size, out ulong free) {
        size = free = 0;
        if (!IsWindows()) return false;
        try {
            var requested = string.IsNullOrWhiteSpace(argument)
                ? Environment.GetEnvironmentVariable("SystemDrive") : argument;
            if (string.IsNullOrWhiteSpace(requested)) requested = Path.GetPathRoot(Environment.SystemDirectory);
            if (string.IsNullOrWhiteSpace(requested)) return false;

            // C and C: deliberately select the drive root, not that drive's process-local directory.
            var drive = requested!.Trim();
            var bareDrive = drive.Length == 1 && char.IsLetter(drive[0]) ||
                drive.Length == 2 && char.IsLetter(drive[0]) && drive[1] == ':';
            var path = bareDrive ? char.ToUpperInvariant(drive[0]) + @":\" : Path.GetFullPath(requested);
            var identity = BgInfoPaths.GetIdentity(path);
            var attributes = File.GetAttributes(identity);
            var directory = (attributes & FileAttributes.Directory) != 0 ? identity : Path.GetDirectoryName(identity);
            if (string.IsNullOrEmpty(directory)) return false;
            // Query the resolved directory itself: a lexical drive root loses junction-mounted
            // volumes. GetDiskFreeSpaceEx also requires a trailing separator for UNC directories.
            if (!directory!.EndsWith("\\", StringComparison.Ordinal)) directory += "\\";
            return GetDiskFreeSpaceEx(directory, out free, out size, out _) && size > 0;
        } catch {
            // Facts are optional, but an explicit path is never replaced with another disk.
            size = free = 0;
            return false;
        }
    }

    [DllImport("kernel32.dll", EntryPoint = "GetDiskFreeSpaceExW", CharSet = CharSet.Unicode, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool GetDiskFreeSpaceEx(string directory, out ulong availableToCaller, out ulong totalForCaller, out ulong totalFree);
}
