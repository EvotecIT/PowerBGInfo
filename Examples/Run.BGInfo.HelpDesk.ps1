<#
    Help-desk wallpaper for shared or front-desk PCs. It tells people how
    to get help and shows the details the service desk asks for first.

    Sample values keep the render reproducible. Each one notes the command
    that supplies it on a real machine.
#>
param(
    [ValidateSet('Light', 'Dark')]
    [string] $Theme = 'Light',
    [int] $Width = 1920,
    [int] $Height = 1080,
    [string] $OutputDirectory = (Join-Path $PSScriptRoot 'Output')
)

Import-Module (Join-Path $PSScriptRoot '..\PowerBGInfo.psd1') -Force
$hudTheme = Join-Path $PSScriptRoot 'Showcase\HudTheme.ps1'
$hud = & $hudTheme -Theme $Theme -Width $Width -Height $Height
$dark = $Theme -eq 'Dark'
$ok, $warn = $hud.Status.Ok, $hud.Status.Warn
$accent = if ($dark) { '#A78BFA' } else { '#7C3AED' }

$background = @{
    Width    = $Width
    Height   = $Height
    Color    = if ($dark) { '#0E0A1A' } else { '#FBF7FF' }
    Gradient = if ($dark) { '#1E1438', '#0A0712' } else {
        '#FFFFFF', '#F1E9FF'
    }
    GradientAngle = 160
    Pattern  = 'Dots'
    PatternColor = if ($dark) { '#FFFFFF12' } else { '#4C1D9512' }
    PatternSpacing = 36
}
$glows = if ($dark) { '#7C3AED55', '#EC489940' } else {
    '#C4B5FD99', '#FBCFE899'
}

$tile = @{ Side = 'Right'; SurfaceStyle = 'Glass'; Accent = $accent }
$tiles = @(
    # Asset tags usually come from the BIOS:
    # (Get-CimInstance Win32_SystemEnclosure).SMBIOSAssetTag
    New-BGInfoVisualCanvasTile @tile -IconKind Shield -Label 'ASSET TAG' `
        -Value 'CT-04217' -Detail 'Sticker under the keyboard'
    New-BGInfoVisualCanvasTile @tile -IconKind Computer `
        -Label 'COMPUTER NAME' -Value 'FRONTDESK-07' -Detail 'Reception, B1'
    New-BGInfoVisualCanvasTile @tile -IconKind Network -Label 'IP ADDRESS' `
        -Value '10.20.4.37' -Detail 'Wired · VLAN 120'
    New-BGInfoVisualCanvasTile @tile -IconKind User -Label 'SIGNED IN AS' `
        -Value 'Reception (shared)' -Detail 'Resets at sign-out'
    New-BGInfoVisualCanvasTile @tile -IconKind OperatingSystem `
        -Label 'WINDOWS' -Value '11 Enterprise 24H2' -Detail 'Build 26100'
)

$text = $hud.Text
$canvas = $hud.Canvas.Clone()
# Center the tile column beside the vertically centered text.
$step = $canvas.TileHeight + $canvas.TileGap
$column = $tiles.Count * $step - $canvas.TileGap
$canvas.RightTileOffsetY += ($Height - $column) / 2 - $hud.MarginY

New-BGInfo -Target File {
    New-BGInfoBackground @background -Glow @(
        New-BGInfoBackgroundGlow -X 0 -Y 0 -Radius 0.6 -Color $glows[0]
        New-BGInfoBackgroundGlow -X 0.75 -Y 1.05 -Radius 0.55 -Color $glows[1]
    )

    New-BGInfoLabel -Name 'IT SERVICE DESK' -Color $accent -FontSize 16 `
        -FontWeight 700
    New-BGInfoLabel -Name 'Need help?' -Color $hud.Ink -FontSize 88 `
        -FontWeight 600
    New-BGInfoLabel -Name 'Call 555-0100, extension 4357' `
        -Color $accent -FontSize 40 -FontWeight 600
    New-BGInfoLabel -Name 'Have the asset tag on the right ready.' `
        -Color $hud.Muted -FontSize 22
    New-BGInfoValue -Name 'Portal' -Value 'help.contoso.com'
    New-BGInfoValue -Name 'Teams' -Value '#it-help'
    New-BGInfoValue -Name 'Walk-in' -Value 'Building B, room 012'
    New-BGInfoValue -Name 'Hours' -Value 'Monday–Friday, 07:00–19:00'

    # Health from your monitoring or status page API.
    New-BGInfoLabel -Name ' ' -FontSize 12
    New-BGInfoLabel -Name 'SERVICE STATUS' -Color $accent -FontSize 16 `
        -FontWeight 700
    New-BGInfoLabel -Name '●  Email, Teams, Wi-Fi and VPN are working' `
        -Color $ok -FontSize 19 -FontWeight 600
    New-BGInfoLabel -Name '●  Floor 2 printers are offline until 14:00' `
        -Color $warn -FontSize 19 -FontWeight 600

    New-BGInfoVisualCanvas @canvas -Tile $tiles -RightTileWidth 460
} @text -TextPosition MiddleLeft `
    -ConfigurationDirectory $OutputDirectory `
    -OutputFileName "helpdesk-$($Theme.ToLower()).png"
