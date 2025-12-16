# Settlement Query System Implementation

**Date:** 2025-12-16  
**Type:** Feature  
**Category:** Query System  
**Status:** Completed

---

## Overview

Implemented a comprehensive settlement query system that enables searching, filtering, and querying settlements (towns, castles, cities, villages, hideouts) in Mount & Blade II: Bannerlord with advanced filtering capabilities.

## Changes Made

### 1. Settlement Extensions Layer
**File:** `Bannerlord.GameMaster/Settlements/SettlementExtensions.cs`

#### SettlementTypes Enum (18 Flags)
- **Settlement Type Flags:**
  - `Town` - General town (castle or city)
  - `Castle` - Castle specifically
  - `City` - City specifically
  - `Village` - Village
  - `Hideout` - Bandit hideout

- **Ownership Flags:**
  - `PlayerOwned` - Owned by player clan

- **State Flags:**
  - `Besieged` - Currently under siege
  - `Raided` - Village being raided

- **Culture Flags:**
  - `Empire`, `Vlandia`, `Sturgia`, `Aserai`, `Khuzait`, `Battania`, `Nord`

- **Prosperity Level Flags:**
  - `LowProsperity` - Prosperity < 3000 (or Hearth < 300 for villages)
  - `MediumProsperity` - Prosperity 3000-6000 (or Hearth 300-600)
  - `HighProsperity` - Prosperity > 6000 (or Hearth > 600)

#### Extension Methods
- [`GetSettlementTypes()`](../../Bannerlord.GameMaster/Settlements/SettlementExtensions.cs:36) - Returns all applicable type flags
- [`HasAllTypes()`](../../Bannerlord.GameMaster/Settlements/SettlementExtensions.cs:131) - AND logic type checking
- [`HasAnyType()`](../../Bannerlord.GameMaster/Settlements/SettlementExtensions.cs:140) - OR logic type checking
- [`FormattedDetails()`](../../Bannerlord.GameMaster/Settlements/SettlementExtensions.cs:149) - Tab-delimited output with category

#### Interface Implementation
- [`SettlementExtensionsWrapper`](../../Bannerlord.GameMaster/Settlements/SettlementExtensions.cs:182) implementing `IEntityExtensions<Settlement, SettlementTypes>`

### 2. Settlement Queries Layer
**File:** `Bannerlord.GameMaster/Settlements/SettlementQueries.cs`

#### Query Methods
- [`GetSettlementById()`](../../Bannerlord.GameMaster/Settlements/SettlementQueries.cs:20) - Find by exact ID (case-insensitive)
- [`QuerySettlements()`](../../Bannerlord.GameMaster/Settlements/SettlementQueries.cs:31) - Main query method with:
  - Name/ID substring filtering
  - Type flag filtering (AND/OR logic)
  - Multi-field sorting (id, name, prosperity, owner, kingdom, culture, or any type flag)

#### Parsing Methods
- [`ParseSettlementType()`](../../Bannerlord.GameMaster/Settlements/SettlementQueries.cs:137) - Convert string to single enum value
  - Supports aliases: "player" → "PlayerOwned", "siege" → "Besieged", "low" → "LowProsperity", etc.
- [`ParseSettlementTypes()`](../../Bannerlord.GameMaster/Settlements/SettlementQueries.cs:175) - Combine multiple strings into flags

#### Formatting
- [`GetFormattedDetails()`](../../Bannerlord.GameMaster/Settlements/SettlementQueries.cs:185) - Format settlement list for display

#### Interface Implementation
- [`SettlementQueriesWrapper`](../../Bannerlord.GameMaster/Settlements/SettlementQueries.cs:195) implementing `IEntityQueries<Settlement, SettlementTypes>`

### 3. Settlement Query Commands Layer
**File:** `Bannerlord.GameMaster/Console/Query/SettlementQueryCommands.cs`

#### Console Commands
1. **`gm.query.settlement`** - Query with AND logic
   - All specified type flags must match
   - Supports search terms, type keywords, and sorting
   
2. **`gm.query.settlement_any`** - Query with OR logic
   - Any specified type flag can match
   - Same parameter support as main command
   
3. **`gm.query.settlement_info`** - Detailed settlement information
   - Shows ID, name, type, owner, kingdom, culture
   - Displays prosperity/hearth, security, loyalty (for towns)
   - Shows siege status and notables count

#### Argument Parsing
- Separates search terms from type keywords automatically
- Handles sort parameters: `sort:field` or `sort:field:desc`
- Supports all SettlementTypes values as keywords

### 4. Testing Layer
**File:** `Bannerlord.GameMaster/Console/Testing/StandardTests.cs`

