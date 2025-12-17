# Hero and Clan Generation Commands

**Date:** 2025-12-17  
**Type:** Feature Addition  
**Category:** Hero Management, Clan Management  
**Status:** Completed

## Overview

Added new commands for generating heroes and clans with customizable parameters, enabling dynamic creation of lords and noble families in the game world.

## New Commands

### Hero Generation Commands

#### `gm.hero.generate_lords [count=1] [clan=random]`

Creates lords from random templates with good gear and decent stats.

**Parameters:**
- `count` (optional, default: 1): Number of lords to generate (1-20)
- `clan` (optional, default: random): Target clan for lords. If not specified, each lord goes to a different random clan

**Features:**
- Age range: 30-40 years
- **Random gender selection** (50/50 male/female)
- Good equipment (Tier 4+) based on culture
- Decent stats (Level 15-25)
- Random valid templates
- Unique ID generation

**Examples:**
```
gm.hero.generate_lords 3
gm.hero.generate_lords 5 empire_south
```

**Output:**
```
Success: Successfully created 3 lord(s):
  - [Lord Name] (ID: gm_lord_..., Age: 35, Clan: Empire South)
  - [Lord Name] (ID: gm_lord_..., Age: 38, Clan: Battania)
  - [Lord Name] (ID: gm_lord_..., Age: 32, Clan: Vlandia)
```

#### `gm.hero.create_lord <gender> <name> <clan>` ✓ WORKING


Creates a fresh lord with minimal stats and equipment.

**Parameters:**
- `gender` (required): 'male' or 'female' (or 'm'/'f')
- `name` (required): Name for the lord
- `clan` (required): Target clan

**Features:**
- Age range: 20-24 years
- **Random culture selection** for variety
- **Randomized body/face appearance** for uniqueness
- Gender-based random template
- Minimal equipment (only basic clothes)
- No stats (fresh hero, level 1)
- Custom naming

**Examples:**
```
gm.hero.create_lord male NewLord empire_south
gm.hero.create_lord female LadyWarrior vlandia
```

**Output:**
```
Success: Created fresh lord 'NewLord' (ID: gm_lord_...):
Age: 22 | Gender: Male | Clan: Empire South
Level: 1 | Equipment: Minimal
```

### Clan Generation Commands

**⚠️ IMPORTANT: Clan creation commands are currently DISABLED due to game engine limitations that cause crashes.**

#### `gm.clan.generate_clans [count=1] [kingdom=none]` ⚠️ DISABLED


**STATUS: DISABLED - This command returns an error message due to engine limitations.**

Creates new clans filled with random heroes.

**Why Disabled:**
- Game engine requires complex internal clan initialization
- Incomplete initialization causes crashes when viewing heroes in encyclopedia
- Heroes show as "[HeroName] of the [blank]" with broken clan references
- Attempting to view hero details causes game crashes

**Workaround:**
Use `gm.hero.generate_lords` to add heroes to existing clans instead.


**Parameters:**
- `count` (optional, default: 1): Number of clans to generate (1-10)
- `kingdom` (optional, default: none): Target kingdom for clans

**Features:**
- Each clan gets 3-7 random heroes
- Random tier between 1-4
- Heroes have random gender and equipment
- Each hero is randomly equipped and leveled
- Random valid templates for all heroes
- Automatic leader assignment

**Examples:**
```
gm.clan.generate_clans 2
gm.clan.generate_clans 1 empire
```

**Output:**
```
Success: Successfully created 2 clan(s):
  - Generated Clan 1085_4523 (ID: gm_clan_..., Heroes: 5, Tier: 3, Kingdom: None)
  - Generated Clan 1085_7891 (ID: gm_clan_..., Heroes: 4, Tier: 2, Kingdom: Empire)
```

#### `gm.clan.create_clan <name> <hero> [kingdom=none]` ⚠️ DISABLED

**STATUS: DISABLED - This command returns an error message due to engine limitations.**


Creates a new clan with a specified hero as leader.

**Why Disabled:**
- Same engine limitations as `generate_clans`
- Creates incomplete clan structure
- Causes encyclopedia crashes
- Hero names become malformed

**Workaround:**
Use `gm.clan.add_hero` or `gm.hero.set_clan` to move heroes between existing clans instead.


**Parameters:**
- `name` (required): Name for the new clan
- `hero` (required): Hero to become clan leader
- `kingdom` (optional, default: none): Target kingdom

**Features:**
- Clan starts at tier 3
- Specified hero becomes leader
- Hero is moved to new clan
- Random banner generation
- Culture based on hero's culture

**Examples:**
```
gm.clan.create_clan NewClan lord_1_1
gm.clan.create_clan "House Stark" lord_2_5 empire
```

**Output:**
```
Success: Created clan 'NewClan' (ID: gm_clan_newclan_...):
Leader: [Hero Name] | Tier: 3 | Kingdom: None
```

## Technical Implementation

### Hero Generation

**Location:** `Bannerlord.GameMaster/Console/HeroCommands/HeroManagementCommands.cs`

**Key Features:**
- Uses `HeroCreator.CreateSpecialHero()` for proper hero creation
- Implements culture-based equipment system
- Random template selection from valid lord templates
- Unique ID generation: `gm_lord_{clan}_{year}_{random}`
- Equipment tier based on lord quality:
  - `generate_lords`: Tier 4+ armor and weapons
  - `create_lord`: Tier 1 civilian clothes only

**Equipment System:**
```csharp
// Equipment slots filled:
- Body Armor
- Head Armor
- Leg Armor
- Hand Armor (Gloves)
- Cape
- Primary Weapon
- Shield (if one-handed weapon)
```

