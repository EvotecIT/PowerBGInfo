Clear-Host

Import-Module $PSScriptRoot\..\PowerBGInfo.psd1 -Force

New-BGInfo -MonitorIndex 0 {
    # Lets add computer name, but lets use builtin values for that
    New-BGInfoValue -BuiltinValue HostName -Color Red -FontSize 20 -FontFamilyName 'Calibri'
    # Lets add user name, but lets use builtin values for that
    New-BGInfoValue -BuiltinValue FullUserName -Name "User Name X" -Color White
    New-BGInfoValue -BuiltinValue CpuName
    New-BGInfoValue -BuiltinValue CpuLogicalCores
    New-BGInfoValue -BuiltinValue RAMSize
    New-BGInfoValue -BuiltinValue RAMSpeed

    # Lets add Label, but without any values, kind of like section starting
    New-BGInfoLabel -Name "Drives" -Color LemonChiffon -FontSize 16 -FontFamilyName 'Calibri'

    # Lets get all drives and their labels
    foreach ($Disk in (Get-Disk)) {
        $Volumes = $Disk | Get-Partition | Get-Volume
        foreach ($V in $Volumes) {
            New-BGInfoValue -Name "Drive $($V.DriveLetter)" -Value $V.SizeRemaining
        }
    }
} -FilePath $PSScriptRoot\Samples\PrzemyslawKlysAndKulkozaurr.jpg -ConfigurationDirectory $PSScriptRoot\Output -WallpaperFit Center -TextPosition MiddleCenter -SpaceBetweenColumns 50 -Target Both