# Troubleshooting

**Navigation:** [← Back: Testing Guide](testing.md) | [Back to Index](../README.md) | [Next: Code Quality Checklist →](../reference/code-quality-checklist.md)

---

## Command Not Found Issues

**Symptom:** Command doesn't appear in console autocomplete or returns "command not found"

**Common Causes:**

1. **Missing Command Attribute** - Verify the command has the correct `[CommandLineFunctionality.CommandLineArgumentFunction]` attribute
2. **Incorrect Command Path** - Check the hierarchy matches the attribute parameters
3. **Namespace Issues** - Ensure the command class is in the correct namespace and properly compiled
4. **Mod Not Loaded** - Verify the mod is enabled in the launcher and [`SubModule.xml`](../../Bannerlord.GameMaster/_Module/SubModule.xml) is configured correctly

**Solutions:**
- Check attribute spelling and hierarchy
- Rebuild the project (Clean → Rebuild)
- Restart the game to reload assemblies
- Check game logs for compilation errors

---

## Entity Resolution Issues

**Symptom:** "Multiple matches found" or "No entity found" errors

### Multiple Matches Returned

The system found more than one entity matching your query.

**Solutions:**
1. Use more specific search terms: `gm.hero.set_clan "lord smith"` instead of `gm.hero.set_clan lord`
2. Use the exact `StringId`: `gm.hero.set_clan lord_1_1 clan_empire_1`
3. Review the disambiguation output showing all matches

### No Matches Found

The entity doesn't exist or the query doesn't match any entity.

**Solutions:**
1. Verify the entity exists: `gm.query.hero` or `gm.query.clan`
2. Check for typos in the ID or name
3. For heroes, ensure you're searching alive heroes (or use `dead` keyword if needed)
4. Case doesn't matter, but spelling does

### Priority Resolution Not Working

**Verification:**
- Check [`CommandBase.ResolveMultipleMatches()`](../../Bannerlord.GameMaster/Console/Common/CommandBase.cs) for priority logic
- Test with: `gm.query.hero lord` to see if StringId matches are prioritized
- See [`NamePriorityTests`](../../Bannerlord.GameMaster/Console/Testing/NamePriorityTests.cs) for expected behavior

---

## Test Failures

### Understanding Test Output

Test results include:
- ✅ **PASS** - Test executed successfully and met expectations
- ❌ **FAIL** - Test ran but didn't meet expectations
- **Details** - What went wrong and actual vs. expected output

### Common Failure Patterns

**1. Validation Failures** - "Expected text not found" or "Unexpected text found"
```
Expected: "hero(es) matching"
Actual: "Error: Must be in campaign mode"
```
**Solution:** Test requires campaign mode but isn't running in one

**2. Execution Failures** - Command threw an exception
```
Exception: NullReferenceException at line X
```
**Solution:** Check for null references in command implementation

**3. Expected Output Mismatch** - Command returned different text
```
Expected: "Missing arguments"
Actual: "Error: Requires at least 2 arguments"
```
**Solution:** Update test's `ExpectedText` to match actual error message

### Debugging Test Logic

1. **Run test manually** in the console to see actual behavior
2. **Enable CommandLogger** to see detailed command execution
3. **Check test definition** in [`StandardTests.cs`](../../Bannerlord.GameMaster/Console/Testing/StandardTests.cs)
4. **Review test results file** in `Console/Testing/Results/` for full output

---

## Runtime Errors

### Null Reference Exceptions

**Common Causes:**
- Accessing properties on null entities
- Missing null checks before property access
- Entity collections returning null

**Solutions:**
```csharp
// ❌ Wrong - No null check
hero.Clan.Name.ToString()

// ✅ Correct - Safe navigation
hero.Clan?.Name?.ToString() ?? "None"
```

### State Modification Errors

**Symptom:** "Cannot modify" or "Operation not allowed" errors

**Common Causes:**
- Trying to modify dead heroes
- Trying to change immutable properties
- Game state doesn't allow the operation

**Solutions:**
- Add business logic validation before modification
- Check entity state (IsAlive, IsPrisoner, etc.)
- Use `ExecuteWithErrorHandling()` to catch and report errors gracefully

### Campaign Mode Check Failures

**Symptom:** "Must be in campaign mode" error

**Cause:** Command requires an active campaign but isn't running in one

**Solutions:**
1. Load a save or start a new campaign
2. Verify command has campaign mode check:
   ```csharp
   if (!CommandBase.ValidateCampaignMode(out string error))
       return error;
   ```
3. For tests, ensure `RequiresCampaign = true` in test definition

---

## Next Steps

1. **Review** [Code Quality Checklist](../reference/code-quality-checklist.md) before committing
2. **Check** [Architecture Analysis](../reference/architecture-analysis.md) for deeper understanding

---

**Navigation:** [← Back: Testing Guide](testing.md) | [Back to Index](../README.md) | [Next: Code Quality Checklist →](../reference/code-quality-checklist.md)