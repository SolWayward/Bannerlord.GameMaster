using System;
using System.Collections.Generic;
using System.Text;
using Bannerlord.GameMaster.Bandits;
using Bannerlord.GameMaster.Console.Common;
using Bannerlord.GameMaster.Cultures;
using Bannerlord.GameMaster.Information;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Library;
using TaleWorlds.ObjectSystem;

namespace Bannerlord.GameMaster.Console.BanditCommands
{
    [CommandLineFunctionality.CommandLineArgumentFunction("bandit", "gm")]
    public static class BanditManagementCommands
    {
        #region Commands

        //MARK: count
        /// <summary>
        /// Display counts of all bandit parties and hideouts
        /// Usage: gm.bandit.count
        /// </summary>
        [CommandLineFunctionality.CommandLineArgumentFunction("count", "gm.bandit")]
        public static string Count(List<string> args)
        {
            return Cmd.Run(args, () =>
            {
                if (!CommandBase.ValidateCampaignMode(out string error))
                    return error;

                string usageMessage = CommandValidator.CreateUsageMessage(
                    "gm.bandit.count", "",
                    "Displays the count of all bandit parties and hideouts by type.",
                    "gm.bandit.count");

                return CommandBase.ExecuteWithErrorHandling(() =>
                {
                    string countsSummary = GetBanditCountsSummary();
                    return CommandBase.FormatSuccessMessage($"Bandit Statistics:\n{countsSummary}");
                }, "Failed to retrieve bandit counts");
            });
        }

        //MARK: destroy_bandit_parties
        /// <summary>
        /// Destroy bandit parties by type
        /// Usage: gm.bandit.destroy_bandit_parties &lt;banditType&gt; [count]
        /// Note: If all bandit parties linked to a hideout are destroyed the hideout is also considered cleared by the game
        /// </summary>
        [CommandLineFunctionality.CommandLineArgumentFunction("destroy_bandit_parties", "gm.bandit")]
        public static string DestroyBanditParties(List<string> args)
        {
            return Cmd.Run(args, () =>
            {
                if (!CommandBase.ValidateCampaignMode(out string error))
                    return error;

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

                // Parse arguments
                CommandBase.ParsedArguments parsedArgs = CommandBase.ParseArguments(args);

                // Define valid arguments
                parsedArgs.SetValidArguments(
                    new CommandBase.ArgumentDefinition("banditType", true, null, "type"),
                    new CommandBase.ArgumentDefinition("count", false)
                );

                // Validate
                string validationError = parsedArgs.GetValidationError();
                if (validationError != null)
                    return CommandBase.FormatErrorMessage(validationError);

                if (parsedArgs.TotalCount < 1)
                    return usageMessage;

                // Parse banditType
                string banditTypeArg = parsedArgs.GetArgument("banditType", 0) ?? parsedArgs.GetNamed("type");
                if (banditTypeArg == null)
                    return CommandBase.FormatErrorMessage("Missing required argument 'banditType'.");

                // Parse optional count
                int? count = null;
                string countArg = parsedArgs.GetArgument("count", 1);
                if (countArg != null)
                {
                    if (!CommandValidator.ValidateIntegerRange(countArg, 1, int.MaxValue, out int countValue, out string countError))
                        return CommandBase.FormatErrorMessage(countError);
                    count = countValue;
                }

                // Parse bandit cultures
                (List<CultureObject> cultures, string parseError) = ParseBanditCultures(banditTypeArg);
                if (parseError != null)
                    return CommandBase.FormatErrorMessage(parseError);

                // Build display
                Dictionary<string, string> resolvedValues = new()
                {
                    { "banditType", FormatBanditTypeList(cultures) },
                    { "count", count.HasValue ? count.Value.ToString() : "All" }
                };
                string argumentDisplay = parsedArgs.FormatArgumentDisplay("destroy_bandit_parties", resolvedValues);

                return CommandBase.ExecuteWithErrorHandling(() =>
                {
                    int totalRemoved = 0;

                    for (int i = 0; i < cultures.Count; i++)
                    {
                        int removed = BanditManager.RemoveBanditPartiesByCulture(cultures[i], count);
                        totalRemoved += removed;
                    }

                    string countInfo = count.HasValue ? $" (requested: {count.Value})" : " (all)";
                    string countsSummary = GetBanditCountsSummary();
                    return argumentDisplay + CommandBase.FormatSuccessMessage(
                        $"Destroyed {totalRemoved} bandit parties{countInfo}.\n" +
                        $"Types: {FormatBanditTypeList(cultures)}\n\n" +
                        $"Remaining Counts:\n{countsSummary}");
                }, "Failed to destroy bandit parties");
            });
        }

