# Troop Query System Implementation

**Date:** 2025-12-16  
**Type:** Feature - New System  
**Scope:** Troop Entity Querying and Filtering

## Overview

Implemented a comprehensive Troop Query System that enables querying, filtering, and sorting of troops (CharacterObject entities - nameless units, NOT heroes/lords). The system provides 30 distinct type flags organized into five categories: Formation (5), Troop Line (4), Equipment (6), Tier (7), and Culture (8). This system follows the established patterns from Hero, Item, and Clan query systems and provides both AND and OR logic querying capabilities.

**CRITICAL FEATURE:** Heroes and Lords are ALWAYS automatically excluded from troop queries. The system returns only troops (nameless units) and never includes named characters. This exclusion happens at multiple levels to ensure complete separation.

## Changes Made

### 1. TroopTypes Enum

**File:** [`Bannerlord.GameMaster/Troops/TroopExtensions.cs`](Bannerlord.GameMaster/Troops/TroopExtensions.cs:13)

**Implementation:**
- 33 distinct flags using powers of 2 (2^0 through 2^32) for efficient bitwise operations
- Uses `long` as underlying type (instead of `int`) to accommodate all flags
- Organized into five logical categories for intuitive usage

**Categories:**
1. **Formation/Combat Roles (5 flags):** Infantry, Ranged, Cavalry, HorseArcher, Mounted
2. **Troop Line (7 flags):** Regular, Noble, Militia, Mercenary, Caravan, Peasant, MinorFaction
3. **Equipment-Based (6 flags):** Shield, TwoHanded, Polearm, Bow, Crossbow, ThrowingWeapon
4. **Tier-Based (7 flags):** Tier0, Tier1, Tier2, Tier3, Tier4, Tier5, Tier6Plus
5. **Culture-Based (8 flags):** Empire, Vlandia, Sturgia, Aserai, Khuzait, Battania, Nord, Bandit

**New Flags Added:**
- **Caravan (2^9):** Caravan guards, caravan masters, and armed traders
- **Peasant (2^10):** Combat-capable villagers and townsfolk
- **MinorFaction (2^11):** Minor faction troops (Eleftheroi, Brotherhood of Woods, Hidden Hand, etc.)

### 2. TroopExtensions Implementation

**File:** [`Bannerlord.GameMaster/Troops/TroopExtensions.cs`](Bannerlord.GameMaster/Troops/TroopExtensions.cs:61)

**Core Methods:**
- [`GetTroopTypes()`](Bannerlord.GameMaster/Troops/TroopExtensions.cs:67) - Returns all troop type flags for a CharacterObject
- [`HasAllTypes()`](Bannerlord.GameMaster/Troops/TroopExtensions.cs:185) - Checks if troop has ALL specified flags (AND logic)
- [`HasAnyType()`](Bannerlord.GameMaster/Troops/TroopExtensions.cs:195) - Checks if troop has ANY specified flags (OR logic)
- [`FormattedDetails()`](Bannerlord.GameMaster/Troops/TroopExtensions.cs:205) - Returns formatted troop information string

**Equipment Detection Helpers:**
- [`HasShield()`](Bannerlord.GameMaster/Troops/TroopExtensions.cs:219) - Checks for shield in equipment
- [`HasWeaponType()`](Bannerlord.GameMaster/Troops/TroopExtensions.cs:240) - Checks for specific weapon item type
- [`HasWeaponClass()`](Bannerlord.GameMaster/Troops/TroopExtensions.cs:260) - Checks for specific weapon class
- [`HasTwoHandedWeapon()`](Bannerlord.GameMaster/Troops/TroopExtensions.cs:283) - Checks for two-handed weapons
- [`HasPolearm()`](Bannerlord.GameMaster/Troops/TroopExtensions.cs:290) - Checks for polearm weapons
- [`IsMounted()`](Bannerlord.GameMaster/Troops/TroopExtensions.cs:299) - Checks if troop is cavalry/horse archer

**CRITICAL - Hero/Lord Exclusion:**
- [`GetTroopTypes()`](Bannerlord.GameMaster/Troops/TroopExtensions.cs:70) returns `TroopTypes.None` immediately if `character.IsHero` is true
- This ensures heroes/lords can never match any troop query criteria

**Interface Wrapper:**
- [`TroopExtensionsWrapper`](Bannerlord.GameMaster/Troops/TroopExtensions.cs:309) implements `IEntityExtensions<CharacterObject, TroopTypes>` for consistency

