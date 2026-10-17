<#
    Build agent or CI runner. The question for the engineer who remotes in:
    is this agent busy or broken, what ran last, and is there disk left?

    The data block returns plain values; the presentation block only reads
    them. Comments name the command or source that supplies each value.
#>
param(
    [ValidateSet('Light', 'Dark')]
    [string] $Theme = 'Dark',
    [int] $Width = 1920,
    [int] $Height = 1080,
    [string] $OutputDirectory = (Join-Path $PSScriptRoot 'Output')
)

Import-Module (Join-Path $PSScriptRoot '..\PowerBGInfo.psd1') -Force

#region Data: your logic goes here
function Get-BuildAgentData {
    [pscustomobject] @{
        Name      = 'BLD-WIN-04'
        Available = $true
        Pool      = 'WIN-BUILD-2025'
        # .agent and .credentials in the agent folder, or the pool API:
        # GET {org}/_apis/distributedtask/pools/{id}/agents
        #     ?includeAssignedRequest=true
        Agent     = '4.255.0 · runs as a service'
        Toolchain = 'VS 2022 17.14 · .NET 10 SDK · Node 22'
        Job       = 'payments-api CI #48213'
        Stage     = 'Test'
        Waiting   = 3
        # Get-Volume -DriveLetter C, D | Select-Object DriveLetter,
        #     SizeRemaining, Size
        Disks     = @(
            [pscustomobject] @{ Name = 'D: work'; FreeGB = 182; SizeGB = 512 }
            [pscustomobject] @{ Name = 'C: system'; FreeGB = 34; SizeGB = 256 }
        )
        # GET {org}/{project}/_apis/build/builds
        #     ?$top=5&queryOrder=finishTimeDescending
        Builds    = @(
            [pscustomobject] @{ Pipeline = 'payments-api CI'; Branch = 'main'
                Result = 'Running'; Time = '6 min' }
            [pscustomobject] @{ Pipeline = 'web-portal PR 1182'
                Branch = 'feature/sso'; Result = 'Passed'; Time = '14 min' }
            [pscustomobject] @{ Pipeline = 'mobile-sdk nightly'; Branch = 'main'
                Result = 'Failed'; Time = '3 min' }
            [pscustomobject] @{ Pipeline = 'infra-templates'
                Branch = 'release/26.10'; Result = 'Passed'; Time = '8 min' }
            [pscustomobject] @{ Pipeline = 'payments-api CI'; Branch = 'main'
                Result = 'Passed'; Time = '11 min' }
        )
        # The stages of the running pipeline, from its YAML.
        Stages    = 'Checkout', 'Restore', 'Build', 'Test', 'Sign', 'Publish'
    }
}

$fallback = [pscustomobject] @{
    Name = $env:COMPUTERNAME; Available = $false
    Pool = 'unknown'; Agent = 'unknown'; Toolchain = 'unknown'
    Job = 'unavailable'; Stage = ''; Waiting = 0
    Disks = @(); Builds = @(); Stages = @()
}
$source = @{
    Name           = 'Build agent data'
    TimeoutSeconds = 15
    ScriptBlock    = ${function:Get-BuildAgentData}
    Default        = $fallback
}
$agent = Invoke-BGInfoDataSource @source
#endregion

#region Presentation: reads only $agent
$scale = [Math]::Round($Height / 1080, 2)
$dark = $Theme -eq 'Dark'
# Graphite with an emerald accent and dots: the terminal look of a runner.
$accent = if ($dark) { '#34D399' } else { '#047857' }
$dots = if ($dark) { '#FFFFFF0E' } else { '#0F172A12' }
$look = New-BGInfoTheme Graphite -Mode $Theme -Scale $scale -Accent $accent
# The pipeline as a Mermaid flow: done stages, the running one, the rest.
$at = [array]::IndexOf([string[]] $agent.Stages, $agent.Stage)
$nodes = for ($i = 0; $i -lt $agent.Stages.Count; $i++) {
    "S$i[$($agent.Stages[$i])]"
}
$done = (0..($at - 1) | ForEach-Object { "S$_" }) -join ','
$fill = if ($dark) {
    @{
        Done = 'fill:#064E3B,stroke:#34D399'
        Now  = 'fill:#047857,stroke:#6EE7B7'
    }
} else {
    @{
        Done = 'fill:#D1FAE5,stroke:#059669'
        Now  = 'fill:#6EE7B7,stroke:#047857'
    }
}
$flow = @(
    'flowchart LR'
    '    ' + ($nodes -join ' --> ')
    "    classDef done $($fill.Done)"
    "    classDef now $($fill.Now),stroke-width:3px"
    if ($at -gt 0) { "    class $done done" }
    "    class S$at now"
) -join "`n"

New-BGInfo -Theme $look -Target File {
    New-BGInfoBackground -Width $Width -Height $Height -Pattern Dots `
        -PatternColor $dots -PatternSpacing ([int](24 * $scale))

    New-BGInfoLabel -Name "BUILD AGENT  ·  POOL $($agent.Pool)" -Accent `
        -FontSize ([int](15 * $scale)) -FontWeight 700
    New-BGInfoLabel -Name $agent.Name -FontSize ([int](72 * $scale)) `
        -FontWeight 600
    if ($agent.Available) {
        New-BGInfoLabel -Name "▶  Running $($agent.Job)" -Accent `
            -FontWeight 600
    } else {
        New-BGInfoLabel -Name 'Build data unavailable' -Status Info `
            -FontWeight 600
    }
    New-BGInfoValue -Name 'Agent' -Value $agent.Agent
    New-BGInfoValue -Name 'Toolchain' -Value $agent.Toolchain
    New-BGInfoValue -Name 'Clean-up' -Value 'Sunday 03:00, keeps 3 days'

    # Right column: the queue, then free space. Free space is worse when
    # low: amber below 100 GB on the work drive, red below 50 GB.
    if ($agent.Available) {
        New-BGInfoMetric -Region Right -Label 'Waiting in the pool' `
            -Value $agent.Waiting -Unit 'jobs' -Caption '4 agents online' `
            -Height ([int](150 * $scale))
    }
    foreach ($disk in $agent.Disks) {
        $limits = if ($disk.Name -like 'D:*') { 100, 50 } else { 40, 20 }
        New-BGInfoMetric -Region Right -Label "$($disk.Name) free" `
            -Value $disk.FreeGB -Unit 'GB' -Caption "of $($disk.SizeGB) GB" `
            -WarningThreshold $limits[0] -CriticalThreshold $limits[1] `
            -ThresholdDirection Below -Height ([int](150 * $scale))
    }

    # Center: what ran here last. Bottom band: where the current job is.
    $agent.Builds | New-BGInfoTable -Region Center -Title 'Last builds' `
        -Note 'on this agent' -StatusColumn Result
    if ($agent.Available) {
        $stageNote = "stage $($at + 1) of $($agent.Stages.Count)"
        New-BGInfoMermaid -Region Bottom -Height ([int](200 * $scale)) `
            -Title $agent.Job -Note $stageNote `
            -Definition $flow
    }
} -ConfigurationDirectory $OutputDirectory `
    -OutputFileName "build-agent-$($Theme.ToLower()).png"
#endregion
