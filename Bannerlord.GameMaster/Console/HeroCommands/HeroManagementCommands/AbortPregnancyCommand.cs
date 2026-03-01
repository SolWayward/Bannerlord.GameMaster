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
/// Terminates a hero's pregnancy without delivering a child.
/// Usage: gm.hero.abort_pregnancy [hero]
/// </summary>
public static class AbortPregnancyCommand
{
    [CommandLineFunctionality.CommandLineArgumentFunction("abort_pregnancy", "gm.hero")]
    public static string AbortPregnancy(List<string> args)
    {
        return Cmd.Run(args, () =>
        {
            // MARK: Validation
            if (!CommandValidator.ValidateCampaignState(out string error))
                return CommandResult.Error(error);

            string usageMessage = CommandValidator.CreateUsageMessage(
                "gm.hero.abort_pregnancy", "<hero>",
                "Terminates a hero's pregnancy without delivering a child.\n" +
                "- hero: required, the pregnant hero (name or ID). Use 'player' for your hero.",
                "gm.hero.abort_pregnancy Ira\n" +
                "gm.hero.abort_pregnancy hero:Ira\n" +
                "gm.hero.abort_pregnancy lord_1_3\n" +
                "gm.hero.abort_pregnancy 'Liena the Fierce'");

            ParsedArguments parsed = ArgumentParser.ParseArguments(args);

            parsed.SetValidArguments(
                new ArgumentDefinition("hero", true)
            );

            string validationError = parsed.GetValidationError();
            if (validationError != null)
                return CommandResult.Error(MessageFormatter.FormatErrorMessage(validationError));

            if (parsed.TotalCount < 1)
                return CommandResult.Success(usageMessage);

            // MARK: Parse Arguments
            string heroArg = parsed.GetArgument("hero", 0);
            if (string.IsNullOrWhiteSpace(heroArg))
                return CommandResult.Error(MessageFormatter.FormatErrorMessage("Missing required argument 'hero'."));

            EntityFinderResult<Hero> heroResult = HeroFinder.FindSingleHero(heroArg);
            if (!heroResult.IsSuccess)
                return CommandResult.Error(heroResult.Message);

            Hero hero = heroResult.Entity;

            if (!hero.IsPregnant)
                return CommandResult.Error(MessageFormatter.FormatErrorMessage(
                    $"{hero.Name} is not pregnant."));

            // MARK: Execute Logic
            BLGMResult result = HeroManager.AbortBirth(hero);

            Dictionary<string, string> resolvedValues = new()
            {
                { "hero", hero.Name.ToString() }
            };

            string argumentDisplay = parsed.FormatArgumentDisplay("gm.hero.abort_pregnancy", resolvedValues);

            if (!result.IsSuccess)
                return CommandResult.Error(argumentDisplay + MessageFormatter.FormatErrorMessage(result.Message));

            return CommandResult.Success(argumentDisplay + MessageFormatter.FormatSuccessMessage(
                $"Pregnancy terminated for {hero.Name}."));
        }).Message;
    }
}
