using Bannerlord.GameMaster.Console.Common.Execution;
using Bannerlord.GameMaster.Console.Common.EntityFinding;
using Bannerlord.GameMaster.Console.Common.Formatting;
using Bannerlord.GameMaster.Console.Common.Parsing;
using Bannerlord.GameMaster.Console.Common.Validation;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Library;

namespace Bannerlord.GameMaster.Console.ClanCommands.ClanManagementCommands;

/// <summary>
/// Set total clan gold by distributing to members
/// Usage: gm.clan.set_gold [clan] [amount]
/// </summary>
public static class SetClanGoldCommand
{
    [CommandLineFunctionality.CommandLineArgumentFunction("set_gold", "gm.clan")]
    public static string SetClanGold(List<string> args)
    {
        return Cmd.Run(args, () =>
        {
            // MARK: Validation
            if (!CommandValidator.ValidateCampaignState(out string error))
                return error;

            string usageMessage = CommandValidator.CreateUsageMessage(
                "gm.clan.set_gold", "<clan> <amount>",
                "Sets the clan's total gold by distributing to all members.\n" +
                "Supports named arguments: clan:empire_south amount:100000",
                "gm.clan.set_gold empire_south 100000");

            ParsedArguments parsed = ArgumentParser.ParseArguments(args);

            parsed.SetValidArguments(
                new ArgumentDefinition("clan", true),
                new ArgumentDefinition("amount", true)
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

            string amountArg = parsed.GetArgument("amount", 1);
            if (amountArg == null)
                return MessageFormatter.FormatErrorMessage("Missing required argument 'amount'.");

            if (!CommandValidator.ValidateIntegerRange(amountArg, 0, int.MaxValue, out int targetAmount, out string goldError))
                return MessageFormatter.FormatErrorMessage(goldError);

            // MARK: Execute Logic
            Dictionary<string, string> resolvedValues = new()
            {
                { "clan", clan.Name.ToString() },
                { "amount", targetAmount.ToString() }
            };

            int previousGold = clan.Gold;
            int membersCount = clan.Heroes.Count(h => h.IsAlive);

            if (membersCount == 0)
            {
                string argumentDisplayError = parsed.FormatArgumentDisplay("set_gold", resolvedValues);
                return argumentDisplayError + MessageFormatter.FormatErrorMessage($"{clan.Name} has no living heroes to receive gold.");
            }

            // First, zero out all member gold
            foreach (Hero hero in clan.Heroes.Where(h => h.IsAlive))
            {
                if (hero.Gold > 0)
                    hero.ChangeHeroGold(-hero.Gold);
            }

            // Then distribute the target amount evenly
            int goldPerMember = targetAmount / membersCount;
            int remainder = targetAmount % membersCount;

            foreach (Hero hero in clan.Heroes.Where(h => h.IsAlive))
            {
                int goldToAdd = goldPerMember;
                if (remainder > 0)
                {
                    goldToAdd++;
                    remainder--;
                }
                hero.ChangeHeroGold(goldToAdd);
            }

            string argumentDisplay = parsed.FormatArgumentDisplay("set_gold", resolvedValues);
            return argumentDisplay + MessageFormatter.FormatSuccessMessage(
                $"Set {clan.Name}'s gold to {targetAmount} (distributed among {membersCount} members).\n" +
                $"Previous clan gold: {previousGold}, New clan gold: {clan.Gold}.");
        });
    }
}
