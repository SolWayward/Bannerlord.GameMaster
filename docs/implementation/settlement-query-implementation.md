# Settlement Query System Implementation Guide

**Navigation:** [← Back to Implementation](../README.md)

---

## Overview

This guide explains the implementation of the Settlement Query System, which provides comprehensive filtering, categorization, and sorting capabilities for settlements (towns, castles, cities, villages, hideouts) in Bannerlord.

### Purpose and Use Cases

- **Settlement Discovery**: Find settlements matching specific criteria (e.g., "all Empire castles with high prosperity")
- **Strategic Analysis**: Analyze settlement distributions by culture, type, and prosperity
- **Ownership Tracking**: Query player-owned or enemy settlements
- **State Monitoring**: Track besieged or raided settlements

### Key Features

- **18 Settlement Type Flags**: Multi-dimensional categorization covering type, culture, ownership, state, and prosperity
- **Prosperity Detection**: Automatic detection of prosperity levels for towns and villages
- **AND/OR Logic**: Flexible querying with `settlement` (all types must match) and `settlement_any` (any type can match)
- **Multi-field Sorting**: Sort by name, prosperity, owner, kingdom, culture, or any type flag
- **State Tracking**: Track siege and raid status

## Architecture

The Settlement Query System follows the established three-layer architecture pattern:

### Three-Layer Design

1. **Extensions Layer** ([`SettlementExtensions.cs`](../../Bannerlord.GameMaster/Settlements/SettlementExtensions.cs))
   - Defines [`SettlementTypes`](../../Bannerlord.GameMaster/Settlements/SettlementExtensions.cs:10) enum with 18 flags
   - Implements type detection via [`GetSettlementTypes()`](../../Bannerlord.GameMaster/Settlements/SettlementExtensions.cs:36)
   - Provides type checking: [`HasAllTypes()`](../../Bannerlord.GameMaster/Settlements/SettlementExtensions.cs:131), [`HasAnyType()`](../../Bannerlord.GameMaster/Settlements/SettlementExtensions.cs:140)

2. **Queries Layer** ([`SettlementQueries.cs`](../../Bannerlord.GameMaster/Settlements/SettlementQueries.cs))
   - Implements filtering via [`QuerySettlements()`](../../Bannerlord.GameMaster/Settlements/SettlementQueries.cs:31)
   - Handles prosperity-based filtering and sorting
   - Returns filtered and sorted results

3. **Commands Layer** ([`SettlementQueryCommands.cs`](../../Bannerlord.GameMaster/Console/Query/SettlementQueryCommands.cs))
   - Parses user input
   - Validates arguments
   - Formats output
   - Exposes three console commands: `settlement`, `settlement_any`, `settlement_info`

### Data Source

```csharp
// Access all Settlement entities
Settlement.All

// Get specific settlement by ID
Settlement.All.FirstOrDefault(s => s.StringId.Equals(id, StringComparison.OrdinalIgnoreCase))
```

## SettlementTypes Enum

The [`SettlementTypes`](../../Bannerlord.GameMaster/Settlements/SettlementExtensions.cs:10) enum uses flag values (powers of 2) organized into logical categories:

### Settlement Type Flags (1-16)

```csharp
Town = 1,              // 2^0  - General town (castle or city)
Castle = 2,            // 2^1  - Castle specifically
City = 4,              // 2^2  - City specifically
Village = 8,           // 2^3  - Village
Hideout = 16,          // 2^4  - Bandit hideout
```

### Ownership Flags (32)

```csharp
PlayerOwned = 32,      // 2^5  - Owned by player clan
```

### State Flags (64-128)

```csharp
Besieged = 64,         // 2^6  - Currently under siege
Raided = 128,          // 2^7  - Village is being raided
```

### Culture Flags (256-16384)

```csharp
Empire = 256,          // 2^8  - Empire culture
Vlandia = 512,         // 2^9  - Vlandia culture
Sturgia = 1024,        // 2^10 - Sturgia culture
Aserai = 2048,         // 2^11 - Aserai culture
Khuzait = 4096,        // 2^12 - Khuzait culture
Battania = 8192,       // 2^13 - Battania culture
Nord = 16384,          // 2^14 - Nord culture (Warsails DLC)
```

