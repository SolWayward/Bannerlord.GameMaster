using Bannerlord.GameMaster.Console.Common.Execution;
using Bannerlord.GameMaster.Console.Common.Validation;
using Bannerlord.GameMaster.Console.Query.CommandQueryHelpers;
using Bannerlord.GameMaster.Heroes;
using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Library;

namespace Bannerlord.GameMaster.Console.Query.HeroQueryCommands;

/// <summary>
/// Unified hero listing command with AND logic for type filters
/// Usage: gm.query.hero [search terms] [type keywords] [sort parameters]
/// Example: gm.query.hero john lord female clanleader
/// Example: gm.query.hero aserai wanderer sort:name
/// Example: gm.query.hero dead kingdomruler sort:age:desc
/// Example: gm.query.hero sort:wanderer (sorts by wanderer flag)
/// </summary>
public static class QueryHeroCommand
{
    [CommandLineFunctionality.CommandLineArgumentFunction("hero", "gm.query")]
    public static string Execute(List<string> args)
    {
        return Cmd.Run(args, () =>
        {
            // MARK: Validation
            if (!CommandValidator.ValidateCampaignState(out string error))
                return error;

            // MARK: Parse Arguments
            HeroQueryArguments queryArgs = HeroQueryHelpers.ParseHeroQueryArguments(args);

            // MARK: Execute Logic
            List<Hero> matchedHeroes = HeroQueries.QueryHeroes(
                queryArgs.QueryArgs.Query, 
                queryArgs.Types, 
                matchAll: true, 
                includeDead: queryArgs.IncludeDead, 
                queryArgs.QueryArgs.SortBy, 
                queryArgs.QueryArgs.SortDesc);

            string criteriaDesc = queryArgs.GetCriteriaString();
            
            if (matchedHeroes.Count == 0)
            {
                return $"Found 0 hero(es) matching {criteriaDesc}\n" +
                       "Usage: gm.query.hero [search] [type keywords] [sort]\n" +
                       "Type keywords: lord, wanderer, notable, female, male, clanleader, kingdomruler, dead, etc.\n" +
                       "Sort: sort:name, sort:age, sort:clan, sort:kingdom, sort:<type> (add :desc for descending)\n" +
                       "Example: gm.query.hero john lord female sort:name\n";
            }

            return $"Found {matchedHeroes.Count} hero(es) matching {criteriaDesc}:\n" +
                   $"{HeroQueries.GetFormattedDetails(matchedHeroes)}";
        });
    }
}
