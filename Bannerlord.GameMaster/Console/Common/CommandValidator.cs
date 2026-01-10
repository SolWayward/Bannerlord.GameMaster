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
		/// Validates if creating the specified number of heroes would exceed BLGM hero limits.
		/// Limits are in place to maintain performance but can be bypassed using 'gm.ignore_limits true'.
		/// </summary>
		/// <param name="countToCreate">Number of heroes that will be created</param>
		/// <param name="error">Error message if validation fails</param>
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
		/// <param name="error">Error message if validation fails</param>
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
		/// <param name="error">Error message if validation fails</param>
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

		/// <summary>
		/// Creates usage message for commands
		/// </summary>
		public static string CreateUsageMessage(string commandName, string syntax, string description, string example = null)
		{
			string message = $"Usage: {commandName} {syntax}\n{description}\n";

			if (!string.IsNullOrEmpty(example))
				message += $"Example: {example}\n";

			return message;
		}
	}
}