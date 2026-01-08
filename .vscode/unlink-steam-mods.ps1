# unlink-steam-mods.ps1
# Removes symbolic links created by link-steam-mods.ps1
# This cleanup ensures workshop mods don't interfere with normal game launches

param(
    [string]$TrackingFile = "$PSScriptRoot\steam-mod-links.txt"
)

$ErrorActionPreference = "Stop"

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Unlinking Steam Workshop Mods" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan

# Check if tracking file exists
if (-not (Test-Path $TrackingFile)) {
    Write-Host "No tracking file found. No links to remove." -ForegroundColor Yellow
    exit 0
}

# Get Bannerlord directory from environment
$bannerlordDir = $env:BANNERLORD_GAME_DIR
if ([string]::IsNullOrEmpty($bannerlordDir)) {
    Write-Host "ERROR: BANNERLORD_GAME_DIR environment variable not set" -ForegroundColor Red
    exit 1
}

$modulesDir = Join-Path $bannerlordDir "Modules"
if (-not (Test-Path $modulesDir)) {
    Write-Host "ERROR: Modules directory not found: $modulesDir" -ForegroundColor Red
    exit 1
}

# Read list of linked mods
$linkedMods = Get-Content $TrackingFile -ErrorAction SilentlyContinue
if ($null -eq $linkedMods -or $linkedMods.Count -eq 0) {
    Write-Host "No mods to unlink." -ForegroundColor Yellow
    Remove-Item $TrackingFile -Force -ErrorAction SilentlyContinue
    exit 0
}

Write-Host "Removing $($linkedMods.Count) symbolic link(s)..." -ForegroundColor Green

$removedCount = 0
$errorCount = 0

foreach ($modName in $linkedMods) {
    if ([string]::IsNullOrWhiteSpace($modName)) {
        continue
    }
    
    $targetPath = Join-Path $modulesDir $modName
    
    Write-Host "`nProcessing: $modName" -ForegroundColor Cyan
    
    # Check if path exists
    if (-not (Test-Path $targetPath)) {
        Write-Host "  Link does not exist (may have been manually removed)" -ForegroundColor Yellow
        $removedCount++
        continue
    }
    
    # Verify it's a symbolic link before removing
    $item = Get-Item $targetPath
    if ($item.Attributes -band [System.IO.FileAttributes]::ReparsePoint) {
        try {
            Remove-Item $targetPath -Recurse -Force -ErrorAction Stop
            Write-Host "  Removed symbolic link: $modName" -ForegroundColor Green
            $removedCount++
        } catch {
            Write-Host "  ERROR: Failed to remove link: $_" -ForegroundColor Red
            $errorCount++
        }
    } else {
        Write-Host "  WARNING: Path exists but is not a symbolic link. Skipping removal." -ForegroundColor Yellow
        Write-Host "  This folder may be a manually installed mod." -ForegroundColor Yellow
        $errorCount++
    }
}

# Remove tracking file
Remove-Item $TrackingFile -Force -ErrorAction SilentlyContinue

Write-Host "`n========================================" -ForegroundColor Cyan
if ($errorCount -eq 0) {
    Write-Host "Successfully removed all $removedCount link(s)" -ForegroundColor Green
} else {
    Write-Host "Removed $removedCount link(s) with $errorCount error(s)" -ForegroundColor Yellow
}
Write-Host "========================================" -ForegroundColor Cyan
