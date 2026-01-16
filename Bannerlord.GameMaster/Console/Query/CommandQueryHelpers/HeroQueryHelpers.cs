using Bannerlord.GameMaster.Heroes;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Bannerlord.GameMaster.Console.Query.CommandQueryHelpers;

/// <summary>
/// Helper methods for hero query commands
/// </summary>
public static class HeroQueryHelpers
{
    /// <summary>
    /// Parse command arguments into HeroQueryArguments struct
    /// </summary>
    public static HeroQueryArguments ParseHeroQueryArguments(List<string> args)
    {
        if (args == null || args.Count == 0)
            return new("", HeroTypes.Alive, false, "id", false);

        HashSet<string> typeKeywords = new(StringComparer.OrdinalIgnoreCase)
        {
            "hero", "lord", "wanderer", "notable", "merchant", "children", "child",
            "female", "male", "clanleader", "kingdomruler", "partyleader",
            "fugitive", "alive", "dead", "prisoner", "withoutclan", "withoutkingdom", "married"
        };

        List<string> searchTerms = new();
        List<string> typeTerms = new();
        string sortBy = "id";
        bool sortDesc = false;

        // Check if "dead" keyword is present
        bool includeDead = args.Any(arg => arg.Equals("dead", StringComparison.OrdinalIgnoreCase));

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
        HeroTypes types = HeroQueries.ParseHeroTypes(typeTerms);

        // Default to Alive if no life status specified and not searching dead
        if (!includeDead && !types.HasFlag(HeroTypes.Dead) && !types.HasFlag(HeroTypes.Alive))
        {
            types |= HeroTypes.Alive;
        }

        return new(query, types, includeDead, sortBy, sortDesc);
    }
}