### Prosperity Level Flags (32768-131072)

```csharp
LowProsperity = 32768,     // 2^15 - Prosperity < 3000 (or Hearth < 300 for villages)
MediumProsperity = 65536,  // 2^16 - Prosperity 3000-6000 (or Hearth 300-600)
HighProsperity = 131072,   // 2^17 - Prosperity > 6000 (or Hearth > 600)
```

### Flag Logic

Settlements can have multiple flags set simultaneously:

```csharp
// Seonon (Empire City) has: City | Town | Empire | HighProsperity
// Husn Fulq (Aserai Castle) has: Castle | Town | Aserai | MediumProsperity
// Danustica (Empire Village) has: Village | Empire | LowProsperity
// Player-owned castle under siege has: Castle | Town | PlayerOwned | Besieged
```

## SettlementExtensions Implementation

### GetSettlementTypes() Method

The core type detection method that analyzes settlements across multiple dimensions:

```csharp
public static SettlementTypes GetSettlementTypes(this Settlement settlement)
{
    SettlementTypes types = SettlementTypes.None;

    // 1. Settlement type categories
    if (settlement.IsTown)
    {
        types |= SettlementTypes.Town;
        if (settlement.IsCastle)
            types |= SettlementTypes.Castle;
        else
            types |= SettlementTypes.City;
    }
    else if (settlement.IsVillage)
    {
        types |= SettlementTypes.Village;
    }
    else if (settlement.IsHideout)
    {
        types |= SettlementTypes.Hideout;
    }

    // 2. Ownership
    if (settlement.OwnerClan == Hero.MainHero.Clan)
    {
        types |= SettlementTypes.PlayerOwned;
    }

    // 3. State flags
    if (settlement.IsUnderSiege)
    {
        types |= SettlementTypes.Besieged;
    }
    
    if (settlement.IsVillage && settlement.Village?.VillageState == Village.VillageStates.BeingRaided)
    {
        types |= SettlementTypes.Raided;
    }

    // 4. Culture detection
    if (settlement.Culture != null)
    {
        string cultureId = settlement.Culture.StringId.ToLower();
        if (cultureId.Contains("empire"))
            types |= SettlementTypes.Empire;
        // ... other cultures
    }

    // 5. Prosperity levels
    if (settlement.IsTown && settlement.Town != null)
    {
        float prosperity = settlement.Town.Prosperity;
        if (prosperity < 3000)
            types |= SettlementTypes.LowProsperity;
        else if (prosperity <= 6000)
            types |= SettlementTypes.MediumProsperity;
        else
            types |= SettlementTypes.HighProsperity;
    }
    else if (settlement.IsVillage && settlement.Village != null)
    {
        float hearth = settlement.Village.Hearth;
        if (hearth < 300)
            types |= SettlementTypes.LowProsperity;
        else if (hearth <= 600)
            types |= SettlementTypes.MediumProsperity;
        else
            types |= SettlementTypes.HighProsperity;
    }

    return types;
}
```

**Critical Considerations:**

- **Town vs Castle vs City**: Use `IsCastle` within `IsTown` to distinguish
- **Village State**: Check `VillageState` enum for raid status
- **Prosperity vs Hearth**: Towns use Prosperity, villages use Hearth (different scales)
- **Culture Detection**: Uses `StringId` pattern matching (case-insensitive)
- **Null Checks**: Always check for null before accessing properties

### Type Checking Methods

#### HasAllTypes (AND Logic)

Returns true only if settlement has ALL specified flags:

```csharp
public static bool HasAllTypes(this Settlement settlement, SettlementTypes types)
{
    if (types == SettlementTypes.None) return true;
    var settlementTypes = settlement.GetSettlementTypes();
    return (settlementTypes & types) == types;
}
```

Example:
```
Query: castle empire high
Required: Castle | Empire | HighProsperity
Settlement has: Castle | Town | Empire | HighProsperity ✓ MATCH
Settlement has: City | Town | Empire | HighProsperity ✗ NO MATCH (not castle)
```

#### HasAnyType (OR Logic)

