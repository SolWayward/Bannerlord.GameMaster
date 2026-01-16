using System.Collections.Generic;
using TaleWorlds.Core;
using Bannerlord.GameMaster.Items;

namespace Bannerlord.GameMaster.Console.Common.EntityFinding
{
    /// <summary>
    /// Provides methods to find single ItemObject entities from query strings.
    /// </summary>
    public static class ItemFinder
    {
        /// <summary>
        /// Finds a single item from a query string (name, ID, or partial match).
        /// </summary>
        /// <param name="query">The search query (item name or ID)</param>
        /// <returns>EntityFinderResult containing the found item or error message</returns>
        public static EntityFinderResult<ItemObject> FindSingleItem(string query)
        {
            List<ItemObject> matchedItems = ItemQueries.QueryItems(query);

            if (matchedItems == null || matchedItems.Count == 0)
                return EntityFinderResult<ItemObject>.Error($"Error: No item matching query '{query}' found.\n");

            if (matchedItems.Count == 1)
                return EntityFinderResult<ItemObject>.Success(matchedItems[0]);

            // Use smart matching for multiple results
            return EntityFinder.ResolveMultipleMatches(
                matches: matchedItems,
                query: query,
                getStringId: i => i.StringId,
                getName: i => i.Name?.ToString() ?? "",
                formatDetails: ItemQueries.GetFormattedDetails,
                entityType: "item");
        }
    }
}
