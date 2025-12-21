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
                if (!CommandBase.ValidateCampaignMode(out string error))
                    return error;

                // Create usage message
                var usageMessage = CommandValidator.CreateUsageMessage(
                    "gm.troops.give_hero_troops", "<hero_query> <character_query> <count>",
                    "Gives characters/troops to a hero's party. Accepts any CharacterObject including dancers, refugees, etc.",
                    "gm.troops.give_hero_troops player imperial_recruit 10");

                // Validate argument count (exactly 3)
                if (!CommandBase.ValidateArgumentCount(args, 3, usageMessage, out error))
                    return error;

                // Find hero
                var (hero, heroError) = CommandBase.FindSingleHero(args[0]);
                if (heroError != null) return heroError;

                // Find character (accepts any CharacterObject including dancers, refugees, etc.)
                var (character, characterError) = CommandBase.FindSingleCharacterObject(args[1]);
                if (characterError != null) return characterError;

                // Validate character is not a hero
                if (character.IsHero)
                    return CommandBase.FormatErrorMessage($"{character.Name} is a hero and cannot be added as a troop.");

                // Validate count (1 to 10000)
                if (!CommandValidator.ValidateIntegerRange(args[2], 1, 10000, out int count, out string countError))
                    return CommandBase.FormatErrorMessage(countError);

                // Execute the operation with error handling
                return CommandBase.ExecuteWithErrorHandling(() =>
                {
                    // Check hero has a party
                    if (hero.PartyBelongedTo == null)
                        return CommandBase.FormatErrorMessage($"{hero.Name} does not belong to a party.");

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

                    return CommandBase.FormatSuccessMessage(result.ToString());
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
                if (!CommandBase.ValidateCampaignMode(out string error))
                    return error;

                var usageMessage = CommandValidator.CreateUsageMessage(
                    "gm.troops.add_basic", "<partyLeader> <count>",
                    "Adds basic tier troops from the party leader's culture to their party.",
                    "gm.troops.add_basic derthert 50\n" +
                    "gm.troops.add_basic player 100");

                if (!CommandBase.ValidateArgumentCount(args, 2, usageMessage, out error))
                    return error;

                var (hero, heroError) = CommandBase.FindSingleHero(args[0]);
                if (heroError != null) return heroError;

                if (!CommandValidator.ValidateIntegerRange(args[1], 1, 10000, out int count, out string countError))
                    return CommandBase.FormatErrorMessage(countError);

                return CommandBase.ExecuteWithErrorHandling(() =>
                {
                    if (hero.PartyBelongedTo == null)
                        return CommandBase.FormatErrorMessage($"{hero.Name} does not belong to a party.");

                    if (hero.PartyBelongedTo.LeaderHero != hero)
                        return CommandBase.FormatErrorMessage($"{hero.Name} is not a party leader. They belong to {hero.PartyBelongedTo.LeaderHero.Name}'s party.");

                    hero.PartyBelongedTo.AddBasicTroops(count);

                    string troopName = hero.Culture?.BasicTroop?.Name?.ToString() ?? "basic troops";
                    return CommandBase.FormatSuccessMessage(
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
                if (!CommandBase.ValidateCampaignMode(out string error))
                    return error;

                var usageMessage = CommandValidator.CreateUsageMessage(
                    "gm.troops.add_elite", "<partyLeader> <count>",
                    "Adds elite tier troops from the party leader's culture to their party.",
                    "gm.troops.add_elite derthert 30\n" +
                    "gm.troops.add_elite player 50");

                if (!CommandBase.ValidateArgumentCount(args, 2, usageMessage, out error))
                    return error;

                var (hero, heroError) = CommandBase.FindSingleHero(args[0]);
                if (heroError != null) return heroError;

                if (!CommandValidator.ValidateIntegerRange(args[1], 1, 10000, out int count, out string countError))
                    return CommandBase.FormatErrorMessage(countError);

                return CommandBase.ExecuteWithErrorHandling(() =>
                {
                    if (hero.PartyBelongedTo == null)
                        return CommandBase.FormatErrorMessage($"{hero.Name} does not belong to a party.");

                    if (hero.PartyBelongedTo.LeaderHero != hero)
                        return CommandBase.FormatErrorMessage($"{hero.Name} is not a party leader. They belong to {hero.PartyBelongedTo.LeaderHero.Name}'s party.");

                    hero.PartyBelongedTo.AddEliteTroops(count);

                    string troopName = hero.Culture?.EliteBasicTroop?.Name?.ToString() ?? "elite troops";
                    return CommandBase.FormatSuccessMessage(
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
                if (!CommandBase.ValidateCampaignMode(out string error))
                    return error;

                var usageMessage = CommandValidator.CreateUsageMessage(
                    "gm.troops.add_mercenary", "<partyLeader> <count>",
                    "Adds random mercenary troops from the party leader's culture to their party.",
                    "gm.troops.add_mercenary derthert 20\n" +
                    "gm.troops.add_mercenary player 40");

                if (!CommandBase.ValidateArgumentCount(args, 2, usageMessage, out error))
                    return error;

                var (hero, heroError) = CommandBase.FindSingleHero(args[0]);
                if (heroError != null) return heroError;

                if (!CommandValidator.ValidateIntegerRange(args[1], 1, 10000, out int count, out string countError))
                    return CommandBase.FormatErrorMessage(countError);

                return CommandBase.ExecuteWithErrorHandling(() =>
                {
                    if (hero.PartyBelongedTo == null)
                        return CommandBase.FormatErrorMessage($"{hero.Name} does not belong to a party.");

                    if (hero.PartyBelongedTo.LeaderHero != hero)
                        return CommandBase.FormatErrorMessage($"{hero.Name} is not a party leader. They belong to {hero.PartyBelongedTo.LeaderHero.Name}'s party.");

                    hero.PartyBelongedTo.AddMercenaryTroops(count);

                    return CommandBase.FormatSuccessMessage(
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
                if (!CommandBase.ValidateCampaignMode(out string error))
                    return error;

                var usageMessage = CommandValidator.CreateUsageMessage(
                    "gm.troops.add_mixed", "<partyLeader> <countOfEach>",
                    "Adds mixed tier troops to the party. The count specifies how many of EACH type (basic, elite, mercenary) to add.\n" +
                    "For example, countOfEach=10 will add 30 total troops: 10 basic + 10 elite + 10 mercenary.",
                    "gm.troops.add_mixed derthert 15\n" +
                    "gm.troops.add_mixed player 20");

                if (!CommandBase.ValidateArgumentCount(args, 2, usageMessage, out error))
                    return error;

                var (hero, heroError) = CommandBase.FindSingleHero(args[0]);
                if (heroError != null) return heroError;

                if (!CommandValidator.ValidateIntegerRange(args[1], 1, 3000, out int countOfEach, out string countError))
                    return CommandBase.FormatErrorMessage(countError);

                return CommandBase.ExecuteWithErrorHandling(() =>
                {
                    if (hero.PartyBelongedTo == null)
                        return CommandBase.FormatErrorMessage($"{hero.Name} does not belong to a party.");

                    if (hero.PartyBelongedTo.LeaderHero != hero)
                        return CommandBase.FormatErrorMessage($"{hero.Name} is not a party leader. They belong to {hero.PartyBelongedTo.LeaderHero.Name}'s party.");

                    hero.PartyBelongedTo.AddMixedTierTroops(countOfEach);

                    int totalAdded = countOfEach * 3;
                    return CommandBase.FormatSuccessMessage(
                        $"Added {totalAdded} mixed tier troops to {hero.Name}'s party ({countOfEach} of each: basic, elite, mercenary).\n" +
                        $"Party: {hero.PartyBelongedTo.Name} (Total size: {hero.PartyBelongedTo.MemberRoster.TotalManCount})");
                }, "Failed to add mixed troops");
            });
        }
    }
}