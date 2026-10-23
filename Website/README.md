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

## Quality gates and source snapshots

`./build.ps1 -CI` verifies the immutable engine lock, builds sitemap and agent discovery files, checks SEO, navigation, links and assets, then optimizes the output and repeats the checks. The image optimizer generates WebP variants at 480, 960 and 1440 pixels for `powerbginfo/**`. Both audits enforce at most six render-blocking head resources.

The verify baseline contains one known engine warning: collection-relative TOC entries are compared against site-root routes ([PSPublishModule #1165](https://github.com/EvotecIT/PSPublishModule/issues/1165)). Keep the shared TOC relative for the evotec.xyz importer. Remove that baseline after the engine fix is released and the lock is updated. The SEO baseline records 17 existing title/description length advisories; it does not allow missing canonicals, broken links or content leaks. Copy changes belong with the site's visual/copy owner.

The data scripts default `RepositoryPath` to this repository. Pass the exact `-Ref` recorded by both showcase/feature render manifests to `Sync-PowerBGInfo.ps1`. `Render-PowerBGInfoSteps.ps1 -Ref v2.0.6` exports an isolated source snapshot before building and rendering on Windows. An explicit `ModulePath` must point to a separate source snapshot. Both scripts stage output and restore the previous website snapshot if promotion fails.

The committed renders and source URLs remain pinned to `2f6f22d910ba1da63adbebf18147e5085158ed08`: release v2.0.6 changes their source scripts and renderer. Refresh renders and recorded sources together on Windows before moving those links to a release tag. Do not relabel old images as release output.

## Launch and GitHub Pages

The deployment workflow builds only `v2-speedygonzales` and publishes through the pinned PowerForge GitHub Pages workflow. Pull requests use the separate website CI workflow and do not deploy. `static/CNAME` is copied to the artifact as `powerbginfo.com`; GitHub Actions Pages deployments also require the repository's Pages custom-domain setting (a CNAME artifact alone does not set it).

Before the owner merges or dispatches deployment:

1. Pages is configured with Source **GitHub Actions** and Custom domain **powerbginfo.com**. Verify these settings before launch. This repository's default branch is `v2-speedygonzales`.
2. Configure the `production` environment with `CLOUDFLARE_ZONE_ID` (variable) and `CLOUDFLARE_API_TOKEN` (secret) for the pinned Cloudflare site-policy action. It applies the site's response/cache policy after deployment. HSTS is explicitly disabled for this initial cutover; review it separately after HTTPS works.
3. The owner adds these DNS records in Cloudflare, initially DNS only so GitHub can verify the domain and issue its certificate:

   | Type | Name | Value |
   | --- | --- | --- |
   | A | `@` | `185.199.108.153` |
   | A | `@` | `185.199.109.153` |
   | A | `@` | `185.199.110.153` |
   | A | `@` | `185.199.111.153` |
   | CNAME | `www` | `evotecit.github.io` |

   Optional IPv6: four `AAAA` records at `@`: `2606:50c0:8000::153`, `2606:50c0:8001::153`, `2606:50c0:8002::153`, `2606:50c0:8003::153`. Do not point `www` at a repository path. These values follow [GitHub's custom-domain documentation](https://docs.github.com/en/pages/configuring-a-custom-domain-for-your-github-pages-site/managing-a-custom-domain-for-your-github-pages-site).
4. After the approved deployment, verify the apex and www redirect, enable Enforce HTTPS once the certificate is available, and verify the final Cloudflare policy on the live site.

Launch is pending. The root README labels its site link accordingly. `PowerBGInfo.psd1` contains the new ProjectUri and site-hosted PNG IconUri with an explicit cutover comment: publish that module metadata only after the site is live. This website PR does not publish a module.
