using Bannerlord.GameMaster.Console.Common;
using Bannerlord.GameMaster.Console.Common.Execution;
using Bannerlord.GameMaster.Console.Common.EntityFinding;
using Bannerlord.GameMaster.Console.Common.Formatting;
using Bannerlord.GameMaster.Console.Common.Parsing;
using Bannerlord.GameMaster.Console.Common.Validation;
using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Library;
using Bannerlord.GameMaster.Clans;

namespace Bannerlord.GameMaster.Console.ClanCommands.ClanManagementCommands;

/// <summary>
/// Rename a clan
/// Usage: gm.clan.rename [clan] [newName]
/// </summary>
public static class RenameClanCommand
{
    [CommandLineFunctionality.CommandLineArgumentFunction("rename", "gm.clan")]
    public static string RenameClan(List<string> args)
    {
        return Cmd.Run(args, () =>
        {
            // MARK: Validation
            if (!CommandValidator.ValidateCampaignState(out string error))
                return CommandResult.Error(error).Message
;

            string usageMessage = CommandValidator.CreateUsageMessage(
                "gm.clan.rename", "<clan> <newName>",
                "Renames the specified clan. Use SINGLE QUOTES for multi-word names.\n" +
                "Supports named arguments: clan:empire_south newName:'Southern Empire Lords'",
                "gm.clan.rename empire_south 'Southern Empire Lords'\n" +
                "gm.clan.rename clan_1 NewClanName");

            ParsedArguments parsed = ArgumentParser.ParseArguments(args);

            parsed.SetValidArguments(
                new ArgumentDefinition("clan", true),
                new ArgumentDefinition("newName", true, null, "name")
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

            string newName = parsed.GetArgument("newName", 1) ?? parsed.GetNamed("name");
            if (newName == null)
                return CommandResult.Error(MessageFormatter.FormatErrorMessage("Missing required argument 'newName'.")).Message
;

            if (string.IsNullOrWhiteSpace(newName))
                return CommandResult.Error(MessageFormatter.FormatErrorMessage("New name cannot be empty.")).Message
;

            // MARK: Execute Logic
            Dictionary<string, string> resolvedValues = new()
            {
                { "clan", clan.Name.ToString() },
                { "newName", newName }
            };

            string previousName = clan.Name.ToString();
            clan.SetStringName(newName);

            string argumentDisplay = parsed.FormatArgumentDisplay("gm.clan.rename", resolvedValues);
            return CommandResult.Success(argumentDisplay + MessageFormatter.FormatSuccessMessage(
                $"Clan renamed from '{previousName}' to '{clan.Name}' (ID: {clan.StringId})")).Message
;
        });
    }
}