### 3. TroopQueries Implementation

**File:** [`Bannerlord.GameMaster/Troops/TroopQueries.cs`](Bannerlord.GameMaster/Troops/TroopQueries.cs:14)

**Core Methods:**
- [`GetTroopById()`](Bannerlord.GameMaster/Troops/TroopQueries.cs:19) - Retrieves specific troop by ID
- [`QueryTroops()`](Bannerlord.GameMaster/Troops/TroopQueries.cs:35) - Main query method with filtering and sorting
- [`ApplySorting()`](Bannerlord.GameMaster/Troops/TroopQueries.cs:80) - Applies sorting to query results
- [`ParseTroopType()`](Bannerlord.GameMaster/Troops/TroopQueries.cs:128) - Parses type string with alias support
- [`ParseTroopTypes()`](Bannerlord.GameMaster/Troops/TroopQueries.cs:147) - Parses multiple type strings
- [`GetFormattedDetails()`](Bannerlord.GameMaster/Troops/TroopQueries.cs:162) - Formats query results

**CRITICAL - Hero/Lord Exclusion:**
- [`QueryTroops()`](Bannerlord.GameMaster/Troops/TroopQueries.cs:47) filters out `IsHero=true` as the FIRST operation after getting all CharacterObject entities
- This double-guards against any heroes/lords appearing in results

**Sorting Support:**
- Standard fields: `id`, `name`, `tier`, `level`, `culture`, `occupation`, `formation`
- Any `TroopTypes` flag can be used for sorting (sorts by whether troop has that flag)
- Ascending and descending order support

**Alias Support:**
- `2h` → TwoHanded
- `cav` → Cavalry  
- `ha` → HorseArcher
- `mounted` → Mounted

**Interface Wrapper:**
- [`TroopQueriesWrapper`](Bannerlord.GameMaster/Troops/TroopQueries.cs:173) implements `IEntityQueries<CharacterObject, TroopTypes>` for consistency

### 4. Troop Filtering System (IsActualTroop)

**File:** [`Bannerlord.GameMaster/Troops/TroopExtensions.cs`](Bannerlord.GameMaster/Troops/TroopExtensions.cs:221)

**Purpose:**
Automatically filters out non-troop characters to ensure query results only contain actual military units.

**Implementation:**
The [`IsActualTroop()`](Bannerlord.GameMaster/Troops/TroopExtensions.cs:221) method provides comprehensive filtering with 9 exclusion categories:

1. **Templates/Equipment Sets** - Test characters and equipment templates
2. **Town NPCs** - Merchants, blacksmiths, tavernkeepers, etc. (Tier 0, Level 1)
3. **Notables** - Settlement notables and administrators (Tier 0, Level 1)
4. **Wanderers/Companions** - Recruitable heroes starting with `spc_` or `npc_wanderer`
5. **Children/Teens/Infants** - Age-restricted characters
6. **Practice/Training Dummies** - Combat training targets
7. **Special Characters** - Cutscene, tutorial, and test characters
8. **Non-Combat Villagers/Townsfolk** - Base-level civilians (Tier 0, Level 1)
9. **Caravan Leaders/World Leaders** - Non-combat leaders (Tier 0, Level 1)

**Kept as Troops:**
- Regular military troops (tier 1+)
- Militia (tier 2-3)
- Mercenaries (excluding leaders)
- Caravan Guards (caravan_guard, caravan_master)
- Armed Traders (armed_trader, sea_trader)
- Bandits (all tiers)
- Minor faction troops (tier 2+)

**Integration:**
- Automatically applied in [`QueryTroops()`](Bannerlord.GameMaster/Troops/TroopQueries.cs:47) as the first filter
- Ensures clean query results without manual filtering
- Called by [`GetTroopCategory()`](Bannerlord.GameMaster/Troops/TroopExtensions.cs:299) to identify non-troops

### 5. Troop Categorization System (GetTroopCategory)

**File:** [`Bannerlord.GameMaster/Troops/TroopExtensions.cs`](Bannerlord.GameMaster/Troops/TroopExtensions.cs:299)

**Purpose:**
Provides human-readable category labels for troops, enabling quick identification of troop type.

**Implementation:**
The [`GetTroopCategory()`](Bannerlord.GameMaster/Troops/TroopExtensions.cs:299) method returns primary category with priority order:

