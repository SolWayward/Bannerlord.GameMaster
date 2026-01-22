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
    /// Console command to save hero's main/battle equipment set to a JSON file
    /// </summary>
    public static class SaveEquipmentCommand
    {
        /// <summary>
        /// Save hero's main/battle equipment set to a JSON file
        /// Usage: gm.item.save_equipment [hero_query] [filename]
        /// </summary>
        [CommandLineFunctionality.CommandLineArgumentFunction("save_equipment", "gm.item")]
        public static string Execute(List<string> args)
        {
            return Cmd.Run(args, () =>
            {
                // MARK: Validation
                if (!CommandValidator.ValidateCampaignState(out string error))
                    return error;

                string usageMessage = CommandValidator.CreateUsageMessage(
                    "gm.item.save_equipment", "<hero_query> <filename>",
                    "Saves the hero's main/battle equipment set to a JSON file.",
                    "gm.item.save_equipment player my_loadout");

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
                string filepath = EquipmentFileManager.GetEquipmentFilePath(filename, false);
                EquipmentFileManager.SaveEquipmentToFile(hero, hero.BattleEquipment, filepath, false);

                List<EquipmentItemInfo> savedItems = ItemCommandHelpers.GetEquipmentList(hero.BattleEquipment);

                Dictionary<string, string> resolvedValues = new()
                {
                    { "hero", hero.Name.ToString() },
                    { "filename", Path.GetFileName(filepath) }
                };

                StringBuilder result = new();
                result.AppendLine(parsed.FormatArgumentDisplay("gm.item.save_equipment", resolvedValues));
                result.AppendLine($"Saved {hero.Name}'s battle equipment to: {Path.GetFileName(filepath)}");
                result.AppendLine($"Items saved ({savedItems.Count}):");
                foreach (EquipmentItemInfo item in savedItems)
                {
                    result.AppendLine($"  {item.Slot,-15} {item.ItemName}{item.ModifierText}");
                }

                return result.ToString();
            });
        }
    }
}
