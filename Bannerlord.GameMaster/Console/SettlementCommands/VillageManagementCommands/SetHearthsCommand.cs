using System.Collections.Generic;
using Bannerlord.GameMaster.Console.Common;
using Bannerlord.GameMaster.Console.Common.Execution;
using Bannerlord.GameMaster.Console.Common.EntityFinding;
using Bannerlord.GameMaster.Console.Common.Formatting;
using Bannerlord.GameMaster.Console.Common.Parsing;
using Bannerlord.GameMaster.Console.Common.Validation;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Library;

namespace Bannerlord.GameMaster.Console.SettlementCommands.VillageManagementCommands;

/// <summary>
/// Command to set hearth value for a village.
/// Usage: gm.settlement.set_hearths [settlement] [value]
/// </summary>
public static class SetHearthsCommand
{
    [CommandLineFunctionality.CommandLineArgumentFunction("set_hearths", "gm.settlement")]
    public static string SetHearths(List<string> args)
    {
        return Cmd.Run(args, () =>
        {
            // MARK: Validation
            if (!CommandValidator.ValidateCampaignState(out string error))
                return CommandResult.Error(error).Log().Message;

            string usageMessage = CommandValidator.CreateUsageMessage(
                "gm.settlement.set_hearths", "<settlement> <value>",
                "Sets the hearth value of a village.",
                "gm.settlement.set_hearths village_1 500");

            ParsedArguments parsed = ArgumentParser.ParseArguments(args);
            parsed.SetValidArguments(
                new ArgumentDefinition("settlement", true),
                new ArgumentDefinition("value", true)
            );

            string validationError = parsed.GetValidationError();
            if (validationError != null)
                return CommandResult.Error(MessageFormatter.FormatErrorMessage(validationError)).Log().Message;

            if (parsed.TotalCount < 2)
                return CommandResult.Error(usageMessage).Log().Message;

            // MARK: Parse Arguments
            string settlementQuery = parsed.GetArgument("settlement", 0);
            string valueStr = parsed.GetArgument("value", 1);

            EntityFinderResult<Settlement> settlementResult = SettlementFinder.FindSingleSettlement(settlementQuery);
            if (!settlementResult.IsSuccess) return CommandResult.Error(settlementResult.Message).Log().Message;
            Settlement settlement = settlementResult.Entity;

            if (settlement.Village == null)
                return CommandResult.Error(MessageFormatter.FormatErrorMessage($"Settlement '{settlement.Name}' is not a village.")).Log().Message;

            if (!CommandValidator.ValidateFloatRange(valueStr, 0, 2000, out float value, out string valueError))
                return CommandResult.Error(MessageFormatter.FormatErrorMessage(valueError)).Log().Message;

            // MARK: Execute Logic
            float previousValue = settlement.Village.Hearth;
            settlement.Village.Hearth = value;

            Dictionary<string, string> resolvedValues = new()
            {
                ["settlement"] = settlement.Name.ToString(),
                ["value"] = value.ToString("F0")
            };

            string argumentDisplay = parsed.FormatArgumentDisplay("set_hearths", resolvedValues);
            string fullMessage = argumentDisplay + MessageFormatter.FormatSuccessMessage(
                $"Village '{settlement.Name}' (ID: {settlement.StringId}) hearth changed from {previousValue:F0} to {settlement.Village.Hearth:F0}.");
            return CommandResult.Success(fullMessage).Log().Message;
        });
    }
}
