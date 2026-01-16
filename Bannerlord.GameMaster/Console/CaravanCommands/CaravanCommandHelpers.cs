using System.Text;
using Bannerlord.GameMaster.Caravans;
using Bannerlord.GameMaster.Console.Common.Validation;

namespace Bannerlord.GameMaster.Console.CaravanCommands
{
    /// <summary>
    /// Helper methods for caravan-related console commands.
    /// </summary>
    public static class CaravanCommandHelpers
    {
        #region Count Parsing

        /// <summary>
        /// Parses count argument. Accepts integer or "all". Returns null for "all", or the parsed integer.
        /// </summary>
        /// <param name="countArg">The count argument string to parse</param>
        /// <param name="error">Error message if parsing fails, null otherwise</param>
        /// <returns>Parsed count value, or null if "all" was specified</returns>
        public static int? ParseCountArgument(string countArg, out string error)
        {
            error = null;

            if (string.IsNullOrWhiteSpace(countArg))
            {
                error = "Count argument cannot be empty.";
                return null;
            }

            if (countArg.Equals("all", System.StringComparison.OrdinalIgnoreCase))
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
        /// Formats the count argument for display purposes.
        /// </summary>
        /// <param name="countArg">The original count argument</param>
        /// <param name="parsedCount">The parsed count value (null if "all")</param>
        /// <returns>Formatted string for display</returns>
        public static string FormatCountForDisplay(string countArg, int? parsedCount)
        {
            return countArg.Equals("all", System.StringComparison.OrdinalIgnoreCase) 
                ? "All" 
                : parsedCount?.ToString() ?? "All";
        }

        /// <summary>
        /// Formats the count info suffix for result messages.
        /// </summary>
        /// <param name="count">The parsed count (null if "all")</param>
        /// <returns>Formatted string like " (requested: 5)" or " (all)"</returns>
        public static string FormatCountInfoSuffix(int? count)
        {
            return count.HasValue ? $" (requested: {count.Value})" : " (all)";
        }

        #endregion

        #region Caravan Statistics

        /// <summary>
        /// Gets a formatted summary of all caravan counts by owner type.
        /// </summary>
        /// <returns>Formatted summary string</returns>
        public static string GetCaravanCountsSummary()
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
        /// <param name="sb">StringBuilder to append to</param>
        /// <param name="ownerType">Display name for the owner type</param>
        /// <param name="totalCount">Total count of caravans</param>
        /// <param name="disbandingCount">Count of disbanding caravans</param>
        public static void AppendCaravanOwnerTypeCounts(StringBuilder sb, string ownerType, int totalCount, int disbandingCount)
        {
            int activeCount = totalCount - disbandingCount;
            sb.AppendLine($"  {ownerType}: {activeCount} active, {disbandingCount} disbanding ({totalCount} total)");
        }

        #endregion
    }
}
