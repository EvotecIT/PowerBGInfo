<#
    Feature: metric cards. One KPI per panel with its trend, a caption, a short
    history and a status that follows warning and critical thresholds.
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
# One object per metric. Replace the samples with Get-Counter, CIM, your
# monitoring API or a CSV export; keep the property names.
function Get-MetricExampleData {
    @(
        [pscustomobject] @{ Label = 'CPU'; Value = 38; Unit = '%'; Trend = '-6 %'; Caption = '5 minute average'; History = @(52, 48, 44, 41, 38) }
        [pscustomobject] @{ Label = 'Memory'; Value = 83; Unit = '%'; Trend = '+4 %'; Caption = 'of 128 GB'; History = @(70, 74, 77, 80, 83) }
        [pscustomobject] @{ Label = 'System drive'; Value = 94; Unit = '%'; Trend = '+1 %'; Caption = 'C: used'; History = @(90, 91, 92, 93, 94) }
        [pscustomobject] @{ Label = 'Logon time'; Value = 11; Unit = 's'; Trend = '-2 s'; Caption = 'last sign-in'; History = @(15, 14, 12, 13, 11) }
    )
}

# Time-box the source; on failure the cards show 'unknown' instead of blocking logon.
$metrics = Invoke-BGInfoDataSource -Name 'Metrics' -TimeoutSeconds 10 -ScriptBlock ${function:Get-MetricExampleData} -Default @(
    [pscustomobject] @{ Label = 'Metrics'; Value = 'unknown'; Unit = ''; Trend = ''; Caption = 'source unavailable'; History = @() }
)
#endregion

#region Presentation: reads only $metrics
$scale = [Math]::Round($Height / 1080, 2)
New-BGInfo -Theme (New-BGInfoTheme Midnight -Mode $Theme -Scale $scale) -Target File {
    New-BGInfoBackground -Width $Width -Height $Height
    New-BGInfoLabel -Name 'METRIC CARDS' -Accent -FontWeight 700
    New-BGInfoLabel -Name 'Status follows the thresholds' -FontSize ([int](40 * $scale)) -FontWeight 600

    foreach ($metric in $metrics) {
        # Higher is worse for all four samples: warn at 80, critical at 90 (seconds use 20 and 30).
        $limits = if ($metric.Unit -eq 's') { @{ WarningThreshold = 20; CriticalThreshold = 30 } } else { @{ WarningThreshold = 80; CriticalThreshold = 90 } }
        New-BGInfoMetric @limits -Region Right -Label $metric.Label -Value $metric.Value -Unit $metric.Unit `
            -Trend $metric.Trend -Caption $metric.Caption -History $metric.History -HistoryKind Bars
    }
} -ConfigurationDirectory $OutputDirectory -OutputFileName "metrics-$($Theme.ToLower())-$Width.png"
#endregion
