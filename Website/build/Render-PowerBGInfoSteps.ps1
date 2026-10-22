#Requires -Version 7.4
<#
.SYNOPSIS
    Runs the PowerBGInfo build-it-yourself steps for powerbginfo.com and records each script, the lines
    it changed and the wallpaper it rendered.

.DESCRIPTION
    Reads Website/powerbginfo/steps.json, runs each Website/powerbginfo/steps/<id>.ps1 in a fresh pwsh process with
    the PowerBGInfo build at -ModulePath imported, and writes:
      - Website/static/powerbginfo/build/<id>.png  (the wallpaper each step rendered)
      - Website/data/powerbginfo-build.json         (scripts, changed lines, console output and the source snippets)

    Every step renders with -Target File; nothing is applied to the desktop. Each step is diffed against the step
    before it (or its "base"), so the page can show only the lines that changed. The snippets under
    Website/powerbginfo/sources/ are parsed, not run, and copied into the data file.

    RepositoryPath defaults to this repository. Ref names the PowerBGInfo commit to render.
    Without ModulePath, the script exports that commit to an owned temporary source snapshot and builds there.
    ModulePath can name a separate source snapshot; do not point it at the website working tree. The script checks that the
    module's PowerBGInfo.psd1, PowerBGInfo.psm1 and tracked Sources files match that commit, then builds the
    net8.0-windows development assembly before importing the module. This prevents a stale DLL in the scratch module
    from rendering steps while the data file claims a newer source commit. Each run uses the en-US culture and sets
    COMPUTERNAME to the sample name the steps use, so a render never carries the name of the machine that produced it.

    The script owns static/powerbginfo/build/ and stages new PNG and JSON output before promoting both together.
    On failure it restores the previous website output. It never changes the source repository.
    Rendering requires Windows and PowerShell 7.4 or later.

.EXAMPLE
    pwsh ./Website/build/Render-PowerBGInfoSteps.ps1 -Ref v2.0.6
#>
[CmdletBinding()]
param(
    [string] $ModulePath,
    [string] $RepositoryPath = (Split-Path -Parent (Split-Path -Parent $PSScriptRoot)),
    [Parameter(Mandatory)] [string] $Ref,
    # Optional ChartForgeX project used to rebuild PowerBGInfo against a local development source.
    [string] $ChartForgeXProjectPath,
    # Lines shown on the page must fit a code block without horizontal scrolling.
    [int] $MaximumLineLength = 79
)

$ErrorActionPreference = 'Stop'
$website = Split-Path -Parent $PSScriptRoot
$sourceRoot = Join-Path $website 'powerbginfo'
$target = Join-Path $website 'static/powerbginfo/build'
$dataPath = Join-Path $website 'data/powerbginfo-build.json'
$repository = 'EvotecIT/PowerBGInfo'
$sampleComputerName = 'WS-FIN-0042'

