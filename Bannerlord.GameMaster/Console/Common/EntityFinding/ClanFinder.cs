using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using Bannerlord.GameMaster.Clans;

namespace Bannerlord.GameMaster.Console.Common.EntityFinding
{
    /// <summary>
    /// Provides methods to find single Clan entities from query strings.
    /// </summary>
    public static class ClanFinder
    {
        /// <summary>
        /// Finds a single clan from a query string (name, ID, or partial match).
        /// </summary>
        /// <param name="query">The search query (clan name or ID)</param>
        /// <returns>EntityFinderResult containing the found clan or error message</returns>
        public static EntityFinderResult<Clan> FindSingleClan(string query)
        {
            List<Clan> matchedClans = ClanQueries.QueryClans(query);

            if (matchedClans == null || matchedClans.Count == 0)
                return EntityFinderResult<Clan>.Error($"Error: No clan matching query '{query}' found.\n");

            if (matchedClans.Count == 1)
                return EntityFinderResult<Clan>.Success(matchedClans[0]);

            // Use smart matching for multiple results
            return EntityFinder.ResolveMultipleMatches(
                matches: matchedClans,
                query: query,
                getStringId: c => c.StringId,
                getName: c => c.Name?.ToString() ?? "",
                formatDetails: ClanQueries.GetFormattedDetails,
                entityType: "clan");
        }
    }
}
