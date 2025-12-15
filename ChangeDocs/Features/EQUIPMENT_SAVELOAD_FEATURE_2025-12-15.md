# Equipment Save/Load System

**Date:** 2025-12-15  
**Type:** New Feature  
**Impact:** Item Management Commands

## Overview

Added the ability to save and load hero equipment sets to/from JSON files, enabling players to create, store, and reuse equipment configurations. This feature supports both battle and civilian equipment sets, with automatic directory creation, modifier preservation, and graceful error handling.

Equipment sets are saved as human-readable JSON files in the user's documents folder, making them easy to share, backup, and manage outside the game.

## New Commands

### 1. Save Battle Equipment

**Command:** `gm.item.save_equipment <hero_query> <filename>`

**Purpose:** Saves a hero's battle/main equipment set to a JSON file.

**Usage:**
```
gm.item.save_equipment player my_loadout
gm.item.save_equipment lord_1_1 warrior_setup
```

**Output Example:**
```
Saved Battanian's battle equipment to: my_loadout.json
Items saved (6):
  Head            Northern Fur Hood
  Body            Highland Scale Armor (Fine)
  Leg             Highland Boots
  Weapon0         Highland Axe (Masterwork)
  Weapon1         Highland Round Shield
  Horse           Battanian Pony
```

### 2. Save Civilian Equipment

**Command:** `gm.item.save_equipment_civilian <hero_query> <filename>`

**Purpose:** Saves a hero's civilian equipment set to a JSON file.

**Usage:**
```
gm.item.save_equipment_civilian player my_civilian
gm.item.save_equipment_civilian lord_1_1 noble_attire
```

**Output Example:**
```
Saved Battanian's civilian equipment to: my_civilian.json
Items saved (3):
  Body            Fur-Lined Leather Jacket
  Leg             Leather Boots
  Cape            Fine Cloak (Fine)
```

### 3. Save Both Equipment Sets

**Command:** `gm.item.save_equipment_both <hero_query> <filename>`

**Purpose:** Saves both battle and civilian equipment sets in one operation.

**Usage:**
```
gm.item.save_equipment_both player complete_loadout
gm.item.save_equipment_both lord_1_1 full_setup
```

**Output Example:**
```
Saved Battanian's equipment sets:

Battle equipment -> complete_loadout.json (6 items):
  Head            Northern Fur Hood
  Body            Highland Scale Armor
  Leg             Highland Boots
  Weapon0         Highland Axe
  Weapon1         Highland Round Shield
  Horse           Battanian Pony

Civilian equipment -> complete_loadout.json (3 items):
  Body            Fur-Lined Leather Jacket
  Leg             Leather Boots
  Cape            Fine Cloak
```

### 4. Load Battle Equipment

**Command:** `gm.item.load_equipment <hero_query> <filename>`

**Purpose:** Loads a hero's battle equipment set from a saved JSON file.

**Usage:**
```
gm.item.load_equipment player my_loadout
gm.item.load_equipment lord_1_1 warrior_setup
```

**Output Example:**
```
Loaded Battanian's battle equipment from: my_loadout.json
Items loaded: 6
```

**With Missing Items:**
```
Loaded Battanian's battle equipment from: my_loadout.json
Items loaded: 4
Items skipped (not found in game): 2
  Weapon2         mod_custom_sword (modifier: legendary)
  Cape            dlc_cape_item
```

### 5. Load Civilian Equipment

**Command:** `gm.item.load_equipment_civilian <hero_query> <filename>`

**Purpose:** Loads a hero's civilian equipment set from a saved JSON file.

**Usage:**
```
gm.item.load_equipment_civilian player my_civilian
gm.item.load_equipment_civilian lord_1_1 noble_attire
```

**Output Example:**
```
Loaded Battanian's civilian equipment from: my_civilian.json
Items loaded: 3
```

### 6. Load Both Equipment Sets

**Command:** `gm.item.load_equipment_both <hero_query> <filename>`

