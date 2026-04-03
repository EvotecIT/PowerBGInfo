$scriptRoot = Split-Path -Parent $MyInvocation.MyCommand.Path
$repositoryRoot = (Resolve-Path (Join-Path -Path $scriptRoot -ChildPath '..')).Path
$solutionPath = Join-Path -Path $repositoryRoot -ChildPath 'Sources\PowerBGInfo.sln'
$modulePath = Join-Path -Path $repositoryRoot -ChildPath 'PowerBGInfo.psd1'
$cliPath = Join-Path -Path $repositoryRoot -ChildPath 'Sources\PowerBGInfo.Cli\bin\Debug\net8.0-windows\PowerBGInfo.Cli.exe'

# Build the current checkout so the example uses the latest source instead of stale binaries.
& dotnet build $solutionPath -c Debug --nologo | Out-Null
if ($LASTEXITCODE -ne 0) {
    throw "dotnet build failed for $solutionPath"
}

Import-Module -Name $modulePath -Force
if (-not (Test-Path -LiteralPath $cliPath)) {
    throw "Unable to find CLI executable at $cliPath"
}
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

$configuration = New-BGInfoConfiguration -FilePath '..\Samples\TapC-Evotec-2560x1080.jpg' -ConfigurationDirectory '..\Output' -Target File -WallpaperFit Fill -Entries $entries -Charts $charts -BackgroundColor Black -Color $textColor -ValueColor $valueColor -ChartLayout Stack -ChartStackAnchor BottomLeft -ChartStackDirection Vertical -ChartStackSpacing 12 -ChartStackAlignToTextBlock -ChartStackOutsideTextBlock -ChartStackOffsetX 10 -ChartStackOffsetY 10
$configuration.OutputFileName = 'PowerBGInfo.Cli.Sample.png'
Export-BGInfoConfiguration -InputObject $configuration -Path $configPath -Force

& $cliPath --config $configPath --no-apply
