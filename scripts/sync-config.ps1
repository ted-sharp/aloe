#Requires -Version 5.1
# Copies shared config files from the monorepo root into each subtree directory.
# Run this before 'task treepush' to distribute the latest shared settings.

$ErrorActionPreference = 'Stop'

$REPO = (git rev-parse --show-toplevel).Trim()
Set-Location $REPO

. "$REPO/scripts/subtrees.ps1"

$FLAT_FILES = @(
    'stylecop.json'
    '.editorconfig'
    '.gitattributes'
    '.gitignore'
    'Directory.Build.props'
)

$DOT_SETTINGS_SRC = Join-Path $REPO 'Aloe.slnx.DotSettings'

foreach ($subtree in $SUBTREES) {
    $prefix = $subtree.Prefix
    $subtreeDir = Join-Path $REPO $prefix

    if (-not (Test-Path $subtreeDir)) {
        Write-Host "  [skip] $prefix (directory not found)"
        continue
    }

    Write-Host "  [sync] $prefix ..."

    foreach ($file in $FLAT_FILES) {
        $src = Join-Path $REPO $file
        $dst = Join-Path $subtreeDir $file
        Copy-Item -Path $src -Destination $dst -Force
    }

    $slnxFiles = Get-ChildItem -Path $subtreeDir -Recurse -Filter '*.slnx'
    foreach ($slnx in $slnxFiles) {
        $dstName = [System.IO.Path]::ChangeExtension($slnx.Name, '.slnx.DotSettings')
        $dst = Join-Path $slnx.DirectoryName $dstName
        Copy-Item -Path $DOT_SETTINGS_SRC -Destination $dst -Force
    }
}

Write-Host "sync-push done."
