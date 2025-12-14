# Command Logging Integration Guide

## Quick Integration - One Line Change

To add logging to any command, simply wrap it with `Cmd.Run()`:

### Before:
```csharp
[CommandLineFunctionality.CommandLineArgumentFunction("mycommand", "gm")]
public static string MyCommand(List<string> args)
{
    if (Campaign.Current == null)
        return "Error: Must be in campaign mode.\n";
    
    // ... your logic ...
    return "Success: Done.\n";
}
```

### After:
```csharp
[CommandLineFunctionality.CommandLineArgumentFunction("mycommand", "gm")]
public static string MyCommand(List<string> args)
{
    return Cmd.Run(args, () =>
    {
        if (Campaign.Current == null)
            return "Error: Must be in campaign mode.\n";
        
        // ... same logic, unchanged ...
        return "Success: Done.\n";
    });
}
```

That's it! The command name and arguments are automatically detected using reflection.

## What It Does

`Cmd.Run()` automatically:
- ✅ Extracts the command name from the attribute (e.g., "gm.query.hero")
- ✅ Appends arguments to the command name
- ✅ Logs the output when `CommandLogger.IsEnabled` is true
- ✅ Detects success/failure based on the output string
- ✅ Handles exceptions and logs them

## Currently Integrated Commands

### Query Commands (✅ All integrated)
- `gm.query.hero [args]`
- `gm.query.hero_any [args]`
- `gm.query.hero_info <id>`
- `gm.query.clan [args]`
- `gm.query.clan_any [args]`
- `gm.query.clan_info <id>`
- `gm.query.kingdom [args]`
- `gm.query.kingdom_any [args]`
- `gm.query.kingdom_info <id>`

### Test Commands (✅ Partially integrated)
- `gm.test.run_all`
- `gm.test.list [category]`

### Logger Commands (✅ All integrated)
- `gm.log.enable [path]`
- `gm.log.disable`
- `gm.log.status`
- `gm.log.clear`
- `gm.log.help`

## Commands To Integrate

### Hero Management Commands
Files: `HeroManagementCommands.cs`
- `gm.hero.set_clan`
- `gm.hero.remove_clan`
- `gm.hero.kill`
- `gm.hero.imprison`
- `gm.hero.release`
- `gm.hero.set_age`
- `gm.hero.set_gold`
- `gm.hero.add_gold`
- `gm.hero.heal`
- `gm.hero.set_relation`

### Clan Management Commands
File: `ClanManagementCommands.cs`
- `gm.clan.add_hero`
- `gm.clan.add_gold`
- `gm.clan.set_gold`
- `gm.clan.add_gold_leader`
- `gm.clan.give_gold`
- `gm.clan.set_renown`
- `gm.clan.add_renown`
- `gm.clan.set_tier`
- `gm.clan.destroy`
- `gm.clan.set_leader`

### Kingdom Management Commands
File: `KingdomManagementCommands.cs`
- `gm.kingdom.add_clan`
- `gm.kingdom.remove_clan`
- `gm.kingdom.declare_war`
- `gm.kingdom.make_peace`
- `gm.kingdom.give_fief`
- `gm.kingdom.give_fief_to_clan`
- `gm.kingdom.set_ruler`
- `gm.kingdom.destroy`

## Integration Steps

For each command file:

1. Add the using directive:
```csharp
using Bannerlord.GameMaster.Console.Common;
```

2. For each command method, wrap the body in `Cmd.Run()`:

**Pattern for simple commands:**
```csharp
public static string MyCommand(List<string> args)
{
    return Cmd.Run(args, () =>
    {
        // Existing command logic - no changes
        return result;
    });
}
```

**Pattern for commands using CommandBase.ExecuteWithErrorHandling:**
```csharp
// Before:
public static string MyCommand(List<string> args)
{
    if (!CommandBase.ValidateCampaignMode(out string error))
        return error;
    
    return CommandBase.ExecuteWithErrorHandling(() =>
    {
        // logic
        return CommandBase.FormatSuccessMessage("Done");
    }, "Failed");
}

// After:
public static string MyCommand(List<string> args)
{
    return Cmd.Run(args, () =>
    {
        if (!CommandBase.ValidateCampaignMode(out string error))
            return error;
        
        return CommandBase.ExecuteWithErrorHandling(() =>
        {
            // logic
            return CommandBase.FormatSuccessMessage("Done");
        }, "Failed");
    });
}
```

## Testing

After integrating a command:

1. Enable logging:
   ```
   gm.log.enable
   ```

2. Run the integrated command:
   ```
   gm.mycommand arg1 arg2
   ```

3. Check the log file (path shown by `gm.log.enable`):
   ```
   gm.log.status
   ```

4. Verify the command appears in the log with correct formatting:
   ```
   ================================================================================
   Timestamp: 2025-12-13 18:30:00
   Command: gm.mycommand arg1 arg2
   Status: SUCCESS
   --------------------------------------------------------------------------------
   [command output here]
   ```

## Benefits

- 📝 **Automatic Command Detection**: No need to manually specify command names
- 🎯 **Minimal Code Changes**: One-line wrapper around existing logic
- 🔒 **Non-Breaking**: Commands work the same whether logging is enabled or not
- 🚀 **Zero Performance Impact**: Only active when logging is explicitly enabled
- 🛠️ **Easy Debugging**: Full command history with timestamps and outputs
- ✅ **Status Tracking**: Automatic success/failure detection

## Performance Notes

- The reflection-based command name detection happens once per command execution
- Logging only writes to disk when `CommandLogger.IsEnabled` is true
- No performance impact when logging is disabled (default state)
- File I/O is synchronous but fast for typical command outputs