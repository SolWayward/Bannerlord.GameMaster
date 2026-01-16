using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using Bannerlord.GameMaster.Kingdoms;

namespace Bannerlord.GameMaster.Console.Common.EntityFinding
{
    /// <summary>
    /// Provides methods to find single Kingdom entities from query strings.
    /// </summary>
    public static class KingdomFinder
    {
        /// <summary>
        /// Finds a single kingdom from a query string (name, ID, or partial match).
        /// </summary>
        /// <param name="query">The search query (kingdom name or ID)</param>
        /// <returns>EntityFinderResult containing the found kingdom or error message</returns>
        public static EntityFinderResult<Kingdom> FindSingleKingdom(string query)
        {
            List<Kingdom> matchedKingdoms = KingdomQueries.QueryKingdoms(query);

            if (matchedKingdoms == null || matchedKingdoms.Count == 0)
                return EntityFinderResult<Kingdom>.Error($"Error: No kingdom matching query '{query}' found.\n");

            if (matchedKingdoms.Count == 1)
                return EntityFinderResult<Kingdom>.Success(matchedKingdoms[0]);

            // Use smart matching for multiple results
            return EntityFinder.ResolveMultipleMatches(
                matches: matchedKingdoms,
                query: query,
                getStringId: k => k.StringId,
                getName: k => k.Name?.ToString() ?? "",
                formatDetails: KingdomQueries.GetFormattedDetails,
                entityType: "kingdom");
        }
    }
}
