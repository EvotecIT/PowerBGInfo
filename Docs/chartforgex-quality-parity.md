# ChartForgeX Image Quality Parity

PowerBGInfo must not remove `ImagePlayground.Gdi` until the ChartForgeX composition path proves equal or better output quality on real wallpaper scenarios.

Create render sets from the same JSON configurations in both branches:

```powershell
.\Build\New-PowerBGInfoQualityRenderSet.ps1 `
  -OutputDirectory .\artifacts\quality\gdi
```

Then run the same command from the ChartForgeX migration branch with `-OutputDirectory .\artifacts\quality\chartforgex`.

Use the quality gate to compare the two render sets:

```powershell
.\Build\Test-ChartForgeXQualityParity.ps1 `
  -BaselineDirectory .\artifacts\quality\gdi `
  -CandidateDirectory .\artifacts\quality\chartforgex `
  -OutputDirectory .\artifacts\quality\report `
  -Recursive
```

The gate writes:

- `quality-report.json` with machine-readable metrics.
- `quality-report.html` with baseline, candidate, and diff heatmap contact sheets.
- `diffs\*.diff.png` images showing where pixels changed.

The gate records strict pixel metrics and perceptual metrics. A case passes when it satisfies either the strict pixel thresholds or the perceptual thresholds. The strict path catches accidental same-engine changes. The perceptual path is for the ChartForgeX migration, where JPEG codec differences and antialiasing can legitimately move many pixels while preserving the wallpaper visually.

- mean absolute channel error <= `1.25`
- channel RMSE <= `3.0`
- max channel error <= `48`
- changed pixels <= `2%`
- perceptual mean absolute channel error <= `3.0`
- perceptual channel RMSE <= `12.0`
- structural similarity (SSIM) >= `0.995`

Before migrating the runtime renderer, build a baseline set that covers:

- text-heavy labels and values on dark and bright wallpaper
- ChartForgeX chart overlays
- topology overlays
- visual canvas overlays
- transparent PNG output
- JPEG wallpaper output
- image overlays with opacity
- top-left, top-right, bottom-left, bottom-right, center, and stacked placement
- 1080p, 4K, and ultrawide backgrounds

Any failing case should be treated as a ChartForgeX engine or PowerBGInfo mapping issue first. A perceptual pass is not a blind approval: review `quality-report.html` when the strict pixel path fails, especially for text placement, charts, opacity, and resized backgrounds.
