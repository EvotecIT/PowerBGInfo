---
title: "Layouts, charts, and topology"
description: "Compose PowerBGInfo values, visual-canvas tiles, charts, and topology on Windows backgrounds."
layout: docs
---

Start with the information a person needs while looking at the desktop. A useful background answers identity, ownership, purpose, support, or current-state questions at a glance; it does not need to become a full dashboard.

## Text and sections

Use `New-BGInfoValue` for built-in or custom data and `New-BGInfoLabel` for section headings. Parent settings on `New-BGInfo` provide consistent font, color, spacing, and wrapping defaults, while individual values can override them.

## Images and tiles

`New-BGInfoImage` places a reusable image asset. Visual canvas commands arrange exact-value tiles, sections, and compact visual blocks when plain text needs more structure.

## Charts

`New-BGInfoChart` renders ChartForgeX charts as transparent overlays. Small trends, targets, capacity, or service-state summaries work best. Keep labels readable against the selected wallpaper and test the output at the actual monitor resolution.

## Topology

`New-BGInfoTopology` can show a compact service route, lab layout, tenant boundary, or ownership hierarchy. Use it when relationships matter; use text values when the relationship itself adds no information.

All overlays are composed into the final wallpaper, so the deployed desktop does not need a browser or JavaScript runtime.
