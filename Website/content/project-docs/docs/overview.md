---
title: "PowerBGInfo overview"
description: "What PowerBGInfo is for: desktop and lock screen backgrounds built from PowerShell data for labs, admin workstations, support desks, kiosks and servers."
layout: docs
---

PowerBGInfo generates Windows desktop and lock screen backgrounds from PowerShell data. Use it for lab machines, shared admin workstations, support desktops, build hosts, classrooms, kiosks, session hosts and anywhere machine identity or operational context should be visible at a glance.

## Typical use

- show host, signed-in user, operating system, uptime, IP address, disks, last update, Defender and BitLocker state
- add values collected from PowerShell, CIM, the registry, Active Directory, APIs or RMM tools, time-boxed with `Invoke-BGInfoDataSource` so a slow source never blocks sign-in
- give text, panels, charts and widgets one look with a theme, and let layout regions place them on any screen size
- add metric cards, tables, lists, calendar heatmaps, timelines, treemaps, Mermaid diagrams, QR codes, ChartForgeX charts and topology, coloured by warning and critical thresholds
- render one image per monitor with `Get-BGInfoMonitor`, on a generated backdrop or a corporate image
- preserve wallpaper slideshows, or render a solid-colour background when no wallpaper file exists
- apply the result to the current user, all users or the lock screen, or write a preview file only
- deploy it with Task Scheduler, Group Policy, Intune, Configuration Manager or an RMM tool

PowerBGInfo is the Windows wallpaper product. Image composition and deterministic visuals are provided by [ChartForgeX](/projects/chartforgex/), while [ImagePlayground](/projects/imageplayground/) exposes broader image automation through PowerShell.

## Related project pages

- [Project overview](/projects/powerbginfo/)
- [Installation](../install/)
- [Layouts, themes, widgets and charts](../layouts-and-overlays/)
- [Run in production](../deployment/)
- [Troubleshooting](../troubleshooting/)
- [PowerShell API](/projects/powerbginfo/api/)
- [Examples](/projects/powerbginfo/examples/)
