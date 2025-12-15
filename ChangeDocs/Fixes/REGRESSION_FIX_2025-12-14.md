# Regression Fix: Name Priority Matching Bypass

**Date:** December 14, 2025  
**File Modified:** [`CommandBase.cs`](../Common/CommandBase.cs:99-204)  
**Tests Fixed:** 5 failing tests restored to passing status

---

## Summary

A critical regression was discovered in the entity resolution system where the 3-tier name priority matching logic was being bypassed for entities that matched both name AND ID criteria. This caused 5 name priority tests to fail unexpectedly. The fix restructured the `ResolveMultipleMatches` method to ensure ALL matches are evaluated through the name priority system before falling back to ID-based resolution.

**Result:** All 113 tests now pass (previously 108/113).

---

## The Problem

### Failing Tests

Five name priority tests were failing after recent changes:

1. `id_matching_name_query_001` - ID matching when query could match both name and ID
2. `name_priority_exact_001` - Exact name match priority over ID match
3. `name_priority_exact_002` - Multiple exact name matches requiring ID fallback
4. `name_priority_prefix_001_exact` - Prefix name match taking priority
5. `name_priority_case_001` - Case-insensitive exact name matching

### Symptoms

When a query could match both an entity's name and ID:
- The name priority checks (exact match, prefix match) were never evaluated
- The system would immediately fall back to ID matching logic
- This broke the documented priority hierarchy where name matching should always be checked first

**Example Scenario:**
- Query: `"emp"` 
- Entity: `Name = "Empire", ID = "empire_1"`
- Expected: Match by name priority (prefix match)
- Actual: Bypassed name checks, used ID matching instead

---

## Root Cause Analysis

### The Bug

Located in [`CommandBase.cs`](../Common/CommandBase.cs:129) at line 129:

```csharp
if (matchesName && !matchesId)
{
    nameMatches.Add(entity);
}
```

**Problem:** The condition `!matchesId` prevented entities matching both name AND ID from being added to the `nameMatches` collection. This meant:

1. Name priority checks only saw entities that matched name exclusively
2. Entities matching both criteria were only added to `idMatches`
3. The name priority tiers (exact, prefix) never evaluated these dual-match entities
4. The system would skip directly to ID matching logic

### Why This Mattered

The 3-tier name priority system was designed to provide intuitive matching:

**Intended Priority:**
1. **Tier 1:** Exact name match → immediate selection
2. **Tier 2:** Prefix name match → immediate selection  
3. **Tier 3:** ID-based matching → exact ID, then shortest ID

The bug effectively disabled Tiers 1 and 2 for any entity whose ID also contained the query string, which is common since IDs often derive from or include the entity name.

---

## The Solution

### Code Changes

The fix restructured the matching logic in [`CommandBase.cs`](../Common/CommandBase.cs:99-210) from lines 99-210, implementing a two-phase approach:

**Phase 1: Collect ALL matches** (lines 107-133)
```csharp
var allMatches = new List<T>();
var idMatches = new List<T>();
var nameMatches = new List<T>();

foreach (var entity in matches)
{
    string entityId = getStringId(entity) ?? "";
    string entityName = getName(entity) ?? "";

    bool matchesId = entityId.IndexOf(query, StringComparison.OrdinalIgnoreCase) >= 0;
    bool matchesName = entityName.IndexOf(query, StringComparison.OrdinalIgnoreCase) >= 0;

    if (matchesId || matchesName)
    {
        allMatches.Add(entity);  // ALL matches go here
    }
    
    if (matchesId)
    {
        idMatches.Add(entity);
    }
    if (matchesName && !matchesId)
    {
        nameMatches.Add(entity);  // Pure name matches only
    }
}
```

**Phase 2: Apply priority hierarchy** (lines 135-204)

The key change is using `allMatches` for name priority checks instead of `nameMatches`:

```csharp
// Priority 1: Check for exact name match across ALL matches
var exactNameMatches = allMatches.Where(e => 
    getName(e).Equals(query, StringComparison.OrdinalIgnoreCase)).ToList();

if (exactNameMatches.Count == 1)
{
    return (exactNameMatches[0], null); // Exact name match wins immediately
}

// Priority 2: Check for prefix matches across ALL matches  
var prefixMatches = allMatches.Where(e => 
    getName(e).StartsWith(query, StringComparison.OrdinalIgnoreCase)).ToList();

if (prefixMatches.Count == 1)
{
    return (prefixMatches[0], null); // Single prefix match wins
}

// Priority 3: Check for exact ID match
// Priority 4: Use shortest ID match if available
// Priority 5: Only name matches remain
```

### Key Improvements

