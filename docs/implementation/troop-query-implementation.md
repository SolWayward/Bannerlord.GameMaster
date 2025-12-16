# Troop Query System Implementation Guide

**Navigation:** [← Back to Implementation](../README.md)

---

## Overview

This guide explains the implementation of the Troop Query System, providing comprehensive filtering, categorization, and sorting capabilities for troops (CharacterObject entities) in Bannerlord. The system enables querying troops by formation types, equipment, troop lines, tiers, and cultures using a flexible flag-based architecture.

### Purpose and Use Cases

- **Troop Discovery**: Find troops matching specific criteria (e.g., "all Aserai cavalry with shields")
- **Unit Analysis**: Analyze troop compositions and equipment loadouts
- **Modding Support**: Query custom troops added by mods
- **Testing**: Validate troop configurations and upgrades

### Key Features

- **30 Troop Type Flags**: Multi-dimensional categorization covering formation, equipment, tier, culture, and troop line types
- **Equipment Detection**: Automatic detection of shields, weapon types (bow, crossbow, polearm, two-handed, throwing)
- **AND/OR Logic**: Flexible querying with `troop` (all types must match) and `troop_any` (any type can match)
- **Multi-field Sorting**: Sort by name, tier, level, culture, occupation, formation, or any type flag
- **Hero/Lord Exclusion**: Automatic exclusion of heroes and lords (troops only)

## Architecture

The Troop Query System follows the established three-layer architecture pattern:

### Three-Layer Design

1. **Extensions Layer** ([`TroopExtensions.cs`](../../Bannerlord.GameMaster/Troops/TroopExtensions.cs))
   - Defines [`TroopTypes`](../../Bannerlord.GameMaster/Troops/TroopExtensions.cs:13) enum with 30 flags
   - Implements type detection via [`GetTroopTypes()`](../../Bannerlord.GameMaster/Troops/TroopExtensions.cs:67)
   - Provides type checking: [`HasAllTypes()`](../../Bannerlord.GameMaster/Troops/TroopExtensions.cs:185), [`HasAnyType()`](../../Bannerlord.GameMaster/Troops/TroopExtensions.cs:195)
   - Equipment detection helpers

2. **Queries Layer** ([`TroopQueries.cs`](../../Bannerlord.GameMaster/Troops/TroopQueries.cs))
   - Implements filtering via [`QueryTroops()`](../../Bannerlord.GameMaster/Troops/TroopQueries.cs:35)
   - Handles tier filtering and sorting
   - Returns filtered and sorted results
   - Hero/lord exclusion filter

3. **Commands Layer** ([`TroopQueryCommands.cs`](../../Bannerlord.GameMaster/Console/Query/TroopQueryCommands.cs))
   - Parses user input
   - Validates arguments
   - Formats output
   - Exposes three console commands: `troop`, `troop_any`, `troop_info`

### Data Source

```csharp
// Access all CharacterObject entities
MBObjectManager.Instance.GetObjectTypeList<CharacterObject>()

// Critical: Heroes/Lords must be excluded (IsHero = true)
troops = troops.Where(t => !t.IsHero);
```

**Important**: Heroes and lords are NEVER troops. They are fundamentally different entities and must always be excluded from troop queries.

## TroopTypes Enum

The [`TroopTypes`](../../Bannerlord.GameMaster/Troops/TroopExtensions.cs:13) enum uses flag values (powers of 2) organized into logical categories:

### Formation Types (1-16)

```csharp
Infantry = 1,              // 2^0  - FormationClass: Infantry
Ranged = 2,                // 2^1  - FormationClass: Ranged
Cavalry = 4,               // 2^2  - FormationClass: Cavalry
HorseArcher = 8,           // 2^3  - FormationClass: HorseArcher
Mounted = 16,              // 2^4  - IsMounted (Cavalry or HorseArcher)
```

### Troop Line Types (32-2048)

```csharp
Regular = 32,              // 2^5  - Culture's regular/main troop line
Noble = 64,                // 2^6  - Culture's noble/elite troop line
Militia = 128,             // 2^7  - Culture's militia (garrison) troop line
Mercenary = 256,           // 2^8  - Mercenary troops
Caravan = 512,             // 2^9  - Caravan guards/masters/traders
Peasant = 1024,            // 2^10 - Villagers/peasants/townsfolk
MinorFaction = 2048,       // 2^11 - Minor faction troops (Eleftheroi, Brotherhood, etc.)
```

