using System;
using TaleWorlds.CampaignSystem;
using Bannerlord.GameMaster.Troops;

namespace Bannerlord.GameMaster.Characters
{
    /// <summary>
    /// Extension methods for CharacterObject providing type categorization for ALL character types.
    /// Unlike TroopExtensions which excludes heroes, CharacterExtensions works for EVERYTHING:
    /// heroes, troops, templates, NPCs, etc.
    /// </summary>
    public static class CharacterExtensions
    {
        #region Type Checking

        /// <summary>
        /// Gets all CharacterTypes flags for this CharacterObject.
        /// Works for ALL CharacterObjects including heroes, troops, templates, NPCs.
        /// </summary>
        public static CharacterTypes GetCharacterTypes(this CharacterObject character)
        {
            CharacterTypes types = CharacterTypes.None;
            string stringId = character.StringId;

            // MARK: Character Classification
            if (character.IsHero)
            {
                types |= CharacterTypes.IsHero;
                
                // Hero-specific flags
                Hero hero = character.HeroObject;
                if (hero != null)
                {
                    if (hero.IsLord)
                        types |= CharacterTypes.IsLord;
                    if (hero.IsWanderer)
                        types |= CharacterTypes.IsWanderer;
                    if (hero.IsNotable)
                        types |= CharacterTypes.IsNotable;
                    if (hero.IsChild)
                        types |= CharacterTypes.IsChild;
                }
            }
            else
            {
                // Not a hero - determine if troop, template, or NPC
                if (character.IsTemplate())
                    types |= CharacterTypes.IsTemplate;
                else if (character.IsNonCombatNPC())
                    types |= CharacterTypes.IsNPC;
                else if (character.IsActualTroop())
                    types |= CharacterTypes.IsTroop;
                else
                    types |= CharacterTypes.IsNPC; // Default to NPC for uncategorized non-heroes
            }

            // Check for child patterns in stringId (works for both heroes and templates)
            if (stringId.IndexOf("child", StringComparison.OrdinalIgnoreCase) >= 0 ||
                stringId.IndexOf("infant", StringComparison.OrdinalIgnoreCase) >= 0 ||
                stringId.IndexOf("teenager", StringComparison.OrdinalIgnoreCase) >= 0)
                types |= CharacterTypes.IsChild;

            // MARK: Gender
            if (character.IsFemale)
                types |= CharacterTypes.IsFemale;
            else
                types |= CharacterTypes.IsMale;

            // MARK: Character State (BLGM-created check)
            if (stringId.StartsWith("blgm_", StringComparison.OrdinalIgnoreCase))
                types |= CharacterTypes.IsBlgmCreated;
            else
                types |= CharacterTypes.IsOriginalCharacter;

            // MARK: Tier-Based Categories (for non-hero characters primarily)
            if (!character.IsHero)
            {
                int tier = character.GetBattleTier();
                if (tier == 0)
                    types |= CharacterTypes.Tier0;
                else if (tier == 1)
                    types |= CharacterTypes.Tier1;
                else if (tier == 2)
                    types |= CharacterTypes.Tier2;
                else if (tier == 3)
                    types |= CharacterTypes.Tier3;
                else if (tier == 4)
                    types |= CharacterTypes.Tier4;
                else if (tier == 5)
                    types |= CharacterTypes.Tier5;
                else if (tier >= 6)
                    types |= CharacterTypes.Tier6Plus;
            }

            // MARK: Culture-Based Categories
            if (character.Culture != null)
            {
                string cultureId = character.Culture.StringId;

                if (cultureId.IndexOf("empire", StringComparison.OrdinalIgnoreCase) >= 0)
                    types |= CharacterTypes.Empire;
                else if (cultureId.IndexOf("vlandia", StringComparison.OrdinalIgnoreCase) >= 0)
                    types |= CharacterTypes.Vlandia;
                else if (cultureId.IndexOf("sturgia", StringComparison.OrdinalIgnoreCase) >= 0)
                    types |= CharacterTypes.Sturgia;
                else if (cultureId.IndexOf("aserai", StringComparison.OrdinalIgnoreCase) >= 0)
                    types |= CharacterTypes.Aserai;
                else if (cultureId.IndexOf("khuzait", StringComparison.OrdinalIgnoreCase) >= 0)
                    types |= CharacterTypes.Khuzait;
                else if (cultureId.IndexOf("battania", StringComparison.OrdinalIgnoreCase) >= 0)
                    types |= CharacterTypes.Battania;
                else if (cultureId.IndexOf("nord", StringComparison.OrdinalIgnoreCase) >= 0)
                    types |= CharacterTypes.Nord;
                else if (cultureId.IndexOf("bandit", StringComparison.OrdinalIgnoreCase) >= 0 ||
                         cultureId.IndexOf("looter", StringComparison.OrdinalIgnoreCase) >= 0 ||
                         cultureId.IndexOf("desert_bandits", StringComparison.OrdinalIgnoreCase) >= 0 ||
                         cultureId.IndexOf("forest_bandits", StringComparison.OrdinalIgnoreCase) >= 0 ||
                         cultureId.IndexOf("mountain_bandits", StringComparison.OrdinalIgnoreCase) >= 0 ||
                         cultureId.IndexOf("steppe_bandits", StringComparison.OrdinalIgnoreCase) >= 0 ||
                         cultureId.IndexOf("sea_raiders", StringComparison.OrdinalIgnoreCase) >= 0 ||
                         cultureId.IndexOf("southern_pirates", StringComparison.OrdinalIgnoreCase) >= 0)
                    types |= CharacterTypes.Bandit;
            }

            return types;
        }

