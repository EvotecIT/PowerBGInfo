<#
    Template: copy this file and fill in Get-MyWallpaperData. Nothing else
    needs to change.

    1. Data. Get-MyWallpaperData is your logic: CIM, registry, Active
       Directory, a REST API or a monitoring export. It returns one plain
       object. The samples below show the shape the layout expects.
    2. Safety. Invoke-BGInfoDataSource runs it with a time limit. When it
       throws, returns nothing or takes too long, the wallpaper still renders
       with $fallback, so logon is never blocked by one broken source.
    3. Presentation. The New-BGInfo block reads only $data. The theme picks the
       look; regions place the panels on any screen size.

    The file target only writes an image. Use -Target Wallpaper (or run the
    script from a logon task) when you are ready to apply it.
#>
param(
    [ValidateSet('Light', 'Dark')]
    [string] $Theme = 'Dark',
    [int] $Width = 1920,
    [int] $Height = 1080,
    [string] $OutputDirectory = (Join-Path $PSScriptRoot 'Output')
)

Import-Module (Join-Path $PSScriptRoot '..\PowerBGInfo.psd1') -Force

#region 1. Data: your logic goes here
function Get-MyWallpaperData {
    # your logic: replace these sample values with real lookups, for example
    #   $os = Get-CimInstance Win32_OperatingSystem
    #   $tickets = (Invoke-RestMethod -Uri 'https://help.contoso.com/api/open' -TimeoutSec 5).count
    [pscustomobject] @{
        Title    = 'FINANCE  ·  FLOOR 3'
        Heading  = 'WS-FIN-0042'          # $env:COMPUTERNAME
        Facts    = [ordered] @{
            'Owner'   = 'Finance IT'
            'Support' = 'ext. 4357'
        }
        Cpu      = @(12, 18, 25, 22, 30, 27, 24)
        DiskUsed = 63
        Tasks    = @(
            @{ Text = 'Month-end close'; Checked = $false; Value = 'Fri' }
            @{ Text = 'Backups verified'; Checked = $true }
        )
    }
}

# What the wallpaper shows when the data source fails.
$fallback = [pscustomobject] @{
    Title = 'STATUS UNAVAILABLE'; Heading = $env:COMPUTERNAME; Facts = [ordered] @{ 'Support' = 'ext. 4357' }
    Cpu = @(0); DiskUsed = 0; Tasks = @(@{ Text = 'unknown'; Checked = $false })
}
$data = Invoke-BGInfoDataSource -Name 'Wallpaper data' -TimeoutSeconds 15 -ScriptBlock ${function:Get-MyWallpaperData} -Default $fallback
#endregion

#region 2. Presentation: reads only $data
$scale = [Math]::Round([Math]::Min($Width, $Height) / 1080, 2)
New-BGInfo -Theme (New-BGInfoTheme Graphite -Mode $Theme -Scale $scale) -Target File {
    New-BGInfoBackground -Width $Width -Height $Height

    New-BGInfoLabel -Name $data.Title -Accent -FontWeight 700
    New-BGInfoLabel -Name $data.Heading -FontSize ([int](56 * $scale)) -FontWeight 600
    foreach ($fact in $data.Facts.GetEnumerator()) {
        New-BGInfoValue -Name $fact.Key -Value $fact.Value
    }

    New-BGInfoChart -Region Right -Title 'CPU' -Kind Area -Values $data.Cpu -ValueSuffix ' %' -NoHistory
    New-BGInfoMetric -Region Right -Label 'System drive' -Value $data.DiskUsed -Unit '%' -Caption 'used' `
        -WarningThreshold 80 -CriticalThreshold 90 -Height ([int](150 * $scale))
    New-BGInfoList -Region Right -Title 'This week' -Item $data.Tasks
} -ConfigurationDirectory $OutputDirectory -OutputFileName "template-$($Theme.ToLower()).png"
#endregion
