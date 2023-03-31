Import-Module .\PowerBGInfo.psd1 -Force

New-BGInfo -MonitorIndex 0 {
    # Lets add computer name, but lets use builtin values for that
    New-BGInfoValue -BuiltinValue HostName -Color Red -FontSize 20 -FontFamilyName 'Calibri'
    New-BGInfoValue -BuiltinValue FullUserName -Color White
    New-BGInfoValue -BuiltinValue CpuName -Color White
    New-BGInfoValue -BuiltinValue CpuLogicalCores -Color White -ValueColor Red
    New-BGInfoValue -BuiltinValue RAMSize -Color White
    New-BGInfoValue -BuiltinValue RAMSpeed -Color White -ValueColor ([SixLabors.ImageSharp.Color]::Aquamarine)
    New-BGInfoValue -BuiltinValue RAMPartNumber -Color White
    New-BGInfoValue -BuiltinValue BiosVersion -Color White
    New-BGInfoValue -BuiltinValue BiosManufacturer -Color White
    New-BGInfoValue -BuiltinValue BiosReleaseDate -Color White
    New-BGInfoValue -BuiltinValue OSName -Color White -Name "Operating System"
    New-BGInfoValue -BuiltinValue OSVersion -Color White
    New-BGInfoValue -BuiltinValue OSArchitecture -Color White
    New-BGInfoValue -BuiltinValue OSBuild -Color White
    New-BGInfoValue -BuiltinValue OSInstallDate -Color White
} -FilePath "C:\Support\GitHub\PowerBGInfo\Examples\Samples\TapN-Evotec-1600x900.jpg" -ConfigurationDirectory $PSScriptRoot\Output -PositionX 75 -PositionY 75 -WallpaperFit Fit