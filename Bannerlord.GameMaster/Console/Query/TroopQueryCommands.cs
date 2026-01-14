using Bannerlord.GameMaster.Troops;
using Bannerlord.GameMaster.Console.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Library;

namespace Bannerlord.GameMaster.Console.Query
{
    [CommandLineFunctionality.CommandLineArgumentFunction("query", "gm")]
    public static class TroopQueryCommands
    {
        /// <summary>
        /// Parse command arguments into search filter and troop type flags
        /// </summary>
        private static (string query, TroopTypes types, int tier, string sortBy, bool sortDesc) ParseArguments(List<string> args)
        {
            if (args == null || args.Count == 0)
                return ("", TroopTypes.None, -1, "id", false);

            var typeKeywords = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                // Formation types
                "infantry", "ranged", "cavalry", "horsearcher", "mounted",
                // Troop line types
                "regular", "noble", "militia", "mercenary", "caravan", "peasant", "minorfaction",
                // Equipment types
                "shield", "twohanded", "2h", "polearm", "bow", "crossbow", "throwing", "throwingweapon",
                // Tier types (also as keywords)
                "tier0", "tier1", "tier2", "tier3", "tier4", "tier5", "tier6", "tier6plus",
                // Cultures
                "empire", "vlandia", "sturgia", "aserai", "khuzait", "battania", "nord", "bandit",
                // Gender types
                "female", "male"
            };

            var tierKeywords = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "tier0", "tier1", "tier2", "tier3", "tier4", "tier5", "tier6", "tier6plus"
            };

            List<string> searchTerms = new();
            List<string> typeTerms = new();
            int tier = -1;
            string sortBy = "id";
            bool sortDesc = false;

            foreach (var arg in args)
            {
                // Check for sort parameters
                if (arg.StartsWith("sort:", StringComparison.OrdinalIgnoreCase))
                {
                    ParseSortParameter(arg, ref sortBy, ref sortDesc);
                }
                // Check for tier keywords
                else if (tierKeywords.Contains(arg))
                {
                    int parsedTier = ParseTierKeyword(arg);
                    if (parsedTier >= 0)
                    {
                        tier = parsedTier;
                        // Also add to type terms for type flag filtering
                        typeTerms.Add(arg);
                    }
                }
                // Check for type keywords
                else if (typeKeywords.Contains(arg))
                {
                    typeTerms.Add(arg);
                }
                // Otherwise treat as search term
                else
                {
                    searchTerms.Add(arg);
                }
            }

            string query = string.Join(" ", searchTerms).Trim();
            TroopTypes types = TroopQueries.ParseTroopTypes(typeTerms);

