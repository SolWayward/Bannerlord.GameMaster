using Bannerlord.GameMaster.Clans;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Bannerlord.GameMaster.Console.Query.CommandQueryHelpers;

/// <summary>
/// Query arguments specific to clan queries
/// </summary>
public struct ClanQueryArguments
{
    public QueryArguments QueryArgs;
    public ClanTypes Types;

    public ClanQueryArguments(string query, ClanTypes types, string sortBy, bool sortDesc)
    {
        QueryArgs = new(query, sortBy, sortDesc);
        Types = types;
    }

    /// <summary>
    /// Build a readable criteria string for clan queries
    /// </summary>
    public string GetCriteriaString()
    {
        List<string> parts = new();

        if (!string.IsNullOrEmpty(QueryArgs.Query))
            parts.Add($"search: '{QueryArgs.Query}'");

        if (Types != ClanTypes.None)
        {
            ClanTypes types = Types;
            IEnumerable<string> typeList = Enum.GetValues(typeof(ClanTypes))
                .Cast<ClanTypes>()
                .Where(t => t != ClanTypes.None && types.HasFlag(t))
                .Select(t => t.ToString().ToLower());
            parts.Add($"types: {string.Join(", ", typeList)}");
        }

        if (!string.IsNullOrEmpty(QueryArgs.SortBy) && QueryArgs.SortBy != "id")
            parts.Add($"sort: {QueryArgs.SortBy}{(QueryArgs.SortDesc ? " (desc)" : " (asc)")}");

        return parts.Count > 0 ? string.Join(", ", parts) : "all clans";
    }
}
