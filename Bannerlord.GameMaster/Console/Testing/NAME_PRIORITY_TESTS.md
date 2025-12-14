# Name-Matching Priority System Tests

This document describes the comprehensive test suite for the 3-tier name-matching priority system implemented in [`CommandBase.ResolveMultipleMatches()`](../Common/CommandBase.cs#L163-L193).

## Overview

The name-matching priority system ensures that when multiple entities match a query by name (after ID matching is exhausted), the system prioritizes matches in the following order:

1. **Exact Name Match** - Query matches entity name exactly
2. **Prefix Match** - Query matches the start of entity name
3. **Substring Match** - Query appears anywhere in entity name

## Test File Location

All name priority tests are located in:
- [`NamePriorityTests.cs`](NamePriorityTests.cs)

Tests are automatically registered when [`IntegrationTests.RegisterAll()`](IntegrationTests.cs#L15) is called.

## Test Categories

### 1. Exact Name Match Tests (`NamePriority_ExactMatch`)

These tests verify that exact name matches are prioritized over prefix and substring matches.

| Test ID | Description | Expected Outcome |
|---------|-------------|------------------|
| `name_priority_exact_001` | Query "Garios" should select exact match "Garios" over substring "Pagarios" | Success - Garios selected |
| `name_priority_exact_002` | Query "Lucon" should select exact match even with multiple substring competitors | Success - Lucon selected |

**Key Behavior:** Exact name matches always win, regardless of how many substring or prefix matches exist.

### 2. Prefix Match Tests (`NamePriority_PrefixMatch`)

These tests verify that prefix matches are prioritized over substring matches (but lose to exact matches).

| Test ID | Description | Expected Outcome |
|---------|-------------|------------------|
| `name_priority_prefix_001` | Query "Gar" should select prefix match "Garios" over substring "Pagarios" | Success - Prefix match selected |
| `name_priority_prefix_002` | Query "Der" should select hero starting with "Der" | Success or Error (multiple prefixes) |

**Key Behavior:** When no exact match exists, prefix matches are selected. If multiple prefix matches exist, an error is returned.

### 3. Multiple Match Error Tests (`NamePriority_MultipleMatches`)

These tests verify that clear error messages are provided when multiple matches exist at the same priority level.

| Test ID | Description | Expected Outcome |
|---------|-------------|------------------|
| `name_priority_multi_exact_001` | Multiple exact name matches should return clear error | Error mentioning "exactly matching" or "identical names" |
| `name_priority_multi_prefix_001` | Multiple prefix matches should return error | Error mentioning "starting with" |
| `name_priority_multi_contains_001` | Multiple substring matches should return error | Error mentioning "containing" |

**Key Behavior:** When multiple entities match at the same priority level, a descriptive error is returned asking the user to be more specific.

### 4. Case Insensitivity Tests (`NamePriority_CaseInsensitive`)

These tests verify that name matching is case-insensitive across all priority tiers.

| Test ID | Description | Expected Outcome |
|---------|-------------|------------------|
| `name_priority_case_001` | "GARIOS" should match "Garios" (case-insensitive exact) | Success |
| `name_priority_case_002` | "GAR" should work as prefix match | Success or Error (multiple) |
| `name_priority_case_003` | "lucon" should match "Lucon" | Success |

**Key Behavior:** All name matching is case-insensitive, matching the behavior of ID matching.

### 5. Clan Entity Tests (`NamePriority_ClanEntity`)

These tests verify that the name priority system works correctly for Clan entities (testing generic implementation).

| Test ID | Description | Expected Outcome |
|---------|-------------|------------------|
| `name_priority_clan_exact_001` | Exact clan name match should work | Success |
| `name_priority_clan_prefix_001` | Prefix match for clan names (e.g., "Bat" for Battania clans) | Success or Error (multiple) |
| `name_priority_clan_multi_001` | Multiple clan substring matches should error | Error |

**Key Behavior:** The priority system works identically for Heroes, Clans, and Kingdoms due to generic implementation.

### 6. ID Priority Tests (`NamePriority_IDPriority`)

These tests verify that ID matching still takes priority over name matching (existing behavior preserved).

| Test ID | Description | Expected Outcome |
|---------|-------------|------------------|
| `name_priority_id_001` | ID match should beat name match | Success - ID selected |
| `name_priority_id_002` | Shortest ID selection still works | Success |
| `name_priority_id_003` | ID matching unaffected by similar names | Success |

**Key Behavior:** ID matching remains the highest priority. Name priority only applies when no ID matches are found.

## Test Execution

### Running All Tests

```csharp
// In-game console
gm.test.run
```

### Running Only Name Priority Tests

```csharp
// Run specific category
gm.test.run_category NamePriority_ExactMatch
gm.test.run_category NamePriority_PrefixMatch
gm.test.run_category NamePriority_MultipleMatches
gm.test.run_category NamePriority_CaseInsensitive
gm.test.run_category NamePriority_ClanEntity
gm.test.run_category NamePriority_IDPriority
```

### Running Individual Tests

```csharp
// Run specific test
gm.test.run_id name_priority_exact_001
```

## Compatibility with Existing Tests

### Integration Test: `id_matching_name_query_001`

Located in [`IntegrationTests.cs:1587`](IntegrationTests.cs#L1587), this test queries "Garios" and expects success.

**Status:** ✅ **Compatible**

**Reason:** With the new priority system:
- If only "Garios" exists: Exact name match → Success
- If "Garios" and "Pagarios" exist: Exact match "Garios" wins over substring "Pagarios" → Success

The test will continue to pass as it now benefits from the exact match priority.

## Test Data Considerations

### Realistic Game Data

Tests use actual Bannerlord hero and clan names where possible:
- **Garios** - Northern Empire hero (Lucon's brother)
- **Pagarios** - Contains "arios" but not a prefix
- **Derthert** - King of Vlandia
- **Lucon** - Northern Empire ruler
- **Clan names** - Actual game clans (empire, vlandia, battania, etc.)

### Game State Independence

Tests are designed to:
- Handle cases where heroes may not exist (validation checks)
- Work with any reasonable game state
- Provide clear error messages when expected entities are missing

## Error Message Patterns

The tests verify that error messages are clear and actionable:

### Exact Name Matches (Multiple)
```
Error: Found X heros with names exactly matching 'Query':
...
Multiple entities have identical names. Please use their IDs.
```

### Prefix Matches (Multiple)
```
Error: Found X heros with names starting with 'Query':
...
Please use a more specific name or use their IDs.
```

### Substring Matches (Multiple)
```
Error: Found X heros with names containing 'Query':
...
Please use a more specific name or use their IDs.
```

## Implementation Reference

The 3-tier priority system is implemented in:
[`CommandBase.ResolveMultipleMatches()`](../Common/CommandBase.cs#L163-L193)

**Priority Logic:**
1. Exact ID match → Return immediately
2. Shortest ID match (if unique) → Return
3. **Exact name match** (new) → Return if unique, error if multiple
4. **Prefix name match** (new) → Return if unique, error if multiple  
5. **Substring name match** (new) → Error (multiple matches at lowest priority)

## Coverage Summary

| Scenario | Tests | Status |
|----------|-------|--------|
| Exact Name Match Selection | 2 tests | ✅ Complete |
| Prefix Match Selection | 2 tests | ✅ Complete |
| Multiple Exact Matches Error | 1 test | ✅ Complete |
| Multiple Prefix Matches Error | 1 test | ✅ Complete |
| Multiple Substring Matches Error | 1 test | ✅ Complete |
| Case Insensitivity | 3 tests | ✅ Complete |
| Clan Entity Type | 3 tests | ✅ Complete |
| ID Priority Unaffected | 3 tests | ✅ Complete |

**Total Name Priority Tests:** 16 comprehensive tests across 6 categories

## Related Documentation

- [Testing Guide](TESTING.md) - General testing information
- [Test Fixes Summary](TEST_FIXES_SUMMARY.md) - Historical test issues
- [Integration Tests](IntegrationTests.cs) - Full integration test suite
- [Command Base](../Common/CommandBase.cs) - Implementation details