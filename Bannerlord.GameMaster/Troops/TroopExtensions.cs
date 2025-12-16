using System;
using System.Linq;
using TaleWorlds.Core;
using TaleWorlds.CampaignSystem;
using Bannerlord.GameMaster.Common.Interfaces;

namespace Bannerlord.GameMaster.Troops
{
    /// <summary>
    /// Flags enum for categorizing troop types across multiple dimensions
    /// </summary>
    [Flags]
    public enum TroopTypes : long
    {
        None = 0,
        
        // Formation/Combat Roles (1-16)
        Infantry = 1,              // 2^0  - FormationClass: Infantry
        Ranged = 2,                // 2^1  - FormationClass: Ranged
        Cavalry = 4,               // 2^2  - FormationClass: Cavalry
        HorseArcher = 8,           // 2^3  - FormationClass: HorseArcher
        Mounted = 16,              // 2^4  - IsMounted (Cavalry or HorseArcher)
        
        // Troop Line Categories (32-1024)
        Regular = 32,              // 2^5  - Culture's regular/main troop line
        Noble = 64,                // 2^6  - Culture's noble/elite troop line
        Militia = 128,             // 2^7  - Culture's militia (garrison) troop line
        Mercenary = 256,           // 2^8  - Mercenary troops
        Caravan = 512,             // 2^9  - Caravan guards/masters/traders
        Peasant = 1024,            // 2^10 - Villagers/peasants/townsfolk
        MinorFaction = 2048,       // 2^11 - Minor faction troops (Eleftheroi, Brotherhood, etc.)
        
        // Equipment-Based Categories (4096-65536)
        Shield = 4096,             // 2^12 - Has shield in equipment
        TwoHanded = 8192,          // 2^13 - Has two-handed weapon
        Polearm = 16384,           // 2^14 - Has polearm weapon
        Bow = 32768,               // 2^15 - Has bow
        Crossbow = 65536,          // 2^16 - Has crossbow
        ThrowingWeapon = 131072,   // 2^17 - Has throwing weapon
        
        // Tier-Based Categories (262144-33554432)
        Tier0 = 262144,            // 2^18 - Tier 0 troops
        Tier1 = 524288,            // 2^19 - Tier 1 troops
        Tier2 = 1048576,           // 2^20 - Tier 2 troops
        Tier3 = 2097152,           // 2^21 - Tier 3 troops
        Tier4 = 4194304,           // 2^22 - Tier 4 troops
        Tier5 = 8388608,           // 2^23 - Tier 5 troops
        Tier6Plus = 16777216,      // 2^24 - Tier 6+ troops (includes tier 7 if modded)
        
        // Culture-Based Categories (33554432-8589934592)
        Empire = 33554432,         // 2^25 - Culture: Empire
        Vlandia = 67108864,        // 2^26 - Culture: Vlandia
        Sturgia = 134217728,       // 2^27 - Culture: Sturgia
        Aserai = 268435456,        // 2^28 - Culture: Aserai
        Khuzait = 536870912,       // 2^29 - Culture: Khuzait
        Battania = 1073741824,     // 2^30 - Culture: Battania
        Nord = 2147483648,         // 2^31 - Culture: Nord (Warsails DLC - optional)
        Bandit = 4294967296,       // 2^32 - Culture: Bandit (special culture)
    }

