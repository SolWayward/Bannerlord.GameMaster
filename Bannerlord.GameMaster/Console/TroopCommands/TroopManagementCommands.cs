using Bannerlord.GameMaster.Console.Common;
using Bannerlord.GameMaster.Troops;
using Bannerlord.GameMaster.Party;
using System;
using System.Collections.Generic;
using System.Text;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Library;

namespace Bannerlord.GameMaster.Console.TroopCommands
{
    [CommandLineFunctionality.CommandLineArgumentFunction("troops", "gm")]
    public static class TroopManagementCommands
    {
        /// <summary>
        /// Give troops to a hero's party
        /// Usage: gm.troops.give_hero_troops [heroQuery] [troopQuery] [count]
        /// </summary>
        [CommandLineFunctionality.CommandLineArgumentFunction("give_hero_troops", "gm.troops")]
        public static string GiveHeroTroops(List<string> args)
        {
            return Cmd.Run(args, () =>
            {
                // Validate campaign mode
                if (!CommandBase.ValidateCampaignState(out string error))
                    return error;

                // Create usage message
                var usageMessage = CommandValidator.CreateUsageMessage(
                    "gm.troops.give_hero_troops", "<hero_query> <character_query> <count>",
                    "Gives characters/troops to a hero's party. Accepts any CharacterObject including dancers, refugees, etc.\n" +
                    "Supports named arguments: hero:player character:imperial_recruit count:10",
                    "gm.troops.give_hero_troops player imperial_recruit 10");

                // Parse arguments
                var parsedArgs = CommandBase.ParseArguments(args);

                // Define valid arguments
                parsedArgs.SetValidArguments(
                    new CommandBase.ArgumentDefinition("hero", true),
                    new CommandBase.ArgumentDefinition("character", true, null, "troop"),
                    new CommandBase.ArgumentDefinition("count", true)
                );

                // Validate
                string validationError = parsedArgs.GetValidationError();
                if (validationError != null)
                    return CommandBase.FormatErrorMessage(validationError);

                if (parsedArgs.TotalCount < 3)
                    return usageMessage;

                // Parse hero
                string heroArg = parsedArgs.GetArgument("hero", 0);
                if (heroArg == null)
                    return CommandBase.FormatErrorMessage("Missing required argument 'hero'.");

                var (hero, heroError) = CommandBase.FindSingleHero(heroArg);
                if (heroError != null) return heroError;

                // Parse character
                string characterArg = parsedArgs.GetArgument("character", 1) ?? parsedArgs.GetNamed("troop");
                if (characterArg == null)
                    return CommandBase.FormatErrorMessage("Missing required argument 'character'.");

                var (character, characterError) = CommandBase.FindSingleCharacterObject(characterArg);
                if (characterError != null) return characterError;

                // Validate character is not a hero
                if (character.IsHero)
                    return CommandBase.FormatErrorMessage($"{character.Name} is a hero and cannot be added as a troop.");

                // Parse count
                string countArg = parsedArgs.GetArgument("count", 2);
                if (countArg == null)
                    return CommandBase.FormatErrorMessage("Missing required argument 'count'.");

                if (!CommandValidator.ValidateIntegerRange(countArg, 1, 10000, out int count, out string countError))
                    return CommandBase.FormatErrorMessage(countError);

                // Build display
                var resolvedValues = new Dictionary<string, string>
                {
                    { "hero", hero.Name.ToString() },
                    { "character", character.Name.ToString() },
                    { "count", count.ToString() }
                };
                string argumentDisplay = parsedArgs.FormatArgumentDisplay("give_hero_troops", resolvedValues);

                // Execute the operation with error handling
                return CommandBase.ExecuteWithErrorHandling(() =>
                {
                    // Check hero has a party
                    if (hero.PartyBelongedTo == null)
                        return argumentDisplay + CommandBase.FormatErrorMessage($"{hero.Name} does not belong to a party.");

                    // Check if hero is the party leader
                    bool isPartyLeader = hero.PartyBelongedTo.LeaderHero == hero;
                    
                    // Add character/troop to party
                    hero.PartyBelongedTo.MemberRoster.AddToCounts(character, count);

                    // Build success message
                    StringBuilder result = new StringBuilder();
                    result.AppendLine($"Added {count}x {character.Name} to {hero.Name}'s party.");
                    
                    // Include party info (especially if hero is not the leader)
                    if (!isPartyLeader)
                    {
                        result.AppendLine($"Party: {hero.PartyBelongedTo.Name} (Leader: {hero.PartyBelongedTo.LeaderHero.Name})");
                    }
                    else
                    {
                        result.AppendLine($"Party: {hero.PartyBelongedTo.Name}");
                    }

                    return argumentDisplay + CommandBase.FormatSuccessMessage(result.ToString());
                }, "Failed to give troops");
            });
        }

