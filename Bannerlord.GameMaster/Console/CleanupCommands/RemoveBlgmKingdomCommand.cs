using Bannerlord.GameMaster.Common;
using Bannerlord.GameMaster.Console.Common.EntityFinding;
using Bannerlord.GameMaster.Console.Common.Execution;
using Bannerlord.GameMaster.Console.Common.Formatting;
using Bannerlord.GameMaster.Console.Common.Parsing;
using Bannerlord.GameMaster.Console.Common.Validation;
using Bannerlord.GameMaster.RemovalHelpers;
using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Library;

namespace Bannerlord.GameMaster.Console.CleanupCommands;

public static class RemoveBlgmKingdomCommand
{
    /// <summary>
    /// Removes a single BLGM-generated kingdom
    /// Usage: gm.cleanup.remove_blgm_kingdom <kingdom>
    /// </summary>
    [CommandLineFunctionality.CommandLineArgumentFunction("remove_blgm_kingdom", "gm.cleanup")]
    public static string RemoveBlgmKingdom(List<string> args)
    {
        return Cmd.Run(args, () =>
        {
            // MARK: Validation
            if (!CommandValidator.ValidateCampaignState(out string error))
                return error;

            string usageMessage = CommandValidator.CreateUsageMessage(
                "gm.cleanup.remove_blgm_kingdom", "<kingdom>",
                "Removes a single BLGM-generated kingdom.\n" +
                "- kingdom: Kingdom identifier (name or ID)",
                "gm.cleanup.remove_blgm_kingdom blgm_kingdom_123");

            ParsedArguments parsed = ArgumentParser.ParseArguments(args);

            parsed.SetValidArguments(
                new ArgumentDefinition("kingdom", true)
            );

            string validationError = parsed.GetValidationError();
            if (validationError != null)
                return MessageFormatter.FormatErrorMessage(validationError);

            if (parsed.TotalCount < 1)
                return usageMessage;

            // MARK: Parse Arguments
            string kingdomIdentifier = parsed.GetArgument("kingdom", 0);
            if (string.IsNullOrWhiteSpace(kingdomIdentifier))
                return MessageFormatter.FormatErrorMessage("Kingdom identifier cannot be empty.");

            EntityFinderResult<Kingdom> kingdomResult = KingdomFinder.FindSingleKingdom(kingdomIdentifier);
            if (!kingdomResult.IsSuccess)
                return kingdomResult.Message;
            Kingdom kingdom = kingdomResult.Entity;

            // MARK: Execute Logic
            BLGMResult result = KingdomRemover.RemoveSingleKingdom(kingdom);

            Dictionary<string, string> resolvedValues = new()
            {
                { "kingdom", kingdom.Name.ToString() }
            };

            string argumentDisplay = parsed.FormatArgumentDisplay("remove_blgm_kingdom", resolvedValues);

            if (result.wasSuccessful)
            {
                return argumentDisplay + MessageFormatter.FormatSuccessMessage(result.message);
            }
            else
            {
                return argumentDisplay + MessageFormatter.FormatErrorMessage(result.message);
            }
        });
    }
}
