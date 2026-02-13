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
/// Marry two heroes
/// Usage: gm.hero.marry [hero] [otherHero] [forceMarriage:false] [joinClan:true]
/// </summary>
public static class MarryCommand
{
    [CommandLineFunctionality.CommandLineArgumentFunction("marry", "gm.hero")]
    public static string Marry(List<string> args)
    {
        return Cmd.Run(args, () =>
        {
            // MARK: Validation
            if (!CommandValidator.ValidateCampaignState(out string error))
                return CommandResult.Error(error);

            string usageMessage = CommandValidator.CreateUsageMessage(
                "gm.hero.marry", "<hero> <otherHero> [forceMarriage:false] [joinClan:true]",
                "Marry two heroes.\n" +
                "- hero/hero1: required, the first hero (name or ID). otherHero joins this hero's clan by default\n" +
                "- otherHero/hero2: required, the second hero (name or ID)\n" +
                "- forceMarriage/force: optional, set to true to bypass native validation (default: false)\n" +
                "  Native validation requires: opposite gender, not related, both unmarried, both alive,\n" +
                "  age 18+, not both clan leaders, valid clans, not in army/battle\n" +
                "- joinClan: optional, set to false to keep both heroes in their original clans (default: true)\n" +
                "  When true, otherHero joins hero's clan",
                "gm.hero.marry player Ira\n" +
                "gm.hero.marry hero:'Rhagaea' otherHero:Derthert forceMarriage:true\n" +
                "gm.hero.marry Derthert Ira joinClan:false\n" +
                "gm.hero.marry hero1:'Liena the Fierce' hero2:player force:true");

            ParsedArguments parsed = ArgumentParser.ParseArguments(args);

            parsed.SetValidArguments(
                new ArgumentDefinition("hero", true, null, "hero1"),
                new ArgumentDefinition("otherHero", true, null, "hero2"),
                new ArgumentDefinition("forceMarriage", false, null, "force"),
                new ArgumentDefinition("joinClan", false)
            );

            string validationError = parsed.GetValidationError();
            if (validationError != null)
                return CommandResult.Error(MessageFormatter.FormatErrorMessage(validationError));

            if (parsed.TotalCount < 2)
                return CommandResult.Success(usageMessage);

            // MARK: Parse Arguments
            string heroArg = parsed.GetArgument("hero", 0) ?? parsed.GetArgument("hero1", 0);
            if (string.IsNullOrWhiteSpace(heroArg))
                return CommandResult.Error(MessageFormatter.FormatErrorMessage("Missing required argument 'hero'."));

            string otherHeroArg = parsed.GetArgument("otherHero", 1) ?? parsed.GetArgument("hero2", 1);
            if (string.IsNullOrWhiteSpace(otherHeroArg))
                return CommandResult.Error(MessageFormatter.FormatErrorMessage("Missing required argument 'otherHero'."));

            EntityFinderResult<Hero> heroResult = HeroFinder.FindSingleHero(heroArg);
            if (!heroResult.IsSuccess)
                return CommandResult.Error(heroResult.Message);

            Hero hero = heroResult.Entity;

            EntityFinderResult<Hero> otherHeroResult = HeroFinder.FindSingleHero(otherHeroArg);
            if (!otherHeroResult.IsSuccess)
                return CommandResult.Error(otherHeroResult.Message);

            Hero otherHero = otherHeroResult.Entity;

            // Command-level validation (quick checks before calling HeroManager)
            if (hero == otherHero)
                return CommandResult.Error(MessageFormatter.FormatErrorMessage("Cannot marry a hero to themselves."));

            // Parse forceMarriage (positional 2 or named, default false)
            bool forceMarriage = false;
            string forceArg = parsed.GetArgument("forceMarriage", 2) ?? parsed.GetNamed("force");
            if (forceArg != null)
            {
                if (!CommandValidator.ValidateBoolean(forceArg, out forceMarriage, out string boolError))
                    return CommandResult.Error(MessageFormatter.FormatErrorMessage(boolError));
            }

            // Parse joinClan (positional 3 or named, default true)
            bool joinClan = true;
            string joinClanArg = parsed.GetArgument("joinClan", 3);
            if (joinClanArg != null)
            {
                if (!CommandValidator.ValidateBoolean(joinClanArg, out joinClan, out string boolError))
                    return CommandResult.Error(MessageFormatter.FormatErrorMessage(boolError));
            }

            // MARK: Execute Logic
            BLGMResult result = HeroManager.Marry(hero, otherHero, forceMarriage, joinClan);

            Dictionary<string, string> resolvedValues = new()
            {
                { "hero", hero.Name.ToString() },
                { "otherHero", otherHero.Name.ToString() },
                { "forceMarriage", forceMarriage.ToString().ToLower() },
                { "joinClan", joinClan.ToString().ToLower() }
            };

            string argumentDisplay = parsed.FormatArgumentDisplay("gm.hero.marry", resolvedValues);

            if (!result.IsSuccess)
                return CommandResult.Error(argumentDisplay + MessageFormatter.FormatErrorMessage(result.Message));

            return CommandResult.Success(argumentDisplay + MessageFormatter.FormatSuccessMessage(result.Message));
        }).Message;
    }
}
