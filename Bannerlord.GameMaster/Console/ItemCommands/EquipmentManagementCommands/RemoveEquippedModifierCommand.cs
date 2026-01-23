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
    /// Console command to remove modifiers from all equipped items for a hero
    /// </summary>
    public static class RemoveEquippedModifierCommand
    {
        /// <summary>
        /// Remove modifiers from all equipped items for a hero
        /// Usage: gm.item.remove_equipped_modifier [hero_query]
        /// </summary>
        [CommandLineFunctionality.CommandLineArgumentFunction("remove_equipped_modifier", "gm.item")]
        public static string Execute(List<string> args)
        {
            return Cmd.Run(args, () =>
            {
                // MARK: Validation
                if (!CommandValidator.ValidateCampaignState(out string error))
                    return error;

                string usageMessage = CommandValidator.CreateUsageMessage(
                    "gm.item.remove_equipped_modifier", "<hero_query>",
                    "Removes modifiers from all equipped items for a hero.",
                    "gm.item.remove_equipped_modifier player");

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
                int itemsChanged = 0;

                // Process battle equipment
                for (int i = 0; i < (int)EquipmentIndex.NumEquipmentSetSlots; i++)
                {
                    EquipmentIndex slot = (EquipmentIndex)i;
                    EquipmentElement element = hero.BattleEquipment[slot];
                    if (!element.IsEmpty && element.ItemModifier != null)
                    {
                        hero.BattleEquipment[slot] = new EquipmentElement(element.Item);
                        itemsChanged++;
                    }
                }

                // Process civilian equipment
                for (int i = 0; i < (int)EquipmentIndex.NumEquipmentSetSlots; i++)
                {
                    EquipmentIndex slot = (EquipmentIndex)i;
                    EquipmentElement element = hero.CivilianEquipment[slot];
                    if (!element.IsEmpty && element.ItemModifier != null)
                    {
                        hero.CivilianEquipment[slot] = new EquipmentElement(element.Item);
                        itemsChanged++;
                    }
                }

                if (itemsChanged == 0)
                    return MessageFormatter.FormatSuccessMessage($"{hero.Name} has no equipped items with modifiers.");

                Dictionary<string, string> resolvedValues = new()
                {
                    { "hero", hero.Name.ToString() }
                };

                string argumentDisplay = parsed.FormatArgumentDisplay("gm.item.remove_equipped_modifier", resolvedValues);
                return CommandResult.Success(argumentDisplay + MessageFormatter.FormatSuccessMessage(
                    $"Removed modifiers from {itemsChanged} equipped items for {hero.Name}.")).Message;
            });
        }
    }
}
