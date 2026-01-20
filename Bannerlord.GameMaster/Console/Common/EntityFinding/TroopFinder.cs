using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using Bannerlord.GameMaster.Troops;

namespace Bannerlord.GameMaster.Console.Common.EntityFinding
{
    /// <summary>
    /// Provides methods to find single combat troop entities from query strings.
    /// CRITICAL: Only returns actual combat troops (via IsActualTroop check).
    /// For unfiltered CharacterObject queries (dancers, refugees, NPCs, etc.), use CharacterObjectFinder instead.
    /// </summary>
    public static class TroopFinder
    {
        /// <summary>
        /// Finds a single troop from a query string (filtered combat troops only).
        /// </summary>
        /// <param name="query">The search query (troop name or ID)</param>
        /// <returns>EntityFinderResult containing the found troop or error message</returns>
        public static EntityFinderResult<CharacterObject> FindSingleTroop(string query)
        {
            List<CharacterObject> matchedTroops = TroopQueries.QueryTroops(query);

            if (matchedTroops == null || matchedTroops.Count == 0)
                return EntityFinderResult<CharacterObject>.Error($"Error: No troop matching query '{query}' found.\n");

            if (matchedTroops.Count == 1)
                return EntityFinderResult<CharacterObject>.Success(matchedTroops[0]);

            // Use smart matching for multiple results
            return EntityFinder.ResolveMultipleMatches(
                matches: matchedTroops,
                query: query,
                getStringId: t => t.StringId,
                getName: t => t.Name?.ToString() ?? "",
                formatDetails: TroopQueries.GetFormattedDetails,
                entityType: "troop");
        }
    }
}
