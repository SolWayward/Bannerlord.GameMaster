using System.Collections.Generic;

namespace Bannerlord.GameMaster.Console.Query.CommandQueryHelpers;

/// <summary>
/// Query arguments specific to culture queries
/// </summary>
public struct CultureQueryArguments
{
    public QueryArguments QueryArgs;
    public bool MainOnly;
    public bool BanditOnly;

    public CultureQueryArguments(string query, bool mainOnly, bool banditOnly, string sortBy, bool sortDesc)
    {
        QueryArgs = new(query, sortBy, sortDesc);
        MainOnly = mainOnly;
        BanditOnly = banditOnly;
    }

    /// <summary>
    /// Build a readable criteria string for culture queries
    /// </summary>
    public string GetCriteriaString()
    {
        List<string> parts = new();

        if (!string.IsNullOrEmpty(QueryArgs.Query))
            parts.Add($"search: '{QueryArgs.Query}'");

        if (MainOnly)
            parts.Add("type: main");
        else if (BanditOnly)
            parts.Add("type: bandit");

        if (!string.IsNullOrEmpty(QueryArgs.SortBy) && QueryArgs.SortBy != "id")
            parts.Add($"sort: {QueryArgs.SortBy}{(QueryArgs.SortDesc ? " (desc)" : " (asc)")}");

        return parts.Count > 0 ? string.Join(", ", parts) : "all cultures";
    }
}
