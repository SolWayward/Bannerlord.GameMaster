# Bannerlord.GameMaster - API Quick Reference

## Namespace: Bannerlord.GameMaster

### ObjectManager
```csharp
public class ObjectManager
{
    public static ObjectManager Instance { get; }
    public string[] GetObjectIds()
    public string GetUniqueStringId()
    public string GetUniqueStringId(Type type)
    public string GetUniqueStringId(TextObject nameObj)
    public string GetUniqueStringId(TextObject nameObj, Type type)
}
```

### RandomNumberGen
```csharp
public class RandomNumberGen
{
    public static RandomNumberGen Instance { get; }
    public int NextRandomInt()
    public int NextRandomInt(int max)
    public int NextRandomInt(int min, int max)
    public float NextRandomFloat()
    public double NextRandomDouble()
    public void NextRandomBytes(byte[] buffer)
    public uint NextRandomRGBColor { get; }
}
```

## Namespace: Bannerlord.GameMaster.Banners

### BannerColorPicker
```csharp
public static class BannerColorPicker
{
    public static int GetRandomColorId()
    public static int GetLighterComplementaryColor(int baseColorId, float minLuminanceDifference = 0.15f)
    public static int GetDarkerComplementaryColor(int baseColorId, float minLuminanceDifference = 0.15f)
    public static int GetContrastingColor(int baseColorId, bool preferLighter, float minLuminanceDifference = 0.3f)
    public static void GetBannerColorScheme(out int mainBackgroundId, out int secondaryBackgroundId, out int emblemColorId)
    public static void GetBannerColorScheme(int mainBackgroundId, out int secondaryBackgroundId, out int emblemColorId)
    public static void GetAlternativeBannerColorScheme(out int mainBackgroundId, out int secondaryBackgroundId, out int emblemColorId)
    public static void GetAlternativeBannerColorScheme(int mainBackgroundId, out int secondaryBackgroundId, out int emblemColorId)
    public static void GetStandardBannerColorScheme(int mainBackgroundId, out int secondaryBackgroundId, out int emblemColorId)
    public static string GetColorInfo(int colorId)
    public static bool AreColorsSimilar(int colorId1, int colorId2, float threshold = 0.2f)
    public static bool AreColorsSimilar(uint color1, uint color2, float threshold = 0.2f)
    public static int GetUniqueClanColorId(float minimumThreshold = 0.15f)
}
```

### BannerExtensions
```csharp
public static class BannerExtensions
{
    public static Banner ApplyRandomColorScheme(this Banner banner)
    public static Banner ApplyColorScheme(this Banner banner, int primaryColorId)
    public static Banner ApplyAlternativeColorScheme(this Banner banner)
    public static Banner ApplyAlternativeColorScheme(this Banner banner, int primaryColorId)
    public static Banner ApplyStandardColorScheme(this Banner banner, int primaryColorId)
    public static Banner ApplyUniqueColorScheme(this Banner banner, float minimumThreshold = 0.15f)
}
```

## Namespace: Bannerlord.GameMaster.Characters

### BeardTags
```csharp
public static class BeardTags { /* Constants */ }
```

### CharacterTemplatePooler
```csharp
public class CharacterTemplatePooler
{
    public string Debug_CountTemplates()
    public List<CharacterObject> GetCulturalTemplates(CultureObject culture)
    public List<CharacterObject> GetLordAndWandererCharacters(CultureObject culture)
    public List<CharacterObject> GetGenderCulturalTemplates(CultureObject culture, bool isFemale)
    public List<CharacterObject> GetAllHeroTemplatesFromFlags(CultureFlags cultureFlags, GenderFlags genderFlags)
}
```

### GenderFlags
```csharp
public enum GenderFlags { /* Enum values */ }
```

### HairCoveringType
```csharp
public static class HairCoveringType { /* Constants */ }
```

### HairTags
```csharp
public static class HairTags { /* Constants */ }
```

### TattooTags
```csharp
public static class TattooTags { /* Constants */ }
```

## Namespace: Bannerlord.GameMaster.Clans

### ClanTypes
```csharp
public enum ClanTypes { /* Enum values */ }
```

