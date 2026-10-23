function Get-MyWallpaperData {
    # The monitoring tool exports one JSON and one CSV file per machine.
    $share = '\\mon01.contoso.com\exports'
    $status = Get-Content -Path "$share\$env:COMPUTERNAME.json" -Raw |
        ConvertFrom-Json
    $checks = Import-Csv -Path "$share\$env:COMPUTERNAME-checks.csv"

    [pscustomobject] @{
        Cpu = $status.cpu.history         # numbers, oldest first
        Memory = $status.memory.percent
        Tasks = foreach ($check in $checks) {
            @{ Text = $check.Name; Checked = $check.State -eq 'OK' }
        }
    }
}
