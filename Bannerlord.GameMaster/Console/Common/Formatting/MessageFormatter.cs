namespace Bannerlord.GameMaster.Console.Common.Formatting
{
    /// <summary>
    /// Provides static methods for formatting console command output messages
    /// with consistent styling across all commands.
    /// </summary>
    public static class MessageFormatter
    {
        /// <summary>
        /// Formats a success message with consistent styling.
        /// </summary>
        /// <param name="message">The success message to format.</param>
        /// <returns>A formatted success message string with "Success: " prefix and trailing newline.</returns>
        public static string FormatSuccessMessage(string message)
        {
            return $"Success: {message}\n";
        }

        /// <summary>
        /// Formats an error message with consistent styling.
        /// </summary>
        /// <param name="message">The error message to format.</param>
        /// <returns>A formatted error message string with "Error: " prefix and trailing newline.</returns>
        public static string FormatErrorMessage(string message)
        {
            return $"Error: {message}\n";
        }
    }
}