Returns true if settlement has ANY of the specified flags:

```csharp
public static bool HasAnyType(this Settlement settlement, SettlementTypes types)
{
    if (types == SettlementTypes.None) return true;
    var settlementTypes = settlement.GetSettlementTypes();
    return (settlementTypes & types) != SettlementTypes.None;
}
```

Example:
```
Query: castle city (using settlement_any)
Required: Castle | City
Settlement has: Castle | Town | Empire ✓ MATCH
Settlement has: City | Town | Vlandia ✓ MATCH
Settlement has: Village | Battania ✗ NO MATCH
```

### FormattedDetails() Method

Returns tab-delimited string for display:

```csharp
public static string FormattedDetails(this Settlement settlement)
{
    string settlementType = settlement.IsTown 
        ? (settlement.IsCastle ? "Castle" : "City")
        : settlement.IsVillage ? "Village" 
        : settlement.IsHideout ? "Hideout" 
        : "Unknown";

    string ownerName = settlement.OwnerClan?.Name?.ToString() ?? "None";
    string kingdomName = settlement.MapFaction?.Name?.ToString() ?? "None";
    string cultureName = settlement.Culture?.Name?.ToString() ?? "None";

    string prosperityInfo = "";
    if (settlement.IsTown && settlement.Town != null)
    {
        prosperityInfo = $"Prosperity: {settlement.Town.Prosperity:F0}";
    }
    else if (settlement.IsVillage && settlement.Village != null)
    {
        prosperityInfo = $"Hearth: {settlement.Village.Hearth:F0}";
    }

    return $"{settlement.StringId}\t{settlement.Name}\t[{settlementType}]\tOwner: {ownerName}\t" +
           $"Kingdom: {kingdomName}\tCulture: {cultureName}\t{prosperityInfo}";
}
```

**Output Example:**
```
town_empire_1	Seonon	[City]	Owner: Clan Empire South	Kingdom: Empire	Culture: Empire	Prosperity: 7500
castle_vlandia_2	Epicrotea	[Castle]	Owner: Clan Vlandia 2	Kingdom: Vlandia	Culture: Vlandia	Prosperity: 4200
village_battania_3	Pen Cannoc	[Village]	Owner: Clan Battania 1	Kingdom: Battania	Culture: Battania	Hearth: 450
```

## SettlementQueries Implementation

### GetSettlementById() Method

Direct lookup by StringId:

```csharp
public static Settlement GetSettlementById(string settlementId)
{
    return Settlement.All.FirstOrDefault(s => 
        s.StringId.Equals(settlementId, StringComparison.OrdinalIgnoreCase));
}
```

Returns `null` if not found.

### QuerySettlements() Method

Main query method with comprehensive filtering:

```csharp
public static List<Settlement> QuerySettlements(
    string query = "",
    SettlementTypes requiredTypes = SettlementTypes.None,
    bool matchAll = true,
    string sortBy = "id",
    bool sortDescending = false)
{
    IEnumerable<Settlement> settlements = Settlement.All;

    // Filter by name/ID if provided
    if (!string.IsNullOrEmpty(query))
    {
        string lowerFilter = query.ToLower();
        settlements = settlements.Where(s =>
            s.Name.ToString().ToLower().Contains(lowerFilter) ||
            s.StringId.ToLower().Contains(lowerFilter));
    }

    // Filter by types
    if (requiredTypes != SettlementTypes.None)
    {
        settlements = settlements.Where(s =>
            matchAll ? s.HasAllTypes(requiredTypes) : s.HasAnyType(requiredTypes));
    }

    // Apply sorting
    settlements = ApplySorting(settlements, sortBy, sortDescending);

    return settlements.ToList();
}
```

**Filter Pipeline:**

1. Get all Settlement entities
2. Filter by name/ID substring (case-insensitive)
3. Apply type filtering with AND/OR logic
4. Apply sorting
5. Return results

### ApplySorting() Method

Supports sorting by standard fields and type flags:

