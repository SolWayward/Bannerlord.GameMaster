using System.Collections.Generic;
using TaleWorlds.CampaignSystem.Settlements;
using Bannerlord.GameMaster.Settlements;

namespace Bannerlord.GameMaster.Console.Common.EntityFinding
{
    /// <summary>
    /// Provides methods to find single Settlement entities from query strings.
    /// </summary>
    public static class SettlementFinder
    {
        /// <summary>
        /// Finds a single settlement from a query string (name, ID, or partial match).
        /// </summary>
        /// <param name="query">The search query (settlement name or ID)</param>
        /// <returns>EntityFinderResult containing the found settlement or error message</returns>
        public static EntityFinderResult<Settlement> FindSingleSettlement(string query)
        {
            List<Settlement> matchedSettlements = SettlementQueries.QuerySettlements(query);

            if (matchedSettlements == null || matchedSettlements.Count == 0)
                return EntityFinderResult<Settlement>.Error($"Error: No settlement matching query '{query}' found.\n");

            if (matchedSettlements.Count == 1)
                return EntityFinderResult<Settlement>.Success(matchedSettlements[0]);

            // Use smart matching for multiple results
            return EntityFinder.ResolveMultipleMatches(
                matches: matchedSettlements,
                query: query,
                getStringId: s => s.StringId,
                getName: s => s.Name?.ToString() ?? "",
                formatDetails: SettlementQueries.GetFormattedDetails,
                entityType: "settlement");
        }
    }
}
