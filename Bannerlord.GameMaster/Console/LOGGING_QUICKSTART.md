# Command Logging Quick Start Guide

## Testing the Logger

Follow these steps to verify logging is working:

### Step 1: Enable Logging
```
gm.log.enable
```

You should see:
```
Success: Command logging enabled.
Log file: C:\Users\[YourUser]\Documents\Mount and Blade II Bannerlord\Configs\GameMaster\command_log.txt
```

### Step 2: Run a Logged Command
Try any of these commands that have logging integrated:

```
gm.query.hero lord
gm.query.hero wanderer
gm.test.list
gm.log.status
```

### Step 3: Check the Log File
Open the log file shown in step 1. You should see entries like:

```
================================================================================
Timestamp: 2025-12-13 18:15:30
Command: gm.query.hero lord
Status: SUCCESS
--------------------------------------------------------------------------------
Found 25 hero(es) matching search: '', types: lord, alive:
  [lord_1_1] Derthert (Vlandia) - Clan: derthert, Age: 45
  [lord_2_1] Caladog (Battania) - Clan: caladog, Age: 42
  ...

================================================================================
Timestamp: 2025-12-13 18:15:45
Command: gm.log.status
Status: SUCCESS
--------------------------------------------------------------------------------
Command Logger Status:
Status: ENABLED
Log File: C:\Users\Shadow\Documents\Mount and Blade II Bannerlord\Configs\GameMaster\command_log.txt
File Size: 2.45 KB
Log Entries: 3

```

## Currently Supported Commands

The following commands have logging integration:

### Query Commands
- ✅ `gm.query.hero [args]` - Query heroes
- ✅ `gm.query.hero_any [args]` - Query heroes (ANY match)
- ✅ `gm.query.hero_info <id>` - Get hero info

### Test Commands
- ✅ `gm.test.run_all` - Run all tests
- ✅ `gm.test.list [category]` - List tests

### Logger Commands
- ✅ `gm.log.enable [path]` - Enable logging
- ✅ `gm.log.disable` - Disable logging
- ✅ `gm.log.status` - Show status
- ✅ `gm.log.clear` - Clear log
- ✅ `gm.log.help` - Show help

## Adding Logging to Other Commands

Other commands (clan, kingdom management, etc.) don't have logging yet. To add logging to any command file:

1. Add this using statement at the top:
   ```csharp
   using Bannerlord.GameMaster.Console.Common;
   ```

2. Wrap the command method:
   ```csharp
   public static string MyCommand(List<string> args)
   {
       string commandName = "gm.mycommand" + (args != null && args.Count > 0 ? " " + string.Join(" ", args) : "");
       return CommandBase.ExecuteWithLogging(commandName, () =>
       {
           // Your existing command code here (no changes needed)
           return result;
       });
   }
   ```

That's it! The command will now automatically log when executed.

## Troubleshooting

**Problem:** Log file is created but empty
- **Solution:** Make sure you're running commands that have logging integration (see list above). Other commands need to be updated to use `ExecuteWithLogging`.

**Problem:** Can't find log file
- **Solution:** Run `gm.log.status` to see the exact path.

**Problem:** Want to log to a different location
- **Solution:** Use `gm.log.enable C:\MyCustomPath\commands.txt`

**Problem:** Log file is too large
- **Solution:** Run `gm.log.clear` to empty it, or manually delete the file.