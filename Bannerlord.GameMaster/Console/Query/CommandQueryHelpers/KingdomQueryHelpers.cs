using Bannerlord.GameMaster.Kingdoms;
using System;
using System.Collections.Generic;

namespace Bannerlord.GameMaster.Console.Query.CommandQueryHelpers;

/// <summary>
/// Helper methods for kingdom query commands
/// </summary>
public static class KingdomQueryHelpers
{
    /// <summary>
    /// Parse command arguments into KingdomQueryArguments struct
    /// </summary>
    public static KingdomQueryArguments ParseKingdomQueryArguments(List<string> args)
    {
        if (args == null || args.Count == 0)
            return new("", KingdomTypes.Active, "id", false);

        HashSet<string> typeKeywords = new(StringComparer.OrdinalIgnoreCase)
        {
            "active", "eliminated", "empty", "player", "playerkingdom", "atwar", "war"
        };

        List<string> searchTerms = new();
        List<string> typeTerms = new();
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
        KingdomTypes types = KingdomQueries.ParseKingdomTypes(typeTerms);

        // Default to Active if no status specified
        if (!types.HasFlag(KingdomTypes.Active) && !types.HasFlag(KingdomTypes.Eliminated))
        {
            types |= KingdomTypes.Active;
        }

        return new(query, types, sortBy, sortDesc);
    }
}
