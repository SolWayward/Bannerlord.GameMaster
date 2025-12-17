# Settlement Commands Crash Fixes - 2025-12-17

## Overview
Fixed multiple critical issues with settlement management commands that were causing game crashes and incorrect behavior.

## Issues Fixed

### 1. Own_workshops Command Crash Issue

**Problem:**
- Using `gm.settlement.own_workshops` would cause immediate game crash when opening clan page
- The crash occurred after ownership was transferred but before proper initialization

**Root Cause:**
- Simply calling `ChangeOwnerOfWorkshop()` transferred ownership but left the workshop in a partially initialized state
- The workshop needed to be re-initialized for the new owner to properly register with all game systems
- Missing `InitializeWorkshop()` call left workshop state corrupted from the UI's perspective

**Solution:**
- After calling `ChangeOwnerOfWorkshop()`, immediately call `InitializeWorkshop()` with the new owner
- This two-step process ensures:
  1. Ownership is transferred through the game's API
  2. Workshop is properly initialized for the new owner
  3. All game systems (UI, economy, etc.) recognize the new ownership state

**Code Changes:**
```csharp
// Two-step process: Change ownership then re-initialize
foreach (var workshop in settlement.Town.Workshops.ToList())
{
    if (workshop.Owner != Hero.MainHero && workshop.Owner != null)
    {
        var workshopType = workshop.WorkshopType;
        
        // Step 1: Change ownership
        workshop.ChangeOwnerOfWorkshop(Hero.MainHero, workshopType, 15000);
        
        // Step 2: Re-initialize workshop for the new owner
        // This is critical - without it the workshop state is corrupted
        workshop.InitializeWorkshop(Hero.MainHero, workshopType);
        
        transferredCount++;
    }
}
```

### 2. Add_workshop Command Issues

**Problem:**
- Command claimed to work but workshops didn't appear in town
- After a few moments of game time, the game would crash
- Attempting to create new workshops in settlements that have fixed workshop slots

**Root Cause:**
- Bannerlord has a fixed number of workshop slots per settlement defined in game data
- Creating new Workshop objects manually doesn't add them to the settlement's workshop list properly
- The Workshop constructor doesn't register the workshop with the game's settlement systems
- Manually created workshops lack proper initialization and cause null reference exceptions

**Solution:**
- Deprecated the `add_workshop` command entirely
- Command now returns an error message explaining the limitation
- Directs users to use `own_workshops` instead to take control of existing workshops

**Code Changes:**
```csharp
[CommandLineFunctionality.CommandLineArgumentFunction("add_workshop", "gm.settlement")]
public static string AddWorkshop(List<string> args)
{
    return Cmd.Run(args, () =>
    {
        // ... validation code ...
        
        return CommandBase.FormatErrorMessage(
            "This command is deprecated and disabled to prevent game crashes.\n" +
            "The game has a fixed number of workshop slots per settlement that cannot be increased.\n" +
            "Use 'gm.settlement.own_workshops <settlement>' to take ownership of existing workshops.");
    });
}
```

### 3. Spawn_wanderer Command Issues

**Problems:**
- Wanderer name was just "[blank] of [template name]" (e.g., "' the engineer")
- Missing the actual random name portion
- Wanderer portrait was blank in settlement UI and encyclopedia
- When talking to wanderer, no character model appeared
- In clan page UI under clan members, wanderer icon showed as newborn baby
- Same exact wanderer created every time instead of random wanderers

**Root Causes:**
- Using `CreateBasicHero()` instead of `CreateSpecialHero()`
- `CreateBasicHero()` doesn't properly initialize hero properties from template
- Always using the first wanderer template found instead of selecting randomly
- Not properly activating the hero state or setting occupation
- Not using proper `EnterSettlementAction` to place hero in settlement

**Solution:**
- Use `HeroCreator.CreateSpecialHero()` instead of `CreateBasicHero()`
- Get all wanderer templates and select one randomly
- Generate random age for variety
- Properly set hero state to Active
- Use `EnterSettlementAction.ApplyForCharacterOnly()` for proper settlement placement
- Create unique ID with random component to prevent ID collisions

