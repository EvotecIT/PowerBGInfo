<#
    Feature: treemap. Sizes as nested rectangles: disk usage by folder, VM or share.
#>
param(
    [ValidateSet('Light', 'Dark')]
    [string] $Theme = 'Dark',
    [int] $Width = 1920,
    [int] $Height = 1080,
    [string] $OutputDirectory = (Join-Path $PSScriptRoot '..\Output\Features')
)

Import-Module (Join-Path $PSScriptRoot '..\..\PowerBGInfo.psd1') -Force

#region Data: your logic goes here
# One object per rectangle with Name, Value and an optional Group. A live source
# for the same shape (slow on big trees, so it is time-boxed below):
#   Get-ChildItem D:\ -Directory | ForEach-Object {
#       [pscustomobject] @{ Name = $_.Name; Value = [Math]::Round((Get-ChildItem $_.FullName -Recurse -File | Measure-Object Length -Sum).Sum / 1GB) }
#   }
function Get-DiskUsageExampleData {
    @(
        [pscustomobject] @{ Name = 'Finance'; Value = 412; Group = 'Databases' }
        [pscustomobject] @{ Name = 'Sales'; Value = 180; Group = 'Databases' }
        [pscustomobject] @{ Name = 'Full'; Value = 230; Group = 'Backups' }
        [pscustomobject] @{ Name = 'Logs'; Value = 96; Group = 'Backups' }
        [pscustomobject] @{ Name = 'IIS logs'; Value = 64; Group = 'Logs' }
        [pscustomobject] @{ Name = 'Temp'; Value = 22; Group = 'Logs' }
    )
}

$usage = Invoke-BGInfoDataSource -Name 'Disk usage' -TimeoutSeconds 20 -ScriptBlock ${function:Get-DiskUsageExampleData} `
    -Default @([pscustomobject] @{ Name = 'unknown'; Value = 1 })
#endregion

#region Presentation: reads only $usage
$scale = [Math]::Round($Height / 1080, 2)
$total = ($usage | Measure-Object Value -Sum).Sum
New-BGInfo -Theme (New-BGInfoTheme Graphite -Mode $Theme -Scale $scale) -Target File {
    New-BGInfoBackground -Width $Width -Height $Height
    New-BGInfoLabel -Name 'TREEMAP' -Accent -FontWeight 700
    New-BGInfoLabel -Name 'Where D: went' -FontSize ([int](40 * $scale)) -FontWeight 600
    New-BGInfoValue -Name 'Used' -Value "$total GB"

    $usage | New-BGInfoTreemap -Region Right -Title 'D: by folder' -ValueSuffix ' GB' -Height ([int](420 * $scale))
} -ConfigurationDirectory $OutputDirectory -OutputFileName "treemap-$($Theme.ToLower())-$Width.png"
#endregion
