Import-Module .\PowerBGInfo.psd1 -Force

New-BGInfo -MonitorIndex 0 {
    # Lets add computer name
    New-BGInfoValue -Name 'ComputerName' -Value $env:COMPUTERNAME -Color Red -FontSize 16 -FontFamilyName 'Calibri'

    # Lets add Label, but without any values
    New-BGInfoLabel -Name "Drives" -Color LemonChiffon -FontSize 16 -FontFamilyName 'Calibri'

    # Lets get all drives and their labels
    foreach ($Disk in (Get-Disk)) {
        $Volumes = $Disk | Get-Partition | Get-Volume
        foreach ($V in $Volumes) {
            New-BGInfoValue -Name "Drive $($V.DriveLetter)" -Value $V.SizeRemaining -Color Red -FontSize 16 -FontFamilyName 'Calibri'
        }
    }
} -FilePath $PSScriptRoot\Samples\PrzemyslawKlysAndKulkozaurr.jpg -ConfigurationDirectory $PSScriptRoot\Output -FamilyName 'Calibri' -FontSize 16 -PositionX 100 -PositionY 100 -WallpaperFit Fit