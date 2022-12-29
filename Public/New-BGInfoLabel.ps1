function New-BGInfoLabel {
    [CmdletBinding()]
    param(
        [string] $Name,
        [SixLabors.ImageSharp.Color] $Color,
        [float] $FontSize,
        [string] $FontFamilyName
    )
    [PSCustomObject] @{
        Type           = 'Label'
        Name           = $Name
        Color          = $Color
        FontSize       = $FontSize
        FontFamilyName = $FontFamilyName
    }
}