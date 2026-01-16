using Bannerlord.GameMaster.Console.Common;
using Bannerlord.GameMaster.Console.Common.Execution;
using Bannerlord.GameMaster.Console.Common.Formatting;
using Bannerlord.GameMaster.Console.Common.Parsing;
using System.Collections.Generic;
using TaleWorlds.Library;

namespace Bannerlord.GameMaster.Console.LoggerCommands;

/// <summary>
/// Command to enable command logging
/// </summary>
public static class EnableLoggingCommand
{
    /// <summary>
    /// Enable command logging
    /// Usage: gm.log.enable [path:custom_path]
    /// </summary>
    [CommandLineFunctionality.CommandLineArgumentFunction("enable", "gm.log")]
    public static string EnableLogging(List<string> args)
    {
        return Cmd.Run(args, () =>
        {
            // MARK: Parse Arguments
            ParsedArguments parsed = ArgumentParser.ParseArguments(args);
            parsed.SetValidArguments(
                new ArgumentDefinition("path", false, null, "p")
            );

            string validationError = parsed.GetValidationError();
            if (validationError != null)
                return MessageFormatter.FormatErrorMessage(validationError);

            string customPath = parsed.GetArgument("path", 0);

            // MARK: Execute Logic
            LoggingResult result = LoggingManager.EnableLogging(customPath);

            if (!result.WasSuccessful)
                return MessageFormatter.FormatErrorMessage(result.Message);

            string path = LoggingManager.CurrentLogFilePath;

            Dictionary<string, string> resolvedValues = new()
            {
                { "path", path }
            };

            string argumentDisplay = parsed.FormatArgumentDisplay("enable", resolvedValues);
            return argumentDisplay + MessageFormatter.FormatSuccessMessage(result.Message);
        });
    }
}
