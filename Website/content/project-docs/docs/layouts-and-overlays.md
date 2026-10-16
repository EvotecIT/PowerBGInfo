---
title: "Layouts, themes, widgets and charts"
description: "Compose PowerBGInfo values, themes, layout regions, widgets, charts and topology on Windows backgrounds."
layout: docs
---

Start with the information a person needs while looking at the desktop. A useful background answers identity, ownership, purpose, support, or current-state questions at a glance; it does not need to become a full dashboard.

## Data first

Collect the data in one place and only draw it in the `New-BGInfo` block. `Get-BGInfoSystemInfo` returns the facts support asks for first as one plain object, and `Invoke-BGInfoDataSource` runs your own collection code with a time limit and a fallback value, so a slow or broken source never blocks sign-in. `Examples/Run.BGInfo.Template.ps1` is a copy-and-fill starting point.

## Text and sections

Use `New-BGInfoValue` for built-in or custom data and `New-BGInfoLabel` for section headings. Parent settings on `New-BGInfo` provide consistent font, color, spacing, and wrapping defaults, while individual values can override them.

## Themes and backgrounds

`New-BGInfo -Theme` gives text, panels, charts, diagrams, tiles, widgets and the backdrop one look; parameters you set yourself always win. There are six presets in light and dark, and `New-BGInfoTheme` customizes accent, surface, corner radius, font and scale. `New-BGInfoBackground` paints a generated backdrop at the monitor size, optionally over a corporate image with a scrim.

## Layout regions

`-Region Left`, `Right`, `Top`, `Bottom` and `Center` flow panels into named areas instead of fixed coordinates, so one script fits 1920x1080, 2560x1440, ultrawide and portrait screens. `New-BGInfoLayout` sets margins, gaps, column width, the taskbar reserve and the desktop-icon area.

## Widgets

`New-BGInfoMetric`, `New-BGInfoTable`, `New-BGInfoList`, `New-BGInfoCalendarHeatmap`, `New-BGInfoTimeline`, `New-BGInfoTreemap`, `New-BGInfoMermaid` and `New-BGInfoQRCode` share the panel, header, placement and theme of the other panels. `-WarningThreshold` and `-CriticalThreshold` colour values by status. `New-BGInfoTreemap -ColorProperty` sizes rectangles by one measure and colours them by another, for example VM memory and CPU; with thresholds the colours are the status colours. A region column whose panels add up to more than its height shrinks them to stay inside the safe area.

## Images and tiles

`New-BGInfoImage` places a reusable image asset. Visual canvas commands arrange exact-value tiles, sections, and compact visual blocks when plain text needs more structure.

Tile text follows the `New-BGInfoTheme -Scale` value. Set
`-TileTextScale` on `New-BGInfoVisualCanvas` to change every tile, or set
`-TextScale` on one `New-BGInfoVisualCanvasTile`. Per-role
`-LabelFontSize`, `-ValueFontSize`, and `-DetailFontSize` set individual
sizes. For example:

```powershell
$tile = New-BGInfoVisualCanvasTile -Side Right -Label 'PROCESSOR' `
    -Value 'Intel Core i7' -Detail 'Average load 23%' -TextScale 1.25
New-BGInfo -Theme (New-BGInfoTheme Graphite -Scale 1.25) -Target File {
    New-BGInfoVisualCanvas -Tile $tile
}
```

## Charts

`New-BGInfoChart` renders ChartForgeX charts as transparent overlays. Small trends, targets, capacity, or service-state summaries work best. Keep labels readable against the selected wallpaper and test the output at the actual monitor resolution.

## Topology

`New-BGInfoTopology` can show a compact service route, lab layout, tenant boundary, or ownership hierarchy. Use it when relationships matter; use text values when the relationship itself adds no information.

All overlays are composed into the final wallpaper, so the deployed desktop does not need a browser or JavaScript runtime.