**Purpose:** Loads both battle and civilian equipment sets from saved JSON files.

**Usage:**
```
gm.item.load_equipment_both player complete_loadout
gm.item.load_equipment_both lord_1_1 full_setup
```

**Output Example:**
```
Loading equipment sets for Battanian:

Battle equipment loaded from: complete_loadout.json
  Items loaded: 6

Civilian equipment loaded from: complete_loadout.json
  Items loaded: 3
```

**Graceful Handling of Missing Files:**
```
Loading equipment sets for Battanian:

Battle equipment loaded from: complete_loadout.json
  Items loaded: 6

Civilian equipment file not found: complete_loadout.json
```

## File Structure

### File Paths

Equipment files are saved to the user's documents folder with the following structure:

**Battle Equipment:**
```
Documents\Mount and Blade II Bannerlord\Configs\GameMaster\HeroSets\{filename}.json
```

**Civilian Equipment:**
```
Documents\Mount and Blade II Bannerlord\Configs\GameMaster\HeroSets\civilian\{filename}.json
```

The system automatically creates these directories if they don't exist.

### JSON Format

Equipment files use a clean, human-readable JSON structure:

```json
{
  "HeroName": "Battanian",
  "HeroId": "lord_1_1",
  "SavedDate": "2025-12-15T19:30:00.0000000Z",
  "Equipment": [
    {
      "Slot": "Head",
      "ItemId": "northern_fur_hood",
      "ModifierId": null
    },
    {
      "Slot": "Body",
      "ItemId": "highland_scale_armor",
      "ModifierId": "ironarm_fine"
    },
    {
      "Slot": "Weapon0",
      "ItemId": "highland_axe",
      "ModifierId": "ironarm_masterwork"
    }
  ]
}
```

**JSON Properties:**
- `HeroName`: Display name of the hero when saved (for reference)
- `HeroId`: String ID of the hero (for reference)
- `SavedDate`: ISO 8601 UTC timestamp
- `Equipment`: Array of equipped items
  - `Slot`: Equipment slot name (Head, Body, Leg, Gloves, Cape, Horse, HorseHarness, Weapon0-3)
  - `ItemId`: String ID of the item
  - `ModifierId`: String ID of the quality modifier (null if none)

## Key Features

### Automatic Directory Creation
- Creates necessary directory structure automatically
- No manual setup required
- Cross-platform path handling using `Environment.SpecialFolder.MyDocuments`

### Modifier Preservation
- Saves and restores quality modifiers (Fine, Masterwork, Legendary, etc.)
- Maintains exact equipment state including all bonuses
- Gracefully handles missing modifiers during load

### Graceful Error Handling
- Missing files are reported clearly
- Non-existent items are skipped with detailed feedback
- `load_equipment_both` continues even if one file is missing
- Invalid JSON format provides clear error messages

### Item and Modifier Validation
- Validates items exist in current game during load
- Validates modifiers exist in current game
- Skips missing items/modifiers and reports them
- Loads items without modifiers if modifier not found

### File Management
- `.json` extension added automatically if not provided
- Files are organized by type (battle vs civilian)
- Human-readable format for easy editing/sharing
- Indented JSON for better readability

## Implementation Details

### Files Modified

**Primary Implementation:**
- [`ItemManagementCommands.cs`](../../Bannerlord.GameMaster/Console/ItemManagementCommands.cs:863-1436) - Lines 863-1436
  - Added 6 new command methods (lines 869-1216)
  - Added 4 helper classes for serialization (lines 1225-1273)
  - Added 4 helper methods for file operations (lines 1278-1434)

### Key Helper Methods

**`GetEquipmentFilePath(string filename, bool isCivilian)`** (lines 1278-1301)
- Constructs full file path with proper directory structure
- Creates directories if they don't exist
- Adds `.json` extension automatically
- Handles civilian subdirectory routing

