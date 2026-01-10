using System;
using System.Collections.Generic;
using System.Text;
using Bannerlord.GameMaster.Console.Common;
using Helpers;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Library;

namespace Bannerlord.GameMaster.Console
{
    public static class GeneralCommands
    {
        [CommandLineFunctionality.CommandLineArgumentFunction("ignore_limits", "gm")]
        public static string IgnoreLimitsCommand(List<string> args)
        {
            return Cmd.Run(args, () =>
            {
                // Create usage message
                string usageMessage = CommandValidator.CreateUsageMessage(
                    "gm.ignore_limits",
                    "<enabled>",
                    "Enable or disable object creation limits. Limits exist to prevent performance issues.\n" +
                    "Arguments:\n" +
                    "  enabled - true/false or 1/0 to enable/disable limit checking\n" +
                    "When no argument is provided, displays current status and limits.",
                    "gm.ignore_limits true"
                );

                // If no arguments, display current status and limits
                if (args == null || args.Count == 0)
                {
                    return GetStatusMessage();
                }

                // Validate argument count
                if (!CommandBase.ValidateArgumentCount(args, 1, usageMessage, out string validationError))
                {
                    return validationError;
                }

                return CommandBase.ExecuteWithErrorHandling(() =>
                {
                    string input = args[0].ToLower();
                    bool newValue;

                    // Parse input - support true/false and 1/0
                    if (input == "true" || input == "1")
                    {
                        newValue = true;
                    }
                    else if (input == "false" || input == "0")
                    {
                        newValue = false;
                    }
                    else
                    {
                        return CommandBase.FormatErrorMessage($"Invalid value '{args[0]}'. Must be true, false, 1, or 0.\n{usageMessage}");
                    }

                    // Set the new value
                    BLGMObjectManager.IgnoreLimits = newValue;

                    // Build result message
                    StringBuilder result = new();
                    result.AppendLine(CommandBase.FormatSuccessMessage($"Ignore limits set to: {newValue}"));
                    result.AppendLine();
                    result.Append(GetLimitsInfo());
                    
                    if (newValue)
                    {
                        result.AppendLine();
                        result.AppendLine("WARNING: Exceeding these limits may cause performance degradation on the campaign map.");
                    }

                    return result.ToString();
                }, "Error setting ignore limits");
            });
        }

        #region Helper Methods

        /// <summary>
        /// Gets current status message including limits and counts
        /// </summary>
        private static string GetStatusMessage()
        {
            StringBuilder sb = new();
            sb.AppendLine("=== BLGM Object Creation Limits ===");
            sb.AppendLine();
            sb.AppendLine($"Ignore Limits: {BLGMObjectManager.IgnoreLimits}");
            sb.AppendLine();
            sb.Append(GetLimitsInfo());
            sb.AppendLine();
            sb.AppendLine("Usage: gm.ignore_limits <true|false|1|0>");
            sb.AppendLine("Note: Limits exist to prevent performance issues on the campaign map.");
            
            return sb.ToString();
        }

        /// <summary>
        /// Gets formatted limits and current counts
        /// </summary>
        private static string GetLimitsInfo()
        {
            StringBuilder sb = new();
            sb.AppendLine("Current Limits and Counts:");
            sb.AppendLine($"  Heroes:   {BLGMObjectManager.BlgmHeroCount,4} / {BLGMObjectManager.maxBlgmHeroes,4}");
            sb.AppendLine($"  Clans:    {BLGMObjectManager.BlgmClanCount,4} / {BLGMObjectManager.maxBlgmClans,4}");
            sb.AppendLine($"  Kingdoms: {BLGMObjectManager.BlgmKingdomCount,4} / {BLGMObjectManager.maxBlgmKingdoms,4}");
            
            return sb.ToString();
        }

        #endregion
    }
}