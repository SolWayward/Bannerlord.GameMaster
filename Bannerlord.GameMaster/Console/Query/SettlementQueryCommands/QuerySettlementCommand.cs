using Bannerlord.GameMaster.Console.Common.Execution;
using Bannerlord.GameMaster.Console.Common.Validation;
using Bannerlord.GameMaster.Console.Query.CommandQueryHelpers;
using Bannerlord.GameMaster.Settlements;
using System.Collections.Generic;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Library;

namespace Bannerlord.GameMaster.Console.Query.SettlementQueryCommands;

/// <summary>
/// Unified settlement listing command with AND logic for type filters
/// Usage: gm.query.settlement [search terms] [type keywords] [sort parameters]
/// Example: gm.query.settlement castle empire
/// Example: gm.query.settlement pen city sort:prosperity:desc
/// Example: gm.query.settlement player town sort:name
/// </summary>
public static class QuerySettlementCommand
{
    [CommandLineFunctionality.CommandLineArgumentFunction("settlement", "gm.query")]
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
                matchAll: true, 
                queryArgs.QueryArgs.SortBy, 
                queryArgs.QueryArgs.SortDesc);

            string criteriaDesc = queryArgs.GetCriteriaString();

            if (matchedSettlements.Count == 0)
            {
                return $"Found 0 settlement(s) matching {criteriaDesc}\n" +
                       "Usage: gm.query.settlement [search] [type keywords] [sort]\n" +
                       "Type keywords: town, castle, city, village, hideout, player, besieged, raided, empire, vlandia, etc.\n" +
                       "Prosperity: low, medium, high\n" +
                       "Sort: sort:name, sort:prosperity, sort:owner, sort:kingdom, sort:culture (add :desc for descending)\n" +
                       "Example: gm.query.settlement castle empire sort:prosperity:desc\n";
            }

            return $"Found {matchedSettlements.Count} settlement(s) matching {criteriaDesc}:\n" +
                   $"{SettlementQueries.GetFormattedDetails(matchedSettlements)}";
        });
    }
}