        //MARK: add_basic
        /// <summary>
        /// Add basic tier troops to a party leader's party
        /// Usage: gm.troops.add_basic [partyLeader] [count]
        /// </summary>
        [CommandLineFunctionality.CommandLineArgumentFunction("add_basic", "gm.troops")]
        public static string AddBasicTroops(List<string> args)
        {
            return Cmd.Run(args, () =>
            {
                if (!CommandBase.ValidateCampaignState(out string error))
                    return error;

                var usageMessage = CommandValidator.CreateUsageMessage(
                    "gm.troops.add_basic", "<partyLeader> <count>",
                    "Adds basic tier troops from the party leader's culture to their party.\n" +
                    "Supports named arguments: partyLeader:derthert count:50",
                    "gm.troops.add_basic derthert 50\n" +
                    "gm.troops.add_basic player 100");

                // Parse arguments
                var parsedArgs = CommandBase.ParseArguments(args);

                // Define valid arguments
                parsedArgs.SetValidArguments(
                    new CommandBase.ArgumentDefinition("partyLeader", true, null, "leader"),
                    new CommandBase.ArgumentDefinition("count", true)
                );

                // Validate
                string validationError = parsedArgs.GetValidationError();
                if (validationError != null)
                    return CommandBase.FormatErrorMessage(validationError);

                if (parsedArgs.TotalCount < 2)
                    return usageMessage;

                // Parse partyLeader
                string leaderArg = parsedArgs.GetArgument("partyLeader", 0) ?? parsedArgs.GetNamed("leader");
                if (leaderArg == null)
                    return CommandBase.FormatErrorMessage("Missing required argument 'partyLeader'.");

                var (hero, heroError) = CommandBase.FindSingleHero(leaderArg);
                if (heroError != null) return heroError;

                // Parse count
                string countArg = parsedArgs.GetArgument("count", 1);
                if (countArg == null)
                    return CommandBase.FormatErrorMessage("Missing required argument 'count'.");

                if (!CommandValidator.ValidateIntegerRange(countArg, 1, 10000, out int count, out string countError))
                    return CommandBase.FormatErrorMessage(countError);

                // Build display
                var resolvedValues = new Dictionary<string, string>
                {
                    { "partyLeader", hero.Name.ToString() },
                    { "count", count.ToString() }
                };
                string argumentDisplay = parsedArgs.FormatArgumentDisplay("add_basic", resolvedValues);

                return CommandBase.ExecuteWithErrorHandling(() =>
                {
                    if (hero.PartyBelongedTo == null)
                        return argumentDisplay + CommandBase.FormatErrorMessage($"{hero.Name} does not belong to a party.");

                    if (hero.PartyBelongedTo.LeaderHero != hero)
                        return argumentDisplay + CommandBase.FormatErrorMessage($"{hero.Name} is not a party leader. They belong to {hero.PartyBelongedTo.LeaderHero.Name}'s party.");

                    hero.PartyBelongedTo.AddBasicTroops(count);

                    string troopName = hero.Culture?.BasicTroop?.Name?.ToString() ?? "basic troops";
                    return argumentDisplay + CommandBase.FormatSuccessMessage(
                        $"Added {count}x {troopName} to {hero.Name}'s party.\n" +
                        $"Party: {hero.PartyBelongedTo.Name} (Total size: {hero.PartyBelongedTo.MemberRoster.TotalManCount})");
                }, "Failed to add basic troops");
            });
        }

