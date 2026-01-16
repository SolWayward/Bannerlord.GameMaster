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
/// Add gold to all clan members
/// Usage: gm.clan.add_gold [clan] [amount]
/// </summary>
public static class AddClanGoldCommand
{
    [CommandLineFunctionality.CommandLineArgumentFunction("add_gold", "gm.clan")]
    public static string AddClanGold(List<string> args)
    {
        return Cmd.Run(args, () =>
        {
            // MARK: Validation
            if (!CommandValidator.ValidateCampaignState(out string error))
                return error;

            string usageMessage = CommandValidator.CreateUsageMessage(
                "gm.clan.add_gold", "<clan> <amount>",
                "Adds gold to all members of the clan.\n" +
                "Supports named arguments: clan:empire_south amount:50000",
                "gm.clan.add_gold empire_south 50000");

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

            if (!CommandValidator.ValidateIntegerRange(amountArg, int.MinValue, int.MaxValue, out int amount, out string goldError))
                return MessageFormatter.FormatErrorMessage(goldError);

            // MARK: Execute Logic
            Dictionary<string, string> resolvedValues = new()
            {
                { "clan", clan.Name.ToString() },
                { "amount", amount.ToString() }
            };

            int previousGold = clan.Gold;
            int membersCount = clan.Heroes.Count(h => h.IsAlive);

            if (membersCount == 0)
            {
                string argumentDisplayError = parsed.FormatArgumentDisplay("add_gold", resolvedValues);
                return argumentDisplayError + MessageFormatter.FormatErrorMessage($"{clan.Name} has no living heroes to receive gold.");
            }

            // Distribute gold evenly among all living clan members
            int goldPerMember = amount / membersCount;
            int remainder = amount % membersCount;

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

            string argumentDisplay = parsed.FormatArgumentDisplay("add_gold", resolvedValues);
            return argumentDisplay + MessageFormatter.FormatSuccessMessage(
                $"Added {amount} gold to {clan.Name} (distributed among {membersCount} members).\n" +
                $"Clan gold changed from {previousGold} to {clan.Gold}.");
        });
    }
}