### Equipment Types (4096-131072)

```csharp
Shield = 4096,             // 2^12 - Has shield in equipment
TwoHanded = 8192,          // 2^13 - Has two-handed weapon
Polearm = 16384,           // 2^14 - Has polearm weapon
Bow = 32768,               // 2^15 - Has bow
Crossbow = 65536,          // 2^16 - Has crossbow
ThrowingWeapon = 131072,   // 2^17 - Has throwing weapon
```

### Tier Types (262144-16777216)

Exact tier mapping (not grouped):

```csharp
Tier0 = 262144,            // 2^18 - Tier 0 troops
Tier1 = 524288,            // 2^19 - Tier 1 troops
Tier2 = 1048576,           // 2^20 - Tier 2 troops
Tier3 = 2097152,           // 2^21 - Tier 3 troops
Tier4 = 4194304,           // 2^22 - Tier 4 troops
Tier5 = 8388608,           // 2^23 - Tier 5 troops
Tier6Plus = 16777216,      // 2^24 - Tier 6+ troops (includes tier 7 if modded)
```

### Culture Types (33554432-4294967296)

```csharp
Empire = 33554432,         // 2^25 - Culture: Empire
Vlandia = 67108864,        // 2^26 - Culture: Vlandia
Sturgia = 134217728,       // 2^27 - Culture: Sturgia
Aserai = 268435456,        // 2^28 - Culture: Aserai
Khuzait = 536870912,       // 2^29 - Culture: Khuzait
Battania = 1073741824,     // 2^30 - Culture: Battania
Nord = 2147483648,         // 2^31 - Culture: Nord (Warsails DLC - optional)
Bandit = 4294967296,       // 2^32 - Culture: Bandit (special culture)
```

**Important Note:** The TroopTypes enum uses `long` as its underlying type (instead of `int`) to accommodate all 32+ flags, as some flag values exceed the range of a signed 32-bit integer.

### Flag Logic

Troops can have multiple flags set simultaneously:

```csharp
// Imperial Legionary has: Infantry | Shield | Regular | Empire | Tier4
// Khuzait Horse Archer has: HorseArcher | Mounted | Bow | Regular | Khuzait | Tier3
// Battanian Fian Champion has: Ranged | Bow | Noble | Battania | Tier5
// Caravan Master has: Infantry | Caravan | Tier0
// Eleftheroi Warrior has: Infantry | MinorFaction | Tier2
```

## Troop Filtering System

### IsActualTroop() Method

The [`IsActualTroop()`](../../Bannerlord.GameMaster/Troops/TroopExtensions.cs:221) method provides comprehensive filtering to exclude non-troop characters. This ensures that only actual military units are returned in queries.

**Purpose:**
- Automatically filter out templates, NPCs, children, wanderers, and other non-combat entities
- Provide clean query results containing only recruitable/usable troops
- Distinguish between combat-capable units and civilian characters

**Exclusion Categories:**

1. **Templates/Equipment Sets** - Test characters and equipment templates:
   - Contains `template`, `_equipment`, `_bat_`, `_civ_`, `_noncom_`

2. **Town NPCs** (Tier 0, Level 1) - Civilian service providers:
   - armorer, barber, blacksmith, beggar, merchant, shop_keeper
   - tavern_wench, tavernkeeper, barmaid, weaponsmith, tavern_gamehost

3. **Notables** (Tier 0, Level 1) - Settlement leaders:
   - Contains `notary`

4. **Wanderers/Companions** - Recruitable heroes:
   - Starts with `spc_notable_`, `spc_wanderer_`, `npc_wanderer`

5. **Children/Teens/Infants** - Age-restricted characters:
   - Contains `child`, `infant`, `teenager`

6. **Practice/Training Dummies** - Combat training targets:
   - Contains `_dummy`, `practice_stage`, `weapon_practice`

