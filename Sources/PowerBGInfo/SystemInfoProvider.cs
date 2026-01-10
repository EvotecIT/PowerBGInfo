using System.Runtime.InteropServices;

namespace PowerBGInfo;

/// <summary>
/// Provides built-in system values for BGInfo rendering.
/// </summary>
public static class SystemInfoProvider
{
    /// <summary>
    /// Resolves a built-in token to its system value.
    /// </summary>
    /// <param name="builtin">Built-in key name (for example UserName, HostName, OSVersion).</param>
    /// <returns>The resolved value, or an empty string when unknown.</returns>
    public static string GetValue(string builtin)
    {
        return builtin switch
        {
            "UserName" => Environment.UserName,
            "HostName" => Environment.MachineName,
            "FullUserName" => $"{Environment.UserDomainName}\\{Environment.UserName}",
            "CpuLogicalCores" => Environment.ProcessorCount.ToString(),
            "OSArchitecture" => RuntimeInformation.OSArchitecture.ToString(),
            "OSVersion" => RuntimeInformation.OSDescription,
            _ => string.Empty
        };
    }
}