    /// <summary>
    /// Extension methods for CharacterObject (troops) providing type categorization and formatting
    /// </summary>
    public static class TroopExtensions
    {
        /// <summary>
        /// Gets all troop type flags for this character
        /// CRITICAL: Heroes/Lords are NEVER troops - returns None immediately if IsHero is true
        /// </summary>
        public static TroopTypes GetTroopTypes(this CharacterObject character)
        {
            // CRITICAL: Heroes/Lords are never troops - exclude immediately
            if (character.IsHero)
                return TroopTypes.None;

            TroopTypes types = TroopTypes.None;

            // Formation/Combat Roles
            switch (character.DefaultFormationClass)
            {
                case FormationClass.Infantry:
                    types |= TroopTypes.Infantry;
                    break;
                case FormationClass.Ranged:
                    types |= TroopTypes.Ranged;
                    break;
                case FormationClass.Cavalry:
                    types |= TroopTypes.Cavalry;
                    types |= TroopTypes.Mounted;
                    break;
                case FormationClass.HorseArcher:
                    types |= TroopTypes.HorseArcher;
                    types |= TroopTypes.Mounted;
                    break;
            }

            // Troop Line Categories (based on StringId patterns and culture)
            // CRITICAL: Occupation is for HEROES not TROOPS - use StringId patterns instead
            var stringIdLower = character.StringId.ToLower();
            
            // Detect troop line by StringId patterns
            if (stringIdLower.Contains("noble") || stringIdLower.Contains("knight") ||
                stringIdLower.Contains("druzhnik") || stringIdLower.Contains("cataphract"))
            {
                types |= TroopTypes.Noble;
            }
            else if (stringIdLower.Contains("militia"))
            {
                types |= TroopTypes.Militia;
            }
            else if (stringIdLower.Contains("mercenary") && !stringIdLower.Contains("leader"))
            {
                types |= TroopTypes.Mercenary;
            }
            else if (stringIdLower.Contains("caravan_guard") || stringIdLower.Contains("caravan_master") ||
                     stringIdLower.Contains("armed_trader") || stringIdLower.Contains("sea_trader"))
            {
                types |= TroopTypes.Caravan;
            }
            else if ((stringIdLower.Contains("villager") || stringIdLower.Contains("village_woman") ||
                      stringIdLower.Contains("townsman") || stringIdLower.Contains("townswoman") ||
                      (stringIdLower.Contains("fighter") && character.GetBattleTier() == 0)) &&
                     character.GetBattleTier() == 0 && character.Level == 1)
            {
                types |= TroopTypes.Peasant;
            }
            else if (character.GetBattleTier() >= 2 &&
                     (stringIdLower.Contains("eleftheroi") || stringIdLower.Contains("brotherhood_of_woods") ||
                      stringIdLower.Contains("hidden_hand") || stringIdLower.Contains("jawwal") ||
                      stringIdLower.Contains("lake_rats") || stringIdLower.Contains("forest_people") ||
                      stringIdLower.Contains("karakhuzait")))
            {
                types |= TroopTypes.MinorFaction;
            }
            else if (character.Culture != null)
            {
                // Default to Regular for culture-based troops that aren't noble/militia/mercenary/caravan/peasant
                types |= TroopTypes.Regular;
            }

            // Equipment-Based Categories
            if (character.HasShield())
                types |= TroopTypes.Shield;
            if (character.HasTwoHandedWeapon())
                types |= TroopTypes.TwoHanded;
            if (character.HasPolearm())
                types |= TroopTypes.Polearm;
            if (character.HasWeaponType(ItemObject.ItemTypeEnum.Bow))
                types |= TroopTypes.Bow;
            if (character.HasWeaponType(ItemObject.ItemTypeEnum.Crossbow))
                types |= TroopTypes.Crossbow;
            if (character.HasWeaponType(ItemObject.ItemTypeEnum.Thrown))
                types |= TroopTypes.ThrowingWeapon;

            // Tier-Based Categories (exact tier mapping using GetBattleTier())
            int tier = character.GetBattleTier();
            if (tier == 0)
                types |= TroopTypes.Tier0;
            else if (tier == 1)
                types |= TroopTypes.Tier1;
            else if (tier == 2)
                types |= TroopTypes.Tier2;
            else if (tier == 3)
                types |= TroopTypes.Tier3;
            else if (tier == 4)
                types |= TroopTypes.Tier4;
            else if (tier == 5)
                types |= TroopTypes.Tier5;
            else if (tier >= 6)
                types |= TroopTypes.Tier6Plus;

            // Culture-Based Categories
            if (character.Culture != null)
            {
                string cultureId = character.Culture.StringId.ToLower();
                
                if (cultureId.Contains("empire"))
                    types |= TroopTypes.Empire;
                else if (cultureId.Contains("vlandia"))
                    types |= TroopTypes.Vlandia;
                else if (cultureId.Contains("sturgia"))
                    types |= TroopTypes.Sturgia;
                else if (cultureId.Contains("aserai"))
                    types |= TroopTypes.Aserai;
                else if (cultureId.Contains("khuzait"))
                    types |= TroopTypes.Khuzait;
                else if (cultureId.Contains("battania"))
                    types |= TroopTypes.Battania;
                else if (cultureId.Contains("nord"))
                    types |= TroopTypes.Nord;
                else if (cultureId.Contains("bandit") || cultureId.Contains("looter"))
                    types |= TroopTypes.Bandit;
            }

            return types;
        }

