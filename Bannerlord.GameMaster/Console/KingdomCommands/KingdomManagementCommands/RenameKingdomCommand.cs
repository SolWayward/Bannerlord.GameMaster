using Bannerlord.GameMaster.Console.Common;
using Bannerlord.GameMaster.Console.Common.Execution;
using Bannerlord.GameMaster.Console.Common.EntityFinding;
using Bannerlord.GameMaster.Console.Common.Formatting;
using Bannerlord.GameMaster.Console.Common.Parsing;
using Bannerlord.GameMaster.Console.Common.Validation;
using Bannerlord.GameMaster.Kingdoms;
using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Library;

namespace Bannerlord.GameMaster.Console.KingdomCommands.KingdomManagementCommands;

/// <summary>
/// Rename a kingdom using KingdomExtensions.SetStringName
/// Usage: gm.kingdom.rename &lt;kingdom&gt; &lt;name&gt;
/// </summary>
public static class RenameKingdomCommand
{
    [CommandLineFunctionality.CommandLineArgumentFunction("rename", "gm.kingdom")]
    public static string RenameKingdom(List<string> args)
    {
        return Cmd.Run(args, () =>
        {
            // MARK: Validation
            if (!CommandValidator.ValidateCampaignState(out string error))
                return CommandResult.Error(error);

            string usageMessage = CommandValidator.CreateUsageMessage(
                "gm.kingdom.rename", "<kingdom> <name>",
                "Renames a kingdom.\n" +
                "Use SINGLE QUOTES for multi-word names (double quotes don't work in TaleWorlds console).\n" +
                "Supports named arguments: kingdom:sturgia name:'New Kingdom Name'",
                "gm.kingdom.rename sturgia 'Northern Empire'\n" +
                "gm.kingdom.rename kingdom:southern_empire name:'United Empire'");

            ParsedArguments parsed = ArgumentParser.ParseArguments(args);

            parsed.SetValidArguments(
                new ArgumentDefinition("kingdom", true),
                new ArgumentDefinition("name", true)
            );

            string validationError = parsed.GetValidationError();
            if (validationError != null)
                return CommandResult.Error(MessageFormatter.FormatErrorMessage(validationError));

            if (parsed.TotalCount < 2)
                return CommandResult.Error(usageMessage);

            // MARK: Parse Arguments
            string kingdomArg = parsed.GetArgument("kingdom", 0);
            if (string.IsNullOrWhiteSpace(kingdomArg))
                return CommandResult.Error(MessageFormatter.FormatErrorMessage("Missing required argument 'kingdom'."));

            EntityFinderResult<Kingdom> kingdomResult = KingdomFinder.FindSingleKingdom(kingdomArg);
            if (!kingdomResult.IsSuccess)
                return CommandResult.Error(kingdomResult.Message);
            Kingdom kingdom = kingdomResult.Entity;

            string newName = parsed.GetArgument("name", 1);
            if (string.IsNullOrWhiteSpace(newName))
                return CommandResult.Error(MessageFormatter.FormatErrorMessage("New name cannot be empty."));

            // MARK: Execute Logic
            string previousName = kingdom.Name.ToString();

            kingdom.SetStringName(newName);

            Dictionary<string, string> resolvedValues = new()
            {
                { "kingdom", previousName },
                { "name", newName }
            };

            string argumentDisplay = parsed.FormatArgumentDisplay("gm.kingdom.rename", resolvedValues);
            string fullMessage = argumentDisplay + MessageFormatter.FormatSuccessMessage(
                $"Kingdom renamed from '{previousName}' to '{kingdom.Name}' (ID: {kingdom.StringId}).");
            return CommandResult.Success(fullMessage);
        }).Message;
    }
}