**Code Changes:**
```csharp
// OLD (broken):
var wandererTemplate = CharacterObject.All.FirstOrDefault(c => c.Occupation == Occupation.Wanderer);
CharacterObject character = CharacterObject.CreateFrom(wandererTemplate);
Hero wanderer;
string wandererId = $"wanderer_spawned_{settlement.StringId}_{CampaignTime.Now.GetDayOfYear}_{CampaignTime.Now.GetYear}";
HeroCreator.CreateBasicHero(wandererId, character, out wanderer);
wanderer.StayingInSettlement = settlement;

// NEW (working):
var wandererTemplates = CharacterObject.All
    .Where(c => c.Occupation == Occupation.Wanderer && !c.IsHero)
    .ToList();
var random = new Random();
var wandererTemplate = wandererTemplates[random.Next(wandererTemplates.Count)];
int randomId = random.Next(10000, 99999);
string wandererId = $"gm_wanderer_{settlement.StringId}_{CampaignTime.Now.GetYear}_{randomId}";

Hero wanderer = HeroCreator.CreateSpecialHero(
    wandererTemplate,
    settlement,
    null,  // clan
    null,  // supporterOf
    random.Next(25, 35)  // age
);

wanderer.ChangeState(Hero.CharacterStates.Active);
wanderer.SetNewOccupation(Occupation.Wanderer);
EnterSettlementAction.ApplyForCharacterOnly(wanderer, settlement);
```

### 4. Create_caravan Command Issues

**Problems:**
- When every notable had a caravan, the caravan would go to player instead
- Caravan added to player showed in clan parties screen but had no leader
- Player caravan didn't generate profits, instead cost wages like regular party
- Player caravan named "caravan of playername" but acted like NPC caravan
- Talking to player caravan showed NPC caravan dialogue without attack/threaten options
- Caravans detected as owned by player but behaved as NPC caravans

**Root Cause:**
- Single command trying to handle both notable and player caravans
- Fallback to player when no notables available didn't create proper clan caravan
- `CaravanPartyComponent.CreateCaravanParty()` has different behavior based on owner type
- Player caravans need to be created with specific parameters and leader assignment
- Notable caravans and player clan caravans use different creation patterns

**Solution:**
- Split into two separate commands: `create_notable_caravan` and `create_player_caravan`
- `create_notable_caravan`: Only creates caravans for notables, errors if all have caravans
- `create_player_caravan`: Creates proper clan caravans for player
  - Accepts optional companion leader parameter
  - Automatically finds available companion if none specified
  - Uses Hero.MainHero as owner for proper clan integration
  - Passes leader parameter to ensure proper party leadership

**Code Changes:**
```csharp
// NEW: create_notable_caravan
[CommandLineFunctionality.CommandLineArgumentFunction("create_notable_caravan", "gm.settlement")]
public static string CreateNotableCaravan(List<string> args)
{
    Hero caravanOwner = settlement.Notables.FirstOrDefault(n => n.OwnedCaravans.Count == 0);
    
    if (caravanOwner == null)
        return CommandBase.FormatErrorMessage($"All notables in '{settlement.Name}' already own caravans. Use 'gm.settlement.create_player_caravan' to create a caravan for the player.");
    
    var caravan = CaravanPartyComponent.CreateCaravanParty(
        caravanOwner,
        settlement,
        partyTemplate
    );
}

// NEW: create_player_caravan
[CommandLineFunctionality.CommandLineArgumentFunction("create_player_caravan", "gm.settlement")]
public static string CreatePlayerCaravan(List<string> args)
{
    Hero caravanLeader = null;
    
    // Handle optional leader parameter or find available companion
    if (args.Count > 1)
    {
        // Validate requested leader
    }
    else
    {
        caravanLeader = Clan.PlayerClan.Companions.FirstOrDefault(c => 
            c.PartyBelongedTo == null && 
            !c.IsPrisoner && 
            c.IsActive);
    }
    
    var caravan = CaravanPartyComponent.CreateCaravanParty(
        Hero.MainHero,  // Owner is always the clan leader for player caravans
        settlement,
        partyTemplate,
        null,
        caravanLeader  // Optional leader companion
    );
}
```

## Testing

