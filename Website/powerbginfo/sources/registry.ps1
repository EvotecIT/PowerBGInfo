function Get-MyWallpaperData {
    $key = 'HKLM:\SOFTWARE\Microsoft\Windows NT\CurrentVersion'
    $windows = Get-ItemProperty -Path $key
    # Your own values, written by the build or the asset process.
    $asset = Get-ItemProperty -Path 'HKLM:\SOFTWARE\Contoso\Asset' `
        -ErrorAction SilentlyContinue

    [pscustomobject] @{
        Release = $windows.DisplayVersion
        Build = '{0}.{1}' -f $windows.CurrentBuild, $windows.UBR
        Team = if ($asset) { $asset.Department } else { 'unassigned' }
        AssetTag = if ($asset) { $asset.Tag } else { 'unknown' }
    }
}
