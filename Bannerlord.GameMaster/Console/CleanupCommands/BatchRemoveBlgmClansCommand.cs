using Bannerlord.GameMaster.Console.Common;
using Bannerlord.GameMaster.Console.Common.Execution;
using Bannerlord.GameMaster.Console.Common.Formatting;
using Bannerlord.GameMaster.Console.Common.Parsing;
using Bannerlord.GameMaster.Console.Common.Validation;
using Bannerlord.GameMaster.RemovalHelpers;
using System.Collections.Generic;
using TaleWorlds.Library;

namespace Bannerlord.GameMaster.Console.CleanupCommands;

public static class BatchRemoveBlgmClansCommand
{
    /// <summary>
    /// Removes multiple BLGM-generated clans
    /// Usage: gm.cleanup.batch_remove_blgm_clans [count]
    /// </summary>
    [CommandLineFunctionality.CommandLineArgumentFunction("batch_remove_blgm_clans", "gm.cleanup")]
    public static string BatchRemoveBlgmClans(List<string> args)
    {
        return Cmd.Run(args, () =>
        {
            // MARK: Validation
            if (!CommandValidator.ValidateCampaignState(out string error))
                return error;

            string usageMessage = CommandValidator.CreateUsageMessage(
                "gm.cleanup.batch_remove_blgm_clans", "[count]",
                "Removes multiple BLGM-generated clans.\n" +
                "- count: Number to remove (optional, removes all if not specified)",
                "gm.cleanup.batch_remove_blgm_clans 3\n" +
                "gm.cleanup.batch_remove_blgm_clans");

            ParsedArguments parsed = ArgumentParser.ParseArguments(args);

            parsed.SetValidArguments(
                new ArgumentDefinition("count", false)
            );

            string validationError = parsed.GetValidationError();
            if (validationError != null)
                return CommandResult.Error(MessageFormatter.FormatErrorMessage(validationError)).Log().Message;

            // MARK: Parse Arguments
            int? count = null;
            string resolvedCount = "All";

            if (parsed.TotalCount >= 1)
            {
                string countStr = parsed.GetArgument("count", 0);
                if (!CommandValidator.ValidateIntegerRange(countStr, 1, int.MaxValue, out int countValue, out string countError))
                {
                    return CommandResult.Error(MessageFormatter.FormatErrorMessage(countError)).Log().Message;
                }

                count = countValue;
                resolvedCount = count.Value.ToString();
            }

            // MARK: Execute Logic
            // NOTE: ClanRemover.BatchRemoveClans returns a tuple - consider refactoring to use a struct
            (int removed, string details) = ClanRemover.BatchRemoveClans(count);

            Dictionary<string, string> resolvedValues = new()
            {
                { "count", resolvedCount }
            };

            string argumentDisplay = parsed.FormatArgumentDisplay("gm.cleanup.batch_remove_blgm_clans", resolvedValues);
            string fullMessage = argumentDisplay + MessageFormatter.FormatSuccessMessage(
                $"Removed {removed} BLGM clan(s)\n{details}");
            return CommandResult.Success(fullMessage).Log().Message;
        });
    }
}
