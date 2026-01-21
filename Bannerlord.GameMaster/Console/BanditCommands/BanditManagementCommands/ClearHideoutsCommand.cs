using System.Collections.Generic;
using Bannerlord.GameMaster.Bandits;
using Bannerlord.GameMaster.Console.Common;
using Bannerlord.GameMaster.Console.Common.Execution;
using Bannerlord.GameMaster.Console.Common.Formatting;
using Bannerlord.GameMaster.Console.Common.Parsing;
using Bannerlord.GameMaster.Console.Common.Validation;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Library;

namespace Bannerlord.GameMaster.Console.BanditCommands.BanditManagementCommands
{
    /// <summary>
    /// Console command to clear bandit hideouts by type.
    /// Usage: gm.bandit.clear_hideouts &lt;banditType&gt; [count]
    /// </summary>
    [CommandLineFunctionality.CommandLineArgumentFunction("bandit", "gm")]
    public static class ClearHideoutsCommand
    {
        /// <summary>
        /// Clear bandit hideouts by type.
        /// Usage: gm.bandit.clear_hideouts &lt;banditType&gt; [count]
        /// </summary>
        [CommandLineFunctionality.CommandLineArgumentFunction("clear_hideouts", "gm.bandit")]
        public static string ClearHideouts(List<string> args)
        {
            return Cmd.Run(args, () =>
            {
                // MARK: Validation
                if (!CommandValidator.ValidateCampaignState(out string error))
                    return error;

                string usageMessage = CommandValidator.CreateUsageMessage(
                    "gm.bandit.clear_hideouts", "<banditType> [count]",
                    "Clears bandit hideouts of the specified type(s). If count is omitted, removes ALL matching hideouts.\n" +
                    "- banditType/type: Required. Use 'all', comma-separated types, or single type\n" +
                    "  Valid types: looters, deserters/desert, forest/forest_bandits, mountain/mountain_bandits,\n" +
                    "               sea_raiders/sea, steppe/steppe_bandits, corsairs/southern_pirates\n" +
                    "- count: Optional. Number of hideouts to remove (omit to remove all)\n" +
                    "Supports named arguments: banditType:forest,mountain count:3",
                    "gm.bandit.clear_hideouts all\n" +
                    "gm.bandit.clear_hideouts forest 2\n" +
                    "gm.bandit.clear_hideouts mountain,sea_raiders\n" +
                    "gm.bandit.clear_hideouts type:steppe count:1");

                ParsedArguments parsed = ArgumentParser.ParseArguments(args);

                parsed.SetValidArguments(
                    new ArgumentDefinition("banditType", true, null, "type"),
                    new ArgumentDefinition("count", false)
                );

                string validationError = parsed.GetValidationError();
                if (validationError != null)
                    return CommandResult.Error(MessageFormatter.FormatErrorMessage(validationError)).Log().Message;

                if (parsed.TotalCount < 1)
                    return CommandResult.Error(usageMessage).Log().Message;

                // MARK: Parse Arguments
                string banditTypeArg = parsed.GetArgument("banditType", 0) ?? parsed.GetArgument("type", 0);
                if (banditTypeArg == null)
                    return CommandResult.Error(MessageFormatter.FormatErrorMessage("Missing required argument 'banditType'.")).Log().Message;

                int? count = null;
                string countArg = parsed.GetArgument("count", 1);
                if (countArg != null)
                {
                    if (!CommandValidator.ValidateIntegerRange(countArg, 1, int.MaxValue, out int countValue, out string countError))
                        return CommandResult.Error(MessageFormatter.FormatErrorMessage(countError)).Log().Message;
                    count = countValue;
                }

                (List<CultureObject> cultures, string parseError) = BanditCommandHelpers.ParseBanditCultures(banditTypeArg);
                if (parseError != null)
                    return CommandResult.Error(MessageFormatter.FormatErrorMessage(parseError)).Log().Message;

                // MARK: Execute Logic
                Dictionary<string, string> resolvedValues = new()
                {
                    { "banditType", BanditCommandHelpers.FormatBanditTypeList(cultures) },
                    { "count", count.HasValue ? count.Value.ToString() : "All" }
                };

                int totalRemoved = 0;

                for (int i = 0; i < cultures.Count; i++)
                {
                    int removed = BanditManager.RemoveHideoutsByCulture(cultures[i], count);
                    totalRemoved += removed;
                }

                string countInfo = count.HasValue ? $" (requested: {count.Value})" : " (all)";
                string countsSummary = BanditCommandHelpers.GetBanditCountsSummary();

                string argumentDisplay = parsed.FormatArgumentDisplay("clear_hideouts", resolvedValues);
                string fullMessage = argumentDisplay + MessageFormatter.FormatSuccessMessage(
                    $"Cleared {totalRemoved} bandit hideouts{countInfo}.\n" +
                    $"Types: {BanditCommandHelpers.FormatBanditTypeList(cultures)}\n\n" +
                    $"Remaining Counts:\n{countsSummary}");
                return CommandResult.Success(fullMessage).Log().Message;
            });
        }
    }
}
