# Troop Test Syntax and Expectation Fixes

**Date:** 2025-12-16  
**Type:** Test Fixes  
**Impact:** Testing Infrastructure

## Summary
Fixed 14 failed troop-related tests by correcting command syntax, command names, and test expectations to align with actual command behavior.

## Problem
Tests were failing due to:
1. Incorrect command syntax (using colons for non-sort parameters)
2. Wrong command names (missing `.troops.` namespace prefix)
3. Incorrect test expectations (not matching actual success behavior)
4. Wrong sort order expectations (expecting descending when default is ascending)

## Changes Made

### StandardTests.cs (3 fixes)

**1. troop_filter_008 - Village Defenders Exception**
- **Issue:** Test expected 0 troops but villagers defend village raids
- **Fix:** Changed ExpectedText from "0 troop(s) matching" to "troop(s) matching"
- **Reason:** Village peasants are combat troops that defend against raids

**2. troop_mgmt_011 - Ambiguous Vlandia Query**
- **Issue:** Query "vlandia" matches 67 characters, test expected success
- **Fix:** Changed TestExpectation from Success to Error, added ExpectedText = "multiple"
- **Reason:** Query is correctly ambiguous and should return error

**3. troop_mgmt_012 - Ambiguous Sturgia Query**
- **Issue:** Query "sturgia" matches 64 characters, test expected success
- **Fix:** Changed TestExpectation from Success to Error, added ExpectedText = "multiple"
- **Reason:** Query is correctly ambiguous and should return error

### IntegrationTests.cs (11 fixes)

**4. integration_troop_query_culture_001 - Culture Syntax**
- **Issue:** Used `culture:empire` (incorrect colon syntax)
- **Fix:** Changed command to `gm.query.troop empire`
- **Reason:** Colon is only for sort/orderby, not filter keywords

**5. integration_troop_query_tier_001 - Tier Syntax**
- **Issue:** Used `tier:5+` (incorrect colon syntax)
- **Fix:** Changed command to `gm.query.troop tier5`
- **Reason:** Tier keywords don't use colons

**6. integration_troop_query_combined_001 - Combined Filter Syntax**
- **Issue:** Used `culture:vlandia formation:cavalry`
- **Fix:** Changed command to `gm.query.troop vlandia cavalry`
- **Reason:** Filter keywords don't use colon notation

**7. integration_troop_query_sorting_001 - Sort Order Validation**
- **Issue:** Validator checked for descending order but default is ascending
- **Fix:** Changed previousTier from int.MaxValue to -1, reversed comparison logic
- **Reason:** Default sort is ascending, not descending

**8-12. integration_troop_give_* - Command Name Corrections (5 tests)**
- **Issue:** Used `gm.give_hero_troops` (missing namespace)
- **Fix:** Changed all instances to `gm.troops.give_hero_troops`
- **Tests affected:**
  - integration_troop_give_001
  - integration_troop_give_002
  - integration_troop_give_verify_001
  - integration_troop_give_quantity_001 (Command + SetupCommands)
  - integration_troop_give_multiple_001 (Command + SetupCommands)
- **Reason:** Command requires `.troops.` namespace prefix

**13-14. SetupCommands Fixes**
- **Tests:** integration_troop_give_quantity_001, integration_troop_give_multiple_001
- **Fix:** Updated SetupCommands arrays to use correct command names
- **Reason:** Ensure test setup uses proper syntax

## Testing Impact

### Before Fixes
- 14 tests failing (95.6% pass rate)
- Failures due to incorrect syntax and expectations
- Misleading test results

### After Fixes
- All 14 tests should now pass correctly
- Tests validate actual command behavior
- Accurate representation of system functionality

## Command Syntax Rules Reinforced

1. **Filter Keywords:** No colons (e.g., `empire`, `tier5`, `cavalry`)
2. **Sort/OrderBy:** Uses colons (e.g., `sort:name`, `sort:tier:desc`)
3. **Command Namespaces:** Management commands require namespace prefix (e.g., `gm.troops.give_hero_troops`)
4. **Ambiguous Queries:** Should return error with list of matches
5. **Default Sort:** Ascending order unless `:desc` specified

## Files Modified
- `Bannerlord.GameMaster/Console/Testing/StandardTests.cs` (3 changes)
- `Bannerlord.GameMaster/Console/Testing/IntegrationTests.cs` (11 changes)

## Validation
- Review test results from Console_Test_Results_2025-12-16_005.txt
- All fixes align with user's notes in test results
- Changes preserve test logic and custom validators
- No modifications to tested functionality

## Related Issues
- Fixes complement TROOP_TEST_FIXES_2025-12-16.md
- Aligns with TROOP_TEST_IMPROVEMENTS_2025-12-16.md
- Supports accurate testing of troop query and management systems