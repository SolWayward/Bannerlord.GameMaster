using Bannerlord.GameMaster.Console.Common;
using Bannerlord.GameMaster.Console.Common.EntityFinding;
using Bannerlord.GameMaster.Console.Common.Execution;
using Bannerlord.GameMaster.Console.Common.Formatting;
using Bannerlord.GameMaster.Console.Common.Parsing;
using Bannerlord.GameMaster.Console.Common.Validation;
using Bannerlord.GameMaster.Items;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Library;

namespace Bannerlord.GameMaster.Console.ItemCommands.EquipmentManagementCommands
{
    /// <summary>
    /// Console command to change modifier on all equipped items for a hero (battle and civilian)
    /// </summary>
    public static class SetEquippedModifierCommand
    {
        /// <summary>
        /// Change modifier on all equipped items for a hero (battle and civilian)
        /// Usage: gm.item.set_equipped_modifier [hero_query] [modifier]
        /// </summary>
        [CommandLineFunctionality.CommandLineArgumentFunction("set_equipped_modifier", "gm.item")]
        public static string Execute(List<string> args)
        {
            return Cmd.Run(args, () =>
            {
                // MARK: Validation
                if (!CommandValidator.ValidateCampaignState(out string error))
                    return error;

                string usageMessage = CommandValidator.CreateUsageMessage(
                    "gm.item.set_equipped_modifier", "<hero_query> <modifier>",
                    "Sets modifier on all equipped items for a hero (battle and civilian equipment).",
                    "gm.item.set_equipped_modifier player masterwork\n" +
                    "gm.item.set_equipped_modifier lord_1_1 fine");

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
                EntityFinderResult<Hero> heroResult = HeroFinder.FindSingleHero(heroQuery);
                if (!heroResult.IsSuccess) return heroResult.Message;
                Hero hero = heroResult.Entity;

                string modifierName = string.Join(" ", args.Skip(1));
                (ItemModifier modifier, string modError) = ItemModifierHelper.ParseModifier(modifierName);

                if (modError != null)
                    return MessageFormatter.FormatErrorMessage(modError);

                if (modifier == null)
                    return MessageFormatter.FormatErrorMessage($"Modifier '{modifierName}' not found.");

                // MARK: Execute Logic
                int itemsChanged = 0;
                List<string> changedItems = new();

                // Process battle equipment
                for (int i = 0; i < (int)EquipmentIndex.NumEquipmentSetSlots; i++)
                {
                    EquipmentIndex slot = (EquipmentIndex)i;
                    EquipmentElement element = hero.BattleEquipment[slot];
                    if (!element.IsEmpty && ItemModifierHelper.CanHaveModifier(element.Item))
                    {
                        hero.BattleEquipment[slot] = new EquipmentElement(element.Item, modifier);
                        changedItems.Add($"{element.Item.Name} (battle:{slot})");
                        itemsChanged++;
                    }
                }

                // Process civilian equipment
                for (int i = 0; i < (int)EquipmentIndex.NumEquipmentSetSlots; i++)
                {
                    EquipmentIndex slot = (EquipmentIndex)i;
                    EquipmentElement element = hero.CivilianEquipment[slot];
                    if (!element.IsEmpty && ItemModifierHelper.CanHaveModifier(element.Item))
                    {
                        hero.CivilianEquipment[slot] = new EquipmentElement(element.Item, modifier);
                        changedItems.Add($"{element.Item.Name} (civilian:{slot})");
                        itemsChanged++;
                    }
                }

                if (itemsChanged == 0)
                    return MessageFormatter.FormatSuccessMessage($"{hero.Name} has no equipped items that can have modifiers.");

                Dictionary<string, string> resolvedValues = new()
                {
                    { "hero", hero.Name.ToString() },
                    { "modifier", modifier.Name.ToString() }
                };

                StringBuilder result = new();
                result.AppendLine(parsed.FormatArgumentDisplay("gm.item.set_equipped_modifier", resolvedValues));
                result.AppendLine(MessageFormatter.FormatSuccessMessage($"Set modifier '{modifier.Name}' on {itemsChanged} equipped items for {hero.Name}:"));
                foreach (string item in changedItems)
                {
                    result.AppendLine($"  - {item}");
                }

                return CommandResult.Success(result.ToString()).Log().Message;
            });
        }
    }
}