### ClanExtensions
```csharp
public static class ClanExtensions
{
    public static ClanTypes GetClanTypes(this Clan clan)
    public static bool HasAllTypes(this Clan clan, ClanTypes types)
    public static bool HasAnyType(this Clan clan, ClanTypes types)
    public static ClanTypes GetTypes(this Clan clan)
    public static bool SetClanTier(this Clan clan, int targetTier)
    public static void SetStringName(this Clan clan, string name)
    public static string FormattedDetails(this Clan clan)
}
```

### ClanExtensionsWrapper
```csharp
public class ClanExtensionsWrapper : IEntityExtensions<Clan, ClanTypes>
{
    public ClanTypes GetTypes(Clan entity)
    public bool HasAllTypes(Clan entity, ClanTypes types)
    public bool HasAnyType(Clan entity, ClanTypes types)
    public string FormattedDetails(Clan entity)
}
```

### ClanGenerator
```csharp
public static class ClanGenerator
{
    public static Clan CreateClan(string name = null, Hero leader = null, Kingdom kingdom = null, bool createParty = true, int companionCount = 2, CultureFlags cultureFlags = CultureFlags.AllMainCultures)
    public static List<Clan> GenerateClans(int count, CultureFlags cultureFlags = CultureFlags.AllMainCultures, Kingdom kingdom = null, bool createParties = true, int companionCount = 2)
    public static Clan CreateMinorClan(string name = null, Hero leader = null, CultureFlags cultureFlags = CultureFlags.AllMainCultures, bool createParty = true)
}
```

### ClanQueries
```csharp
public static class ClanQueries
{
    public static Clan GetClanById(string clanId)
    public static List<Clan> QueryClans(string query = "", ClanTypes requiredTypes = ClanTypes.None, bool matchAll = true, string sortBy = "id", bool sortDescending = false)
    public static ClanTypes ParseClanType(string typeString)
    public static ClanTypes ParseClanTypes(IEnumerable<string> typeStrings)
    public static string GetFormattedDetails(List<Clan> clans)
    public static List<Hero> GetPartyLeaders(Clan clan)
}
```

### ClanQueriesWrapper
```csharp
public class ClanQueriesWrapper : IEntityQueries<Clan, ClanTypes>
{
    public Clan GetById(string id)
    public List<Clan> Query(string query, ClanTypes types, bool matchAll)
    public ClanTypes ParseType(string typeString)
    public ClanTypes ParseTypes(IEnumerable<string> typeStrings)
    public string GetFormattedDetails(List<Clan> entities)
}
```

## Namespace: Bannerlord.GameMaster.Common.Interfaces

### IEntityExtensions<TEntity, TTypes>
```csharp
public interface IEntityExtensions<TEntity, TTypes>
{
    TTypes GetTypes(TEntity entity)
    bool HasAllTypes(TEntity entity, TTypes types)
    bool HasAnyType(TEntity entity, TTypes types)
    string FormattedDetails(TEntity entity)
}
```

### IEntityQueries<TEntity, TTypes>
```csharp
public interface IEntityQueries<TEntity, TTypes>
{
    TEntity GetById(string id)
    List<TEntity> Query(string query, TTypes types, bool matchAll)
    TTypes ParseType(string typeString)
    TTypes ParseTypes(IEnumerable<string> typeStrings)
    string GetFormattedDetails(List<TEntity> entities)
}
```

## Namespace: Bannerlord.GameMaster.Cultures

### CultureExtensions
```csharp
public static class CultureExtensions
{
    public static CultureFlags ToCultureFlag(this CultureObject culture)
}
```

### CultureFlags
```csharp
public enum CultureFlags { /* Enum values */ }
```

### CultureLookup
```csharp
public static class CultureLookup
{
    public static string GetUniqueRandomHeroName(CultureObject culture, bool isFemale)
    public static string GetUniqueRandomClanName(CultureObject culture)
    public static string GetUniqueRandomKingdomName(CultureObject culture)
    public static CultureFlags GetCultureFlag(CultureObject culture)
}
```

### CustomNames
```csharp
public static class CustomNames { /* Constants */ }
```

## Namespace: Bannerlord.GameMaster.Heroes

### HeroTypes
```csharp
public enum HeroTypes
{
    None, IsArtisan, Lord, Wanderer, Notable, Merchant, Children,
    Female, Male, ClanLeader, KingdomRuler, PartyLeader, Fugitive,
    Alive, Dead, Prisoner, WithoutClan, WithoutKingdom, Married
}
```

