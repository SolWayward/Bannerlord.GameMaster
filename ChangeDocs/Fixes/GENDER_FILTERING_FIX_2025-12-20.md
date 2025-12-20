# Gender Filtering Fix for Individual Cultures - 2025-12-20

## Overview
Fixed a critical bug in the `generate_lords` command where the gender parameter was ignored when generating heroes from individual cultures (e.g., Vlandia, Sturgia, Aserai).

## Issue Description
### Symptoms
- When using `gamemaster.generate_lords -culture vlandia -gender female`, the command would generate both male and female heroes
- Gender filtering worked correctly for culture groups (AllMainCultures, AllBanditCultures) but failed for individual cultures
- Users could not reliably generate heroes of a specific gender when specifying single cultures

### Root Cause
The `GetTemplatesFromFlags()` method in [`CharacterTemplates.cs`](../../Bannerlord.GameMaster/Characters/CharacterTemplates.cs) had inconsistent gender filtering logic:
- Lines 160-188: Culture groups (AllCultures, AllMainCultures, AllBanditCultures) properly applied gender filtering using pre-filtered template lists
- Lines 194-240: Individual culture processing accumulated templates using `GetCulturalTemplates()` which only filtered by culture, completely ignoring the `genderFlags` parameter
- The method returned unfiltered templates, resulting in random gender selection during hero creation

## Solution
### Implementation
Modified the individual culture processing section to apply gender filtering to accumulated templates before returning them:

```csharp
// Apply gender filtering to accumulated templates
return genderFlags switch
{
    GenderFlags.Female => FilterByGender(templates, true),
    GenderFlags.Male => FilterByGender(templates, false),
    _ => templates
};
```

### Technical Details
- **File Modified**: `Bannerlord.GameMaster/Characters/CharacterTemplates.cs`
- **Lines Changed**: 242-248 (added gender filtering logic before return statement)
- **Method**: `GetTemplatesFromFlags(CultureFlags, GenderFlags)`
- **Approach**: Applied post-accumulation filtering using the existing `FilterByGender()` helper method

### Design Rationale
- Chose Option 2 (post-accumulation filtering) as it:
  - Maintains consistency with the culture group filtering pattern (lines 162-167, 172-177, 182-187)
  - Minimizes code changes and maintains readability
  - Reuses existing `FilterByGender()` helper method
  - Handles all gender flag cases: Female, Male, and Both (default)

## Testing Scope
### Affected Cultures
The fix applies to all individual culture flags:
- **Main Factions**: Calradian, Aserai, Battania, Empire, Khuzait, Nord, Sturgia, Vlandia
- **Bandits**: Corsairs, DesertBandits, ForestBandits, MountainBandits, SeaRaiders, SteppeBandits
- **Special**: DarshiSpecial, VakkenSpecial

### Test Cases Required
1. **Single Culture with Female Gender**: `gamemaster.generate_lords -culture vlandia -gender female -count 10`
   - Expected: All 10 heroes should be female Vlandian
2. **Single Culture with Male Gender**: `gamemaster.generate_lords -culture sturgia -gender male -count 10`
   - Expected: All 10 heroes should be male Sturgian
3. **Single Culture with Both Genders**: `gamemaster.generate_lords -culture empire -count 10`
   - Expected: Mix of male and female Empire heroes
4. **Multiple Cultures with Gender**: `gamemaster.generate_lords -culture "vlandia,battania" -gender female -count 10`
   - Expected: All female heroes from Vlandia and Battania
5. **Culture Group Regression Test**: `gamemaster.generate_lords -culture allmaincultures -gender male -count 10`
   - Expected: All male heroes from main factions (existing functionality should still work)

## Impact Assessment
### Before Fix
- Gender parameter was non-functional for individual cultures
- Users experienced unexpected behavior and inconsistent results
- Workaround required using culture groups instead of specific cultures

### After Fix
- Gender filtering now works consistently across all culture specifications
- Individual cultures behave identically to culture groups regarding gender filtering
- Command behavior matches user expectations and documentation

## Related Files
- **Source**: `Bannerlord.GameMaster/Characters/CharacterTemplates.cs` (lines 242-248)
- **Command**: `Bannerlord.GameMaster/Console/HeroCommands/HeroGenerationCommands.cs`
- **Helper**: `Bannerlord.GameMaster/Characters/GenderFlags.cs`

## Compatibility
- **Breaking Changes**: None
- **API Changes**: None (internal method behavior correction)
- **Backward Compatibility**: Maintained - existing calls continue to work, now with correct behavior

## Notes
- The fix maintains the existing code style and patterns
- No new dependencies or methods were introduced
- The solution is consistent with how culture groups handle gender filtering
- Performance impact is negligible (existing FilterByGender method is already optimized)

## Version
- **Fix Date**: 2025-12-20
- **Severity**: High (core functionality bug)
- **Type**: Bugfix
- **Component**: Character Template System