        /// <summary>
        /// Checks if character has ALL specified flags (AND logic)
        /// </summary>
        public static bool HasAllTypes(this CharacterObject character, TroopTypes types)
        {
            if (types == TroopTypes.None) return true;
            var troopTypes = character.GetTroopTypes();
            return (troopTypes & types) == types;
        }

        /// <summary>
        /// Checks if character has ANY of the specified flags (OR logic)
        /// </summary>
        public static bool HasAnyType(this CharacterObject character, TroopTypes types)
        {
            if (types == TroopTypes.None) return true;
            var troopTypes = character.GetTroopTypes();
            return (troopTypes & types) != TroopTypes.None;
        }

        /// <summary>
        /// Checks if this character is an actual troop (not a template, NPC, child, etc.)
        /// Uses specific exclusion patterns to filter out non-combat NPCs while preserving combat troops.
        /// IMPORTANT: villager_*, fighter_*, and caravan_leader_* are combat troops and should NOT be excluded.
        /// </summary>
        public static bool IsActualTroop(this CharacterObject character)
        {
            if (character.IsHero)
                return false;

            var stringIdLower = character.StringId.ToLower();

            // 1. Templates/Equipment Sets - Non-playable character templates
            if (stringIdLower.Contains("template") || stringIdLower.Contains("_equipment") ||
                stringIdLower.Contains("_bat_") || stringIdLower.Contains("_civ_") ||
                stringIdLower.Contains("_noncom_"))
                return false;

            // 2. Town Service NPCs - Non-combat town workers and merchants
            // These provide services in towns but never participate in combat
            if (stringIdLower.Contains("armorer") || stringIdLower.Contains("barber") ||
                stringIdLower.Contains("blacksmith") || stringIdLower.Contains("beggar") ||
                stringIdLower.Contains("merchant") || stringIdLower.Contains("shop_keeper") ||
                stringIdLower.Contains("shop_worker") || stringIdLower.Contains("weaponsmith") ||
                stringIdLower.Contains("shipwright") || stringIdLower.Contains("ransom_broker") ||
                stringIdLower.Contains("musician"))
                return false;

            // 3. Tavern NPCs - Non-combat tavern workers
            if (stringIdLower.Contains("tavern_wench") || stringIdLower.Contains("tavernkeeper") ||
                stringIdLower.Contains("barmaid") || stringIdLower.Contains("tavern_gamehost") ||
                stringIdLower.Contains("tavern_guard"))
                return false;

            // 4. Horse-related NPCs - Stable workers and horse merchants
            if (stringIdLower.Contains("horse_merchant") || stringIdLower.Contains("horse_trader"))
                return false;

            // 5. Notables - Notable NPCs with "notary" suffix (preacher_notary, merchant_notary, etc.)
            if (stringIdLower.Contains("notary"))
                return false;

            // 6. Wanderers/Companions - Recruitable NPCs that should be excluded
            // These start with specific prefixes identifying them as special characters
            if (stringIdLower.StartsWith("spc_notable_") || stringIdLower.StartsWith("spc_wanderer_") ||
                stringIdLower.StartsWith("npc_wanderer") || stringIdLower.StartsWith("npc_companion") ||
                stringIdLower.StartsWith("npc_armed_wanderer") || stringIdLower.StartsWith("npc_artisan") ||
                stringIdLower.StartsWith("npc_gang_leader") || stringIdLower.StartsWith("npc_merchant") ||
                stringIdLower.StartsWith("npc_preacher") || stringIdLower.StartsWith("npc_gentry") ||
                stringIdLower.StartsWith("npc_poor_wanderer"))
                return false;

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

            // 9. Children/Teens/Infants - Non-combat young characters
            if (stringIdLower.Contains("child") || stringIdLower.Contains("infant") ||
                stringIdLower.Contains("teenager"))
                return false;

            // 10. Practice/Training Dummies - Arena and training targets
            if (stringIdLower.Contains("_dummy") || stringIdLower.Contains("practice_stage") ||
                stringIdLower.Contains("weapon_practice") || stringIdLower.Contains("gear_practice"))
                return false;

            // 11. Special/System Characters - Tutorial, cutscene, and test characters
            if (stringIdLower.Contains("cutscene_") || stringIdLower.Contains("tutorial_") ||
                stringIdLower.Contains("duel_style_") || stringIdLower.Contains("player_char_creation_") ||
                stringIdLower.Contains("disguise_") || stringIdLower.StartsWith("test") ||
                stringIdLower.Contains("crazy_man") || stringIdLower.Contains("unarmed_ai") ||
                stringIdLower.Contains("borrowed_troop") || stringIdLower.Contains("neutral_lord") ||
                stringIdLower.Contains("stealth_character"))
                return false;

            // 12. Townsfolk Civilians - Non-combat town residents (NOT villagers!)
            // townsman/townswoman are passive civilians, unlike villagers who fight in village raids
            if (stringIdLower.Contains("townsman") || stringIdLower.Contains("townswoman"))
                return false;

            // 13. Tier 0 Level 1 Filter - Catch-all for non-combat NPCs
            // Most Tier 0 Level 1 characters are non-combatants (dancers, refugees, quest NPCs, etc.)
            // EXCEPT for known combat troops like villagers and fighters
            if (character.GetBattleTier() == 0 && character.Level == 1)
            {
                // Check for known Tier 0 Level 1 COMBAT troops that should NOT be excluded
                if (stringIdLower.Contains("villager") ||           // villager_* - fight in village raids
                    stringIdLower.Contains("village_woman") ||      // village_woman_* - fight in village raids
                    stringIdLower.Contains("fighter") ||            // fighter_* - basic recruitable troops
                    stringIdLower.Contains("caravan_guard") ||      // caravan_guard_* - fight when attacking caravans
                    stringIdLower.Contains("armed_trader") ||       // armed_trader_* - combat-capable traders
                    stringIdLower.Contains("looter") ||             // looter_* - bandit combat troops
                    stringIdLower.Contains("sea_raider_recruit"))   // sea_raider_recruit - combat troop
                {
                    // This is a known Tier 0 Level 1 combat troop - INCLUDE it
                    return true;
                }
                
                // Not a known combat troop pattern - this is a non-combatant (dancer, refugee, etc.)
                // EXCLUDE from combat troop queries
                return false;
            }

            // Everything else is considered a combat troop, including:
            // - Regular military troops (all tiers)
            // - Militia troops
            // - Mercenaries
            // - Bandits
            // - Minor faction troops
            // - Higher tier troops
            
            return true;
        }

