function New-BGInfoValue {
    [CmdletBinding()]
    param(
        [string] $Name,
        [string] $Value,
        [SixLabors.ImageSharp.Color] $Color,
        [float] $FontSize,
        [string] $FontFamilyName
    )

    [PSCustomObject] @{
        Type           = 'Values'
        Name           = $Name
        Value          = $Value
        Color          = $Color
        FontSize       = $FontSize
        FontFamilyName = $FontFamilyName
    }
}