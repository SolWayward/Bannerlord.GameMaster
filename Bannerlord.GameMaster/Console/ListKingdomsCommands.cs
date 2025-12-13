using Bannerlord.GameMaster.Kingdoms;
using System;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Library;

namespace Bannerlord.GameMaster.Console
{
    [CommandLineFunctionality.CommandLineArgumentFunction("kingdom", "gm")]
    public static class ListKingdomsCommands
    {
        /// <summary>
        /// Parse command arguments into search filter and kingdom type flags
        /// </summary>
        private static (string searchFilter, KingdomTypes types) ParseArguments(List<string> args)
        {
            if (args == null || args.Count == 0)
                return ("", KingdomTypes.Active);

            var typeKeywords = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "active", "eliminated", "empty", "player", "playerkingdom", "atwar", "war"
            };

            List<string> searchTerms = new();
            List<string> typeTerms = new();

            foreach (var arg in args)
            {
                if (typeKeywords.Contains(arg.ToLower()))
                    typeTerms.Add(arg);
                else
                    searchTerms.Add(arg);
            }

            string searchFilter = string.Join(" ", searchTerms).Trim();
            KingdomTypes types = KingdomFinder.ParseKingdomTypes(typeTerms);

            // Default to Active if no status specified
            if (!types.HasFlag(KingdomTypes.Active) && !types.HasFlag(KingdomTypes.Eliminated))
            {
                types |= KingdomTypes.Active;
            }

            return (searchFilter, types);
        }

        /// <summary>
        /// Helper to build a readable criteria string
        /// </summary>
        private static string BuildCriteriaString(string searchFilter, KingdomTypes types)
        {
            List<string> parts = new();

            if (!string.IsNullOrEmpty(searchFilter))
                parts.Add($"search: '{searchFilter}'");

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
        /// Usage: gm.kingdom.find [search terms] [type keywords]
        /// Example: gm.kingdom.find empire atwar
        /// Example: gm.kingdom.find eliminated
        /// </summary>
        [CommandLineFunctionality.CommandLineArgumentFunction("find", "gm.kingdom")]
        public static string FindKingdoms(List<string> args)
        {
            if (Campaign.Current == null)
                return "Error: Must be in campaign mode.\n";

            var (searchFilter, types) = ParseArguments(args);
            List<Kingdom> matchedKingdoms = KingdomFinder.FindKingdoms(searchFilter, types, matchAll: true);

            if (matchedKingdoms.Count == 0)
            {
                string criteria = BuildCriteriaString(searchFilter, types);
                return $"No kingdoms found matching criteria: {criteria}\n" +
                       "Usage: gm.kingdom.find [search] [type keywords]\n" +
                       "Type keywords: active, eliminated, empty, atwar, allies, player, etc.\n" +
                       "Example: gm.kingdom.find empire atwar\n";
            }

            string criteriaDesc = BuildCriteriaString(searchFilter, types);
            return $"Found {matchedKingdoms.Count} kingdom(s) matching {criteriaDesc}:\n" +
                   $"{KingdomFinder.GetFormattedDetails(matchedKingdoms)}";
        }

        /// <summary>
        /// Find kingdoms matching ANY of the specified types (OR logic)
        /// </summary>
        [CommandLineFunctionality.CommandLineArgumentFunction("find_any", "gm.kingdom")]
        public static string FindKingdomsAny(List<string> args)
        {
            if (Campaign.Current == null)
                return "Error: Must be in campaign mode.\n";

            var (searchFilter, types) = ParseArguments(args);
            List<Kingdom> matchedKingdoms = KingdomFinder.FindKingdoms(searchFilter, types, matchAll: false);

            if (matchedKingdoms.Count == 0)
            {
                string criteria = BuildCriteriaString(searchFilter, types);
                return $"No kingdoms found matching ANY of: {criteria}\n" +
                       "Usage: gm.kingdom.find_any [search] [type keywords]\n";
            }

            string criteriaDesc = BuildCriteriaString(searchFilter, types);
            return $"Found {matchedKingdoms.Count} kingdom(s) matching ANY of {criteriaDesc}:\n" +
                   $"{KingdomFinder.GetFormattedDetails(matchedKingdoms)}";
        }

        /// <summary>
        /// Get detailed info about a specific kingdom by ID
        /// </summary>
        [CommandLineFunctionality.CommandLineArgumentFunction("info", "gm.kingdom")]
        public static string GetKingdomInfo(List<string> args)
        {
            if (Campaign.Current == null)
                return "Error: Must be in campaign mode.\n";

            if (args == null || args.Count == 0)
                return "Error: Please provide a kingdom ID.\nUsage: gm.kingdom.info <kingdomId>\n";

            string kingdomId = args[0];
            Kingdom kingdom = KingdomFinder.GetKingdomById(kingdomId);

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
        }
    }
}