# link-steam-mods.ps1
# Creates symbolic links from Bannerlord Modules folder to Steam workshop mods
# This allows debugging with Steam workshop mods without permanently copying them

param(
    [string]$ConfigFile = "$PSScriptRoot\steam-workshop-mods.json"
)

$ErrorActionPreference = "Stop"

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Linking Steam Workshop Mods for Debug" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan

# Check if running as administrator
$isAdmin = ([Security.Principal.WindowsPrincipal] [Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole] "Administrator")
if (-not $isAdmin) {
    Write-Host "WARNING: Not running as Administrator. Symbolic link creation may fail." -ForegroundColor Yellow
    Write-Host "If links fail to create, restart VS Code as Administrator." -ForegroundColor Yellow
}

# Check if config file exists
if (-not (Test-Path $ConfigFile)) {
    Write-Host "Config file not found: $ConfigFile" -ForegroundColor Yellow
    Write-Host "No workshop mods configured for linking." -ForegroundColor Yellow
    exit 0
}

# Load configuration
try {
    $config = Get-Content $ConfigFile -Raw | ConvertFrom-Json
} catch {
    Write-Host "ERROR: Failed to parse config file: $_" -ForegroundColor Red
    exit 1
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

# Get Steam workshop path
$workshopPath = $config.steamWorkshopPath
if ([string]::IsNullOrEmpty($workshopPath)) {
    Write-Host "ERROR: steamWorkshopPath not configured in $ConfigFile" -ForegroundColor Red
    exit 1
}

if (-not (Test-Path $workshopPath)) {
    Write-Host "ERROR: Steam workshop path not found: $workshopPath" -ForegroundColor Red
    exit 1
}

# Create a tracking file to remember which links we created
$trackingFile = Join-Path $PSScriptRoot "steam-mod-links.txt"
$linkedMods = @()

# Process each enabled mod
$enabledMods = $config.mods | Where-Object { $_.enabled -eq $true }
if ($enabledMods.Count -eq 0) {
    Write-Host "No mods enabled in configuration." -ForegroundColor Yellow
    Write-Host "Edit $ConfigFile to enable mods for debugging." -ForegroundColor Yellow
    exit 0
}

Write-Host "Processing $($enabledMods.Count) enabled mod(s)..." -ForegroundColor Green

foreach ($mod in $enabledMods) {
    $workshopId = $mod.workshopId
    $modName = $mod.modName
    
    Write-Host "`nProcessing: $modName (Workshop ID: $workshopId)" -ForegroundColor Cyan
    
    # Source: Steam workshop folder
    $sourcePath = Join-Path $workshopPath $workshopId
    if (-not (Test-Path $sourcePath)) {
        Write-Host "  WARNING: Workshop folder not found: $sourcePath" -ForegroundColor Yellow
        continue
    }
    
    # Target: Bannerlord Modules folder
    $targetPath = Join-Path $modulesDir $modName
    
    # Check if target already exists
    if (Test-Path $targetPath) {
        $item = Get-Item $targetPath
        if ($item.Attributes -band [System.IO.FileAttributes]::ReparsePoint) {
            Write-Host "  Symbolic link already exists: $modName" -ForegroundColor Yellow
            $linkedMods += $modName
        } else {
            Write-Host "  WARNING: Non-link folder already exists: $modName" -ForegroundColor Yellow
            Write-Host "  Skipping to avoid overwriting existing mod installation." -ForegroundColor Yellow
        }
        continue
    }
    
    # Create symbolic link
    try {
        $null = New-Item -ItemType SymbolicLink -Path $targetPath -Target $sourcePath -ErrorAction Stop
        Write-Host "  Created symbolic link: $modName -> $sourcePath" -ForegroundColor Green
        $linkedMods += $modName
    } catch {
        Write-Host "  ERROR: Failed to create symbolic link: $_" -ForegroundColor Red
        if (-not $isAdmin) {
            Write-Host "  Try running VS Code as Administrator." -ForegroundColor Yellow
        }
    }
}

# Save tracking information
if ($linkedMods.Count -gt 0) {
    $linkedMods | Out-File $trackingFile -Encoding UTF8
    Write-Host "`nSuccessfully linked $($linkedMods.Count) mod(s)" -ForegroundColor Green
} else {
    Write-Host "`nNo mods were linked" -ForegroundColor Yellow
}

Write-Host "========================================" -ForegroundColor Cyan
