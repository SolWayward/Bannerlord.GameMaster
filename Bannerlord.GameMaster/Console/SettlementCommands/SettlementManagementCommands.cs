using Bannerlord.GameMaster.Behaviours;
using Bannerlord.GameMaster.Characters;
using Bannerlord.GameMaster.Console.Common;
using Bannerlord.GameMaster.Cultures;
using Bannerlord.GameMaster.Heroes;
using Bannerlord.GameMaster.Settlements;
using Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.Extensions;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Party.PartyComponents;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace Bannerlord.GameMaster.Console.SettlementCommands
{
    [CommandLineFunctionality.CommandLineArgumentFunction("settlement", "gm")]
    public static class SettlementManagementCommands
    {
        #region Settlement Ownership

        /// MARK: set_owner
        /// <summary>
        /// Change settlement owner to a hero
        /// Usage: gm.settlement.set_owner [settlement] [hero]
        /// </summary>
        [CommandLineFunctionality.CommandLineArgumentFunction("set_owner", "gm.settlement")]
        public static string SetOwner(List<string> args)
        {
            return Cmd.Run(args, () =>
            {
                if (!CommandBase.ValidateCampaignMode(out string error))
                    return error;

                var parsed = CommandBase.ParseArguments(args);
                parsed.SetValidArguments(
                    new CommandBase.ArgumentDefinition("settlement", true),
                    new CommandBase.ArgumentDefinition("hero", true)
                );

                string validationError = parsed.GetValidationError();
                if (validationError != null)
                    return CommandBase.FormatErrorMessage(validationError);

                var usageMessage = CommandValidator.CreateUsageMessage(
                    "gm.settlement.set_owner", "<settlement> <hero>",
                    "Changes the settlement owner to the specified hero. Also updates the owner clan to the hero's clan and map faction to the hero's faction (if any).",
                    "gm.settlement.set_owner pen lord_1_1");

                if (!CommandBase.ValidateArgumentCount(args, 2, usageMessage, out error))
                    return error;

                string settlementQuery = parsed.GetArgument("settlement", 0);
                string heroQuery = parsed.GetArgument("hero", 1);

                var (settlement, settlementError) = CommandBase.FindSingleSettlement(settlementQuery);
                if (settlementError != null) return settlementError;

                if (settlement.Town == null)
                    return CommandBase.FormatErrorMessage($"Settlement '{settlement.Name}' has no town likely because it is not a castle of city.");

                var (hero, heroError) = CommandBase.FindSingleHero(heroQuery);
                if (heroError != null) return heroError;

                return CommandBase.ExecuteWithErrorHandling(() =>
                {
                    string previousOwner = settlement.Owner?.Name?.ToString() ?? "None";
                    string previousClan = settlement.OwnerClan?.Name?.ToString() ?? "None";
                    string previousFaction = settlement.MapFaction?.Name?.ToString() ?? "None";

                    // Set owner if city or castle
                    settlement.Town.OwnerClan = hero.Clan;

                    var resolvedValues = new Dictionary<string, string>
                    {
                        ["settlement"] = settlement.Name.ToString(),
                        ["hero"] = hero.Name.ToString()
                    };

                    string display = parsed.FormatArgumentDisplay("gm.settlement.set_owner", resolvedValues);
                    return display + CommandBase.FormatSuccessMessage(
                        $"Settlement '{settlement.Name}' (ID: {settlement.StringId}) ownership changed:\n" +
                        $"Owner: {previousOwner} -> {settlement.Owner?.Name?.ToString() ?? "None"}\n" +
                        $"Owner Clan: {previousClan} -> {settlement.OwnerClan?.Name?.ToString() ?? "None"}\n" +
                        $"Map Faction: {previousFaction} -> {settlement.MapFaction?.Name?.ToString() ?? "None"}");
                }, "Failed to change settlement owner");
            });
        }

        /// MARK: set_owner_clan
        /// <summary>
        /// Change settlement owner clan
        /// Usage: gm.settlement.set_owner_clan [settlement] [clan]
        /// </summary>
        [CommandLineFunctionality.CommandLineArgumentFunction("set_owner_clan", "gm.settlement")]
        public static string SetOwnerClan(List<string> args)
        {
            return Cmd.Run(args, () =>
            {
                if (!CommandBase.ValidateCampaignMode(out string error))
                    return error;

                var parsed = CommandBase.ParseArguments(args);
                parsed.SetValidArguments(
                    new CommandBase.ArgumentDefinition("settlement", true),
                    new CommandBase.ArgumentDefinition("clan", true)
                );

                string validationError = parsed.GetValidationError();
                if (validationError != null)
                    return CommandBase.FormatErrorMessage(validationError);

                var usageMessage = CommandValidator.CreateUsageMessage(
                    "gm.settlement.set_owner_clan", "<settlement> <clan>",
                    "Changes the settlement owner clan to the specified clan. Also updates the owner to the clan leader and map faction to the clan's kingdom (if any).",
                    "gm.settlement.set_owner_clan pen empire_south");

                if (!CommandBase.ValidateArgumentCount(args, 2, usageMessage, out error))
                    return error;

                string settlementQuery = parsed.GetArgument("settlement", 0);
                string clanQuery = parsed.GetArgument("clan", 1);

                var (settlement, settlementError) = CommandBase.FindSingleSettlement(settlementQuery);
                if (settlementError != null) return settlementError;

                if (settlement.Town == null)
                    return CommandBase.FormatErrorMessage($"Settlement '{settlement.Name}' has no town likely because it is not a castle of city.");

                var (clan, clanError) = CommandBase.FindSingleClan(clanQuery);
                if (clanError != null) return clanError;

                if (clan.Leader == null)
                    return CommandBase.FormatErrorMessage($"Clan '{clan.Name}' has no leader.");

                return CommandBase.ExecuteWithErrorHandling(() =>
                {
                    string previousOwner = settlement.Owner?.Name?.ToString() ?? "None";
                    string previousClan = settlement.OwnerClan?.Name?.ToString() ?? "None";
                    string previousFaction = settlement.MapFaction?.Name?.ToString() ?? "None";

                    // Set Owner if city or castle
                    settlement.Town.OwnerClan = clan;

                    var resolvedValues = new Dictionary<string, string>
                    {
                        ["settlement"] = settlement.Name.ToString(),
                        ["clan"] = clan.Name.ToString()
                    };

                    string display = parsed.FormatArgumentDisplay("gm.settlement.set_owner_clan", resolvedValues);
                    return display + CommandBase.FormatSuccessMessage(
                        $"Settlement '{settlement.Name}' (ID: {settlement.StringId}) ownership changed:\n" +
                        $"Owner: {previousOwner} -> {settlement.Owner?.Name?.ToString() ?? "None"}\n" +
                        $"Owner Clan: {previousClan} -> {settlement.OwnerClan?.Name?.ToString() ?? "None"}\n" +
                        $"Map Faction: {previousFaction} -> {settlement.MapFaction?.Name?.ToString() ?? "None"}");
                }, "Failed to change settlement owner clan");
            });
        }

        /// MARK: set_owner_kingdom
        /// <summary>
        /// Change settlement owner kingdom
        /// Usage: gm.settlement.set_owner_kingdom [settlement] [kingdom]
        /// </summary>
        [CommandLineFunctionality.CommandLineArgumentFunction("set_owner_kingdom", "gm.settlement")]
        public static string SetOwnerKingdom(List<string> args)
        {
            return Cmd.Run(args, () =>
            {
                if (!CommandBase.ValidateCampaignMode(out string error))
                    return error;

                var parsed = CommandBase.ParseArguments(args);
                parsed.SetValidArguments(
                    new CommandBase.ArgumentDefinition("settlement", true),
                    new CommandBase.ArgumentDefinition("kingdom", true)
                );

                string validationError = parsed.GetValidationError();
                if (validationError != null)
                    return CommandBase.FormatErrorMessage(validationError);

                var usageMessage = CommandValidator.CreateUsageMessage(
                    "gm.settlement.set_owner_kingdom", "<settlement> <kingdom>",
                    "Changes the settlement owner kingdom. Also updates the owner to the kingdom ruler, owner clan to the ruler's clan, and map faction to the kingdom.",
                    "gm.settlement.set_owner_kingdom pen empire");

                if (!CommandBase.ValidateArgumentCount(args, 2, usageMessage, out error))
                    return error;

                string settlementQuery = parsed.GetArgument("settlement", 0);
                string kingdomQuery = parsed.GetArgument("kingdom", 1);

                var (settlement, settlementError) = CommandBase.FindSingleSettlement(settlementQuery);
                if (settlementError != null) return settlementError;

                if (settlement.Town == null)
                    return CommandBase.FormatErrorMessage($"Settlement '{settlement.Name}' has no town likely because it is not a castle of city.");

                var (kingdom, kingdomError) = CommandBase.FindSingleKingdom(kingdomQuery);
                if (kingdomError != null) return kingdomError;

                if (kingdom.Leader == null)
                    return CommandBase.FormatErrorMessage($"Kingdom '{kingdom.Name}' has no ruler.");

                if (kingdom.RulingClan == null)
                    return CommandBase.FormatErrorMessage($"Kingdom '{kingdom.Name}' has no ruling clan.");

                return CommandBase.ExecuteWithErrorHandling(() =>
                {
                    string previousOwner = settlement.Owner?.Name?.ToString() ?? "None";
                    string previousClan = settlement.OwnerClan?.Name?.ToString() ?? "None";
                    string previousFaction = settlement.MapFaction?.Name?.ToString() ?? "None";

                    // Set owner if castle or town
                    settlement.Town.OwnerClan = kingdom.RulingClan;

                    var resolvedValues = new Dictionary<string, string>
                    {
                        ["settlement"] = settlement.Name.ToString(),
                        ["kingdom"] = kingdom.Name.ToString()
                    };

                    string display = parsed.FormatArgumentDisplay("gm.settlement.set_owner_kingdom", resolvedValues);
                    return display + CommandBase.FormatSuccessMessage(
                        $"Settlement '{settlement.Name}' (ID: {settlement.StringId}) ownership changed:\n" +
                        $"Owner: {previousOwner} -> {settlement.Owner?.Name?.ToString() ?? "None"}\n" +
                        $"Owner Clan: {previousClan} -> {settlement.OwnerClan?.Name?.ToString() ?? "None"}\n" +
                        $"Map Faction: {previousFaction} -> {settlement.MapFaction?.Name?.ToString() ?? "None"}");
                }, "Failed to change settlement owner kingdom");
            });
        }

        #endregion

        #region Settlement Properties

        /// MARK: set_prosperity
        /// <summary>
        /// Set prosperity for a city or castle
        /// Usage: gm.settlement.set_prosperity [settlement] [value]
        /// </summary>
        [CommandLineFunctionality.CommandLineArgumentFunction("set_prosperity", "gm.settlement")]
        public static string SetProsperity(List<string> args)
        {
            return Cmd.Run(args, () =>
            {
                if (!CommandBase.ValidateCampaignMode(out string error))
                    return error;

                var parsed = CommandBase.ParseArguments(args);
                parsed.SetValidArguments(
                    new CommandBase.ArgumentDefinition("settlement", true),
                    new CommandBase.ArgumentDefinition("value", true)
                );

                string validationError = parsed.GetValidationError();
                if (validationError != null)
                    return CommandBase.FormatErrorMessage(validationError);

                var usageMessage = CommandValidator.CreateUsageMessage(
                    "gm.settlement.set_prosperity", "<settlement> <value>",
                    "Sets the prosperity of a city or castle.",
                    "gm.settlement.set_prosperity pen 5000");

                if (!CommandBase.ValidateArgumentCount(args, 2, usageMessage, out error))
                    return error;

                string settlementQuery = parsed.GetArgument("settlement", 0);
                string valueStr = parsed.GetArgument("value", 1);

                var (settlement, settlementError) = CommandBase.FindSingleSettlement(settlementQuery);
                if (settlementError != null) return settlementError;

                if (settlement.Town == null)
                    return CommandBase.FormatErrorMessage($"Settlement '{settlement.Name}' is not a city or castle.");

                if (!CommandValidator.ValidateFloatRange(valueStr, 0, 20000, out float value, out string valueError))
                    return CommandBase.FormatErrorMessage(valueError);

                return CommandBase.ExecuteWithErrorHandling(() =>
                {
                    float previousValue = settlement.Town.Prosperity;
                    settlement.Town.Prosperity = value;

                    var resolvedValues = new Dictionary<string, string>
                    {
                        ["settlement"] = settlement.Name.ToString(),
                        ["value"] = value.ToString("F0")
                    };

                    string display = parsed.FormatArgumentDisplay("gm.settlement.set_prosperity", resolvedValues);
                    return display + CommandBase.FormatSuccessMessage(
                        $"Settlement '{settlement.Name}' (ID: {settlement.StringId}) prosperity changed from {previousValue:F0} to {settlement.Town.Prosperity:F0}.");
                }, "Failed to set prosperity");
            });
        }

        /// MARK: set_hearths
        /// <summary>
        /// Set hearth for a village
        /// Usage: gm.settlement.set_hearths [settlement] [value]
        /// </summary>
        [CommandLineFunctionality.CommandLineArgumentFunction("set_hearths", "gm.settlement")]
        public static string SetHearths(List<string> args)
        {
            return Cmd.Run(args, () =>
            {
                if (!CommandBase.ValidateCampaignMode(out string error))
                    return error;

                var parsed = CommandBase.ParseArguments(args);
                parsed.SetValidArguments(
                    new CommandBase.ArgumentDefinition("settlement", true),
                    new CommandBase.ArgumentDefinition("value", true)
                );

                string validationError = parsed.GetValidationError();
                if (validationError != null)
                    return CommandBase.FormatErrorMessage(validationError);

                var usageMessage = CommandValidator.CreateUsageMessage(
                    "gm.settlement.set_hearths", "<settlement> <value>",
                    "Sets the hearth value of a village.",
                    "gm.settlement.set_hearths village_1 500");

                if (!CommandBase.ValidateArgumentCount(args, 2, usageMessage, out error))
                    return error;

                string settlementQuery = parsed.GetArgument("settlement", 0);
                string valueStr = parsed.GetArgument("value", 1);

                var (settlement, settlementError) = CommandBase.FindSingleSettlement(settlementQuery);
                if (settlementError != null) return settlementError;

                if (settlement.Village == null)
                    return CommandBase.FormatErrorMessage($"Settlement '{settlement.Name}' is not a village.");

                if (!CommandValidator.ValidateFloatRange(valueStr, 0, 2000, out float value, out string valueError))
                    return CommandBase.FormatErrorMessage(valueError);

                return CommandBase.ExecuteWithErrorHandling(() =>
                {
                    float previousValue = settlement.Village.Hearth;
                    settlement.Village.Hearth = value;

                    var resolvedValues = new Dictionary<string, string>
                    {
                        ["settlement"] = settlement.Name.ToString(),
                        ["value"] = value.ToString("F0")
                    };

                    string display = parsed.FormatArgumentDisplay("gm.settlement.set_hearths", resolvedValues);
                    return display + CommandBase.FormatSuccessMessage(
                        $"Village '{settlement.Name}' (ID: {settlement.StringId}) hearth changed from {previousValue:F0} to {settlement.Village.Hearth:F0}.");
                }, "Failed to set hearths");
            });
        }

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
                if (!CommandBase.ValidateCampaignMode(out string error))
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
                if (!CommandBase.ValidateCampaignMode(out string error))
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
                if (!CommandBase.ValidateCampaignMode(out string error))
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
                if (!CommandBase.ValidateCampaignMode(out string error))
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

        /// MARK: set_loyalty
        /// <summary>
        /// Set loyalty for a city or castle
        /// Usage: gm.settlement.set_loyalty [settlement] [value]
        /// </summary>
        [CommandLineFunctionality.CommandLineArgumentFunction("set_loyalty", "gm.settlement")]
        public static string SetLoyalty(List<string> args)
        {
            return Cmd.Run(args, () =>
            {
                if (!CommandBase.ValidateCampaignMode(out string error))
                    return error;

                var parsed = CommandBase.ParseArguments(args);
                parsed.SetValidArguments(
                    new CommandBase.ArgumentDefinition("settlement", true),
                    new CommandBase.ArgumentDefinition("value", true)
                );

                string validationError = parsed.GetValidationError();
                if (validationError != null)
                    return CommandBase.FormatErrorMessage(validationError);

                var usageMessage = CommandValidator.CreateUsageMessage(
                    "gm.settlement.set_loyalty", "<settlement> <value>",
                    "Sets the loyalty of a city or castle (0-100).",
                    "gm.settlement.set_loyalty pen 100");

                if (!CommandBase.ValidateArgumentCount(args, 2, usageMessage, out error))
                    return error;

                string settlementQuery = parsed.GetArgument("settlement", 0);
                string valueStr = parsed.GetArgument("value", 1);

                var (settlement, settlementError) = CommandBase.FindSingleSettlement(settlementQuery);
                if (settlementError != null) return settlementError;

                if (settlement.Town == null)
                    return CommandBase.FormatErrorMessage($"Settlement '{settlement.Name}' is not a city or castle.");

                if (!CommandValidator.ValidateFloatRange(valueStr, 0, 100, out float value, out string valueError))
                    return CommandBase.FormatErrorMessage(valueError);

                return CommandBase.ExecuteWithErrorHandling(() =>
                {
                    float previousValue = settlement.Town.Loyalty;
                    settlement.Town.Loyalty = value;

                    var resolvedValues = new Dictionary<string, string>
                    {
                        ["settlement"] = settlement.Name.ToString(),
                        ["value"] = value.ToString("F1")
                    };

                    string display = parsed.FormatArgumentDisplay("gm.settlement.set_loyalty", resolvedValues);
                    return display + CommandBase.FormatSuccessMessage(
                        $"Settlement '{settlement.Name}' (ID: {settlement.StringId}) loyalty changed from {previousValue:F1} to {settlement.Town.Loyalty:F1}.");
                }, "Failed to set loyalty");
            });
        }

        /// MARK: set_security
        /// <summary>
        /// Set security for a city or castle
        /// Usage: gm.settlement.set_security [settlement] [value]
        /// </summary>
        [CommandLineFunctionality.CommandLineArgumentFunction("set_security", "gm.settlement")]
        public static string SetSecurity(List<string> args)
        {
            return Cmd.Run(args, () =>
            {
                if (!CommandBase.ValidateCampaignMode(out string error))
                    return error;

                var parsed = CommandBase.ParseArguments(args);
                parsed.SetValidArguments(
                    new CommandBase.ArgumentDefinition("settlement", true),
                    new CommandBase.ArgumentDefinition("value", true)
                );

                string validationError = parsed.GetValidationError();
                if (validationError != null)
                    return CommandBase.FormatErrorMessage(validationError);

                var usageMessage = CommandValidator.CreateUsageMessage(
                    "gm.settlement.set_security", "<settlement> <value>",
                    "Sets the security of a city or castle (0-100).",
                    "gm.settlement.set_security pen 100");

                if (!CommandBase.ValidateArgumentCount(args, 2, usageMessage, out error))
                    return error;

                string settlementQuery = parsed.GetArgument("settlement", 0);
                string valueStr = parsed.GetArgument("value", 1);

                var (settlement, settlementError) = CommandBase.FindSingleSettlement(settlementQuery);
                if (settlementError != null) return settlementError;

                if (settlement.Town == null)
                    return CommandBase.FormatErrorMessage($"Settlement '{settlement.Name}' is not a city or castle.");

                if (!CommandValidator.ValidateFloatRange(valueStr, 0, 100, out float value, out string valueError))
                    return CommandBase.FormatErrorMessage(valueError);

                return CommandBase.ExecuteWithErrorHandling(() =>
                {
                    float previousValue = settlement.Town.Security;
                    settlement.Town.Security = value;

                    var resolvedValues = new Dictionary<string, string>
                    {
                        ["settlement"] = settlement.Name.ToString(),
                        ["value"] = value.ToString("F1")
                    };

                    string display = parsed.FormatArgumentDisplay("gm.settlement.set_security", resolvedValues);
                    return display + CommandBase.FormatSuccessMessage(
                        $"Settlement '{settlement.Name}' (ID: {settlement.StringId}) security changed from {previousValue:F1} to {settlement.Town.Security:F1}.");
                }, "Failed to set security");
            });
        }

        #endregion

        #region Settlement Resources

        /// MARK: upgrade_buildings
        /// <summary>
        /// Upgrade all buildings in a city or castle to specified level
        /// Usage: gm.settlement.upgrade_buildings [settlement] [level]
        /// </summary>
        [CommandLineFunctionality.CommandLineArgumentFunction("upgrade_buildings", "gm.settlement")]
        public static string UpgradeBuildings(List<string> args)
        {
            return Cmd.Run(args, () =>
            {
                if (!CommandBase.ValidateCampaignMode(out string error))
                    return error;

                var parsed = CommandBase.ParseArguments(args);
                parsed.SetValidArguments(
                    new CommandBase.ArgumentDefinition("settlement", true),
                    new CommandBase.ArgumentDefinition("level", true)
                );

                string validationError = parsed.GetValidationError();
                if (validationError != null)
                    return CommandBase.FormatErrorMessage(validationError);

                var usageMessage = CommandValidator.CreateUsageMessage(
                    "gm.settlement.upgrade_buildings", "<settlement> <level>",
                    "Upgrades all buildings in a city or castle to the specified level (0-3). WARNING: Level 4+ will crash the game.",
                    "gm.settlement.upgrade_buildings pen 3");

                if (!CommandBase.ValidateArgumentCount(args, 2, usageMessage, out error))
                    return error;

                string settlementQuery = parsed.GetArgument("settlement", 0);
                string levelStr = parsed.GetArgument("level", 1);

                var (settlement, settlementError) = CommandBase.FindSingleSettlement(settlementQuery);
                if (settlementError != null) return settlementError;

                if (settlement.Town == null)
                    return CommandBase.FormatErrorMessage($"Settlement '{settlement.Name}' is not a city or castle.");

                if (!CommandValidator.ValidateIntegerRange(levelStr, 0, 3, out int targetLevel, out string levelError))
                    return CommandBase.FormatErrorMessage(levelError);

                return CommandBase.ExecuteWithErrorHandling(() =>
                {
                    int upgradedCount = 0;
                    int skippedCount = 0;
                    
                    foreach (var building in settlement.Town.Buildings)
                    {
                        if (building.CurrentLevel < targetLevel)
                        {
                            building.CurrentLevel = targetLevel;
                            upgradedCount++;
                        }
                        else
                        {
                            skippedCount++;
                        }
                    }

                    var resolvedValues = new Dictionary<string, string>
                    {
                        ["settlement"] = settlement.Name.ToString(),
                        ["level"] = targetLevel.ToString()
                    };

                    string display = parsed.FormatArgumentDisplay("gm.settlement.upgrade_buildings", resolvedValues);

                    if (upgradedCount == 0)
                        return display + CommandBase.FormatSuccessMessage($"All buildings in '{settlement.Name}' are already at level {targetLevel} or higher.");

                    return display + CommandBase.FormatSuccessMessage(
                        $"Settlement '{settlement.Name}' (ID: {settlement.StringId}): upgraded {upgradedCount} building(s) to level {targetLevel}, {skippedCount} already at or above target level.");
                }, "Failed to upgrade buildings");
            });
        }

        /// MARK: give_food
        /// <summary>
        /// Give food to a settlement
        /// Usage: gm.settlement.give_food [settlement] [amount]
        /// </summary>
        [CommandLineFunctionality.CommandLineArgumentFunction("give_food", "gm.settlement")]
        public static string GiveFood(List<string> args)
        {
            return Cmd.Run(args, () =>
            {
                if (!CommandBase.ValidateCampaignMode(out string error))
                    return error;

                var parsed = CommandBase.ParseArguments(args);
                parsed.SetValidArguments(
                    new CommandBase.ArgumentDefinition("settlement", true),
                    new CommandBase.ArgumentDefinition("amount", true)
                );

                string validationError = parsed.GetValidationError();
                if (validationError != null)
                    return CommandBase.FormatErrorMessage(validationError);

                var usageMessage = CommandValidator.CreateUsageMessage(
                    "gm.settlement.give_food", "<settlement> <amount>",
                    "Adds food to a settlement's food stock.",
                    "gm.settlement.give_food pen 1000");

                if (!CommandBase.ValidateArgumentCount(args, 2, usageMessage, out error))
                    return error;

                string settlementQuery = parsed.GetArgument("settlement", 0);
                string amountStr = parsed.GetArgument("amount", 1);

                var (settlement, settlementError) = CommandBase.FindSingleSettlement(settlementQuery);
                if (settlementError != null) return settlementError;

                if (settlement.Town == null)
                    return CommandBase.FormatErrorMessage($"Settlement '{settlement.Name}' is not a city or castle.");

                if (!CommandValidator.ValidateFloatRange(amountStr, -100000, 100000, out float amount, out string amountError))
                    return CommandBase.FormatErrorMessage(amountError);

                return CommandBase.ExecuteWithErrorHandling(() =>
                {
                    float previousValue = settlement.Town.FoodStocks;
                    settlement.Town.FoodStocks += amount;

                    var resolvedValues = new Dictionary<string, string>
                    {
                        ["settlement"] = settlement.Name.ToString(),
                        ["amount"] = amount.ToString("F0")
                    };

                    string display = parsed.FormatArgumentDisplay("gm.settlement.give_food", resolvedValues);
                    return display + CommandBase.FormatSuccessMessage(
                        $"Settlement '{settlement.Name}' (ID: {settlement.StringId}) food stocks changed from {previousValue:F0} to {settlement.Town.FoodStocks:F0} ({(amount >= 0 ? "+" : "")}{amount:F0}).");
                }, "Failed to give food");
            });
        }

        /// MARK: give_gold
        /// <summary>
        /// Give gold to a settlement
        /// Usage: gm.settlement.give_gold [settlement] [amount]
        /// </summary>
        [CommandLineFunctionality.CommandLineArgumentFunction("give_gold", "gm.settlement")]
        public static string GiveGold(List<string> args)
        {
            return Cmd.Run(args, () =>
            {
                if (!CommandBase.ValidateCampaignMode(out string error))
                    return error;

                var parsed = CommandBase.ParseArguments(args);
                parsed.SetValidArguments(
                    new CommandBase.ArgumentDefinition("settlement", true),
                    new CommandBase.ArgumentDefinition("amount", true)
                );

                string validationError = parsed.GetValidationError();
                if (validationError != null)
                    return CommandBase.FormatErrorMessage(validationError);

                var usageMessage = CommandValidator.CreateUsageMessage(
                    "gm.settlement.give_gold", "<settlement> <amount>",
                    "Adds gold to a settlement's treasury.",
                    "gm.settlement.give_gold pen 10000");

                if (!CommandBase.ValidateArgumentCount(args, 2, usageMessage, out error))
                    return error;

                string settlementQuery = parsed.GetArgument("settlement", 0);
                string amountStr = parsed.GetArgument("amount", 1);

                var (settlement, settlementError) = CommandBase.FindSingleSettlement(settlementQuery);
                if (settlementError != null) return settlementError;

                if (settlement.Town == null)
                    return CommandBase.FormatErrorMessage($"Settlement '{settlement.Name}' is not a city or castle.");

                if (!CommandValidator.ValidateIntegerRange(amountStr, int.MinValue, int.MaxValue, out int amount, out string amountError))
                    return CommandBase.FormatErrorMessage(amountError);

                return CommandBase.ExecuteWithErrorHandling(() =>
                {
                    int previousValue = settlement.Town.Gold;
                    settlement.Town.ChangeGold(amount);

                    var resolvedValues = new Dictionary<string, string>
                    {
                        ["settlement"] = settlement.Name.ToString(),
                        ["amount"] = amount.ToString()
                    };

                    string display = parsed.FormatArgumentDisplay("gm.settlement.give_gold", resolvedValues);
                    return display + CommandBase.FormatSuccessMessage(
                        $"Settlement '{settlement.Name}' (ID: {settlement.StringId}) gold changed from {previousValue} to {settlement.Town.Gold} ({(amount >= 0 ? "+" : "")}{amount}).");
                }, "Failed to give gold");
            });
        }

        #endregion

        #region Settlement Military

        /// MARK: add_militia
        /// <summary>
        /// Add militia to a city or castle
        /// Usage: gm.settlement.add_militia [settlement] [amount]
        /// </summary>
        [CommandLineFunctionality.CommandLineArgumentFunction("add_militia", "gm.settlement")]
        public static string AddMilitia(List<string> args)
        {
            return Cmd.Run(args, () =>
            {
                if (!CommandBase.ValidateCampaignMode(out string error))
                    return error;

                var parsed = CommandBase.ParseArguments(args);
                parsed.SetValidArguments(
                    new CommandBase.ArgumentDefinition("settlement", true),
                    new CommandBase.ArgumentDefinition("amount", true)
                );

                string validationError = parsed.GetValidationError();
                if (validationError != null)
                    return CommandBase.FormatErrorMessage(validationError);

                var usageMessage = CommandValidator.CreateUsageMessage(
                    "gm.settlement.add_militia", "<settlement> <amount>",
                    "Adds militia troops to a city or castle.",
                    "gm.settlement.add_militia pen 100");

                if (!CommandBase.ValidateArgumentCount(args, 2, usageMessage, out error))
                    return error;

                string settlementQuery = parsed.GetArgument("settlement", 0);
                string amountStr = parsed.GetArgument("amount", 1);

                var (settlement, settlementError) = CommandBase.FindSingleSettlement(settlementQuery);
                if (settlementError != null) return settlementError;

                if (settlement.Town == null)
                    return CommandBase.FormatErrorMessage($"Settlement '{settlement.Name}' is not a city or castle.");

                if (!CommandValidator.ValidateFloatRange(amountStr, 0, 1000, out float amount, out string amountError))
                    return CommandBase.FormatErrorMessage(amountError);

                return CommandBase.ExecuteWithErrorHandling(() =>
                {
                    float previousValue = settlement.Militia;
                    settlement.Militia += amount;

                    var resolvedValues = new Dictionary<string, string>
                    {
                        ["settlement"] = settlement.Name.ToString(),
                        ["amount"] = amount.ToString("F0")
                    };

                    string display = parsed.FormatArgumentDisplay("gm.settlement.add_militia", resolvedValues);
                    return display + CommandBase.FormatSuccessMessage(
                        $"Settlement '{settlement.Name}' (ID: {settlement.StringId}) militia changed from {previousValue:F0} to {settlement.Militia:F0} (+{amount:F0}).");
                }, "Failed to add militia");
            });
        }

        /// MARK: fill_garison
        /// <summary>
        /// Fill garrison to maximum capacity using existing troop types
        /// Usage: gm.settlement.fill_garrison [settlement]
        /// </summary>
        [CommandLineFunctionality.CommandLineArgumentFunction("fill_garrison", "gm.settlement")]
        public static string FillGarrison(List<string> args)
        {
            return Cmd.Run(args, () =>
            {
                if (!CommandBase.ValidateCampaignMode(out string error))
                    return error;

                var parsed = CommandBase.ParseArguments(args);
                parsed.SetValidArguments(
                    new CommandBase.ArgumentDefinition("settlement", true)
                );

                string validationError = parsed.GetValidationError();
                if (validationError != null)
                    return CommandBase.FormatErrorMessage(validationError);

                var usageMessage = CommandValidator.CreateUsageMessage(
                    "gm.settlement.fill_garrison", "<settlement>",
                    "Fills the garrison to maximum capacity using a mix of troops already present.",
                    "gm.settlement.fill_garrison pen");

                if (!CommandBase.ValidateArgumentCount(args, 1, usageMessage, out error))
                    return error;

                string settlementQuery = parsed.GetArgument("settlement", 0);

                var (settlement, settlementError) = CommandBase.FindSingleSettlement(settlementQuery);
                if (settlementError != null) return settlementError;

                if (settlement.Town == null)
                    return CommandBase.FormatErrorMessage($"Settlement '{settlement.Name}' is not a city or castle.");

                if (settlement.Town.GarrisonParty == null)
                    return CommandBase.FormatErrorMessage($"Settlement '{settlement.Name}' has no garrison party.");

                return CommandBase.ExecuteWithErrorHandling(() =>
                {
                    var garrison = settlement.Town.GarrisonParty;
                    int currentSize = garrison.MemberRoster.TotalManCount;
                    int maxSize = garrison.Party.PartySizeLimit;
                    int spaceAvailable = maxSize - currentSize;

                    var resolvedValues = new Dictionary<string, string>
                    {
                        ["settlement"] = settlement.Name.ToString()
                    };

                    string display = parsed.FormatArgumentDisplay("gm.settlement.fill_garrison", resolvedValues);

                    if (spaceAvailable <= 0)
                        return display + CommandBase.FormatSuccessMessage($"Settlement '{settlement.Name}' garrison is already at maximum capacity ({currentSize}/{maxSize}).");

                    // Get existing troops and their proportions
                    var troopTypes = new List<(CharacterObject troop, int count)>();
                    foreach (var element in garrison.MemberRoster.GetTroopRoster())
                    {
                        if (element.Character != null && !element.Character.IsHero)
                        {
                            troopTypes.Add((element.Character, element.Number));
                        }
                    }

                    if (troopTypes.Count == 0)
                        return display + CommandBase.FormatErrorMessage($"Settlement '{settlement.Name}' has no troops to use as template for filling.");

                    // Calculate total existing troops for proportions
                    int totalExisting = troopTypes.Sum(t => t.count);

                    // Add troops proportionally
                    int addedCount = 0;
                    foreach (var (troop, count) in troopTypes)
                    {
                        // Calculate proportion of this troop type
                        float proportion = (float)count / totalExisting;
                        int toAdd = (int)(spaceAvailable * proportion);
                        
                        if (toAdd > 0)
                        {
                            garrison.MemberRoster.AddToCounts(troop, toAdd);
                            addedCount += toAdd;
                        }
                    }

                    // Add any remaining space with the most common troop
                    int remaining = spaceAvailable - addedCount;
                    if (remaining > 0)
                    {
                        var mostCommon = troopTypes.OrderByDescending(t => t.count).First();
                        garrison.MemberRoster.AddToCounts(mostCommon.troop, remaining);
                        addedCount += remaining;
                    }

                    return display + CommandBase.FormatSuccessMessage(
                        $"Settlement '{settlement.Name}' (ID: {settlement.StringId}) garrison filled from {currentSize} to {garrison.MemberRoster.TotalManCount} (+{addedCount}).");
                }, "Failed to fill garrison");
            });
        }

        #endregion


        #region Settlement Caravans and NPCs

        /// MARK: spawn_wanderer
        /// <summary>
        /// Spawn a wanderer hero in a settlement
        /// Usage: gm.settlement.spawn_wanderer [settlement] [name] [cultures] [gender] [randomFactor]
        /// </summary>
        [CommandLineFunctionality.CommandLineArgumentFunction("spawn_wanderer", "gm.settlement")]
        public static string SpawnWanderer(List<string> args)
        {
            return Cmd.Run(args, () =>
            {
                if (!CommandBase.ValidateCampaignMode(out string error))
                    return error;
          
                var usageMessage = CommandValidator.CreateUsageMessage(
                    "gm.settlement.spawn_wanderer", "<settlement> [name] [cultures] [gender] [randomFactor]",
                    "Spawns a wanderer hero with proper name, portrait, and stats in the specified settlement.\n" +
                    "- settlement: required, settlement ID or name where the wanderer will spawn\n" +
                    "- name: optional, custom name for the wanderer. Use SINGLE QUOTES for multi-word names. If not provided, generates random name\n" +
                    "- cultures: optional, defines the pool of cultures allowed. Defaults to main_cultures\n" +
                    "- gender: optional, use keywords both, female, or male. also allowed b, f, and m. Defaults to both\n" +
                    "- randomFactor: optional, float value between 0 and 1. defaults to 0.5\n" +
                    "Supports named arguments: settlement:pen name:'Wandering Bard' cultures:vlandia,battania gender:female\n",
                    "gm.settlement.spawn_wanderer pen\n" +
                    "gm.settlement.spawn_wanderer pen 'Wandering Bard'\n" +
                    "gm.settlement.spawn_wanderer pen null vlandia female\n" +
                    "gm.settlement.spawn_wanderer settlement:zeonica name:'Skilled Archer' cultures:empire,aserai gender:male\n" +
                    "gm.settlement.spawn_wanderer zeonica 'Skilled Archer' empire,aserai male 0.8");
          
                if (!CommandBase.ValidateArgumentCount(args, 1, usageMessage, out error))
                    return error;
          
                var (settlement, settlementError) = CommandBase.FindSingleSettlement(args[0]);
                if (settlementError != null) return settlementError;
          
                // Parse optional name
                string name = null;
                int currentArgIndex = 1;
                if (args.Count > currentArgIndex)
                {
                    // Check if this is NOT a culture or gender keyword
                    GenderFlags testGender = FlagParser.ParseGenderArgument(args[currentArgIndex]);
                    CultureFlags testCulture = FlagParser.ParseCultureArgument(args[currentArgIndex]);
                    
                    if (testGender == GenderFlags.None && testCulture == CultureFlags.None && args[currentArgIndex].ToLower() != "null")
                    {
                        name = args[currentArgIndex];
                        currentArgIndex++;
                    }
                    else if (args[currentArgIndex].ToLower() == "null")
                    {
                        currentArgIndex++;
                    }
                }
          
                // Parse cultures (optional, defaults to AllMainCultures)
                CultureFlags cultureFlags = CultureFlags.AllMainCultures;
                if (args.Count > currentArgIndex)
                {
                    GenderFlags testGender = FlagParser.ParseGenderArgument(args[currentArgIndex]);
                    if (testGender != GenderFlags.None)
                    {
                        // Skip cultures, this is gender
                    }
                    else
                    {
                        cultureFlags = FlagParser.ParseCultureArgument(args[currentArgIndex]);
                        if (cultureFlags == CultureFlags.None)
                            return CommandBase.FormatErrorMessage($"Invalid culture(s): '{args[currentArgIndex]}'");
                        currentArgIndex++;
                    }
                }
          
                // Parse gender (optional, defaults to Either)
                GenderFlags genderFlags = GenderFlags.Either;
                if (args.Count > currentArgIndex)
                {
                    genderFlags = FlagParser.ParseGenderArgument(args[currentArgIndex]);
                    if (genderFlags == GenderFlags.None)
                        return CommandBase.FormatErrorMessage($"Invalid gender: '{args[currentArgIndex]}'. Use 'both/b', 'female/f', or 'male/m'");
                    currentArgIndex++;
                }
                
                // Parse randomFactor
                float randomFactor = 0.5f;
                if (args.Count > currentArgIndex)
                {
                    if (!CommandValidator.ValidateFloatRange(args[currentArgIndex], 0f, 1f, out randomFactor, out string randomError))
                        return CommandBase.FormatErrorMessage(randomError);
                }
          
                return CommandBase.ExecuteWithErrorHandling(() =>
                {
                    Hero wanderer;
                    
                    // Use new architecture - CreateWanderer method
                    if (string.IsNullOrWhiteSpace(name))
                    {
                        // Generate random name from culture
                        wanderer = HeroGenerator.CreateWanderers(1, cultureFlags, genderFlags, settlement, randomFactor)[0];
                    }
                    else
                    {
                        wanderer = HeroGenerator.CreateWanderer(name, cultureFlags, genderFlags, settlement, randomFactor);
                    }
          
                    if (wanderer == null)
                        return CommandBase.FormatErrorMessage("Failed to spawn wanderer - no templates found matching criteria");
                    
                    return CommandBase.FormatSuccessMessage(
                        $"Spawned wanderer '{wanderer.Name}' (ID: {wanderer.StringId}) in '{settlement.Name}'.\n" +
                        $"Culture: {wanderer.Culture?.Name} | Age: {(int)wanderer.Age} | Gender: {(wanderer.IsFemale ? "Female" : "Male")}\n" +
                        $"Wanderer can be recruited from the tavern in {settlement.Name}.");
                }, "Failed to spawn wanderer");
            });
        }

        #endregion

    }
}
