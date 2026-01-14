using Bannerlord.GameMaster.Behaviours;
using Bannerlord.GameMaster.Characters;
using Bannerlord.GameMaster.Console.Common;
using Bannerlord.GameMaster.Cultures;
using Bannerlord.GameMaster.Heroes;
using Bannerlord.GameMaster.Settlements;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Library;

namespace Bannerlord.GameMaster.Console.SettlementCommands
{
    /// <summary>
    /// Commands that operate on any settlement type, towns, castles, villages.
    /// </summary>
    [CommandLineFunctionality.CommandLineArgumentFunction("settlement", "gm")]
    public static class SettlementManagementCommands
    {
        /// MARK: rename
        /// <summary>
        /// Rename a settlement with save persistence
        /// Usage: gm.settlement.rename [settlement] [new_name]
        /// </summary>
        [CommandLineFunctionality.CommandLineArgumentFunction("rename", "gm.settlement")]
        public static string RenameSettlement(List<string> args)
        {
            return Cmd.Run(args, () =>
            {
                if (!CommandBase.ValidateCampaignState(out string error))
                    return error;

                var parsed = CommandBase.ParseArguments(args);
                parsed.SetValidArguments(
                    new CommandBase.ArgumentDefinition("settlement", true),
                    new CommandBase.ArgumentDefinition("name", true)
                );

                string validationError = parsed.GetValidationError();
                if (validationError != null)
                    return CommandBase.FormatErrorMessage(validationError);

                var usageMessage = CommandValidator.CreateUsageMessage(
                    "gm.settlement.rename", "<settlement> <new_name>",
                    "Changes the name of any settlement type (city, castle, village, hideout).\n" +
                    "The new name persists through save/load cycles.\n" +
                    "Use SINGLE QUOTES for multi-word names (double quotes don't work in TaleWorlds console).",
                    "gm.settlement.rename pen NewName\ngm.settlement.rename pen 'Castle of Stone'");

                if (!CommandBase.ValidateArgumentCount(args, 2, usageMessage, out error))
                    return error;

                string settlementQuery = parsed.GetArgument("settlement", 0);
                string newName = parsed.GetArgument("name", 1);

                var (settlement, settlementError) = CommandBase.FindSingleSettlement(settlementQuery);
                if (settlementError != null) return settlementError;

                if (string.IsNullOrWhiteSpace(newName))
                    return CommandBase.FormatErrorMessage("New name cannot be empty.");

                return CommandBase.ExecuteWithErrorHandling(() =>
                {
                    string previousName = settlement.Name.ToString();
                    
                    // Get the settlement name behavior
                    var behavior = Campaign.Current.GetCampaignBehavior<SettlementNameBehavior>();
                    if (behavior == null)
                        return CommandBase.FormatErrorMessage("Settlement name behavior not initialized. Please restart the game.");

                    // Use behavior to rename (handles save persistence)
                    if (!behavior.RenameSettlement(settlement, newName))
                        return CommandBase.FormatErrorMessage("Failed to rename settlement. Check the error log for details.");

                    var resolvedValues = new Dictionary<string, string>
                    {
                        ["settlement"] = previousName,
                        ["name"] = newName
                    };

                    string display = parsed.FormatArgumentDisplay("gm.settlement.rename", resolvedValues);
                    return display + CommandBase.FormatSuccessMessage(
                        $"Settlement renamed from '{previousName}' to '{settlement.Name}' (ID: {settlement.StringId}).\n" +
                        $"The new name will persist through save/load cycles.\n" +
                        $"Note: Map label may take a moment to update.");
                }, "Failed to rename settlement");
            });
        }

