using System;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;

namespace Bannerlord.GameMaster.Heroes
{
    /// <summary>
    /// Provides utility methods for working with hero entities.
    /// </summary>
    public static class HeroQueries
    {
        /// <summary>
        /// Finds a hero with the specified heroId, using a case-insensitive comparison.
        /// </summary>
        public static Hero GetHeroById(string heroId)
        {
            return Hero.FindFirst(h => h.StringId.Equals(heroId, StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// Main unified method to find heroes by search string and type flags
        /// </summary>
        /// <param name="searchFilter">Optional case-insensitive substring to filter by name or ID</param>
        /// <param name="requiredTypes">Hero type flags that ALL must match (AND logic)</param>
        /// <param name="matchAll">If true, hero must have ALL flags. If false, hero must have ANY flag</param>
        /// <param name="includeDead">If true, searches dead heroes instead of alive ones</param>
        /// <returns>List of heroes matching all criteria</returns>
        public static List<Hero> FindHeroes(
            string searchFilter = "",
            HeroTypes requiredTypes = HeroTypes.None,
            bool matchAll = true,
            bool includeDead = false)
        {
            IEnumerable<Hero> heroes = includeDead ? Hero.DeadOrDisabledHeroes : Hero.AllAliveHeroes;

            // Filter by name/ID if provided
            if (!string.IsNullOrEmpty(searchFilter))
            {
                string lowerFilter = searchFilter.ToLower();
                heroes = heroes.Where(h =>
                    h.Name.ToString().ToLower().Contains(lowerFilter) ||
                    h.StringId.ToLower().Contains(lowerFilter));
            }

            // Filter by hero types
            if (requiredTypes != HeroTypes.None)
            {
                heroes = heroes.Where(h => matchAll ? h.HasAllTypes(requiredTypes) : h.HasAnyType(requiredTypes));
            }

            return heroes.ToList();
        }

        /// <summary>
        /// Parse a string into HeroTypes enum value
        /// </summary>
        public static HeroTypes ParseHeroType(string typeString)
        {
            if (Enum.TryParse<HeroTypes>(typeString, true, out var result))
                return result;
            return HeroTypes.None;
        }

        /// <summary>
        /// Parse multiple strings and combine into HeroTypes flags
        /// </summary>
        public static HeroTypes ParseHeroTypes(IEnumerable<string> typeStrings)
        {
            HeroTypes combined = HeroTypes.None;
            foreach (var typeString in typeStrings)
            {
                var parsed = ParseHeroType(typeString);
                if (parsed != HeroTypes.None)
                    combined |= parsed;
            }
            return combined;
        }

        /// <summary>
        /// Returns a formatted string listing hero details
        /// </summary>
        public static string GetFormattedDetails(List<Hero> heroes)
        {
            if (heroes.Count == 0)
                return "";
            return string.Join("\n", heroes.Select(h => h.FormattedDetails())) + "\n";
        }
    }
}