        //MARK: clear_hideouts
        /// <summary>
        /// Clear bandit hideouts by type
        /// Usage: gm.bandit.clear_hideouts &lt;banditType&gt; [count]
        /// </summary>
        [CommandLineFunctionality.CommandLineArgumentFunction("clear_hideouts", "gm.bandit")]
        public static string ClearHideouts(List<string> args)
        {
            return Cmd.Run(args, () =>
            {
                if (!CommandBase.ValidateCampaignMode(out string error))
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

                // Parse arguments
                CommandBase.ParsedArguments parsedArgs = CommandBase.ParseArguments(args);

                // Define valid arguments
                parsedArgs.SetValidArguments(
                    new CommandBase.ArgumentDefinition("banditType", true, null, "type"),
                    new CommandBase.ArgumentDefinition("count", false)
                );

                // Validate
                string validationError = parsedArgs.GetValidationError();
                if (validationError != null)
                    return CommandBase.FormatErrorMessage(validationError);

                if (parsedArgs.TotalCount < 1)
                    return usageMessage;

                // Parse banditType
                string banditTypeArg = parsedArgs.GetArgument("banditType", 0) ?? parsedArgs.GetNamed("type");
                if (banditTypeArg == null)
                    return CommandBase.FormatErrorMessage("Missing required argument 'banditType'.");

                // Parse optional count
                int? count = null;
                string countArg = parsedArgs.GetArgument("count", 1);
                if (countArg != null)
                {
                    if (!CommandValidator.ValidateIntegerRange(countArg, 1, int.MaxValue, out int countValue, out string countError))
                        return CommandBase.FormatErrorMessage(countError);
                    count = countValue;
                }

                // Parse bandit cultures
                (List<CultureObject> cultures, string parseError) = ParseBanditCultures(banditTypeArg);
                if (parseError != null)
                    return CommandBase.FormatErrorMessage(parseError);

                // Build display
                Dictionary<string, string> resolvedValues = new()
                {
                    { "banditType", FormatBanditTypeList(cultures) },
                    { "count", count.HasValue ? count.Value.ToString() : "All" }
                };
                string argumentDisplay = parsedArgs.FormatArgumentDisplay("clear_hideouts", resolvedValues);

                return CommandBase.ExecuteWithErrorHandling(() =>
                {
                    int totalRemoved = 0;

                    for (int i = 0; i < cultures.Count; i++)
                    {
                        int removed = BanditManager.RemoveHideoutsByCulture(cultures[i], count);
                        totalRemoved += removed;
                    }

                    string countInfo = count.HasValue ? $" (requested: {count.Value})" : " (all)";
                    string countsSummary = GetBanditCountsSummary();
                    return argumentDisplay + CommandBase.FormatSuccessMessage(
                        $"Cleared {totalRemoved} bandit hideouts{countInfo}.\n" +
                        $"Types: {FormatBanditTypeList(cultures)}\n\n" +
                        $"Remaining Counts:\n{countsSummary}");
                }, "Failed to clear hideouts");
            });
        }

