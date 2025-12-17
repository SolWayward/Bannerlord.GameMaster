# Settlement Ownership Commands Feature

**Date:** 2025-12-17  
**Type:** Feature Addition  
**Category:** Settlement Management  
**Scope:** Console Commands, Command Base, Tests

## Overview

Added three new console commands to manage settlement ownership in Bannerlord.GameMaster, allowing players to change settlement ownership by hero, clan, or kingdom. These commands automatically update related properties (owner, owner clan, and map faction) to maintain consistency.

## Changes Made

### 1. CommandBase.cs Updates
**File:** `Bannerlord.GameMaster/Console/Common/CommandBase.cs`

- Added `FindSingleSettlement()` method to support settlement entity resolution
- Added `using` statements for Settlement support
- Follows the existing pattern used for Hero, Clan, Kingdom, Item, and Troop entities

**Method Signature:**
```csharp
public static (Settlement settlement, string error) FindSingleSettlement(string query)
```

### 2. New SettlementManagementCommands.cs
**File:** `Bannerlord.GameMaster/Console/SettlementCommands/SettlementManagementCommands.cs`

Created new command class with three ownership management commands:

#### Command 1: `gm.settlement.set_owner`
- **Syntax:** `gm.settlement.set_owner <settlementQuery> <heroQuery>`
- **Purpose:** Sets settlement owner to a specific hero
- **Behavior:**
  - Sets settlement owner to the specified hero
  - Automatically sets owner clan to the hero's clan
  - Automatically sets map faction to the hero's faction (or clan if no faction)

#### Command 2: `gm.settlement.set_owner_clan`
- **Syntax:** `gm.settlement.set_owner_clan <settlementQuery> <clanQuery>`
- **Purpose:** Sets settlement ownership to a clan
- **Behavior:**
  - Sets settlement owner clan to the specified clan
  - Automatically sets owner to the clan leader
  - Automatically sets map faction to the clan's kingdom (or clan if no kingdom)

#### Command 3: `gm.settlement.set_owner_kingdom`
- **Syntax:** `gm.settlement.set_owner_kingdom <settlementQuery> <kingdomQuery>`
- **Purpose:** Sets settlement ownership to a kingdom
- **Behavior:**
  - Sets settlement owner clan to the kingdom's ruling clan
  - Automatically sets owner to the kingdom ruler
  - Automatically sets map faction to the kingdom

### 3. Test Cases Added
**File:** `Bannerlord.GameMaster/Console/Testing/StandardTests.cs`

Added comprehensive test coverage for all three commands:

**Tests Added:**
- `settlement_mgmt_001` to `settlement_mgmt_012`: 12 test cases covering:
  - Missing arguments validation
  - Invalid entity ID validation
  - Proper error messaging
  - All three ownership commands

**Test Categories:**
- Argument validation tests
- Entity resolution error tests
- Invalid input handling tests

## Implementation Details

### Design Patterns

1. **Consistent Error Handling:**
   - Uses `Cmd.Run()` wrapper for automatic logging
   - Uses `CommandBase.ExecuteWithErrorHandling()` for state-modifying operations
   - Follows established error message formatting

2. **Entity Resolution:**
   - Uses `CommandBase.FindSingleSettlement()` for settlement queries
   - Uses `CommandBase.FindSingleHero()` for hero queries
   - Uses `CommandBase.FindSingleClan()` for clan queries
   - Uses `CommandBase.FindSingleKingdom()` for kingdom queries

3. **Validation Chain:**
   - Campaign mode validation
   - Argument count validation
   - Entity resolution validation
   - Business logic validation (e.g., clan has leader, kingdom has ruler)

### Code Quality

- All methods include XML documentation comments
- Follows established naming conventions
- Uses regions for organization
- Implements proper validation before execution
- Provides detailed success messages showing before/after states

## Usage Examples

### Change owner to a hero:
```
gm.settlement.set_owner pen lord_1_1
```
Sets Pen Cannoc's owner to lord_1_1, automatically updating the owner clan and map faction.

### Change owner to a clan:
```
gm.settlement.set_owner_clan marunath empire_south
```
Sets Marunath's ownership to clan_empire_south_1, automatically setting the owner to the clan leader.

### Change owner to a kingdom:
```
gm.settlement.set_owner_kingdom zeonica empire
```
Sets Zeonica's ownership to the Empire, automatically setting the owner to the empire ruler and owner clan to the ruling clan.

## Testing

### Test Execution
Run the following commands to test:
```
gm.test.run_category SettlementManagement
```

### Expected Results
- All 12 test cases should pass
- Error messages should be clear and helpful
- Invalid inputs should be properly rejected

## Benefits

1. **Comprehensive Ownership Management:** Provides three different ways to change settlement ownership based on the desired scope
2. **Automatic Updates:** Automatically maintains consistency between owner, owner clan, and map faction
3. **User-Friendly:** Clear syntax and helpful error messages
4. **Consistent with Existing Patterns:** Follows all established patterns for command implementation, testing, and documentation
5. **Well-Tested:** Includes comprehensive test coverage for error cases

## Related Files

- `Bannerlord.GameMaster/Console/Common/CommandBase.cs` - Added FindSingleSettlement()
- `Bannerlord.GameMaster/Console/SettlementCommands/SettlementManagementCommands.cs` - New command class
- `Bannerlord.GameMaster/Console/Testing/StandardTests.cs` - Added test cases
- `Bannerlord.GameMaster/Settlements/SettlementQueries.cs` - Used for settlement queries
- `Bannerlord.GameMaster/Settlements/SettlementExtensions.cs` - Used for settlement type checking

## Notes

- The commands work with all settlement types: cities, castles, villages, and hideouts
- Settlement queries support both name and ID matching
- The commands maintain game state consistency by updating all related properties
- Success messages show the previous and new values for owner, owner clan, and map faction

## Future Enhancements

Potential future additions:
- `gm.settlement.set_prosperity` - Change settlement prosperity
- `gm.settlement.set_garrison` - Modify garrison troops
- `gm.settlement.set_loyalty` - Change settlement loyalty
- `gm.settlement.add_notables` - Add notable characters to settlements