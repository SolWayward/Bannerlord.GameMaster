using System;
using System.Collections.Generic;
using TaleWorlds.Core;
using TaleWorlds.ObjectSystem;
using Bannerlord.GameMaster.Console.Common.Formatting;
using TaleWorlds.Library;

namespace Bannerlord.GameMaster.Items
{
	/// <summary>
	/// Provides query methods for searching, finding, filtering, or sorting item entities.
	/// </summary>
	public static class ItemQueries
	{
		#region Query

		/// <summary>
		/// Finds an item with the specified itemId. Fast but case sensitive (all string ids SHOULD be lower case) <br />
		/// Use QueryItems().FirstOrDefault() to find item with case insensitive partial name or partial stringIds <br />
		/// Example: QueryItems("iron_sword").FirstOrDefault()
		/// </summary>
		public static ItemObject GetItemById(string itemId) => MBObjectManager.Instance.GetObject<ItemObject>(itemId);

		/// <summary>
		/// Performance focused method to find items matching multiple parameters. All parameters are optional and can be used with none, one or a combination of any parameters<br />
		/// Note: <paramref name="query"/> parameter is a string used that will match partial item names or partial stringIds
		/// </summary>
		/// <param name="query">Optional case-insensitive substring to filter by name or ID</param>
		/// <param name="requiredTypes">Item type flags that ALL must match (AND logic)</param>
		/// <param name="matchAll">If true, item must have ALL flags. If false, item must have ANY flag</param>
		/// <param name="tierFilter">Optional tier filter (0-6, -1 for no filter)</param>
		/// <param name="cultureFilter">Optional culture StringId filter</param>
		/// <param name="civilianFilter">Optional civilian filter (null = any, true = civilian, false = battle)</param>
		/// <param name="sortBy">Sort field (id, name, tier, value, type, culture, loadout, or any ItemType flag)</param>
		/// <param name="sortDescending">True for descending, false for ascending</param>
		/// <returns>List of items matching all criteria</returns>
		public static MBReadOnlyList<ItemObject> QueryItems(
			string query = "",
			ItemTypes requiredTypes = ItemTypes.None,
			bool matchAll = true,
			int tierFilter = -1,
			string cultureFilter = "",
			bool? civilianFilter = null,
			string sortBy = "id",
			bool sortDescending = false)
		{
			// MARK: Get source collection
			MBReadOnlyList<ItemObject> source = MBObjectManager.Instance.GetObjectTypeList<ItemObject>();
			MBReadOnlyList<ItemObject> results = new(source.Count);

			bool hasQuery = !string.IsNullOrEmpty(query);
			bool hasTypes = requiredTypes != ItemTypes.None;
			bool hasTierFilter = tierFilter >= 0;
			bool hasCultureFilter = !string.IsNullOrEmpty(cultureFilter);

			// Filter items
			for (int i = 0; i < source.Count; i++)
			{
				ItemObject item = source[i];
				if (MatchesFilters(item, query, hasQuery, requiredTypes, hasTypes, tierFilter, hasTierFilter, matchAll, cultureFilter, hasCultureFilter, civilianFilter))
				{
					results.Add(item);
				}
			}

			// Sorting - only if needed
			if (results.Count > 1)
			{
				IComparer<ItemObject> comparer = GetItemComparer(sortBy, sortDescending);
				results.Sort(comparer);
			}

			return results;
		}

		/// <summary>
		/// Check if item matches all filter criteria
		/// </summary>
		public static bool MatchesFilters(
			ItemObject item,
			string query,
			bool hasQuery,
			ItemTypes requiredTypes,
			bool hasTypes,
			int tierFilter,
			bool hasTierFilter,
			bool matchAll,
			string cultureFilter,
			bool hasCultureFilter,
			bool? civilianFilter)
		{
			// Name/ID filter using OrdinalIgnoreCase (no string allocation)
			if (hasQuery)
			{
				bool nameMatch = item.Name.ToString().IndexOf(query, StringComparison.OrdinalIgnoreCase) >= 0;
				bool idMatch = item.StringId.IndexOf(query, StringComparison.OrdinalIgnoreCase) >= 0;
				if (!nameMatch && !idMatch)
					return false;
			}

			// Type filter
			if (hasTypes)
			{
				bool matches = matchAll ? item.HasAllTypes(requiredTypes) : item.HasAnyType(requiredTypes);
				if (!matches)
					return false;
			}

			// Tier filter
			// Note: ItemTiers enum values are offset by 1 (Tier0=-1, Tier1=0, Tier2=1, etc.)
			// So we subtract 1 from the user's tier input to match the enum value
			if (hasTierFilter)
			{
				if ((int)item.Tier != tierFilter - 1)
					return false;
			}

			// Culture filter
			if (hasCultureFilter)
			{
				string itemCulture = item.Culture?.StringId ?? "";
				if (!itemCulture.Equals(cultureFilter, StringComparison.OrdinalIgnoreCase))
					return false;
			}

			// Civilian/Battle filter
			if (civilianFilter.HasValue)
			{
				bool itemIsCivilian = item.IsCivilianEquipment();
				if (itemIsCivilian != civilianFilter.Value)
					return false;
			}

			return true;
		}

