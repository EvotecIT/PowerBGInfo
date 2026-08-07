---
title: "Add operational charts and topology"
description: "Add compact ChartForgeX visuals to a PowerBGInfo wallpaper."
layout: docs
---

PowerBGInfo 2.x can composite ChartForgeX-backed charts and topology into the generated wallpaper. This works well for a small CPU or capacity trend, service ownership, a lab route, or a build target.

Start from the maintained repository examples:

- [`PowerBGInfo.OperationalCharts.ps1`](https://github.com/EvotecIT/PowerBGInfo/blob/v2-speedygonzales/Examples/Scripts/PowerBGInfo.OperationalCharts.ps1) for rolling system charts
- [`PowerBGInfo.TopologyOverlay.ps1`](https://github.com/EvotecIT/PowerBGInfo/blob/v2-speedygonzales/Examples/Scripts/PowerBGInfo.TopologyOverlay.ps1) for a service topology overlay
- [`PowerBGInfo.VisualCanvas.ps1`](https://github.com/EvotecIT/PowerBGInfo/blob/v2-speedygonzales/Examples/Scripts/PowerBGInfo.VisualCanvas.ps1) for structured tiles and panels

Render to a file first, inspect it at the target monitor resolution, and only then switch to the current-user, all-users, or logon-screen target.
