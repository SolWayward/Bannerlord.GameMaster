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
using TaleWorlds.CampaignSystem.CharacterDevelopment;
using TaleWorlds.Library;

namespace Bannerlord.GameMaster.Console.HeroCommands.HeroManagementCommands
{
    /// <summary>
    /// Console command to save a hero's development (skills, attributes, perks) to a JSON file.
    /// </summary>
    public static class SaveDevelopmentCommand
    {
        /// <summary>
        /// Save a hero's development to a JSON file.
        /// Usage: gm.hero.save_development &lt;hero&gt; &lt;filename&gt;
        /// </summary>
        [CommandLineFunctionality.CommandLineArgumentFunction("save_development", "gm.hero")]
        public static string SaveDevelopment(List<string> args)
        {
            return Cmd.Run(args, () =>
            {
                // MARK: Validation
                if (!CommandValidator.ValidateCampaignState(out string error))
                    return CommandResult.Error(error);

                string usageMessage = CommandValidator.CreateUsageMessage(
                    "gm.hero.save_development", "<hero> <filename>",
                    "Saves a hero's full development state (skills, attributes, perks, focus, XP, level) to a JSON file.\n" +
                    "- hero: required, hero name or ID\n" +
                    "- filename: required, name for the save file (without .json extension)\n" +
                    "Supports named arguments: hero:derthert filename:derthert_build\n",
                    "gm.hero.save_development derthert derthert_build\n" +
                    "gm.hero.save_development 'Ira of the Aserai' ira_skills\n" +
                    "gm.hero.save_development hero:derthert filename:king_build");

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
                DevelopmentFileManager.Default.SaveDevelopmentToFile(hero, filepath);

                // Count selected perks
                int selectedPerks = 0;
                foreach (PerkObject perk in PerkObject.All)
                {
                    if (hero.GetPerkValue(perk))
                        selectedPerks++;
                }

                Dictionary<string, string> resolvedValues = new()
                {
                    { "hero", hero.Name.ToString() },
                    { "filename", Path.GetFileName(filepath) }
                };

                StringBuilder result = new();
                result.AppendLine(parsed.FormatArgumentDisplay("gm.hero.save_development", resolvedValues));
                result.AppendLine(MessageFormatter.FormatSuccessMessage(
                    $"Saved {hero.Name}'s development to: {Path.GetFileName(filepath)}"));
                result.AppendLine($"Level: {hero.Level} | Total XP: {hero.HeroDeveloper.TotalXp}");
                result.AppendLine($"Skills: {TaleWorlds.CampaignSystem.Extensions.Skills.All.Count} | Perks Selected: {selectedPerks}");
                result.AppendLine($"Unspent Attribute Points: {hero.HeroDeveloper.UnspentAttributePoints}");
                result.AppendLine($"Unspent Focus Points: {hero.HeroDeveloper.UnspentFocusPoints}");

                return CommandResult.Success(result.ToString());
            }).Message;
        }
    }
}
