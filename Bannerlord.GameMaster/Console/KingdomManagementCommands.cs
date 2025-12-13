using System;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;
using TaleWorlds.Library;

namespace Bannerlord.GameMaster.Console
{
    [CommandLineFunctionality.CommandLineArgumentFunction("kingdom", "gm")]
    public static class KingdomManagementCommands
    {
        #region Helper Methods

        /// <summary>
        /// Helper method to find a single kingdom from a query
        /// </summary>
        private static (Kingdom kingdom, string error) FindSingleKingdom(string query)
        {
            List<Kingdom> matchedKingdoms = Kingdoms.KingdomFinder.FindKingdoms(query);

            if (matchedKingdoms == null || matchedKingdoms.Count == 0)
                return (null, $"Error: No kingdom matching query '{query}' found.\n");

            if (matchedKingdoms.Count > 1)
            {
                return (null, $"Error: Found {matchedKingdoms.Count} kingdoms matching query '{query}':\n" +
                             $"{Kingdoms.KingdomFinder.GetFormattedDetails(matchedKingdoms)}" +
                             $"Please use a more specific name or ID.\n");
            }

            return (matchedKingdoms[0], null);
        }

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
            List<Hero> matchedHeroes = Heroes.HeroFinder.FindHeroes(query);

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
        /// Add a clan to a kingdom
        /// Usage: gm.kingdom.add_clan [kingdom] [clan]
        /// </summary>
        [CommandLineFunctionality.CommandLineArgumentFunction("add_clan", "gm.kingdom")]
        public static string AddClanToKingdom(List<string> args)
        {
            if (!ValidateCampaignMode(out string error))
                return error;

            if (args == null || args.Count < 2)
            {
                return "Error: Missing arguments.\n" +
                       "Usage: gm.kingdom.add_clan <kingdom> <clan>\n" +
                       "Adds a clan to the specified kingdom.\n" +
                       "Example: gm.kingdom.add_clan empire clan_battania_1\n";
            }

            var (kingdom, kingdomError) = FindSingleKingdom(args[0]);
            if (kingdomError != null) return kingdomError;

            var (clan, clanError) = FindSingleClan(args[1]);
            if (clanError != null) return clanError;

            if (clan.Kingdom == kingdom)
                return $"Error: {clan.Name} is already part of {kingdom.Name}.\n";

            try
            {
                string previousKingdom = clan.Kingdom?.Name?.ToString() ?? "No Kingdom";

                // Use the correct overload - award gold amount instead of bool
                ChangeKingdomAction.ApplyByJoinToKingdom(clan, kingdom, showNotification: true);

                return $"Success: {clan.Name} joined {kingdom.Name} from {previousKingdom}.\n";
            }
            catch (Exception ex)
            {
                return $"Error: Failed to add clan to kingdom.\n{ex.Message}\n";
            }
        }

        /// <summary>
        /// Remove a clan from its kingdom
        /// Usage: gm.kingdom.remove_clan [clan]
        /// </summary>
        [CommandLineFunctionality.CommandLineArgumentFunction("remove_clan", "gm.kingdom")]
        public static string RemoveClanFromKingdom(List<string> args)
        {
            if (!ValidateCampaignMode(out string error))
                return error;

            if (args == null || args.Count < 1)
            {
                return "Error: Missing argument.\n" +
                       "Usage: gm.kingdom.remove_clan <clan>\n" +
                       "Removes a clan from its current kingdom.\n" +
                       "Example: gm.kingdom.remove_clan clan_empire_south_1\n";
            }

            var (clan, clanError) = FindSingleClan(args[0]);
            if (clanError != null) return clanError;

            if (clan.Kingdom == null)
                return $"Error: {clan.Name} is not part of any kingdom.\n";

            if (clan == clan.Kingdom.RulingClan)
                return $"Error: Cannot remove the ruling clan ({clan.Name}) from {clan.Kingdom.Name}.\n";

            try
            {
                string previousKingdom = clan.Kingdom.Name.ToString();
                clan.Kingdom = null;

                return $"Success: {clan.Name} removed from {previousKingdom}.\n";
            }
            catch (Exception ex)
            {
                return $"Error: Failed to remove clan from kingdom.\n{ex.Message}\n";
            }
        }

        #endregion

        #region Diplomacy

