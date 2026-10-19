# PowerBGInfo Website Content

This folder contains curated source content that the Evotec website imports for the PowerBGInfo project page.

## Layout

- `content/project-docs/docs/` contains the project documentation shown under /projects/powerbginfo/docs/, including the production deployment guides (`deployment.md`, `task-scheduler.md`, `group-policy.md`, `intune.md`, `configuration-manager.md`, `remote-desktop.md`, `security.md`, `troubleshooting.md`). These files are the only copy of that material; the repository README links to the published pages. Keep their file names stable: other sites link to them.
- `content/examples/` contains curated website examples shown under /projects/powerbginfo/examples/.
- API artifacts are not enabled from this folder yet. Add `WebsiteArtifacts/apidocs` and wire it in the website catalog only when external help/API metadata is generated and reviewed.

## Editing Rules

- Keep this folder intentional and small.
- Do not mirror raw `Examples/`, `Example/`, or generated output folders into the public website.
- Add only examples that have a clear explanation, a small code sample, and a link back to the original source file.
- Keep links rooted at /projects/powerbginfo/ so the same content works on localhost, evotec.xyz, and evotec.pl. Between docs pages use the relative form `../security/` or `../security/#anchor`; the hub's link check resolves those, while `security.md` links would break on the site.

The website pipeline prefers this `Website/content/...` layout over legacy root-level content folders.

