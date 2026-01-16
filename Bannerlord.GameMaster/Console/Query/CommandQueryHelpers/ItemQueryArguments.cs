using Bannerlord.GameMaster.Items;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Bannerlord.GameMaster.Console.Query.CommandQueryHelpers;

/// <summary>
/// Query arguments specific to item queries
/// </summary>
public struct ItemQueryArguments
{
    public QueryArguments QueryArgs;
    public ItemTypes Types;
    public int Tier;

    public ItemQueryArguments(string query, ItemTypes types, int tier, string sortBy, bool sortDesc)
    {
        QueryArgs = new(query, sortBy, sortDesc);
        Types = types;
        Tier = tier;
    }

    /// <summary>
    /// Build a readable criteria string for item queries
    /// </summary>
    public string GetCriteriaString()
    {
        List<string> parts = new();

        if (!string.IsNullOrEmpty(QueryArgs.Query))
            parts.Add($"search: '{QueryArgs.Query}'");

        if (Types != ItemTypes.None)
        {
            ItemTypes types = Types;
            IEnumerable<string> typeList = Enum.GetValues(typeof(ItemTypes))
                .Cast<ItemTypes>()
                .Where(t => t != ItemTypes.None && types.HasFlag(t))
                .Select(t => t.ToString().ToLower());
            parts.Add($"types: {string.Join(", ", typeList)}");
        }

        if (Tier >= 0)
            parts.Add($"tier: {Tier}");

        if (!string.IsNullOrEmpty(QueryArgs.SortBy) && QueryArgs.SortBy != "id")
            parts.Add($"sort: {QueryArgs.SortBy}{(QueryArgs.SortDesc ? " (desc)" : " (asc)")}");

        return parts.Count > 0 ? string.Join(", ", parts) : "all items";
    }
}
