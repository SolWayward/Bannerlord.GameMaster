# Character Template Occupation Override Fix

**Date:** 2025-12-21  
**Type:** Bug Fix  
**Severity:** Critical  
**Affected Components:** Hero Generation, Template System  

## Issue Description

When creating lords using [`HeroGenerator`](Bannerlord.GameMaster/Heroes/HeroGenerator.cs), if a notable template (RuralNotable, Artisan, Merchant, etc.) was randomly selected, the created hero would retain the notable occupation in its `CharacterObject`, even after calling `hero.SetNewOccupation(Occupation.Lord)`.

### The Problem

Heroes have TWO occupation properties:
1. `hero.Occupation` - Runtime occupation (changeable via `SetNewOccupation()`)
2. `hero.CharacterObject.Occupation` - Template occupation (read-only from template)

Game systems check properties like `hero.IsRuralNotable`, which internally reads `hero.CharacterObject.Occupation`, NOT the hero's runtime occupation. This caused multiple crashes:

### Crash Example

```
System.NullReferenceException at DefaultVolunteerModel.GetBasicVolunteer()
Line 113: if (sellerHero.IsRuralNotable && sellerHero.CurrentSettlement.Village.Bound.IsCastle)

Problem:
- Hero "Vorstan the Brave" created from "Nord Land Captain Helpful Notable" template
- Hero.Occupation was set to Lord (correct)
- Hero.CharacterObject.Occupation was RuralNotable (from template - WRONG)
- Hero.IsRuralNotable returned true (checks CharacterObject)
- Hero was in Gretysfjord (a town, not a village)
- sellerHero.CurrentSettlement.Village was NULL (towns don't have Village property)
- CRASH: Null reference exception
```

### Why This Happened

The [`CharacterTemplatePooler.GetTemplatesFromFlags()`](Bannerlord.GameMaster/Characters/CharacterTemplates.cs:209) method returns ALL templates including:
- Lord templates ✓
- **Notable templates** (Headman, RuralNotable, Artisan, Merchant) ⚠️
- Wanderer templates
- Troop templates

When creating lords, a notable template could be randomly selected, creating a lord-hero with a notable CharacterObject occupation, causing the game to treat it as a notable in certain systems.

## Solution

Modified [`HeroGenerator.CreateBasicHero()`](Bannerlord.GameMaster/Heroes/HeroGenerator.cs:39) to automatically detect and override notable occupations in the CharacterObject using reflection.

### Changes Made

**File:** [`Bannerlord.GameMaster/Heroes/HeroGenerator.cs`](Bannerlord.GameMaster/Heroes/HeroGenerator.cs)

#### 1. Added Reflection-Based Occupation Override (Lines 28-41)

```csharp
/// <summary>
/// Overrides a CharacterObject's occupation to ensure consistency with hero's role.
/// This allows using any template while preventing occupation conflicts in game systems.
/// Uses reflection to set the private _occupation field on CharacterObject.
/// </summary>
private static void OverrideCharacterObjectOccupation(CharacterObject characterObject, Occupation newOccupation)
{
    try
    {
        // Use reflection to set the private _occupation field
        var occupationField = typeof(CharacterObject).GetField("_occupation", BindingFlags.NonPublic | BindingFlags.Instance);
        if (occupationField != null)
        {
            occupationField.SetValue(characterObject, newOccupation);
        }
    }
    catch (Exception ex)
    {
        InfoMessage.Display($"Warning: Failed to override CharacterObject occupation: {ex.Message}");
    }
}
```

#### 2. Updated InitializeAsLord() (Line ~110)

```csharp
// CRITICAL: Override CharacterObject occupation to match hero occupation
// This ensures game systems checking CharacterObject.Occupation get correct value
// Works with ANY template (notable, troop, etc.) while preserving visual properties
OverrideCharacterObjectOccupation(hero.CharacterObject, Occupation.Lord);
```

#### 3. Updated InitializeAsWanderer() (Line ~143)

```csharp
// CRITICAL: Override CharacterObject occupation to match hero occupation
// Ensures game systems treat wanderer correctly regardless of template used
OverrideCharacterObjectOccupation(hero.CharacterObject, Occupation.Wanderer);
```

#### 4. Updated InitializeAsCompanion() (Line ~165)

```csharp
// CRITICAL: Override CharacterObject occupation for companions
// Companions don't have a specific occupation, but we ensure consistency
// This prevents any template occupation from interfering
// Note: We use Lord here as companions typically share lord-like properties
OverrideCharacterObjectOccupation(hero.CharacterObject, Occupation.Lord);
```

