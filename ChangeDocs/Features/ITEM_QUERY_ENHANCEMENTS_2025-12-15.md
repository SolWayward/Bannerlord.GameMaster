# Item Query Enhancements - 2025-12-15

## Overview
Added comprehensive filtering and sorting capabilities to the item query system, including new item type categories, tier filtering, and flexible sorting options.

## New Features

### 1. New Item Type Categories

#### Bow and Crossbow Separation
- **Before**: Both bows and crossbows were grouped under `Ranged`
- **After**: Added separate flags `Bow` and `Crossbow` while maintaining `Ranged` for backward compatibility
- **Usage**: `gm.query.item bow` or `gm.query.item crossbow`

#### Civilian vs Combat Items
- **Civilian**: Items equippable in civilian outfit slots (clothing without armor, trade goods, food)
- **Combat**: Items designed for combat (weapons, armor with protection values)
- **Usage**: `gm.query.item civilian` or `gm.query.item combat`

#### Horse Armor (Mount Armor)
- **New**: `HorseArmor` flag for barding/horse harness items
- **Usage**: `gm.query.item horsearmor`

### 2. Tier Filtering

Items can now be filtered by tier level (0-6):

**Keyword-based filtering:**
```
gm.query.item tier3          # All tier 3 items
gm.query.item sword tier4    # Tier 4 swords
gm.query.item armor tier5    # Tier 5 armor
```

**Search string filtering:**
Items with "tier 3" in their search results will also match tier searches:
```
gm.query.item imperial tier3  # Imperial items at tier 3
```

### 3. Sorting Capabilities

Items can now be sorted by multiple criteria with ascending/descending order:

**Sort Options:**
- `sort:name` - Alphabetical by name
- `sort:tier` - By tier level (0-6)
- `sort:value` - By gold value
- `sort:type` - By item type
- `sort:id` - By string ID (default)

**Sort Direction:**
- Default: Ascending (`:asc`)
- Descending: `:desc`

**Examples:**
```
gm.query.item sort:name              # Sort alphabetically
gm.query.item sort:value:desc        # Most expensive first
gm.query.item sword sort:tier:asc    # Swords by tier (low to high)
gm.query.item armor tier4 sort:name  # Tier 4 armor alphabetically
```

## Implementation Details

### Modified Files

#### ItemExtensions.cs
- Added new `ItemTypes` enum flags:
  - `Bow = 1048576`
  - `Crossbow = 2097152`
  - `Civilian = 4194304`
  - `Combat = 8388608`
  - `HorseArmor = 16777216`

- Updated `GetItemTypes()` method:
  - Differentiates between bow and crossbow
  - Detects civilian items (zero armor value, trade goods, food)
  - Identifies combat items (weapons, armor with protection)
  - Recognizes horse harness items

#### ItemQueries.cs
- Enhanced `QueryItems()` method signature:
  - Added `tierFilter` parameter (int, -1 for no filter)
  - Added `sortBy` parameter (string, default "id")
  - Added `sortDescending` parameter (bool, default false)

- Added `ApplySorting()` private method:
  - Handles sorting by name, tier, value, type, or id
  - Supports ascending and descending order

- Updated tier search in query string matching:
  - Tier property is now included in search comparisons

#### ItemQueryCommands.cs
- Enhanced `ParseArguments()` method:
  - Returns 5-tuple: `(query, types, tier, sortBy, sortDesc)`
  - Added tier keyword recognition (`tier0`-`tier6`)
  - Added sort parameter parsing (`sort:field:direction`)
  - Updated type keywords list with new categories

- Added helper methods:
  - `ParseSortParameter()` - Parses sort syntax
  - `ParseTierKeyword()` - Extracts tier number from keyword

- Updated command documentation:
  - `QueryItems()` - Shows new filtering and sorting options
  - `QueryItemsAny()` - Shows new filtering and sorting options
  - `BuildCriteriaString()` - Includes tier and sort in output

## Usage Examples

### Basic Type Filtering
```
gm.query.item bow                    # All bows
gm.query.item crossbow               # All crossbows
gm.query.item civilian               # All civilian items
gm.query.item combat                 # All combat items
gm.query.item horsearmor             # All horse armor
```

### Combined Filtering
```
gm.query.item bow tier5              # Tier 5 bows only
gm.query.item armor civilian         # Civilian armor (clothing)
gm.query.item weapon combat tier4    # Combat weapons at tier 4
```

### Sorting
```
gm.query.item bow sort:value:desc    # Most expensive bows
gm.query.item armor tier3 sort:name  # Tier 3 armor alphabetically
gm.query.item sword sort:tier        # Swords by tier (low to high)
```

### Complex Queries
```
gm.query.item imperial bow tier5 sort:value:desc    # Most expensive tier 5 imperial bows
gm.query.item_any bow crossbow tier4 sort:name      # All tier 4 ranged weapons alphabetically
gm.query.item armor combat tier6 sort:value:desc    # Most expensive tier 6 combat armor
```

## Backward Compatibility

All existing queries continue to work:
- `Ranged` flag still includes both bows and crossbows
- Default sorting (by ID) is unchanged
- Existing type keywords remain functional
- No breaking changes to API

## Testing

Build Status: ✅ Success (No compilation errors)

Testing should verify:
1. Bow/Crossbow separation works correctly
2. Civilian items properly identified (zero armor clothing, food, trade goods)
3. Combat items properly identified (weapons, armored equipment)
4. Horse armor detection works
5. Tier filtering returns correct results
6. Sorting works for all fields in both directions
7. Combined filters and sorting work together
8. Backward compatibility maintained

## Future Enhancements

Potential additions:
- Weight-based filtering/sorting
- Damage/armor value filtering
- Culture-specific filtering
- Multi-field sorting (primary and secondary sort keys)