**`SaveEquipmentToFile(Hero hero, Equipment equipment, string filepath, bool isCivilian)`** (lines 1306-1338)
- Serializes equipment to JSON format
- Includes hero metadata and timestamp
- Only saves non-empty equipment slots
- Uses pretty-printed JSON with indentation

**`LoadEquipmentFromFile(Hero hero, string filepath, bool isCivilian)`** (lines 1343-1407)
- Deserializes JSON to equipment objects
- Validates items and modifiers exist in game
- Clears existing equipment before loading
- Returns counts of loaded and skipped items
- Returns detailed list of skipped items for reporting

**`GetEquipmentList(Equipment equipment)`** (lines 1412-1434)
- Extracts equipment information for display
- Used for save command output
- Shows slot, item name, and modifier

### Data Classes

**`EquipmentSetData`** (lines 1225-1238)
- Top-level serialization container
- Contains hero metadata and equipment array

**`EquipmentSlotData`** (lines 1243-1253)
- Individual slot serialization
- Stores slot name, item ID, and modifier ID

**`EquipmentItemInfo`** (lines 1258-1263)
- Helper class for display output
- Used in save command feedback

**`SkippedItemInfo`** (lines 1268-1273)
- Tracks items that couldn't be loaded
- Used in load command error reporting

### JSON Serialization

Uses **Newtonsoft.Json** library:
- `JsonConvert.SerializeObject()` with `Formatting.Indented`
- `JsonConvert.DeserializeObject<EquipmentSetData>()`
- `[JsonProperty]` attributes for clean property names

## Testing

### Test Coverage

Added **21 comprehensive validation tests** in [`StandardTests.cs`](../../Bannerlord.GameMaster/Console/Testing/StandardTests.cs:1084-1370):

**Save Command Tests (9 tests):** Category `ItemEquipmentSave`
- `equipment_save_001-003`: Test `save_equipment` validation
- `equipment_save_004-006`: Test `save_equipment_civilian` validation
- `equipment_save_007-009`: Test `save_equipment_both` validation

**Load Command Tests (12 tests):** Category `ItemEquipmentLoad`
- `equipment_load_001-004`: Test `load_equipment` validation
- `equipment_load_005-008`: Test `load_equipment_civilian` validation
- `equipment_load_009-012`: Test `load_equipment_both` validation

### Test Scenarios

**Error Validation Tests:**
- Missing arguments (hero and filename)
- Invalid hero queries
- Non-existent files
- Campaign mode requirement

### Running Tests

Run all equipment save/load tests:
```
gm.test.run ItemEquipmentSave
gm.test.run ItemEquipmentLoad
```

Run all item management tests:
```
gm.test.run item
```

### Manual Testing Recommendations

1. **Basic Save/Load:**
   ```
   gm.item.equip imperial_sword player
   gm.item.equip lamellar_armor player
   gm.item.save_equipment player test_loadout
   gm.item.unequip_all player
   gm.item.load_equipment player test_loadout
   gm.item.list_equipped player
   ```

2. **With Modifiers:**
   ```
   gm.item.equip sword player
   gm.item.set_equipped_modifier player masterwork
   gm.item.save_equipment player legendary_gear
   gm.item.remove_equipped player
   gm.item.load_equipment player legendary_gear
   gm.item.list_equipped player
   ```

3. **Both Equipment Sets:**
   ```
   gm.item.equip sword player
   gm.item.equip robe player civilian
   gm.item.save_equipment_both player complete_set
   gm.item.remove_equipped player
   gm.item.load_equipment_both player complete_set
   gm.item.list_equipped player
   ```

4. **Transfer Between Heroes:**
   ```
   gm.item.save_equipment player my_equipment
   gm.item.load_equipment lord_1_1 my_equipment
   ```

## Use Cases

### 1. Equipment Templates
Create and reuse optimal equipment configurations:
```
gm.item.save_equipment player cavalry_loadout
gm.item.save_equipment player archer_loadout
gm.item.save_equipment player infantry_loadout
```