### Design Decision: Always Override

The fix **unconditionally overrides** the CharacterObject occupation in every Initialize method, rather than checking if the template is a notable. This approach:

1. **Handles ALL edge cases** - Any weird template with any occupation is handled
2. **Simpler code** - No need for template occupation checking
3. **Guaranteed consistency** - CharacterObject.Occupation ALWAYS matches hero.Occupation
4. **Future-proof** - Works with any templates added by mods or game updates

## Benefits

### 1. Maximum Template Variety
- Can now use ALL character templates for lord creation
- Notables provide unique visual styles and equipment
- Much greater variety in hero appearance

### 2. No More Occupation Conflicts
- CharacterObject occupation is forcibly set to Lord
- Game systems correctly identify hero as Lord, not Notable
- No more crashes from notable-specific checks

### 3. Backwards Compatible
- Only affects newly created heroes
- Doesn't modify existing heroes in saves
- Safe to add/remove from mod

### 4. Preserves Template Properties
- Visual appearance unchanged
- Equipment from template retained
- Culture and other properties preserved
- Only occupation is modified

## Impact on Game Systems

### Before Fix
- `hero.IsRuralNotable` → **TRUE** (reads CharacterObject.Occupation)
- `hero.IsLord` → TRUE (reads hero.Occupation)
- Game confused about hero identity → CRASH

### After Fix
- `hero.IsRuralNotable` → **FALSE** (CharacterObject.Occupation is Lord)
- `hero.IsLord` → TRUE (hero.Occupation is Lord)
- Game correctly identifies hero as Lord → No crash

## Testing

### Test Cases

1. **Create lord from notable template:**
   ```
   gm.hero.create_lord TestLord
   ```
   - Hero may be created from notable template
   - Should function as lord without crashes
   - `hero.IsRuralNotable` should be false

2. **Verify occupation override:**
   - Check hero.CharacterObject.Occupation in debugger
   - Should be Occupation.Lord, not RuralNotable

3. **Settlement recruitment:**
   - Let game time pass
   - NPC parties should recruit from settlements without crashes
   - No null reference in DefaultVolunteerModel.GetBasicVolunteer()

4. **Issue generation:**
   - Let game check for issues at settlements
   - No crashes in NearbyBanditBaseIssueBehavior.ConditionsHold()

### Expected Results

**Before Fix:**
- Random crashes when notable templates selected
- Heroes incorrectly identified as notables
- Game systems fail with null reference exceptions

**After Fix:**
- All templates work correctly for lord creation
- Heroes always identified as lords
- No occupation-related crashes
- Greater visual variety in created heroes

## Related Issues

- Previous crash: [`HERO_INITIALIZATION_NOTABLE_CRASH_FIX_2025-12-21.md`](ChangeDocs/Fixes/HERO_INITIALIZATION_NOTABLE_CRASH_FIX_2025-12-21.md)
- Current crash: DefaultVolunteerModel.GetBasicVolunteer() line 113 NullReferenceException
- Future prevention: NearbyBanditBaseIssueBehavior.ConditionsHold() line 681 NullReferenceException

## Technical Notes

### Why Reflection is Necessary

The `CharacterObject._occupation` field is private with no public setter. The only way to change it after creation is through reflection. This is safe because:
1. We're only changing a simple enum value
2. Change happens immediately after hero creation
3. No other code has referenced the hero yet
4. Game systems will read the correct occupation

### Why Not Filter Templates Instead

Originally considered filtering notable templates out, but:
- Loses significant visual variety
- Notable templates often have unique, high-quality appearances
- Equipment from notables can be interesting
- Reflection solution is cleaner and more flexible

### Mod Removal Safety

This fix modifies heroes at creation time, making them indistinguishable from vanilla lords. If mod is removed:
- Heroes remain functional
- No save corruption
- Heroes treated as normal lords by vanilla game

## Implementation Notes

The fix is applied in `CreateBasicHero()`, which is the foundation method used by:
- `CreateLord()`
- `CreateLords()`
- `CreateWanderer()` (not affected, uses different templates)
- `CreateWanderers()` (not affected)
- `CreateCompanions()` (not affected)

This ensures ALL lord creation paths are protected from notable template conflicts.

---

**Status:** Completed and Ready for Testing  
**Approval:** Pending Verification  
**Version:** 1.0.0  
**Last Updated:** 2025-12-21
