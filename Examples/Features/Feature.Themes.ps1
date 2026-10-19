<#
    Feature: themes. One parameter gives the text, panels, charts, tiles and the
    backdrop one look. Every PowerBGInfo cmdlet inside New-BGInfo -Theme uses the
    theme for the parameters you leave out; parameters you set always win.

    Presets: Graphite, Midnight, Aurora, Ember, Orchid, Corporate (Light and Dark).
#>
param(
    [ValidateSet('Graphite', 'Midnight', 'Aurora', 'Ember', 'Orchid', 'Corporate')]
    [string] $Preset = 'Aurora',
    [ValidateSet('Light', 'Dark')]
    [string] $Theme = 'Dark',
    [int] $Width = 1920,
    [int] $Height = 1080,
    [string] $OutputDirectory = (Join-Path $PSScriptRoot '..\Output\Features')
)

Import-Module (Join-Path $PSScriptRoot '..\..\PowerBGInfo.psd1') -Force

#region Data: your logic goes here
# Replace the body with your own sources: CIM, registry, AD, a REST API or a
# monitoring export. Return plain values. Sample values keep the render reproducible.
function Get-ThemeExampleData {
    [pscustomobject] @{
        HostName = 'APP-WEB-02'
        Role     = 'IIS front end · pool B'
        Owner    = 'Web platform team'
        Cpu      = @(22, 31, 28, 44, 38, 35, 41, 37)
        Requests = '1,240 / min'
    }
}

# A slow or broken source must never block logon: time-box it and fall back.
$data = Invoke-BGInfoDataSource -Name 'Theme example data' -TimeoutSeconds 10 `
    -ScriptBlock ${function:Get-ThemeExampleData} -Default ([pscustomobject] @{
        HostName = $env:COMPUTERNAME; Role = 'unknown'; Owner = 'unknown'; Cpu = @(0); Requests = 'unknown'
    })
#endregion

#region Presentation: reads only $data
# Scale type and spacing with the screen: 1.0 at 1080 lines, 1.33 at 1440.
$look = New-BGInfoTheme -Preset $Preset -Mode $Theme -Scale ([Math]::Round($Height / 1080, 2))

New-BGInfo -Theme $look -Target File {
    New-BGInfoBackground -Width $Width -Height $Height

    New-BGInfoLabel -Name "$($Preset.ToUpper()) THEME  ·  $($Theme.ToUpper())" -Accent -FontWeight 700
    New-BGInfoLabel -Name $data.HostName -FontSize ([int](64 * $look.Scale)) -FontWeight 600
    New-BGInfoValue -Name 'Role' -Value $data.Role
    New-BGInfoValue -Name 'Owner' -Value $data.Owner

    New-BGInfoMetric -Region Right -Height ([int](150 * $look.Scale)) -Label 'Requests' -Value $data.Requests `
        -Caption 'last 5 minutes' -Status Ok
    New-BGInfoChart -Region Right -Height ([int](240 * $look.Scale)) -Title 'CPU' -Kind Area -Values $data.Cpu `
        -ValueSuffix ' %' -ShowGrid -GridLineCount 3 -NoHistory
} -ConfigurationDirectory $OutputDirectory `
    -OutputFileName "themes-$($Preset.ToLower())-$($Theme.ToLower())-$Width.png"
#endregion
