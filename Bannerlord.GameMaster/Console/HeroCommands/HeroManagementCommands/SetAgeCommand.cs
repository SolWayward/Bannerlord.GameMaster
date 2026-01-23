using Bannerlord.GameMaster.Console.Common;
using Bannerlord.GameMaster.Console.Common.Execution;
using Bannerlord.GameMaster.Console.Common.EntityFinding;
using Bannerlord.GameMaster.Console.Common.Formatting;
using Bannerlord.GameMaster.Console.Common.Parsing;
using Bannerlord.GameMaster.Console.Common.Validation;
using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Library;

namespace Bannerlord.GameMaster.Console.HeroCommands.HeroManagementCommands;

/// <summary>
/// Change hero's age
/// Usage: gm.hero.set_age [hero] [age]
/// </summary>
public static class SetAgeCommand
{
    [CommandLineFunctionality.CommandLineArgumentFunction("set_age", "gm.hero")]
    public static string SetAge(List<string> args)
    {
        return Cmd.Run(args, () =>
        {
            // MARK: Validation
            if (!CommandValidator.ValidateCampaignState(out string error))
                return CommandResult.Error(error).Message;

            string usageMessage = CommandValidator.CreateUsageMessage(
                "gm.hero.set_age", "<hero> <age>",
                "Sets the hero's age.\n" +
                "Supports named arguments: hero:lord_1_1 age:30",
                "gm.hero.set_age lord_1_1 30");

            ParsedArguments parsed = ArgumentParser.ParseArguments(args);

            parsed.SetValidArguments(
                new ArgumentDefinition("hero", true),
                new ArgumentDefinition("age", true)
            );

            string validationError = parsed.GetValidationError();
            if (validationError != null)
                return CommandResult.Error(MessageFormatter.FormatErrorMessage(validationError)).Message;

            if (parsed.TotalCount < 2)
                return CommandResult.Error(usageMessage).Message;

            // MARK: Parse Arguments
            string heroArg = parsed.GetArgument("hero", 0);
            if (heroArg == null)
                return CommandResult.Error(MessageFormatter.FormatErrorMessage("Missing required argument 'hero'.")).Message;

            EntityFinderResult<Hero> heroResult = HeroFinder.FindSingleHero(heroArg);
            if (!heroResult.IsSuccess) return CommandResult.Error(heroResult.Message).Message;
            Hero hero = heroResult.Entity;

            string ageArg = parsed.GetArgument("age", 1);
            if (ageArg == null)
                return CommandResult.Error(MessageFormatter.FormatErrorMessage("Missing required argument 'age'.")).Message;

            if (!CommandValidator.ValidateFloatRange(ageArg, 0, 128, out float age, out string ageError))
                return CommandResult.Error(MessageFormatter.FormatErrorMessage(ageError)).Message;

            // MARK: Execute Logic
            Dictionary<string, string> resolvedValues = new()
            {
                { "hero", hero.Name.ToString() },
                { "age", age.ToString("F0") }
            };

            float previousAge = hero.Age;
            hero.SetBirthDay(CampaignTime.YearsFromNow(-age));

            string argumentDisplay = parsed.FormatArgumentDisplay("gm.hero.set_age", resolvedValues);
            string fullMessage = argumentDisplay + MessageFormatter.FormatSuccessMessage($"{hero.Name}'s age changed from {previousAge:F0} to {hero.Age:F0}.");
            return CommandResult.Success(fullMessage).Message;
        });
    }
}
