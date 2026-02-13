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
    public static class ReloadNamesCommand
    {
        private static readonly HashSet<string> ValidCultures = new(StringComparer.OrdinalIgnoreCase)
        {
            "vlandia", "aserai", "battania", "empire", "khuzait", "sturgia", "nord", "all"
        };

        /// <summary>
        /// Reloads custom name override JSON files from disk, clearing the name cache.
        /// Usage: gm.names.reload [culture]
        /// </summary>
        [CommandLineFunctionality.CommandLineArgumentFunction("reload", "gm.names")]
        public static string ReloadNames(List<string> args)
        {
            return Cmd.Run(args, () =>
            {
                // MARK: Validation
                string usageMessage = CommandValidator.CreateUsageMessage(
                    "gm.names.reload", "[culture]",
                    "Reloads custom name override JSON files from disk, clearing the name cache.\n" +
                    "If force-defaults mode is active, it will be cleared before reloading.\n" +
                    "- culture: optional, culture to reload. Defaults to all cultures\n" +
                    "  Valid values: vlandia, aserai, battania, empire, khuzait, sturgia, nord, all\n" +
                    "Supports named arguments: culture:vlandia\n",
                    "gm.names.reload\n" +
                    "gm.names.reload vlandia\n" +
                    "gm.names.reload culture:battania\n" +
                    "gm.names.reload culture:all");

                ParsedArguments parsed = ArgumentParser.ParseArguments(args);

                parsed.SetValidArguments(
                    new ArgumentDefinition("culture", false)
                );

                string validationError = parsed.GetValidationError();
                if (validationError != null)
                    return CommandResult.Error(MessageFormatter.FormatErrorMessage(validationError));

                // MARK: Parse Arguments
                string cultureArg = parsed.GetArgument("culture", 0);

                // Validate culture if provided
                if (cultureArg != null && !ValidCultures.Contains(cultureArg))
                {
                    return CommandResult.Error(MessageFormatter.FormatErrorMessage(
                        $"Invalid culture '{cultureArg}'.\n" +
                        $"Valid cultures: vlandia, aserai, battania, empire, khuzait, sturgia, nord, all\n\n" +
                        usageMessage));
                }

                // MARK: Execute Logic
                bool wasForceDefaultsActive = NameProvider.IsForceDefaultsActive;
                string forceDefaultsMessage = "";

                // Clear force-defaults if it was active
                if (wasForceDefaultsActive)
                {
                    NameProvider.ClearForceDefaults();
                    forceDefaultsMessage = "Force-defaults mode was active and has been cleared.\n";
                }

                bool reloadAll = cultureArg == null || cultureArg.Equals("all", StringComparison.OrdinalIgnoreCase);

                if (reloadAll)
                {
                    NameProvider.ReloadAll();
                }

                else
                {
                    NameProvider.ReloadCulture(cultureArg);
                }

                int overrideFileCount = NameFileManager.CountOverrideFiles();
                string namesDirectory = NameFileManager.GetNamesBaseDirectory();
                string cultureDisplay = reloadAll ? "all cultures" : cultureArg;

                Dictionary<string, string> resolvedValues = new()
                {
                    { "culture", cultureDisplay }
                };

                string argumentDisplay = parsed.FormatArgumentDisplay("gm.names.reload", resolvedValues);

                string resultMessage = argumentDisplay + MessageFormatter.FormatSuccessMessage(
                    $"Reloaded name cache for {cultureDisplay}.\n" +
                    forceDefaultsMessage +
                    $"Override files found: {overrideFileCount}\n" +
                    $"Names directory: {namesDirectory}");

                return CommandResult.Success(resultMessage);
            }).Message;
        }
    }
}
