using Bannerlord.GameMaster.Console.Common;
using Bannerlord.GameMaster.Console.Common.EntityFinding;
using Bannerlord.GameMaster.Console.Common.Execution;
using Bannerlord.GameMaster.Console.Common.Formatting;
using Bannerlord.GameMaster.Console.Common.Parsing;
using Bannerlord.GameMaster.Console.Common.Validation;
using Bannerlord.GameMaster.Heroes;
using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Library;

namespace Bannerlord.GameMaster.Console.HeroCommands.HeroManagementCommands
{
    /// <summary>
    /// Console command to export a hero's full character data to a single JSON file.
    /// Saves appearance, development, traits, battle equipment, and civilian equipment.
    /// </summary>
    public static class ExportCharacterCommand
    {
        /// <summary>
        /// Export a hero's full character data to a character set file.
        /// Usage: gm.hero.export_character &lt;hero&gt; &lt;filename&gt;
        /// </summary>
        [CommandLineFunctionality.CommandLineArgumentFunction("export_character", "gm.hero")]
        public static string ExportCharacter(List<string> args)
        {
            return Cmd.Run(args, () =>
            {
                // MARK: Validation
                if (!CommandValidator.ValidateCampaignState(out string error))
                    return CommandResult.Error(error);

                string usageMessage = CommandValidator.CreateUsageMessage(
                    "gm.hero.export_character", "<hero> <filename>",
                    "Exports a hero's full character data (appearance, development, traits, equipment) to a single JSON file.\n" +
                    "- hero: required, hero name, stringId, or 'player' for the player hero\n" +
                    "- filename: required, output filename without extension\n" +
                    "Supports named arguments: hero:derthert filename:my_king\n",
                    "gm.hero.export_character derthert my_king\n" +
                    "gm.hero.export_character 'Ira of the Aserai' ira_backup\n" +
                    "gm.hero.export_character hero:derthert filename:king_export");

                ParsedArguments parsed = ArgumentParser.ParseArguments(args);

                parsed.SetValidArguments(
                    new ArgumentDefinition("hero", true),
                    new ArgumentDefinition("filename", true)
                );

                string validationError = parsed.GetValidationError();
                if (validationError != null)
                    return CommandResult.Error(MessageFormatter.FormatErrorMessage(validationError));

                if (parsed.TotalCount < 2)
                    return CommandResult.Success(usageMessage);

                // MARK: Parse Arguments
                string heroArg = parsed.GetArgument("hero", 0);
                if (string.IsNullOrWhiteSpace(heroArg))
                    return CommandResult.Error(MessageFormatter.FormatErrorMessage("Hero argument cannot be empty."));

                string filenameArg = parsed.GetArgument("filename", 1);
                if (string.IsNullOrWhiteSpace(filenameArg))
                    return CommandResult.Error(MessageFormatter.FormatErrorMessage("Filename argument cannot be empty."));

                // Resolve hero
                EntityFinderResult<Hero> heroResult = HeroFinder.FindSingleHero(heroArg);
                if (!heroResult.IsSuccess)
                    return CommandResult.Error(heroResult.Message);

                Hero hero = heroResult.Entity;

                // MARK: Execute Logic
                Dictionary<string, string> resolvedValues = new()
                {
                    { "hero", hero.Name.ToString() },
                    { "filename", filenameArg }
                };

                string argumentDisplay = parsed.FormatArgumentDisplay("gm.hero.export_character", resolvedValues);

                CharacterSetFileManager fileManager = CharacterSetFileManager.Default;
                string filepath = fileManager.GetCharacterSetFilePath(filenameArg);
                fileManager.SaveCharacterSetToFile(hero, filepath);

                return CommandResult.Success(argumentDisplay + MessageFormatter.FormatSuccessMessage(
                    $"Exported character '{hero.Name}' to '{filenameArg}'.\n" +
                    $"Hero: {hero.Name} (ID: {hero.StringId}), Level: {hero.Level}, Age: {(int)hero.Age}\n" +
                    $"File: {filepath}"));
            }).Message;
        }
    }
}
