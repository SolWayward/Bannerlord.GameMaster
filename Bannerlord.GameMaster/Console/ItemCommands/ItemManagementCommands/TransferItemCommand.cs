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
    /// Transfer item from one hero's party to another
    /// Usage: gm.item.transfer [item_query] [count] [from_hero] [to_hero]
    /// </summary>
    public static class TransferItemCommand
    {
        [CommandLineFunctionality.CommandLineArgumentFunction("transfer", "gm.item")]
        public static string Execute(List<string> args)
        {
            return Cmd.Run(args, () =>
            {
                // MARK: Validation
                if (!CommandValidator.ValidateCampaignState(out string error))
                    return CommandResult.Error(error).Message;

                string usageMessage = CommandValidator.CreateUsageMessage(
                    "gm.item.transfer", "<item_query> <count> <from_hero> <to_hero>",
                    "Transfers item(s) from one hero's party to another.\n" +
                    "- item: required, item ID or name to transfer\n" +
                    "- count: required, number of items to transfer (1-10000)\n" +
                    "- from/from_hero: required, hero ID or name to take items from\n" +
                    "- to/to_hero: required, hero ID or name to give items to",
                    "gm.item.transfer imperial_sword 5 lord_1_1 player");

                ParsedArguments parsed = ArgumentParser.ParseArguments(args);

                parsed.SetValidArguments(
                    new ArgumentDefinition("item", true),
                    new ArgumentDefinition("count", true),
                    new ArgumentDefinition("from", true, null, "from_hero"),
                    new ArgumentDefinition("to", true, null, "to_hero")
                );

                string validationError = parsed.GetValidationError();
                if (validationError != null)
                    return CommandResult.Error(MessageFormatter.FormatErrorMessage(validationError)).Message;

                if (parsed.TotalCount < 4)
                    return usageMessage;

                // MARK: Parse Arguments
                string itemQuery = parsed.GetArgument("item", 0);
                string countStr = parsed.GetArgument("count", 1);
                string fromHeroQuery = parsed.GetArgument("from", 2) ?? parsed.GetArgument("from_hero", 2);
                string toHeroQuery = parsed.GetArgument("to", 3) ?? parsed.GetArgument("to_hero", 3);

                // Find item
                EntityFinderResult<ItemObject> itemResult = ItemFinder.FindSingleItem(itemQuery);
                if (!itemResult.IsSuccess) return CommandResult.Error(itemResult.Message).Message;
                ItemObject item = itemResult.Entity;

                // Validate count
                if (!CommandValidator.ValidateIntegerRange(countStr, 1, 10000, out int count, out string countError))
                    return CommandResult.Error(MessageFormatter.FormatErrorMessage(countError)).Message;

                // Find from hero
                EntityFinderResult<Hero> fromHeroResult = HeroFinder.FindSingleHero(fromHeroQuery);
                if (!fromHeroResult.IsSuccess) return CommandResult.Error(fromHeroResult.Message).Message;
                Hero fromHero = fromHeroResult.Entity;

                // Find to hero
                EntityFinderResult<Hero> toHeroResult = HeroFinder.FindSingleHero(toHeroQuery);
                if (!toHeroResult.IsSuccess) return CommandResult.Error(toHeroResult.Message).Message;
                Hero toHero = toHeroResult.Entity;

                // MARK: Execute Logic
                if (fromHero.PartyBelongedTo == null)
                    return CommandResult.Error(MessageFormatter.FormatErrorMessage($"{fromHero.Name} does not belong to a party.")).Message;
                if (toHero.PartyBelongedTo == null)
                    return CommandResult.Error(MessageFormatter.FormatErrorMessage($"{toHero.Name} does not belong to a party.")).Message;

                int currentCount = fromHero.PartyBelongedTo.ItemRoster.GetItemNumber(item);
                if (currentCount < count)
                    return CommandResult.Error(MessageFormatter.FormatErrorMessage($"{fromHero.Name}'s party only has {currentCount}x {item.Name}, cannot transfer {count}.")).Message;

                fromHero.PartyBelongedTo.ItemRoster.AddToCounts(item, -count);
                toHero.PartyBelongedTo.ItemRoster.AddToCounts(item, count);

                Dictionary<string, string> resolvedValues = new()
                {
                    ["item"] = item.Name.ToString(),
                    ["count"] = count.ToString(),
                    ["from"] = fromHero.Name.ToString(),
                    ["to"] = toHero.Name.ToString()
                };

                string display = parsed.FormatArgumentDisplay("gm.item.transfer", resolvedValues);
                return CommandResult.Success(display + MessageFormatter.FormatSuccessMessage(
                    $"Transferred {count}x {item.Name} from {fromHero.Name} to {toHero.Name}.")).Message;
            });
        }
    }
}