### HeroExtensions
```csharp
public static class HeroExtensions
{
    public static HeroTypes GetHeroTypes(this Hero hero)
    public static bool HasAllTypes(this Hero hero, HeroTypes types)
    public static bool HasAnyType(this Hero hero, HeroTypes types)
    public static HeroTypes GetTypes(this Hero hero)
    public static MobileParty CreateParty(this Hero hero, Settlement spawnSettlement)
    public static Settlement GetHomeOrAlternativeSettlement(this Hero hero)
    public static void EquipHeroBasedOnCulture(this Hero hero)
    public static void EquipLordBasedOnCulture(this Hero hero)
    public static void SetStringName(this Hero hero, string name)
    public static string FormattedDetails(this Hero hero)
}
```

### HeroExtensionsWrapper
```csharp
public class HeroExtensionsWrapper : IEntityExtensions<Hero, HeroTypes>
{
    public HeroTypes GetTypes(Hero entity)
    public bool HasAllTypes(Hero entity, HeroTypes types)
    public bool HasAnyType(Hero entity, HeroTypes types)
    public string FormattedDetails(Hero entity)
}
```

### HeroGenerator
```csharp
public static class HeroGenerator
{
    public static void InitializeAsLord(Hero hero, Settlement homeSettlement, bool createParty = true)
    public static void InitializeAsWanderer(Hero hero, Settlement settlement)
    public static void InitializeAsCompanion(Hero hero)
    public static void CleanupHeroState(Hero hero)
    public static Hero CreateLord(string name, CultureFlags cultureFlags, GenderFlags genderFlags, Clan clan, bool withParty = true, float randomFactor = 0.5f)
    public static List<Hero> CreateLords(int count, CultureFlags cultureFlags, GenderFlags genderFlags, Clan clan, bool withParties = true, float randomFactor = 0.5f)
    public static Hero CreateWanderer(string name, CultureFlags cultureFlags, GenderFlags genderFlags, Settlement settlement, float randomFactor = 0.5f)
    public static List<Hero> CreateWanderers(int count, CultureFlags cultureFlags, GenderFlags genderFlags, Settlement settlement, float randomFactor = 0.5f)
    public static List<Hero> CreateCompanions(int count, CultureFlags cultureFlags, GenderFlags genderFlags = GenderFlags.Either, float randomFactor = 0.5f)
    public static CharacterObject RandomizeCharacterObject(CharacterObject template, float randomFactor, bool useFaceConstraints = true, bool useBuildConstraints = true, bool useHairConstraints = true)
}
```

### HeroQueries
```csharp
public static class HeroQueries
{
    public static Hero GetHeroById(string heroId)
    public static List<Hero> QueryHeroes(string query = "", HeroTypes requiredTypes = HeroTypes.None, bool matchAll = true, bool includeDead = false, string sortBy = "id", bool sortDescending = false)
    public static HeroTypes ParseHeroType(string typeString)
    public static HeroTypes ParseHeroTypes(IEnumerable<string> typeStrings)
    public static string GetFormattedDetails(List<Hero> heroes)
}
```

### HeroQueriesWrapper
```csharp
public class HeroQueriesWrapper : IEntityQueries<Hero, HeroTypes>
{
    public Hero GetById(string id)
    public List<Hero> Query(string query, HeroTypes types, bool matchAll)
    public HeroTypes ParseType(string typeString)
    public HeroTypes ParseTypes(IEnumerable<string> typeStrings)
    public string GetFormattedDetails(List<Hero> entities)
}
```

## Namespace: Bannerlord.GameMaster.Information

### InfoMessage
```csharp
public static class InfoMessage
{
    public static void Log(string message)
    public static void Success(string message)
    public static void Warning(string message)
    public static void Error(string message)
    public static void Important(string message)
    public static void Status(string message)
    public static void Status2(string message)
    public static void Write(string message, Color color)
}
```

## Namespace: Bannerlord.GameMaster.Items

### ItemTypes
```csharp
public enum ItemTypes { /* Enum values */ }
```

