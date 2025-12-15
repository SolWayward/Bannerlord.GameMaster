# Testing Guide

**Navigation:** [← Back: Best Practices](best-practices.md) | [Back to Index](../README.md) | [Next: Troubleshooting →](troubleshooting.md)

---

## Overview

This guide covers how to write, run, and debug tests for the Bannerlord.GameMaster project.

## Test Categories

| Category | Purpose | Example Tests |
|----------|---------|---------------|
| `HeroQuery` | Hero search and list commands | Query command validation |
| `HeroManagement` | Hero modification commands | Set clan, set age, kill |
| `ClanQuery` | Clan search and list commands | List clans, clan info |
| `ClanManagement` | Clan modification commands | Add hero, set gold |
| `KingdomQuery` | Kingdom search commands | List kingdoms, kingdom info |
| `KingdomManagement` | Kingdom modification commands | Add clan, declare war |
| `SuccessPaths_HeroManagement` | Successful hero command execution | Verify command success paths |
| `SuccessPaths_ClanManagement` | Successful clan command execution | Verify command success paths |
| `SuccessPaths_KingdomManagement` | Successful kingdom command execution | Verify command success paths |
| `SuccessPaths_Query` | Successful query execution | Verify queries return results |
| `NamePriority_ExactMatch` | Name matching priority - exact | Exact name match tests |
| `NamePriority_PrefixMatch` | Name matching priority - prefix | Prefix match tests |
| `NamePriority_SubstringMatch` | Name matching priority - substring | Substring match tests |
| `NamePriority_MultipleMatches` | Multiple match error handling | Error message validation |
| `NamePriority_CaseInsensitive` | Case-insensitive matching | Case sensitivity tests |
| `NamePriority_IDPriority` | ID-based matching priority | ID vs name priority |
| `NamePriority_ClanEntity` | Clan entity name matching | Clan-specific tests |
| `NamePriority_KingdomEntity` | Kingdom entity name matching | Kingdom-specific tests |
| `NamePriority_Ambiguous` | Ambiguous query handling | Error message quality |
| `Integration` | Multi-step workflows | Complex command sequences |

## Running Tests

### Console Commands

```bash
# Register all tests (if not auto-registered)
gm.test.register_examples

# Run all registered tests
gm.test.run_all

# Run tests in a specific category
gm.test.run_category HeroQuery

# Run a single test by ID
gm.test.run_single hero_query_001

# List all registered tests
gm.test.list

# View last test results
gm.test.last_results
gm.test.last_results verbose  # For detailed output
```

### Test Result Files

Test results are automatically saved to:
```
Bannerlord.GameMaster/Console/Testing/Results/Console_Test_Results_YYYY-MM-DD_NNN.txt
```

## Writing Test Cases

### Basic Test

```csharp
TestRunner.RegisterTest(new TestCase(
    testId: "hero_query_001",
    description: "Query without args should return all heroes",
    command: "gm.query.hero",
    expectation: TestExpectation.Contains
)
{
    Category = "HeroQuery",
    ExpectedText = "hero(es) matching"
});
```

### Test Expectations

- `TestExpectation.Success` - Output starts with "Success:"
- `TestExpectation.Error` - Output starts with "Error:"
- `TestExpectation.Contains` - Output contains `ExpectedText`
- `TestExpectation.NotContains` - Output doesn't contain `UnexpectedText`
- `TestExpectation.NoException` - Command doesn't crash

### Test with Custom Validator

```csharp
TestRunner.RegisterTest(new TestCase(
    testId: "hero_query_custom_001",
    description: "Should return at least 50 heroes",
    command: "gm.query.hero",
    expectation: TestExpectation.NoException
)
{
    Category = "HeroQuery",
    CustomValidator = (output) =>
    {
        var match = Regex.Match(output, @"Found (\d+) hero");
        if (match.Success && int.TryParse(match.Groups[1].Value, out int count))
        {
            if (count >= 50)
                return (true, null);
            return (false, $"Expected at least 50 heroes, found {count}");
        }
        return (false, "Could not parse hero count");
    }
});
```

## Expanded Test Coverage

### Success Path Tests (16 tests)

Located in [`StandardTests.cs`](../../Bannerlord.GameMaster/Console/Testing/StandardTests.cs:425), these tests verify successful command execution:

**Hero Management (6 tests):**
- Transfer hero to new clan
- Set hero age
- Set hero gold
- Add gold to hero
- Heal hero
- Set hero relations

**Clan Management (4 tests):**
- Set clan gold
- Add gold to clan
- Set clan renown
- Increase clan tier

**Kingdom Management (3 tests):**
- Add clan to kingdom
- Remove clan from kingdom
- Set kingdom ruler

**Query Tests (3 tests):**
- Query heroes with results
- Query clans with results
- Query kingdoms with results

### Name Priority Tests (10 test scenarios)

Located in [`NamePriorityTests.cs`](../../Bannerlord.GameMaster/Console/Testing/NamePriorityTests.cs:9), these tests verify the 3-tier name-matching priority system:

