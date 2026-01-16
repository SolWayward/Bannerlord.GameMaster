using Bannerlord.GameMaster.Settlements;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Bannerlord.GameMaster.Console.Query.CommandQueryHelpers;

/// <summary>
/// Query arguments specific to settlement queries
/// </summary>
public struct SettlementQueryArguments
{
    public QueryArguments QueryArgs;
    public SettlementTypes Types;

    public SettlementQueryArguments(string query, SettlementTypes types, string sortBy, bool sortDesc)
    {
        QueryArgs = new(query, sortBy, sortDesc);
        Types = types;
    }

    /// <summary>
    /// Build a readable criteria string for settlement queries
    /// </summary>
    public string GetCriteriaString()
    {
        List<string> parts = new();

        if (!string.IsNullOrEmpty(QueryArgs.Query))
            parts.Add($"search: '{QueryArgs.Query}'");

        if (Types != SettlementTypes.None)
        {
            SettlementTypes types = Types;
            IEnumerable<string> typeList = Enum.GetValues(typeof(SettlementTypes))
                .Cast<SettlementTypes>()
                .Where(t => t != SettlementTypes.None && types.HasFlag(t))
                .Select(t => t.ToString().ToLower());
            parts.Add($"types: {string.Join(", ", typeList)}");
        }

        if (!string.IsNullOrEmpty(QueryArgs.SortBy) && QueryArgs.SortBy != "id")
            parts.Add($"sort: {QueryArgs.SortBy}{(QueryArgs.SortDesc ? " (desc)" : " (asc)")}");

        return parts.Count > 0 ? string.Join(", ", parts) : "all settlements";
    }
}
