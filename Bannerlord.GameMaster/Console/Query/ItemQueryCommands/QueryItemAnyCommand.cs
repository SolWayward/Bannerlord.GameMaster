using Bannerlord.GameMaster.Console.Common.Execution;
using Bannerlord.GameMaster.Console.Common.Validation;
using Bannerlord.GameMaster.Console.Query.CommandQueryHelpers;
using Bannerlord.GameMaster.Items;
using System.Collections.Generic;
using TaleWorlds.Core;
using TaleWorlds.Library;

namespace Bannerlord.GameMaster.Console.Query.ItemQueryCommands;

/// <summary>
/// Find items matching ANY of the specified types (OR logic)
/// Usage: gm.query.item_any [search terms] [type keywords] [tier keywords] [sort parameters]
/// Example: gm.query.item_any weapon armor (finds anything that is weapon OR armor)
/// Example: gm.query.item_any bow crossbow tier5 sort:value
/// </summary>
public static class QueryItemAnyCommand
{
    [CommandLineFunctionality.CommandLineArgumentFunction("item_any", "gm.query")]
    public static string Execute(List<string> args)
    {
        return Cmd.Run(args, () =>
        {
            // MARK: Validation
            if (!CommandValidator.ValidateCampaignState(out string error))
                return error;

            // MARK: Parse Arguments
            ItemQueryArguments queryArgs = ItemQueryHelpers.ParseItemQueryArguments(args);

            // MARK: Execute Logic
            List<ItemObject> matchedItems = ItemQueries.QueryItems(
                queryArgs.QueryArgs.Query, 
                queryArgs.Types, 
                matchAll: false, 
                queryArgs.Tier, 
                queryArgs.QueryArgs.SortBy, 
                queryArgs.QueryArgs.SortDesc);

            string criteriaDesc = queryArgs.GetCriteriaString();

            if (matchedItems.Count == 0)
            {
                return $"Found 0 item(s) matching ANY of {criteriaDesc}\n" +
                       "Usage: gm.query.item_any [search] [type keywords] [tier] [sort]\n" +
                       "Example: gm.query.item_any weapon armor tier3 sort:name\n";
            }

            return $"Found {matchedItems.Count} item(s) matching ANY of {criteriaDesc}:\n" +
                   $"{ItemQueries.GetFormattedDetails(matchedItems)}";
        });
    }
}
