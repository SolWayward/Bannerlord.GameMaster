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

public static class TradeAgreementCommand
{
    [CommandLineFunctionality.CommandLineArgumentFunction("trade_agreement", "gm.kingdom")]
    public static string TradeAgreement(List<string> args)
    {
        return Cmd.Run(args, () =>
        {
            // MARK: Validation
            if (!CommandValidator.ValidateCampaignState(out string error))
                return error;

            string usageMessage = CommandValidator.CreateUsageMessage(
                "gm.kingdom.trade_agreement", "<proposingKingdom> <receivingKingdom>",
                "Creates a trade agreement between two kingdoms.\n" +
                "Supports named arguments: proposingKingdom:empire receivingKingdom:battania",
                "gm.kingdom.trade_agreement empire battania");

            ParsedArguments parsed = ArgumentParser.ParseArguments(args);

            parsed.SetValidArguments(
                new ArgumentDefinition("proposingKingdom", true),
                new ArgumentDefinition("receivingKingdom", true)
            );

            string validationError = parsed.GetValidationError();
            if (validationError != null)
                return CommandResult.Error(MessageFormatter.FormatErrorMessage(validationError)).Log().Message;

            if (parsed.TotalCount < 2)
                return usageMessage;

            // MARK: Parse Arguments
            string proposingKingdomArg = parsed.GetArgument("proposingKingdom", 0);
            if (proposingKingdomArg == null)
                return MessageFormatter.FormatErrorMessage("Missing required argument 'proposingKingdom'.");

            EntityFinderResult<Kingdom> proposingResult = KingdomFinder.FindSingleKingdom(proposingKingdomArg);
            if (!proposingResult.IsSuccess) return proposingResult.Message;
            Kingdom proposingKingdom = proposingResult.Entity;

            string receivingKingdomArg = parsed.GetArgument("receivingKingdom", 1);
            if (receivingKingdomArg == null)
                return MessageFormatter.FormatErrorMessage("Missing required argument 'receivingKingdom'.");

            EntityFinderResult<Kingdom> receivingResult = KingdomFinder.FindSingleKingdom(receivingKingdomArg);
            if (!receivingResult.IsSuccess) return receivingResult.Message;
            Kingdom receivingKingdom = receivingResult.Entity;

            if (proposingKingdom == receivingKingdom)
                return MessageFormatter.FormatErrorMessage("A kingdom cannot make a trade agreement with itself.");

            if (FactionManager.IsAtWarAgainstFaction(proposingKingdom, receivingKingdom))
                return MessageFormatter.FormatErrorMessage($"{proposingKingdom.Name} and {receivingKingdom.Name} are at war. Make peace first.");

            // MARK: Execute Logic
            Dictionary<string, string> resolvedValues = new()
            {
                { "proposingKingdom", proposingKingdom.Name.ToString() },
                { "receivingKingdom", receivingKingdom.Name.ToString() }
            };
            string argumentDisplay = parsed.FormatArgumentDisplay("trade_agreement", resolvedValues);

            proposingKingdom.MakeTradeAgreement(receivingKingdom);
            return argumentDisplay + MessageFormatter.FormatSuccessMessage(
                $"Trade agreement established between {proposingKingdom.Name} and {receivingKingdom.Name}.");
        });
    }
}
