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
/// Makes a female hero pregnant with an optional specified father.
/// Usage: gm.hero.impregnate [mother] [father] [allowAnyCulture:false]
/// </summary>
public static class ImpregnateCommand
{
    [CommandLineFunctionality.CommandLineArgumentFunction("impregnate", "gm.hero")]
    public static string Impregnate(List<string> args)
    {
        return Cmd.Run(args, () =>
        {
            // MARK: Validation
            if (!CommandValidator.ValidateCampaignState(out string error))
                return CommandResult.Error(error);

            string usageMessage = CommandValidator.CreateUsageMessage(
                "gm.hero.impregnate", "<mother> [father] [allowAnyCulture:false]",
                "Makes a female hero pregnant.\n" +
                "- mother/hero: required, the female hero to impregnate (name or ID)\n" +
                "- father: optional, the male hero to be the father (name or ID)\n" +
                "  If not specified: uses mother's spouse, or a random nearby male hero\n" +
                "- allowAnyCulture: optional (default: false), set to true to allow non-main-culture parents",
                "gm.hero.impregnate Ira\n" +
                "gm.hero.impregnate 'Liena the Fierce' Derthert\n" +
                "gm.hero.impregnate mother:Ira father:Derthert\n" +
                "gm.hero.impregnate hero:Ira allowAnyCulture:true");

            ParsedArguments parsed = ArgumentParser.ParseArguments(args);

            parsed.SetValidArguments(
                new ArgumentDefinition("mother", true, null, "hero"),
                new ArgumentDefinition("father", false),
                new ArgumentDefinition("allowAnyCulture", false)
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

            if (mother.IsPregnant)
                return CommandResult.Error(MessageFormatter.FormatErrorMessage($"{mother.Name} is already pregnant."));

            Hero father = null;
            string fatherArg = parsed.GetArgument("father", 1);
            if (!string.IsNullOrWhiteSpace(fatherArg))
            {
                EntityFinderResult<Hero> fatherResult = HeroFinder.FindSingleHero(fatherArg);
                if (!fatherResult.IsSuccess)
                    return CommandResult.Error(fatherResult.Message);

                father = fatherResult.Entity;

                if (father.IsFemale)
                    return CommandResult.Error(MessageFormatter.FormatErrorMessage($"Father must be male. {father.Name} is female."));
            }

            // Parse allowAnyCulture (positional 2 or named, default false)
            bool allowAnyCulture = false;
            string allowAnyCultureArg = parsed.GetArgument("allowAnyCulture", 2);
            if (allowAnyCultureArg != null)
            {
                if (!CommandValidator.ValidateBoolean(allowAnyCultureArg, out allowAnyCulture, out string boolError))
                    return CommandResult.Error(MessageFormatter.FormatErrorMessage(boolError));
            }

            // Main culture validation (command-only guard)
            if (!allowAnyCulture)
            {
                if (mother.Culture != null && !mother.Culture.IsMainCulture)
                {
                    return CommandResult.Error(MessageFormatter.FormatErrorMessage(
                        $"Mother '{mother.Name}' has non-main culture '{mother.Culture.Name}'.\n" +
                        "Children born to non-main-culture heroes may cause crashes.\n" +
                        "Use allowAnyCulture:true to bypass this check."));
                }

                if (father != null && father.Culture != null && !father.Culture.IsMainCulture)
                {
                    return CommandResult.Error(MessageFormatter.FormatErrorMessage(
                        $"Father '{father.Name}' has non-main culture '{father.Culture.Name}'.\n" +
                        "Children born to non-main-culture heroes may cause crashes.\n" +
                        "Use allowAnyCulture:true to bypass this check."));
                }
            }

            // MARK: Execute Logic
            BLGMResult result = HeroManager.Impregnate(mother, father);

            Dictionary<string, string> resolvedValues = new()
            {
                { "mother", mother.Name.ToString() },
                { "father", father != null ? father.Name.ToString() : "Auto-resolved" },
                { "allowAnyCulture", allowAnyCulture ? "true" : "false" }
            };

            string argumentDisplay = parsed.FormatArgumentDisplay("gm.hero.impregnate", resolvedValues);

            if (!result.IsSuccess)
                return CommandResult.Error(argumentDisplay + MessageFormatter.FormatErrorMessage(result.Message));

            return CommandResult.Success(argumentDisplay + MessageFormatter.FormatSuccessMessage(result.Message));
        }).Message;
    }
}
