#Requires -Version 5.1
# Pushes local changes in a subtree back to its remote using worktree copy.
# Use this as a fallback when push-subtree.ps1 gets stuck.
# Usage: powershell -File scripts\push-subtree-sync.ps1 -PREFIX aloe-utils

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

$REMOTE_SHA = (git ls-remote $PREFIX main | Select-Object -First 1).Split()[0].Trim()
if (-not $REMOTE_SHA) {
    Write-Error "Error: could not get remote HEAD for '$PREFIX'."
    exit 1
}

Write-Host "  [push-sync] $PREFIX -> main (worktree mode) ..."
Write-Host "  [push-sync] remote HEAD: $REMOTE_SHA"

$WORKTREE = "$env:TEMP\aloe-push-sync-$PREFIX"

if (Test-Path $WORKTREE) {
    git worktree remove --force $WORKTREE 2>&1 | Out-Null
    Remove-Item -Recurse -Force $WORKTREE -ErrorAction SilentlyContinue
}

git worktree add $WORKTREE $REMOTE_SHA
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }

try {
    Copy-Item -Path "$REPO\$PREFIX\*" -Destination $WORKTREE -Recurse -Force

    Set-Location $WORKTREE
    git add -A
    $STATUS = (git status --porcelain).Trim()
    if (-not $STATUS) {
        Write-Host "  [push-sync] nothing to push: $PREFIX"
    } else {
        git commit -m "Sync changes from monorepo"
        if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
        git push $PREFIX HEAD:main
        if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
        Write-Host "  [push-sync] done: $PREFIX"
    }
} finally {
    Set-Location $REPO
    git worktree remove --force $WORKTREE 2>&1 | Out-Null
}
