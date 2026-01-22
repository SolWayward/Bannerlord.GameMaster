using System.Collections.Generic;
using Bannerlord.GameMaster.Bandits;
using Bannerlord.GameMaster.Console.Common;
using Bannerlord.GameMaster.Console.Common.Execution;
using Bannerlord.GameMaster.Console.Common.Formatting;
using Bannerlord.GameMaster.Console.Common.Parsing;
using Bannerlord.GameMaster.Console.Common.Validation;
using TaleWorlds.Library;

namespace Bannerlord.GameMaster.Console.BanditCommands.BanditManagementCommands
{
    /// <summary>
    /// Console command to remove all bandit parties and hideouts.
    /// Usage: gm.bandit.remove_all &lt;confirmation&gt;
    /// </summary>
    [CommandLineFunctionality.CommandLineArgumentFunction("bandit", "gm")]
    public static class RemoveAllBanditsCommand
    {
        /// <summary>
        /// Remove all bandit parties and hideouts.
        /// Usage: gm.bandit.remove_all &lt;confirmation&gt;
        /// </summary>
        [CommandLineFunctionality.CommandLineArgumentFunction("remove_all", "gm.bandit")]
        public static string RemoveAll(List<string> args)
        {
            return Cmd.Run(args, () =>
            {
                // MARK: Validation
                if (!CommandValidator.ValidateCampaignState(out string error))
                    return error;

                string usageMessage = CommandValidator.CreateUsageMessage(
                    "gm.bandit.remove_all", "<confirmation>",
                    "Removes ALL bandit parties AND ALL bandit hideouts from the game.\n" +
                    "WARNING: This is a destructive operation that cannot be undone!\n" +
                    "- confirmation: Required. Must be 'confirm' to execute\n" +
                    "Supports named arguments: confirmation:confirm",
                    "gm.bandit.remove_all confirm");

                ParsedArguments parsed = ArgumentParser.ParseArguments(args);

                parsed.SetValidArguments(
                    new ArgumentDefinition("confirmation", true)
                );

                string validationError = parsed.GetValidationError();
                if (validationError != null)
                    return CommandResult.Error(MessageFormatter.FormatErrorMessage(validationError)).Log().Message;

                if (parsed.TotalCount < 1)
                    return CommandResult.Error(usageMessage).Log().Message;

                // MARK: Parse Arguments
                string confirmationArg = parsed.GetArgument("confirmation", 0);
                if (confirmationArg == null)
                    return CommandResult.Error(MessageFormatter.FormatErrorMessage("Missing required argument 'confirmation'.")).Log().Message;

                if (confirmationArg.ToLower() != "confirm")
                    return CommandResult.Error(MessageFormatter.FormatErrorMessage(
                        $"Invalid confirmation value: '{confirmationArg}'. Must be 'confirm' to execute this command.")).Log().Message;

                // MARK: Execute Logic
                Dictionary<string, string> resolvedValues = new()
                {
                    { "confirmation", "confirm" }
                };

                int partiesRemoved = BanditManager.RemoveAllBanditParties(null);
                int hideoutsRemoved = BanditManager.RemoveAllHideouts(null);

                string countsSummary = BanditCommandHelpers.GetBanditCountsSummary();

                string argumentDisplay = parsed.FormatArgumentDisplay("gm.bandit.remove_all", resolvedValues);
                string fullMessage = argumentDisplay + MessageFormatter.FormatSuccessMessage(
                    $"Removed all bandits from the game.\n" +
                    $"Parties destroyed: {partiesRemoved}\n" +
                    $"Hideouts cleared: {hideoutsRemoved}\n\n" +
                    $"Remaining Counts:\n{countsSummary}");
                return CommandResult.Success(fullMessage).Log().Message;
            });
        }
    }
}
