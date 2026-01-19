using System;
using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using Bannerlord.GameMaster.Console.Common.Formatting;
using TaleWorlds.Library;

namespace Bannerlord.GameMaster.Heroes
{
	/// <summary>
	/// Provides query methods for searching, finding, filtering, or sorting hero entities.
	/// </summary>
	public static class HeroQueries
	{
		#region Query

		/// <summary>
		/// Finds a hero with the specified heroId, Fast but case sensitive (all string ids SHOULD be lower case) <br />
		/// Use QueryHeroes().FirstOrDefault() to find hero with case insensitive partial name or partial stringIds <br />
		/// Example: QueryHeroes("Henry").FirstOrDefault()
		/// </summary>
		public static Hero GetHeroById(string heroId) => Hero.Find(heroId);

		/// <summary>
		/// Performance focused method to find heroes matching multiple parameters. All parameters are optional and can be used with none, one or a combination of any parameters<br />
		/// Note: <paramref name="query"/> parameter is a string used that will match partial hero names or partial stringIds
		/// </summary>
		/// <param name="query">Optional case-insensitive substring to filter by name or ID</param>
		/// <param name="requiredTypes">Hero type flags that ALL must match (AND logic)</param>
		/// <param name="matchAll">If true, hero must have ALL flags. If false, hero must have ANY flag</param>
		/// <param name="includeDead">If true, searches dead heroes instead of alive ones</param>
		/// <param name="sortBy">Sort field (id, name, age, clan, kingdom, or any HeroType flag)</param>
		/// <param name="sortDescending">True for descending, false for ascending</param>
		/// <returns>List of heroes matching all criteria</returns>
		public static MBReadOnlyList<Hero> QueryHeroes(
			string query = "",
			HeroTypes requiredTypes = HeroTypes.None,
			bool matchAll = true,
			bool includeDead = false,
			string sortBy = "id",
			bool sortDescending = false)
		{
			// Handle player alias
			if (!string.IsNullOrEmpty(query) && query.Equals("player", StringComparison.OrdinalIgnoreCase))
			{
				query = "main_hero";
			}

			// Determine source collection
			MBReadOnlyList<Hero> aliveSource = Hero.AllAliveHeroes;
			MBReadOnlyList<Hero> deadSource = Hero.DeadOrDisabledHeroes;

			bool searchBoth = !matchAll && requiredTypes != HeroTypes.None &&
				(requiredTypes.HasFlag(HeroTypes.Alive) || requiredTypes.HasFlag(HeroTypes.Dead));

			int estimatedCapacity = searchBoth
				? aliveSource.Count + deadSource.Count
				: (includeDead ? deadSource.Count : aliveSource.Count);

			MBReadOnlyList<Hero> results = new(estimatedCapacity);

			bool hasQuery = !string.IsNullOrEmpty(query);
			bool hasTypes = requiredTypes != HeroTypes.None;

			// Filter alive heroes
			if (searchBoth || !includeDead)
			{
				for (int i = 0; i < aliveSource.Count; i++)
				{
					Hero h = aliveSource[i];
					if (MatchesFilters(h, query, hasQuery, requiredTypes, hasTypes, matchAll))
					{
						results.Add(h);
					}
				}
			}

			// Filter dead heroes
			if (searchBoth || includeDead)
			{
				for (int i = 0; i < deadSource.Count; i++)
				{
					Hero h = deadSource[i];
					if (MatchesFilters(h, query, hasQuery, requiredTypes, hasTypes, matchAll))
					{
						results.Add(h);
					}
				}
			}

			// Sorting - only if needed
			if (results.Count > 1)
			{
				IComparer<Hero> comparer = GetHeroComparer(sortBy, sortDescending);
				results.Sort(comparer);
			}

			return results;
		}

		/// <summary>
		/// Check if hero matches all filter criteria
		/// </summary>
		public static bool MatchesFilters(
			Hero h,
			string query,
			bool hasQuery,
			HeroTypes requiredTypes,
			bool hasTypes,
			bool matchAll)
		{
			// Name/ID filter using OrdinalIgnoreCase (no string allocation)
			if (hasQuery)
			{
				bool nameMatch = h.Name.ToString().IndexOf(query, StringComparison.OrdinalIgnoreCase) >= 0;
				bool idMatch = h.StringId.IndexOf(query, StringComparison.OrdinalIgnoreCase) >= 0;
				if (!nameMatch && !idMatch)
					return false;
			}

			// Type filter
			if (hasTypes)
			{
				bool matches = matchAll ? h.HasAllTypes(requiredTypes) : h.HasAnyType(requiredTypes);
				if (!matches)
					return false;
			}

			return true;
		}

		/// <summary>
		/// Get comparer for hero sorting
		/// </summary>
		public static IComparer<Hero> GetHeroComparer(string sortBy, bool descending)
		{
			sortBy = sortBy.ToLowerInvariant();

			// Check if sortBy matches a HeroType flag
			if (Enum.TryParse<HeroTypes>(sortBy, true, out HeroTypes heroType) && heroType != HeroTypes.None)
			{
				return Comparer<Hero>.Create((a, b) =>
				{
					bool aHas = a.GetHeroTypes().HasFlag(heroType);
					bool bHas = b.GetHeroTypes().HasFlag(heroType);
					int result = aHas.CompareTo(bHas);
					return descending ? -result : result;
				});
			}

			// Standard field comparers
			return sortBy switch
			{
				"name" => Comparer<Hero>.Create((a, b) =>
				{
					int result = string.Compare(a.Name.ToString(), b.Name.ToString(), StringComparison.Ordinal);
					return descending ? -result : result;
				}),
				"age" => Comparer<Hero>.Create((a, b) =>
				{
					int result = a.Age.CompareTo(b.Age);
					return descending ? -result : result;
				}),
				"clan" => Comparer<Hero>.Create((a, b) =>
				{
					int result = string.Compare(
						a.Clan?.Name?.ToString() ?? "",
						b.Clan?.Name?.ToString() ?? "",
						StringComparison.Ordinal);
					return descending ? -result : result;
				}),
				"kingdom" => Comparer<Hero>.Create((a, b) =>
				{
					int result = string.Compare(
						a.Clan?.Kingdom?.Name?.ToString() ?? "",
						b.Clan?.Kingdom?.Name?.ToString() ?? "",
						StringComparison.Ordinal);
					return descending ? -result : result;
				}),
				_ => Comparer<Hero>.Create((a, b) =>  // default: id
				{
					int result = string.Compare(a.StringId, b.StringId, StringComparison.Ordinal);
					return descending ? -result : result;
				})
			};
		}


		#endregion

		#region Parsing / Formatting

		/// <summary>
		/// Parse a string into HeroTypes enum value
		/// </summary>
		public static HeroTypes ParseHeroType(string typeString)
		{
			if (Enum.TryParse<HeroTypes>(typeString, true, out HeroTypes result))
				return result;
			return HeroTypes.None;
		}

		/// <summary>
		/// Parse multiple strings and combine into HeroTypes flags
		/// </summary>
		public static HeroTypes ParseHeroTypes(IEnumerable<string> typeStrings)
		{
			HeroTypes combined = HeroTypes.None;
			foreach (string typeString in typeStrings)
			{
				HeroTypes parsed = ParseHeroType(typeString);
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
		#endregion
	}
}