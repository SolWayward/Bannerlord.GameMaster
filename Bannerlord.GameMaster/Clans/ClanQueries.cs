using System;
using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using Bannerlord.GameMaster.Console.Common.Formatting;
using TaleWorlds.Library;

namespace Bannerlord.GameMaster.Clans
{
	/// <summary>
	/// Provides query methods for searching, finding, filtering, or sorting clan entities.
	/// </summary>
	public static class ClanQueries
	{
		#region Query

		/// <summary>
		/// Finds a clan with the specified clanId, Fast but case sensitive (all string ids SHOULD be lower case) <br />
		/// Use QueryClans().FirstOrDefault() to find clan with case insensitive partial name or partial stringIds <br />
		/// Example: QueryClans("vlandia").FirstOrDefault()
		/// </summary>
		public static Clan GetClanById(string clanId) => Campaign.Current.CampaignObjectManager.Find<Clan>(clanId);

		/// <summary>
		/// Performance focused method to find clans matching multiple parameters. All parameters are optional and can be used with none, one or a combination of any parameters<br />
		/// Note: <paramref name="query"/> parameter is a string used that will match partial clan names or partial stringIds
		/// </summary>
		/// <param name="query">Optional case-insensitive substring to filter by name or ID</param>
		/// <param name="requiredTypes">Clan type flags that ALL must match (AND logic)</param>
		/// <param name="matchAll">If true, clan must have ALL flags. If false, clan must have ANY flag</param>
		/// <param name="sortBy">Sort field (id, name, tier, gold, renown, kingdom, leader, heroes, or any ClanType flag)</param>
		/// <param name="sortDescending">True for descending, false for ascending</param>
		/// <returns>MBReadOnlyList of clans matching all criteria</returns>
		public static MBReadOnlyList<Clan> QueryClans(
			string query = "",
			ClanTypes requiredTypes = ClanTypes.None,
			bool matchAll = true,
			string sortBy = "id",
			bool sortDescending = false)
		{
			MBReadOnlyList<Clan> source = Clan.All;
			int estimatedCapacity = source.Count;
			MBReadOnlyList<Clan> results = new(estimatedCapacity);

			bool hasQuery = !string.IsNullOrEmpty(query);
			bool hasTypes = requiredTypes != ClanTypes.None;

			// Filter clans
			for (int i = 0; i < source.Count; i++)
			{
				Clan c = source[i];
				if (MatchesFilters(c, query, hasQuery, requiredTypes, hasTypes, matchAll))
				{
					results.Add(c);
				}
			}

			// Sorting - only if needed
			if (results.Count > 1)
			{
				IComparer<Clan> comparer = GetClanComparer(sortBy, sortDescending);
				results.Sort(comparer);
			}

			return results;
		}

		/// <summary>
		/// Check if clan matches all filter criteria
		/// </summary>
		public static bool MatchesFilters(
			Clan c,
			string query,
			bool hasQuery,
			ClanTypes requiredTypes,
			bool hasTypes,
			bool matchAll)
		{
			// Name/ID filter using OrdinalIgnoreCase (no string allocation)
			if (hasQuery)
			{
				bool nameMatch = c.Name.ToString().IndexOf(query, StringComparison.OrdinalIgnoreCase) >= 0;
				bool idMatch = c.StringId.IndexOf(query, StringComparison.OrdinalIgnoreCase) >= 0;
				if (!nameMatch && !idMatch)
					return false;
			}

			// Type filter
			if (hasTypes)
			{
				bool matches = matchAll ? c.HasAllTypes(requiredTypes) : c.HasAnyType(requiredTypes);
				if (!matches)
					return false;
			}

			return true;
		}

