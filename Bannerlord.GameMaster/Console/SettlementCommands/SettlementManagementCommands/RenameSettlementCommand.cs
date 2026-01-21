using Bannerlord.GameMaster.Common;
using Bannerlord.GameMaster.Console.Common;
using Bannerlord.GameMaster.Console.Common.Execution;
using Bannerlord.GameMaster.Console.Common.EntityFinding;
using Bannerlord.GameMaster.Console.Common.Formatting;
using Bannerlord.GameMaster.Console.Common.Parsing;
using Bannerlord.GameMaster.Console.Common.Validation;
using Bannerlord.GameMaster.Settlements;
using System.Collections.Generic;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Library;

namespace Bannerlord.GameMaster.Console.SettlementCommands.SettlementManagementCommands;

/// <summary>
/// Rename a settlement with save persistence
/// Usage: gm.settlement.rename [settlement] [new_name]
/// </summary>
public static class RenameSettlementCommand
{
    [CommandLineFunctionality.CommandLineArgumentFunction("rename", "gm.settlement")]
    public static string RenameSettlement(List<string> args)
    {
        return Cmd.Run(args, () =>
        {
            // MARK: Validation
            if (!CommandValidator.ValidateCampaignState(out string error))
                return CommandResult.Error(error).Log().Message;

            string usageMessage = CommandValidator.CreateUsageMessage(
                "gm.settlement.rename", "<settlement> <new_name>",
                "Changes the name of any settlement type (city, castle, village, hideout).\n" +
                "The new name persists through save/load cycles.\n" +
                "Use SINGLE QUOTES for multi-word names (double quotes don't work in TaleWorlds console).",
                "gm.settlement.rename pen NewName\ngm.settlement.rename pen 'Castle of Stone'");

            ParsedArguments parsed = ArgumentParser.ParseArguments(args);
            parsed.SetValidArguments(
                new ArgumentDefinition("settlement", true),
                new ArgumentDefinition("name", true)
            );

            string validationError = parsed.GetValidationError();
            if (validationError != null)
                return CommandResult.Error(MessageFormatter.FormatErrorMessage(validationError)).Log().Message;

            if (parsed.TotalCount < 2)
                return CommandResult.Error(usageMessage).Log().Message;

            // MARK: Parse Arguments
            string settlementQuery = parsed.GetArgument("settlement", 0);
            string newName = parsed.GetArgument("name", 1);

            EntityFinderResult<Settlement> settlementResult = SettlementFinder.FindSingleSettlement(settlementQuery);
            if (!settlementResult.IsSuccess) return CommandResult.Error(settlementResult.Message).Log().Message;
            Settlement settlement = settlementResult.Entity;

            if (string.IsNullOrWhiteSpace(newName))
                return CommandResult.Error(MessageFormatter.FormatErrorMessage("New name cannot be empty.")).Log().Message;

            // MARK: Execute Logic
            string previousName = settlement.Name.ToString();

            BLGMResult result = SettlementManager.RenameSettlement(settlement, newName);
            if (!result.IsSuccess)
                return CommandResult.Error(MessageFormatter.FormatErrorMessage(result.Message)).Log().Message;

            Dictionary<string, string> resolvedValues = new()
            {
                { "settlement", previousName },
                { "name", newName }
            };

            string argumentDisplay = parsed.FormatArgumentDisplay("rename", resolvedValues);
            string fullMessage = argumentDisplay + MessageFormatter.FormatSuccessMessage(
                $"Settlement renamed from '{previousName}' to '{settlement.Name}' (ID: {settlement.StringId}).\n" +
                $"The new name will persist through save/load cycles.\n" +
                $"The name may not visual update on the campaign map immediately, Open and Close any menu to refresh name");
            return CommandResult.Success(fullMessage).Log().Message;
        });
    }
}
