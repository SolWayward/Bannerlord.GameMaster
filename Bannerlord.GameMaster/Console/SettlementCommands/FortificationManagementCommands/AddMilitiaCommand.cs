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
/// Command to add militia to a city or castle.
/// Usage: gm.settlement.add_militia [settlement] [amount]
/// </summary>
public static class AddMilitiaCommand
{
    [CommandLineFunctionality.CommandLineArgumentFunction("add_militia", "gm.settlement")]
    public static string AddMilitia(List<string> args)
    {
        return Cmd.Run(args, () =>
        {
            // MARK: Validation
            if (!CommandValidator.ValidateCampaignState(out string error))
                return error;

            string usageMessage = CommandValidator.CreateUsageMessage(
                "gm.settlement.add_militia", "<settlement> <amount>",
                "Adds militia troops to a city or castle.",
                "gm.settlement.add_militia pen 100");

            ParsedArguments parsed = ArgumentParser.ParseArguments(args);
            parsed.SetValidArguments(
                new ArgumentDefinition("settlement", true),
                new ArgumentDefinition("amount", true)
            );

            string validationError = parsed.GetValidationError();
            if (validationError != null)
                return MessageFormatter.FormatErrorMessage(validationError);

            if (parsed.TotalCount < 2)
                return usageMessage;

            // MARK: Parse Arguments
            string settlementQuery = parsed.GetArgument("settlement", 0);
            string amountStr = parsed.GetArgument("amount", 1);

            EntityFinderResult<Settlement> settlementResult = SettlementFinder.FindSingleSettlement(settlementQuery);
            if (!settlementResult.IsSuccess) return settlementResult.Message;
            Settlement settlement = settlementResult.Entity;

            if (settlement.Town == null)
                return MessageFormatter.FormatErrorMessage($"Settlement '{settlement.Name}' is not a city or castle.");

            if (!CommandValidator.ValidateFloatRange(amountStr, 0, 1000, out float amount, out string amountError))
                return MessageFormatter.FormatErrorMessage(amountError);

            // MARK: Execute Logic
            float previousValue = settlement.Militia;
            settlement.Militia += amount;

            Dictionary<string, string> resolvedValues = new()
            {
                ["settlement"] = settlement.Name.ToString(),
                ["amount"] = amount.ToString("F0")
            };

            string argumentDisplay = parsed.FormatArgumentDisplay("add_militia", resolvedValues);
            return argumentDisplay + MessageFormatter.FormatSuccessMessage(
                $"Settlement '{settlement.Name}' (ID: {settlement.StringId}) militia changed from {previousValue:F0} to {settlement.Militia:F0} (+{amount:F0}).");
        });
    }
}
