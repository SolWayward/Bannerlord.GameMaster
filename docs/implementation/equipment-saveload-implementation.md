# Equipment Save/Load System - Implementation Guide

**Navigation:** [← Back: Item Management Example](item-management-example.md) | [Back to Index](../README.md) | [Next: Code Quality Checklist →](code-quality-checklist.md)

---

## Overview

The Equipment Save/Load system enables saving and loading hero equipment configurations to/from JSON files. This document provides a comprehensive implementation guide covering architecture, workflows, error handling, and extension points.

**Key Features:**
- ✅ Save/load battle and civilian equipment separately or together
- ✅ Preserve item quality modifiers (Fine, Masterwork, Legendary, etc.)
- ✅ Cross-platform file storage using user's documents folder
- ✅ Graceful error handling for missing items/modifiers
- ✅ Human-readable JSON format for easy sharing and editing

**Implementation Files:**
- [`ItemManagementCommands.cs`](../../Bannerlord.GameMaster/Console/ItemManagementCommands.cs:863-1436) - Main implementation (lines 863-1436)
- [`StandardTests.cs`](../../Bannerlord.GameMaster/Console/Testing/StandardTests.cs:1084-1370) - Test coverage (21 tests)

**Related Documentation:**
- [Feature Documentation](../../ChangeDocs/Features/EQUIPMENT_SAVELOAD_FEATURE_2025-12-15.md) - Complete feature details
- [File I/O Best Practices](../guides/best-practices.md#file-io-operations) - File handling guidelines

---

## Architecture Overview

### Component Structure

```
ItemManagementCommands.cs
├── Commands (6 public methods)
│   ├── SaveEquipment()           - Save battle equipment
│   ├── SaveEquipmentCivilian()   - Save civilian equipment
│   ├── SaveEquipmentBoth()       - Save both equipment sets
│   ├── LoadEquipment()           - Load battle equipment
│   ├── LoadEquipmentCivilian()   - Load civilian equipment
│   └── LoadEquipmentBoth()       - Load both equipment sets
│
├── Helper Methods (4 private methods)
│   ├── GetEquipmentFilePath()    - Construct file paths
│   ├── SaveEquipmentToFile()     - Serialize to JSON
│   ├── LoadEquipmentFromFile()   - Deserialize from JSON
│   └── GetEquipmentList()        - Extract display info
│
└── Data Classes (4 private classes)
    ├── EquipmentSetData          - Top-level container
    ├── EquipmentSlotData         - Individual slot data
    ├── EquipmentItemInfo         - Display helper
    └── SkippedItemInfo           - Error reporting helper
```

### Data Flow

**Save Operation:**
```
User Command
    ↓
Command Method (SaveEquipment)
    ↓
GetEquipmentFilePath() → Create directory structure
    ↓
SaveEquipmentToFile() → Serialize Equipment to EquipmentSetData
    ↓
JsonConvert.SerializeObject() → Pretty-print JSON
    ↓
File.WriteAllText() → Write to disk
    ↓
GetEquipmentList() → Format output for user
    ↓
Return success message
```

**Load Operation:**
```
User Command
    ↓
Command Method (LoadEquipment)
    ↓
GetEquipmentFilePath() → Get file path
    ↓
File.Exists() → Validate file exists
    ↓
LoadEquipmentFromFile()
    ↓
File.ReadAllText() → Read JSON
    ↓
JsonConvert.DeserializeObject() → Parse to EquipmentSetData
    ↓
Clear existing equipment
    ↓
For each slot:
    - Validate item exists
    - Validate modifier exists (optional)
    - Apply equipment or skip if invalid
    ↓
Return (loadedCount, skippedCount, skippedItems)
    ↓
Format and return result message
```

---

## Data Model Classes

### EquipmentSetData

**Purpose:** Top-level serialization container for equipment sets.

**Location:** [`ItemManagementCommands.cs:1225`](../../Bannerlord.GameMaster/Console/ItemManagementCommands.cs:1225)

```csharp
private class EquipmentSetData
{
    [JsonProperty("HeroName")]
    public string HeroName { get; set; }
    
    [JsonProperty("HeroId")]
    public string HeroId { get; set; }
    
    [JsonProperty("SavedDate")]
    public string SavedDate { get; set; }
    
    [JsonProperty("Equipment")]
    public List<EquipmentSlotData> Equipment { get; set; }
}
```

**Properties:**
- `HeroName` - Display name for reference (not used in loading)
- `HeroId` - String ID for reference (not used in loading)
- `SavedDate` - ISO 8601 UTC timestamp
- `Equipment` - Array of equipment slots

**Design Decisions:**
- Hero metadata is for reference only; equipment can load to any hero
- ISO 8601 format for universal timestamp compatibility
- `[JsonProperty]` attributes ensure clean JSON property names

### EquipmentSlotData

**Purpose:** Individual equipment slot serialization.

**Location:** [`ItemManagementCommands.cs:1243`](../../Bannerlord.GameMaster/Console/ItemManagementCommands.cs:1243)

```csharp
private class EquipmentSlotData
{
    [JsonProperty("Slot")]
    public string Slot { get; set; }
    
    [JsonProperty("ItemId")]
    public string ItemId { get; set; }
    
    [JsonProperty("ModifierId")]
    public string ModifierId { get; set; }
}
```

**Properties:**
- `Slot` - EquipmentIndex enum as string (Head, Body, Weapon0, etc.)
- `ItemId` - Item's StringId for game lookup
- `ModifierId` - ItemModifier's StringId (null if no modifier)

**Design Decisions:**
- Store IDs rather than names for reliable game object lookup
- ModifierId is nullable to support items without modifiers
- Slot stored as string for JSON readability

### EquipmentItemInfo

**Purpose:** Helper class for display output in save commands.

**Location:** [`ItemManagementCommands.cs:1258`](../../Bannerlord.GameMaster/Console/ItemManagementCommands.cs:1258)

```csharp
private class EquipmentItemInfo
{
    public string Slot { get; set; }
    public string ItemName { get; set; }
    public string ModifierText { get; set; }
}
```

**Usage:**
```csharp
var savedItems = GetEquipmentList(hero.BattleEquipment);
foreach (var item in savedItems)
{
    result.AppendLine($"  {item.Slot,-15} {item.ItemName}{item.ModifierText}");
}
```

### SkippedItemInfo

**Purpose:** Track items that couldn't be loaded for error reporting.

**Location:** [`ItemManagementCommands.cs:1268`](../../Bannerlord.GameMaster/Console/ItemManagementCommands.cs:1268)

```csharp
private class SkippedItemInfo
{
    public string Slot { get; set; }
    public string ItemId { get; set; }
    public string ModifierInfo { get; set; }
}
```

**Usage:**
```csharp
if (item == null)
{
    skippedItems.Add(new SkippedItemInfo
    {
        Slot = slot.ToString(),
        ItemId = slotData.ItemId,
        ModifierInfo = modifierInfo
    });
    continue;
}
```

---

## Save Workflow

### Implementation: SaveEquipmentToFile()

**Location:** [`ItemManagementCommands.cs:1306`](../../Bannerlord.GameMaster/Console/ItemManagementCommands.cs:1306)

**Parameters:**
- `Hero hero` - Hero whose equipment to save
- `Equipment equipment` - BattleEquipment or CivilianEquipment
- `string filepath` - Full path to JSON file
- `bool isCivilian` - Whether this is civilian equipment (for logging)

**Process:**

1. **Create Container:**
```csharp
var equipmentData = new EquipmentSetData
{
    HeroName = hero.Name?.ToString() ?? "",
    HeroId = hero.StringId,
    SavedDate = DateTime.UtcNow.ToString("o"), // ISO 8601 format
    Equipment = new List<EquipmentSlotData>()
};
```

2. **Iterate Equipment Slots:**
```csharp
for (int i = 0; i < (int)EquipmentIndex.NumEquipmentSetSlots; i++)
{
    EquipmentIndex slot = (EquipmentIndex)i;
    var element = equipment[slot];
    
    if (!element.IsEmpty)
    {
        equipmentData.Equipment.Add(new EquipmentSlotData
        {
            Slot = slot.ToString(),
            ItemId = element.Item.StringId,
            ModifierId = element.ItemModifier?.StringId
        });
    }
}
```

3. **Serialize and Write:**
```csharp
string jsonString = JsonConvert.SerializeObject(equipmentData, Formatting.Indented);
File.WriteAllText(filepath, jsonString);
```

**Key Design Decisions:**
- Only save non-empty slots (reduces file size)
- Use ISO 8601 timestamp format for universal compatibility
- Pretty-print JSON with indentation for human readability
- Store modifier StringId for reliable lookup

---

## Load Workflow

### Implementation: LoadEquipmentFromFile()

**Location:** [`ItemManagementCommands.cs:1343`](../../Bannerlord.GameMaster/Console/ItemManagementCommands.cs:1343)

**Parameters:**
- `Hero hero` - Hero to apply equipment to
- `string filepath` - Full path to JSON file
- `bool isCivilian` - Whether to load to civilian equipment

**Returns:**
- `(int loadedCount, int skippedCount, List<SkippedItemInfo> skippedItems)`

**Process:**

1. **Read and Deserialize:**
```csharp
string jsonString = File.ReadAllText(filepath);
var equipmentData = JsonConvert.DeserializeObject<EquipmentSetData>(jsonString);

if (equipmentData == null || equipmentData.Equipment == null)
{
    throw new Exception("Invalid equipment file format.");
}
```

2. **Clear Existing Equipment:**
```csharp
Equipment equipment = isCivilian ? hero.CivilianEquipment : hero.BattleEquipment;

for (int i = 0; i < (int)EquipmentIndex.NumEquipmentSetSlots; i++)
{
    equipment[(EquipmentIndex)i] = EquipmentElement.Invalid;
}
```

3. **Load Each Slot with Validation:**
```csharp
foreach (var slotData in equipmentData.Equipment)
{
    // Parse slot
    if (!Enum.TryParse<EquipmentIndex>(slotData.Slot, out EquipmentIndex slot))
        continue;

    // Find item
    ItemObject item = ItemQueries.QueryItems(slotData.ItemId)
        .FirstOrDefault(i => i.StringId == slotData.ItemId);
    
    if (item == null)
    {
        // Track skipped item
        skippedCount++;
        skippedItems.Add(new SkippedItemInfo { /* ... */ });
        continue;
    }

    // Try to find modifier (optional)
    ItemModifier modifier = null;
    if (!string.IsNullOrEmpty(slotData.ModifierId))
    {
        var modifierResult = ItemModifierHelper.ParseModifier(slotData.ModifierId);
        modifier = modifierResult.Item1; // null if not found
    }

    // Apply equipment
    equipment[slot] = new EquipmentElement(item, modifier);
    loadedCount++;
}
```

**Key Design Decisions:**
- Clear equipment before loading for clean state
- Validate items exist in current game before applying
- Continue loading even if some items are missing (graceful degradation)
- Load items without modifiers if modifier not found
- Track and report all skipped items for transparency

---

## Error Handling Strategy

### File Not Found

**Strategy:** Return clear error message with filename.

```csharp
if (!File.Exists(filepath))
    return CommandBase.FormatErrorMessage($"Equipment file not found: {Path.GetFileName(filepath)}");
```

### Invalid JSON Format

**Strategy:** Let `ExecuteWithErrorHandling()` catch and report exception.

```csharp
return CommandBase.ExecuteWithErrorHandling(() =>
{
    // JSON operations that may throw JsonException
}, "Failed to load equipment");
```

### Missing Items

**Strategy:** Skip invalid items, track them, and report in output.

```csharp
if (item == null)
{
    skippedCount++;
    skippedItems.Add(new SkippedItemInfo
    {
        Slot = slot.ToString(),
        ItemId = slotData.ItemId,
        ModifierInfo = modifierInfo
    });
    continue; // Don't fail, just skip
}
```

**Output:**
```
Items loaded: 4
Items skipped (not found in game): 2
  Weapon2         mod_custom_sword (modifier: legendary)
  Cape            dlc_cape_item
```

### Missing Modifiers

**Strategy:** Load item without modifier rather than failing.

```csharp
ItemModifier modifier = null;
if (!string.IsNullOrEmpty(slotData.ModifierId))
{
    var modifierResult = ItemModifierHelper.ParseModifier(slotData.ModifierId);
    if (modifierResult.Item1 != null)
        modifier = modifierResult.Item1;
    // If modifier not found, modifier stays null - item still loads
}
```

### Partial Load Success

**Strategy:** For `load_equipment_both`, continue if one file is missing.

```csharp
// Try battle equipment
if (File.Exists(battlePath))
{
    // Load battle equipment
    battleLoaded = true;
}
else
{
    result.AppendLine($"\nBattle equipment file not found: {Path.GetFileName(battlePath)}");
}

// Try civilian equipment (even if battle failed)
if (File.Exists(civilianPath))
{
    // Load civilian equipment
    civilianLoaded = true;
}

// Only error if BOTH files are missing
if (!battleLoaded && !civilianLoaded)
{
    return CommandBase.FormatErrorMessage("Neither battle nor civilian equipment files were found.");
}
```

---

## Testing Approach

### Test Coverage

**Location:** [`StandardTests.cs:1084-1370`](../../Bannerlord.GameMaster/Console/Testing/StandardTests.cs:1084)

**Categories:**
- `ItemEquipmentSave` - 9 tests for save commands
- `ItemEquipmentLoad` - 12 tests for load commands

### Test Structure

```csharp
TestRunner.RegisterTest(new TestCase(
    "equipment_save_001",
    "Save equipment without args should error",
    "gm.item.save_equipment",
    TestExpectation.Error
)
{
    Category = "ItemEquipmentSave",
    ExpectedText = "Missing arguments"
});
```

### Test Scenarios Covered

**Save Commands:**
1. Missing arguments (hero and filename)
2. Invalid hero query
3. Campaign mode requirement

**Load Commands:**
1. Missing arguments (hero and filename)
2. Invalid hero query
3. Non-existent file
4. Campaign mode requirement

### Manual Testing Recommendations

**Basic Save/Load Flow:**
```bash
# Setup: Equip items
gm.item.equip imperial_sword player
gm.item.equip lamellar_armor player
gm.item.save_equipment player test_loadout

# Test: Clear and reload
gm.item.unequip_all player
gm.item.load_equipment player test_loadout
gm.item.list_equipped player
```

**Modifier Preservation:**
```bash
# Setup: Equip with modifiers
gm.item.equip sword player
gm.item.set_equipped_modifier player masterwork
gm.item.save_equipment player legendary_gear

# Test: Verify modifier preserved
gm.item.remove_equipped player
gm.item.load_equipment player legendary_gear
gm.item.list_equipped player  # Should show (Masterwork)
```

**Missing Item Handling:**
```bash
# Setup: Save equipment from modded game
# (with mod installed) gm.item.save_equipment player mod_loadout

# Test: Load in vanilla (without mod)
# (vanilla game) gm.item.load_equipment player mod_loadout
# Should report skipped modded items
```

---

## Extension Points

### Adding New Equipment Types

If the game adds new equipment slots:

1. **No code changes needed** - System uses `EquipmentIndex.NumEquipmentSetSlots`
2. **JSON automatically includes new slots** when they're equipped
3. **Loading validates slot enum** and skips unknown slots gracefully

### Custom Validation

To add custom item validation before loading:

```csharp
// In LoadEquipmentFromFile(), after finding item:
if (!CustomValidateItem(item, hero))
{
    skippedItems.Add(new SkippedItemInfo { /* ... */ });
    continue;
}

// Add validation method:
private static bool CustomValidateItem(ItemObject item, Hero hero)
{
    // Example: Check if hero can use item's culture
    // Example: Validate item tier restrictions
    return true;
}
```

### Additional Metadata

To store additional information in saves:

1. Add property to `EquipmentSetData`:
```csharp
[JsonProperty("HeroLevel")]
public int HeroLevel { get; set; }
```

2. Set during save:
```csharp
equipmentData.HeroLevel = hero.Level;
```

3. Use during load (optional):
```csharp
if (equipmentData.HeroLevel > hero.Level)
{
    // Warn or restrict loading
}
```

### File Format Versioning

To support format changes over time:

1. Add version to `EquipmentSetData`:
```csharp
[JsonProperty("Version")]
public int Version { get; set; } = 1;
```

2. Handle version in load:
```csharp
if (equipmentData.Version == 1)
{
    // Load version 1 format
}
else if (equipmentData.Version == 2)
{
    // Load version 2 format with new features
}
```

### Batch Operations

To save/load multiple heroes at once:

```csharp
[CommandLineFunctionality.CommandLineArgumentFunction("save_party_equipment", "gm.item")]
public static string SavePartyEquipment(List<string> args)
{
    // Iterate party members
    // Save each to {heroId}_equipment.json
    // Return summary of all saves
}
```

---

## Performance Considerations

### File I/O

- **Synchronous operations** - Commands wait for completion
- **File size** - Typical equipment file: 1-3 KB
- **Load time** - Negligible for small JSON files

### Memory Usage

- **Transient allocations** - Data classes garbage collected after operation
- **Equipment objects** - Reuse existing game objects (items, modifiers)

### Optimization Opportunities

**Not currently needed, but for future reference:**

1. **Caching** - Could cache parsed JSON for repeated loads
2. **Async I/O** - Could use async file operations
3. **Compression** - Could compress JSON for storage
4. **Batch operations** - Could combine multiple saves into one file

---

## Related Documentation

- [Item Management Example](item-management-example.md#equipment-saveload-system) - Quick overview section
- [File I/O Best Practices](../guides/best-practices.md#file-io-operations) - File handling guidelines
- [Feature Documentation](../../ChangeDocs/Features/EQUIPMENT_SAVELOAD_FEATURE_2025-12-15.md) - Complete feature details
- [Testing Guide](../guides/testing.md) - Test procedures

---

## Next Steps

1. **Review** [Code Quality Checklist](code-quality-checklist.md) before implementing similar features
2. **Study** [Best Practices](../guides/best-practices.md) for consistent patterns
3. **Test** thoroughly using [Testing Guide](../guides/testing.md)

---

**Navigation:** [← Back: Item Management Example](item-management-example.md) | [Back to Index](../README.md) | [Next: Code Quality Checklist →](code-quality-checklist.md)