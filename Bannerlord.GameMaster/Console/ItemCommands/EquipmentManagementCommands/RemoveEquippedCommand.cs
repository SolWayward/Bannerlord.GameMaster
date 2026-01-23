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
    /// Console command to remove all equipped items from a hero (items are deleted, not moved to inventory)
    /// </summary>
    public static class RemoveEquippedCommand
    {
        /// <summary>
        /// Remove all equipped items from a hero (both battle and civilian equipment) - items are deleted
        /// Usage: gm.item.remove_equipped [hero_query]
        /// </summary>
        [CommandLineFunctionality.CommandLineArgumentFunction("remove_equipped", "gm.item")]
        public static string Execute(List<string> args)
        {
            return Cmd.Run(args, () =>
            {
                // MARK: Validation
                if (!CommandValidator.ValidateCampaignState(out string error))
                    return error;

                string usageMessage = CommandValidator.CreateUsageMessage(
                    "gm.item.remove_equipped", "<hero_query>",
                    "Removes all equipped items from a hero (battle and civilian equipment). Items are deleted, not moved to inventory.",
                    "gm.item.remove_equipped player");

                ParsedArguments parsed = ArgumentParser.ParseArguments(args);

                parsed.SetValidArguments(
                    new ArgumentDefinition("hero", true)
                );

                string validationError = parsed.GetValidationError();
                if (validationError != null)
                    return CommandResult.Error(MessageFormatter.FormatErrorMessage(validationError)).Message;

                if (parsed.TotalCount < 1)
                    return usageMessage;

                // MARK: Parse Arguments
                string heroQuery = parsed.GetArgument("hero", 0);

                EntityFinderResult<Hero> heroResult = HeroFinder.FindSingleHero(heroQuery);
                if (!heroResult.IsSuccess) return heroResult.Message;
                Hero hero = heroResult.Entity;

                // MARK: Execute Logic
                int itemsRemoved = 0;

                // Count items before clearing
                for (int i = 0; i < (int)EquipmentIndex.NumEquipmentSetSlots; i++)
                {
                    EquipmentIndex slot = (EquipmentIndex)i;
                    if (!hero.BattleEquipment[slot].IsEmpty) itemsRemoved++;
                    if (!hero.CivilianEquipment[slot].IsEmpty) itemsRemoved++;
                }

                Equipment battleEquipment = new();
                Equipment civilianEquipment = new();

                hero.BattleEquipment.FillFrom(battleEquipment);
                hero.CivilianEquipment.FillFrom(civilianEquipment);

                if (itemsRemoved == 0)
                    return MessageFormatter.FormatSuccessMessage($"{hero.Name} has no items equipped.");

                Dictionary<string, string> resolvedValues = new()
                {
                    { "hero", hero.Name.ToString() }
                };

                string argumentDisplay = parsed.FormatArgumentDisplay("gm.item.remove_equipped", resolvedValues);
                return CommandResult.Success(argumentDisplay + MessageFormatter.FormatSuccessMessage(
                    $"Removed {itemsRemoved} equipped items from {hero.Name} (battle and civilian equipment cleared).")).Message;
            });
        }
    }
}
