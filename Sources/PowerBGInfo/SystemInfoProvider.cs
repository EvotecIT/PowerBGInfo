using System.Runtime.InteropServices;

namespace PowerBGInfo;

public static class SystemInfoProvider
{
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