### Clan Generation

**Location:** `Bannerlord.GameMaster/Console/ClanCommands/ClanManagementCommands.cs`

**Key Features:**
- Uses `MBObjectManager.Instance.CreateObject<Clan>()` for clan creation
- Implements `Banner.CreateRandomClanBanner()` for unique banners
- Random hero generation (3-7 per clan)
- Tier-based equipment quality:
  - Tier 1: Tier 2+ equipment
  - Tier 2: Tier 3+ equipment
  - Tier 3: Tier 4+ equipment
  - Tier 4: Tier 5+ equipment
- Unique ID generation: `gm_clan_{year}_{random}`

**Helper Method:**
```csharp
private static void EquipHeroWithRandomGear(Hero hero, BasicCultureObject culture, int tier, Random random)
```

## Validation

### Hero Commands
- Count validation (1-20 for `generate_lords`)
- Gender validation ('male', 'female', 'm', 'f')
- Clan existence validation
- Name emptiness check
- Template availability validation

### Clan Commands
- Count validation (1-10 for `generate_clans`)
- Kingdom existence validation
- Hero existence validation
- Leader status validation (prevents existing clan leaders)
- Name emptiness check
- Culture availability validation

## Error Handling

All commands implement:
- `CommandBase.ValidateCampaignMode()` - Campaign mode check
- `CommandBase.ValidateArgumentCount()` - Argument validation
- `CommandBase.FindSingleHero/Clan/Kingdom()` - Entity resolution
- `CommandValidator.ValidateIntegerRange()` - Numeric validation
- `CommandBase.ExecuteWithErrorHandling()` - Exception handling

## Logging

All commands use `Cmd.Run()` wrapper for automatic logging:
- Command name
- Arguments
- Success/failure status
- Error messages

## Testing

### Test Coverage

**Hero Management Tests:**
- `hero_mgmt_007`: Invalid count (negative)
- `hero_mgmt_008`: Invalid count (>20)
- `hero_mgmt_009`: Invalid clan
- `hero_mgmt_010`: Missing arguments
- `hero_mgmt_011`: Partial arguments
- `hero_mgmt_012`: Invalid gender
- `hero_mgmt_013`: Invalid clan

**Clan Management Tests:**
- `clan_mgmt_006`: Invalid count (negative)
- `clan_mgmt_007`: Invalid count (>10)
- `clan_mgmt_008`: Invalid kingdom
- `clan_mgmt_009`: Missing arguments
- `clan_mgmt_010`: Partial arguments
- `clan_mgmt_011`: Invalid hero
- `clan_mgmt_012`: Invalid kingdom

**Test Location:** `Bannerlord.GameMaster/Console/Testing/StandardTests.cs`

## Integration Points

### Dependencies
- `TaleWorlds.CampaignSystem.HeroCreator`
- `TaleWorlds.CampaignSystem.Clan`
- `TaleWorlds.CampaignSystem.Kingdom`
- `TaleWorlds.Core.ItemObject`
- `TaleWorlds.Core.CharacterObject`
- `TaleWorlds.Core.Banner`

### Related Systems
- Hero Extensions (culture, occupation)
- Clan Extensions (tier, renown)
- Equipment System (slots, modifiers)
- Query System (clan/hero lookup)

## Usage Examples

### Generate Multiple Lords for Different Clans
```
gm.hero.generate_lords 10
```

### Create Specific Lord for Clan
```
gm.hero.create_lord female "Lady Isabella" empire_south
```

### Generate Multiple Clans for Kingdom
```
gm.clan.generate_clans 3 empire
```

### Create Custom Clan with Existing Lord
```
gm.clan.create_clan "House of Dragons" lord_5_7 vlandia
```

### Combined Workflow: Create Clan with Fresh Lord
```
gm.hero.create_lord male "Lord Stark" battania
gm.clan.create_clan "House Stark" [generated_lord_id]
```

## Known Limitations

1. **Template Dependency**: Requires valid lord templates in game data
2. **Equipment Availability**: Equipment quality depends on available items in culture
3. **Clan Tier**: Cannot decrease clan tier after creation (engine limitation)
4. **Age Restriction**: Fixed age ranges (30-40 for `generate_lords`, 20-24 for `create_lord`)
5. **Random Assignment**: When no clan specified in `generate_lords`, uses available non-eliminated clans

## Future Enhancements

Potential improvements:
1. Custom age range parameters
2. Specific equipment template selection
3. Skill distribution customization
4. Relationship initialization
5. Starting gold/resources
6. Party creation option
7. Batch operations with JSON config

## Best Practices

### When to Use `generate_lords`
- Quick population of game world
- Testing combat mechanics
- Filling depleted clan rosters
- Creating armies

### When to Use `create_lord`
- Custom story characters
- Player-controlled vassals
- Starting from scratch scenarios
- RP-focused gameplay

### When to Use `generate_clans`
- World population
- Dynamic faction creation
- Testing clan systems
- Mod compatibility testing

### When to Use `create_clan`
- Custom noble houses
- Player clan offshoots
- Specific story elements
- Controlled clan creation

## Related Documentation

- [Hero Query System](../../docs/implementation/hero-query-implementation.md)
- [Clan Query System](../../docs/implementation/clan-query-implementation.md)
- [Command Best Practices](../../docs/guides/best-practices.md)
- [Testing Guide](../../docs/guides/testing.md)

## Version History

- **2025-12-17**: Initial implementation
  - Added `gm.hero.generate_lords`
  - Added `gm.hero.create_lord`
  - Added `gm.clan.generate_clans`
  - Added `gm.clan.create_clan`
  - Added comprehensive test coverage
  - Added documentation