        //MARK: add_elite
        /// <summary>
        /// Add elite tier troops to a party leader's party
        /// Usage: gm.troops.add_elite [partyLeader] [count]
        /// </summary>
        [CommandLineFunctionality.CommandLineArgumentFunction("add_elite", "gm.troops")]
        public static string AddEliteTroops(List<string> args)
        {
            return Cmd.Run(args, () =>
            {
                if (!CommandBase.ValidateCampaignState(out string error))
                    return error;

                var usageMessage = CommandValidator.CreateUsageMessage(
                    "gm.troops.add_elite", "<partyLeader> <count>",
                    "Adds elite tier troops from the party leader's culture to their party.\n" +
                    "Supports named arguments: partyLeader:derthert count:30",
                    "gm.troops.add_elite derthert 30\n" +
                    "gm.troops.add_elite player 50");

                // Parse arguments
                var parsedArgs = CommandBase.ParseArguments(args);

                // Define valid arguments
                parsedArgs.SetValidArguments(
                    new CommandBase.ArgumentDefinition("partyLeader", true, null, "leader"),
                    new CommandBase.ArgumentDefinition("count", true)
                );

                // Validate
                string validationError = parsedArgs.GetValidationError();
                if (validationError != null)
                    return CommandBase.FormatErrorMessage(validationError);

                if (parsedArgs.TotalCount < 2)
                    return usageMessage;

                // Parse partyLeader
                string leaderArg = parsedArgs.GetArgument("partyLeader", 0) ?? parsedArgs.GetNamed("leader");
                if (leaderArg == null)
                    return CommandBase.FormatErrorMessage("Missing required argument 'partyLeader'.");

                var (hero, heroError) = CommandBase.FindSingleHero(leaderArg);
                if (heroError != null) return heroError;

                // Parse count
                string countArg = parsedArgs.GetArgument("count", 1);
                if (countArg == null)
                    return CommandBase.FormatErrorMessage("Missing required argument 'count'.");

                if (!CommandValidator.ValidateIntegerRange(countArg, 1, 10000, out int count, out string countError))
                    return CommandBase.FormatErrorMessage(countError);

                // Build display
                var resolvedValues = new Dictionary<string, string>
                {
                    { "partyLeader", hero.Name.ToString() },
                    { "count", count.ToString() }
                };
                string argumentDisplay = parsedArgs.FormatArgumentDisplay("add_elite", resolvedValues);

                return CommandBase.ExecuteWithErrorHandling(() =>
                {
                    if (hero.PartyBelongedTo == null)
                        return argumentDisplay + CommandBase.FormatErrorMessage($"{hero.Name} does not belong to a party.");

                    if (hero.PartyBelongedTo.LeaderHero != hero)
                        return argumentDisplay + CommandBase.FormatErrorMessage($"{hero.Name} is not a party leader. They belong to {hero.PartyBelongedTo.LeaderHero.Name}'s party.");

                    hero.PartyBelongedTo.AddEliteTroops(count);

                    string troopName = hero.Culture?.EliteBasicTroop?.Name?.ToString() ?? "elite troops";
                    return argumentDisplay + CommandBase.FormatSuccessMessage(
                        $"Added {count}x {troopName} to {hero.Name}'s party.\n" +
                        $"Party: {hero.PartyBelongedTo.Name} (Total size: {hero.PartyBelongedTo.MemberRoster.TotalManCount})");
                }, "Failed to add elite troops");
            });
        }

