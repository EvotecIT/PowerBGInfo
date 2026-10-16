<#
    Feature: calendar heatmap. One cell per day, darker for larger values:
    sign-ins, patch installs, backup failures or incidents over recent weeks.
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
# One object per day with Date and Value. Live sources for the same shape:
#   Get-WinEvent -FilterHashtable @{ LogName = 'Security'; Id = 4624; StartTime = (Get-Date).AddDays(-120) } |
#       Group-Object { $_.TimeCreated.Date } | ForEach-Object { [pscustomobject] @{ Date = [datetime] $_.Name; Value = $_.Count } }
# Plain dates work too: each one counts once.
function Get-SignInHistory {
    $today = Get-Date '2026-10-16'
    foreach ($day in 0..125) {
        $date = $today.AddDays(-$day)
        $weekend = $date.DayOfWeek -in 'Saturday', 'Sunday'
        [pscustomobject] @{ Date = $date; Value = if ($weekend) { $day % 3 } else { 6 + ($day * 7) % 9 } }
    }
}

$history = Invoke-BGInfoDataSource -Name 'Sign-in history' -TimeoutSeconds 15 -ScriptBlock ${function:Get-SignInHistory} `
    -Default @([pscustomobject] @{ Date = (Get-Date).Date; Value = 0 })
#endregion

#region Presentation: reads only $history
$scale = [Math]::Round($Height / 1080, 2)
New-BGInfo -Theme (New-BGInfoTheme Aurora -Mode $Theme -Scale $scale) -Target File {
    New-BGInfoBackground -Width $Width -Height $Height
    New-BGInfoLabel -Name 'CALENDAR HEATMAP' -Accent -FontWeight 700
    New-BGInfoLabel -Name 'Interactive sign-ins' -FontSize ([int](40 * $scale)) -FontWeight 600
    New-BGInfoValue -Name 'Busiest day' -Value (($history | Sort-Object Value -Descending | Select-Object -First 1).Date.ToString('dddd d MMMM'))

    $history | New-BGInfoCalendarHeatmap -Region Right -Title 'Sign-ins per day' -Note 'last 18 weeks' -ShowLegend `
        -Height ([int](300 * $scale))
} -ConfigurationDirectory $OutputDirectory -OutputFileName "calendar-heatmap-$($Theme.ToLower())-$Width.png"
#endregion