        /// <summary>
        /// Declare war between two kingdoms
        /// Usage: gm.kingdom.declare_war [kingdom1] [kingdom2]
        /// </summary>
        [CommandLineFunctionality.CommandLineArgumentFunction("declare_war", "gm.kingdom")]
        public static string DeclareWar(List<string> args)
        {
            if (!ValidateCampaignMode(out string error))
                return error;

            if (args == null || args.Count < 2)
            {
                return "Error: Missing arguments.\n" +
                       "Usage: gm.kingdom.declare_war <kingdom1> <kingdom2>\n" +
                       "Declares war between two kingdoms.\n" +
                       "Example: gm.kingdom.declare_war empire battania\n";
            }

            var (kingdom1, kingdom1Error) = FindSingleKingdom(args[0]);
            if (kingdom1Error != null) return kingdom1Error;

            var (kingdom2, kingdom2Error) = FindSingleKingdom(args[1]);
            if (kingdom2Error != null) return kingdom2Error;

            if (kingdom1 == kingdom2)
                return "Error: A kingdom cannot declare war on itself.\n";

            if (FactionManager.IsAtWarAgainstFaction(kingdom1, kingdom2))
                return $"Error: {kingdom1.Name} and {kingdom2.Name} are already at war.\n";

            try
            {
                DeclareWarAction.ApplyByDefault(kingdom1, kingdom2);
                return $"Success: War declared between {kingdom1.Name} and {kingdom2.Name}.\n";
            }
            catch (Exception ex)
            {
                return $"Error: Failed to declare war.\n{ex.Message}\n";
            }
        }

        /// <summary>
        /// Make peace between two kingdoms
        /// Usage: gm.kingdom.make_peace [kingdom1] [kingdom2]
        /// </summary>
        [CommandLineFunctionality.CommandLineArgumentFunction("make_peace", "gm.kingdom")]
        public static string MakePeace(List<string> args)
        {
            if (!ValidateCampaignMode(out string error))
                return error;

            if (args == null || args.Count < 2)
            {
                return "Error: Missing arguments.\n" +
                       "Usage: gm.kingdom.make_peace <kingdom1> <kingdom2>\n" +
                       "Makes peace between two kingdoms.\n" +
                       "Example: gm.kingdom.make_peace empire battania\n";
            }

            var (kingdom1, kingdom1Error) = FindSingleKingdom(args[0]);
            if (kingdom1Error != null) return kingdom1Error;

            var (kingdom2, kingdom2Error) = FindSingleKingdom(args[1]);
            if (kingdom2Error != null) return kingdom2Error;

            if (kingdom1 == kingdom2)
                return "Error: A kingdom cannot make peace with itself.\n";

            if (!FactionManager.IsAtWarAgainstFaction(kingdom1, kingdom2))
                return $"Error: {kingdom1.Name} and {kingdom2.Name} are not at war.\n";

            try
            {
                MakePeaceAction.Apply(kingdom1, kingdom2);
                return $"Success: Peace established between {kingdom1.Name} and {kingdom2.Name}.\n";
            }
            catch (Exception ex)
            {
                return $"Error: Failed to make peace.\n{ex.Message}\n";
            }
        }

        #endregion

        #region Settlement Management

        /// <summary>
        /// Transfer a settlement to another kingdom
        /// Usage: gm.kingdom.give_fief [settlement] [kingdom]
        /// </summary>
        [CommandLineFunctionality.CommandLineArgumentFunction("give_fief", "gm.kingdom")]
        public static string GiveFief(List<string> args)
        {
            if (!ValidateCampaignMode(out string error))
                return error;

            if (args == null || args.Count < 2)
            {
                return "Error: Missing arguments.\n" +
                       "Usage: gm.kingdom.give_fief <settlement> <kingdom>\n" +
                       "Transfers a settlement to another kingdom.\n" +
                       "Example: gm.kingdom.give_fief town_empire_1 battania\n";
            }

            Settlement settlement = Settlement.Find(args[0]);
            if (settlement == null)
                return $"Error: Settlement '{args[0]}' not found.\n";

            if (!settlement.IsTown && !settlement.IsCastle)
                return $"Error: {settlement.Name} is not a town or castle.\n";

            var (kingdom, kingdomError) = FindSingleKingdom(args[1]);
            if (kingdomError != null) return kingdomError;

            if (settlement.MapFaction == kingdom)
                return $"Error: {settlement.Name} already belongs to {kingdom.Name}.\n";

            try
            {
                string previousOwner = settlement.OwnerClan?.Name?.ToString() ?? "None";
                string previousKingdom = (settlement.MapFaction as Kingdom)?.Name?.ToString() ?? "None";

                // Get a random eligible clan from the kingdom
                var eligibleClans = kingdom.Clans.Where(c => !c.IsEliminated && c.Leader != null && c.Leader.IsAlive).ToList();
                if (eligibleClans.Count == 0)
                    return $"Error: No valid clan found in {kingdom.Name} to receive the settlement.\n";

                var targetClan = eligibleClans.GetRandomElementInefficiently();

                ChangeOwnerOfSettlementAction.ApplyByGift(settlement, targetClan.Leader);

                return $"Success: {settlement.Name} transferred from {previousKingdom} ({previousOwner}) to {kingdom.Name} ({targetClan.Name}).\n";
            }
            catch (Exception ex)
            {
                return $"Error: Failed to transfer settlement.\n{ex.Message}\n";
            }
        }

