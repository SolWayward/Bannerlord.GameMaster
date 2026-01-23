using System.Collections.Generic;
using Bannerlord.GameMaster.Console.Common;
using Bannerlord.GameMaster.Console.Common.Execution;
using Bannerlord.GameMaster.Console.Common.EntityFinding;
using Bannerlord.GameMaster.Console.Common.Formatting;
using Bannerlord.GameMaster.Console.Common.Parsing;
using Bannerlord.GameMaster.Console.Common.Validation;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.Library;

namespace Bannerlord.GameMaster.Console.KingdomCommands.KingdomManagementCommands;

public static class AddClanCommand
{
    /// <summary>
    /// Add a clan to a kingdom
    /// Usage: gm.kingdom.add_clan [clan] [kingdom]
    /// </summary>
    [CommandLineFunctionality.CommandLineArgumentFunction("add_clan", "gm.kingdom")]
    public static string AddClanToKingdom(List<string> args)
    {
        return Cmd.Run(args, () =>
        {
            // MARK: Validation
            if (!CommandValidator.ValidateCampaignState(out string error))
                return error;

            string usageMessage = CommandValidator.CreateUsageMessage(
                "gm.kingdom.add_clan", "<clan> <kingdom>",
                "Adds a clan to the specified kingdom.\n" +
                "Supports named arguments: clan:clan_battania_1 kingdom:empire",
                "gm.kingdom.add_clan clan_battania_1 empire");

            ParsedArguments parsed = ArgumentParser.ParseArguments(args);

            parsed.SetValidArguments(
                new ArgumentDefinition("clan", true),
                new ArgumentDefinition("kingdom", true)
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
                return MessageFormatter.FormatErrorMessage("Missing required argument 'clan'.");

            EntityFinderResult<Clan> clanResult = ClanFinder.FindSingleClan(clanArg);
            if (!clanResult.IsSuccess)
                return clanResult.Message;
            Clan clan = clanResult.Entity;

            string kingdomArg = parsed.GetArgument("kingdom", 1);
            if (kingdomArg == null)
                return MessageFormatter.FormatErrorMessage("Missing required argument 'kingdom'.");

            EntityFinderResult<Kingdom> kingdomResult = KingdomFinder.FindSingleKingdom(kingdomArg);
            if (!kingdomResult.IsSuccess)
                return kingdomResult.Message;
            Kingdom kingdom = kingdomResult.Entity;

            if (clan.Kingdom == kingdom)
                return MessageFormatter.FormatErrorMessage($"{clan.Name} is already part of {kingdom.Name}.");

            // MARK: Execute Logic
            Dictionary<string, string> resolvedValues = new()
            {
                { "clan", clan.Name.ToString() },
                { "kingdom", kingdom.Name.ToString() }
            };

            string previousKingdom = clan.Kingdom?.Name?.ToString() ?? "No Kingdom";
            ChangeKingdomAction.ApplyByJoinToKingdom(clan, kingdom, showNotification: true);

            string argumentDisplay = parsed.FormatArgumentDisplay("gm.kingdom.add_clan", resolvedValues);
            return CommandResult.Success(argumentDisplay + MessageFormatter.FormatSuccessMessage(
                $"{clan.Name} joined {kingdom.Name} from {previousKingdom}.")).Message
;
        });
    }
}
