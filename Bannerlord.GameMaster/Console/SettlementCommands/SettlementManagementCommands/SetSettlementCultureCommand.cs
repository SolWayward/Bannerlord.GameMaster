using Bannerlord.GameMaster.Behaviours;
using Bannerlord.GameMaster.Console.Common;
using Bannerlord.GameMaster.Console.Common.Execution;
using Bannerlord.GameMaster.Console.Common.EntityFinding;
using Bannerlord.GameMaster.Console.Common.Formatting;
using Bannerlord.GameMaster.Console.Common.Parsing;
using Bannerlord.GameMaster.Console.Common.Validation;
using Bannerlord.GameMaster.Settlements;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;
using TaleWorlds.Library;

namespace Bannerlord.GameMaster.Console.SettlementCommands.SettlementManagementCommands;

/// <summary>
/// Change settlement culture with persistence
/// Usage: gm.settlement.set_culture [settlement] [culture] [update_bound_villages]
/// </summary>
public static class SetSettlementCultureCommand
{
    [CommandLineFunctionality.CommandLineArgumentFunction("set_culture", "gm.settlement")]
    public static string SetCulture(List<string> args)
    {
        return Cmd.Run(args, () =>
        {
            // MARK: Validation
            if (!CommandValidator.ValidateCampaignState(out string error))
                return CommandResult.Error(error).Message;

            string usageMessage = CommandValidator.CreateUsageMessage(
                "gm.settlement.set_culture", "<settlement> <culture> [update_bound_villages]",
                "Changes the culture of a settlement with save persistence. This affects available troops, architecture style, and names.\n" +
                "The culture change persists through save/load cycles and updates all notables in the settlement.\n" +
                "- settlement: Settlement name or ID to change culture for\n" +
                "- culture: Culture string ID (empire, sturgia, aserai, vlandia, battania, khuzait)\n" +
                "- update_bound_villages: Optional boolean (true/false), defaults to false. When true AND settlement is a town, also updates all bound villages",
                "gm.settlement.set_culture pen empire\n" +
                "gm.settlement.set_culture marunath empire true\n" +
                "gm.settlement.set_culture zeonica vlandia false");

            ParsedArguments parsed = ArgumentParser.ParseArguments(args);
            parsed.SetValidArguments(
                new ArgumentDefinition("settlement", true),
                new ArgumentDefinition("culture", true),
                new ArgumentDefinition("update_bound_villages", false)
            );

            string validationError = parsed.GetValidationError();
            if (validationError != null)
                return CommandResult.Error(MessageFormatter.FormatErrorMessage(validationError)).Message;

            if (parsed.TotalCount < 2)
                return CommandResult.Error(usageMessage).Message;

            // MARK: Parse Arguments
            string settlementQuery = parsed.GetArgument("settlement", 0);
            string cultureQuery = parsed.GetArgument("culture", 1);

            EntityFinderResult<Settlement> settlementResult = SettlementFinder.FindSingleSettlement(settlementQuery);
            if (!settlementResult.IsSuccess) return CommandResult.Error(settlementResult.Message).Message;
            Settlement settlement = settlementResult.Entity;

            // Parse optional update_bound_villages parameter (defaults to false)
            bool updateBoundVillages = false;
            string updateBoundVillagesStr = parsed.GetArgument("update_bound_villages", 2);
            if (!string.IsNullOrWhiteSpace(updateBoundVillagesStr))
            {
                if (updateBoundVillagesStr.ToLower() == "true")
                {
                    updateBoundVillages = true;
                }
                else if (updateBoundVillagesStr.ToLower() != "false")
                {
                    return CommandResult.Error(MessageFormatter.FormatErrorMessage($"Invalid value for update_bound_villages: '{updateBoundVillagesStr}'. Must be 'true' or 'false'.")).Message;
                }
            }

            // Find the culture
            CultureObject culture = Campaign.Current.ObjectManager.GetObjectTypeList<CultureObject>()
                .FirstOrDefault(c => c.StringId.ToLower().Contains(cultureQuery.ToLower()) ||
                                    c.Name.ToString().ToLower().Contains(cultureQuery.ToLower()));

            if (culture == null)
                return CommandResult.Error(MessageFormatter.FormatErrorMessage($"Culture not found matching '{cultureQuery}'. Valid cultures include: empire, sturgia, aserai, vlandia, battania, khuzait.")).Message;

            // MARK: Execute Logic
            SettlementCultureBehavior behavior = Campaign.Current.GetCampaignBehavior<SettlementCultureBehavior>();
            if (behavior == null)
                return CommandResult.Error(MessageFormatter.FormatErrorMessage("Settlement culture behavior not initialized. Please restart the game.")).Message;

            string previousCulture = settlement.Culture?.Name?.ToString() ?? "None";
            int notableCount = settlement.Notables?.Count() ?? 0;
            int boundVillageCount = updateBoundVillages ? SettlementManager.GetBoundVillagesCount(settlement) : 0;

            bool success = behavior.SetSettlementCulture(settlement, culture, updateNotables: true, includeBoundVillages: updateBoundVillages);

            if (!success)
                return CommandResult.Error(MessageFormatter.FormatErrorMessage("Failed to change settlement culture. Check the error log for details.")).Message;

            Dictionary<string, string> resolvedValues = new()
            {
                { "settlement", settlement.Name.ToString() },
                { "culture", culture.Name.ToString() },
                { "update_bound_villages", updateBoundVillages.ToString().ToLower() }
            };

            string argumentDisplay = parsed.FormatArgumentDisplay("gm.settlement.set_culture", resolvedValues);

            string message = $"Settlement culture changed successfully.\n" +
                           $"Changed '{settlement.Name}' (ID: {settlement.StringId}) from '{previousCulture}' to '{culture.Name}'.\n" +
                           $"Updated {notableCount} notable(s).";

            if (updateBoundVillages && boundVillageCount > 0)
            {
                message += $"\nUpdated {boundVillageCount} bound village(s).";
            }

            message += "\nCulture change persists through save/load.\n" +
                      "Recruit slots will refresh naturally over time.";

            string fullMessage = argumentDisplay + MessageFormatter.FormatSuccessMessage(message);
            return CommandResult.Success(fullMessage).Message;
        });
    }
}
