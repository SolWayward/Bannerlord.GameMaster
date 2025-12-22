# Command Argument Validation and Display Enhancement

**Date:** 2025-12-22
**Type:** Feature Enhancement
**Status:** Completed - All Core Commands Implemented

## Overview

Enhanced the command system to provide:
1. **Named Argument Validation** - Commands now validate that all named arguments match defined argument names (case-insensitive), and throw errors for unknown arguments
2. **Argument Display** - Commands display all argument values (both used and unused) before the command output, showing what values were resolved for each parameter

## Implementation Details

### 1. Extended ParsedArguments Class

Added new functionality to [`CommandBase.ParsedArguments`](Bannerlord.GameMaster/Console/Common/CommandBase.cs:564):

- **ArgumentDefinition class**: Defines valid arguments with name, required flag, default display, and aliases
- **SetValidArguments()**: Method to define valid arguments for a command
- **GetValidationError()**: Returns error message if unknown named arguments are found
- **FormatArgumentDisplay()**: Formats the argument display header showing all argument values

### 2. Validation Process

```csharp
// 1. Parse arguments
var parsedArgs = CommandBase.ParseArguments(args);

// 2. Define valid arguments
parsedArgs.SetValidArguments(
    new CommandBase.ArgumentDefinition("count", true),                          // Required
    new CommandBase.ArgumentDefinition("cultures", false, null, "culture"),     // Optional with alias
    new CommandBase.ArgumentDefinition("gender", false),                        // Optional
    new CommandBase.ArgumentDefinition("randomFactor", false, null, "random")   // Optional with alias
);

// 3. Check for validation errors
string validationError = parsedArgs.GetValidationError();
if (validationError != null)
    return CommandBase.FormatErrorMessage(validationError);
```

**Error Output Example:**
```
Error: Unknown named argument(s): randomfactr, cultur
Valid argument names: count, cultures/culture, gender, clan, randomFactor/random
```

### 3. Argument Display Format

Before the command output, a formatted line shows all resolved argument values:

**Format:**
- Required arguments in angle brackets: `<ArgumentName: Value>`
- Optional arguments in square brackets: `[ArgumentName: Value]`
- Arguments displayed in positional order as defined in usage

**Example Output:**
```
generate_lords <Count: 20> [Cultures: Vlandia, Sturgia] [Gender: Both] [Clan: Random] [RandomFactor: 0.7]

Success: Created 20 lord(s):
...
```

## Implementation Pattern

All commands should follow this pattern:

```csharp
public static string CommandName(List<string> args)
{
    return Cmd.Run(args, () =>
    {
        if (!CommandBase.ValidateCampaignMode(out string error))
            return error;

        var usageMessage = CommandValidator.CreateUsageMessage(...);

        // Parse arguments
        var parsedArgs = CommandBase.ParseArguments(args);
        
        // Define valid arguments with aliases
        parsedArgs.SetValidArguments(
            new CommandBase.ArgumentDefinition("arg1", true),              // Required
            new CommandBase.ArgumentDefinition("arg2", false, null, "a2"), // Optional with alias
            new CommandBase.ArgumentDefinition("arg3", false)              // Optional
        );

        // Validate named arguments
        string validationError = parsedArgs.GetValidationError();
        if (validationError != null)
            return CommandBase.FormatErrorMessage(validationError);

        // Validate minimum argument count
        if (parsedArgs.TotalCount < requiredCount)
            return usageMessage;

        // Parse required arguments
        string arg1 = parsedArgs.GetArgument("arg1", 0);
        if (arg1 == null)
            return CommandBase.FormatErrorMessage("Missing required argument 'arg1'.");

        // Parse optional arguments with defaults
        string arg2 = parsedArgs.GetArgument("arg2", 1) ?? parsedArgs.GetNamed("a2");
        // ... parse other arguments

        // Build resolved values dictionary
        var resolvedValues = new Dictionary<string, string>
        {
            { "arg1", parsedValue1.ToString() },
            { "arg2", parsedValue2 ?? "Default" },
            { "arg3", parsedValue3.ToString() }
        };

        // Format argument display
        string argumentDisplay = parsedArgs.FormatArgumentDisplay("command_name", resolvedValues);

        // Execute command logic
        return CommandBase.ExecuteWithErrorHandling(() =>
        {
            // ... command logic
            return argumentDisplay + CommandBase.FormatSuccessMessage("Success message");
        }, "Error context");
    });
}
```