7. **Special Characters** - Cutscene and tutorial characters:
   - Contains `cutscene_`, `tutorial_`, `duel_style_`, `player_char_creation_`
   - Contains `disguise_`, `test`, `crazy_man`

8. **Non-Combat Villagers/Peasants/Townsfolk** (Tier 0, Level 1):
   - `villager`, `village_woman`, `townsman`, `townswoman`
   - These are filtered ONLY if Tier 0 and Level 1 (combat-capable peasants are kept)

9. **Caravan Leaders/World Leaders** (Tier 0, Level 1):
   - `caravan_leader` and other `_leader` patterns (except bandit leaders)

**Kept as Troops:**
- Regular military troops (tier 1+)
- Militia (tier 2-3, contains "militia")
- Mercenaries (contains "mercenary" but NOT "leader")
- Caravan Guards (contains "caravan_guard" or "caravan_master")
- Armed Traders (contains "armed_trader" or "sea_trader")
- Bandits (all tiers)
- Minor faction troops (tier 2+)

**Implementation:**

```csharp
public static bool IsActualTroop(this CharacterObject character)
{
    if (character.IsHero)
        return false;

    var stringIdLower = character.StringId.ToLower();
    int tier = character.GetBattleTier();
    int level = character.Level;

    // 1. Templates/Equipment Sets
    if (stringIdLower.Contains("template") || stringIdLower.Contains("_equipment") ||
        stringIdLower.Contains("_bat_") || stringIdLower.Contains("_civ_") ||
        stringIdLower.Contains("_noncom_"))
        return false;

    // 2. Town NPCs (Tier 0, Level 1)
    if (tier == 0 && level == 1 &&
        (stringIdLower.Contains("armorer") || stringIdLower.Contains("barber") ||
         // ... other NPC checks))
        return false;

    // ... additional exclusion checks

    return true; // Keep as actual troop
}
```

**Usage in Queries:**

The [`QueryTroops()`](../../Bannerlord.GameMaster/Troops/TroopQueries.cs:35) method automatically applies this filter:

```csharp
IEnumerable<CharacterObject> troops = MBObjectManager.Instance.GetObjectTypeList<CharacterObject>();

// CRITICAL: Filter out non-troops (heroes, NPCs, children, templates, etc.)
troops = troops.Where(t => t.IsActualTroop());
```

## Troop Categorization System

### GetTroopCategory() Method

The [`GetTroopCategory()`](../../Bannerlord.GameMaster/Troops/TroopExtensions.cs:299) method returns the primary category label for a troop, useful for display and quick identification.

**Purpose:**
- Provide human-readable category labels
- Support the new FormattedDetails output format
- Enable quick troop type identification

**Category Priority** (checked in order):

1. **"Non-Troop"** - Character is not an actual troop (filtered by IsActualTroop)
2. **"Bandit"** - Has Bandit culture flag
3. **"Minor Faction"** - Has MinorFaction flag (Eleftheroi, Brotherhood of Woods, etc.)
4. **"Caravan"** - Has Caravan flag (guards, masters, traders)
5. **"Peasant"** - Has Peasant flag (villagers, townsfolk)
6. **"Noble/Elite"** - Has Noble flag (knights, elite troops)
7. **"Militia"** - Has Militia flag (garrison troops)
8. **"Mercenary"** - Has Mercenary flag
9. **"Regular"** - Has Regular flag (standard culture troops)
10. **"Unknown"** - Fallback for unrecognized troops

**Implementation:**

```csharp
public static string GetTroopCategory(this CharacterObject character)
{
    if (!character.IsActualTroop())
        return "Non-Troop";

    var types = character.GetTroopTypes();

    // Check specific categories first
    if (types.HasFlag(TroopTypes.Bandit))
        return "Bandit";
    
    if (types.HasFlag(TroopTypes.MinorFaction))
        return "Minor Faction";

    if (types.HasFlag(TroopTypes.Caravan))
        return "Caravan";

    if (types.HasFlag(TroopTypes.Peasant))
        return "Peasant";

    if (types.HasFlag(TroopTypes.Noble))
        return "Noble/Elite";

    if (types.HasFlag(TroopTypes.Militia))
        return "Militia";

    if (types.HasFlag(TroopTypes.Mercenary))
        return "Mercenary";

    if (types.HasFlag(TroopTypes.Regular))
        return "Regular";

    return "Unknown";
}
```

