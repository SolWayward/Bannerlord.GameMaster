using System;
using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Settlements;
using Bannerlord.GameMaster.Console.Common.Formatting;
using TaleWorlds.Library;

namespace Bannerlord.GameMaster.Settlements
{
	/// <summary>
	/// Provides query methods for searching, finding, filtering, or sorting settlement entities.
	/// </summary>
	public static class SettlementQueries
	{
		#region Query

		/// <summary>
		/// Finds a settlement with the specified settlementId, Fast but case sensitive (all string ids SHOULD be lower case) <br />
		/// Use QuerySettlements().FirstOrDefault() to find settlement with case insensitive partial name or partial stringIds <br />
		/// Example: QuerySettlements("Maracanda").FirstOrDefault()
		/// </summary>
		public static Settlement GetSettlementById(string settlementId) => Settlement.Find(settlementId);

		/// <summary>
		/// Performance focused method to find settlements matching multiple parameters. All parameters are optional and can be used with none, one or a combination of any parameters<br />
		/// Note: <paramref name="query"/> parameter is a string used that will match partial settlement names or partial stringIds
		/// </summary>
		/// <param name="query">Optional case-insensitive substring to filter by name or ID</param>
		/// <param name="requiredTypes">Settlement type flags that ALL must match (AND logic)</param>
		/// <param name="matchAll">If true, settlement must have ALL flags. If false, settlement must have ANY flag</param>
		/// <param name="sortBy">Sort field (id, name, type, owner, kingdom, culture, prosperity, or any SettlementType flag)</param>
		/// <param name="sortDescending">True for descending, false for ascending</param>
		/// <returns>List of settlements matching all criteria</returns>
		public static MBReadOnlyList<Settlement> QuerySettlements(
			string query = "",
			SettlementTypes requiredTypes = SettlementTypes.None,
			bool matchAll = true,
			string sortBy = "id",
			bool sortDescending = false)
		{
			// Determine source collection
			MBReadOnlyList<Settlement> source = Settlement.All;

			int estimatedCapacity = source.Count;
			MBReadOnlyList<Settlement> results = new(estimatedCapacity);

			bool hasQuery = !string.IsNullOrEmpty(query);
			bool hasTypes = requiredTypes != SettlementTypes.None;

			// Filter settlements
			for (int i = 0; i < source.Count; i++)
			{
				Settlement s = source[i];
				if (MatchesFilters(s, query, hasQuery, requiredTypes, hasTypes, matchAll))
				{
					results.Add(s);
				}
			}

			// Sorting - only if needed
			if (results.Count > 1)
			{
				IComparer<Settlement> comparer = GetSettlementComparer(sortBy, sortDescending);
				results.Sort(comparer);
			}

			return results;
		}

		/// <summary>
		/// Check if settlement matches all filter criteria
		/// </summary>
		public static bool MatchesFilters(
			Settlement s,
			string query,
			bool hasQuery,
			SettlementTypes requiredTypes,
			bool hasTypes,
			bool matchAll)
		{
			// Name/ID filter using OrdinalIgnoreCase (no string allocation)
			if (hasQuery)
			{
				bool nameMatch = s.Name.ToString().IndexOf(query, StringComparison.OrdinalIgnoreCase) >= 0;
				bool idMatch = s.StringId.IndexOf(query, StringComparison.OrdinalIgnoreCase) >= 0;
				if (!nameMatch && !idMatch)
					return false;
			}

			// Type filter
			if (hasTypes)
			{
				bool matches = matchAll ? s.HasAllTypes(requiredTypes) : s.HasAnyType(requiredTypes);
				if (!matches)
					return false;
			}

			return true;
		}

		/// <summary>
		/// Get comparer for settlement sorting
		/// </summary>
		public static IComparer<Settlement> GetSettlementComparer(string sortBy, bool descending)
		{
			sortBy = sortBy.ToLowerInvariant();

			// Check if sortBy matches a SettlementType flag
			if (Enum.TryParse<SettlementTypes>(sortBy, true, out SettlementTypes settlementType) && settlementType != SettlementTypes.None)
			{
				return Comparer<Settlement>.Create((a, b) =>
				{
					bool aHas = a.GetSettlementTypes().HasFlag(settlementType);
					bool bHas = b.GetSettlementTypes().HasFlag(settlementType);
					int result = aHas.CompareTo(bHas);
					return descending ? -result : result;
				});
			}

			// Standard field comparers
			return sortBy switch
			{
				"name" => Comparer<Settlement>.Create((a, b) =>
				{
					int result = string.Compare(a.Name.ToString(), b.Name.ToString(), StringComparison.Ordinal);
					return descending ? -result : result;
				}),
				"type" => Comparer<Settlement>.Create((a, b) =>
				{
					string aType = GetSettlementType(a);
					string bType = GetSettlementType(b);
					int result = string.Compare(aType, bType, StringComparison.Ordinal);
					return descending ? -result : result;
				}),
				"owner" => Comparer<Settlement>.Create((a, b) =>
				{
					int result = string.Compare(
						a.OwnerClan?.Name?.ToString() ?? "",
						b.OwnerClan?.Name?.ToString() ?? "",
						StringComparison.Ordinal);
					return descending ? -result : result;
				}),
				"kingdom" => Comparer<Settlement>.Create((a, b) =>
				{
					int result = string.Compare(
						a.MapFaction?.Name?.ToString() ?? "",
						b.MapFaction?.Name?.ToString() ?? "",
						StringComparison.Ordinal);
					return descending ? -result : result;
				}),
				"culture" => Comparer<Settlement>.Create((a, b) =>
				{
					int result = string.Compare(
						a.Culture?.Name?.ToString() ?? "",
						b.Culture?.Name?.ToString() ?? "",
						StringComparison.Ordinal);
					return descending ? -result : result;
				}),
				"prosperity" => Comparer<Settlement>.Create((a, b) =>
				{
					float aProsperity = GetProsperityValue(a);
					float bProsperity = GetProsperityValue(b);
					int result = aProsperity.CompareTo(bProsperity);
					return descending ? -result : result;
				}),
				_ => Comparer<Settlement>.Create((a, b) =>  // default: id
				{
					int result = string.Compare(a.StringId, b.StringId, StringComparison.Ordinal);
					return descending ? -result : result;
				})
			};
		}

