using Bannerlord.GameMaster.Caravans;
using Bannerlord.GameMaster.Console.Common.Execution;
using Bannerlord.GameMaster.Console.Common.Formatting;
using Bannerlord.GameMaster.Console.Common.Parsing;
using Bannerlord.GameMaster.Console.Common.Validation;
using System.Collections.Generic;
using TaleWorlds.Library;

namespace Bannerlord.GameMaster.Console.CaravanCommands.CaravanManagementCommands
{
    [CommandLineFunctionality.CommandLineArgumentFunction("caravan", "gm")]
    public static class DisbandCaravansCommand
    {
        /// <summary>
        /// Disband caravans from all caravans.
        /// Usage: gm.caravan.disband_caravans &lt;count&gt;
        /// </summary>
        [CommandLineFunctionality.CommandLineArgumentFunction("disband_caravans", "gm.caravan")]
        public static string DisbandCaravans(List<string> args)
        {
            return Cmd.Run(args, () =>
            {
                // MARK: Validation
                if (!CommandValidator.ValidateCampaignState(out string error))
                    return error;

                string usageMessage = CommandValidator.CreateUsageMessage(
                    "gm.caravan.disband_caravans", "<count>",
                    "Disbands the specified number of random caravans from ALL caravans.\n" +
                    "- count: Required. Number of caravans to disband, or 'all' to disband all\n" +
                    "Supports named arguments: count:10",
                    "gm.caravan.disband_caravans all\n" +
                    "gm.caravan.disband_caravans 5\n" +
                    "gm.caravan.disband_caravans count:10");

                ParsedArguments parsed = ArgumentParser.ParseArguments(args);

                parsed.SetValidArguments(
                    new ArgumentDefinition("count", true)
                );

                string validationError = parsed.GetValidationError();
                if (validationError != null)
                    return MessageFormatter.FormatErrorMessage(validationError);

                if (parsed.TotalCount < 1)
                    return usageMessage;

                // MARK: Parse Arguments
                string countArg = parsed.GetArgument("count", 0);
                if (string.IsNullOrWhiteSpace(countArg))
                    return MessageFormatter.FormatErrorMessage("Missing required argument 'count'.");

                int? count = CaravanCommandHelpers.ParseCountArgument(countArg, out string parseError);
                if (parseError != null)
                    return MessageFormatter.FormatErrorMessage(parseError);

                // MARK: Execute Logic
                Dictionary<string, string> resolvedValues = new()
                {
                    { "count", CaravanCommandHelpers.FormatCountForDisplay(countArg, count) }
                };

                int disbanded = CaravanManager.DisbandCaravans(count);

                string countInfo = CaravanCommandHelpers.FormatCountInfoSuffix(count);
                string countsSummary = CaravanCommandHelpers.GetCaravanCountsSummary();

                string argumentDisplay = parsed.FormatArgumentDisplay("disband_caravans", resolvedValues);
                return argumentDisplay + MessageFormatter.FormatSuccessMessage(
                    $"Disbanded {disbanded} caravans{countInfo}.\n\n" +
                    $"Remaining Counts:\n{countsSummary}");
            });
        }
    }
}
