using Bannerlord.GameMaster.Settlements;
using System;
using System.Collections.Generic;

namespace Bannerlord.GameMaster.Console.Query.CommandQueryHelpers;

/// <summary>
/// Helper methods for settlement query commands
/// </summary>
public static class SettlementQueryHelpers
{
    /// <summary>
    /// Parse command arguments into SettlementQueryArguments struct
    /// </summary>
    public static SettlementQueryArguments ParseSettlementQueryArguments(List<string> args)
    {
        if (args == null || args.Count == 0)
            return new("", SettlementTypes.None, "id", false);

        HashSet<string> typeKeywords = new(StringComparer.OrdinalIgnoreCase)
        {
            "town", "castle", "city", "village", "hideout",
            "player", "playerowned",
            "besieged", "siege", "raided",
            "empire", "vlandia", "sturgia", "aserai", "khuzait", "battania", "nord",
            "lowprosperity", "mediumprosperity", "highprosperity",
            "low", "medium", "high"
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
        SettlementTypes types = SettlementQueries.ParseSettlementTypes(typeTerms);

        return new(query, types, sortBy, sortDesc);
    }
}
