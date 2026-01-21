using System.Collections.Generic;
using Bannerlord.GameMaster.Console.Common;
using Bannerlord.GameMaster.Console.Common.EntityFinding;
using Bannerlord.GameMaster.Console.Common.Execution;
using Bannerlord.GameMaster.Console.Common.Formatting;
using Bannerlord.GameMaster.Console.Common.Parsing;
using Bannerlord.GameMaster.Console.Common.Validation;
using Bannerlord.GameMaster.Kingdoms;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Library;

namespace Bannerlord.GameMaster.Console.KingdomCommands.KingdomDiplomacyCommands;

public static class PayTributeCommand
{
    [CommandLineFunctionality.CommandLineArgumentFunction("pay_tribute", "gm.kingdom")]
    public static string PayTribute(List<string> args)
    {
        return Cmd.Run(args, () =>
        {
            // MARK: Validation
            if (!CommandValidator.ValidateCampaignState(out string error))
                return error;

            string usageMessage = CommandValidator.CreateUsageMessage(
                "gm.kingdom.pay_tribute", "<payingKingdom> <receivingKingdom> <dailyAmount> <days>",
                "Makes one kingdom pay tribute to another kingdom.\n" +
                "dailyAmount: Amount of gold paid per day\n" +
                "days: Number of days the tribute will be paid\n" +
                "Supports named arguments: payingKingdom:battania receivingKingdom:empire dailyAmount:100 days:30",
                "gm.kingdom.pay_tribute battania empire 100 30");

            ParsedArguments parsed = ArgumentParser.ParseArguments(args);

            parsed.SetValidArguments(
                new ArgumentDefinition("payingKingdom", true),
                new ArgumentDefinition("receivingKingdom", true),
                new ArgumentDefinition("dailyAmount", true),
                new ArgumentDefinition("days", true)
            );

            string validationError = parsed.GetValidationError();
            if (validationError != null)
                return CommandResult.Error(MessageFormatter.FormatErrorMessage(validationError)).Log().Message;

            if (parsed.TotalCount < 4)
                return usageMessage;

            // MARK: Parse Arguments
            string payingKingdomArg = parsed.GetArgument("payingKingdom", 0);
            if (payingKingdomArg == null)
                return MessageFormatter.FormatErrorMessage("Missing required argument 'payingKingdom'.");

            EntityFinderResult<Kingdom> payingResult = KingdomFinder.FindSingleKingdom(payingKingdomArg);
            if (!payingResult.IsSuccess) return payingResult.Message;
            Kingdom payingKingdom = payingResult.Entity;

            string receivingKingdomArg = parsed.GetArgument("receivingKingdom", 1);
            if (receivingKingdomArg == null)
                return MessageFormatter.FormatErrorMessage("Missing required argument 'receivingKingdom'.");

            EntityFinderResult<Kingdom> receivingResult = KingdomFinder.FindSingleKingdom(receivingKingdomArg);
            if (!receivingResult.IsSuccess) return receivingResult.Message;
            Kingdom receivingKingdom = receivingResult.Entity;

            if (payingKingdom == receivingKingdom)
                return MessageFormatter.FormatErrorMessage("A kingdom cannot pay tribute to itself.");

            string dailyAmountArg = parsed.GetArgument("dailyAmount", 2);
            if (dailyAmountArg == null)
                return MessageFormatter.FormatErrorMessage("Missing required argument 'dailyAmount'.");

            if (!int.TryParse(dailyAmountArg, out int dailyAmount))
                return MessageFormatter.FormatErrorMessage($"Invalid value for 'dailyAmount': '{dailyAmountArg}'. Must be an integer.");

            if (dailyAmount < 0)
                return MessageFormatter.FormatErrorMessage("Daily amount cannot be negative.");

            string daysArg = parsed.GetArgument("days", 3);
            if (daysArg == null)
                return MessageFormatter.FormatErrorMessage("Missing required argument 'days'.");

            if (!int.TryParse(daysArg, out int days))
                return MessageFormatter.FormatErrorMessage($"Invalid value for 'days': '{daysArg}'. Must be an integer.");

            if (days <= 0)
                return MessageFormatter.FormatErrorMessage("Days must be greater than 0.");

            // MARK: Execute Logic
            Dictionary<string, string> resolvedValues = new()
            {
                { "payingKingdom", payingKingdom.Name.ToString() },
                { "receivingKingdom", receivingKingdom.Name.ToString() },
                { "dailyAmount", dailyAmount.ToString() },
                { "days", days.ToString() }
            };
            string argumentDisplay = parsed.FormatArgumentDisplay("pay_tribute", resolvedValues);

            TributeInfo tributeInfo = payingKingdom.PayTribute(receivingKingdom, dailyAmount, days);

            string message = $"{payingKingdom.Name} will pay {dailyAmount} gold per day to {receivingKingdom.Name} for {days} days.\n" +
                            $"Total tribute: {dailyAmount * days} gold";

            return argumentDisplay + MessageFormatter.FormatSuccessMessage(message);
        });
    }
}
