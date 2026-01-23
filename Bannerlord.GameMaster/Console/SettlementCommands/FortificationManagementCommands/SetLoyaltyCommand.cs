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
/// Command to set loyalty for a city or castle.
/// Usage: gm.settlement.set_loyalty [settlement] [value]
/// </summary>
public static class SetLoyaltyCommand
{
    [CommandLineFunctionality.CommandLineArgumentFunction("set_loyalty", "gm.settlement")]
    public static string SetLoyalty(List<string> args)
    {
        return Cmd.Run(args, () =>
        {
            // MARK: Validation
            if (!CommandValidator.ValidateCampaignState(out string error))
                return CommandResult.Error(error).Message
;

            string usageMessage = CommandValidator.CreateUsageMessage(
                "gm.settlement.set_loyalty", "<settlement> <value>",
                "Sets the loyalty of a city or castle (0-100).",
                "gm.settlement.set_loyalty pen 100");

            ParsedArguments parsed = ArgumentParser.ParseArguments(args);
            parsed.SetValidArguments(
                new ArgumentDefinition("settlement", true),
                new ArgumentDefinition("value", true)
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
            string valueStr = parsed.GetArgument("value", 1);

            EntityFinderResult<Settlement> settlementResult = SettlementFinder.FindSingleSettlement(settlementQuery);
            if (!settlementResult.IsSuccess) return CommandResult.Error(settlementResult.Message).Message
;
            Settlement settlement = settlementResult.Entity;

            if (settlement.Town == null)
                return CommandResult.Error(MessageFormatter.FormatErrorMessage($"Settlement '{settlement.Name}' is not a city or castle.")).Message
;

            if (!CommandValidator.ValidateFloatRange(valueStr, 0, 100, out float value, out string valueError))
                return CommandResult.Error(MessageFormatter.FormatErrorMessage(valueError)).Message
;

            // MARK: Execute Logic
            float previousValue = settlement.Town.Loyalty;
            settlement.Town.Loyalty = value;

            Dictionary<string, string> resolvedValues = new()
            {
                ["settlement"] = settlement.Name.ToString(),
                ["value"] = value.ToString("F1")
            };

            string argumentDisplay = parsed.FormatArgumentDisplay("gm.settlement.set_loyalty", resolvedValues);
            string fullMessage = argumentDisplay + MessageFormatter.FormatSuccessMessage(
                $"Settlement '{settlement.Name}' (ID: {settlement.StringId}) loyalty changed from {previousValue:F1} to {settlement.Town.Loyalty:F1}.");
            return CommandResult.Success(fullMessage).Message
;
        });
    }
}
