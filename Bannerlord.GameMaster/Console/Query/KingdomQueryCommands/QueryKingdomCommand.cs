using Bannerlord.GameMaster.Console.Common;
using Bannerlord.GameMaster.Console.Common.Execution;
using Bannerlord.GameMaster.Console.Common.Validation;
using Bannerlord.GameMaster.Console.Query.CommandQueryHelpers;
using Bannerlord.GameMaster.Kingdoms;
using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Library;

namespace Bannerlord.GameMaster.Console.Query.KingdomQueryCommands;

/// <summary>
/// Unified kingdom listing command with AND logic for type filters
/// Usage: gm.query.kingdom [search terms] [type keywords] [sort parameters]
/// Example: gm.query.kingdom empire atwar
/// Example: gm.query.kingdom eliminated sort:name
/// Example: gm.query.kingdom sort:strength:desc
/// Example: gm.query.kingdom sort:atwar (sorts by atwar flag)
/// </summary>
public static class QueryKingdomCommand
{
    [CommandLineFunctionality.CommandLineArgumentFunction("kingdom", "gm.query")]
    public static string Execute(List<string> args)
    {
        return Cmd.Run(args, () =>
        {
            // MARK: Validation
            if (!CommandValidator.ValidateCampaignState(out string error))
                return CommandResult.Error(error).Log().Message;

            // MARK: Parse Arguments
            KingdomQueryArguments queryArgs = KingdomQueryHelpers.ParseKingdomQueryArguments(args);

            // MARK: Execute Logic
            List<Kingdom> matchedKingdoms = KingdomQueries.QueryKingdoms(
                queryArgs.QueryArgs.Query, 
                queryArgs.Types, 
                matchAll: true, 
                queryArgs.QueryArgs.SortBy, 
                queryArgs.QueryArgs.SortDesc);

            string criteriaDesc = queryArgs.GetCriteriaString();
            
            if (matchedKingdoms.Count == 0)
            {
                return CommandResult.Success($"Found 0 kingdom(s) matching {criteriaDesc}\n" +
                       "Usage: gm.query.kingdom [search] [type keywords] [sort]\n" +
                       "Type keywords: active, eliminated, empty, atwar, player, etc.\n" +
                       "Sort: sort:name, sort:clans, sort:heroes, sort:fiefs, sort:strength, sort:<type> (add :desc for descending)\n" +
                       "Example: gm.query.kingdom empire atwar sort:strength:desc\n").Log().Message;
            }

            return CommandResult.Success($"Found {matchedKingdoms.Count} kingdom(s) matching {criteriaDesc}:\n" +
                   $"{KingdomQueries.GetFormattedDetails(matchedKingdoms)}").Log().Message;
        });
    }
}
