#Requires -Version 5.1
# Pulls updates from all subtrees.
# Usage: powershell -File scripts\pull-subtrees.ps1  (run from repo root)

$ErrorActionPreference = 'Stop'

$REPO = (git rev-parse --show-toplevel).Trim()
Set-Location $REPO

. "$PSScriptRoot\subtrees.ps1"

foreach ($entry in $SUBTREES) {
    $prefix = $entry.Prefix
    Write-Host "  [pull] $prefix ..."
    git subtree pull "--prefix=$prefix" $prefix main
    if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
    Write-Host "  [pull] done: $prefix"
}

Write-Host ""
Write-Host "=== Done ==="
