<#
    Feature: status thresholds. Values pick ok, warning or critical colors by
    themselves, so scripts stop hand-picking colors. Charts color each bar,
    progress row or bullet; gauges and lines take the color of the latest value.
    Use -ThresholdDirection Below when lower values are worse.
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
# Live sources for the same shape:
#   Get-Volume | Where-Object DriveLetter   (used % per drive)
#   (Get-CimInstance Win32_Battery).EstimatedChargeRemaining
function Get-ThresholdExampleData {
    [pscustomobject] @{
        Volumes     = [ordered] @{ 'C: System' = 61; 'D: Data' = 88; 'L: Logs' = 96 }
        CpuNow      = 74
        FreeMemory  = 9
        Certificate = 12
    }
}

$data = Invoke-BGInfoDataSource -Name 'Threshold data' -TimeoutSeconds 10 -ScriptBlock ${function:Get-ThresholdExampleData} `
    -Default ([pscustomobject] @{ Volumes = @{ 'C:' = 0 }; CpuNow = 0; FreeMemory = 100; Certificate = 365 })
#endregion

#region Presentation: reads only $data
$scale = [Math]::Round($Height / 1080, 2)
New-BGInfo -Theme (New-BGInfoTheme Aurora -Mode $Theme -Scale $scale) -Target File {
    New-BGInfoBackground -Width $Width -Height $Height
    New-BGInfoLabel -Name 'STATUS THRESHOLDS' -Accent -FontWeight 700
    New-BGInfoLabel -Name 'Colors follow the values' -FontSize ([int](40 * $scale)) -FontWeight 600
    New-BGInfoLabel -Name '●  D: and L: need attention' -Status Warning -FontWeight 600

    # Higher is worse: warning from 80 %, critical from 90 %.
    New-BGInfoChart -Region Bottom -Title 'Volumes used' -Kind ProgressBar -Values @($data.Volumes.Values) `
        -Labels @($data.Volumes.Keys) -Maximum 100 -ValueSuffix ' %' -NoProgressHandles -ShowLatestValue:$false `
        -WarningThreshold 80 -CriticalThreshold 90 -NoHistory
    New-BGInfoChart -Region Bottom -Title 'CPU now' -Kind Gauge -Values $data.CpuNow -Maximum 100 -ValueSuffix ' %' `
        -ShowLatestValue:$false -WarningThreshold 70 -CriticalThreshold 90 -NoHistory
    # Lower is worse: free memory and days until a certificate expires.
    New-BGInfoMetric -Region Bottom -Label 'Free memory' -Value $data.FreeMemory -Unit '%' `
        -WarningThreshold 15 -CriticalThreshold 5 -ThresholdDirection Below -Caption 'warning below 15 %'
    New-BGInfoMetric -Region Bottom -Label 'TLS certificate' -Value $data.Certificate -Unit 'days' `
        -WarningThreshold 30 -CriticalThreshold 7 -ThresholdDirection Below -Caption 'until it expires'
} -ConfigurationDirectory $OutputDirectory -OutputFileName "thresholds-$($Theme.ToLower())-$Width.png"
#endregion
