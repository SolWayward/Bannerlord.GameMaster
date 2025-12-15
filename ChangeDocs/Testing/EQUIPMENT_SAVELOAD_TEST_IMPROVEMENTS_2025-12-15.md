# Equipment Save/Load Integration Test Improvements

**Date:** 2025-12-15  
**Type:** Test Fix & Enhancement  
**Status:** Completed  
**Files Modified:**
- [`Bannerlord.GameMaster/Console/Testing/IntegrationTests.cs`](../../Bannerlord.GameMaster/Console/Testing/IntegrationTests.cs)

## Summary

Fixed critical issues in equipment save/load integration tests that were causing false test failures and preventing proper validation of the equipment save/load functionality.

## Problems Identified

### 1. Test Expectation Mismatch
**Issue:** Save tests used `TestExpectation.Success` but commands returned "Saved..." messages instead of "Success", causing valid operations to be marked as failures.

**Tests Affected:**
- `integration_equipment_save_001` (battle equipment)
- `integration_equipment_save_002` (civilian equipment)
- `integration_equipment_save_003` (both equipment sets)

**Evidence from Test Results:**
```
[FAIL] integration_equipment_save_001: Save player equipment to file and verify file exists
  Error: Expected success but got: Saved Sol's battle equipment to: test_equipment_save.json
Items saved (9):
  WeaponItemBeginSlot Norse Hatchet
  ...
```

### 2. Equipment Load Returning Zero Items
**Issue:** Load tests reported "Items loaded (0)" even though save operations showed items were successfully saved. Root cause was using `remove_equipped` on the same hero that had equipment saved, corrupting the test state.

**Tests Affected:**
- `integration_equipment_load_001` (battle equipment)
- `integration_equipment_load_002` (civilian equipment)
- `integration_equipment_load_003` (both equipment sets)

**Evidence from Test Results:**
```
[FAIL] integration_equipment_load_002: Save then load player civilian equipment and verify it was loaded
  Error: Player should have civilian equipment after loading
  Command: gm.item.load_equipment_civilian main_hero test_equipment_load_civilian
  Actual Output: Loaded Sol's civilian equipment from: test_equipment_load_civilian.json
Items loaded (0):
```

### 3. Poor Test Design - State Corruption
**Issue:** Tests were saving and loading equipment on the SAME hero (`main_hero`), then using `remove_equipped` to clear equipment before loading. This permanently destroyed the source equipment, making tests non-repeatable and causing the "0 items loaded" issue.

## Solutions Implemented

### 1. Fixed Save Test Expectations (Lines 268-403)

Changed all three save tests from `TestExpectation.Success` to `TestExpectation.Contains`:

```csharp
// BEFORE
TestExpectation.Success

// AFTER
TestExpectation.Contains
ExpectedText = "Saved"
```

**Changes:**
- **integration_equipment_save_001**: Added `ExpectedText = "Saved"` for battle equipment
- **integration_equipment_save_002**: Added `ExpectedText = "Saved"` for civilian equipment
- **integration_equipment_save_003**: Added `ExpectedText = "Saved"` for both sets

**Rationale:** Commands correctly return "Saved..." messages. Tests should validate the message appears rather than expecting "Success" keyword.

### 2. Redesigned Load Tests with Two-Hero Pattern (Lines 405-586)

Completely redesigned all load tests to use separate source and target heroes:

**Pattern:**
1. Save equipment from `lord_1_1` (source hero - has known equipment)
2. Clear equipment on `lord_4_1` (target hero)
3. Load saved equipment onto `lord_4_1` (target hero)
4. Validate target hero now has the equipment

**integration_equipment_load_001 (Battle Equipment):**
```csharp
SetupCommands = new List<string>
{
    // Save from source hero
    "gm.item.save_equipment lord_1_1 test_equipment_load",
    // Clear target hero's equipment
    "gm.item.remove_equipped lord_4_1"
},
Command = "gm.item.load_equipment lord_4_1 test_equipment_load"
```

