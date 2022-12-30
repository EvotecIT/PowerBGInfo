Import-Module .\PowerBGInfo.psd1 -Force

New-BGInfo -MonitorIndex 0 {
    # Lets add computer name, but lets use builtin values for that
    New-BGInfoValue -BuiltinValue HostName -Color Red -FontSize 20 -FontFamilyName 'Calibri'
    New-BGInfoValue -BuiltinValue FullUserName
    New-BGInfoValue -BuiltinValue CpuName
    New-BGInfoValue -BuiltinValue CpuLogicalCores
    New-BGInfoValue -BuiltinValue RAMSize
    New-BGInfoValue -BuiltinValue RAMSpeed

    # Lets add Label, but without any values, kinf of like section starting
    New-BGInfoLabel -Name "Drives" -Color LemonChiffon -FontSize 16 -FontFamilyName 'Calibri'

    # Lets get all drives and their labels
    foreach ($Disk in (Get-Disk)) {
        $Volumes = $Disk | Get-Partition | Get-Volume
        foreach ($V in $Volumes) {
            New-BGInfoValue -Name "Drive $($V.DriveLetter)" -Value $V.SizeRemaining
        }
    }
} -FilePath $PSScriptRoot\Samples\PrzemyslawKlysAndKulkozaurr.jpg -ConfigurationDirectory $PSScriptRoot\Output -PositionX 100 -PositionY 100 -WallpaperFit Stretch