# Query Output Formatting Fixes - 2025-12-20

## Summary
Fixed three output formatting issues across query commands to improve consistency and readability.

## Issues Fixed

### Issue 1: Missing Fields in generate_lords Output
**File:** `Bannerlord.GameMaster/Heroes/HeroQueries.cs`

**Problem:** The `generate_lords` command output was missing the `level` field when displaying generated heroes.

**Solution:** Added the `Level` field to the [`GetFormattedDetails()`](Bannerlord.GameMaster/Heroes/HeroQueries.cs:154) method in HeroQueries, positioning it after Culture and before Gender for logical grouping.

**Output Before:**
- StringId, Name, Culture, Gender, Clan, Kingdom

**Output After:**
- StringId, Name, Culture, Level, Gender, Clan, Kingdom

---

### Issue 2: Missing Headers in Item Modifiers Query
**File:** `Bannerlord.GameMaster/Console/Query/ItemModifierQueryCommands.cs`

**Problem:** The item modifiers query output had no column headers, making it difficult to understand what each column represented.

**Solution:** Added properly formatted headers with separators in the [`QueryModifiers()`](Bannerlord.GameMaster/Console/Query/ItemModifierQueryCommands.cs:18) command. Headers are calculated based on actual data width to ensure proper alignment.

**Changes:**
- Added column headers: "StringId", "Name", "Price Factor"
- Added separator line using dashes
- Adjusted Price Factor display format from "Price Factor: x{value}" to "x{value}" in data rows for cleaner output
- Headers align with data columns using calculated widths

---

### Issue 3: Culture Query Header Misalignment
**File:** `Bannerlord.GameMaster/Console/Query/CultureQueryCommands.cs`

**Problem:** The culture query output headers used tab characters and fixed-width padding that didn't align properly with the data columns.

**Solution:** Refactored [`GetFormattedCultureList()`](Bannerlord.GameMaster/Console/Query/CultureQueryCommands.cs:197) to use the `ColumnFormatter` utility class, ensuring proper alignment consistent with other query commands like hero and settlement queries.

**Technical Details:**
- Removed manual tab-based formatting
- Implemented `ColumnFormatter<CultureObject>.FormatList()` for automatic column width calculation
- Simplified type determination logic within column extractor
- Headers are now implicit and handled by the ColumnFormatter utility

---

## Files Modified

1. **Bannerlord.GameMaster/Heroes/HeroQueries.cs**
   - Added Level field to hero detail output
   - Updated `GetFormattedDetails()` method (line 154)

2. **Bannerlord.GameMaster/Console/Query/ItemModifierQueryCommands.cs**
   - Added column headers with proper alignment
   - Added separator line for clarity
   - Adjusted Price Factor label format
   - Updated `QueryModifiers()` method (line 18)

3. **Bannerlord.GameMaster/Console/Query/CultureQueryCommands.cs**
   - Refactored to use ColumnFormatter utility
   - Removed manual tab-based alignment
   - Updated `GetFormattedCultureList()` method (line 197)

---

## Testing Recommendations

### Test Case 1: Hero Generation Output
```
gm.hero.generate_lords 3
```
**Expected:** Output should display Culture and Level fields for each hero in aligned columns.

### Test Case 2: Item Modifier Query
```
gm.query.modifiers
```
**Expected:** Output should have clear headers (StringId, Name, Price Factor) with separator line, and data columns properly aligned beneath headers.

### Test Case 3: Culture Query Alignment
```
gm.query.culture
```
**Expected:** StringId, Name, and Type columns should be properly aligned without misalignment caused by varying text lengths.

---

## Implementation Notes

- All three fixes follow the existing ColumnFormatter pattern used by other query commands
- Changes maintain backward compatibility with existing functionality
- Output format is now consistent across all query commands
- The ColumnFormatter utility automatically handles column width calculation based on actual data

---

## Related Files
- [`ColumnFormatter.cs`](Bannerlord.GameMaster/Console/Common/ColumnFormatter.cs) - Utility used for proper column alignment
- [`HeroQueryCommands.cs`](Bannerlord.GameMaster/Console/Query/HeroQueryCommands.cs) - Reference implementation for query formatting
- [`SettlementQueryCommands.cs`](Bannerlord.GameMaster/Console/Query/SettlementQueryCommands.cs) - Reference implementation for query formatting
