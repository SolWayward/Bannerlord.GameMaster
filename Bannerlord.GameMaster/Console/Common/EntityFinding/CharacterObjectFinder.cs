using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Library;
using Bannerlord.GameMaster.Characters;

namespace Bannerlord.GameMaster.Console.Common.EntityFinding
{
    /// <summary>
    /// Provides methods to find single CharacterObject entities from query strings.
    /// CRITICAL: Returns ALL CharacterObjects (unfiltered - includes dancers, refugees, NPCs, etc.)
    /// For filtered combat troops only, use TroopFinder instead.
    /// </summary>
    public static class CharacterObjectFinder
    {
        /// <summary>
        /// Finds a single CharacterObject from a query string (unfiltered - includes all CharacterObjects).
        /// Use this when you need to accept any CharacterObject including dancers, refugees, NPCs, etc.
        /// For combat troops only, use TroopFinder.FindSingleTroop() instead.
        /// </summary>
        /// <param name="query">The search query (character name or ID)</param>
        /// <returns>EntityFinderResult containing the found character or error message</returns>
        public static EntityFinderResult<CharacterObject> FindSingleCharacterObject(string query)
        {
            MBReadOnlyList<CharacterObject> matchedCharacters = CharacterQueries.QueryCharacterObjects(query);

            if (matchedCharacters == null || matchedCharacters.Count == 0)
                return EntityFinderResult<CharacterObject>.Error($"Error: No character matching query '{query}' found.\n");

            if (matchedCharacters.Count == 1)
                return EntityFinderResult<CharacterObject>.Success(matchedCharacters[0]);

            // Convert to List for ResolveMultipleMatches (required by existing API)
            List<CharacterObject> matchList = new(matchedCharacters.Count);
            for (int i = 0; i < matchedCharacters.Count; i++)
            {
                matchList.Add(matchedCharacters[i]);
            }

            // Use smart matching for multiple results
            return EntityFinder.ResolveMultipleMatches(
                matches: matchList,
                query: query,
                getStringId: c => c.StringId,
                getName: c => c.Name?.ToString() ?? "",
                formatDetails: CharacterQueries.GetFormattedDetails,
                entityType: "character");
        }
    }
}
