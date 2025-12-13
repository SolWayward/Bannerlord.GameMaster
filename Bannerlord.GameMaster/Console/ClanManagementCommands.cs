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
        #region Helper Methods

        /// <summary>
        /// Helper method to find a single clan from a query
        /// </summary>
        private static (Clan clan, string error) FindSingleClan(string query)
        {
            List<Clan> matchedClans = Clans.ClanFinder.FindClans(query);

            if (matchedClans == null || matchedClans.Count == 0)
                return (null, $"Error: No clan matching query '{query}' found.\n");

            if (matchedClans.Count > 1)
            {
                return (null, $"Error: Found {matchedClans.Count} clans matching query '{query}':\n" +
                             $"{Clans.ClanFinder.GetFormattedDetails(matchedClans)}" +
                             $"Please use a more specific name or ID.\n");
            }

            return (matchedClans[0], null);
        }

        /// <summary>
        /// Helper method to find a single hero from a query
        /// </summary>
        private static (Hero hero, string error) FindSingleHero(string query)
        {
            List<Hero> matchedHeroes = Heroes.HeroFinder.GetHeroes(query);

            if (matchedHeroes == null || matchedHeroes.Count == 0)
                return (null, $"Error: No hero matching query '{query}' found.\n");

            if (matchedHeroes.Count > 1)
            {
                return (null, $"Error: Found {matchedHeroes.Count} heroes matching query '{query}':\n" +
                             $"{Heroes.HeroFinder.GetFormattedDetails(matchedHeroes)}" +
                             $"Please use a more specific name or ID.\n");
            }

            return (matchedHeroes[0], null);
        }

        /// <summary>
        /// Validates campaign mode
        /// </summary>
        private static bool ValidateCampaignMode(out string error)
        {
            if (Campaign.Current == null)
            {
                error = "Error: Must be in campaign mode.\n";
                return false;
            }
            error = null;
            return true;
        }

        #endregion

        #region Clan Management

        /// <summary>
        /// Transfer a hero to another clan
        /// Usage: gm.clan.add_hero [clan] [hero]
        /// </summary>
        [CommandLineFunctionality.CommandLineArgumentFunction("add_hero", "gm.clan")]
        public static string AddHeroToClan(List<string> args)
        {
            if (!ValidateCampaignMode(out string error))
                return error;

            if (args == null || args.Count < 2)
            {
                return "Error: Missing arguments.\n" +
                       "Usage: gm.clan.add_hero <clan> <hero>\n" +
                       "Adds a hero to the specified clan.\n" +
                       "Example: gm.clan.add_hero empire_south lord_1_1\n";
            }

            var (clan, clanError) = FindSingleClan(args[0]);
            if (clanError != null) return clanError;

            var (hero, heroError) = FindSingleHero(args[1]);
            if (heroError != null) return heroError;

            try
            {
                string previousClanName = hero.Clan?.Name?.ToString() ?? "No Clan";
                hero.Clan = clan;

                return $"Success: {hero.Name} transferred from '{previousClanName}' to '{clan.Name}'.\n";
            }
            catch (Exception ex)
            {
                return $"Error: Failed to add hero to clan.\n{ex.Message}\n";
            }
        }


        /// <summary>
        /// Add gold to all clan members
        /// Usage: gm.clan.add_gold [clan] [amount]
        /// </summary>
        [CommandLineFunctionality.CommandLineArgumentFunction("add_gold", "gm.clan")]
        public static string AddClanGold(List<string> args)
        {
            if (!ValidateCampaignMode(out string error))
                return error;

            if (args == null || args.Count < 2)
            {
                return "Error: Missing arguments.\n" +
                       "Usage: gm.clan.add_gold <clan> <amount>\n" +
                       "Adds gold to all members of the clan.\n" +
                       "Example: gm.clan.add_gold empire_south 50000\n";
            }

            var (clan, clanError) = FindSingleClan(args[0]);
            if (clanError != null) return clanError;

            if (!int.TryParse(args[1], out int amount))
                return "Error: Invalid gold amount. Must be a number.\n";

            try
            {
                int previousGold = clan.Gold;
                int membersCount = clan.Heroes.Count(h => h.IsAlive);

                if (membersCount == 0)
                    return $"Error: {clan.Name} has no living heroes to receive gold.\n";

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

                return $"Success: Added {amount} gold to {clan.Name} (distributed among {membersCount} members).\n" +
                       $"Clan gold changed from {previousGold} to {clan.Gold}.\n";
            }
            catch (Exception ex)
            {
                return $"Error: Failed to add gold.\n{ex.Message}\n";
            }
        }

        /// <summary>
        /// Set total clan gold by distributing to members
        /// Usage: gm.clan.set_gold [clan] [amount]
        /// </summary>
        [CommandLineFunctionality.CommandLineArgumentFunction("set_gold", "gm.clan")]
        public static string SetClanGold(List<string> args)
        {
            if (!ValidateCampaignMode(out string error))
                return error;

            if (args == null || args.Count < 2)
            {
                return "Error: Missing arguments.\n" +
                       "Usage: gm.clan.set_gold <clan> <amount>\n" +
                       "Sets the clan's total gold by distributing to all members.\n" +
                       "Example: gm.clan.set_gold empire_south 100000\n";
            }

            var (clan, clanError) = FindSingleClan(args[0]);
            if (clanError != null) return clanError;

            if (!int.TryParse(args[1], out int targetAmount) || targetAmount < 0)
                return "Error: Invalid gold amount. Must be a non-negative number.\n";

            try
            {
                int previousGold = clan.Gold;
                int difference = targetAmount - previousGold;
                int membersCount = clan.Heroes.Count(h => h.IsAlive);

                if (membersCount == 0)
                    return $"Error: {clan.Name} has no living heroes to receive gold.\n";

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

                return $"Success: Set {clan.Name}'s gold to {targetAmount} (distributed among {membersCount} members).\n" +
                       $"Previous clan gold: {previousGold}, New clan gold: {clan.Gold}.\n";
            }
            catch (Exception ex)
            {
                return $"Error: Failed to set gold.\n{ex.Message}\n";
            }
        }

        /// <summary>
        /// Add gold to clan leader only
        /// Usage: gm.clan.add_gold_leader [clan] [amount]
        /// </summary>
        [CommandLineFunctionality.CommandLineArgumentFunction("add_gold_leader", "gm.clan")]
        public static string AddGoldToLeader(List<string> args)
        {
            if (!ValidateCampaignMode(out string error))
                return error;

            if (args == null || args.Count < 2)
            {
                return "Error: Missing arguments.\n" +
                       "Usage: gm.clan.add_gold_leader <clan> <amount>\n" +
                       "Adds gold to the clan leader only.\n" +
                       "Example: gm.clan.add_gold_leader empire_south 50000\n";
            }

            var (clan, clanError) = FindSingleClan(args[0]);
            if (clanError != null) return clanError;

            if (clan.Leader == null)
                return $"Error: {clan.Name} has no leader.\n";

            if (!int.TryParse(args[1], out int amount))
                return "Error: Invalid gold amount. Must be a number.\n";

            try
            {
                int previousClanGold = clan.Gold;
                int previousLeaderGold = clan.Leader.Gold;

                clan.Leader.ChangeHeroGold(amount);

                return $"Success: Added {amount} gold to {clan.Leader.Name} (leader of {clan.Name}).\n" +
                       $"Leader gold: {previousLeaderGold} → {clan.Leader.Gold}\n" +
                       $"Clan total gold: {previousClanGold} → {clan.Gold}\n";
            }
            catch (Exception ex)
            {
                return $"Error: Failed to add gold to leader.\n{ex.Message}\n";
            }
        }

        /// <summary>
        /// Distribute gold to specific clan member
        /// Usage: gm.clan.give_gold [clan] [hero] [amount]
        /// </summary>
        [CommandLineFunctionality.CommandLineArgumentFunction("give_gold", "gm.clan")]
        public static string GiveGoldToMember(List<string> args)
        {
            if (!ValidateCampaignMode(out string error))
                return error;

            if (args == null || args.Count < 3)
            {
                return "Error: Missing arguments.\n" +
                       "Usage: gm.clan.give_gold <clan> <hero> <amount>\n" +
                       "Gives gold to a specific clan member.\n" +
                       "Example: gm.clan.give_gold empire_south lord_1_1 10000\n";
            }

            var (clan, clanError) = FindSingleClan(args[0]);
            if (clanError != null) return clanError;

            var (hero, heroError) = FindSingleHero(args[1]);
            if (heroError != null) return heroError;

            if (hero.Clan != clan)
                return $"Error: {hero.Name} is not a member of {clan.Name}.\n";

            if (!int.TryParse(args[2], out int amount))
                return "Error: Invalid gold amount. Must be a number.\n";

            try
            {
                int previousClanGold = clan.Gold;
                int previousHeroGold = hero.Gold;

                hero.ChangeHeroGold(amount);

                return $"Success: Added {amount} gold to {hero.Name} (member of {clan.Name}).\n" +
                       $"Hero gold: {previousHeroGold} → {hero.Gold}\n" +
                       $"Clan total gold: {previousClanGold} → {clan.Gold}\n";
            }
            catch (Exception ex)
            {
                return $"Error: Failed to give gold to member.\n{ex.Message}\n";
            }
        }

        /// <summary>
        /// Set clan renown
        /// Usage: gm.clan.set_renown [clan] [amount]
        /// </summary>
        [CommandLineFunctionality.CommandLineArgumentFunction("set_renown", "gm.clan")]
        public static string SetClanRenown(List<string> args)
        {
            if (!ValidateCampaignMode(out string error))
                return error;

            if (args == null || args.Count < 2)
            {
                return "Error: Missing arguments.\n" +
                       "Usage: gm.clan.set_renown <clan> <amount>\n" +
                       "Sets the clan's renown.\n" +
                       "Example: gm.clan.set_renown empire_south 500\n";
            }

            var (clan, clanError) = FindSingleClan(args[0]);
            if (clanError != null) return clanError;

            if (!float.TryParse(args[1], out float amount) || amount < 0)
                return "Error: Invalid renown amount. Must be a positive number.\n";

            try
            {
                float previousRenown = clan.Renown;
                clan.Renown = amount;
                return $"Success: {clan.Name}'s renown changed from {previousRenown:F0} to {clan.Renown:F0}.\n";
            }
            catch (Exception ex)
            {
                return $"Error: Failed to set renown.\n{ex.Message}\n";
            }
        }

        /// <summary>
        /// Add renown to clan
        /// Usage: gm.clan.add_renown [clan] [amount]
        /// </summary>
        [CommandLineFunctionality.CommandLineArgumentFunction("add_renown", "gm.clan")]
        public static string AddClanRenown(List<string> args)
        {
            if (!ValidateCampaignMode(out string error))
                return error;

            if (args == null || args.Count < 2)
            {
                return "Error: Missing arguments.\n" +
                       "Usage: gm.clan.add_renown <clan> <amount>\n" +
                       "Adds renown to the clan.\n" +
                       "Example: gm.clan.add_renown empire_south 100\n";
            }

            var (clan, clanError) = FindSingleClan(args[0]);
            if (clanError != null) return clanError;

            if (!float.TryParse(args[1], out float amount))
                return "Error: Invalid renown amount. Must be a number.\n";

            try
            {
                float previousRenown = clan.Renown;
                clan.AddRenown(amount, true);
                return $"Success: {clan.Name}'s renown changed from {previousRenown:F0} to {clan.Renown:F0} ({(amount >= 0 ? "+" : "")}{amount:F0}).\n";
            }
            catch (Exception ex)
            {
                return $"Error: Failed to add renown.\n{ex.Message}\n";
            }
        }

        /// <summary>
        /// Change clan tier
        /// Usage: gm.clan.set_tier [clan] [tier]
        /// </summary>
        [CommandLineFunctionality.CommandLineArgumentFunction("set_tier", "gm.clan")]
        public static string SetClanTier(List<string> args)
        {
            if (!ValidateCampaignMode(out string error))
                return error;

            if (args == null || args.Count < 2)
            {
                return "Error: Missing arguments.\n" +
                       "Usage: gm.clan.set_tier <clan> <tier>\n" +
                       "Sets the clan's tier (0-6).\n" +
                       "Example: gm.clan.set_tier empire_south 5\n";
            }

            var (clan, clanError) = FindSingleClan(args[0]);
            if (clanError != null) return clanError;

            if (!int.TryParse(args[1], out int tier) || tier < 0 || tier > 6)
                return "Error: Invalid tier. Must be between 0 and 6.\n";

            try
            {
                int previousTier = clan.Tier;

                // Calculate renown needed for target tier
                float targetRenown = Campaign.Current.Models.ClanTierModel.GetRequiredRenownForTier(tier);
                clan.Renown = targetRenown;

                return $"Success: {clan.Name}'s tier changed from {previousTier} to {clan.Tier}.\n";
            }
            catch (Exception ex)
            {
                return $"Error: Failed to set tier.\n{ex.Message}\n";
            }
        }

        /// <summary>
        /// Destroy/Eliminate a clan
        /// Usage: gm.clan.destroy [clan]
        /// </summary>
        [CommandLineFunctionality.CommandLineArgumentFunction("destroy", "gm.clan")]
        public static string DestroyClan(List<string> args)
        {
            if (!ValidateCampaignMode(out string error))
                return error;

            if (args == null || args.Count < 1)
            {
                return "Error: Missing argument.\n" +
                       "Usage: gm.clan.destroy <clan>\n" +
                       "Destroys/eliminates the specified clan.\n" +
                       "Example: gm.clan.destroy rebel_clan_1\n";
            }

            var (clan, clanError) = FindSingleClan(args[0]);
            if (clanError != null) return clanError;

            if (clan.IsEliminated)
                return $"Error: {clan.Name} is already eliminated.\n";

            if (clan == Clan.PlayerClan)
                return "Error: Cannot destroy the player's clan.\n";

            try
            {
                DestroyClanAction.Apply(clan);
                return $"Success: {clan.Name} has been destroyed/eliminated.\n";
            }
            catch (Exception ex)
            {
                return $"Error: Failed to destroy clan.\n{ex.Message}\n";
            }
        }

        /// <summary>
        /// Change clan leader
        /// Usage: gm.clan.set_leader [clan] [hero]
        /// </summary>
        [CommandLineFunctionality.CommandLineArgumentFunction("set_leader", "gm.clan")]
        public static string SetClanLeader(List<string> args)
        {
            if (!ValidateCampaignMode(out string error))
                return error;

            if (args == null || args.Count < 2)
            {
                return "Error: Missing arguments.\n" +
                       "Usage: gm.clan.set_leader <clan> <hero>\n" +
                       "Changes the clan leader to the specified hero.\n" +
                       "Example: gm.clan.set_leader empire_south lord_1_1\n";
            }

            var (clan, clanError) = FindSingleClan(args[0]);
            if (clanError != null) return clanError;

            var (hero, heroError) = FindSingleHero(args[1]);
            if (heroError != null) return heroError;

            if (hero.Clan != clan)
                return $"Error: {hero.Name} is not a member of {clan.Name}.\n";

            try
            {
                string previousLeader = clan.Leader?.Name?.ToString() ?? "None";
                clan.SetLeader(hero);
                return $"Success: {clan.Name}'s leader changed from {previousLeader} to {hero.Name}.\n";
            }
            catch (Exception ex)
            {
                return $"Error: Failed to set clan leader.\n{ex.Message}\n";
            }
        }

        #endregion
    }
}