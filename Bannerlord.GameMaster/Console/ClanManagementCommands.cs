using Bannerlord.GameMaster.Console.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.Library;

namespace Bannerlord.GameMaster.Console
{
    [CommandLineFunctionality.CommandLineArgumentFunction("clan", "gm")]
    public static class ClanManagementCommands
    {
        #region Clan Management

        /// <summary>
        /// Transfer a hero to another clan
        /// Usage: gm.clan.add_hero [clan] [hero]
        /// </summary>
        [CommandLineFunctionality.CommandLineArgumentFunction("add_hero", "gm.clan")]
        public static string AddHeroToClan(List<string> args)
        {
            if (!CommandBase.ValidateCampaignMode(out string error))
                return error;

            var usageMessage = CommandValidator.CreateUsageMessage(
                "gm.clan.add_hero", "<clan> <hero>",
                "Adds a hero to the specified clan.",
                "gm.clan.add_hero empire_south lord_1_1");

            if (!CommandBase.ValidateArgumentCount(args, 2, usageMessage, out error))
                return error;

            var (clan, clanError) = CommandBase.FindSingleClan(args[0]);
            if (clanError != null) return clanError;

            var (hero, heroError) = CommandBase.FindSingleHero(args[1]);
            if (heroError != null) return heroError;

            return CommandBase.ExecuteWithErrorHandling(() =>
            {
                string previousClanName = hero.Clan?.Name?.ToString() ?? "No Clan";
                hero.Clan = clan;

                return CommandBase.FormatSuccessMessage($"{hero.Name} transferred from '{previousClanName}' to '{clan.Name}'.");
            }, "Failed to add hero to clan");
        }


        /// <summary>
        /// Add gold to all clan members
        /// Usage: gm.clan.add_gold [clan] [amount]
        /// </summary>
        [CommandLineFunctionality.CommandLineArgumentFunction("add_gold", "gm.clan")]
        public static string AddClanGold(List<string> args)
        {
            if (!CommandBase.ValidateCampaignMode(out string error))
                return error;

            var usageMessage = CommandValidator.CreateUsageMessage(
                "gm.clan.add_gold", "<clan> <amount>",
                "Adds gold to all members of the clan.",
                "gm.clan.add_gold empire_south 50000");

            if (!CommandBase.ValidateArgumentCount(args, 2, usageMessage, out error))
                return error;

            var (clan, clanError) = CommandBase.FindSingleClan(args[0]);
            if (clanError != null) return clanError;

            if (!CommandValidator.ValidateIntegerRange(args[1], int.MinValue, int.MaxValue, out int amount, out string goldError))
                return CommandBase.FormatErrorMessage(goldError);

            return CommandBase.ExecuteWithErrorHandling(() =>
            {
                int previousGold = clan.Gold;
                int membersCount = clan.Heroes.Count(h => h.IsAlive);

                if (membersCount == 0)
                    return CommandBase.FormatErrorMessage($"{clan.Name} has no living heroes to receive gold.");

                // Distribute gold evenly among all living clan members
                int goldPerMember = amount / membersCount;
                int remainder = amount % membersCount;

                foreach (var hero in clan.Heroes.Where(h => h.IsAlive))
                {
                    int goldToAdd = goldPerMember;
                    if (remainder > 0)
                    {
                        goldToAdd++;
                        remainder--;
                    }
                    hero.ChangeHeroGold(goldToAdd);
                }

                return CommandBase.FormatSuccessMessage(
                    $"Added {amount} gold to {clan.Name} (distributed among {membersCount} members).\n" +
                    $"Clan gold changed from {previousGold} to {clan.Gold}.");
            }, "Failed to add gold");
        }

