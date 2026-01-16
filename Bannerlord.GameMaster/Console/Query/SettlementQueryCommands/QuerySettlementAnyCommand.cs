using Bannerlord.GameMaster.Console.Common.Execution;
using Bannerlord.GameMaster.Console.Common.Validation;
using Bannerlord.GameMaster.Console.Query.CommandQueryHelpers;
using Bannerlord.GameMaster.Settlements;
using System.Collections.Generic;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Library;

namespace Bannerlord.GameMaster.Console.Query.SettlementQueryCommands;

/// <summary>
/// Find settlements matching ANY of the specified types (OR logic)
/// Usage: gm.query.settlement_any [search terms] [type keywords] [sort parameters]
/// Example: gm.query.settlement_any castle city (finds castles OR cities)
/// Example: gm.query.settlement_any empire vlandia sort:name
/// </summary>
public static class QuerySettlementAnyCommand
{
    [CommandLineFunctionality.CommandLineArgumentFunction("settlement_any", "gm.query")]
    public static string Execute(List<string> args)
    {
        return Cmd.Run(args, () =>
        {
            // MARK: Validation
            if (!CommandValidator.ValidateCampaignState(out string error))
                return error;

            // MARK: Parse Arguments
            SettlementQueryArguments queryArgs = SettlementQueryHelpers.ParseSettlementQueryArguments(args);

            // MARK: Execute Logic
            List<Settlement> matchedSettlements = SettlementQueries.QuerySettlements(
                queryArgs.QueryArgs.Query, 
                queryArgs.Types, 
                matchAll: false, 
                queryArgs.QueryArgs.SortBy, 
                queryArgs.QueryArgs.SortDesc);

            string criteriaDesc = queryArgs.GetCriteriaString();

            if (matchedSettlements.Count == 0)
            {
                return $"Found 0 settlement(s) matching ANY of {criteriaDesc}\n" +
                       "Usage: gm.query.settlement_any [search] [type keywords] [sort]\n" +
                       "Example: gm.query.settlement_any castle city sort:prosperity:desc\n";
            }

            return $"Found {matchedSettlements.Count} settlement(s) matching ANY of {criteriaDesc}:\n" +
                   $"{SettlementQueries.GetFormattedDetails(matchedSettlements)}";
        });
    }
}
