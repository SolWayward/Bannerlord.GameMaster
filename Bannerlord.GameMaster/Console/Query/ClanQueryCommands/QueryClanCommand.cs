using Bannerlord.GameMaster.Clans;
using Bannerlord.GameMaster.Console.Common;
using Bannerlord.GameMaster.Console.Common.Execution;
using Bannerlord.GameMaster.Console.Common.Parsing;
using Bannerlord.GameMaster.Console.Common.Validation;
using Bannerlord.GameMaster.Console.Query.CommandQueryHelpers;
using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Library;

namespace Bannerlord.GameMaster.Console.Query.ClanQueryCommands;

/// <summary>
/// Unified clan listing command with AND logic for type filters
/// Usage: gm.query.clan [search terms] [type keywords] [sort parameters]
/// Example: gm.query.clan empire noble
/// Example: gm.query.clan bandit sort:name
/// Example: gm.query.clan eliminated empty sort:tier:desc
/// Example: gm.query.clan sort:mercenary (sorts by mercenary flag)
/// </summary>
public static class QueryClanCommand
{
    [CommandLineFunctionality.CommandLineArgumentFunction("clan", "gm.query")]
    public static string Execute(List<string> args)
    {
        return Cmd.Run(args, () =>
        {
            // MARK: Validation
            if (!CommandValidator.ValidateCampaignState(out string error))
                return CommandResult.Error(error).Log().Message;

            // MARK: Parse Arguments
            ClanQueryArguments queryArgs = ClanQueryHelpers.ParseClanQueryArguments(args);

            // MARK: Execute Logic
            List<Clan> matchedClans = ClanQueries.QueryClans(
                queryArgs.QueryArgs.Query,
                queryArgs.Types,
                matchAll: true,
                queryArgs.QueryArgs.SortBy,
                queryArgs.QueryArgs.SortDesc);

            Dictionary<string, string> resolvedValues = new()
            {
                { "criteria", queryArgs.GetCriteriaString() }
            };
            string argumentDisplay = new ParsedArguments(new()).FormatArgumentDisplay("gm.query.clan", resolvedValues);

            string criteriaDesc = queryArgs.GetCriteriaString();
            
            if (matchedClans.Count == 0)
            {
                return CommandResult.Success(argumentDisplay + $"Found 0 clan(s) matching {criteriaDesc}\n" +
                       "Usage: gm.query.clan [search] [type keywords] [sort]\n" +
                       "Type keywords: noble, minor, bandit, mercenary, eliminated, empty, etc.\n" +
                       "Sort: sort:name, sort:tier, sort:gold, sort:renown, sort:<type> (add :desc for descending)\n" +
                       "Example: gm.query.clan empire noble sort:name\n").Log().Message;
            }

            return CommandResult.Success(argumentDisplay + $"Found {matchedClans.Count} clan(s) matching {criteriaDesc}:\n" +
                   $"{ClanQueries.GetFormattedDetails(matchedClans)}").Log().Message;
        });
    }
}
