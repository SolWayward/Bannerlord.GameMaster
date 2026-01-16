using System;

namespace Bannerlord.GameMaster.Console.Query.CommandQueryHelpers;

/// <summary>
/// Common helper methods shared across query command helpers
/// </summary>
public static class CommonQueryHelpers
{
    /// <summary>
    /// Parse sort parameter (e.g., "sort:name:desc" or "sort:age")
    /// </summary>
    public static void ParseSortParameter(string sortParam, ref string sortBy, ref bool sortDesc)
    {
        string[] parts = sortParam.Split(':');
        if (parts.Length >= 2)
        {
            sortBy = parts[1].ToLower();
        }
        if (parts.Length >= 3)
        {
            sortDesc = parts[2].Equals("desc", StringComparison.OrdinalIgnoreCase);
        }
    }
}
