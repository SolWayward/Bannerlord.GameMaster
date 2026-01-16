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
/// Command to give gold to a settlement.
/// Usage: gm.settlement.give_gold [settlement] [amount]
/// </summary>
public static class GiveGoldCommand
{
    [CommandLineFunctionality.CommandLineArgumentFunction("give_gold", "gm.settlement")]
    public static string GiveGold(List<string> args)
    {
        return Cmd.Run(args, () =>
        {
            // MARK: Validation
            if (!CommandValidator.ValidateCampaignState(out string error))
                return error;

            string usageMessage = CommandValidator.CreateUsageMessage(
                "gm.settlement.give_gold", "<settlement> <amount>",
                "Adds gold to a settlement's treasury.",
                "gm.settlement.give_gold pen 10000");

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

            if (!CommandValidator.ValidateIntegerRange(amountStr, int.MinValue, int.MaxValue, out int amount, out string amountError))
                return MessageFormatter.FormatErrorMessage(amountError);

            // MARK: Execute Logic
            int previousValue = settlement.Town.Gold;
            settlement.Town.ChangeGold(amount);

            Dictionary<string, string> resolvedValues = new()
            {
                ["settlement"] = settlement.Name.ToString(),
                ["amount"] = amount.ToString()
            };

            string argumentDisplay = parsed.FormatArgumentDisplay("give_gold", resolvedValues);
            return argumentDisplay + MessageFormatter.FormatSuccessMessage(
                $"Settlement '{settlement.Name}' (ID: {settlement.StringId}) gold changed from {previousValue} to {settlement.Town.Gold} ({(amount >= 0 ? "+" : "")}{amount}).");
        });
    }
}
