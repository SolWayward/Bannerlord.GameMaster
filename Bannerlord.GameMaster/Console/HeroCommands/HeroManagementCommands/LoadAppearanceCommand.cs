using Bannerlord.GameMaster.Common;
using Bannerlord.GameMaster.Console.Common;
using Bannerlord.GameMaster.Console.Common.EntityFinding;
using Bannerlord.GameMaster.Console.Common.Execution;
using Bannerlord.GameMaster.Console.Common.Formatting;
using Bannerlord.GameMaster.Console.Common.Parsing;
using Bannerlord.GameMaster.Console.Common.Validation;
using Bannerlord.GameMaster.Heroes;
using System.Collections.Generic;
using System.IO;
using System.Text;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Library;

namespace Bannerlord.GameMaster.Console.HeroCommands.HeroManagementCommands
{
    /// <summary>
    /// Console command to load a hero's appearance from a JSON file.
    /// </summary>
    public static class LoadAppearanceCommand
    {
        /// <summary>
        /// Load a hero's appearance from a JSON file.
        /// Usage: gm.hero.load_appearance &lt;hero&gt; &lt;filename&gt; [force]
        /// </summary>
        [CommandLineFunctionality.CommandLineArgumentFunction("load_appearance", "gm.hero")]
        public static string LoadAppearance(List<string> args)
        {
            return Cmd.Run(args, () =>
            {
                // MARK: Validation
                if (!CommandValidator.ValidateCampaignState(out string error))
                    return CommandResult.Error(error);

                string usageMessage = CommandValidator.CreateUsageMessage(
                    "gm.hero.load_appearance", "<hero> <filename> [force]",
                    "Loads a hero's appearance (face, hair, tattoos, body shape, height) from a JSON file.\n" +
                    "Age is NOT applied (tied to hero birth date/timeline).\n" +
                    "- hero: required, hero name or ID\n" +
                    "- filename: required, name of the save file (without .json extension)\n" +
                    "- force: optional, set to true to allow loading across genders (default: false)\n" +
                    "Supports named arguments: hero:derthert filename:my_face force:true\n",
                    "gm.hero.load_appearance derthert my_face\n" +
                    "gm.hero.load_appearance 'Ira of the Aserai' warrior_face force:true\n" +
                    "gm.hero.load_appearance hero:derthert filename:king_look");

                ParsedArguments parsed = ArgumentParser.ParseArguments(args);

                parsed.SetValidArguments(
                    new ArgumentDefinition("hero", true),
                    new ArgumentDefinition("filename", true),
                    new ArgumentDefinition("force", false, "false")
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

                bool forceGenderMismatch = parsed.GetBool("force", 2, false);

                // MARK: Execute Logic
                string filepath = AppearanceFileManager.Default.GetAppearanceFilePath(filename);

                if (!AppearanceFileManager.Default.AppearanceFileExists(filename))
                    return CommandResult.Error(MessageFormatter.FormatErrorMessage($"Appearance file not found: {Path.GetFileName(filepath)}"));

                BLGMResult loadResult = AppearanceFileManager.Default.LoadAppearanceToHero(hero, filepath, forceGenderMismatch);

                if (!loadResult.IsSuccess)
                    return CommandResult.Error(loadResult.Message);

                Dictionary<string, string> resolvedValues = new()
                {
                    { "hero", hero.Name.ToString() },
                    { "filename", Path.GetFileName(filepath) },
                    { "force", forceGenderMismatch.ToString() }
                };

                StringBuilder result = new();
                result.AppendLine(parsed.FormatArgumentDisplay("gm.hero.load_appearance", resolvedValues));
                result.AppendLine(MessageFormatter.FormatSuccessMessage(loadResult.Message));

                return CommandResult.Success(result.ToString());
            }).Message;
        }
    }
}
