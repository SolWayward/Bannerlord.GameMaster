# BLGM Console Commands Reference

A comprehensive reference for BLGM's 100+ console commands with 3-level path structure (gm.category.command). This document covers command infrastructure, all available commands organized by category, common patterns, and practical examples.

**Note:** This is a developer-focused companion to BLGM_API_QUICK_REFERENCE.md, specifically documenting the console command system.
  

If you are looking for the full console command documentation for **Users** instead use the following link:  
[BLGM User Documentation](https://github.com/SolWayward/Bannerlord.GameMaster/wiki)

---

## Table of Contents

1. [Command Infrastructure & Systems](#command-infrastructure--systems)
2. [Command Categories](#command-categories)
3. [Common Patterns & Usage](#common-patterns--usage)
4. [Quick Reference Examples](#quick-reference-examples)

---

## Command Infrastructure & Systems

### Argument Parsing System

The argument parsing system handles Bannerlord's console quirk where double quotes are stripped but single quotes are preserved. All argument parsing is centralized in [`ArgumentParser.cs`](Bannerlord.GameMaster/Console/Common/Parsing/ArgumentParser.cs).

#### Core Classes

**[`ArgumentParser.cs`](Bannerlord.GameMaster/Console/Common/Parsing/ArgumentParser.cs)**
- `ParseQuotedArguments(List<string> args)` - Reconstructs multi-word arguments wrapped in single quotes
  - Example: `["'Castle", "of", "Rocks'"]` → `["Castle of Rocks"]`
  - Handles both regular quoted arguments and named arguments with quoted values
  - Named argument example: `["name:'Sir", "Galahad'", "count:5"]` → `["name:Sir Galahad", "count:5"]`

- `JoinRemainingArgs(List<string> args, int startIndex)` - Joins remaining arguments from specified index
  - Useful for commands accepting multi-word text as final parameter

- `ParseArguments(List<string> args)` - Main entry point, returns `ParsedArguments` instance
  - Combines quote parsing with named/positional argument separation

**[`ParsedArguments.cs`](Bannerlord.GameMaster/Console/Common/Parsing/ParsedArguments.cs)**
- Stores and retrieves both named and positional arguments
- Methods:
  - `GetArgument(string name, int positionalIndex)` - Tries named first, falls back to positional
  - `GetNamed(string name)` - Returns only named argument
  - `GetPositional(int index)` - Returns positional argument at index
  - `GetString(string name, int positionalIndex, string default)` - With default value
  - `GetInt(string name, int positionalIndex, int default)` - Parse integer
  - `GetFloat(string name, int positionalIndex, float default)` - Parse float
  - `GetBool(string name, int positionalIndex, bool default)` - Parse boolean (true/false/yes/no/1/0/on/off)
  - `HasArgument(string name, int positionalIndex)` - Check if argument exists
  - `HasNamed(string name)` - Check if named argument exists
  - `SetValidArguments(params ArgumentDefinition[] definitions)` - Define valid arguments
  - `GetValidationError()` - Returns validation error if unknown named arguments exist
  - `FormatArgumentDisplay(string commandName, Dictionary<string, string> resolvedValues)` - Format argument header for output

**[`ArgumentDefinition.cs`](Bannerlord.GameMaster/Console/Common/Parsing/ArgumentDefinition.cs)**
- Defines argument requirements for validation and display
- Properties:
  - `Name` - Primary argument name used in named syntax (e.g., "count" for count:5)
  - `IsRequired` - Whether argument is required
  - `DefaultDisplay` - Display text for default value in usage messages
  - `Aliases` - List of alternative names (case-insensitive matching)

#### Named vs Positional Arguments

Commands support both named and positional argument styles:

```
Named: gm.clan.create_clan name:MyClone leader:Aragorn culture:vlandia
Positional: gm.clan.create_clan MyClone Aragorn vlandia
Mixed: gm.clan.create_clan name:MyClone Aragorn culture:vlandia
```

**Important:** Multi-word arguments must use single quotes:
```
gm.clan.create_clan name:'My Clan Name' leader:aragorn
gm.hero.rename aragorn name:'Sir Galahad'
```

---

### Validation System

All validation utilities are provided by [`CommandValidator.cs`](Bannerlord.GameMaster/Console/Common/Validation/CommandValidator.cs).

#### Campaign State Validation

```csharp
ValidateCampaignState(out string error)
```
- Checks campaign exists
- Verifies no conversation is in progress
- Validates no settlement ownership decisions are pending
- Called by most commands before execution

#### Argument Validation

```csharp
ValidateArgumentCount(List<string> args, int requiredCount, string usageMessage, out string error)
ValidateIntegerRange(string value, int min, int max, out int result, out string error)
ValidateFloatRange(string value, float min, float max, out float result, out string error)
ValidateBoolean(string value, out bool result, out string error)
```

#### Creation Limit Validation

```csharp
ValidateHeroCreationLimit(int countToCreate, out string error)
ValidateClanCreationLimit(int countToCreate, out string error)
ValidateKingdomCreationLimit(int countToCreate, out string error)
```

- Creation limits prevent performance degradation
- Can be bypassed with `gm.ignore_limits true`
- Limits tracked via `BLGMObjectManager`:
  - `BlgmHeroCount` / `maxBlgmHeroes`
  - `BlgmClanCount` / `maxBlgmClans`
  - `BlgmKingdomCount` / `maxBlgmKingdoms`

#### Usage Message Generation

```csharp
CreateUsageMessage(string commandName, string syntax, string description, string example = null)
```

Example:
```csharp
string usage = CommandValidator.CreateUsageMessage(
    "gm.hero.create_lord",
    "<name> [culture] [party_name]",
    "Creates a new lord hero with optional culture and party",
    "gm.hero.create_lord 'Sir Galahad' vlandia 'Galahad Party'"
);
```

---

### Entity Finding System

Smart entity resolution is provided by [`EntityFinder.cs`](Bannerlord.GameMaster/Console/Common/EntityFinding/EntityFinder.cs) and type-specific finders (HeroFinder, ClanFinder, etc.).

#### Priority-Based Matching

When multiple entities match a query, resolution follows this priority:

1. **Exact Name Match** - Entity name exactly equals query (case-insensitive)
2. **Name Prefix Match** - Entity name starts with query (only if single match overall)
3. **Exact ID Match** - Entity StringId exactly equals query
4. **ID Prefix Match** - Entity StringId starts with query
5. **Shortest ID Match** - If multiple ID matches, select entity with shortest ID
6. **Substring Matches** - Last resort, but returns error with multiple matches

#### Type-Specific Finders

Each entity type has dedicated finder class:

| Class | Location | Purpose |
|-------|----------|---------|
| [`HeroFinder`](Bannerlord.GameMaster/Console/Common/EntityFinding/HeroFinder.cs) | Common/EntityFinding | Find heroes by name/ID |
| [`ClanFinder`](Bannerlord.GameMaster/Console/Common/EntityFinding/ClanFinder.cs) | Common/EntityFinding | Find clans by name/ID |
| [`KingdomFinder`](Bannerlord.GameMaster/Console/Common/EntityFinding/KingdomFinder.cs) | Common/EntityFinding | Find kingdoms by name/ID |
| [`SettlementFinder`](Bannerlord.GameMaster/Console/Common/EntityFinding/SettlementFinder.cs) | Common/EntityFinding | Find settlements by name/ID |
| [`ItemFinder`](Bannerlord.GameMaster/Console/Common/EntityFinding/ItemFinder.cs) | Common/EntityFinding | Find items by name/ID |
| [`TroopFinder`](Bannerlord.GameMaster/Console/Common/EntityFinding/TroopFinder.cs) | Common/EntityFinding | Find troops by name/ID |

#### Return Type

All finders return [`EntityFinderResult<T>`](Bannerlord.GameMaster/Console/Common/EntityFinding/EntityFinderResult.cs):

```csharp
public class EntityFinderResult<T>
{
    public T Entity { get; }
    public bool IsSuccess { get; }
    public string ErrorMessage { get; }
    
    public static EntityFinderResult<T> Success(T entity) { ... }
    public static EntityFinderResult<T> Error(string message) { ... }
}
```

---

### Formatting & Output System

#### Message Formatting

[`MessageFormatter.cs`](Bannerlord.GameMaster/Console/Common/Formatting/MessageFormatter.cs) provides consistent output styling:

```csharp
MessageFormatter.FormatSuccessMessage(string message)
MessageFormatter.FormatErrorMessage(string message)
```

Both add consistent prefixes and trailing newlines.

#### Column Formatting

[`ColumnFormatter.cs`](Bannerlord.GameMaster/Console/Common/Formatting/ColumnFormatter.cs) formats query results into aligned columns:

```csharp
ColumnFormatter<Hero> formatter = new();
formatter
    .AddColumn(h => h.Name.ToString())
    .AddColumn(h => h.StringId)
    .AddColumn(h => h.Clan?.Name.ToString() ?? "None");

string output = formatter.Format(heroes);
```

Or use static helper:
```csharp
string output = ColumnFormatter<Hero>.FormatList(heroes,
    h => h.Name.ToString(),
    h => h.StringId,
    h => h.Gold.ToString()
);
```

---

### Command Execution System

[`CommandExecutor.cs`](Bannerlord.GameMaster/Console/Common/Execution/CommandExecutor.cs) (aliased as `Cmd`) handles command execution with automatic logging and error handling.

#### Cmd.Run Methods

```csharp
// String return type
public static string Run(List<string> args, Func<string> action)

// CommandResult return type
public static CommandResult Run(List<string> args, Func<CommandResult> action)
```

Both variants:
- Automatically parse quoted arguments
- Replace args list with parsed version
- Extract command name via reflection
- Log commands to debug file if enabled
- Catch exceptions and log them

#### Logging

[`CommandLogger.cs`](Bannerlord.GameMaster/Console/Common/Execution/CommandLogger.cs) provides debug logging:

```csharp
CommandLogger.IsEnabled // Check if logging enabled
CommandLogger.Log(string message) // Manual logging
CommandLogger.LogCommand(string commandName, string result, bool isSuccess)
CommandLogger.HandleAndLogException(string commandName, Exception ex)
```

[`LoggingManager.cs`](Bannerlord.GameMaster/Console/Common/Execution/LoggingManager.cs) manages log file operations.

---

## Command Categories

### 1. Bandit Commands (gm.bandit)

Manage bandits and their hideouts.

| Command | File | Purpose | Arguments |
|---------|------|---------|-----------|
| `gm.bandit.clear_hideouts` | [`ClearHideoutsCommand.cs`](Bannerlord.GameMaster/Console/BanditCommands/BanditManagementCommands/ClearHideoutsCommand.cs) | Clear all bandit hideouts | None |
| `gm.bandit.count` | [`CountCommand.cs`](Bannerlord.GameMaster/Console/BanditCommands/BanditManagementCommands/CountCommand.cs) | Count total bandits | None |
| `gm.bandit.destroy_parties` | [`DestroyBanditPartiesCommand.cs`](Bannerlord.GameMaster/Console/BanditCommands/BanditManagementCommands/DestroyBanditPartiesCommand.cs) | Destroy all bandit parties | None |
| `gm.bandit.remove_all` | [`RemoveAllBanditsCommand.cs`](Bannerlord.GameMaster/Console/BanditCommands/BanditManagementCommands/RemoveAllBanditsCommand.cs) | Remove all bandits from game | None |

---

### 2. Caravan Commands (gm.caravan)

Manage caravans for player, NPCs, and notables.

#### Creation

| Command | File | Purpose | Arguments |
|---------|------|---------|-----------|
| `gm.caravan.create_notable_caravan` | [`CreateNotableCaravanCommand.cs`](Bannerlord.GameMaster/Console/CaravanCommands/CaravanCreationCommands/CreateNotableCaravanCommand.cs) | Create caravan for notable hero | hero, culture (optional) |
| `gm.caravan.create_player_caravan` | [`CreatePlayerCaravanCommand.cs`](Bannerlord.GameMaster/Console/CaravanCommands/CaravanCreationCommands/CreatePlayerCaravanCommand.cs) | Create caravan for player | name (optional) |

#### Management

| Command | File | Purpose | Arguments |
|---------|------|---------|-----------|
| `gm.caravan.count` | [`CountCaravansCommand.cs`](Bannerlord.GameMaster/Console/CaravanCommands/CaravanManagementCommands/CountCaravansCommand.cs) | Count total caravans | None |
| `gm.caravan.disband` | [`DisbandCaravansCommand.cs`](Bannerlord.GameMaster/Console/CaravanCommands/CaravanManagementCommands/DisbandCaravansCommand.cs) | Disband all caravans | None |
| `gm.caravan.disband_notable` | [`DisbandNotableCaravansCommand.cs`](Bannerlord.GameMaster/Console/CaravanCommands/CaravanManagementCommands/DisbandNotableCaravansCommand.cs) | Disband notable hero caravans | None |
| `gm.caravan.disband_npc_lord` | [`DisbandNpcLordCaravansCommand.cs`](Bannerlord.GameMaster/Console/CaravanCommands/CaravanManagementCommands/DisbandNpcLordCaravansCommand.cs) | Disband NPC lord caravans | None |
| `gm.caravan.disband_player` | [`DisbandPlayerCaravansCommand.cs`](Bannerlord.GameMaster/Console/CaravanCommands/CaravanManagementCommands/DisbandPlayerCaravansCommand.cs) | Disband player caravans | None |
| `gm.caravan.force_destroy_disbanding` | [`ForceDestroyDisbandingCaravansCommand.cs`](Bannerlord.GameMaster/Console/CaravanCommands/CaravanManagementCommands/ForceDestroyDisbandingCaravansCommand.cs) | Force destroy disbanding caravans | None |

---

### 3. Clan Commands (gm.clan)

Create and manage clans.

#### Generation

| Command | File | Purpose | Arguments |
|---------|------|---------|-----------|
| `gm.clan.create_clan` | [`CreateClanCommand.cs`](Bannerlord.GameMaster/Console/ClanCommands/ClanGenerationCommands/CreateClanCommand.cs) | Create new noble clan | name, leader (optional), initial_gold (optional), renown (optional), tier (optional) |
| `gm.clan.create_minor_clan` | [`CreateMinorClanCommand.cs`](Bannerlord.GameMaster/Console/ClanCommands/ClanGenerationCommands/CreateMinorClanCommand.cs) | Create minor clan | name, initial_gold (optional) |
| `gm.clan.generate_clans` | [`GenerateClansCommand.cs`](Bannerlord.GameMaster/Console/ClanCommands/ClanGenerationCommands/GenerateClansCommand.cs) | Generate multiple clans | count, culture (optional, supports all_cultures/main_cultures/etc) |

#### Management

| Command | File | Purpose | Arguments |
|---------|------|---------|-----------|
| `gm.clan.add_gold` | [`AddClanGoldCommand.cs`](Bannerlord.GameMaster/Console/ClanCommands/ClanManagementCommands/AddClanGoldCommand.cs) | Add gold to clan | clan, gold_amount |
| `gm.clan.add_renown` | [`AddClanRenownCommand.cs`](Bannerlord.GameMaster/Console/ClanCommands/ClanManagementCommands/AddClanRenownCommand.cs) | Add renown to clan | clan, renown_amount |
| `gm.clan.add_gold_to_leader` | [`AddGoldToLeaderCommand.cs`](Bannerlord.GameMaster/Console/ClanCommands/ClanManagementCommands/AddGoldToLeaderCommand.cs) | Add gold to clan leader | clan, gold_amount |
| `gm.clan.add_hero_to_clan` | [`AddHeroToClanCommand.cs`](Bannerlord.GameMaster/Console/ClanCommands/ClanManagementCommands/AddHeroToClanCommand.cs) | Add hero to clan | hero, clan |
| `gm.clan.destroy_clan` | [`DestroyClanCommand.cs`](Bannerlord.GameMaster/Console/ClanCommands/ClanManagementCommands/DestroyClanCommand.cs) | Destroy clan | clan |
| `gm.clan.give_gold_to_member` | [`GiveGoldToMemberCommand.cs`](Bannerlord.GameMaster/Console/ClanCommands/ClanManagementCommands/GiveGoldToMemberCommand.cs) | Give gold to specific clan member | hero, gold_amount |
| `gm.clan.rename_clan` | [`RenameClanCommand.cs`](Bannerlord.GameMaster/Console/ClanCommands/ClanManagementCommands/RenameClanCommand.cs) | Rename clan | clan, new_name |
| `gm.clan.set_clan_gold` | [`SetClanGoldCommand.cs`](Bannerlord.GameMaster/Console/ClanCommands/ClanManagementCommands/SetClanGoldCommand.cs) | Set clan gold to exact amount | clan, gold_amount |
| `gm.clan.set_clan_leader` | [`SetClanLeaderCommand.cs`](Bannerlord.GameMaster/Console/ClanCommands/ClanManagementCommands/SetClanLeaderCommand.cs) | Change clan leader | clan, hero |
| `gm.clan.set_clan_renown` | [`SetClanRenownCommand.cs`](Bannerlord.GameMaster/Console/ClanCommands/ClanManagementCommands/SetClanRenownCommand.cs) | Set clan renown to exact amount | clan, renown_amount |
| `gm.clan.set_clan_tier` | [`SetClanTierCommand.cs`](Bannerlord.GameMaster/Console/ClanCommands/ClanManagementCommands/SetClanTierCommand.cs) | Set clan tier level | clan, tier (1-6) |
| `gm.clan.set_culture` | [`SetCultureCommand.cs`](Bannerlord.GameMaster/Console/ClanCommands/ClanManagementCommands/SetCultureCommand.cs) | Change clan culture | clan, culture |

---

### 4. Cleanup Commands (gm.cleanup)

Remove BLGM-created entities in bulk.

| Command | File | Purpose | Arguments |
|---------|------|---------|-----------|
| `gm.cleanup.batch_remove_blgm_clans` | [`BatchRemoveBlgmClansCommand.cs`](Bannerlord.GameMaster/Console/CleanupCommands/BatchRemoveBlgmClansCommand.cs) | Remove all BLGM clans | None |
| `gm.cleanup.batch_remove_blgm_kingdoms` | [`BatchRemoveBlgmKingdomsCommand.cs`](Bannerlord.GameMaster/Console/CleanupCommands/BatchRemoveBlgmKingdomsCommand.cs) | Remove all BLGM kingdoms | None |
| `gm.cleanup.batch_remove_blgm_parties` | [`BatchRemoveBlgmPartiesCommand.cs`](Bannerlord.GameMaster/Console/CleanupCommands/BatchRemoveBlgmPartiesCommand.cs) | Remove all BLGM parties | None |
| `gm.cleanup.batch_remove_heroes` | [`BatchRemoveHeroesCommand.cs`](Bannerlord.GameMaster/Console/CleanupCommands/BatchRemoveHeroesCommand.cs) | Remove all BLGM heroes | None |
| `gm.cleanup.remove_blgm_clan` | [`RemoveBlgmClanCommand.cs`](Bannerlord.GameMaster/Console/CleanupCommands/RemoveBlgmClanCommand.cs) | Remove specific BLGM clan | clan |
| `gm.cleanup.remove_blgm_hero` | [`RemoveBlgmHeroCommand.cs`](Bannerlord.GameMaster/Console/CleanupCommands/RemoveBlgmHeroCommand.cs) | Remove specific BLGM hero | hero |
| `gm.cleanup.remove_blgm_kingdom` | [`RemoveBlgmKingdomCommand.cs`](Bannerlord.GameMaster/Console/CleanupCommands/RemoveBlgmKingdomCommand.cs) | Remove specific BLGM kingdom | kingdom |

---

### 5. General Commands (gm)

Global BLGM settings.

| Command | File | Purpose | Arguments |
|---------|------|---------|-----------|
| `gm.ignore_limits` | [`IgnoreLimitsCommand.cs`](Bannerlord.GameMaster/Console/GeneralCommands/IgnoreLimitsCommand.cs) | Enable/disable creation limits bypass | true/false |
| `gm.show_system_console` | [`ShowSystemConsoleCommand.cs`](Bannerlord.GameMaster/Console/GeneralCommands/ShowSystemConsoleCommand.cs) | Opens the System Console window | None |

#### gm.show_system_console Details

The System Console displays results and output of in-game BLGM commands and other debug info/errors. Features:

- Opens a native Windows console window for viewing command results
- Displays output of all BLGM commands and debug information
- Allows running any game console command including BLGM commands from the Windows console window outside the game
- Can also be opened on launch by running Bannerlord with `/systemconsole` command line option when BLGM mod is enabled
- The in-game console remains fully usable - System Console use is entirely optional

---

### 6. Hero Commands (gm.hero)

Create and manage heroes.

#### Generation

| Command | File | Purpose | Arguments |
|---------|------|---------|-----------|
| `gm.hero.create_companions` | [`CreateCompanionsCommand.cs`](Bannerlord.GameMaster/Console/HeroCommands/HeroGenerationCommands/CreateCompanionsCommand.cs) | Create companion heroes | count, culture (optional, supports all_cultures/main_cultures/etc) |
| `gm.hero.create_lord` | [`CreateLordCommand.cs`](Bannerlord.GameMaster/Console/HeroCommands/HeroGenerationCommands/CreateLordCommand.cs) | Create lord hero with party | name (optional), culture (optional), party_name (optional) |
| `gm.hero.generate_lords` | [`GenerateLordsCommand.cs`](Bannerlord.GameMaster/Console/HeroCommands/HeroGenerationCommands/GenerateLordsCommand.cs) | Generate multiple lord heroes | count, culture (optional, supports all_cultures/main_cultures/etc) |

#### Management

| Command | File | Purpose | Arguments |
|---------|------|---------|-----------|
| `gm.hero.add_gold` | [`AddGoldCommand.cs`](Bannerlord.GameMaster/Console/HeroCommands/HeroManagementCommands/AddGoldCommand.cs) | Add gold to hero | hero, gold_amount |
| `gm.hero.add_hero_to_party` | [`AddHeroToPartyCommand.cs`](Bannerlord.GameMaster/Console/HeroCommands/HeroManagementCommands/AddHeroToPartyCommand.cs) | Add hero to party | hero, party |
| `gm.hero.create_party` | [`CreatePartyCommand.cs`](Bannerlord.GameMaster/Console/HeroCommands/HeroManagementCommands/CreatePartyCommand.cs) | Create party for hero | hero, party_name (optional) |
| `gm.hero.heal` | [`HealHeroCommand.cs`](Bannerlord.GameMaster/Console/HeroCommands/HeroManagementCommands/HealHeroCommand.cs) | Fully heal hero and party | hero |
| `gm.hero.imprison` | [`ImprisonHeroCommand.cs`](Bannerlord.GameMaster/Console/HeroCommands/HeroManagementCommands/ImprisonHeroCommand.cs) | Imprison hero | hero |
| `gm.hero.kill` | [`KillHeroCommand.cs`](Bannerlord.GameMaster/Console/HeroCommands/HeroManagementCommands/KillHeroCommand.cs) | Kill hero | hero |
| `gm.hero.release` | [`ReleaseHeroCommand.cs`](Bannerlord.GameMaster/Console/HeroCommands/HeroManagementCommands/ReleaseHeroCommand.cs) | Release imprisoned hero | hero |
| `gm.hero.remove_clan` | [`RemoveClanCommand.cs`](Bannerlord.GameMaster/Console/HeroCommands/HeroManagementCommands/RemoveClanCommand.cs) | Remove hero from clan | hero |
| `gm.hero.rename` | [`RenameHeroCommand.cs`](Bannerlord.GameMaster/Console/HeroCommands/HeroManagementCommands/RenameHeroCommand.cs) | Rename hero | hero, new_name |
| `gm.hero.set_age` | [`SetAgeCommand.cs`](Bannerlord.GameMaster/Console/HeroCommands/HeroManagementCommands/SetAgeCommand.cs) | Set hero age | hero, age |
| `gm.hero.set_clan` | [`SetClanCommand.cs`](Bannerlord.GameMaster/Console/HeroCommands/HeroManagementCommands/SetClanCommand.cs) | Add hero to clan | hero, clan |
| `gm.hero.set_culture` | [`SetCultureCommand.cs`](Bannerlord.GameMaster/Console/HeroCommands/HeroManagementCommands/SetCultureCommand.cs) | Change hero culture | hero, culture |
| `gm.hero.set_gold` | [`SetGoldCommand.cs`](Bannerlord.GameMaster/Console/HeroCommands/HeroManagementCommands/SetGoldCommand.cs) | Set hero gold to exact amount | hero, gold_amount |
| `gm.hero.set_relation` | [`SetRelationCommand.cs`](Bannerlord.GameMaster/Console/HeroCommands/HeroManagementCommands/SetRelationCommand.cs) | Set hero relation level | hero, target_hero, relation_value |

---

### 7. Info Commands (gm.info)

Display game and BLGM information.

| Command | File | Purpose | Arguments |
|---------|------|---------|-----------|
| `gm.info.bannerlord_version` | [`BannerlordVersionCommand.cs`](Bannerlord.GameMaster/Console/InfoCommands/BannerlordVersionCommand.cs) | Show game version | None |
| `gm.info.blgm_object_count` | [`BlgmObjectCountCommand.cs`](Bannerlord.GameMaster/Console/InfoCommands/BlgmObjectCountCommand.cs) | Show BLGM object counts | None |
| `gm.info.blgm_version` | [`BlgmVersionCommand.cs`](Bannerlord.GameMaster/Console/InfoCommands/BlgmVersionCommand.cs) | Show BLGM version | None |
| `gm.info.list_mods` | [`ListModsCommand.cs`](Bannerlord.GameMaster/Console/InfoCommands/ListModsCommand.cs) | List loaded mods | None |
| `gm.info.list_mods_launch` | [`ListModsLaunchCommand.cs`](Bannerlord.GameMaster/Console/InfoCommands/ListModsLaunchCommand.cs) | List mods at launch | None |

---

### 8. Item Commands (gm.item)

Manage hero equipment and inventory.

#### Equipment Management

| Command | File | Purpose | Arguments |
|---------|------|---------|-----------|
| `gm.item.equip` | [`EquipItemCommand.cs`](Bannerlord.GameMaster/Console/ItemCommands/EquipmentManagementCommands/EquipItemCommand.cs) | Equip item on hero | hero, item |
| `gm.item.equip_slot` | [`EquipSlotCommand.cs`](Bannerlord.GameMaster/Console/ItemCommands/EquipmentManagementCommands/EquipSlotCommand.cs) | Equip specific item slot | hero, slot, item |
| `gm.item.list_equipped` | [`ListEquippedCommand.cs`](Bannerlord.GameMaster/Console/ItemCommands/EquipmentManagementCommands/ListEquippedCommand.cs) | List hero equipped items | hero |
| `gm.item.load_equipment` | [`LoadEquipmentCommand.cs`](Bannerlord.GameMaster/Console/ItemCommands/EquipmentManagementCommands/LoadEquipmentCommand.cs) | Load saved equipment | hero, save_name |
| `gm.item.load_equipment_both` | [`LoadEquipmentBothCommand.cs`](Bannerlord.GameMaster/Console/ItemCommands/EquipmentManagementCommands/LoadEquipmentBothCommand.cs) | Load battle and civilian equipment | hero, save_name |
| `gm.item.load_equipment_civilian` | [`LoadEquipmentCivilianCommand.cs`](Bannerlord.GameMaster/Console/ItemCommands/EquipmentManagementCommands/LoadEquipmentCivilianCommand.cs) | Load civilian equipment | hero, save_name |
| `gm.item.remove_equipped` | [`RemoveEquippedCommand.cs`](Bannerlord.GameMaster/Console/ItemCommands/EquipmentManagementCommands/RemoveEquippedCommand.cs) | Remove all equipped items | hero |
| `gm.item.remove_equipped_modifier` | [`RemoveEquippedModifierCommand.cs`](Bannerlord.GameMaster/Console/ItemCommands/EquipmentManagementCommands/RemoveEquippedModifierCommand.cs) | Remove modifier from equipped item | hero, slot |
| `gm.item.save_equipment` | [`SaveEquipmentCommand.cs`](Bannerlord.GameMaster/Console/ItemCommands/EquipmentManagementCommands/SaveEquipmentCommand.cs) | Save hero equipment to file | hero, save_name |
| `gm.item.save_equipment_both` | [`SaveEquipmentBothCommand.cs`](Bannerlord.GameMaster/Console/ItemCommands/EquipmentManagementCommands/SaveEquipmentBothCommand.cs) | Save battle and civilian equipment | hero, save_name |
| `gm.item.save_equipment_civilian` | [`SaveEquipmentCivilianCommand.cs`](Bannerlord.GameMaster/Console/ItemCommands/EquipmentManagementCommands/SaveEquipmentCivilianCommand.cs) | Save civilian equipment | hero, save_name |
| `gm.item.set_equipped_modifier` | [`SetEquippedModifierCommand.cs`](Bannerlord.GameMaster/Console/ItemCommands/EquipmentManagementCommands/SetEquippedModifierCommand.cs) | Apply modifier to equipped item | hero, slot, modifier |
| `gm.item.unequip_all` | [`UnequipAllCommand.cs`](Bannerlord.GameMaster/Console/ItemCommands/EquipmentManagementCommands/UnequipAllCommand.cs) | Unequip all items | hero |
| `gm.item.unequip_item` | [`UnequipItemCommand.cs`](Bannerlord.GameMaster/Console/ItemCommands/EquipmentManagementCommands/UnequipItemCommand.cs) | Unequip specific item | hero, item |
| `gm.item.unequip_slot` | [`UnequipSlotCommand.cs`](Bannerlord.GameMaster/Console/ItemCommands/EquipmentManagementCommands/UnequipSlotCommand.cs) | Unequip specific slot | hero, slot |

#### Item Management

| Command | File | Purpose | Arguments |
|---------|------|---------|-----------|
| `gm.item.add_item` | [`AddItemCommand.cs`](Bannerlord.GameMaster/Console/ItemCommands/ItemManagementCommands/AddItemCommand.cs) | Add item to hero inventory | hero, item, count (optional) |
| `gm.item.remove_all_items` | [`RemoveAllItemsCommand.cs`](Bannerlord.GameMaster/Console/ItemCommands/ItemManagementCommands/RemoveAllItemsCommand.cs) | Remove all items from hero | hero |
| `gm.item.remove_item` | [`RemoveItemCommand.cs`](Bannerlord.GameMaster/Console/ItemCommands/ItemManagementCommands/RemoveItemCommand.cs) | Remove specific item | hero, item, count (optional) |
| `gm.item.set_inventory_modifier` | [`SetInventoryModifierCommand.cs`](Bannerlord.GameMaster/Console/ItemCommands/ItemManagementCommands/SetInventoryModifierCommand.cs) | Apply modifier to inventory item | hero, item, modifier |
| `gm.item.transfer_item` | [`TransferItemCommand.cs`](Bannerlord.GameMaster/Console/ItemCommands/ItemManagementCommands/TransferItemCommand.cs) | Transfer item between heroes | from_hero, to_hero, item, count (optional) |

---

### 9. Kingdom Commands (gm.kingdom)

Create and manage kingdoms.

#### Generation

| Command | File | Purpose | Arguments |
|---------|------|---------|-----------|
| `gm.kingdom.create_kingdom` | [`CreateKingdomCommand.cs`](Bannerlord.GameMaster/Console/KingdomCommands/KingdomGenerationCommands/CreateKingdomCommand.cs) | Create new kingdom | name, ruler, primary_culture (optional), color1 (optional), color2 (optional) |
| `gm.kingdom.generate_kingdoms` | [`GenerateKingdomsCommand.cs`](Bannerlord.GameMaster/Console/KingdomCommands/KingdomGenerationCommands/GenerateKingdomsCommand.cs) | Generate multiple kingdoms | count, culture (optional, supports all_cultures/main_cultures/etc) |

#### Management

| Command | File | Purpose | Arguments |
|---------|------|---------|-----------|
| `gm.kingdom.add_clan` | [`AddClanCommand.cs`](Bannerlord.GameMaster/Console/KingdomCommands/KingdomManagementCommands/AddClanCommand.cs) | Add clan to kingdom | kingdom, clan |
| `gm.kingdom.destroy_kingdom` | [`DestroyKingdomCommand.cs`](Bannerlord.GameMaster/Console/KingdomCommands/KingdomManagementCommands/DestroyKingdomCommand.cs) | Destroy kingdom | kingdom |
| `gm.kingdom.remove_clan` | [`RemoveClanCommand.cs`](Bannerlord.GameMaster/Console/KingdomCommands/KingdomManagementCommands/RemoveClanCommand.cs) | Remove clan from kingdom | kingdom, clan |
| `gm.kingdom.set_ruler` | [`SetRulerCommand.cs`](Bannerlord.GameMaster/Console/KingdomCommands/KingdomManagementCommands/SetRulerCommand.cs) | Change kingdom ruler | kingdom, hero |

#### Diplomacy

| Command | File | Purpose | Arguments |
|---------|------|---------|-----------|
| `gm.kingdom.call_ally_to_war` | [`CallAllyToWarCommand.cs`](Bannerlord.GameMaster/Console/KingdomCommands/KingdomDiplomacyCommands/CallAllyToWarCommand.cs) | Call allied kingdom to war | kingdom, enemy_kingdom |
| `gm.kingdom.declare_alliance` | [`DeclareAllianceCommand.cs`](Bannerlord.GameMaster/Console/KingdomCommands/KingdomDiplomacyCommands/DeclareAllianceCommand.cs) | Declare alliance between kingdoms | kingdom1, kingdom2 |
| `gm.kingdom.declare_war` | [`DeclareWarCommand.cs`](Bannerlord.GameMaster/Console/KingdomCommands/KingdomDiplomacyCommands/DeclareWarCommand.cs) | Declare war on kingdom | kingdom, enemy_kingdom |
| `gm.kingdom.get_tribute_info` | [`GetTributeInfoCommand.cs`](Bannerlord.GameMaster/Console/KingdomCommands/KingdomDiplomacyCommands/GetTributeInfoCommand.cs) | Get kingdom tribute information | kingdom (optional) |
| `gm.kingdom.make_peace` | [`MakePeaceCommand.cs`](Bannerlord.GameMaster/Console/KingdomCommands/KingdomDiplomacyCommands/MakePeaceCommand.cs) | End war between kingdoms | kingdom, enemy_kingdom |
| `gm.kingdom.pay_tribute` | [`PayTributeCommand.cs`](Bannerlord.GameMaster/Console/KingdomCommands/KingdomDiplomacyCommands/PayTributeCommand.cs) | Pay tribute to enemy | kingdom, tribute_amount |
| `gm.kingdom.trade_agreement` | [`TradeAgreementCommand.cs`](Bannerlord.GameMaster/Console/KingdomCommands/KingdomDiplomacyCommands/TradeAgreementCommand.cs) | Establish trade agreement | kingdom1, kingdom2 |

---

### 10. Logger Commands (gm.log)

Manage command logging.

| Command | File | Purpose | Arguments |
|---------|------|---------|-----------|
| `gm.log.clear` | [`ClearLogCommand.cs`](Bannerlord.GameMaster/Console/LoggerCommands/ClearLogCommand.cs) | Clear log file | None |
| `gm.log.disable` | [`DisableLoggingCommand.cs`](Bannerlord.GameMaster/Console/LoggerCommands/DisableLoggingCommand.cs) | Disable command logging | None |
| `gm.log.enable` | [`EnableLoggingCommand.cs`](Bannerlord.GameMaster/Console/LoggerCommands/EnableLoggingCommand.cs) | Enable command logging | None |
| `gm.log.help` | [`LoggingHelpCommand.cs`](Bannerlord.GameMaster/Console/LoggerCommands/LoggingHelpCommand.cs) | Show logging help | None |
| `gm.log.status` | [`LoggingStatusCommand.cs`](Bannerlord.GameMaster/Console/LoggerCommands/LoggingStatusCommand.cs) | Show logging status | None |

---

### 11. Query Commands (gm.query)

Search and display information about game entities.

#### Clan Queries

| Command | File | Purpose | Arguments |
|---------|------|---------|-----------|
| `gm.query.clan` | [`QueryClanCommand.cs`](Bannerlord.GameMaster/Console/Query/ClanQueryCommands/QueryClanCommand.cs) | Query clans (AND logic) | Clan filters and sorting options |
| `gm.query.clan_any` | [`QueryClanAnyCommand.cs`](Bannerlord.GameMaster/Console/Query/ClanQueryCommands/QueryClanAnyCommand.cs) | Query clans (OR logic) | Clan filters and sorting options |
| `gm.query.clan_info` | [`QueryClanInfoCommand.cs`](Bannerlord.GameMaster/Console/Query/ClanQueryCommands/QueryClanInfoCommand.cs) | Detailed clan info | clan_name |

#### Culture Queries

| Command | File | Purpose | Arguments |
|---------|------|---------|-----------|
| `gm.query.culture` | [`QueryCultureCommand.cs`](Bannerlord.GameMaster/Console/Query/CultureQueryCommands/QueryCultureCommand.cs) | Query cultures | Culture filters |
| `gm.query.culture_info` | [`QueryCultureInfoCommand.cs`](Bannerlord.GameMaster/Console/Query/CultureQueryCommands/QueryCultureInfoCommand.cs) | Detailed culture info | culture_name |

#### Hero Queries

| Command | File | Purpose | Arguments |
|---------|------|---------|-----------|
| `gm.query.hero` | [`QueryHeroCommand.cs`](Bannerlord.GameMaster/Console/Query/HeroQueryCommands/QueryHeroCommand.cs) | Query heroes (AND logic) | Hero filters and sorting options |
| `gm.query.hero_any` | [`QueryHeroAnyCommand.cs`](Bannerlord.GameMaster/Console/Query/HeroQueryCommands/QueryHeroAnyCommand.cs) | Query heroes (OR logic) | Hero filters and sorting options |
| `gm.query.hero_info` | [`QueryHeroInfoCommand.cs`](Bannerlord.GameMaster/Console/Query/HeroQueryCommands/QueryHeroInfoCommand.cs) | Detailed hero info | hero_name |

#### Item Queries

| Command | File | Purpose | Arguments |
|---------|------|---------|-----------|
| `gm.query.item` | [`QueryItemCommand.cs`](Bannerlord.GameMaster/Console/Query/ItemQueryCommands/QueryItemCommand.cs) | Query items (AND logic) | Item filters and sorting options |
| `gm.query.item_any` | [`QueryItemAnyCommand.cs`](Bannerlord.GameMaster/Console/Query/ItemQueryCommands/QueryItemAnyCommand.cs) | Query items (OR logic) | Item filters and sorting options |
| `gm.query.item_info` | [`QueryItemInfoCommand.cs`](Bannerlord.GameMaster/Console/Query/ItemQueryCommands/QueryItemInfoCommand.cs) | Detailed item info | item_name |

#### Modifier Queries

| Command | File | Purpose | Arguments |
|---------|------|---------|-----------|
| `gm.query.modifier_info` | [`QueryModifierInfoCommand.cs`](Bannerlord.GameMaster/Console/Query/ItemModifierQueryCommands/QueryModifierInfoCommand.cs) | Get modifier info | modifier_name |
| `gm.query.modifiers` | [`QueryModifiersCommand.cs`](Bannerlord.GameMaster/Console/Query/ItemModifierQueryCommands/QueryModifiersCommand.cs) | List all modifiers | None |

#### Kingdom Queries

| Command | File | Purpose | Arguments |
|---------|------|---------|-----------|
| `gm.query.kingdom` | [`QueryKingdomCommand.cs`](Bannerlord.GameMaster/Console/Query/KingdomQueryCommands/QueryKingdomCommand.cs) | Query kingdoms (AND logic) | Kingdom filters and sorting options |
| `gm.query.kingdom_any` | [`QueryKingdomAnyCommand.cs`](Bannerlord.GameMaster/Console/Query/KingdomQueryCommands/QueryKingdomAnyCommand.cs) | Query kingdoms (OR logic) | Kingdom filters and sorting options |
| `gm.query.kingdom_info` | [`QueryKingdomInfoCommand.cs`](Bannerlord.GameMaster/Console/Query/KingdomQueryCommands/QueryKingdomInfoCommand.cs) | Detailed kingdom info | kingdom_name |

#### Settlement Queries

| Command | File | Purpose | Arguments |
|---------|------|---------|-----------|
| `gm.query.settlement` | [`QuerySettlementCommand.cs`](Bannerlord.GameMaster/Console/Query/SettlementQueryCommands/QuerySettlementCommand.cs) | Query settlements (AND logic) | Settlement filters and sorting options |
| `gm.query.settlement_any` | [`QuerySettlementAnyCommand.cs`](Bannerlord.GameMaster/Console/Query/SettlementQueryCommands/QuerySettlementAnyCommand.cs) | Query settlements (OR logic) | Settlement filters and sorting options |
| `gm.query.settlement_info` | [`QuerySettlementInfoCommand.cs`](Bannerlord.GameMaster/Console/Query/SettlementQueryCommands/QuerySettlementInfoCommand.cs) | Detailed settlement info | settlement_name |

#### Troop Queries

| Command | File | Purpose | Arguments |
|---------|------|---------|-----------|
| `gm.query.character_objects` | [`QueryCharacterObjectsCommand.cs`](Bannerlord.GameMaster/Console/Query/TroopQueryCommands/QueryCharacterObjectsCommand.cs) | Query character objects (AND logic) | Character filters |
| `gm.query.character_objects_any` | [`QueryCharacterObjectsAnyCommand.cs`](Bannerlord.GameMaster/Console/Query/TroopQueryCommands/QueryCharacterObjectsAnyCommand.cs) | Query character objects (OR logic) | Character filters |
| `gm.query.character_objects_info` | [`QueryCharacterObjectsInfoCommand.cs`](Bannerlord.GameMaster/Console/Query/TroopQueryCommands/QueryCharacterObjectsInfoCommand.cs) | Detailed character info | character_name |
| `gm.query.troop` | [`QueryTroopCommand.cs`](Bannerlord.GameMaster/Console/Query/TroopQueryCommands/QueryTroopCommand.cs) | Query troops (AND logic) | Troop filters |
| `gm.query.troop_any` | [`QueryTroopAnyCommand.cs`](Bannerlord.GameMaster/Console/Query/TroopQueryCommands/QueryTroopAnyCommand.cs) | Query troops (OR logic) | Troop filters |
| `gm.query.troop_info` | [`QueryTroopInfoCommand.cs`](Bannerlord.GameMaster/Console/Query/TroopQueryCommands/QueryTroopInfoCommand.cs) | Detailed troop info | troop_name |

---

### 12. Settlement Commands (gm.settlement)

Manage settlements and villages.

#### Fortification Management

| Command | File | Purpose | Arguments |
|---------|------|---------|-----------|
| `gm.settlement.add_militia` | [`AddMilitiaCommand.cs`](Bannerlord.GameMaster/Console/SettlementCommands/FortificationManagementCommands/AddMilitiaCommand.cs) | Add militia to settlement | settlement, count |
| `gm.settlement.fill_garrison` | [`FillGarrisonCommand.cs`](Bannerlord.GameMaster/Console/SettlementCommands/FortificationManagementCommands/FillGarrisonCommand.cs) | Fill garrison with troops | settlement, culture (optional) |
| `gm.settlement.give_food` | [`GiveFoodCommand.cs`](Bannerlord.GameMaster/Console/SettlementCommands/FortificationManagementCommands/GiveFoodCommand.cs) | Add food to settlement | settlement, amount |
| `gm.settlement.give_gold` | [`GiveGoldCommand.cs`](Bannerlord.GameMaster/Console/SettlementCommands/FortificationManagementCommands/GiveGoldCommand.cs) | Add gold to settlement | settlement, amount |
| `gm.settlement.set_loyalty` | [`SetLoyaltyCommand.cs`](Bannerlord.GameMaster/Console/SettlementCommands/FortificationManagementCommands/SetLoyaltyCommand.cs) | Set settlement loyalty | settlement, loyalty_value |
| `gm.settlement.set_owner` | [`SetOwnerCommand.cs`](Bannerlord.GameMaster/Console/SettlementCommands/FortificationManagementCommands/SetOwnerCommand.cs) | Set settlement owner hero | settlement, hero |
| `gm.settlement.set_owner_clan` | [`SetOwnerClanCommand.cs`](Bannerlord.GameMaster/Console/SettlementCommands/FortificationManagementCommands/SetOwnerClanCommand.cs) | Set settlement owner clan | settlement, clan |
| `gm.settlement.set_owner_kingdom` | [`SetOwnerKingdomCommand.cs`](Bannerlord.GameMaster/Console/SettlementCommands/FortificationManagementCommands/SetOwnerKingdomCommand.cs) | Set settlement owner kingdom | settlement, kingdom |
| `gm.settlement.set_prosperity` | [`SetProsperityCommand.cs`](Bannerlord.GameMaster/Console/SettlementCommands/FortificationManagementCommands/SetProsperityCommand.cs) | Set settlement prosperity | settlement, prosperity_value |
| `gm.settlement.set_security` | [`SetSecurityCommand.cs`](Bannerlord.GameMaster/Console/SettlementCommands/FortificationManagementCommands/SetSecurityCommand.cs) | Set settlement security | settlement, security_value |
| `gm.settlement.spawn_wanderer` | [`SpawnWandererCommand.cs`](Bannerlord.GameMaster/Console/SettlementCommands/FortificationManagementCommands/SpawnWandererCommand.cs) | Spawn wanderer in settlement | settlement, culture (optional) |
| `gm.settlement.upgrade_buildings` | [`UpgradeBuildingsCommand.cs`](Bannerlord.GameMaster/Console/SettlementCommands/FortificationManagementCommands/UpgradeBuildingsCommand.cs) | Upgrade all buildings | settlement |

#### Settlement Management

| Command | File | Purpose | Arguments |
|---------|------|---------|-----------|
| `gm.settlement.rename_settlement` | [`RenameSettlementCommand.cs`](Bannerlord.GameMaster/Console/SettlementCommands/SettlementManagementCommands/RenameSettlementCommand.cs) | Rename settlement | settlement, new_name |
| `gm.settlement.reset_all_settlement_names` | [`ResetAllSettlementNamesCommand.cs`](Bannerlord.GameMaster/Console/SettlementCommands/SettlementManagementCommands/ResetAllSettlementNamesCommand.cs) | Reset all to default names | None |
| `gm.settlement.reset_settlement_name` | [`ResetSettlementNameCommand.cs`](Bannerlord.GameMaster/Console/SettlementCommands/SettlementManagementCommands/ResetSettlementNameCommand.cs) | Reset settlement name to default | settlement |
| `gm.settlement.set_settlement_culture` | [`SetSettlementCultureCommand.cs`](Bannerlord.GameMaster/Console/SettlementCommands/SettlementManagementCommands/SetSettlementCultureCommand.cs) | Change settlement culture | settlement, culture |

#### Village Management

| Command | File | Purpose | Arguments |
|---------|------|---------|-----------|
| `gm.settlement.set_hearths` | [`SetHearthsCommand.cs`](Bannerlord.GameMaster/Console/SettlementCommands/VillageManagementCommands/SetHearthsCommand.cs) | Set village hearths | village, hearths_value |
| `gm.settlement.set_village_bound_settlement` | [`SetVillageBoundSettlementCommand.cs`](Bannerlord.GameMaster/Console/SettlementCommands/VillageManagementCommands/SetVillageBoundSettlementCommand.cs) | Set village tax binding | village, settlement |
| `gm.settlement.set_village_trade_bound_settlement` | [`SetVillageTradeBoundSettlementCommand.cs`](Bannerlord.GameMaster/Console/SettlementCommands/VillageManagementCommands/SetVillageTradeBoundSettlementCommand.cs) | Set village trade binding | village, settlement |

---

### 13. Troop Commands (gm.troop)

Manage troop composition and upgrades.

| Command | File | Purpose | Arguments |
|---------|------|---------|-----------|
| `gm.troop.add_basic_troops` | [`AddBasicTroopsCommand.cs`](Bannerlord.GameMaster/Console/TroopCommands/TroopPartyManagementCommands/AddBasicTroopsCommand.cs) | Add basic troops to party | hero, count, culture (optional) |
| `gm.troop.add_elite_troops` | [`AddEliteTroopsCommand.cs`](Bannerlord.GameMaster/Console/TroopCommands/TroopPartyManagementCommands/AddEliteTroopsCommand.cs) | Add elite troops to party | hero, count, culture (optional) |
| `gm.troop.add_mercenary_troops` | [`AddMercenaryTroopsCommand.cs`](Bannerlord.GameMaster/Console/TroopCommands/TroopPartyManagementCommands/AddMercenaryTroopsCommand.cs) | Add mercenary troops to party | hero, count, culture (optional) |
| `gm.troop.add_mixed_troops` | [`AddMixedTroopsCommand.cs`](Bannerlord.GameMaster/Console/TroopCommands/TroopPartyManagementCommands/AddMixedTroopsCommand.cs) | Add mixed troop types to party | hero, count, culture (optional) |
| `gm.troop.give_hero_troops` | [`GiveHeroTroopsCommand.cs`](Bannerlord.GameMaster/Console/TroopCommands/TroopPartyManagementCommands/GiveHeroTroopsCommand.cs) | Give troops to hero | hero, count |
| `gm.troop.give_xp` | [`GiveXpCommand.cs`](Bannerlord.GameMaster/Console/TroopCommands/TroopPartyManagementCommands/GiveXpCommand.cs) | Give XP to hero and troops | hero, xp_amount |
| `gm.troop.upgrade_troops` | [`UpgradeTroopsCommand.cs`](Bannerlord.GameMaster/Console/TroopCommands/TroopPartyManagementCommands/UpgradeTroopsCommand.cs) | Upgrade all troops in party | hero |

---

### 14. Dev Commands (gm.dev)

Development and debugging commands.

| Command | File | Purpose | Arguments |
|---------|------|---------|-----------|
| `gm.dev.check_heroes_match_characters_stringid` | [`CheckHeroesMatchesCharactersStringIdCommand.cs`](Bannerlord.GameMaster/Console/DevCommands/CheckHeroesMatchesCharactersStringIdCommand.cs) | Verify hero string IDs match characters | None |
| `gm.dev.dump_banner_colors` | [`DumpBannerColorsCommand.cs`](Bannerlord.GameMaster/Console/DevCommands/DumpBannerColorsCommand.cs) | Dump available banner colors | None |
| `gm.dev.dump_hotkey_categories` | [`DumpHotkeyCategoriesCommand.cs`](Bannerlord.GameMaster/Console/DevCommands/DumpHotkeyCategoriesCommand.cs) | Dump hotkey categories | None |
| `gm.dev.get_player_captain_perks` | [`GetPlayerCaptainPerksCommand.cs`](Bannerlord.GameMaster/Console/DevCommands/GetPlayerCaptainPerksCommand.cs) | Get player captain perks | None |
| `gm.dev.reinitialize_blgm_object_manager` | [`ReinitializeBlgmObjectManagerCommand.cs`](Bannerlord.GameMaster/Console/DevCommands/ReinitializeBlgmObjectManagerCommand.cs) | Reinitialize BLGM object manager | None |

---

## Common Patterns & Usage

### Standard Command Structure

Commands follow a consistent pattern with MARK sections for organization:

```csharp
[CommandLineFunctionality.CommandLineArgumentFunction("command_name", "gm.category")]
public static string CommandName(List<string> args)
{
    return Cmd.Run(args, () =>
    {
        // MARK: Validation
        if (!CommandValidator.ValidateCampaignState(out string error))
            return error;

        ParsedArguments parsed = ArgumentParser.ParseArguments(args);
        parsed.SetValidArguments(
            new ArgumentDefinition("name", isRequired: true),
            new ArgumentDefinition("count", isRequired: false, defaultDisplay: "1")
        );
        
        if (parsed.GetValidationError() != null)
            return MessageFormatter.FormatErrorMessage(parsed.GetValidationError());

        // MARK: Parse Arguments
        string name = parsed.GetArgument("name", 0);
        int count = parsed.GetInt("count", 1, defaultValue: 1);

        // MARK: Execute Logic
        // ... perform operation ...

        Dictionary<string, string> resolvedValues = new()
        {
            { "name", name },
            { "count", count.ToString() }
        };
        
        string argumentDisplay = parsed.FormatArgumentDisplay("gm.group.command_name", resolvedValues);
        return argumentDisplay + MessageFormatter.FormatSuccessMessage("Operation completed");
    });
}
```

### Argument Patterns

**Named Arguments Only:**
```
gm.clan.create_clan name:MyClone leader:Aragorn culture:vlandia
```

**Positional Arguments Only:**
```
gm.clan.create_clan MyClone Aragorn vlandia
```

**Mixed Arguments:**
```
gm.clan.create_clan name:MyClone Aragorn vlandia
```

**Multi-Word Arguments (Single Quotes):**
```
gm.hero.rename aragorn name:'Sir Galahad the Brave'
gm.clan.create_clan name:'My Wonderful Clan' leader:aragorn
```

### Common Argument Types

**Culture Arguments**
Support multiple formats:
- Single culture: `culture:vlandia`
- Multiple cultures: `culture:vlandia,battania,empire` (no spaces)
- Named groups: `culture:all_cultures`, `culture:main_cultures`, `culture:bandit_cultures`

**Boolean Arguments**
Support multiple formats:
- true/false: `enable:true`
- yes/no: `enable:yes`
- 1/0: `enable:1`
- on/off: `enable:on`

**Entity References**
Use smart matching:
- Exact name: `hero:Aragorn`
- Name prefix: `hero:Ara`
- Exact ID: `hero:hero_1`
- ID prefix: `hero:hero_`

### Query Command Patterns

Query commands have three variants for each entity type:

**Basic Query (AND Logic):**
```
gm.query.hero name:Ara clan:Swadia
```
Returns heroes where name contains "Ara" AND clan is Swadia

**Any Query (OR Logic):**
```
gm.query.hero_any name:Ara clan:Swadia
```
Returns heroes where name contains "Ara" OR clan is Swadia

**Info Query (Detailed):**
```
gm.query.hero_info Aragorn
```
Returns detailed information about specific hero

---

## Quick Reference Examples

### Creating a Custom Lord with Party

```
gm.hero.create_lord name:'Sir Galahad' culture:vlandia party_name:'Galahad Party'
gm.hero.create_party 'Sir Galahad' party_name:'Galahad Party'
gm.troop.add_elite_troops 'Sir Galahad' count:50 culture:vlandia
gm.hero.set_clan 'Sir Galahad' clan:MyClone
gm.kingdom.add_clan MyKingdom MyClone
```

### Managing Kingdom Diplomacy

```
gm.kingdom.create_kingdom name:'New Kingdom' ruler:Aragorn primary_culture:vlandia
gm.kingdom.add_clan 'New Kingdom' clan:Swadia
gm.kingdom.declare_war 'New Kingdom' enemy_kingdom:Battania
gm.query.kingdom_info 'New Kingdom'
gm.kingdom.make_peace 'New Kingdom' enemy_kingdom:Battania
gm.kingdom.declare_alliance 'New Kingdom' kingdom2:Sturgia
```

### Batch Creating Clans

```
gm.ignore_limits true
gm.clan.generate_clans count:10 culture:all_cultures
gm.query.clan count:10 sort:name
```

### Using Query Commands to Find Entities

```
gm.query.hero name:lord clan:Swadia
gm.query.hero_any name:companion is_alive:true
gm.query.settlement owner_type:clan owner:Swadia
gm.query.item type:armor quality:legendary
gm.query.troop culture:vlandia tier:3
```

### Equipment Save/Load Workflow

```
gm.item.save_equipment Aragorn save_name:Aragorn_Battle
gm.item.save_equipment_civilian Aragorn save_name:Aragorn_Civilian
gm.item.list_equipped Aragorn
gm.item.load_equipment Aragorn save_name:Aragorn_Battle
gm.item.unequip_slot Aragorn slot:head
gm.item.equip Aragorn item:leather_helmet
gm.item.set_equipped_modifier Aragorn slot:armor modifier:reinforced
```

### Hero Management

```
gm.hero.create_lord name:Aragorn culture:vlandia
gm.hero.add_gold Aragorn gold_amount:10000
gm.hero.set_culture Aragorn culture:sturgia
gm.hero.set_relation Aragorn target_hero:Legolas relation_value:50
gm.hero.create_party Aragorn party_name:'Aragorn Party'
gm.troop.add_mixed_troops Aragorn count:100
```

### Settlement Management

```
gm.settlement.set_owner 'Praven' hero:Aragorn
gm.settlement.set_prosperity 'Praven' prosperity_value:1000
gm.settlement.set_security 'Praven' security_value:100
gm.settlement.set_loyalty 'Praven' loyalty_value:50
gm.settlement.fill_garrison 'Praven' culture