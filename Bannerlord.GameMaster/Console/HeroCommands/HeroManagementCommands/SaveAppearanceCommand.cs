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
    /// Console command to save a hero's appearance to a JSON file.
    /// </summary>
    public static class SaveAppearanceCommand
    {
        /// <summary>
        /// Save a hero's appearance to a JSON file.
        /// Usage: gm.hero.save_appearance &lt;hero&gt; &lt;filename&gt;
        /// </summary>
        [CommandLineFunctionality.CommandLineArgumentFunction("save_appearance", "gm.hero")]
        public static string SaveAppearance(List<string> args)
        {
            return Cmd.Run(args, () =>
            {
                // MARK: Validation
                if (!CommandValidator.ValidateCampaignState(out string error))
                    return CommandResult.Error(error);

                string usageMessage = CommandValidator.CreateUsageMessage(
                    "gm.hero.save_appearance", "<hero> <filename>",
                    "Saves a hero's appearance (face, hair, tattoos, body shape, height) to a JSON file.\n" +
                    "- hero: required, hero name or ID\n" +
                    "- filename: required, name for the save file (without .json extension)\n" +
                    "Supports named arguments: hero:derthert filename:my_face\n",
                    "gm.hero.save_appearance derthert my_face\n" +
                    "gm.hero.save_appearance 'Ira of the Aserai' ira_look\n" +
                    "gm.hero.save_appearance hero:derthert filename:king_look");

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
                string filepath = AppearanceFileManager.Default.GetAppearanceFilePath(filename);
                AppearanceFileManager.Default.SaveAppearanceToFile(hero, filepath);

                Dictionary<string, string> resolvedValues = new()
                {
                    { "hero", hero.Name.ToString() },
                    { "filename", Path.GetFileName(filepath) }
                };

                string gender = hero.IsFemale ? "Female" : "Male";
                string culture = hero.Culture?.Name?.ToString() ?? "Unknown";

                StringBuilder result = new();
                result.AppendLine(parsed.FormatArgumentDisplay("gm.hero.save_appearance", resolvedValues));
                result.AppendLine(MessageFormatter.FormatSuccessMessage(
                    $"Saved {hero.Name}'s appearance to: {Path.GetFileName(filepath)}"));
                result.AppendLine($"Hero: {hero.Name} ({gender}, {culture})");
                result.AppendLine($"BodyProperties: {hero.BodyProperties}");

                return CommandResult.Success(result.ToString());
            }).Message;
        }
    }
}
