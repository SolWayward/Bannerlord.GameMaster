namespace Bannerlord.GameMaster.Console.Common
{
    /// <summary>
  /// Represents the result of a command execution
    /// </summary>
    public class CommandResult
 {
     public bool IsSuccess { get; set; }
        public string Message { get; set; }

    private CommandResult(bool isSuccess, string message)
        {
        IsSuccess = isSuccess;
Message = message;
        }

 /// <summary>
        /// Creates a successful command result
   /// </summary>
  public static CommandResult Success(string message) => new CommandResult(true, $"Success: {message}\n");

        /// <summary>
   /// Creates a failed command result
        /// </summary>
        public static CommandResult Error(string message) => new CommandResult(false, $"Error: {message}\n");

        /// <summary>
        /// Implicitly converts CommandResult to string for backward compatibility
  /// </summary>
        public static implicit operator string(CommandResult result) => result.Message;

        /// <summary>
   /// Creates CommandResult from string message (assumes success)
      /// </summary>
        public static implicit operator CommandResult(string message) => Success(message);
    }
}