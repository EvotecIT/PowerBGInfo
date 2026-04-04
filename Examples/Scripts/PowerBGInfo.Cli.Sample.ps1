Import-Module (Join-Path -Path $PSScriptRoot -ChildPath '..\..\PowerBGInfo.psd1') -Force

$sampleImage = '..\Samples\TapC-Evotec-2560x1080.jpg'
$outputDirectory = '..\Output'

New-BGInfo {
    New-BGInfoValue -BuiltinValue HostName
    New-BGInfoValue -BuiltinValue FullUserName -Name 'User'
    New-BGInfoLabel -Name 'Performance' -Color LemonChiffon -FontSize 14 -FontFamilyName 'Calibri'
    New-BGInfoChart -Id 'cpu' -Title 'CPU' -Metric CpuPercent -ValueSuffix '%' -Kind Sparkline -MaxPoints 60 -Anchor BottomLeft -OffsetX 20 -OffsetY 20
} -MonitorIndex 0 `
    -FilePath $sampleImage `
    -ConfigurationDirectory $outputDirectory `
    -OutputFileName 'PowerBGInfo.Script.Sample.png' `
    -Target File `
    -PassThru