        /// <summary>
        /// Set total clan gold by distributing to members
        /// Usage: gm.clan.set_gold [clan] [amount]
        /// </summary>
        [CommandLineFunctionality.CommandLineArgumentFunction("set_gold", "gm.clan")]
        public static string SetClanGold(List<string> args)
        {
            if (!CommandBase.ValidateCampaignMode(out string error))
                return error;

            var usageMessage = CommandValidator.CreateUsageMessage(
                "gm.clan.set_gold", "<clan> <amount>",
                "Sets the clan's total gold by distributing to all members.",
                "gm.clan.set_gold empire_south 100000");

            if (!CommandBase.ValidateArgumentCount(args, 2, usageMessage, out error))
                return error;

            var (clan, clanError) = CommandBase.FindSingleClan(args[0]);
            if (clanError != null) return clanError;

            if (!CommandValidator.ValidateIntegerRange(args[1], 0, int.MaxValue, out int targetAmount, out string goldError))
                return CommandBase.FormatErrorMessage(goldError);

            return CommandBase.ExecuteWithErrorHandling(() =>
            {
                int previousGold = clan.Gold;
                int membersCount = clan.Heroes.Count(h => h.IsAlive);

                if (membersCount == 0)
                    return CommandBase.FormatErrorMessage($"{clan.Name} has no living heroes to receive gold.");

                // First, zero out all member gold
                foreach (var hero in clan.Heroes.Where(h => h.IsAlive))
                {
                    if (hero.Gold > 0)
                        hero.ChangeHeroGold(-hero.Gold);
                }

                // Then distribute the target amount evenly
                int goldPerMember = targetAmount / membersCount;
                int remainder = targetAmount % membersCount;

                foreach (var hero in clan.Heroes.Where(h => h.IsAlive))
                {
                    int goldToAdd = goldPerMember;
                    if (remainder > 0)
                    {
                        goldToAdd++;
                        remainder--;
                    }
                    hero.ChangeHeroGold(goldToAdd);
                }

                return CommandBase.FormatSuccessMessage(
                    $"Set {clan.Name}'s gold to {targetAmount} (distributed among {membersCount} members).\n" +
                    $"Previous clan gold: {previousGold}, New clan gold: {clan.Gold}.");
            }, "Failed to set gold");
        }

        /// <summary>
        /// Add gold to clan leader only
        /// Usage: gm.clan.add_gold_leader [clan] [amount]
        /// </summary>
        [CommandLineFunctionality.CommandLineArgumentFunction("add_gold_leader", "gm.clan")]
        public static string AddGoldToLeader(List<string> args)
        {
            if (!CommandBase.ValidateCampaignMode(out string error))
                return error;

            var usageMessage = CommandValidator.CreateUsageMessage(
                "gm.clan.add_gold_leader", "<clan> <amount>",
                "Adds gold to the clan leader only.",
                "gm.clan.add_gold_leader empire_south 50000");

            if (!CommandBase.ValidateArgumentCount(args, 2, usageMessage, out error))
                return error;

            var (clan, clanError) = CommandBase.FindSingleClan(args[0]);
            if (clanError != null) return clanError;

            if (clan.Leader == null)
                return CommandBase.FormatErrorMessage($"{clan.Name} has no leader.");

            if (!CommandValidator.ValidateIntegerRange(args[1], int.MinValue, int.MaxValue, out int amount, out string goldError))
                return CommandBase.FormatErrorMessage(goldError);

            return CommandBase.ExecuteWithErrorHandling(() =>
            {
                int previousClanGold = clan.Gold;
                int previousLeaderGold = clan.Leader.Gold;

                clan.Leader.ChangeHeroGold(amount);

                return CommandBase.FormatSuccessMessage(
                    $"Added {amount} gold to {clan.Leader.Name} (leader of {clan.Name}).\n" +
                    $"Leader gold: {previousLeaderGold} → {clan.Leader.Gold}\n" +
                    $"Clan total gold: {previousClanGold} → {clan.Gold}");
            }, "Failed to add gold to leader");
        }

