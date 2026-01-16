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
/// Kill a hero
/// Usage: gm.hero.kill [hero] [optional: show_death_log]
/// </summary>
public static class KillHeroCommand
{
    [CommandLineFunctionality.CommandLineArgumentFunction("kill", "gm.hero")]
    public static string KillHero(List<string> args)
    {
        return Cmd.Run(args, () =>
        {
            // MARK: Validation
            if (!CommandValidator.ValidateCampaignState(out string error))
                return error;

            string usageMessage = CommandValidator.CreateUsageMessage(
                "gm.hero.kill", "<hero> [show_death_log]",
                "Kills the specified hero.\n" +
                "Supports named arguments: hero:lord_1_1 showDeathLog:true",
                "gm.hero.kill lord_1_1 true (shows death log)");

            ParsedArguments parsed = ArgumentParser.ParseArguments(args);

            parsed.SetValidArguments(
                new ArgumentDefinition("hero", true),
                new ArgumentDefinition("showDeathLog", false, null, "show_death_log")
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

            if (!hero.IsAlive)
                return MessageFormatter.FormatErrorMessage($"{hero.Name} is already dead.");

            bool showDeathLog = false;
            string showDeathLogArg = parsed.GetArgument("showDeathLog", 1) ?? parsed.GetNamed("show_death_log");
            if (showDeathLogArg != null)
            {
                if (!CommandValidator.ValidateBoolean(showDeathLogArg, out showDeathLog, out string boolError))
                    return MessageFormatter.FormatErrorMessage(boolError);
            }

            // MARK: Execute Logic
            Dictionary<string, string> resolvedValues = new()
            {
                { "hero", hero.Name.ToString() },
                { "showDeathLog", showDeathLog.ToString() }
            };

            KillCharacterAction.ApplyByMurder(hero, null, showDeathLog);

            string argumentDisplay = parsed.FormatArgumentDisplay("kill", resolvedValues);
            return argumentDisplay + MessageFormatter.FormatSuccessMessage($"{hero.Name} (ID: {hero.StringId}) has been killed.");
        });
    }
}
