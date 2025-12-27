# Steam Workshop Mods for Debugging

This system allows you to debug Bannerlord with Steam workshop mods without permanently copying them to your game's Modules folder.

## How It Works

1. **Pre-Launch**: Before debugging starts, symbolic links are created from your Bannerlord Modules folder to Steam workshop mod folders
2. **Debug**: You can debug with workshop mods loaded
3. **Post-Debug**: After debugging ends, the symbolic links are automatically removed

This prevents workshop mods from being loaded when you launch the game normally through Steam.

## Setup

### 1. Configure Workshop Mods

Edit `.vscode/steam-workshop-mods.json`:

```json
{
  "steamWorkshopPath": "C:\\Program Files (x86)\\Steam\\steamapps\\workshop\\content\\261550",
  "mods": [
    {
      "workshopId": "2859233432",
      "modName": "MyAwesomeMod",
      "enabled": true
    }
  ]
}
```

**To find your workshop mod information:**

1. Navigate to `C:\Program Files (x86)\Steam\steamapps\workshop\content\261550`
2. Each folder is a workshop mod (the folder name is the workshopId)
3. Inside each folder, open `SubModule.xml` to find the mod's `<Id>` tag (this is the modName)

### 2. Enable Mods for Debugging

Set `"enabled": true` for each mod you want to use during debugging.

### 3. Run as Administrator (First Time)

Creating symbolic links requires administrator privileges:
- Close VS Code
- Right-click VS Code icon
- Select "Run as administrator"
- This is only needed the first time or when adding new mods

### 4. Start Debugging

Press F5 or use the debug panel. The system will:
1. Build your mod
2. Create symbolic links for enabled workshop mods
3. Launch Bannerlord with all mods loaded
4. Clean up symbolic links when debugging ends

## Important Notes

### Administrator Privileges
- Symbolic link creation requires admin rights
- If you see link creation failures, run VS Code as administrator
- This is a Windows security requirement, not a script limitation

### Module List in launch.json
- You still need to add workshop mod names to the `_MODULES_` list in launch.json
- Use the `gm.info.list_mods_launch` command in-game to generate the correct format
- Example: `_MODULES_*Native*SandBoxCore*MyAwesomeMod*Bannerlord.GameMaster*_MODULES_`

### Existing Mod Folders
- The script will NOT overwrite existing non-link folders
- If a mod is already installed in Modules folder, it will be skipped
- This prevents accidentally replacing manually installed mods

### Cleanup
- Symbolic links are automatically removed after debugging
- If VS Code crashes, run the unlink task manually: `Terminal > Run Task > unlink-steam-mods`

## Manual Link Management

You can also run the tasks manually:

- **Create Links**: Terminal > Run Task > `link-steam-mods`
- **Remove Links**: Terminal > Run Task > `unlink-steam-mods`

## Troubleshooting

### Links not being created
- **Solution**: Run VS Code as administrator
- **Why**: Windows requires admin rights for symbolic links

### Mod not loading in game
- **Check 1**: Verify the modName in config matches the SubModule.xml `<Id>` exactly
- **Check 2**: Ensure the mod is added to launch.json `_MODULES_` argument
- **Check 3**: Verify the workshop folder exists and contains mod files

### Links not removed after debugging
- **Solution**: Run the unlink task manually
- **Command**: Terminal > Run Task > `unlink-steam-mods`

### Workshop path not found
- **Check**: Verify your Steam is installed in the default location
- **Custom Path**: Update `steamWorkshopPath` in the config file

## Example Configuration

```json
{
  "steamWorkshopPath": "C:\\Program Files (x86)\\Steam\\steamapps\\workshop\\content\\261550",
  "mods": [
    {
      "workshopId": "2859233432",
      "modName": "ButterLib",
      "enabled": true
    },
    {
      "workshopId": "2859170932",
      "modName": "HarmonyExtensions",
      "enabled": true
    },
    {
      "workshopId": "2849127462",
      "modName": "UIExtenderEx",
      "enabled": false
    }
  ]
}
```

Then in launch.json:
```json
"args": [
    "/singleplayer",
    "/continuegame",
    "_MODULES_*Native*SandBoxCore*CustomBattle*Sandbox*StoryMode*ButterLib*HarmonyExtensions*Bannerlord.GameMaster*_MODULES_"
]
```

## Files Created

- `.vscode/steam-workshop-mods.json` - Configuration file (edit this)
- `.vscode/link-steam-mods.ps1` - Script to create links
- `.vscode/unlink-steam-mods.ps1` - Script to remove links
- `.vscode/steam-mod-links.txt` - Tracking file (auto-generated, don't edit)
