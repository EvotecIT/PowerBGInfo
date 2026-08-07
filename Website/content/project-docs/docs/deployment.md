---
title: "Deploy and refresh PowerBGInfo"
description: "Choose the PowerBGInfo target and refresh model for user, fleet, and logon backgrounds."
layout: docs
---

PowerBGInfo can render a file, change the current desktop, stage a background for multiple profiles, or update the Windows logon screen. Choose the narrowest target that matches the job.

## Preview before applying

Use `-Target File` and `-OutputFileName` while authoring a layout. This creates an inspectable image without changing the current user environment.

## Current user

The default workflow renders for the selected monitor and applies the output to the current user. Run it at sign-in or from a scheduled task when values need periodic refresh.

## Existing and future users

Use `-AllUsers` from an elevated process to stage the same generated background for existing profiles and the default profile. Add `-ExcludeDefaultUserProfile` when new profiles should retain their own default.

## Logon and lock screen

`-Target LogonScreen` and `-Target Both` require elevation because they update system-level settings. Validate this path on the Windows version and management baseline used by the fleet.

## Slideshows and caching

When the current desktop is a slideshow, PowerBGInfo can render each source and preserve slideshow behavior. Use `-DisableWallpaperSlideshow` for one static result. Keep the default refresh behavior unless a deployment has a specific reason to avoid it; Windows can otherwise reuse a cached wallpaper path after sign-in.

Export JSON configuration when the layout should be versioned separately from the deployment script, then call `Invoke-BGInfo` from the scheduled or RMM workflow.
