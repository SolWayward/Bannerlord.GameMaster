using Bannerlord.GameMaster.Kingdoms;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Bannerlord.GameMaster.Console.Query.CommandQueryHelpers;

/// <summary>
/// Query arguments specific to kingdom queries
/// </summary>
public struct KingdomQueryArguments
{
    public QueryArguments QueryArgs;
    public KingdomTypes Types;

    public KingdomQueryArguments(string query, KingdomTypes types, string sortBy, bool sortDesc)
    {
        QueryArgs = new(query, sortBy, sortDesc);
        Types = types;
    }

    /// <summary>
    /// Build a readable criteria string for kingdom queries
    /// </summary>
    public string GetCriteriaString()
    {
        List<string> parts = new();

        if (!string.IsNullOrEmpty(QueryArgs.Query))
            parts.Add($"search: '{QueryArgs.Query}'");

        if (Types != KingdomTypes.None)
        {
            KingdomTypes types = Types;
            IEnumerable<string> typeList = Enum.GetValues(typeof(KingdomTypes))
                .Cast<KingdomTypes>()
                .Where(t => t != KingdomTypes.None && types.HasFlag(t))
                .Select(t => t.ToString().ToLower());
            parts.Add($"types: {string.Join(", ", typeList)}");
        }

        if (!string.IsNullOrEmpty(QueryArgs.SortBy) && QueryArgs.SortBy != "id")
            parts.Add($"sort: {QueryArgs.SortBy}{(QueryArgs.SortDesc ? " (desc)" : " (asc)")}");

        return parts.Count > 0 ? string.Join(", ", parts) : "all kingdoms";
    }
}
