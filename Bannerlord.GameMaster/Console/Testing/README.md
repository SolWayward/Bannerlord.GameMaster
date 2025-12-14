# Game Master Test Automation System

This directory contains the automated testing infrastructure for Game Master console commands.

## Overview

The test automation system allows you to:
- Define test cases for console commands
- Execute tests automatically or on-demand
- Validate command outputs
- Track test results and generate reports
- Optionally auto-run tests on game startup

## Quick Start

### 1. Register Example Tests
```
gm.test.register_examples
```

### 2. List Available Tests
```
gm.test.list
```

### 3. Run All Tests
```
gm.test.run_all
```

### 4. View Results
```
gm.test.last_results verbose
```

## Console Commands

### Test Management
- `gm.test.register_examples` - Register example tests for demonstration
- `gm.test.list [category]` - List all registered tests (optionally filter by category)
- `gm.test.clear` - Clear all registered tests
- `gm.test.help` - Show help for test commands

### Running Tests
- `gm.test.run_all` - Run all registered tests
- `gm.test.run_category <category>` - Run tests in a specific category
- `gm.test.run_single <test_id>` - Run a specific test by ID

### Viewing Results
- `gm.test.last_results` - Show summary of last test run
- `gm.test.last_results verbose` - Show detailed results of last test run

## Test Categories

The example tests are organized into the following categories:

- **HeroQuery** - Tests for hero query commands (gm.query.hero, etc.)
- **ClanQuery** - Tests for clan query commands (gm.query.clan, etc.)
- **KingdomQuery** - Tests for kingdom query commands (gm.query.kingdom, etc.)
- **HeroManagement** - Tests for hero management commands (gm.hero.set_clan, etc.)
- **ClanManagement** - Tests for clan management commands (gm.clan.add_gold, etc.)
- **KingdomManagement** - Tests for kingdom management commands (gm.kingdom.add_clan, etc.)

## Test Expectations

Tests can validate different types of outcomes:

- **Success** - Command should return "Success:" message
- **Error** - Command should return "Error:" message
- **Contains** - Output should contain specific text
- **NotContains** - Output should NOT contain specific text
- **NoException** - Command should run without throwing an exception

## Creating Custom Tests

### Basic Test Definition

```csharp
var test = new TestCase(
    "my_test_001",                           // Unique test ID
    "Description of what this test does",    // Description
    "gm.query.hero aserai lord",            // Command to execute
    TestExpectation.Contains                 // Expected outcome
)
{
    Category = "MyTests",                    // Category for organization
    ExpectedText = "hero(es) matching",      // Text that should appear
    RequiresCampaign = true                  // Whether campaign is needed
};

TestRunner.RegisterTest(test);
```

### Advanced Test with Setup/Cleanup

```csharp
var test = new TestCase(
    "hero_gold_test",
    "Test setting hero gold",
    "gm.hero.set_gold lord_1_1 5000",
    TestExpectation.Success
)
{
    Category = "HeroManagement",
    SetupCommands = new List<string>
    {
        // Commands to run before test
        "gm.query.hero lord_1_1"
    },
    CleanupCommands = new List<string>
    {
        // Commands to run after test
        "gm.hero.set_gold lord_1_1 1000"
    }
};
```

## Auto-Run Tests on Startup

To automatically run tests when entering campaign mode, modify [`SubModule.cs`](../../SubModule.cs:15):

```csharp
public static bool AutoRunTestsOnStartup => true;  // Change to true
```

This will:
1. Automatically register example tests if none exist
2. Run all tests when a campaign starts
3. Display a summary in-game
4. Run tests only once per game session

## Architecture

### Core Components

1. **TestCase.cs** - Defines individual test cases with expectations
2. **TestResult.cs** - Stores results from test execution
3. **TestRunner.cs** - Executes tests and validates outcomes
4. **TestCommands.cs** - Console commands for running tests
5. **ExampleTests.cs** - Pre-defined example tests

### How It Works

1. Tests are registered in memory using [`TestRunner.RegisterTest()`](TestRunner.cs:40)
2. When executed, [`TestRunner`](TestRunner.cs:140) uses reflection to find and invoke command methods
3. The command's output is captured and validated against expectations
4. Results are stored and can be viewed/analyzed
5. Reports are generated showing pass/fail status and details

## Test Execution Flow

```
Register Tests → Run Tests → Execute Commands → Validate Output → Generate Report
```

Each test:
1. Validates prerequisites (e.g., campaign mode required)
2. Runs optional setup commands
3. Executes the test command
4. Captures output
5. Validates against expectations
6. Runs optional cleanup commands
7. Records results with timing information

## Example Test Scenarios

### Query Commands
Tests validate that query commands return expected formats and handle edge cases:
- Empty queries return all items
- Type filters work correctly
- Invalid IDs produce appropriate errors
- Info commands require valid IDs

### Management Commands
Tests validate that management commands:
- Require proper arguments
- Return usage information when arguments are missing
- Handle invalid entity references appropriately

## Tips for Writing Tests

1. **Use descriptive test IDs** - e.g., `hero_query_basic_001` instead of `test1`
2. **Write clear descriptions** - Explain what the test validates
3. **Choose appropriate expectations** - Match the command's expected behavior
4. **Group related tests** - Use categories to organize tests
5. **Test edge cases** - Include tests for invalid inputs, missing arguments, etc.
6. **Keep tests focused** - Each test should validate one specific behavior

## Troubleshooting

### Tests Fail with "Must be in campaign mode"
Start a campaign before running tests. Most commands require an active game.

### Command Not Found Errors
Ensure the command name in the test matches the actual console command exactly.

### Tests Pass Locally But Fail in Practice
Check if the game state affects the test. Some tests may need specific game conditions.

## Integration with CI/CD

The test system can be integrated with automated build processes:

1. Use launch options: `--console-command "gm.test.register_examples" --console-command "gm.test.run_all"`
2. Parse console output for test results
3. Exit code could be based on test success (future enhancement)

## Future Enhancements

Potential additions to the testing system:
- JSON/XML test case definitions
- Test result export to file
- Performance benchmarking
- Test coverage reports
- Parallel test execution
- Test dependency management
- Mocking/stubbing support

## Support

For questions or issues with the testing system:
1. Check this README
2. Use `gm.test.help` for command reference
3. Review standard tests in [`StandardTests.cs`](StandardTests.cs)
4. Examine test execution in [`TestRunner.cs`](TestRunner.cs)