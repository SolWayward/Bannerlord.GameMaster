using Bannerlord.GameMaster.Console.Common;
using Bannerlord.GameMaster.Console.Common.EntityFinding;
using Bannerlord.GameMaster.Console.Common.Execution;
using Bannerlord.GameMaster.Console.Common.Formatting;
using Bannerlord.GameMaster.Console.Common.Parsing;
using Bannerlord.GameMaster.Console.Common.Validation;
using Bannerlord.GameMaster.Items;
using System.Collections.Generic;
using System.IO;
using System.Text;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Library;

namespace Bannerlord.GameMaster.Console.ItemCommands.EquipmentManagementCommands
{
    /// <summary>
    /// Console command to save both battle and civilian equipment sets to JSON files
    /// </summary>
    public static class SaveEquipmentBothCommand
    {
        /// <summary>
        /// Save both battle and civilian equipment sets to JSON files
        /// Usage: gm.item.save_equipment_both [hero_query] [filename]
        /// </summary>
        [CommandLineFunctionality.CommandLineArgumentFunction("save_equipment_both", "gm.item")]
        public static string Execute(List<string> args)
        {
            return Cmd.Run(args, () =>
            {
                // MARK: Validation
                if (!CommandValidator.ValidateCampaignState(out string error))
                    return error;

                string usageMessage = CommandValidator.CreateUsageMessage(
                    "gm.item.save_equipment_both", "<hero_query> <filename>",
                    "Saves both battle and civilian equipment sets to JSON files.",
                    "gm.item.save_equipment_both player my_complete_loadout");

                ParsedArguments parsed = ArgumentParser.ParseArguments(args);

                parsed.SetValidArguments(
                    new ArgumentDefinition("hero", true),
                    new ArgumentDefinition("filename", true)
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

                string filename = parsed.GetArgument("filename", 1);
                if (string.IsNullOrWhiteSpace(filename))
                    return MessageFormatter.FormatErrorMessage("Filename cannot be empty.");

                // MARK: Execute Logic
                (string battlePath, string civilianPath) = EquipmentFileManager.SaveBothEquipmentSets(
                    hero, filename, hero.BattleEquipment, hero.CivilianEquipment);

                List<EquipmentItemInfo> battleItems = ItemCommandHelpers.GetEquipmentList(hero.BattleEquipment);
                List<EquipmentItemInfo> civilianItems = ItemCommandHelpers.GetEquipmentList(hero.CivilianEquipment);

                Dictionary<string, string> resolvedValues = new()
                {
                    { "hero", hero.Name.ToString() },
                    { "filename", filename }
                };

                StringBuilder result = new();
                result.AppendLine(parsed.FormatArgumentDisplay("gm.item.save_equipment_both", resolvedValues));
                result.AppendLine(MessageFormatter.FormatSuccessMessage($"Saved {hero.Name}'s equipment sets:"));
                
                result.AppendLine($"\nBattle equipment -> {Path.GetFileName(battlePath)} ({battleItems.Count} items):");
                foreach (EquipmentItemInfo item in battleItems)
                {
                    result.AppendLine($"  {item.Slot,-15} {item.ItemName}{item.ModifierText}");
                }
                
                result.AppendLine($"\nCivilian equipment -> {Path.GetFileName(civilianPath)} ({civilianItems.Count} items):");
                foreach (EquipmentItemInfo item in civilianItems)
                {
                    result.AppendLine($"  {item.Slot,-15} {item.ItemName}{item.ModifierText}");
                }

                return CommandResult.Success(result.ToString()).Log().Message;
            });
        }
    }
}
