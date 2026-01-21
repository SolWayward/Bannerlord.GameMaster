using Bannerlord.GameMaster.Caravans;
using Bannerlord.GameMaster.Console.Common;
using Bannerlord.GameMaster.Console.Common.Execution;
using Bannerlord.GameMaster.Console.Common.Formatting;
using Bannerlord.GameMaster.Console.Common.Parsing;
using Bannerlord.GameMaster.Console.Common.Validation;
using System.Collections.Generic;
using TaleWorlds.Library;

namespace Bannerlord.GameMaster.Console.CaravanCommands.CaravanManagementCommands
{
    [CommandLineFunctionality.CommandLineArgumentFunction("caravan", "gm")]
    public static class DisbandNotableCaravansCommand
    {
        /// <summary>
        /// Disband notable caravans.
        /// Usage: gm.caravan.disband_notable_caravans &lt;count&gt;
        /// </summary>
        [CommandLineFunctionality.CommandLineArgumentFunction("disband_notable_caravans", "gm.caravan")]
        public static string DisbandNotableCaravans(List<string> args)
        {
            return Cmd.Run(args, () =>
            {
                // MARK: Validation
                if (!CommandValidator.ValidateCampaignState(out string error))
                    return CommandResult.Error(error).Log().Message;

                string usageMessage = CommandValidator.CreateUsageMessage(
                    "gm.caravan.disband_notable_caravans", "<count>",
                    "Disbands the specified number of notable caravans.\n" +
                    "- count: Required. Number of caravans to disband, or 'all' to disband all\n" +
                    "Supports named arguments: count:8",
                    "gm.caravan.disband_notable_caravans all\n" +
                    "gm.caravan.disband_notable_caravans 5\n" +
                    "gm.caravan.disband_notable_caravans count:3");

                ParsedArguments parsed = ArgumentParser.ParseArguments(args);

                parsed.SetValidArguments(
                    new ArgumentDefinition("count", true)
                );

                string validationError = parsed.GetValidationError();
                if (validationError != null)
                    return CommandResult.Error(MessageFormatter.FormatErrorMessage(validationError)).Log().Message;

                if (parsed.TotalCount < 1)
                    return CommandResult.Error(usageMessage).Log().Message;

                // MARK: Parse Arguments
                string countArg = parsed.GetArgument("count", 0);
                if (string.IsNullOrWhiteSpace(countArg))
                    return CommandResult.Error(MessageFormatter.FormatErrorMessage("Missing required argument 'count'.")).Log().Message;

                int? count = CaravanCommandHelpers.ParseCountArgument(countArg, out string parseError);
                if (parseError != null)
                    return CommandResult.Error(MessageFormatter.FormatErrorMessage(parseError)).Log().Message;

                // MARK: Execute Logic
                Dictionary<string, string> resolvedValues = new()
                {
                    { "count", CaravanCommandHelpers.FormatCountForDisplay(countArg, count) }
                };

                int disbanded = CaravanManager.DisbandNotableCaravans(count);

                string countInfo = CaravanCommandHelpers.FormatCountInfoSuffix(count);
                string countsSummary = CaravanCommandHelpers.GetCaravanCountsSummary();

                string argumentDisplay = parsed.FormatArgumentDisplay("disband_notable_caravans", resolvedValues);
                string fullMessage = argumentDisplay + MessageFormatter.FormatSuccessMessage(
                    $"Disbanded {disbanded} notable caravans{countInfo}.\n\n" +
                    $"Remaining Counts:\n{countsSummary}");
                return CommandResult.Success(fullMessage).Log().Message;
            });
        }
    }
}
