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
    /// Console command to load hero's civilian equipment set from a JSON file
    /// </summary>
    public static class LoadEquipmentCivilianCommand
    {
        /// <summary>
        /// Load hero's civilian equipment set from a JSON file
        /// Usage: gm.item.load_equipment_civilian [hero_query] [filename]
        /// </summary>
        [CommandLineFunctionality.CommandLineArgumentFunction("load_equipment_civilian", "gm.item")]
        public static string Execute(List<string> args)
        {
            return Cmd.Run(args, () =>
            {
                // MARK: Validation
                if (!CommandValidator.ValidateCampaignState(out string error))
                    return error;

                string usageMessage = CommandValidator.CreateUsageMessage(
                    "gm.item.load_equipment_civilian", "<hero_query> <filename>",
                    "Loads the hero's civilian equipment set from a JSON file.",
                    "gm.item.load_equipment_civilian player my_civilian_loadout");

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
                string filepath = EquipmentFileManager.GetEquipmentFilePath(filename, true);

                if (!EquipmentFileManager.EquipmentFileExists(filename, true))
                    return MessageFormatter.FormatErrorMessage($"Civilian equipment file not found: {Path.GetFileName(filepath)}");

                (int loadedCount, int skippedCount, List<SkippedItemInfo> skippedItems) = 
                    EquipmentFileManager.LoadEquipmentFromFile(hero, filepath, true);

                List<EquipmentItemInfo> loadedItems = ItemCommandHelpers.GetEquipmentList(hero.CivilianEquipment);

                Dictionary<string, string> resolvedValues = new()
                {
                    { "hero", hero.Name.ToString() },
                    { "filename", Path.GetFileName(filepath) }
                };

                StringBuilder result = new();
                result.AppendLine(parsed.FormatArgumentDisplay("gm.item.load_equipment_civilian", resolvedValues));
                result.AppendLine(MessageFormatter.FormatSuccessMessage($"Loaded {hero.Name}'s civilian equipment from: {Path.GetFileName(filepath)}"));
                result.AppendLine($"Items loaded ({loadedCount}):");

                foreach (EquipmentItemInfo item in loadedItems)
                {
                    result.AppendLine($"  {item.Slot,-15} {item.ItemName}{item.ModifierText}");
                }

                if (skippedCount > 0)
                {
                    result.AppendLine($"\nItems skipped (not found in game): {skippedCount}");
                    foreach (SkippedItemInfo item in skippedItems)
                    {
                        result.AppendLine($"  {item.Slot,-15} {item.ItemId} {item.ModifierInfo}");
                    }
                }

                return CommandResult.Success(result.ToString()).Message;
            });
        }
    }
}