1. **`allMatches` collection** - Contains every entity matching either name OR ID
2. **Name priority on ALL matches** - Both exact and prefix checks now evaluate `allMatches`
3. **Preserved ID logic** - The ID matching tiers (exact, shortest) remain unchanged
4. **Clean fallback** - Pure name substring matches still handled at the end

---

## Technical Details

### New Priority Hierarchy

The complete, correct priority order is now:

1. **Exact Name Match** (evaluated on `allMatches`)
   - Single exact match → return immediately
   - Multiple exact matches → ambiguity error with ID suggestion

2. **Prefix Name Match** (evaluated on `allMatches`)
   - Single prefix match → return immediately
   - Multiple prefix matches → ambiguity error with ID suggestion

3. **Exact ID Match** (evaluated on `idMatches`)
   - Single exact ID match → return immediately

4. **Shortest ID Match** (evaluated on `idMatches`)
   - Single shortest ID → return immediately
   - Multiple same-length shortest IDs → ambiguity error

5. **Substring Name Match** (evaluated on pure `nameMatches`)
   - Always returns ambiguity error (no auto-selection)

### Code Structure

**Method:** [`ResolveMultipleMatches<T>`](../Common/CommandBase.cs:99)  
**Lines:** 99-210  
**Parameters:** 
- `matches` - Initial list of matched entities
- `query` - User's search query
- `getStringId` - Function to extract entity ID
- `getName` - Function to extract entity name
- `formatDetails` - Function to format entity list for errors
- `entityType` - Entity type name for error messages

**Returns:** `(T entity, string error)` tuple

---

## Test Results

### Before Fix (Run 005)

**Results:** 108/113 tests passed (5 failures)

**Failing Tests:**
```
❌ id_matching_name_query_001
❌ name_priority_exact_001
❌ name_priority_exact_002
❌ name_priority_prefix_001_exact
❌ name_priority_case_001
```

### After Fix (Run 006+)

**Results:** 113/113 tests passed ✓

All name priority tests now pass, confirming:
- Exact name matches are prioritized correctly
- Prefix name matches work as expected
- ID matching serves as proper fallback
- Case-insensitive matching functions properly
- Mixed ID/name query scenarios resolve correctly

---

## Verification

### How to Verify the Fix

1. **Run Test Suite:**
   ```
   gm.test.run
   ```
   Expected: All 113 tests pass

2. **Manual Testing:**
   Test with a query that matches both name and ID:
   ```
   gm.hero.find emp
   ```
   - Should find "Empire" hero by name (if exists)
   - Should prioritize exact/prefix name match over ID substring match

3. **Review Specific Tests:**
   - [`NamePriorityTests.cs`](./NamePriorityTests.cs) - Contains all name priority test cases
   - [`Console_Test_Results_2025-12-14_006.txt`](./Results/Console_Test_Results_2025-12-14_006.txt) - Post-fix test results

### Regression Testing

To prevent future regressions:
- All name priority tests must continue passing
- Any changes to `ResolveMultipleMatches` must maintain the documented priority hierarchy
- Test coverage should include scenarios where query matches both name and ID

---

## Impact

### Behavior Changes

**Fixed Behavior:**
- Entities matching both name and ID now correctly participate in name priority checks
- Exact name matches always win, even if ID also matches
- Prefix name matches take priority over any ID-based matching

**No Breaking Changes:**
- Existing commands continue to work as before
- Error messages remain the same format
- API/interface unchanged

### Backward Compatibility

✓ **Fully Compatible** - This fix restores the originally intended behavior. No breaking changes to:
- Command syntax
- Return types
- Error handling
- Public methods

### Performance

- **No performance impact** - The fix reorganizes existing logic without adding computational complexity
- All operations remain O(n) where n is the number of matches
- Memory usage unchanged (same collections, different organization)

---

## Related Documentation

- [`NAME_PRIORITY_IMPLEMENTATION_SUMMARY.md`](./NAME_PRIORITY_IMPLEMENTATION_SUMMARY.md) - Original implementation details
- [`NAME_PRIORITY_TESTS.md`](./NAME_PRIORITY_TESTS.md) - Test case documentation  
- [`NAME_PRIORITY_VERIFICATION.md`](./NAME_PRIORITY_VERIFICATION.md) - Verification procedures
- [`TEST_FIXES_SUMMARY.md`](./TEST_FIXES_SUMMARY.md) - Historical test fixes

---

## Conclusion

This regression fix restores the correct functioning of the 3-tier name priority matching system by ensuring ALL matches are evaluated through name priority checks before falling back to ID-based resolution. The fix is minimal, focused, and fully backward compatible while resolving all 5 failing tests.

**Status:** ✅ Fixed and verified  
**Tests:** 113/113 passing  
**Regression Risk:** Low - restores intended behavior without introducing new logic