		/// <summary>
		/// Get comparer for clan sorting
		/// </summary>
		public static IComparer<Clan> GetClanComparer(string sortBy, bool descending)
		{
			sortBy = sortBy.ToLowerInvariant();

			// Check if sortBy matches a ClanType flag
			if (Enum.TryParse<ClanTypes>(sortBy, true, out ClanTypes clanType) && clanType != ClanTypes.None)
			{
				return Comparer<Clan>.Create((a, b) =>
				{
					bool aHas = a.GetClanTypes().HasFlag(clanType);
					bool bHas = b.GetClanTypes().HasFlag(clanType);
					int result = aHas.CompareTo(bHas);
					return descending ? -result : result;
				});
			}

			// Standard field comparers
			return sortBy switch
			{
				"name" => Comparer<Clan>.Create((a, b) =>
				{
					int result = string.Compare(a.Name.ToString(), b.Name.ToString(), StringComparison.Ordinal);
					return descending ? -result : result;
				}),
				"tier" => Comparer<Clan>.Create((a, b) =>
				{
					int result = a.Tier.CompareTo(b.Tier);
					return descending ? -result : result;
				}),
				"gold" => Comparer<Clan>.Create((a, b) =>
				{
					int result = a.Gold.CompareTo(b.Gold);
					return descending ? -result : result;
				}),
				"renown" => Comparer<Clan>.Create((a, b) =>
				{
					int result = a.Renown.CompareTo(b.Renown);
					return descending ? -result : result;
				}),
				"kingdom" => Comparer<Clan>.Create((a, b) =>
				{
					int result = string.Compare(
						a.Kingdom?.Name?.ToString() ?? "",
						b.Kingdom?.Name?.ToString() ?? "",
						StringComparison.Ordinal);
					return descending ? -result : result;
				}),
				"leader" => Comparer<Clan>.Create((a, b) =>
				{
					int result = string.Compare(
						a.Leader?.Name?.ToString() ?? "",
						b.Leader?.Name?.ToString() ?? "",
						StringComparison.Ordinal);
					return descending ? -result : result;
				}),
				"heroes" => Comparer<Clan>.Create((a, b) =>
				{
					int result = a.Heroes.Count.CompareTo(b.Heroes.Count);
					return descending ? -result : result;
				}),
				_ => Comparer<Clan>.Create((a, b) =>  // default: id
				{
					int result = string.Compare(a.StringId, b.StringId, StringComparison.Ordinal);
					return descending ? -result : result;
				})
			};
		}

		#endregion

		#region Parsing / Formatting

		/// <summary>
		/// Parse a string into ClanTypes enum value
		/// </summary>
		public static ClanTypes ParseClanType(string typeString)
		{
			// Handle special cases and aliases
			string normalizedType = typeString.ToLower() switch
			{
				"active" => "Active",
				"eliminated" => "Eliminated",
				"bandit" => "Bandit",
				"nonbandit" => "NonBandit",
				"mapfaction" => "MapFaction",
				"noble" => "Noble",
				"minor" => "MinorFaction",
				"minorfaction" => "MinorFaction",
				"rebel" => "Rebel",
				"mercenary" => "Mercenary",
				"merc" => "Mercenary",
				"undermercenaryservice" => "UnderMercenaryService",
				"mafia" => "Mafia",
				"outlaw" => "Outlaw",
				"nomad" => "Nomad",
				"sect" => "Sect",
				"withoutkingdom" => "WithoutKingdom",
				"empty" => "Empty",
				"player" => "PlayerClan",
				"playerclan" => "PlayerClan",
				_ => typeString
			};

			if (Enum.TryParse<ClanTypes>(normalizedType, true, out ClanTypes result))
				return result;
			return ClanTypes.None;
		}

		/// <summary>
		/// Parse multiple strings and combine into ClanTypes flags
		/// </summary>
		public static ClanTypes ParseClanTypes(IEnumerable<string> typeStrings)
		{
			ClanTypes combined = ClanTypes.None;
			foreach (string typeString in typeStrings)
			{
				ClanTypes parsed = ParseClanType(typeString);
				if (parsed != ClanTypes.None)
					combined |= parsed;
			}
			
			return combined;
		}

		/// <summary>
		/// Returns a formatted string listing clan details with aligned columns
		/// </summary>
		public static string GetFormattedDetails(List<Clan> clans)
		{
			if (clans.Count == 0)
				return "";

			return ColumnFormatter<Clan>.FormatList(
				clans,
				c => c.StringId,
				c => c.Name.ToString(),
				c => $"Culture: {c.Culture?.Name?.ToString() ?? "None"}",
				c => $"Heroes: {c.Heroes.Count}",
				c => $"Leader: {c.Leader?.Name?.ToString() ?? "None"}",
				c => $"Kingdom: {c.Kingdom?.Name?.ToString() ?? "None"}"
			);
		}

		#endregion
	}
}
