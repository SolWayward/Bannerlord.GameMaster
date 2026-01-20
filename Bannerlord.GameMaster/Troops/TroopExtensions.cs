using System;
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
        
        // Culture-Based Categories (33554432-4294967296)
        Empire = 33554432,         // 2^25 - Culture: Empire
        Vlandia = 67108864,        // 2^26 - Culture: Vlandia
        Sturgia = 134217728,       // 2^27 - Culture: Sturgia
        Aserai = 268435456,        // 2^28 - Culture: Aserai
        Khuzait = 536870912,       // 2^29 - Culture: Khuzait
        Battania = 1073741824,     // 2^30 - Culture: Battania
        Nord = 2147483648,         // 2^31 - Culture: Nord (Warsails DLC - optional)
        Bandit = 4294967296,       // 2^32 - Culture: Bandit (special culture)
        
        // Gender Categories (8589934592-17179869184)
        Female = 8589934592,       // 2^33 - Female troops
        Male = 17179869184,        // 2^34 - Male troops
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
            string stringId = character.StringId;
            
            // Detect troop line by StringId patterns
            if (stringId.IndexOf("noble", StringComparison.OrdinalIgnoreCase) >= 0 ||
                stringId.IndexOf("knight", StringComparison.OrdinalIgnoreCase) >= 0 ||
                stringId.IndexOf("druzhnik", StringComparison.OrdinalIgnoreCase) >= 0 ||
                stringId.IndexOf("cataphract", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                types |= TroopTypes.Noble;
            }
            else if (stringId.IndexOf("militia", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                types |= TroopTypes.Militia;
            }
            else if (stringId.IndexOf("mercenary", StringComparison.OrdinalIgnoreCase) >= 0 &&
                     stringId.IndexOf("leader", StringComparison.OrdinalIgnoreCase) < 0)
            {
                types |= TroopTypes.Mercenary;
            }
            else if (stringId.IndexOf("caravan_guard", StringComparison.OrdinalIgnoreCase) >= 0 ||
                     stringId.IndexOf("caravan_master", StringComparison.OrdinalIgnoreCase) >= 0 ||
                     stringId.IndexOf("armed_trader", StringComparison.OrdinalIgnoreCase) >= 0 ||
                     stringId.IndexOf("sea_trader", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                types |= TroopTypes.Caravan;
            }
            else if ((stringId.IndexOf("villager", StringComparison.OrdinalIgnoreCase) >= 0 ||
                      stringId.IndexOf("village_woman", StringComparison.OrdinalIgnoreCase) >= 0 ||
                      stringId.IndexOf("townsman", StringComparison.OrdinalIgnoreCase) >= 0 ||
                      stringId.IndexOf("townswoman", StringComparison.OrdinalIgnoreCase) >= 0 ||
                      (stringId.IndexOf("fighter", StringComparison.OrdinalIgnoreCase) >= 0 && character.GetBattleTier() == 0)) &&
                     character.GetBattleTier() == 0 && character.Level == 1)
            {
                types |= TroopTypes.Peasant;
            }
            else if (character.GetBattleTier() >= 2 &&
                     (stringId.IndexOf("eleftheroi", StringComparison.OrdinalIgnoreCase) >= 0 ||
                      stringId.IndexOf("brotherhood_of_woods", StringComparison.OrdinalIgnoreCase) >= 0 ||
                      stringId.IndexOf("hidden_hand", StringComparison.OrdinalIgnoreCase) >= 0 ||
                      stringId.IndexOf("jawwal", StringComparison.OrdinalIgnoreCase) >= 0 ||
                      stringId.IndexOf("lake_rats", StringComparison.OrdinalIgnoreCase) >= 0 ||
                      stringId.IndexOf("forest_people", StringComparison.OrdinalIgnoreCase) >= 0 ||
                      stringId.IndexOf("karakhuzait", StringComparison.OrdinalIgnoreCase) >= 0))
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
                string cultureId = character.Culture.StringId;
                
                if (cultureId.IndexOf("empire", StringComparison.OrdinalIgnoreCase) >= 0)
                    types |= TroopTypes.Empire;
                else if (cultureId.IndexOf("vlandia", StringComparison.OrdinalIgnoreCase) >= 0)
                    types |= TroopTypes.Vlandia;
                else if (cultureId.IndexOf("sturgia", StringComparison.OrdinalIgnoreCase) >= 0)
                    types |= TroopTypes.Sturgia;
                else if (cultureId.IndexOf("aserai", StringComparison.OrdinalIgnoreCase) >= 0)
                    types |= TroopTypes.Aserai;
                else if (cultureId.IndexOf("khuzait", StringComparison.OrdinalIgnoreCase) >= 0)
                    types |= TroopTypes.Khuzait;
                else if (cultureId.IndexOf("battania", StringComparison.OrdinalIgnoreCase) >= 0)
                    types |= TroopTypes.Battania;
                else if (cultureId.IndexOf("nord", StringComparison.OrdinalIgnoreCase) >= 0)
                    types |= TroopTypes.Nord;
                else if (cultureId.IndexOf("bandit", StringComparison.OrdinalIgnoreCase) >= 0 ||
                         cultureId.IndexOf("looter", StringComparison.OrdinalIgnoreCase) >= 0)
                    types |= TroopTypes.Bandit;
            }

            // Gender Categories
            if (character.IsFemale)
                types |= TroopTypes.Female;
            else
                types |= TroopTypes.Male;

            return types;
        }

        /// <summary>
        /// Checks if character has ALL specified flags (AND logic)
        /// </summary>
        public static bool HasAllTypes(this CharacterObject character, TroopTypes types)
        {
            if (types == TroopTypes.None) return true;
            TroopTypes troopTypes = character.GetTroopTypes();
            return (troopTypes & types) == types;
        }

        /// <summary>
        /// Checks if character has ANY of the specified flags (OR logic)
        /// </summary>
        public static bool HasAnyType(this CharacterObject character, TroopTypes types)
        {
            if (types == TroopTypes.None) return true;
            TroopTypes troopTypes = character.GetTroopTypes();
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

            string stringId = character.StringId;

            // 1. Templates/Equipment Sets - Non-playable character templates
            if (stringId.IndexOf("template", StringComparison.OrdinalIgnoreCase) >= 0 ||
                stringId.IndexOf("_equipment", StringComparison.OrdinalIgnoreCase) >= 0 ||
                stringId.IndexOf("_bat_", StringComparison.OrdinalIgnoreCase) >= 0 ||
                stringId.IndexOf("_civ_", StringComparison.OrdinalIgnoreCase) >= 0 ||
                stringId.IndexOf("_noncom_", StringComparison.OrdinalIgnoreCase) >= 0)
                return false;

            // 2. Town Service NPCs - Non-combat town workers and merchants
            // These provide services in towns but never participate in combat
            if (stringId.IndexOf("armorer", StringComparison.OrdinalIgnoreCase) >= 0 ||
                stringId.IndexOf("barber", StringComparison.OrdinalIgnoreCase) >= 0 ||
                stringId.IndexOf("blacksmith", StringComparison.OrdinalIgnoreCase) >= 0 ||
                stringId.IndexOf("beggar", StringComparison.OrdinalIgnoreCase) >= 0 ||
                stringId.IndexOf("merchant", StringComparison.OrdinalIgnoreCase) >= 0 ||
                stringId.IndexOf("shop_keeper", StringComparison.OrdinalIgnoreCase) >= 0 ||
                stringId.IndexOf("shop_worker", StringComparison.OrdinalIgnoreCase) >= 0 ||
                stringId.IndexOf("weaponsmith", StringComparison.OrdinalIgnoreCase) >= 0 ||
                stringId.IndexOf("shipwright", StringComparison.OrdinalIgnoreCase) >= 0 ||
                stringId.IndexOf("ransom_broker", StringComparison.OrdinalIgnoreCase) >= 0 ||
                stringId.IndexOf("musician", StringComparison.OrdinalIgnoreCase) >= 0)
                return false;

            // 3. Tavern NPCs - Non-combat tavern workers
            if (stringId.IndexOf("tavern_wench", StringComparison.OrdinalIgnoreCase) >= 0 ||
                stringId.IndexOf("tavernkeeper", StringComparison.OrdinalIgnoreCase) >= 0 ||
                stringId.IndexOf("barmaid", StringComparison.OrdinalIgnoreCase) >= 0 ||
                stringId.IndexOf("tavern_gamehost", StringComparison.OrdinalIgnoreCase) >= 0 ||
                stringId.IndexOf("tavern_guard", StringComparison.OrdinalIgnoreCase) >= 0)
                return false;

            // 4. Horse-related NPCs - Stable workers and horse merchants
            if (stringId.IndexOf("horse_merchant", StringComparison.OrdinalIgnoreCase) >= 0 ||
                stringId.IndexOf("horse_trader", StringComparison.OrdinalIgnoreCase) >= 0)
                return false;

            // 5. Notables - Notable NPCs with "notary" suffix (preacher_notary, merchant_notary, etc.)
            if (stringId.IndexOf("notary", StringComparison.OrdinalIgnoreCase) >= 0)
                return false;

            // 6. Wanderers/Companions - Recruitable NPCs that should be excluded
            // These start with specific prefixes identifying them as special characters
            if (stringId.StartsWith("spc_notable_", StringComparison.OrdinalIgnoreCase) ||
                stringId.StartsWith("spc_wanderer_", StringComparison.OrdinalIgnoreCase) ||
                stringId.StartsWith("npc_wanderer", StringComparison.OrdinalIgnoreCase) ||
                stringId.StartsWith("npc_companion", StringComparison.OrdinalIgnoreCase) ||
                stringId.StartsWith("npc_armed_wanderer", StringComparison.OrdinalIgnoreCase) ||
                stringId.StartsWith("npc_artisan", StringComparison.OrdinalIgnoreCase) ||
                stringId.StartsWith("npc_gang_leader", StringComparison.OrdinalIgnoreCase) ||
                stringId.StartsWith("npc_merchant", StringComparison.OrdinalIgnoreCase) ||
                stringId.StartsWith("npc_preacher", StringComparison.OrdinalIgnoreCase) ||
                stringId.StartsWith("npc_gentry", StringComparison.OrdinalIgnoreCase) ||
                stringId.StartsWith("npc_poor_wanderer", StringComparison.OrdinalIgnoreCase))
                return false;

            // 7. Entertainment/Event NPCs - Dancers, tournament masters, game hosts
            // These are Tier 0, Level 1 non-combat NPCs
            if (stringId.IndexOf("dancer", StringComparison.OrdinalIgnoreCase) >= 0 ||
                stringId.IndexOf("tournament_master", StringComparison.OrdinalIgnoreCase) >= 0 ||
                stringId.IndexOf("taverngamehost", StringComparison.OrdinalIgnoreCase) >= 0)  // Note: no underscore in ID
                return false;

            // 8. Special Character NPCs - Minor faction leaders, headmen, etc.
            // These are Tier 0, Level 1 quest/story NPCs, NOT the actual combat troops
            // Actual minor faction troops (tier_1/2/3) are properly included
            if (stringId.StartsWith("spc_", StringComparison.OrdinalIgnoreCase) &&
                (stringId.IndexOf("_leader_", StringComparison.OrdinalIgnoreCase) >= 0 ||
                 stringId.IndexOf("_headman_", StringComparison.OrdinalIgnoreCase) >= 0 ||
                 stringId.IndexOf("_gangleader_", StringComparison.OrdinalIgnoreCase) >= 0 ||
                 stringId.IndexOf("_artisan_", StringComparison.OrdinalIgnoreCase) >= 0 ||
                 stringId.IndexOf("_rural_notable_", StringComparison.OrdinalIgnoreCase) >= 0 ||
                 stringId.IndexOf("_e3_character_", StringComparison.OrdinalIgnoreCase) >= 0))
                return false;

            // 9. Children/Teens/Infants - Non-combat young characters
            if (stringId.IndexOf("child", StringComparison.OrdinalIgnoreCase) >= 0 ||
                stringId.IndexOf("infant", StringComparison.OrdinalIgnoreCase) >= 0 ||
                stringId.IndexOf("teenager", StringComparison.OrdinalIgnoreCase) >= 0)
                return false;

            // 10. Practice/Training Dummies - Arena and training targets
            if (stringId.IndexOf("_dummy", StringComparison.OrdinalIgnoreCase) >= 0 ||
                stringId.IndexOf("practice_stage", StringComparison.OrdinalIgnoreCase) >= 0 ||
                stringId.IndexOf("weapon_practice", StringComparison.OrdinalIgnoreCase) >= 0 ||
                stringId.IndexOf("gear_practice", StringComparison.OrdinalIgnoreCase) >= 0)
                return false;

            // 11. Special/System Characters - Tutorial, cutscene, and test characters
            if (stringId.IndexOf("cutscene_", StringComparison.OrdinalIgnoreCase) >= 0 ||
                stringId.IndexOf("tutorial_", StringComparison.OrdinalIgnoreCase) >= 0 ||
                stringId.IndexOf("duel_style_", StringComparison.OrdinalIgnoreCase) >= 0 ||
                stringId.IndexOf("player_char_creation_", StringComparison.OrdinalIgnoreCase) >= 0 ||
                stringId.IndexOf("disguise_", StringComparison.OrdinalIgnoreCase) >= 0 ||
                stringId.StartsWith("test", StringComparison.OrdinalIgnoreCase) ||
                stringId.IndexOf("crazy_man", StringComparison.OrdinalIgnoreCase) >= 0 ||
                stringId.IndexOf("unarmed_ai", StringComparison.OrdinalIgnoreCase) >= 0 ||
                stringId.IndexOf("borrowed_troop", StringComparison.OrdinalIgnoreCase) >= 0 ||
                stringId.IndexOf("neutral_lord", StringComparison.OrdinalIgnoreCase) >= 0 ||
                stringId.IndexOf("stealth_character", StringComparison.OrdinalIgnoreCase) >= 0)
                return false;

            // 12. Townsfolk Civilians - Non-combat town residents (NOT villagers!)
            // townsman/townswoman are passive civilians, unlike villagers who fight in village raids
            if (stringId.IndexOf("townsman", StringComparison.OrdinalIgnoreCase) >= 0 ||
                stringId.IndexOf("townswoman", StringComparison.OrdinalIgnoreCase) >= 0)
                return false;

            // 13. Tier 0 Level 1 Filter - Catch-all for non-combat NPCs
            // Most Tier 0 Level 1 characters are non-combatants (dancers, refugees, quest NPCs, etc.)
            // EXCEPT for known combat troops like villagers and fighters
            if (character.GetBattleTier() == 0 && character.Level == 1)
            {
                // Check for known Tier 0 Level 1 COMBAT troops that should NOT be excluded
                if (stringId.IndexOf("villager", StringComparison.OrdinalIgnoreCase) >= 0 ||           // villager_* - fight in village raids
                    stringId.IndexOf("village_woman", StringComparison.OrdinalIgnoreCase) >= 0 ||      // village_woman_* - fight in village raids
                    stringId.IndexOf("fighter", StringComparison.OrdinalIgnoreCase) >= 0 ||            // fighter_* - basic recruitable troops
                    stringId.IndexOf("caravan_guard", StringComparison.OrdinalIgnoreCase) >= 0 ||      // caravan_guard_* - fight when attacking caravans
                    stringId.IndexOf("armed_trader", StringComparison.OrdinalIgnoreCase) >= 0 ||       // armed_trader_* - combat-capable traders
                    stringId.IndexOf("looter", StringComparison.OrdinalIgnoreCase) >= 0 ||             // looter_* - bandit combat troops
                    stringId.IndexOf("sea_raider_recruit", StringComparison.OrdinalIgnoreCase) >= 0)   // sea_raider_recruit - combat troop
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

            string stringId = character.StringId;
            TroopTypes types = character.GetTroopTypes();

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
            string gender = character.IsFemale ? "Female" : "Male";
            return $"{character.StringId}\t{character.Name}\tGender: {gender}\t[{category}]\tTier: {character.GetBattleTier()}\tLevel: {character.Level}\tCulture: {cultureName}\tFormation: {character.DefaultFormationClass}";
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
                
            Equipment equipment = character.FirstBattleEquipment;
            for (int i = 0; i < 12; i++) // Equipment slot count
            {
                EquipmentElement equipmentElement = equipment[i];
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
                
            Equipment equipment = character.FirstBattleEquipment;
            for (int i = 0; i < 12; i++)
            {
                EquipmentElement equipmentElement = equipment[i];
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
                
            Equipment equipment = character.FirstBattleEquipment;
            for (int i = 0; i < 12; i++)
            {
                EquipmentElement equipmentElement = equipment[i];
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