		/// <summary>
		/// Get comparer for item sorting
		/// </summary>
		public static IComparer<ItemObject> GetItemComparer(string sortBy, bool descending)
		{
			sortBy = sortBy.ToLowerInvariant();

			// Standard field comparers
			return sortBy switch
			{
				"name" => Comparer<ItemObject>.Create((a, b) =>
				{
					int result = string.Compare(a.Name.ToString(), b.Name.ToString(), StringComparison.Ordinal);
					return descending ? -result : result;
				}),
				"tier" => Comparer<ItemObject>.Create((a, b) =>
				{
					int result = a.Tier.CompareTo(b.Tier);
					return descending ? -result : result;
				}),
				"value" => Comparer<ItemObject>.Create((a, b) =>
				{
					int result = a.Value.CompareTo(b.Value);
					return descending ? -result : result;
				}),
				"type" => Comparer<ItemObject>.Create((a, b) =>
				{
					int result = string.Compare(
						a.ItemType.ToString(),
						b.ItemType.ToString(),
						StringComparison.Ordinal);
					return descending ? -result : result;
				}),
				"weight" => Comparer<ItemObject>.Create((a, b) =>
				{
					int result = a.Weight.CompareTo(b.Weight);
					return descending ? -result : result;
				}),
				"culture" => Comparer<ItemObject>.Create((a, b) =>
				{
					string cultureA = a.Culture?.Name?.ToString() ?? "";
					string cultureB = b.Culture?.Name?.ToString() ?? "";
					int result = string.Compare(cultureA, cultureB, StringComparison.Ordinal);
					return descending ? -result : result;
				}),
				"loadout" or "civilian" => Comparer<ItemObject>.Create((a, b) =>
				{
					int result = a.IsCivilianEquipment().CompareTo(b.IsCivilianEquipment());
					return descending ? -result : result;
				}),
				_ => Comparer<ItemObject>.Create((a, b) =>  // default: id
				{
					int result = string.Compare(a.StringId, b.StringId, StringComparison.Ordinal);
					return descending ? -result : result;
				})
			};
		}

		#endregion

		#region Parsing / Formatting

		/// <summary>
		/// Parse a string into ItemTypes enum value
		/// </summary>
		public static ItemTypes ParseItemType(string typeString)
		{
			// Handle common aliases
			string normalized = typeString.ToLower() switch
			{
				"1h" => "OneHanded",
				"2h" => "TwoHanded",
				"head" => "HeadArmor",
				"body" => "BodyArmor",
				"leg" => "LegArmor",
				"hand" => "HandArmor",
				_ => typeString
			};

			if (Enum.TryParse<ItemTypes>(normalized, true, out ItemTypes result))
				return result;
			return ItemTypes.None;
		}

		/// <summary>
		/// Parse multiple strings and combine into ItemTypes flags
		/// </summary>
		public static ItemTypes ParseItemTypes(IEnumerable<string> typeStrings)
		{
			ItemTypes combined = ItemTypes.None;
			foreach (string typeString in typeStrings)
			{
				ItemTypes parsed = ParseItemType(typeString);
				if (parsed != ItemTypes.None)
					combined |= parsed;
			}
			
			return combined;
		}

		/// <summary>
		/// Returns a formatted string listing item details with aligned columns
		/// </summary>
		public static string GetFormattedDetails(List<ItemObject> items)
		{
			if (items.Count == 0)
				return "";

			return ColumnFormatter<ItemObject>.FormatList(
				items,
				i => i.StringId,
				i => i.Name.ToString(),
				i => i.Culture?.Name?.ToString() ?? "None",
				i => $"Type: {i.ItemType}",
				i =>
				{
					// Note: ItemTiers enum values are offset by 1
					string tier = (int)i.Tier >= -1 ? $"Tier: {(int)i.Tier + 1}" : "Tier: N/A";
					return tier;
				},
				i => $"Value: {i.Value}",
				i => $"Weight: {i.Weight}",
				i => i.IsCivilianEquipment() ? "Civilian" : "Battle"
			);
		}

		#endregion
	}
}
