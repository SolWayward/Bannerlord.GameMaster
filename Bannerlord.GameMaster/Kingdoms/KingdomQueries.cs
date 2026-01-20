using System;
using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using Bannerlord.GameMaster.Console.Common.Formatting;
using TaleWorlds.Library;
using TaleWorlds.ObjectSystem;

namespace Bannerlord.GameMaster.Kingdoms
{
	/// <summary>
	/// Provides query methods for searching, finding, filtering, or sorting kingdom entities.
	/// </summary>
	public static class KingdomQueries
	{
		#region Query

		/// <summary>
		/// Finds a kingdom with the specified kingdomId, Fast but case sensitive (all string ids SHOULD be lower case) <br />
		/// Use QueryKingdoms().FirstOrDefault() to find kingdom with case insensitive partial name or partial stringIds <br />
		/// Example: QueryKingdoms("Vlandia").FirstOrDefault()
		/// </summary>
		public static Kingdom GetKingdomById(string kingdomId) => Campaign.Current.CampaignObjectManager.Find<Kingdom>(kingdomId);

		/// <summary>
		/// Performance focused method to find kingdoms matching multiple parameters. All parameters are optional and can be used with none, one or a combination of any parameters<br />
		/// Note: <paramref name="query"/> parameter is a string used that will match partial kingdom names or partial stringIds
		/// </summary>
		/// <param name="query">Optional case-insensitive substring to filter by name or ID</param>
		/// <param name="requiredTypes">Kingdom type flags that ALL must match (AND logic)</param>
		/// <param name="matchAll">If true, kingdom must have ALL flags. If false, kingdom must have ANY flag</param>
		/// <param name="sortBy">Sort field (id, name, clans, heroes, fiefs, strength, ruler, or any KingdomType flag)</param>
		/// <param name="sortDescending">True for descending, false for ascending</param>
		/// <returns>List of kingdoms matching all criteria</returns>
		public static MBReadOnlyList<Kingdom> QueryKingdoms(
			string query = "",
			KingdomTypes requiredTypes = KingdomTypes.None,
			bool matchAll = true,
			string sortBy = "id",
			bool sortDescending = false)
		{
			// Filter kingdoms
			MBReadOnlyList<Kingdom> source = Kingdom.All;
			MBReadOnlyList<Kingdom> results = new(source.Count);

			bool hasQuery = !string.IsNullOrEmpty(query);
			bool hasTypes = requiredTypes != KingdomTypes.None;

			for (int i = 0; i < source.Count; i++)
			{
				Kingdom k = source[i];
				if (MatchesFilters(k, query, hasQuery, requiredTypes, hasTypes, matchAll))
				{
					results.Add(k);
				}
			}

			// Sorting - only if needed
			if (results.Count > 1)
			{
				IComparer<Kingdom> comparer = GetKingdomComparer(sortBy, sortDescending);
				results.Sort(comparer);
			}

			return results;
		}

		/// <summary>
		/// Check if kingdom matches all filter criteria
		/// </summary>
		public static bool MatchesFilters(
			Kingdom k,
			string query,
			bool hasQuery,
			KingdomTypes requiredTypes,
			bool hasTypes,
			bool matchAll)
		{
			// Name/ID filter using OrdinalIgnoreCase (no string allocation)
			if (hasQuery)
			{
				bool nameMatch = k.Name.ToString().IndexOf(query, StringComparison.OrdinalIgnoreCase) >= 0;
				bool idMatch = k.StringId.IndexOf(query, StringComparison.OrdinalIgnoreCase) >= 0;
				if (!nameMatch && !idMatch)
					return false;
			}

			// Type filter
			if (hasTypes)
			{
				bool matches = matchAll ? k.HasAllTypes(requiredTypes) : k.HasAnyType(requiredTypes);
				if (!matches)
					return false;
			}

			return true;
		}