```csharp
private static IEnumerable<Settlement> ApplySorting(
    IEnumerable<Settlement> settlements,
    string sortBy,
    bool descending)
{
    sortBy = sortBy.ToLower();

    // Check if sortBy matches a SettlementTypes flag
    if (Enum.TryParse<SettlementTypes>(sortBy, true, out var settlementType) && 
        settlementType != SettlementTypes.None)
    {
        return descending
            ? settlements.OrderByDescending(s => s.GetSettlementTypes().HasFlag(settlementType))
            : settlements.OrderBy(s => s.GetSettlementTypes().HasFlag(settlementType));
    }

    // Sort by standard fields
    IOrderedEnumerable<Settlement> orderedSettlements = sortBy switch
    {
        "name" => descending
            ? settlements.OrderByDescending(s => s.Name.ToString())
            : settlements.OrderBy(s => s.Name.ToString()),
        "prosperity" => descending
            ? settlements.OrderByDescending(s => GetProsperityValue(s))
            : settlements.OrderBy(s => GetProsperityValue(s)),
        // ... other fields
        _ => descending
            ? settlements.OrderByDescending(s => s.StringId)
            : settlements.OrderBy(s => s.StringId)
    };

    return orderedSettlements;
}
```

**Supported Sort Fields:**

- `id` - StringId (default)
- `name` - Display name
- `prosperity` - Prosperity or Hearth value
- `owner` - Owner clan name
- `kingdom` - Kingdom name
- `culture` - Culture name
- Any [`SettlementTypes`](../../Bannerlord.GameMaster/Settlements/SettlementExtensions.cs:10) flag name (sorts by presence of flag)

### Parse Methods

#### ParseSettlementType() with Alias Support

```csharp
public static SettlementTypes ParseSettlementType(string typeString)
{
    var normalized = typeString.ToLower() switch
    {
        "player" => "PlayerOwned",
        "siege" => "Besieged",
        "low" => "LowProsperity",
        "medium" => "MediumProsperity",
        "high" => "HighProsperity",
        _ => typeString
    };

    return Enum.TryParse<SettlementTypes>(normalized, true, out var result)
        ? result : SettlementTypes.None;
}
```

## SettlementQueryCommands Implementation

### Three Console Commands

1. **`gm.query.settlement`** - Query with AND logic (all types must match)
2. **`gm.query.settlement_any`** - Query with OR logic (any type can match)
3. **`gm.query.settlement_info`** - Detailed info about specific settlement

### Argument Parsing Strategy

The [`ParseArguments()`](../../Bannerlord.GameMaster/Console/Query/SettlementQueryCommands.cs:18) method categorizes input:

```csharp
private static (string query, SettlementTypes types, string sortBy, bool sortDesc)
    ParseArguments(List<string> args)
{
    var typeKeywords = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        "town", "castle", "city", "village", "hideout",
        "player", "playerowned",
        "besieged", "siege", "raided",
        "empire", "vlandia", "sturgia", "aserai", "khuzait", "battania", "nord",
        "lowprosperity", "mediumprosperity", "highprosperity",
        "low", "medium", "high"
    };

    // Categorize each argument
    foreach (var arg in args)
    {
        if (arg.StartsWith("sort:"))
            ParseSortParameter(arg, ref sortBy, ref sortDesc);
        else if (typeKeywords.Contains(arg))
            typeTerms.Add(arg);
        else
            searchTerms.Add(arg);
    }

    // Combine results
    string query = string.Join(" ", searchTerms);
    SettlementTypes types = SettlementQueries.ParseSettlementTypes(typeTerms);

    return (query, types, sortBy, sortDesc);
}
```

## Usage Examples

### Basic Queries

```bash
# List all settlements
gm.query.settlement

# List all castles
gm.query.settlement castle

# List all cities
gm.query.settlement city
```

### Culture-Based Queries

```bash
# Empire settlements
gm.query.settlement empire

# Vlandia castles
gm.query.settlement vlandia castle

# Aserai cities with high prosperity
gm.query.settlement aserai city high
```

### State-Based Queries

```bash
# Player-owned settlements
gm.query.settlement player

# Besieged settlements
gm.query.settlement besieged

# Raided villages
gm.query.settlement raided village
```

### Sorting

