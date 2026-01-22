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
/// Set clan renown
/// Usage: gm.clan.set_renown [clan] [amount]
/// </summary>
public static class SetClanRenownCommand
{
    [CommandLineFunctionality.CommandLineArgumentFunction("set_renown", "gm.clan")]
    public static string SetClanRenown(List<string> args)
    {
        return Cmd.Run(args, () =>
        {
            // MARK: Validation
            if (!CommandValidator.ValidateCampaignState(out string error))
                return error;

            string usageMessage = CommandValidator.CreateUsageMessage(
                "gm.clan.set_renown", "<clan> <amount>",
                "Sets the clan's renown.\n" +
                "Supports named arguments: clan:empire_south amount:500",
                "gm.clan.set_renown empire_south 500");

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

            if (!CommandValidator.ValidateFloatRange(amountArg, 0, float.MaxValue, out float amount, out string renownError))
                return MessageFormatter.FormatErrorMessage(renownError);

            // MARK: Execute Logic
            Dictionary<string, string> resolvedValues = new()
            {
                { "clan", clan.Name.ToString() },
                { "amount", amount.ToString("F0") }
            };

            float previousRenown = clan.Renown;
            clan.Renown = amount;

            string argumentDisplay = parsed.FormatArgumentDisplay("gm.clan.set_renown", resolvedValues);
            return argumentDisplay + MessageFormatter.FormatSuccessMessage($"{clan.Name}'s renown changed from {previousRenown:F0} to {clan.Renown:F0}.");
        });
    }
}
