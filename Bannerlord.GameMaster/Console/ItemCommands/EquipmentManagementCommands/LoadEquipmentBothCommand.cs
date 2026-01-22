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
    /// Console command to load both battle and civilian equipment sets from JSON files
    /// </summary>
    public static class LoadEquipmentBothCommand
    {
        /// <summary>
        /// Load both battle and civilian equipment sets from JSON files
        /// Usage: gm.item.load_equipment_both [hero_query] [filename]
        /// </summary>
        [CommandLineFunctionality.CommandLineArgumentFunction("load_equipment_both", "gm.item")]
        public static string Execute(List<string> args)
        {
            return Cmd.Run(args, () =>
            {
                // MARK: Validation
                if (!CommandValidator.ValidateCampaignState(out string error))
                    return error;

                string usageMessage = CommandValidator.CreateUsageMessage(
                    "gm.item.load_equipment_both", "<hero_query> <filename>",
                    "Loads both battle and civilian equipment sets from JSON files (handles missing files gracefully).",
                    "gm.item.load_equipment_both player my_complete_loadout");

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
                Dictionary<string, string> resolvedValues = new()
                {
                    { "hero", hero.Name.ToString() },
                    { "filename", filename }
                };

                StringBuilder result = new();
                result.AppendLine(parsed.FormatArgumentDisplay("gm.item.load_equipment_both", resolvedValues));
                result.AppendLine($"Loading equipment sets for {hero.Name}:");

                // Track overall results
                bool battleLoaded = false;
                bool civilianLoaded = false;
                int battleLoadedCount = 0;
                int battleSkippedCount = 0;
                List<SkippedItemInfo> battleSkippedItems = new();
                int civilianLoadedCount = 0;
                int civilianSkippedCount = 0;
                List<SkippedItemInfo> civilianSkippedItems = new();

                // Try to load battle equipment
                string battlePath = EquipmentFileManager.GetEquipmentFilePath(filename, false);
                if (EquipmentFileManager.EquipmentFileExists(filename, false))
                {
                    (battleLoadedCount, battleSkippedCount, battleSkippedItems) = 
                        EquipmentFileManager.LoadEquipmentFromFile(hero, battlePath, false);
                    battleLoaded = true;

                    result.AppendLine($"\nBattle equipment loaded from: {Path.GetFileName(battlePath)}");
                    result.AppendLine($"Items loaded ({battleLoadedCount}):");

                    List<EquipmentItemInfo> battleItems = ItemCommandHelpers.GetEquipmentList(hero.BattleEquipment);
                    foreach (EquipmentItemInfo item in battleItems)
                    {
                        result.AppendLine($"  {item.Slot,-15} {item.ItemName}{item.ModifierText}");
                    }

                    if (battleSkippedCount > 0)
                    {
                        result.AppendLine($"Items skipped: {battleSkippedCount}");
                    }
                }
                else
                {
                    result.AppendLine($"\nBattle equipment file not found: {Path.GetFileName(battlePath)}");
                }

                // Try to load civilian equipment
                string civilianPath = EquipmentFileManager.GetEquipmentFilePath(filename, true);
                if (EquipmentFileManager.EquipmentFileExists(filename, true))
                {
                    (civilianLoadedCount, civilianSkippedCount, civilianSkippedItems) = 
                        EquipmentFileManager.LoadEquipmentFromFile(hero, civilianPath, true);
                    civilianLoaded = true;

                    result.AppendLine($"\nCivilian equipment loaded from: {Path.GetFileName(civilianPath)}");
                    result.AppendLine($"Items loaded ({civilianLoadedCount}):");

                    List<EquipmentItemInfo> civilianItems = ItemCommandHelpers.GetEquipmentList(hero.CivilianEquipment);
                    foreach (EquipmentItemInfo item in civilianItems)
                    {
                        result.AppendLine($"  {item.Slot,-15} {item.ItemName}{item.ModifierText}");
                    }

                    if (civilianSkippedCount > 0)
                    {
                        result.AppendLine($"Items skipped: {civilianSkippedCount}");
                    }
                }
                else
                {
                    result.AppendLine($"\nCivilian equipment file not found: {Path.GetFileName(civilianPath)}");
                }

                // Show all skipped items if any
                if (battleSkippedCount > 0 || civilianSkippedCount > 0)
                {
                    result.AppendLine("\nSkipped items (not found in current game):");
                    foreach (SkippedItemInfo item in battleSkippedItems)
                    {
                        result.AppendLine($"  [Battle] {item.Slot,-15} {item.ItemId} {item.ModifierInfo}");
                    }
                    foreach (SkippedItemInfo item in civilianSkippedItems)
                    {
                        result.AppendLine($"  [Civilian] {item.Slot,-15} {item.ItemId} {item.ModifierInfo}");
                    }
                }

                if (!battleLoaded && !civilianLoaded)
                {
                    return MessageFormatter.FormatErrorMessage("Neither battle nor civilian equipment files were found.");
                }

                return result.ToString();
            });
        }
    }
}
