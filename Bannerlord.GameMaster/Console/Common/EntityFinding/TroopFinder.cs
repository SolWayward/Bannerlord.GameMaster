using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using Bannerlord.GameMaster.Troops;

namespace Bannerlord.GameMaster.Console.Common.EntityFinding
{
    /// <summary>
    /// Provides methods to find single CharacterObject entities from query strings.
    /// Includes both filtered troop-only and unfiltered CharacterObject finders.
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

        /// <summary>
        /// Finds a single CharacterObject from a query string (unfiltered - includes all non-hero CharacterObjects).
        /// Use this when you need to accept any CharacterObject including dancers, refugees, etc.
        /// </summary>
        /// <param name="query">The search query (character name or ID)</param>
        /// <returns>EntityFinderResult containing the found character or error message</returns>
        public static EntityFinderResult<CharacterObject> FindSingleCharacterObject(string query)
        {
            List<CharacterObject> matchedCharacters = TroopQueries.QueryCharacterObjects(query);

            if (matchedCharacters == null || matchedCharacters.Count == 0)
                return EntityFinderResult<CharacterObject>.Error($"Error: No character matching query '{query}' found.\n");

            if (matchedCharacters.Count == 1)
                return EntityFinderResult<CharacterObject>.Success(matchedCharacters[0]);

            // Use smart matching for multiple results
            return EntityFinder.ResolveMultipleMatches(
                matches: matchedCharacters,
                query: query,
                getStringId: c => c.StringId,
                getName: c => c.Name?.ToString() ?? "",
                formatDetails: TroopQueries.GetFormattedDetails,
                entityType: "character");
        }
    }
}