**Example Categories:**
- Imperial Legionary → "Regular"
- Vlandian Knight → "Noble/Elite"
- Empire Militia Veteran → "Militia"
- Caravan Guard → "Caravan"
- Forest Bandit → "Bandit"
- Eleftheroi Warrior → "Minor Faction"

## TroopExtensions Implementation

### GetTroopTypes() Method

The core type detection method that analyzes troops across multiple dimensions:

```csharp
public static TroopTypes GetTroopTypes(this CharacterObject character)
{
    // CRITICAL: Heroes/Lords are never troops - exclude immediately
    if (character.IsHero)
        return TroopTypes.None;

    TroopTypes types = TroopTypes.None;

    // 1. Formation/Combat Roles
    switch (character.DefaultFormationClass)
    {
        case FormationClass.Infantry:
            types |= TroopTypes.Infantry;
            break;
        case FormationClass.Cavalry:
            types |= TroopTypes.Cavalry | TroopTypes.Mounted;
            break;
        // ... other cases
    }

    // 2. Troop Line Categories (Occupation + StringId patterns)
    if (character.Occupation == Occupation.Soldier)
        types |= TroopTypes.Regular;
    else if (stringId.Contains("noble") || stringId.Contains("knight"))
        types |= TroopTypes.Noble;
    // ... other patterns

    // 3. Equipment-Based Categories
    if (character.HasShield())
        types |= TroopTypes.Shield;
    if (character.HasWeaponType(ItemObject.ItemTypeEnum.Bow))
        types |= TroopTypes.Bow;
    // ... other equipment

    // 4. Exact Tier Mapping
    int tier = (int)character.Tier;
    if (tier == 3)
        types |= TroopTypes.Tier3;
    // ... other tiers

    // 5. Culture Detection
    if (character.Culture.StringId.Contains("empire"))
        types |= TroopTypes.Empire;
    // ... other cultures

    return types;
}
```

**Critical Considerations:**

- **Hero/Lord Check First**: Return `TroopTypes.None` immediately if `IsHero` is true
- **FormationClass.Unset**: Some troops have no formation; don't assign formation flags
- **Mounted Flag**: Set if troop is Cavalry OR HorseArcher
- **Troop Line Detection**: Uses both `Occupation` enum and `StringId` patterns
- **Exact Tier Mapping**: Each tier gets its own flag (Tier0-Tier6Plus)

### Type Checking Methods

#### HasAllTypes (AND Logic)

Returns true only if troop has ALL specified flags:

```csharp
public static bool HasAllTypes(this CharacterObject character, TroopTypes types)
{
    if (types == TroopTypes.None) return true;
    var troopTypes = character.GetTroopTypes();
    return (troopTypes & types) == types;  // Bitwise AND equals types
}
```

Example:
```
Query: infantry shield empire
Required: Infantry | Shield | Empire
Troop has: Infantry | Shield | Regular | Empire | Tier4 ✓ MATCH
Troop has: Cavalry | Shield | Empire | Tier3 ✗ NO MATCH (not infantry)
```

#### HasAnyType (OR Logic)

Returns true if troop has ANY of the specified flags:

```csharp
public static bool HasAnyType(this CharacterObject character, TroopTypes types)
{
    if (types == TroopTypes.None) return true;
    var troopTypes = character.GetTroopTypes();
    return (troopTypes & types) != TroopTypes.None;  // Any overlap
}
```

Example:
```
Query: cavalry ranged (using troop_any)
Required: Cavalry | Ranged
Troop has: Cavalry | Mounted | Aserai ✓ MATCH
Troop has: Ranged | Bow | Battania ✓ MATCH
Troop has: Infantry | Shield | Empire ✗ NO MATCH
```

### FormattedDetails() Method

Returns tab-delimited string for display with category information:

```csharp
public static string FormattedDetails(this CharacterObject character)
{
    string cultureName = character.Culture?.Name?.ToString() ?? "None";
    string category = character.GetTroopCategory();
    return $"{character.StringId}\t{character.Name}\t[{category}]\tTier: {character.GetBattleTier()}\t" +
           $"Level: {character.Level}\tCulture: {cultureName}\t" +
           $"Formation: {character.DefaultFormationClass}";
}
```

