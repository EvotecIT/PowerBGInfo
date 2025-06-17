function Set-LogonScreen {
    <#
    .SYNOPSIS
    Sets the Windows logon screen background image.

    .DESCRIPTION
    Sets the Windows logon screen background image by modifying registry settings.
    #>
    [CmdletBinding()]
    param(
        [Parameter(Mandatory)][string] $FilePath
    )

    $RegPath = 'HKLM:\SOFTWARE\Microsoft\Windows\CurrentVersion\PersonalizationCSP'
    if (-not (Test-Path -Path $RegPath)) {
        New-Item -Path $RegPath -Force | Out-Null
    }

    # Convert file path to the correct format
    $FullPath = [System.IO.Path]::GetFullPath($FilePath)

    # Set registry values
    Set-ItemProperty -Path $RegPath -Name 'LockScreenImagePath' -Value $FullPath -Type String -Force
    Set-ItemProperty -Path $RegPath -Name 'LockScreenImageUrl' -Value $FullPath -Type String -Force
    Set-ItemProperty -Path $RegPath -Name 'LockScreenImageStatus' -Value 1 -Type DWord -Force
}
