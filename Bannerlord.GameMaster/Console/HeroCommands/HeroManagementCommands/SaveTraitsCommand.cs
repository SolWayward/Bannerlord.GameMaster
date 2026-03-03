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
using TaleWorlds.CampaignSystem.CharacterDevelopment;
using TaleWorlds.Library;

namespace Bannerlord.GameMaster.Console.HeroCommands.HeroManagementCommands
{
    /// <summary>
    /// Console command to save a hero's traits to a JSON file.
    /// </summary>
    public static class SaveTraitsCommand
    {
        /// <summary>
        /// Save a hero's traits to a JSON file.
        /// Usage: gm.hero.save_traits &lt;hero&gt; &lt;filename&gt;
        /// </summary>
        [CommandLineFunctionality.CommandLineArgumentFunction("save_traits", "gm.hero")]
        public static string SaveTraits(List<string> args)
        {
            return Cmd.Run(args, () =>
            {
                // MARK: Validation
                if (!CommandValidator.ValidateCampaignState(out string error))
                    return CommandResult.Error(error);

                string usageMessage = CommandValidator.CreateUsageMessage(
                    "gm.hero.save_traits", "<hero> <filename>",
                    "Saves all of a hero's traits (personality, persona, political, role/skill) to a JSON file.\n" +
                    "Captures all traits including modded ones via TraitObject.All iteration.\n" +
                    "- hero: required, hero name or ID\n" +
                    "- filename: required, name for the save file (without .json extension)\n" +
                    "Supports named arguments: hero:derthert filename:derthert_personality\n",
                    "gm.hero.save_traits derthert derthert_personality\n" +
                    "gm.hero.save_traits 'Ira of the Aserai' ira_traits\n" +
                    "gm.hero.save_traits hero:derthert filename:king_traits");

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
                string filepath = TraitFileManager.Default.GetTraitFilePath(filename);
                TraitFileManager.Default.SaveTraitsToFile(hero, filepath);

                // Build personality summary from well-known traits
                StringBuilder personalitySummary = new();
                AppendPersonalityTrait(personalitySummary, hero, DefaultTraits.Mercy, "Mercy");
                AppendPersonalityTrait(personalitySummary, hero, DefaultTraits.Valor, "Valor");
                AppendPersonalityTrait(personalitySummary, hero, DefaultTraits.Honor, "Honor");
                AppendPersonalityTrait(personalitySummary, hero, DefaultTraits.Generosity, "Generosity");
                AppendPersonalityTrait(personalitySummary, hero, DefaultTraits.Calculating, "Calculating");

                int totalTraits = 0;
                foreach (TraitObject trait in TraitObject.All)
                {
                    totalTraits++;
                }

                Dictionary<string, string> resolvedValues = new()
                {
                    { "hero", hero.Name.ToString() },
                    { "filename", Path.GetFileName(filepath) }
                };

                StringBuilder result = new();
                result.AppendLine(parsed.FormatArgumentDisplay("gm.hero.save_traits", resolvedValues));
                result.AppendLine(MessageFormatter.FormatSuccessMessage(
                    $"Saved {hero.Name}'s traits to: {Path.GetFileName(filepath)}"));
                result.AppendLine($"Total traits saved: {totalTraits}");
                result.AppendLine($"Personality: {personalitySummary}");

                return CommandResult.Success(result.ToString());
            }).Message;
        }

        /// <summary>
        /// Appends a personality trait value to the summary string.
        /// </summary>
        private static void AppendPersonalityTrait(StringBuilder sb, Hero hero, TraitObject trait, string name)
        {
            if (sb.Length > 0)
                sb.Append(", ");

            int level = hero.GetTraitLevel(trait);
            sb.Append($"{name}: {level}");
        }
    }
}
