# PowerBGInfo Website

This folder is the source of two things:

- **powerbginfo.com**, a standalone PowerForge site for PowerBGInfo, DesktopManager and InformationBox (`site.json`, `pipeline.json`, `themes/powerbginfo/`, `content/pages/`, `data/`, `static/`).
- **The PowerBGInfo docs that evotec.xyz imports** for /projects/powerbginfo/ (`content/project-docs/docs/` and `content/examples/`). powerbginfo.com publishes the same files under /docs/ and /examples/.

## Layout

- `content/project-docs/docs/` holds the documentation, including the production deployment guides (`deployment.md`, `task-scheduler.md`, `group-policy.md`, `intune.md`, `configuration-manager.md`, `remote-desktop.md`, `security.md`, `troubleshooting.md`). These files are the only copy of that material; the repository README links to the published pages. Keep their file names stable: other sites link to them. `toc.yml` orders the docs menu on both sites.
- `content/examples/` holds curated website examples.
- `content/pages/` holds the powerbginfo.com pages. Most are front matter only; their copy lives in `data/`.
- `data/` holds page data: `home.json`, `products.json`, `platforms.json` (the one source of platform support, read by the home page and /platforms/), `desktopmanager.json`, `informationbox.json`, `platforms-page.json`, and the wallpaper story data `powerbginfo.json`, `powerbginfo-showcase.json` and `powerbginfo-build.json`.
- `build/Sync-PowerBGInfo.ps1` snapshots the showcase and feature renders into `static/powerbginfo/` and writes `data/powerbginfo-showcase.json`. `build/Render-PowerBGInfoSteps.ps1` runs `powerbginfo/steps/*.ps1` and writes `static/powerbginfo/build/` and `data/powerbginfo-build.json`. Both read this repository at a recorded commit and never change it.
- `themes/powerbginfo/` is forked from the ChartForgeX site theme. Classes use the `pb-` prefix (shared components) and `pbg-` (wallpaper story parts in `partials/pbg/`).

## Building

```powershell
./build.ps1            # full pipeline: build, sitemap, verify, audit
./build.ps1 -Serve     # build and serve locally
```

`build.ps1` finds the PowerForge engine through `-PowerForgeRoot`, `$env:POWERFORGE_ROOT` or a sibling `PSPublishModule` checkout.

## Editing rules

- Keep this folder intentional and small. Do not mirror raw `Examples/` or generated output folders into it.
- Add only examples that have a clear explanation, a small code sample and a link back to the original source file.
- Docs links stay rooted at /projects/powerbginfo/ so the same files work on evotec.xyz and evotec.pl. powerbginfo.com rewrites them when it renders a docs page (`themes/powerbginfo/layouts/docs.html`). Between docs pages use the relative form `../security/` or `../security/#anchor`; `security.md` links break on both sites.
- Platform support changes in `data/platforms.json` only. Use `preview` until the release that ships it is published.
- Every wallpaper on the site is real PowerBGInfo output. Mockups and illustrations must say so in their caption.