**Output Example:**
```
imperial_legionary	Imperial Legionary	[Regular]	Tier: 4	Level: 21	Culture: Empire	Formation: Infantry
vlandian_knight	Vlandian Knight	[Noble/Elite]	Tier: 5	Level: 28	Culture: Vlandia	Formation: Cavalry
caravan_guard	Caravan Guard	[Caravan]	Tier: 2	Level: 15	Culture: Empire	Formation: Infantry
forest_bandit	Forest Bandit	[Bandit]	Tier: 1	Level: 8	Culture: Bandit	Formation: Infantry
```

**Key Changes:**
- Added `[{category}]` display between name and tier
- Shows the primary troop category for quick identification
- Uses [`GetTroopCategory()`](../../Bannerlord.GameMaster/Troops/TroopExtensions.cs:299) for consistent categorization
- Uses `GetBattleTier()` instead of `Tier` property for accuracy

## TroopQueries Implementation

### GetTroopById() Method

Direct lookup by StringId:

```csharp
public static CharacterObject GetTroopById(string troopId)
{
    return MBObjectManager.Instance.GetObject<CharacterObject>(troopId);
}
```

Returns `null` if not found.

### QueryTroops() Method

Main query method with comprehensive filtering:

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

**Filter Pipeline:**

1. Get all CharacterObject from MBObjectManager
2. **Exclude heroes/lords** (CRITICAL - first filter after retrieval)
3. Filter by name/ID substring (case-insensitive)
4. Filter by exact tier if specified
5. Apply type filtering with AND/OR logic
6. Apply sorting
7. Return results

### ApplySorting() Method

Supports sorting by standard fields and type flags:

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

**Supported Sort Fields:**

- `id` - StringId (default)
- `name` - Display name
- `tier` - Tier property (0-6+)
- `level` - Character level
- `culture` - Culture name
- `occupation` - Occupation enum value
- `formation` - DefaultFormationClass enum value
- Any [`TroopTypes`](../../Bannerlord.GameMaster/Troops/TroopExtensions.cs:13) flag name (sorts by presence of flag)

### Parse Methods

#### ParseTroopType() with Alias Support

```csharp
public static TroopTypes ParseTroopType(string typeString)
{
    // Handle common aliases
    var normalized = typeString.ToLower() switch
    {
        "2h" => "TwoHanded",
        "cav" => "Cavalry",
        "ha" => "HorseArcher",
        _ => typeString
    };
    
    return Enum.TryParse<TroopTypes>(normalized, true, out var result)
        ? result : TroopTypes.None;
}
```

#### ParseTroopTypes() for Multiple Values

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

## TroopQueryCommands Implementation

### Three Console Commands

1. **`gm.query.troop`** - Query with AND logic (all types must match)
2. **`gm.query.troop_any`** - Query with OR logic (any type can match)
3. **`gm.query.troop_info`** - Detailed info about specific troop

### Argument Parsing Strategy

The [`ParseArguments()`](../../Bannerlord.GameMaster/Console/Query/TroopQueryCommands.cs:18) method categorizes input:

```csharp
private static (string query, TroopTypes types, int tier, string sortBy, bool sortDesc)
    ParseArguments(List<string> args)
{
    var typeKeywords = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        // Formation types
        "infantry", "ranged", "cavalry", "horsearcher", "mounted",
        // Troop line types
        "regular", "noble", "militia", "mercenary",
        // Equipment types
        "shield", "twohanded", "2h", "polearm", "bow", "crossbow", "throwing",
        // Tier types
        "tier0", "tier1", "tier2", "tier3", "tier4", "tier5", "tier6", "tier6plus",
        // Cultures
        "empire", "vlandia", "sturgia", "aserai", "khuzait", "battania", "nord", "bandit"
    };

    var tierKeywords = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        "tier0", "tier1", "tier2", "tier3", "tier4", "tier5", "tier6", "tier6plus"
    };

    // Categorize each argument
    foreach (var arg in args)
    {
        if (arg.StartsWith("sort:"))
            ParseSortParameter(arg, ref sortBy, ref sortDesc);
        else if (tierKeywords.Contains(arg))
            tier = ParseTierKeyword(arg);
        else if (typeKeywords.Contains(arg))
            typeTerms.Add(arg);
        else
            searchTerms.Add(arg);
    }
    
    // Combine results
    string query = string.Join(" ", searchTerms);
    TroopTypes types = TroopQueries.ParseTroopTypes(typeTerms);
    
    return (query, types, tier, sortBy, sortDesc);
}
```