1. **"Non-Troop"** - Character filtered by IsActualTroop
2. **"Bandit"** - Has Bandit culture flag
3. **"Minor Faction"** - Has MinorFaction flag
4. **"Caravan"** - Has Caravan flag
5. **"Peasant"** - Has Peasant flag
6. **"Noble/Elite"** - Has Noble flag
7. **"Militia"** - Has Militia flag
8. **"Mercenary"** - Has Mercenary flag
9. **"Regular"** - Has Regular flag
10. **"Unknown"** - Fallback for unrecognized troops

**Example Categories:**
- Imperial Legionary → "Regular"
- Vlandian Knight → "Noble/Elite"
- Empire Militia Veteran → "Militia"
- Caravan Guard → "Caravan"
- Forest Bandit → "Bandit"
- Eleftheroi Warrior → "Minor Faction"

### 6. FormattedDetails Enhancement

**File:** [`Bannerlord.GameMaster/Troops/TroopExtensions.cs`](Bannerlord.GameMaster/Troops/TroopExtensions.cs:338)

**Changes:**
- Added category display between name and tier: `[{category}]`
- Uses [`GetTroopCategory()`](Bannerlord.GameMaster/Troops/TroopExtensions.cs:299) for consistent categorization
- Changed from `Tier` property to `GetBattleTier()` method for accuracy

**Old Format:**
```
imperial_legionary	Imperial Legionary	Tier: 4	Level: 21	Culture: Empire	Formation: Infantry
```

**New Format:**
```
imperial_legionary	Imperial Legionary	[Regular]	Tier: 4	Level: 21	Culture: Empire	Formation: Infantry
vlandian_knight	Vlandian Knight	[Noble/Elite]	Tier: 5	Level: 28	Culture: Vlandia	Formation: Cavalry
caravan_guard	Caravan Guard	[Caravan]	Tier: 2	Level: 15	Culture: Empire	Formation: Infantry
```

**Benefits:**
- Quick visual identification of troop type
- Consistent categorization across all commands
- Better organization when viewing query results

### 7. Console Commands Implementation

**File:** [`Bannerlord.GameMaster/Console/Query/TroopQueryCommands.cs`](Bannerlord.GameMaster/Console/Query/TroopQueryCommands.cs:13)

**Commands:**
1. [`gm.query.troop`](Bannerlord.GameMaster/Console/Query/TroopQueryCommands.cs:157) - Query with AND logic (all specified types must match)
2. [`gm.query.troop_any`](Bannerlord.GameMaster/Console/Query/TroopQueryCommands.cs:195) - Query with OR logic (any specified type must match)
3. [`gm.query.troop_info`](Bannerlord.GameMaster/Console/Query/TroopQueryCommands.cs:228) - Get detailed info about specific troop

**Helper Methods:**
- [`ParseArguments()`](Bannerlord.GameMaster/Console/Query/TroopQueryCommands.cs:18) - Parses command arguments into query components
- [`ParseSortParameter()`](Bannerlord.GameMaster/Console/Query/TroopQueryCommands.cs:87) - Extracts sort field and order
- [`ParseTierKeyword()`](Bannerlord.GameMaster/Console/Query/TroopQueryCommands.cs:103) - Converts tier keywords to tier numbers
- [`BuildCriteriaString()`](Bannerlord.GameMaster/Console/Query/TroopQueryCommands.cs:122) - Builds readable criteria description

**Integration:**
- Full integration with [`Cmd.Run()`](Bannerlord.GameMaster/Console/Query/TroopQueryCommands.cs:159) for error handling
- Proper [`ValidateCampaignMode()`](Bannerlord.GameMaster/Console/Query/TroopQueryCommands.cs:161) checks
- Hero/lord detection in `troop_info` command with helpful error message

## Usage Examples

### Basic Queries

```
# Get all troops (excluding heroes/lords automatically)
gm.query.troop

# Find troops with "imperial" in name or ID
gm.query.troop imperial

# Get detailed info about specific troop
gm.query.troop_info imperial_legionary
```

### Formation Filtering

```
# Find all infantry troops
gm.query.troop infantry

# Find all ranged troops
gm.query.troop ranged

# Find all cavalry troops
gm.query.troop cavalry

# Find all horse archer troops
gm.query.troop horsearcher

# Find all mounted troops (cavalry + horse archers)
gm.query.troop mounted
```

### Equipment Filtering

```
# Find troops with shields
gm.query.troop shield

# Find troops with bows
gm.query.troop bow

# Find troops with crossbows
gm.query.troop crossbow

# Find infantry with shields (AND logic)
gm.query.troop infantry shield

# Find troops with bow OR crossbow (OR logic)
gm.query.troop_any bow crossbow
```