### ItemExtensions
```csharp
public static class ItemExtensions
{
    public static ItemTypes GetItemTypes(this ItemObject item)
    public static bool HasAllTypes(this ItemObject item, ItemTypes types)
    public static bool HasAnyType(this ItemObject item, ItemTypes types)
    public static string FormattedDetails(this ItemObject item)
    public static ItemTypes GetTypes(this ItemObject item)
}
```

### ItemExtensionsWrapper
```csharp
public class ItemExtensionsWrapper : IEntityExtensions<ItemObject, ItemTypes>
{
    public ItemTypes GetTypes(ItemObject entity)
    public bool HasAllTypes(ItemObject entity, ItemTypes types)
    public bool HasAnyType(ItemObject entity, ItemTypes types)
    public string FormattedDetails(ItemObject entity)
}
```

### ItemModifierHelper
```csharp
public static class ItemModifierHelper
{
    public static List<ItemModifier> GetAllModifiers()
    public static ItemModifier GetModifierByName(string modifierName)
    public static string GetFormattedModifierList()
    public static bool CanHaveModifier(ItemObject item)
    public static string GetModifierInfo(ItemModifier modifier)
    public static class CommonModifiers { /* Constants */ }
}
```

### ItemQueries
```csharp
public static class ItemQueries
{
    public static ItemObject GetItemById(string itemId)
    public static List<ItemObject> QueryItems(string query = "", ItemTypes requiredTypes = ItemTypes.None, bool matchAll = true, string sortBy = "id", bool sortDescending = false)
    public static ItemTypes ParseItemType(string typeString)
    public static ItemTypes ParseItemTypes(IEnumerable<string> typeStrings)
    public static string GetFormattedDetails(List<ItemObject> items)
}
```

### ItemQueriesWrapper
```csharp
public class ItemQueriesWrapper : IEntityQueries<ItemObject, ItemTypes>
{
    public ItemObject GetById(string id)
    public List<ItemObject> Query(string query, ItemTypes types, bool matchAll)
    public ItemTypes ParseType(string typeString)
    public ItemTypes ParseTypes(IEnumerable<string> typeStrings)
    public string GetFormattedDetails(List<ItemObject> entities)
}
```

## Namespace: Bannerlord.GameMaster.Kingdoms

### KingdomTypes
```csharp
public enum KingdomTypes { /* Enum values */ }
```

### KingdomExtensions
```csharp
public static class KingdomExtensions
{
    public static KingdomTypes GetKingdomTypes(this Kingdom kingdom)
    public static bool HasAllTypes(this Kingdom kingdom, KingdomTypes types)
    public static bool HasAnyType(this Kingdom kingdom, KingdomTypes types)
    public static string FormattedDetails(this Kingdom kingdom)
    public static KingdomTypes GetTypes(this Kingdom kingdom)
}
```

### KingdomExtensionsWrapper
```csharp
public class KingdomExtensionsWrapper : IEntityExtensions<Kingdom, KingdomTypes>
{
    public KingdomTypes GetTypes(Kingdom entity)
    public bool HasAllTypes(Kingdom entity, KingdomTypes types)
    public bool HasAnyType(Kingdom entity, KingdomTypes types)
    public string FormattedDetails(Kingdom entity)
}
```

### KingdomAllianceExtensions
```csharp
public static class KingdomAllianceExtensions
{
    public static void DeclareAlliance(this Kingdom proposingKindom, Kingdom receivingkingdom, bool callToWar = true)
    public static void ProposeCallAllyToWarForceAccept(this Kingdom proposer, Kingdom ally, Kingdom enemy)
    public static void ProposeCallAllyToWarForceAccept(this Kingdom proposer, Kingdom ally)
    public static void ProposeCallAllyToWar(this Kingdom proposer, Kingdom ally, Kingdom enemy)
    public static void ProposeCallAllyToWar(this Kingdom proposer, Kingdom ally)
    public static void AcceptCallAllyToWar(this Kingdom proposer, Kingdom ally, Kingdom enemy)
    public static void AcceptCallAllyToWar(this Kingdom proposer, Kingdom ally)
}
```

### KingdomGenerator
```csharp
public class KingdomGenerator
{
    public static Kingdom CreateKingdom(Settlement homeSettlement, int vassalClanCount = 4, string name = null, string rulingClanName = null, CultureFlags cultureFlags = CultureFlags.AllMainCultures)
    public static List<Kingdom> GenerateKingdoms(int count, int vassalClanCount = 4, CultureFlags cultureFlags = CultureFlags.AllMainCultures)
}
```

