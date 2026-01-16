using Bannerlord.GameMaster.Troops;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Bannerlord.GameMaster.Console.Query.CommandQueryHelpers;

/// <summary>
/// Query arguments specific to troop queries
/// </summary>
public struct TroopQueryArguments
{
    public QueryArguments QueryArgs;
    public TroopTypes Types;
    public int Tier;

    public TroopQueryArguments(string query, TroopTypes types, int tier, string sortBy, bool sortDesc)
    {
        QueryArgs = new(query, sortBy, sortDesc);
        Types = types;
        Tier = tier;
    }

    /// <summary>
    /// Build a readable criteria string for troop queries
    /// </summary>
    public string GetCriteriaString()
    {
        List<string> parts = new();

        if (!string.IsNullOrEmpty(QueryArgs.Query))
            parts.Add($"search: '{QueryArgs.Query}'");

        if (Types != TroopTypes.None)
        {
            TroopTypes types = Types;
            IEnumerable<string> typeList = Enum.GetValues(typeof(TroopTypes))
                .Cast<TroopTypes>()
                .Where(t => t != TroopTypes.None && types.HasFlag(t))
                .Select(t => t.ToString().ToLower());
            parts.Add($"types: {string.Join(", ", typeList)}");
        }

        if (Tier >= 0)
            parts.Add($"tier: {Tier}");

        if (!string.IsNullOrEmpty(QueryArgs.SortBy) && QueryArgs.SortBy != "id")
            parts.Add($"sort: {QueryArgs.SortBy}{(QueryArgs.SortDesc ? " (desc)" : " (asc)")}");

        return parts.Count > 0 ? string.Join(", ", parts) : "all troops";
    }
}
