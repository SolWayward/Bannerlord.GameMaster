# Query Column Alignment System - December 16, 2025

## Summary

Implemented a reusable column alignment system to properly format query results in the game console. The game console treats tabs as fixed spaces rather than alignment points, so a dynamic width calculation system was needed to ensure columns line up correctly.

## Problem Statement

Previously, all query results used simple tab separators (`\t`) between columns, which resulted in misaligned output in the game console because:
- The console treats tabs as a fixed number of spaces (not as alignment points)
- Different column values have varying lengths (e.g., "town_empire_1" vs "castle_battania_s1")
- This caused subsequent columns to be jagged and difficult to read

## Solution

Created a generic `ColumnFormatter<T>` utility class that:
1. Calculates the maximum width needed for each column across all results
2. Pads each column value to its maximum width plus minimum spacing
3. Ensures consistent column alignment regardless of content length

## Implementation Details

### New Files Created

**`Bannerlord.GameMaster/Console/Common/ColumnFormatter.cs`**
- Generic column formatting utility
- Supports fluent API for defining columns
- Automatically calculates column widths
- Provides both builder pattern and static helper methods

### Modified Files

**All Queries Classes Updated:**
1. `Bannerlord.GameMaster/Settlements/SettlementQueries.cs`
2. `Bannerlord.GameMaster/Clans/ClanQueries.cs`
3. `Bannerlord.GameMaster/Heroes/HeroQueries.cs`
4. `Bannerlord.GameMaster/Items/ItemQueries.cs`
5. `Bannerlord.GameMaster/Troops/TroopQueries.cs`
6. `Bannerlord.GameMaster/Kingdoms/KingdomQueries.cs`

**Changes Made:**
- Added `using Bannerlord.GameMaster.Console.Common;` for ColumnFormatter access
- Updated `GetFormattedDetails()` methods to use `ColumnFormatter<T>.FormatList()`
- Replaced tab-separated formatting with aligned column formatting

## Usage Examples

### Before (Tab-Separated)
```csharp
return $"{settlement.StringId}\t{settlement.Name}\t[{settlementType}]\tOwner: {ownerName}...";
```

**Result:** Misaligned columns due to varying field lengths

### After (Column-Aligned)
```csharp
return ColumnFormatter<Settlement>.FormatList(
    settlements,
    s => s.StringId,
    s => s.Name.ToString(),
    s => $"[{type}]",
    s => $"Owner: {s.OwnerClan?.Name?.ToString() ?? "None"}",
    // ... more columns
);
```

**Result:** Perfectly aligned columns with consistent spacing

## Column Definitions by Entity Type

### Settlement Queries
1. StringId
2. Name
3. Type ([City], [Castle], [Village], [Hideout])
4. Owner
5. Kingdom
6. Culture
7. Prosperity/Hearth

### Clan Queries
1. StringId
2. Name
3. Heroes Count
4. Leader
5. Kingdom

### Hero Queries
1. StringId
2. Name
3. Clan
4. Kingdom

### Item Queries
1. StringId
2. Name
3. Type
4. Value
5. Tier

### Troop Queries
1. StringId
2. Name
3. Category ([Regular], [Noble], [Militia], etc.)
4. Tier
5. Level
6. Culture
7. Formation

### Kingdom Queries
1. StringId
2. Name
3. Clans Count
4. Heroes Count
5. Ruling Clan
6. Ruler

## Technical Approach

### Width Calculation Algorithm
```
For each column:
  1. Extract value from every entity
  2. Measure text length of each value
  3. Store maximum length for column
  
For each row:
  1. Get value for each column
  2. Pad value to (maxWidth + MIN_SPACING)
  3. Append to row string
  4. Last column has no padding (no trailing spaces)
```

### Design Decisions

**Why Generic Class?**
- Reusable across all entity types
- Type-safe column definitions
- No code duplication

**Why Calculate Width Dynamically?**
- Accommodates varying data lengths
- Works with modded content
- No hard-coded column widths to maintain

**Why MIN_SPACING = 2?**
- Provides visual separation between columns
- Consistent with common terminal/console conventions
- Prevents columns from appearing cramped

## Benefits

1. **Improved Readability**: Columns line up perfectly regardless of content
2. **Consistency**: Same formatting approach across all query types
3. **Maintainability**: Single utility class to maintain
4. **Flexibility**: Easy to add/remove/reorder columns
5. **Performance**: Single-pass width calculation, efficient formatting

## Breaking Changes

None. This is an internal formatting change that improves output quality without affecting:
- Command syntax
- Query functionality
- Data returned
- API contracts

## Testing Recommendations

Users should test various query commands to verify column alignment:

```
gm.query.settlement empire
gm.query.clan noble
gm.query.hero lord
gm.query.item weapon
gm.query.troop infantry
gm.query.kingdom active
```

Expected: All columns should align vertically across all result rows.

## Future Enhancements

Potential improvements for future consideration:
1. Configurable minimum spacing per use case
2. Column header row support
3. Maximum column width limiting (with truncation)
4. Right-alignment for numeric columns
5. Color coding support (if game console allows)

## Related Files

- Implementation: `Bannerlord.GameMaster/Console/Common/ColumnFormatter.cs`
- Usage: All `*Queries.cs` files
- Interface: No changes to command interfaces

## Notes

- The Extensions classes' `FormattedDetails()` methods remain unchanged as they're primarily used by the Queries classes
- The ColumnFormatter is designed to work with the game's console output system
- Tab character (`\t`) usage has been eliminated from query result formatting