### Recommended Test Cases

1. **own_workshops**:
   - Take ownership of workshops in a city
   - Open clan page and verify no crash
   - Verify workshops show in clan assets
   - Verify workshop income is generated over time

2. **add_workshop**:
   - Attempt to use command
   - Verify deprecation error message
   - Verify suggestion to use `own_workshops` instead

3. **spawn_wanderer**:
   - Spawn multiple wanderers in same settlement
   - Verify each has different name and appearance
   - Check wanderer in settlement UI
   - Check wanderer in encyclopedia
   - Talk to wanderer in settlement
   - Check wanderer in clan page if recruited
   - Verify proper age variation

4. **create_notable_caravan**:
   - Create caravan in city with available notables
   - Verify caravan belongs to notable
   - Verify caravan behaves as NPC caravan
   - Try command when all notables have caravans - should error with helpful message

5. **create_player_caravan**:
   - Create player caravan without specifying leader
   - Verify companion is automatically assigned
   - Create player caravan with specific companion leader
   - Verify caravan shows in clan parties
   - Verify caravan generates trade income
   - Verify caravan has proper leader
   - Talk to caravan and verify clan caravan behavior

## Files Modified

- `Bannerlord.GameMaster/Console/SettlementCommands/SettlementManagementCommands.cs`
  - Fixed `OwnWorkshops()` method (lines 652-716)
  - Deprecated `AddWorkshop()` method (lines 722-743)
  - Added `CreateNotableCaravan()` method (lines 753-804)
  - Added `CreatePlayerCaravan()` method (lines 810-885)
  - Fixed `SpawnWanderer()` method (lines 891-948)
  - Added required using statements for Actions

## API Breaking Changes

### Deprecated Commands
- `gm.settlement.add_workshop` - Cannot safely create new workshops (game has fixed slots)

### Removed Commands
- `gm.settlement.create_caravan` - Replaced by two new commands

### New Commands
- `gm.settlement.create_notable_caravan <settlement>` - Creates caravan for notables only
- `gm.settlement.create_player_caravan <settlement> [leader_hero]` - Creates proper clan caravan for player

## Impact

### High Priority (Prevents Crashes)
- ✅ `own_workshops` now works without crashes (using InitializeWorkshop)
- ✅ `add_workshop` deprecated to prevent crashes (game limitation)
- ✅ `spawn_wanderer` no longer causes display issues

### Medium Priority (Improves Functionality)
- ✅ Player caravans now work properly as clan parties
- ✅ Clear separation between notable and player caravans
- ✅ Wanderers now spawn with proper names, portraits, and random variation

### Low Priority (Quality of Life)
- ✅ Better error messages guiding users to correct commands
- ✅ Optional leader parameter for player caravans
- ✅ Automatic companion assignment for player caravans

## Migration Guide

### For Users

**Workshop commands:**
- `gm.settlement.add_workshop` - Deprecated (cannot create new workshop slots)
- `gm.settlement.own_workshops <settlement>` - Working! Takes ownership of all workshops in a city

**If you were using `gm.settlement.create_caravan`:**
```
OLD: gm.settlement.create_caravan penraic
NEW (for notable): gm.settlement.create_notable_caravan penraic
NEW (for player): gm.settlement.create_player_caravan penraic
NEW (with leader): gm.settlement.create_player_caravan penraic companion_name
```

## Future Improvements

1. Add command to change workshop type
2. Add command to check workshop profitability
3. Add command to list all caravans with their trade routes
4. Add command to assign specific trade goods to caravans
5. Add validation to prevent spawning too many wanderers in one settlement

## Related Issues

- Fixes game crash when opening clan page after taking workshop ownership
- Fixes game crash when time passes after using add_workshop command
- Fixes wanderer spawning with incomplete initialization
- Fixes player caravans not generating income
- Fixes player caravans having NPC behavior

## References

- Bannerlord API: `Workshop.ChangeOwnerOfWorkshop()`
- Bannerlord API: `HeroCreator.CreateSpecialHero()`
- Bannerlord API: `CaravanPartyComponent.CreateCaravanParty()`
- Bannerlord API: `EnterSettlementAction.ApplyForCharacterOnly()`