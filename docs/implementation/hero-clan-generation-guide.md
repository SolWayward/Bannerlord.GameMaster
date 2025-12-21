# Hero and Clan Generation Implementation Guide

**Navigation:** [← Back: Code Quality Checklist](code-quality-checklist.md) | [Back to Index](../README.md)

---

## Overview

This guide explains how to use the refactored hero and clan generation system. The new architecture separates creation from initialization, providing clear intent and preventing hidden side effects.

## Table of Contents

1. [Architecture Overview](#architecture-overview)
2. [Hero Creation](#hero-creation)
3. [Clan Creation](#clan-creation)
4. [Common Use Cases](#common-use-cases)
5. [Best Practices](#best-practices)
6. [Troubleshooting](#troubleshooting)

---

## Architecture Overview

### Three-Layer System

```
Layer 1: Core Creation (Private)
  ↓ CreateBasicHero() - No side effects
  
Layer 2: Role Initialization (Public)
  ↓ InitializeAsLord(), InitializeAsWanderer(), InitializeAsCompanion()
  
Layer 3: Convenience Methods (Public)
  ↓ CreateLord(), CreateWanderer(), CreateCompanions()
```

### Design Principles

1. **Separation of Concerns**: Creation is separate from initialization
2. **Explicit Side Effects**: Method names clearly indicate what happens
3. **Composable**: Can mix and match initialization methods
4. **Backward Compatible**: Legacy methods still work
5. **Testing-Friendly**: Each method does one thing

---

## Hero Creation

### Creating Lords

Lords are nobles with occupation, equipment, and optionally parties.

#### Single Lord

```csharp
using Bannerlord.GameMaster.Heroes;
using Bannerlord.GameMaster.Characters;

// Create lord with party (most common)
Hero lord = HeroGenerator.CreateLord(
    name: "Sir Galahad",
    cultureFlags: CultureFlags.Vlandia,
    genderFlags: GenderFlags.Male,
    clan: myClan,
    withParty: true,
    randomFactor: 0.5f
);

// Create lord without party (family member)
Hero familyMember = HeroGenerator.CreateLord(
    name: "Lady Elara",
    cultureFlags: CultureFlags.Vlandia,
    genderFlags: GenderFlags.Female,
    clan: myClan,
    withParty: false,  // No party
    randomFactor: 0.5f
);
```

#### Multiple Lords

```csharp
// Create 5 lords, all with parties
List<Hero> lords = HeroGenerator.CreateLords(
    count: 5,
    cultureFlags: CultureFlags.Vlandia | CultureFlags.Battania,
    genderFlags: GenderFlags.Either,
    clan: myClan,
    withParties: true,
    randomFactor: 0.7f
);

// Names are randomly generated from culture
```

### Creating Wanderers

Wanderers are recruitable companions that appear in settlement taverns.

#### Single Wanderer

```csharp
Settlement town = Settlement.Find("town_empire_3");

Hero wanderer = HeroGenerator.CreateWanderer(
    name: "Wandering Bard",
    cultureFlags: CultureFlags.Empire,
    genderFlags: GenderFlags.Either,
    settlement: town,
    randomFactor: 0.5f
);

// Wanderer now waits in town's tavern
// Can be recruited by player or AI
```

#### Multiple Wanderers

```csharp
Settlement town = Settlement.Find("town_vlandia_1");

// Create 5 wanderers at same settlement
List<Hero> wanderers = HeroGenerator.CreateWanderers(
    count: 5,
    cultureFlags: CultureFlags.AllMainCultures,
    genderFlags: GenderFlags.Either,
    settlement: town,
    randomFactor: 0.6f
);

// All wanderers wait in town's tavern
```

### Creating Companions

Companions are heroes ready to be added directly to party rosters (not wanderers in settlements).

```csharp
// Get party to add companions to
MobileParty party = Hero.MainHero.PartyBelongedTo;

// Create companions ready for party addition
List<Hero> companions = HeroGenerator.CreateCompanions(
    count: 3,
    cultureFlags: CultureFlags.Battania,
    genderFlags: GenderFlags.Either,
    clan: party.LeaderHero.Clan,
    randomFactor: 0.5f
);

// Add companions to party (extension method)
party.AddCompanionsToParty(companions);
```

### Manual Initialization (Advanced)

For custom hero creation workflows:

```csharp
// Step 1: Create template pool
CharacterTemplatePooler pooler = new CharacterTemplatePooler();
List<CharacterObject> templates = pooler.GetTemplatesFromFlags(
    CultureFlags.Vlandia, 
    GenderFlags.Male
);

// Step 2: Select template
CharacterObject template = templates[0];
TextObject nameObj = new TextObject("Custom Hero");

// Step 3: Create basic hero (private method - use via public methods)
// Note: CreateBasicHero is private, this is just conceptual

// Use public initialization methods after creation
Hero hero = /* created through other means */;

// Initialize as lord with party
Settlement homeSettlement = hero.GetHomeOrAlternativeSettlement();
HeroGenerator.InitializeAsLord(hero, homeSettlement, createParty: true);

// OR initialize as wanderer
HeroGenerator.InitializeAsWanderer(hero, settlement);

// OR initialize as companion
HeroGenerator.InitializeAsCompanion(hero);
```

### State Cleanup

When moving heroes between roles or clans:

```csharp
Hero existingHero = /* get hero from somewhere */;

// Clean up any existing party/settlement state
HeroGenerator.CleanupHeroState(existingHero);

// Now safe to reinitialize
existingHero.Clan = newClan;
HeroGenerator.InitializeAsLord(existingHero, settlement, createParty: true);
```

---

## Clan Creation

### Basic Clan Creation

```csharp
using Bannerlord.GameMaster.Clans;

// Create clan with auto-generated leader
Clan clan = ClanGenerator.CreateClan(
    name: "House Stark",
    leader: null,  // Auto-generate
    kingdom: null, // Independent
    createParty: true,      // Leader gets party
    companionCount: 2       // 2 companions in leader's party
);

// Create clan with existing hero as leader
Hero myHero = /* get hero */;
Clan clan2 = ClanGenerator.CreateClan(
    name: "House Lannister",
    leader: myHero,
    kingdom: myKingdom,
    createParty: true,
    companionCount: 5
);

// Create clan without party (for testing or special cases)
Clan clan3 = ClanGenerator.CreateClan(
    name: "House Baratheon",
    leader: null,
    kingdom: null,
    createParty: false,     // No party
    companionCount: 0       // No companions
);
```

### Generating Multiple Clans

```csharp
// Generate 10 clans, all join kingdom
List<Clan> clans = ClanGenerator.GenerateClans(
    count: 10,
    cultureFlags: CultureFlags.AllMainCultures,
    kingdom: myKingdom,
    createParties: true,    // All leaders get parties
    companionCount: 2       // 2 companions per clan
);

// Generate independent clans without parties
List<Clan> minorClans = ClanGenerator.GenerateClans(
    count: 5,
    cultureFlags: CultureFlags.Vlandia | CultureFlags.Battania,
    kingdom: null,          // Independent
    createParties: false,   // No parties
    companionCount: 0       // No companions
);
```

### Creating Minor Clans

Minor clans are not noble houses (tier 1, less gold/influence):

```csharp
// Create mercenary company
Clan mercenaries = ClanGenerator.CreateMinorClan(
    name: "Mercenary Company",
    leader: null,
    cultureFlags: CultureFlags.AllMainCultures,
    createParty: true
);

// Create bandit clan
Clan bandits = ClanGenerator.CreateMinorClan(
    name: "Forest Bandits",
    leader: null,
    cultureFlags: CultureFlags.BanditCultures,
    createParty: true
);
```

---

## Common Use Cases

### Use Case 1: Populate Kingdom with Clans

```csharp
Kingdom myKingdom = /* get kingdom */;

// Generate 15 clans for the kingdom
List<Clan> kingdomClans = ClanGenerator.GenerateClans(
    count: 15,
    cultureFlags: myKingdom.Culture.ToCultureFlag(),
    kingdom: myKingdom,
    createParties: true,
    companionCount: 3
);

// Each clan:
// - Has leader with party
// - Has 3 companions
// - Is member of kingdom
// - Has tier 3, good gold/influence
```

### Use Case 2: Create Recruiting Pool in Town

```csharp
Settlement town = Settlement.Find("town_empire_3");

// Create 10 wanderers for recruitment
for (int i = 0; i < 10; i++)
{
    string[] names = { "Jeremus", "Ymira", "Matheld", "Nizar", "Rolf" };
    
    HeroGenerator.CreateWanderer(
        name: names[i % names.Length] + " " + i,
        cultureFlags: town.Culture.ToCultureFlag(),
        genderFlags: GenderFlags.Either,
        settlement: town,
        randomFactor: 0.7f
    );
}

// All wanderers now available in town tavern
```

### Use Case 3: Create Noble Family

```csharp
Clan clan = /* existing clan */;

// Create family head (leader)
Hero leader = HeroGenerator.CreateLord(
    "Lord Eddard",
    clan.Culture.ToCultureFlag(),
    GenderFlags.Male,
    clan,
    withParty: true,
    randomFactor: 0.5f
);

// Create spouse
Hero spouse = HeroGenerator.CreateLord(
    "Lady Catelyn",
    clan.Culture.ToCultureFlag(),
    GenderFlags.Female,
    clan,
    withParty: false,  // No party for spouse
    randomFactor: 0.5f
);

// Create children
List<Hero> children = HeroGenerator.CreateLords(
    count: 3,
    cultureFlags: clan.Culture.ToCultureFlag(),
    genderFlags: GenderFlags.Either,
    clan: clan,
    withParties: false,  // Children don't have parties yet
    randomFactor: 0.5f
);

// Set relationships (requires additional relationship system)
```

### Use Case 4: Expand Player's Party

```csharp
MobileParty playerParty = Hero.MainHero.PartyBelongedTo;

// Add 5 companions to player's party
List<Hero> newCompanions = HeroGenerator.CreateCompanions(
    count: 5,
    cultureFlags: CultureFlags.AllMainCultures,
    genderFlags: GenderFlags.Either,
    clan: Hero.MainHero.Clan,
    randomFactor: 0.6f
);

playerParty.AddCompanionsToParty(newCompanions);

// Companions are now in party roster
// Ready for battle, leveling, etc.
```

### Use Case 5: Spawn Enemy Lord with Army

```csharp
Clan enemyClan = /* get enemy clan */;

// Create enemy lord with large party
Hero enemyCommander = HeroGenerator.CreateLord(
    "Enemy Commander",
    enemyClan.Culture.ToCultureFlag(),
    GenderFlags.Male,
    enemyClan,
    withParty: true,
    randomFactor: 0.5f
);

// Add extra troops to party
if (enemyCommander.PartyBelongedTo != null)
{
    // Add 50 of each troop type
    enemyCommander.PartyBelongedTo.AddBasicTroops(50);
    enemyCommander.PartyBelongedTo.AddEliteTroops(50);
    enemyCommander.PartyBelongedTo.AddMercenaryTroops(50);
}

// Enemy lord now patrols with strong army
```

---

## Best Practices

### DO: Use Specific Methods

```csharp
// ✅ Clear intent
var lord = HeroGenerator.CreateLord(name, culture, gender, clan, withParty: true);
var wanderer = HeroGenerator.CreateWanderer(name, culture, gender, settlement);
var companions = HeroGenerator.CreateCompanions(count, culture, clan: clan);

// ❌ Unclear intent (legacy)
var hero = HeroGenerator.CreateHeroesFromRandomTemplates(
    1, culture, occupation: Occupation.Lord)[0];
```

### DO: Clean State Before Reinitializing

```csharp
// ✅ Proper cleanup
HeroGenerator.CleanupHeroState(hero);
hero.Clan = newClan;
HeroGenerator.InitializeAsLord(hero, settlement);

// ❌ State conflicts
hero.Clan = newClan; // Old party still references old clan!
```

### DO: Use Appropriate Companion Creation

```csharp
// ✅ For party members
var companions = HeroGenerator.CreateCompanions(3, culture, clan: clan);
party.AddCompanionsToParty(companions);

// ❌ Creates settlement state conflicts (old way)
var companions = HeroGenerator.CreateHeroesFromRandomTemplates(
    3, culture, clan: clan, occupation: Occupation.Wanderer);
```

### DO: Specify Party Creation Explicitly

```csharp
// ✅ Explicit control
var familyMember = HeroGenerator.CreateLord(name, culture, gender, clan, withParty: false);
var partyLeader = HeroGenerator.CreateLord(name, culture, gender, clan, withParty: true);

// ❌ Unclear (legacy - party creation depends on clan party count)
var hero = HeroGenerator.CreateHeroesFromRandomTemplates(...);
```

### DON'T: Mix Creation Methods

```csharp
// ❌ Inconsistent approach
var lord1 = HeroGenerator.CreateLord(...);  // New way
var lord2 = HeroGenerator.CreateHeroesFromRandomTemplates(...)[0];  // Old way

// ✅ Consistent approach
var lord1 = HeroGenerator.CreateLord(...);
var lord2 = HeroGenerator.CreateLord(...);
```

### DON'T: Create Wanderers for Party Members

```csharp
// ❌ Wrong - wanderers are for settlement recruitment
var companions = HeroGenerator.CreateWanderers(3, culture, settlement);
party.AddCompanionsToParty(companions);  // Will cause issues!

// ✅ Correct - use CreateCompanions for party members
var companions = HeroGenerator.CreateCompanions(3, culture, clan: clan);
party.AddCompanionsToParty(companions);
```

### DON'T: Forget to Assign Clan

```csharp
// ❌ Lord without clan will crash
var hero = /* created somehow without clan */;
HeroGenerator.InitializeAsLord(hero, settlement);  // Throws exception!

// ✅ Ensure clan is assigned
var hero = /* created somehow */;
hero.Clan = myClan;
HeroGenerator.InitializeAsLord(hero, settlement);
```

---

## Troubleshooting

### Problem: Crash After Creating Many Clans

**Symptoms:**
- Game runs fine initially
- After some game time, crashes occur
- More clans = faster crashes

**Cause:**
- Using legacy methods that create party/clan mismatches
- Companions created as wanderers with settlement state

**Solution:**
```csharp
// Use new methods
List<Clan> clans = ClanGenerator.GenerateClans(
    count, cultureFlags, kingdom, 
    createParties: true,  // Explicit
    companionCount: 2     // Explicit
);
```

### Problem: Heroes Have No Party When Expected

**Symptoms:**
- Lord created but `PartyBelongedTo` is null
- Expected party but it doesn't exist

**Cause:**
- `withParty: false` was specified
- Clan already has 6+ parties (legacy behavior)

**Solution:**
```csharp
// Explicitly create with party
var lord = HeroGenerator.CreateLord(
    name, culture, gender, clan, 
    withParty: true,  // Explicit true
    randomFactor: 0.5f
);

// Or manually create party
if (lord.PartyBelongedTo == null)
{
    Settlement settlement = lord.GetHomeOrAlternativeSettlement();
    lord.CreateParty(settlement);
}
```

### Problem: Companions Not in Party

**Symptoms:**
- Created companions but they're not in party roster
- Companions appear in settlements instead

**Cause:**
- Used `CreateWanderers()` instead of `CreateCompanions()`
- Forgot to call `AddCompanionsToParty()`

**Solution:**
```csharp
// Create companions (not wanderers)
var companions = HeroGenerator.CreateCompanions(3, culture, clan: clan);

// Add to party
party.AddCompanionsToParty(companions);
```

### Problem: Null Reference Exception

**Symptoms:**
- `NullReferenceException` when calling `InitializeAsLord()`
- Error: "Hero must have a clan assigned before initializing as Lord"

**Cause:**
- Hero.Clan is null
- Trying to initialize lord without clan

**Solution:**
```csharp
// Assign clan before initializing as lord
hero.Clan = myClan;
HeroGenerator.InitializeAsLord(hero, settlement, createParty: true);

// Or use convenience method that handles clan assignment
var hero = HeroGenerator.CreateLord(name, culture, gender, myClan, withParty: true);
```

### Problem: Hero String IDs Have Spaces

**Symptoms:**
- Hero created with name "John Doe"
- String ID is "hero_john doe_guid" (has space)

**Cause:**
- Old `ObjectManager.CleanString()` bug (now fixed)

**Solution:**
- Update to latest code (fix applied in ObjectManager.cs)
- New heroes will have proper IDs: "hero_john_doe_guid"

---

## API Reference

### HeroGenerator Methods

#### High-Level Methods (Recommended)

| Method | Purpose | Returns |
|--------|---------|---------|
| `CreateLord()` | Create single lord | Hero |
| `CreateLords()` | Create multiple lords | List\<Hero\> |
| `CreateWanderer()` | Create single wanderer | Hero |
| `CreateWanderers()` | Create multiple wanderers | List\<Hero\> |
| `CreateCompanions()` | Create companions for party | List\<Hero\> |

#### Initialization Methods (Advanced)

| Method | Purpose | Parameters |
|--------|---------|------------|
| `InitializeAsLord()` | Initialize hero as lord | hero, settlement, createParty |
| `InitializeAsWanderer()` | Initialize hero as wanderer | hero, settlement |
| `InitializeAsCompanion()` | Initialize hero as companion | hero |
| `CleanupHeroState()` | Remove party/settlement state | hero |

#### Legacy Methods (Backward Compatibility)

| Method | Status | Note |
|--------|--------|------|
| `CreateSingleHeroFromRandomTemplates()` | Deprecated | Use `CreateLord()` instead |
| `CreateRandomWandererAtSettlement()` | Deprecated | Use `CreateWanderer()` instead |
| `CreateHeroesFromRandomTemplates()` | Legacy | Works but has hidden side effects |

### ClanGenerator Methods

| Method | Purpose | Returns |
|--------|---------|---------|
| `CreateClan()` | Create single clan | Clan |
| `GenerateClans()` | Generate multiple clans | List\<Clan\> |
| `CreateMinorClan()` | Create minor faction clan | Clan |

---

## Console Commands Reference

### Hero Commands

```bash
# Create lord
gm.hero.create_lord <name> [cultures] [gender] [clan] [randomFactor]

# Generate lords
gm.hero.generate_lords <count> [cultures] [gender] [clan] [randomFactor]

# Create wanderer
gm.hero.create_wanderer <name> <settlement> [cultures] [gender] [randomFactor]

# Create companions
gm.hero.create_companions <count> <party> [cultures] [gender] [randomFactor]

# Rename hero
gm.hero.rename <heroQuery> <name>
```

### Clan Commands

```bash
# Create clan
gm.clan.create_clan <clanName> [leaderHero] [kingdom] [createParty] [companionCount]

# Generate clans
gm.clan.generate_clans <count> [cultures] [kingdom] [createParties] [companionCount]

# Create minor clan
gm.clan.create_minor_clan <clanName> [leaderHero] [cultures] [createParty]
```

---

## Related Documentation

- [Best Practices Guide](../guides/best-practices.md) - Coding standards
- [Testing Guide](../guides/testing.md) - How to test hero/clan creation
- [Architecture Overview](../getting-started/architecture-overview.md) - System design

---

**Navigation:** [← Back: Code Quality Checklist](code-quality-checklist.md) | [Back to Index](../README.md)
