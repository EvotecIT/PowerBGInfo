using System;
using System.IO;
using System.Management.Automation;

namespace PowerBGInfo.PowerShell;

/// <summary>Resolves command paths without changing the process working directory.</summary>
internal static class PowerShellFileSystemPath {
    /// <summary>Resolves an optional filesystem path against the current PowerShell location.</summary>
    internal static string Resolve(SessionState sessionState, string path, string parameterName) {
        if (string.IsNullOrWhiteSpace(path)) return path;

        var resolved = sessionState.Path.GetUnresolvedProviderPathFromPSPath(path, out var provider, out _);
        if (!string.Equals(provider.Name, "FileSystem", StringComparison.OrdinalIgnoreCase)) {
            throw new PSArgumentException($"{parameterName} requires a FileSystem path.", parameterName);
        }

        return resolved;
    }

    /// <summary>Preserves output paths relative to the configuration directory and resolves qualified paths.</summary>
    internal static string ResolveOutputFileName(SessionState sessionState, string path) {
        if (string.IsNullOrWhiteSpace(path)) return path;
        if (Path.IsPathRooted(path) || sessionState.Path.IsPSAbsolute(path, out _)
            || path.Equals("~", StringComparison.Ordinal) || path.StartsWith("~/", StringComparison.Ordinal)
            || path.StartsWith("~\\", StringComparison.Ordinal)) {
            return Resolve(sessionState, path, "OutputFileName");
        }

        return path;
    }
}
