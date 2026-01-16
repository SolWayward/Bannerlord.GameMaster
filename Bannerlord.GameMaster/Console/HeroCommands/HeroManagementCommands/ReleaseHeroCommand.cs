using Bannerlord.GameMaster.Console.Common.Execution;
using Bannerlord.GameMaster.Console.Common.EntityFinding;
using Bannerlord.GameMaster.Console.Common.Formatting;
using Bannerlord.GameMaster.Console.Common.Parsing;
using Bannerlord.GameMaster.Console.Common.Validation;
using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.Library;

namespace Bannerlord.GameMaster.Console.HeroCommands.HeroManagementCommands;

/// <summary>
/// Release a hero from prison
/// Usage: gm.hero.release [hero]
/// </summary>
public static class ReleaseHeroCommand
{
    [CommandLineFunctionality.CommandLineArgumentFunction("release", "gm.hero")]
    public static string ReleaseHero(List<string> args)
    {
        return Cmd.Run(args, () =>
        {
            // MARK: Validation
            if (!CommandValidator.ValidateCampaignState(out string error))
                return error;

            string usageMessage = CommandValidator.CreateUsageMessage(
                "gm.hero.release", "<hero>",
                "Releases a hero from prison.\n" +
                "Supports named arguments: hero:lord_1_1",
                "gm.hero.release lord_1_1");

            ParsedArguments parsed = ArgumentParser.ParseArguments(args);

            parsed.SetValidArguments(
                new ArgumentDefinition("hero", true)
            );

            string validationError = parsed.GetValidationError();
            if (validationError != null)
                return MessageFormatter.FormatErrorMessage(validationError);

            if (parsed.TotalCount < 1)
                return usageMessage;

            // MARK: Parse Arguments
            string heroArg = parsed.GetArgument("hero", 0);
            if (heroArg == null)
                return MessageFormatter.FormatErrorMessage("Missing required argument 'hero'.");

            EntityFinderResult<Hero> heroResult = HeroFinder.FindSingleHero(heroArg);
            if (!heroResult.IsSuccess) return heroResult.Message;
            Hero hero = heroResult.Entity;

            if (!hero.IsPrisoner)
                return MessageFormatter.FormatErrorMessage($"{hero.Name} is not a prisoner.");

            // MARK: Execute Logic
            Dictionary<string, string> resolvedValues = new()
            {
                { "hero", hero.Name.ToString() }
            };

            EndCaptivityAction.ApplyByReleasedAfterBattle(hero);

            string argumentDisplay = parsed.FormatArgumentDisplay("release", resolvedValues);
            return argumentDisplay + MessageFormatter.FormatSuccessMessage($"{hero.Name} (ID: {hero.StringId}) has been released from captivity.");
        });
    }
}
