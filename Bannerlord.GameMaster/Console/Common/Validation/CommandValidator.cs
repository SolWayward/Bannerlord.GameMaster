using System;
using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Election;
using TaleWorlds.Library;

namespace Bannerlord.GameMaster.Console.Common.Validation
{
    /// <summary>
    /// Provides validation utilities for command arguments and game state
    /// </summary>
    public static class CommandValidator
    {
        #region Game State Validation

        /// <summary>
        /// Validates that player is in a valid campaign state before executing any commands.
        /// Checks for campaign existence, conversation status, and pending settlement decisions.
        /// </summary>
        /// <param name="error">Error message if validation fails, null if successful</param>
        /// <returns>True if in valid campaign state, false otherwise</returns>
        public static bool ValidateCampaignState(out string error)
        {
            if (Campaign.Current == null || !BLGMObjectManager.CampaignFullyLoaded)
            {
                error = "Error: Must be in campaign mode.\n";
                return false;
            }

            // Some commands crash or glitch while conversation is inprogress
            if (Campaign.Current.ConversationManager.IsConversationInProgress)
            {
                error = "Error: Please end conversation before executing command\n";
                return false;
            }

            // Prevents commands running while settlement ownership vote is pending as it can cause some crashes
            if (!ValidateNoSettlementClaimantDecisionsPending(out error))
                return false;

            error = null;
            return true;
        }

        /// <summary>
        /// Validates that no settlement claimant decisions are pending.
        /// Some commands can cause crashes when executed during pending settlement ownership votes.
        /// </summary>
        /// <param name="error">Error message if validation fails, null if successful</param>
        /// <returns>True if no settlement claimant decisions are pending, false otherwise</returns>
        public static bool ValidateNoSettlementClaimantDecisionsPending(out string error)
        {
            if (Clan.PlayerClan?.Kingdom != null)
            {
                MBReadOnlyList<KingdomDecision> unresolvedDecisions = Clan.PlayerClan.Kingdom.UnresolvedDecisions;
                if (unresolvedDecisions != null)
                {
                    for (int i = 0; i < unresolvedDecisions.Count; i++)
                    {
                        KingdomDecision decision = unresolvedDecisions[i];
                        if (decision is SettlementClaimantDecision || decision is SettlementClaimantPreliminaryDecision)
                        {
                            error = "Error: Cannot execute commands while settlement ownership decisions are pending.\nPlease resolve the 'who gets the fief' decision first (click the notification on the map).\n";
                            return false;
                        }
                    }
                }
            }

            error = null;
            return true;
        }

        #endregion

        #region Argument Validation

        /// <summary>
        /// Validates minimum argument count for command execution.
        /// </summary>
        /// <param name="args">List of arguments to validate</param>
        /// <param name="requiredCount">Minimum number of arguments required</param>
        /// <param name="usageMessage">Usage message to display if validation fails</param>
        /// <param name="error">Error message if validation fails, null if successful</param>
        /// <returns>True if argument count is sufficient, false otherwise</returns>
        public static bool ValidateArgumentCount(List<string> args, int requiredCount, string usageMessage, out string error)
        {
            if (args == null || args.Count < requiredCount)
            {
                error = $"Error: Missing arguments.\n{usageMessage}";
                return false;
            }
            error = null;
            return true;
        }

        /// <summary>
        /// Validates integer argument within a specified range.
        /// </summary>
        /// <param name="value">String value to parse and validate</param>
        /// <param name="min">Minimum allowed value (inclusive)</param>
        /// <param name="max">Maximum allowed value (inclusive)</param>
        /// <param name="result">Parsed integer value if successful</param>
        /// <param name="error">Error message if validation fails, null if successful</param>
        /// <returns>True if value is valid integer within range, false otherwise</returns>
        public static bool ValidateIntegerRange(string value, int min, int max, out int result, out string error)
        {
            if (!int.TryParse(value, out result))
            {
                error = $"Invalid value '{value}'. Must be a number.";
                return false;
            }

            if (result < min || result > max)
            {
                error = $"Invalid value '{value}'. Must be between {min} and {max}.";
                return false;
            }

            error = null;
            return true;
        }

        /// <summary>
        /// Validates float argument within a specified range.
        /// </summary>
        /// <param name="value">String value to parse and validate</param>
        /// <param name="min">Minimum allowed value (inclusive)</param>
        /// <param name="max">Maximum allowed value (inclusive)</param>
        /// <param name="result">Parsed float value if successful</param>
        /// <param name="error">Error message if validation fails, null if successful</param>
        /// <returns>True if value is valid float within range, false otherwise</returns>
        public static bool ValidateFloatRange(string value, float min, float max, out float result, out string error)
        {
            if (!float.TryParse(value, out result))
            {
                error = $"Invalid value '{value}'. Must be a number.";
                return false;
            }

            if (result < min || result > max)
            {
                error = $"Invalid value '{value}'. Must be between {min} and {max}.";
                return false;
            }

            error = null;
            return true;
        }

        /// <summary>
        /// Validates boolean argument from string value.
        /// </summary>
        /// <param name="value">String value to parse (true/false)</param>
        /// <param name="result">Parsed boolean value if successful</param>
        /// <param name="error">Error message if validation fails, null if successful</param>
        /// <returns>True if value is valid boolean, false otherwise</returns>
        public static bool ValidateBoolean(string value, out bool result, out string error)
        {
            if (!bool.TryParse(value, out result))
            {
                error = $"Invalid value '{value}'. Must be true or false.";
                return false;
            }

            error = null;
            return true;
        }

