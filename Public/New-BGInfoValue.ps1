function New-BGInfoValue {
    <#
    .SYNOPSIS
    Special function that provides a way to create a value that will be displayed on the background image.

    .DESCRIPTION
    Special function that provides a way to create a value that will be displayed on the background image.
    It allows using builtin values, or custom values depending on user needs.

    .PARAMETER Name
    Label that will be used on the left side of the value.

    .PARAMETER Value
    Cystom Value that will be displayed on the right side of the label.

    .PARAMETER BuiltinValue
    Builtin value that will be displayed on the right side of the label. It can be one of the following:
    - UserName - Current user name
    - HostName - Current host name
    - FullUserName - Current user name with domain
    - CpuName - CPU name
    - CpuMaxClockSpeed - CPU max clock speed
    - CpuCores - CPU cores
    - CpuLogicalCores - CPU logical cores
    - RAMSize - RAM size
    - RAMSpeed - RAM speed
    - RAMPartNumber - RAM part number
    - BiosVersion - BIOS version
    - BiosManufacturer - BIOS manufacturer
    - BiosReleaseDate - BIOS release date
    - OSName - OS name
    - OSVersion - OS version
    - OSArchitecture - OS architecture
    - OSBuild - OS build
    - OSInstallDate - OS install date
    - OSLastBootUpTime - OS last boot up time
    - UserDNSDomain - User DNS domain
    - FQDN - Fully qualified domain name

    .PARAMETER Color
    Color for the label. If not provided it will be taken from the parent New-BGInfo command.

    .PARAMETER FontSize
    Font size for the label. If not provided it will be taken from the parent New-BGInfo command.

    .PARAMETER FontFamilyName
    Font family name for the label. If not provided it will be taken from the parent New-BGInfo command.

    .PARAMETER ValueColor
    Color for the value. If not provided it will be taken first from Color of the label and if that is not provided from the parent New-BGInfo command.

    .PARAMETER ValueFontSize
    Font size for the value. If not provided it will be taken first from FontSize of the label and if that is not provided from the parent New-BGInfo command.

    .PARAMETER ValueFontFamilyName
    Font family name for the value. If not provided it will be taken first from FontFamilyName of the label and if that is not provided from the parent New-BGInfo command.

    .EXAMPLE
    New-BGInfoValue -BuiltinValue HostName -Color Red -FontSize 20 -FontFamilyName 'Calibri'
    New-BGInfoValue -BuiltinValue FullUserName
    New-BGInfoValue -BuiltinValue CpuName

    .EXAMPLE
    # Lets get all drives and their labels
    foreach ($Disk in (Get-Disk)) {
        $Volumes = $Disk | Get-Partition | Get-Volume
        foreach ($V in $Volumes) {
            New-BGInfoValue -Name "Drive $($V.DriveLetter)" -Value $V.SizeRemaining
        }
    }

    .NOTES
    General notes
    #>
    [CmdletBinding(DefaultParameterSetName = 'Values')]
    param(
        [parameter(ParameterSetName = 'Builtin')]
        [parameter(ParameterSetName = 'Values', Mandatory)]
        [string] $Name,
        [parameter(ParameterSetName = 'Values', Mandatory)]
        [string] $Value,
        [parameter(ParameterSetName = 'Builtin', Mandatory)]
        [ValidateSet(
            'UserName', 'HostName', 'FullUserName',
            'CpuName', 'CpuMaxClockSpeed', 'CpuCores', 'CpuLogicalCores',
            'RAMSize', 'RAMSpeed', 'RAMPartNumber',
            'BiosVersion', 'BiosManufacturer', 'BiosReleaseDate',
            'OSName', 'OSVersion', 'OSArchitecture', 'OSBuild', 'OSInstallDate', 'OSLastBootUpTime',
            'UserDNSDomain', 'FQDN'
        )][string] $BuiltinValue,
        [parameter(ParameterSetName = 'Values')]
        [parameter(ParameterSetName = 'Builtin')]
        [SixLabors.ImageSharp.Color] $Color,
        [parameter(ParameterSetName = 'Values')]
        [parameter(ParameterSetName = 'Builtin')]
        [float] $FontSize,
        [parameter(ParameterSetName = 'Values')]
        [parameter(ParameterSetName = 'Builtin')]
        [string] $FontFamilyName,
        [parameter(ParameterSetName = 'Values')]
        [parameter(ParameterSetName = 'Builtin')]
        [SixLabors.ImageSharp.Color] $ValueColor,
        [parameter(ParameterSetName = 'Values')]
        [parameter(ParameterSetName = 'Builtin')]
        [float] $ValueFontSize,
        [parameter(ParameterSetName = 'Values')]
        [parameter(ParameterSetName = 'Builtin')]
        [string] $ValueFontFamilyName
    )

    if ($BuiltinValue) {

        if ($BuiltinValue -in 'CPUName', 'CpuMaxClockSpeed', 'CpuCores', 'CpuLogicalCores') {
            $ComputerCPU = Get-ComputerCPU
        } elseif ($BuiltinValue -in 'RAMSize', 'RAMSpeed', 'RAMPartNumber') {
            $ComputerRAM = Get-ComputerRAM
        } elseif ($BuiltinValue -in 'BiosVersion', 'BiosManufacturer', 'BiosReleaseDate') {
            $ComputerBios = Get-ComputerBios
        } elseif ($BuiltinValue -in 'OSName', 'OSVersion', 'OSArchitecture', 'OSBuild', 'OSInstallDate', 'OSLastBootUpTime') {
            $ComputerOS = Get-ComputerOperatingSystem
        }
        if ($BuiltinValue -eq 'UserName') {
            $SetValue = $env:USERNAME
        } elseif ($BuiltinValue -eq 'HostName') {
            $SetValue = $env:COMPUTERNAME
        } elseif ($BuiltinValue -eq 'FullUserName') {
            $SetValue = $env:USERDOMAIN + '\' + $env:USERNAME
        } elseif ($BuiltinValue -eq 'CPUName') {
            $SetValue = $ComputerCPU.Name
        } elseif ($BuiltinValue -eq 'CpuMaxClockSpeed') {
            $SetValue = $ComputerCPU.MaxClockSpeed
        } elseif ($BuiltinValue -eq 'CpuCores') {
            $SetValue = $ComputerCPU.NumberOfEnabledCore
        } elseif ($BuiltinValue -eq 'CpuLogicalCores') {
            $SetValue = $ComputerCPU.NumberOfLogicalProcessors
        } elseif ($BuiltinValue -eq 'RAMSize') {
            $SetValue = ($ComputerRAM.Size | ForEach-Object { $_.ToString('N0') + 'GB' }) -join " / "
        } elseif ($BuiltinValue -eq 'RAMSpeed') {
            $SetValue = ($ComputerRAM.Speed | ForEach-Object { $_.ToString('N0') + 'MHz' }) -join " / "
        } elseif ($BuiltinValue -eq 'RAMPartNumber') {
            $SetValue = $ComputerRAM.PartNumber.Trim() -join ", "
        } elseif ($BuiltinValue -eq 'BiosVersion') {
            $SetValue = $ComputerBios.Version
        } elseif ($BuiltinValue -eq 'BiosManufacturer') {
            $SetValue = $ComputerBios.Manufacturer
        } elseif ($BuiltinValue -eq 'BiosReleaseDate') {
            $SetValue = $ComputerBios.ReleaseDate
        } elseif ($BuiltinValue -eq 'OSName') {
            $SetValue = $ComputerOS.OperatingSystem
        } elseif ($BuiltinValue -eq 'OSVersion') {
            $SetValue = $ComputerOS.OperatingSystemVersion
        } elseif ($BuiltinValue -eq 'OSArchitecture') {
            $SetValue = $ComputerOS.OSArchitecture
        } elseif ($BuiltinValue -eq 'OSBuild') {
            $SetValue = $ComputerOS.OperatingSystemBuild
        } elseif ($BuiltinValue -eq 'OSInstallDate') {
            $SetValue = $ComputerOS.InstallDate
        } elseif ($BuiltinValue -eq 'OSLastBootUpTime') {
            $SetValue = $ComputerOS.LastBootUpTime
        } elseif ($BuiltinValue -eq 'UserDNSDomain') {
            $SetValue = $env:USERDNSDOMAIN
        } elseif ($BuiltinValue -eq 'FQDN') {
            $SetValue = ((Get-CimInstance win32_computersystem).name).ToLower() + '.' + ((Get-CimInstance win32_computersystem).domain).ToLower()
        }
        if ($Name) {
            $SetName = $Name
        } else {
            $SetName = $BuiltinValue
        }
    } else {
        $SetValue = $Value
        $SetName = $Name
    }

    [PSCustomObject] @{
        Type                = 'Values'
        Name                = $SetName
        Value               = $SetValue
        Color               = $Color
        FontSize            = $FontSize
        FontFamilyName      = $FontFamilyName
        ValueColor          = $ValueColor
        ValueFontSize       = $ValueFontSize
        ValueFontFamilyName = $ValueFontFamilyName
    }
}