        /// <summary>
        /// Checks if character has ALL specified CharacterTypes flags (AND logic)
        /// </summary>
        public static bool HasAllCharacterTypes(this CharacterObject character, CharacterTypes types)
        {
            if (types == CharacterTypes.None) return true;
            CharacterTypes characterTypes = character.GetCharacterTypes();
            return (characterTypes & types) == types;
        }

        /// <summary>
        /// Checks if character has ANY of the specified CharacterTypes flags (OR logic)
        /// </summary>
        public static bool HasAnyCharacterType(this CharacterObject character, CharacterTypes types)
        {
            if (types == CharacterTypes.None) return true;
            CharacterTypes characterTypes = character.GetCharacterTypes();
            return (characterTypes & types) != CharacterTypes.None;
        }

        #endregion

        #region Classification

        /// <summary>
        /// Gets the primary classification for this CharacterObject.
        /// Returns: "Hero", "Troop", "Template", "NPC", or "Unknown"
        /// </summary>
        public static string GetCharacterClassification(this CharacterObject character)
        {
            if (character.IsHero)
            {
                Hero hero = character.HeroObject;
                if (hero != null)
                {
                    if (hero.IsLord)
                        return "Lord";
                    if (hero.IsWanderer)
                        return "Wanderer";
                    if (hero.IsNotable)
                        return "Notable";
                    if (hero.IsChild)
                        return "Child";
                }
                return "Hero";
            }

            if (character.IsTemplate())
                return "Template";

            if (character.IsNonCombatNPC())
                return "NPC";

            if (character.IsActualTroop())
                return "Troop";

            return "Unknown";
        }

        /// <summary>
        /// Returns true if this is a template/equipment character (not spawnable).
        /// Templates include equipment sets and character templates used for spawning.
        /// </summary>
        public static bool IsTemplate(this CharacterObject character)
        {
            if (character.IsHero)
                return false;

            string stringId = character.StringId;

            // Template patterns
            if (stringId.IndexOf("template", StringComparison.OrdinalIgnoreCase) >= 0 ||
                stringId.IndexOf("_equipment", StringComparison.OrdinalIgnoreCase) >= 0)
                return true;

            // Additional template patterns
            if (stringId.IndexOf("_bat_", StringComparison.OrdinalIgnoreCase) >= 0 ||
                stringId.IndexOf("_civ_", StringComparison.OrdinalIgnoreCase) >= 0 ||
                stringId.IndexOf("_noncom_", StringComparison.OrdinalIgnoreCase) >= 0)
                return true;

            return false;
        }

