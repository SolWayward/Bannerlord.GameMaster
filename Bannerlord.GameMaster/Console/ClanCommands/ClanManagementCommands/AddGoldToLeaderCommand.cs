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
/// Add gold to clan leader only
/// Usage: gm.clan.add_gold_leader [clan] [amount]
/// </summary>
public static class AddGoldToLeaderCommand
{
    [CommandLineFunctionality.CommandLineArgumentFunction("add_gold_leader", "gm.clan")]
    public static string AddGoldToLeader(List<string> args)
    {
        return Cmd.Run(args, () =>
        {
            // MARK: Validation
            if (!CommandValidator.ValidateCampaignState(out string error))
                return CommandResult.Error(error).Message
;

            string usageMessage = CommandValidator.CreateUsageMessage(
                "gm.clan.add_gold_leader", "<clan> <amount>",
                "Adds gold to the clan leader only.\n" +
                "Supports named arguments: clan:empire_south amount:50000",
                "gm.clan.add_gold_leader empire_south 50000");

            ParsedArguments parsed = ArgumentParser.ParseArguments(args);

            parsed.SetValidArguments(
                new ArgumentDefinition("clan", true),
                new ArgumentDefinition("amount", true)
            );

            string validationError = parsed.GetValidationError();
            if (validationError != null)
                return CommandResult.Error(MessageFormatter.FormatErrorMessage(validationError)).Message
;

            if (parsed.TotalCount < 2)
                return usageMessage;

            // MARK: Parse Arguments
            string clanArg = parsed.GetArgument("clan", 0);
            if (clanArg == null)
                return CommandResult.Error(MessageFormatter.FormatErrorMessage("Missing required argument 'clan'.")).Message
;

            EntityFinderResult<Clan> clanResult = ClanFinder.FindSingleClan(clanArg);
            if (!clanResult.IsSuccess) return clanResult.Message;
            Clan clan = clanResult.Entity;

            if (clan.Leader == null)
                return CommandResult.Error(MessageFormatter.FormatErrorMessage($"{clan.Name} has no leader.")).Message
;

            string amountArg = parsed.GetArgument("amount", 1);
            if (amountArg == null)
                return CommandResult.Error(MessageFormatter.FormatErrorMessage("Missing required argument 'amount'.")).Message
;

            if (!CommandValidator.ValidateIntegerRange(amountArg, int.MinValue, int.MaxValue, out int amount, out string goldError))
                return CommandResult.Error(MessageFormatter.FormatErrorMessage(goldError)).Message
;

            // MARK: Execute Logic
            Dictionary<string, string> resolvedValues = new()
            {
                { "clan", clan.Name.ToString() },
                { "amount", amount.ToString() }
            };

            int previousClanGold = clan.Gold;
            int previousLeaderGold = clan.Leader.Gold;

            clan.Leader.ChangeHeroGold(amount);

            string argumentDisplay = parsed.FormatArgumentDisplay("gm.clan.add_gold_leader", resolvedValues);
            return CommandResult.Success(argumentDisplay + MessageFormatter.FormatSuccessMessage(
                $"Added {amount} gold to {clan.Leader.Name} (leader of {clan.Name}).\n" +
                $"Leader gold: {previousLeaderGold} -> {clan.Leader.Gold}\n" +
                $"Clan total gold: {previousClanGold} -> {clan.Gold}")).Message
;
        });
    }
}