        /// <summary>
        /// Gets the primary category for this troop
        /// </summary>
        public static string GetTroopCategory(this CharacterObject character)
        {
            if (!character.IsActualTroop())
                return "Non-Troop";

            var stringIdLower = character.StringId.ToLower();
            var types = character.GetTroopTypes();

            // Check specific categories first
            if (types.HasFlag(TroopTypes.Bandit))
                return "Bandit";
            
            if (types.HasFlag(TroopTypes.MinorFaction))
                return "Minor Faction";

            if (types.HasFlag(TroopTypes.Caravan))
                return "Caravan";

            if (types.HasFlag(TroopTypes.Peasant))
                return "Peasant";

            if (types.HasFlag(TroopTypes.Noble))
                return "Noble/Elite";

            if (types.HasFlag(TroopTypes.Militia))
                return "Militia";

            if (types.HasFlag(TroopTypes.Mercenary))
                return "Mercenary";

            if (types.HasFlag(TroopTypes.Regular))
                return "Regular";

            return "Unknown";
        }

        /// <summary>
        /// Returns a formatted string containing the troop's details
        /// </summary>
        public static string FormattedDetails(this CharacterObject character)
        {
            string cultureName = character.Culture?.Name?.ToString() ?? "None";
            string category = character.GetTroopCategory();
            return $"{character.StringId}\t{character.Name}\t[{category}]\tTier: {character.GetBattleTier()}\tLevel: {character.Level}\tCulture: {cultureName}\tFormation: {character.DefaultFormationClass}";
        }