### KingdomQueries
```csharp
public static class KingdomQueries
{
    public static Kingdom GetKingdomById(string kingdomId)
    public static List<Kingdom> QueryKingdoms(string query = "", KingdomTypes requiredTypes = KingdomTypes.None, bool matchAll = true, string sortBy = "id", bool sortDescending = false)
    public static KingdomTypes ParseKingdomType(string typeString)
    public static KingdomTypes ParseKingdomTypes(IEnumerable<string> typeStrings)
    public static string GetFormattedDetails(List<Kingdom> kingdoms)
    public static List<Hero> GetClanLeaders(Kingdom kingdom)
    public static List<Hero> GetPartyLeaders(Kingdom kingdom)
    public static List<Hero> GetHeroes(Kingdom kingdom)
}
```

### KingdomQueriesWrapper
```csharp
public class KingdomQueriesWrapper : IEntityQueries<Kingdom, KingdomTypes>
{
    public Kingdom GetById(string id)
    public List<Kingdom> Query(string query, KingdomTypes types, bool matchAll)
    public KingdomTypes ParseType(string typeString)
    public KingdomTypes ParseTypes(IEnumerable<string> typeStrings)
    public string GetFormattedDetails(List<Kingdom> entities)
}
```

### KingdomTradeAgreementExtensions
```csharp
public static class KingdomTradeAgreementExtensions
{
    public static void MakeTradeAgreement(this Kingdom proposingKindom, Kingdom receivingkingdom)
}
```

### KingdomTributeExtensions
```csharp
public static class KingdomTributeExtensions
{
    public static TributeInfo PayTribute(this Kingdom kingdom, Kingdom otherKingdom, int dailyAmount, int days)
    public static TributeInfo GetTributeInfo(this Kingdom kingdom, Kingdom otherKingdom)
}
```

### TributeInfo
```csharp
public struct TributeInfo
{
    public string GetTributeString()
}
```

## Namespace: Bannerlord.GameMaster.Party

### MobilePartyExtensions
```csharp
public static class MobilePartyExtensions
{
    public static void AddCompanionToParty(this MobileParty mobileParty, Hero hero)
    public static void AddCompanionsToParty(this MobileParty mobileParty, List<Hero> heroes)
    public static void AddLordToParty(this MobileParty mobileParty, Hero hero)
    public static void AddLordsToParty(this MobileParty mobileParty, List<Hero> heroes)
    public static void AddBasicTroops(this MobileParty mobileParty, int count)
    public static void AddEliteTroops(this MobileParty mobileParty, int count)
    public static void AddMercenaryTroops(this MobileParty mobileParty, int count)
    public static void AddMixedTierTroops(this MobileParty mobileParty, int countOfEach)
    public static void UpgradeTroops(this MobileParty mobileParty, int upgradeCount = -1, bool upgradeAllTroops = false)
    public static void AddXp(this MobileParty mobileParty, int xp)
}
```

## Namespace: Bannerlord.GameMaster.Settlements

### SettlementTypes
```csharp
public enum SettlementTypes : long { /* Enum values */ }
```

### SettlementExtensions
```csharp
public static class SettlementExtensions
{
    public static SettlementTypes GetSettlementTypes(this Settlement settlement)
    public static bool HasAllTypes(this Settlement settlement, SettlementTypes types)
    public static bool HasAnyType(this Settlement settlement, SettlementTypes types)
    public static string FormattedDetails(this Settlement settlement)
    public static SettlementTypes GetTypes(this Settlement settlement)
}
```

### SettlementExtensionsWrapper
```csharp
public class SettlementExtensionsWrapper : IEntityExtensions<Settlement, SettlementTypes>
{
    public SettlementTypes GetTypes(Settlement entity)
    public bool HasAllTypes(Settlement entity, SettlementTypes types)
    public bool HasAnyType(Settlement entity, SettlementTypes types)
    public string FormattedDetails(Settlement entity)
}
```

### SettlementNameBehavior
```csharp
public class SettlementNameBehavior : CampaignBehaviorBase
{
    public bool RenameSettlement(Settlement settlement, string newName)
    public bool ResetSettlementName(Settlement settlement)
    public int ResetAllSettlementNames()
    public string GetOriginalName(Settlement settlement)
    public bool IsRenamed(Settlement settlement)
    public int GetRenamedSettlementCount()
}
```

