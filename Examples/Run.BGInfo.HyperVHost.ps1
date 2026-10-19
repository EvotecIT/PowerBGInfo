<#
    Hyper-V host. The question for the administrator at this console: does
    this host have room for another VM, and which VMs are using it?

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
function Get-HyperVHostData {
    # Get-VM returns these properties (MemoryAssigned, ProcessorCount and
    # CPUUsage); the workload tag comes from the VM notes or your CMDB.
    # The sample keeps them as CSV to stay short.
    $vms = @'
Name,Workload,vCPU,MemoryGB,Cpu
SQL-PROD-01,Databases,16,128,71
SQL-PROD-02,Databases,16,96,34
SQL-RPT-01,Databases,8,48,12
ERP-APP-01,ERP,12,64,38
ERP-APP-02,ERP,12,64,29
ERP-BATCH-01,ERP,8,32,55
VDI-POOL-A,Desktops,48,96,41
VDI-POOL-B,Desktops,24,48,18
WEB-01,Web,4,16,22
WEB-02,Web,4,16,19
WEB-03,Web,4,16,9
FS-01,Infrastructure,4,32,6
MON-01,Infrastructure,4,24,14
DC02,Infrastructure,2,8,3
'@ | ConvertFrom-Csv | ForEach-Object {
        [pscustomobject] @{
            Name = $_.Name; Workload = $_.Workload; vCPU = [int] $_.vCPU
            MemoryGB = [int] $_.MemoryGB; Cpu = [int] $_.Cpu; State = 'Running'
        }
    }

    [pscustomobject] @{
        Name     = 'HV-AMS-07'
        # Get-Cluster and Get-ClusterNode
        Cluster  = 'HVC-AMS-01 · node 3 of 6'
        Hardware = '2 × Xeon Gold 6448Y · 64 cores'
        Version  = 'Windows Server 2025 Datacenter'
        # Get-Counter '\Hyper-V Hypervisor Logical Processor(_Total)\
        #     % Total Run Time' sampled every 5 minutes
        Cpu      = @(38, 41, 52, 47, 44, 49, 46)
        # (Get-VMHost).MemoryCapacity and the sum of MemoryAssigned
        MemoryGB = [int] ($vms | Measure-Object -Property MemoryGB -Sum).Sum
        # The cluster keeps one node's worth of memory free for failover.
        ReserveGB = 64
        TotalGB  = 768
        # Get-ClusterSharedVolume | Select-Object -ExpandProperty
        #     SharedVolumeInfo | Select-Object -ExpandProperty Partition
        Storage  = @{ Name = 'CSV-02'; UsedPercent = 68; FreeTB = 6.1 }
        Vms      = $vms
        # Get-ClusterGroup | Where-Object OwnerNode -NE $env:COMPUTERNAME,
        # or the Failover Clustering event log, event 5143 and 1641
        Moves    = @(
            [pscustomobject] @{ Time = '09:12'; VM = 'ERP-BATCH-01'
                Move = 'from HV-AMS-04'; Result = 'OK' }
            [pscustomobject] @{ Time = '08:47'; VM = 'WEB-03'
                Move = 'from HV-AMS-02'; Result = 'OK' }
            [pscustomobject] @{ Time = '02:05'; VM = 'VDI-POOL-B'
                Move = 'to HV-AMS-05'; Result = 'Failed' }
        )
        # Memory assigned on every node: Get-ClusterNode | ForEach-Object {
        #     Get-VM -ComputerName $_.Name | Measure-Object MemoryAssigned }
        Nodes       = [ordered] @{
            'HV-AMS-04' = 71; 'HV-AMS-05' = 64; 'HV-AMS-07' = 82
            'HV-AMS-08' = 91; 'HV-AMS-09' = 58; 'HV-AMS-10' = 47
        }
        # Get-VM | Get-VMSnapshot | Where-Object CreationTime -LT
        #     (Get-Date).AddDays(-7)
        Checkpoints = @(
            @{ Text = 'ERP-APP-02 · before CU 14'; Status = 'Warning'
                Value = '12 days' }
            @{ Text = 'WEB-02 · config test'; Status = 'Warning'
                Value = '9 days' }
        )
    }
}

