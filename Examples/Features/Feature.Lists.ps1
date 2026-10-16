<#
    Feature: lists. Bullets, numbered steps, a checklist or status rows.
    Items are strings, or hashtables and objects with Text, Value, Status and
    Checked properties.
#>
param(
    [ValidateSet('Light', 'Dark')]
    [string] $Theme = 'Light',
    [int] $Width = 1920,
    [int] $Height = 1080,
    [string] $OutputDirectory = (Join-Path $PSScriptRoot '..\Output\Features')
)

Import-Module (Join-Path $PSScriptRoot '..\..\PowerBGInfo.psd1') -Force

#region Data: your logic goes here
# Checklist state could come from a file share, Intune compliance or a ticket API.
function Get-ListExampleData {
    [pscustomobject] @{
        Steps     = 'Restart the PC', 'Check the cable or Wi-Fi', 'Call 4357 with the asset tag'
        Checklist = @(
            @{ Text = 'Backups verified'; Checked = $true; Value = '02:10' }
            @{ Text = 'Antivirus signatures'; Checked = $true; Value = '2 h old' }
            @{ Text = 'Disaster-recovery test'; Checked = $false; Value = 'due 30 Oct' }
        )
        Services  = @(
            @{ Text = 'Email and Teams'; Status = 'Ok'; Value = 'working' }
            @{ Text = 'VPN'; Status = 'Warning'; Value = 'slow in EU' }
            @{ Text = 'Floor 2 printers'; Status = 'Critical'; Value = 'offline' }
        )
    }
}

$fallback = [pscustomobject] @{ Steps = @('Call the service desk'); Checklist = @(@{ Text = 'Status unknown'; Checked = $false }); Services = @(@{ Text = 'Service status'; Status = 'Info'; Value = 'unknown' }) }
$data = Invoke-BGInfoDataSource -Name 'List data' -TimeoutSeconds 10 -ScriptBlock ${function:Get-ListExampleData} -Default $fallback
#endregion

#region Presentation: reads only $data
$scale = [Math]::Round($Height / 1080, 2)
New-BGInfo -Theme (New-BGInfoTheme Orchid -Mode $Theme -Scale $scale) -Target File {
    New-BGInfoBackground -Width $Width -Height $Height
    New-BGInfoLabel -Name 'LISTS' -Accent -FontWeight 700
    New-BGInfoLabel -Name 'Steps, checks and status' -FontSize ([int](40 * $scale)) -FontWeight 600

    New-BGInfoList -Region Right -Title 'If something is wrong' -Item $data.Steps -Marker Number
    New-BGInfoList -Region Right -Title 'Nightly checks' -Item $data.Checklist
    New-BGInfoList -Region Right -Title 'Service status' -Item $data.Services
} -ConfigurationDirectory $OutputDirectory -OutputFileName "lists-$($Theme.ToLower())-$Width.png"
#endregion
