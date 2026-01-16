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
    public static class ForceDestroyDisbandingCaravansCommand
    {
        /// <summary>
        /// Destroys all disbanding caravans.
        /// Usage: gm.caravan.force_destroy_disbanding_caravans confirm
        /// </summary>
        [CommandLineFunctionality.CommandLineArgumentFunction("force_destroy_disbanding_caravans", "gm.caravan")]
        public static string ForceDestroyDisbandingCaravans(List<string> args)
        {
            return Cmd.Run(args, () =>
            {
                // MARK: Validation
                if (!CommandValidator.ValidateCampaignState(out string error))
                    return error;

                string usageMessage = CommandValidator.CreateUsageMessage(
                    "gm.caravan.force_destroy_disbanding_caravans", "<confirm>",
                    "Destroys all disbanding caravans.\n" +
                    "- confirmation: Required. Must be 'confirm' to execute",
                    "gm.caravan.force_destroy_disbanding_caravans confirm");

                ParsedArguments parsed = ArgumentParser.ParseArguments(args);

                parsed.SetValidArguments(
                    new ArgumentDefinition("confirm", true)
                );

                if (parsed.TotalCount < 1)
                    return usageMessage;

                // MARK: Parse Arguments
                string confirmArg = parsed.GetArgument("confirm", 0);
                if (string.IsNullOrWhiteSpace(confirmArg) || confirmArg.ToLower().Trim() != "confirm")
                    return usageMessage;

                // MARK: Execute Logic
                Dictionary<string, string> resolvedValues = new()
                {
                    { "confirm", "confirm" }
                };

                int destroyed = CaravanManager.ForceDestroyDisbandingCaravans();

                string countsSummary = CaravanCommandHelpers.GetCaravanCountsSummary();

                string argumentDisplay = parsed.FormatArgumentDisplay("force_destroy_disbanding_caravans", resolvedValues);
                return argumentDisplay + MessageFormatter.FormatSuccessMessage(
                    $"Destroyed: {destroyed} caravans\n\n" +
                    $"Remaining Counts:\n{countsSummary}");
            });
        }
    }
}
