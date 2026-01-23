using System.Collections.Generic;
using Bannerlord.GameMaster.Console.Common;
using Bannerlord.GameMaster.Console.Common.Execution;
using Bannerlord.GameMaster.Console.Common.EntityFinding;
using Bannerlord.GameMaster.Console.Common.Formatting;
using Bannerlord.GameMaster.Console.Common.Parsing;
using Bannerlord.GameMaster.Console.Common.Validation;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Library;

namespace Bannerlord.GameMaster.Console.ItemCommands.ItemManagementCommands
{
    /// <summary>
    /// Remove specific item(s) from a hero's party inventory
    /// Usage: gm.item.remove [item_query] [count] [hero_query]
    /// </summary>
    public static class RemoveItemCommand
    {
        [CommandLineFunctionality.CommandLineArgumentFunction("remove", "gm.item")]
        public static string Execute(List<string> args)
        {
            return Cmd.Run(args, () =>
            {
                // MARK: Validation
                if (!CommandValidator.ValidateCampaignState(out string error))
                    return error;

                string usageMessage = CommandValidator.CreateUsageMessage(
                    "gm.item.remove", "<item_query> <count> <hero_query>",
                    "Removes specific item(s) from a hero's party inventory.\n" +
                    "- item/item_query: required, item ID or name to remove\n" +
                    "- count: required, number of items to remove (1-10000)\n" +
                    "- hero: required, hero ID or name whose party will lose items",
                    "gm.item.remove imperial_sword 5 player");

                ParsedArguments parsed = ArgumentParser.ParseArguments(args);

                parsed.SetValidArguments(
                    new ArgumentDefinition("item", true),
                    new ArgumentDefinition("count", true),
                    new ArgumentDefinition("hero", true)
                );

                string validationError = parsed.GetValidationError();
                if (validationError != null)
                    return MessageFormatter.FormatErrorMessage(validationError);

                if (parsed.TotalCount < 3)
                    return usageMessage;

                // MARK: Parse Arguments
                string itemQuery = parsed.GetArgument("item", 0);
                string countStr = parsed.GetArgument("count", 1);
                string heroQuery = parsed.GetArgument("hero", 2);

                // Find item
                EntityFinderResult<ItemObject> itemResult = ItemFinder.FindSingleItem(itemQuery);
                if (!itemResult.IsSuccess) return itemResult.Message;
                ItemObject item = itemResult.Entity;

                // Validate count
                if (!CommandValidator.ValidateIntegerRange(countStr, 1, 10000, out int count, out string countError))
                    return MessageFormatter.FormatErrorMessage(countError);

                // Find hero
                EntityFinderResult<Hero> heroResult = HeroFinder.FindSingleHero(heroQuery);
                if (!heroResult.IsSuccess) return heroResult.Message;
                Hero hero = heroResult.Entity;

                // MARK: Execute Logic
                if (hero.PartyBelongedTo == null)
                    return MessageFormatter.FormatErrorMessage($"{hero.Name} does not belong to a party.");

                int currentCount = hero.PartyBelongedTo.ItemRoster.GetItemNumber(item);
                if (currentCount < count)
                    return MessageFormatter.FormatErrorMessage($"{hero.Name}'s party only has {currentCount}x {item.Name}, cannot remove {count}.");

                hero.PartyBelongedTo.ItemRoster.AddToCounts(item, -count);

                Dictionary<string, string> resolvedValues = new()
                {
                    ["item"] = item.Name.ToString(),
                    ["count"] = count.ToString(),
                    ["hero"] = hero.Name.ToString()
                };

                string display = parsed.FormatArgumentDisplay("gm.item.remove", resolvedValues);
                return CommandResult.Success(display + MessageFormatter.FormatSuccessMessage(
                    $"Removed {count}x {item.Name} from {hero.Name}'s party inventory.")).Message;
            });
        }
    }
}
