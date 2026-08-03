---
external help file: PowerBGInfo-help.xml
Module Name: PowerBGInfo
online version: https://github.com/EvotecIT/PowerBGInfo
schema: 2.0.0
---
# New-BGInfoTopologyEdge
## SYNOPSIS
Creates a BGInfo topology edge definition.

## SYNTAX
### __AllParameterSets
```powershell
New-BGInfoTopologyEdge [-SourceNodeId] <string> [-TargetNodeId] <string> [[-Label] <string>] [-Id <string>] [-Kind <TopologyEdgeKind>] [-Status <TopologyHealthStatus>] [-Direction <VisualLinkDirection>] [-Routing <TopologyEdgeRouting>] [-Color <string>] [-Muted] [<CommonParameters>]
```

## DESCRIPTION
Creates a BGInfo topology edge definition.

## EXAMPLES

### EXAMPLE 1
```powershell
New-BGInfoTopologyEdge -Color 'Value'
```


## PARAMETERS

### -Color
Optional edge color as CSS hex.

```yaml
Type: String
Parameter Sets: __AllParameterSets
Aliases: None
Possible values:

Required: False
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Direction
Direction marker behavior.

```yaml
Type: VisualLinkDirection
Parameter Sets: __AllParameterSets
Aliases: None
Possible values: None, Forward, Backward, Bidirectional

Required: False
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Id
Stable edge identifier. When omitted, one is derived from source and target ids.

```yaml
Type: String
Parameter Sets: __AllParameterSets
Aliases: None
Possible values:

Required: False
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Kind
Relationship kind.

```yaml
Type: TopologyEdgeKind
Parameter Sets: __AllParameterSets
Aliases: None
Possible values: Generic, Link, Replication, Connectivity, Dependency, Trust, Mapping, AuthenticationPath, CertificateChain, DataFlow, Ownership, Membership

Required: False
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Label
Primary edge label.

```yaml
Type: String
Parameter Sets: __AllParameterSets
Aliases: None
Possible values:

Required: False
Position: 2
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Muted
Render the edge as a quiet structural relationship.

```yaml
Type: SwitchParameter
Parameter Sets: __AllParameterSets
Aliases: None
Possible values:

Required: False
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Routing
Edge routing mode.

```yaml
Type: TopologyEdgeRouting
Parameter Sets: __AllParameterSets
Aliases: None
Possible values: Straight, Curved, Orthogonal, ObstacleAvoidingOrthogonal

Required: False
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -SourceNodeId
Source node identifier.

```yaml
Type: String
Parameter Sets: __AllParameterSets
Aliases: None
Possible values:

Required: True
Position: 0
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Status
Relationship health or state.

```yaml
Type: TopologyHealthStatus
Parameter Sets: __AllParameterSets
Aliases: None
Possible values: Healthy, Warning, Critical, Unknown, Disabled

Required: False
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -TargetNodeId
Target node identifier.

```yaml
Type: String
Parameter Sets: __AllParameterSets
Aliases: None
Possible values:

Required: True
Position: 1
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### CommonParameters
This cmdlet supports the common parameters: -Debug, -ErrorAction, -ErrorVariable, -InformationAction, -InformationVariable, -OutVariable, -OutBuffer, -PipelineVariable, -Verbose, -WarningAction, and -WarningVariable. For more information, see [about_CommonParameters](http://go.microsoft.com/fwlink/?LinkID=113216).

## INPUTS

- `None`

## OUTPUTS

- `ChartForgeX.Topology.TopologyEdge`

## RELATED LINKS

- None