Added 25 comprehensive test cases covering:
- Basic queries (all settlements, name search)
- Type filtering (castle, city, village, hideout)
- Culture filtering (all 7 cultures)
- State filtering (player-owned, besieged, raided)
- Prosperity filtering (low, medium, high)
- Combined filters with sorting
- OR logic queries
- Error handling (missing ID, invalid ID)
- Sort validation (name, prosperity, owner, kingdom, culture)

## Usage Examples

### Basic Queries
```bash
# List all settlements
gm.query.settlement

# Find settlements by name
gm.query.settlement pen

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

# Aserai cities sorted by prosperity
gm.query.settlement aserai city sort:prosperity:desc
```

### State-Based Queries
```bash
# Player-owned settlements
gm.query.settlement player

# Besieged settlements
gm.query.settlement besieged

# High prosperity settlements
gm.query.settlement high
```

### Combined Queries
```bash
# Empire castles sorted by name
gm.query.settlement empire castle sort:name

# High prosperity cities
gm.query.settlement city high sort:prosperity:desc

# Player-owned towns
gm.query.settlement player town
```

### OR Logic Queries
```bash
# Castles OR cities
gm.query.settlement_any castle city

# Empire OR Vlandia settlements
gm.query.settlement_any empire vlandia

# Besieged OR raided settlements
gm.query.settlement_any besieged raided
```

### Detailed Information
```bash
# Get detailed info about a specific settlement
gm.query.settlement_info town_empire_1
```

## Technical Details

### Prosperity Detection
- **Towns:** Uses `Town.Prosperity` property
- **Villages:** Uses `Village.Hearth` property (scaled differently)
- Thresholds:
  - Towns: Low < 3000, Medium 3000-6000, High > 6000
  - Villages: Low < 300, Medium 300-600, High > 600

### Settlement Type Detection
- Uses `IsTown`, `IsCastle`, `IsVillage`, `IsHideout` properties
- Distinguishes between castles and cities within towns
- Handles ownership via `OwnerClan` comparison with `Hero.MainHero.Clan`

### State Detection
- **Besieged:** Uses `IsUnderSiege` property
- **Raided:** Checks village state for `VillageStates.BeingRaided`

### Culture Detection
- Uses `Culture.StringId` with case-insensitive pattern matching
- Supports all base game cultures plus Nord (Warsails DLC)

## Integration Points

- **CommandBase:** Campaign mode validation
- **Cmd.Run():** Command wrapper with logging
- **IEntityExtensions:** Type flag interface implementation
- **IEntityQueries:** Query interface implementation
- **Settlement.All:** Data source for all settlements

## Best Practices Followed

1. **Consistent Naming:**
   - Files: PascalCase
   - Commands: lowercase.with.dots
   - Methods: PascalCase

2. **Error Handling:**
   - Campaign mode validation
   - Argument count validation
   - Clear error messages

3. **Code Organization:**
   - Three-layer architecture (Extensions → Queries → Commands)
   - Interface implementations for generic operations
   - Helper methods for parsing and formatting

4. **Documentation:**
   - XML comments on all public methods
   - Usage examples in error messages
   - Comprehensive test coverage

## Benefits

1. **Powerful Filtering:** 18 different type flags for precise queries
2. **Flexible Search:** Name/ID substring matching with culture/type filtering
3. **Multi-Field Sorting:** Sort by standard fields or type flag presence
4. **AND/OR Logic:** Support for both all-match and any-match queries
5. **Detailed Information:** Comprehensive settlement data in info command
6. **Consistent Interface:** Matches patterns from Hero/Clan/Kingdom/Troop/Item queries

## Testing

All 25 test cases validate:
- Basic query functionality
- Type filtering accuracy
- Culture filtering
- State filtering
- Prosperity level detection
- Sorting functionality
- Combined filter operations
- OR logic operations
- Error handling
- Info command validation

## Future Enhancements

Potential additions:
- Settlement population filtering
- Garrison strength filtering
- Building level queries
- Trade good filtering
- Notable type filtering
- Distance-based queries
- Faction relationship filtering

---

**Related Files:**
- [`SettlementExtensions.cs`](../../Bannerlord.GameMaster/Settlements/SettlementExtensions.cs)
- [`SettlementQueries.cs`](../../Bannerlord.GameMaster/Settlements/SettlementQueries.cs)
- [`SettlementQueryCommands.cs`](../../Bannerlord.GameMaster/Console/Query/SettlementQueryCommands.cs)
- [`StandardTests.cs`](../../Bannerlord.GameMaster/Console/Testing/StandardTests.cs) (RegisterSettlementQueryTests method)