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
        private static (string query, KingdomTypes types) ParseArguments(List<string> args)
        {
            if (args == null || args.Count == 0)
                return ("", KingdomTypes.Active);

            var typeKeywords = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "active", "eliminated", "empty", "player", "playerkingdom", "atwar", "war"
            };

            // Use generic parser to separate search terms from type keywords
            var (query, types) = QueryArgumentParser<KingdomTypes>.Parse(
                args,
                typeKeywords,
                KingdomQueries.ParseKingdomTypes,
                KingdomTypes.None);

            // Default to Active if no status specified
            if (!types.HasFlag(KingdomTypes.Active) && !types.HasFlag(KingdomTypes.Eliminated))
            {
                types |= KingdomTypes.Active;
            }

            return (query, types);
        }

        /// <summary>
        /// Helper to build a readable criteria string
        /// </summary>
        private static string BuildCriteriaString(string query, KingdomTypes types)
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

            return parts.Count > 0 ? string.Join(", ", parts) : "all kingdoms";
        }

        /// <summary>
        /// Unified kingdom finding command
        /// Usage: gm.query.kingdom [search terms] [type keywords]
        /// Example: gm.query.kingdom empire atwar
        /// Example: gm.query.kingdom eliminated
        /// </summary>
        [CommandLineFunctionality.CommandLineArgumentFunction("kingdom", "gm.query")]
        public static string QueryKingdoms(List<string> args)
        {
            return Cmd.Run(args, () =>
            {
                if (Campaign.Current == null)
                    return "Error: Must be in campaign mode.\n";

                var (query, types) = ParseArguments(args);
                List<Kingdom> matchedKingdoms = KingdomQueries.QueryKingdoms(query, types, matchAll: true);

                string criteriaDesc = BuildCriteriaString(query, types);
                
                if (matchedKingdoms.Count == 0)
                {
                    return $"Found 0 kingdom(s) matching {criteriaDesc}\n" +
                           "Usage: gm.query.kingdom [search] [type keywords]\n" +
                           "Type keywords: active, eliminated, empty, atwar, allies, player, etc.\n" +
                           "Example: gm.query.kingdom empire atwar\n";
                }

                return $"Found {matchedKingdoms.Count} kingdom(s) matching {criteriaDesc}:\n" +
                       $"{KingdomQueries.GetFormattedDetails(matchedKingdoms)}";
            });
        }

        /// <summary>
        /// Find kingdoms matching ANY of the specified types (OR logic)
        /// </summary>
        [CommandLineFunctionality.CommandLineArgumentFunction("kingdom_any", "gm.query")]
        public static string QueryKingdomsAny(List<string> args)
        {
            return Cmd.Run(args, () =>
            {
                if (Campaign.Current == null)
                    return "Error: Must be in campaign mode.\n";

                var (query, types) = ParseArguments(args);
                List<Kingdom> matchedKingdoms = KingdomQueries.QueryKingdoms(query, types, matchAll: false);

                string criteriaDesc = BuildCriteriaString(query, types);
                
                if (matchedKingdoms.Count == 0)
                {
                    return $"Found 0 kingdom(s) matching ANY of {criteriaDesc}\n" +
                           "Usage: gm.query.kingdom_any [search] [type keywords]\n";
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