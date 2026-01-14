using System;
using System.Collections.Generic;
using System.Text;
using Bannerlord.GameMaster.Caravans;
using Bannerlord.GameMaster.Console.Common;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Library;

namespace Bannerlord.GameMaster.Console.CaravanCommands
{
    [CommandLineFunctionality.CommandLineArgumentFunction("caravan", "gm")]
    public static class CaravanManagementCommands
    {
        #region Commands

        //MARK: count
        /// <summary>
        /// Display counts of all caravans by owner type
        /// Usage: gm.caravan.count
        /// </summary>
        [CommandLineFunctionality.CommandLineArgumentFunction("count", "gm.caravan")]
        public static string Count(List<string> args)
        {
            return Cmd.Run(args, () =>
            {
                if (!CommandBase.ValidateCampaignState(out string error))
                    return error;

                string usageMessage = CommandValidator.CreateUsageMessage(
                    "gm.caravan.count", "",
                    "Displays the count of all caravans by owner type.",
                    "gm.caravan.count");

                return CommandBase.ExecuteWithErrorHandling(() =>
                {
                    string countsSummary = GetCaravanCountsSummary();
                    return CommandBase.FormatSuccessMessage($"Caravan Statistics:\n{countsSummary}");
                }, "Failed to retrieve caravan counts");
            });
        }

        //MARK: disband_caravans
        /// <summary>
        /// Disband caravans from all caravans
        /// Usage: gm.caravan.disband_caravans &lt;count&gt;
        /// </summary>
        [CommandLineFunctionality.CommandLineArgumentFunction("disband_caravans", "gm.caravan")]
        public static string DisbandCaravans(List<string> args)
        {
            return Cmd.Run(args, () =>
            {
                if (!CommandBase.ValidateCampaignState(out string error))
                    return error;

                string usageMessage = CommandValidator.CreateUsageMessage(
                    "gm.caravan.disband_caravans", "<count>",
                    "Disbands the specified number of random caravans from ALL caravans.\n" +
                    "- count: Required. Number of caravans to disband, or 'all' to disband all\n" +
                    "Supports named arguments: count:10",
                    "gm.caravan.disband_caravans all\n" +
                    "gm.caravan.disband_caravans 5\n" +
                    "gm.caravan.disband_caravans count:10");

                // Parse arguments
                CommandBase.ParsedArguments parsedArgs = CommandBase.ParseArguments(args);

                // Define valid arguments
                parsedArgs.SetValidArguments(
                    new CommandBase.ArgumentDefinition("count", true)
                );

                // Validate
                string validationError = parsedArgs.GetValidationError();
                if (validationError != null)
                    return CommandBase.FormatErrorMessage(validationError);

                if (parsedArgs.TotalCount < 1)
                    return usageMessage;

                // Parse count
                string countArg = parsedArgs.GetArgument("count", 0);
                if (countArg == null)
                    return CommandBase.FormatErrorMessage("Missing required argument 'count'.");

                int? count = ParseCountArgument(countArg, out string parseError);
                if (parseError != null)
                    return CommandBase.FormatErrorMessage(parseError);

                // Build display
                Dictionary<string, string> resolvedValues = new()
                {
                    { "count", countArg.ToLower() == "all" ? "All" : count.Value.ToString() }
                };
                string argumentDisplay = parsedArgs.FormatArgumentDisplay("disband_caravans", resolvedValues);

                return CommandBase.ExecuteWithErrorHandling(() =>
                {
                    int disbanded = CaravanManager.DisbandCaravans(count);

                    string countInfo = count.HasValue ? $" (requested: {count.Value})" : " (all)";
                    string countsSummary = GetCaravanCountsSummary();
                    return argumentDisplay + CommandBase.FormatSuccessMessage(
                        $"Disbanded {disbanded} caravans{countInfo}.\n\n" +
                        $"Remaining Counts:\n{countsSummary}");
                }, "Failed to disband caravans");
            });
        }

