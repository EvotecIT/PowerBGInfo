function New-BGInfo {
    <#
    .SYNOPSIS
    Provides a simple way to create PowerBGInfo configuration.

    .DESCRIPTION
    Provides a simple way to create PowerBGInfo configuration.
    It allows writting useful information on your desktop background.
    Every time the script is run, it will update existing image with new information.

    .PARAMETER BGInfoContent
    Special parameter that works as a scriptblock. It takes input and converts it into configuration.
    By using New-BGInfoLabel and New-BGInfoValue along with other supported PowerShell commands you can create your own configuration.

    .PARAMETER FilePath
    Path to the image that will be used as a background. If not provided current Desktop Background will be used.

    .PARAMETER ConfigurationDirectory
    Path to the directory where configuration will be stored, and where image for desktop background will be placed. If not provided, it will be stored in C:\TEMP

    .PARAMETER FontFamilyName
    Font family name that will be used to display information for Label.
    It's only used if New-BGInfoLabel or New-BGIInfoValue doesn't provide it's own value.
    If ValueFontFamilyName is not provided it will be used as a default value for that property as well

    .PARAMETER Color
    Color that will be used to display information for Label.
    It's only used if New-BGInfoLabel or New-BGIInfoValue doesn't provide it's own value.
    If ValueColor is not provided it will be used as a default value for that property as well

    .PARAMETER FontSize
    Font size that will be used to display information for Label.
    It's only used if New-BGInfoLabel or New-BGIInfoValue doesn't provide it's own value.
    If ValueFontSize is not provided it will be used as a default value for that property as well

    .PARAMETER ValueColor
    Color that will be used to display information for Value.
    It's only used if New-BGInfoLabel or New-BGIInfoValue doesn't provide it's own value.
    If not provided it will be taken from Color property.

    .PARAMETER ValueFontSize
    Font size that will be used to display information for Value.
    It's only used if New-BGInfoLabel or New-BGIInfoValue doesn't provide it's own value.
    If not provided it will be taken from FontSize property.

    .PARAMETER ValueFontFamilyName
    Font family name that will be used to display information for Value.
    It's only used if New-BGInfoLabel or New-BGIInfoValue doesn't provide it's own value.
    If not provided it will be taken from FontFamilyName property.

    .PARAMETER SpaceBetweenLines
    Length of the space between lines

    .PARAMETER SpaceBetweenColumns
    Length of the space between columns (Label and Value)

    .PARAMETER PositionX
    Position of the first column on the X axis.

    .PARAMETER PositionY
    Position of the first column on the Y axis.

    .PARAMETER MonitorIndex
    Index of the monitor that will be used to display the background image. By default it will be 0 (first monitor)

    .PARAMETER WallpaperFit
    WHat to do with the image if it is not the same size as the monitor resolution. It can be one of the following: 'Center', 'Fit', 'Stretch', 'Tile', 'Span', 'Fill'

    .PARAMETER TextPosition
    Where to place the text. It can be one of the following: 'TopLeft', 'TopCenter', 'TopRight', 'MiddleLeft', 'MiddleCenter', 'MiddleRight', 'BottomLeft', 'BottomCenter', 'BottomRight'

    .PARAMETER SpaceX
    Space on the X axis from the edge of the screen

    .PARAMETER SpaceY
    Space on the Y axis from the edge of the screen

    .PARAMETER UseScreenCoordinates
    If set, the script will use screen coordinates instead of image coordinates. This is useful when you have multiple monitors with different resolutions.

    .EXAMPLE
    New-BGInfo -MonitorIndex 0 {
        # Lets add computer name, but lets use builtin values for that
        New-BGInfoValue -BuiltinValue HostName -Color Red -FontSize 20 -FontFamilyName 'Calibri'
        New-BGInfoValue -BuiltinValue FullUserName
        New-BGInfoValue -BuiltinValue CpuName
        New-BGInfoValue -BuiltinValue CpuLogicalCores
        New-BGInfoValue -BuiltinValue RAMSize
        New-BGInfoValue -BuiltinValue RAMSpeed

        # Lets add Label, but without any values, kinf of like section starting
        New-BGInfoLabel -Name "Drives" -Color LemonChiffon -FontSize 16 -FontFamilyName 'Calibri'

        # Lets get all drives and their labels
        foreach ($Disk in (Get-Disk)) {
            $Volumes = $Disk | Get-Partition | Get-Volume
            foreach ($V in $Volumes) {
                New-BGInfoValue -Name "Drive $($V.DriveLetter)" -Value $V.SizeRemaining
            }
        }
    } -FilePath $PSScriptRoot\Samples\PrzemyslawKlysAndKulkozaurr.jpg -ConfigurationDirectory $PSScriptRoot\Output -PositionX 100 -PositionY 100 -WallpaperFit Center

    .NOTES
    General notes
    #>
    [CmdletBinding()]
    param(
        [parameter(Mandatory)][scriptblock] $BGInfoContent,
        [string] $FilePath,
        [parameter(Mandatory)][string] $ConfigurationDirectory,
        [string] $FontFamilyName = 'Calibri',
        [SixLabors.ImageSharp.Color] $Color = [SixLabors.ImageSharp.Color]::Black,
        [int] $FontSize = 16,
        [SixLabors.ImageSharp.Color] $ValueColor = [SixLabors.ImageSharp.Color]::Black,
        [float] $ValueFontSize = 16,
        [string] $ValueFontFamilyName = 'Calibri',
        [int] $SpaceBetweenLines = 10,
        [int] $SpaceBetweenColumns = 30,
        [int] $PositionX = 10,
        [int] $PositionY = 10,
        [int] $MonitorIndex = 0,
        [int] $SpaceX = 10,
        [int] $SpaceY = 10,
        [ValidateSet('Center', 'Fit', 'Stretch', 'Tile', 'Span', 'Fill')][string] $WallpaperFit,
        [ValidateSet('TopLeft', 'TopCenter', 'TopRight', 'MiddleLeft', 'MiddleCenter', 'MiddleRight', 'BottomLeft', 'BottomCenter', 'BottomRight')][string] $TextPosition = 'TopLeft',
        [switch] $UseScreenCoordinates
    )

    $ConfigurationPath = [io.path]::Combine($ConfigurationDirectory, "PowerBGInfoConfiguration.xml")
    if (Test-Path -LiteralPath $ConfigurationPath) {
        $Configuration = Import-Clixml -LiteralPath $ConfigurationPath
    } else {
        $Configuration = [ordered] @{
            OriginalImage = ''
        }
    }

    if ($FilePath -eq "") {
        $WallpaperPath = Get-DesktopWallpaper -Index $MonitorIndex
    } else {
        $WallpaperPath = $FilePath
    }
    if ($WallpaperPath -eq "" -or (Test-Path -LiteralPath $WallpaperPath) -eq $false) {
        Write-Warning -Message "New-BGInfo - Wallpaper ($WallpaperPath) not found. Provide new wallpaper, or make sure one is already set."
        return
    }

    $Configuration['OriginalImage'] = $WallpaperPath

    # Copy wallpaper to use as a base
    $FileName = [io.path]::GetFileName($Configuration['OriginalImage'])
    $FileNameWithoutExtension = [io.path]::GetFileNameWithoutExtension(($Configuration['OriginalImage']))
    $FileNameExtension = [io.path]::GetExtension($FileName)
    $NewFileName = "$($FileNameWithoutExtension)_PowerBgInfo" + $FileNameExtension
    $FilePathOutput = [io.path]::Combine($ConfigurationDirectory, $NewFileName)

    # Wallpaper and output are the same file, if so, we already applied BGInfo at least once
    if ($FilePathOutput -ne $Configuration['OriginalImage'] ) {
        Copy-Item -Path $Configuration['OriginalImage'] -Destination $FilePathOutput -Force
    }
    # We need to check if file exists, because Copy-Item may not do what it's supposed to do
    if ($FilePathOutput -eq "" -or (Test-Path -LiteralPath $FilePathOutput) -eq $false) {
        Write-Warning -Message "New-BGInfo - Wallpaper ($FilePathOutput) not found. Copying failed?"
        return
    }
    # Load the file and get monitor info
    $Image = Get-Image -FilePath $FilePathOutput
    if ($UseScreenCoordinates) {
        $Monitor = Get-DesktopMonitor -Index $MonitorIndex
        $ScreenWidth = $Monitor.PositionRight - $Monitor.PositionLeft
        $ScreenHeight = $Monitor.PositionBottom - $Monitor.PositionTop

        # Handle image scaling based on WallpaperFit
        if ($WallpaperFit -in 'Fill', 'Stretch') {
            $Image.Resize($ScreenWidth, $ScreenHeight)
            $ScaleX = 1
            $ScaleY = 1
        } else {
            # Calculate scaling factors
            $ScaleX = $ScreenWidth / $Image.Width
            $ScaleY = $ScreenHeight / $Image.Height
        }
    } else {
        $ScaleX = 1
        $ScaleY = 1
    }

    $BGContent = & $BGInfoContent

    # Do assessment of the longest text so we can make sure columns are not overlapping
    $HighestWidth = 0
    $HighestHeight = 0
    $HighestValueWidth = 0
    foreach ($Info in $BGContent) {

        if (-not $Info.Color) {
            $Info.Color = $Color
        }
        if (-not $Info.FontSize) {
            $Info.FontSize = $FontSize
        }
        if (-not $Info.FontFamilyName) {
            $Info.FontFamilyName = $FontFamilyName
        }
        if ($Info.Type -ne 'Label') {
            if (-not $Info.ValueColor) {
                if ($Info.Color) {
                    $Info.ValueColor = $Info.Color
                } else {
                    $Info.ValueColor = $ValueColor
                }
            }
            if (-not $Info.ValueFontSize) {
                if ($Info.FontSize) {
                    $Info.ValueFontSize = $Info.FontSize
                } else {
                    $Info.ValueFontSize = $ValueFontSize
                }
            }
            if (-not $Info.ValueFontFamilyName) {
                if ($Info.FontFamilyName) {
                    $Info.ValueFontFamilyName = $Info.FontFamilyName
                } else {
                    $Info.ValueFontFamilyName = $ValueFontFamilyName
                }
            }
        }
        $SizeOfText = $Image.GetTextSize($Info.Name, $Info.FontSize, $Info.FontFamilyName)
        if ($SizeOfText.Width -gt $HighestWidth) {
            $HighestWidth = $SizeOfText.Width
        }
        if ($SizeOfText.Height -gt $HighestHeight) {
            $HighestHeight = $SizeOfText.Height
        }
        # Calculate the width of value column
        if ($Info.Type -ne 'Label') {
            $ValueSize = $Image.GetTextSize($Info.Value, $Info.ValueFontSize, $Info.ValueFontFamilyName)
            if ($ValueSize.Width -gt $HighestValueWidth) {
                $HighestValueWidth = $ValueSize.Width
            }
        }
    }

    # Calculate total width needed for both columns
    $TotalWidth = $HighestWidth + $SpaceBetweenColumns + $HighestValueWidth

    # Calculate positions with scaling if using screen coordinates
    if ($UseScreenCoordinates) {
        if ($TextPosition -eq 'TopLeft') {
            $StartX = $SpaceX / $ScaleX
            $PositionY = $SpaceY / $ScaleY
        } elseif ($TextPosition -eq 'TopCenter') {
            $StartX = ($ScreenWidth / 2 - $TotalWidth * $ScaleX / 2) / $ScaleX
            $PositionY = $SpaceY / $ScaleY
        } elseif ($TextPosition -eq 'TopRight') {
            $StartX = ($ScreenWidth - $TotalWidth * $ScaleX - $SpaceX) / $ScaleX
            $PositionY = $SpaceY / $ScaleY
        } elseif ($TextPosition -eq 'MiddleLeft') {
            $StartX = $SpaceX / $ScaleX
            $PositionY = ($ScreenHeight / 2 - ($BGContent.Count * ($HighestHeight + $SpaceBetweenLines)) * $ScaleY / 2) / $ScaleY
        } elseif ($TextPosition -eq 'MiddleCenter') {
            $StartX = ($ScreenWidth / 2 - $TotalWidth * $ScaleX / 2) / $ScaleX
            $PositionY = ($ScreenHeight / 2 - ($BGContent.Count * ($HighestHeight + $SpaceBetweenLines)) * $ScaleY / 2) / $ScaleY
        } elseif ($TextPosition -eq 'MiddleRight') {
            $StartX = ($ScreenWidth - $TotalWidth * $ScaleX - $SpaceX) / $ScaleX
            $PositionY = ($ScreenHeight / 2 - ($BGContent.Count * ($HighestHeight + $SpaceBetweenLines)) * $ScaleY / 2) / $ScaleY
        } elseif ($TextPosition -eq 'BottomLeft') {
            $StartX = $SpaceX / $ScaleX
            $PositionY = ($ScreenHeight - ($BGContent.Count * ($HighestHeight + $SpaceBetweenLines)) * $ScaleY - $SpaceY) / $ScaleY
        } elseif ($TextPosition -eq 'BottomCenter') {
            $StartX = ($ScreenWidth / 2 - $TotalWidth * $ScaleX / 2) / $ScaleX
            $PositionY = ($ScreenHeight - ($BGContent.Count * ($HighestHeight + $SpaceBetweenLines)) * $ScaleY - $SpaceY) / $ScaleY
        } elseif ($TextPosition -eq 'BottomRight') {
            $StartX = ($ScreenWidth - $TotalWidth * $ScaleX - $SpaceX) / $ScaleX
            $PositionY = ($ScreenHeight - ($BGContent.Count * ($HighestHeight + $SpaceBetweenLines)) * $ScaleY - $SpaceY) / $ScaleY
        }
    } else {
        # Use existing image-based positioning code
        if ($TextPosition -eq 'TopLeft') {
            $StartX = $SpaceX
            $PositionY = $SpaceY
        } elseif ($TextPosition -eq 'TopCenter') {
            $StartX = ($Image.Width / 2) - ($TotalWidth / 2)
            $PositionY = $SpaceY
        } elseif ($TextPosition -eq 'TopRight') {
            $StartX = $Image.Width - $TotalWidth - $SpaceX
            $PositionY = $SpaceY
        } elseif ($TextPosition -eq 'MiddleLeft') {
            $StartX = $SpaceX
            $PositionY = ($Image.Height / 2) - (($BGContent.Count * ($HighestHeight + $SpaceBetweenLines)) / 2)
        } elseif ($TextPosition -eq 'MiddleCenter') {
            $StartX = ($Image.Width / 2) - ($TotalWidth / 2)
            $PositionY = ($Image.Height / 2) - (($BGContent.Count * ($HighestHeight + $SpaceBetweenLines)) / 2)
        } elseif ($TextPosition -eq 'MiddleRight') {
            $StartX = $Image.Width - $TotalWidth - $SpaceX
            $PositionY = ($Image.Height / 2) - (($BGContent.Count * ($HighestHeight + $SpaceBetweenLines)) / 2)
        } elseif ($TextPosition -eq 'BottomLeft') {
            $StartX = $SpaceX
            $PositionY = $Image.Height - ($BGContent.Count * ($HighestHeight + $SpaceBetweenLines)) - $SpaceY
        } elseif ($TextPosition -eq 'BottomCenter') {
            $StartX = ($Image.Width / 2) - ($TotalWidth / 2)
            $PositionY = $Image.Height - ($BGContent.Count * ($HighestHeight + $SpaceBetweenLines)) - $SpaceY
        } elseif ($TextPosition -eq 'BottomRight') {
            $StartX = $Image.Width - $TotalWidth - $SpaceX
            $PositionY = $Image.Height - ($BGContent.Count * ($HighestHeight + $SpaceBetweenLines)) - $SpaceY
        }
    }

    $PositionX = $StartX

    # Add text
    foreach ($Info in $BGContent) {
        if ($Info.Type -eq 'Label') {
            $Image.AddText($PositionX, $PositionY, $Info.Name, $Info.Color, $Info.FontSize, $Info.FontFamilyName)
        } else {
            $Image.AddText($PositionX, $PositionY, $Info.Name, $Info.Color, $Info.FontSize, $Info.FontFamilyName)
            $Image.AddText($PositionX + $HighestWidth + $SpaceBetweenColumns, $PositionY, $Info.Value, $Info.ValueColor, $Info.ValueFontSize, $Info.ValueFontFamilyName)
        }
        $PositionY += $HighestHeight + $SpaceBetweenLines
    }
    # lets now save image after modifications
    Save-Image -Image $Image -FilePath $FilePathOutput

    # finally lets set the image as wallpapeer
    if ($WallpaperFit) {
        Set-DesktopWallpaper -Index $MonitorIndex -FilePath $FilePathOutput -Position $WallpaperFit
    } else {
        Set-DesktopWallpaper -Index $MonitorIndex -FilePath $FilePathOutput
    }

    # lets export configuration, so we know what was done
    $Configuration | Export-Clixml -LiteralPath $ConfigurationPath -Force
}