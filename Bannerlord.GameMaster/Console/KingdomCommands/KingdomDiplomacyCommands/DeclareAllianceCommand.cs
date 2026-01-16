using System.Collections.Generic;
using Bannerlord.GameMaster.Console.Common.EntityFinding;
using Bannerlord.GameMaster.Console.Common.Execution;
using Bannerlord.GameMaster.Console.Common.Formatting;
using Bannerlord.GameMaster.Console.Common.Parsing;
using Bannerlord.GameMaster.Console.Common.Validation;
using Bannerlord.GameMaster.Kingdoms;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Library;

namespace Bannerlord.GameMaster.Console.KingdomCommands.KingdomDiplomacyCommands;

public static class DeclareAllianceCommand
{
    [CommandLineFunctionality.CommandLineArgumentFunction("declare_alliance", "gm.kingdom")]
    public static string DeclareAlliance(List<string> args)
    {
        return Cmd.Run(args, () =>
        {
            // MARK: Validation
            if (!CommandValidator.ValidateCampaignState(out string error))
                return error;

            string usageMessage = CommandValidator.CreateUsageMessage(
                "gm.kingdom.declare_alliance", "<proposingKingdom> <receivingKingdom> [callToWar]",
                "Declares an alliance between two kingdoms.\n" +
                "callToWar (optional): If true, receiving kingdom declares war on proposing kingdom's enemies (default: true)\n" +
                "Supports named arguments: proposingKingdom:empire receivingKingdom:battania callToWar:false",
                "gm.kingdom.declare_alliance empire battania\n" +
                "gm.kingdom.declare_alliance proposingKingdom:empire receivingKingdom:battania callToWar:false");

            ParsedArguments parsed = ArgumentParser.ParseArguments(args);

            parsed.SetValidArguments(
                new ArgumentDefinition("proposingKingdom", true),
                new ArgumentDefinition("receivingKingdom", true),
                new ArgumentDefinition("callToWar", false, "true")
            );

            string validationError = parsed.GetValidationError();
            if (validationError != null)
                return MessageFormatter.FormatErrorMessage(validationError);

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

            bool callToWar = true;
            string callToWarArg = parsed.GetArgument("callToWar", 2);
            if (callToWarArg != null)
            {
                if (!bool.TryParse(callToWarArg, out callToWar))
                    return MessageFormatter.FormatErrorMessage($"Invalid value for 'callToWar': '{callToWarArg}'. Must be true or false.");
            }

            if (proposingKingdom == receivingKingdom)
                return MessageFormatter.FormatErrorMessage("A kingdom cannot form an alliance with itself.");

            if (FactionManager.IsAtWarAgainstFaction(proposingKingdom, receivingKingdom))
                return MessageFormatter.FormatErrorMessage($"{proposingKingdom.Name} and {receivingKingdom.Name} are at war. Make peace first.");

            if (proposingKingdom.IsAllyWith(receivingKingdom))
                return MessageFormatter.FormatErrorMessage($"{proposingKingdom.Name} and {receivingKingdom.Name} are already allies.");

            // MARK: Execute Logic
            Dictionary<string, string> resolvedValues = new()
            {
                { "proposingKingdom", proposingKingdom.Name.ToString() },
                { "receivingKingdom", receivingKingdom.Name.ToString() },
                { "callToWar", callToWar.ToString() }
            };
            string argumentDisplay = parsed.FormatArgumentDisplay("declare_alliance", resolvedValues);

            proposingKingdom.DeclareAlliance(receivingKingdom, callToWar);

            string message = $"Alliance formed between {proposingKingdom.Name} and {receivingKingdom.Name}.";
            if (callToWar && proposingKingdom.FactionsAtWarWith.Count > 0)
                message += $"\n{receivingKingdom.Name} called to war against {proposingKingdom.Name}'s enemies.";

            return argumentDisplay + MessageFormatter.FormatSuccessMessage(message);
        });
    }
}