        //MARK: disband_player
        /// <summary>
        /// Disband player caravans
        /// Usage: gm.caravan.disband_player_caravans &lt;count&gt;
        /// </summary>
        [CommandLineFunctionality.CommandLineArgumentFunction("disband_player_caravans", "gm.caravan")]
        public static string DisbandPlayerCaravans(List<string> args)
        {
            return Cmd.Run(args, () =>
            {
                if (!CommandBase.ValidateCampaignState(out string error))
                    return error;

                string usageMessage = CommandValidator.CreateUsageMessage(
                    "gm.caravan.disband_player_caravans", "<count>",
                    "Disbands the specified number of player caravans.\n" +
                    "- count: Required. Number of caravans to disband, or 'all' to disband all\n" +
                    "Supports named arguments: count:5",
                    "gm.caravan.disband_player_caravans all\n" +
                    "gm.caravan.disband_player_caravans 3\n" +
                    "gm.caravan.disband_player_caravans count:2");

                // Parse arguments
                CommandBase.ParsedArguments parsedArgs = CommandBase.ParseArguments(args);

                // Define valid arguments
                parsedArgs.SetValidArguments(
                    new CommandBase.ArgumentDefinition("count", true)
                );

                // Validate
                string validationError = parsedArgs.GetValidationError();
                if (validationError != null)
                    return CommandBase.FormatErrorMessage(validationError);

                if (parsedArgs.TotalCount < 1)
                    return usageMessage;

                // Parse count
                string countArg = parsedArgs.GetArgument("count", 0);
                if (countArg == null)
                    return CommandBase.FormatErrorMessage("Missing required argument 'count'.");

                int? count = ParseCountArgument(countArg, out string parseError);
                if (parseError != null)
                    return CommandBase.FormatErrorMessage(parseError);

                // Build display
                Dictionary<string, string> resolvedValues = new()
                {
                    { "count", countArg.ToLower() == "all" ? "All" : count.Value.ToString() }
                };
                string argumentDisplay = parsedArgs.FormatArgumentDisplay("disband_player_caravans", resolvedValues);

                return CommandBase.ExecuteWithErrorHandling(() =>
                {
                    int disbanded = CaravanManager.DisbandPlayerCaravans(count);

                    string countInfo = count.HasValue ? $" (requested: {count.Value})" : " (all)";
                    string countsSummary = GetCaravanCountsSummary();
                    return argumentDisplay + CommandBase.FormatSuccessMessage(
                        $"Disbanded {disbanded} player caravans{countInfo}.\n\n" +
                        $"Remaining Counts:\n{countsSummary}");
                }, "Failed to disband player caravans");
            });
        }