### SettlementQueries
```csharp
public static class SettlementQueries
{
    public static Settlement GetSettlementById(string settlementId)
    public static List<Settlement> QuerySettlements(string query = "", SettlementTypes requiredTypes = SettlementTypes.None, bool matchAll = true, string sortBy = "id", bool sortDescending = false)
    public static SettlementTypes ParseSettlementType(string typeString)
    public static SettlementTypes ParseSettlementTypes(IEnumerable<string> typeStrings)
    public static string GetFormattedDetails(List<Settlement> settlements)
}
```

### SettlementQueriesWrapper
```csharp
public class SettlementQueriesWrapper : IEntityQueries<Settlement, SettlementTypes>
{
    public Settlement GetById(string id)
    public List<Settlement> Query(string query, SettlementTypes types, bool matchAll)
    public SettlementTypes ParseType(string typeString)
    public SettlementTypes ParseTypes(IEnumerable<string> typeStrings)
    public string GetFormattedDetails(List<Settlement> entities)
}
```

### SettlementSaveDefiner
```csharp
public class SettlementSaveDefiner : SaveableTypeDefiner { }
```

### SettlementNameData
```csharp
public class SettlementNameData { }
```

## Namespace: Bannerlord.GameMaster.Troops

### TroopTypes
```csharp
public enum TroopTypes : long { /* Enum values */ }
```

### TroopExtensions
```csharp
public static class TroopExtensions
{
    public static TroopTypes GetTroopTypes(this CharacterObject character)
    public static bool HasAllTypes(this CharacterObject character, TroopTypes types)
    public static bool HasAnyType(this CharacterObject character, TroopTypes types)
    public static bool IsActualTroop(this CharacterObject character)
    public static string GetTroopCategory(this CharacterObject character)
    public static string FormattedDetails(this CharacterObject character)
    public static TroopTypes GetTypes(this CharacterObject character)
    public static bool HasShield(this CharacterObject character)
    public static bool HasWeaponType(this CharacterObject character, ItemObject.ItemTypeEnum weaponType)
    public static bool HasWeaponClass(this CharacterObject character, WeaponClass weaponClass)
    public static bool HasTwoHandedWeapon(this CharacterObject character)
    public static bool HasPolearm(this CharacterObject character)
    public static bool IsMounted(this CharacterObject character)
}
```

### TroopExtensionsWrapper
```csharp
public class TroopExtensionsWrapper : IEntityExtensions<CharacterObject, TroopTypes>
{
    public TroopTypes GetTypes(CharacterObject entity)
    public bool HasAllTypes(CharacterObject entity, TroopTypes types)
    public bool HasAnyType(CharacterObject entity, TroopTypes types)
    public string FormattedDetails(CharacterObject entity)
}
```

### TroopQueries
```csharp
public static class TroopQueries
{
    public static CharacterObject GetTroopById(string troopId)
    public static List<CharacterObject> QueryTroops(string query = "", TroopTypes requiredTypes = TroopTypes.None, bool matchAll = true, string sortBy = "id", bool sortDescending = false)
    public static List<CharacterObject> QueryCharacterObjects(string query = "", TroopTypes requiredTypes = TroopTypes.None, bool matchAll = true, string sortBy = "id", bool sortDescending = false)
    public static TroopTypes ParseTroopType(string typeString)
    public static TroopTypes ParseTroopTypes(IEnumerable<string> typeStrings)
    public static string GetFormattedDetails(List<CharacterObject> troops)
}
```

### TroopQueriesWrapper
```csharp
public class TroopQueriesWrapper : IEntityQueries<CharacterObject, TroopTypes>
{
    public CharacterObject GetById(string id)
    public List<CharacterObject> Query(string query, TroopTypes types, bool matchAll)
    public TroopTypes ParseType(string typeString)
    public TroopTypes ParseTypes(IEnumerable<string> typeStrings)
    public string GetFormattedDetails(List<CharacterObject> entities)
}
```

### TroopUpgrader
```csharp
public class TroopUpgrader
{
    public static void UpgradeTroops(TroopRoster memberRoster, int upgradeCount = -1, bool upgradeAllTroops = false)
}
```