## Completed Implementations

### HeroGenerationCommands.cs
All commands updated with validation and display:

1. **generate_lords**
   - Arguments: count (required), cultures/culture, gender, clan, randomFactor/random
   - Display: Shows resolved count, cultures, gender, clan, and randomFactor

2. **create_lord**
   - Arguments: name (required), cultures/culture, gender, clan, withParty, settlement, randomFactor/random
   - Display: Shows all resolved argument values including defaults

3. **create_companions**
   - Arguments: count (required), heroLeader/hero (required), cultures/culture, gender, randomFactor/random
   - Display: Shows resolved count, hero leader name, cultures, gender, and randomFactor

4. **rename**
   - Arguments: heroQuery/hero (required), name (required)
   - Display: Shows hero name and new name

## Pending Implementations

The following command files need the same updates:

1. **HeroManagementCommands.cs** - Hero modification commands
2. **ClanGenerationCommands.cs** - Clan generation commands
3. **ClanManagementCommands.cs** - Clan modification commands
4. **KingdomManagementCommands.cs** - Kingdom management commands
5. **SettlementManagementCommands.cs** - Settlement commands
6. **ItemManagementCommands.cs** - Item commands
7. **TroopManagementCommands.cs** - Troop commands
8. **Query Commands** (in Console/Query folder):
   - ClanQueryCommands.cs
   - CultureQueryCommands.cs
   - HeroQueryCommands.cs
   - ItemModifierQueryCommands.cs
   - ItemQueryCommands.cs
   - KingdomQueryCommands.cs
   - SettlementQueryCommands.cs
   - TroopQueryCommands.cs

## Benefits

1. **Better Error Messages**: Users get clear feedback when they mistype named arguments
2. **Transparency**: Users see exactly what values were used for each argument
3. **Debugging**: Makes it easier to diagnose issues with command execution
4. **Documentation**: The display serves as confirmation of what the command will do
5. **Case-Insensitive**: Named arguments are matched case-insensitively for better UX

## Example Use Cases

### Valid Named Argument
```
> gm.hero.generate_lords count:20 cultures:vlandia,sturgia randomFactor:0.7

generate_lords <Count: 20> [Cultures: Vlandia, Sturgia] [Gender: Both] [Clan: Random] [RandomFactor: 0.7]
Success: Created 20 lord(s):
...
```

### Invalid Named Argument (Typo)
```
> gm.hero.generate_lords count:20 culters:vlandia randomfactr:0.7

Error: Unknown named argument(s): culters, randomfactr
Valid argument names: count, cultures/culture, gender, clan, randomFactor/random
```

### Mixed Positional and Named
```
> gm.hero.generate_lords 20 cultures:vlandia gender:male

generate_lords <Count: 20> [Cultures: Vlandia] [Gender: Male] [Clan: Random] [RandomFactor: 1.0]
Success: Created 20 lord(s):
...
```

### Using Aliases
```
> gm.hero.generate_lords count:20 culture:vlandia random:0.5

generate_lords <Count: 20> [Cultures: Vlandia] [Gender: Both] [Clan: Random] [RandomFactor: 0.5]
Success: Created 20 lord(s):
...
```

## Testing

Build completed successfully with no errors or warnings:
```
dotnet build Bannerlord.GameMaster/Bannerlord.GameMaster.csproj
Build succeeded. 0 Warning(s) 0 Error(s)
```

## Notes

- All named argument matching is case-insensitive
- Aliases allow for shorter or alternative argument names
- The display format matches the standard command usage format (required in `<>`, optional in `[]`)
- Argument display is prepended to all command output (success or error)
- Default values are shown when arguments aren't specified
- The system gracefully handles both positional and named arguments simultaneously

## Migration Guide

To update a command:

1. Parse arguments with `CommandBase.ParseArguments(args)`
2. Call `SetValidArguments()` with all valid argument names and aliases
3. Check `GetValidationError()` and return error if not null
4. Parse all arguments (required and optional)
5. Build `resolvedValues` dictionary with actual resolved values
6. Call `FormatArgumentDisplay()` to get the display string
7. Prepend `argumentDisplay` to all return statements in the command logic

## Future Enhancements

Potential improvements:
- Auto-generate argument definitions from usage strings
- Support for argument value validation in ArgumentDefinition
- Support for argument type specifications (int, float, bool, string, etc.)
- Interactive prompt for missing required arguments
- Argument value suggestions based on valid options
