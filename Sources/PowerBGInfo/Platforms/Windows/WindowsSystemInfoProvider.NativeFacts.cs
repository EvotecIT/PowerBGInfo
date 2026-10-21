using Microsoft.Win32;
using System.Runtime.InteropServices;

namespace PowerBGInfo;

internal sealed partial class WindowsSystemInfoProvider
{
    // Keep native fallbacks in the shared fact provider so .NET, PowerShell and the CLI agree.
    // Module speed/part numbers still require WMI; a total is not presented as individual DIMMs.
    private static string ReadNativeCpuName()
    {
        try
        {
            using var processor = Registry.LocalMachine.OpenSubKey(@"HARDWARE\DESCRIPTION\System\CentralProcessor\0");
            return (processor?.GetValue("ProcessorNameString") as string)?.Trim() ?? string.Empty;
        }
        catch
        {
            return string.Empty;
        }
    }

    private static bool TryReadNativeInstalledMemory(out ulong kilobytes)
    {
        kilobytes = 0;
        try
        {
            return GetPhysicallyInstalledSystemMemory(out kilobytes) && kilobytes > 0;
        }
        catch
        {
            return false;
        }
    }

    [DllImport("kernel32.dll", ExactSpelling = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool GetPhysicallyInstalledSystemMemory(out ulong totalMemoryInKilobytes);
}
