using System;
using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using Bannerlord.GameMaster.Console.Common.Formatting;
using TaleWorlds.Library;

namespace Bannerlord.GameMaster.Troops
{
	/// <summary>
	/// Provides query methods for searching, finding, filtering, or sorting troop entities.
	/// CRITICAL: Heroes/Lords are NEVER troops - they are automatically excluded.
	/// </summary>
	public static class TroopQueries
	{
		#region Query

		/// <summary>
		/// Finds a troop with the specified troopId, Fast but case sensitive (all string ids SHOULD be lower case) <br />
		/// Use QueryTroops().FirstOrDefault() to find troop with case insensitive partial name or partial stringIds <br />
		/// Example: QueryTroops("henry").FirstOrDefault()
		/// </summary>
		public static CharacterObject GetTroopById(string troopId) => CharacterObject.Find(troopId);

		/// <summary>
		/// Performance focused method to find troops matching multiple parameters. All parameters are optional and can be used with none, one or a combination of any parameters<br />
		/// Note: <paramref name="query"/> parameter is a string used that will match partial troop names or partial stringIds
		/// CRITICAL: This method only returns actual combat troops (via IsActualTroop check), excluding NPCs, templates, and other non-troops.
		/// </summary>
		/// <param name="query">Optional case-insensitive substring to filter by name or ID</param>
		/// <param name="requiredTypes">Troop type flags that ALL must match (AND logic)</param>
		/// <param name="matchAll">If true, troop must have ALL flags. If false, troop must have ANY flag</param>
		/// <param name="tierFilter">Optional tier filter (0-6+, -1 for no filter)</param>
		/// <param name="sortBy">Sort field (id, name, tier, level, culture, formation, or any TroopTypes flag)</param>
		/// <param name="sortDescending">True for descending, false for ascending</param>
		/// <returns>List of troops matching all criteria</returns>
		public static MBReadOnlyList<CharacterObject> QueryTroops(
			string query = "",
			TroopTypes requiredTypes = TroopTypes.None,
			bool matchAll = true,
			int tierFilter = -1,
			string sortBy = "id",
			bool sortDescending = false)
		{
			// Get all character objects and filter to actual troops only
			MBReadOnlyList<CharacterObject> allCharacters = CharacterObject.All;
			
			int estimatedCapacity = allCharacters.Count;
			MBReadOnlyList<CharacterObject> results = new(estimatedCapacity);

			bool hasQuery = !string.IsNullOrEmpty(query);
			bool hasTypes = requiredTypes != TroopTypes.None;
			bool hasTierFilter = tierFilter >= 0;

			// Filter troops
			for (int i = 0; i < allCharacters.Count; i++)
			{
				CharacterObject troop = allCharacters[i];
				
				// CRITICAL: Only include actual combat troops
				if (!troop.IsActualTroop())
					continue;

				if (MatchesFilters(troop, query, hasQuery, requiredTypes, hasTypes, matchAll, tierFilter, hasTierFilter))
				{
					results.Add(troop);
				}
			}

			// Sorting - only if needed
			if (results.Count > 1)
			{
				IComparer<CharacterObject> comparer = GetTroopComparer(sortBy, sortDescending);
				results.Sort(comparer);
			}

			return results;
		}

		/// <summary>
		/// Check if troop matches all filter criteria
		/// </summary>
		public static bool MatchesFilters(
			CharacterObject troop,
			string query,
			bool hasQuery,
			TroopTypes requiredTypes,
			bool hasTypes,
			bool matchAll,
			int tierFilter,
			bool hasTierFilter)
		{
			// Name/ID filter using OrdinalIgnoreCase (no string allocation)
			if (hasQuery)
			{
				bool nameMatch = troop.Name.ToString().IndexOf(query, StringComparison.OrdinalIgnoreCase) >= 0;
				bool idMatch = troop.StringId.IndexOf(query, StringComparison.OrdinalIgnoreCase) >= 0;
				if (!nameMatch && !idMatch)
					return false;
			}

			// Tier filter (exact match using GetBattleTier())
			if (hasTierFilter)
			{
				if (troop.GetBattleTier() != tierFilter)
					return false;
			}

			// Type filter
			if (hasTypes)
			{
				bool matches = matchAll ? troop.HasAllTypes(requiredTypes) : troop.HasAnyType(requiredTypes);
				if (!matches)
					return false;
			}

			return true;
		}

