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
    /// Console command to unequip an item from a specific equipment slot
    /// </summary>
    public static class UnequipSlotCommand
    {
        /// <summary>
        /// Unequip item from specific equipment slot
        /// Usage: gm.item.unequip_slot [hero_query] [slot] [civilian]
        /// </summary>
        [CommandLineFunctionality.CommandLineArgumentFunction("unequip_slot", "gm.item")]
        public static string Execute(List<string> args)
        {
            return Cmd.Run(args, () =>
            {
                // MARK: Validation
                if (!CommandValidator.ValidateCampaignState(out string error))
                    return error;

                string usageMessage = CommandValidator.CreateUsageMessage(
                    "gm.item.unequip_slot", "<hero_query> <slot> [civilian]",
                    "Unequips item from a specific equipment slot. Add 'civilian' for civilian equipment.\n" +
                    "Valid slots: Head, Body, Leg, Gloves, Cape, Horse, HorseHarness, Weapon0-3",
                    "gm.item.unequip_slot player Weapon0\n" +
                    "gm.item.unequip_slot player Body civilian");

                ParsedArguments parsed = ArgumentParser.ParseArguments(args);

                parsed.SetValidArguments(
                    new ArgumentDefinition("hero", true),
                    new ArgumentDefinition("slot", true),
                    new ArgumentDefinition("civilian", false)
                );

                string validationError = parsed.GetValidationError();
                if (validationError != null)
                    return MessageFormatter.FormatErrorMessage(validationError);

                if (parsed.TotalCount < 2)
                    return usageMessage;

                // MARK: Parse Arguments
                string heroQuery = parsed.GetArgument("hero", 0);
                string slotArg = parsed.GetArgument("slot", 1);

                EntityFinderResult<Hero> heroResult = HeroFinder.FindSingleHero(heroQuery);
                if (!heroResult.IsSuccess) return heroResult.Message;
                Hero hero = heroResult.Entity;

                if (!ItemCommandHelpers.TryParseEquipmentSlot(slotArg, out EquipmentIndex slot))
                    return MessageFormatter.FormatErrorMessage($"Invalid equipment slot: '{slotArg}'. Valid slots: Head, Body, Leg, Gloves, Cape, Horse, HorseHarness, Weapon0, Weapon1, Weapon2, Weapon3.");

                string civilianArg = parsed.GetArgument("civilian", 2);
                bool isCivilian = civilianArg != null && civilianArg.Equals("civilian", StringComparison.OrdinalIgnoreCase);

                // MARK: Execute Logic
                Equipment equipment = isCivilian ? hero.CivilianEquipment : hero.BattleEquipment;
                ItemObject previousItem = equipment[slot].Item;

                if (previousItem == null)
                {
                    string equipmentType = isCivilian ? "civilian" : "battle";
                    return MessageFormatter.FormatErrorMessage($"No item equipped in {hero.Name}'s {equipmentType} equipment slot {slot}.");
                }

                equipment[slot] = EquipmentElement.Invalid;

                string eqType = isCivilian ? "civilian" : "battle";

                Dictionary<string, string> resolvedValues = new()
                {
                    { "hero", hero.Name.ToString() },
                    { "slot", slot.ToString() },
                    { "civilian", isCivilian.ToString() }
                };

                string argumentDisplay = parsed.FormatArgumentDisplay("gm.item.unequip_slot", resolvedValues);
                return CommandResult.Success(argumentDisplay + MessageFormatter.FormatSuccessMessage(
                    $"Unequipped {previousItem.Name} from {hero.Name}'s {eqType} equipment slot {slot}.")).Message;
            });
        }
    }
}
