# Troop Filtering Fix - 2025-12-16

## Overview
Fixed false positives in troop filtering where non-combat NPCs were incorrectly classified as combat troops.

## Problem Identified
When comparing `gm.query.troops` (594 results) vs `gm.query.character_objects` (1,369 results), analysis revealed ~74-94 non-combat NPCs were incorrectly included in the troop list.

### False Positives Found
1. **Entertainment/Event NPCs (~14 entries)**
   - `female_dancer_*` (7) - Tier 0, Level 1 tavern dancers
   - `taverngamehost_*` (7) - Tier 0, Level 1 game hosts
   - `tournament_master_*` (7) - Tier 0, Level 1 tournament organizers

2. **Special Character NPCs (~60+ entries)**
   - `spc_*_leader_*` - Minor faction leader NPCs (Tier 0, Level 1)
   - `spc_*_headman_*` - Village headmen NPCs
   - `spc_*_gangleader_*` - Gang leader NPCs
   - `spc_*_artisan_*` - Artisan NPCs
   - `spc_*_rural_notable_*` - Rural notable NPCs
   - `spc_*_e3_character_*` - E3 demo characters

### Root Causes
1. **Typo in filter pattern**: Code checked for `tavern_gamehost` but actual ID is `taverngamehost` (no underscore)
2. **Incomplete SPC filtering**: Only excluded `spc_notable_*` and `spc_wanderer_*` but many other `spc_` NPCs exist
3. **Missing entertainment NPC patterns**: Dancers and tournament masters not filtered

## Solution
Added comprehensive exclusion patterns to [`TroopExtensions.IsActualTroop()`](../../Bannerlord.GameMaster/Troops/TroopExtensions.cs:223) method:

### New Exclusion Rules (Section 7-8)
```csharp
// 7. Entertainment/Event NPCs - Dancers, tournament masters, game hosts
// These are Tier 0, Level 1 non-combat NPCs
if (stringIdLower.Contains("dancer") || 
    stringIdLower.Contains("tournament_master") || 
    stringIdLower.Contains("taverngamehost"))  // Note: no underscore in ID
    return false;

// 8. Special Character NPCs - Minor faction leaders, headmen, etc.
// These are Tier 0, Level 1 quest/story NPCs, NOT the actual combat troops
// Actual minor faction troops (tier_1/2/3) are properly included
if (stringIdLower.StartsWith("spc_") && 
    (stringIdLower.Contains("_leader_") || 
     stringIdLower.Contains("_headman_") ||
     stringIdLower.Contains("_gangleader_") ||
     stringIdLower.Contains("_artisan_") ||
     stringIdLower.Contains("_rural_notable_") ||
     stringIdLower.Contains("_e3_character_")))
    return false;
```

### Design Decision
**Only exclude Tier 0, Level 1 NPCs** - All combat-capable troops (even quest-specific) are preserved.

**Rationale:**
- Quest troops like `company_of_trouble_character` (Tier 3, Level 16 Mercenary) have real combat stats
- If added to party or used in battle, they would function properly
- Better to be inclusive for combat-capable units than risk excluding usable troops

## What's Preserved
✓ All regular military troops (all tiers, all cultures)
✓ Militia troops
✓ Mercenary troops  
✓ Caravan guards/masters/traders
✓ Bandits and bandit bosses
✓ Minor faction combat troops (e.g., `beni_zilal_tier_1/2/3`, `brotherhood_of_woods_tier_1/2/3`)
✓ Peasants/villagers (combat-capable in village raids)
✓ Quest-specific combat troops (e.g., `company_of_trouble_character`)

## Impact
- **Before**: 594 troops (included ~74-94 non-combat NPCs)
- **After**: ~500-520 actual combat troops
- **Filtered**: ~74-94 non-combat NPCs now correctly excluded

## Testing Recommendations
1. Run `gm.query.troops` to verify reduced count
2. Check that no combat-capable troops were excluded
3. Verify minor faction troops (tier_1/2/3) are still included
4. Confirm quest troops with combat stats remain accessible

## Files Modified
- [`Bannerlord.GameMaster/Troops/TroopExtensions.cs`](../../Bannerlord.GameMaster/Troops/TroopExtensions.cs) - Enhanced `IsActualTroop()` method with new exclusion patterns

## Related Documentation
- [Troop Query Implementation](../../docs/implementation/troop-query-implementation.md)
- [Troop Query Commands Wiki](../../wiki/Bannerlord.GameMaster.wiki/Troop-Query-Commands.md)