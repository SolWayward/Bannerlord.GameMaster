using Bannerlord.GameMaster.Console.Common;
using Bannerlord.GameMaster.Console.Common.Execution;
using Bannerlord.GameMaster.Console.Common.Formatting;
using Bannerlord.GameMaster.Console.Common.Parsing;
using Bannerlord.GameMaster.Console.Common.Validation;
using Bannerlord.GameMaster.RemovalHelpers;
using System.Collections.Generic;
using TaleWorlds.Library;

namespace Bannerlord.GameMaster.Console.CleanupCommands;

public static class BatchRemoveHeroesCommand
{
    /// <summary>
    /// Removes multiple BLGM-generated heroes
    /// Usage: gm.cleanup.batch_remove_heroes [count]
    /// </summary>
    [CommandLineFunctionality.CommandLineArgumentFunction("batch_remove_heroes", "gm.cleanup")]
    public static string BatchRemoveHeroes(List<string> args)
    {
        return Cmd.Run(args, () =>
        {
            // MARK: Validation
            if (!CommandValidator.ValidateCampaignState(out string error))
                return CommandResult.Error(error).Log().Message;

            string usageMessage = CommandValidator.CreateUsageMessage(
                "gm.cleanup.batch_remove_heroes", "[count]",
                "Removes multiple BLGM-generated heroes.\n" +
                "- count: Number to remove (optional, removes all if not specified)",
                "gm.cleanup.batch_remove_heroes 5\n" +
                "gm.cleanup.batch_remove_heroes");

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
            // NOTE: HeroRemover.BatchRemoveHeroes returns a tuple - consider refactoring to use a struct
            (int removed, string details) = HeroRemover.BatchRemoveHeroes(count);

            Dictionary<string, string> resolvedValues = new()
            {
                { "count", resolvedCount }
            };

            string argumentDisplay = parsed.FormatArgumentDisplay("gm.cleanup.batch_remove_heroes", resolvedValues);
            string fullMessage = argumentDisplay + MessageFormatter.FormatSuccessMessage(
                $"Removed {removed} BLGM hero(es)\n{details}");
            return CommandResult.Success(fullMessage).Log().Message;
        });
    }
}
