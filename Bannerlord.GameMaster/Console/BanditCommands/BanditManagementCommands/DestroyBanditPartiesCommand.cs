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
    /// Console command to destroy bandit parties by type.
    /// Usage: gm.bandit.destroy_bandit_parties &lt;banditType&gt; [count]
    /// Note: If all bandit parties linked to a hideout are destroyed the hideout is also considered cleared by the game.
    /// </summary>
    [CommandLineFunctionality.CommandLineArgumentFunction("bandit", "gm")]
    public static class DestroyBanditPartiesCommand
    {
        /// <summary>
        /// Destroy bandit parties by type.
        /// Usage: gm.bandit.destroy_bandit_parties &lt;banditType&gt; [count]
        /// Note: If all bandit parties linked to a hideout are destroyed the hideout is also considered cleared by the game.
        /// </summary>
        [CommandLineFunctionality.CommandLineArgumentFunction("destroy_bandit_parties", "gm.bandit")]
        public static string DestroyBanditParties(List<string> args)
        {
            return Cmd.Run(args, () =>
            {
                // MARK: Validation
                if (!CommandValidator.ValidateCampaignState(out string error))
                    return CommandResult.Error(error).Message
;

                string usageMessage = CommandValidator.CreateUsageMessage(
                    "gm.bandit.destroy_bandit_parties", "<banditType> [count]",
                    "Destroys bandit parties of the specified type(s). If count is omitted, removes ALL matching parties.\n" +
                    "Note: If all bandit parties linked to a hideout are destroyed the hideout is also considered cleared by the game.\n" +
                    "- banditType/type: Required. Use 'all', comma-separated types, or single type\n" +
                    "  Valid types: looters, deserters/desert, forest/forest_bandits, mountain/mountain_bandits,\n" +
                    "               sea_raiders/sea, steppe/steppe_bandits, corsairs/southern_pirates\n" +
                    "- count: Optional. Number of parties to remove (omit to remove all)\n" +
                    "Supports named arguments: banditType:looters,forest count:5",
                    "gm.bandit.destroy_bandit_parties all\n" +
                    "gm.bandit.destroy_bandit_parties looters 10\n" +
                    "gm.bandit.destroy_bandit_parties looters,forest,mountain\n" +
                    "gm.bandit.destroy_bandit_parties type:sea_raiders count:5");

                ParsedArguments parsed = ArgumentParser.ParseArguments(args);

                parsed.SetValidArguments(
                    new ArgumentDefinition("banditType", true, null, "type"),
                    new ArgumentDefinition("count", false)
                );

                string validationError = parsed.GetValidationError();
                if (validationError != null)
                    return CommandResult.Error(MessageFormatter.FormatErrorMessage(validationError)).Message
;

                if (parsed.TotalCount < 1)
                    return CommandResult.Error(usageMessage).Message
;

                // MARK: Parse Arguments
                string banditTypeArg = parsed.GetArgument("banditType", 0) ?? parsed.GetArgument("type", 0);
                if (banditTypeArg == null)
                    return CommandResult.Error(MessageFormatter.FormatErrorMessage("Missing required argument 'banditType'.")).Message
;

                int? count = null;
                string countArg = parsed.GetArgument("count", 1);
                if (countArg != null)
                {
                    if (!CommandValidator.ValidateIntegerRange(countArg, 1, int.MaxValue, out int countValue, out string countError))
                        return CommandResult.Error(MessageFormatter.FormatErrorMessage(countError)).Message
;
                    count = countValue;
                }

                (List<CultureObject> cultures, string parseError) = BanditCommandHelpers.ParseBanditCultures(banditTypeArg);
                if (parseError != null)
                    return CommandResult.Error(MessageFormatter.FormatErrorMessage(parseError)).Message
;

                // MARK: Execute Logic
                Dictionary<string, string> resolvedValues = new()
                {
                    { "banditType", BanditCommandHelpers.FormatBanditTypeList(cultures) },
                    { "count", count.HasValue ? count.Value.ToString() : "All" }
                };

                int totalRemoved = 0;

                for (int i = 0; i < cultures.Count; i++)
                {
                    int removed = BanditManager.RemoveBanditPartiesByCulture(cultures[i], count);
                    totalRemoved += removed;
                }

                string countInfo = count.HasValue ? $" (requested: {count.Value})" : " (all)";
                string countsSummary = BanditCommandHelpers.GetBanditCountsSummary();

                string argumentDisplay = parsed.FormatArgumentDisplay("gm.bandit.destroy_bandit_parties", resolvedValues);
                string fullMessage = argumentDisplay + MessageFormatter.FormatSuccessMessage(
                    $"Destroyed {totalRemoved} bandit parties{countInfo}.\n" +
                    $"Types: {BanditCommandHelpers.FormatBanditTypeList(cultures)}\n\n" +
                    $"Remaining Counts:\n{countsSummary}");
                return CommandResult.Success(fullMessage).Message
;
            });
        }
    }
}
