# Clan Creation Null Reference Exception Fix

**Date:** 2025-12-21
**Type:** Bug Fix
**Severity:** Critical
**Affected Components:** Clan Generation, Hero Generation

## Issue Description

When calling [`gm.clan.create_clan`](Bannerlord.GameMaster/Console/ClanCommands/ClanGenerationCommands.cs:29) command, a null reference exception was thrown inside `LordPartyComponent.CreateLordParty()` method. The exception occurred at `Clan.get_DefaultPartyTemplate()` when the system attempted to create a party for the clan leader.

### Stack Trace
```
System.NullReferenceException: Object reference not set to an instance of an object.
   at TaleWorlds.CampaignSystem.Clan.get_DefaultPartyTemplate()
   at TaleWorlds.CampaignSystem.Party.PartyComponents.LordPartyComponent.InitializationArgs.InitializeLordPartyProperties(MobileParty mobileParty, Hero owner)
   at TaleWorlds.CampaignSystem.Party.PartyComponents.LordPartyComponent.OnMobilePartySetOnCreation()
   at TaleWorlds.CampaignSystem.Party.MobileParty.CreateParty(String stringId, PartyComponent component)
   at Bannerlord.GameMaster.Heroes.HeroExtensions.CreateParty(Hero hero, Settlement spawnSettlement)
   at Bannerlord.GameMaster.Heroes.HeroGenerator.CreateHero(CharacterObject template, TextObject nameObj, Occupation occupation, Clan clan)
```

## Root Cause

The clan creation workflow had a critical timing issue:

1. [`ClanGenerator.CreateClan()`](Bannerlord.GameMaster/Clans/ClanGenerator.cs:42) created a new clan
2. The clan's `DefaultPartyTemplate` property was not initialized
3. When creating a hero for the clan via [`HeroGenerator.CreateHeroesFromRandomTemplates()`](Bannerlord.GameMaster/Heroes/HeroGenerator.cs:58)
4. Inside [`HeroGenerator.CreateHero()`](Bannerlord.GameMaster/Heroes/HeroGenerator.cs:126), if the hero occupation is Lord, it automatically creates a party
5. [`HeroExtensions.CreateParty()`](Bannerlord.GameMaster/Heroes/HeroExtensions.cs:107) calls `LordPartyComponent.CreateLordParty()`
6. This internal method attempts to access `hero.Clan.DefaultPartyTemplate`, which was null, causing the exception

## Solution

Set the clan's `DefaultPartyTemplate` **before** creating any heroes that might automatically create parties.

### Changes Made

**File:** [`Bannerlord.GameMaster/Clans/ClanGenerator.cs`](Bannerlord.GameMaster/Clans/ClanGenerator.cs:42)

Added initialization logic after clan creation (lines 52-60):

```csharp
// Set initial culture to determine default party template
// If leader is provided, use their culture, otherwise default to a random culture temporarily
BasicCultureObject initialCulture = leader?.Culture ?? BasicCultureObject.All.First();
clan.Culture = initialCulture;
clan.BasicTroop = initialCulture.BasicTroop;

// CRITICAL: Set DefaultPartyTemplate before creating any heroes with parties
// This prevents null reference exception when LordPartyComponent.CreateLordParty is called
clan.InitializeClan(initialCulture.DefaultPartyTemplate, initialCulture.DefaultPartyTemplate, null, null);

leader ??= HeroGenerator.CreateHeroesFromRandomTemplates(1, clan: clan, randomFactor: 1)[0];
```

### Key Points

1. **Early Initialization**: The clan is now initialized with a `DefaultPartyTemplate` immediately after creation
2. **Culture Handling**: If a leader is provided, use their culture; otherwise, use a temporary culture that will be updated later
3. **Order of Operations**: The fix ensures the clan is fully initialized before any hero creation that might trigger party creation

## Testing

### Manual Testing Steps
1. Execute: `gm.clan.create_clan TestClan`
2. Execute: `gm.clan.create_clan TestClan2 existing_hero_id`
3. Execute: `gm.clan.generate_clans 5`
4. Verify no null reference exceptions occur
5. Verify clans are created with proper parties and leaders

### Expected Results
- Clans should be created successfully without exceptions
- Clan leaders should have parties created automatically
- All party properties should be properly initialized

## Related Files
- [`Bannerlord.GameMaster/Clans/ClanGenerator.cs`](Bannerlord.GameMaster/Clans/ClanGenerator.cs:42)
- [`Bannerlord.GameMaster/Heroes/HeroGenerator.cs`](Bannerlord.GameMaster/Heroes/HeroGenerator.cs:126)
- [`Bannerlord.GameMaster/Heroes/HeroExtensions.cs`](Bannerlord.GameMaster/Heroes/HeroExtensions.cs:107)
- [`Bannerlord.GameMaster/Console/ClanCommands/ClanGenerationCommands.cs`](Bannerlord.GameMaster/Console/ClanCommands/ClanGenerationCommands.cs:29)

## Impact
- **Severity**: Critical - Command was completely non-functional
- **User Impact**: High - Users can now successfully create clans via console commands
- **Backward Compatibility**: No breaking changes - this is a pure fix

## Notes
- The culture and basic troop are set twice in the code (once before hero creation, once after). This is intentional:
  - First setting: Uses provided leader's culture or temporary culture for initialization
  - Second setting: Updates to actual leader's culture after leader is created/assigned
- This fix also resolves the same issue in [`ClanGenerator.GenerateClans()`](Bannerlord.GameMaster/Clans/ClanGenerator.cs:125) since it calls `CreateClan()`