```bash
# Sort by prosperity descending
gm.query.settlement sort:prosperity:desc

# Sort empire castles by name
gm.query.settlement empire castle sort:name
```

### OR Logic (settlement_any)

```bash
# Castles OR cities
gm.query.settlement_any castle city

# Empire OR Vlandia settlements
gm.query.settlement_any empire vlandia
```

### Detailed Info

```bash
# Get detailed settlement information
gm.query.settlement_info town_empire_1
```

## Best Practices

### Null Checks

Always check for null before accessing properties:

```csharp
// Check Town property
if (settlement.IsTown && settlement.Town != null)
{
    float prosperity = settlement.Town.Prosperity;
}

// Check Village property
if (settlement.IsVillage && settlement.Village != null)
{
    float hearth = settlement.Village.Hearth;
}

// Check owner/kingdom
string ownerName = settlement.OwnerClan?.Name?.ToString() ?? "None";
string kingdomName = settlement.MapFaction?.Name?.ToString() ?? "None";
```

### Performance Considerations

1. **Filter Order**: Simple filters first (name/ID), then expensive ones (type detection)
2. **Lazy Evaluation**: Use LINQ queries that don't enumerate until needed
3. **Single Enumeration**: Convert to List only at the end

## Integration Points

### IEntityExtensions Interface

Implemented by [`SettlementExtensionsWrapper`](../../Bannerlord.GameMaster/Settlements/SettlementExtensions.cs:182):

```csharp
public class SettlementExtensionsWrapper : IEntityExtensions<Settlement, SettlementTypes>
{
    public SettlementTypes GetTypes(Settlement entity) => entity.GetSettlementTypes();
    public bool HasAllTypes(Settlement entity, SettlementTypes types) => entity.HasAllTypes(types);
    public bool HasAnyType(Settlement entity, SettlementTypes types) => entity.HasAnyType(types);
    public string FormattedDetails(Settlement entity) => entity.FormattedDetails();
}
```

### IEntityQueries Interface

Implemented by [`SettlementQueriesWrapper`](../../Bannerlord.GameMaster/Settlements/SettlementQueries.cs:195):

```csharp
public class SettlementQueriesWrapper : IEntityQueries<Settlement, SettlementTypes>
{
    public Settlement GetById(string id) => SettlementQueries.GetSettlementById(id);
    public List<Settlement> Query(string query, SettlementTypes types, bool matchAll) =>
        SettlementQueries.QuerySettlements(query, types, matchAll);
    public SettlementTypes ParseType(string typeString) => SettlementQueries.ParseSettlementType(typeString);
    public SettlementTypes ParseTypes(IEnumerable<string> typeStrings) =>
        SettlementQueries.ParseSettlementTypes(typeStrings);
    public string GetFormattedDetails(List<Settlement> entities) =>
        SettlementQueries.GetFormattedDetails(entities);
}
```

## Testing

The Settlement Query System includes **25 comprehensive test cases** covering:

- Basic queries (all settlements, name search)
- Type filtering (castle, city, village, hideout)
- Culture filtering (all 7 cultures)
- State filtering (player-owned, besieged, raided)
- Prosperity filtering (low, medium, high)
- Combined filters with sorting
- OR logic queries
- Error handling (missing ID, invalid ID)
- Sort validation (all supported fields)

### Running Tests

```bash
gm.test.run_category SettlementQuery
```

## Reference Files

- [`SettlementExtensions.cs`](../../Bannerlord.GameMaster/Settlements/SettlementExtensions.cs) - Extensions layer implementation
- [`SettlementQueries.cs`](../../Bannerlord.GameMaster/Settlements/SettlementQueries.cs) - Queries layer implementation
- [`SettlementQueryCommands.cs`](../../Bannerlord.GameMaster/Console/Query/SettlementQueryCommands.cs) - Commands layer implementation
- [`StandardTests.cs`](../../Bannerlord.GameMaster/Console/Testing/StandardTests.cs) - Test cases (RegisterSettlementQueryTests method)
- [Change Documentation](../../ChangeDocs/Features/SETTLEMENT_QUERY_SYSTEM_2025-12-16.md) - Feature documentation

---

**Navigation:** [← Back to Implementation](../README.md)