# Testing Your Console Commands

This document provides a quick guide to using the automated testing system for Game Master console commands.

## Quick Start

### In-Game Console Commands

1. **Start a campaign** in Mount & Blade II: Bannerlord
2. **Open the console** (typically with `~` or `` ` `` key)
3. **Register example tests:**
   ```
   gm.test.register_examples
   ```
4. **Run all tests:**
   ```
   gm.test.run_all
   ```
5. **View detailed results:**
   ```
   gm.test.last_results verbose
   ```

## Available Commands

```
gm.test.help                     - Show help
gm.test.register_examples        - Load example tests
gm.test.list                     - List all tests
gm.test.list HeroQuery           - List tests in category
gm.test.run_all                  - Run all tests
gm.test.run_category HeroQuery   - Run category tests
gm.test.run_single test_id       - Run specific test
gm.test.last_results             - Show summary
gm.test.last_results verbose     - Show details
gm.test.clear                    - Clear all tests
```

## Using Startup Launch Options

You can automatically run tests when the game starts using launch options:

### Steam Launch Options
Right-click game → Properties → Launch Options:
```
--console-command "gm.test.register_examples" --console-command "gm.test.run_all"
```

This will:
- Register example tests
- Run all tests automatically
- Display results in the console

## Auto-Run on Campaign Start

To automatically run tests every time you enter a campaign:

1. Open [`Bannerlord.GameMaster/SubModule.cs`](SubModule.cs)
2. Change line 15 from:
   ```csharp
   public static bool AutoRunTestsOnStartup => false;
   ```
   to:
   ```csharp
   public static bool AutoRunTestsOnStartup => true;
   ```
3. Rebuild the mod
4. Tests will now run automatically when entering campaign mode

## What Gets Tested

The standard tests validate:

### Query Commands
- ✓ Hero queries (by name, type, etc.)
- ✓ Clan queries (by name, kingdom, etc.)  
- ✓ Kingdom queries (by name, etc.)
- ✓ Invalid ID handling
- ✓ Missing parameter errors

### Management Commands
- ✓ Parameter validation
- ✓ Usage message display
- ✓ Error handling for invalid inputs

## Expected Results

When you run the example tests, you should see:
- **~40 tests** registered across 6 categories
- **100% pass rate** when run in a valid campaign
- Tests complete in **under 2 seconds** typically

Example output:
```
=== TEST RESULTS ===
Total Tests: 42
Passed: 42
Failed: 0
Success Rate: 100.0%
Total Time: 1847ms
```

## Creating Your Own Tests

See [`Console/Testing/README.md`](Console/Testing/README.md) for detailed documentation on:
- Writing custom tests
- Test expectations and validation
- Setup and cleanup commands
- Test organization and categories

## Troubleshooting

### "Must be in campaign mode" errors
Most commands require an active campaign. Start a campaign before running tests.

### "Command not found" errors
Ensure you're using the correct command syntax. Use `gm.test.list` to see available tests.

### All tests fail
Check that:
1. You're in campaign mode
2. The mod is properly loaded
3. Console commands are enabled

## Test Coverage

Current test coverage includes:
- **Hero Commands**: Query, management, relationships
- **Clan Commands**: Query, management, gold/renown
- **Kingdom Commands**: Query, management, wars/peace

## Learn More

For detailed information about the testing system architecture and advanced usage, see:
- [Testing System README](Console/Testing/README.md)
- [Test Case Definitions](Console/Testing/TestCase.cs)
- [Test Runner Implementation](Console/Testing/TestRunner.cs)
- [Standard Tests](Console/Testing/StandardTests.cs)
- [Integration Tests](Console/Testing/IntegrationTests.cs)

## Support

If you encounter issues:
1. Check this guide
2. Use `gm.test.help` in-game
3. Review the console output for error messages
4. Check the detailed README in the Testing directory