using System;
using Bannerlord.GameMaster.Common;

namespace Bannerlord.GameMaster.Console.Common
{
	/// <summary>
	/// An object containing a bool indicating if a Command operation succeeded, a string message with details of the result of the operation,
	/// and an exception if an exception occured. Also includes convenience methods for logging to game rgl log and system console and or displaying in game.
	/// </summary>
	public class CommandResult : ResultBase<CommandResult>
	{
		protected override string Prefix => "[BLGM COMMAND]";

		/// <inheritdoc/>
		public CommandResult() : base() { }

		/// <inheritdoc/>
		public CommandResult(bool isSuccess, string message) : base(isSuccess, message) { }

		/// <inheritdoc/>
		public CommandResult(bool isSuccess, string message, Exception ex) : base(isSuccess, message, ex) { }

		public override CommandResult Log()
		{
			base.Log();
			
			// Log to custom command log file (only if logging is enabled)
			if (Execution.CommandLogger.IsEnabled)
			{
				Execution.CommandLogger.LogCommandResult(this);
			}
			
			return this;
		}
	}
}