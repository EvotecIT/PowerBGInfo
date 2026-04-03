$scriptRoot = Split-Path -Parent $MyInvocation.MyCommand.Path
$framework = if ($PSEdition -eq 'Core') { 'net8.0-windows' } else { 'net472' }
$binaryPath = Join-Path -Path $scriptRoot -ChildPath "..\Sources\PowerBGInfo.PowerShell\bin\Release\$framework\PowerBGInfo.PowerShell.dll"
if (-not (Test-Path -LiteralPath $binaryPath)) {
    $binaryPath = Join-Path -Path $scriptRoot -ChildPath "..\Sources\PowerBGInfo.PowerShell\bin\Debug\$framework\PowerBGInfo.PowerShell.dll"
}
Import-Module -Name $binaryPath -Force
$sampleImage = Join-Path -Path $scriptRoot -ChildPath 'Samples\TapC-Evotec-2560x1080.jpg'
$outputDir = Join-Path -Path $scriptRoot -ChildPath 'Output'
$configPath = Join-Path -Path $scriptRoot -ChildPath 'Configuration\PowerBGInfo.Generated.json'

$textColor = [System.Drawing.Color]::White
$valueColor = [System.Drawing.Color]::Aqua
$chartBackground = [System.Drawing.Color]::FromArgb(160, 0, 0, 0)

$entries = @(
    New-BGInfoValue -BuiltinValue HostName
    New-BGInfoValue -BuiltinValue FullUserName
    New-BGInfoLabel -Name 'Performance' -Color LemonChiffon -FontSize 14 -FontFamilyName 'Calibri'
)

$charts = @(
    New-BGInfoChart -Id 'cpu' -Title 'CPU' -Metric CpuPercent -ValueSuffix '%' -Kind Sparkline -MaxPoints 60 -Anchor BottomLeft -OffsetX 20 -OffsetY 20 -LineColor $valueColor -TextColor $textColor -BackgroundColor $chartBackground -TitleFontSize 18 -ValueFontSize 16 -ShowGrid -GridLineCount 3
    New-BGInfoChart -Id 'mem' -Title 'Memory' -Metric MemoryPercent -ValueSuffix '%' -Kind Sparkline -MaxPoints 60 -Anchor BottomLeft -OffsetX 20 -OffsetY 20 -LineColor $valueColor -TextColor $textColor -BackgroundColor $chartBackground -TitleFontSize 18 -ValueFontSize 16 -ShowGrid -GridLineCount 3
)

$configuration = New-BGInfoConfiguration -FilePath $sampleImage -ConfigurationDirectory $outputDir -Target File -WallpaperFit Fill -Entries $entries -Charts $charts -BackgroundColor Black -Color $textColor -ValueColor $valueColor -ChartLayout Stack -ChartStackAnchor BottomLeft -ChartStackDirection Vertical -ChartStackSpacing 12 -ChartStackAlignToTextBlock -ChartStackOutsideTextBlock -ChartStackOffsetX 10 -ChartStackOffsetY 10
Export-BGInfoConfiguration -InputObject $configuration -Path $configPath -Force

Invoke-BGInfo -Path $configPath -NoApply
