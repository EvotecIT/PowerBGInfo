Import-Module (Join-Path -Path $PSScriptRoot -ChildPath '..\..\PowerBGInfo.psd1') -Force

$examplesPath = (Resolve-Path (Join-Path -Path $PSScriptRoot -ChildPath '..')).Path
$sampleImage = Join-Path -Path $examplesPath -ChildPath 'Samples\TapC-Evotec-2560x1080.jpg'
$outputDirectory = Join-Path -Path $examplesPath -ChildPath 'Output'
$systemDrive = if ($env:SystemDrive) { $env:SystemDrive } else { 'C:' }
$driveRoot = if ($systemDrive.EndsWith('\')) { $systemDrive } else { $systemDrive + '\' }
$driveInfo = [System.IO.DriveInfo]::new($driveRoot)

$diskUsedPercent = 0
$diskFreePercent = 0
if ($driveInfo.TotalSize -gt 0) {
    $diskFreePercent = [Math]::Round(($driveInfo.AvailableFreeSpace / $driveInfo.TotalSize) * 100, 1)
    $diskUsedPercent = [Math]::Round(100 - $diskFreePercent, 1)
}

$serviceNames = @('EventLog', 'WinRM', 'W32Time', 'LanmanWorkstation', 'LanmanServer')
$services = foreach ($serviceName in $serviceNames) {
    Get-Service -Name $serviceName -ErrorAction SilentlyContinue
}

$runningServices = 0
foreach ($service in $services) {
    if ($service.Status -eq 'Running') {
        $runningServices++
    }
}

$serviceCount = if ($services.Count -gt 0) { $services.Count } else { 1 }
$stoppedServices = [Math]::Max(0, $serviceCount - $runningServices)

$white = 'White'
$muted = '#E6D2DCE8'
$panel = '#AC0A101C'
$cyan = '#2DD4BF'
$blue = '#60A5FA'
$green = '#34D399'
$orange = '#FB923C'
$red = '#F87171'
$purple = '#A78BFA'

New-BGInfo -MonitorIndex 0 -Target File {
    New-BGInfoValue -BuiltinValue HostName -Color LemonChiffon -ValueColor $white -FontSize 24 -ValueFontSize 18 -FontFamilyName 'Calibri'
    New-BGInfoValue -BuiltinValue FullUserName -Name 'User' -Color $muted -ValueColor $white
    New-BGInfoValue -BuiltinValue OSName -Name 'OS' -Color $muted -ValueColor $white
    New-BGInfoValue -Name 'Chart mode' -Value 'live metrics + local status' -Color $muted -ValueColor $white

    New-BGInfoChart -Id 'ops-cpu-history' -Title 'CPU history' -Metric CpuPercent -Kind Area -ValueSuffix '%' -Width 360 -Height 145 -LineColor $cyan -FillColor $cyan -TextColor $white -BackgroundColor $panel -ShowGrid -GridColor $muted -GridLineCount 3 -MaxPoints 60
    New-BGInfoChart -Id 'ops-memory-history' -Title 'Memory history' -Metric MemoryPercent -Kind Line -ValueSuffix '%' -Width 360 -Height 145 -LineColor $blue -TextColor $white -BackgroundColor $panel -ShowGrid -GridColor $muted -GridLineCount 3 -MaxPoints 60
    New-BGInfoChart -Id 'ops-system-drive' -Title "$systemDrive used/free" -Kind Donut -Values $diskUsedPercent,$diskFreePercent -Labels 'Used','Free' -ValueSuffix '%' -Width 360 -Height 205 -Palette $red,$green -TextColor $white -BackgroundColor $panel -ShowLegend -ShowPointLegend -LegendPosition Right -ShowDataLabels -Maximum 100 -DonutCenterValue "$diskUsedPercent%" -DonutCenterLabel 'Used' -ShowLatestValue:$false -NoHistory
    New-BGInfoChart -Id 'ops-patch-target' -Title 'Fleet patch compliance' -Kind Bullet -Values 89 -Target 95 -RangeEnds 70,85 -Width 360 -Height 150 -LineColor $orange -TextColor $white -BackgroundColor $panel -Maximum 100 -ShowLatestValue:$false -NoHistory
    New-BGInfoChart -Id 'ops-services' -Title 'Core services' -Kind Pictorial -Values $runningServices,$stoppedServices -Labels 'Running','Other' -Width 360 -Height 145 -Palette $green,$orange -TextColor $white -BackgroundColor $panel -PictorialSymbol Person -PictorialColumns $serviceCount -ShowDataLabels -Maximum $serviceCount -ShowLatestValue:$false -NoHistory
} -FilePath $sampleImage `
    -ConfigurationDirectory $outputDirectory `
    -OutputFileName 'PowerBGInfo.OperationalCharts.jpg' `
    -WallpaperFit Fill `
    -BackgroundColor Black `
    -Color $muted `
    -ValueColor $white `
    -ValueWrapWidth 360 `
    -TextPosition TopLeft `
    -SpaceX 42 `
    -SpaceY 42 `
    -ChartLayout Stack `
    -ChartStackAnchor BottomRight `
    -ChartStackDirection Vertical `
    -ChartStackSpacing 12 `
    -ChartStackOffsetX 32 `
    -ChartStackOffsetY 32
