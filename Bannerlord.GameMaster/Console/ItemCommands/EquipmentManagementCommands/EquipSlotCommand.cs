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
    /// Console command to equip an item to a specific equipment slot
    /// </summary>
    public static class EquipSlotCommand
    {
        /// <summary>
        /// Equip item to specific equipment slot
        /// Usage: gm.item.equip_slot [item_query] [hero_query] [slot] [civilian]
        /// </summary>
        [CommandLineFunctionality.CommandLineArgumentFunction("equip_slot", "gm.item")]
        public static string Execute(List<string> args)
        {
            return Cmd.Run(args, () =>
            {
                // MARK: Validation
                if (!CommandValidator.ValidateCampaignState(out string error))
                    return error;

                string usageMessage = CommandValidator.CreateUsageMessage(
                    "gm.item.equip_slot", "<item_query> <hero_query> <slot> [civilian]",
                    "Equips an item to a specific equipment slot. Add 'civilian' for civilian equipment.\n" +
                    "Valid slots: Head, Body, Leg, Gloves, Cape, Horse, HorseHarness, Weapon0-3",
                    "gm.item.equip_slot imperial_sword player Weapon0\n" +
                    "gm.item.equip_slot fine_robe player Body civilian");

                ParsedArguments parsed = ArgumentParser.ParseArguments(args);

                parsed.SetValidArguments(
                    new ArgumentDefinition("item", true),
                    new ArgumentDefinition("hero", true),
                    new ArgumentDefinition("slot", true),
                    new ArgumentDefinition("civilian", false)
                );

                string validationError = parsed.GetValidationError();
                if (validationError != null)
                    return MessageFormatter.FormatErrorMessage(validationError);

                if (parsed.TotalCount < 3)
                    return usageMessage;

                // MARK: Parse Arguments
                string itemQuery = parsed.GetArgument("item", 0);
                string heroQuery = parsed.GetArgument("hero", 1);
                string slotArg = parsed.GetArgument("slot", 2);

                EntityFinderResult<ItemObject> itemResult = ItemFinder.FindSingleItem(itemQuery);
                if (!itemResult.IsSuccess) return itemResult.Message;
                ItemObject item = itemResult.Entity;

                EntityFinderResult<Hero> heroResult = HeroFinder.FindSingleHero(heroQuery);
                if (!heroResult.IsSuccess) return heroResult.Message;
                Hero hero = heroResult.Entity;

                if (!ItemCommandHelpers.TryParseEquipmentSlot(slotArg, out EquipmentIndex slot))
                    return MessageFormatter.FormatErrorMessage($"Invalid equipment slot: '{slotArg}'. Valid slots: Head, Body, Leg, Gloves, Cape, Horse, HorseHarness, Weapon0, Weapon1, Weapon2, Weapon3.");

                string civilianArg = parsed.GetArgument("civilian", 3);
                bool isCivilian = civilianArg != null && civilianArg.Equals("civilian", StringComparison.OrdinalIgnoreCase);

                // MARK: Execute Logic
                Equipment equipment = isCivilian ? hero.CivilianEquipment : hero.BattleEquipment;
                equipment[slot] = new EquipmentElement(item);

                string equipmentType = isCivilian ? "civilian" : "battle";

                Dictionary<string, string> resolvedValues = new()
                {
                    { "item", item.Name.ToString() },
                    { "hero", hero.Name.ToString() },
                    { "slot", slot.ToString() },
                    { "civilian", isCivilian.ToString() }
                };

                string argumentDisplay = parsed.FormatArgumentDisplay("gm.item.equip_slot", resolvedValues);
                return argumentDisplay + MessageFormatter.FormatSuccessMessage(
                    $"Equipped {item.Name} to {hero.Name}'s {equipmentType} equipment slot {slot}.");
            });
        }
    }
}
