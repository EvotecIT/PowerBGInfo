---
title: "PowerBGInfo overview"
description: "PowerBGInfo generates desktop background information from PowerShell-authored configuration."
layout: docs
---

PowerBGInfo generates Windows desktop and logon backgrounds from PowerShell data. Use it for lab machines, shared admin workstations, support desktops, build hosts, classrooms, kiosks, and environments where machine identity or operational context should be visible at a glance.

## Typical use

- show host, user, operating system, CPU, memory, BIOS, disk, and network details
- add values collected from PowerShell, CIM, registry, Active Directory, APIs, or RMM tools
- position content by corner, center anchor, or explicit screen coordinates
- add transparent ChartForgeX charts, topology, images, and visual-canvas tiles
- preserve wallpaper slideshows or render a solid-color background when no wallpaper file exists
- apply output to the current user, all users, the logon screen, both targets, or a preview file only
- export JSON configuration for scheduled tasks, RMM, imaging, or repeatable deployment

PowerBGInfo is the Windows wallpaper product. Image composition and deterministic visuals are provided by [ChartForgeX](/projects/chartforgex/), while [ImagePlayground](/projects/imageplayground/) exposes broader image automation through PowerShell.

## Related project pages

- [Project overview](/projects/powerbginfo/)
- [Installation](../install/)
- [Deployment and refresh](../deployment/)
- [Layouts, charts, and topology](../layouts-and-overlays/)
- [Troubleshooting](../troubleshooting/)
- [PowerShell API](/projects/powerbginfo/api/)
- [Examples](/projects/powerbginfo/examples/)
