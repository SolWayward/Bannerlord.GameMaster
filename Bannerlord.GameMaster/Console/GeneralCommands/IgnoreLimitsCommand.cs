using Bannerlord.GameMaster.Console.Common.Execution;
using Bannerlord.GameMaster.Console.Common.Formatting;
using Bannerlord.GameMaster.Console.Common.Parsing;
using Bannerlord.GameMaster.Console.Common.Validation;
using System.Collections.Generic;
using System.Text;
using TaleWorlds.Library;

namespace Bannerlord.GameMaster.Console.GeneralCommands;

/// <summary>
/// Command to enable or disable object creation limits
/// </summary>
public static class IgnoreLimitsCommand
{
    /// <summary>
    /// Enable or disable object creation limits
    /// Usage: gm.ignore_limits [enabled]
    /// </summary>
    [CommandLineFunctionality.CommandLineArgumentFunction("ignore_limits", "gm")]
    public static string IgnoreLimits(List<string> args)
    {
        return Cmd.Run(args, () =>
        {
            // MARK: Validation
            string usageMessage = CommandValidator.CreateUsageMessage(
                "gm.ignore_limits",
                "[enabled]",
                "Enable or disable object creation limits. Limits exist to prevent performance issues.\n" +
                "Arguments:\n" +
                "  enabled - true/false or 1/0 to enable/disable limit checking\n" +
                "When no argument is provided, displays current status and limits.",
                "gm.ignore_limits true"
            );

            ParsedArguments parsed = ArgumentParser.ParseArguments(args);
            parsed.SetValidArguments(
                new ArgumentDefinition("enabled", false, null, "value")
            );

            string validationError = parsed.GetValidationError();
            if (validationError != null)
                return MessageFormatter.FormatErrorMessage(validationError);

            // If no arguments, display current status and limits
            if (parsed.TotalCount == 0)
            {
                return GeneralCommandHelpers.GetStatusMessage();
            }

            // MARK: Parse Arguments
            string enabledArg = parsed.GetArgument("enabled", 0) ?? parsed.GetArgument("value", 0);
            if (string.IsNullOrWhiteSpace(enabledArg))
            {
                return GeneralCommandHelpers.GetStatusMessage();
            }

            string input = enabledArg.ToLower();
            bool newValue;

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
                return MessageFormatter.FormatErrorMessage($"Invalid value '{enabledArg}'. Must be true, false, 1, or 0.\n{usageMessage}");
            }

            // MARK: Execute Logic
            BLGMObjectManager.IgnoreLimits = newValue;

            Dictionary<string, string> resolvedValues = new()
            {
                { "enabled", newValue.ToString() }
            };

            StringBuilder result = new();
            result.AppendLine(MessageFormatter.FormatSuccessMessage($"Ignore limits set to: {newValue}"));
            result.AppendLine();
            result.Append(GeneralCommandHelpers.GetLimitsInfo());
            
            if (newValue)
            {
                result.AppendLine();
                result.AppendLine("WARNING: Exceeding these limits may cause performance degradation on the campaign map.");
            }

            string argumentDisplay = parsed.FormatArgumentDisplay("ignore_limits", resolvedValues);
            return argumentDisplay + result.ToString();
        });
    }
}
