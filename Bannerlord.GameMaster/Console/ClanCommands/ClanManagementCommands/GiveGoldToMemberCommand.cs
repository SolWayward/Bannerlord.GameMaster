using Bannerlord.GameMaster.Console.Common;
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
/// Distribute gold to specific clan member
/// Usage: gm.clan.give_gold [clan] [hero] [amount]
/// </summary>
public static class GiveGoldToMemberCommand
{
    [CommandLineFunctionality.CommandLineArgumentFunction("give_gold", "gm.clan")]
    public static string GiveGoldToMember(List<string> args)
    {
        return Cmd.Run(args, () =>
        {
            // MARK: Validation
            if (!CommandValidator.ValidateCampaignState(out string error))
                return CommandResult.Error(error).Log().Message;

            string usageMessage = CommandValidator.CreateUsageMessage(
                "gm.clan.give_gold", "<clan> <hero> <amount>",
                "Gives gold to a specific clan member.\n" +
                "Supports named arguments: clan:empire_south hero:lord_1_1 amount:10000",
                "gm.clan.give_gold empire_south lord_1_1 10000");

            ParsedArguments parsed = ArgumentParser.ParseArguments(args);

            parsed.SetValidArguments(
                new ArgumentDefinition("clan", true),
                new ArgumentDefinition("hero", true),
                new ArgumentDefinition("amount", true)
            );

            string validationError = parsed.GetValidationError();
            if (validationError != null)
                return CommandResult.Error(MessageFormatter.FormatErrorMessage(validationError)).Log().Message;

            if (parsed.TotalCount < 3)
                return usageMessage;

            // MARK: Parse Arguments
            string clanArg = parsed.GetArgument("clan", 0);
            if (clanArg == null)
                return CommandResult.Error(MessageFormatter.FormatErrorMessage("Missing required argument 'clan'.")).Log().Message;

            EntityFinderResult<Clan> clanResult = ClanFinder.FindSingleClan(clanArg);
            if (!clanResult.IsSuccess) return clanResult.Message;
            Clan clan = clanResult.Entity;

            string heroArg = parsed.GetArgument("hero", 1);
            if (heroArg == null)
                return CommandResult.Error(MessageFormatter.FormatErrorMessage("Missing required argument 'hero'.")).Log().Message;

            EntityFinderResult<Hero> heroResult = HeroFinder.FindSingleHero(heroArg);
            if (!heroResult.IsSuccess) return heroResult.Message;
            Hero hero = heroResult.Entity;

            if (hero.Clan != clan)
                return CommandResult.Error(MessageFormatter.FormatErrorMessage($"{hero.Name} is not a member of {clan.Name}.")).Log().Message;

            string amountArg = parsed.GetArgument("amount", 2);
            if (amountArg == null)
                return CommandResult.Error(MessageFormatter.FormatErrorMessage("Missing required argument 'amount'.")).Log().Message;

            if (!CommandValidator.ValidateIntegerRange(amountArg, int.MinValue, int.MaxValue, out int amount, out string goldError))
                return CommandResult.Error(MessageFormatter.FormatErrorMessage(goldError)).Log().Message;

            // MARK: Execute Logic
            Dictionary<string, string> resolvedValues = new()
            {
                { "clan", clan.Name.ToString() },
                { "hero", hero.Name.ToString() },
                { "amount", amount.ToString() }
            };

            int previousClanGold = clan.Gold;
            int previousHeroGold = hero.Gold;

            hero.ChangeHeroGold(amount);

            string argumentDisplay = parsed.FormatArgumentDisplay("gm.clan.give_gold", resolvedValues);
            return CommandResult.Success(argumentDisplay + MessageFormatter.FormatSuccessMessage(
                $"Added {amount} gold to {hero.Name} (member of {clan.Name}).\n" +
                $"Hero gold: {previousHeroGold} -> {hero.Gold}\n" +
                $"Clan total gold: {previousClanGold} -> {clan.Gold}")).Log().Message;
        });
    }
}
