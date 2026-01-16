using Bannerlord.GameMaster.Heroes;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Bannerlord.GameMaster.Console.Query.CommandQueryHelpers;

/// <summary>
/// Query arguments specific to hero queries
/// </summary>
public struct HeroQueryArguments
{
    public QueryArguments QueryArgs;
    public HeroTypes Types;
    public bool IncludeDead;

    public HeroQueryArguments(string query, HeroTypes types, bool includeDead, string sortBy, bool sortDesc)
    {
        QueryArgs = new(query, sortBy, sortDesc);
        Types = types;
        IncludeDead = includeDead;
    }

    /// <summary>
    /// Build a readable criteria string for hero queries
    /// </summary>
    public string GetCriteriaString()
    {
        List<string> parts = new();

        if (!string.IsNullOrEmpty(QueryArgs.Query))
            parts.Add($"search: '{QueryArgs.Query}'");

        if (Types != HeroTypes.None)
        {
            HeroTypes types = Types;
            IEnumerable<string> typeList = Enum.GetValues(typeof(HeroTypes))
                .Cast<HeroTypes>()
                .Where(t => t != HeroTypes.None && types.HasFlag(t))
                .Select(t => t.ToString().ToLower());
            parts.Add($"types: {string.Join(", ", typeList)}");
        }

        if (!string.IsNullOrEmpty(QueryArgs.SortBy) && QueryArgs.SortBy != "id")
            parts.Add($"sort: {QueryArgs.SortBy}{(QueryArgs.SortDesc ? " (desc)" : " (asc)")}");

        return parts.Count > 0 ? string.Join(", ", parts) : "all heroes";
    }
}
