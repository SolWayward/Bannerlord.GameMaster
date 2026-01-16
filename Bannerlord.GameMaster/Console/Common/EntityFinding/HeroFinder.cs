using System.Collections.Generic;
using System.Text;
using TaleWorlds.CampaignSystem;
using Bannerlord.GameMaster.Heroes;
using Bannerlord.GameMaster.Console.Common.Execution;

namespace Bannerlord.GameMaster.Console.Common.EntityFinding
{
    /// <summary>
    /// Provides methods to find single Hero entities from query strings.
    /// </summary>
    public static class HeroFinder
    {
        /// <summary>
        /// Finds a single hero from a query string (name, ID, or partial match).
        /// </summary>
        /// <param name="query">The search query (hero name or ID)</param>
        /// <returns>EntityFinderResult containing the found hero or error message</returns>
        public static EntityFinderResult<Hero> FindSingleHero(string query)
        {
            List<Hero> matchedHeroes = HeroQueries.QueryHeroes(query);

            // DEBUG: Log all matched heroes
            if (CommandLogger.IsEnabled && matchedHeroes != null && matchedHeroes.Count > 0)
            {
                StringBuilder debugInfo = new();
                debugInfo.AppendLine($"[BLGM_DEBUG] FindSingleHero query '{query}' found {matchedHeroes.Count} matches:");
                foreach (Hero hero in matchedHeroes)
                {
                    debugInfo.AppendLine($"  - Name: '{hero.Name}' | ID: '{hero.StringId}' | Culture: {hero.Culture?.Name}");
                }
                CommandLogger.Log(debugInfo.ToString());
            }

            if (matchedHeroes == null || matchedHeroes.Count == 0)
                return EntityFinderResult<Hero>.Error($"Error: No hero matching query '{query}' found.\n");

            if (matchedHeroes.Count == 1)
                return EntityFinderResult<Hero>.Success(matchedHeroes[0]);

            // Use smart matching for multiple results
            return EntityFinder.ResolveMultipleMatches(
                matches: matchedHeroes,
                query: query,
                getStringId: h => h.StringId,
                getName: h => h.Name?.ToString() ?? "",
                formatDetails: HeroQueries.GetFormattedDetails,
                entityType: "hero");
        }
    }
}
