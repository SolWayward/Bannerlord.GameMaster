using Bannerlord.GameMaster.Kingdoms;
using Bannerlord.GameMaster.Console.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Library;

namespace Bannerlord.GameMaster.Console.Query
{
    [CommandLineFunctionality.CommandLineArgumentFunction("query", "gm")]
    public static class KingdomQueryCommands
    {
        /// <summary>
        /// Parse command arguments into search filter and kingdom type flags
        /// </summary>
        private static (string query, KingdomTypes types, string sortBy, bool sortDesc) ParseArguments(List<string> args)
        {
            if (args == null || args.Count == 0)
                return ("", KingdomTypes.Active, "id", false);

            var typeKeywords = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "active", "eliminated", "empty", "player", "playerkingdom", "atwar", "war"
            };

            List<string> searchTerms = new();
            List<string> typeTerms = new();
            string sortBy = "id";
            bool sortDesc = false;

            foreach (var arg in args)
            {
                // Check for sort parameters
                if (arg.StartsWith("sort:", StringComparison.OrdinalIgnoreCase))
                {
                    ParseSortParameter(arg, ref sortBy, ref sortDesc);
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
            KingdomTypes types = KingdomQueries.ParseKingdomTypes(typeTerms);

            // Default to Active if no status specified
            if (!types.HasFlag(KingdomTypes.Active) && !types.HasFlag(KingdomTypes.Eliminated))
            {
                types |= KingdomTypes.Active;
            }

            return (query, types, sortBy, sortDesc);
        }

        /// <summary>
        /// Parse sort parameter (e.g., "sort:name:desc" or "sort:clans")
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
        /// Helper to build a readable criteria string
        /// </summary>
        private static string BuildCriteriaString(string query, KingdomTypes types, string sortBy, bool sortDesc)
        {
            List<string> parts = new();

            if (!string.IsNullOrEmpty(query))
                parts.Add($"search: '{query}'");

            if (types != KingdomTypes.None)
            {
                var typeList = Enum.GetValues(typeof(KingdomTypes))
                    .Cast<KingdomTypes>()
                    .Where(t => t != KingdomTypes.None && types.HasFlag(t))
                    .Select(t => t.ToString().ToLower());
                parts.Add($"types: {string.Join(", ", typeList)}");
            }

            if (!string.IsNullOrEmpty(sortBy) && sortBy != "id")
                parts.Add($"sort: {sortBy}{(sortDesc ? " (desc)" : " (asc)")}");

            return parts.Count > 0 ? string.Join(", ", parts) : "all kingdoms";
        }

        /// <summary>
        /// Unified kingdom finding command
        /// Usage: gm.query.kingdom [search terms] [type keywords] [sort parameters]
        /// Example: gm.query.kingdom empire atwar
        /// Example: gm.query.kingdom eliminated sort:name
        /// Example: gm.query.kingdom sort:strength:desc
        /// Example: gm.query.kingdom sort:atwar (sorts by atwar flag)
        /// </summary>
        [CommandLineFunctionality.CommandLineArgumentFunction("kingdom", "gm.query")]
        public static string QueryKingdoms(List<string> args)
        {
            return Cmd.Run(args, () =>
            {
                if (Campaign.Current == null)
                    return "Error: Must be in campaign mode.\n";

                var (query, types, sortBy, sortDesc) = ParseArguments(args);
                List<Kingdom> matchedKingdoms = KingdomQueries.QueryKingdoms(query, types, matchAll: true, sortBy, sortDesc);

                string criteriaDesc = BuildCriteriaString(query, types, sortBy, sortDesc);
                
                if (matchedKingdoms.Count == 0)
                {
                    return $"Found 0 kingdom(s) matching {criteriaDesc}\n" +
                           "Usage: gm.query.kingdom [search] [type keywords] [sort]\n" +
                           "Type keywords: active, eliminated, empty, atwar, player, etc.\n" +
                           "Sort: sort:name, sort:clans, sort:heroes, sort:fiefs, sort:strength, sort:<type> (add :desc for descending)\n" +
                           "Example: gm.query.kingdom empire atwar sort:strength:desc\n";
                }

                return $"Found {matchedKingdoms.Count} kingdom(s) matching {criteriaDesc}:\n" +
                       $"{KingdomQueries.GetFormattedDetails(matchedKingdoms)}";
            });
        }

        /// <summary>
        /// Find kingdoms matching ANY of the specified types (OR logic)
        /// Usage: gm.query.kingdom_any [search terms] [type keywords] [sort parameters]
        /// Example: gm.query.kingdom_any atwar eliminated sort:name:desc
        /// </summary>
        [CommandLineFunctionality.CommandLineArgumentFunction("kingdom_any", "gm.query")]
        public static string QueryKingdomsAny(List<string> args)
        {
            return Cmd.Run(args, () =>
            {
                if (Campaign.Current == null)
                    return "Error: Must be in campaign mode.\n";

                var (query, types, sortBy, sortDesc) = ParseArguments(args);
                List<Kingdom> matchedKingdoms = KingdomQueries.QueryKingdoms(query, types, matchAll: false, sortBy, sortDesc);

                string criteriaDesc = BuildCriteriaString(query, types, sortBy, sortDesc);
                
                if (matchedKingdoms.Count == 0)
                {
                    return $"Found 0 kingdom(s) matching ANY of {criteriaDesc}\n" +
                           "Usage: gm.query.kingdom_any [search] [type keywords] [sort]\n";
                }

                return $"Found {matchedKingdoms.Count} kingdom(s) matching ANY of {criteriaDesc}:\n" +
                       $"{KingdomQueries.GetFormattedDetails(matchedKingdoms)}";
            });
        }

        /// <summary>
        /// Get detailed info about a specific kingdom by ID
        /// </summary>
        [CommandLineFunctionality.CommandLineArgumentFunction("kingdom_info", "gm.query")]
        public static string QueryKingdomInfo(List<string> args)
        {
            return Cmd.Run(args, () =>
            {
                if (Campaign.Current == null)
                    return "Error: Must be in campaign mode.\n";

                if (args == null || args.Count == 0)
                    return "Error: Please provide a kingdom ID.\nUsage: gm.kingdom.info <kingdomId>\n";

                string kingdomId = args[0];
                Kingdom kingdom = KingdomQueries.GetKingdomById(kingdomId);

                if (kingdom == null)
                    return $"Error: Kingdom with ID '{kingdomId}' not found.\n";

                var types = kingdom.GetKingdomTypes();

                // Get kingdoms at war with this kingdom
                List<Kingdom> enemies = new List<Kingdom>();
                foreach (var otherKingdom in Kingdom.All)
                {
                    if (otherKingdom != kingdom && FactionManager.IsAtWarAgainstFaction(kingdom, otherKingdom))
                    {
                        enemies.Add(otherKingdom);
                    }
                }

                return $"Kingdom Information:\n" +
                       $"ID: {kingdom.StringId}\n" +
                       $"Name: {kingdom.Name}\n" +
                       $"Culture: {kingdom.Culture?.Name}\n" +
                       $"Ruler: {kingdom.Leader?.Name}\n" +
                       $"Ruling Clan: {kingdom.RulingClan?.Name}\n" +
                       $"Total Clans: {kingdom.Clans.Count}\n" +
                       $"Total Heroes: {kingdom.Heroes.Count()}\n" +
                       $"Total Fiefs: {kingdom.Fiefs.Count}\n" +
                       $"Total Strength: {kingdom.CurrentTotalStrength:F0}\n" +
                       $"Types: {types}\n" +
                       $"Is Eliminated: {kingdom.IsEliminated}\n" +
                       $"At War With ({enemies.Count}): {string.Join(", ", enemies.Select(k => k.Name))}\n";
            });
        }
    }
}