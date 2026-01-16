using Bannerlord.GameMaster.Clans;
using System;
using System.Collections.Generic;

namespace Bannerlord.GameMaster.Console.Query.CommandQueryHelpers;

/// <summary>
/// Helper methods for clan query commands
/// </summary>
public static class ClanQueryHelpers
{
    /// <summary>
    /// Parse command arguments into ClanQueryArguments struct
    /// </summary>
    public static ClanQueryArguments ParseClanQueryArguments(List<string> args)
    {
        if (args == null || args.Count == 0)
            return new("", ClanTypes.Active, "id", false);

        HashSet<string> typeKeywords = new(StringComparer.OrdinalIgnoreCase)
        {
            "active", "eliminated", "bandit", "nonbandit", "mapfaction", "noble",
            "minor", "minorfaction", "rebel", "mercenary", "merc", "undermercenaryservice",
            "mafia", "outlaw", "nomad", "sect", "withoutkingdom", "empty", "player", "playerclan"
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
        ClanTypes types = ClanQueries.ParseClanTypes(typeTerms);

        // Default to Active if no status specified
        if (!types.HasFlag(ClanTypes.Active) && !types.HasFlag(ClanTypes.Eliminated))
        {
            types |= ClanTypes.Active;
        }

        return new(query, types, sortBy, sortDesc);
    }
}