$fallback = [pscustomobject] @{
    Name = $env:COMPUTERNAME; Cluster = 'unknown'; Hardware = 'unknown'
    Version = 'unknown'; Cpu = @(0); MemoryGB = 0; TotalGB = 1; ReserveGB = 0
    Storage = @{ Name = 'CSV'; UsedPercent = 0; FreeTB = 0 }
    Vms = @([pscustomobject] @{ Name = 'unknown'; Workload = ''
            vCPU = 0; MemoryGB = 1; Cpu = 0; State = 'Unknown' })
    Moves = @(); Checkpoints = @(); Nodes = [ordered] @{ 'unknown' = 0 }
}
$source = @{
    Name           = 'Hyper-V host'
    TimeoutSeconds = 20
    ScriptBlock    = ${function:Get-HyperVHostData}
    Default        = $fallback
}
$hv = Invoke-BGInfoDataSource @source
#endregion

#region Presentation: reads only $hv
$scale = [Math]::Round($Height / 1080, 2)
$look = New-BGInfoTheme Corporate -Mode $Theme -Scale $scale
$memory = [Math]::Round(100 * $hv.MemoryGB / $hv.TotalGB)
$running = @($hv.Vms | Where-Object State -EQ 'Running').Count
$vcpu = ($hv.Vms | Measure-Object vCPU -Sum).Sum
$free = $hv.TotalGB - $hv.MemoryGB - $hv.ReserveGB

New-BGInfo -Theme $look -Target File {
    New-BGInfoBackground -Width $Width -Height $Height -Pattern Grid `
        -PatternSpacing ([int](56 * $scale))
    # A wider right column for the tables; the treemap may fill the center.
    New-BGInfoLayout -ColumnWidth ([int](520 * $scale)) `
        -MaximumPanelHeight ([int](640 * $scale))

    New-BGInfoLabel -Name 'HYPER-V HOST  ·  PRODUCTION CLUSTER' -Accent `
        -FontSize ([int](15 * $scale)) -FontWeight 700
    New-BGInfoLabel -Name $hv.Name -FontSize ([int](72 * $scale)) `
        -FontWeight 600
    New-BGInfoLabel -Name "Room for $free GB of VMs before failover reserve" `
        -FontSize ([int](19 * $scale)) -FontWeight 600
    New-BGInfoValue -Name 'Cluster' -Value $hv.Cluster
    New-BGInfoValue -Name 'Hardware' -Value $hv.Hardware
    New-BGInfoValue -Name 'System' -Value $hv.Version

    # Top band: four host metrics beside the text. Above 80 % memory or
    # 75 % storage the card turns amber; above 90 % it turns red.
    $card = @{ Region = 'Top'; Height = [int](170 * $scale) }
    New-BGInfoMetric @card -Label 'CPU' -Value $hv.Cpu[-1] -Unit '%' `
        -Caption '64 logical cores' -History $hv.Cpu -HistoryMaximum 100 `
        -WarningThreshold 70 -CriticalThreshold 85
    New-BGInfoMetric @card -Label 'Memory' -Value $memory -Unit '%' `
        -Caption "$($hv.MemoryGB) of $($hv.TotalGB) GB assigned" `
        -WarningThreshold 80 -CriticalThreshold 90
    New-BGInfoMetric @card -Label $hv.Storage.Name -Unit '%' `
        -Value $hv.Storage.UsedPercent `
        -Caption "$($hv.Storage.FreeTB) TB free" `
        -WarningThreshold 75 -CriticalThreshold 90
    New-BGInfoMetric @card -Label 'VMs running' -Value $running `
        -Caption "$vcpu vCPU on 64 cores"

    # Center: where the memory goes, grouped by workload. The color is the
    # VM's CPU: amber from 60 %, red from 85 %.
    $hv.Vms | New-BGInfoTreemap -Region Center -Title 'Memory by VM' `
        -Note 'size: memory · color: CPU' -ValueProperty MemoryGB `
        -GroupProperty Workload -ValueSuffix ' GB' -ColorProperty Cpu `
        -WarningThreshold 60 -CriticalThreshold 85

    # Right column: where else VMs could go, today's live migrations and
    # the checkpoints left behind.
    New-BGInfoChart -Region Right -Id 'nodes' -Title 'Cluster memory' `
        -Kind ProgressBar -Values @($hv.Nodes.Values) `
        -Labels @($hv.Nodes.Keys) -Maximum 100 -ValueSuffix ' %' `
        -NoProgressHandles -ShowLatestValue:$false -NoHistory `
        -WarningThreshold 80 -CriticalThreshold 90 `
        -Height ([int](250 * $scale))
    $hv.Moves | New-BGInfoTable -Region Right -Title 'Live migrations' `
        -Note 'today' -Property Time, VM, Result -StatusColumn Result
    New-BGInfoList -Region Right -Title 'Old checkpoints' `
        -Note 'older than 7 days' -Item $hv.Checkpoints
} -ConfigurationDirectory $OutputDirectory `
    -OutputFileName "hyperv-host-$($Theme.ToLower()).png"
#endregion
