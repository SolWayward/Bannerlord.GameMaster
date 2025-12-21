# Command Additions and Updates - December 20, 2025

## Summary
Added 10 new console commands and updated 1 existing command across clan, troop, and hero management systems to enhance gameplay control and customization capabilities.

## Changes

### Clan Commands

#### 1. Updated: `gm.clan.create_clan`
**File:** `Bannerlord.GameMaster/Console/ClanCommands/ClanGenerationCommands.cs`

**Changes:**
- Made leader parameter optional (previously required)
- Leader is now auto-generated if not specified
- Updated validation to accept 1-3 arguments instead of requiring minimum 2
- Enhanced success message with more details

**Usage:**
```
gm.clan.create_clan <clanName> [leaderHero] [kingdom]
```

**Examples:**
```
gm.clan.create_clan Highlanders
gm.clan.create_clan 'The Highland Clan' derthert
gm.clan.create_clan NewClan myHero empire
```

#### 2. New: `gm.clan.generate_clans`
**File:** `Bannerlord.GameMaster/Console/ClanCommands/ClanGenerationCommands.cs`

**Purpose:** Generate multiple clans at once with random names from culture lists

**Parameters:**
- `count` (required): Number of clans to generate (1-50)
- `cultures` (optional): Culture flags, defaults to main_cultures
- `kingdom` (optional): Kingdom for all clans to join

**Usage:**
```
gm.clan.generate_clans <count> [cultures] [kingdom]
```

**Examples:**
```
gm.clan.generate_clans 5
gm.clan.generate_clans 10 vlandia;battania
gm.clan.generate_clans 3 main_cultures empire
```

**Implementation:**
- Uses `FlagParser.ParseCultureArgument` for culture parsing
- Leverages `ClanGenerator.GenerateClans` method
- Formats output using `ClanQueries.GetFormattedDetails`

#### 3. New: `gm.clan.rename`
**File:** `Bannerlord.GameMaster/Console/ClanCommands/ClanManagementCommands.cs`

**Purpose:** Rename an existing clan

**Parameters:**
- `clan` (required): Clan query to find the clan
- `newName` (required): New name for the clan

**Usage:**
```
gm.clan.rename <clan> <newName>
```

**Examples:**
```
gm.clan.rename empire_south 'Southern Empire Lords'
gm.clan.rename clan_1 NewClanName
```

**Implementation:**
- Uses `ClanExtensions.SetStringName` extension method
- Displays previous and new name in success message

#### 4. New: `gm.clan.set_culture`
**File:** `Bannerlord.GameMaster/Console/ClanCommands/ClanManagementCommands.cs`

**Purpose:** Change a clan's culture

**Parameters:**
- `clan` (required): Clan query to find the clan
- `culture` (required): Culture string ID

**Usage:**
```
gm.clan.set_culture <clan> <culture>
```

**Examples:**
```
gm.clan.set_culture empire_south vlandia
gm.clan.set_culture my_clan battania
```

**Implementation:**
- Parses culture using `MBObjectManager.Instance.GetObject<CultureObject>`
- Updates both `clan.Culture` and `clan.BasicTroop` properties
- Displays previous and new culture with basic troop info

### Troop Commands

#### 5. New: `gm.troops.add_basic`
**File:** `Bannerlord.GameMaster/Console/TroopCommands/TroopManagementCommands.cs`

**Purpose:** Add basic tier troops from party leader's culture to their party

**Parameters:**
- `partyLeader` (required): Hero who leads the party
- `count` (required): Number of basic troops to add (1-10000)

**Usage:**
```
gm.troops.add_basic <partyLeader> <count>
```

**Examples:**
```
gm.troops.add_basic derthert 50
gm.troops.add_basic player 100
```

**Implementation:**
- Uses `MobilePartyExtensions.AddBasicTroops`
- Validates hero is party leader
- Displays troop name and updated party size