        //MARK: disband_notable
        /// <summary>
        /// Disband notable caravans
        /// Usage: gm.caravan.disband_notable_caravans &lt;count&gt;
        /// </summary>
        [CommandLineFunctionality.CommandLineArgumentFunction("disband_notable_caravans", "gm.caravan")]
        public static string DisbandNotableCaravans(List<string> args)
        {
            return Cmd.Run(args, () =>
            {
                if (!CommandBase.ValidateCampaignState(out string error))
                    return error;

                string usageMessage = CommandValidator.CreateUsageMessage(
                    "gm.caravan.disband_notable_caravans", "<count>",
                    "Disbands the specified number of notable caravans.\n" +
                    "- count: Required. Number of caravans to disband, or 'all' to disband all\n" +
                    "Supports named arguments: count:8",
                    "gm.caravan.disband_notable_caravans all\n" +
                    "gm.caravan.disband_notable_caravans 5\n" +
                    "gm.caravan.disband_notable_caravans count:3");

                // Parse arguments
                CommandBase.ParsedArguments parsedArgs = CommandBase.ParseArguments(args);

                // Define valid arguments
                parsedArgs.SetValidArguments(
                    new CommandBase.ArgumentDefinition("count", true)
                );

                // Validate
                string validationError = parsedArgs.GetValidationError();
                if (validationError != null)
                    return CommandBase.FormatErrorMessage(validationError);

                if (parsedArgs.TotalCount < 1)
                    return usageMessage;

                // Parse count
                string countArg = parsedArgs.GetArgument("count", 0);
                if (countArg == null)
                    return CommandBase.FormatErrorMessage("Missing required argument 'count'.");

                int? count = ParseCountArgument(countArg, out string parseError);
                if (parseError != null)
                    return CommandBase.FormatErrorMessage(parseError);

                // Build display
                Dictionary<string, string> resolvedValues = new()
                {
                    { "count", countArg.ToLower() == "all" ? "All" : count.Value.ToString() }
                };
                string argumentDisplay = parsedArgs.FormatArgumentDisplay("disband_notable_caravans", resolvedValues);

                return CommandBase.ExecuteWithErrorHandling(() =>
                {
                    int disbanded = CaravanManager.DisbandNotableCaravans(count);

                    string countInfo = count.HasValue ? $" (requested: {count.Value})" : " (all)";
                    string countsSummary = GetCaravanCountsSummary();
                    return argumentDisplay + CommandBase.FormatSuccessMessage(
                        $"Disbanded {disbanded} notable caravans{countInfo}.\n\n" +
                        $"Remaining Counts:\n{countsSummary}");
                }, "Failed to disband notable caravans");
            });
        }

        //MARK: disband_npc_lord
        /// <summary>
        /// Disband NPC Lord caravans
        /// Usage: gm.caravan.disband_npc_lord_caravans &lt;count&gt;
        /// </summary>
        [CommandLineFunctionality.CommandLineArgumentFunction("disband_npc_lord_caravans", "gm.caravan")]
        public static string DisbandNPCLordCaravans(List<string> args)
        {
            return Cmd.Run(args, () =>
            {
                if (!CommandBase.ValidateCampaignState(out string error))
                    return error;

                string usageMessage = CommandValidator.CreateUsageMessage(
                    "gm.caravan.disband_npc_lord_caravans", "<count>",
                    "Disbands the specified number of NPC Lord caravans.\n" +
                    "- count: Required. Number of caravans to disband, or 'all' to disband all\n" +
                    "Supports named arguments: count:7",
                    "gm.caravan.disband_npc_lord_caravans all\n" +
                    "gm.caravan.disband_npc_lord_caravans 4\n" +
                    "gm.caravan.disband_npc_lord_caravans count:6");

                // Parse arguments
                CommandBase.ParsedArguments parsedArgs = CommandBase.ParseArguments(args);

                // Define valid arguments
                parsedArgs.SetValidArguments(
                    new CommandBase.ArgumentDefinition("count", true)
                );

                // Validate
                string validationError = parsedArgs.GetValidationError();
                if (validationError != null)
                    return CommandBase.FormatErrorMessage(validationError);

                if (parsedArgs.TotalCount < 1)
                    return usageMessage;

                // Parse count
                string countArg = parsedArgs.GetArgument("count", 0);
                if (countArg == null)
                    return CommandBase.FormatErrorMessage("Missing required argument 'count'.");

                int? count = ParseCountArgument(countArg, out string parseError);
                if (parseError != null)
                    return CommandBase.FormatErrorMessage(parseError);

                // Build display
                Dictionary<string, string> resolvedValues = new()
                {
                    { "count", countArg.ToLower() == "all" ? "All" : count.Value.ToString() }
                };
                string argumentDisplay = parsedArgs.FormatArgumentDisplay("disband_npc_lord_caravans", resolvedValues);

                return CommandBase.ExecuteWithErrorHandling(() =>
                {
                    int disbanded = CaravanManager.DisbandNPCLordCaravans(count);

                    string countInfo = count.HasValue ? $" (requested: {count.Value})" : " (all)";
                    string countsSummary = GetCaravanCountsSummary();
                    return argumentDisplay + CommandBase.FormatSuccessMessage(
                        $"Disbanded {disbanded} NPC Lord caravans{countInfo}.\n\n" +
                        $"Remaining Counts:\n{countsSummary}");
                }, "Failed to disband NPC Lord caravans");
            });
        }

