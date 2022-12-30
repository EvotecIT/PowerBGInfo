function New-BGInfo {
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
        [ValidateSet('Center', 'Fit', 'Stretch', 'Tile', 'Span', 'Fill')][string] $WallpaperFit
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

    if ($Configuration['OriginalImage'] -ne "") {
        Write-Verbose -Message "New-BGInfo - Wallpaper ($WallpaperPath) already has BGInfo applied, reusing what is set."
    } else {
        $Configuration['OriginalImage'] = $WallpaperPath
    }

    # Copy wallpaper to use as a base
    $FileName = [io.path]::GetFileName($Configuration['OriginalImage'])
    $FilePathOutput = [io.path]::Combine($ConfigurationDirectory, $FileName)

    # Wallpaper and output are the same file, if so, we already applied BGInfo at least once
    if ($FilePathOutput -ne $Configuration['OriginalImage'] ) {
        Copy-Item -Path $Configuration['OriginalImage'] -Destination $FilePathOutput -Force
    }
    # We need to check if file exists, because Copy-Item may not do what it's supposed to do
    if ($FilePathOutput -eq "" -or (Test-Path -LiteralPath $FilePathOutput) -eq $false) {
        Write-Warning -Message "New-BGInfo - Wallpaper ($FilePathOutput) not found. Copying failed?"
        return
    }
    # Load the file
    $Image = Get-Image -FilePath $FilePathOutput

    $BGContent = & $BGInfoContent

    # Do assesment of the longest text so we can make sure columns are not overlapping
    $HighestWidth = 0
    $HighestHeight = 0
    foreach ($Info in $BGContent) {

        if ($Info.Color) {
            #$SetColor = $Info.Color
        } else {
            $Info.Color = $Color
        }
        if ($Info.FontSize) {
            #$SetFontSize = $Info.FontSize
        } else {
            $Info.FontSize = $FontSize
        }
        if ($Info.FontFamilyName) {
            #$SetFontFamilyName = $Info.FontFamilyName
        } else {
            $Info.FontFamilyName = $FontFamilyName
        }
        if ($Info.Type -ne 'Label') {
            if ($Info.ValueColor) {
                #$SetValueColor = $Info.ValueColor
            } else {
                if ($Info.Color) {
                    $Info.ValueColor = $Info.Color
                } else {
                    $Info.ValueColor = $ValueColor
                }
            }
            if ($Info.ValueFontSize) {
                #$SetValueFontSize = $Info.ValueFontSize
            } else {
                if ($Info.FontSize) {
                    $Info.ValueFontSize = $Info.FontSize
                } else {
                    $Info.ValueFontSize = $ValueFontSize
                }
            }
            if ($Info.ValueFontFamilyName) {
                # $SetValueFontFamilyName = $Info.ValueFontFamilyName
            } else {
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
    }
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