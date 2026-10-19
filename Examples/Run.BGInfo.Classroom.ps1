<#
    Lab or classroom wallpaper. The seat name is readable from the back of
    the room, students see when the machine resets, and the instructor sees
    lab progress at a glance.

    Sample values keep the render reproducible. Replace them with your
    lab schedule, timetable or LMS data.
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
$accent = if ($dark) { '#FB923C' } else { '#EA580C' }
$second = if ($dark) { '#F472B6' } else { '#DB2777' }

$background = @{
    Width         = $Width
    Height        = $Height
    Color         = if ($dark) { '#120A0A' } else { '#FFF7F0' }
    Gradient      = if ($dark) { '#2A1208', '#0B0710' } else {
        '#FFFBF5', '#FFE4E1'
    }
    GradientAngle = 120
}
$glows = if ($dark) { '#F9731655', '#DB277740', '#7C3AED33' } else {
    '#FDBA7499', '#F9A8D488', '#C4B5FD66'
}

$text = $hud.Text.Clone()
$text.SpaceY = 120
$panel = $hud.Panel
$row = @{ Height = 250; Anchor = 'BottomCenter'; OffsetY = $hud.MarginY }

New-BGInfo -Target File {
    New-BGInfoBackground @background -Glow @(
        New-BGInfoBackgroundGlow -X 0.1 -Y 0.1 -Radius 0.5 -Color $glows[0]
        New-BGInfoBackgroundGlow -X 0.95 -Y 0.3 -Radius 0.45 -Color $glows[1]
        New-BGInfoBackgroundGlow -X 0.5 -Y 1.1 -Radius 0.6 -Color $glows[2]
    )

    New-BGInfoLabel -Name 'LAB B  ·  INTRODUCTION TO POWERSHELL' `
        -Color $accent -FontSize 18 -FontWeight 700
    New-BGInfoLabel -Name 'LAB-B-14' -Color $hud.Ink -FontSize 150 `
        -FontWeight 700
    New-BGInfoLabel -Name 'This PC resets every night at 02:00.' `
        -Color $hud.Ink -FontSize 30 -FontWeight 600
    New-BGInfoLabel -Name 'Save your work to OneDrive before you leave.' `
        -Color $hud.Muted -FontSize 24
    New-BGInfoLabel -Name ' ' -FontSize 10
    New-BGInfoValue -Name 'Instructor' -Value 'Anna Novak'
    New-BGInfoValue -Name 'Wi-Fi' -Value 'CAMPUS-LAB, sign in with your ID'
    New-BGInfoValue -Name 'Need a hand?' -Value 'Raise your hand or call 4357'

    New-BGInfoChart @panel @row -Id 'time' -Title 'Session time left' `
        -Kind Gauge -Values 35 -Maximum 90 -ValueSuffix ' min' `
        -LineColor $accent -ShowLatestValue:$false `
        -Width 360 -OffsetX -416
    New-BGInfoChart @panel @row -Id 'exercises' -Title 'Your exercises' `
        -Kind Pictorial -Values 3 -Labels 'Completed' -Palette $second `
        -PictorialSymbol Star -PictorialColumns 5 -Maximum 5 `
        -ShowDataLabels -ShowLatestValue:$false `
        -Width 420 -OffsetX 0
    # Students who finished each exercise, from the LMS gradebook.
    New-BGInfoChart @panel @row -Id 'class' -Title 'Class · exercises 1–5' `
        -Kind Bar -Values 26, 24, 19, 9, 2 -LineColor $second `
        -ShowGrid -GridLineCount 3 -ShowLatestValue:$false `
        -Width 360 -OffsetX 416
} @text -TextPosition TopCenter `
    -ConfigurationDirectory $OutputDirectory `
    -OutputFileName "classroom-$($Theme.ToLower()).png"
