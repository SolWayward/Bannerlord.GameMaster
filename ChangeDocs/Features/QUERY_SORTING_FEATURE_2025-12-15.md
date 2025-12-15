# Query Sorting Feature Implementation

**Date:** 2025-12-15  
**Type:** Feature Enhancement  
**Scope:** Hero, Clan, and Kingdom Query Commands

## Overview

Added comprehensive sorting functionality to Hero, Clan, and Kingdom query commands, matching the existing sorting capabilities of Item queries. Users can now sort query results by various fields and type flags, with both ascending and descending order support.

## Changes Made

### 1. Hero Query Sorting

**Modified Files:**
- `Bannerlord.GameMaster/Heroes/HeroQueries.cs`
- `Bannerlord.GameMaster/Console/Query/HeroQueryCommands.cs`

**New Functionality:**
- Added `sortBy` and `sortDescending` parameters to `QueryHeroes()` method
- Implemented `ApplySorting()` method supporting:
  - Standard fields: `id`, `name`, `age`, `clan`, `kingdom`
  - Type flag sorting: Any `HeroTypes` flag (e.g., `wanderer`, `lord`, `female`)
- Updated command parsers to extract `sort:` parameters
- Enhanced help messages with sorting examples

**Usage Examples:**
```
gm.query.hero sort:name
gm.query.hero sort:age:desc
gm.query.hero lord sort:name
gm.query.hero sort:wanderer (sorts by wanderer flag)
```

### 2. Clan Query Sorting

**Modified Files:**
- `Bannerlord.GameMaster/Clans/ClanQueries.cs`
- `Bannerlord.GameMaster/Console/Query/ClanQueryCommands.cs`

**New Functionality:**
- Added `sortBy` and `sortDescending` parameters to `QueryClans()` method
- Implemented `ApplySorting()` method supporting:
  - Standard fields: `id`, `name`, `tier`, `gold`, `renown`, `kingdom`, `heroes`
  - Type flag sorting: Any `ClanTypes` flag (e.g., `mercenary`, `noble`, `bandit`)
- Updated command parsers to extract `sort:` parameters
- Enhanced help messages with sorting examples

**Usage Examples:**
```
gm.query.clan sort:name
gm.query.clan sort:gold:desc
gm.query.clan noble sort:renown:desc
gm.query.clan sort:mercenary (sorts by mercenary flag)
```

### 3. Kingdom Query Sorting

**Modified Files:**
- `Bannerlord.GameMaster/Kingdoms/KingdomQueries.cs`
- `Bannerlord.GameMaster/Console/Query/KingdomQueryCommands.cs`

**New Functionality:**
- Added `sortBy` and `sortDescending` parameters to `QueryKingdoms()` method
- Implemented `ApplySorting()` method supporting:
  - Standard fields: `id`, `name`, `clans`, `heroes`, `fiefs`, `strength`, `ruler`
  - Type flag sorting: Any `KingdomTypes` flag (e.g., `atwar`, `active`, `eliminated`)
- Updated command parsers to extract `sort:` parameters
- Enhanced help messages with sorting examples
- Fixed property name to use `CurrentTotalStrength` for strength sorting

**Usage Examples:**
```
gm.query.kingdom sort:name
gm.query.kingdom sort:strength:desc
gm.query.kingdom atwar sort:clans:desc
gm.query.kingdom sort:atwar (sorts by atwar flag)
```

## Sort Parameter Format

All queries now support the following sort parameter formats:

- `sort:field` - Sort by field in ascending order (default)
- `sort:field:asc` - Explicitly sort ascending
- `sort:field:desc` - Sort in descending order
- `sort:typename` - Sort by whether entity has a specific type flag

## Default Behavior

- **Default Sort Field:** `id` (ascending)
- **Default Sort Order:** Ascending (`asc`)
- Sorting is applied after all filtering operations
- Type flag sorting returns boolean comparison (entities with flag come first in ascending, last in descending)

## Consistency with Item Queries

This implementation maintains consistency with existing Item query sorting:
- Same parameter syntax (`sort:field:order`)
- Same default behavior (id, ascending)
- Same support for type flag sorting
- Same help message format

## Technical Details

### Sorting Implementation Pattern

Each entity type implements sorting via:
1. `ApplySorting()` private method in the Queries class
2. Type flag detection using `Enum.TryParse<TType>()`
3. Switch expression for standard field sorting
4. Default fallback to ID sorting

### Parse Argument Changes

Updated `ParseArguments()` methods to:
1. Extract search terms, type keywords, and sort parameters separately
2. Parse sort parameters before creating type filters
3. Return tuple including `sortBy` and `sortDesc` values
4. Build criteria strings including sort information

### BuildCriteriaString Enhancement

Updated to include sort information in output:
- Only shows sort info if different from default (`id`)
- Displays sort order as `(asc)` or `(desc)`
- Format: `sort: field (order)`

## Benefits

1. **Enhanced User Experience:** Users can organize query results in meaningful ways
2. **Flexible Sorting:** Support for both standard fields and type flags
3. **Consistency:** Unified sorting syntax across all entity types
4. **Discoverability:** Clear help messages with examples
5. **Power User Features:** Type flag sorting enables advanced filtering strategies

## Backwards Compatibility

- All changes are additive; existing commands work unchanged
- Default behavior (no sort parameter) returns results sorted by ID ascending
- No breaking changes to existing functionality

## Testing Recommendations

1. Test sorting by each supported field for each entity type
2. Verify ascending and descending order work correctly
3. Test type flag sorting (e.g., `sort:wanderer`, `sort:mercenary`)
4. Confirm sort works with complex queries combining search terms and type filters
5. Verify default sorting behavior when no sort parameter is provided
6. Test `_any` variant commands with sorting

## Future Enhancements

Potential improvements for consideration:
- Multi-field sorting (e.g., `sort:clan,name`)
- Numeric range sorting
- Custom sort expressions
- Sort by calculated fields (e.g., troop count, relationship values)