function Get-TestModuleAssembly {
    <#
    .SYNOPSIS
    Resolves a test dependency in the context of the imported compiled module.
    .PARAMETER Name
    Simple assembly name used by the module.
    .OUTPUTS
    System.Reflection.Assembly
    #>
    param([Parameter(Mandatory = $true)][string] $Name)

    $identity = [Reflection.AssemblyName]::new($Name)
    if ($PSVersionTable.PSEdition -eq 'Core') {
        $assembly = (Get-Command New-BGInfo).ImplementingType.Assembly
        $context = [Runtime.Loader.AssemblyLoadContext]::GetLoadContext($assembly)
        return $context.LoadFromAssemblyName($identity)
    }
    return [Reflection.Assembly]::Load($identity)
}
