# Troop Query System Architecture

**Version:** 1.0  
**Date:** 2025-12-16  
**Author:** Architecture Mode  
**Status:** Design Complete - Ready for Implementation

## Table of Contents
1. [Overview](#overview)
2. [TroopTypes Enum Specification](#trooptypes-enum-specification)
3. [TroopExtensions.cs Architecture](#troopextensionscs-architecture)
4. [TroopQueries.cs Architecture](#troopqueriescs-architecture)
5. [TroopQueryCommands.cs Architecture](#troopquerycommandscs-architecture)
6. [Equipment Detection Strategy](#equipment-detection-strategy)
7. [Sorting Strategy](#sorting-strategy)
8. [Error Handling](#error-handling)
9. [Integration Points](#integration-points)
10. [Implementation Notes](#implementation-notes)
11. [File Structure](#file-structure)

---

## Overview

The Troop Query System provides a comprehensive framework for querying, filtering, and categorizing troops (CharacterObject entities) in Bannerlord.GameMaster. The system follows established patterns from HeroQueries, ItemQueries, and ClanQueries to ensure consistency and maintainability.

**Key Features:**
- Multi-dimensional troop categorization via [Flags] enum
- Equipment-based type detection (shields, weapon classes, mounted status)
- Flexible query interface with AND/OR logic
- Console command integration with smart argument parsing
- Sorting by multiple fields including type flags
- Automatic exclusion of heroes/lords (troops only)

**Data Source:**
- Access Method: `MBObjectManager.Instance.GetObjectTypeList<CharacterObject>()`
- Namespace: `TaleWorlds.Core`
- Entity Type: `CharacterObject` (represents troops - nameless units, NOT heroes/lords)

---

## TroopTypes Enum Specification

### Design Principles
1. **Powers of 2**: Each flag must be a unique power of 2 for bitwise operations
2. **Logical Grouping**: Related types are grouped together numerically
3. **Extensibility**: Room left between groups for future additions
4. **No Conflicts**: No overlapping bit values

### Complete Enum Definition

```csharp
using System;

namespace Bannerlord.GameMaster.Troops
{
    [Flags]
    public enum TroopTypes
    {
        None = 0,
        
        // Formation/Combat Roles (1-16)
        Infantry = 1,              // 2^0  - FormationClass: Infantry
        Ranged = 2,                // 2^1  - FormationClass: Ranged
        Cavalry = 4,               // 2^2  - FormationClass: Cavalry
        HorseArcher = 8,           // 2^3  - FormationClass: HorseArcher
        Mounted = 16,              // 2^4  - IsMounted (Cavalry or HorseArcher)
        
        // Troop Line Categories (32-256)
        Regular = 32,              // 2^5  - Culture's regular/main troop line
        Noble = 64,                // 2^6  - Culture's noble/elite troop line
        Militia = 128,             // 2^7  - Culture's militia (garrison) troop line
        Mercenary = 256,           // 2^8  - Mercenary/Caravan guard troops
        
        // Equipment-Based Categories (512-16384)
        Shield = 512,              // 2^9  - Has shield in equipment
        TwoHanded = 1024,          // 2^10 - Has two-handed weapon
        Polearm = 2048,            // 2^11 - Has polearm weapon
        Bow = 4096,                // 2^12 - Has bow
        Crossbow = 8192,           // 2^13 - Has crossbow
        ThrowingWeapon = 16384,    // 2^14 - Has throwing weapon
        
        // Tier-Based Categories (32768-2097152)
        Tier0 = 32768,             // 2^15 - Tier 0 troops
        Tier1 = 65536,             // 2^16 - Tier 1 troops
        Tier2 = 131072,            // 2^17 - Tier 2 troops
        Tier3 = 262144,            // 2^18 - Tier 3 troops
        Tier4 = 524288,            // 2^19 - Tier 4 troops
        Tier5 = 1048576,           // 2^20 - Tier 5 troops
        Tier6Plus = 2097152,       // 2^21 - Tier 6+ troops (includes tier 7 if modded)
        
        // Culture-Based Categories (4194304-1073741824)
        Empire = 4194304,          // 2^22 - Culture: Empire
        Vlandia = 8388608,         // 2^23 - Culture: Vlandia
        Sturgia = 16777216,        // 2^24 - Culture: Sturgia
        Aserai = 33554432,         // 2^25 - Culture: Aserai
        Khuzait = 67108864,        // 2^26 - Culture: Khuzait
        Battania = 134217728,      // 2^27 - Culture: Battania
        Nord = 268435456,          // 2^28 - Culture: Nord (Warsails DLC - optional)
        Bandit = 536870912,        // 2^29 - Culture: Bandit (special culture)
    }
}
```

### Bit Value Allocation Map

| Range | Purpose | Flags Available |
|-------|---------|-----------------|
| 2^0 - 2^4 | Formation Roles | 5 flags used |
| 2^5 - 2^8 | Troop Line Types | 4 flags used |
| 2^9 - 2^14 | Equipment Types | 6 flags used |
| 2^15 - 2^21 | Tier Categories | 7 flags used (exact tiers) |
| 2^22 - 2^29 | Cultures | 8 flags used |
| 2^30 - 2^31 | Reserved | 2 flags available |

**Notes:**
- Minor faction troops will be detected via Culture.StringId pattern matching
- Nord culture requires Warsails DLC - system handles gracefully if not present
- Bandit is a special culture, not an occupation
- Heroes/Lords are NEVER troops and will always be excluded from queries

---

## TroopExtensions.cs Architecture

### File Location
`Bannerlord.GameMaster/Troops/TroopExtensions.cs`

### Class Structure

```csharp
using System;
using System.Linq;
using TaleWorlds.Core;
using Bannerlord.GameMaster.Common.Interfaces;

namespace Bannerlord.GameMaster.Troops
{
    public static class TroopExtensions
    {
        // Main type detection method
        public static TroopTypes GetTroopTypes(this CharacterObject character);
        
        // Bitwise flag checking
        public static bool HasAllTypes(this CharacterObject character, TroopTypes types);
        public static bool HasAnyType(this CharacterObject character, TroopTypes types);
        
        // Display formatting
        public static string FormattedDetails(this CharacterObject character);
        
        // Interface alias
        public static TroopTypes GetTypes(this CharacterObject character);
        
        // Equipment detection helpers
        public static bool HasShield(this CharacterObject character);
        public static bool HasWeaponType(this CharacterObject character, ItemObject.ItemTypeEnum weaponType);
        public static bool HasWeaponClass(this CharacterObject character, WeaponClass weaponClass);
        public static bool HasTwoHandedWeapon(this CharacterObject character);
        public static bool HasPolearm(this CharacterObject character);
        public static bool IsMounted(this CharacterObject character);
    }
    
    public class TroopExtensionsWrapper : IEntityExtensions<CharacterObject, TroopTypes>
    {
        public TroopTypes GetTypes(CharacterObject entity);
        public bool HasAllTypes(CharacterObject entity, TroopTypes types);
        public bool HasAnyType(CharacterObject entity, TroopTypes types);
        public string FormattedDetails(CharacterObject entity);
    }
}
```

### Method Specifications

#### GetTroopTypes(this CharacterObject character)
**Purpose:** Analyze troop and return all applicable type flags

**IMPORTANT:** Heroes/Lords are never troops. This method must return TroopTypes.None immediately if character.IsHero is true.

**Algorithm:**
```
1. Initialize types = TroopTypes.None
2. IMMEDIATELY return None if character.IsHero is true (heroes are never troops)
3. Check formation class (Infantry, Ranged, Cavalry, HorseArcher)
4. Check if mounted (Cavalry OR HorseArcher)
5. Determine troop line type (Regular, Noble, Militia, Mercenary) via Occupation/StringId patterns
6. Analyze equipment for weapon/shield types
7. Set exact tier flag (Tier0, Tier1, Tier2, Tier3, Tier4, Tier5, Tier6Plus)
8. Check culture against major factions (including Nord if Warsails present)
9. Return combined flags
```

**Key Considerations:**
- **CRITICAL**: Heroes/Lords must NEVER be included - return None immediately if IsHero
- FormationClass.Unset should not set any formation flags
- Mounted flag is set if character is Cavalry OR HorseArcher
- Equipment analysis uses BattleEquipments[0] if available
- Tier mapping: Exact tier numbers (0→Tier0, 1→Tier1, ..., 6+→Tier6Plus)
- Troop line detection may use Occupation enum AND StringId patterns
- Culture strings must be case-insensitive contains checks
- Nord culture requires checking if Warsails DLC is loaded

#### HasAllTypes(this CharacterObject character, TroopTypes types)
**Purpose:** Check if character has ALL specified flags (AND logic)

**Implementation:**
```csharp
public static bool HasAllTypes(this CharacterObject character, TroopTypes types)
{
    if (types == TroopTypes.None) return true;
    var troopTypes = character.GetTroopTypes();
    return (troopTypes & types) == types;
}
```

#### HasAnyType(this CharacterObject character, TroopTypes types)
**Purpose:** Check if character has ANY of specified flags (OR logic)

**Implementation:**
```csharp
public static bool HasAnyType(this CharacterObject character, TroopTypes types)
{
    if (types == TroopTypes.None) return true;
    var troopTypes = character.GetTroopTypes();
    return (troopTypes & types) != TroopTypes.None;
}
```

#### FormattedDetails(this CharacterObject character)
**Purpose:** Return tab-delimited string for display

**Format:**
```
{StringId}\t{Name}\tTier: {Tier}\tLevel: {Level}\tCulture: {Culture}\tFormation: {DefaultFormationClass}
```

**Example Output:**
```
imperial_recruit\tImperial Recruit\tTier: 1\tLevel: 5\tCulture: Empire\tFormation: Infantry
```

#### Equipment Detection Helpers

##### HasShield(this CharacterObject character)
```csharp
public static bool HasShield(this CharacterObject character)
{
    if (character.BattleEquipments == null || !character.BattleEquipments.Any())
        return false;
        
    var equipment = character.BattleEquipments[0];
    for (int i = 0; i < 12; i++) // Equipment.EquipmentSlotCount
    {
        var equipmentElement = equipment[i];
        if (equipmentElement.Item != null && 
            equipmentElement.Item.ItemType == ItemObject.ItemTypeEnum.Shield)
        {
            return true;
        }
    }
    return false;
}
```

##### HasWeaponType(this CharacterObject character, ItemObject.ItemTypeEnum weaponType)
**Purpose:** Check for specific weapon item type
**Parameters:** weaponType - e.g., ItemObject.ItemTypeEnum.Bow
**Returns:** true if character has weapon of specified type

##### HasWeaponClass(this CharacterObject character, WeaponClass weaponClass)
**Purpose:** Check for specific weapon class (more granular than ItemType)
**Parameters:** weaponClass - e.g., WeaponClass.TwoHandedSword
**Returns:** true if character has weapon of specified class

##### HasTwoHandedWeapon(this CharacterObject character)
**Purpose:** Detect two-handed weapons
**Logic:** Check ItemType == TwoHandedWeapon

##### HasPolearm(this CharacterObject character)
**Purpose:** Detect polearm weapons
**Logic:** Check ItemType == Polearm

##### IsMounted(this CharacterObject character)
**Purpose:** Check if character is cavalry/horse archer
**Logic:** Check DefaultFormationClass is Cavalry OR HorseArcher

---

## TroopQueries.cs Architecture

### File Location
`Bannerlord.GameMaster/Troops/TroopQueries.cs`

### Class Structure

```csharp
using System;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.Core;
using TaleWorlds.ObjectSystem;
using Bannerlord.GameMaster.Common.Interfaces;

namespace Bannerlord.GameMaster.Troops
{
    public static class TroopQueries
    {
        public static CharacterObject GetTroopById(string troopId);
        
        public static List<CharacterObject> QueryTroops(
            string query = "",
            TroopTypes requiredTypes = TroopTypes.None,
            bool matchAll = true,
            int tierFilter = -1,
            string sortBy = "id",
            bool sortDescending = false);
            
        private static IEnumerable<CharacterObject> ApplySorting(
            IEnumerable<CharacterObject> troops,
            string sortBy,
            bool descending);
            
        public static TroopTypes ParseTroopType(string typeString);
        public static TroopTypes ParseTroopTypes(IEnumerable<string> typeStrings);
        public static string GetFormattedDetails(List<CharacterObject> troops);
    }
    
    public class TroopQueriesWrapper : IEntityQueries<CharacterObject, TroopTypes>
    {
        public CharacterObject GetById(string id);
        public List<CharacterObject> Query(string query, TroopTypes types, bool matchAll);
        public TroopTypes ParseType(string typeString);
        public TroopTypes ParseTypes(IEnumerable<string> typeStrings);
        public string GetFormattedDetails(List<CharacterObject> entities);
    }
}
```

### Method Specifications

#### GetTroopById(string troopId)
**Purpose:** Direct lookup by StringId

**Implementation:**
```csharp
public static CharacterObject GetTroopById(string troopId)
{
    return MBObjectManager.Instance.GetObject<CharacterObject>(troopId);
}
```

**Note:** Returns null if not found (consistent with ItemQueries pattern)

#### QueryTroops(...)
**Purpose:** Main query method with comprehensive filtering

**Parameters:**
- `query` (string, default ""): Name/ID substring filter (case-insensitive)
- `requiredTypes` (TroopTypes, default None): Type flags to match
- `matchAll` (bool, default true): If true uses AND logic, if false uses OR logic
- `tierFilter` (int, default -1): Specific tier (0-6+), -1 for no filter
- `sortBy` (string, default "id"): Sort field name
- `sortDescending` (bool, default false): Sort direction

**Algorithm:**
```
1. Get all CharacterObject from MBObjectManager
2. IMMEDIATELY filter out all IsHero=true entries (heroes are never troops)
3. Filter by query string (name/ID contains)
4. Filter by tier if specified (exact match)
5. Apply type filtering (matchAll ? HasAllTypes : HasAnyType)
6. Apply sorting
7. Return list
```

**Implementation Pattern:**
```csharp
public static List<CharacterObject> QueryTroops(
    string query = "",
    TroopTypes requiredTypes = TroopTypes.None,
    bool matchAll = true,
    int tierFilter = -1,
    string sortBy = "id",
    bool sortDescending = false)
{
    IEnumerable<CharacterObject> troops =
        MBObjectManager.Instance.GetObjectTypeList<CharacterObject>();
    
    // CRITICAL: Heroes/Lords are NEVER troops - exclude immediately
    troops = troops.Where(t => !t.IsHero);
    
    // Filter by name/ID
    if (!string.IsNullOrEmpty(query))
    {
        string lowerFilter = query.ToLower();
        troops = troops.Where(t =>
            t.Name.ToString().ToLower().Contains(lowerFilter) ||
            t.StringId.ToLower().Contains(lowerFilter));
    }
    
    // Filter by tier (exact match)
    if (tierFilter >= 0)
    {
        troops = troops.Where(t => (int)t.Tier == tierFilter);
    }
    
    // Filter by types
    if (requiredTypes != TroopTypes.None)
    {
        troops = troops.Where(t =>
            matchAll ? t.HasAllTypes(requiredTypes) : t.HasAnyType(requiredTypes));
    }
    
    // Apply sorting
    troops = ApplySorting(troops, sortBy, sortDescending);
    
    return troops.ToList();
}
```

#### ApplySorting(IEnumerable<CharacterObject> troops, string sortBy, bool descending)
**Purpose:** Apply sorting to troop collection

**Supported Sort Fields:**
- `id` - StringId (default)
- `name` - Name
- `tier` - Tier property
- `level` - Level property
- `culture` - Culture.Name
- `occupation` - Occupation enum value
- `formation` - DefaultFormationClass enum value
- Any TroopTypes flag name (sorts by presence of flag)

**Implementation Pattern:**
```csharp
private static IEnumerable<CharacterObject> ApplySorting(
    IEnumerable<CharacterObject> troops,
    string sortBy,
    bool descending)
{
    sortBy = sortBy.ToLower();
    
    // Check if sortBy matches a TroopTypes flag
    if (Enum.TryParse<TroopTypes>(sortBy, true, out var troopType) && 
        troopType != TroopTypes.None)
    {
        return descending
            ? troops.OrderByDescending(t => t.GetTroopTypes().HasFlag(troopType))
            : troops.OrderBy(t => t.GetTroopTypes().HasFlag(troopType));
    }
    
    // Sort by standard fields
    IOrderedEnumerable<CharacterObject> orderedTroops = sortBy switch
    {
        "name" => descending
            ? troops.OrderByDescending(t => t.Name.ToString())
            : troops.OrderBy(t => t.Name.ToString()),
        "tier" => descending
            ? troops.OrderByDescending(t => t.Tier)
            : troops.OrderBy(t => t.Tier),
        "level" => descending
            ? troops.OrderByDescending(t => t.Level)
            : troops.OrderBy(t => t.Level),
        "culture" => descending
            ? troops.OrderByDescending(t => t.Culture?.Name?.ToString() ?? "")
            : troops.OrderBy(t => t.Culture?.Name?.ToString() ?? ""),
        "occupation" => descending
            ? troops.OrderByDescending(t => t.Occupation)
            : troops.OrderBy(t => t.Occupation),
        "formation" => descending
            ? troops.OrderByDescending(t => t.DefaultFormationClass)
            : troops.OrderBy(t => t.DefaultFormationClass),
        _ => descending  // default to id
            ? troops.OrderByDescending(t => t.StringId)
            : troops.OrderBy(t => t.StringId)
    };
    
    return orderedTroops;
}
```

#### ParseTroopType(string typeString)
**Purpose:** Parse single type string to TroopTypes enum

**Alias Support:**
- "1h" → OneHanded (if added to enum)
- "2h" → TwoHanded
- Culture names should work with or without case

**Implementation:**
```csharp
public static TroopTypes ParseTroopType(string typeString)
{
    // Handle common aliases
    var normalized = typeString.ToLower() switch
    {
        "2h" => "TwoHanded",
        "mounted" => "Mounted",
        "cav" => "Cavalry",
        "ha" => "HorseArcher",
        _ => typeString
    };
    
    return Enum.TryParse<TroopTypes>(normalized, true, out var result)
        ? result : TroopTypes.None;
}
```

#### ParseTroopTypes(IEnumerable<string> typeStrings)
**Purpose:** Parse and combine multiple type strings

**Implementation:**
```csharp
public static TroopTypes ParseTroopTypes(IEnumerable<string> typeStrings)
{
    TroopTypes combined = TroopTypes.None;
    foreach (var typeString in typeStrings)
    {
        var parsed = ParseTroopType(typeString);
        if (parsed != TroopTypes.None)
            combined |= parsed;
    }
    return combined;
}
```

#### GetFormattedDetails(List<CharacterObject> troops)
**Purpose:** Format troop list for console output

**Implementation:**
```csharp
public static string GetFormattedDetails(List<CharacterObject> troops)
{
    if (troops.Count == 0)
        return "";
    return string.Join("\n", troops.Select(t => t.FormattedDetails())) + "\n";
}
```

---

## TroopQueryCommands.cs Architecture

### File Location
`Bannerlord.GameMaster/Console/Query/TroopQueryCommands.cs`

### Class Structure

```csharp
using Bannerlord.GameMaster.Troops;
using Bannerlord.GameMaster.Console.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Library;

namespace Bannerlord.GameMaster.Console.Query
{
    [CommandLineFunctionality.CommandLineArgumentFunction("query", "gm")]
    public static class TroopQueryCommands
    {
        // Private helper methods
        private static (string query, TroopTypes types, int tier, string sortBy, bool sortDesc)
            ParseArguments(List<string> args);
        private static void ParseSortParameter(string sortParam, ref string sortBy, ref bool sortDesc);
        private static int ParseTierKeyword(string tierKeyword);
        private static string BuildCriteriaString(string query, TroopTypes types, int tier, string sortBy, bool sortDesc);
        
        // Command methods
        [CommandLineFunctionality.CommandLineArgumentFunction("troop", "gm.query")]
        public static string QueryTroops(List<string> args);
        
        [CommandLineFunctionality.CommandLineArgumentFunction("troop_any", "gm.query")]
        public static string QueryTroopsAny(List<string> args);
        
        [CommandLineFunctionality.CommandLineArgumentFunction("troop_info", "gm.query")]
        public static string QueryTroopInfo(List<string> args);
    }
}
```

### Method Specifications

#### ParseArguments(List<string> args)
**Purpose:** Extract query parameters from command arguments

**Returns:** Tuple containing:
- `query` - Search string
- `types` - Combined TroopTypes flags
- `tier` - Tier filter (-1 for none)
- `sortBy` - Sort field
- `sortDesc` - Sort direction

**Type Keywords:**
```csharp
var typeKeywords = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
{
    // Formation types
    "infantry", "ranged", "cavalry", "horsearcher", "mounted",
    // Troop line types
    "regular", "noble", "militia", "mercenary",
    // Equipment types
    "shield", "twohanded", "2h", "polearm", "bow", "crossbow", "throwing",
    // Tier types (also as keywords)
    "tier0", "tier1", "tier2", "tier3", "tier4", "tier5", "tier6plus",
    // Cultures
    "empire", "vlandia", "sturgia", "aserai", "khuzait", "battania", "nord", "bandit"
};
```

**Tier Keywords:**
```csharp
var tierKeywords = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
{
    "tier0", "tier1", "tier2", "tier3", "tier4", "tier5", "tier6", "tier6plus"
};
```

**Algorithm:**
```
1. Initialize collections for search terms, type terms
2. Initialize tier=-1, sortBy="id", sortDesc=false
3. For each argument:
   - If starts with "sort:" → parse sort parameter
   - Else if matches tier keyword → parse tier
   - Else if matches type keyword → add to type terms
   - Else → add to search terms
4. Combine search terms into query string
5. Parse type terms into TroopTypes flags
6. Return tuple

Note: Heroes are NEVER included - no includeHeroes parameter needed
```

#### QueryTroops(List<string> args)
**Purpose:** Main troop query command (AND logic)

**Usage Examples:**
```
gm.query.troop imperial infantry
gm.query.troop aserai cavalry tier3
gm.query.troop shield infantry sort:tier:desc
gm.query.troop battania ranged bow
gm.query.troop noble empire tier5
```

**Implementation:**
```csharp
[CommandLineFunctionality.CommandLineArgumentFunction("troop", "gm.query")]
public static string QueryTroops(List<string> args)
{
    return Cmd.Run(args, () =>
    {
        if (!CommandBase.ValidateCampaignMode(out string error))
            return error;
        
        var (query, types, tier, sortBy, sortDesc) = ParseArguments(args);
        
        List<CharacterObject> matchedTroops = TroopQueries.QueryTroops(
            query, types, matchAll: true, tier, sortBy, sortDesc);
        
        string criteriaDesc = BuildCriteriaString(query, types, tier, sortBy, sortDesc);
        
        if (matchedTroops.Count == 0)
        {
            return $"Found 0 troop(s) matching {criteriaDesc}\n" +
                   "Usage: gm.query.troop [search] [type keywords] [tier] [sort]\n" +
                   "Type keywords: infantry, ranged, cavalry, horsearcher, shield, bow, crossbow, etc.\n" +
                   "Tier keywords: tier0, tier1, tier2, tier3, tier4, tier5, tier6, tier6plus\n" +
                   "Sort: sort:name, sort:tier, sort:level, sort:culture, sort:<type> (add :desc for descending)\n" +
                   "Example: gm.query.troop imperial infantry tier2 sort:name\n" +
                   "Note: Heroes/Lords are automatically excluded (troops only).\n";
        }
        
        return $"Found {matchedTroops.Count} troop(s) matching {criteriaDesc}:\n" +
               $"{TroopQueries.GetFormattedDetails(matchedTroops)}";
    });
}
```

#### QueryTroopsAny(List<string> args)
**Purpose:** Troop query with OR logic

**Usage Examples:**
```
gm.query.troop_any cavalry ranged (cavalry OR ranged)
gm.query.troop_any bow crossbow tier4
gm.query.troop_any empire vlandia infantry
```

**Implementation:**
```csharp
[CommandLineFunctionality.CommandLineArgumentFunction("troop_any", "gm.query")]
public static string QueryTroopsAny(List<string> args)
{
    return Cmd.Run(args, () =>
    {
        if (!CommandBase.ValidateCampaignMode(out string error))
            return error;
        
        var (query, types, tier, sortBy, sortDesc) = ParseArguments(args);
        
        List<CharacterObject> matchedTroops = TroopQueries.QueryTroops(
            query, types, matchAll: false, tier, sortBy, sortDesc);
        
        string criteriaDesc = BuildCriteriaString(query, types, tier, sortBy, sortDesc);
        
        if (matchedTroops.Count == 0)
        {
            return $"Found 0 troop(s) matching ANY of {criteriaDesc}\n" +
                   "Usage: gm.query.troop_any [search] [type keywords] [tier] [sort]\n" +
                   "Example: gm.query.troop_any cavalry ranged tier3 sort:tier\n";
        }
        
        return $"Found {matchedTroops.Count} troop(s) matching ANY of {criteriaDesc}:\n" +
               $"{TroopQueries.GetFormattedDetails(matchedTroops)}";
    });
}
```

#### QueryTroopInfo(List<string> args)
**Purpose:** Detailed information about specific troop

**Usage:**
```
gm.query.troop_info <troopId>
```

**Example:**
```
gm.query.troop_info imperial_legionary
```

**Output Format:**
```
Troop Information:
ID: imperial_legionary
Name: Imperial Legionary
Tier: 4
Level: 21
Culture: Empire
Occupation: Soldier
Formation: Infantry
Types: Infantry, Soldier, Shield, Regular, Empire
Equipment: [list of equipped items]
Upgrades: [list of upgrade paths if any]
```

**Implementation:**
```csharp
[CommandLineFunctionality.CommandLineArgumentFunction("troop_info", "gm.query")]
public static string QueryTroopInfo(List<string> args)
{
    return Cmd.Run(args, () =>
    {
        if (!CommandBase.ValidateCampaignMode(out string error))
            return error;
        
        if (args == null || args.Count == 0)
            return "Error: Please provide a troop ID.\nUsage: gm.query.troop_info <troopId>\n";
        
        string troopId = args[0];
        CharacterObject troop = TroopQueries.GetTroopById(troopId);
        
        if (troop == null)
            return $"Error: Troop with ID '{troopId}' not found.\n";
        
        var types = troop.GetTroopTypes();
        string cultureName = troop.Culture?.Name?.ToString() ?? "None";
        
        // Build equipment list
        string equipmentInfo = "";
        if (troop.BattleEquipments != null && troop.BattleEquipments.Any())
        {
            var equipment = troop.BattleEquipments[0];
            List<string> items = new List<string>();
            for (int i = 0; i < 12; i++)
            {
                var item = equipment[i].Item;
                if (item != null)
                {
                    items.Add(item.Name.ToString());
                }
            }
            equipmentInfo = items.Count > 0 
                ? "Equipment: " + string.Join(", ", items) + "\n"
                : "Equipment: None\n";
        }
        
        // Build upgrade paths
        string upgradeInfo = "";
        if (troop.UpgradeTargets != null && troop.UpgradeTargets.Length > 0)
        {
            var upgrades = troop.UpgradeTargets.Select(u => u.Name.ToString());
            upgradeInfo = "Upgrades: " + string.Join(", ", upgrades) + "\n";
        }
        else
        {
            upgradeInfo = "Upgrades: None\n";
        }
        
        return $"Troop Information:\n" +
               $"ID: {troop.StringId}\n" +
               $"Name: {troop.Name}\n" +
               $"Tier: {(int)troop.Tier}\n" +
               $"Level: {troop.Level}\n" +
               $"Culture: {cultureName}\n" +
               $"Occupation: {troop.Occupation}\n" +
               $"Formation: {troop.DefaultFormationClass}\n" +
               $"Types: {types}\n" +
               equipmentInfo +
               upgradeInfo;
    });
}
```

#### BuildCriteriaString(...)
**Purpose:** Create human-readable description of query criteria

**Implementation:**
```csharp
private static string BuildCriteriaString(
    string query, TroopTypes types, int tier, string sortBy, bool sortDesc)
{
    List<string> parts = new();
    
    if (!string.IsNullOrEmpty(query))
        parts.Add($"search: '{query}'");
    
    if (types != TroopTypes.None)
    {
        var typeList = Enum.GetValues(typeof(TroopTypes))
            .Cast<TroopTypes>()
            .Where(t => t != TroopTypes.None && types.HasFlag(t))
            .Select(t => t.ToString().ToLower());
        parts.Add($"types: {string.Join(", ", typeList)}");
    }
    
    if (tier >= 0)
        parts.Add($"tier: {tier}");
    
    if (!string.IsNullOrEmpty(sortBy) && sortBy != "id")
        parts.Add($"sort: {sortBy}{(sortDesc ? " (desc)" : " (asc)")}");
    
    return parts.Count > 0 ? string.Join(", ", parts) : "all troops";
}
```

---

## Equipment Detection Strategy

### Overview
Equipment detection is crucial for accurate type categorization. The system analyzes the character's battle equipment to determine weapon and shield usage.

### Equipment Access Pattern

```csharp
// Safe equipment access
if (character.BattleEquipments == null || !character.BattleEquipments.Any())
    return false; // No equipment data

var equipment = character.BattleEquipments[0]; // Use first battle equipment set
```

### Equipment Slot Iteration

```csharp
// Iterate through all equipment slots
for (int i = 0; i < 12; i++) // Equipment.EquipmentSlotCount = 12
{
    var equipmentElement = equipment[i];
    if (equipmentElement.Item == null)
        continue; // Empty slot
        
    // Analyze item
    ItemObject item = equipmentElement.Item;
    // Check item.ItemType, item.WeaponComponent, etc.
}
```

### Weapon Type Detection

**Method 1: ItemType Check (Broad Categories)**
```csharp
switch (item.ItemType)
{
    case ItemObject.ItemTypeEnum.Bow:
        // Has bow
        break;
    case ItemObject.ItemTypeEnum.Crossbow:
        // Has crossbow
        break;
    case ItemObject.ItemTypeEnum.TwoHandedWeapon:
        // Has two-handed weapon
        break;
    case ItemObject.ItemTypeEnum.Polearm:
        // Has polearm
        break;
    case ItemObject.ItemTypeEnum.Thrown:
        // Has throwing weapon
        break;
    case ItemObject.ItemTypeEnum.Shield:
        // Has shield
        break;
}
```

**Method 2: WeaponClass Check (Granular)**
```csharp
if (item.WeaponComponent != null)
{
    WeaponClass weaponClass = item.WeaponComponent.PrimaryWeapon.WeaponClass;
    
    switch (weaponClass)
    {
        case WeaponClass.TwoHandedSword:
        case WeaponClass.TwoHandedAxe:
        case WeaponClass.TwoHandedMace:
            // Two-handed weapon
            break;
        case WeaponClass.LongBow:
        case WeaponClass.Bow:
            // Bow
            break;
        case WeaponClass.Crossbow:
            // Crossbow
            break;
        // ... other classes
    }
}
```

### Performance Considerations

1. **Cache Results**: Consider caching GetTroopTypes() results if called repeatedly
2. **Early Exit**: Return as soon as type is confirmed if only checking one type
3. **Null Checks**: Always check for null equipment before access
4. **Equipment Set**: Use BattleEquipments[0] as primary; avoid iterating all sets

### Edge Cases

1. **No Equipment**: Troops without equipment (shouldn't happen but handle gracefully)
2. **Multiple Weapons**: Character may have multiple weapon types equipped
3. **Hybrid Troops**: May have both ranged and melee weapons
4. **FormationClass.Unset**: Some troops may not have formation assigned

---

## Sorting Strategy

### Supported Sort Fields

| Field | Type | Description | Example |
|-------|------|-------------|---------|
| `id` | string | StringId (default) | `imperial_recruit` |
| `name` | string | Display name | `Imperial Recruit` |
| `tier` | int | Tier property (0-6) | `3` |
| `level` | int | Character level | `21` |
| `culture` | string | Culture name | `Empire` |
| `occupation` | enum | Occupation type | `Soldier` |
| `formation` | enum | Formation class | `Infantry` |
| Any TroopTypes flag | bool | Presence of flag | `shield`, `cavalry` |

### Sort Direction

- Ascending (default): `sort:name`
- Descending: `sort:name:desc`

### Implementation Priority

1. **Check TroopTypes Flags First**: If sortBy matches enum value, sort by flag presence
2. **Standard Fields**: Then check standard field names
3. **Default to ID**: If no match, sort by StringId

### Null Handling

- Culture: Use `?? ""` for null culture names
- Always provide stable sort (secondary sort by ID if needed)

### Performance

- Use LINQ OrderBy/OrderByDescending for simplicity
- For large result sets, consider ThenBy for secondary sorting

---

## Error Handling

### Validation Chain (CommandBase Pattern)

```csharp
public static string QueryTroops(List<string> args)
{
    return Cmd.Run(args, () =>
    {
        // 1. Campaign mode check
        if (!CommandBase.ValidateCampaignMode(out string error))
            return error;
        
        // 2. Parse arguments (no error state)
        var (query, types, tier, sortBy, sortDesc) = ParseArguments(args);
        
        // 3. Execute query (null-safe)
        List<CharacterObject> matchedTroops = TroopQueries.QueryTroops(...);
        
        // 4. Handle empty results
        if (matchedTroops.Count == 0)
        {
            return FormatEmptyResultMessage(...);
        }
        
        // 5. Return success
        return FormatSuccessMessage(...);
    });
}
```

### Error Message Formatting

Follow CommandBase patterns:

**Error Messages:**
```csharp
CommandBase.FormatErrorMessage("message")
// Output: "Error: message\n"
```

**Success Messages:**
```csharp
CommandBase.FormatSuccessMessage("message")
// Output: "Success: message\n"
```

### Common Error Scenarios

1. **Not in Campaign Mode**
   ```
   Error: Must be in campaign mode.
   ```

2. **No Results Found**
   ```
   Found 0 troop(s) matching [criteria]
   Usage: ...
   ```

3. **Invalid Troop ID** (troop_info)
   ```
   Error: Troop with ID 'xyz' not found.
   ```

4. **Missing Arguments** (troop_info)
   ```
   Error: Please provide a troop ID.
   Usage: gm.query.troop_info <troopId>
   ```

### Logging Integration

All commands use `Cmd.Run()` wrapper which provides:
- Automatic command logging via CommandLogger
- Exception handling with formatted error messages
- Success/failure tracking

---

## Integration Points

### Interface Implementations

#### IEntityExtensions<CharacterObject, TroopTypes>
Location: `Bannerlord.GameMaster/Common/Interfaces/IEntityExtensions.cs`

Implemented by: `TroopExtensionsWrapper`

**Methods:**
- `TroopTypes GetTypes(CharacterObject entity)`
- `bool HasAllTypes(CharacterObject entity, TroopTypes types)`
- `bool HasAnyType(CharacterObject entity, TroopTypes types)`
- `string FormattedDetails(CharacterObject entity)`

#### IEntityQueries<CharacterObject, TroopTypes>
Location: `Bannerlord.GameMaster/Common/Interfaces/IEntityQueries.cs`

Implemented by: `TroopQueriesWrapper`

**Methods:**
- `CharacterObject GetById(string id)`
- `List<CharacterObject> Query(string query, TroopTypes types, bool matchAll)`
- `TroopTypes ParseType(string typeString)`
- `TroopTypes ParseTypes(IEnumerable<string> typeStrings)`
- `string GetFormattedDetails(List<CharacterObject> entities)`

### CommandBase Integration

**Used Methods:**
- `CommandBase.ValidateCampaignMode(out string error)` - Campaign mode check
- `CommandBase.FormatErrorMessage(string message)` - Error formatting
- `CommandBase.FormatSuccessMessage(string message)` - Success formatting
- `Cmd.Run(List<string> args, Func<string> action)` - Command wrapper

### MBObjectManager Integration

**Data Access:**
```csharp
using TaleWorlds.ObjectSystem;

// Get all troops
var troops = MBObjectManager.Instance.GetObjectTypeList<CharacterObject>();

// Get specific troop
var troop = MBObjectManager.Instance.GetObject<CharacterObject>(troopId);
```

### Testing Framework Integration

Location: `Bannerlord.GameMaster/Console/Testing/`

**Integration Points:**
- Commands follow same pattern as existing query commands
- Use `Cmd.Run()` for automatic logging compatibility
- Return formatted strings compatible with test validation
- Follow error message format for test assertions

### TaleWorlds API Dependencies

**Required Namespaces:**
```csharp
using TaleWorlds.Core;              // CharacterObject, ItemObject, Equipment
using TaleWorlds.ObjectSystem;       // MBObjectManager
using TaleWorlds.CampaignSystem;     // Campaign
using TaleWorlds.Library;            // CommandLineFunctionality
```

**Key Classes:**
- `CharacterObject` - Main entity type
- `MBObjectManager` - Data access
- `Equipment` - Equipment data structure
- `ItemObject` - Equipment items
- `Occupation` - Occupation enum
- `FormationClass` - Formation enum
- `WeaponClass` - Weapon type enum

---

## Implementation Notes

### Critical Considerations

1. **Hero/Lord Exclusion - MANDATORY**
   - Heroes/Lords are NEVER troops and are ALWAYS excluded
   - No option to include them - they are completely different entities
   - GetTroopTypes() returns None immediately if IsHero is true
   - QueryTroops() filters out all IsHero=true entries at the start

2. **FormationClass.Unset Handling**
   - Some troops may have FormationClass.Unset
   - Don't assign any formation type flags for these
   - Document this behavior

3. **Culture Matching**
   - Use case-insensitive contains matching
   - Support both full names and abbreviations
   - Minor factions detected via Culture.StringId patterns

4. **Equipment Access Safety**
   - Always check for null BattleEquipments
   - Check if collection has elements before accessing [0]
   - Handle empty equipment slots gracefully

5. **Tier Mapping**
   - Game uses tier 0-6 (7 tiers total, possibly tier 7 with mods)
   - Map to exact tier flags: Tier0, Tier1, Tier2, Tier3, Tier4, Tier5, Tier6Plus
   - Tier6Plus includes tier 6 and any higher tiers added by mods

6. **Performance**
   - GetTroopTypes() does significant work (equipment analysis)
   - Consider caching if called repeatedly on same entity
   - Filter by simple properties first before type detection

### Gotchas and Edge Cases

1. **Multiple Weapon Types**
   - A character may have bow AND melee weapon
   - Set multiple equipment flags as appropriate
   - HorseArcher typically has both bow and melee

2. **Shield Detection**
   - Shield is an ItemType, not a WeaponClass
   - Check ItemType directly for shields
   - Infantry with shields should have both Infantry AND Shield flags

3. **Mounted Detection**
   - Mounted flag = Cavalry OR HorseArcher
   - Use IsMounted() helper method
   - Don't rely solely on equipment (use FormationClass)

4. **Occupation Enum**
   - Occupation enum has many values
   - Map most common ones to TroopTypes flags
   - Others handled via enum-to-string display

5. **Culture Enum vs String**
   - Culture property has both enum and Name
   - Use Culture.Name.ToString() for display
   - Use Culture.StringId for precise matching

6. **Troop Line Detection**
   - Regular: Culture's main troop line
   - Noble: Culture's elite/noble troop line
   - Militia: Culture's militia/garrison troops
   - Mercenary: Mercenary and caravan guard troops
   - Detection may use Occupation enum and/or StringId patterns

7. **Sort by Type Flag**
   - Sorts true values first in ascending order
   - Use this to group troops by type
   - Combine with secondary sort for better results

### Testing Recommendations

1. **Test with No Equipment**
   - Some troops might lack equipment data
   - Verify no crashes or null reference exceptions

2. **Test Hero/Lord Exclusion**
   - Verify heroes/lords are never returned
   - Test with queries that might match hero names
   - Verify GetTroopTypes() returns None for heroes

3. **Test Multiple Type Combinations**
   - "cavalry shield empire" - should be AND logic
   - Verify correct bitwise operations

4. **Test Edge Tiers**
   - tier0 and tier6 boundary cases
   - Verify tier filter works correctly

5. **Test Sort Stability**
   - Multiple troops with same sort value
   - Should maintain stable order

6. **Test Parse Edge Cases**
   - Empty arguments
   - Invalid tier keywords
   - Invalid type keywords (should be ignored)

7. **Test Culture Matching**
   - Case insensitivity
   - Partial matches
   - Minor faction culture patterns

### Best Practices

1. **Follow Existing Patterns**
   - Match HeroExtensions/HeroQueries structure exactly
   - Use same naming conventions
   - Follow same parameter order

2. **Documentation**
   - XML comments on all public methods
   - Usage examples in command help text
   - Document bit flag values in enum

3. **Code Organization**
   - Keep equipment helpers in TroopExtensions
   - Keep query logic in TroopQueries
   - Keep command parsing in TroopQueryCommands

4. **Error Messages**
   - Provide helpful usage examples
   - List available keywords
   - Suggest corrections when possible

5. **Performance**
   - Filter by simple properties before expensive ones
   - Use LINQ efficiently (don't iterate multiple times)
   - Consider query result size limits if needed

---

## File Structure

### New Files to Create

```
Bannerlord.GameMaster/
├── Troops/
│   ├── TroopExtensions.cs      (New - ~300 lines)
│   └── TroopQueries.cs         (New - ~200 lines)
└── Console/
    └── Query/
        └── TroopQueryCommands.cs (New - ~250 lines)
```

### Folder Creation
- Create `Troops/` folder at same level as `Heroes/`, `Items/`, `Clans/`, `Kingdoms/`
- Commands go in existing `Console/Query/` folder

### Namespace Structure

```csharp
// TroopExtensions.cs and TroopQueries.cs
namespace Bannerlord.GameMaster.Troops

// TroopQueryCommands.cs
namespace Bannerlord.GameMaster.Console.Query
```

### Project File Integration

Add to `Bannerlord.GameMaster.csproj` (if not auto-detected):
```xml
<ItemGroup>
  <Compile Include="Troops\TroopExtensions.cs" />
  <Compile Include="Troops\TroopQueries.cs" />
  <Compile Include="Console\Query\TroopQueryCommands.cs" />
</ItemGroup>
```

---

## Summary

This architecture provides a complete, production-ready design for the Troop Query System. Key features:

✅ **Comprehensive Type System**: 30 distinct troop type flags covering all categories
✅ **Equipment Detection**: Robust weapon and shield detection via equipment analysis
✅ **Flexible Querying**: Supports AND/OR logic, exact tier filtering, multi-field sorting
✅ **Console Integration**: Three commands with smart argument parsing
✅ **Pattern Consistency**: Follows established patterns from Heroes/Items/Clans
✅ **Interface Compliance**: Implements IEntityExtensions and IEntityQueries
✅ **Error Handling**: Comprehensive validation and formatted error messages
✅ **Hero/Lord Exclusion**: Automatic and mandatory - troops only, never heroes
✅ **Performance**: Efficient filtering and sorting strategies
✅ **Extensibility**: Supports Warsails DLC (Nord culture) and future additions

**Critical Design Notes:**
- CharacterObject represents **troops** (nameless units), NOT heroes/lords
- Heroes/Lords are ALWAYS excluded - this is not optional
- Exact tier mapping (Tier0-Tier6Plus) instead of grouped categories
- Troop line types (Regular, Noble, Militia, Mercenary) replace occupation flags
- Nord culture support for Warsails DLC with graceful handling if not present
- Bandit is a special culture, not an occupation type

The system is ready for implementation by Code mode following the specifications and examples provided in this document.

---

## Implementation Checklist

For Code mode:

- [ ] Create `Troops/` folder
- [ ] Implement `TroopTypes` enum with exact bit values
- [ ] Implement `TroopExtensions.cs` with all methods
- [ ] Implement equipment detection helpers
- [ ] Implement `TroopExtensionsWrapper` class
- [ ] Implement `TroopQueries.cs` with all methods
- [ ] Implement `ApplySorting` method with all sort fields
- [ ] Implement `TroopQueriesWrapper` class
- [ ] Implement `TroopQueryCommands.cs` with all commands
- [ ] Implement `ParseArguments` method
- [ ] Implement three console commands (troop, troop_any, troop_info)
- [ ] Test all type detection logic
- [ ] Test equipment detection with various troop configurations
- [ ] Test query filtering with multiple type combinations
- [ ] Test sorting by all supported fields
- [ ] Test hero/lord automatic exclusion
- [ ] Test tier filtering
- [ ] Verify console command registration
- [ ] Test error handling for all edge cases
- [ ] Verify interface implementations
- [ ] Add XML documentation comments
- [ ] Final integration testing

**Document Version:** 1.0  
**Status:** Complete - Ready for Implementation  
**Next Step:** Hand off to Code mode for implementation