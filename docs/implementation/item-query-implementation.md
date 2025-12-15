# Item Query System Implementation Guide

**Navigation:** [← Back to Implementation](../README.md)

---

## Overview

This guide explains the implementation of the item query filtering and sorting system, including the new features added for bow/crossbow separation, civilian/combat distinction, tier filtering, and sorting capabilities.

## Architecture

### Three-Layer Design

1. **Extensions Layer** ([`ItemExtensions.cs`](../../Bannerlord.GameMaster/Items/ItemExtensions.cs))
   - Defines `ItemTypes` enum with flags
   - Implements type detection logic
   - Provides type checking methods

2. **Queries Layer** ([`ItemQueries.cs`](../../Bannerlord.GameMaster/Items/ItemQueries.cs))
   - Implements filtering logic
   - Handles tier filtering
   - Provides sorting functionality
   - Returns filtered and sorted results

3. **Commands Layer** ([`ItemQueryCommands.cs`](../../Bannerlord.GameMaster/Console/Query/ItemQueryCommands.cs))
   - Parses user input
   - Validates arguments
   - Formats output
   - Exposes console commands

## Item Types Enum

The `ItemTypes` enum uses flag values (powers of 2) to allow bitwise combinations:

```csharp
[Flags]
public enum ItemTypes
{
    None = 0,
    Weapon = 1,
    Armor = 2,
    Mount = 4,
    Food = 8,
    Trade = 16,
    OneHanded = 32,
    TwoHanded = 64,
    Ranged = 128,          // Generic ranged (bow OR crossbow)
    Shield = 256,
    HeadArmor = 512,
    BodyArmor = 1024,
    LegArmor = 2048,
    HandArmor = 4096,
    Cape = 8192,
    Thrown = 16384,
    Arrows = 32768,
    Bolts = 65536,
    Polearm = 131072,
    Banner = 262144,
    Goods = 524288,
    Bow = 1048576,         // Specific bow type
    Crossbow = 2097152,    // Specific crossbow type
    Civilian = 4194304,    // Civilian equipment
    Combat = 8388608,      // Combat equipment
    HorseArmor = 16777216  // Horse armor/barding
}
```

### Flag Logic

Items can have multiple flags set simultaneously:
```csharp
// A bow has: Weapon | Ranged | Bow | Combat
// A civilian robe has: Armor | BodyArmor | Civilian
// Imperial scale armor has: Armor | BodyArmor | Combat
```

## Type Detection Logic

### GetItemTypes() Method

The method assigns appropriate flags based on item properties:

```csharp
public static ItemTypes GetItemTypes(this ItemObject item)
{
    ItemTypes types = ItemTypes.None;
    bool isCombatItem = false;
    bool isCivilianItem = false;

    // Switch on ItemType enum
    switch (item.ItemType)
    {
        case ItemObject.ItemTypeEnum.Bow:
            types |= ItemTypes.Weapon | ItemTypes.Ranged | ItemTypes.Bow;
            isCombatItem = true;
            break;
        case ItemObject.ItemTypeEnum.Crossbow:
            types |= ItemTypes.Weapon | ItemTypes.Ranged | ItemTypes.Crossbow;
            isCombatItem = true;
            break;
        // ... other cases
    }

    // Check armor for civilian status
    if (item.ArmorComponent != null)
    {
        if (AllArmorValuesZero(item.ArmorComponent))
            isCivilianItem = true;
        else
            isCombatItem = true;
    }

    // Apply civilian/combat flags
    if (isCivilianItem) types |= ItemTypes.Civilian;
    if (isCombatItem) types |= ItemTypes.Combat;
    if (item.IsFood) types |= ItemTypes.Civilian;

    return types;
}
```

### Civilian vs Combat Detection

**Civilian Items:**
- Armor with zero protection values (clothing)
- Trade goods
- Food items
- Decorative items

**Combat Items:**
- All weapons
- Armor with protection values
- Shields
- Ammunition

## Filtering Implementation

### QueryItems Method Signature

```csharp
public static List<ItemObject> QueryItems(
    string query = "",              // Name/ID search
    ItemTypes requiredTypes = ItemTypes.None,  // Type flags
    bool matchAll = true,           // AND vs OR logic
    int tierFilter = -1,            // Tier level (0-6, -1 = no filter)
    string sortBy = "id",           // Sort field
    bool sortDescending = false)    // Sort direction
```

### Filter Pipeline

1. **Text Search** - Filter by name/ID/tier text
2. **Type Filtering** - Apply type flags (AND or OR)
3. **Tier Filtering** - Filter by exact tier level
4. **Sorting** - Order results

### Code Example

```csharp
IEnumerable<ItemObject> items = MBObjectManager.Instance.GetObjectTypeList<ItemObject>();

// Step 1: Text search
if (!string.IsNullOrEmpty(query))
{
    string lowerFilter = query.ToLower();
    items = items.Where(i =>
        i.Name.ToString().ToLower().Contains(lowerFilter) ||
        i.StringId.ToLower().Contains(lowerFilter) ||
        (i.Tier >= 0 && i.Tier.ToString().Contains(lowerFilter)));
}

// Step 2: Type filtering
if (requiredTypes != ItemTypes.None)
{
    items = items.Where(i => 
        matchAll ? i.HasAllTypes(requiredTypes) : i.HasAnyType(requiredTypes));
}

// Step 3: Tier filtering
if (tierFilter >= 0)
{
    items = items.Where(i => (int)i.Tier == tierFilter);
}

// Step 4: Sorting
items = ApplySorting(items, sortBy, sortDescending);

return items.ToList();
```

## Sorting Implementation

### ApplySorting Method