        #endregion

        #region Creation Limit Validation

        /// <summary>
        /// Validates if creating the specified number of heroes would exceed BLGM hero limits.
        /// Limits are in place to maintain performance but can be bypassed using 'gm.ignore_limits true'.
        /// </summary>
        /// <param name="countToCreate">Number of heroes that will be created</param>
        /// <param name="error">Error message if validation fails, null if successful</param>
        /// <returns>True if operation is allowed, false if it would exceed limits</returns>
        public static bool ValidateHeroCreationLimit(int countToCreate, out string error)
        {
            // Allow if limits are being ignored
            if (BLGMObjectManager.IgnoreLimits)
            {
                error = null;
                return true;
            }

            int currentCount = BLGMObjectManager.BlgmHeroCount;
            int maxLimit = BLGMObjectManager.maxBlgmHeroes;
            int afterCreation = currentCount + countToCreate;

            if (afterCreation > maxLimit)
            {
                error = $"Operation would exceed BLGM hero limit.\n" +
                        $"Current BLGM heroes: {currentCount}\n" +
                        $"Attempting to create: {countToCreate}\n" +
                        $"Total after operation: {afterCreation}\n" +
                        $"Maximum allowed: {maxLimit}\n" +
                        $"Hero limits are in place to maintain game performance.\n" +
                        $"Use 'gm.ignore_limits true' to bypass this limit (not recommended for large numbers).";
                return false;
            }

            error = null;
            return true;
        }

        /// <summary>
        /// Validates if creating the specified number of clans would exceed BLGM clan limits.
        /// Limits are in place to maintain performance but can be bypassed using 'gm.ignore_limits true'.
        /// </summary>
        /// <param name="countToCreate">Number of clans that will be created</param>
        /// <param name="error">Error message if validation fails, null if successful</param>
        /// <returns>True if operation is allowed, false if it would exceed limits</returns>
        public static bool ValidateClanCreationLimit(int countToCreate, out string error)
        {
            // Allow if limits are being ignored
            if (BLGMObjectManager.IgnoreLimits)
            {
                error = null;
                return true;
            }

            int currentCount = BLGMObjectManager.BlgmClanCount;
            int maxLimit = BLGMObjectManager.maxBlgmClans;
            int afterCreation = currentCount + countToCreate;

            if (afterCreation > maxLimit)
            {
                error = $"Operation would exceed BLGM clan limit.\n" +
                        $"Current BLGM clans: {currentCount}\n" +
                        $"Attempting to create: {countToCreate}\n" +
                        $"Total after operation: {afterCreation}\n" +
                        $"Maximum allowed: {maxLimit}\n" +
                        $"Clan limits are in place to maintain game performance.\n" +
                        $"Use 'gm.ignore_limits true' to bypass this limit (not recommended for large numbers).";
                return false;
            }

            error = null;
            return true;
        }

        /// <summary>
        /// Validates if creating the specified number of kingdoms would exceed BLGM kingdom limits.
        /// Limits are in place to maintain performance but can be bypassed using 'gm.ignore_limits true'.
        /// </summary>
        /// <param name="countToCreate">Number of kingdoms that will be created</param>
        /// <param name="error">Error message if validation fails, null if successful</param>
        /// <returns>True if operation is allowed, false if it would exceed limits</returns>
        public static bool ValidateKingdomCreationLimit(int countToCreate, out string error)
        {
            // Allow if limits are being ignored
            if (BLGMObjectManager.IgnoreLimits)
            {
                error = null;
                return true;
            }

            int currentCount = BLGMObjectManager.BlgmKingdomCount;
            int maxLimit = BLGMObjectManager.maxBlgmKingdoms;
            int afterCreation = currentCount + countToCreate;

            if (afterCreation > maxLimit)
            {
                error = $"Operation would exceed BLGM kingdom limit.\n" +
                        $"Current BLGM kingdoms: {currentCount}\n" +
                        $"Attempting to create: {countToCreate}\n" +
                        $"Total after operation: {afterCreation}\n" +
                        $"Maximum allowed: {maxLimit}\n" +
                        $"Kingdom limits are in place to maintain game performance.\n" +
                        $"Use 'gm.ignore_limits true' to bypass this limit (not recommended for large numbers).";
                return false;
            }

            error = null;
            return true;
        }

        #endregion

        #region Usage Message Generation

        /// <summary>
        /// Creates a formatted usage message for console commands.
        /// </summary>
        /// <param name="commandName">Name of the command (e.g., "gm.hero.create")</param>
        /// <param name="syntax">Syntax description (e.g., "&lt;name&gt; [culture]")</param>
        /// <param name="description">Description of what the command does</param>
        /// <param name="example">Optional example usage</param>
        /// <returns>Formatted usage message string</returns>
        public static string CreateUsageMessage(string commandName, string syntax, string description, string example = null)
        {
            string message = $"Usage: {commandName} {syntax}\n{description}\n";

            if (!string.IsNullOrEmpty(example))
                message += $"Example: {example}\n";

            return message;
        }

        #endregion
    }
}