$commit = (git -C $RepositoryPath rev-parse --verify "$Ref^{commit}").Trim()
if ($LASTEXITCODE -ne 0 -or -not $commit) { throw "Cannot resolve '$Ref' in $RepositoryPath." }
$snapshotRoot = $null
try {
if ([string]::IsNullOrWhiteSpace($ModulePath)) {
    $snapshotRoot = Join-Path ([System.IO.Path]::GetTempPath()) "PowerBGInfo-source-$([guid]::NewGuid().ToString('N'))"
    New-Item -ItemType Directory -Path $snapshotRoot | Out-Null
    $archivePath = Join-Path $snapshotRoot 'source.tar'
    git -C $RepositoryPath archive --format=tar "--output=$archivePath" $commit
    if ($LASTEXITCODE -ne 0) { throw "Cannot export PowerBGInfo source at $commit." }
    tar -xf $archivePath -C $snapshotRoot
    if ($LASTEXITCODE -ne 0) { throw "Cannot extract PowerBGInfo source at $commit." }
    Remove-Item -LiteralPath $archivePath
    $ModulePath = Join-Path $snapshotRoot 'PowerBGInfo.psd1'
}
$ModulePath = (Resolve-Path -LiteralPath $ModulePath).Path
$moduleRoot = Split-Path -Parent $ModulePath
$sourceRepositoryRoot = (git -C $RepositoryPath rev-parse --show-toplevel).Trim()
if ($LASTEXITCODE -ne 0) { throw "Cannot locate the PowerBGInfo repository root." }
if ([System.IO.Path]::GetFullPath($moduleRoot).TrimEnd('\', '/') -eq [System.IO.Path]::GetFullPath($sourceRepositoryRoot).TrimEnd('\', '/')) {
    throw 'ModulePath must be an isolated source snapshot. Omit it to export the recorded commit without building in the source repository.'
}

function Get-Text([string] $Text) { ($Text.Replace("`r", '').TrimStart([char] 0xFEFF)).TrimEnd() }

# The module must be the commit we record: compare its manifest and loader with the committed files.
foreach ($file in 'PowerBGInfo.psd1', 'PowerBGInfo.psm1') {
    $committed = (git -C $RepositoryPath show "${commit}:$file") -join "`n"
    if ($LASTEXITCODE -ne 0) { throw "$file does not exist at $commit." }
    $local = Get-Content -LiteralPath (Join-Path $moduleRoot $file) -Raw
    if ((Get-Text $committed) -ne (Get-Text $local)) { throw "$moduleRoot\$file does not match $file at $commit." }
}
$moduleVersion = (Import-PowerShellDataFile -LiteralPath $ModulePath).ModuleVersion

$trackedFiles = @(git -C $RepositoryPath ls-tree -r --name-only $commit)
if ($LASTEXITCODE -ne 0 -or -not $trackedFiles.Count) { throw "Cannot list build inputs at $commit." }
$buildInputPattern = '(?i)(\.(cs|csproj|props|targets|resx|razor|xaml|vb|fs|json)$|(^|/)(Public|Private|Classes|Enums)/.*\.ps1$|(^|/)(global\.json|NuGet\.Config|Directory\.Build\.[^/]+|Directory\.Packages\.props|packages\.lock\.json|MSBuild\.rsp)$)'
$expectedInputs = @($trackedFiles | Where-Object { $_ -match $buildInputPattern })
$excludedOutput = '(^|/)(\.git|bin|obj|artifacts|TestResults|\.vs)(/|$)'
$actualInputs = @(Get-ChildItem -LiteralPath $moduleRoot -File -Recurse -Force | ForEach-Object {
    $relative = [System.IO.Path]::GetRelativePath($moduleRoot, $_.FullName).Replace('\', '/')
    if ($relative -notmatch $excludedOutput -and $relative -match $buildInputPattern) { $relative }
})
$expectedSet = [System.Collections.Generic.HashSet[string]]::new([StringComparer]::Ordinal)
$actualSet = [System.Collections.Generic.HashSet[string]]::new([StringComparer]::Ordinal)
foreach ($path in $expectedInputs) { [void] $expectedSet.Add($path) }
foreach ($path in $actualInputs) { [void] $actualSet.Add($path) }
$extraInputs = @($actualSet | Where-Object { -not $expectedSet.Contains($_) })
$missingInputs = @($expectedSet | Where-Object { -not $actualSet.Contains($_) })
if ($extraInputs.Count -or $missingInputs.Count) {
    throw "The module build tree does not match the recorded commit's build inputs. Extra: [$($extraInputs -join ', ')]; missing: [$($missingInputs -join ', ')]."
}
foreach ($relativePath in $expectedInputs) {
    $localSource = Join-Path $moduleRoot $relativePath
    $expectedBlob = (git -C $RepositoryPath rev-parse "${commit}:$relativePath").Trim()
    if ($LASTEXITCODE -ne 0) { throw "Cannot resolve '$relativePath' at $commit." }
    $actualBlob = (git -C $RepositoryPath hash-object "--path=$relativePath" -- $localSource).Trim()
    if ($LASTEXITCODE -ne 0 -or $actualBlob -ne $expectedBlob) { throw "$localSource does not match '$relativePath' at $commit." }
}

$project = Join-Path $moduleRoot 'Sources/PowerBGInfo.PowerShell/PowerBGInfo.PowerShell.csproj'
if (-not (Test-Path -LiteralPath $project -PathType Leaf)) { throw "The source build is missing '$project'. Pass a PowerBGInfo source checkout or archive." }
$externalBuildInputs = [System.Collections.Generic.List[string]]::new()
$directory = Split-Path -Parent $project
$buildInputNames = 'Directory.Build.props', 'Directory.Build.targets', 'Directory.Packages.props', 'Directory.Build.rsp', 'MSBuild.rsp', 'global.json', 'NuGet.Config'
while ($directory) {
    foreach ($name in $buildInputNames) {
        $candidate = Join-Path $directory $name
        if (Test-Path -LiteralPath $candidate -PathType Leaf) {
            $relative = [System.IO.Path]::GetRelativePath($moduleRoot, $candidate).Replace('\', '/')
            if ($relative.StartsWith('../', [StringComparison]::Ordinal) -or $relative -eq '..') {
                $externalBuildInputs.Add($candidate)
            } elseif (-not $expectedSet.Contains($relative)) {
                throw "Build input '$candidate' is not recorded in commit $commit."
            }
        }
    }
    $parent = Split-Path -Parent $directory
    if (-not $parent -or $parent -eq $directory) { break }
    $directory = $parent
}
if ($externalBuildInputs.Count) {
    throw "The build depends on inputs outside the recorded module snapshot: [$($externalBuildInputs -join ', ')]. Pass a repository-root snapshot that includes these inputs."
}
if (-not $IsWindows) { throw 'PowerBGInfo step rendering requires Windows; the committed renders are retained on other platforms.' }
Write-Host "Building PowerBGInfo $($commit.Substring(0, 7)) for the step renders..." -ForegroundColor Cyan
Push-Location $moduleRoot
try {
    $buildArguments = @(
        'build', 'Sources/PowerBGInfo.PowerShell/PowerBGInfo.PowerShell.csproj',
        '--configuration', 'Debug', '--framework', 'net8.0-windows', '--no-incremental',
        '--property:GeneratePackageOnBuild=false', '-nodeReuse:false', '--disable-build-servers'
    )
    if ($ChartForgeXProjectPath) { $buildArguments += "--property:ChartForgeXProjectPath=$ChartForgeXProjectPath" }
    dotnet @buildArguments
    $buildExitCode = $LASTEXITCODE
} finally {
    Pop-Location
}
if ($buildExitCode -ne 0) { throw "PowerBGInfo $($commit.Substring(0, 7)) source build failed with exit code $buildExitCode." }

function Test-Lines([string] $Name, [string] $Text) {
    $wide = @($Text.Split("`n") | Where-Object { $_.Length -gt $MaximumLineLength })
    if ($wide.Count) { throw "$Name has $($wide.Count) line(s) over $MaximumLineLength characters, for example: $($wide[0])" }
    $tokens = $null; $errors = $null
    [void] [System.Management.Automation.Language.Parser]::ParseInput($Text, [ref] $tokens, [ref] $errors)
    if ($errors.Count) { throw "$Name does not parse: $($errors[0].Message)" }
}

function Remove-SharedIndent([string] $Text) {
    $lines = $Text.Split("`n")
    $indents = @($lines | Where-Object { $_.Trim() } | ForEach-Object { $_.Length - $_.TrimStart(' ').Length })
    $shared = if ($indents.Count) { ($indents | Measure-Object -Minimum).Minimum } else { 0 }
    return (($lines | ForEach-Object { if ($_.Length -ge $shared) { $_.Substring($shared) } else { $_.TrimStart(' ') } }) -join "`n")
}

# Lines of $New that are not part of the longest common subsequence with $Old, grouped into runs.
function Get-ChangedLines([string[]] $Old, [string[]] $New) {
    $m = $Old.Count; $n = $New.Count
    $length = [int[, ]]::new($m + 1, $n + 1)
    for ($i = $m - 1; $i -ge 0; $i--) {
        for ($j = $n - 1; $j -ge 0; $j--) {
            $length[$i, $j] = if ($Old[$i] -ceq $New[$j]) { $length[($i + 1), ($j + 1)] + 1 } else { [Math]::Max($length[($i + 1), $j], $length[$i, ($j + 1)]) }
        }
    }
    $kept = [bool[]]::new($n); $matched = [bool[]]::new($m); $i = 0; $j = 0
    while ($i -lt $m -and $j -lt $n) {
        if ($Old[$i] -ceq $New[$j]) { $kept[$j] = $true; $matched[$i] = $true; $i++; $j++ }
        elseif ($length[($i + 1), $j] -ge $length[$i, ($j + 1)]) { $i++ }
        else { $j++ }
    }
    $runs = [System.Collections.Generic.List[string]]::new(); $run = [System.Collections.Generic.List[string]]::new()
    for ($j = 0; $j -le $n; $j++) {
        if ($j -lt $n -and -not $kept[$j]) { $run.Add($New[$j]); continue }
        $hunk = ($run -join "`n").Trim("`n")
        if ($hunk.Trim()) { $runs.Add($hunk) }
        $run.Clear()
    }
    [pscustomobject] @{
        # Changed lines are shown without the indentation they share, so nested lines need no horizontal scrolling.
        Text    = Remove-SharedIndent ($runs -join "`n`n")
        Added   = @(0..($n - 1) | Where-Object { -not $kept[$_] -and $New[$_].Trim() }).Count
        Removed = @(0..($m - 1) | Where-Object { -not $matched[$_] -and $Old[$_].Trim() }).Count
    }
}

function Get-PngSize([string] $Path) {
    $bytes = [System.IO.File]::ReadAllBytes($Path)
    $read = { param($offset) ([int] $bytes[$offset] -shl 24) -bor ([int] $bytes[$offset + 1] -shl 16) -bor ([int] $bytes[$offset + 2] -shl 8) -bor [int] $bytes[$offset + 3] }
    return @{ Width = & $read 16; Height = & $read 20; Bytes = $bytes.Length }
}

$manifest = Get-Content -LiteralPath (Join-Path $sourceRoot 'steps.json') -Raw | ConvertFrom-Json
$stagingRoot = Join-Path $website ".powerbginfo-steps-$([guid]::NewGuid().ToString('N'))"
$stagedTarget = Join-Path $stagingRoot 'build'
$stagedDataPath = Join-Path $stagingRoot 'powerbginfo-build.json'
$keepStaging = $false
New-Item -ItemType Directory -Force -Path $stagedTarget | Out-Null
try {
$work = Join-Path ([System.IO.Path]::GetTempPath()) "PowerBGInfo-steps-$PID"

$scripts = @{}
$previous = $null
try {
    $steps = foreach ($step in $manifest.steps) {
        $file = "$($step.id).ps1"
        $text = Get-Text (Get-Content -LiteralPath (Join-Path $sourceRoot "steps/$file") -Raw)
        Test-Lines $file $text
        $scripts[$step.id] = $text
        $base = if ($step.base) { $step.base } else { $previous }
        $changed = if ($base) { Get-ChangedLines $scripts[$base].Split("`n") $text.Split("`n") } else {
            [pscustomobject] @{ Text = $text; Added = @($text.Split("`n") | Where-Object { $_.Trim() }).Count; Removed = 0 }
        }
        if ($step.section -ne 'logic') { $previous = $step.id }

        $output = Join-Path $work $step.id
        New-Item -ItemType Directory -Force -Path $output | Out-Null
        $command = @"
`$culture = [System.Globalization.CultureInfo]::GetCultureInfo('en-US')
[System.Threading.Thread]::CurrentThread.CurrentCulture = `$culture
[System.Threading.Thread]::CurrentThread.CurrentUICulture = `$culture
`$env:COMPUTERNAME = '$sampleComputerName'
Import-Module '$($ModulePath.Replace("'", "''"))' -WarningAction SilentlyContinue
`$ErrorActionPreference = 'Stop'
& '$((Join-Path $sourceRoot "steps/$file").Replace("'", "''"))' -OutputDirectory '$($output.Replace("'", "''"))' *>&1 | ForEach-Object { if (`$_ -is [System.Management.Automation.WarningRecord]) { "WARNING: `$(`$_.Message)" } else { `$_ } } | Out-String -Width 200
"@
        $console = pwsh -NoProfile -NonInteractive -Command $command 2>&1 | Out-String
        if ($LASTEXITCODE -ne 0) { throw "Step '$($step.id)' failed:`n$console" }
        $png = Join-Path $output 'wallpaper.png'
        if (-not (Test-Path -LiteralPath $png)) { throw "Step '$($step.id)' wrote no wallpaper.png:`n$console" }
        Copy-Item -LiteralPath $png -Destination (Join-Path $stagedTarget "$($step.id).png")
        $size = Get-PngSize $png

        # The data region, when the step has one: the part the reader replaces with their own logic.
        $region = [regex]::Match($text, '(?ms)^#region Data.*?^#endregion')
        # Keep the warnings a reader would see; drop the module's development-build notice and the output path.
        $lines = (Get-Text $console).Split("`n") | ForEach-Object { $_.TrimEnd() } |
            Where-Object { $_ -match '^WARNING: ' -and $_ -notmatch '^WARNING: Development mode' }
        Write-Host "$($step.id): +$($changed.Added) -$($changed.Removed), $([math]::Round($size.Bytes / 1KB)) KB"
        [ordered] @{
            id       = $step.id
            step     = $step.step
            group    = $step.group
            section  = if ($step.section) { $step.section } else { 'build' }
            title    = $step.title
            body     = $step.body
            file     = $file
            script   = $text + "`n"
            lines    = $text.Split("`n").Count
            data     = if ($region.Success) { $region.Value + "`n" } else { $null }
            changed  = $changed.Text + "`n"
            added    = $changed.Added
            removed  = $changed.Removed
            warnings = (@($lines) -join "`n")
            render   = [ordered] @{
                url    = "/powerbginfo/build/$($step.id).png"
                file   = "$($step.id).png"
                width  = $size.Width
                height = $size.Height
                kb     = [math]::Round($size.Bytes / 1KB)
            }
        }
    }
} finally {
    if (Test-Path -LiteralPath $work) { [System.IO.Directory]::Delete($work, $true) }
}

$sources = foreach ($source in $manifest.sources) {
    $text = Get-Text (Get-Content -LiteralPath (Join-Path $sourceRoot "sources/$($source.file)") -Raw)
    Test-Lines $source.file $text
    [ordered] @{ id = $source.id; label = $source.label; body = $source.body; file = $source.file; code = $text + "`n" }
}

$data = [ordered] @{
    generatedBy = 'Website/build/Render-PowerBGInfoSteps.ps1'
    source      = [ordered] @{
        repository = $repository
        url        = "https://github.com/$repository"
        commit     = $commit
        short      = $commit.Substring(0, 7)
        version    = "$moduleVersion"
        template   = 'Examples/Run.BGInfo.Template.ps1'
        templateUrl = "https://github.com/$repository/blob/$commit/Examples/Run.BGInfo.Template.ps1"
    }
    steps       = @($steps)
    sources     = @($sources)
}
Set-Content -LiteralPath $stagedDataPath -Value (($data | ConvertTo-Json -Depth 8).Replace("`r`n", "`n") + "`n") -NoNewline -Encoding utf8NoBOM

$targetBackup = Join-Path $stagingRoot 'previous-build'
$dataBackup = Join-Path $stagingRoot 'previous-build.json'
$targetMoved = $false
$targetPromoted = $false
$dataMoved = $false
$dataPromoted = $false
try {
    if (Test-Path -LiteralPath $target -PathType Container) { Move-Item -LiteralPath $target -Destination $targetBackup; $targetMoved = $true }
    Move-Item -LiteralPath $stagedTarget -Destination $target; $targetPromoted = $true
    if (Test-Path -LiteralPath $dataPath -PathType Leaf) { Move-Item -LiteralPath $dataPath -Destination $dataBackup; $dataMoved = $true }
    Move-Item -LiteralPath $stagedDataPath -Destination $dataPath; $dataPromoted = $true
} catch {
    $promotionError = $_
    $rollbackErrors = [System.Collections.Generic.List[string]]::new()
    if ($dataPromoted) { try { Remove-Item -LiteralPath $dataPath } catch { $rollbackErrors.Add("remove promoted data: $($_.Exception.Message)") } }
    if ($dataMoved) { try { Move-Item -LiteralPath $dataBackup -Destination $dataPath } catch { $rollbackErrors.Add("restore previous data: $($_.Exception.Message)") } }
    if ($targetPromoted) { try { [System.IO.Directory]::Delete($target, $true) } catch { $rollbackErrors.Add("remove promoted renders: $($_.Exception.Message)") } }
    if ($targetMoved) { try { Move-Item -LiteralPath $targetBackup -Destination $target } catch { $rollbackErrors.Add("restore previous renders: $($_.Exception.Message)") } }
    if ($rollbackErrors.Count) {
        $keepStaging = $true
        throw "Promotion failed: $($promotionError.Exception.Message) Rollback also failed: $($rollbackErrors -join '; '). Backups remain under '$stagingRoot'."
    }
    throw $promotionError
}
$total = (Get-ChildItem -LiteralPath $target -Filter '*.png' -File | Measure-Object Length -Sum).Sum
Write-Host "PowerBGInfo $($commit.Substring(0, 7)): $(@($steps).Count) steps, $(@($sources).Count) snippets, $([math]::Round($total / 1KB)) KB -> $dataPath"
} finally {
    if (-not $keepStaging -and (Test-Path -LiteralPath $stagingRoot -PathType Container)) {
        [System.IO.Directory]::Delete($stagingRoot, $true)
    }
}

} finally {
    if ($snapshotRoot -and (Test-Path -LiteralPath $snapshotRoot -PathType Container)) {
        [System.IO.Directory]::Delete($snapshotRoot, $true)
    }
}
