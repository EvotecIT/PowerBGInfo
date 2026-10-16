<#
    Feature: tables. Pipe objects into New-BGInfoTable and pick columns with
    -Property, including calculated columns. A -StatusColumn turns values such
    as OK, Warning or Failed into colored dots.
#>
param(
    [ValidateSet('Light', 'Dark')]
    [string] $Theme = 'Dark',
    [int] $Width = 1920,
    [int] $Height = 1080,
    [string] $OutputDirectory = (Join-Path $PSScriptRoot '..\Output\Features')
)

Import-Module (Join-Path $PSScriptRoot '..\..\PowerBGInfo.psd1') -Force

#region Data: your logic goes here
# Live sources for the same shapes:
#   Get-Process | Sort-Object CPU -Descending | Select-Object -First 5
#   Get-Service -Name Spooler, W32Time, WinRM | Select-Object Name, Status
#   Get-WinEvent -LogName System -MaxEvents 5
function Get-TableExampleData {
    [pscustomobject] @{
        Processes   = @(
            [pscustomobject] @{ Name = 'sqlservr'; Id = 4120; CpuSeconds = 18233.4; WorkingSet = 96.1GB }
            [pscustomobject] @{ Name = 'MsMpEng'; Id = 3388; CpuSeconds = 912.2; WorkingSet = 0.41GB }
            [pscustomobject] @{ Name = 'w3wp'; Id = 7712; CpuSeconds = 640.8; WorkingSet = 1.9GB }
            [pscustomobject] @{ Name = 'svchost'; Id = 1504; CpuSeconds = 210.5; WorkingSet = 0.12GB }
        )
        Maintenance = @(
            [pscustomobject] @{ Window = 'Sun 02:00'; Change = 'October cumulative update'; State = 'Pending' }
            [pscustomobject] @{ Window = 'Tue 21:00'; Change = 'SQL CU 14'; State = 'OK' }
            [pscustomobject] @{ Window = 'Thu 22:00'; Change = 'Storage firmware'; State = 'Warning' }
        )
    }
}

$fallback = [pscustomobject] @{ Processes = @(); Maintenance = @([pscustomobject] @{ Window = 'unknown'; Change = 'source unavailable'; State = 'Unknown' }) }
$data = Invoke-BGInfoDataSource -Name 'Table data' -TimeoutSeconds 10 -ScriptBlock ${function:Get-TableExampleData} -Default $fallback
#endregion

#region Presentation: reads only $data
$scale = [Math]::Round($Height / 1080, 2)
New-BGInfo -Theme (New-BGInfoTheme Graphite -Mode $Theme -Scale $scale) -Target File {
    New-BGInfoBackground -Width $Width -Height $Height
    New-BGInfoLabel -Name 'TABLES' -Accent -FontWeight 700
    New-BGInfoLabel -Name 'Objects in, rows out' -FontSize ([int](40 * $scale)) -FontWeight 600

    $data.Processes | New-BGInfoTable -Region Right -Title 'Top processes' -Note 'by CPU time' -Property @(
        'Name', 'Id'
        @{ Name = 'CPU (s)'; Expression = { [Math]::Round($_.CpuSeconds) } }
        @{ Name = 'Memory'; Expression = { '{0:N1} GB' -f ($_.WorkingSet / 1GB) } }
    )
    $data.Maintenance | New-BGInfoTable -Region Right -Title 'Upcoming maintenance' -StatusColumn State
} -ConfigurationDirectory $OutputDirectory -OutputFileName "tables-$($Theme.ToLower())-$Width.png"
#endregion
