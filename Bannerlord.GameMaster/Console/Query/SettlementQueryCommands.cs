using Bannerlord.GameMaster.Settlements;
using Bannerlord.GameMaster.Console.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Library;

namespace Bannerlord.GameMaster.Console.Query
{
    [CommandLineFunctionality.CommandLineArgumentFunction("query", "gm")]
    public static class SettlementQueryCommands
    {
        /// <summary>
        /// Parse command arguments into search filter and settlement type flags
        /// </summary>
        private static (string query, SettlementTypes types, string sortBy, bool sortDesc) ParseArguments(List<string> args)
        {
            if (args == null || args.Count == 0)
                return ("", SettlementTypes.None, "id", false);

            var typeKeywords = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "town", "castle", "city", "village", "hideout",
                "player", "playerowned",
                "besieged", "siege", "raided",
                "empire", "vlandia", "sturgia", "aserai", "khuzait", "battania", "nord",
                "lowprosperity", "mediumprosperity", "highprosperity",
                "low", "medium", "high"
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
            SettlementTypes types = SettlementQueries.ParseSettlementTypes(typeTerms);

            return (query, types, sortBy, sortDesc);
        }

        /// <summary>
        /// Parse sort parameter (e.g., "sort:name:desc" or "sort:prosperity")
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
        /// Unified settlement listing command (AND logic)
        /// Usage: gm.query.settlement [search terms] [type keywords] [sort parameters]
        /// Example: gm.query.settlement castle empire
        /// Example: gm.query.settlement pen city sort:prosperity:desc
        /// Example: gm.query.settlement player town sort:name
        /// </summary>
        [CommandLineFunctionality.CommandLineArgumentFunction("settlement", "gm.query")]
        public static string QuerySettlements(List<string> args)
        {
            return Cmd.Run(args, () =>
            {
                if (!CommandBase.ValidateCampaignMode(out string error))
                    return error;

                var (query, types, sortBy, sortDesc) = ParseArguments(args);

                List<Settlement> matchedSettlements = SettlementQueries.QuerySettlements(
                    query, types, matchAll: true, sortBy, sortDesc);

                string criteriaDesc = BuildCriteriaString(query, types, sortBy, sortDesc);

                if (matchedSettlements.Count == 0)
                {
                    return $"Found 0 settlement(s) matching {criteriaDesc}\n" +
                           "Usage: gm.query.settlement [search] [type keywords] [sort]\n" +
                           "Type keywords: town, castle, city, village, hideout, player, besieged, raided, empire, vlandia, etc.\n" +
                           "Prosperity: low, medium, high\n" +
                           "Sort: sort:name, sort:prosperity, sort:owner, sort:kingdom, sort:culture (add :desc for descending)\n" +
                           "Example: gm.query.settlement castle empire sort:prosperity:desc\n";
                }

                return $"Found {matchedSettlements.Count} settlement(s) matching {criteriaDesc}:\n" +
                       $"{SettlementQueries.GetFormattedDetails(matchedSettlements)}";
            });
        }

        /// <summary>
        /// Find settlements matching ANY of the specified types (OR logic)
        /// Usage: gm.query.settlement_any [search terms] [type keywords] [sort parameters]
        /// Example: gm.query.settlement_any castle city (finds castles OR cities)
        /// Example: gm.query.settlement_any empire vlandia sort:name
        /// </summary>
        [CommandLineFunctionality.CommandLineArgumentFunction("settlement_any", "gm.query")]
        public static string QuerySettlementsAny(List<string> args)
        {
            return Cmd.Run(args, () =>
            {
                if (!CommandBase.ValidateCampaignMode(out string error))
                    return error;

                var (query, types, sortBy, sortDesc) = ParseArguments(args);

                List<Settlement> matchedSettlements = SettlementQueries.QuerySettlements(
                    query, types, matchAll: false, sortBy, sortDesc);

                string criteriaDesc = BuildCriteriaString(query, types, sortBy, sortDesc);

                if (matchedSettlements.Count == 0)
                {
                    return $"Found 0 settlement(s) matching ANY of {criteriaDesc}\n" +
                           "Usage: gm.query.settlement_any [search] [type keywords] [sort]\n" +
                           "Example: gm.query.settlement_any castle city sort:prosperity:desc\n";
                }

                return $"Found {matchedSettlements.Count} settlement(s) matching ANY of {criteriaDesc}:\n" +
                       $"{SettlementQueries.GetFormattedDetails(matchedSettlements)}";
            });
        }

