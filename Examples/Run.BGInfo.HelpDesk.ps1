<#
    Help-desk wallpaper for shared or front-desk PCs. It tells people how
    to get help and shows the details the service desk asks for first.

    The data block returns plain values; the presentation block only reads
    them. Sample values keep the render reproducible, and each one notes the
    command that supplies it on a real machine.
#>
param(
    [ValidateSet('Light', 'Dark')]
    [string] $Theme = 'Light',
    [int] $Width = 1920,
    [int] $Height = 1080,
    [string] $OutputDirectory = (Join-Path $PSScriptRoot 'Output')
)

Import-Module (Join-Path $PSScriptRoot '..\PowerBGInfo.psd1') -Force

#region Data: your logic goes here
function Get-HelpDeskFacts {
    [pscustomobject] @{
        # (Get-CimInstance Win32_SystemEnclosure).SMBIOSAssetTag
        AssetTag     = 'CT-04217'
        ComputerName = 'FRONTDESK-07'
        Location     = 'Reception, B1'
        # (Get-NetIPConfiguration | Where-Object IPv4DefaultGateway).IPv4Address.IPAddress
        IPAddress    = '10.20.4.37'
        Network      = 'Wired · VLAN 120'
        SignedInAs   = 'Reception (shared)'
        # (Get-CimInstance Win32_OperatingSystem).Caption and .BuildNumber
        Windows      = '11 Enterprise 24H2'
        Build        = '26100'
        # Your monitoring or status-page API
        Services     = @(
            @{ Text = 'Email, Teams, Wi-Fi and VPN are working'; Status = 'Ok' }
            @{ Text = 'Floor 2 printers are offline until 14:00'; Status = 'Warning' }
        )
    }
}

$facts = Invoke-BGInfoDataSource -Name 'Help-desk facts' -TimeoutSeconds 10 -ScriptBlock ${function:Get-HelpDeskFacts} -Default ([pscustomobject] @{
        AssetTag = 'unknown'; ComputerName = $env:COMPUTERNAME; Location = 'unknown'; IPAddress = 'unknown'; Network = ''
        SignedInAs = 'unknown'; Windows = 'unknown'; Build = ''; Services = @(@{ Text = 'Service status is unavailable'; Status = 'Info' })
    })
#endregion

#region Presentation: reads only $facts
$scale = [Math]::Round($Height / 1080, 2)
$tile = @{ SurfaceStyle = 'Glass' }
$tiles = @(
    New-BGInfoVisualCanvasTile @tile -IconKind Shield -Label 'ASSET TAG' -Value $facts.AssetTag -Detail 'Sticker under the keyboard'
    New-BGInfoVisualCanvasTile @tile -IconKind Computer -Label 'COMPUTER NAME' -Value $facts.ComputerName -Detail $facts.Location
    New-BGInfoVisualCanvasTile @tile -IconKind Network -Label 'IP ADDRESS' -Value $facts.IPAddress -Detail $facts.Network
    New-BGInfoVisualCanvasTile @tile -IconKind User -Label 'SIGNED IN AS' -Value $facts.SignedInAs -Detail 'Resets at sign-out'
    New-BGInfoVisualCanvasTile @tile -IconKind OperatingSystem -Label 'WINDOWS' -Value $facts.Windows -Detail "Build $($facts.Build)"
)

New-BGInfo -Theme (New-BGInfoTheme Orchid -Mode $Theme -Scale $scale) -TextPosition MiddleLeft -Target File {
    New-BGInfoBackground -Width $Width -Height $Height
    # The tile column sits vertically centered beside the centered text.
    New-BGInfoLayout -ColumnAlignment Center

    New-BGInfoLabel -Name 'IT SERVICE DESK' -Accent -FontSize ([int](16 * $scale)) -FontWeight 700
    New-BGInfoLabel -Name 'Need help?' -FontSize ([int](88 * $scale)) -FontWeight 600
    New-BGInfoLabel -Name 'Call 555-0100, extension 4357' -Accent -FontSize ([int](40 * $scale)) -FontWeight 600
    New-BGInfoLabel -Name 'Have the asset tag on the right ready.' -Muted -FontSize ([int](22 * $scale))
    New-BGInfoValue -Name 'Portal' -Value 'help.contoso.com'
    New-BGInfoValue -Name 'Teams' -Value '#it-help'
    New-BGInfoValue -Name 'Walk-in' -Value 'Building B, room 012'
    New-BGInfoValue -Name 'Hours' -Value 'Monday–Friday, 07:00–19:00'

    New-BGInfoLabel -Name ' ' -FontSize ([int](12 * $scale))
    New-BGInfoLabel -Name 'SERVICE STATUS' -Accent -FontSize ([int](16 * $scale)) -FontWeight 700
    foreach ($service in $facts.Services) {
        New-BGInfoLabel -Name "●  $($service.Text)" -Status $service.Status -FontSize ([int](19 * $scale)) -FontWeight 600
    }

    New-BGInfoVisualCanvas -Region Right -Tile $tiles
} -ConfigurationDirectory $OutputDirectory `
    -OutputFileName "helpdesk-$($Theme.ToLower()).png"
#endregion