        /// <summary>
        /// Distribute gold to specific clan member
        /// Usage: gm.clan.give_gold [clan] [hero] [amount]
        /// </summary>
        [CommandLineFunctionality.CommandLineArgumentFunction("give_gold", "gm.clan")]
        public static string GiveGoldToMember(List<string> args)
        {
            if (!CommandBase.ValidateCampaignMode(out string error))
                return error;

            var usageMessage = CommandValidator.CreateUsageMessage(
                "gm.clan.give_gold", "<clan> <hero> <amount>",
                "Gives gold to a specific clan member.",
                "gm.clan.give_gold empire_south lord_1_1 10000");

            if (!CommandBase.ValidateArgumentCount(args, 3, usageMessage, out error))
                return error;

            var (clan, clanError) = CommandBase.FindSingleClan(args[0]);
            if (clanError != null) return clanError;

            var (hero, heroError) = CommandBase.FindSingleHero(args[1]);
            if (heroError != null) return heroError;

            if (hero.Clan != clan)
                return CommandBase.FormatErrorMessage($"{hero.Name} is not a member of {clan.Name}.");

            if (!CommandValidator.ValidateIntegerRange(args[2], int.MinValue, int.MaxValue, out int amount, out string goldError))
                return CommandBase.FormatErrorMessage(goldError);

            return CommandBase.ExecuteWithErrorHandling(() =>
            {
                int previousClanGold = clan.Gold;
                int previousHeroGold = hero.Gold;

                hero.ChangeHeroGold(amount);

                return CommandBase.FormatSuccessMessage(
                    $"Added {amount} gold to {hero.Name} (member of {clan.Name}).\n" +
                    $"Hero gold: {previousHeroGold} → {hero.Gold}\n" +
                    $"Clan total gold: {previousClanGold} → {clan.Gold}");
            }, "Failed to give gold to member");
        }

        /// <summary>
        /// Set clan renown
        /// Usage: gm.clan.set_renown [clan] [amount]
        /// </summary>
        [CommandLineFunctionality.CommandLineArgumentFunction("set_renown", "gm.clan")]
        public static string SetClanRenown(List<string> args)
        {
            if (!CommandBase.ValidateCampaignMode(out string error))
                return error;

            var usageMessage = CommandValidator.CreateUsageMessage(
                "gm.clan.set_renown", "<clan> <amount>",
                "Sets the clan's renown.",
                "gm.clan.set_renown empire_south 500");

            if (!CommandBase.ValidateArgumentCount(args, 2, usageMessage, out error))
                return error;

            var (clan, clanError) = CommandBase.FindSingleClan(args[0]);
            if (clanError != null) return clanError;

            if (!CommandValidator.ValidateFloatRange(args[1], 0, float.MaxValue, out float amount, out string renownError))
                return CommandBase.FormatErrorMessage(renownError);

            return CommandBase.ExecuteWithErrorHandling(() =>
            {
                float previousRenown = clan.Renown;
                clan.Renown = amount;
                return CommandBase.FormatSuccessMessage($"{clan.Name}'s renown changed from {previousRenown:F0} to {clan.Renown:F0}.");
            }, "Failed to set renown");
        }

        /// <summary>
        /// Add renown to clan
        /// Usage: gm.clan.add_renown [clan] [amount]
        /// </summary>
        [CommandLineFunctionality.CommandLineArgumentFunction("add_renown", "gm.clan")]
        public static string AddClanRenown(List<string> args)
        {
            if (!CommandBase.ValidateCampaignMode(out string error))
                return error;

            var usageMessage = CommandValidator.CreateUsageMessage(
                "gm.clan.add_renown", "<clan> <amount>",
                "Adds renown to the clan.",
                "gm.clan.add_renown empire_south 100");

            if (!CommandBase.ValidateArgumentCount(args, 2, usageMessage, out error))
                return error;

            var (clan, clanError) = CommandBase.FindSingleClan(args[0]);
            if (clanError != null) return clanError;

            if (!CommandValidator.ValidateFloatRange(args[1], float.MinValue, float.MaxValue, out float amount, out string renownError))
                return CommandBase.FormatErrorMessage(renownError);

            return CommandBase.ExecuteWithErrorHandling(() =>
            {
                float previousRenown = clan.Renown;
                clan.AddRenown(amount, true);
                return CommandBase.FormatSuccessMessage(
                    $"{clan.Name}'s renown changed from {previousRenown:F0} to {clan.Renown:F0} ({(amount >= 0 ? "+" : "")}{amount:F0}).");
            }, "Failed to add renown");
        }

