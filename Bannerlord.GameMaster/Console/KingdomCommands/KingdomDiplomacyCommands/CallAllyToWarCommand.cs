using System.Collections.Generic;
using System.Linq;
using Bannerlord.GameMaster.Console.Common.EntityFinding;
using Bannerlord.GameMaster.Console.Common.Execution;
using Bannerlord.GameMaster.Console.Common.Formatting;
using Bannerlord.GameMaster.Console.Common.Parsing;
using Bannerlord.GameMaster.Console.Common.Validation;
using Bannerlord.GameMaster.Kingdoms;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Library;

namespace Bannerlord.GameMaster.Console.KingdomCommands.KingdomDiplomacyCommands;

public static class CallAllyToWarCommand
{
    [CommandLineFunctionality.CommandLineArgumentFunction("call_ally_to_war", "gm.kingdom")]
    public static string CallAllyToWar(List<string> args)
    {
        return Cmd.Run(args, () =>
        {
            // MARK: Validation
            if (!CommandValidator.ValidateCampaignState(out string error))
                return error;

            string usageMessage = CommandValidator.CreateUsageMessage(
                "gm.kingdom.call_ally_to_war", "<proposingKingdom> <allyKingdom> [enemyKingdom]",
                "Calls ally kingdom to war against specified enemy or all enemies.\n" +
                "If enemyKingdom is omitted, ally declares war on all of proposer's enemies.\n" +
                "Supports named arguments: proposingKingdom:empire allyKingdom:battania enemyKingdom:sturgia",
                "gm.kingdom.call_ally_to_war empire battania sturgia\n" +
                "gm.kingdom.call_ally_to_war empire battania");

            ParsedArguments parsed = ArgumentParser.ParseArguments(args);

            parsed.SetValidArguments(
                new ArgumentDefinition("proposingKingdom", true),
                new ArgumentDefinition("allyKingdom", true),
                new ArgumentDefinition("enemyKingdom", false, "All enemies")
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

            string allyKingdomArg = parsed.GetArgument("allyKingdom", 1);
            if (allyKingdomArg == null)
                return MessageFormatter.FormatErrorMessage("Missing required argument 'allyKingdom'.");

            EntityFinderResult<Kingdom> allyResult = KingdomFinder.FindSingleKingdom(allyKingdomArg);
            if (!allyResult.IsSuccess) return allyResult.Message;
            Kingdom allyKingdom = allyResult.Entity;

            if (proposingKingdom == allyKingdom)
                return MessageFormatter.FormatErrorMessage("A kingdom cannot call itself to war.");

            if (!proposingKingdom.IsAllyWith(allyKingdom))
                return MessageFormatter.FormatErrorMessage($"{proposingKingdom.Name} and {allyKingdom.Name} are not allies.");

            string enemyKingdomArg = parsed.GetArgument("enemyKingdom", 2);
            Kingdom enemyKingdom = null;

            if (enemyKingdomArg != null)
            {
                EntityFinderResult<Kingdom> enemyResult = KingdomFinder.FindSingleKingdom(enemyKingdomArg);
                if (!enemyResult.IsSuccess) return enemyResult.Message;
                enemyKingdom = enemyResult.Entity;

                if (!FactionManager.IsAtWarAgainstFaction(proposingKingdom, enemyKingdom))
                    return MessageFormatter.FormatErrorMessage($"{proposingKingdom.Name} is not at war with {enemyKingdom.Name}.");

                if (FactionManager.IsAtWarAgainstFaction(allyKingdom, enemyKingdom))
                    return MessageFormatter.FormatErrorMessage($"{allyKingdom.Name} is already at war with {enemyKingdom.Name}.");
            }
            else
            {
                if (proposingKingdom.FactionsAtWarWith.Count == 0)
                    return MessageFormatter.FormatErrorMessage($"{proposingKingdom.Name} is not at war with any kingdoms.");
            }

            // MARK: Execute Logic
            Dictionary<string, string> resolvedValues = new()
            {
                { "proposingKingdom", proposingKingdom.Name.ToString() },
                { "allyKingdom", allyKingdom.Name.ToString() },
                { "enemyKingdom", enemyKingdom != null ? enemyKingdom.Name.ToString() : "All enemies" }
            };
            string argumentDisplay = parsed.FormatArgumentDisplay("call_ally_to_war", resolvedValues);

            if (enemyKingdom != null)
            {
                proposingKingdom.ProposeCallAllyToWarForceAccept(allyKingdom, enemyKingdom);
                return argumentDisplay + MessageFormatter.FormatSuccessMessage(
                    $"{allyKingdom.Name} called to war against {enemyKingdom.Name}.");
            }
            else
            {
                List<Kingdom> enemies = proposingKingdom.FactionsAtWarWith
                    .Where(f => f.IsKingdomFaction)
                    .Cast<Kingdom>()
                    .ToList();

                proposingKingdom.ProposeCallAllyToWarForceAccept(allyKingdom);

                string enemyList = string.Join(", ", enemies.Select(k => k.Name.ToString()));
                return argumentDisplay + MessageFormatter.FormatSuccessMessage(
                    $"{allyKingdom.Name} called to war against all enemies: {enemyList}");
            }
        });
    }
}
