using System;
using System.Collections.Generic;

namespace Bannerlord.GameMaster.Console.Common
{
	/// <summary>
	/// Provides validation utilities for command arguments
	/// </summary>
	public static class CommandValidator
	{
		/// <summary>
		/// Validates integer argument within a range
		/// </summary>
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
		/// Validates float argument within a range
		/// </summary>
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
		/// Validates boolean argument
		/// </summary>
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

		/// <summary>
		/// Creates usage message for commands
		/// </summary>
		public static string CreateUsageMessage(string commandName, string syntax, string description, string example = null)
		{
			var message = $"Usage: {commandName} {syntax}\n{description}\n";

			if (!string.IsNullOrEmpty(example))
				message += $"Example: {example}\n";

			return message;
		}
	}
}