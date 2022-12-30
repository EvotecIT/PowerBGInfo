function New-BGInfoValue {
    [CmdletBinding(DefaultParameterSetName = 'Values')]
    param(
        [parameter(ParameterSetName = 'Values', Mandatory)]
        [string] $Name,
        [parameter(ParameterSetName = 'Values', Mandatory)]
        [string] $Value,
        [parameter(ParameterSetName = 'Builtin', Mandatory)]
        [ValidateSet(
            'UserName',
            'HostName',
            'FullUserName',
            'CpuName',
            'CpuMaxClockSpeed',
            'CpuCores',
            'CpuLogicalCores',
            'RAMSize',
            'RAMSpeed',
            'RAMPartNumber'
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
            $SetValue = $ComputerRAM.PartNumber -join ", "
        }
        $SetName = $BuiltinValue
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