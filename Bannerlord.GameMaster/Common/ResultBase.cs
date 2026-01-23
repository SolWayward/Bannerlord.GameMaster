using System;
using Bannerlord.GameMaster.Common.Interfaces;
using Bannerlord.GameMaster.Information;

namespace Bannerlord.GameMaster.Common
{
    /// <summary>
    /// Abstract base class used for different result types.
    /// Contains a bool indicating if an operation succeeded, a string message with details of the result of the operation,
    /// and an exception if an exception occured. Also includes convenience methods for logging to game rgl log and system console and or displaying in game.
    /// </summary>
    public abstract class ResultBase<TSelf> where TSelf : ResultBase<TSelf>, new()
    {
        /// <summary>Indicates if operation succeded or failed</summary>
        public bool IsSuccess { get; set; }

        /// <summary>Message with details of operation result</summary>
        public string Message { get; set; }

        /// <summary>Exception details for logging</summary>
        public Exception Exception { get; set; }

        /// <summary>Prefix used to indicate result type in Logs(Override this)</summary>
        protected abstract string Prefix { get; }

        /// <summary>Factory Method for creating a success result</summary>
        public static TSelf Success(string message) => new() { IsSuccess = true, Message = message };

        /// <summary>Factory Method for creating a non exception error result</summary>
        public static TSelf Error(string message) => new() { IsSuccess = false, Message = message };

        /// <summary>Factory Method for creating an esception error result</summary>
        public static TSelf Error(string message, Exception ex) => new() { IsSuccess = false, Message = message, Exception = ex };

        /// <summary>Creates an unhandled failure result, recommended to use one of the other constructors</summary>
        public ResultBase()
        {
            IsSuccess = false;
            Message = "unhandled failure";
            Exception = null;
        }

        /// <summary>Used for successful results or results that failed without an exception</summary>
        public ResultBase(bool isSuccess, string message)
        {
            IsSuccess = isSuccess;
            Message = message;
            Exception = null;
        }

        /// <summary>Used for results in exception handling blocks</summary>
        public ResultBase(bool isSuccessful, string message, Exception exception)
        {
            IsSuccess = isSuccessful;
            Message = message;
            Exception = exception;
        }

        /// <summary>
        /// Displays a game log message using InformationManager.DisplayMessage(). Only displays contents of Prefix and Message. 
        /// Does not display exception details by default<br />
        /// Message is red if wasSuccessful = false, green if wasSuccessful = true
        /// </summary>
        /// <returns>Returns itself to allow chaining</returns>
        public TSelf DisplayMessage()
        {
            if (IsSuccess)
                InfoMessage.Success(GetFormattedMessage());
            else
                InfoMessage.Error(GetFormattedMessage());
            return (TSelf)this;
        }

        /// <summary>
        /// Writes to the game's main rgl log file the message and success status if exception occured. 
        /// Will also always write to system console if console is attached.
        /// Automatically writes exception details if applicable.
        /// Does not write to custom log file, that is handled by CommandExecutor.Run()
        /// </summary>
        /// <returns>Returns itself to allow chaining</returns>
        public virtual TSelf Log()
        {
            string logEntry = GetFormattedLogEntry();

            // Write to game RGL log file if exception
            if (!IsSuccess && Exception != null)
                TaleWorlds.Library.Debug.Print(logEntry);

            // Always Write to system console for easier debugging
            SystemConsoleManager.WriteLog(logEntry);
            return (TSelf)this;
        }

        /// <summary>
        /// Returns the message formatted with prefix and if failure, also prefixed with "Error". 
        /// Does not contain exception details automatically<br />
        /// Example: "[PREFIX] Error: result message" OR "[PREFIX] result message"
        /// </summary>
        public string GetFormattedMessage()
        {
            if (IsSuccess)
                return $"{Prefix} {Message}";
            else
                return $"{Prefix} Error: {Message}";
        }

        /// <summary>
        /// Gets a formatted string for logging, includes exception details if available.
        /// Not meant for direct display to user.
        /// </summary>
        public string GetFormattedLogEntry()
        {
            string logEntry = GetFormattedMessage();

            if (!IsSuccess && Exception != null)
                logEntry += $"\nException Details:\n{Exception}";

            return logEntry;
        }

        /// <summary>
        /// Displays message in game and writes to game's main log file. Will also write to system console if console is attached.
        /// Automatically logs exception details if applicable, but does not display exception details in game.
        /// </summary>
        /// <returns>Returns itself to allow chaining</returns>
        public TSelf DisplayAndLog() { DisplayMessage(); Log(); return (TSelf)this; }
    }
}