# Cross-platform engine split

PowerBGInfo owns system facts, data binding, wallpaper layout, history, output
ownership, and run coordination. ChartForgeX owns reusable visual composition.
DesktopManager owns display, session, and wallpaper integration. PowerShell and
the CLI remain thin entry points into the same engine.

## Implementation

- [x] Inspect current default branches and create dedicated worktrees in all three repositories.
- [x] Extract the portable generation path from desktop discovery and Windows policy operations.
- [x] Split generation into focused rendering, desktop, slideshow, text, and history responsibilities.
- [x] Preserve the Windows fact provider behind a platform boundary and add common, macOS, and Linux facts.
- [x] Report unsupported and unavailable facts explicitly while preserving existing public snapshot and token APIs.
- [x] Add DesktopManager capabilities, display/session discovery, wallpaper application and readback for Windows, macOS, GNOME, and KDE Plasma.
- [x] Keep Windows-specific lock-screen, all-user, and slideshow operations explicit; reject unsupported requests before changing the desktop.
- [x] Move reusable panel, text-header, and background-pattern rendering into ChartForgeX and migrate PowerBGInfo callers.
- [x] Preserve Windows PowerShell 5.1/net472 and add portable PowerShell 7 and CLI builds.
- [x] Bound configuration-script execution and preserve CLI warnings and run diagnostics.
- [x] Update examples, configuration/deployment documentation, and CI for the supported platform paths.

## Validation

- [x] Run focused provider, rendering, generation, desktop-contract, configuration, and process-boundary tests.
- [x] Build the Windows/net472 paths; execution remains part of native qualification below.
- [x] Run the portable test suite and real module/CLI file rendering on macOS.
- [x] Inspect representative rendered wallpapers and check shared-renderer extraction for visual regressions.
- [x] Inspect actual macOS desktop discovery and wallpaper readback.
- [x] Complete one independent review of each repository's changed contracts.
- [x] Close independent review findings and confirm their bounded corrections.
- [x] Run ChartForgeX's complete stable-candidate quality loop, including NativeAOT, gallery baselines, and package checks.
- [x] Run final DesktopManager regressions and all four library target builds.
- [x] Run PowerBGInfo integrated validation, including packaged module isolation and single-file/NativeAOT file exports.
- [x] Validate the KDE AOT correction and refresh the affected package/executable proof against the final shared candidate.
- [x] Merge the qualified ChartForgeX and DesktopManager producer PRs.
- [x] Finish the qualified PowerBGInfo consumer in its dedicated worktree.
- [ ] Pin released producer APIs and publish the PowerBGInfo consumer PR after those packages are available.

## Native qualification

Windows/WSL/Linux execution and graphical desktop acceptance remain outstanding.
Automated CI remains useful evidence for the paths it actually executes.

- [ ] Verify the packaged Windows PowerShell 5.1 import, WMI facts, and rendering on Windows.
- [ ] Qualify Windows desktop changes separately from NativeAOT file exports; legacy COM/history paths emit AOT warnings.
- [ ] Run portable module/CLI file workflows and native facts on Linux/WSL.
- [ ] Qualify GNOME and KDE application/readback on real graphical sessions; headless CI does not establish desktop acceptance.
- [x] Qualify macOS Center, Fit, Fill and Stretch application with native readback and visual inspection on one display; verify five unsupported/invalid requests do not change it; apply a generated image through the packaged module.
- [x] Restore and verify the original macOS aerial selection, all-Spaces setting, complete native URL/options and photo list. Aerial restoration requires Wallpaper settings beyond the static AppKit image operation.

## Delivery boundaries

The first desktop adapters cover the current user's session. macOS all-Spaces
behavior and arbitrary Linux desktops require separate qualification. Desktop
capabilities are reported rather than inferred from the operating-system name.
File rendering accepts explicit dimensions and does not require a graphical
session.

The implementation uses the existing ChartForgeX and DesktopManager source
reference overrides for cross-repository qualification. Public packages and
PowerShell module delivery remain separate gates. New shared APIs must be
published before a downstream public package pin can consume them. Package
publication, merge, and deployment are not implied by local source validation.