### 2. Transferring Equipment Between Heroes
Easily copy equipment from one hero to another:
```
gm.item.save_equipment lord_1_1 veteran_setup
gm.item.load_equipment player veteran_setup
```

### 3. Backup and Restore
Backup equipment before major changes:
```
gm.item.save_equipment_both player backup_pre_battle
# ... make changes or engage in battle ...
gm.item.load_equipment_both player backup_pre_battle
```

### 4. Creating Loadouts for Different Situations
Save specialized equipment sets:
```
gm.item.save_equipment player siege_attacker
gm.item.save_equipment player siege_defender
gm.item.save_equipment player open_field_cavalry
gm.item.save_equipment player tournament_fighter
```

### 5. Sharing Configurations
Share equipment setups with other players:
- Export the JSON files from your documents folder
- Share with others
- They can import by placing files in their HeroSets folder

### 6. Mod Compatibility Testing
Save equipment sets to test across different mod configurations:
```
gm.item.save_equipment_both player vanilla_setup
# ... add mods ...
gm.item.load_equipment_both player vanilla_setup
# Check which items are missing from mods
```

## Known Limitations

### 1. Campaign Mode Requirement
- All save/load commands require an active campaign
- Cannot be used in the main menu or custom battles
- Returns clear error message if not in campaign mode

### 2. Item Availability
- Items and modifiers must exist in the current game
- Saving equipment from one mod and loading in vanilla may skip modded items
- System reports all skipped items for transparency

### 3. Equipment Validation
- Skips missing items during load rather than failing completely
- If a modifier is missing, item loads without modifier
- This ensures partial loads succeed even with incompatible saves

### 4. File Naming
- Filenames should be valid for the file system
- Avoid special characters: `\ / : * ? " < > |`
- Use alphanumeric characters, underscores, and hyphens

### 5. No Automatic Inventory Management
- Loading equipment doesn't automatically remove items from inventory
- Saving equipment doesn't modify party inventory
- Use [`gm.item.unequip_all`](../../Bannerlord.GameMaster/Console/ItemManagementCommands.cs:230) to move items to inventory first

### 6. Single Hero Equipment Only
- Cannot batch-save equipment for multiple heroes
- Each save is specific to one hero's equipment
- Use multiple commands for multiple heroes

## Technical Notes

### Cross-Platform Compatibility
- Uses `Environment.SpecialFolder.MyDocuments` for path resolution
- Automatically handles Windows/Mac/Linux path separators
- `Path.Combine()` ensures proper path construction

### Error Recovery
- Invalid JSON returns descriptive error messages
- File not found returns specific filename in error
- Missing items are reported individually with slot and ID

### Performance
- File I/O is synchronous (commands wait for completion)
- JSON serialization is fast for equipment-sized data
- No caching—files are read fresh each load

### Future Enhancements
Potential improvements for future versions:
- Bulk save/load for multiple heroes
- Equipment set browser/manager UI
- Automatic backup before major battles
- Cloud sync support
- Equipment comparison tools

## Related Commands

These commands work well with the save/load system:

- [`gm.item.list_equipped`](../../Bannerlord.GameMaster/Console/ItemManagementCommands.cs:565) - View current equipment
- [`gm.item.unequip_all`](../../Bannerlord.GameMaster/Console/ItemManagementCommands.cs:230) - Clear equipment before loading
- [`gm.item.set_equipped_modifier`](../../Bannerlord.GameMaster/Console/ItemManagementCommands.cs:638) - Modify equipment before saving
- [`gm.item.equip`](../../Bannerlord.GameMaster/Console/ItemManagementCommands.cs:360) - Manually equip items
- [`gm.query.item`](../../Bannerlord.GameMaster/Console/Query/ItemQueryCommands.cs) - Find item IDs for manual JSON editing

## Breaking Changes

None. This is a new feature with no impact on existing commands or functionality.

## Compatibility

- Compatible with all existing item management commands
- Works with all quality modifiers
- Supports all equipment slot types
- Compatible with custom/modded items (with validation)