---
title: "PowerBGInfo troubleshooting"
description: "Diagnose wallpaper placement, permissions, caching, and PowerShell host issues."
layout: docs
---

## Content is outside the visible area

Check the monitor index, resolution, wallpaper fit, text anchor, and whether screen coordinates are enabled. Author the layout with `-Target File` at the target dimensions before applying it.

## The old wallpaper remains visible

Keep the default refresh behavior enabled. Windows may cache a wallpaper path even when the file changed. For a slideshow, verify whether the deployment should preserve the slideshow or use `-DisableWallpaperSlideshow`.

## All-users or logon output fails

Run from an elevated process and confirm that endpoint policy allows the relevant system setting. Current-user and file-only targets do not require the same system-wide writes.

## Text is hard to read

Use a contrasting color, add a visual-canvas panel, or select a less busy part of the wallpaper. Validate at normal desktop scaling instead of only zooming into the generated image.

## Windows PowerShell 5.1 in VS Code

There is a known host-specific issue with the VS Code PowerShell extension on Windows PowerShell 5.1. Use the regular Windows PowerShell console, Windows PowerShell ISE, or PowerShell 7+ for that workflow.

For parameter details and supported values, open the [PowerShell API reference](/projects/powerbginfo/api/).