		#endregion

		#region Parsing / Formatting

		/// <summary>
		/// Parse a string into SettlementTypes enum value
		/// </summary>
		public static SettlementTypes ParseSettlementType(string typeString)
		{
			// Handle common aliases
			string normalized = typeString.ToLower() switch
			{
				"town" => "Town",
				"castle" => "Castle",
				"city" => "City",
				"village" => "Village",
				"hideout" => "Hideout",
				"player" => "PlayerOwned",
				"playerowned" => "PlayerOwned",
				"besieged" => "Besieged",
				"siege" => "Besieged",
				"raided" => "Raided",
				"empire" => "Empire",
				"vlandia" => "Vlandia",
				"sturgia" => "Sturgia",
				"aserai" => "Aserai",
				"khuzait" => "Khuzait",
				"battania" => "Battania",
				"nord" => "Nord",
				"lowprosperity" => "LowProsperity",
				"mediumprosperity" => "MediumProsperity",
				"highprosperity" => "HighProsperity",
				"low" => "LowProsperity",
				"medium" => "MediumProsperity",
				"high" => "HighProsperity",
				_ => typeString
			};

			if (Enum.TryParse<SettlementTypes>(normalized, true, out SettlementTypes result))
				return result;
			return SettlementTypes.None;
		}

		/// <summary>
		/// Parse multiple strings and combine into SettlementTypes flags
		/// </summary>
		public static SettlementTypes ParseSettlementTypes(IEnumerable<string> typeStrings)
		{
			SettlementTypes combined = SettlementTypes.None;
			foreach (string typeString in typeStrings)
			{
				SettlementTypes parsed = ParseSettlementType(typeString);
				if (parsed != SettlementTypes.None)
					combined |= parsed;
			}

			return combined;
		}

		/// <summary>
		/// Returns a formatted string listing settlement details with aligned columns
		/// </summary>
		public static string GetFormattedDetails(List<Settlement> settlements)
		{
			if (settlements.Count == 0)
				return "";

			return ColumnFormatter<Settlement>.FormatList(
				settlements,
				s => s.StringId,
				s => s.Name.ToString(),
				s => $"[{GetSettlementType(s)}]",
				s => $"Owner: {s.OwnerClan?.Name?.ToString() ?? "None"}",
				s => $"Kingdom: {s.MapFaction?.Name?.ToString() ?? "None"}",
				s => $"Culture: {s.Culture?.Name?.ToString() ?? "None"}",
				s => GetProsperityDisplay(s)
			);
		}

		#endregion

		#region Helpers

		/// <summary>
		/// Helper method to get settlement type as string
		/// </summary>
		private static string GetSettlementType(Settlement settlement)
		{
			return settlement.IsTown ? "City"
				: settlement.IsCastle ? "Castle"
				: settlement.IsVillage ? "Village"
				: settlement.IsHideout ? "Hideout"
				: "Unknown";
		}

		/// <summary>
		/// Helper method to get prosperity value for sorting
		/// </summary>
		private static float GetProsperityValue(Settlement settlement)
		{
			if (settlement.IsTown && settlement.Town != null)
				return settlement.Town.Prosperity;
			if (settlement.IsVillage && settlement.Village != null)
				return settlement.Village.Hearth;
			return 0f;
		}

		/// <summary>
		/// Helper method to get prosperity display string
		/// </summary>
		private static string GetProsperityDisplay(Settlement settlement)
		{
			if ((settlement.IsTown || settlement.IsCastle) && settlement.Town != null)
				return $"Prosperity: {settlement.Town.Prosperity:F0}";
			if (settlement.IsVillage && settlement.Village != null)
				return $"Hearth: {settlement.Village.Hearth:F0}";
			return "";
		}

		#endregion
	}
}
