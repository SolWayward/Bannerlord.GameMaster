using Bannerlord.GameMaster.Common;
using Bannerlord.GameMaster.Console.Common.Execution;
using Bannerlord.GameMaster.Console.Common.Formatting;
using Bannerlord.GameMaster.Console.Common.Validation;
using Bannerlord.GameMaster.Settlements;
using System.Collections.Generic;
using TaleWorlds.Library;

namespace Bannerlord.GameMaster.Console.SettlementCommands.SettlementManagementCommands;

/// <summary>
/// Reset all settlements to their original names
/// Usage: gm.settlement.reset_all_names
/// </summary>
public static class ResetAllSettlementNamesCommand
{
    [CommandLineFunctionality.CommandLineArgumentFunction("reset_all_names", "gm.settlement")]
    public static string ResetAllSettlementNames(List<string> args)
    {
        return Cmd.Run(args, () =>
        {
            // MARK: Validation
            if (!CommandValidator.ValidateCampaignState(out string error))
                return error;

            string usageMessage = CommandValidator.CreateUsageMessage(
                "gm.settlement.reset_all_names", "",
                "Restores all settlements to their original names.",
                "gm.settlement.reset_all_names");

            if (args.Count > 0)
                return usageMessage;

            // MARK: Execute Logic
            int renamedCount = SettlementManager.GetRenamedSettlementCount();
            if (renamedCount == 0)
                return MessageFormatter.FormatSuccessMessage("No settlements have been renamed.");

            BLGMResult result = SettlementManager.ResetAllSettlementNames();
            if (!result.wasSuccessful)
                return MessageFormatter.FormatErrorMessage(result.message);

            return MessageFormatter.FormatSuccessMessage(result.message);
        });
    }
}
