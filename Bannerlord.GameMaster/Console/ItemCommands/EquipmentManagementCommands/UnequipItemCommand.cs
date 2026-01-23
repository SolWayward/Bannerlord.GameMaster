using Bannerlord.GameMaster.Console.Common;
using Bannerlord.GameMaster.Console.Common.EntityFinding;
using Bannerlord.GameMaster.Console.Common.Execution;
using Bannerlord.GameMaster.Console.Common.Formatting;
using Bannerlord.GameMaster.Console.Common.Parsing;
using Bannerlord.GameMaster.Console.Common.Validation;
using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Library;

namespace Bannerlord.GameMaster.Console.ItemCommands.EquipmentManagementCommands
{
    /// <summary>
    /// Console command to unequip a specific item if it's currently equipped
    /// </summary>
    public static class UnequipItemCommand
    {
        /// <summary>
        /// Unequip a specific item if it's currently equipped
        /// Usage: gm.item.unequip [item_query] [hero_query]
        /// </summary>
        [CommandLineFunctionality.CommandLineArgumentFunction("unequip", "gm.item")]
        public static string Execute(List<string> args)
        {
            return Cmd.Run(args, () =>
            {
                // MARK: Validation
                if (!CommandValidator.ValidateCampaignState(out string error))
                    return error;

                string usageMessage = CommandValidator.CreateUsageMessage(
                    "gm.item.unequip", "<item_query> <hero_query>",
                    "Unequips a specific item if currently equipped (checks both battle and civilian).",
                    "gm.item.unequip imperial_sword player");

                ParsedArguments parsed = ArgumentParser.ParseArguments(args);

                parsed.SetValidArguments(
                    new ArgumentDefinition("item", true),
                    new ArgumentDefinition("hero", true)
                );

                string validationError = parsed.GetValidationError();
                if (validationError != null)
                    return MessageFormatter.FormatErrorMessage(validationError);

                if (parsed.TotalCount < 2)
                    return usageMessage;

                // MARK: Parse Arguments
                string itemQuery = parsed.GetArgument("item", 0);
                string heroQuery = parsed.GetArgument("hero", 1);

                EntityFinderResult<ItemObject> itemResult = ItemFinder.FindSingleItem(itemQuery);
                if (!itemResult.IsSuccess) return itemResult.Message;
                ItemObject item = itemResult.Entity;

                EntityFinderResult<Hero> heroResult = HeroFinder.FindSingleHero(heroQuery);
                if (!heroResult.IsSuccess) return heroResult.Message;
                Hero hero = heroResult.Entity;

                // MARK: Execute Logic
                bool foundInBattle = false;
                bool foundInCivilian = false;
                List<string> unequippedSlots = new();

                // Check battle equipment
                for (int i = 0; i < (int)EquipmentIndex.NumEquipmentSetSlots; i++)
                {
                    EquipmentIndex slot = (EquipmentIndex)i;
                    if (hero.BattleEquipment[slot].Item == item)
                    {
                        hero.BattleEquipment[slot] = EquipmentElement.Invalid;
                        foundInBattle = true;
                        unequippedSlots.Add($"battle:{slot}");
                    }
                }

                // Check civilian equipment
                for (int i = 0; i < (int)EquipmentIndex.NumEquipmentSetSlots; i++)
                {
                    EquipmentIndex slot = (EquipmentIndex)i;
                    if (hero.CivilianEquipment[slot].Item == item)
                    {
                        hero.CivilianEquipment[slot] = EquipmentElement.Invalid;
                        foundInCivilian = true;
                        unequippedSlots.Add($"civilian:{slot}");
                    }
                }

                if (!foundInBattle && !foundInCivilian)
                    return MessageFormatter.FormatErrorMessage($"{item.Name} is not currently equipped by {hero.Name}.");

                Dictionary<string, string> resolvedValues = new()
                {
                    { "item", item.Name.ToString() },
                    { "hero", hero.Name.ToString() }
                };

                string argumentDisplay = parsed.FormatArgumentDisplay("gm.item.unequip", resolvedValues);
                return CommandResult.Success(argumentDisplay + MessageFormatter.FormatSuccessMessage(
                    $"Unequipped {item.Name} from {hero.Name} (removed from: {string.Join(", ", unequippedSlots)}).")).Message
;
            });
        }
    }
}
