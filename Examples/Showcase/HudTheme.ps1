<#
    Shared HUD splat tables for scripts written before New-BGInfoTheme.

    New scripts can pass -Theme to New-BGInfo instead: every cmdlet then picks
    up these colors, panels and type sizes without splatting. This file now
    reads its values from New-BGInfoTheme, so both routes give the same look.
#>
param(
    [ValidateSet('Light', 'Dark')]
    [string] $Theme = 'Dark',
    [int] $Width = 1920,
    [int] $Height = 1080
)

# Import PowerBGInfo before running this file.
$look = New-BGInfoTheme -Preset Graphite -Mode $Theme

# Visual canvas rails start at 4.5 % from the sides and 16 % from the top.
# These offsets move them onto the shared margin.
$railX = $look.MarginX - [Math]::Min(132, [Math]::Round($Width * 0.045))
$railY = $look.MarginY - [Math]::Min(190, [Math]::Round($Height * 0.16))

# The glass panel every chart and diagram sits on.
$surface = @{
    ThemeMode       = $Theme
    BackgroundColor = $look.Surface
    BorderColor     = $look.SurfaceBorder
    CornerRadius    = $look.CornerRadius
    Padding         = $look.Padding
    TitleColor      = $look.Muted
    TitleFontSize   = $look.TitleFontSize
    TitleFontWeight = $look.TitleFontWeight
    TitleTextCase   = $look.TitleTextCase
}

@{
    Theme   = $Theme
    Ink     = $look.Ink
    Muted   = $look.Muted
    Status  = @{
        Ok   = $look.Ok
        Warn = $look.Warning
        Bad  = $look.Critical
        Info = $look.Info
    }
    MarginX = $look.MarginX
    MarginY = $look.MarginY

    # Classic label/value text: New-BGInfo @hud.Text
    Text    = @{
        FontFamilyName      = $look.FontFamilyName
        ValueFontFamilyName = $look.FontFamilyName
        Color               = $look.Muted
        ValueColor          = $look.Ink
        FontSize            = [int] $look.FontSize
        ValueFontSize       = $look.FontSize
        SpaceBetweenLines   = $look.LineSpacing
        SpaceBetweenColumns = $look.ColumnSpacing
        SpaceX              = $look.MarginX
        SpaceY              = $look.MarginY
    }

    # Chart panels: New-BGInfoChart @hud.Panel
    Panel   = $surface + @{
        ValueColor    = $look.Ink
        ValueFontSize = $look.TitleFontSize
        GridColor     = $look.GridColor
        NoHistory     = $true
    }

    # Topology panels: New-BGInfoTopology @hud.Diagram
    Diagram = $surface.Clone()

    # Visual canvas tiles: New-BGInfoVisualCanvas @hud.Canvas
    Canvas  = @{
        ThemeMode        = $Theme
        NoHeroContent    = $true
        NoTechBackdrop   = $true
        TileGlassTop     = $look.TileGlassTop
        TileGlassBottom  = $look.TileGlassBottom
        TileLabelColor   = $look.Muted
        TileValueColor   = $look.Ink
        TileDetailColor  = $look.Muted
        TileHeight       = $look.TileHeight
        TileGap          = $look.TileGap
        LeftTileOffsetX  = $railX
        RightTileOffsetX = $railX
        LeftTileOffsetY  = $railY
        RightTileOffsetY = $railY
    }
}