        //MARK: add_mercenary
        /// <summary>
        /// Add random mercenary troops to a party leader's party
        /// Usage: gm.troops.add_mercenary [partyLeader] [count]
        /// </summary>
        [CommandLineFunctionality.CommandLineArgumentFunction("add_mercenary", "gm.troops")]
        public static string AddMercenaryTroops(List<string> args)
        {
            return Cmd.Run(args, () =>
            {
                if (!CommandBase.ValidateCampaignState(out string error))
                    return error;

                var usageMessage = CommandValidator.CreateUsageMessage(
                    "gm.troops.add_mercenary", "<partyLeader> <count>",
                    "Adds random mercenary troops from the party leader's culture to their party.\n" +
                    "Supports named arguments: partyLeader:derthert count:20",
                    "gm.troops.add_mercenary derthert 20\n" +
                    "gm.troops.add_mercenary player 40");

                // Parse arguments
                var parsedArgs = CommandBase.ParseArguments(args);

                // Define valid arguments
                parsedArgs.SetValidArguments(
                    new CommandBase.ArgumentDefinition("partyLeader", true, null, "leader"),
                    new CommandBase.ArgumentDefinition("count", true)
                );

                // Validate
                string validationError = parsedArgs.GetValidationError();
                if (validationError != null)
                    return CommandBase.FormatErrorMessage(validationError);

                if (parsedArgs.TotalCount < 2)
                    return usageMessage;

                // Parse partyLeader
                string leaderArg = parsedArgs.GetArgument("partyLeader", 0) ?? parsedArgs.GetNamed("leader");
                if (leaderArg == null)
                    return CommandBase.FormatErrorMessage("Missing required argument 'partyLeader'.");

                var (hero, heroError) = CommandBase.FindSingleHero(leaderArg);
                if (heroError != null) return heroError;

                // Parse count
                string countArg = parsedArgs.GetArgument("count", 1);
                if (countArg == null)
                    return CommandBase.FormatErrorMessage("Missing required argument 'count'.");

                if (!CommandValidator.ValidateIntegerRange(countArg, 1, 10000, out int count, out string countError))
                    return CommandBase.FormatErrorMessage(countError);

                // Build display
                var resolvedValues = new Dictionary<string, string>
                {
                    { "partyLeader", hero.Name.ToString() },
                    { "count", count.ToString() }
                };
                string argumentDisplay = parsedArgs.FormatArgumentDisplay("add_mercenary", resolvedValues);

                return CommandBase.ExecuteWithErrorHandling(() =>
                {
                    if (hero.PartyBelongedTo == null)
                        return argumentDisplay + CommandBase.FormatErrorMessage($"{hero.Name} does not belong to a party.");

                    if (hero.PartyBelongedTo.LeaderHero != hero)
                        return argumentDisplay + CommandBase.FormatErrorMessage($"{hero.Name} is not a party leader. They belong to {hero.PartyBelongedTo.LeaderHero.Name}'s party.");

                    hero.PartyBelongedTo.AddMercenaryTroops(count);

                    return argumentDisplay + CommandBase.FormatSuccessMessage(
                        $"Added {count}x random mercenary troops from {hero.Culture?.Name} culture to {hero.Name}'s party.\n" +
                        $"Party: {hero.PartyBelongedTo.Name} (Total size: {hero.PartyBelongedTo.MemberRoster.TotalManCount})");
                }, "Failed to add mercenary troops");
            });
        }

        //MARK: add_mixed
        /// <summary>
        /// Add mixed tier troops (basic, elite, and mercenary) to a party leader's party
        /// Usage: gm.troops.add_mixed [partyLeader] [countOfEach]
        /// </summary>
        [CommandLineFunctionality.CommandLineArgumentFunction("add_mixed", "gm.troops")]
        public static string AddMixedTroops(List<string> args)
        {
            return Cmd.Run(args, () =>
            {
                if (!CommandBase.ValidateCampaignState(out string error))
                    return error;

                var usageMessage = CommandValidator.CreateUsageMessage(
                    "gm.troops.add_mixed", "<partyLeader> <countOfEach>",
                    "Adds mixed tier troops to the party. The count specifies how many of EACH type (basic, elite, mercenary) to add.\n" +
                    "For example, countOfEach=10 will add 30 total troops: 10 basic + 10 elite + 10 mercenary.\n" +
                    "Supports named arguments: partyLeader:derthert countOfEach:15",
                    "gm.troops.add_mixed derthert 15\n" +
                    "gm.troops.add_mixed player 20");

                // Parse arguments
                var parsedArgs = CommandBase.ParseArguments(args);

                // Define valid arguments
                parsedArgs.SetValidArguments(
                    new CommandBase.ArgumentDefinition("partyLeader", true, null, "leader"),
                    new CommandBase.ArgumentDefinition("countOfEach", true, null, "count")
                );

                // Validate
                string validationError = parsedArgs.GetValidationError();
                if (validationError != null)
                    return CommandBase.FormatErrorMessage(validationError);

                if (parsedArgs.TotalCount < 2)
                    return usageMessage;

                // Parse partyLeader
                string leaderArg = parsedArgs.GetArgument("partyLeader", 0) ?? parsedArgs.GetNamed("leader");
                if (leaderArg == null)
                    return CommandBase.FormatErrorMessage("Missing required argument 'partyLeader'.");

                var (hero, heroError) = CommandBase.FindSingleHero(leaderArg);
                if (heroError != null) return heroError;

                // Parse countOfEach
                string countArg = parsedArgs.GetArgument("countOfEach", 1) ?? parsedArgs.GetNamed("count");
                if (countArg == null)
                    return CommandBase.FormatErrorMessage("Missing required argument 'countOfEach'.");

                if (!CommandValidator.ValidateIntegerRange(countArg, 1, 3000, out int countOfEach, out string countError))
                    return CommandBase.FormatErrorMessage(countError);

                // Build display
                var resolvedValues = new Dictionary<string, string>
                {
                    { "partyLeader", hero.Name.ToString() },
                    { "countOfEach", countOfEach.ToString() }
                };
                string argumentDisplay = parsedArgs.FormatArgumentDisplay("add_mixed", resolvedValues);

                return CommandBase.ExecuteWithErrorHandling(() =>
                {
                    if (hero.PartyBelongedTo == null)
                        return argumentDisplay + CommandBase.FormatErrorMessage($"{hero.Name} does not belong to a party.");

                    if (hero.PartyBelongedTo.LeaderHero != hero)
                        return argumentDisplay + CommandBase.FormatErrorMessage($"{hero.Name} is not a party leader. They belong to {hero.PartyBelongedTo.LeaderHero.Name}'s party.");

                    hero.PartyBelongedTo.AddMixedTierTroops(countOfEach);

                    int totalAdded = countOfEach * 3;
                    return argumentDisplay + CommandBase.FormatSuccessMessage(
                        $"Added {totalAdded} mixed tier troops to {hero.Name}'s party ({countOfEach} of each: basic, elite, mercenary).\n" +
                        $"Party: {hero.PartyBelongedTo.Name} (Total size: {hero.PartyBelongedTo.MemberRoster.TotalManCount})");
                }, "Failed to add mixed troops");
            });
        }