### Tier Filtering

```
# Find tier 1 troops
gm.query.troop tier1

# Find tier 3 troops
gm.query.troop tier3

# Find tier 5 troops
gm.query.troop tier5

# Find tier 6+ troops
gm.query.troop tier6plus
```

### Culture Filtering

```
# Find all empire troops
gm.query.troop empire

# Find all vlandian troops
gm.query.troop vlandia

# Find all aserai troops
gm.query.troop aserai

# Find all battanian troops
gm.query.troop battania

# Find empire OR vlandia troops
gm.query.troop_any empire vlandia
```

### Combined Filters

```
# Find empire infantry (both must match)
gm.query.troop empire infantry

# Find aserai cavalry tier 3
gm.query.troop aserai cavalry tier3

# Find battanian ranged troops with bows
gm.query.troop battania ranged bow

# Find shield infantry from empire
gm.query.troop shield infantry empire

# Find noble vlandian cavalry
gm.query.troop noble vlandia cavalry
```

### Sorting Examples

```
# Sort all troops by name
gm.query.troop sort:name

# Sort troops by tier descending
gm.query.troop sort:tier:desc

# Sort infantry by level
gm.query.troop infantry sort:level

# Sort cavalry by culture
gm.query.troop cavalry sort:culture

# Sort by formation type
gm.query.troop sort:formation

# Sort by whether they're mounted
gm.query.troop sort:mounted

# Sort tier 5 troops by value (using type flag)
gm.query.troop tier5 sort:tier:desc
```

### OR Logic Examples

```
# Find cavalry OR ranged troops
gm.query.troop_any cavalry ranged

# Find bow OR crossbow troops
gm.query.troop_any bow crossbow

# Find empire OR vlandia infantry
gm.query.troop_any empire vlandia infantry

# Find tier 4 or tier 5 cavalry
gm.query.troop_any tier4 tier5 cavalry
```

### Detailed Info

```
# Get full details about a specific troop
gm.query.troop_info battanian_highborn_warrior

# Output includes:
# - ID, Name, Tier, Level
# - Culture, Occupation, Formation
# - Type flags (all matching categories)
# - Equipment list
# - Upgrade paths
```

## Type Flags Reference

### Formation/Combat Roles (5 Flags)
- **Infantry** (2^0) - FormationClass: Infantry
- **Ranged** (2^1) - FormationClass: Ranged  
- **Cavalry** (2^2) - FormationClass: Cavalry
- **HorseArcher** (2^3) - FormationClass: HorseArcher
- **Mounted** (2^4) - IsMounted (Cavalry or HorseArcher)

### Troop Line Categories (4 Flags)
- **Regular** (2^5) - Culture's regular/main troop line
- **Noble** (2^6) - Culture's noble/elite troop line
- **Militia** (2^7) - Culture's militia (garrison) troop line
- **Mercenary** (2^8) - Mercenary/Caravan guard troops

### Equipment-Based Categories (6 Flags)
- **Shield** (2^9) - Has shield in equipment
- **TwoHanded** (2^10) - Has two-handed weapon
- **Polearm** (2^11) - Has polearm weapon
- **Bow** (2^12) - Has bow
- **Crossbow** (2^13) - Has crossbow
- **ThrowingWeapon** (2^14) - Has throwing weapon

### Tier-Based Categories (7 Flags)
- **Tier0** (2^15) - Tier 0 troops (recruits)
- **Tier1** (2^16) - Tier 1 troops
- **Tier2** (2^17) - Tier 2 troops
- **Tier3** (2^18) - Tier 3 troops
- **Tier4** (2^19) - Tier 4 troops
- **Tier5** (2^20) - Tier 5 troops
- **Tier6Plus** (2^21) - Tier 6+ troops (includes tier 7 if modded)

### Culture-Based Categories (8 Flags)
- **Empire** (2^22) - Culture: Empire
- **Vlandia** (2^23) - Culture: Vlandia
- **Sturgia** (2^24) - Culture: Sturgia
- **Aserai** (2^25) - Culture: Aserai
- **Khuzait** (2^26) - Culture: Khuzait
- **Battania** (2^27) - Culture: Battania
- **Nord** (2^28) - Culture: Nord (Warsails DLC - optional)
- **Bandit** (2^29) - Culture: Bandit (special culture)

## Technical Details