        //MARK: remove_all
        /// <summary>
        /// Remove all bandit parties and hideouts
        /// Usage: gm.bandit.remove_all &lt;confirmation&gt;
        /// </summary>
        [CommandLineFunctionality.CommandLineArgumentFunction("remove_all", "gm.bandit")]
        public static string RemoveAll(List<string> args)
        {
            return Cmd.Run(args, () =>
            {
                if (!CommandBase.ValidateCampaignMode(out string error))
                    return error;

                string usageMessage = CommandValidator.CreateUsageMessage(
                    "gm.bandit.remove_all", "<confirmation>",
                    "Removes ALL bandit parties AND ALL bandit hideouts from the game.\n" +
                    "WARNING: This is a destructive operation that cannot be undone!\n" +
                    "- confirmation: Required. Must be 'confirm' to execute\n" +
                    "Supports named arguments: confirmation:confirm",
                    "gm.bandit.remove_all confirm");

                // Parse arguments
                CommandBase.ParsedArguments parsedArgs = CommandBase.ParseArguments(args);

                // Define valid arguments
                parsedArgs.SetValidArguments(
                    new CommandBase.ArgumentDefinition("confirmation", true)
                );

                // Validate
                string validationError = parsedArgs.GetValidationError();
                if (validationError != null)
                    return CommandBase.FormatErrorMessage(validationError);

                if (parsedArgs.TotalCount < 1)
                    return usageMessage;

                // Parse confirmation
                string confirmationArg = parsedArgs.GetArgument("confirmation", 0);
                if (confirmationArg == null)
                    return CommandBase.FormatErrorMessage("Missing required argument 'confirmation'.");

                if (confirmationArg.ToLower() != "confirm")
                    return CommandBase.FormatErrorMessage(
                        $"Invalid confirmation value: '{confirmationArg}'. Must be 'confirm' to execute this command.");

                // Build display
                Dictionary<string, string> resolvedValues = new()
                {
                    { "confirmation", "confirm" }
                };
                string argumentDisplay = parsedArgs.FormatArgumentDisplay("remove_all", resolvedValues);

                return CommandBase.ExecuteWithErrorHandling(() =>
                {
                    int partiesRemoved = BanditManager.RemoveAllBanditParties(null);
                    int hideoutsRemoved = BanditManager.RemoveAllHideouts(null);

                    string countsSummary = GetBanditCountsSummary();
                    return argumentDisplay + CommandBase.FormatSuccessMessage(
                        $"Removed all bandits from the game.\n" +
                        $"Parties destroyed: {partiesRemoved}\n" +
                        $"Hideouts cleared: {hideoutsRemoved}\n\n" +
                        $"Remaining Counts:\n{countsSummary}");
                }, "Failed to remove all bandits");
            });
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Parses bandit type argument and returns list of matching bandit cultures.
        /// Supports 'all', comma-separated list, culture names, IDs, and flexible naming.
        /// </summary>
        private static (List<CultureObject> cultures, string error) ParseBanditCultures(string banditTypeArg)
        {
            List<CultureObject> cultures = new();

            // Handle "all" keyword
            if (banditTypeArg.Equals("all", StringComparison.OrdinalIgnoreCase))
            {
                cultures.AddRange(CultureLookup.BanditCultures);
                return (cultures, null);
            }

            // Split by comma for multiple types
            string[] types = banditTypeArg.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

            for (int i = 0; i < types.Length; i++)
            {
                string type = types[i].Trim();
                CultureObject culture = ParseSingleBanditType(type);

                if (culture == null)
                {
                    return (null, $"Invalid bandit type: '{type}'. Valid types: all, looters, deserters/desert, " +
                        "forest/forest_bandits, mountain/mountain_bandits, sea_raiders/sea, steppe/steppe_bandits, " +
                        "corsairs/southern_pirates");
                }

                // Avoid duplicates
                if (!cultures.Contains(culture))
                {
                    cultures.Add(culture);
                }
            }

            if (cultures.Count == 0)
            {
                return (null, "No valid bandit types specified.");
            }

            return (cultures, null);
        }

        /// <summary>
        /// Parses a single bandit type string and returns the matching CultureObject.
        /// Supports culture names, IDs, and flexible/shortened names.
        /// </summary>
        private static CultureObject ParseSingleBanditType(string type)
        {
            string lowerType = type.ToLower();

            // Try exact match first using MBObjectManager
            CultureObject culture = MBObjectManager.Instance.GetObject<CultureObject>(lowerType);
            if (culture != null && culture.IsBandit)
                return culture;

            // Map common names and shortcuts to bandit cultures
            return lowerType switch
            {
                "looters" or "looter" => CultureLookup.Looters,
                "deserters" or "deserter" or "desert" or "desert_bandits" => CultureLookup.Deserters,
                "forest" or "forest_bandits" or "forestbandits" => CultureLookup.ForestBandits,
                "mountain" or "mountain_bandits" or "mountainbandits" => CultureLookup.MountainBandits,
                "sea" or "sea_raiders" or "searaiders" => CultureLookup.SeaRaiders,
                "steppe" or "steppe_bandits" or "steppebandits" => CultureLookup.SteppeBandits,
                "corsairs" or "corsair" or "southern_pirates" or "pirates" => CultureLookup.Corsairs,
                _ => null
            };
        }

        /// <summary>
        /// Formats a list of bandit cultures into a readable string.
        /// </summary>
        private static string FormatBanditTypeList(List<CultureObject> cultures)
        {
            if (cultures.Count == 0)
                return "None";

            StringBuilder sb = new();
            for (int i = 0; i < cultures.Count; i++)
            {
                if (i > 0)
                    sb.Append(", ");
                sb.Append(cultures[i].Name.ToString());
            }
            return sb.ToString();
        }

        /// <summary>
        /// Gets a formatted summary of all bandit party and hideout counts.
        /// </summary>
        private static string GetBanditCountsSummary()
        {
            StringBuilder sb = new();
            
            // Total counts
            int totalParties = BanditManager.TotalBanditPartyCount;
            int totalHideouts = BanditManager.TotalHideoutCount;
            
            sb.AppendLine($"Total Parties: {totalParties}");
            sb.AppendLine($"Total Hideouts: {totalHideouts}");
            sb.AppendLine();
            
            // Individual counts by type
            sb.AppendLine("By Type:");
            AppendBanditTypeCounts(sb, "Looters", BanditManager.LootersPartyCount, BanditManager.LooterHideoutCount);
            AppendBanditTypeCounts(sb, "Deserters", BanditManager.DeserterPartyCount, BanditManager.DeserterHideoutCount);
            AppendBanditTypeCounts(sb, "Forest Bandits", BanditManager.ForestBanditPartyCount, BanditManager.ForestBanditHideoutCount);
            AppendBanditTypeCounts(sb, "Mountain Bandits", BanditManager.MountainBanditPartyCount, BanditManager.MountainBanditHideoutCount);
            AppendBanditTypeCounts(sb, "Sea Raiders", BanditManager.SeaRaiderPartyCount, BanditManager.SeaRaiderHideoutCount);
            AppendBanditTypeCounts(sb, "Steppe Bandits", BanditManager.SteppeBanditPartyCount, BanditManager.SteppeBanditHideoutCount);
            
            // Only include Corsairs if Warsails DLC is loaded
            if (GameEnvironment.IsWarsailsDlcLoaded)
            {
                AppendBanditTypeCounts(sb, "Corsairs", BanditManager.CorsairPartyCount, BanditManager.CorsairHideoutCount);
            }
            
            return sb.ToString().TrimEnd();
        }

        /// <summary>
        /// Appends formatted count information for a specific bandit type.
        /// </summary>
        private static void AppendBanditTypeCounts(StringBuilder sb, string typeName, int partyCount, int hideoutCount)
        {
            sb.AppendLine($"  {typeName}: {partyCount} parties, {hideoutCount} hideouts");
        }

        #endregion
    }
}
