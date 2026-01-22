using System.Collections.Generic;
using Bannerlord.GameMaster.Console.Common;
using Bannerlord.GameMaster.Console.Common.Execution;
using Bannerlord.GameMaster.Console.Common.EntityFinding;
using Bannerlord.GameMaster.Console.Common.Formatting;
using Bannerlord.GameMaster.Console.Common.Parsing;
using Bannerlord.GameMaster.Console.Common.Validation;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.Library;

namespace Bannerlord.GameMaster.Console.KingdomCommands.KingdomManagementCommands;

public static class DestroyKingdomCommand
{
    /// <summary>
    /// Destroy/Eliminate a kingdom
    /// Usage: gm.kingdom.destroy [kingdom]
    /// </summary>
    [CommandLineFunctionality.CommandLineArgumentFunction("destroy", "gm.kingdom")]
    public static string DestroyKingdom(List<string> args)
    {
        return Cmd.Run(args, () =>
        {
            // MARK: Validation
            if (!CommandValidator.ValidateCampaignState(out string error))
                return error;

            string usageMessage = CommandValidator.CreateUsageMessage(
                "gm.kingdom.destroy", "<kingdom>",
                "Destroys/eliminates the specified kingdom.\n" +
                "Supports named arguments: kingdom:battania",
                "gm.kingdom.destroy battania");

            ParsedArguments parsed = ArgumentParser.ParseArguments(args);

            parsed.SetValidArguments(
                new ArgumentDefinition("kingdom", true)
            );

            string validationError = parsed.GetValidationError();
            if (validationError != null)
                return CommandResult.Error(MessageFormatter.FormatErrorMessage(validationError)).Log().Message;

            if (parsed.TotalCount < 1)
                return usageMessage;

            // MARK: Parse Arguments
            string kingdomArg = parsed.GetArgument("kingdom", 0);
            if (kingdomArg == null)
                return MessageFormatter.FormatErrorMessage("Missing required argument 'kingdom'.");

            EntityFinderResult<Kingdom> kingdomResult = KingdomFinder.FindSingleKingdom(kingdomArg);
            if (!kingdomResult.IsSuccess)
                return kingdomResult.Message;
            Kingdom kingdom = kingdomResult.Entity;

            if (kingdom.IsEliminated)
                return MessageFormatter.FormatErrorMessage($"{kingdom.Name} is already eliminated.");

            if (kingdom == Hero.MainHero.MapFaction)
                return MessageFormatter.FormatErrorMessage("Cannot destroy the player's kingdom.");

            // MARK: Execute Logic
            Dictionary<string, string> resolvedValues = new()
            {
                { "kingdom", kingdom.Name.ToString() }
            };

            DestroyKingdomAction.Apply(kingdom);

            string argumentDisplay = parsed.FormatArgumentDisplay("gm.kingdom.destroy", resolvedValues);
            return CommandResult.Success(argumentDisplay + MessageFormatter.FormatSuccessMessage(
                $"{kingdom.Name} has been destroyed/eliminated.")).Log().Message;
        });
    }
}