        //MARK: upgrade_troops
        /// <summary>
        /// Upgrade all troops in a party leader's party to specified tier
        /// Usage: gm.troops.upgrade_troops [partyLeader] [tier] [infantryRatio] [rangedRatio] [cavalryRatio]
        /// </summary>
        [CommandLineFunctionality.CommandLineArgumentFunction("upgrade_troops", "gm.troops")]
        public static string UpgradeTroops(List<string> args)
        {
            return Cmd.Run(args, () =>
            {
                if (!CommandBase.ValidateCampaignState(out string error))
                    return error;

                var usageMessage = CommandValidator.CreateUsageMessage(
                    "gm.troops.upgrade_troops", "<partyLeader> [tier] [infantryRatio] [rangedRatio] [cavalryRatio]",
                    "Upgrades all troops in the hero's party to specified tier or max tier of the troop if specified tier is higher.\n" +
                    "Attempts to maintain a ratio of troop types.\n" +
                    "Optional tier defaults to 7. Optional ratios 0 to 1 (defaults to infantry:0.5, ranged:0.3, cavalry:0.2).\n" +
                    "All ratios must add up to 1. If only one or two ratios are specified, remaining ratios will default to evenly add up to 1.\n" +
                    "Supports named arguments: partyLeader:derthert tier:6 infantryRatio:0.5 rangedRatio:0.3 cavalryRatio:0.2",
                    "gm.troops.upgrade_troops derthert\n" +
                    "gm.troops.upgrade_troops player 6\n" +
                    "gm.troops.upgrade_troops derthert 7 0.4 0.4 0.2");

                // Parse arguments
                var parsedArgs = CommandBase.ParseArguments(args);

                // Define valid arguments
                parsedArgs.SetValidArguments(
                    new CommandBase.ArgumentDefinition("partyLeader", true, null, "leader"),
                    new CommandBase.ArgumentDefinition("tier", false),
                    new CommandBase.ArgumentDefinition("infantryRatio", false, null, "infantry"),
                    new CommandBase.ArgumentDefinition("rangedRatio", false, null, "ranged"),
                    new CommandBase.ArgumentDefinition("cavalryRatio", false, null, "cavalry")
                );

                // Validate
                string validationError = parsedArgs.GetValidationError();
                if (validationError != null)
                    return CommandBase.FormatErrorMessage(validationError);

                if (parsedArgs.TotalCount < 1)
                    return usageMessage;

                // Parse partyLeader
                string leaderArg = parsedArgs.GetArgument("partyLeader", 0) ?? parsedArgs.GetNamed("leader");
                if (leaderArg == null)
                    return CommandBase.FormatErrorMessage("Missing required argument 'partyLeader'.");

                var (hero, heroError) = CommandBase.FindSingleHero(leaderArg);
                if (heroError != null) return heroError;

                // Parse tier (optional, default 7)
                int tier = 7;
                string tierArg = parsedArgs.GetArgument("tier", 1);
                if (tierArg != null)
                {
                    if (!CommandValidator.ValidateIntegerRange(tierArg, 1, 10, out tier, out string tierError))
                        return CommandBase.FormatErrorMessage(tierError);
                }

                // Parse ratios (optional)
                float? infantryRatio = null;
                string infantryArg = parsedArgs.GetArgument("infantryRatio", 2) ?? parsedArgs.GetNamed("infantry");
                if (infantryArg != null)
                {
                    if (!CommandValidator.ValidateFloatRange(infantryArg, 0f, 1f, out float infantry, out string infantryError))
                        return CommandBase.FormatErrorMessage(infantryError);
                    infantryRatio = infantry;
                }

                float? rangedRatio = null;
                string rangedArg = parsedArgs.GetArgument("rangedRatio", 3) ?? parsedArgs.GetNamed("ranged");
                if (rangedArg != null)
                {
                    if (!CommandValidator.ValidateFloatRange(rangedArg, 0f, 1f, out float ranged, out string rangedError))
                        return CommandBase.FormatErrorMessage(rangedError);
                    rangedRatio = ranged;
                }

                float? cavalryRatio = null;
                string cavalryArg = parsedArgs.GetArgument("cavalryRatio", 4) ?? parsedArgs.GetNamed("cavalry");
                if (cavalryArg != null)
                {
                    if (!CommandValidator.ValidateFloatRange(cavalryArg, 0f, 1f, out float cavalry, out string cavalryError))
                        return CommandBase.FormatErrorMessage(cavalryError);
                    cavalryRatio = cavalry;
                }

                // Validate ratios sum
                if (infantryRatio.HasValue && rangedRatio.HasValue && cavalryRatio.HasValue)
                {
                    float sum = infantryRatio.Value + rangedRatio.Value + cavalryRatio.Value;
                    if (Math.Abs(sum - 1.0f) > 0.01f)
                        return CommandBase.FormatErrorMessage($"Troop ratios must add up to 1.0. Current sum: {sum:F2}");
                }

                // Calculate actual ratios that will be used (including defaults for unspecified)
                var (actualRangedRatio, actualCavalryRatio, actualInfantryRatio) =
                    TroopUpgrader.NormalizeRatios(rangedRatio, cavalryRatio, infantryRatio);

                // Build display
                var resolvedValues = new Dictionary<string, string>
                {
                    { "partyLeader", hero.Name.ToString() },
                    { "tier", tier.ToString() },
                    { "infantryRatio", actualInfantryRatio.ToString("F2") },
                    { "rangedRatio", actualRangedRatio.ToString("F2") },
                    { "cavalryRatio", actualCavalryRatio.ToString("F2") }
                };

                string argumentDisplay = parsedArgs.FormatArgumentDisplay("upgrade_troops", resolvedValues);

                return CommandBase.ExecuteWithErrorHandling(() =>
                {
                    if (hero.PartyBelongedTo == null)
                        return argumentDisplay + CommandBase.FormatErrorMessage($"{hero.Name} does not belong to a party.");

                    if (hero.PartyBelongedTo.LeaderHero != hero)
                        return argumentDisplay + CommandBase.FormatErrorMessage($"{hero.Name} is not a party leader. They belong to {hero.PartyBelongedTo.LeaderHero.Name}'s party.");

                    // Call the extension method with proper parameter order (ranged, cavalry, infantry)
                    hero.PartyBelongedTo.UpgradeTroops(tier, rangedRatio, cavalryRatio, infantryRatio);

                    StringBuilder result = new StringBuilder();
                    result.AppendLine($"Upgraded troops in {hero.Name}'s party to tier {tier}.");
                    
                    if (infantryRatio.HasValue || rangedRatio.HasValue || cavalryRatio.HasValue)
                    {
                        result.Append("Target ratios - ");
                        if (infantryRatio.HasValue)
                            result.Append($"Infantry: {infantryRatio.Value:P0} ");
                        if (rangedRatio.HasValue)
                            result.Append($"Ranged: {rangedRatio.Value:P0} ");
                        if (cavalryRatio.HasValue)
                            result.Append($"Cavalry: {cavalryRatio.Value:P0}");
                        result.AppendLine();
                    }
                    
                    result.AppendLine($"Party: {hero.PartyBelongedTo.Name} (Total size: {hero.PartyBelongedTo.MemberRoster.TotalManCount})");

                    return argumentDisplay + CommandBase.FormatSuccessMessage(result.ToString());
                }, "Failed to upgrade troops");
            });
        }

