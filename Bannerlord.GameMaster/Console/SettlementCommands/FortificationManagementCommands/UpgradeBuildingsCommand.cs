using System.Collections.Generic;
using Bannerlord.GameMaster.Console.Common;
using Bannerlord.GameMaster.Console.Common.Execution;
using Bannerlord.GameMaster.Console.Common.EntityFinding;
using Bannerlord.GameMaster.Console.Common.Formatting;
using Bannerlord.GameMaster.Console.Common.Parsing;
using Bannerlord.GameMaster.Console.Common.Validation;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Library;

namespace Bannerlord.GameMaster.Console.SettlementCommands.FortificationManagementCommands;

/// <summary>
/// Command to upgrade all buildings in a city or castle to a specified level.
/// Usage: gm.settlement.upgrade_buildings [settlement] [level]
/// </summary>
public static class UpgradeBuildingsCommand
{
    [CommandLineFunctionality.CommandLineArgumentFunction("upgrade_buildings", "gm.settlement")]
    public static string UpgradeBuildings(List<string> args)
    {
        return Cmd.Run(args, () =>
        {
            // MARK: Validation
            if (!CommandValidator.ValidateCampaignState(out string error))
                return CommandResult.Error(error).Message
;

            string usageMessage = CommandValidator.CreateUsageMessage(
                "gm.settlement.upgrade_buildings", "<settlement> <level>",
                "Upgrades all buildings in a city or castle to the specified level (0-3).",
                "gm.settlement.upgrade_buildings pen 3");

            ParsedArguments parsed = ArgumentParser.ParseArguments(args);
            parsed.SetValidArguments(
                new ArgumentDefinition("settlement", true),
                new ArgumentDefinition("level", true)
            );

            string validationError = parsed.GetValidationError();
            if (validationError != null)
                return CommandResult.Error(MessageFormatter.FormatErrorMessage(validationError)).Message
;

            if (parsed.TotalCount < 2)
                return CommandResult.Error(usageMessage).Message
;

            // MARK: Parse Arguments
            string settlementQuery = parsed.GetArgument("settlement", 0);
            string levelStr = parsed.GetArgument("level", 1);

            EntityFinderResult<Settlement> settlementResult = SettlementFinder.FindSingleSettlement(settlementQuery);
            if (!settlementResult.IsSuccess) return CommandResult.Error(settlementResult.Message).Message
;
            Settlement settlement = settlementResult.Entity;

            if (settlement.Town == null)
                return CommandResult.Error(MessageFormatter.FormatErrorMessage($"Settlement '{settlement.Name}' is not a city or castle.")).Message
;

            if (!CommandValidator.ValidateIntegerRange(levelStr, 0, 3, out int targetLevel, out string levelError))
                return CommandResult.Error(MessageFormatter.FormatErrorMessage(levelError)).Message
;

            // MARK: Execute Logic
            int upgradedCount = 0;
            int skippedCount = 0;

            foreach (TaleWorlds.CampaignSystem.Settlements.Buildings.Building building in settlement.Town.Buildings)
            {
                if (building.CurrentLevel < targetLevel)
                {
                    building.CurrentLevel = targetLevel;
                    upgradedCount++;
                }
                else
                {
                    skippedCount++;
                }
            }

            Dictionary<string, string> resolvedValues = new()
            {
                ["settlement"] = settlement.Name.ToString(),
                ["level"] = targetLevel.ToString()
            };

            string argumentDisplay = parsed.FormatArgumentDisplay("gm.settlement.upgrade_buildings", resolvedValues);

            if (upgradedCount == 0)
                return CommandResult.Success(argumentDisplay + MessageFormatter.FormatSuccessMessage($"All buildings in '{settlement.Name}' are already at level {targetLevel} or higher.")).Message
;

            string fullMessage = argumentDisplay + MessageFormatter.FormatSuccessMessage(
                $"Settlement '{settlement.Name}' (ID: {settlement.StringId}): upgraded {upgradedCount} building(s) to level {targetLevel}, {skippedCount} already at or above target level.");
            return CommandResult.Success(fullMessage).Message
;
        });
    }
}
