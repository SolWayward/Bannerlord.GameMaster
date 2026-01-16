using System.Collections.Generic;
using Bannerlord.GameMaster.Console.Common.Execution;
using Bannerlord.GameMaster.Console.Common.EntityFinding;
using Bannerlord.GameMaster.Console.Common.Formatting;
using Bannerlord.GameMaster.Console.Common.Parsing;
using Bannerlord.GameMaster.Console.Common.Validation;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Library;

namespace Bannerlord.GameMaster.Console.KingdomCommands.KingdomManagementCommands;

public static class RemoveClanCommand
{
    /// <summary>
    /// Remove a clan from its kingdom
    /// Usage: gm.kingdom.remove_clan [clan]
    /// </summary>
    [CommandLineFunctionality.CommandLineArgumentFunction("remove_clan", "gm.kingdom")]
    public static string RemoveClanFromKingdom(List<string> args)
    {
        return Cmd.Run(args, () =>
        {
            // MARK: Validation
            if (!CommandValidator.ValidateCampaignState(out string error))
                return error;

            string usageMessage = CommandValidator.CreateUsageMessage(
                "gm.kingdom.remove_clan", "<clan>",
                "Removes a clan from its current kingdom.\n" +
                "Supports named arguments: clan:clan_empire_south_1",
                "gm.kingdom.remove_clan clan_empire_south_1");

            ParsedArguments parsed = ArgumentParser.ParseArguments(args);

            parsed.SetValidArguments(
                new ArgumentDefinition("clan", true)
            );

            string validationError = parsed.GetValidationError();
            if (validationError != null)
                return MessageFormatter.FormatErrorMessage(validationError);

            if (parsed.TotalCount < 1)
                return usageMessage;

            // MARK: Parse Arguments
            string clanArg = parsed.GetArgument("clan", 0);
            if (clanArg == null)
                return MessageFormatter.FormatErrorMessage("Missing required argument 'clan'.");

            EntityFinderResult<Clan> clanResult = ClanFinder.FindSingleClan(clanArg);
            if (!clanResult.IsSuccess)
                return clanResult.Message;
            Clan clan = clanResult.Entity;

            if (clan.Kingdom == null)
                return MessageFormatter.FormatErrorMessage($"{clan.Name} is not part of any kingdom.");

            if (clan == clan.Kingdom.RulingClan)
                return MessageFormatter.FormatErrorMessage($"Cannot remove the ruling clan ({clan.Name}) from {clan.Kingdom.Name}.");

            // MARK: Execute Logic
            Dictionary<string, string> resolvedValues = new()
            {
                { "clan", clan.Name.ToString() }
            };

            string previousKingdom = clan.Kingdom.Name.ToString();
            clan.Kingdom = null;

            string argumentDisplay = parsed.FormatArgumentDisplay("remove_clan", resolvedValues);
            return argumentDisplay + MessageFormatter.FormatSuccessMessage(
                $"{clan.Name} removed from {previousKingdom}.");
        });
    }
}
