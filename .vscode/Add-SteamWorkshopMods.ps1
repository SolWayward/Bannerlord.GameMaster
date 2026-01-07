#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Automatically scans Steam Workshop directory and adds mod entries to steam-workshop-mods.json

.DESCRIPTION
    This script scans the Steam Workshop directory for Bannerlord mods, reads their SubModule.xml
    files to get the proper mod names, and adds them to the steam-workshop-mods.json file.
    
    Existing entries are preserved and not duplicated. New mods are added with enabled=false by default.

.PARAMETER JsonPath
    Path to the steam-workshop-mods.json file. Defaults to the current directory.

.PARAMETER EnableNewMods
    If specified, new mods will be added with enabled=true instead of false.

.EXAMPLE
    .\Add-SteamWorkshopMods.ps1
    
.EXAMPLE
    .\Add-SteamWorkshopMods.ps1 -EnableNewMods
#>

param(
    [Parameter(Mandatory=$false)]
    [string]$JsonPath,
    
    [Parameter(Mandatory=$false)]
    [switch]$EnableNewMods
)

# Resolve JsonPath if not provided
if ([string]::IsNullOrWhiteSpace($JsonPath)) {
    # Get the script's directory
    if ($PSScriptRoot) {
        $scriptDir = $PSScriptRoot
    }
    else {
        $scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
    }
    
    $JsonPath = Join-Path $scriptDir "steam-workshop-mods.json"
}

# Ensure the path is absolute
if (-not [System.IO.Path]::IsPathRooted($JsonPath)) {
    $JsonPath = Join-Path (Get-Location) $JsonPath
}

# MARK: Helper Functions

function Read-JsonConfig {
    param([string]$Path)
    
    if (-not (Test-Path $Path)) {
        Write-Error "JSON config file not found at: $Path"
        exit 1
    }
    
    try {
        $content = Get-Content -Path $Path -Raw | ConvertFrom-Json
        return $content
    }
    catch {
        Write-Error "Failed to parse JSON config: $_"
        exit 1
    }
}

function Write-JsonConfig {
    param(
        [string]$Path,
        [object]$Config
    )
    
    try {
        $json = $Config | ConvertTo-Json -Depth 10
        $json | Set-Content -Path $Path -Encoding UTF8
        Write-Host "Successfully updated $Path" -ForegroundColor Green
    }
    catch {
        Write-Error "Failed to write JSON config: $_"
        exit 1
    }
}

function Get-ModNameFromSubModule {
    param([string]$ModPath)
    
    $subModulePath = Join-Path $ModPath "SubModule.xml"
    
    if (-not (Test-Path $subModulePath)) {
        # Try common alternate locations
        $subModulePath = Join-Path $ModPath "bin\Win64_Shipping_Client\SubModule.xml"
        if (-not (Test-Path $subModulePath)) {
            return $null
        }
    }
    
    try {
        [xml]$xml = Get-Content -Path $subModulePath
        
        # Try to get Id attribute value first
        $modId = $xml.Module.Id.value
        
        # If that's empty, try Name attribute value
        if ([string]::IsNullOrWhiteSpace($modId)) {
            $modId = $xml.Module.Name.value
        }
        
        # Fallback to InnerText if no value attribute
        if ([string]::IsNullOrWhiteSpace($modId)) {
            $modId = $xml.Module.Id
            if ($modId -is [System.Xml.XmlElement]) {
                $modId = $modId.InnerText
            }
        }
        
        return $modId
    }
    catch {
        Write-Warning "Failed to parse SubModule.xml for mod at: $ModPath"
        return $null
    }
}

