using Bannerlord.GameMaster.Common;
using Bannerlord.GameMaster.Console.Common;
using Bannerlord.GameMaster.Console.Common.EntityFinding;
using Bannerlord.GameMaster.Console.Common.Execution;
using Bannerlord.GameMaster.Console.Common.Formatting;
using Bannerlord.GameMaster.Console.Common.Parsing;
using Bannerlord.GameMaster.Console.Common.Validation;
using Bannerlord.GameMaster.Heroes.HeroDevelopment;
using System.Collections.Generic;
using System.IO;
using System.Text;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Library;

namespace Bannerlord.GameMaster.Console.HeroCommands.HeroManagementCommands
{
    /// <summary>
    /// Console command to load a hero's development (skills, attributes, perks) from a JSON file.
    /// </summary>
    public static class LoadDevelopmentCommand
    {
        /// <summary>
        /// Load a hero's development from a JSON file.
        /// Usage: gm.hero.load_development &lt;hero&gt; &lt;filename&gt;
        /// </summary>
        [CommandLineFunctionality.CommandLineArgumentFunction("load_development", "gm.hero")]
        public static string LoadDevelopment(List<string> args)
        {
            return Cmd.Run(args, () =>
            {
                // MARK: Validation
                if (!CommandValidator.ValidateCampaignState(out string error))
                    return CommandResult.Error(error);

                string usageMessage = CommandValidator.CreateUsageMessage(
                    "gm.hero.load_development", "<hero> <filename>",
                    "Loads a hero's full development state (skills, attributes, perks, focus, XP, level) from a JSON file.\n" +
                    "Recalculates level after loading to ensure it matches the loaded skills.\n" +
                    "- hero: required, hero name or ID\n" +
                    "- filename: required, name of the save file (without .json extension)\n" +
                    "Supports named arguments: hero:derthert filename:derthert_build\n",
                    "gm.hero.load_development derthert derthert_build\n" +
                    "gm.hero.load_development 'Ira of the Aserai' warrior_build\n" +
                    "gm.hero.load_development hero:derthert filename:king_build");

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
                string heroQuery = parsed.GetArgument("hero", 0);
                if (string.IsNullOrWhiteSpace(heroQuery))
                    return CommandResult.Error(MessageFormatter.FormatErrorMessage("Hero argument cannot be empty."));

                EntityFinderResult<Hero> heroResult = HeroFinder.FindSingleHero(heroQuery);
                if (!heroResult.IsSuccess)
                    return CommandResult.Error(heroResult.Message);
                Hero hero = heroResult.Entity;

                string filename = parsed.GetArgument("filename", 1);
                if (string.IsNullOrWhiteSpace(filename))
                    return CommandResult.Error(MessageFormatter.FormatErrorMessage("Filename cannot be empty."));

                // MARK: Execute Logic
                string filepath = DevelopmentFileManager.Default.GetDevelopmentFilePath(filename);

                if (!DevelopmentFileManager.Default.DevelopmentFileExists(filename))
                    return CommandResult.Error(MessageFormatter.FormatErrorMessage($"Development file not found: {Path.GetFileName(filepath)}"));

                BLGMResult loadResult = DevelopmentFileManager.Default.LoadDevelopmentToHero(hero, filepath);

                if (!loadResult.IsSuccess)
                    return CommandResult.Error(loadResult.Message);

                Dictionary<string, string> resolvedValues = new()
                {
                    { "hero", hero.Name.ToString() },
                    { "filename", Path.GetFileName(filepath) }
                };

                StringBuilder result = new();
                result.AppendLine(parsed.FormatArgumentDisplay("gm.hero.load_development", resolvedValues));
                result.AppendLine(MessageFormatter.FormatSuccessMessage(loadResult.Message));

                return CommandResult.Success(result.ToString());
            }).Message;
        }
    }
}
