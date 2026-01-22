using Bannerlord.GameMaster.Console.Common;
using Bannerlord.GameMaster.Console.Common.EntityFinding;
using Bannerlord.GameMaster.Console.Common.Execution;
using Bannerlord.GameMaster.Console.Common.Formatting;
using Bannerlord.GameMaster.Console.Common.Parsing;
using Bannerlord.GameMaster.Console.Common.Validation;
using System;
using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Library;

namespace Bannerlord.GameMaster.Console.ItemCommands.EquipmentManagementCommands
{
    /// <summary>
    /// Console command to equip an item to a hero (auto-detects appropriate slot)
    /// </summary>
    public static class EquipItemCommand
    {
        /// <summary>
        /// Equip a specific item to a hero (auto-detects appropriate slot)
        /// Usage: gm.item.equip [item_query] [hero_query] [civilian]
        /// </summary>
        [CommandLineFunctionality.CommandLineArgumentFunction("equip", "gm.item")]
        public static string Execute(List<string> args)
        {
            return Cmd.Run(args, () =>
            {
                // MARK: Validation
                if (!CommandValidator.ValidateCampaignState(out string error))
                    return error;

                string usageMessage = CommandValidator.CreateUsageMessage(
                    "gm.item.equip", "<item_query> <hero_query> [civilian]",
                    "Equips an item to a hero's first available slot. Add 'civilian' for civilian equipment.",
                    "gm.item.equip imperial_sword player\n" +
                    "gm.item.equip robe player civilian");

                ParsedArguments parsed = ArgumentParser.ParseArguments(args);

                parsed.SetValidArguments(
                    new ArgumentDefinition("item", true),
                    new ArgumentDefinition("hero", true),
                    new ArgumentDefinition("civilian", false)
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

                string civilianArg = parsed.GetArgument("civilian", 2);
                bool isCivilian = civilianArg != null && civilianArg.Equals("civilian", StringComparison.OrdinalIgnoreCase);

                // MARK: Execute Logic
                EquipmentIndex slot = ItemCommandHelpers.GetAppropriateSlotForItem(item);
                if (slot == EquipmentIndex.None)
                    return MessageFormatter.FormatErrorMessage($"{item.Name} cannot be equipped (no appropriate slot).");

                Equipment equipment = isCivilian ? hero.CivilianEquipment : hero.BattleEquipment;
                equipment[slot] = new EquipmentElement(item);

                string equipmentType = isCivilian ? "civilian" : "battle";

                Dictionary<string, string> resolvedValues = new()
                {
                    { "item", item.Name.ToString() },
                    { "hero", hero.Name.ToString() },
                    { "civilian", isCivilian.ToString() }
                };

                string argumentDisplay = parsed.FormatArgumentDisplay("gm.item.equip", resolvedValues);
                return argumentDisplay + MessageFormatter.FormatSuccessMessage(
                    $"Equipped {item.Name} to {hero.Name}'s {equipmentType} equipment (slot: {slot}).");
            });
        }
    }
}
