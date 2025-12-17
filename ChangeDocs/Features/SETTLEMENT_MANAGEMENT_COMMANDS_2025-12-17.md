# Settlement Management Commands - UPDATED

**Date:** 2025-12-17
**Type:** Feature Addition
**Category:** Settlement Management
**Status:** Corrected and Finalized

## Overview

Added comprehensive settlement management commands to allow modification of settlement properties, resources, military capabilities, workshops, caravans, and NPCs. These commands provide granular control over settlements including cities, castles, and villages.

## Implemented Commands (13 total)

### Settlement Properties (5 commands)

#### `gm.settlement.set_prosperity`
Sets the prosperity level of a city or castle.

**Usage:** `gm.settlement.set_prosperity <settlement> <value>`
**Parameters:**
- `settlement`: Settlement name or ID
- `value`: Prosperity value (0-20000)

**Example:** `gm.settlement.set_prosperity pen 5000`

#### `gm.settlement.set_hearths`
Sets the hearth value of a village (equivalent to village prosperity).

**Usage:** `gm.settlement.set_hearths <settlement> <value>`
**Parameters:**
- `settlement`: Village name or ID
- `value`: Hearth value (0-2000)

**Example:** `gm.settlement.set_hearths village_1 500`

#### `gm.settlement.rename`
Changes the name of any settlement type (city, castle, village, hideout).

**Usage:** `gm.settlement.rename <settlement> <new_name>`
**Parameters:**
- `settlement`: Settlement name or ID
- `new_name`: The new name for the settlement

**Example:** `gm.settlement.rename pen NewPenraic`

**Implementation Note:** Uses reflection to set the private `_name` field due to read-only property restrictions.

#### `gm.settlement.set_loyalty`
Sets the loyalty level of a city or castle.

**Usage:** `gm.settlement.set_loyalty <settlement> <value>`
**Parameters:**
- `settlement`: Settlement name or ID
- `value`: Loyalty value (0-100)

**Example:** `gm.settlement.set_loyalty pen 100`

#### `gm.settlement.set_security`
Sets the security level of a city or castle.

**Usage:** `gm.settlement.set_security <settlement> <value>`
**Parameters:**
- `settlement`: Settlement name or ID
- `value`: Security value (0-100)

**Example:** `gm.settlement.set_security pen 100`

### Settlement Resources (2 commands)

#### `gm.settlement.give_food`
Adds or removes food from a settlement's food stock.

**Usage:** `gm.settlement.give_food <settlement> <amount>`
**Parameters:**
- `settlement`: Settlement name or ID
- `amount`: Food amount to add/subtract (-100000 to 100000)

**Example:** `gm.settlement.give_food pen 1000`

#### `gm.settlement.give_gold`
Adds or removes gold from a settlement's treasury.

**Usage:** `gm.settlement.give_gold <settlement> <amount>`
**Parameters:**
- `settlement`: Settlement name or ID
- `amount`: Gold amount to add/subtract

**Example:** `gm.settlement.give_gold pen 10000`

### Settlement Military (2 commands)

#### `gm.settlement.add_militia`
Adds militia troops to a city or castle.

**Usage:** `gm.settlement.add_militia <settlement> <amount>`
**Parameters:**
- `settlement`: Settlement name or ID
- `amount`: Militia count to add (0-1000)

**Example:** `gm.settlement.add_militia pen 100`

**Implementation:** Uses `Settlement.Militia` property (get/set) instead of `Settlement.Town.Militia` (get only).

#### `gm.settlement.fill_garrison`
Fills the garrison to maximum capacity using a proportional mix of existing troop types.

**Usage:** `gm.settlement.fill_garrison <settlement>`
**Parameters:**
- `settlement`: Settlement name or ID

**Example:** `gm.settlement.fill_garrison pen`

**Details:**
- Analyzes existing troop composition in the garrison
- Adds troops proportionally based on existing ratios
- Fills to maximum party size limit
- Requires at least one troop type in garrison to use as template

### Settlement Projects (1 command)

#### `gm.settlement.upgrade_buildings`
Upgrades all buildings in a city or castle to the specified level.

**Usage:** `gm.settlement.upgrade_buildings <settlement> <level>`
**Parameters:**
- `settlement`: Settlement name or ID
- `level`: Target building level (0-3) - **CRITICAL: Level 4+ will crash the game**

**Example:** `gm.settlement.upgrade_buildings pen 3`

**Safety:** Maximum level is capped at 3 to prevent game crashes.

**Details:**
- Sets `building.CurrentLevel` directly for each building
- Reports number of buildings upgraded and skipped
- Buildings already at or above target level are skipped

### Settlement Workshops (2 commands)

#### `gm.settlement.own_workshops`
Takes ownership of all workshops in a city for the player.

**Usage:** `gm.settlement.own_workshops <settlement>`
**Parameters:**
- `settlement`: City name or ID

**Example:** `gm.settlement.own_workshops pen`

**Implementation:** Uses `workshop.ChangeOwnerOfWorkshop(Hero.MainHero, workshopType, 20000)` to transfer ownership.

#### `gm.settlement.add_workshop`
Adds new workshops to a settlement, assigned to notables.

**Usage:** `gm.settlement.add_workshop <settlement> <count>`
**Parameters:**
- `settlement`: City name or ID
- `count`: Number of workshops to add (1-10)

**Example:** `gm.settlement.add_workshop pen 2`

**Details:**
- Creates new Workshop instances
- Assigns to notables who have the fewest existing workshops
- Requires at least one notable in the settlement

### Settlement NPCs and Parties (2 commands)

#### `gm.settlement.create_caravan`
Creates a new caravan in a settlement.

