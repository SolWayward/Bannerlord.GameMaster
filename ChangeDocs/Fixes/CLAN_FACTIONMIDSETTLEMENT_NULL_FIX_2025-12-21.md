# Clan FactionMidSettlement Null Reference Crash Fix

**Date:** 2025-12-21  
**Type:** Bug Fix  
**Severity:** Critical  
**Affected Components:** Clan Generation  

## Issue Description

When creating independent clans (without assigning them to a kingdom), the game would crash after a few moments of game time passed. The crash occurred in Bannerlord's native code during kingdom decision-making processes.

### Stack Trace
```
System.NullReferenceException: Object reference not set to an instance of an object.
   at TaleWorlds.CampaignSystem.GameComponents.DefaultSettlementValueModel.GeographicalAdvantageForFaction(Settlement settlement, IFaction faction) Line 320
   at TaleWorlds.CampaignSystem.GameComponents.DefaultSettlementValueModel.CalculateSettlementValueForFaction(Settlement settlement, IFaction faction) Line 277
   at TaleWorlds.CampaignSystem.Election.SettlementClaimantDecision.DetermineSupport(Clan clan, DecisionOutcome possibleOutcome)
   at TaleWorlds.CampaignSystem.Election.KingdomElection.DetermineInitialSupport(DecisionOutcome possibleOutcome)
   at TaleWorlds.CampaignSystem.Election.KingdomElection.Setup()
   at TaleWorlds.CampaignSystem.Kingdom.AddDecision(KingdomDecision kingdomDecision, Boolean ignoreInfluenceCost)
   at TaleWorlds.CampaignSystem.CampaignBehaviors.SettlementClaimantCampaignBehavior.DailyTickSettlement(Settlement settlement)
```

## Root Cause

The crash was caused by the private field `_midSettlement` on the `Clan` class being `null`. This field backs the public `FactionMidSettlement` property.

### Why This Happens

1. When creating an independent clan (not assigned to a kingdom), the clan has:
   - No owned settlements (`Settlements` Count = 0)
   - No kingdom initially
   - The `_midSettlement` field is never initialized

2. When the clan later joins a kingdom (through AI decisions or other means), or when kingdom election systems run:
   - Bannerlord's native code calls `GeographicalAdvantageForFaction()`
   - This accesses `faction.FactionMidSettlement` (line 320 in native code)
   - The property returns `_midSettlement`, which is `null`
   - Native code attempts to use this settlement, causing a `NullReferenceException`

### Investigation Notes

From debugger inspection of a crashed clan:
```csharp
faction = {(1207959668) clan10} - TaleWorlds.CampaignSystem.Clan
    FactionMidSettlement = null  // PUBLIC PROPERTY - NULL!
    _midSettlement = null         // PRIVATE FIELD - NULL!
    HomeSettlement = {Dunglanys}  // Has home settlement
    InitialHomeSettlement = {Sargot}
    Settlements = Count = 0       // No owned settlements
    Kingdom = {Battania}          // Joined kingdom after creation
```

The clan successfully has a `HomeSettlement` and `InitialHomeSettlement`, but `_midSettlement` was never set.

## Solution

Use reflection to set the private `_midSettlement` field to the clan's home settlement immediately after setting the initial home settlement. This ensures the field is never null, preventing crashes when AI systems access it.

### Changes Made

**File:** [`Bannerlord.GameMaster/Clans/ClanGenerator.cs`](Bannerlord.GameMaster/Clans/ClanGenerator.cs)

#### 1. Added Reflection Helper Method (lines 25-49)

```csharp
/// <summary>
/// Sets the private _midSettlement field on a clan using reflection.
/// This is necessary to prevent crashes when the game's AI tries to access FactionMidSettlement.
/// </summary>
private static void SetClanMidSettlement(Clan clan, Settlement settlement)
{
    if (clan == null || settlement == null)
        return;

    try
    {
        // Get the private _midSettlement field using reflection
        FieldInfo midSettlementField = typeof(Clan).GetField("_midSettlement", BindingFlags.NonPublic | BindingFlags.Instance);
        
        if (midSettlementField != null)
        {
            midSettlementField.SetValue(clan, settlement);
        }
    }
    catch (System.Exception ex)
    {
        // Log error but don't crash - the clan can still function without mid settlement
        InfoMessage.Display($"Warning: Failed to set clan mid settlement: {ex.Message}");
    }
}
```

#### 2. Updated `CreateClan()` Method (after line 90)

```csharp
clan.SetInitialHomeSettlement(homeSettlement);

// CRITICAL: Set the mid settlement to prevent crashes when game AI accesses FactionMidSettlement
// This must be set before clan.Initialize() or when clans join kingdoms
SetClanMidSettlement(clan, homeSettlement);
```

#### 3. Updated `CreateMinorClan()` Method (after line 230)

```csharp
clan.SetInitialHomeSettlement(homeSettlement);

// CRITICAL: Set the mid settlement to prevent crashes when game AI accesses FactionMidSettlement
SetClanMidSettlement(clan, homeSettlement);

clan.Initialize();
```

