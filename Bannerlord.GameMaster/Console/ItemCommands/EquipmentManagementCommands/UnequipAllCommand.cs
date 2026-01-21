using Bannerlord.GameMaster.Console.Common;
using Bannerlord.GameMaster.Console.Common.EntityFinding;
using Bannerlord.GameMaster.Console.Common.Execution;
using Bannerlord.GameMaster.Console.Common.Formatting;
using Bannerlord.GameMaster.Console.Common.Parsing;
using Bannerlord.GameMaster.Console.Common.Validation;
using System.Collections.Generic;
using System.Text;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Library;

namespace Bannerlord.GameMaster.Console.ItemCommands.EquipmentManagementCommands
{
    /// <summary>
    /// Console command to unequip all items from a hero and add them back to inventory
    /// </summary>
    public static class UnequipAllCommand
    {
        /// <summary>
        /// Unequip all items from a hero and add them back to inventory (both battle and civilian equipment)
        /// Usage: gm.item.unequip_all [hero_query]
        /// </summary>
        [CommandLineFunctionality.CommandLineArgumentFunction("unequip_all", "gm.item")]
        public static string Execute(List<string> args)
        {
            return Cmd.Run(args, () =>
            {
                // MARK: Validation
                if (!CommandValidator.ValidateCampaignState(out string error))
                    return error;

                string usageMessage = CommandValidator.CreateUsageMessage(
                    "gm.item.unequip_all", "<hero_query>",
                    "Unequips all items from a hero and adds them to party inventory (battle and civilian equipment).",
                    "gm.item.unequip_all player");

                ParsedArguments parsed = ArgumentParser.ParseArguments(args);

                parsed.SetValidArguments(
                    new ArgumentDefinition("hero", true)
                );

                string validationError = parsed.GetValidationError();
                if (validationError != null)
                    return MessageFormatter.FormatErrorMessage(validationError);

                if (parsed.TotalCount < 1)
                    return usageMessage;

                // MARK: Parse Arguments
                string heroQuery = parsed.GetArgument("hero", 0);

                EntityFinderResult<Hero> heroResult = HeroFinder.FindSingleHero(heroQuery);
                if (!heroResult.IsSuccess) return heroResult.Message;
                Hero hero = heroResult.Entity;

                // MARK: Execute Logic
                if (hero.PartyBelongedTo == null)
                    return MessageFormatter.FormatErrorMessage($"{hero.Name} does not belong to a party and cannot hold items in inventory.");

                int itemsUnequipped = 0;
                List<string> unequippedItems = new();

                // Unequip battle equipment
                for (int i = 0; i < (int)EquipmentIndex.NumEquipmentSetSlots; i++)
                {
                    EquipmentIndex slot = (EquipmentIndex)i;
                    EquipmentElement element = hero.BattleEquipment[slot];
                    if (!element.IsEmpty)
                    {
                        // Add to party inventory
                        hero.PartyBelongedTo.ItemRoster.AddToCounts(element, 1);
                        unequippedItems.Add($"{element.Item.Name} (battle:{slot})");
                        itemsUnequipped++;

                        // Remove from equipment
                        hero.BattleEquipment[slot] = EquipmentElement.Invalid;
                    }
                }

                // Unequip civilian equipment
                for (int i = 0; i < (int)EquipmentIndex.NumEquipmentSetSlots; i++)
                {
                    EquipmentIndex slot = (EquipmentIndex)i;
                    EquipmentElement element = hero.CivilianEquipment[slot];
                    if (!element.IsEmpty)
                    {
                        // Add to party inventory
                        hero.PartyBelongedTo.ItemRoster.AddToCounts(element, 1);
                        unequippedItems.Add($"{element.Item.Name} (civilian:{slot})");
                        itemsUnequipped++;

                        // Remove from equipment
                        hero.CivilianEquipment[slot] = EquipmentElement.Invalid;
                    }
                }

                if (itemsUnequipped == 0)
                    return MessageFormatter.FormatSuccessMessage($"{hero.Name} has no items equipped.");

                StringBuilder result = new();
                result.AppendLine($"Unequipped {itemsUnequipped} items from {hero.Name} and added them to party inventory:");
                foreach (string item in unequippedItems)
                {
                    result.AppendLine($"  - {item}");
                }

                return result.ToString();
            });
        }
    }
}
