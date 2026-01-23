using System.Collections.Generic;
using Bannerlord.GameMaster.Console.Common;
using Bannerlord.GameMaster.Console.Common.Execution;
using Bannerlord.GameMaster.Console.Common.Formatting;
using Bannerlord.GameMaster.Console.Common.Validation;
using TaleWorlds.Library;

namespace Bannerlord.GameMaster.Console.BanditCommands.BanditManagementCommands
{
    /// <summary>
    /// Console command to display counts of all bandit parties and hideouts.
    /// Usage: gm.bandit.count
    /// </summary>
    [CommandLineFunctionality.CommandLineArgumentFunction("bandit", "gm")]
    public static class CountCommand
    {
        /// <summary>
        /// Display counts of all bandit parties and hideouts.
        /// Usage: gm.bandit.count
        /// </summary>
        [CommandLineFunctionality.CommandLineArgumentFunction("count", "gm.bandit")]
        public static string Count(List<string> args)
        {
            return Cmd.Run(args, () =>
            {
                // MARK: Validation
                if (!CommandValidator.ValidateCampaignState(out string error))
                    return CommandResult.Error(error).Message
;

                // MARK: Execute Logic
                string countsSummary = BanditCommandHelpers.GetBanditCountsSummary();
                string fullMessage = MessageFormatter.FormatSuccessMessage($"Bandit Statistics:\n{countsSummary}");
                return CommandResult.Success(fullMessage).Message
;
            });
        }
    }
}