### Data Source
- **Primary:** `MBObjectManager.Instance.GetObjectTypeList<CharacterObject>()`
- Returns all CharacterObject entities in the game
- Includes both troops and heroes/lords initially

### Hero/Lord Exclusion Strategy (CRITICAL)

This is the most important technical feature of the Troop Query System:

**Two-Level Exclusion:**
1. **Extension Level:** [`GetTroopTypes()`](Bannerlord.GameMaster/Troops/TroopExtensions.cs:70) returns `TroopTypes.None` immediately if `character.IsHero` is true
2. **Query Level:** [`QueryTroops()`](Bannerlord.GameMaster/Troops/TroopQueries.cs:47) filters `.Where(t => !t.IsHero)` as first operation

**Why This Matters:**
- CharacterObject represents BOTH troops (nameless units) AND heroes/lords in Bannerlord
- Users expect troop queries to return only troops, never named characters
- The double-guard ensures no heroes/lords can ever appear in troop query results
- Even if someone tries to get hero info via `troop_info`, it provides a helpful error message

### Equipment Detection
- Uses `BattleEquipments[0]` to access the first battle equipment set
- Iterates through 12 equipment slots to detect weapons, shields, etc.
- Equipment-based flags (Shield, Bow, Crossbow, etc.) are determined dynamically

### Troop Line Detection
- Primary: Uses `Occupation` enum (Soldier, Mercenary, Garrison)
- Secondary: Analyzes `StringId` patterns for noble indicators (knight, cataphract, druzhnik, noble)
- Militia detection via StringId patterns (militia, guard)

### Tier Mapping
- Direct mapping from `character.Tier` property
- Exact tier flags (Tier0-Tier5)
- Tier6Plus encompasses tier 6 and higher (modded games may have tier 7+)

### Culture Detection
- Uses `character.Culture.StringId` for matching
- Case-insensitive substring matching
- Supports all main cultures plus Nord (DLC) and Bandit special culture

### Sorting Implementation
- Default sort: `id` (ascending)
- Type flag sorting: Orders by boolean flag presence
- Standard field sorting: Direct property comparison
- Descending order: Reverses the comparison order

## Testing

### Comprehensive Test Coverage

**File:** [`Bannerlord.GameMaster/Console/Testing/StandardTests.cs`](Bannerlord.GameMaster/Console/Testing/StandardTests.cs:527)

**Total Test Cases:** 42 comprehensive test cases covering all functionality

**Test Categories:**

1. **Basic Query Tests (6 tests)**
   - Query without parameters
   - Formation filtering
   - Equipment filtering  
   - OR logic queries
   - Info command validation
   - Invalid ID handling

2. **Formation Type Tests (4 tests)**
   - Infantry queries
   - Ranged queries
   - Cavalry queries
   - Horse archer queries

3. **Equipment Type Tests (6 tests)**
   - Shield detection
   - Bow detection
   - Crossbow detection
   - Two-handed weapons
   - Polearms
   - Throwing weapons

4. **Tier Filtering Tests (4 tests)**
   - Tier 1 filtering
   - Tier 3 filtering
   - Tier 5 filtering
   - Tier 6+ filtering

5. **Culture Tests (7 tests)**
   - Empire troops
   - Vlandia troops
   - Sturgia troops
   - Aserai troops
   - Khuzait troops
   - Battania troops
   - Bandit troops

6. **Troop Line Tests (4 tests)**
   - Regular troops
   - Noble troops
   - Militia troops
   - Mercenary troops

7. **Combined Filter Tests (4 tests)**
   - Multiple type filtering
   - Culture + formation
   - Equipment + formation + sort
   - Complex multi-filter queries

8. **Sorting Tests (4 tests)**
   - Sort by name
   - Sort by tier descending
   - Sort by level
   - Sort by culture

9. **OR Logic Tests (3 tests)**
   - Formation OR queries
   - Equipment OR queries
   - Culture OR queries with additional filters

**Test Validation:**
- All tests use proper `TestExpectation` types
- Error cases validate proper error messages
- Success cases validate output contains expected text
- Tests cover both positive and negative scenarios

## Benefits

1. **Comprehensive Troop Discovery:** 30 type flags across 5 categories enable precise troop identification
2. **Hero/Lord Safety:** Automatic exclusion prevents confusion between troops and named characters
3. **Flexible Querying:** Both AND and OR logic support for complex queries
4. **Equipment Intelligence:** Dynamic equipment detection provides useful filtering capabilities
5. **Sorting Flexibility:** Sort by standard fields or type flags for organized results
6. **Pattern Consistency:** Follows established Hero/Clan/Item query patterns for familiarity
7. **Tier-Based Planning:** Easy identification of troops by upgrade tier for army composition
8. **Culture-Specific Queries:** Quickly find troops from specific cultures for thematic armies
9. **Testing Coverage:** 42 comprehensive tests ensure reliability and catch regressions
10. **Documentation:** Complete user and developer documentation for easy adoption