```csharp
private static IEnumerable<ItemObject> ApplySorting(
    IEnumerable<ItemObject> items, 
    string sortBy, 
    bool descending)
{
    sortBy = sortBy.ToLower();

    IOrderedEnumerable<ItemObject> orderedItems = sortBy switch
    {
        "name" => descending 
            ? items.OrderByDescending(i => i.Name.ToString())
            : items.OrderBy(i => i.Name.ToString()),
        "tier" => descending
            ? items.OrderByDescending(i => i.Tier)
            : items.OrderBy(i => i.Tier),
        "value" => descending
            ? items.OrderByDescending(i => i.Value)
            : items.OrderBy(i => i.Value),
        "type" => descending
            ? items.OrderByDescending(i => i.ItemType.ToString())
            : items.OrderBy(i => i.ItemType.ToString()),
        _ => descending  // default to id
            ? items.OrderByDescending(i => i.StringId)
            : items.OrderBy(i => i.StringId)
    };

    return orderedItems;
}
```

### Supported Sort Fields

- **name** - Alphabetical by display name
- **tier** - Numerical by tier level (enum cast to int)
- **value** - Numerical by gold value
- **type** - Alphabetical by ItemType enum name
- **id** - Alphabetical by StringId (default)

## Argument Parsing

### ParseArguments Method

Separates user input into components:

```csharp
private static (string query, ItemTypes types, int tier, string sortBy, bool sortDesc) 
    ParseArguments(List<string> args)
{
    // Define keyword sets
    var typeKeywords = new HashSet<string> { "weapon", "armor", "bow", ... };
    var tierKeywords = new HashSet<string> { "tier0", "tier1", ... };

    // Parse each argument
    foreach (var arg in args)
    {
        if (arg.StartsWith("sort:"))
            ParseSortParameter(arg, ref sortBy, ref sortDesc);
        else if (tierKeywords.Contains(arg))
            tierFilter = ParseTierKeyword(arg);
        else if (typeKeywords.Contains(arg))
            typeTerms.Add(arg);
        else
            searchTerms.Add(arg);
    }

    // Combine results
    string query = string.Join(" ", searchTerms);
    ItemTypes types = ItemQueries.ParseItemTypes(typeTerms);
    
    return (query, types, tierFilter, sortBy, sortDesc);
}
```

### Sort Parameter Parsing

Handles formats like:
- `sort:name` → field="name", desc=false
- `sort:value:desc` → field="value", desc=true
- `sort:tier:asc` → field="tier", desc=false

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

## Type Checking Methods

### HasAllTypes (AND Logic)

Returns true only if item has ALL specified flags:

```csharp
public static bool HasAllTypes(this ItemObject item, ItemTypes types)
{
    if (types == ItemTypes.None) return true;
    var itemTypes = item.GetItemTypes();
    return (itemTypes & types) == types;  // Bitwise AND equals types
}
```

Example:
```
Query: weapon 1h
Required: Weapon | OneHanded
Item has: Weapon | OneHanded | Combat ✓ MATCH
Item has: Weapon | TwoHanded | Combat ✗ NO MATCH
```

### HasAnyType (OR Logic)

Returns true if item has ANY of the specified flags:

```csharp
public static bool HasAnyType(this ItemObject item, ItemTypes types)
{
    if (types == ItemTypes.None) return true;
    var itemTypes = item.GetItemTypes();
    return (itemTypes & types) != ItemTypes.None;  // Any overlap
}
```

Example:
```
Query: bow crossbow (using item_any)
Required: Bow | Crossbow
Item has: Weapon | Ranged | Bow | Combat ✓ MATCH
Item has: Weapon | Ranged | Crossbow | Combat ✓ MATCH
Item has: Weapon | TwoHanded | Combat ✗ NO MATCH
```

## Adding New Categories

To add a new item category:

1. **Add enum flag** in `ItemExtensions.cs`:
```csharp
MagicItem = 33554432,  // Next power of 2
```

2. **Update GetItemTypes()** to detect and assign:
```csharp
if (item.HasMagicProperties())
    types |= ItemTypes.MagicItem;
```

3. **Add keyword** in `ItemQueryCommands.cs`:
```csharp
var typeKeywords = new HashSet<string> 
{ 
    ..., 
    "magic" 
};
```

4. **Update aliases** in `ItemQueries.ParseItemType()` if needed:
```csharp
var normalized = typeString.ToLower() switch
{
    ...,
    "enchanted" => "MagicItem",
    _ => typeString
};
```

## Performance Considerations

### Efficient Filtering Order
1. Text search first (reduces dataset quickly)
2. Type filtering (fast bitwise operations)
3. Tier filtering (simple integer comparison)
4. Sorting last (operates on filtered set)

### Caching Opportunities
- Item types are computed on-demand but could be cached
- Sort operations could maintain sorted lists
- Consider lazy evaluation for large datasets

## Testing

Test cases should cover:

1. **Type Detection**
   - All item types correctly assigned flags
   - Civilian/combat distinction works
   - Bow/crossbow separation works

2. **Filtering**
   - Text search matches name/ID/tier
   - Type filtering with AND logic
   - Type filtering with OR logic
   - Tier filtering accuracy

3. **Sorting**
   - All sort fields work
   - Ascending/descending order correct
   - Default sorting (by ID)

4. **Combined Operations**
   - Multiple filters together
   - Filters + sorting
   - Edge cases (no results, all items, etc.)

## Future Enhancements

Possible additions:
- **Range filtering**: tier range, value range, weight range
- **Multi-field sorting**: Primary and secondary sort keys
- **Culture filtering**: Filter by item culture
- **Stat filtering**: Damage range, armor value range
- **Regex support**: Advanced pattern matching
- **Save queries**: Reusable named queries

---

**Navigation:** [← Back to Implementation](../README.md)