        //MARK: destroy disbanding
        /// <summary>
        /// Disband caravans from all caravans
        /// Usage: gm.caravan.disband_caravans &lt;count&gt;
        /// </summary>
        [CommandLineFunctionality.CommandLineArgumentFunction("force_destroy_disbanding_caravans", "gm.caravan")]
        public static string DestroyDisbandingCaravans(List<string> args)
        {
            return Cmd.Run(args, () =>
            {
                if (!CommandBase.ValidateCampaignState(out string error))
                    return error;

                string usageMessage = CommandValidator.CreateUsageMessage(
                    "force_destroy_disbanding_caravans", "<confirm>",
                    "Destroys all disbanding caravans.\n" +
                     "- confirmation: Required. Must be 'confirm' to execute\n",
                    "force_destroy_disbanding_caravans confirm");

                // Require confirm
                if (args.Count < 1 || args[0].ToLower().Trim() != "confirm")
                    return usageMessage;

                return CommandBase.ExecuteWithErrorHandling(() =>
                {
                    int destroyed = CaravanManager.ForceDestroyDisbandingCaravans();

                    string countsSummary = GetCaravanCountsSummary();
                    return "gm.caravan.force_destroy_disbanding_caravans confirm\n" + CommandBase.FormatSuccessMessage(
                        $"Destroyed: {destroyed} caravans\n\n" +
                        $"Remaining Counts:\n{countsSummary}");
                }, "Failed to destroy caravans");
            });
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Parses count argument. Accepts integer or "all". Returns null for "all", or the parsed integer.
        /// </summary>
        private static int? ParseCountArgument(string countArg, out string error)
        {
            error = null;

            if (countArg.Equals("all", StringComparison.OrdinalIgnoreCase))
            {
                return null;
            }

            if (!CommandValidator.ValidateIntegerRange(countArg, 1, int.MaxValue, out int countValue, out string countError))
            {
                error = countError;
                return null;
            }

            return countValue;
        }

        /// <summary>
        /// Gets a formatted summary of all caravan counts by owner type.
        /// </summary>
        private static string GetCaravanCountsSummary()
        {
            StringBuilder sb = new();

            // Total counts
            int totalCaravans = CaravanManager.TotalCaravanCount;
            int totalDisbanding = CaravanManager.TotalDisbandingCaravans;
            int totalActive = totalCaravans - totalDisbanding;

            sb.AppendLine($"Total Caravans: {totalActive} active, {totalDisbanding} disbanding ({totalCaravans} total)");
            sb.AppendLine();

            // Individual counts by owner type
            sb.AppendLine("By Owner Type:");
            AppendCaravanOwnerTypeCounts(sb, "Player",
                CaravanManager.TotalPlayerCaravans,
                CaravanManager.TotalPlayerCaravansDisbanding);
            AppendCaravanOwnerTypeCounts(sb, "Notable",
                CaravanManager.TotalNotableCaravans,
                CaravanManager.TotalNotableCaravansDisbanding);
            AppendCaravanOwnerTypeCounts(sb, "NPC Lord",
                CaravanManager.TotalNPCLordCaravans,
                CaravanManager.TotalNPCLordCaravansDisbanding);

            return sb.ToString().TrimEnd();
        }

        /// <summary>
        /// Appends formatted count information for a specific caravan owner type.
        /// </summary>
        private static void AppendCaravanOwnerTypeCounts(StringBuilder sb, string ownerType, int totalCount, int disbandingCount)
        {
            int activeCount = totalCount - disbandingCount;
            sb.AppendLine($"  {ownerType}: {activeCount} active, {disbandingCount} disbanding ({totalCount} total)");
        }

        #endregion
    }
}
