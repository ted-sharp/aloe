#Requires -Version 5.1
# Pushes local changes in a subtree back to its remote.
# Usage: powershell -File scripts\push-subtree.ps1 -PREFIX aloe-utils

param(
    [Parameter(Mandatory = $true)]
    [string]$PREFIX
)

$ErrorActionPreference = 'Stop'

$REPO = (git rev-parse --show-toplevel).Trim()
Set-Location $REPO

git remote get-url $PREFIX 2>&1 | Out-Null
if ($LASTEXITCODE -ne 0) {
    Write-Error "Error: remote '$PREFIX' not found. Run 'task setup' first."
    exit 1
}

Write-Host "  [push] $PREFIX -> main ..."
git subtree push "--prefix=$PREFIX" $PREFIX main
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
Write-Host "  [push] done: $PREFIX"
