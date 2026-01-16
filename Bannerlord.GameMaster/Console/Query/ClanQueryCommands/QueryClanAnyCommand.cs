using Bannerlord.GameMaster.Clans;
using Bannerlord.GameMaster.Console.Common.Execution;
using Bannerlord.GameMaster.Console.Common.Validation;
using Bannerlord.GameMaster.Console.Query.CommandQueryHelpers;
using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Library;

namespace Bannerlord.GameMaster.Console.Query.ClanQueryCommands;

/// <summary>
/// Find clans matching ANY of the specified types (OR logic)
/// Usage: gm.query.clan_any [search terms] [type keywords] [sort parameters]
/// Example: gm.query.clan_any bandit outlaw sort:name:desc
/// </summary>
public static class QueryClanAnyCommand
{
    [CommandLineFunctionality.CommandLineArgumentFunction("clan_any", "gm.query")]
    public static string Execute(List<string> args)
    {
        return Cmd.Run(args, () =>
        {
            // MARK: Validation
            if (!CommandValidator.ValidateCampaignState(out string error))
                return error;

            // MARK: Parse Arguments
            ClanQueryArguments queryArgs = ClanQueryHelpers.ParseClanQueryArguments(args);

            // MARK: Execute Logic
            List<Clan> matchedClans = ClanQueries.QueryClans(
                queryArgs.QueryArgs.Query, 
                queryArgs.Types, 
                matchAll: false, 
                queryArgs.QueryArgs.SortBy, 
                queryArgs.QueryArgs.SortDesc);

            string criteriaDesc = queryArgs.GetCriteriaString();
            
            if (matchedClans.Count == 0)
            {
                return $"Found 0 clan(s) matching ANY of {criteriaDesc}\n" +
                       "Usage: gm.query.clan_any [search] [type keywords] [sort]\n" +
                       "Example: gm.query.clan_any bandit outlaw sort:name\n";
            }

            return $"Found {matchedClans.Count} clan(s) matching ANY of {criteriaDesc}:\n" +
                   $"{ClanQueries.GetFormattedDetails(matchedClans)}";
        });
    }
}