        /// <summary>
        /// Alias for GetTroopTypes to match IEntityExtensions interface
        /// </summary>
        public static TroopTypes GetTypes(this CharacterObject character) => character.GetTroopTypes();

        /// <summary>
        /// Check if character has a shield in equipment
        /// </summary>
        public static bool HasShield(this CharacterObject character)
        {
            if (character.FirstBattleEquipment == null)
                return false;
                
            var equipment = character.FirstBattleEquipment;
            for (int i = 0; i < 12; i++) // Equipment slot count
            {
                var equipmentElement = equipment[i];
                if (equipmentElement.Item != null &&
                    equipmentElement.Item.ItemType == ItemObject.ItemTypeEnum.Shield)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Check if character has a specific weapon item type
        /// </summary>
        public static bool HasWeaponType(this CharacterObject character, ItemObject.ItemTypeEnum weaponType)
        {
            if (character.FirstBattleEquipment == null)
                return false;
                
            var equipment = character.FirstBattleEquipment;
            for (int i = 0; i < 12; i++)
            {
                var equipmentElement = equipment[i];
                if (equipmentElement.Item != null && equipmentElement.Item.ItemType == weaponType)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Check if character has a specific weapon class
        /// </summary>
        public static bool HasWeaponClass(this CharacterObject character, WeaponClass weaponClass)
        {
            if (character.FirstBattleEquipment == null)
                return false;
                
            var equipment = character.FirstBattleEquipment;
            for (int i = 0; i < 12; i++)
            {
                var equipmentElement = equipment[i];
                if (equipmentElement.Item?.WeaponComponent != null)
                {
                    if (equipmentElement.Item.WeaponComponent.PrimaryWeapon.WeaponClass == weaponClass)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// Check if character has a two-handed weapon
        /// </summary>
        public static bool HasTwoHandedWeapon(this CharacterObject character)
        {
            return character.HasWeaponType(ItemObject.ItemTypeEnum.TwoHandedWeapon);
        }

        /// <summary>
        /// Check if character has a polearm weapon
        /// </summary>
        public static bool HasPolearm(this CharacterObject character)
        {
            return character.HasWeaponType(ItemObject.ItemTypeEnum.Polearm);
        }

        /// <summary>
        /// Check if character is mounted (cavalry or horse archer)
        /// </summary>
        public static bool IsMounted(this CharacterObject character)
        {
            return character.DefaultFormationClass == FormationClass.Cavalry ||
                   character.DefaultFormationClass == FormationClass.HorseArcher;
        }
    }

    /// <summary>
    /// Wrapper class implementing IEntityExtensions interface for CharacterObject entities
    /// </summary>
    public class TroopExtensionsWrapper : IEntityExtensions<CharacterObject, TroopTypes>
    {
        public TroopTypes GetTypes(CharacterObject entity) => entity.GetTroopTypes();
        public bool HasAllTypes(CharacterObject entity, TroopTypes types) => entity.HasAllTypes(types);
        public bool HasAnyType(CharacterObject entity, TroopTypes types) => entity.HasAnyType(types);
        public string FormattedDetails(CharacterObject entity) => entity.FormattedDetails();
    }
}