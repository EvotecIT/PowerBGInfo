<#
    Feature: layout regions. Panels flow into named regions instead of
    hand-placed coordinates, so one script lays out correctly at 1920x1080,
    2560x1440, ultrawide and portrait, and on every monitor you render for.

      Top     full-width band, flows left to right
      Left    column between the bands, stacks top to bottom
      Center  space between the columns, stacked and centered
      Right   column between the bands, stacks top to bottom
      Bottom  full-width band, flows left to right

    A panel without -Width or -Height shares the space left in its region.
    New-BGInfoLayout keeps the taskbar and the desktop-icon area free.
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
# Replace the body with your own sources. Return plain values.
function Get-RegionExampleData {
    [pscustomobject] @{
        HostName = 'FILE-SRV-07'
        Cpu      = @(18, 22, 35, 28, 31, 26, 24, 29)
        Memory   = 71
        Network  = @(120, 180, 160, 220, 260, 240, 210, 230)
        Volumes  = [ordered] @{ 'C:' = 58; 'E: Shares' = 84; 'F: Archive' = 67 }
        Shares   = @(
            [pscustomobject] @{ Share = 'Finance'; Sessions = 42; State = 'OK' }
            [pscustomobject] @{ Share = 'HR'; Sessions = 17; State = 'OK' }
            [pscustomobject] @{ Share = 'Projects'; Sessions = 88; State = 'Warning' }
        )
    }
}

# Time-box the source and fall back, so a broken source never blocks logon.
$data = Invoke-BGInfoDataSource -Name 'Region example data' -TimeoutSeconds 10 `
    -ScriptBlock ${function:Get-RegionExampleData} -Default ([pscustomobject] @{
        HostName = $env:COMPUTERNAME; Cpu = @(0); Memory = 0; Network = @(0); Volumes = @{ 'C:' = 0 }; Shares = @()
    })
#endregion

#region Presentation: reads only $data
$scale = [Math]::Round([Math]::Min($Width, $Height) / 1080, 2)
New-BGInfo -Theme (New-BGInfoTheme Graphite -Mode $Theme -Scale $scale) -Target File {
    New-BGInfoBackground -Width $Width -Height $Height
    # Keep 96 px free on the left for desktop icons; the taskbar reserve is estimated from the height.
    New-BGInfoLayout -IconAreaWidth 96

    New-BGInfoLabel -Name 'REGIONS  ·  ONE SCRIPT, EVERY SCREEN' -Accent -FontWeight 700
    New-BGInfoLabel -Name $data.HostName -FontSize ([int](56 * $scale)) -FontWeight 600 -Status Info

    # Left column: starts below the text block it shares.
    New-BGInfoMetric -Region Left -Label 'Memory' -Value $data.Memory -Unit '%' -Height ([int](140 * $scale)) `
        -WarningThreshold 80 -CriticalThreshold 92
    New-BGInfoChart -Region Left -Title 'CPU' -Kind Area -Values $data.Cpu -ValueSuffix ' %' -NoHistory

    # Right column: two panels share the height.
    $data.Shares | New-BGInfoTable -Region Right -Title 'Open shares' -StatusColumn State
    New-BGInfoChart -Region Right -Title 'Volumes used' -Kind ProgressBar -Values @($data.Volumes.Values) `
        -Labels @($data.Volumes.Keys) -Maximum 100 -ValueSuffix ' %' -NoProgressHandles -ShowLatestValue:$false `
        -WarningThreshold 80 -CriticalThreshold 90 -NoHistory

    # Bottom band: three panels share the width.
    New-BGInfoChart -Region Bottom -Title 'Network' -Kind Line -Values $data.Network -ValueSuffix ' Mbps' -NoHistory
    New-BGInfoChart -Region Bottom -Title 'CPU now' -Kind Gauge -Values $data.Cpu[-1] -Maximum 100 -ValueSuffix ' %' `
        -ShowLatestValue:$false -NoHistory
    New-BGInfoList -Region Bottom -Title 'Regions' -Item 'Left and Right columns', 'Top and Bottom bands', 'Center between them'
} -ConfigurationDirectory $OutputDirectory -OutputFileName "regions-$($Theme.ToLower())-$($Width)x$Height.png"
#endregion