        /// <summary>
        /// Transfer a settlement to a specific clan
        /// Usage: gm.kingdom.give_fief_to_clan [settlement] [clan]
        /// </summary>
        [CommandLineFunctionality.CommandLineArgumentFunction("give_fief_to_clan", "gm.kingdom")]
        public static string GiveFiefToClan(List<string> args)
        {
            if (!ValidateCampaignMode(out string error))
                return error;

            if (args == null || args.Count < 2)
            {
                return "Error: Missing arguments.\n" +
                       "Usage: gm.kingdom.give_fief_to_clan <settlement> <clan>\n" +
                       "Transfers a settlement to a specific clan.\n" +
                       "Example: gm.kingdom.give_fief_to_clan town_empire_1 clan_battania_1\n";
            }

            Settlement settlement = Settlement.Find(args[0]);
            if (settlement == null)
                return $"Error: Settlement '{args[0]}' not found.\n";

            if (!settlement.IsTown && !settlement.IsCastle)
                return $"Error: {settlement.Name} is not a town or castle.\n";

            var (clan, clanError) = FindSingleClan(args[1]);
            if (clanError != null) return clanError;

            if (clan.Leader == null || !clan.Leader.IsAlive)
                return $"Error: {clan.Name} has no living leader to receive the settlement.\n";

            try
            {
                string previousOwner = settlement.OwnerClan?.Name?.ToString() ?? "None";
                string previousKingdom = (settlement.MapFaction as Kingdom)?.Name?.ToString() ?? "None";

                ChangeOwnerOfSettlementAction.ApplyByGift(settlement, clan.Leader);

                return $"Success: {settlement.Name} transferred from {previousKingdom} ({previousOwner}) to {clan.Name}.\n";
            }
            catch (Exception ex)
            {
                return $"Error: Failed to transfer settlement.\n{ex.Message}\n";
            }
        }

        #endregion

        #region Kingdom Properties

        /// <summary>
        /// Change kingdom ruler
        /// Usage: gm.kingdom.set_ruler [kingdom] [hero]
        /// </summary>
        [CommandLineFunctionality.CommandLineArgumentFunction("set_ruler", "gm.kingdom")]
        public static string SetKingdomRuler(List<string> args)
        {
            if (!ValidateCampaignMode(out string error))
                return error;

            if (args == null || args.Count < 2)
            {
                return "Error: Missing arguments.\n" +
                       "Usage: gm.kingdom.set_ruler <kingdom> <hero>\n" +
                       "Changes the kingdom ruler.\n" +
                       "Example: gm.kingdom.set_ruler empire lord_1_1\n";
            }

            var (kingdom, kingdomError) = FindSingleKingdom(args[0]);
            if (kingdomError != null) return kingdomError;

            var (hero, heroError) = FindSingleHero(args[1]);
            if (heroError != null) return heroError;

            if (hero.MapFaction != kingdom)
                return $"Error: {hero.Name} is not part of {kingdom.Name}.\n";

            if (hero.Clan == null)
                return $"Error: {hero.Name} has no clan.\n";

            try
            {
                string previousRuler = kingdom.Leader?.Name?.ToString() ?? "None";

                // Set hero's clan as ruling clan
                kingdom.RulingClan = hero.Clan;

                // Set hero as clan leader if not already
                if (hero.Clan.Leader != hero)
                    hero.Clan.SetLeader(hero);

                return $"Success: {kingdom.Name}'s ruler changed from {previousRuler} to {hero.Name}.\n" +
                       $"Ruling clan is now {hero.Clan.Name}.\n";
            }
            catch (Exception ex)
            {
                return $"Error: Failed to set kingdom ruler.\n{ex.Message}\n";
            }
        }

        /// <summary>
        /// Destroy/Eliminate a kingdom
        /// Usage: gm.kingdom.destroy [kingdom]
        /// </summary>
        [CommandLineFunctionality.CommandLineArgumentFunction("destroy", "gm.kingdom")]
        public static string DestroyKingdom(List<string> args)
        {
            if (!ValidateCampaignMode(out string error))
                return error;

            if (args == null || args.Count < 1)
            {
                return "Error: Missing argument.\n" +
                       "Usage: gm.kingdom.destroy <kingdom>\n" +
                       "Destroys/eliminates the specified kingdom.\n" +
                       "Example: gm.kingdom.destroy battania\n";
            }

            var (kingdom, kingdomError) = FindSingleKingdom(args[0]);
            if (kingdomError != null) return kingdomError;

            if (kingdom.IsEliminated)
                return $"Error: {kingdom.Name} is already eliminated.\n";

            if (kingdom == Hero.MainHero.MapFaction)
                return "Error: Cannot destroy the player's kingdom.\n";

            try
            {
                DestroyKingdomAction.Apply(kingdom);
                return $"Success: {kingdom.Name} has been destroyed/eliminated.\n";
            }
            catch (Exception ex)
            {
                return $"Error: Failed to destroy kingdom.\n{ex.Message}\n";
            }
        }

        #endregion
    }
}