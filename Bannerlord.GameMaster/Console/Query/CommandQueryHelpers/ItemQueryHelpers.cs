using Bannerlord.GameMaster.Items;
using System;
using System.Collections.Generic;

namespace Bannerlord.GameMaster.Console.Query.CommandQueryHelpers;

/// <summary>
/// Helper methods for item query commands
/// </summary>
public static class ItemQueryHelpers
{
    /// <summary>
    /// Parse command arguments into ItemQueryArguments struct
    /// </summary>
    public static ItemQueryArguments ParseItemQueryArguments(List<string> args)
    {
        if (args == null || args.Count == 0)
            return new("", ItemTypes.None, -1, "id", false);

        HashSet<string> typeKeywords = new(StringComparer.OrdinalIgnoreCase)
        {
            "weapon", "armor", "mount", "food", "trade", "goods", "banner",
            "1h", "onehanded", "2h", "twohanded", "ranged", "shield", "polearm", "thrown",
            "arrows", "bolts", "head", "headarmor", "body", "bodyarmor",
            "leg", "legarmor", "hand", "handarmor", "cape",
            "bow", "crossbow", "civilian", "combat", "horsearmor"
        };

        HashSet<string> tierKeywords = new(StringComparer.OrdinalIgnoreCase)
        {
            "tier0", "tier1", "tier2", "tier3", "tier4", "tier5", "tier6"
        };

        List<string> searchTerms = new();
        List<string> typeTerms = new();
        int tierFilter = -1;
        string sortBy = "id";
        bool sortDesc = false;

        foreach (string arg in args)
        {
            // Check for sort parameters
            if (arg.StartsWith("sort:", StringComparison.OrdinalIgnoreCase))
            {
                CommonQueryHelpers.ParseSortParameter(arg, ref sortBy, ref sortDesc);
            }
            // Check for tier keywords
            else if (tierKeywords.Contains(arg))
            {
                tierFilter = ParseTierKeyword(arg);
            }
            // Check for type keywords
            else if (typeKeywords.Contains(arg))
            {
                typeTerms.Add(arg);
            }
            // Otherwise treat as search term
            else
            {
                searchTerms.Add(arg);
            }
        }

        string query = string.Join(" ", searchTerms).Trim();
        ItemTypes types = ItemQueries.ParseItemTypes(typeTerms);

        return new(query, types, tierFilter, sortBy, sortDesc);
    }

    /// <summary>
    /// Parse tier keyword (e.g., "tier3" returns 3)
    /// </summary>
    public static int ParseTierKeyword(string tierKeyword)
    {
        if (tierKeyword.Length >= 5 && tierKeyword.StartsWith("tier", StringComparison.OrdinalIgnoreCase))
        {
            if (int.TryParse(tierKeyword.Substring(4), out int tier))
            {
                return tier;
            }
        }
        return -1;
    }
}
