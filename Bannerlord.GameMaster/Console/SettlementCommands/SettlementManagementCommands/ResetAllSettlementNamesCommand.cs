using Bannerlord.GameMaster.Behaviours;
using Bannerlord.GameMaster.Console.Common.Execution;
using Bannerlord.GameMaster.Console.Common.Formatting;
using Bannerlord.GameMaster.Console.Common.Validation;
using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
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
            SettlementNameBehavior behavior = Campaign.Current.GetCampaignBehavior<SettlementNameBehavior>();
            if (behavior == null)
                return MessageFormatter.FormatErrorMessage("Settlement name behavior not initialized. Please restart the game.");

            int renamedCount = behavior.GetRenamedSettlementCount();
            if (renamedCount == 0)
                return MessageFormatter.FormatSuccessMessage("No settlements have been renamed.");

            int resetCount = behavior.ResetAllSettlementNames();

            return MessageFormatter.FormatSuccessMessage(
                $"Reset {resetCount} settlement(s) to their original names.");
        });
    }
}