        /// <summary>
        /// Change clan tier
        /// Usage: gm.clan.set_tier [clan] [tier]
        /// </summary>
        [CommandLineFunctionality.CommandLineArgumentFunction("set_tier", "gm.clan")]
        public static string SetClanTier(List<string> args)
        {
            if (!CommandBase.ValidateCampaignMode(out string error))
                return error;

            var usageMessage = CommandValidator.CreateUsageMessage(
                "gm.clan.set_tier", "<clan> <tier>",
                "Sets the clan's tier (0-6).",
                "gm.clan.set_tier empire_south 5");

            if (!CommandBase.ValidateArgumentCount(args, 2, usageMessage, out error))
                return error;

            var (clan, clanError) = CommandBase.FindSingleClan(args[0]);
            if (clanError != null) return clanError;

            if (!CommandValidator.ValidateIntegerRange(args[1], 0, 6, out int tier, out string tierError))
                return CommandBase.FormatErrorMessage(tierError);

            return CommandBase.ExecuteWithErrorHandling(() =>
            {
                int previousTier = clan.Tier;

                // Calculate renown needed for target tier
                float targetRenown = Campaign.Current.Models.ClanTierModel.GetRequiredRenownForTier(tier);
                clan.Renown = targetRenown;

                return CommandBase.FormatSuccessMessage($"{clan.Name}'s tier changed from {previousTier} to {clan.Tier}.");
            }, "Failed to set tier");
        }

        /// <summary>
        /// Destroy/Eliminate a clan
        /// Usage: gm.clan.destroy [clan]
        /// </summary>
        [CommandLineFunctionality.CommandLineArgumentFunction("destroy", "gm.clan")]
        public static string DestroyClan(List<string> args)
        {
            if (!CommandBase.ValidateCampaignMode(out string error))
                return error;

            var usageMessage = CommandValidator.CreateUsageMessage(
                "gm.clan.destroy", "<clan>",
                "Destroys/eliminates the specified clan.",
                "gm.clan.destroy rebel_clan_1");

            if (!CommandBase.ValidateArgumentCount(args, 1, usageMessage, out error))
                return error;

            var (clan, clanError) = CommandBase.FindSingleClan(args[0]);
            if (clanError != null) return clanError;

            if (clan.IsEliminated)
                return CommandBase.FormatErrorMessage($"{clan.Name} is already eliminated.");

            if (clan == Clan.PlayerClan)
                return CommandBase.FormatErrorMessage("Cannot destroy the player's clan.");

            return CommandBase.ExecuteWithErrorHandling(() =>
            {
                DestroyClanAction.Apply(clan);
                return CommandBase.FormatSuccessMessage($"{clan.Name} has been destroyed/eliminated.");
            }, "Failed to destroy clan");
        }

        /// <summary>
        /// Change clan leader
        /// Usage: gm.clan.set_leader [clan] [hero]
        /// </summary>
        [CommandLineFunctionality.CommandLineArgumentFunction("set_leader", "gm.clan")]
        public static string SetClanLeader(List<string> args)
        {
            if (!CommandBase.ValidateCampaignMode(out string error))
                return error;

            var usageMessage = CommandValidator.CreateUsageMessage(
                "gm.clan.set_leader", "<clan> <hero>",
                "Changes the clan leader to the specified hero.",
                "gm.clan.set_leader empire_south lord_1_1");

            if (!CommandBase.ValidateArgumentCount(args, 2, usageMessage, out error))
                return error;

            var (clan, clanError) = CommandBase.FindSingleClan(args[0]);
            if (clanError != null) return clanError;

            var (hero, heroError) = CommandBase.FindSingleHero(args[1]);
            if (heroError != null) return heroError;

            if (hero.Clan != clan)
                return CommandBase.FormatErrorMessage($"{hero.Name} is not a member of {clan.Name}.");

            return CommandBase.ExecuteWithErrorHandling(() =>
            {
                string previousLeader = clan.Leader?.Name?.ToString() ?? "None";
                clan.SetLeader(hero);
                return CommandBase.FormatSuccessMessage($"{clan.Name}'s leader changed from {previousLeader} to {hero.Name}.");
            }, "Failed to set clan leader");
        }

        #endregion
    }
}