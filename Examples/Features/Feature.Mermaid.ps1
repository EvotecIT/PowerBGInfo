<#
    Feature: Mermaid diagrams. Describe an escalation path or a network flow
    in a few lines of Mermaid text; ChartForgeX renders it natively in the
    panel theme, without a browser.
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
# The diagram is data too: keep it in a file next to the script, a wiki export
# or a CMDB query, and fall back to a short default when it cannot be read.
function Get-EscalationDiagram {
    @'
flowchart LR
    U([You]) --> D[Service desk<br/>ext. 4357]
    D --> Q{Fixed in<br/>30 min?}
    Q -- Yes --> R([Resolved])
    Q -- No --> E[On-call engineer]
    E --> M[Major incident<br/>bridge]
'@
}

$diagram = Invoke-BGInfoDataSource -Name 'Escalation diagram' -TimeoutSeconds 5 -ScriptBlock ${function:Get-EscalationDiagram} `
    -Default "flowchart LR`n    U([You]) --> D[Service desk]"
#endregion

#region Presentation: reads only $diagram
$scale = [Math]::Round($Height / 1080, 2)
New-BGInfo -Theme (New-BGInfoTheme Midnight -Mode $Theme -Scale $scale) -Target File {
    New-BGInfoBackground -Width $Width -Height $Height
    New-BGInfoLayout -BandHeight ([int](720 * $scale))
    New-BGInfoLabel -Name 'MERMAID' -Accent -FontWeight 700
    New-BGInfoLabel -Name 'Who to call, and when' -FontSize ([int](40 * $scale)) -FontWeight 600

    New-BGInfoMermaid -Region Bottom -Title 'Escalation path' -Definition $diagram -Height ([int](720 * $scale))
} -ConfigurationDirectory $OutputDirectory -OutputFileName "mermaid-$($Theme.ToLower())-$Width.png"
#endregion