#### 6. New: `gm.troops.add_elite`
**File:** `Bannerlord.GameMaster/Console/TroopCommands/TroopManagementCommands.cs`

**Purpose:** Add elite tier troops from party leader's culture to their party

**Parameters:**
- `partyLeader` (required): Hero who leads the party
- `count` (required): Number of elite troops to add (1-10000)

**Usage:**
```
gm.troops.add_elite <partyLeader> <count>
```

**Examples:**
```
gm.troops.add_elite derthert 30
gm.troops.add_elite player 50
```

**Implementation:**
- Uses `MobilePartyExtensions.AddEliteTroops`
- Validates hero is party leader
- Displays elite troop name and updated party size

#### 7. New: `gm.troops.add_mercenary`
**File:** `Bannerlord.GameMaster/Console/TroopCommands/TroopManagementCommands.cs`

**Purpose:** Add random mercenary troops from party leader's culture to their party

**Parameters:**
- `partyLeader` (required): Hero who leads the party
- `count` (required): Number of mercenary troops to add (1-10000)

**Usage:**
```
gm.troops.add_mercenary <partyLeader> <count>
```

**Examples:**
```
gm.troops.add_mercenary derthert 20
gm.troops.add_mercenary player 40
```

**Implementation:**
- Uses `MobilePartyExtensions.AddMercenaryTroops`
- Mercenaries are randomly selected from culture's mercenary roster
- Validates hero is party leader

#### 8. New: `gm.troops.add_mixed`
**File:** `Bannerlord.GameMaster/Console/TroopCommands/TroopManagementCommands.cs`

**Purpose:** Add mixed tier troops (basic, elite, and mercenary) to party

**Parameters:**
- `partyLeader` (required): Hero who leads the party
- `countOfEach` (required): Number of each type to add (1-3000)

**Usage:**
```
gm.troops.add_mixed <partyLeader> <countOfEach>
```

**Examples:**
```
gm.troops.add_mixed derthert 15
gm.troops.add_mixed player 20
```

**Implementation:**
- Uses `MobilePartyExtensions.AddMixedTierTroops`
- Adds equal counts of basic, elite, and mercenary troops
- Total troops added = countOfEach * 3
- Validates hero is party leader

### Hero Commands

#### 9. New: `gm.hero.add_hero_to_party`
**File:** `Bannerlord.GameMaster/Console/HeroCommands/HeroManagementCommands.cs`

**Purpose:** Add a hero as companion to another hero's party

**Parameters:**
- `hero` (required): Hero to add to party
- `partyLeader` (required): Hero who leads the target party

**Usage:**
```
gm.hero.add_hero_to_party <hero> <partyLeader>
```

**Examples:**
```
gm.hero.add_hero_to_party companion_1 player
gm.hero.add_hero_to_party wanderer_1 derthert
```

**Implementation:**
- Hero leaves current party if already in one
- Updates hero's clan to match party leader's clan
- Uses `MobilePartyExtensions.AddCompanionToParty`
- Prevents party leaders from joining other parties (would require disbanding)
- Displays previous and new party information

**Limitations:**
- Heroes leading their own party cannot join another without disbanding first
- Disbanding functionality not yet implemented

#### 10. New: `gm.hero.create_party`
**File:** `Bannerlord.GameMaster/Console/HeroCommands/HeroManagementCommands.cs`

**Purpose:** Create a party for any hero at their location or home settlement

**Parameters:**
- `hero` (required): Hero to create party for

**Usage:**
```
gm.hero.create_party <hero>
```

**Examples:**
```
gm.hero.create_party lord_1_1
gm.hero.create_party wanderer_1
```

**Implementation:**
- Uses `HeroExtensions.CreateParty` extension method
- Spawn location priority:
  1. Hero's last seen place (if it's a settlement)
  2. Hero's home settlement
  3. Alternative settlement via `GetHomeOrAlternativeSettlement`
- Party initialized with:
  - 10 basic troops from hero's culture
  - 20000 trade gold
  - AI enabled
