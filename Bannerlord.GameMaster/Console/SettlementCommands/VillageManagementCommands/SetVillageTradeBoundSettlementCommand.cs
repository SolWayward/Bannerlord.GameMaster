using System.Collections.Generic;
using Bannerlord.GameMaster.Common;
using Bannerlord.GameMaster.Console.Common;
using Bannerlord.GameMaster.Console.Common.Execution;
using Bannerlord.GameMaster.Console.Common.EntityFinding;
using Bannerlord.GameMaster.Console.Common.Formatting;
using Bannerlord.GameMaster.Console.Common.Parsing;
using Bannerlord.GameMaster.Console.Common.Validation;
using Bannerlord.GameMaster.Settlements;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Library;

namespace Bannerlord.GameMaster.Console.SettlementCommands.VillageManagementCommands;

/// <summary>
/// Command to change which town a village is trade-bound to.
/// Usage: gm.settlement.set_village_trade_bound_settlement [village] [settlement]
/// </summary>
public static class SetVillageTradeBoundSettlementCommand
{
    [CommandLineFunctionality.CommandLineArgumentFunction("set_village_trade_bound_settlement", "gm.settlement")]
    public static string SetVillageTradeBoundSettlement(List<string> args)
    {
        return Cmd.Run(args, () =>
        {
            // MARK: Validation
            if (!CommandValidator.ValidateCampaignState(out string error))
                return CommandResult.Error(error).Message;

            string usageMessage = CommandValidator.CreateUsageMessage(
                "gm.settlement.set_village_trade_bound_settlement", "<village> <settlement>",
                "Changes which town a village is trade-bound to.\n" +
                "- village: Required. Village settlement name or ID (must have Village component)\n" +
                "- settlement: Required. Town settlement name or ID (must be a town, not a castle)\n" +
                "Note: If the village is bound to a town, the game ignores the trade-bound settlement.\n" +
                "Trade-bound is primarily used when a village is bound to a castle (since castles lack markets).\n" +
                "Changes persist through save/load cycles.\n" +
                "Supports named arguments: village:village_name settlement:town_name",
                "gm.settlement.set_village_trade_bound_settlement village_e1 town_e2\n" +
                "gm.settlement.set_village_trade_bound_settlement 'Village Name' 'Town Name'\n\n" +
                "Important Note: It is not reccomended bind or trade bind a village to a settlment far away from the village. This will cause villagers and etc to travel way too far.");

            ParsedArguments parsed = ArgumentParser.ParseArguments(args);
            parsed.SetValidArguments(
                new ArgumentDefinition("village", true),
                new ArgumentDefinition("settlement", true)
            );

            string validationError = parsed.GetValidationError();
            if (validationError != null)
                return CommandResult.Error(MessageFormatter.FormatErrorMessage(validationError)).Message;

            if (parsed.TotalCount < 2)
                return CommandResult.Error(usageMessage).Message;

            // MARK: Parse Arguments
            string villageQuery = parsed.GetArgument("village", 0);
            string settlementQuery = parsed.GetArgument("settlement", 1);

            EntityFinderResult<Settlement> villageResult = SettlementFinder.FindSingleSettlement(villageQuery);
            if (!villageResult.IsSuccess) return CommandResult.Error(villageResult.Message).Message;
            Settlement villageSettlement = villageResult.Entity;

            if (villageSettlement.Village == null)
                return CommandResult.Error(MessageFormatter.FormatErrorMessage($"Settlement '{villageSettlement.Name}' is not a village.")).Message;

            EntityFinderResult<Settlement> settlementResult = SettlementFinder.FindSingleSettlement(settlementQuery);
            if (!settlementResult.IsSuccess) return CommandResult.Error(settlementResult.Message).Message;
            Settlement tradeBoundSettlement = settlementResult.Entity;

            // MARK: Execute Logic
            Village village = villageSettlement.Village;
            BLGMResult result = village.SetTradeBoundSettlement(tradeBoundSettlement);

            Dictionary<string, string> resolvedValues = new()
            {
                ["village"] = villageSettlement.Name.ToString(),
                ["settlement"] = tradeBoundSettlement.Name.ToString()
            };

            string argumentDisplay = parsed.FormatArgumentDisplay("gm.settlement.set_village_trade_bound_settlement", resolvedValues);

            if (result.IsSuccess)
                return CommandResult.Success(argumentDisplay + MessageFormatter.FormatSuccessMessage(result.Message)).Message;
            else
                return CommandResult.Error(argumentDisplay + MessageFormatter.FormatErrorMessage(result.Message)).Message;
        });
    }
}
