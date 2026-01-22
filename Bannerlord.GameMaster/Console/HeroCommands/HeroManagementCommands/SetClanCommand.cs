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
/// Transfer a hero to another clan
/// Usage: gm.hero.set_clan [hero] [clan]
/// </summary>
public static class SetClanCommand
{
    [CommandLineFunctionality.CommandLineArgumentFunction("set_clan", "gm.hero")]
    public static string SetClan(List<string> args)
    {
        return Cmd.Run(args, () =>
        {
            // MARK: Validation
            if (!CommandValidator.ValidateCampaignState(out string error))
                return CommandResult.Error(error).Log().Message;

            string usageMessage = CommandValidator.CreateUsageMessage(
                "gm.hero.set_clan", "<hero> <clan>",
                "Transfers a hero to another clan.\n" +
                "Supports named arguments: hero:lord_1_1 clan:clan_empire_south_1",
                "gm.hero.set_clan lord_1_1 clan_empire_south_1");

            ParsedArguments parsed = ArgumentParser.ParseArguments(args);

            parsed.SetValidArguments(
                new ArgumentDefinition("hero", true),
                new ArgumentDefinition("clan", true)
            );

            string validationError = parsed.GetValidationError();
            if (validationError != null)
                return CommandResult.Error(MessageFormatter.FormatErrorMessage(validationError)).Log().Message;

            if (parsed.TotalCount < 2)
                return CommandResult.Error(usageMessage).Log().Message;

            // MARK: Parse Arguments
            string heroArg = parsed.GetArgument("hero", 0);
            if (heroArg == null)
                return CommandResult.Error(MessageFormatter.FormatErrorMessage("Missing required argument 'hero'.")).Log().Message;

            EntityFinderResult<Hero> heroResult = HeroFinder.FindSingleHero(heroArg);
            if (!heroResult.IsSuccess) return CommandResult.Error(heroResult.Message).Log().Message;
            Hero hero = heroResult.Entity;

            string clanArg = parsed.GetArgument("clan", 1);
            if (clanArg == null)
                return CommandResult.Error(MessageFormatter.FormatErrorMessage("Missing required argument 'clan'.")).Log().Message;

            EntityFinderResult<Clan> clanResult = ClanFinder.FindSingleClan(clanArg);
            if (!clanResult.IsSuccess) return CommandResult.Error(clanResult.Message).Log().Message;
            Clan clan = clanResult.Entity;

            // MARK: Execute Logic
            Dictionary<string, string> resolvedValues = new()
            {
                { "hero", hero.Name.ToString() },
                { "clan", clan.Name.ToString() }
            };

            string previousClanName = hero.Clan?.Name?.ToString() ?? "No Clan";
            hero.Clan = clan;

            string argumentDisplay = parsed.FormatArgumentDisplay("gm.hero.set_clan", resolvedValues);
            string fullMessage = argumentDisplay + MessageFormatter.FormatSuccessMessage(
                $"{hero.Name} (ID: {hero.StringId}) transferred from '{previousClanName}' to '{clan.Name}'.\n" +
                $"Updated details: {hero.FormattedDetails()}");
            return CommandResult.Success(fullMessage).Log().Message;
        });
    }
}
