using Bannerlord.GameMaster.Console.Common.Execution;
using Bannerlord.GameMaster.Console.Common.EntityFinding;
using Bannerlord.GameMaster.Console.Common.Formatting;
using Bannerlord.GameMaster.Console.Common.Parsing;
using Bannerlord.GameMaster.Console.Common.Validation;
using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.Library;

namespace Bannerlord.GameMaster.Console.ClanCommands.ClanManagementCommands;

/// <summary>
/// Destroy/Eliminate a clan
/// Usage: gm.clan.destroy [clan]
/// </summary>
public static class DestroyClanCommand
{
    [CommandLineFunctionality.CommandLineArgumentFunction("destroy", "gm.clan")]
    public static string DestroyClan(List<string> args)
    {
        return Cmd.Run(args, () =>
        {
            // MARK: Validation
            if (!CommandValidator.ValidateCampaignState(out string error))
                return error;

            string usageMessage = CommandValidator.CreateUsageMessage(
                "gm.clan.destroy", "<clan>",
                "Destroys/eliminates the specified clan.\n" +
                "Supports named arguments: clan:rebel_clan_1",
                "gm.clan.destroy rebel_clan_1");

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
            if (!clanResult.IsSuccess) return clanResult.Message;
            Clan clan = clanResult.Entity;

            if (clan.IsEliminated)
                return MessageFormatter.FormatErrorMessage($"{clan.Name} is already eliminated.");

            if (clan == Clan.PlayerClan)
                return MessageFormatter.FormatErrorMessage("Cannot destroy the player's clan.");

            // MARK: Execute Logic
            Dictionary<string, string> resolvedValues = new()
            {
                { "clan", clan.Name.ToString() }
            };

            DestroyClanAction.Apply(clan);

            string argumentDisplay = parsed.FormatArgumentDisplay("gm.clan.destroy", resolvedValues);
            return argumentDisplay + MessageFormatter.FormatSuccessMessage($"{clan.Name} has been destroyed/eliminated.");
        });
    }
}
