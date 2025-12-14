# Command Logging

The Game Master mod includes a command logging system that captures all console command outputs to a file for debugging, testing, and record-keeping purposes.

## Features

- **Automatic Logging**: When enabled, all console command outputs are automatically logged
- **Timestamped Entries**: Each log entry includes a timestamp for tracking when commands were executed
- **Success/Failure Tracking**: Logs include whether commands succeeded or failed
- **Session Markers**: Clear markers indicate when new logging sessions begin
- **File Management**: Commands to enable, disable, clear, and check status of the log

## Quick Start

### Enable Logging

```
gm.log.enable
```

This enables logging with the default location:
```
Documents\Mount and Blade II Bannerlord\Configs\GameMaster\command_log.txt
```

**Note:** Only commands that have been integrated with the logging system will be logged. The following commands currently support logging:
- `gm.query.hero` and variants
- `gm.test.*` commands
- `gm.log.*` commands

To add logging to other commands, see the Integration section below.

### Custom Log Path

```
gm.log.enable C:\MyLogs\commands.txt
```

### Check Status

```
gm.log.status
```

Shows:
- Whether logging is enabled
- Log file path
- File size
- Number of entries

### Disable Logging

```
gm.log.disable
```

### Clear Log

```
gm.log.clear
```

Empties the log file while keeping logging enabled.

## Log Format

Each log entry follows this format:

```
================================================================================
Timestamp: 2024-01-15 14:30:45
Command: gm.query.hero john lord
Status: SUCCESS
--------------------------------------------------------------------------------
Found 2 hero(es) matching search: 'john', types: lord:
  [lord_1_1] John Doe (Vlandia) - Clan: Doe Clan, Age: 35
  [lord_2_5] John Smith (Battania) - Clan: Smith Clan, Age: 42

================================================================================
```

## Available Commands

| Command | Description |
|---------|-------------|
| `gm.log.enable [path]` | Enable logging (optional custom path) |
| `gm.log.disable` | Disable logging |
| `gm.log.status` | Show logging status and statistics |
| `gm.log.clear` | Clear the log file |
| `gm.log.help` | Show help for logging commands |

## Integration with Existing Commands

**Important:** Commands need to be updated to use the logging helpers - logging is not automatic for all commands. This gives you control over which commands are logged and allows for proper command name formatting.

Commands can integrate logging by using helper methods in `CommandBase`:

### Method 1: String-based Commands (Recommended)

Wrap your entire command logic in `ExecuteWithLogging`:

```csharp
[CommandLineFunctionality.CommandLineArgumentFunction("mycommand", "gm")]
public static string MyCommand(List<string> args)
{
    // Build command name with arguments for better logging
    string commandName = "gm.mycommand" + (args != null && args.Count > 0 ? " " + string.Join(" ", args) : "");
    
    return CommandBase.ExecuteWithLogging(commandName, () =>
    {
        // Your existing command logic here - no changes needed
        if (Campaign.Current == null)
            return "Error: Must be in campaign mode.\n";
            
        // ... rest of your logic
        return "Success: Command completed.\n";
    });
}
```

**Example from HeroQueryCommands.cs:**

```csharp
[CommandLineFunctionality.CommandLineArgumentFunction("hero", "gm.query")]
public static string QueryHeroes(List<string> args)
{
    string commandName = "gm.query.hero" + (args != null && args.Count > 0 ? " " + string.Join(" ", args) : "");
    return CommandBase.ExecuteWithLogging(commandName, () =>
    {
        if (Campaign.Current == null)
            return "Error: Must be in campaign mode.\n";

        var (query, types, includeDead) = ParseArguments(args);
        List<Hero> matchedHeroes = HeroQueries.QueryHeroes(query, types, matchAll: true, includeDead: includeDead);
        
        if (matchedHeroes.Count == 0)
        {
            string criteria = BuildCriteriaString(query, types);
            return $"No heroes found matching ALL criteria: {criteria}\n" +
                   "Usage: gm.query.hero [search] [type keywords]\n";
        }

        string criteriaDesc = BuildCriteriaString(query, types);
        return $"Found {matchedHeroes.Count} hero(es) matching {criteriaDesc}:\n" +
               $"{HeroQueries.GetFormattedDetails(matchedHeroes)}";
    });
}
```

### Method 2: CommandResult-based Commands

```csharp
[CommandLineFunctionality.CommandLineArgumentFunction("mycommand", "gm")]
public static string MyCommand(List<string> args)
{
    return CommandBase.ExecuteWithLogging("gm.mycommand", () =>
    {
        // Your command logic here
        return CommandResult.Success("Command completed.");
    });
}
```

### Manual Logging

For more control, you can log manually:

```csharp
if (CommandLogger.IsEnabled)
{
    CommandLogger.LogCommand("gm.mycommand arg1 arg2", outputString, isSuccess);
}
```

## Use Cases

### Debugging

Enable logging when troubleshooting issues:
```
gm.log.enable
gm.query.hero john lord female
gm.clan.change_leader clan_1 hero_2
gm.log.status
```

Review the log file to see exact outputs and any errors.

### Testing

Use logging to record test results:
```
gm.log.enable C:\Tests\test_results.txt
gm.test.run_all
gm.log.disable
```

### Documentation

Capture command examples for documentation:
```
gm.log.enable C:\Docs\examples.txt
gm.query.hero wanderer
gm.query.clan battania
gm.log.disable
```

## Performance

- **Minimal Impact**: Logging uses asynchronous file writes and is thread-safe
- **Optional**: Logging is disabled by default and only runs when explicitly enabled
- **Efficient**: Only active when `CommandLogger.IsEnabled` is true

## File Management

The logger automatically:
- Creates the log directory if it doesn't exist
- Appends to existing log files (doesn't overwrite)
- Handles file locks and concurrent access safely
- Fails silently to avoid disrupting command execution

To prevent log files from growing too large, periodically:
1. Check the size: `gm.log.status`
2. Clear if needed: `gm.log.clear`
3. Or manually archive/delete the log file

## Notes

- The logger writes to disk after each command, ensuring data is saved even if the game crashes
- Session markers help identify when the game was restarted
- Commands that return errors are marked as "FAILED" in the log
- All timestamps use the local system time zone