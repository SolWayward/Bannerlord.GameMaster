using Bannerlord.GameMaster.Console.Common;
using Bannerlord.GameMaster.Console.Common.Execution;
using Bannerlord.GameMaster.Console.Common.EntityFinding;
using Bannerlord.GameMaster.Console.Common.Formatting;
using Bannerlord.GameMaster.Console.Common.Parsing;
using Bannerlord.GameMaster.Console.Common.Validation;
using Bannerlord.GameMaster.Clans;
using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Library;

namespace Bannerlord.GameMaster.Console.ClanCommands.ClanManagementCommands;

/// <summary>
/// Change clan tier
/// Usage: gm.clan.set_tier [clan] [tier]
/// </summary>
public static class SetClanTierCommand
{
    [CommandLineFunctionality.CommandLineArgumentFunction("set_tier", "gm.clan")]
    public static string SetClanTier(List<string> args)
    {
        return Cmd.Run(args, () =>
        {
            // MARK: Validation
            if (!CommandValidator.ValidateCampaignState(out string error))
                return CommandResult.Error(error).Message;

            string usageMessage = CommandValidator.CreateUsageMessage(
                "gm.clan.set_tier", "<clan> <tier>",
                "Sets the clan's tier (0-6).\n" +
                "Supports named arguments: clan:empire_south tier:5",
                "gm.clan.set_tier empire_south 5");

            ParsedArguments parsed = ArgumentParser.ParseArguments(args);

            parsed.SetValidArguments(
                new ArgumentDefinition("clan", true),
                new ArgumentDefinition("tier", true)
            );

            string validationError = parsed.GetValidationError();
            if (validationError != null)
                return CommandResult.Error(MessageFormatter.FormatErrorMessage(validationError)).Message;

            if (parsed.TotalCount < 2)
                return usageMessage;

            // MARK: Parse Arguments
            string clanArg = parsed.GetArgument("clan", 0);
            if (clanArg == null)
                return CommandResult.Error(MessageFormatter.FormatErrorMessage("Missing required argument 'clan'.")).Message;

            EntityFinderResult<Clan> clanResult = ClanFinder.FindSingleClan(clanArg);
            if (!clanResult.IsSuccess) return clanResult.Message;
            Clan clan = clanResult.Entity;

            string tierArg = parsed.GetArgument("tier", 1);
            if (tierArg == null)
                return CommandResult.Error(MessageFormatter.FormatErrorMessage("Missing required argument 'tier'.")).Message;

            if (!CommandValidator.ValidateIntegerRange(tierArg, 0, 6, out int tier, out string tierError))
                return CommandResult.Error(MessageFormatter.FormatErrorMessage(tierError)).Message;

            if (clan.Tier == tier)
                return CommandResult.Error(MessageFormatter.FormatErrorMessage($"Clan is already tier {clan.Tier}")).Message;

            // MARK: Execute Logic
            Dictionary<string, string> resolvedValues = new()
            {
                { "clan", clan.Name.ToString() },
                { "tier", tier.ToString() }
            };

            int previousTier = clan.Tier;
            clan.SetClanTier(tier);

            string argumentDisplay = parsed.FormatArgumentDisplay("gm.clan.set_tier", resolvedValues);
            return CommandResult.Success(argumentDisplay + MessageFormatter.FormatSuccessMessage($"{clan.Name}'s tier changed from {previousTier} to {clan.Tier}.")).Message;
        });
    }
}