- Prevents creating party if hero already leads one

#### 11. New: `gm.hero.set_culture`
**File:** `Bannerlord.GameMaster/Console/HeroCommands/HeroManagementCommands.cs`

**Purpose:** Change a hero's culture

**Parameters:**
- `hero` (required): Hero to change culture
- `culture` (required): Culture string ID

**Usage:**
```
gm.hero.set_culture <hero> <culture>
```

**Examples:**
```
gm.hero.set_culture lord_1_1 vlandia
gm.hero.set_culture companion_1 battania
```

**Implementation:**
- Parses culture using `MBObjectManager.Instance.GetObject<CultureObject>`
- Updates `hero.Culture` property
- Displays previous and new culture

**Note:** Does not change hero's equipment or appearance, only the culture property

## Technical Details

### Code Quality
All commands follow existing patterns:
- Use `Cmd.Run(args, () => { })` wrapper
- Validate campaign mode with `CommandBase.ValidateCampaignMode`
- Create usage messages with `CommandValidator.CreateUsageMessage`
- Validate argument counts with `CommandBase.ValidateArgumentCount`
- Execute with error handling using `CommandBase.ExecuteWithErrorHandling`
- Format output with `CommandBase.FormatSuccessMessage` and `CommandBase.FormatErrorMessage`
- Include MARK comments for navigation

### Validation
All commands include comprehensive validation:
- Campaign mode checks
- Argument count validation
- Entity lookup validation (heroes, clans, cultures, kingdoms)
- Range validation for numeric inputs
- State validation (e.g., hero has party, hero is party leader)

### Error Handling
- All commands wrapped in error handling
- Clear error messages for common scenarios
- Graceful fallbacks where appropriate

### Dependencies
New using statements added:
- `Bannerlord.GameMaster.Characters` to ClanGenerationCommands.cs for CultureFlags
- `Bannerlord.GameMaster.Party` to TroopManagementCommands.cs for MobilePartyExtensions
- `Bannerlord.GameMaster.Party` to HeroManagementCommands.cs for MobilePartyExtensions

## Extension Methods Used

### Existing Methods Leveraged
- `ClanExtensions.SetStringName` - Clan renaming
- `ClanGenerator.CreateClan` - Clan creation
- `ClanGenerator.GenerateClans` - Batch clan generation
- `MobilePartyExtensions.AddBasicTroops` - Add basic troops
- `MobilePartyExtensions.AddEliteTroops` - Add elite troops
- `MobilePartyExtensions.AddMercenaryTroops` - Add mercenary troops
- `MobilePartyExtensions.AddMixedTierTroops` - Add mixed troops
- `MobilePartyExtensions.AddCompanionToParty` - Add hero to party
- `HeroExtensions.CreateParty` - Create party for hero
- `HeroExtensions.GetHomeOrAlternativeSettlement` - Settlement fallback

## Testing Recommendations

### Manual Testing Required
1. Clan creation with/without leader
2. Batch clan generation with various culture flags
3. Clan renaming with multi-word names
4. Culture changes for clans and heroes
5. Troop addition to various party leaders
6. Hero joining parties (with/without current party)
7. Party creation at various locations
8. Edge cases: invalid inputs, null references, etc.

### Integration Testing
- Verify clan creation integrates with kingdom systems
- Test troop additions reflect in party rosters
- Confirm hero party changes update all related systems
- Validate culture changes propagate correctly

## Breaking Changes
None. All changes are additive.

## Migration Notes
- The updated `gm.clan.create_clan` command is backward compatible
- Old usage: `gm.clan.create_clan <name> <hero> [kingdom]` still works
- New usage: `gm.clan.create_clan <name> [hero] [kingdom]` allows omitting hero

## Future Enhancements
1. Add party disbanding functionality for `add_hero_to_party` to support party leaders
2. Consider adding batch hero operations
3. Add equipment update option to hero culture change
4. Consider adding troop removal commands
5. Add party management commands (disband, merge, etc.)
