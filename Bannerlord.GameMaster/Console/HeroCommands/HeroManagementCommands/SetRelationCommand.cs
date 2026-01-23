using Bannerlord.GameMaster.Console.Common;
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
/// Set relation between two heroes
/// Usage: gm.hero.set_relation [hero1] [hero2] [value]
/// </summary>
public static class SetRelationCommand
{
    [CommandLineFunctionality.CommandLineArgumentFunction("set_relation", "gm.hero")]
    public static string SetRelation(List<string> args)
    {
        return Cmd.Run(args, () =>
        {
            // MARK: Validation
            if (!CommandValidator.ValidateCampaignState(out string error))
                return CommandResult.Error(error).Message
;

            string usageMessage = CommandValidator.CreateUsageMessage(
                "gm.hero.set_relation", "<hero1> <hero2> <value>",
                "Sets the relationship value between two heroes (-100 to 100).\n" +
                "Supports named arguments: hero1:lord_1_1 hero2:lord_2_1 value:50",
                "gm.hero.set_relation lord_1_1 lord_2_1 50");

            ParsedArguments parsed = ArgumentParser.ParseArguments(args);

            parsed.SetValidArguments(
                new ArgumentDefinition("hero1", true),
                new ArgumentDefinition("hero2", true),
                new ArgumentDefinition("value", true)
            );

            string validationError = parsed.GetValidationError();
            if (validationError != null)
                return CommandResult.Error(MessageFormatter.FormatErrorMessage(validationError)).Message
;

            if (parsed.TotalCount < 3)
                return CommandResult.Error(usageMessage).Message
;

            // MARK: Parse Arguments
            string hero1Arg = parsed.GetArgument("hero1", 0);
            if (hero1Arg == null)
                return CommandResult.Error(MessageFormatter.FormatErrorMessage("Missing required argument 'hero1'.")).Message
;

            EntityFinderResult<Hero> hero1Result = HeroFinder.FindSingleHero(hero1Arg);
            if (!hero1Result.IsSuccess) return CommandResult.Error(hero1Result.Message).Message
;
            Hero hero1 = hero1Result.Entity;

            string hero2Arg = parsed.GetArgument("hero2", 1);
            if (hero2Arg == null)
                return CommandResult.Error(MessageFormatter.FormatErrorMessage("Missing required argument 'hero2'.")).Message
;

            EntityFinderResult<Hero> hero2Result = HeroFinder.FindSingleHero(hero2Arg);
            if (!hero2Result.IsSuccess) return CommandResult.Error(hero2Result.Message).Message
;
            Hero hero2 = hero2Result.Entity;

            string valueArg = parsed.GetArgument("value", 2);
            if (valueArg == null)
                return CommandResult.Error(MessageFormatter.FormatErrorMessage("Missing required argument 'value'.")).Message
;

            if (!CommandValidator.ValidateIntegerRange(valueArg, -100, 100, out int value, out string relationError))
                return CommandResult.Error(MessageFormatter.FormatErrorMessage(relationError)).Message
;

            // MARK: Execute Logic
            Dictionary<string, string> resolvedValues = new()
            {
                { "hero1", hero1.Name.ToString() },
                { "hero2", hero2.Name.ToString() },
                { "value", value.ToString() }
            };

            int previousRelation = hero1.GetRelation(hero2);
            int change = value - previousRelation;
            ChangeRelationAction.ApplyRelationChangeBetweenHeroes(hero1, hero2, change, true);

            string argumentDisplay = parsed.FormatArgumentDisplay("gm.hero.set_relation", resolvedValues);
            string fullMessage = argumentDisplay + MessageFormatter.FormatSuccessMessage(
                $"Relation between {hero1.Name} and {hero2.Name} changed from {previousRelation} to {value}.");
            return CommandResult.Success(fullMessage).Message
;
        });
    }
}
