using System;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using Bannerlord.GameMaster.Common.Interfaces;
using Bannerlord.GameMaster.Console.Common;

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
        /// <param name="query">Optional case-insensitive substring to filter by name or ID</param>
        /// <param name="requiredTypes">Hero type flags that ALL must match (AND logic)</param>
        /// <param name="matchAll">If true, hero must have ALL flags. If false, hero must have ANY flag</param>
        /// <param name="includeDead">If true, searches dead heroes instead of alive ones</param>
        /// <param name="sortBy">Sort field (id, name, age, clan, kingdom, or any HeroType flag)</param>
        /// <param name="sortDescending">True for descending, false for ascending</param>
        /// <returns>List of heroes matching all criteria</returns>
        public static List<Hero> QueryHeroes(
            string query = "",
            HeroTypes requiredTypes = HeroTypes.None,
            bool matchAll = true,
            bool includeDead = false,
            string sortBy = "id",
            bool sortDescending = false)
        {
            // Handle player alias - "player" is an alias for "main_hero"
            if (!string.IsNullOrEmpty(query) && query.Equals("player", StringComparison.OrdinalIgnoreCase))
            {
                query = "main_hero";
            }

            IEnumerable<Hero> heroes;
            
            // When using OR logic with life status flags (Alive or Dead), search both collections
            // to ensure we find all matching heroes regardless of life status
            if (!matchAll && requiredTypes != HeroTypes.None &&
                (requiredTypes.HasFlag(HeroTypes.Alive) || requiredTypes.HasFlag(HeroTypes.Dead)))
            {
                // Search both alive and dead heroes for OR queries involving life status
                heroes = Hero.AllAliveHeroes.Concat(Hero.DeadOrDisabledHeroes);
            }
            else
            {
                // For AND queries or queries without life status flags, use the standard collection
                heroes = includeDead ? Hero.DeadOrDisabledHeroes : Hero.AllAliveHeroes;
            }

            // Filter by name/ID if provided
            if (!string.IsNullOrEmpty(query))
            {
                string lowerFilter = query.ToLower();
                heroes = heroes.Where(h =>
                    h.Name.ToString().ToLower().Contains(lowerFilter) ||
                    h.StringId.ToLower().Contains(lowerFilter));
            }

            // Filter by hero types
            if (requiredTypes != HeroTypes.None)
            {
                heroes = heroes.Where(h => matchAll ? h.HasAllTypes(requiredTypes) : h.HasAnyType(requiredTypes));
            }

            // Apply sorting
            heroes = ApplySorting(heroes, sortBy, sortDescending);

            return heroes.ToList();
        }

        /// <summary>
        /// Apply sorting to heroes collection
        /// </summary>
        private static IEnumerable<Hero> ApplySorting(
            IEnumerable<Hero> heroes,
            string sortBy,
            bool descending)
        {
            sortBy = sortBy.ToLower();

            // Check if sortBy matches a HeroType flag
            if (Enum.TryParse<HeroTypes>(sortBy, true, out var heroType) && heroType != HeroTypes.None)
            {
                // Sort by whether hero has this type flag
                return descending
                    ? heroes.OrderByDescending(h => h.GetHeroTypes().HasFlag(heroType))
                    : heroes.OrderBy(h => h.GetHeroTypes().HasFlag(heroType));
            }

            // Sort by standard fields
            IOrderedEnumerable<Hero> orderedHeroes = sortBy switch
            {
                "name" => descending
                    ? heroes.OrderByDescending(h => h.Name.ToString())
                    : heroes.OrderBy(h => h.Name.ToString()),
                "age" => descending
                    ? heroes.OrderByDescending(h => h.Age)
                    : heroes.OrderBy(h => h.Age),
                "clan" => descending
                    ? heroes.OrderByDescending(h => h.Clan?.Name?.ToString() ?? "")
                    : heroes.OrderBy(h => h.Clan?.Name?.ToString() ?? ""),
                "kingdom" => descending
                    ? heroes.OrderByDescending(h => h.Clan?.Kingdom?.Name?.ToString() ?? "")
                    : heroes.OrderBy(h => h.Clan?.Kingdom?.Name?.ToString() ?? ""),
                _ => descending  // default to id
                    ? heroes.OrderByDescending(h => h.StringId)
                    : heroes.OrderBy(h => h.StringId)
            };

            return orderedHeroes;
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
        /// Returns a formatted string listing hero details with aligned columns
        /// </summary>
        public static string GetFormattedDetails(List<Hero> heroes)
        {
            if (heroes.Count == 0)
                return "";

            return ColumnFormatter<Hero>.FormatList(
                heroes,
                h => h.StringId,
                h => h.Name.ToString(),
                h => $"Culture: {h.Culture?.Name?.ToString() ?? "None"}",
                h => $"Level: {h.Level}",
                h => $"Gender: {(h.IsFemale ? "Female" : "Male")}",
                h => $"Clan: {h.Clan?.Name?.ToString() ?? "None"}",
                h => $"Kingdom: {h.Clan?.Kingdom?.Name?.ToString() ?? "None"}"
            );
        }
 }

 /// <summary>
 /// Wrapper class implementing IEntityQueries interface for Hero entities
 /// </summary>
 public class HeroQueriesWrapper : IEntityQueries<Hero, HeroTypes>
 {
  public Hero GetById(string id) => HeroQueries.GetHeroById(id);
  public List<Hero> Query(string query, HeroTypes types, bool matchAll) => HeroQueries.QueryHeroes(query, types, matchAll);
  public HeroTypes ParseType(string typeString) => HeroQueries.ParseHeroType(typeString);
  public HeroTypes ParseTypes(IEnumerable<string> typeStrings) => HeroQueries.ParseHeroTypes(typeStrings);
  public string GetFormattedDetails(List<Hero> entities) => HeroQueries.GetFormattedDetails(entities);
 }
}