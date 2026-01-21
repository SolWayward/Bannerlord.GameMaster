using System.Collections.Generic;
using Bannerlord.GameMaster.Console.Common;
using Bannerlord.GameMaster.Console.Common.EntityFinding;
using Bannerlord.GameMaster.Console.Common.Execution;
using Bannerlord.GameMaster.Console.Common.Formatting;
using Bannerlord.GameMaster.Console.Common.Parsing;
using Bannerlord.GameMaster.Console.Common.Validation;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.Library;

namespace Bannerlord.GameMaster.Console.KingdomCommands.KingdomDiplomacyCommands;

public static class DeclareWarCommand
{
    [CommandLineFunctionality.CommandLineArgumentFunction("declare_war", "gm.kingdom")]
    public static string DeclareWar(List<string> args)
    {
        return Cmd.Run(args, () =>
        {
            // MARK: Validation
            if (!CommandValidator.ValidateCampaignState(out string error))
                return error;

            string usageMessage = CommandValidator.CreateUsageMessage(
                "gm.kingdom.declare_war", "<kingdom1> <kingdom2>",
                "Declares war between two kingdoms.\n" +
                "Supports named arguments: kingdom1:empire kingdom2:battania",
                "gm.kingdom.declare_war empire battania");

            ParsedArguments parsed = ArgumentParser.ParseArguments(args);

            parsed.SetValidArguments(
                new ArgumentDefinition("kingdom1", true),
                new ArgumentDefinition("kingdom2", true)
            );

            string validationError = parsed.GetValidationError();
            if (validationError != null)
                return CommandResult.Error(MessageFormatter.FormatErrorMessage(validationError)).Log().Message;

            if (parsed.TotalCount < 2)
                return usageMessage;

            // MARK: Parse Arguments
            string kingdom1Arg = parsed.GetArgument("kingdom1", 0);
            if (kingdom1Arg == null)
                return MessageFormatter.FormatErrorMessage("Missing required argument 'kingdom1'.");

            EntityFinderResult<Kingdom> kingdom1Result = KingdomFinder.FindSingleKingdom(kingdom1Arg);
            if (!kingdom1Result.IsSuccess) return kingdom1Result.Message;
            Kingdom kingdom1 = kingdom1Result.Entity;

            string kingdom2Arg = parsed.GetArgument("kingdom2", 1);
            if (kingdom2Arg == null)
                return MessageFormatter.FormatErrorMessage("Missing required argument 'kingdom2'.");

            EntityFinderResult<Kingdom> kingdom2Result = KingdomFinder.FindSingleKingdom(kingdom2Arg);
            if (!kingdom2Result.IsSuccess) return kingdom2Result.Message;
            Kingdom kingdom2 = kingdom2Result.Entity;

            if (kingdom1 == kingdom2)
                return MessageFormatter.FormatErrorMessage("A kingdom cannot declare war on itself.");

            if (FactionManager.IsAtWarAgainstFaction(kingdom1, kingdom2))
                return MessageFormatter.FormatErrorMessage($"{kingdom1.Name} and {kingdom2.Name} are already at war.");

            // MARK: Execute Logic
            Dictionary<string, string> resolvedValues = new()
            {
                { "kingdom1", kingdom1.Name.ToString() },
                { "kingdom2", kingdom2.Name.ToString() }
            };
            string argumentDisplay = parsed.FormatArgumentDisplay("declare_war", resolvedValues);

            DeclareWarAction.ApplyByDefault(kingdom1, kingdom2);
            return argumentDisplay + MessageFormatter.FormatSuccessMessage($"War declared between {kingdom1.Name} and {kingdom2.Name}.");
        });
    }
}
