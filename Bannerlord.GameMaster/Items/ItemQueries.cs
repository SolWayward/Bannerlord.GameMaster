using System;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.Core;
using TaleWorlds.ObjectSystem;
using Bannerlord.GameMaster.Common.Interfaces;
using Bannerlord.GameMaster.Console.Common;
using Bannerlord.GameMaster.Console.Common.Formatting;

namespace Bannerlord.GameMaster.Items
{
    /// <summary>
    /// Provides utility methods for querying item entities
    /// </summary>
    public static class ItemQueries
    {
        /// <summary>
        /// Finds an item with the specified itemId, using a case-insensitive comparison
        /// </summary>
        public static ItemObject GetItemById(string itemId)
        {
            return MBObjectManager.Instance.GetObject<ItemObject>(itemId);
        }

        /// <summary>
        /// Main unified method to find items by search string and type flags
        /// </summary>
        /// <param name="query">Optional case-insensitive substring to filter by name, ID, or tier</param>
        /// <param name="requiredTypes">Item type flags that ALL must match (AND logic)</param>
        /// <param name="matchAll">If true, item must have ALL flags. If false, item must have ANY flag</param>
        /// <param name="tierFilter">Optional tier filter (0-6, -1 for no filter)</param>
        /// <param name="sortBy">Sort field (name, tier, value, type, id)</param>
        /// <param name="sortDescending">True for descending, false for ascending</param>
        /// <returns>List of items matching all criteria</returns>
        public static List<ItemObject> QueryItems(
            string query = "",
            ItemTypes requiredTypes = ItemTypes.None,
            bool matchAll = true,
            int tierFilter = -1,
            string sortBy = "id",
            bool sortDescending = false)
        {
            IEnumerable<ItemObject> items = MBObjectManager.Instance.GetObjectTypeList<ItemObject>();

            // Filter by name/ID/tier if provided
            if (!string.IsNullOrEmpty(query))
            {
                string lowerFilter = query.ToLower();
                items = items.Where(i =>
                    i.Name.ToString().ToLower().Contains(lowerFilter) ||
                    i.StringId.ToLower().Contains(lowerFilter) ||
                    (i.Tier >= 0 && i.Tier.ToString().Contains(lowerFilter)));
            }

            // Filter by item types
            if (requiredTypes != ItemTypes.None)
            {
                items = items.Where(i =>
                    matchAll ? i.HasAllTypes(requiredTypes) : i.HasAnyType(requiredTypes));
            }

            // Filter by tier
            // Note: ItemTiers enum values are offset by 1 (Tier0=-1, Tier1=0, Tier2=1, etc.)
            // So we subtract 1 from the user's tier input to match the enum value
            if (tierFilter >= 0)
            {
                items = items.Where(i => (int)i.Tier == tierFilter - 1);
            }

            // Apply sorting
            items = ApplySorting(items, sortBy, sortDescending);

            return items.ToList();
        }

        /// <summary>
        /// Apply sorting to items collection
        /// </summary>
        private static IEnumerable<ItemObject> ApplySorting(
            IEnumerable<ItemObject> items,
            string sortBy,
            bool descending)
        {
            sortBy = sortBy.ToLower();

            IOrderedEnumerable<ItemObject> orderedItems = sortBy switch
            {
                "name" => descending
                    ? items.OrderByDescending(i => i.Name.ToString())
                    : items.OrderBy(i => i.Name.ToString()),
                "tier" => descending
                    ? items.OrderByDescending(i => i.Tier)
                    : items.OrderBy(i => i.Tier),
                "value" => descending
                    ? items.OrderByDescending(i => i.Value)
                    : items.OrderBy(i => i.Value),
                "type" => descending
                    ? items.OrderByDescending(i => i.ItemType.ToString())
                    : items.OrderBy(i => i.ItemType.ToString()),
                _ => descending  // default to id
                    ? items.OrderByDescending(i => i.StringId)
                    : items.OrderBy(i => i.StringId)
            };

            return orderedItems;
        }

        /// <summary>
        /// Parse a string into ItemTypes enum value
        /// </summary>
        public static ItemTypes ParseItemType(string typeString)
        {
            // Handle common aliases
            var normalized = typeString.ToLower() switch
            {
                "1h" => "OneHanded",
                "2h" => "TwoHanded",
                "head" => "HeadArmor",
                "body" => "BodyArmor",
                "leg" => "LegArmor",
                "hand" => "HandArmor",
                _ => typeString
            };

            return Enum.TryParse<ItemTypes>(normalized, true, out var result) 
                ? result : ItemTypes.None;
        }

        /// <summary>
        /// Parse multiple strings and combine into ItemTypes flags
        /// </summary>
        public static ItemTypes ParseItemTypes(IEnumerable<string> typeStrings)
        {
            ItemTypes combined = ItemTypes.None;
            foreach (var typeString in typeStrings)
            {
                var parsed = ParseItemType(typeString);
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
                i => $"Type: {i.ItemType}",
                i => $"Value: {i.Value}",
                i => {
                    // Note: ItemTiers enum values are offset by 1
                    string tier = (int)i.Tier >= -1 ? $"Tier: {(int)i.Tier + 1}" : "Tier: N/A";
                    return tier;
                }
            );
        }
    }

    /// <summary>
    /// Wrapper class implementing IEntityQueries interface for ItemObject entities
    /// </summary>
    public class ItemQueriesWrapper : IEntityQueries<ItemObject, ItemTypes>
    {
        public ItemObject GetById(string id) => ItemQueries.GetItemById(id);
        public List<ItemObject> Query(string query, ItemTypes types, bool matchAll) => 
            ItemQueries.QueryItems(query, types, matchAll);
        public ItemTypes ParseType(string typeString) => ItemQueries.ParseItemType(typeString);
        public ItemTypes ParseTypes(IEnumerable<string> typeStrings) => 
            ItemQueries.ParseItemTypes(typeStrings);
        public string GetFormattedDetails(List<ItemObject> entities) => 
            ItemQueries.GetFormattedDetails(entities);
    }
}