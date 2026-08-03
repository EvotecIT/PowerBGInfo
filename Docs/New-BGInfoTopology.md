---
external help file: PowerBGInfo-help.xml
Module Name: PowerBGInfo
online version: https://github.com/EvotecIT/PowerBGInfo
schema: 2.0.0
---
# New-BGInfoTopology
## SYNOPSIS
Creates a BGInfo topology overlay definition.

## SYNTAX
### __AllParameterSets
```powershell
New-BGInfoTopology [-TopologyDefinition] <scriptblock> [-Title <string>] [-Subtitle <string>] [-Width <int>] [-Height <int>] [-Anchor <BgInfoTextPosition>] [-OffsetX <int>] [-OffsetY <int>] [-PositionX <int>] [-PositionY <int>] [-Layout <TopologyLayoutMode>] [-Direction <TopologyLayoutDirection>] [-NodeDisplayMode <TopologyNodeDisplayMode>] [-Theme <string>] [-Opaque] [-NoTitle] [-ShowLegend] [-NoGroups] [-NoEdgeLabels] [<CommonParameters>]
```

## DESCRIPTION
Creates a BGInfo topology overlay definition.

## EXAMPLES

### EXAMPLE 1
```powershell
New-BGInfoTopology -Anchor 'Value'
```


## PARAMETERS

### -Anchor
Anchor position for placement.

```yaml
Type: BgInfoTextPosition
Parameter Sets: __AllParameterSets
Aliases: None
Possible values: TopLeft, TopCenter, TopRight, MiddleLeft, MiddleCenter, MiddleRight, BottomLeft, BottomCenter, BottomRight

Required: False
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Direction
Topology layout direction.

```yaml
Type: TopologyLayoutDirection
Parameter Sets: __AllParameterSets
Aliases: None
Possible values: TopToBottom, LeftToRight, BottomToTop, RightToLeft

Required: False
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Height
Topology height in pixels.

```yaml
Type: Int32
Parameter Sets: __AllParameterSets
Aliases: None
Possible values:

Required: False
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Layout
Topology layout mode.

```yaml
Type: TopologyLayoutMode
Parameter Sets: __AllParameterSets
Aliases: None
Possible values: Manual, GroupGrid, HubAndSpoke, Layered, Matrix, DenseGrouped, Geographic, ForceDirected, RelationshipRadial, MindMap

Required: False
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -NodeDisplayMode
Node presentation mode.

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

### -NoEdgeLabels
Hide edge labels.

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

### -NoGroups
Hide group containers.

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

### -NoTitle
Hide the topology title.

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

### -OffsetX
Horizontal offset from the anchor.

```yaml
Type: Int32
Parameter Sets: __AllParameterSets
Aliases: None
Possible values:

Required: False
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -OffsetY
Vertical offset from the anchor.

```yaml
Type: Int32
Parameter Sets: __AllParameterSets
Aliases: None
Possible values:

Required: False
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Opaque
Use an opaque topology canvas.

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

### -PositionX
Absolute X position.

```yaml
Type: Int32
Parameter Sets: __AllParameterSets
Aliases: None
Possible values:

Required: False
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -PositionY
Absolute Y position.

```yaml
Type: Int32
Parameter Sets: __AllParameterSets
Aliases: None
Possible values:

Required: False
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -ShowLegend
Show the topology legend.

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

### -Subtitle
Topology subtitle.

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

### -Theme
Theme name.

```yaml
Type: String
Parameter Sets: __AllParameterSets
Aliases: None
Possible values: Light, Dark

Required: False
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Title
Topology title.

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

### -TopologyDefinition
Script block that emits topology groups, nodes, and edges.

```yaml
Type: ScriptBlock
Parameter Sets: __AllParameterSets
Aliases: None
Possible values:

Required: True
Position: 0
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Width
Topology width in pixels.

```yaml
Type: Int32
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

- `PowerBGInfo.BgInfoTopology`

## RELATED LINKS

- None
