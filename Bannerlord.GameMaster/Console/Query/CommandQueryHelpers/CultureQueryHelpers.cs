using Bannerlord.GameMaster.Console.Common;
using Bannerlord.GameMaster.Console.Common.Formatting;
using System;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;

namespace Bannerlord.GameMaster.Console.Query.CommandQueryHelpers;

/// <summary>
/// Helper methods for culture query commands
/// </summary>
public static class CultureQueryHelpers
{
    /// <summary>
    /// Parse command arguments into CultureQueryArguments struct
    /// </summary>
    public static CultureQueryArguments ParseCultureQueryArguments(List<string> args)
    {
        if (args == null || args.Count == 0)
            return new("", false, false, "id", false);

        HashSet<string> typeKeywords = new(StringComparer.OrdinalIgnoreCase)
        {
            "main", "bandit", "minor"
        };

        List<string> searchTerms = new();
        bool mainOnly = false;
        bool banditOnly = false;
        string sortBy = "id";
        bool sortDesc = false;

        foreach (string arg in args)
        {
            // Check for sort parameters
            if (arg.StartsWith("sort:", StringComparison.OrdinalIgnoreCase))
            {
                CommonQueryHelpers.ParseSortParameter(arg, ref sortBy, ref sortDesc);
            }
            // Check for type keywords
            else if (arg.Equals("main", StringComparison.OrdinalIgnoreCase))
            {
                mainOnly = true;
            }
            else if (arg.Equals("bandit", StringComparison.OrdinalIgnoreCase))
            {
                banditOnly = true;
            }
            // Otherwise treat as search term
            else
            {
                searchTerms.Add(arg);
            }
        }

        string query = string.Join(" ", searchTerms).Trim();

        return new(query, mainOnly, banditOnly, sortBy, sortDesc);
    }

    /// <summary>
    /// Sort cultures based on the specified criteria
    /// </summary>
    public static List<CultureObject> SortCultures(List<CultureObject> cultures, string sortBy, bool descending)
    {
        IOrderedEnumerable<CultureObject> sorted = sortBy.ToLower() switch
        {
            "name" => descending
                ? cultures.OrderByDescending(c => c.Name.ToString())
                : cultures.OrderBy(c => c.Name.ToString()),
            "id" => descending
                ? cultures.OrderByDescending(c => c.StringId)
                : cultures.OrderBy(c => c.StringId),
            _ => descending
                ? cultures.OrderByDescending(c => c.StringId)
                : cultures.OrderBy(c => c.StringId)
        };

        return sorted.ToList();
    }

    /// <summary>
    /// Format culture list for display
    /// </summary>
    public static string GetFormattedCultureList(List<CultureObject> cultures)
    {
        if (cultures.Count == 0)
            return "";

        return ColumnFormatter<CultureObject>.FormatList(
            cultures,
            c => c.StringId,
            c => c.Name.ToString(),
            c => c.IsMainCulture ? "Main" : c.IsBandit ? "Bandit" : "Minor"
        );
    }
}