        /// <summary>
        /// Returns true if this is a non-combat NPC (merchant, barber, tavernkeeper, etc.)
        /// These are characters that exist in settlements but don't participate in combat.
        /// </summary>
        public static bool IsNonCombatNPC(this CharacterObject character)
        {
            if (character.IsHero)
                return false;

            string stringId = character.StringId;

            // Town Service NPCs
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
                return true;

            // Tavern NPCs
            if (stringId.IndexOf("tavern_wench", StringComparison.OrdinalIgnoreCase) >= 0 ||
                stringId.IndexOf("tavernkeeper", StringComparison.OrdinalIgnoreCase) >= 0 ||
                stringId.IndexOf("barmaid", StringComparison.OrdinalIgnoreCase) >= 0 ||
                stringId.IndexOf("tavern_gamehost", StringComparison.OrdinalIgnoreCase) >= 0 ||
                stringId.IndexOf("tavern_guard", StringComparison.OrdinalIgnoreCase) >= 0)
                return true;

            // Horse-related NPCs
            if (stringId.IndexOf("horse_merchant", StringComparison.OrdinalIgnoreCase) >= 0 ||
                stringId.IndexOf("horse_trader", StringComparison.OrdinalIgnoreCase) >= 0)
                return true;

            // Notables with notary suffix
            if (stringId.IndexOf("notary", StringComparison.OrdinalIgnoreCase) >= 0)
                return true;

            // Special NPCs (wanderers/companions before recruitment)
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
                return true;

            // Entertainment/Event NPCs
            if (stringId.IndexOf("dancer", StringComparison.OrdinalIgnoreCase) >= 0 ||
                stringId.IndexOf("tournament_master", StringComparison.OrdinalIgnoreCase) >= 0 ||
                stringId.IndexOf("taverngamehost", StringComparison.OrdinalIgnoreCase) >= 0)
                return true;

            // Special character NPCs
            if (stringId.StartsWith("spc_", StringComparison.OrdinalIgnoreCase) &&
                (stringId.IndexOf("_leader_", StringComparison.OrdinalIgnoreCase) >= 0 ||
                 stringId.IndexOf("_headman_", StringComparison.OrdinalIgnoreCase) >= 0 ||
                 stringId.IndexOf("_gangleader_", StringComparison.OrdinalIgnoreCase) >= 0 ||
                 stringId.IndexOf("_artisan_", StringComparison.OrdinalIgnoreCase) >= 0 ||
                 stringId.IndexOf("_rural_notable_", StringComparison.OrdinalIgnoreCase) >= 0 ||
                 stringId.IndexOf("_e3_character_", StringComparison.OrdinalIgnoreCase) >= 0))
                return true;

            // Children/Teens/Infants (non-combat)
            if (stringId.IndexOf("child", StringComparison.OrdinalIgnoreCase) >= 0 ||
                stringId.IndexOf("infant", StringComparison.OrdinalIgnoreCase) >= 0 ||
                stringId.IndexOf("teenager", StringComparison.OrdinalIgnoreCase) >= 0)
                return true;

            // Townsfolk civilians
            if (stringId.IndexOf("townsman", StringComparison.OrdinalIgnoreCase) >= 0 ||
                stringId.IndexOf("townswoman", StringComparison.OrdinalIgnoreCase) >= 0)
                return true;

            // Practice/Training characters
            if (stringId.IndexOf("_dummy", StringComparison.OrdinalIgnoreCase) >= 0 ||
                stringId.IndexOf("practice_stage", StringComparison.OrdinalIgnoreCase) >= 0 ||
                stringId.IndexOf("weapon_practice", StringComparison.OrdinalIgnoreCase) >= 0 ||
                stringId.IndexOf("gear_practice", StringComparison.OrdinalIgnoreCase) >= 0)
                return true;

            // Special/System characters
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
                return true;

            return false;
        }

        #endregion

        #region Formatting

        /// <summary>
        /// Returns a formatted string containing the character's details.
        /// Works for all character types (heroes, troops, templates, NPCs).
        /// </summary>
        public static string FormattedCharacterDetails(this CharacterObject character)
        {
            string cultureName = character.Culture?.Name?.ToString() ?? "None";
            string classification = character.GetCharacterClassification();
            string gender = character.IsFemale ? "Female" : "Male";
            int tier = character.IsHero ? -1 : character.GetBattleTier();
            string tierDisplay = tier >= 0 ? tier.ToString() : "N/A";

            return $"{character.StringId}\t{character.Name}\tGender: {gender}\t[{classification}]\tTier: {tierDisplay}\tLevel: {character.Level}\tCulture: {cultureName}";
        }

        #endregion
    }
}
