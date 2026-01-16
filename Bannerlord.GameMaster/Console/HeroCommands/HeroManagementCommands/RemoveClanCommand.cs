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
/// Remove a hero from their clan
/// Usage: gm.hero.remove_clan [hero]
/// </summary>
public static class RemoveClanCommand
{
    [CommandLineFunctionality.CommandLineArgumentFunction("remove_clan", "gm.hero")]
    public static string RemoveClan(List<string> args)
    {
        return Cmd.Run(args, () =>
        {
            // MARK: Validation
            if (!CommandValidator.ValidateCampaignState(out string error))
                return error;

            string usageMessage = CommandValidator.CreateUsageMessage(
                "gm.hero.remove_clan", "<hero>",
                "Removes a hero from their current clan.\n" +
                "Supports named arguments: hero:lord_1_1",
                "gm.hero.remove_clan lord_1_1");

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

            if (hero.Clan == null)
                return MessageFormatter.FormatErrorMessage($"{hero.Name} is not a member of any clan.");

            // MARK: Execute Logic
            Dictionary<string, string> resolvedValues = new()
            {
                { "hero", hero.Name.ToString() }
            };

            string previousClanName = hero.Clan.Name.ToString();
            hero.Clan = null;

            string argumentDisplay = parsed.FormatArgumentDisplay("remove_clan", resolvedValues);
            return argumentDisplay + MessageFormatter.FormatSuccessMessage(
                $"{hero.Name} (ID: {hero.StringId}) removed from clan '{previousClanName}'.");
        });
    }
}
