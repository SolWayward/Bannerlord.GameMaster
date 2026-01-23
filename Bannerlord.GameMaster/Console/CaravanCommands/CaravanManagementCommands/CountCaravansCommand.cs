using Bannerlord.GameMaster.Console.Common;
using Bannerlord.GameMaster.Console.Common.Execution;
using Bannerlord.GameMaster.Console.Common.Formatting;
using Bannerlord.GameMaster.Console.Common.Validation;
using System.Collections.Generic;
using TaleWorlds.Library;

namespace Bannerlord.GameMaster.Console.CaravanCommands.CaravanManagementCommands
{
    [CommandLineFunctionality.CommandLineArgumentFunction("caravan", "gm")]
    public static class CountCaravansCommand
    {
        /// <summary>
        /// Display counts of all caravans by owner type.
        /// Usage: gm.caravan.count
        /// </summary>
        [CommandLineFunctionality.CommandLineArgumentFunction("count", "gm.caravan")]
        public static string Count(List<string> args)
        {
            return Cmd.Run(args, () =>
            {
                // MARK: Validation
                if (!CommandValidator.ValidateCampaignState(out string error))
                    return CommandResult.Error(error).Message;

                // MARK: Execute Logic
                string countsSummary = CaravanCommandHelpers.GetCaravanCountsSummary();
                string fullMessage = MessageFormatter.FormatSuccessMessage($"Caravan Statistics:\n{countsSummary}");
                return CommandResult.Success(fullMessage).Message;
            });
        }
    }
}
