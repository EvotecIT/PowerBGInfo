<#
    Shared HUD language for the showcase wallpapers.

    Every scenario splats these tables so charts, tiles and text share one
    look: the same panel glass, corner radius, typography and status colors.
    Scenarios only choose their accent and background.
#>
param(
    [ValidateSet('Light', 'Dark')]
    [string] $Theme = 'Dark',
    [int] $Width = 1920,
    [int] $Height = 1080
)

$dark = $Theme -eq 'Dark'

# Every block keeps the same outer margin from the screen edge.
$marginX = 64
$marginY = 56

# Visual canvas rails start at 4.5 % from the sides and 16 % from the top.
# These offsets move them onto the shared margin.
$railX = $marginX - [Math]::Min(132, [Math]::Round($Width * 0.045))
$railY = $marginY - [Math]::Min(190, [Math]::Round($Height * 0.16))

$ink = if ($dark) { '#F1F5F9' } else { '#0F172A' }
$muted = if ($dark) { '#94A3B8' } else { '#475569' }
$status = if ($dark) {
    @{ Ok = '#34D399'; Warn = '#FBBF24'; Bad = '#F87171'; Info = '#60A5FA' }
} else {
    @{ Ok = '#059669'; Warn = '#D97706'; Bad = '#DC2626'; Info = '#2563EB' }
}

# The glass panel every chart and diagram sits on.
$surface = @{
    ThemeMode       = $Theme
    BackgroundColor = if ($dark) { '#0B1222C2' } else { '#FFFFFFC7' }
    BorderColor     = if ($dark) { '#FFFFFF1F' } else { '#0F172A1A' }
    CornerRadius    = 18
    Padding         = 20
    TitleColor      = $muted
    TitleFontSize   = 15
    TitleFontWeight = 600
    TitleTextCase   = 'Uppercase'
}

@{
    Theme   = $Theme
    Ink     = $ink
    Muted   = $muted
    Status  = $status
    MarginX = $marginX
    MarginY = $marginY

    # Classic label/value text: New-BGInfo @hud.Text
    Text   = @{
        FontFamilyName      = 'Segoe UI'
        ValueFontFamilyName = 'Segoe UI'
        Color               = $muted
        ValueColor          = $ink
        FontSize            = 17
        ValueFontSize       = 17
        SpaceBetweenLines   = 8
        SpaceBetweenColumns = 28
        SpaceX              = $marginX
        SpaceY              = $marginY
    }

    # Chart panels: New-BGInfoChart @hud.Panel
    Panel  = $surface + @{
        ValueColor    = $ink
        ValueFontSize = 15
        GridColor     = if ($dark) { '#FFFFFF33' } else { '#0F172A26' }
        NoHistory     = $true
    }

    # Topology panels: New-BGInfoTopology @hud.Diagram
    Diagram = $surface.Clone()

    # Visual canvas tiles: New-BGInfoVisualCanvas @hud.Canvas
    Canvas = @{
        ThemeMode        = $Theme
        NoHeroContent    = $true
        NoTechBackdrop   = $true
        TileGlassTop     = if ($dark) { '#16213ACC' } else { '#FFFFFFEB' }
        TileGlassBottom  = if ($dark) { '#0B1222E6' } else { '#F1F5F9E0' }
        TileLabelColor   = $muted
        TileValueColor   = $ink
        TileDetailColor  = $muted
        TileHeight       = 116
        TileGap          = 16
        LeftTileOffsetX  = $railX
        RightTileOffsetX = $railX
        LeftTileOffsetY  = $railY
        RightTileOffsetY = $railY
    }
}