### Sort Parameter Parsing

Handles formats like:
- `sort:name` → field="name", desc=false
- `sort:tier:desc` → field="tier", desc=true
- `sort:shield:desc` → field="shield", desc=true (sorts by Shield flag presence)

```csharp
private static void ParseSortParameter(string sortParam, ref string sortBy, ref bool sortDesc)
{
    var parts = sortParam.Split(':');
    if (parts.Length >= 2)
        sortBy = parts[1].ToLower();
    if (parts.Length >= 3)
        sortDesc = parts[2].Equals("desc", StringComparison.OrdinalIgnoreCase);
}
```

### Error Handling and Validation

All commands use the `Cmd.Run()` wrapper pattern:

```csharp
[CommandLineFunctionality.CommandLineArgumentFunction("troop", "gm.query")]
public static string QueryTroops(List<string> args)
{
    return Cmd.Run(args, () =>
    {
        // 1. Campaign mode check
        if (!CommandBase.ValidateCampaignMode(out string error))
            return error;
        
        // 2. Parse arguments
        var (query, types, tier, sortBy, sortDesc) = ParseArguments(args);
        
        // 3. Execute query
        List<CharacterObject> matchedTroops = TroopQueries.QueryTroops(
            query, types, matchAll: true, tier, sortBy, sortDesc);
        
        // 4. Handle empty results
        if (matchedTroops.Count == 0)
        {
            return $"Found 0 troop(s) matching {criteriaDesc}\n" +
                   "Usage: gm.query.troop [search] [type keywords] [tier] [sort]\n" +
                   // ... usage instructions
        }
        
        // 5. Return success
        return $"Found {matchedTroops.Count} troop(s) matching {criteriaDesc}:\n" +
               $"{TroopQueries.GetFormattedDetails(matchedTroops)}";
    });
}
```

## Equipment Detection

### Safe Equipment Access Pattern

Always check for null and empty equipment:

```csharp
public static bool HasShield(this CharacterObject character)
{
    if (character.BattleEquipments == null || !character.BattleEquipments.Any())
        return false;
        
    var equipment = character.BattleEquipments[0]; // Use first battle equipment set
    
    for (int i = 0; i < 12; i++) // Equipment.EquipmentSlotCount = 12
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

### ItemType vs WeaponClass Detection

**ItemType Detection** (Broad Categories):

```csharp
public static bool HasWeaponType(this CharacterObject character, ItemObject.ItemTypeEnum weaponType)
{
    if (character.BattleEquipments == null || !character.BattleEquipments.Any())
        return false;
        
    var equipment = character.BattleEquipments[0];
    for (int i = 0; i < 12; i++)
    {
        var equipmentElement = equipment[i];
        if (equipmentElement.Item != null && equipmentElement.Item.ItemType == weaponType)
        {
            return true;
        }
    }
    return false;
}
```

**WeaponClass Detection** (Granular):

```csharp
public static bool HasWeaponClass(this CharacterObject character, WeaponClass weaponClass)
{
    if (character.BattleEquipments == null || !character.BattleEquipments.Any())
        return false;
        
    var equipment = character.BattleEquipments.First();
    for (int i = 0; i < 12; i++)
    {
        var equipmentElement = equipment[i];
        if (equipmentElement.Item?.WeaponComponent != null)
        {
            if (equipmentElement.Item.WeaponComponent.PrimaryWeapon.WeaponClass == weaponClass)
            {
                return true;
            }
        }
    }
    return false;
}
```

### Null Handling

**Critical Safety Checks:**

1. Check `BattleEquipments` is not null
2. Check `BattleEquipments` has elements
3. Check equipment slot item is not null
4. Check `WeaponComponent` is not null (for weapon checks)
5. Use null-coalescing for culture names: `?? ""`

## Usage Examples

### Basic Queries

List all troops:
```bash
gm.query.troop
```

List all infantry:
```bash
gm.query.troop infantry
```

List all Aserai troops:
```bash
gm.query.troop aserai
```

### Combined Filters (AND Logic)

Aserai cavalry at tier 3:
```bash
gm.query.troop aserai cavalry tier3
```

Infantry with shields from Empire:
```bash
gm.query.troop empire infantry shield
```

Battanian noble ranged units with bows:
```bash
gm.query.troop battania noble ranged bow
```

### Sorting

Sort troops by tier (ascending):
```bash
gm.query.troop infantry sort:tier
```

Sort by tier descending:
```bash
gm.query.troop shield sort:tier:desc
```

Sort by presence of Shield flag:
```bash
gm.query.troop infantry sort:shield:desc
```

Sort by culture name:
```bash
gm.query.troop tier5 sort:culture
```

### OR Logic (troop_any)

Find troops that are cavalry OR ranged:
```bash
gm.query.troop_any cavalry ranged
```

Find troops with bow OR crossbow:
```bash
gm.query.troop_any bow crossbow tier4
```

Find troops from Empire OR Vlandia:
```bash
gm.query.troop_any empire vlandia infantry
```

### Detailed Info

Get detailed information about specific troop:
```bash
gm.query.troop_info imperial_legionary
```

Output:
```
Troop Information:
ID: imperial_legionary
Name: Imperial Legionary
Tier: 4
Level: 21
Culture: Empire
Occupation: Soldier
Formation: Infantry
Types: Infantry, Regular, Shield, Empire, Tier4
Equipment: Imperial Scale Armor, Imperial Padded Cloth, Imperial Guarded Town Boots, ...
Upgrades: Imperial Elite Cataphract, Imperial Bucellarii
```

## Testing

### Test Location

Comprehensive tests are located in [`StandardTests.cs`](../../Bannerlord.GameMaster/Console/Testing/StandardTests.cs).

### Test Coverage

The Troop Query System includes **42 comprehensive test cases** organized into categories:

#### Basic Tests
- Query all troops
- Query by name
- Query non-existent troop
- Verify hero/lord exclusion

#### Formation Tests
- Infantry only
- Ranged only
- Cavalry only
- Horse archer only
- Mounted troops (cavalry + horse archers)

#### Equipment Tests
- Troops with shields
- Troops with bows
- Troops with crossbows
- Troops with polearms
- Troops with two-handed weapons
- Troops with throwing weapons

#### Tier Tests
- Tier 0 troops
- Tier 1 troops
- Tier 2 troops
- Tier 3 troops
- Tier 4 troops
- Tier 5 troops
- Tier 6+ troops

#### Culture Tests
- Empire troops
- Vlandia troops
- Sturgia troops
- Aserai troops
- Khuzait troops
- Battania troops
- Bandit troops

#### Troop Line Tests
- Regular troops
- Noble troops
- Militia troops
- Mercenary troops

#### Combined Filter Tests
- Multiple type combinations (AND logic)
- Complex queries with name + types + tier
- Edge cases and boundary conditions

#### Sorting Tests
- Sort by name
- Sort by tier
- Sort by level
- Sort by culture
- Sort by type flags

#### OR Logic Tests
- Multiple formations (troop_any)
- Multiple equipment types (troop_any)
- Multiple cultures (troop_any)

### Running Tests

```bash
gm.test.run TroopQueryTests
```

## Best Practices

### Hero/Lord Exclusion

**CRITICAL**: Heroes and lords are NEVER troops:

- [`GetTroopTypes()`](../../Bannerlord.GameMaster/Troops/TroopExtensions.cs:67) returns `TroopTypes.None` immediately if `IsHero` is true
- [`QueryTroops()`](../../Bannerlord.GameMaster/Troops/TroopQueries.cs:35) filters out all `IsHero=true` entries at the start
- This is automatic and mandatory - there is no option to include heroes

```csharp
// ALWAYS check IsHero first in GetTroopTypes()
if (character.IsHero)
    return TroopTypes.None;

