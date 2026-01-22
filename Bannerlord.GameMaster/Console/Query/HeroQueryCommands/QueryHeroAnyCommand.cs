using Bannerlord.GameMaster.Console.Common;
using Bannerlord.GameMaster.Console.Common.Execution;
using Bannerlord.GameMaster.Console.Common.Validation;
using Bannerlord.GameMaster.Console.Query.CommandQueryHelpers;
using Bannerlord.GameMaster.Heroes;
using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Library;

namespace Bannerlord.GameMaster.Console.Query.HeroQueryCommands;

/// <summary>
/// Find heroes matching ANY of the specified types (OR logic)
/// Usage: gm.query.hero_any [search terms] [type keywords] [sort parameters]
/// Example: gm.query.hero_any lord wanderer (finds anyone who is lord OR wanderer)
/// Example: gm.query.hero_any lord wanderer sort:name:desc
/// </summary>
public static class QueryHeroAnyCommand
{
    [CommandLineFunctionality.CommandLineArgumentFunction("hero_any", "gm.query")]
    public static string Execute(List<string> args)
    {
        return Cmd.Run(args, () =>
        {
            // MARK: Validation
            if (!CommandValidator.ValidateCampaignState(out string error))
                return CommandResult.Error(error).Log().Message;

            // MARK: Parse Arguments
            HeroQueryArguments queryArgs = HeroQueryHelpers.ParseHeroQueryArguments(args);

            // MARK: Execute Logic
            List<Hero> matchedHeroes = HeroQueries.QueryHeroes(
                queryArgs.QueryArgs.Query, 
                queryArgs.Types, 
                matchAll: false, 
                includeDead: queryArgs.IncludeDead, 
                queryArgs.QueryArgs.SortBy, 
                queryArgs.QueryArgs.SortDesc);

            string criteriaDesc = queryArgs.GetCriteriaString();
            
            if (matchedHeroes.Count == 0)
            {
                return CommandResult.Success($"Found 0 hero(es) matching ANY of {criteriaDesc}\n" +
                       "Usage: gm.query.hero_any [search] [type keywords] [sort]\n" +
                       "Example: gm.query.hero_any lord wanderer sort:name\n").Log().Message;
            }

            return CommandResult.Success($"Found {matchedHeroes.Count} hero(es) matching ANY of {criteriaDesc}:\n" +
                   $"{HeroQueries.GetFormattedDetails(matchedHeroes)}").Log().Message;
        });
    }
}
