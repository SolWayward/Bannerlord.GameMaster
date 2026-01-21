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
/// Command to change which settlement (town/castle) a village is bound to.
/// Usage: gm.settlement.set_village_bound_settlement [village] [settlement]
/// </summary>
public static class SetVillageBoundSettlementCommand
{
    [CommandLineFunctionality.CommandLineArgumentFunction("set_village_bound_settlement", "gm.settlement")]
    public static string SetVillageBoundSettlement(List<string> args)
    {
        return Cmd.Run(args, () =>
        {
            // MARK: Validation
            if (!CommandValidator.ValidateCampaignState(out string error))
                return CommandResult.Error(error).Log().Message;

            string usageMessage = CommandValidator.CreateUsageMessage(
                "gm.settlement.set_village_bound_settlement", "<village> <settlement>",
                "Changes which settlement (town or castle) a village is bound to.\n" +
                "- village: Required. Village settlement name or ID (must have Village component)\n" +
                "- settlement: Required. Town or castle settlement name or ID to bind the village to\n" +
                "If bound to a town, the village will also be trade-bound to that town.\n" +
                "If bound to a castle, the command will attempt to automatically set a trade-bound town.\n" +
                "Changes persist through save/load cycles.\n" +
                "Supports named arguments: village:village_name settlement:town_name",
                "gm.settlement.set_village_bound_settlement village_e1 town_e1\n" +
                "gm.settlement.set_village_bound_settlement 'Village Name' 'Castle Name'\n" +
                "Important Note: It is not reccomended bind or trade bind a village to a settlment far away from the village. This will cause villagers and etc to travel way too far.");

            ParsedArguments parsed = ArgumentParser.ParseArguments(args);
            parsed.SetValidArguments(
                new ArgumentDefinition("village", true),
                new ArgumentDefinition("settlement", true)
            );

            string validationError = parsed.GetValidationError();
            if (validationError != null)
                return CommandResult.Error(MessageFormatter.FormatErrorMessage(validationError)).Log().Message;

            if (parsed.TotalCount < 2)
                return CommandResult.Error(usageMessage).Log().Message;

            // MARK: Parse Arguments
            string villageQuery = parsed.GetArgument("village", 0);
            string settlementQuery = parsed.GetArgument("settlement", 1);

            EntityFinderResult<Settlement> villageResult = SettlementFinder.FindSingleSettlement(villageQuery);
            if (!villageResult.IsSuccess) return CommandResult.Error(villageResult.Message).Log().Message;
            Settlement villageSettlement = villageResult.Entity;

            if (villageSettlement.Village == null)
                return CommandResult.Error(MessageFormatter.FormatErrorMessage($"Settlement '{villageSettlement.Name}' is not a village.")).Log().Message;

            EntityFinderResult<Settlement> settlementResult = SettlementFinder.FindSingleSettlement(settlementQuery);
            if (!settlementResult.IsSuccess) return CommandResult.Error(settlementResult.Message).Log().Message;
            Settlement newBoundSettlement = settlementResult.Entity;

            // MARK: Execute Logic
            Village village = villageSettlement.Village;
            BLGMResult result = village.SetBoundSettlement(newBoundSettlement);

            Dictionary<string, string> resolvedValues = new()
            {
                ["village"] = villageSettlement.Name.ToString(),
                ["settlement"] = newBoundSettlement.Name.ToString()
            };

            string argumentDisplay = parsed.FormatArgumentDisplay("set_village_bound_settlement", resolvedValues);

            if (result.IsSuccess)
                return CommandResult.Success(argumentDisplay + MessageFormatter.FormatSuccessMessage(result.Message)).Log().Message;
            else
                return CommandResult.Error(argumentDisplay + MessageFormatter.FormatErrorMessage(result.Message)).Log().Message;
        });
    }
}
