using Bannerlord.GameMaster.Console.Common;
using Bannerlord.GameMaster.Console.Common.Execution;
using Bannerlord.GameMaster.Console.Common.EntityFinding;
using Bannerlord.GameMaster.Console.Common.Formatting;
using Bannerlord.GameMaster.Console.Common.Parsing;
using Bannerlord.GameMaster.Console.Common.Validation;
using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.ObjectSystem;

namespace Bannerlord.GameMaster.Console.ClanCommands.ClanManagementCommands;

/// <summary>
/// Change a clan's culture
/// Usage: gm.clan.set_culture [clan] [culture]
/// Note: This command also updates the clan's basic troop to match the new culture.
/// This game logic is documented in Plans/CommandLogicExtractionNotes.md for future extraction.
/// </summary>
public static class SetCultureCommand
{
    [CommandLineFunctionality.CommandLineArgumentFunction("set_culture", "gm.clan")]
    public static string SetCulture(List<string> args)
    {
        return Cmd.Run(args, () =>
        {
            // MARK: Validation
            if (!CommandValidator.ValidateCampaignState(out string error))
                return CommandResult.Error(error).Message
;

            string usageMessage = CommandValidator.CreateUsageMessage(
                "gm.clan.set_culture", "<clan> <culture>",
                "Changes the clan's culture. Also updates the clan's basic troop to match the new culture.\n" +
                "Supports named arguments: clan:empire_south culture:vlandia",
                "gm.clan.set_culture empire_south vlandia\n" +
                "gm.clan.set_culture my_clan battania");

            ParsedArguments parsed = ArgumentParser.ParseArguments(args);

            parsed.SetValidArguments(
                new ArgumentDefinition("clan", true),
                new ArgumentDefinition("culture", true)
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

            string cultureArg = parsed.GetArgument("culture", 1);
            if (cultureArg == null)
                return CommandResult.Error(MessageFormatter.FormatErrorMessage("Missing required argument 'culture'.")).Message
;

            CultureObject newCulture = MBObjectManager.Instance.GetObject<CultureObject>(cultureArg);
            if (newCulture == null)
                return CommandResult.Error(MessageFormatter.FormatErrorMessage($"Culture '{cultureArg}' not found. Valid cultures: aserai, battania, empire, khuzait, nord, sturgia, vlandia")).Message
;

            // MARK: Execute Logic
            Dictionary<string, string> resolvedValues = new()
            {
                { "clan", clan.Name.ToString() },
                { "culture", newCulture.Name.ToString() }
            };

            string previousCulture = clan.Culture?.Name?.ToString() ?? "None";
            
            // Update both culture and basic troop (game logic - documented for future extraction)
            clan.Culture = newCulture;
            clan.BasicTroop = newCulture.BasicTroop;

            string argumentDisplay = parsed.FormatArgumentDisplay("gm.clan.set_culture", resolvedValues);
            return CommandResult.Success(argumentDisplay + MessageFormatter.FormatSuccessMessage(
                $"{clan.Name}'s culture changed from '{previousCulture}' to '{clan.Culture.Name}'.\n" +
                $"Basic troop updated to: {clan.BasicTroop?.Name}")).Message
;
        });
    }
}
