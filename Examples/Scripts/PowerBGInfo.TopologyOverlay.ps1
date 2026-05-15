$examplesPath = Split-Path -Path $PSScriptRoot -Parent
$sampleImage = Join-Path -Path $examplesPath -ChildPath 'Samples\TapC-Evotec-2560x1080.jpg'

New-BGInfo {
    New-BGInfoValue -Name 'Topology' -Value 'ChartForgeX overlay' -Color White -ValueColor White -FontSize 22

    New-BGInfoTopology -Title 'Lab topology' -Subtitle 'Gateway, API, SQL' -Width 560 -Height 310 -Anchor BottomRight -OffsetX 34 -OffsetY 34 -TopologyDefinition {
        New-BGInfoTopologyGroup -Id 'lab' -Label 'Lab Site' -Status Healthy -Symbol region
        New-BGInfoTopologyNode -Id 'gateway' -Label 'Gateway' -Kind Network -Status Healthy -GroupId 'lab' -Symbol GW
        New-BGInfoTopologyNode -Id 'api' -Label 'API' -Kind Service -Status Healthy -GroupId 'lab' -Symbol API
        New-BGInfoTopologyNode -Id 'sql' -Label 'SQL' -Kind Database -Status Warning -GroupId 'lab' -Symbol SQL
        New-BGInfoTopologyEdge -SourceNodeId 'gateway' -TargetNodeId 'api' -Label 'HTTPS' -Kind Connectivity -Status Healthy -Direction Forward
        New-BGInfoTopologyEdge -SourceNodeId 'api' -TargetNodeId 'sql' -Label '32 ms' -Kind Dependency -Status Warning -Direction Forward
    }
} -FilePath $sampleImage `
    -ConfigurationDirectory (Join-Path -Path $examplesPath -ChildPath 'Output') `
    -OutputFileName 'PowerBGInfo.TopologyOverlay.jpg' `
    -Target File `
    -WallpaperFit Fill
