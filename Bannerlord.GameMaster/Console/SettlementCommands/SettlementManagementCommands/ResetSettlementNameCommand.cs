using Bannerlord.GameMaster.Behaviours;
using Bannerlord.GameMaster.Console.Common.Execution;
using Bannerlord.GameMaster.Console.Common.EntityFinding;
using Bannerlord.GameMaster.Console.Common.Formatting;
using Bannerlord.GameMaster.Console.Common.Parsing;
using Bannerlord.GameMaster.Console.Common.Validation;
using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Library;

namespace Bannerlord.GameMaster.Console.SettlementCommands.SettlementManagementCommands;

/// <summary>
/// Reset a settlement to its original name
/// Usage: gm.settlement.reset_name [settlement]
/// </summary>
public static class ResetSettlementNameCommand
{
    [CommandLineFunctionality.CommandLineArgumentFunction("reset_name", "gm.settlement")]
    public static string ResetSettlementName(List<string> args)
    {
        return Cmd.Run(args, () =>
        {
            // MARK: Validation
            if (!CommandValidator.ValidateCampaignState(out string error))
                return error;

            string usageMessage = CommandValidator.CreateUsageMessage(
                "gm.settlement.reset_name", "<settlement>",
                "Restores a settlement to its original name.",
                "gm.settlement.reset_name pen");

            ParsedArguments parsed = ArgumentParser.ParseArguments(args);
            parsed.SetValidArguments(
                new ArgumentDefinition("settlement", true)
            );

            string validationError = parsed.GetValidationError();
            if (validationError != null)
                return MessageFormatter.FormatErrorMessage(validationError);

            if (parsed.TotalCount < 1)
                return usageMessage;

            // MARK: Parse Arguments
            string settlementQuery = parsed.GetArgument("settlement", 0);

            EntityFinderResult<Settlement> settlementResult = SettlementFinder.FindSingleSettlement(settlementQuery);
            if (!settlementResult.IsSuccess) return settlementResult.Message;
            Settlement settlement = settlementResult.Entity;

            // MARK: Execute Logic
            SettlementNameBehavior behavior = Campaign.Current.GetCampaignBehavior<SettlementNameBehavior>();
            if (behavior == null)
                return MessageFormatter.FormatErrorMessage("Settlement name behavior not initialized. Please restart the game.");

            if (!behavior.IsRenamed(settlement))
                return MessageFormatter.FormatErrorMessage($"Settlement '{settlement.Name}' (ID: {settlement.StringId}) has not been renamed.");

            string originalName = behavior.GetOriginalName(settlement);
            string currentName = settlement.Name.ToString();

            if (!behavior.ResetSettlementName(settlement))
                return MessageFormatter.FormatErrorMessage("Failed to reset settlement name. Check the error log for details.");

            Dictionary<string, string> resolvedValues = new()
            {
                { "settlement", currentName }
            };

            string argumentDisplay = parsed.FormatArgumentDisplay("reset_name", resolvedValues);
            return argumentDisplay + MessageFormatter.FormatSuccessMessage(
                $"Settlement name reset from '{currentName}' to '{settlement.Name}' (original: '{originalName}') (ID: {settlement.StringId}).");
        });
    }
}
