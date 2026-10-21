<#
.SYNOPSIS
Renders a wallpaper using portable machine facts and explicit image dimensions.
.PARAMETER Width
Output width in pixels.
.PARAMETER Height
Output height in pixels.
.PARAMETER Target
File generates a preview. Wallpaper applies it to the current desktop session.
.PARAMETER OutputDirectory
Directory for the generated image and bounded chart history.
.EXAMPLE
./Run.BGInfo.CrossPlatform.ps1 -Width 1920 -Height 1080
.EXAMPLE
./Run.BGInfo.CrossPlatform.ps1 -Width 1080 -Height 1920 -Target File
.OUTPUTS
The generated image path.
#>
[CmdletBinding()]
param(
    [ValidateRange(320, 16384)]
    [int] $Width = 1920,
    [ValidateRange(320, 16384)]
    [int] $Height = 1080,
    [ValidateSet('File', 'Wallpaper')]
    [string] $Target = 'File',
    [string] $OutputDirectory = (Join-Path ([IO.Path]::GetTempPath()) 'PowerBGInfo-CrossPlatform')
)

Import-Module PowerBGInfo -ErrorAction Stop
$facts = Get-BGInfoSystemInfo

New-BGInfo -Theme Graphite -Target $Target -ConfigurationDirectory $OutputDirectory -OutputFileName 'machine.png' {
    New-BGInfoBackground -Width $Width -Height $Height
    New-BGInfoLabel -Name $facts.ComputerName -FontSize 56
    New-BGInfoValue -Name 'OS' -Value $facts.OSName
    New-BGInfoValue -Name 'User' -Value $facts.UserName
    New-BGInfoValue -Name 'Uptime' -Value $facts.UptimeText
    New-BGInfoValue -Name 'IP address' -Value $facts.IPv4Address
    $facts.Disks | New-BGInfoTable -Region Right -Title 'Volumes' -Property Drive, FreeGB, UsedPercent
    New-BGInfoChart -Region Bottom -Title 'Memory used' -Kind ProgressBar -Metric MemoryPercent -Maximum 100 -NoHistory
}
