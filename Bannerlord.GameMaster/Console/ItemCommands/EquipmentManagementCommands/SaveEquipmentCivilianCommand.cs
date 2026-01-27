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
    /// Console command to save hero's civilian equipment set to a JSON file
    /// </summary>
    public static class SaveEquipmentCivilianCommand
    {
        /// <summary>
        /// Save hero's civilian equipment set to a JSON file
        /// Usage: gm.item.save_equipment_civilian [hero_query] [filename]
        /// </summary>
        [CommandLineFunctionality.CommandLineArgumentFunction("save_equipment_civilian", "gm.item")]
        public static string Execute(List<string> args)
        {
            return Cmd.Run(args, () =>
            {
                // MARK: Validation
                if (!CommandValidator.ValidateCampaignState(out string error))
                    return error;

                string usageMessage = CommandValidator.CreateUsageMessage(
                    "gm.item.save_equipment_civilian", "<hero_query> <filename>",
                    "Saves the hero's civilian equipment set to a JSON file.",
                    "gm.item.save_equipment_civilian player my_civilian_loadout");

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
                string filepath = EquipmentFileManager.Default.GetEquipmentFilePath(filename, true);
                EquipmentFileManager.Default.SaveEquipmentToFile(hero, hero.CivilianEquipment, filepath, true);

                List<EquipmentItemInfo> savedItems = ItemCommandHelpers.GetEquipmentList(hero.CivilianEquipment);

                Dictionary<string, string> resolvedValues = new()
                {
                    { "hero", hero.Name.ToString() },
                    { "filename", Path.GetFileName(filepath) }
                };

                StringBuilder result = new();
                result.AppendLine(parsed.FormatArgumentDisplay("gm.item.save_equipment_civilian", resolvedValues));
                result.AppendLine(MessageFormatter.FormatSuccessMessage($"Saved {hero.Name}'s civilian equipment to: {Path.GetFileName(filepath)}"));
                result.AppendLine($"Items saved ({savedItems.Count}):");
                foreach (EquipmentItemInfo item in savedItems)
                {
                    result.AppendLine($"  {item.Slot,-15} {item.ItemName}{item.ModifierText}");
                }

                return CommandResult.Success(result.ToString()).Message;
            });
        }
    }
}
