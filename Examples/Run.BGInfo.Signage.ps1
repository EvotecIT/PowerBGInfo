<#
    Kiosk or digital signage. One message, large enough to read from across
    the lobby, and a code to scan for the details. No clock, no metrics.

    Landscape screens put the code beside the message; portrait screens put
    it underneath. The data block returns plain values; the presentation
    block only reads them.
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
function Get-SignageMessage {
    # A list on your intranet, a JSON file on a share, or a CMS feed:
    #   Invoke-RestMethod https://intranet.contoso.com/api/signage/lobby-c
    [pscustomobject] @{
        Place    = 'CONTOSO INNOVATION HUB  ·  BUILDING C'
        Headline = 'Open day'
        When     = 'Saturday 24 October, 10:00–16:00'
        Body     = 'Lab tours, live demos and a coding corner for kids.'
        Link     = 'https://events.contoso.com/open-day'
        Program  = @(
            @{ Text = 'Robotics lab tour'; Value = '10:30' }
            @{ Text = 'Rooftop drone show'; Value = '11:30' }
            @{ Text = 'Keynote in hall C1'; Value = '13:00' }
            @{ Text = 'Ask an engineer'; Value = '14:00' }
            @{ Text = 'Kids coding corner'; Value = '15:00' }
        )
    }
}

$fallback = [pscustomobject] @{
    Place = 'CONTOSO'; Headline = 'Welcome'; When = 'Details unavailable'
    Body = 'Please contact reception for current information.'
    Link = 'https://www.contoso.com'; Program = @()
}
$source = @{
    Name           = 'Signage message'
    TimeoutSeconds = 10
    ScriptBlock    = ${function:Get-SignageMessage}
    Default        = $fallback
}
$sign = Invoke-BGInfoDataSource @source
#endregion

#region Presentation: reads only $sign
$portrait = $Height -gt $Width
$scale = [Math]::Round([Math]::Min($Width, $Height) / 1080, 2)
$look = New-BGInfoTheme Corporate -Mode $Theme -Scale $scale
$link = [uri] $sign.Link
$glow = if ($Theme -eq 'Dark') { '#38BDF844' } else { '#7DD3FC88' }
$panels = if ($portrait) { 'Bottom' } else { 'Right' }
# On a portrait screen the code and the list sit side by side, equally tall.
$listHeight = if ($portrait) { [int](480 * $scale) } else { 0 }
# Portrait: the message sits in the open space above the panels.
$text = if ($portrait) { 'TopLeft' } else { 'MiddleLeft' }
$top = if ($portrait) { [int](360 * $scale) } else { $look.MarginY }

New-BGInfo -Theme $look -TextPosition $text -SpaceY $top -Target File {
    New-BGInfoBackground -Width $Width -Height $Height -Pattern None `
        -GradientAngle 160 -Glow @(
        New-BGInfoBackgroundGlow -X 0.15 -Y 0.2 -Radius 0.55 -Color $glow
        New-BGInfoBackgroundGlow -X 0.95 -Y 0.9 -Radius 0.5 -Color $glow
    )
    New-BGInfoLayout -ColumnAlignment Center `
        -ColumnWidth ([int](420 * $scale)) -BandHeight ([int](520 * $scale))

    New-BGInfoLabel -Name $sign.Place -Accent -FontSize ([int](20 * $scale)) `
        -FontWeight 700
    New-BGInfoLabel -Name $sign.Headline -FontSize ([int](190 * $scale)) `
        -FontWeight 700
    New-BGInfoLabel -Name $sign.When -Accent -FontSize ([int](44 * $scale)) `
        -FontWeight 600
    New-BGInfoLabel -Name $sign.Body -Muted -FontSize ([int](30 * $scale))

    New-BGInfoQRCode -Region $panels -Url $sign.Link -Title 'Scan for the day' `
        -Caption "$($link.Host)$($link.AbsolutePath)" `
        -Width ([int](420 * $scale)) -Height ([int](480 * $scale))
    # Signage is read from a distance: the list renders at a larger density.
    if (@($sign.Program).Count -gt 0) {
        New-BGInfoList -Region $panels -Title 'Highlights' -Item $sign.Program `
            -Width ([int](420 * $scale)) -Height $listHeight -Scale (1.8 * $scale)
    }
} -ConfigurationDirectory $OutputDirectory `
    -OutputFileName "signage-$($Theme.ToLower()).png"
#endregion