        //MARK: give_xp
        /// <summary>
        /// Give XP to all troops in a party leader's party
        /// Usage: gm.troops.give_xp [partyLeader] [xp]
        /// </summary>
        [CommandLineFunctionality.CommandLineArgumentFunction("give_xp", "gm.troops")]
        public static string GiveXp(List<string> args)
        {
            return Cmd.Run(args, () =>
            {
                if (!CommandBase.ValidateCampaignState(out string error))
                    return error;

                var usageMessage = CommandValidator.CreateUsageMessage(
                    "gm.troops.give_xp", "<partyLeader> <xp>",
                    "Adds the specified experience to all troops in the hero's party.\n" +
                    "Supports named arguments: partyLeader:derthert xp:1000",
                    "gm.troops.give_xp derthert 1000\n" +
                    "gm.troops.give_xp player 500");

                // Parse arguments
                var parsedArgs = CommandBase.ParseArguments(args);

                // Define valid arguments
                parsedArgs.SetValidArguments(
                    new CommandBase.ArgumentDefinition("partyLeader", true, null, "leader"),
                    new CommandBase.ArgumentDefinition("xp", true)
                );

                // Validate
                string validationError = parsedArgs.GetValidationError();
                if (validationError != null)
                    return CommandBase.FormatErrorMessage(validationError);

                if (parsedArgs.TotalCount < 2)
                    return usageMessage;

                // Parse partyLeader
                string leaderArg = parsedArgs.GetArgument("partyLeader", 0) ?? parsedArgs.GetNamed("leader");
                if (leaderArg == null)
                    return CommandBase.FormatErrorMessage("Missing required argument 'partyLeader'.");

                var (hero, heroError) = CommandBase.FindSingleHero(leaderArg);
                if (heroError != null) return heroError;

                // Parse xp
                string xpArg = parsedArgs.GetArgument("xp", 1);
                if (xpArg == null)
                    return CommandBase.FormatErrorMessage("Missing required argument 'xp'.");

                if (!CommandValidator.ValidateIntegerRange(xpArg, 1, 1000000, out int xp, out string xpError))
                    return CommandBase.FormatErrorMessage(xpError);

                // Build display
                var resolvedValues = new Dictionary<string, string>
                {
                    { "partyLeader", hero.Name.ToString() },
                    { "xp", xp.ToString() }
                };
                string argumentDisplay = parsedArgs.FormatArgumentDisplay("give_xp", resolvedValues);

                return CommandBase.ExecuteWithErrorHandling(() =>
                {
                    if (hero.PartyBelongedTo == null)
                        return argumentDisplay + CommandBase.FormatErrorMessage($"{hero.Name} does not belong to a party.");

                    if (hero.PartyBelongedTo.LeaderHero != hero)
                        return argumentDisplay + CommandBase.FormatErrorMessage($"{hero.Name} is not a party leader. They belong to {hero.PartyBelongedTo.LeaderHero.Name}'s party.");

                    // Count non-hero troops before adding XP
                    int troopCount = 0;
                    foreach (var troop in hero.PartyBelongedTo.MemberRoster.GetTroopRoster())
                    {
                        if (!troop.Character.IsHero)
                            troopCount++;
                    }

                    hero.PartyBelongedTo.AddXp(xp);

                    return argumentDisplay + CommandBase.FormatSuccessMessage(
                        $"Added {xp} XP to {troopCount} troop types in {hero.Name}'s party.\n" +
                        $"Party: {hero.PartyBelongedTo.Name} (Total size: {hero.PartyBelongedTo.MemberRoster.TotalManCount})");
                }, "Failed to give XP to troops");
            });
        }
    }
}