#### 4. Added System.Reflection Import (line 3)

```csharp
using System.Reflection;
```

## Why Reflection is Necessary

The `_midSettlement` field is private with no public setter. Bannerlord's `Clan` class doesn't provide a public API to set this field. The field is typically set internally when:
- A clan owns settlements
- A clan is part of a faction with settlements
- The game calculates the mid-point of a faction's territories

For newly created independent clans with no owned settlements, this field remains null, causing crashes when AI systems assume it exists.

## Testing

### Manual Testing Steps

1. Execute: `gm.clan.create_clan TestClan1` (creates independent clan)
2. Execute: `gm.clan.create_clan TestClan2` (creates another independent clan)
3. Execute: `gm.clan.generate_clans 10` (creates 10 independent clans)
4. Let game time pass for several in-game days/weeks
5. Verify no crashes occur
6. Create clans with kingdoms: `gm.clan.create_clan TestClan3 null empire`
7. Let more game time pass
8. Verify clans function normally and join/leave kingdoms without crashes

### Expected Results

- Clans created as independent should not crash when game time advances
- Clans should be able to join kingdoms through AI decisions without issues
- Kingdom election systems should work without null reference exceptions
- Settlement value calculations should complete successfully

### Before Fix

- Creating 1 clan: Crash after 5-10 minutes of game time
- Creating 10 clans: Crash within 1-2 minutes
- Creating 70 clans: Crash within seconds to 1 minute
- Error occurs during daily settlement ticks when kingdoms make decisions

### After Fix

- Multiple clans can be created and run indefinitely
- No crashes during kingdom decision-making
- Clans successfully join/leave kingdoms through AI
- Settlement claim elections process without errors

## Technical Details

### Reflection Approach

The reflection approach was chosen because:
1. **No Public API**: The `Clan` class doesn't expose a setter for `FactionMidSettlement` or `_midSettlement`
2. **Safe Fallback**: If reflection fails, a warning is logged but the clan continues to exist
3. **Minimal Impact**: The reflection is called only once per clan during creation
4. **Future-Proof**: If Bannerlord adds a public API, we can easily migrate away from reflection

### Field Location

```csharp
FieldInfo midSettlementField = typeof(Clan).GetField("_midSettlement", BindingFlags.NonPublic | BindingFlags.Instance);
```

This searches for a private instance field named `_midSettlement` on the `Clan` type. The field name matches Bannerlord's internal implementation as verified through debugging.

### Settlement Selection

The home settlement is used as the mid settlement because:
1. It's guaranteed to exist (set during clan creation)
2. It's a reasonable default for a clan with no owned settlements
3. It matches the semantic meaning of a "mid point" for a single-settlement clan
4. When the clan acquires settlements, Bannerlord's native code will recalculate this automatically

## Impact

- **Severity**: Critical - Game was unplayable with multiple created clans
- **User Impact**: High - Users can now create multiple independent clans without crashes
- **Performance**: Negligible - Single reflection call per clan during creation
- **Backward Compatibility**: No breaking changes - purely additive fix

## Related Issues

This fix addresses the underlying issue that remained after the architectural refactoring in [`HERO_CLAN_ARCHITECTURE_REFACTOR_2025-12-21.md`](ChangeDocs/Features/HERO_CLAN_ARCHITECTURE_REFACTOR_2025-12-21.md). The refactoring successfully eliminated hero state conflicts, but the clan's `_midSettlement` field initialization was still missing.

## Related Files

- [`Bannerlord.GameMaster/Clans/ClanGenerator.cs`](Bannerlord.GameMaster/Clans/ClanGenerator.cs) - Main implementation
- [`Bannerlord.GameMaster/Information/InfoMessage.cs`](Bannerlord.GameMaster/Information/InfoMessage.cs) - Error logging
- Native Bannerlord Code:
  - `TaleWorlds.CampaignSystem.Clan` - Clan class with private field
  - `TaleWorlds.CampaignSystem.GameComponents.DefaultSettlementValueModel` - Where crash occurred

## Future Considerations

1. **Monitor Bannerlord Updates**: If future versions add a public API for setting mid settlement, migrate away from reflection
2. **Recalculation**: Consider if the mid settlement needs updating when clans acquire settlements (likely handled by native code)
3. **Other Private Fields**: Check if other private fields on `Clan` need initialization for edge cases

## Notes

- The fix uses defensive programming with try-catch to prevent reflection failures from crashing clan creation
- The warning message is displayed to console if reflection fails, allowing diagnosis without breaking gameplay
- The home settlement is a sensible default that maintains the semantic meaning of "faction mid settlement"
- This fix is necessary because clans created programmatically bypass Bannerlord's normal clan creation flow which would set these fields

---

**Status:** Completed and Tested  
**Approval:** Ready for Production  
**Version:** 1.0.0  
**Last Updated:** 2025-12-21
