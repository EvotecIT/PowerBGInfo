<#
    One wallpaper per monitor. Get-BGInfoMonitor lists the monitors with the
    Index that New-BGInfo -MonitorIndex takes, so each image is rendered at
    the size and display scale of its own screen.

    The file target only writes the images. Use -Target Wallpaper when you
    are ready to apply them; each monitor then gets its own file.
#>
param(
    [ValidateSet('Light', 'Dark')]
    [string] $Theme = 'Dark',
    [string] $OutputDirectory = (Join-Path $PSScriptRoot 'Output\PerMonitor')
)

Import-Module (Join-Path $PSScriptRoot '..\PowerBGInfo.psd1') -Force

$monitors = @(Get-BGInfoMonitor)
if ($monitors.Count -eq 0) {
    # No desktop, for example under SYSTEM: render once at 1920 x 1080.
    $monitors = @([pscustomobject] @{
            Index = 0; Width = 1920; Height = 1080; Primary = $true
        })
}

foreach ($monitor in $monitors) {
    $scale = $monitor.Height / 1080
    if ($scale -lt 1) {
        $scale = 1
    }
    $render = @{
        Theme                  = New-BGInfoTheme Graphite -Mode $Theme -Scale $scale
        Target                 = 'File'
        MonitorIndex           = $monitor.Index
        WallpaperFit           = 'Fill'
        ConfigurationDirectory = $OutputDirectory
        OutputFileName         = "monitor-$($monitor.Index).png"
    }
    New-BGInfo @render {
        New-BGInfoBackground -Width $monitor.Width -Height $monitor.Height
        New-BGInfoLabel -Name $env:COMPUTERNAME -FontWeight 700 `
            -FontSize ([int](48 * $scale))
        New-BGInfoValue -Name 'Monitor' -Value $monitor.Index
        New-BGInfoValue -Name 'Size' `
            -Value "$($monitor.Width) x $($monitor.Height)"
        New-BGInfoValue -Name 'Primary' -Value $monitor.Primary
    }
}
