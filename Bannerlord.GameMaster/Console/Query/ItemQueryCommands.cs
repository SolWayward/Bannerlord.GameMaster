using Bannerlord.GameMaster.Items;
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
    public static class ItemQueryCommands
    {
        /// <summary>
        /// Parse command arguments into search filter, item type flags, tier filter, and sort options
        /// </summary>
        private static (string query, ItemTypes types, int tier, string sortBy, bool sortDesc) ParseArguments(List<string> args)
        {
            if (args == null || args.Count == 0)
                return ("", ItemTypes.None, -1, "id", false);

            var typeKeywords = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "weapon", "armor", "mount", "food", "trade", "goods", "banner",
                "1h", "onehanded", "2h", "twohanded", "ranged", "shield", "polearm", "thrown",
                "arrows", "bolts", "head", "headarmor", "body", "bodyarmor",
                "leg", "legarmor", "hand", "handarmor", "cape",
                "bow", "crossbow", "civilian", "combat", "horsearmor"
            };

            var tierKeywords = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "tier0", "tier1", "tier2", "tier3", "tier4", "tier5", "tier6"
            };

            List<string> searchTerms = new();
            List<string> typeTerms = new();
            int tierFilter = -1;
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
                else if (tierKeywords.Contains(arg, StringComparer.OrdinalIgnoreCase))
                {
                    tierFilter = ParseTierKeyword(arg);
                }
                // Check for type keywords
                else if (typeKeywords.Contains(arg, StringComparer.OrdinalIgnoreCase))
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
            ItemTypes types = ItemQueries.ParseItemTypes(typeTerms);

            return (query, types, tierFilter, sortBy, sortDesc);
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
        /// Parse tier keyword (e.g., "tier3" returns 3)
        /// </summary>
        private static int ParseTierKeyword(string tierKeyword)
        {
            if (tierKeyword.Length >= 5 && tierKeyword.StartsWith("tier", StringComparison.OrdinalIgnoreCase))
            {
                if (int.TryParse(tierKeyword.Substring(4), out int tier))
                {
                    return tier;
                }
            }
            return -1;
        }

        /// <summary>
        /// Unified item listing command
        /// Usage: gm.query.item [search terms] [type keywords] [tier keywords] [sort parameters]
        /// Example: gm.query.item sword weapon 1h
        /// Example: gm.query.item imperial armor tier3
        /// Example: gm.query.item food sort:value:desc
        /// Example: gm.query.item bow ranged tier4 sort:name
        /// </summary>
        [CommandLineFunctionality.CommandLineArgumentFunction("item", "gm.query")]
        public static string QueryItems(List<string> args)
        {
            return Cmd.Run(args, () =>
            {
                if (!CommandBase.ValidateCampaignState(out string error))
                    return error;

                var (query, types, tier, sortBy, sortDesc) = ParseArguments(args);

                List<ItemObject> matchedItems = ItemQueries.QueryItems(query, types, matchAll: true, tier, sortBy, sortDesc);

                string criteriaDesc = BuildCriteriaString(query, types, tier, sortBy, sortDesc);

                if (matchedItems.Count == 0)
                {
                    return $"Found 0 item(s) matching {criteriaDesc}\n" +
                           "Usage: gm.query.item [search] [type keywords] [tier] [sort]\n" +
                           "Type keywords: weapon, armor, mount, food, trade, 1h, 2h, ranged, bow, crossbow, civilian, combat, horsearmor, etc.\n" +
                           "Tier keywords: tier0, tier1, tier2, tier3, tier4, tier5, tier6\n" +
                           "Sort: sort:name, sort:tier, sort:value, sort:type (add :desc for descending)\n" +
                           "Example: gm.query.item sword weapon 1h tier3 sort:value:desc\n";
                }

                return $"Found {matchedItems.Count} item(s) matching {criteriaDesc}:\n" +
                       $"{ItemQueries.GetFormattedDetails(matchedItems)}";
            });
        }

        /// <summary>
        /// Find items matching ANY of the specified types (OR logic)
        /// Usage: gm.query.item_any [search terms] [type keywords] [tier keywords] [sort parameters]
        /// Example: gm.query.item_any weapon armor (finds anything that is weapon OR armor)
        /// Example: gm.query.item_any bow crossbow tier5 sort:value
        /// </summary>
        [CommandLineFunctionality.CommandLineArgumentFunction("item_any", "gm.query")]
        public static string QueryItemsAny(List<string> args)
        {
            return Cmd.Run(args, () =>
            {
                if (!CommandBase.ValidateCampaignState(out string error))
                    return error;

                var (query, types, tier, sortBy, sortDesc) = ParseArguments(args);

                List<ItemObject> matchedItems = ItemQueries.QueryItems(query, types, matchAll: false, tier, sortBy, sortDesc);

                string criteriaDesc = BuildCriteriaString(query, types, tier, sortBy, sortDesc);

                if (matchedItems.Count == 0)
                {
                    return $"Found 0 item(s) matching ANY of {criteriaDesc}\n" +
                           "Usage: gm.query.item_any [search] [type keywords] [tier] [sort]\n" +
                           "Example: gm.query.item_any weapon armor tier3 sort:name\n";
                }

                return $"Found {matchedItems.Count} item(s) matching ANY of {criteriaDesc}:\n" +
                       $"{ItemQueries.GetFormattedDetails(matchedItems)}";
            });
        }

        /// <summary>
        /// Get detailed info about a specific item by ID
        /// </summary>
        [CommandLineFunctionality.CommandLineArgumentFunction("item_info", "gm.query")]
        public static string QueryItemInfo(List<string> args)
        {
            return Cmd.Run(args, () =>
            {
                if (!CommandBase.ValidateCampaignState(out string error))
                    return error;

                if (args == null || args.Count == 0)
                    return "Error: Please provide an item ID.\nUsage: gm.query.item_info <itemId>\n";

                string itemId = args[0];
                ItemObject item = ItemQueries.GetItemById(itemId);

                if (item == null)
                    return $"Error: Item with ID '{itemId}' not found.\n";

                var types = item.GetItemTypes();
                // Note: ItemTiers enum values are offset by 1 (Tier0=-1, Tier1=0, Tier2=1, etc.)
                // So we add 1 to display the user-friendly tier number
                string tier = (int)item.Tier >= -1 ? ((int)item.Tier + 1).ToString() : "N/A";

                // Build detailed stats based on item type
                string additionalInfo = "";

                // Weapon stats
                if (item.WeaponComponent != null)
                {
                    var weapon = item.WeaponComponent.PrimaryWeapon;
                    additionalInfo += $"Weapon Class: {weapon.WeaponClass}\n" +
                                     $"Damage: {weapon.SwingDamage} (Swing), {weapon.ThrustDamage} (Thrust)\n" +
                                     $"Speed: {weapon.SwingSpeed} (Swing), {weapon.ThrustSpeed} (Thrust)\n" +
                                     $"Handling: {weapon.Handling}\n";
                }

                // Armor stats
                if (item.ArmorComponent != null)
                {
                    var armor = item.ArmorComponent;
                    additionalInfo += $"Head Armor: {armor.HeadArmor}\n" +
                                     $"Body Armor: {armor.BodyArmor}\n" +
                                     $"Leg Armor: {armor.LegArmor}\n" +
                                     $"Arm Armor: {armor.ArmArmor}\n";
                }

                // Mount stats
                if (item.HorseComponent != null)
                {
                    var horse = item.HorseComponent;
                    additionalInfo += $"Charge Damage: {horse.ChargeDamage}\n" +
                                     $"Speed: {horse.Speed}\n" +
                                     $"Maneuver: {horse.Maneuver}\n" +
                                     $"Hit Points: {horse.HitPoints}\n";
                }

                return $"Item Information:\n" +
                       $"ID: {item.StringId}\n" +
                       $"Name: {item.Name}\n" +
                       $"Type: {item.ItemType}\n" +
                       $"Value: {item.Value}\n" +
                       $"Weight: {item.Weight}\n" +
                       $"Tier: {tier}\n" +
                       $"Types: {types}\n" +
                       additionalInfo;
            });
        }

        /// <summary>
        /// Helper to build a readable criteria string
        /// </summary>
        private static string BuildCriteriaString(string query, ItemTypes types, int tier, string sortBy, bool sortDesc)
        {
            List<string> parts = new();

            if (!string.IsNullOrEmpty(query))
                parts.Add($"search: '{query}'");

            if (types != ItemTypes.None)
            {
                var typeList = Enum.GetValues(typeof(ItemTypes))
                    .Cast<ItemTypes>()
                    .Where(t => t != ItemTypes.None && types.HasFlag(t))
                    .Select(t => t.ToString().ToLower());
                parts.Add($"types: {string.Join(", ", typeList)}");
            }

            if (tier >= 0)
                parts.Add($"tier: {tier}");

            if (!string.IsNullOrEmpty(sortBy) && sortBy != "id")
                parts.Add($"sort: {sortBy}{(sortDesc ? " (desc)" : " (asc)")}");

            return parts.Count > 0 ? string.Join(", ", parts) : "all items";
        }
    }
}