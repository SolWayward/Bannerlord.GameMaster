using Bannerlord.GameMaster.Common;
using Bannerlord.GameMaster.Console.Common;
using Bannerlord.GameMaster.Console.Common.EntityFinding;
using Bannerlord.GameMaster.Console.Common.Execution;
using Bannerlord.GameMaster.Console.Common.Formatting;
using Bannerlord.GameMaster.Console.Common.Parsing;
using Bannerlord.GameMaster.Console.Common.Validation;
using Bannerlord.GameMaster.RemovalHelpers;
using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Library;

namespace Bannerlord.GameMaster.Console.CleanupCommands;

public static class RemoveBlgmClanCommand
{
    /// <summary>
    /// Removes a single BLGM-generated clan
    /// Usage: gm.cleanup.remove_blgm_clan <clan>
    /// </summary>
    [CommandLineFunctionality.CommandLineArgumentFunction("remove_blgm_clan", "gm.cleanup")]
    public static string RemoveBlgmClan(List<string> args)
    {
        return Cmd.Run(args, () =>
        {
            // MARK: Validation
            if (!CommandValidator.ValidateCampaignState(out string error))
                return CommandResult.Error(error).Log().Message;

            string usageMessage = CommandValidator.CreateUsageMessage(
                "gm.cleanup.remove_blgm_clan", "<clan>",
                "Removes a single BLGM-generated clan.\n" +
                "- clan: Clan identifier (name or ID)",
                "gm.cleanup.remove_blgm_clan blgm_clan_123");

            ParsedArguments parsed = ArgumentParser.ParseArguments(args);

            parsed.SetValidArguments(
                new ArgumentDefinition("clan", true)
            );

            string validationError = parsed.GetValidationError();
            if (validationError != null)
                return CommandResult.Error(MessageFormatter.FormatErrorMessage(validationError)).Log().Message;

            if (parsed.TotalCount < 1)
                return CommandResult.Error(usageMessage).Log().Message;

            // MARK: Parse Arguments
            string clanIdentifier = parsed.GetArgument("clan", 0);
            if (string.IsNullOrWhiteSpace(clanIdentifier))
                return CommandResult.Error(MessageFormatter.FormatErrorMessage("Clan identifier cannot be empty.")).Log().Message;

            EntityFinderResult<Clan> clanResult = ClanFinder.FindSingleClan(clanIdentifier);
            if (!clanResult.IsSuccess)
                return CommandResult.Error(clanResult.Message).Log().Message;
            Clan clan = clanResult.Entity;

            // MARK: Execute Logic
            BLGMResult result = ClanRemover.RemoveSingleClan(clan);

            Dictionary<string, string> resolvedValues = new()
            {
                { "clan", clan.Name.ToString() }
            };

            string argumentDisplay = parsed.FormatArgumentDisplay("gm.cleanup.remove_blgm_clan", resolvedValues);

            if (result.IsSuccess)
            {
                string fullMessage = argumentDisplay + MessageFormatter.FormatSuccessMessage(result.Message);
                return CommandResult.Success(fullMessage).Log().Message;
            }
            else
            {
                string fullMessage = argumentDisplay + MessageFormatter.FormatErrorMessage(result.Message);
                return CommandResult.Error(fullMessage).Log().Message;
            }
        });
    }
}