        /// MARK: reset_name
        /// <summary>
        /// Reset a settlement to its original name
        /// Usage: gm.settlement.reset_name [settlement]
        /// </summary>
        [CommandLineFunctionality.CommandLineArgumentFunction("reset_name", "gm.settlement")]
        public static string ResetSettlementName(List<string> args)
        {
            return Cmd.Run(args, () =>
            {
                if (!CommandBase.ValidateCampaignState(out string error))
                    return error;

                var parsed = CommandBase.ParseArguments(args);
                parsed.SetValidArguments(
                    new CommandBase.ArgumentDefinition("settlement", true)
                );

                string validationError = parsed.GetValidationError();
                if (validationError != null)
                    return CommandBase.FormatErrorMessage(validationError);

                var usageMessage = CommandValidator.CreateUsageMessage(
                    "gm.settlement.reset_name", "<settlement>",
                    "Restores a settlement to its original name.",
                    "gm.settlement.reset_name pen");

                if (!CommandBase.ValidateArgumentCount(args, 1, usageMessage, out error))
                    return error;

                string settlementQuery = parsed.GetArgument("settlement", 0);

                var (settlement, settlementError) = CommandBase.FindSingleSettlement(settlementQuery);
                if (settlementError != null) return settlementError;

                return CommandBase.ExecuteWithErrorHandling(() =>
                {
                    var behavior = Campaign.Current.GetCampaignBehavior<SettlementNameBehavior>();
                    if (behavior == null)
                        return CommandBase.FormatErrorMessage("Settlement name behavior not initialized. Please restart the game.");

                    if (!behavior.IsRenamed(settlement))
                        return CommandBase.FormatErrorMessage($"Settlement '{settlement.Name}' (ID: {settlement.StringId}) has not been renamed.");

                    string originalName = behavior.GetOriginalName(settlement);
                    string currentName = settlement.Name.ToString();

                    if (!behavior.ResetSettlementName(settlement))
                        return CommandBase.FormatErrorMessage("Failed to reset settlement name. Check the error log for details.");

                    var resolvedValues = new Dictionary<string, string>
                    {
                        ["settlement"] = currentName
                    };

                    string display = parsed.FormatArgumentDisplay("gm.settlement.reset_name", resolvedValues);
                    return display + CommandBase.FormatSuccessMessage(
                        $"Settlement name reset from '{currentName}' to '{settlement.Name}' (original: '{originalName}') (ID: {settlement.StringId}).");
                }, "Failed to reset settlement name");
            });
        }

        /// MARK: reset_all_names
        /// <summary>
        /// Reset all settlements to their original names
        /// Usage: gm.settlement.reset_all_names
        /// </summary>
        [CommandLineFunctionality.CommandLineArgumentFunction("reset_all_names", "gm.settlement")]
        public static string ResetAllSettlementNames(List<string> args)
        {
            return Cmd.Run(args, () =>
            {
                if (!CommandBase.ValidateCampaignState(out string error))
                    return error;

                var usageMessage = CommandValidator.CreateUsageMessage(
                    "gm.settlement.reset_all_names", "",
                    "Restores all settlements to their original names.",
                    "gm.settlement.reset_all_names");

                if (!CommandBase.ValidateArgumentCount(args, 0, usageMessage, out error))
                    return error;

                return CommandBase.ExecuteWithErrorHandling(() =>
                {
                    var behavior = Campaign.Current.GetCampaignBehavior<SettlementNameBehavior>();
                    if (behavior == null)
                        return CommandBase.FormatErrorMessage("Settlement name behavior not initialized. Please restart the game.");

                    int renamedCount = behavior.GetRenamedSettlementCount();
                    if (renamedCount == 0)
                        return CommandBase.FormatSuccessMessage("No settlements have been renamed.");

                    int resetCount = behavior.ResetAllSettlementNames();

                    return CommandBase.FormatSuccessMessage(
                        $"Reset {resetCount} settlement(s) to their original names.");
                }, "Failed to reset settlement names");
            });
        }

