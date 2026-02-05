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
    /// Usage: gm.kingdom.add_clan [clan] [kingdom] [asMercenary]
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
                "gm.kingdom.add_clan", "<clan> <kingdom> [asMercenary]",
                "Adds a clan to the specified kingdom.\n" +
                "Optional asMercenary (defaults to false): Set to true to join as mercenary.\n" +
                "Supports named arguments: clan:clan_battania_1 kingdom:empire asMercenary:true",
                "gm.kingdom.add_clan clan_battania_1 empire\n" +
                "gm.kingdom.add_clan clan:clan_battania_1 kingdom:empire mercenary:true");

            ParsedArguments parsed = ArgumentParser.ParseArguments(args);

            parsed.SetValidArguments(
                new ArgumentDefinition("clan", true),
                new ArgumentDefinition("kingdom", true),
                new ArgumentDefinition("asMercenary", false, null, "mercenary")
            );

            string validationError = parsed.GetValidationError();
            if (validationError != null)
                return CommandResult.Error(MessageFormatter.FormatErrorMessage(validationError)).Message;

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

            // Parse asMercenary optional argument
            string asMercenaryArg = parsed.GetArgument("asMercenary", 2) ?? parsed.GetArgument("mercenary", 2);
            bool asMercenary = false;
            if (asMercenaryArg != null)
            {
                if (asMercenaryArg.ToLower() == "true")
                    asMercenary = true;
                else if (asMercenaryArg.ToLower() != "false")
                    return MessageFormatter.FormatErrorMessage($"Invalid value for asMercenary: '{asMercenaryArg}'. Must be 'true' or 'false'.");
            }

            // MARK: Execute Logic
            Dictionary<string, string> resolvedValues = new()
            {
                { "clan", clan.Name.ToString() },
                { "kingdom", kingdom.Name.ToString() },
                { "asMercenary", asMercenary.ToString() }
            };

            string previousKingdom = clan.Kingdom?.Name?.ToString() ?? "No Kingdom";
            
            if (asMercenary)
            {
                ChangeKingdomAction.ApplyByJoinFactionAsMercenary(clan, kingdom, CampaignTime.Never, 50, true);
            }
            else
            {
                ChangeKingdomAction.ApplyByJoinToKingdom(clan, kingdom, showNotification: true);
            }
            
            string joinType = asMercenary ? "as mercenary" : "as vassal";
            string argumentDisplay = parsed.FormatArgumentDisplay("gm.kingdom.add_clan", resolvedValues);
            return CommandResult.Success(argumentDisplay + MessageFormatter.FormatSuccessMessage(
                $"{clan.Name} joined {kingdom.Name} {joinType} from {previousKingdom}.")).Message;
        });
    }
}
