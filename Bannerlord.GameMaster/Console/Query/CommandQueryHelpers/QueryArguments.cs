namespace Bannerlord.GameMaster.Console.Query.CommandQueryHelpers;

/// <summary>
/// Base query arguments containing common search and sort parameters
/// </summary>
public struct QueryArguments
{
    public string Query;
    public string SortBy;
    public bool SortDesc;

    public QueryArguments(string query, string sortBy, bool sortDesc)
    {
        Query = query;
        SortBy = sortBy;
        SortDesc = sortDesc;
    }
}