        /// MARK: set_culture
        /// <summary>
        /// Change settlement culture with persistence
        /// Usage: gm.settlement.set_culture [settlement] [culture] [update_bound_villages]
        /// </summary>
        [CommandLineFunctionality.CommandLineArgumentFunction("set_culture", "gm.settlement")]
        public static string SetCulture(List<string> args)
        {
            return Cmd.Run(args, () =>
            {
                if (!CommandBase.ValidateCampaignState(out string error))
                    return error;

                var parsed = CommandBase.ParseArguments(args);
                parsed.SetValidArguments(
                    new CommandBase.ArgumentDefinition("settlement", true),
                    new CommandBase.ArgumentDefinition("culture", true),
                    new CommandBase.ArgumentDefinition("update_bound_villages", false)
                );

                string validationError = parsed.GetValidationError();
                if (validationError != null)
                    return CommandBase.FormatErrorMessage(validationError);

                var usageMessage = CommandValidator.CreateUsageMessage(
                    "gm.settlement.set_culture", "<settlement> <culture> [update_bound_villages]",
                    "Changes the culture of a settlement with save persistence. This affects available troops, architecture style, and names.\n" +
                    "The culture change persists through save/load cycles and updates all notables in the settlement.\n" +
                    "- settlement: Settlement name or ID to change culture for\n" +
                    "- culture: Culture string ID (empire, sturgia, aserai, vlandia, battania, khuzait)\n" +
                    "- update_bound_villages: Optional boolean (true/false), defaults to false. When true AND settlement is a town, also updates all bound villages",
                    "gm.settlement.set_culture pen empire\n" +
                    "gm.settlement.set_culture marunath empire true\n" +
                    "gm.settlement.set_culture zeonica vlandia false");

                if (!CommandBase.ValidateArgumentCount(args, 2, usageMessage, out error))
                    return error;

                string settlementQuery = parsed.GetArgument("settlement", 0);
                string cultureQuery = parsed.GetArgument("culture", 1);

                // Parse optional update_bound_villages parameter (defaults to false)
                bool updateBoundVillages = false;
                if (args.Count > 2)
                {
                    string updateBoundVillagesStr = parsed.GetArgument("update_bound_villages", 2);
                    if (!string.IsNullOrWhiteSpace(updateBoundVillagesStr))
                    {
                        if (updateBoundVillagesStr.ToLower() == "true")
                        {
                            updateBoundVillages = true;
                        }
                        else if (updateBoundVillagesStr.ToLower() == "false")
                        {
                            updateBoundVillages = false;
                        }
                        else
                        {
                            return CommandBase.FormatErrorMessage($"Invalid value for update_bound_villages: '{updateBoundVillagesStr}'. Must be 'true' or 'false'.");
                        }
                    }
                }

                var (settlement, settlementError) = CommandBase.FindSingleSettlement(settlementQuery);
                if (settlementError != null) return settlementError;

                // Find the culture
                CultureObject culture = Campaign.Current.ObjectManager.GetObjectTypeList<CultureObject>()
                    .FirstOrDefault(c => c.StringId.ToLower().Contains(cultureQuery.ToLower()) ||
                                        c.Name.ToString().ToLower().Contains(cultureQuery.ToLower()));

                if (culture == null)
                    return CommandBase.FormatErrorMessage($"Culture not found matching '{cultureQuery}'. Valid cultures include: empire, sturgia, aserai, vlandia, battania, khuzait.");

                return CommandBase.ExecuteWithErrorHandling(() =>
                {
                    // Get the settlement culture behavior
                    SettlementCultureBehavior behavior = Campaign.Current.GetCampaignBehavior<SettlementCultureBehavior>();
                    if (behavior == null)
                        return CommandBase.FormatErrorMessage("Settlement culture behavior not initialized. Please restart the game.");

                    string previousCulture = settlement.Culture?.Name?.ToString() ?? "None";
                    int notableCount = settlement.Notables?.Count() ?? 0;
                    int boundVillageCount = updateBoundVillages ? SettlementManager.GetBoundVillagesCount(settlement) : 0;

                    // Use behavior to change culture with persistence
                    bool success = behavior.SetSettlementCulture(settlement, culture, updateNotables: true, includeBoundVillages: updateBoundVillages);
                    
                    if (!success)
                        return CommandBase.FormatErrorMessage("Failed to change settlement culture. Check the error log for details.");

                    var resolvedValues = new Dictionary<string, string>
                    {
                        ["settlement"] = settlement.Name.ToString(),
                        ["culture"] = culture.Name.ToString(),
                        ["update_bound_villages"] = updateBoundVillages.ToString().ToLower()
                    };

                    string display = parsed.FormatArgumentDisplay("gm.settlement.set_culture", resolvedValues);
                    
                    string message = $"Settlement culture changed successfully.\n" +
                                   $"Changed '{settlement.Name}' (ID: {settlement.StringId}) from '{previousCulture}' to '{culture.Name}'.\n" +
                                   $"Updated {notableCount} notable(s).";
                    
                    if (updateBoundVillages && boundVillageCount > 0)
                    {
                        message += $"\nUpdated {boundVillageCount} bound village(s).";
                    }
                    
                    message += "\nCulture change persists through save/load.\n" +
                              "Recruit slots will refresh naturally over time.";

                    return display + CommandBase.FormatSuccessMessage(message);
                }, "Failed to change settlement culture");
            });
        }
    }
}