            return (query, types, tier, sortBy, sortDesc);
        }

        /// <summary>
        /// Parse sort parameter (e.g., "sort:name:desc" or "sort:tier")
        /// </summary>
        private static void ParseSortParameter(string sortParam, ref string sortBy, ref bool sortDesc)
        {
            var parts = sortParam.Split(':');
            if (parts.Length >= 2)
            {
                sortBy = parts[1].ToLower();
            }
            if (parts.Length >= 3)
            {
                sortDesc = parts[2].Equals("desc", StringComparison.OrdinalIgnoreCase);
            }
        }

        /// <summary>
        /// Parse tier keyword into tier number
        /// </summary>
        private static int ParseTierKeyword(string tierKeyword)
        {
            return tierKeyword.ToLower() switch
            {
                "tier0" => 0,
                "tier1" => 1,
                "tier2" => 2,
                "tier3" => 3,
                "tier4" => 4,
                "tier5" => 5,
                "tier6" => 6,
                "tier6plus" => 6,
                _ => -1
            };
        }

        /// <summary>
        /// Helper to build a readable criteria string
        /// </summary>
        private static string BuildCriteriaString(string query, TroopTypes types, int tier, string sortBy, bool sortDesc)
        {
            List<string> parts = new();

            if (!string.IsNullOrEmpty(query))
                parts.Add($"search: '{query}'");

            if (types != TroopTypes.None)
            {
                var typeList = Enum.GetValues(typeof(TroopTypes))
                    .Cast<TroopTypes>()
                    .Where(t => t != TroopTypes.None && types.HasFlag(t))
                    .Select(t => t.ToString().ToLower());
                parts.Add($"types: {string.Join(", ", typeList)}");
            }

            if (tier >= 0)
                parts.Add($"tier: {tier}");

            if (!string.IsNullOrEmpty(sortBy) && sortBy != "id")
                parts.Add($"sort: {sortBy}{(sortDesc ? " (desc)" : " (asc)")}");

            return parts.Count > 0 ? string.Join(", ", parts) : "all troops";
        }

        /// <summary>
        /// Unified troop listing command with AND logic
        /// Usage: gm.query.troop [search terms] [type keywords] [tier] [sort parameters]
        /// Example: gm.query.troop imperial infantry
        /// Example: gm.query.troop aserai cavalry tier3
        /// Example: gm.query.troop shield infantry sort:tier:desc
        /// Example: gm.query.troop battania ranged bow
        /// Example: gm.query.troop noble empire tier5
        /// </summary>
        [CommandLineFunctionality.CommandLineArgumentFunction("troop", "gm.query")]
        public static string QueryTroops(List<string> args)
        {
            return Cmd.Run(args, () =>
            {
                if (!CommandBase.ValidateCampaignState(out string error))
                    return error;

                var (query, types, tier, sortBy, sortDesc) = ParseArguments(args);

                List<CharacterObject> matchedTroops = TroopQueries.QueryTroops(
                    query, types, matchAll: true, tier, sortBy, sortDesc);

                string criteriaDesc = BuildCriteriaString(query, types, tier, sortBy, sortDesc);

                if (matchedTroops.Count == 0)
                {
                    return $"Found 0 troop(s) matching {criteriaDesc}\n" +
                           "Usage: gm.query.troop [search] [type keywords] [tier] [sort]\n" +
                           "Type keywords: infantry, ranged, cavalry, horsearcher, shield, bow, crossbow, regular, noble, militia, mercenary, caravan, bandit, female, male, etc.\n" +
                           "Tier keywords: tier0, tier1, tier2, tier3, tier4, tier5, tier6, tier6plus\n" +
                           "Sort: sort:name, sort:tier, sort:level, sort:culture, sort:<type> (add :desc for descending)\n" +
                           "Example: gm.query.troop imperial infantry tier2 sort:name\n" +
                           "Example: gm.query.troop female cavalry (find female cavalry troops)\n" +
                           "Note: Non-troops (heroes, NPCs, children, templates, etc.) are automatically excluded.\n";
                }

                return $"Found {matchedTroops.Count} troop(s) matching {criteriaDesc}:\n" +
                       $"{TroopQueries.GetFormattedDetails(matchedTroops)}";
            });
        }

        /// <summary>
        /// Find troops matching ANY of the specified types (OR logic)
        /// Usage: gm.query.troop_any [search terms] [type keywords] [tier] [sort parameters]
        /// Example: gm.query.troop_any cavalry ranged (cavalry OR ranged)
        /// Example: gm.query.troop_any bow crossbow tier4
        /// Example: gm.query.troop_any empire vlandia infantry
        /// </summary>
        [CommandLineFunctionality.CommandLineArgumentFunction("troop_any", "gm.query")]
        public static string QueryTroopsAny(List<string> args)
        {
            return Cmd.Run(args, () =>
            {
                if (!CommandBase.ValidateCampaignState(out string error))
                    return error;

                var (query, types, tier, sortBy, sortDesc) = ParseArguments(args);

                List<CharacterObject> matchedTroops = TroopQueries.QueryTroops(
                    query, types, matchAll: false, tier, sortBy, sortDesc);

                string criteriaDesc = BuildCriteriaString(query, types, tier, sortBy, sortDesc);

                if (matchedTroops.Count == 0)
                {
                    return $"Found 0 troop(s) matching ANY of {criteriaDesc}\n" +
                           "Usage: gm.query.troop_any [search] [type keywords] [tier] [sort]\n" +
                           "Example: gm.query.troop_any cavalry ranged tier3 sort:tier\n" +
                           "Note: Non-troops (heroes, NPCs, children, templates, etc.) are automatically excluded.\n";
                }

                return $"Found {matchedTroops.Count} troop(s) matching ANY of {criteriaDesc}:\n" +
                       $"{TroopQueries.GetFormattedDetails(matchedTroops)}";
            });
        }

        /// <summary>
        /// Get detailed info about a specific troop by ID
        /// Usage: gm.query.troop_info &lt;troopId&gt;
        /// Example: gm.query.troop_info imperial_legionary
        /// </summary>
        [CommandLineFunctionality.CommandLineArgumentFunction("troop_info", "gm.query")]
        public static string QueryTroopInfo(List<string> args)
        {
            return Cmd.Run(args, () =>
            {
                if (!CommandBase.ValidateCampaignState(out string error))
                    return error;

                if (args == null || args.Count == 0)
                    return "Error: Please provide a troop ID.\nUsage: gm.query.troop_info <troopId>\n";

                string troopId = args[0];
                CharacterObject troop = TroopQueries.GetTroopById(troopId);

                if (troop == null)
                    return $"Error: Troop with ID '{troopId}' not found.\n";

                if (troop.IsHero)
                    return $"Error: '{troopId}' is a hero/lord, not a troop. Use gm.query.hero_info instead.\n";

                if (!troop.IsActualTroop())
                    return $"Error: '{troopId}' is not an actual troop (may be NPC, child, template, etc.).\n";

                var types = troop.GetTroopTypes();
                string cultureName = troop.Culture?.Name?.ToString() ?? "None";

                // Build equipment list
                string equipmentInfo = "";
                if (troop.FirstBattleEquipment != null)
                {
                    var equipment = troop.FirstBattleEquipment;
                    List<string> items = new List<string>();
                    for (int i = 0; i < 12; i++)
                    {
                        var item = equipment[i].Item;
                        if (item != null)
                        {
                            items.Add(item.Name.ToString());
                        }
                    }
                    equipmentInfo = items.Count > 0 
                        ? "Equipment: " + string.Join(", ", items) + "\n"
                        : "Equipment: None\n";
                }
                else
                {
                    equipmentInfo = "Equipment: None\n";
                }

                // Build upgrade paths
                string upgradeInfo = "";
                if (troop.UpgradeTargets != null && troop.UpgradeTargets.Length > 0)
                {
                    var upgrades = troop.UpgradeTargets.Select(u => u.Name.ToString());
                    upgradeInfo = "Upgrades: " + string.Join(", ", upgrades) + "\n";
                }
                else
                {
                    upgradeInfo = "Upgrades: None\n";
                }

                string category = troop.GetTroopCategory();

                return $"Troop Information:\n" +
                       $"ID: {troop.StringId}\n" +
                       $"Name: {troop.Name}\n" +
                       $"Category: {category}\n" +
                       $"Tier: {troop.GetBattleTier()}\n" +
                       $"Level: {troop.Level}\n" +
                       $"Culture: {cultureName}\n" +
                       $"Formation: {troop.DefaultFormationClass}\n" +
                       $"Types: {types}\n" +
                       equipmentInfo +
                       upgradeInfo;
            });
        }

        /// <summary>
        /// UNFILTERED query for ALL CharacterObjects (except heroes) with AND logic
        /// WARNING: Returns NPCs, templates, children, etc. - not just combat troops
        /// Usage: gm.query.character_objects [search terms] [type keywords] [tier] [sort parameters]
        /// Example: gm.query.character_objects imperial infantry
        /// Example: gm.query.character_objects militia sort:name
        /// Note: Use gm.query.troop for filtered combat troops only
        /// </summary>
        [CommandLineFunctionality.CommandLineArgumentFunction("character_objects", "gm.query")]
        public static string QueryCharacterObjects(List<string> args)
        {
            return Cmd.Run(args, () =>
            {
                if (!CommandBase.ValidateCampaignState(out string error))
                    return error;

                var (query, types, tier, sortBy, sortDesc) = ParseArguments(args);

                List<CharacterObject> matchedCharacters = TroopQueries.QueryCharacterObjects(
                    query, types, matchAll: true, tier, sortBy, sortDesc);

                string criteriaDesc = BuildCriteriaString(query, types, tier, sortBy, sortDesc);

                string headerNote = "⚠️ UNFILTERED QUERY - Shows ALL CharacterObjects (except heroes)\n" +
                                    "Includes NPCs, templates, children, etc. Use gm.query.troop for combat troops only.\n\n";

                if (matchedCharacters.Count == 0)
                {
                    return headerNote +
                           $"Found 0 character(s) matching {criteriaDesc}\n" +
                           "Usage: gm.query.character_objects [search] [type keywords] [tier] [sort]\n" +
                           "Type keywords: infantry, ranged, cavalry, horsearcher, shield, bow, crossbow, regular, noble, militia, mercenary, caravan, bandit, etc.\n" +
                           "Tier keywords: tier0, tier1, tier2, tier3, tier4, tier5, tier6, tier6plus\n" +
                           "Sort: sort:name, sort:tier, sort:level, sort:culture, sort:<type> (add :desc for descending)\n" +
                           "Example: gm.query.character_objects imperial infantry tier2 sort:name\n";
                }

                return headerNote +
                       $"Found {matchedCharacters.Count} character(s) matching {criteriaDesc}:\n" +
                       $"{TroopQueries.GetFormattedDetails(matchedCharacters)}";
            });
        }

        /// <summary>
        /// UNFILTERED query for ALL CharacterObjects (except heroes) matching ANY of the specified types (OR logic)
        /// WARNING: Returns NPCs, templates, children, etc. - not just combat troops
        /// Usage: gm.query.character_objects_any [search terms] [type keywords] [tier] [sort parameters]
        /// Example: gm.query.character_objects_any cavalry ranged (cavalry OR ranged)
        /// Example: gm.query.character_objects_any bow crossbow tier4
        /// Note: Use gm.query.troop_any for filtered combat troops only
        /// </summary>
        [CommandLineFunctionality.CommandLineArgumentFunction("character_objects_any", "gm.query")]
        public static string QueryCharacterObjectsAny(List<string> args)
        {
            return Cmd.Run(args, () =>
            {
                if (!CommandBase.ValidateCampaignState(out string error))
                    return error;

                var (query, types, tier, sortBy, sortDesc) = ParseArguments(args);

                List<CharacterObject> matchedCharacters = TroopQueries.QueryCharacterObjects(
                    query, types, matchAll: false, tier, sortBy, sortDesc);

                string criteriaDesc = BuildCriteriaString(query, types, tier, sortBy, sortDesc);

                string headerNote = "⚠️ UNFILTERED QUERY - Shows ALL CharacterObjects (except heroes)\n" +
                                    "Includes NPCs, templates, children, etc. Use gm.query.troop_any for combat troops only.\n\n";

                if (matchedCharacters.Count == 0)
                {
                    return headerNote +
                           $"Found 0 character(s) matching ANY of {criteriaDesc}\n" +
                           "Usage: gm.query.character_objects_any [search] [type keywords] [tier] [sort]\n" +
                           "Example: gm.query.character_objects_any cavalry ranged tier3 sort:tier\n";
                }

                return headerNote +
                       $"Found {matchedCharacters.Count} character(s) matching ANY of {criteriaDesc}:\n" +
                       $"{TroopQueries.GetFormattedDetails(matchedCharacters)}";
            });
        }

        /// <summary>
        /// Get detailed info about a specific CharacterObject by ID (UNFILTERED)
        /// WARNING: Can return info on NPCs, templates, children, etc. - not just combat troops
        /// Usage: gm.query.character_objects_info <characterId>
        /// Example: gm.query.character_objects_info imperial_legionary
        /// Note: Use gm.query.troop_info for filtered combat troops only
        /// </summary>
        [CommandLineFunctionality.CommandLineArgumentFunction("character_objects_info", "gm.query")]
        public static string QueryCharacterObjectInfo(List<string> args)
        {
            return Cmd.Run(args, () =>
            {
                if (!CommandBase.ValidateCampaignState(out string error))
                    return error;

                if (args == null || args.Count == 0)
                    return "Error: Please provide a character ID.\nUsage: gm.query.character_objects_info <characterId>\n";

                string characterId = args[0];
                CharacterObject character = TroopQueries.GetTroopById(characterId);

                if (character == null)
                    return $"Error: Character with ID '{characterId}' not found.\n";

                if (character.IsHero)
                    return $"Error: '{characterId}' is a hero/lord. Use gm.query.hero_info instead.\n";

                string headerNote = "⚠️ UNFILTERED - Showing info for ANY CharacterObject (may be NPC, template, child, etc.)\n";

                // Check if it's an actual troop
                string troopStatus = character.IsActualTroop()
                    ? "✓ This is a valid combat troop"
                    : "⚠️ This is NOT a combat troop (NPC, template, child, etc.)";

                var types = character.GetTroopTypes();
                string cultureName = character.Culture?.Name?.ToString() ?? "None";

                // Build equipment list
                string equipmentInfo = "";
                if (character.FirstBattleEquipment != null)
                {
                    var equipment = character.FirstBattleEquipment;
                    List<string> items = new List<string>();
                    for (int i = 0; i < 12; i++)
                    {
                        var item = equipment[i].Item;
                        if (item != null)
                        {
                            items.Add(item.Name.ToString());
                        }
                    }
                    equipmentInfo = items.Count > 0
                        ? "Equipment: " + string.Join(", ", items) + "\n"
                        : "Equipment: None\n";
                }
                else
                {
                    equipmentInfo = "Equipment: None\n";
                }

                // Build upgrade paths
                string upgradeInfo = "";
                if (character.UpgradeTargets != null && character.UpgradeTargets.Length > 0)
                {
                    var upgrades = character.UpgradeTargets.Select(u => u.Name.ToString());
                    upgradeInfo = "Upgrades: " + string.Join(", ", upgrades) + "\n";
                }
                else
                {
                    upgradeInfo = "Upgrades: None\n";
                }

                string category = character.GetTroopCategory();

                return headerNote + "\n" +
                       $"Character Information:\n" +
                       $"ID: {character.StringId}\n" +
                       $"Name: {character.Name}\n" +
                       $"Status: {troopStatus}\n" +
                       $"Category: {category}\n" +
                       $"Tier: {character.GetBattleTier()}\n" +
                       $"Level: {character.Level}\n" +
                       $"Culture: {cultureName}\n" +
                       $"Formation: {character.DefaultFormationClass}\n" +
                       $"Types: {types}\n" +
                       equipmentInfo +
                       upgradeInfo;
            });
        }
    }
}