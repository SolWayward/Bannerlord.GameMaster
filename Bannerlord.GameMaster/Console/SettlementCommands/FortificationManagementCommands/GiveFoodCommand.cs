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
/// Command to give food to a settlement.
/// Usage: gm.settlement.give_food [settlement] [amount]
/// </summary>
public static class GiveFoodCommand
{
    [CommandLineFunctionality.CommandLineArgumentFunction("give_food", "gm.settlement")]
    public static string GiveFood(List<string> args)
    {
        return Cmd.Run(args, () =>
        {
            // MARK: Validation
            if (!CommandValidator.ValidateCampaignState(out string error))
                return CommandResult.Error(error).Log().Message;

            string usageMessage = CommandValidator.CreateUsageMessage(
                "gm.settlement.give_food", "<settlement> <amount>",
                "Adds food to a settlement's food stock.",
                "gm.settlement.give_food pen 1000");

            ParsedArguments parsed = ArgumentParser.ParseArguments(args);
            parsed.SetValidArguments(
                new ArgumentDefinition("settlement", true),
                new ArgumentDefinition("amount", true)
            );

            string validationError = parsed.GetValidationError();
            if (validationError != null)
                return CommandResult.Error(MessageFormatter.FormatErrorMessage(validationError)).Log().Message;

            if (parsed.TotalCount < 2)
                return CommandResult.Error(usageMessage).Log().Message;

            // MARK: Parse Arguments
            string settlementQuery = parsed.GetArgument("settlement", 0);
            string amountStr = parsed.GetArgument("amount", 1);

            EntityFinderResult<Settlement> settlementResult = SettlementFinder.FindSingleSettlement(settlementQuery);
            if (!settlementResult.IsSuccess) return CommandResult.Error(settlementResult.Message).Log().Message;
            Settlement settlement = settlementResult.Entity;

            if (settlement.Town == null)
                return CommandResult.Error(MessageFormatter.FormatErrorMessage($"Settlement '{settlement.Name}' is not a city or castle.")).Log().Message;

            if (!CommandValidator.ValidateFloatRange(amountStr, -100000, 100000, out float amount, out string amountError))
                return CommandResult.Error(MessageFormatter.FormatErrorMessage(amountError)).Log().Message;

            // MARK: Execute Logic
            float previousValue = settlement.Town.FoodStocks;
            settlement.Town.FoodStocks += amount;

            Dictionary<string, string> resolvedValues = new()
            {
                ["settlement"] = settlement.Name.ToString(),
                ["amount"] = amount.ToString("F0")
            };

            string argumentDisplay = parsed.FormatArgumentDisplay("give_food", resolvedValues);
            string fullMessage = argumentDisplay + MessageFormatter.FormatSuccessMessage(
                $"Settlement '{settlement.Name}' (ID: {settlement.StringId}) food stocks changed from {previousValue:F0} to {settlement.Town.FoodStocks:F0} ({(amount >= 0 ? "+" : "")}{amount:F0}).");
            return CommandResult.Success(fullMessage).Log().Message;
        });
    }
}
