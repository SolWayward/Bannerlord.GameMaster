using Bannerlord.GameMaster.Common;
using Bannerlord.GameMaster.Console.Common;
using Bannerlord.GameMaster.Console.Common.Execution;
using Bannerlord.GameMaster.Console.Common.EntityFinding;
using Bannerlord.GameMaster.Console.Common.Formatting;
using Bannerlord.GameMaster.Console.Common.Parsing;
using Bannerlord.GameMaster.Console.Common.Validation;
using Bannerlord.GameMaster.Heroes;
using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Library;

namespace Bannerlord.GameMaster.Console.HeroCommands.HeroManagementCommands;

/// <summary>
/// Forces immediate birth for a pregnant hero.
/// Usage: gm.hero.give_birth [mother] [father] [gender]
/// </summary>
public static class GiveBirthCommand
{
    [CommandLineFunctionality.CommandLineArgumentFunction("give_birth", "gm.hero")]
    public static string GiveBirth(List<string> args)
    {
        return Cmd.Run(args, () =>
        {
            // MARK: Validation
            if (!CommandValidator.ValidateCampaignState(out string error))
                return CommandResult.Error(error);

            string usageMessage = CommandValidator.CreateUsageMessage(
                "gm.hero.give_birth", "<mother> [father] [gender]",
                "Forces immediate birth for a pregnant hero.\n" +
                "- mother/hero: required, the pregnant female hero (name or ID). Use 'player' for your hero.\n" +
                "- father: optional, the father hero (name or ID). If not specified, resolved from pregnancy record or spouse.\n" +
                "- gender: optional, gender of the child: 'male' or 'female'. Default: random.",
                "gm.hero.give_birth Ira\n" +
                "gm.hero.give_birth Ira female\n" +
                "gm.hero.give_birth 'Liena the Fierce' Derthert male\n" +
                "gm.hero.give_birth mother:Ira father:Derthert gender:female\n" +
                "gm.hero.give_birth hero:Ira gender:male");

            ParsedArguments parsed = ArgumentParser.ParseArguments(args);

            parsed.SetValidArguments(
                new ArgumentDefinition("mother", true, null, "hero"),
                new ArgumentDefinition("father", false),
                new ArgumentDefinition("gender", false)
            );

            string validationError = parsed.GetValidationError();
            if (validationError != null)
                return CommandResult.Error(MessageFormatter.FormatErrorMessage(validationError));

            if (parsed.TotalCount < 1)
                return CommandResult.Error(usageMessage);

            // MARK: Parse Arguments
            string motherArg = parsed.GetArgument("mother", 0) ?? parsed.GetArgument("hero", 0);
            if (string.IsNullOrWhiteSpace(motherArg))
                return CommandResult.Error(MessageFormatter.FormatErrorMessage("Missing required argument 'mother'."));

            EntityFinderResult<Hero> motherResult = HeroFinder.FindSingleHero(motherArg);
            if (!motherResult.IsSuccess)
                return CommandResult.Error(motherResult.Message);

            Hero mother = motherResult.Entity;

            if (!mother.IsFemale)
                return CommandResult.Error(MessageFormatter.FormatErrorMessage($"Mother must be female. {mother.Name} is male."));

            if (!mother.IsPregnant)
                return CommandResult.Error(MessageFormatter.FormatErrorMessage($"{mother.Name} is not pregnant."));

            if (!mother.IsAlive)
                return CommandResult.Error(MessageFormatter.FormatErrorMessage($"{mother.Name} is not alive."));

            // Parse father (optional)
            Hero father = null;
            string fatherArg = parsed.GetArgument("father", 1);

            // Determine if positional arg 1 is a father or a gender keyword
            if (fatherArg != null && IsGenderKeyword(fatherArg))
            {
                // Positional 1 is actually the gender, not the father
                fatherArg = null;
            }

            if (!string.IsNullOrWhiteSpace(fatherArg))
            {
                EntityFinderResult<Hero> fatherResult = HeroFinder.FindSingleHero(fatherArg);
                if (!fatherResult.IsSuccess)
                    return CommandResult.Error(fatherResult.Message);

                father = fatherResult.Entity;

                if (father.IsFemale)
                    return CommandResult.Error(MessageFormatter.FormatErrorMessage($"Father must be male. {father.Name} is female."));
            }

            // Parse gender (optional, default random)
            bool isFemale;
            string genderArg = parsed.GetArgument("gender", father != null ? 2 : 1);

            // Also check positional 2 if father was specified, or positional 1 if father was not
            if (genderArg == null)
            {
                // Scan positional args for gender keyword
                for (int i = 1; i < parsed.PositionalCount; i++)
                {
                    if (IsGenderKeyword(parsed.GetPositional(i)))
                    {
                        genderArg = parsed.GetPositional(i);
                        break;
                    }
                }
            }

            if (genderArg != null)
            {
                string genderLower = genderArg.ToLower();
                if (genderLower == "female" || genderLower == "f")
                    isFemale = true;
                else if (genderLower == "male" || genderLower == "m")
                    isFemale = false;
                else
                    return CommandResult.Error(MessageFormatter.FormatErrorMessage($"Invalid gender: '{genderArg}'. Use 'male' or 'female'."));
            }
            else
            {
                isFemale = RandomNumberGen.Instance.NextRandomInt(0, 2) == 0;
            }

            // MARK: Execute Logic
            BLGMResult result;
            if (father != null)
                result = HeroManager.GiveBirth(mother, father, isFemale);
            else
                result = HeroManager.GiveBirth(mother, isFemale);

            Dictionary<string, string> resolvedValues = new()
            {
                { "mother", mother.Name.ToString() },
                { "father", father != null ? father.Name.ToString() : "Auto-resolved" },
                { "gender", isFemale ? "Female" : "Male" }
            };

            string argumentDisplay = parsed.FormatArgumentDisplay("gm.hero.give_birth", resolvedValues);

            if (!result.IsSuccess)
                return CommandResult.Error(argumentDisplay + MessageFormatter.FormatErrorMessage(result.Message));

            return CommandResult.Success(argumentDisplay + MessageFormatter.FormatSuccessMessage(result.Message));
        }).Message;
    }

    /// <summary>
    /// Checks if a string is a gender keyword (male/female/m/f).
    /// </summary>
    private static bool IsGenderKeyword(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return false;

        string lower = value.ToLower();
        return lower == "male" || lower == "female" || lower == "m" || lower == "f";
    }
}