**Priority Order: Exact Match > Prefix Match > Substring Match**

1. **Exact Name Match Tests** - Verify exact matches win over prefix/substring
2. **Prefix Match Tests** - Verify prefix matches beat substring matches
3. **Substring Match Tests** - Verify substring matching works when no exact/prefix
4. **Multiple Match Errors** - Verify clear error messages for ambiguous queries
5. **Case Insensitivity Tests** - Verify case-insensitive matching works
6. **ID Priority Tests** - Verify ID matches take priority over name matches
7. **Clan Entity Tests** - Verify priority system works for clans
8. **Kingdom Entity Tests** - Verify priority system works for kingdoms
9. **Ambiguous Match Tests** - Verify helpful error messages
10. **Priority Order Verification** - Verify correct selection between match types

See [`NamePriorityTests.cs`](../../Bannerlord.GameMaster/Console/Testing/NamePriorityTests.cs:9) for detailed test implementations.

## Debugging Failed Tests

### Understanding Test Output

Each test result includes:
- **Status**: PASS/FAIL
- **Test ID**: Unique identifier
- **Description**: What the test validates
- **Command**: Exact command executed
- **Execution Time**: How long it took
- **Failure Details**: Expected vs. actual output (for failures)

### Common Failure Patterns

**1. Validation Failures** - Expected text not found
```
[FAIL] Expected: "Missing arguments"
Actual: "Error: Requires at least 2 arguments"
```
**Fix**: Update `ExpectedText` to match actual error message

**2. Execution Failures** - Command threw exception
```
[FAIL] Exception: NullReferenceException
```
**Fix**: Add null checks in command implementation

**3. Expected Output Mismatch** - Wrong TestExpectation type
```
[FAIL] Expected Success, got Error
```
**Fix**: Review command logic or update test expectation

### Using CommandLogger for Debugging

```bash
# Enable logger
gm.logger.enable

# Set to debug level
gm.logger.set_level debug

# Run your test
gm.test.run_single hero_query_001

# Check logs for detailed execution trace
```

## Test Execution Workflow

### When to Run Tests

1. **After Code Changes** - Always run tests before committing
2. **Before Pull Requests** - Ensure all tests pass
3. **After Merging** - Verify integration didn't break anything
4. **Periodically** - Run full test suite to catch regressions

### Incremental Testing Approach

```bash
# 1. Unit Testing (Individual Commands)
gm.test.run_single hero_mgmt_001

# 2. Category Testing (Related Commands)
gm.test.run_category HeroManagement

# 3. Success Path Testing (Verify Successful Execution)
gm.test.run_category SuccessPaths_HeroManagement

# 4. Name Priority Testing (Verify Name Matching Logic)
gm.test.run_category NamePriority_ExactMatch

# 5. Full Testing (All Commands)
gm.test.run_all

# 6. Targeted Re-testing (After Fixes)
gm.test.run_single hero_mgmt_002
```

## Writing Success Path Tests

Success path tests verify that commands work correctly under normal conditions:

```csharp
TestRunner.RegisterTest(new TestCase(
    "hero_mgmt_success_001",
    "Successfully transfer hero to new clan",
    "gm.hero.set_clan lord_1_1 clan_empire_south_1",
    TestExpectation.Success
)
{
    Category = "SuccessPaths_HeroManagement"
});
```

**Guidelines:**
- Use real game entity IDs that exist in a standard playthrough
- Verify the command succeeds (not just doesn't crash)
- Add cleanup commands if the test modifies persistent state
- Focus on typical use cases, not edge cases

## Writing Name Priority Tests

Name priority tests verify the 3-tier matching system works correctly:

```csharp
TestRunner.RegisterTest(new TestCase(
    "name_priority_exact_001",
    "Exact name match 'Garios' should select 'Garios' over 'Pagarios'",
    "gm.hero.set_gold Garios 10000",
    TestExpectation.Success
)
{
    Category = "NamePriority_ExactMatch",
    CustomValidator = (output) =>
    {
        // Verify correct hero was selected
        var hero = Hero.AllAliveHeroes.FirstOrDefault(h =>
            h.Name.ToString().Equals("Garios", StringComparison.OrdinalIgnoreCase));
        
        if (hero == null)
            return (false, "Hero 'Garios' not found");
            
        if (hero.Gold != 10000)
            return (false, $"Expected gold 10000, got {hero.Gold}");
            
        return (true, null);
    }
});
```

**Guidelines:**
- Test exact match wins over prefix and substring
- Test prefix match wins over substring
- Verify error messages for multiple matches
- Test case insensitivity across all match types
- Verify ID matching takes priority over name matching

---

## Next Steps

1. **Review** [Troubleshooting](troubleshooting.md) for common issues
2. **Check** [Code Quality Checklist](../reference/code-quality-checklist.md) before committing
3. **Reference** [Implementation Improvements](../reference/implementation-improvements.md) for architecture patterns

---

**Navigation:** [← Back: Best Practices](best-practices.md) | [Back to Index](../README.md) | [Next: Troubleshooting →](troubleshooting.md)