function Get-WorkshopMods {
    param([string]$WorkshopPath)
    
    if (-not (Test-Path $WorkshopPath)) {
        Write-Error "Steam Workshop path not found: $WorkshopPath"
        Write-Error "Please verify the 'steamWorkshopPath' in your JSON config is correct."
        exit 1
    }
    
    Write-Host "Scanning workshop directory: $WorkshopPath" -ForegroundColor Cyan
    
    $mods = @()
    $folders = Get-ChildItem -Path $WorkshopPath -Directory
    
    foreach ($folder in $folders) {
        $workshopId = $folder.Name
        
        # Verify it's a numeric workshop ID
        if ($workshopId -notmatch '^\d+$') {
            continue
        }
        
        Write-Host "  Processing workshop ID: $workshopId" -ForegroundColor Gray
        
        $modName = Get-ModNameFromSubModule -ModPath $folder.FullName
        
        if ($null -eq $modName) {
            Write-Warning "    Could not find mod name for workshop ID: $workshopId - Skipping"
            continue
        }
        
        Write-Host "    Found mod: $modName" -ForegroundColor Gray
        
        $mods += @{
            workshopId = $workshopId
            modName = $modName
            enabled = $false
        }
    }
    
    return $mods
}

function Merge-ModLists {
    param(
        [array]$ExistingMods,
        [array]$NewMods,
        [bool]$EnableNew
    )
    
    $existingIds = @{}
    foreach ($mod in $ExistingMods) {
        $existingIds[$mod.workshopId] = $mod
    }
    
    $addedCount = 0
    
    foreach ($newMod in $NewMods) {
        if ($existingIds.ContainsKey($newMod.workshopId)) {
            # Update mod name if it changed
            $existing = $existingIds[$newMod.workshopId]
            if ($existing.modName -ne $newMod.modName) {
                Write-Host "  Updating mod name for $($newMod.workshopId): $($existing.modName) -> $($newMod.modName)" -ForegroundColor Yellow
                $existing.modName = $newMod.modName
            }
        }
        else {
            # Add new mod
            $newMod.enabled = $EnableNew
            $ExistingMods += $newMod
            $addedCount++
            Write-Host "  Added new mod: $($newMod.modName) ($($newMod.workshopId))" -ForegroundColor Green
        }
    }
    
    return @{
        Mods = $ExistingMods
        AddedCount = $addedCount
    }
}

# MARK: Main Script

Write-Host "`n========================================" -ForegroundColor Cyan
Write-Host "Steam Workshop Mod Scanner for BLGM" -ForegroundColor Cyan
Write-Host "========================================`n" -ForegroundColor Cyan

# Debug output
Write-Host "Script Directory: $PSScriptRoot" -ForegroundColor Gray
Write-Host "Current Location: $(Get-Location)" -ForegroundColor Gray
Write-Host "Resolved JSON Path: $JsonPath" -ForegroundColor Gray
Write-Host ""

# Read existing config
Write-Host "Reading configuration from: $JsonPath" -ForegroundColor Cyan
$config = Read-JsonConfig -Path $JsonPath

# Verify workshop path exists
$workshopPath = $config.steamWorkshopPath
if ([string]::IsNullOrWhiteSpace($workshopPath)) {
    Write-Error "No 'steamWorkshopPath' found in config file"
    exit 1
}

# Scan workshop directory
$discoveredMods = Get-WorkshopMods -WorkshopPath $workshopPath

if ($discoveredMods.Count -eq 0) {
    Write-Warning "No mods found in workshop directory"
    exit 0
}

Write-Host "`nDiscovered $($discoveredMods.Count) mod(s) in workshop directory" -ForegroundColor Cyan

# Merge with existing mods
Write-Host "`nMerging with existing configuration..." -ForegroundColor Cyan
$result = Merge-ModLists -ExistingMods $config.mods -NewMods $discoveredMods -EnableNew $EnableNewMods.IsPresent

$config.mods = $result.Mods

# Write updated config
Write-Host "`nWriting updated configuration..." -ForegroundColor Cyan
Write-JsonConfig -Path $JsonPath -Config $config

Write-Host "`n========================================" -ForegroundColor Cyan
Write-Host "Summary:" -ForegroundColor Cyan
Write-Host "  Total mods in config: $($result.Mods.Count)" -ForegroundColor White
Write-Host "  New mods added: $($result.AddedCount)" -ForegroundColor White
Write-Host "========================================`n" -ForegroundColor Cyan

if ($result.AddedCount -gt 0) {
    Write-Host "Note: New mods were added with enabled=$(if($EnableNewMods.IsPresent){'true'}else{'false'})" -ForegroundColor Yellow
    Write-Host "Edit the JSON file to change the 'enabled' property as needed.`n" -ForegroundColor Yellow
}