		/// <summary>
		/// Get comparer for troop sorting (uses case-insensitive comparison, no string allocation)
		/// </summary>
		public static IComparer<CharacterObject> GetTroopComparer(string sortBy, bool descending)
		{
			// Check if sortBy matches a TroopTypes flag (case-insensitive parse, no allocation)
			if (Enum.TryParse<TroopTypes>(sortBy, true, out TroopTypes troopType) && troopType != TroopTypes.None)
			{
				return Comparer<CharacterObject>.Create((a, b) =>
				{
					bool aHas = a.GetTroopTypes().HasFlag(troopType);
					bool bHas = b.GetTroopTypes().HasFlag(troopType);
					int result = aHas.CompareTo(bHas);
					return descending ? -result : result;
				});
			}

			// Standard field comparers - use OrdinalIgnoreCase to avoid ToLowerInvariant() allocation
			if (string.Equals(sortBy, "name", StringComparison.OrdinalIgnoreCase))
			{
				return Comparer<CharacterObject>.Create((a, b) =>
				{
					int result = string.Compare(a.Name.ToString(), b.Name.ToString(), StringComparison.Ordinal);
					return descending ? -result : result;
				});
			}
			
			if (string.Equals(sortBy, "tier", StringComparison.OrdinalIgnoreCase))
			{
				return Comparer<CharacterObject>.Create((a, b) =>
				{
					int result = a.Tier.CompareTo(b.Tier);
					return descending ? -result : result;
				});
			}
			
			if (string.Equals(sortBy, "level", StringComparison.OrdinalIgnoreCase))
			{
				return Comparer<CharacterObject>.Create((a, b) =>
				{
					int result = a.Level.CompareTo(b.Level);
					return descending ? -result : result;
				});
			}
			
			if (string.Equals(sortBy, "culture", StringComparison.OrdinalIgnoreCase))
			{
				return Comparer<CharacterObject>.Create((a, b) =>
				{
					int result = string.Compare(
						a.Culture?.Name?.ToString() ?? "",
						b.Culture?.Name?.ToString() ?? "",
						StringComparison.Ordinal);
					return descending ? -result : result;
				});
			}
			
			if (string.Equals(sortBy, "formation", StringComparison.OrdinalIgnoreCase))
			{
				return Comparer<CharacterObject>.Create((a, b) =>
				{
					int result = a.DefaultFormationClass.CompareTo(b.DefaultFormationClass);
					return descending ? -result : result;
				});
			}
			
			// Default: sort by id
			return Comparer<CharacterObject>.Create((a, b) =>
			{
				int result = string.Compare(a.StringId, b.StringId, StringComparison.Ordinal);
				return descending ? -result : result;
			});
		}

		#endregion

		#region Parsing / Formatting

		/// <summary>
		/// Parse a string into TroopTypes enum value
		/// </summary>
		public static TroopTypes ParseTroopType(string typeString)
		{
			if (Enum.TryParse<TroopTypes>(typeString, true, out TroopTypes result))
				return result;
			return TroopTypes.None;
		}

		/// <summary>
		/// Parse multiple strings and combine into TroopTypes flags
		/// </summary>
		public static TroopTypes ParseTroopTypes(IEnumerable<string> typeStrings)
		{
			TroopTypes combined = TroopTypes.None;
			foreach (string typeString in typeStrings)
			{
				TroopTypes parsed = ParseTroopType(typeString);
				if (parsed != TroopTypes.None)
					combined |= parsed;
			}
			
			return combined;
		}

		/// <summary>
		/// Returns a formatted string listing troop details with aligned columns
		/// </summary>
		public static string GetFormattedDetails(List<CharacterObject> troops)
		{
			if (troops.Count == 0)
				return "";

			return ColumnFormatter<CharacterObject>.FormatList(
				troops,
				t => t.StringId,
				t => t.Name.ToString(),
				t => $"Gender: {(t.IsFemale ? "Female" : "Male")}",
				t => $"Tier: {t.GetBattleTier()}",
				t => $"Level: {t.Level}",
				t => $"Culture: {t.Culture?.Name?.ToString() ?? "None"}",
				t => $"Formation: {t.DefaultFormationClass}"
			);
		}

		#endregion
	}
}
