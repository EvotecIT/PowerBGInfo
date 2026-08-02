---
external help file: PowerBGInfo-help.xml
Module Name: PowerBGInfo
online version: https://github.com/EvotecIT/PowerBGInfo
schema: 2.0.0
---
# New-BGInfoTopologyNode
## SYNOPSIS
Creates a BGInfo topology node definition.

## SYNTAX
### __AllParameterSets
```powershell
New-BGInfoTopologyNode [-Id] <string> [-Label] <string> [-Subtitle <string>] [-Kind <TopologyNodeKind>] [-Status <TopologyHealthStatus>] [-GroupId <string>] [-Symbol <string>] [-Badge <string>] [-Color <string>] [-DisplayMode <TopologyNodeDisplayMode>] [<CommonParameters>]
```

## DESCRIPTION
Creates a BGInfo topology node definition.

## EXAMPLES

### EXAMPLE 1
```powershell
New-BGInfoTopologyNode -Badge 'Value'
```


## PARAMETERS

### -Badge
Optional badge text.

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

### -Color
Optional node accent color as CSS hex.

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

### -DisplayMode
Optional node display mode override.

```yaml
Type: TopologyNodeDisplayMode
Parameter Sets: __AllParameterSets
Aliases: None
Possible values: Card, CompactCard, Tile, Pill, Icon, Artwork, Dot, Hidden

Required: False
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -GroupId
Optional parent group identifier.

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

### -Id
Stable node identifier used by topology edges.

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

### -Kind
Node kind used for icon and legend selection.

```yaml
Type: TopologyNodeKind
Parameter Sets: __AllParameterSets
Aliases: None
Possible values: Generic, Group, Location, Hub, Branch, Server, Service, Endpoint, Gateway, Cloud, Database, Storage, Network, NetworkSegment, Namespace, Application, Process, Queue, Person, Team, Certificate

Required: False
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Label
Node label rendered in the topology.

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

### -Status
Node health or state.

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

### -Subtitle
Optional node subtitle.

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

### -Symbol
Short symbol rendered inside or near the node icon.

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

### CommonParameters
This cmdlet supports the common parameters: -Debug, -ErrorAction, -ErrorVariable, -InformationAction, -InformationVariable, -OutVariable, -OutBuffer, -PipelineVariable, -Verbose, -WarningAction, and -WarningVariable. For more information, see [about_CommonParameters](http://go.microsoft.com/fwlink/?LinkID=113216).

## INPUTS

- `None`

## OUTPUTS

- `ChartForgeX.Topology.TopologyNode`

## RELATED LINKS

- None
