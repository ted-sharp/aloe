#Requires -Version 5.1
# Adds all subtree remotes and subtrees (idempotent, no --squash).
# Usage: powershell -File scripts\setup-subtrees.ps1  (run from repo root)

$ErrorActionPreference = 'Stop'

$REPO = (git rev-parse --show-toplevel).Trim()
Set-Location $REPO

. "$PSScriptRoot\subtrees.ps1"

Write-Host "=== Step 1: remotes ==="
foreach ($entry in $SUBTREES) {
    $prefix = $entry.Prefix
    $url    = $entry.Url
    git remote get-url $prefix 2>&1 | Out-Null
    if ($LASTEXITCODE -eq 0) {
        Write-Host "  [remote] '$prefix' already exists, skip"
    } else {
        git remote add $prefix $url
        Write-Host "  [remote] added '$prefix' -> $url"
    }
}

Write-Host ""
Write-Host "=== Step 2: subtrees ==="
foreach ($entry in $SUBTREES) {
    $prefix = $entry.Prefix
    if (Test-Path (Join-Path $REPO $prefix)) {
        Write-Host "  [subtree] '$prefix' already exists, skip"
    } else {
        Write-Host "  [subtree] adding '$prefix' ..."
        git subtree add "--prefix=$prefix" $prefix main
        if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
        Write-Host "  [subtree] done: $prefix"
    }
}

Write-Host ""
Write-Host "=== Done ==="
