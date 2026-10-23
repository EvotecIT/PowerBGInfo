# The built-in helper: read-only, each probe limited to five seconds.
$facts = Get-BGInfoSystemInfo

# Your own function with the same property names drops in for it.
function Get-MySystemInfo {
    $os = Get-CimInstance -ClassName Win32_OperatingSystem
    $uptime = (Get-Date) - $os.LastBootUpTime
    $volumes = Get-Volume | Where-Object { $_.DriveLetter -and $_.Size }

    [pscustomobject] @{
        ComputerName = $env:COMPUTERNAME
        OSName = $os.Caption
        UptimeText = '{0} days {1} h' -f $uptime.Days, $uptime.Hours
        Disks = foreach ($volume in $volumes) {
            [pscustomobject] @{
                Drive = "$($volume.DriveLetter):"
                FreeGB = [Math]::Round($volume.SizeRemaining / 1GB, 1)
                UsedPercent = [Math]::Round(
                    100 * (1 - $volume.SizeRemaining / $volume.Size), 1)
            }
        }
    }
}
$facts = Get-MySystemInfo

# The presentation code reads the same names either way.
$facts.Disks | New-BGInfoTable -Region Right -Title 'Disks' `
    -Property Drive, FreeGB, UsedPercent