**Usage:** `gm.settlement.create_caravan <settlement>`
**Parameters:**
- `settlement`: City name or ID

**Example:** `gm.settlement.create_caravan pen`

**Details:**
- Uses `CaravanPartyComponent.CreateCaravanParty(owner, settlement, template)`
- Assigns to a notable without caravans, or to player if none available
- Uses caravan party template from game data

#### `gm.settlement.spawn_wanderer`
Spawns a random wanderer hero in a settlement.

**Usage:** `gm.settlement.spawn_wanderer <settlement>`
**Parameters:**
- `settlement`: City or castle name or ID

**Example:** `gm.settlement.spawn_wanderer pen`

**Details:**
- Creates character from wanderer template using `CharacterObject.CreateFrom()`
- Creates hero using `HeroCreator.CreateBasicHero()`
- Places wanderer in settlement using `wanderer.StayingInSettlement`

## Implementation Details

### Architecture
- All commands follow established patterns from existing management command classes
- Consistent use of `Cmd.Run()` wrapper for automatic logging
- Proper validation chain: campaign mode → argument count → entity resolution → value validation → execution
- Uses `CommandBase.ExecuteWithErrorHandling()` for safe state modifications

### Key API Corrections
Based on user feedback, several corrections were made:

1. **Militia**: Changed from using reflection on `Settlement.Town.Militia` (read-only) to using `Settlement.Militia` (get/set)
2. **Construction**: Replaced with `upgrade_buildings` command that sets `building.CurrentLevel` directly
3. **Workshops**: Uses `workshop.ChangeOwnerOfWorkshop()` method and `new Workshop()` constructor
4. **Caravans**: Uses correct `CaravanPartyComponent.CreateCaravanParty()` signature
5. **Wanderers**: Uses `CharacterObject.CreateFrom()` and `HeroCreator.CreateBasicHero()`

### Error Handling
- All commands include comprehensive validation before execution
- Clear error messages for invalid inputs or state
- Graceful degradation when features are unavailable
- Minimal use of reflection (only for settlement renaming)

## Code Changes

### Modified Files
- `Bannerlord.GameMaster/Console/SettlementCommands/SettlementManagementCommands.cs`
  - Added 13 new command methods in organized regions
  - All commands use consistent patterns and error handling
  - Includes XML documentation for all methods
  - Added necessary using statements for workshops and caravans

### File Organization
Commands are organized into logical regions:
- **Settlement Ownership** (3 commands): set_owner, set_owner_clan, set_owner_kingdom
- **Settlement Properties** (5 commands): set_prosperity, set_hearths, rename, set_loyalty, set_security
- **Settlement Resources** (2 commands): give_food, give_gold
- **Settlement Military** (2 commands): add_militia, fill_garrison
- **Settlement Projects** (1 command): upgrade_buildings
- **Settlement Workshops** (2 commands): own_workshops, add_workshop
- **Settlement Caravans and NPCs** (2 commands): create_caravan, spawn_wanderer

## Testing

### Test Coverage
Added 32 test cases total for settlement management commands:
- 12 tests for original ownership commands
- 22 tests for new property/resource/workshop/caravan/NPC commands

### Standard Tests
- Argument validation (missing args, invalid args)
- Invalid settlement ID/name handling
- Settlement type validation (e.g., villages don't have prosperity)
- Value range validation
- Error message formatting

### Test Categories
All tests are categorized under `"SettlementManagement"` for easy filtering with `gm.test.run_category SettlementManagement`

## Breaking Changes
None. All new commands are additive.

## Dependencies
- Existing CommandBase infrastructure
- CommandValidator utility methods
- Settlement query system
- New namespaces: `TaleWorlds.CampaignSystem.Party.PartyComponents`, `TaleWorlds.CampaignSystem.Settlements.Workshops`

## Commands Not Implemented

### Tournament Starting
Could not implement due to unavailable `TournamentsCampaignBehavior` API in the accessible namespace.

### Crime Setting
No accessible API found for modifying settlement crime rates.

### Caravan Ownership Transfer
The `own_caravans` command was not implemented as caravan ownership is complex and would require unsafe manipulation of party ownership that could break game state.

## Performance Impact
Minimal. All commands execute synchronously with O(1) or O(n) complexity where n is small (e.g., buildings in settlement, workshops in city).

## Usage Examples

```bash
# Set a city's prosperity to 5000
gm.settlement.set_prosperity pen 5000

# Set a village's hearth to 500
gm.settlement.set_hearths village_E1_1 500

# Rename a settlement
gm.settlement.rename pen "New Penraic"

# Max out loyalty and security
gm.settlement.set_loyalty pen 100
gm.settlement.set_security pen 100

# Upgrade all buildings to level 3
gm.settlement.upgrade_buildings pen 3

# Give resources
gm.settlement.give_food pen 1000
gm.settlement.give_gold pen 10000

# Add militia
gm.settlement.add_militia pen 100

# Fill garrison to max capacity
gm.settlement.fill_garrison pen

# Manage workshops
gm.settlement.own_workshops pen
gm.settlement.add_workshop pen 2

# Create NPCs and parties
gm.settlement.create_caravan pen
gm.settlement.spawn_wanderer pen
```

## Related Changes
- Works with existing settlement query system ([`SettlementQueries.cs`](../../Bannerlord.GameMaster/Settlements/SettlementQueries.cs))
- Integrates with settlement ownership commands added previously
- Follows patterns from [`HeroManagementCommands.cs`](../../Bannerlord.GameMaster/Console/HeroCommands/HeroManagementCommands.cs)

## Future Enhancements
- Crime rate modification (if API becomes available)
- Tournament triggering (if API becomes stable)
- Advanced building management (specific building upgrades)
- Workshop type changing after creation