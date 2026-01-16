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
/// Change clan leader
/// Usage: gm.clan.set_leader [clan] [hero]
/// </summary>
public static class SetClanLeaderCommand
{
    [CommandLineFunctionality.CommandLineArgumentFunction("set_leader", "gm.clan")]
    public static string SetClanLeader(List<string> args)
    {
        return Cmd.Run(args, () =>
        {
            // MARK: Validation
            if (!CommandValidator.ValidateCampaignState(out string error))
                return error;

            string usageMessage = CommandValidator.CreateUsageMessage(
                "gm.clan.set_leader", "<clan> <hero>",
                "Changes the clan leader to the specified hero.\n" +
                "Supports named arguments: clan:empire_south hero:lord_1_1",
                "gm.clan.set_leader empire_south lord_1_1");

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

            if (hero.Clan != clan)
                return MessageFormatter.FormatErrorMessage($"{hero.Name} is not a member of {clan.Name}.");

            // MARK: Execute Logic
            Dictionary<string, string> resolvedValues = new()
            {
                { "clan", clan.Name.ToString() },
                { "hero", hero.Name.ToString() }
            };

            string previousLeader = clan.Leader?.Name?.ToString() ?? "None";
            clan.SetLeader(hero);

            string argumentDisplay = parsed.FormatArgumentDisplay("set_leader", resolvedValues);
            return argumentDisplay + MessageFormatter.FormatSuccessMessage($"{clan.Name}'s leader changed from {previousLeader} to {hero.Name}.");
        });
    }
}
