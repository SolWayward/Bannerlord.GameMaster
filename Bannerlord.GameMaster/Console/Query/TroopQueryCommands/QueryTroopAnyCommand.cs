using Bannerlord.GameMaster.Console.Common;
using Bannerlord.GameMaster.Console.Common.Execution;
using Bannerlord.GameMaster.Console.Common.Validation;
using Bannerlord.GameMaster.Console.Query.CommandQueryHelpers;
using Bannerlord.GameMaster.Troops;
using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Library;

namespace Bannerlord.GameMaster.Console.Query.TroopQueryCommands;

/// <summary>
/// Find troops matching ANY of the specified types (OR logic)
/// Usage: gm.query.troop_any [search terms] [type keywords] [tier] [sort parameters]
/// Example: gm.query.troop_any cavalry ranged (cavalry OR ranged)
/// Example: gm.query.troop_any bow crossbow tier4
/// Example: gm.query.troop_any empire vlandia infantry
/// </summary>
public static class QueryTroopAnyCommand
{
    [CommandLineFunctionality.CommandLineArgumentFunction("troop_any", "gm.query")]
    public static string Execute(List<string> args)
    {
        return Cmd.Run(args, () =>
        {
            // MARK: Validation
            if (!CommandValidator.ValidateCampaignState(out string error))
                return CommandResult.Error(error).Log().Message;

            // MARK: Parse Arguments
            TroopQueryArguments queryArgs = TroopQueryHelpers.ParseTroopQueryArguments(args);

            // MARK: Execute Logic
            List<CharacterObject> matchedTroops = TroopQueries.QueryTroops(
                queryArgs.QueryArgs.Query, 
                queryArgs.Types, 
                matchAll: false, 
                queryArgs.Tier, 
                queryArgs.QueryArgs.SortBy, 
                queryArgs.QueryArgs.SortDesc);

            string criteriaDesc = queryArgs.GetCriteriaString();

            if (matchedTroops.Count == 0)
            {
                return CommandResult.Success($"Found 0 troop(s) matching ANY of {criteriaDesc}\n" +
                       "Usage: gm.query.troop_any [search] [type keywords] [tier] [sort]\n" +
                       "Example: gm.query.troop_any cavalry ranged tier3 sort:tier\n" +
                       "Note: Non-troops (heroes, NPCs, children, templates, etc.) are automatically excluded.\n").Log().Message;
            }

            return CommandResult.Success($"Found {matchedTroops.Count} troop(s) matching ANY of {criteriaDesc}:\n" +
                   $"{TroopQueries.GetFormattedDetails(matchedTroops)}").Log().Message;
        });
    }
}
