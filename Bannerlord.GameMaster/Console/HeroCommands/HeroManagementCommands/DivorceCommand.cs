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
/// Divorce a hero from their current spouse
/// Usage: gm.hero.divorce <hero>
/// </summary>
public static class DivorceCommand
{
    [CommandLineFunctionality.CommandLineArgumentFunction("divorce", "gm.hero")]
    public static string Divorce(List<string> args)
    {
        return Cmd.Run(args, () =>
        {
            // MARK: Validation
            if (!CommandValidator.ValidateCampaignState(out string error))
                return CommandResult.Error(error);

            string usageMessage = CommandValidator.CreateUsageMessage(
                "gm.hero.divorce", "<hero>",
                "Divorce a hero from their current spouse.\n" +
                "- hero: required, the hero to divorce (name or ID). Use 'player' for your hero.\n" +
                "  The hero's current spouse is automatically divorced as well.",
                "gm.hero.divorce player\n" +
                "gm.hero.divorce Derthert\n" +
                "gm.hero.divorce hero:'Rhagaea'");

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

            // Check if hero is actually married
            if (hero.Spouse == null)
                return CommandResult.Error(MessageFormatter.FormatErrorMessage(
                    $"{hero.Name} is not married to anyone."));

            string spouseName = hero.Spouse.Name.ToString();

            // MARK: Execute Logic
            BLGMResult result = HeroManager.Divorce(hero);

            Dictionary<string, string> resolvedValues = new()
            {
                { "hero", hero.Name.ToString() }
            };

            string argumentDisplay = parsed.FormatArgumentDisplay("gm.hero.divorce", resolvedValues);

            if (!result.IsSuccess)
                return CommandResult.Error(argumentDisplay + MessageFormatter.FormatErrorMessage(result.Message));

            return CommandResult.Success(argumentDisplay + MessageFormatter.FormatSuccessMessage(
                $"{hero.Name} has been divorced from {spouseName}"));
        }).Message;
    }
}
