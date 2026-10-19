<#
    Feature: timeline. Ranges on named lanes with a Now marker: maintenance
    windows, change freezes, backups and the on-call rota.
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
# One object per range with Lane, Label, Start, End and Category. Live sources:
# a change calendar API, an on-call export, or scheduled tasks on this machine.
function Get-ScheduleExampleData {
    $monday = Get-Date '2026-10-12'
    @(
        [pscustomobject] @{ Lane = 'On call'; Label = 'A. Novak'; Start = $monday; End = $monday.AddDays(3.5); Category = 'On call' }
        [pscustomobject] @{ Lane = 'On call'; Label = 'J. Smith'; Start = $monday.AddDays(3.5); End = $monday.AddDays(7); Category = 'On call' }
        [pscustomobject] @{ Lane = 'SQL-PROD-01'; Label = 'Patching'; Start = $monday.AddDays(6).AddHours(2); End = $monday.AddDays(6).AddHours(6); Category = 'Maintenance'; Status = 'Warning' }
        [pscustomobject] @{ Lane = 'Backups'; Label = 'Full'; Start = $monday.AddDays(5).AddHours(20); End = $monday.AddDays(6).AddHours(1); Category = 'Backup' }
        [pscustomobject] @{ Lane = 'Change freeze'; Label = 'Quarter close'; Start = $monday.AddDays(1); End = $monday.AddDays(3); Category = 'Freeze'; Status = 'Critical' }
    )
}

$schedule = Invoke-BGInfoDataSource -Name 'Schedule' -TimeoutSeconds 10 -ScriptBlock ${function:Get-ScheduleExampleData} `
    -Default @([pscustomobject] @{ Lane = 'Schedule'; Label = 'unknown'; Start = (Get-Date).Date; End = (Get-Date).Date.AddDays(1); Category = 'Unknown' })
#endregion

#region Presentation: reads only $schedule
$scale = [Math]::Round($Height / 1080, 2)
New-BGInfo -Theme (New-BGInfoTheme Corporate -Mode $Theme -Scale $scale) -Target File {
    New-BGInfoBackground -Width $Width -Height $Height
    New-BGInfoLabel -Name 'TIMELINE' -Accent -FontWeight 700
    New-BGInfoLabel -Name 'This week' -FontSize ([int](40 * $scale)) -FontWeight 600

    New-BGInfoTimeline -Region Bottom -Title 'Maintenance and on call' -Note 'Mon 12 - Sun 18 Oct' `
        -Item $schedule -Now (Get-Date '2026-10-16 10:30') -Height ([int](330 * $scale))
} -ConfigurationDirectory $OutputDirectory -OutputFileName "timeline-$($Theme.ToLower())-$Width.png"
#endregion
