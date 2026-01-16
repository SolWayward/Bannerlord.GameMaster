using Bannerlord.GameMaster.Console.Common.Execution;
using Bannerlord.GameMaster.Console.Common.EntityFinding;
using Bannerlord.GameMaster.Console.Common.Formatting;
using Bannerlord.GameMaster.Console.Common.Parsing;
using Bannerlord.GameMaster.Console.Common.Validation;
using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Library;

namespace Bannerlord.GameMaster.Console.ClanCommands.ClanManagementCommands;

/// <summary>
/// Transfer a hero to another clan
/// Usage: gm.clan.add_hero [clan] [hero]
/// </summary>
public static class AddHeroToClanCommand
{
    [CommandLineFunctionality.CommandLineArgumentFunction("add_hero", "gm.clan")]
    public static string AddHeroToClan(List<string> args)
    {
        return Cmd.Run(args, () =>
        {
            // MARK: Validation
            if (!CommandValidator.ValidateCampaignState(out string error))
                return error;

            string usageMessage = CommandValidator.CreateUsageMessage(
                "gm.clan.add_hero", "<clan> <hero>",
                "Adds a hero to the specified clan.\n" +
                "Supports named arguments: clan:empire_south hero:lord_1_1",
                "gm.clan.add_hero empire_south lord_1_1");

            ParsedArguments parsed = ArgumentParser.ParseArguments(args);

            parsed.SetValidArguments(
                new ArgumentDefinition("clan", true),
                new ArgumentDefinition("hero", true)
            );

            string validationError = parsed.GetValidationError();
            if (validationError != null)
                return MessageFormatter.FormatErrorMessage(validationError);

            if (parsed.TotalCount < 2)
                return usageMessage;

            // MARK: Parse Arguments
            string clanArg = parsed.GetArgument("clan", 0);
            if (clanArg == null)
                return MessageFormatter.FormatErrorMessage("Missing required argument 'clan'.");

            EntityFinderResult<Clan> clanResult = ClanFinder.FindSingleClan(clanArg);
            if (!clanResult.IsSuccess) return clanResult.Message;
            Clan clan = clanResult.Entity;

            string heroArg = parsed.GetArgument("hero", 1);
            if (heroArg == null)
                return MessageFormatter.FormatErrorMessage("Missing required argument 'hero'.");

            EntityFinderResult<Hero> heroResult = HeroFinder.FindSingleHero(heroArg);
            if (!heroResult.IsSuccess) return heroResult.Message;
            Hero hero = heroResult.Entity;

            // MARK: Execute Logic
            Dictionary<string, string> resolvedValues = new()
            {
                { "clan", clan.Name.ToString() },
                { "hero", hero.Name.ToString() }
            };

            string previousClanName = hero.Clan?.Name?.ToString() ?? "No Clan";
            hero.Clan = clan;

            // Prevents crash if hero is moved to a clan and is not the leader, the game will crash when player choses to be released from his oath in conversation
            if (hero == Hero.MainHero)
                clan.SetLeader(Hero.MainHero);

            string argumentDisplay = parsed.FormatArgumentDisplay("add_hero", resolvedValues);
            return argumentDisplay + MessageFormatter.FormatSuccessMessage(
                $"{hero.Name} transferred from '{previousClanName}' to '{clan.Name}'.");
        });
    }
}