// ALWAYS filter heroes in QueryTroops()
troops = troops.Where(t => !t.IsHero);
```

### Equipment Detection

Always use safe access patterns:

```csharp
// Check for null equipment
if (character.BattleEquipments == null || !character.BattleEquipments.Any())
    return false;

// Use first equipment set
var equipment = character.BattleEquipments[0];

// Check each slot for null
if (equipmentElement.Item != null)
{
    // Process item
}
```

### Troop Line Detection

Uses both `Occupation` enum and `StringId` patterns:

```csharp
// Occupation enum
if (character.Occupation == Occupation.Soldier)
    types |= TroopTypes.Regular;

// StringId pattern matching
if (stringId.Contains("noble") || stringId.Contains("knight"))
    types |= TroopTypes.Noble;
```

### Performance Considerations

1. **Filter Order**: Simple filters first (name/ID), then expensive ones (type detection)
2. **Cache Results**: Consider caching [`GetTroopTypes()`](../../Bannerlord.GameMaster/Troops/TroopExtensions.cs:67) results if called repeatedly
3. **Early Hero Check**: Return immediately if `IsHero` to avoid unnecessary processing
4. **Equipment Analysis**: Only iterate equipment once during type detection

## Integration Points

### IEntityExtensions Interface

Implemented by [`TroopExtensionsWrapper`](../../Bannerlord.GameMaster/Troops/TroopExtensions.cs:309):

```csharp
public class TroopExtensionsWrapper : IEntityExtensions<CharacterObject, TroopTypes>
{
    public TroopTypes GetTypes(CharacterObject entity) => entity.GetTroopTypes();
    public bool HasAllTypes(CharacterObject entity, TroopTypes types) => entity.HasAllTypes(types);
    public bool HasAnyType(CharacterObject entity, TroopTypes types) => entity.HasAnyType(types);
    public string FormattedDetails(CharacterObject entity) => entity.FormattedDetails();
}
```

### IEntityQueries Interface

Implemented by [`TroopQueriesWrapper`](../../Bannerlord.GameMaster/Troops/TroopQueries.cs:173):

```csharp
public class TroopQueriesWrapper : IEntityQueries<CharacterObject, TroopTypes>
{
    public CharacterObject GetById(string id) => TroopQueries.GetTroopById(id);
    public List<CharacterObject> Query(string query, TroopTypes types, bool matchAll) => 
        TroopQueries.QueryTroops(query, types, matchAll);
    public TroopTypes ParseType(string typeString) => TroopQueries.ParseTroopType(typeString);
    public TroopTypes ParseTypes(IEnumerable<string> typeStrings) => 
        TroopQueries.ParseTroopTypes(typeStrings);
    public string GetFormattedDetails(List<CharacterObject> entities) => 
        TroopQueries.GetFormattedDetails(entities);
}
```

### CommandBase Integration

All commands use CommandBase utilities:

```csharp
// Campaign mode validation
if (!CommandBase.ValidateCampaignMode(out string error))
    return error;

// Command wrapper with logging
return Cmd.Run(args, () => { /* command logic */ });
```

### MBObjectManager Integration

Data access through MBObjectManager:

```csharp
// Get all CharacterObject entities
var troops = MBObjectManager.Instance.GetObjectTypeList<CharacterObject>();

// Get specific troop by ID
var troop = MBObjectManager.Instance.GetObject<CharacterObject>(troopId);
```

### Testing Framework Integration

Tests use the same framework as other query systems:

- Located in [`Console/Testing/`](../../Bannerlord.GameMaster/Console/Testing/)
- Use [`TestCase`](../../Bannerlord.GameMaster/Console/Testing/TestCase.cs) class for test definitions
- Execute via [`TestRunner`](../../Bannerlord.GameMaster/Console/Testing/TestRunner.cs)
- Results validated against expected patterns

## Reference Architecture Document

For detailed architectural specifications, design decisions, and implementation guidelines, see:
- [`TroopQuerySystem_Architecture.md`](../../plans/TroopQuerySystem_Architecture.md)

---

**Navigation:** [← Back to Implementation](../README.md)