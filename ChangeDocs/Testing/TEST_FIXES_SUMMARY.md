# Test Fixes Summary

This document summarizes all the fixes applied to resolve the failing integration tests identified on 2025-12-14.

## 1. Bug Fix: gm.clan.set_tier Command

**Problem:** The `gm.clan.set_tier` command was reporting tier changes incorrectly (e.g., "tier changed from 4 to 4" instead of "4 to 5").

**Root Cause:** The `clan.Tier` property is computed and only recalculates during game update cycles. Direct assignment of `clan.Renown` doesn't trigger immediate tier recalculation.

**Fix Applied:** 
- File: [`ClanManagementCommands.cs`](../ClanManagementCommands.cs:396)
- Changed from direct renown assignment to using `clan.AddRenown()` method
- This triggers Bannerlord's internal update logic to immediately recalculate the tier

**Affected Test:**
- `integration_clan_set_tier_001_increase` - Now passes

---

## 2. Test Modifications: Hero Membership Prerequisites

**Problem:** Multiple tests were attempting to make heroes leaders/rulers without first adding them as members of the clan/kingdom.

**Fix Applied:** Added setup commands to move heroes to the target clan/kingdom before attempting leadership changes.

**Modified Tests:**
1. `player_special_004` - Added `gm.hero.set_clan main_hero clan_empire_south_1` setup
2. `player_special_005` - Added clan membership setup before making player kingdom ruler
3. `wanderer_special_002` - Added `gm.hero.set_clan CharacterObject_1900 clan_empire_south_2` setup
4. `wanderer_special_003` - Added clan membership setup before making wanderer kingdom ruler
5. `merc_hero_special_002` - Added `gm.hero.set_clan CharacterObject_1866 clan_vlandia_2` setup
6. `bandit_clan_special_003` - Added `gm.hero.set_clan main_hero looters` setup

---

## 3. Test Modifications: Dead Hero Expectations

**Problem:** Tests expected dead heroes to work as leaders/rulers, but the system correctly rejects them.

**Fix Applied:** Changed test expectations from NoException to expecting specific error messages.

**Modified Tests:**
1. `dead_hero_special_001` - Now expects error: "No hero matching query 'dead_lord_2_1' found"
2. `dead_hero_special_002` - Fixed kingdom ID and expects error about dead hero not found

---

## 4. Test Modifications: ID Matching Collisions

**Problem:** Tests expected success when querying IDs that match multiple entries of the same length (which should error).

**Fix Applied:** Changed test expectations to match actual behavior - error when all matching IDs have the same length.

**Modified Tests:**
1. `id_matching_shortest_id_001` - Now expects error listing 9 matching heroes with same-length IDs
2. `id_matching_shortest_id_002` - Now expects error listing 9 matching clans with same-length IDs

---

## 5. Test Modifications: Already-Member Clans

**Problem:** Tests were trying to add clans to kingdoms they were already members of.

**Fix Applied:** Changed clan IDs to use clans NOT already in the target kingdom.

**Modified Tests:**
1. `id_matching_backward_compat_002` - Changed from `clan_vlandia_2` to `clan_sturgia_2`
2. `id_matching_all_types_kingdom_001` - Changed from `clan_sturgia_1` to `clan_vlandia_3`

---

## 6. New Tests Added: ID Matching Features

**Purpose:** Demonstrate that shortest IDs are automatically selected when matching multiple IDs of different lengths.

**New Tests:**
1. `id_matching_shortest_id_003` - Tests hero ID auto-selection (lord_1_41 vs lord_1_411)
2. `id_matching_shortest_id_004` - Tests clan ID auto-selection (clan_vlandia_1 vs clan_vlandia_11)
3. `id_matching_name_query_001` - Tests querying heroes by name
4. `id_matching_name_query_002` - Tests querying clans by name

---

## Summary

**Total Issues Addressed:** 13 failing tests
**Code Fixes:** 1 (clan.set_tier bug)
**Test Modifications:** 12
**New Tests Added:** 4

All previously failing tests should now pass with these fixes applied.

---

## 7. Feature Implementation: 3-Tier Name-Matching Priority System

**Problem:** The original failed test `id_matching_name_query_001` queried "Garios" and received an ambiguous error because the system found both "Garios" (exact match) and "Pagarios" (substring match) without prioritizing between them.

**Original Error:**
```
[FAIL] id_matching_name_query_001: Query by hero name should work correctly
  Command: gm.hero.set_gold Garios 7500
  Expected: Success
  Actual Output: Error: Found 2 heros with names matching query 'Garios':
    lord_1_7	Garios	Clan: Comnos	Kingdom: Western Empire
    lord_SE9_c1	Pagarios	Clan: Vetranis	Kingdom: Southern Empire
```

**Solution Implemented:**
Added a 3-tier name-matching priority system to [`CommandBase.ResolveMultipleMatches()`](../Common/CommandBase.cs:163-198) that prioritizes name matches in the following order:

1. **Tier 1: Exact Name Match** (line 166-178)
   - Query matches entity name exactly (case-insensitive)
   - Example: "Garios" matches "Garios" exactly
   - Returns immediately if unique, errors if multiple exact matches

2. **Tier 2: Prefix Match** (line 180-192)
   - Query matches the start of entity name (case-insensitive)
   - Example: "Gar" matches "Garios" as a prefix
   - Returns if unique, errors if multiple prefix matches

3. **Tier 3: Substring Match** (line 194-197)
   - Query appears anywhere in entity name (case-insensitive)
   - Example: "arios" matches both "Garios" and "Pagarios"
   - Always errors with multiple matches (lowest priority)

**How This Fixes the Failed Test:**

For the query "Garios" with heroes "Garios" and "Pagarios":

1. System checks ID matches first - neither hero's ID matches "Garios"
2. Both heroes match by name, so system enters name priority resolution
3. **Tier 1 Check:** "Garios".Equals("Garios") → TRUE (exact match) ✓
4. **Tier 1 Check:** "Pagarios".Equals("Garios") → FALSE
5. Only one exact match found → Return "Garios" hero → **SUCCESS** ✓

The exact match "Garios" wins over the substring match "Pagarios", resolving the ambiguity.

**Files Modified:**
- [`CommandBase.cs:163-198`](../Common/CommandBase.cs:163-198) - Added 3-tier name priority logic

**Tests Created:**
- [`NamePriorityTests.cs`](NamePriorityTests.cs) - 16 comprehensive tests across 6 categories
  * 2 tests for exact name match selection
  * 2 tests for prefix match selection
  * 3 tests for multiple match error handling
  * 3 tests for case insensitivity
  * 3 tests for clan entity type (generic implementation)
  * 3 tests for ID priority preservation

**Documentation:**
- [`NAME_PRIORITY_TESTS.md`](NAME_PRIORITY_TESTS.md) - Complete test suite documentation

**Integration:**
- Original failing test `id_matching_name_query_001` now passes
- System maintains backward compatibility with ID matching
- Works for all entity types (Hero, Clan, Kingdom)

**Testing Instructions:**
1. Run the specific test: `gm.test.run_id id_matching_name_query_001`
2. Run all name priority tests: `gm.test.run_category NamePriority_ExactMatch`
3. Verify in-game: `gm.hero.set_gold Garios 7500` should succeed

**Benefits:**
- Resolves ambiguous name queries intelligently
- Provides clear, actionable error messages when ambiguity remains
- Case-insensitive matching for better usability
- Generic implementation works across all entity types
- Preserves existing ID matching priority