## Backwards Compatibility

- **All changes are additive:** No existing functionality was modified or removed
- **No breaking changes:** Existing commands and queries continue to work unchanged
- **Follows established patterns:** Users familiar with Hero/Clan/Item queries will find troop queries intuitive
- **Optional feature:** System can be used or ignored without affecting other functionality
- **Safe implementation:** Hero/lord exclusion ensures no unintended behavior

## Integration Points

### IEntityExtensions and IEntityQueries Interfaces
- [`TroopExtensionsWrapper`](Bannerlord.GameMaster/Troops/TroopExtensions.cs:309) implements `IEntityExtensions<CharacterObject, TroopTypes>`
- [`TroopQueriesWrapper`](Bannerlord.GameMaster/Troops/TroopQueries.cs:173) implements `IEntityQueries<CharacterObject, TroopTypes>`
- Provides consistent interface with Hero, Clan, Item, and Kingdom systems

### CommandBase Integration
- Uses [`Cmd.Run()`](Bannerlord.GameMaster/Console/Query/TroopQueryCommands.cs:159) for error handling
- Implements [`ValidateCampaignMode()`](Bannerlord.GameMaster/Console/Query/TroopQueryCommands.cs:161) checks
- Follows standard command attribute decoration pattern
- Consistent error messaging and result formatting

### Testing Framework Integration
- Integrated with [`StandardTests.cs`](Bannerlord.GameMaster/Console/Testing/StandardTests.cs:527)
- 42 test cases registered via `RegisterTroopQueryTests()`
- Uses standard `TestCase` and `TestExpectation` patterns
- Categorized as "TroopQuery" for organized test execution

## Documentation

### Developer Documentation
- **Implementation Guide:** [`docs/implementation/troop-query-implementation.md`](docs/implementation/troop-query-implementation.md)
  - Detailed technical implementation notes
  - Architecture decisions and rationale
  - Code examples and best practices

### User Documentation  
- **User Guide:** [`wiki/Bannerlord.GameMaster.wiki/Troop-Query-Commands.md`](wiki/Bannerlord.GameMaster.wiki/Troop-Query-Commands.md)
  - Command usage examples
  - Common use cases and workflows
  - Troubleshooting guidance

### Architecture Documentation
- **Architecture Plan:** [`plans/TroopQuerySystem_Architecture.md`](plans/TroopQuerySystem_Architecture.md)
  - System design and component relationships
  - Type flag organization rationale
  - Integration with existing systems

## Future Enhancements

Potential improvements for consideration:

1. **Weapon Proficiency Filtering:** Filter troops by skill levels in specific weapon types
2. **Role-Based Queries:** Define meta-roles (e.g., "tank", "damage dealer") for tactical queries
3. **Upgrade Path Analysis:** Query troops by their upgrade destinations
4. **Formation AI Optimization:** Query optimal troop compositions for specific battle scenarios
5. **Multi-Field Sorting:** Sort by multiple criteria (e.g., tier then name)
6. **Custom Troop Sets:** Save and reuse frequently used query combinations
7. **Export Capabilities:** Export query results to CSV or JSON for external analysis
8. **Battle Simulation Integration:** Use query results to simulate battles with specific troop compositions
9. **Mod Support:** Detect and include modded cultures and troop types
10. **Performance Metrics:** Display average stats for queried troop groups

## Notes

- **Heroes/Lords are NEVER included:** This is enforced at multiple levels and is not configurable
- **CharacterObject Semantics:** In Bannerlord's API, CharacterObject represents both troops and heroes; our system correctly distinguishes them
- **Equipment Detection:** Based on first battle equipment set; civilian equipment is not considered
- **Tier 6+:** Includes tier 6 and any higher tiers from mods (tier 7, etc.)
- **Alias Support:** Common shortcuts like "2h", "cav", "ha" work in queries
- **Sort Performance:** Sorting is efficient due to indexed properties and enumeration ordering
- **Culture Detection:** Case-insensitive and uses substring matching for flexibility
- **Test Coverage:** 42 tests provide comprehensive validation of all features