**integration_equipment_load_002 (Civilian Equipment):**
```csharp
SetupCommands = new List<string>
{
    "gm.item.save_equipment_civilian lord_1_1 test_equipment_load_civilian",
    "gm.item.remove_equipped lord_4_1"
},
Command = "gm.item.load_equipment_civilian lord_4_1 test_equipment_load_civilian"
```

**integration_equipment_load_003 (Both Equipment Sets):**
```csharp
SetupCommands = new List<string>
{
    "gm.item.save_equipment_both lord_1_1 test_equipment_load_both",
    "gm.item.remove_equipped lord_4_1"
},
Command = "gm.item.load_equipment_both lord_4_1 test_equipment_load_both"
```

**Validator Updates:**
- Changed from checking `Hero.MainHero` to checking `lord_4_1` (`Hero.FindFirst(h => h.StringId == "lord_4_1")`)
- Validates target hero has equipment after loading
- Source hero (`lord_1_1`) retains equipment for future test runs

### 3. Hero Selection Rationale

**Source Hero (`lord_1_1` - Lucon):**
- From sample file: `lord_1_1 Lucon Clan: Osticos Kingdom: Northern Empire`
- Lord hero with known equipment setup
- Stable across game sessions
- Equipment preserved for repeated testing

**Target Hero (`lord_4_1` - Derthert):**
- From sample file: `lord_4_1 Derthert Clan: dey Meroc Kingdom: Vlandia`
- Different clan/kingdom than source
- Equipment can be safely cleared and restored
- No interference with other tests

## Benefits

### 1. Test Reliability
- ✅ Tests correctly validate command output messages
- ✅ No more false failures from expectation mismatches
- ✅ Tests are repeatable without state corruption

### 2. Proper State Management
- ✅ Source hero equipment is preserved
- ✅ Target hero state is cleanly managed
- ✅ Tests don't interfere with each other

### 3. Better Test Coverage
- ✅ Tests validate cross-hero equipment transfer
- ✅ More realistic usage pattern (save from one hero, load to another)
- ✅ Validates file I/O and equipment application separately

### 4. Debugging Support
- ✅ Clear separation of save and load operations
- ✅ Easy to identify which step fails
- ✅ Source equipment always available for comparison

## Test Results Expected

After these fixes, the tests should:

1. **Save Tests** - Pass when files are created with "Saved" message
2. **Load Tests** - Pass when equipment is successfully loaded onto target hero
3. **Cleanup Test** - Pass when test files are properly cleaned up

## Code Quality

These changes follow:
- ✅ Best practices from [`/docs/guides/best-practices.md`](../../docs/guides/best-practices.md)
- ✅ Implementation patterns from [`/docs/implementation/equipment-saveload-implementation.md`](../../docs/implementation/equipment-saveload-implementation.md)
- ✅ Testing guidelines from [`/docs/guides/testing.md`](../../docs/guides/testing.md)

## Related Documentation

- **Feature Documentation:** [`ChangeDocs/Features/EQUIPMENT_SAVELOAD_FEATURE_2025-12-15.md`](../Features/EQUIPMENT_SAVELOAD_FEATURE_2025-12-15.md)
- **Implementation Guide:** [`docs/implementation/equipment-saveload-implementation.md`](../../docs/implementation/equipment-saveload-implementation.md)
- **Previous Test Fixes:** [`ChangeDocs/Testing/EQUIPMENT_SAVELOAD_TEST_FIXES_2025-12-15.md`](EQUIPMENT_SAVELOAD_TEST_FIXES_2025-12-15.md)

## Next Steps

1. Run tests in campaign mode to verify fixes
2. Verify that `lord_1_1` equipment is preserved across test runs
3. Verify that `lord_4_1` correctly receives loaded equipment
4. Confirm all 7 failed tests now pass

## Notes

- This fix addresses the root cause mentioned in the user's original concern: "tests removing equipment were run before the save load tests causing the player to have no equipment"
- The solution implements the suggested improvement: "tests really should save equipment from one hero and load it onto another hero"
- Tests now properly demonstrate the intended use case: saving a hero's equipment and loading it onto a different hero