using Bannerlord.GameMaster.Console.Common;
using Bannerlord.GameMaster.Console.Common.Execution;
using Bannerlord.GameMaster.Console.Common.Formatting;
using Bannerlord.GameMaster.Console.Common.Parsing;
using Bannerlord.GameMaster.Console.Common.Validation;
using Bannerlord.GameMaster.Cultures.Names;
using System;
using System.Collections.Generic;
using TaleWorlds.Library;

namespace Bannerlord.GameMaster.Console.NameCommands
{
    public static class ForceDefaultNamesCommand
    {
        /// <summary>
        /// Toggles or sets force-default names mode, which ignores all external JSON override files.
        /// Usage: gm.names.force_default [mode]
        /// </summary>
        [CommandLineFunctionality.CommandLineArgumentFunction("force_default", "gm.names")]
        public static string ForceDefaultNames(List<string> args)
        {
            return Cmd.Run(args, () =>
            {
                // MARK: Validation
                string usageMessage = CommandValidator.CreateUsageMessage(
                    "gm.names.force_default", "[mode]",
                    "Toggles or sets force-default names mode.\n" +
                    "When enabled, all external JSON override files are ignored and only hardcoded defaults are used.\n" +
                    "- mode: optional, 'on' to enable, 'off' to disable. Toggles if omitted\n" +
                    "Supports named arguments: mode:on\n",
                    "gm.names.force_default\n" +
                    "gm.names.force_default on\n" +
                    "gm.names.force_default off\n" +
                    "gm.names.force_default mode:on");

                ParsedArguments parsed = ArgumentParser.ParseArguments(args);

                parsed.SetValidArguments(
                    new ArgumentDefinition("mode", false)
                );

                string validationError = parsed.GetValidationError();
                if (validationError != null)
                    return CommandResult.Error(MessageFormatter.FormatErrorMessage(validationError));

                // MARK: Parse Arguments
                string modeArg = parsed.GetArgument("mode", 0);
                bool previousState = NameProvider.IsForceDefaultsActive;
                bool newState;

                if (modeArg == null)
                {
                    // Toggle
                    newState = !previousState;
                }

                else if (modeArg.Equals("on", StringComparison.OrdinalIgnoreCase))
                {
                    newState = true;
                }

                else if (modeArg.Equals("off", StringComparison.OrdinalIgnoreCase))
                {
                    newState = false;
                }

                else
                {
                    return CommandResult.Error(MessageFormatter.FormatErrorMessage(
                        $"Invalid mode '{modeArg}'. Use 'on' or 'off', or omit to toggle.\n\n" +
                        usageMessage));
                }

                // MARK: Execute Logic
                if (newState)
                {
                    NameProvider.ForceDefaults();
                }

                else
                {
                    NameProvider.ClearForceDefaults();
                }

                int overrideFileCount = NameFileManager.CountOverrideFiles();
                string stateDisplay = newState ? "ENABLED" : "DISABLED";
                string actionDisplay = previousState == newState
                    ? $"Force-defaults mode was already {stateDisplay}. Cache has been refreshed."
                    : $"Force-defaults mode is now {stateDisplay}.";

                string overrideInfo = newState
                    ? $"Override files on disk: {overrideFileCount} (currently ignored)"
                    : $"Override files found: {overrideFileCount} (will be loaded on next name access)";

                Dictionary<string, string> resolvedValues = new()
                {
                    { "mode", modeArg ?? (newState ? "on (toggled)" : "off (toggled)") }
                };

                string argumentDisplay = parsed.FormatArgumentDisplay("gm.names.force_default", resolvedValues);

                string resultMessage = argumentDisplay + MessageFormatter.FormatSuccessMessage(
                    $"{actionDisplay}\n" +
                    $"{overrideInfo}\n" +
                    $"Names directory: {NameFileManager.GetNamesBaseDirectory()}");

                return CommandResult.Success(resultMessage);
            }).Message;
        }
    }
}
