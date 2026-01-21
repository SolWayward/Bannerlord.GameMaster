using Bannerlord.GameMaster.Console.Common;
using Bannerlord.GameMaster.Console.Common.EntityFinding;
using Bannerlord.GameMaster.Console.Common.Execution;
using Bannerlord.GameMaster.Console.Common.Formatting;
using Bannerlord.GameMaster.Console.Common.Parsing;
using Bannerlord.GameMaster.Console.Common.Validation;
using System.Collections.Generic;
using System.Text;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Library;

namespace Bannerlord.GameMaster.Console.ItemCommands.EquipmentManagementCommands
{
    /// <summary>
    /// Console command to list all equipped items on a hero
    /// </summary>
    public static class ListEquippedCommand
    {
        /// <summary>
        /// List all equipped items on a hero
        /// Usage: gm.item.list_equipped [hero_query]
        /// </summary>
        [CommandLineFunctionality.CommandLineArgumentFunction("list_equipped", "gm.item")]
        public static string Execute(List<string> args)
        {
            return Cmd.Run(args, () =>
            {
                // MARK: Validation
                if (!CommandValidator.ValidateCampaignState(out string error))
                    return error;

                string usageMessage = CommandValidator.CreateUsageMessage(
                    "gm.item.list_equipped", "<hero_query>",
                    "Lists all equipped items on a hero (battle and civilian equipment).",
                    "gm.item.list_equipped player");

                ParsedArguments parsed = ArgumentParser.ParseArguments(args);

                parsed.SetValidArguments(
                    new ArgumentDefinition("hero", true)
                );

                string validationError = parsed.GetValidationError();
                if (validationError != null)
                    return MessageFormatter.FormatErrorMessage(validationError);

                if (parsed.TotalCount < 1)
                    return usageMessage;

                // MARK: Parse Arguments
                string heroQuery = parsed.GetArgument("hero", 0);

                EntityFinderResult<Hero> heroResult = HeroFinder.FindSingleHero(heroQuery);
                if (!heroResult.IsSuccess) return heroResult.Message;
                Hero hero = heroResult.Entity;

                // MARK: Execute Logic
                StringBuilder result = new();
                result.AppendLine($"Equipped items for {hero.Name}:\n");

                // Battle Equipment
                result.AppendLine("=== BATTLE EQUIPMENT ===");
                bool hasBattleItems = false;
                List<EquipmentItemInfo> battleItems = ItemCommandHelpers.GetEquipmentList(hero.BattleEquipment);
                
                if (battleItems.Count > 0)
                {
                    hasBattleItems = true;
                    foreach (EquipmentItemInfo item in battleItems)
                    {
                        result.AppendLine($"  {item.Slot,-15} {item.ItemName}{item.ModifierText}");
                    }
                }
                
                if (!hasBattleItems)
                    result.AppendLine("  (No battle equipment)");

                result.AppendLine();

                // Civilian Equipment
                result.AppendLine("=== CIVILIAN EQUIPMENT ===");
                bool hasCivilianItems = false;
                List<EquipmentItemInfo> civilianItems = ItemCommandHelpers.GetEquipmentList(hero.CivilianEquipment);
                
                if (civilianItems.Count > 0)
                {
                    hasCivilianItems = true;
                    foreach (EquipmentItemInfo item in civilianItems)
                    {
                        result.AppendLine($"  {item.Slot,-15} {item.ItemName}{item.ModifierText}");
                    }
                }
                
                if (!hasCivilianItems)
                    result.AppendLine("  (No civilian equipment)");

                return result.ToString();
            });
        }
    }
}
