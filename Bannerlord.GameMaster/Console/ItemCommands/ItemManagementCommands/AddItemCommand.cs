using System.Collections.Generic;
using System.Linq;
using Bannerlord.GameMaster.Console.Common;
using Bannerlord.GameMaster.Console.Common.Execution;
using Bannerlord.GameMaster.Console.Common.EntityFinding;
using Bannerlord.GameMaster.Console.Common.Formatting;
using Bannerlord.GameMaster.Console.Common.Parsing;
using Bannerlord.GameMaster.Console.Common.Validation;
using Bannerlord.GameMaster.Items;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Library;

namespace Bannerlord.GameMaster.Console.ItemCommands.ItemManagementCommands
{
    /// <summary>
    /// Add item(s) to a hero's party inventory
    /// Usage: gm.item.add [item_query] [count] [hero_query] [modifier]
    /// </summary>
    public static class AddItemCommand
    {
        [CommandLineFunctionality.CommandLineArgumentFunction("add", "gm.item")]
        public static string Execute(List<string> args)
        {
            return Cmd.Run(args, () =>
            {
                // MARK: Validation
                if (!CommandValidator.ValidateCampaignState(out string error))
                    return error;

                string usageMessage = CommandValidator.CreateUsageMessage(
                    "gm.item.add", "<item_query> <count> <hero_query> [modifier]",
                    "Adds item(s) to a hero's party inventory with optional quality modifier.\n" +
                    "- item/item_query: required, item ID or name to add\n" +
                    "- count: required, number of items to add (1-10000)\n" +
                    "- hero: required, hero ID or name who will receive items\n" +
                    "- modifier: optional, quality modifier (e.g., masterwork, fine, legendary)",
                    "gm.item.add imperial_sword 5 player\n" +
                    "gm.item.add sturgia_axe 1 lord_1_1 masterwork\n" +
                    "gm.item.add shield 3 player fine");

                ParsedArguments parsed = ArgumentParser.ParseArguments(args);

                parsed.SetValidArguments(
                    new ArgumentDefinition("item", true),
                    new ArgumentDefinition("count", true),
                    new ArgumentDefinition("hero", true),
                    new ArgumentDefinition("modifier", false, "None")
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

                // Parse optional modifier
                ItemModifier modifier = null;
                string modifierText = "None";
                if (args.Count > 3)
                {
                    string modifierName = string.Join(" ", args.Skip(3));
                    (ItemModifier parsedModifier, string modError) = ItemModifierHelper.ParseModifier(modifierName);

                    if (modError != null)
                        return MessageFormatter.FormatErrorMessage(modError);

                    if (parsedModifier != null && !ItemModifierHelper.CanHaveModifier(item))
                        return MessageFormatter.FormatErrorMessage($"{item.Name} cannot have quality modifiers.");

                    modifier = parsedModifier;
                    modifierText = modifier?.Name?.ToString() ?? "None";
                }

                // MARK: Execute Logic
                if (hero.PartyBelongedTo == null)
                    return MessageFormatter.FormatErrorMessage($"{hero.Name} does not belong to a party and cannot hold items.");

                EquipmentElement equipElement = new(item, modifier);
                hero.PartyBelongedTo.ItemRoster.AddToCounts(equipElement, count);

                Dictionary<string, string> resolvedValues = new()
                {
                    ["item"] = item.Name.ToString(),
                    ["count"] = count.ToString(),
                    ["hero"] = hero.Name.ToString(),
                    ["modifier"] = modifierText
                };

                string display = parsed.FormatArgumentDisplay("gm.item.add", resolvedValues);
                string modText = modifier != null ? $" ({modifier.Name})" : "";
                return CommandResult.Success(display + MessageFormatter.FormatSuccessMessage(
                    $"Added {count}x {item.Name}{modText} to {hero.Name}'s party inventory.")).Message;
            });
        }
    }
}
