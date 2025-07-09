function New-BGInfoLabel {
    <#
    .SYNOPSIS
    Provides ability to set label without value. It can be used to separate different sections of information.

    .DESCRIPTION
    Provides ability to set label without value. It can be used to separate different sections of information.

    .PARAMETER Name
    Name of the label/section

    .PARAMETER Color
    Color for the label. If not provided it will be taken from the parent New-BGInfo command.

    .PARAMETER FontSize
    Font size for the label. If not provided it will be taken from the parent New-BGInfo command.

    .PARAMETER FontFamilyName
    Font family name for the label. If not provided it will be taken from the parent New-BGInfo command.

    .EXAMPLE
    # Lets add Label, but without any values, kinf of like section starting
    New-BGInfoLabel -Name "Drives" -Color LemonChiffon -FontSize 16 -FontFamilyName 'Calibri'

    .NOTES
    General notes
    #>
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