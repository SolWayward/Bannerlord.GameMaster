using Bannerlord.GameMaster.Console.Common;
using Bannerlord.GameMaster.Console.Common.Execution;
using Bannerlord.GameMaster.Console.Common.Validation;
using Bannerlord.GameMaster.Console.Query.CommandQueryHelpers;
using Bannerlord.GameMaster.Items;
using System.Collections.Generic;
using TaleWorlds.Core;
using TaleWorlds.Library;

namespace Bannerlord.GameMaster.Console.Query.ItemQueryCommands;

/// <summary>
/// Unified item listing command with AND logic for type filters
/// Usage: gm.query.item [search terms] [type keywords] [tier keywords] [sort parameters]
/// Example: gm.query.item sword weapon 1h
/// Example: gm.query.item imperial armor tier3
/// Example: gm.query.item food sort:value:desc
/// Example: gm.query.item bow ranged tier4 sort:name
/// </summary>
public static class QueryItemCommand
{
    [CommandLineFunctionality.CommandLineArgumentFunction("item", "gm.query")]
    public static string Execute(List<string> args)
    {
        return Cmd.Run(args, () =>
        {
            // MARK: Validation
            if (!CommandValidator.ValidateCampaignState(out string error))
                return CommandResult.Error(error).Log().Message;

            // MARK: Parse Arguments
            ItemQueryArguments queryArgs = ItemQueryHelpers.ParseItemQueryArguments(args);

            // MARK: Execute Logic
            List<ItemObject> matchedItems = ItemQueries.QueryItems(
                queryArgs.QueryArgs.Query, 
                queryArgs.Types, 
                matchAll: true, 
                queryArgs.Tier, 
                queryArgs.QueryArgs.SortBy, 
                queryArgs.QueryArgs.SortDesc);

            string criteriaDesc = queryArgs.GetCriteriaString();

            if (matchedItems.Count == 0)
            {
                return CommandResult.Success($"Found 0 item(s) matching {criteriaDesc}\n" +
                       "Usage: gm.query.item [search] [type keywords] [tier] [sort]\n" +
                       "Type keywords: weapon, armor, mount, food, trade, 1h, 2h, ranged, bow, crossbow, civilian, combat, horsearmor, etc.\n" +
                       "Tier keywords: tier0, tier1, tier2, tier3, tier4, tier5, tier6\n" +
                       "Sort: sort:name, sort:tier, sort:value, sort:type (add :desc for descending)\n" +
                       "Example: gm.query.item sword weapon 1h tier3 sort:value:desc\n").Log().Message;
            }

            return CommandResult.Success($"Found {matchedItems.Count} item(s) matching {criteriaDesc}:\n" +
                   $"{ItemQueries.GetFormattedDetails(matchedItems)}").Log().Message;
        });
    }
}