		/// <summary>
		/// Get comparer for kingdom sorting
		/// </summary>
		public static IComparer<Kingdom> GetKingdomComparer(string sortBy, bool descending)
		{
			sortBy = sortBy.ToLowerInvariant();

			// Check if sortBy matches a KingdomType flag
			if (Enum.TryParse<KingdomTypes>(sortBy, true, out KingdomTypes kingdomType) && kingdomType != KingdomTypes.None)
			{
				return Comparer<Kingdom>.Create((a, b) =>
				{
					bool aHas = a.GetKingdomTypes().HasFlag(kingdomType);
					bool bHas = b.GetKingdomTypes().HasFlag(kingdomType);
					int result = aHas.CompareTo(bHas);
					return descending ? -result : result;
				});
			}

			// Standard field comparers
			return sortBy switch
			{
				"name" => Comparer<Kingdom>.Create((a, b) =>
				{
					int result = string.Compare(a.Name.ToString(), b.Name.ToString(), StringComparison.Ordinal);
					return descending ? -result : result;
				}),
				"clans" => Comparer<Kingdom>.Create((a, b) =>
				{
					int result = a.Clans.Count.CompareTo(b.Clans.Count);
					return descending ? -result : result;
				}),
				"heroes" => Comparer<Kingdom>.Create((a, b) =>
				{
					int result = a.Heroes.Count.CompareTo(b.Heroes.Count);
					return descending ? -result : result;
				}),
				"fiefs" => Comparer<Kingdom>.Create((a, b) =>
				{
					int result = a.Fiefs.Count.CompareTo(b.Fiefs.Count);
					return descending ? -result : result;
				}),
				"strength" => Comparer<Kingdom>.Create((a, b) =>
				{
					int result = a.CurrentTotalStrength.CompareTo(b.CurrentTotalStrength);
					return descending ? -result : result;
				}),
				"ruler" => Comparer<Kingdom>.Create((a, b) =>
				{
					int result = string.Compare(
						a.Leader?.Name?.ToString() ?? "",
						b.Leader?.Name?.ToString() ?? "",
						StringComparison.Ordinal);
					return descending ? -result : result;
				}),
				_ => Comparer<Kingdom>.Create((a, b) =>  // default: id
				{
					int result = string.Compare(a.StringId, b.StringId, StringComparison.Ordinal);
					return descending ? -result : result;
				})
			};
		}

		#endregion

		#region Parsing / Formatting

		/// <summary>
		/// Parse a string into KingdomTypes enum value
		/// </summary>
		public static KingdomTypes ParseKingdomType(string typeString)
		{
			if (Enum.TryParse<KingdomTypes>(typeString, true, out KingdomTypes result))
				return result;
			return KingdomTypes.None;
		}

		/// <summary>
		/// Parse multiple strings and combine into KingdomTypes flags
		/// </summary>
		public static KingdomTypes ParseKingdomTypes(IEnumerable<string> typeStrings)
		{
			KingdomTypes combined = KingdomTypes.None;
			foreach (string typeString in typeStrings)
			{
				KingdomTypes parsed = ParseKingdomType(typeString);
				if (parsed != KingdomTypes.None)
					combined |= parsed;
			}
			
			return combined;
		}

		/// <summary>
		/// Returns a formatted string listing kingdom details with aligned columns
		/// </summary>
		public static string GetFormattedDetails(List<Kingdom> kingdoms)
		{
			if (kingdoms.Count == 0)
				return "";

			return ColumnFormatter<Kingdom>.FormatList(
				kingdoms,
				k => k.StringId,
				k => k.Name.ToString(),
				k => $"Clans: {k.Clans.Count}",
				k => $"Heroes: {k.Heroes.Count}",
				k => $"Fiefs: {k.Fiefs.Count}",
				k => $"Ruler: {k.Leader?.Name?.ToString() ?? "None"}"
			);
		}

		#endregion
	}
}
