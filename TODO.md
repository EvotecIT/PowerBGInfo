# Cross-platform follow-up

PowerBGInfo uses ChartForgeX for portable rendering and DesktopManager for
desktop integration. Windows PowerShell 5.1 and PowerShell 7 retain separate
packaged payloads. The portable engine, Windows/Linux runtime and packaged
module checks are qualified. DesktopManager 4.6.1 supplies the target/state APIs,
effective GNOME image opacity and narrowly qualified optional X11 geometry.

## Native capabilities

- [ ] Define supported dynamic-source behavior per backend. HEIC decoding, aerial animation, KDE dynamic plugins and wallpaper image packages are not implemented through a static-image fallback.
- [ ] Qualify physical multi-output and Wayland geometry independently. Optional already-installed XRandR discovery currently requires one unrotated, unreflected origin-aligned X11 output, an identity transform and matching native-mode dimensions; other layouts need explicit dimensions.
- [ ] Qualify multi-display changes and Space/activity transitions independently of single-display checks.
- [ ] Qualify Windows desktop/history NativeAOT paths or retain the managed CLI requirement for those operations.

The current macOS static-image check covers native Fit, implicit source/fit
inheritance, repeated packaged generation, complete AppKit URL/options
restoration and visual inspection on one display. The original aerial and
all-Spaces selection were recovered through native Settings after an independent
recovery preflight. This does not establish automatic aerial restoration,
all-Spaces enumeration or HEIC decoding. Earlier Center/Fill/Stretch checks are
separate historical evidence.

Packaged static-image application, repeated rendering and checkpoint restoration
also pass on Windows and in GNOME 46/Plasma 5.27 graphical X11 sessions on a
disposable virtual display. GNOME enabled/disabled opacity and automatic gradient
inheritance are qualified there. Those graphical checks are separate from WSL
file execution and do not establish physical multi-display or Wayland behavior.

See the [cross-platform guide](Website/content/project-docs/docs/cross-platform.md)
for supported targets and explicit limitations. Shared-source proof, public
packages, native acceptance and PowerBGInfo publication are distinct delivery
steps.
