using System.Collections.Generic;
using System.Linq;
using System.Text;
using Bannerlord.GameMaster.Console.Common;
using Bannerlord.GameMaster.Console.Common.Execution;
using Bannerlord.GameMaster.Console.Common.EntityFinding;
using Bannerlord.GameMaster.Console.Common.Formatting;
using Bannerlord.GameMaster.Console.Common.Parsing;
using Bannerlord.GameMaster.Console.Common.Validation;
using Bannerlord.GameMaster.Items;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Roster;
using TaleWorlds.Core;
using TaleWorlds.Library;

namespace Bannerlord.GameMaster.Console.ItemCommands.ItemManagementCommands
{
    /// <summary>
    /// Change modifier on all compatible items in a hero's party inventory
    /// Usage: gm.item.set_inventory_modifier [hero_query] [modifier]
    /// </summary>
    public static class SetInventoryModifierCommand
    {
        [CommandLineFunctionality.CommandLineArgumentFunction("set_inventory_modifier", "gm.item")]
        public static string Execute(List<string> args)
        {
            return Cmd.Run(args, () =>
            {
                // MARK: Validation
                if (!CommandValidator.ValidateCampaignState(out string error))
                    return error;

                string usageMessage = CommandValidator.CreateUsageMessage(
                    "gm.item.set_inventory_modifier", "<hero_query> <modifier>",
                    "Sets modifier on all compatible items in a hero's party inventory.\n" +
                    "- hero: required, hero ID or name whose party inventory items will be modified\n" +
                    "- modifier: required, quality modifier to apply (e.g., masterwork, fine, legendary)",
                    "gm.item.set_inventory_modifier player legendary\n" +
                    "gm.item.set_inventory_modifier lord_1_1 fine");

                ParsedArguments parsed = ArgumentParser.ParseArguments(args);

                parsed.SetValidArguments(
                    new ArgumentDefinition("hero", true),
                    new ArgumentDefinition("modifier", true)
                );

                string validationError = parsed.GetValidationError();
                if (validationError != null)
                    return MessageFormatter.FormatErrorMessage(validationError);

                if (parsed.TotalCount < 2)
                    return usageMessage;

                // MARK: Parse Arguments
                string heroQuery = parsed.GetArgument("hero", 0);
                
                // Find hero
                EntityFinderResult<Hero> heroResult = HeroFinder.FindSingleHero(heroQuery);
                if (!heroResult.IsSuccess) return heroResult.Message;
                Hero hero = heroResult.Entity;

                // Parse modifier (could be multi-word, so join remaining args)
                string modifierName = string.Join(" ", args.Skip(1));
                (ItemModifier modifier, string modError) = ItemModifierHelper.ParseModifier(modifierName);

                if (modError != null)
                    return MessageFormatter.FormatErrorMessage(modError);

                if (modifier == null)
                    return MessageFormatter.FormatErrorMessage($"Modifier '{modifierName}' not found.");

                // MARK: Execute Logic
                if (hero.PartyBelongedTo == null)
                    return MessageFormatter.FormatErrorMessage($"{hero.Name} does not belong to a party.");

                ItemRoster roster = hero.PartyBelongedTo.ItemRoster;
                int itemTypesChanged = 0;
                int totalItemsChanged = 0;
                List<string> changedItems = new();

                // Collect items that can be modified
                List<(ItemObject item, int count, ItemModifier oldModifier)> itemsToModify = new();

                for (int i = 0; i < roster.Count; i++)
                {
                    ItemRosterElement element = roster.GetElementCopyAtIndex(i);
                    if (ItemModifierHelper.CanHaveModifier(element.EquipmentElement.Item))
                    {
                        itemsToModify.Add((element.EquipmentElement.Item, element.Amount, element.EquipmentElement.ItemModifier));
                    }
                }

                // Remove old items and add with new modifier
                foreach ((ItemObject item, int count, ItemModifier oldModifier) in itemsToModify)
                {
                    // Remove old version
                    roster.AddToCounts(new EquipmentElement(item, oldModifier), -count);

                    // Add new version with modifier
                    roster.AddToCounts(new EquipmentElement(item, modifier), count);

                    string oldModText = oldModifier != null ? $" ({oldModifier.Name})" : "";
                    changedItems.Add($"{count}x {item.Name}{oldModText}");
                    itemTypesChanged++;
                    totalItemsChanged += count;
                }

                if (itemTypesChanged == 0)
                    return MessageFormatter.FormatSuccessMessage($"{hero.Name}'s party has no items that can have modifiers.");

                Dictionary<string, string> resolvedValues = new()
                {
                    ["hero"] = hero.Name.ToString(),
                    ["modifier"] = modifier.Name.ToString()
                };

                StringBuilder result = new();
                string display = parsed.FormatArgumentDisplay("gm.item.set_inventory_modifier", resolvedValues);
                result.Append(display);
                result.AppendLine(MessageFormatter.FormatSuccessMessage(
                    $"Set modifier '{modifier.Name}' on {totalItemsChanged} items ({itemTypesChanged} types) in {hero.Name}'s party:"));
                
                foreach (string item in changedItems.Take(10)) // Limit display to first 10
                {
                    result.AppendLine($"  - {item}");
                }
                if (changedItems.Count > 10)
                {
                    result.AppendLine($"  ... and {changedItems.Count - 10} more item types");
                }

                return CommandResult.Success(result.ToString()).Message;
            });
        }
    }
}
