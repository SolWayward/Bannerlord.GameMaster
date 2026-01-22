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
/// Command to set prosperity for a city or castle.
/// Usage: gm.settlement.set_prosperity [settlement] [value]
/// </summary>
public static class SetProsperityCommand
{
    [CommandLineFunctionality.CommandLineArgumentFunction("set_prosperity", "gm.settlement")]
    public static string SetProperity(List<string> args)
    {
        return Cmd.Run(args, () =>
        {
            // MARK: Validation
            if (!CommandValidator.ValidateCampaignState(out string error))
                return CommandResult.Error(error).Log().Message;

            string usageMessage = CommandValidator.CreateUsageMessage(
                "gm.settlement.set_prosperity", "<settlement> <value>",
                "Sets the prosperity of a city or castle.",
                "gm.settlement.set_prosperity pen 5000");

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

            if (settlement.Town == null)
                return CommandResult.Error(MessageFormatter.FormatErrorMessage($"Settlement '{settlement.Name}' is not a city or castle.")).Log().Message;

            if (!CommandValidator.ValidateFloatRange(valueStr, 0, 20000, out float value, out string valueError))
                return CommandResult.Error(MessageFormatter.FormatErrorMessage(valueError)).Log().Message;

            // MARK: Execute Logic
            float previousValue = settlement.Town.Prosperity;
            settlement.Town.Prosperity = value;

            Dictionary<string, string> resolvedValues = new()
            {
                ["settlement"] = settlement.Name.ToString(),
                ["value"] = value.ToString("F0")
            };

            string argumentDisplay = parsed.FormatArgumentDisplay("gm.settlement.set_prosperity", resolvedValues);
            string fullMessage = argumentDisplay + MessageFormatter.FormatSuccessMessage(
                $"Settlement '{settlement.Name}' (ID: {settlement.StringId}) prosperity changed from {previousValue:F0} to {settlement.Town.Prosperity:F0}.");
            return CommandResult.Success(fullMessage).Log().Message;
        });
    }
}
