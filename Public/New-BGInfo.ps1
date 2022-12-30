function New-BGInfo {
    [CmdletBinding()]
    param(
        [parameter(Mandatory)][scriptblock] $BGInfoContent,
        [string] $FilePath,
        [parameter(Mandatory)][string] $ConfigurationDirectory,
        [string] $FamilyName = 'Calibri',
        [int] $FontSize = 16,
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
        Write-Verbose -Message "New-BGInfo - Wallpaper ($WallpaperPath) already has BGInfo applied. Skipping."
    } else {
        $Configuration['OriginalImage'] = $WallpaperPath
    }

    # Copy wallpaper to use as a base
    $FileName = [io.path]::GetFileName($Configuration['OriginalImage'])
    $FilePathOutput = [io.path]::Combine($ConfigurationDirectory, $FileName)
    #$FilePathOutputOriginal = [io.path]::Combine($ConfigurationDirectory, "Original_$FileName")

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
            $Image.AddText($PositionX + $HighestWidth + $SpaceBetweenColumns, $PositionY, $Info.Value, $Info.Color, $Info.FontSize, $Info.FontFamilyName)

        }
        $PositionY += $HighestHeight + $SpaceBetweenLines
    }

    Save-Image -Image $Image -FilePath $FilePathOutput

    if ($WallpaperFit) {
        Set-DesktopWallpaper -Index $MonitorIndex -FilePath $FilePathOutput -Position $WallpaperFit
    } else {
        Set-DesktopWallpaper -Index $MonitorIndex -FilePath $FilePathOutput
    }

    $Configuration | Export-Clixml -LiteralPath $ConfigurationPath -Force
}