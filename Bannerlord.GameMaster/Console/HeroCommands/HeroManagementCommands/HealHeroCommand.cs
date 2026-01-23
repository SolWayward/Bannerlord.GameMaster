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
/// Heal a hero to full health
/// Usage: gm.hero.heal [hero]
/// </summary>
public static class HealHeroCommand
{
    [CommandLineFunctionality.CommandLineArgumentFunction("heal", "gm.hero")]
    public static string HealHero(List<string> args)
    {
        return Cmd.Run(args, () =>
        {
            // MARK: Validation
            if (!CommandValidator.ValidateCampaignState(out string error))
                return CommandResult.Error(error).Message;

            string usageMessage = CommandValidator.CreateUsageMessage(
                "gm.hero.heal", "<hero>",
                "Heals a hero to full health.\n" +
                "Supports named arguments: hero:lord_1_1",
                "gm.hero.heal lord_1_1");

            ParsedArguments parsed = ArgumentParser.ParseArguments(args);

            parsed.SetValidArguments(
                new ArgumentDefinition("hero", true)
            );

            string validationError = parsed.GetValidationError();
            if (validationError != null)
                return CommandResult.Error(MessageFormatter.FormatErrorMessage(validationError)).Message;

            if (parsed.TotalCount < 1)
                return CommandResult.Error(usageMessage).Message;

            // MARK: Parse Arguments
            string heroArg = parsed.GetArgument("hero", 0);
            if (heroArg == null)
                return CommandResult.Error(MessageFormatter.FormatErrorMessage("Missing required argument 'hero'.")).Message;

            EntityFinderResult<Hero> heroResult = HeroFinder.FindSingleHero(heroArg);
            if (!heroResult.IsSuccess) return CommandResult.Error(heroResult.Message).Message;
            Hero hero = heroResult.Entity;

            if (!hero.IsAlive)
                return CommandResult.Error(MessageFormatter.FormatErrorMessage($"Cannot heal {hero.Name} - hero is dead.")).Message;

            // MARK: Execute Logic
            Dictionary<string, string> resolvedValues = new()
            {
                { "hero", hero.Name.ToString() }
            };

            hero.HitPoints = hero.CharacterObject.MaxHitPoints();

            string argumentDisplay = parsed.FormatArgumentDisplay("gm.hero.heal", resolvedValues);
            string fullMessage = argumentDisplay + MessageFormatter.FormatSuccessMessage(
                $"{hero.Name} has been healed to full health ({hero.HitPoints}/{hero.CharacterObject.MaxHitPoints()}).");
            return CommandResult.Success(fullMessage).Message;
        });
    }
}
