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
/// Find kingdoms matching ANY of the specified types (OR logic)
/// Usage: gm.query.kingdom_any [search terms] [type keywords] [sort parameters]
/// Example: gm.query.kingdom_any atwar eliminated sort:name:desc
/// </summary>
public static class QueryKingdomAnyCommand
{
    [CommandLineFunctionality.CommandLineArgumentFunction("kingdom_any", "gm.query")]
    public static string Execute(List<string> args)
    {
        return Cmd.Run(args, () =>
        {
            // MARK: Validation
            if (!CommandValidator.ValidateCampaignState(out string error))
                return CommandResult.Error(error).Message;

            // MARK: Parse Arguments
            KingdomQueryArguments queryArgs = KingdomQueryHelpers.ParseKingdomQueryArguments(args);

            // MARK: Execute Logic
            List<Kingdom> matchedKingdoms = KingdomQueries.QueryKingdoms(
                queryArgs.QueryArgs.Query, 
                queryArgs.Types, 
                matchAll: false, 
                queryArgs.QueryArgs.SortBy, 
                queryArgs.QueryArgs.SortDesc);

            string criteriaDesc = queryArgs.GetCriteriaString();
            
            if (matchedKingdoms.Count == 0)
            {
                return CommandResult.Success($"Found 0 kingdom(s) matching ANY of {criteriaDesc}\n" +
                       "Usage: gm.query.kingdom_any [search] [type keywords] [sort]\n").Message;
            }

            return CommandResult.Success($"Found {matchedKingdoms.Count} kingdom(s) matching ANY of {criteriaDesc}:\n" +
                   $"{KingdomQueries.GetFormattedDetails(matchedKingdoms)}").Message;
        });
    }
}
