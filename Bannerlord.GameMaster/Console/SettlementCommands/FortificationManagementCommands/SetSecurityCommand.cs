using System.Collections.Generic;
using Bannerlord.GameMaster.Console.Common.Execution;
using Bannerlord.GameMaster.Console.Common.EntityFinding;
using Bannerlord.GameMaster.Console.Common.Formatting;
using Bannerlord.GameMaster.Console.Common.Parsing;
using Bannerlord.GameMaster.Console.Common.Validation;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Library;

namespace Bannerlord.GameMaster.Console.SettlementCommands.FortificationManagementCommands;

/// <summary>
/// Command to set security for a city or castle.
/// Usage: gm.settlement.set_security [settlement] [value]
/// </summary>
public static class SetSecurityCommand
{
    [CommandLineFunctionality.CommandLineArgumentFunction("set_security", "gm.settlement")]
    public static string SetSecurity(List<string> args)
    {
        return Cmd.Run(args, () =>
        {
            // MARK: Validation
            if (!CommandValidator.ValidateCampaignState(out string error))
                return error;

            string usageMessage = CommandValidator.CreateUsageMessage(
                "gm.settlement.set_security", "<settlement> <value>",
                "Sets the security of a city or castle (0-100).",
                "gm.settlement.set_security pen 100");

            ParsedArguments parsed = ArgumentParser.ParseArguments(args);
            parsed.SetValidArguments(
                new ArgumentDefinition("settlement", true),
                new ArgumentDefinition("value", true)
            );

            string validationError = parsed.GetValidationError();
            if (validationError != null)
                return MessageFormatter.FormatErrorMessage(validationError);

            if (parsed.TotalCount < 2)
                return usageMessage;

            // MARK: Parse Arguments
            string settlementQuery = parsed.GetArgument("settlement", 0);
            string valueStr = parsed.GetArgument("value", 1);

            EntityFinderResult<Settlement> settlementResult = SettlementFinder.FindSingleSettlement(settlementQuery);
            if (!settlementResult.IsSuccess) return settlementResult.Message;
            Settlement settlement = settlementResult.Entity;

            if (settlement.Town == null)
                return MessageFormatter.FormatErrorMessage($"Settlement '{settlement.Name}' is not a city or castle.");

            if (!CommandValidator.ValidateFloatRange(valueStr, 0, 100, out float value, out string valueError))
                return MessageFormatter.FormatErrorMessage(valueError);

            // MARK: Execute Logic
            float previousValue = settlement.Town.Security;
            settlement.Town.Security = value;

            Dictionary<string, string> resolvedValues = new()
            {
                ["settlement"] = settlement.Name.ToString(),
                ["value"] = value.ToString("F1")
            };

            string argumentDisplay = parsed.FormatArgumentDisplay("set_security", resolvedValues);
            return argumentDisplay + MessageFormatter.FormatSuccessMessage(
                $"Settlement '{settlement.Name}' (ID: {settlement.StringId}) security changed from {previousValue:F1} to {settlement.Town.Security:F1}.");
        });
    }
}
