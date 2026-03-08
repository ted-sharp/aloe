#Requires -Version 5.1
# Pushes local changes in all subtrees back to their remotes.
# Usage: powershell -File scripts\push-subtrees.ps1  (run from repo root)

$ErrorActionPreference = 'Stop'

$REPO = (git rev-parse --show-toplevel).Trim()
Set-Location $REPO

. "$PSScriptRoot\subtrees.ps1"

foreach ($entry in $SUBTREES) {
    $prefix = $entry.Prefix
    Write-Host "  [push] $prefix -> main ..."
    git subtree push "--prefix=$prefix" $prefix main
    if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
    Write-Host "  [push] done: $prefix"
}

Write-Host ""
Write-Host "=== Done ==="