        /// <summary>
        /// Get detailed info about a specific settlement by ID
        /// Usage: gm.query.settlement_info &lt;settlementId&gt;
        /// Example: gm.query.settlement_info town_empire_1
        /// </summary>
        [CommandLineFunctionality.CommandLineArgumentFunction("settlement_info", "gm.query")]
        public static string QuerySettlementInfo(List<string> args)
        {
            return Cmd.Run(args, () =>
            {
                if (!CommandBase.ValidateCampaignMode(out string error))
                    return error;

                if (args == null || args.Count == 0)
                    return "Error: Please provide a settlement ID.\nUsage: gm.query.settlement_info <settlementId>\n";

                string settlementId = args[0];
                Settlement settlement = SettlementQueries.GetSettlementById(settlementId);

                if (settlement == null)
                    return $"Error: Settlement with ID '{settlementId}' not found.\n";

                var types = settlement.GetSettlementTypes();
                string settlementType = settlement.IsTown
                    ? (settlement.IsCastle ? "Castle" : "City")
                    : settlement.IsVillage ? "Village"
                    : settlement.IsHideout ? "Hideout"
                    : "Unknown";

                string ownerName = settlement.OwnerClan?.Name?.ToString() ?? "None";
                string kingdomName = settlement.MapFaction?.Name?.ToString() ?? "None";
                string cultureName = settlement.Culture?.Name?.ToString() ?? "None";

                string prosperityInfo = "";
                if (settlement.IsTown && settlement.Town != null)
                {
                    prosperityInfo = $"Prosperity: {settlement.Town.Prosperity:F0}\n" +
                                   $"Security: {settlement.Town.Security:F0}\n" +
                                   $"Loyalty: {settlement.Town.Loyalty:F0}\n" +
                                   $"Food Stocks: {settlement.Town.FoodStocks:F0}\n";
                }
                else if (settlement.IsVillage && settlement.Village != null)
                {
                    prosperityInfo = $"Hearth: {settlement.Village.Hearth:F0}\n" +
                                   $"Bound Town: {settlement.Village.Bound?.Name}\n";
                }

                string siegeInfo = settlement.IsUnderSiege
                    ? $"Under Siege: Yes\nBesieger: {settlement.SiegeEvent?.BesiegerCamp?.LeaderParty?.Name}\n"
                    : "Under Siege: No\n";

                string notableInfo = "";
                if (settlement.Notables != null && settlement.Notables.Any())
                {
                    notableInfo = $"Notables: {settlement.Notables.Count()}\n";
                }

                return $"Settlement Information:\n" +
                       $"ID: {settlement.StringId}\n" +
                       $"Name: {settlement.Name}\n" +
                       $"Type: {settlementType}\n" +
                       $"Owner: {ownerName}\n" +
                       $"Kingdom: {kingdomName}\n" +
                       $"Culture: {cultureName}\n" +
                       $"{prosperityInfo}" +
                       $"{siegeInfo}" +
                       $"{notableInfo}" +
                       $"Types: {types}\n" +
                       $"Position: X={settlement.GetPosition2D.X:F1}, Y={settlement.GetPosition2D.Y:F1}\n";
            });
        }

        /// <summary>
        /// Helper to build a readable criteria string
        /// </summary>
        private static string BuildCriteriaString(string query, SettlementTypes types, string sortBy, bool sortDesc)
        {
            List<string> parts = new();

            if (!string.IsNullOrEmpty(query))
                parts.Add($"search: '{query}'");

            if (types != SettlementTypes.None)
            {
                var typeList = Enum.GetValues(typeof(SettlementTypes))
                    .Cast<SettlementTypes>()
                    .Where(t => t != SettlementTypes.None && types.HasFlag(t))
                    .Select(t => t.ToString().ToLower());
                parts.Add($"types: {string.Join(", ", typeList)}");
            }

            if (!string.IsNullOrEmpty(sortBy) && sortBy != "id")
                parts.Add($"sort: {sortBy}{(sortDesc ? " (desc)" : " (asc)")}");

            return parts.Count > 0 ? string.Join(", ", parts) : "all settlements";
        }
    }
}