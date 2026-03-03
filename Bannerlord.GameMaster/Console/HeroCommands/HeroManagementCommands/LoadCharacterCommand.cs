using Bannerlord.GameMaster.Common;
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
    /// Console command to load a character set file onto an existing hero.
    /// Applies appearance, development, traits, equipment, age, and culture but does NOT change
    /// name, gender, stringId, MBGUID, or type (occupation).
    /// </summary>
    public static class LoadCharacterCommand
    {
        /// <summary>
        /// Load a character set file onto an existing hero.
        /// Usage: gm.hero.load_character &lt;hero&gt; &lt;filename&gt;
        /// </summary>
        [CommandLineFunctionality.CommandLineArgumentFunction("load_character", "gm.hero")]
        public static string LoadCharacter(List<string> args)
        {
            return Cmd.Run(args, () =>
            {
                // MARK: Validation
                if (!CommandValidator.ValidateCampaignState(out string error))
                    return CommandResult.Error(error);

                string usageMessage = CommandValidator.CreateUsageMessage(
                    "gm.hero.load_character", "<hero> <filename>",
                    "Loads a previously exported character set file and applies it to an existing hero.\n" +
                    "Applies: appearance, development, traits, battle equipment, civilian equipment, age, culture.\n" +
                    "Does NOT apply: name, gender, stringId, MBGUID, type (occupation).\n" +
                    "- hero: required, target hero name, stringId, or 'player' for the player hero\n" +
                    "- filename: required, character set filename (without extension)\n" +
                    "Supports named arguments: hero:derthert filename:my_king\n",
                    "gm.hero.load_character derthert my_king\n" +
                    "gm.hero.load_character 'Ira of the Aserai' warrior_template\n" +
                    "gm.hero.load_character hero:derthert filename:king_export");

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

                // Verify file exists
                CharacterSetFileManager fileManager = CharacterSetFileManager.Default;
                if (!fileManager.CharacterSetFileExists(filenameArg))
                {
                    return CommandResult.Error(MessageFormatter.FormatErrorMessage(
                        $"Character set file '{filenameArg}' not found.\n" +
                        $"Directory: {fileManager.GetCharacterSetDirectory()}"));
                }

                // MARK: Execute Logic
                Dictionary<string, string> resolvedValues = new()
                {
                    { "hero", hero.Name.ToString() },
                    { "filename", filenameArg }
                };

                string argumentDisplay = parsed.FormatArgumentDisplay("gm.hero.load_character", resolvedValues);

                string filepath = fileManager.GetCharacterSetFilePath(filenameArg);
                BLGMResult loadResult = fileManager.LoadCharacterSetToHero(hero, filepath);

                if (!loadResult.IsSuccess)
                    return CommandResult.Error(argumentDisplay + MessageFormatter.FormatErrorMessage(loadResult.Message));

                return CommandResult.Success(argumentDisplay + MessageFormatter.FormatSuccessMessage(loadResult.Message));
            }).Message;
        }
    }
}
