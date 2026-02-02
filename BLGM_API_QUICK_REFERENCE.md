# Bannerlord.GameMaster - API Quick Reference (Out of date by a few versions)

Complete API documentation for the BLGM framework. This reference covers all major systems, extensions, and utilities for interacting with and managing game state in Mount & Blade II: Bannerlord.

**Note:** This is a developer-focused reference for API usage
  

If you are looking for the full console command documentation for **Users** instead use the following link:  
[BLGM User Documentation](https://github.com/SolWayward/Bannerlord.GameMaster/wiki)


---

## 1. CORE UTILITIES & INFRASTRUCTURE

### RandomNumberGen
Singleton wrapper for System.Random providing centralized random number generation.

**Location:** [`RandomNumberGen.cs`](Bannerlord.GameMaster/RandomNumberGen.cs)

**Key Properties & Methods:**
- `Instance` (static) - Singleton instance
- `NextRandomInt()` - Random int
- `NextRandomInt(int max)` - Random int [0, max)
- `NextRandomInt(int min, int max)` - Random int [min, max)
- `NextRandomFloat()` - Random float [0.0f, 1.0f)
- `NextRandomDouble()` - Random double [0.0, 1.0)
- `NextRandomBytes(byte[] buffer)` - Fill buffer with random bytes
- `NextRandomRGBColor` (property) - Random RGB color with full opacity

### BLGMResult
Struct for returning success/failure with optional messages and exception details. Supports method chaining for Display() and Log().

**Location:** [`BLGMResult.cs`](Bannerlord.GameMaster/Common/BLGMResult.cs)

**Key Properties:**
- `wasSuccessful` (bool) - Operation success state
- `message` (string) - Detail message
- `exception` (Exception) - Optional exception (null if no error)

**Key Methods:**
- `DisplayMessage()` - Shows in-game message (green if success, red if failure)
- `Log()` - Writes to RGL log file (includes stack trace if exception present)
- `DisplayAndLog()` - Both display and log simultaneously

**Usage Example:**
```csharp
return new BLGMResult(true, "Clan created successfully").DisplayAndLog();
return new BLGMResult(false, "Failed to create clan", ex).Log();
```

### GameEnvironment
Static utilities for detecting game environment and version information.

**Location:** [`GameEnvironment.cs`](Bannerlord.GameMaster/Information/GameEnvironment.cs)

**Key Properties:**
- `IsWarsailsDlcLoaded` (bool) - Warsails DLC detection
- `BannerlordVersion` (string) - Current game version
- `BLGMVersion` (string) - Current BLGM mod version
- `LoadedModules` (string[]) - Array of active mod IDs

### InfoMessage
Wrapper around InformationManager with pre-configured colors for different message types.

**Location:** [`InfoMessage.cs`](Bannerlord.GameMaster/Information/InfoMessage.cs)

**Static Methods:**
- `Log(string message)` - White message
- `Success(string message)` - Green message
- `Warning(string message)` - Yellow message
- `Error(string message)` - Red message
- `Important(string message)` - Magenta message
- `Status(string message)` - Cyan message
- `Status2(string message)` - Blue message
- `Write(string message, Color color)` - Custom color message

### SystemConsoleCommands
**Location:** [`SystemConsoleCommands.cs`](Bannerlord.GameMaster/SystemConsoleCommands.cs)

Command handlers and registration for system-level console commands. Manages command execution and routing for system console functionality.

### SystemConsoleHelper
**Location:** [`SystemConsoleHelper.cs`](Bannerlord.GameMaster/SystemConsoleHelper.cs)

Helper utilities and common operations for system console functionality. Provides shared utilities for console command processing and system interactions.

### SystemConsoleManager
**Location:** [`SystemConsoleManager.cs`](Bannerlord.GameMaster/SystemConsoleManager.cs)

Core manager for system console operations. Handles initialization, state management, and coordination of system console features.

### Interfaces: IEntityExtensions & IEntityQueries

**Location:** [`IEntityExtensions.cs`](Bannerlord.GameMaster/Common/Interfaces/IEntityExtensions.cs), [`IEntityQueries.cs`](Bannerlord.GameMaster/Common/Interfaces/IEntityQueries.cs)

**IEntityExtensions<TEntity, TTypes>:**
```csharp
TTypes GetTypes(TEntity entity);
bool HasAllTypes(TEntity entity, TTypes types);
bool HasAnyType(TEntity entity, TTypes types);
string FormattedDetails(TEntity entity);
```

**IEntityQueries<TEntity, TTypes>:**
```csharp
TEntity GetById(string id);
List<TEntity> Query(string query, TTypes types, bool matchAll);
TTypes ParseType(string typeString);
TTypes ParseTypes(IEnumerable<string> typeStrings);
string GetFormattedDetails(List<TEntity> entities);
```

---

## 2. CHARACTER & BODY CUSTOMIZATION

### Style Constants

**Location:** 
- [`BeardTags.cs`](Bannerlord.GameMaster/Characters/BeardTags.cs) - Beard style filters
- [`HairCoveringTypes.cs`](Bannerlord.GameMaster/Characters/HairCoveringTypes.cs) - Hair covering levels (0-5)
- HairTags.cs - Hair style constants
- TattooTags.cs - Tattoo style constants

### CharacterTemplatePooler

**Location:** [`CharacterTemplates.cs`](Bannerlord.GameMaster/Characters/CharacterTemplates.cs)

**Key Properties:**
- `AllTemplates` - All character templates
- `MainFactionTemplates` - Main culture templates only
- `BanditTemplates` - Bandit culture templates
- Cached gender variants of all above

**Key Methods:**
- `GetCulturalTemplates(CultureObject culture)` - Get templates for specific culture
- `GetLordAndWandererCharacters(CultureObject culture)` - Combat-eligible characters (safe for hero creation)
- `GetGenderCulturalTemplates(CultureObject culture, bool isFemale)` - Gender-specific templates
- `GetAllHeroTemplatesFromFlags(CultureFlags cultureFlags, GenderFlags genderFlags)` - Flexible multi-culture lookup

### StaticBodyPropertiesHelper

**Location:** [`StaticBodyPropertiesHelper.cs`](Bannerlord.GameMaster/Heroes/StaticBodyPropertiesHelper.cs)

**Bit Manipulation Methods:**
- `GetBitsValueFromKey(ulong key, int startBit, int numBits)` - Extract bit field
- `SetBits(ulong key, int startBit, int numBits, int newValue)` - Set bit field with validation

**Height Operations (0-1 range):**
- `GetHeight(StaticBodyProperties props)` - Extract height value
- `SetHeight(StaticBodyProperties props, float height)` - Create new props with modified height

### BodyConstraints

**Location:** [`BodyConstraints.cs`](Bannerlord.GameMaster/Heroes/BodyConstraints.cs)

**Structure:**
- `Height` (Constraint) - Min/max height (0-1)
- `Weight` (Constraint) - Min/max weight (0-1)
- `Build` (Constraint) - Min/max muscle/tone (0-1)

**Static Presets:**
- `GenderConstraints(bool isFemale)` - Gender-optimized constraints
- `FemaleConstraints` - Height: 0.3-0.8, Weight: 0-0.5, Build: 0-0.5
- `MaleConstraints` - Height: 0.5-1.0, Weight: 0-1.0, Build: 0-1.0

### HeroBodyEditor

**Location:** [`HeroBodyEditor.cs`](Bannerlord.GameMaster/Heroes/HeroBodyEditor.cs)

**Key Properties:**
- `Hero` - Target hero
- `BodyEditor` (sub-editor) - Height, Weight, Build modifications
- `IsDirty` - Whether changes have been made

**Properties (constrained by BodyConstraints):**
- `Height` (0-1) - Get/set with automatic constraint application
- `Weight` (0-1) - Get/set with automatic constraint application
- `Build` (0-1) - Get/set with automatic constraint application

**Methods:**
- `SetProperty(string name, float value)` - Set by property name (height, weight, build/muscle)
- `GetProperty(string name)` - Get by property name
- `Reset()` - Revert to original state
- `ApplyConstraints()` - Re-apply current constraints
- `ApplyRandomizedProperties(BodyProperties randomProps)` - Apply randomized body with constraint enforcement
- `RandomizeAppearance(float randomFactor)` - Randomize entire appearance with constraints

### HeroEditor

**Location:** [`HeroEditor.cs`](Bannerlord.GameMaster/Heroes/HeroEditor.cs)

**Key Properties:**
- `Hero` - Target hero
- `BodyEditor` - Sub-editor for body properties
- `IsDirty` - Whether any sub-editor has been modified

**Methods:**
- `RandomizeAppearance(float randomFactor)` - Randomize with constraint enforcement
- `Reset()` - Reset all sub-editors

---

## 3. CHARACTERS: HEROES, CLANS & KINGDOMS

### Character Queries & Finding

#### CharacterFinder
**Location:** [`Characters/`](Bannerlord.GameMaster/Characters/) - Character finding utilities

Utilities for finding and locating CharacterObjects by various criteria and filters.

#### CharacterQueries
**Location:** [`CharacterQueries.cs`](Bannerlord.GameMaster/Characters/CharacterQueries.cs)

Queries all CharacterObjects including heroes, troops, NPCs, templates, and children. Provides comprehensive filtering and search capabilities across all character types in the game.

**Key Methods:**
- `GetCharacterById(string characterId)` - Lookup by StringId
- `QueryCharacterObjects(string query, CharacterTypes types, bool matchAll)` - Unified query for all characters
- `ParseCharacterType(string typeString)` - Parse single type
- `ParseCharacterTypes(IEnumerable<string> typeStrings)` - Parse multiple types
- `GetFormattedDetails(List<CharacterObject> characters)` - Formatted output

---

### Heroes

**Location:** [`HeroExtensions.cs`](Bannerlord.GameMaster/Heroes/HeroExtensions.cs), [`HeroManager.cs`](Bannerlord.GameMaster/Heroes/HeroManager.cs), [`HeroQueries.cs`](Bannerlord.GameMaster/Heroes/HeroQueries.cs)

#### HeroTypes Enum
Flag-based type categorization with 19 flags: `None`, `IsArtisan`, `Lord`, `Wanderer`, `Notable`, `Merchant`, `Children`, `Female`, `Male`, `ClanLeader`, `KingdomRuler`, `PartyLeader`, `Fugitive`, `Alive`, `Dead`, `Prisoner`, `WithoutClan`, `WithoutKingdom`, `Married`

#### HeroExtensions
**Key Methods:**
- `GetHeroTypes(Hero hero)` - Get all type flags
- `HasAllTypes(Hero hero, HeroTypes types)` - AND logic check
- `HasAnyType(Hero hero, HeroTypes types)` - OR logic check
- `CreateParty(Hero hero, Settlement spawnSettlement)` - Create party with initial troops
- `GetHomeOrAlternativeSettlement(Hero hero)` - Get home or nearest alternative
- `InitializeHomeSettlement(Hero hero, Settlement settlement)` - Set home settlement
- `EquipHeroBasedOnCulture(Hero hero)` - Equip from elite culture troops
- `EquipLordBasedOnCulture(Hero hero)` - Equip from lords/tier 5+ troops (same gender/culture)
- `SetStringName(Hero hero, string name)` - Set name from string
- `SetAge(Hero hero, int age)` - Set age by birthdate
- `SetRandomDeathDate(Hero hero)` - Set random death (age 55-92)
- `FormattedDetails(Hero hero)` - Formatted details string

#### HeroManager
**Static Methods:**
- `GetBestInitialSettlement(Hero hero)` - Intelligent settlement selection (clan > kingdom > random)
- `TrySetHomeSettlement(Hero hero, Settlement settlement)` - Reflection-based home settlement setter

#### HeroQueries
**Static Methods:**
- `GetHeroById(string heroId)` - Lookup by StringId (case-insensitive)
- `QueryHeroes(string query, HeroTypes types, bool matchAll, bool includeDead, string sortBy, bool sortDescending)` - Unified query with multiple filter/sort options
- `ParseHeroType(string typeString)` - Parse single type flag
- `ParseHeroTypes(IEnumerable<string> typeStrings)` - Parse multiple flags
- `GetFormattedDetails(List<Hero> heroes)` - Formatted column output

---

### Clans

**Location:** [`ClanExtensions.cs`](Bannerlord.GameMaster/Clans/ClanExtensions.cs), [`ClanGenerator.cs`](Bannerlord.GameMaster/Clans/ClanGenerator.cs), [`ClanQueries.cs`](Bannerlord.GameMaster/Clans/ClanQueries.cs)

#### ClanTypes Enum
Flag-based: `None`, `Active`, `Eliminated`, `Bandit`, `NonBandit`, `MapFaction`, `Noble`, `MinorFaction`, `Rebel`, `Mercenary`, `UnderMercenaryService`, `Mafia`, `Outlaw`, `Nomad`, `Sect`, `WithoutKingdom`, `Empty`, `PlayerClan`

#### ClanExtensions
**Key Methods:**
- `GetClanTypes(Clan clan)` - Get all type flags
- `HasAllTypes(Clan clan, ClanTypes types)` - AND check
- `HasAnyType(Clan clan, ClanTypes types)` - OR check
- `SetClanTier(Clan clan, int targetTier)` - Set tier (0-6) with automatic renown adjustment
- `SetStringName(Clan clan, string name)` - Rename clan
- `FormattedDetails(Clan clan)` - Formatted details

#### ClanGenerator
**Static Methods:**
- `CreateNobleClan(string name, Hero leader, Kingdom kingdom, bool createParty, int companionCount, CultureFlags cultureFlags)` - Create noble clan (tier 3-5)
- `CreateMinorClan(string name, Hero leader, CultureFlags cultureFlags, bool createParty, int companionCount)` - Create minor faction clan (tier 1-3)
- `GenerateClans(int count, CultureFlags cultureFlags, Kingdom kingdom, bool createParties, int companionCount)` - Batch clan creation

#### ClanQueries
**Static Methods:**
- `GetClanById(string clanId)` - Lookup by StringId
- `QueryClans(string query, ClanTypes types, bool matchAll, string sortBy, bool sortDescending)` - Unified query
- `ParseClanType(string typeString)` - Parse single type
- `ParseClanTypes(IEnumerable<string> typeStrings)` - Parse multiple types
- `GetFormattedDetails(List<Clan> clans)` - Formatted output
- `GetPartyLeaders(Clan clan)` - Get all party leaders in clan

---

### Kingdoms

**Location:** [`KingdomExtensions.cs`](Bannerlord.GameMaster/Kingdoms/KingdomExtensions.cs), [`KingdomGenerator.cs`](Bannerlord.GameMaster/Kingdoms/KingdomGenerator.cs), [`KingdomQueries.cs`](Bannerlord.GameMaster/Kingdoms/KingdomQueries.cs)

#### KingdomTypes Enum
Flag-based: `None`, `Active`, `Eliminated`, `Empty`, `PlayerKingdom`, `AtWar`

#### KingdomExtensions
**Key Methods:**
- `GetKingdomTypes(Kingdom kingdom)` - Get all type flags
- `HasAllTypes(Kingdom kingdom, KingdomTypes types)` - AND check
- `HasAnyType(Kingdom kingdom, KingdomTypes types)` - OR check
- `FormattedDetails(Kingdom kingdom)` - Formatted details

#### KingdomGenerator
**Static Methods:**
- `CreateKingdom(Settlement homeSettlement, int vassalClanCount, string name, string rulingClanName, CultureFlags cultureFlags)` - Create kingdom with ruling clan and vassals
- `GenerateKingdoms(int count, int vassalClanCount, CultureFlags cultureFlags)` - Batch kingdom creation from existing settlements

#### KingdomQueries
**Static Methods:**
- `GetKingdomById(string kingdomId)` - Lookup by StringId
- `QueryKingdoms(string query, KingdomTypes types, bool matchAll, string sortBy, bool sortDescending)` - Unified query
- `ParseKingdomType(string typeString)` - Parse single type
- `ParseKingdomTypes(IEnumerable<string> typeStrings)` - Parse multiple types
- `GetFormattedDetails(List<Kingdom> kingdoms)` - Formatted output
- `GetClanLeaders(Kingdom kingdom)` - Get all clan leaders
- `GetPartyLeaders(Kingdom kingdom)` - Get all party leaders
- `GetHeroes(Kingdom kingdom)` - Get all heroes

#### Kingdom Diplomacy Extensions

**KingdomAllianceExtensions** (`KingdomAllianceExtensions.cs`):
- `DeclareAlliance(Kingdom proposing, Kingdom receiving, bool callToWar)` - Form alliance
- `ProposeCallAllyToWar(Kingdom proposer, Kingdom ally, Kingdom enemy)` - Request ally to declare war
- `ProposeCallAllyToWarForceAccept(Kingdom proposer, Kingdom ally, Kingdom enemy)` - Force ally to declare war
- `AcceptCallAllyToWar(Kingdom proposer, Kingdom ally, Kingdom enemy)` - Accept call to war

**KingdomTradeAgreementExtensions** (`KingdomTradeAgreementExtensions.cs`):
- `MakeTradeAgreement(Kingdom proposing, Kingdom receiving)` - Establish trade agreement

**KingdomTributeExtensions** (`KingdomTributeExtensions.cs`):
- `PayTribute(Kingdom kingdom, Kingdom otherKingdom, int dailyAmount, int days)` - Pay tribute (returns TributeInfo)
- `GetTributeInfo(Kingdom kingdom, Kingdom otherKingdom)` - Get current tribute details

**TributeInfo Struct:**
- `GetTributeString()` - Formatted tribute details

---

## 4. CULTURES & NAMING

### CultureLookup
Central repository for culture queries and name generation.

**Location:** [`CultureLookup.cs`](Bannerlord.GameMaster/Cultures/CultureLookup.cs)

**Main Culture Properties:**
- `Aserai`, `Battania`, `Empire`, `Khuzait`, `Sturgia`, `Vlandia` - Main cultures
- `Nord` - Returns Nord if Warsails DLC active, otherwise Sturgia
- `CalradianNeutral` - Neutral culture

**Bandit Culture Properties:**
- `Deserters`, `ForestBandits`, `Looters`, `MountainBandits`, `SeaRaiders`, `SteppeBandits`
- `Corsairs` - Returns Corsairs if Warsails DLC active, otherwise SeaRaiders

**Special Cultures:**
- `DarshiSpecial`, `VakkenSpecial`

**Collections:**
- `AllCultures` - All cultures
- `MainCultures` - Main factions only
- `BanditCultures` - Bandit factions only
- `RandomMainCulture()` - Random main culture

**Name Generation:**
- `GetUniqueRandomHeroName(CultureObject culture, bool isFemale)` - Unique hero name generation
- `GetUniqueRandomClanName(CultureObject culture)` - Unique clan name generation
- `GetUniqueRandomKingdomName(CultureObject culture)` - Unique kingdom name generation

**Utilities:**
- `GetCultureFlag(CultureObject culture)` - Convert to CultureFlags enum

### CultureExtensions
**Location:** [`CultureExtensions.cs`](Bannerlord.GameMaster/Cultures/CultureExtensions.cs)

**Key Methods:**
- `ToCultureFlag(this CultureObject culture)` - Convert to CultureFlags

### CultureFlags Enum
**Location:** [`CultureFlags.cs`](Bannerlord.GameMaster/Cultures/CultureFlags.cs)

Main cultures (bit 0-7), Bandit cultures (bit 8-14), Special cultures (bit 15-16)
- `AllMainCultures` - Combination flag
- `AllBanditCultures` - Combination flag
- `AllCultures` - All flags combined

### Name Generation Files
**Location:** Cultures/HeroNames/ and Cultures/FactionNames/ directories
- Culture-specific name arrays (Aserai, Battania, Empire, Khuzait, Nord, Sturgia, Vlandia)
- Male/Female hero names
- Clan and Kingdom name arrays

---

## 5. ITEMS & EQUIPMENT

### ItemExtensions
**Location:** [`ItemExtensions.cs`](Bannerlord.GameMaster/Items/ItemExtensions.cs)

#### ItemTypes Enum
25+ flags: `None`, `Weapon`, `Armor`, `Mount`, `Food`, `Trade`, `OneHanded`, `TwoHanded`, `Ranged`, `Shield`, `HeadArmor`, `BodyArmor`, `LegArmor`, `HandArmor`, `Cape`, `Thrown`, `Arrows`, `Bolts`, `Polearm`, `Banner`, `Goods`, `Bow`, `Crossbow`, `Civilian`, `Combat`, `HorseArmor`

**Key Methods:**
- `GetItemTypes(ItemObject item)` - Get all type flags
- `HasAllTypes(ItemObject item, ItemTypes types)` - AND check
- `HasAnyType(ItemObject item, ItemTypes types)` - OR check
- `FormattedDetails(ItemObject item)` - Formatted details
- `GetTypes(ItemObject item)` - Alias for GetItemTypes

### ItemModifierHelper
**Location:** [`ItemModifierHelper.cs`](Bannerlord.GameMaster/Items/ItemModifierHelper.cs)

**Key Methods:**
- `GetAllModifiers()` - Get all item modifiers
- `GetModifierByName(string modifierName)` - Lookup with case-insensitive partial match
- `GetFormattedModifierList()` - Formatted list of all modifiers
- `CanHaveModifier(ItemObject item)` - Check if item supports modifiers
- `GetModifierInfo(ItemModifier modifier)` - Get modifier details
- `ParseModifier(string modifierName)` - Parse with error suggestions

**CommonModifiers Class:**
Pre-defined property accessors: `Fine`, `Masterwork`, `Legendary`, `Bent`, `Chipped`, `Rusty`, `Cracked`, `Balanced`, `Sharp`, `Heavy`

### ItemQueries
**Location:** [`ItemQueries.cs`](Bannerlord.GameMaster/Items/ItemQueries.cs)

**Key Methods:**
- `GetItemById(string itemId)` - Lookup by StringId
- `QueryItems(string query, ItemTypes types, bool matchAll, int tierFilter, string sortBy, bool sortDescending)` - Unified query with tier filtering
- `ParseItemType(string typeString)` - Parse single type (supports aliases: 1h, 2h, head, body, leg, hand)
- `ParseItemTypes(IEnumerable<string> typeStrings)` - Parse multiple types
- `GetFormattedDetails(List<ItemObject> items)` - Formatted output

### EquipmentFileManager
**Location:** [`EquipmentFileManager.cs`](Bannerlord.GameMaster/Items/EquipmentFileManager.cs)

**File Operations:**
- `GetEquipmentFilePath(string filename, bool isCivilian)` - Get full path
- `GetEquipmentDirectory(bool isCivilian)` - Get directory path
- `EquipmentFileExists(string filename, bool isCivilian)` - Check existence
- `ListEquipmentFiles(bool isCivilian)` - List all equipment files

**Save Operations:**
- `SaveEquipmentToFile(Hero hero, Equipment equipment, string filepath, bool isCivilian)` - Save single set to JSON
- `SaveBothEquipmentSets(Hero hero, string filename, Equipment battle, Equipment civilian)` - Save both sets

**Load Operations:**
- `LoadEquipmentData(string filepath)` - Load raw equipment data
- `LoadEquipmentFromFile(Hero hero, string filepath, bool isCivilian)` - Load and apply to hero (returns load/skip counts)

---

## 6. TROOPS & MILITARY

### TroopExtensions
**Location:** [`TroopExtensions.cs`](Bannerlord.GameMaster/Troops/TroopExtensions.cs)

#### TroopTypes Enum (long flags, 35+ flags)
- Formation/Combat Roles: `Infantry`, `Ranged`, `Cavalry`, `HorseArcher`, `Mounted`
- Troop Lines: `Regular`, `Noble`, `Militia`, `Mercenary`, `Caravan`, `Peasant`, `MinorFaction`
- Equipment-Based: `Shield`, `TwoHanded`, `Polearm`, `Bow`, `Crossbow`, `ThrowingWeapon`
- Tier-Based: `Tier0` through `Tier6Plus`
- Cultures: `Empire`, `Vlandia`, `Sturgia`, `Aserai`, `Khuzait`, `Battania`, `Nord`, `Bandit`
- Gender: `Female`, `Male`

#### Key Methods:
- `GetTroopTypes(CharacterObject character)` - Get all type flags (CRITICAL: Returns None for heroes)
- `HasAllTypes(CharacterObject character, TroopTypes types)` - AND check
- `HasAnyType(CharacterObject character, TroopTypes types)` - OR check
- `IsActualTroop(CharacterObject character)` - Filters out heroes, NPCs, templates, children
- `GetTroopCategory(CharacterObject character)` - Primary category string
- `FormattedDetails(CharacterObject character)` - Formatted details

**Equipment Checks:**
- `HasShield(CharacterObject character)` - Has shield
- `HasWeaponType(CharacterObject character, ItemObject.ItemTypeEnum type)` - Has specific weapon type
- `HasWeaponClass(CharacterObject character, WeaponClass class)` - Has weapon class
- `HasTwoHandedWeapon(CharacterObject character)` - Has 2H weapon
- `HasPolearm(CharacterObject character)` - Has polearm
- `IsMounted(CharacterObject character)` - Cavalry or HorseArcher

### TroopQueries
**Location:** [`TroopQueries.cs`](Bannerlord.GameMaster/Troops/TroopQueries.cs)

**Key Methods (TROOPS ONLY):**
- `GetTroopById(string troopId)` - Lookup by StringId
- `QueryTroops(string query, TroopTypes types, bool matchAll, int tierFilter, string sortBy, bool sortDescending)` - FILTERED to actual combat troops only
- `ParseTroopType(string typeString)` - Parse single type (aliases: 2h, mounted, cav, ha)
- `ParseTroopTypes(IEnumerable<string> typeStrings)` - Parse multiple types
- `GetFormattedDetails(List<CharacterObject> troops)` - Formatted output

**Note:** All CharacterObject querying (including NPCs, templates, children) has been moved to [`CharacterQueries`](Bannerlord.GameMaster/Characters/CharacterQueries.cs). This class now exclusively handles actual combat troops.

### TroopUpgrader
**Location:** [`TroopUpgrader.cs`](Bannerlord.GameMaster/Troops/TroopUpgrader.cs)

**Sophisticated upgrade system with composition ratio management:**

**Main Method:**
- `UpgradeTroops(TroopRoster memberRoster, int targetTier, float? targetRangedRatio, float? targetCavalryRatio, float? targetInfantryRatio)`
  - Upgrades troops while maintaining desired combat composition
  - Intelligently handles multi-path upgrades
  - Pre-analyzes all upgrade paths before execution

**Helper Methods:**
- `NormalizeRatios(float? ranged, float? cavalry, float? infantry)` - Validates and auto-fills unspecified ratios
- `AnalyzeUpgradePaths(TroopRoster roster, int targetTier)` - Pre-compute upgrade possibilities
- `CalculateAdjustedRatios(...)` - Account for locked-in troops with limited options
- `SplitTroopsWithAdjustedRatios(...)` - Smart troop splitting based on desirability scores

**Example Usage:**
```csharp
// Upgrade to tier 5, maintaining 30% ranged, 20% cavalry, 50% infantry
TroopUpgrader.UpgradeTroops(party.MemberRoster, targetTier: 5, 
    targetRangedRatio: 0.30f, 
    targetCavalryRatio: 0.20f, 
    targetInfantryRatio: 0.50f);
```

---

## 7. SETTLEMENTS & VILLAGES

### Clan Settlement Extensions
**Location:** [`Settlements/`](Bannerlord.GameMaster/Settlements/) - Clan settlement management

Extensions for managing settlement-related operations at the clan level, including ownership and clan-specific settlement interactions.

### KingdomSettlementExtensions
**Location:** [`Settlements/`](Bannerlord.GameMaster/Settlements/) - Kingdom settlement management

Extensions for managing settlement-related operations at the kingdom level, including kingdom-wide settlement management and properties.

### SettlementExtensions
**Location:** [`SettlementExtensions.cs`](Bannerlord.GameMaster/Settlements/SettlementExtensions.cs)

#### SettlementTypes Enum (long flags)
Core types: `Settlement`, `Castle`, `City`, `Village`, `Hideout`, `PlayerOwned`, `Besieged`, `Raided`
Culture flags and prosperity levels also included

**Key Methods:**
- `GetSettlementTypes(Settlement settlement)` - Get all type flags
- `HasAllTypes(Settlement settlement, SettlementTypes types)` - AND check
- `HasAnyType(Settlement settlement, SettlementTypes types)` - OR check
- `Rename(Settlement settlement, string newName)` - Rename settlement
- `ResetName(Settlement settlement)` - Reset to original name
- `GetOriginalName(Settlement settlement)` - Get original name
- `IsRenamed(Settlement settlement)` - Check if renamed
- `FormattedDetails(Settlement settlement)` - Formatted details

### SettlementManager
**Location:** [`SettlementManager.cs`](Bannerlord.GameMaster/Settlements/SettlementManager.cs)

**Key Methods:**
- `GetRandomSettlement()` / `GetRandomTown()` / `GetRandomCastle()` / `GetRandomVillage()`
- Overloads: by clan, by kingdom
- `ChangeSettlementOwner(Settlement settlement, Hero newOwner)` - Transfer ownership
- `SetSettlementCulture(Settlement settlement, CultureObject culture)` - Change culture
- `GetBoundVillagesCount(Settlement settlement)` - Get village count
- `RenameSettlement(Settlement settlement, string newName)` - Rename with tracking
- `ResetSettlementName(Settlement settlement)` - Reset with tracking
- `ResetAllSettlementNames()` - Batch reset
- `GetOriginalSettlementName(Settlement settlement)` - Lookup original
- `IsSettlementRenamed(Settlement settlement)` - Check renamed status
- `GetRenamedSettlementCount()` - Total renamed count

### VillageExtensions & VillageManager
**Location:** [`VillageExtensions.cs`](Bannerlord.GameMaster/Settlements/VillageExtensions.cs), [`VillageManager.cs`](Bannerlord.GameMaster/Settlements/VillageManager.cs)

**Key Methods (VillageExtensions):**
- `SetBoundSettlement(Village village, Settlement settlement)` - Set primary settlement
- `SetTradeBoundSettlement(Village village, Settlement settlement)` - Set trade settlement
- `GetRecommendedTradeBound(Village village)` - Get recommended trade settlement

### SettlementQueries
**Location:** [`SettlementQueries.cs`](Bannerlord.GameMaster/Settlements/SettlementQueries.cs)

**Key Methods:**
- `GetSettlementById(string settlementId)` - Lookup by StringId
- `QuerySettlements(string query, SettlementTypes types, bool matchAll, string sortBy, bool sortDescending)` - Unified query
- `ParseSettlementType(string typeString)` - Parse single type
- `ParseSettlementTypes(IEnumerable<string> typeStrings)` - Parse multiple types
- `GetFormattedDetails(List<Settlement> settlements)` - Formatted output

### Settlement Save/Data Structures
**Location:** [`SettlementSaveDefiner.cs`](Bannerlord.GameMaster/Settlements/SettlementSaveDefiner.cs), [`SettlementNameData.cs`](Bannerlord.GameMaster/Settlements/SettlementNameData.cs)

---

## 8. PARTIES & MILITARY GROUPS

### MobilePartyExtensions
**Location:** [`MobilePartyExtensions.cs`](Bannerlord.GameMaster/Party/MobilePartyExtensions.cs)

**Companion Methods:**
- `AddCompanionToParty(MobileParty party, Hero hero)` - Add single companion
- `AddCompanionsToParty(MobileParty party, List<Hero> heroes)` - Add multiple companions

**Lord Methods:**
- `AddLordToParty(MobileParty party, Hero hero)` - Add single lord
- `AddLordsToParty(MobileParty party, List<Hero> heroes)` - Add multiple lords

**Troop Addition Methods:**
- `AddBasicTroops(MobileParty party, int count)` - Add tier 0-1 troops
- `AddEliteTroops(MobileParty party, int count)` - Add tier 4-6 troops
- `AddMercenaryTroops(MobileParty party, int count)` - Add mercenary troops
- `AddMixedTierTroops(MobileParty party, int countOfEach)` - Add mixed tier distribution

**Party Management:**
- `UpgradeTroops(MobileParty party, int upgradeCount, bool upgradeAllTroops)` - Upgrade troops with ratio management
- `AddXp(MobileParty party, int xp)` - Add party experience
- `Disband(MobileParty party)` - Initiate disbanding
- `CancelDisband(MobileParty party)` - Cancel disbanding
- `DestroyParty(MobileParty party)` - Immediate destruction

---

## 9. CARAVANS

### CaravanManager
**Location:** [`CaravanManager.cs`](Bannerlord.GameMaster/Caravans/CaravanManager.cs)

**Properties:**
- `AllCaravanParties` - All caravan parties
- `TotalCaravanCount` - Total count
- `TotalNonDisbandingCaravans` - Active caravans
- `TotalDisbandingCaravans` - Disbanding caravans
- `TotalPlayerCaravans` - Player-owned caravans
- `TotalNotableCaravans` - Notable-owned caravans
- `TotalNPCLordCaravans` - Lord-owned caravans

**Creation Methods:**
- `CreateNotableCaravan(Settlement settlement)` - Create notable caravan
- `CreatePlayerCaravan(Settlement settlement)` - Create player caravan

**Disbanding Methods:**
- `DisbandAllCaravanParties()` - Disband all
- `DisbandPlayerCaravans()` - Disband player caravans
- `DisbandNotableCaravans()` - Disband notable caravans
- `DisbandNPCLordCaravans()` - Disband lord caravans

**Management:**
- `CancelAllDisbandingCaravans()` - Cancel all disbanding
- `ForceDestroyDisbandingCaravans()` - Force destroy disbanding

---

## 10. BANDITS

### BanditManager
**Location:** [`BanditManager.cs`](Bannerlord.GameMaster/Bandits/BanditManager.cs)

Comprehensive bandit faction management system.

### HideoutExtensions
**Location:** [`HideoutExtensions.cs`](Bannerlord.GameMaster/Bandits/HideoutExtensions.cs)

Hideout-specific operations for bandit management.

---

## 11. BANNERS

### BannerColorPicker
**Location:** [`BannerColorPicker.cs`](Bannerlord.GameMaster/Banners/BannerColorPicker.cs)

**Key Methods:**
- `GetRandomColorId()` - Random color
- `GetLighterComplementaryColor(int baseColorId, float minLuminanceDifference)` - Lighter complement
- `GetDarkerComplementaryColor(int baseColorId, float minLuminanceDifference)` - Darker complement
- `GetContrastingColor(int baseColorId, bool preferLighter, float minLuminanceDifference)` - Contrasting color
- `GetBannerColorScheme(out int main, out int secondary, out int emblem)` - Full color scheme
- `GetAlternativeBannerColorScheme(...)` - Alternative scheme
- `GetStandardBannerColorScheme(...)` - Standard scheme
- `GetColorInfo(int colorId)` - Color information
- `AreColorsSimilar(int colorId1, int colorId2, float threshold)` - Similarity check
- `GetUniqueClanColorId(float minimumThreshold)` - Unique clan color

### BannerExtensions
**Location:** [`BannerExtensions.cs`](Bannerlord.GameMaster/Banners/BannerExtensions.cs)

**Key Methods:**
- `ApplyRandomColorScheme(Banner banner)` - Apply random scheme
- `ApplyColorScheme(Banner banner, int primaryColorId)` - Apply scheme
- `ApplyAlternativeColorScheme(Banner banner)` - Apply alternative
- `ApplyStandardColorScheme(Banner banner, int primaryColorId)` - Apply standard
- `ApplyUniqueColorScheme(Banner banner, float minimumThreshold)` - Apply unique

---

## 12. REMOVAL & CLEANUP HELPERS

### Remover Classes
**Location:** RemovalHelpers/ directory

**ClanRemover** (`ClanRemover.cs`) - Safe clan deletion
**HeroRemover** (`HeroRemover.cs`) - Safe hero deletion
**KingdomRemover** (`KingdomRemover.cs`) - Safe kingdom deletion
**PartyRemover** (`PartyRemover.cs`) - Safe party deletion

All provide safe removal procedures with proper cleanup.

---

## 13. BEHAVIORS

### Campaign Behaviors
**Location:** Behaviours/ directory

**BLGMObjectManagerBehaviour** (`BLGMObjectManagerBehaviour.cs`) - Central object management
**SettlementCultureBehavior** (`SettlementCultureBehavior.cs`) - Settlement culture management
**SettlementNameBehavior** (`SettlementNameBehavior.cs`) - Settlement naming with persistence
**VillageTradeBoundBehavior** (`VillageTradeBoundBehavior.cs`) - Village trade binding

---

## 14. OBJECT MANAGEMENT

### BLGMObjectManager
**Location:** [`BLGMObjectManager.cs`](Bannerlord.GameMaster/BLGMObjectManager.cs)

Central object manager for MBGUID assignment and entity registration.

---

## PATTERN NOTES

### Type Enum Pattern (Flags)
All major entities (Hero, Clan, Kingdom, Item, Troop, Settlement) follow a consistent flags-based type enumeration pattern:
```csharp
public enum EntityTypes : long // or ulong for large enums
{
    None = 0,
    Flag1 = 1,
    Flag2 = 2,
    // ...
}

// Extensions provide:
public static EntityTypes GetTypes(this Entity entity) { /* implementation */ }
public static bool HasAllTypes(this Entity entity, EntityTypes types) { /* AND logic */ }
public static bool HasAnyType(this Entity entity, EntityTypes types) { /* OR logic */ }
public static string FormattedDetails(this Entity entity) { /* formatted */ }
```

### Query Pattern
All major entity types provide query classes with consistent interface:
```csharp
public static class EntityQueries
{
    public static Entity GetById(string id) { /* lookup */ }
    public static List<Entity> Query(string query, EntityTypes types, bool matchAll) { /* filtering */ }
    public static EntityTypes ParseType(string typeString) { /* parsing */ }
    public static string GetFormattedDetails(List<Entity> entities) { /* formatting */ }
}
```

### Result Pattern
Operations returning success/failure use [`BLGMResult`](Bannerlord.GameMaster/Common/BLGMResult.cs) struct with `.DisplayMessage()`, `.Log()`, `.DisplayAndLog()` chaining methods.

---

## QUICK EXAMPLE: CREATING A KINGDOM

```csharp
using Bannerlord.GameMaster.Kingdoms;
using Bannerlord.GameMaster.Settlements;

// Find or get a settlement
Settlement capital = SettlementManager.GetRandomTown();

// Create kingdom with auto-generated names and 4 vassal clans
Kingdom newKingdom = KingdomGenerator.CreateKingdom(
    homeSettlement: capital,
    vassalClanCount: 4,
    name: null,  // auto-generate
    rulingClanName: null,  // auto-generate
    cultureFlags: CultureFlags.AllMainCultures
);

// Query result
var kingdoms = KingdomQueries.QueryKingdoms("new", KingdomTypes.Active);
```

---

**Last Updated:** 2026-01-17
**BLGM Version:** Current
**Note:** Console namespace commands excluded from API reference. See console command files for command implementation patterns.
