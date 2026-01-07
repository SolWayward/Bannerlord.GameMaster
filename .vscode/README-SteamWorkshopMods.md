# Steam Workshop Mods Auto-Scanner

This directory contains a PowerShell script that automatically scans your Steam Workshop directory and adds mod entries to [`steam-workshop-mods.json`](.vscode/steam-workshop-mods.json).

## Files

- [`steam-workshop-mods.json`](.vscode/steam-workshop-mods.json) - Configuration file containing Steam Workshop mods for debugging
- [`Add-SteamWorkshopMods.ps1`](.vscode/Add-SteamWorkshopMods.ps1) - PowerShell script to scan and add mods automatically
- [`Add-SteamWorkshopMods.bat`](.vscode/Add-SteamWorkshopMods.bat) - Batch file wrapper for easy execution

## Quick Start

### Option 1: Using Batch File (Easiest)
Simply double-click [`Add-SteamWorkshopMods.bat`](.vscode/Add-SteamWorkshopMods.bat) or run from command prompt:
```batch
Add-SteamWorkshopMods.bat
```

### Option 2: Using PowerShell Directly
```powershell
.\Add-SteamWorkshopMods.ps1
```

## What It Does

1. Reads the current [`steam-workshop-mods.json`](.vscode/steam-workshop-mods.json) configuration
2. Scans the Steam Workshop directory specified in the JSON file
3. For each mod folder found:
   - Reads the SubModule.xml to get the proper mod name (Module Id)
   - Creates a new entry in the JSON file
4. Preserves existing entries (does not duplicate or remove)
5. Updates mod names if they have changed
6. Saves the updated configuration back to the JSON file

## Features

- **No Duplicates**: Existing workshop IDs are not added again
- **Preserves Settings**: Keeps your existing enabled/disabled settings
- **Auto-Updates Names**: If a mod's name changes in SubModule.xml, it updates the JSON
- **Safe Operation**: Creates a backup of your configuration before making changes

## Script Parameters

### EnableNewMods
By default, newly discovered mods are added with `enabled: false`. To add them as enabled:

```powershell
.\Add-SteamWorkshopMods.ps1 -EnableNewMods
```

### JsonPath
Specify a different JSON file path:

```powershell
.\Add-SteamWorkshopMods.ps1 -JsonPath "C:\path\to\custom.json"
```

## Configuration

The script reads the Steam Workshop path from [`steam-workshop-mods.json`](.vscode/steam-workshop-mods.json). Make sure the `steamWorkshopPath` is correctly set:

```json
{
  "steamWorkshopPath": "C:\\Program Files (x86)\\Steam\\steamapps\\workshop\\content\\261550",
  "mods": [ ... ]
}
```

The workshop ID `261550` is for Mount & Blade II: Bannerlord.

## Example Output

```
========================================
Steam Workshop Mod Scanner for BLGM
========================================

Reading configuration from: .\steam-workshop-mods.json
Scanning workshop directory: C:\Program Files (x86)\Steam\steamapps\workshop\content\261550
  Processing workshop ID: 2859233432
    Found mod: Harmony
  Processing workshop ID: 2912840792
    Found mod: ButterLib

Discovered 2 mod(s) in workshop directory

Merging with existing configuration...
  Added new mod: ButterLib (2912840792)

Writing updated configuration...
Successfully updated .\steam-workshop-mods.json

========================================
Summary:
  Total mods in config: 2
  New mods added: 1
========================================

Note: New mods were added with enabled=false
Edit the JSON file to change the 'enabled' property as needed.
```

## Troubleshooting

### "Steam Workshop path not found"
- Verify the `steamWorkshopPath` in your JSON file is correct
- Make sure Steam is installed and you have subscribed to at least one Bannerlord mod

### "Could not find mod name for workshop ID"
- The mod's SubModule.xml file may be missing or corrupted
- The mod may not be fully downloaded by Steam
- Try verifying the mod files in Steam Workshop

### Script Won't Run
If you get a PowerShell execution policy error:
```powershell
Set-ExecutionPolicy -ExecutionPolicy RemoteSigned -Scope CurrentUser
```

## Manual Editing

You can still manually edit [`steam-workshop-mods.json`](.vscode/steam-workshop-mods.json):

```json
{
  "workshopId": "2859233432",
  "modName": "Harmony",
  "enabled": true
}
```

- `workshopId`: The folder name in your Steam Workshop directory
- `modName`: Should match the Module's SubModule.xml `<Id>` value
- `enabled`: Set to `true` to include during debugging, `false` to ignore

## Notes

- The script preserves the `instructions` array in the JSON file
- Mods are not removed automatically - remove them manually if needed
- The script is idempotent - running it multiple times is safe
