using Bannerlord.GameMaster.Clans;
using Bannerlord.GameMaster.Heroes;
using System;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;
using TaleWorlds.Library;

namespace Bannerlord.GameMaster.Console
{
    [CommandLineFunctionality.CommandLineArgumentFunction("hero", "gm")]
    public static class HeroManagementCommands
    {
        #region Helper Methods

        /// <summary>
        /// Helper method to find a single hero from a query
        /// </summary>
        private static (Hero hero, string error) FindSingleHero(string query)
        {
            List<Hero> matchedHeroes = HeroFinder.GetHeroes(query);

            if (matchedHeroes == null || matchedHeroes.IsEmpty())
                return (null, $"Error: No hero matching query '{query}' found.\n");

            if (matchedHeroes.Count > 1)
            {
                return (null, $"Error: Found {matchedHeroes.Count} heroes matching query '{query}':\n" +
                             $"{HeroFinder.GetFormattedDetails(matchedHeroes)}" +
                             $"Please use a more specific name or ID.\n");
            }

            return (matchedHeroes[0], null);
        }

        /// <summary>
        /// Helper method to find a single clan from a query
        /// </summary>
        private static (Clan clan, string error) FindSingleClan(string query)
        {
            List<Clan> matchedClans = ClanFinder.GetAllClans(query);

            if (matchedClans == null || matchedClans.IsEmpty())
                return (null, $"Error: No clan matching query '{query}' found.\n");

            if (matchedClans.Count > 1)
            {
                return (null, $"Error: Found {matchedClans.Count} clans matching query '{query}':\n" +
                             $"{ClanFinder.GetFormattedDetails(matchedClans)}" +
                             $"Please use a more specific name or ID.\n");
            }

            return (matchedClans[0], null);
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
        /// Usage: gm.hero.set_clan [hero] [clan]
        /// </summary>
        [CommandLineFunctionality.CommandLineArgumentFunction("set_clan", "gm.hero")]
        public static string SetClan(List<string> args)
        {
            if (!ValidateCampaignMode(out string error))
                return error;

            if (args == null || args.Count < 2)
            {
                return "Error: Missing arguments.\n" +
                       "Usage: gm.hero.set_clan <hero> <clan>\n" +
                       "Transfers a hero to another clan.\n" +
                       "Example: gm.hero.set_clan lord_1_1 clan_empire_south_1\n" +
                       "You can use partial names or IDs for both hero and clan.\n";
            }

            string heroQuery = args[0];
            string clanQuery = args[1];

            // Find the hero
            var (hero, heroError) = FindSingleHero(heroQuery);
            if (heroError != null) return heroError;

            // Find the target clan
            var (clan, clanError) = FindSingleClan(clanQuery);
            if (clanError != null) return clanError;

            try
            {
                string previousClanName = hero.Clan?.Name?.ToString() ?? "No Clan";
                hero.Clan = clan;

                return $"Success: {hero.Name} (ID: {hero.StringId}) transferred from '{previousClanName}' to '{clan.Name}'.\n" +
                       $"Updated details: {hero.FormattedDetails()}\n";
            }
            catch (Exception ex)
            {
                return $"Error: Failed to transfer hero.\n{ex.Message}\n";
            }
        }

        /// <summary>
        /// Remove a hero from their clan
        /// Usage: gm.hero.remove_clan [hero]
        /// </summary>
        [CommandLineFunctionality.CommandLineArgumentFunction("remove_clan", "gm.hero")]
        public static string RemoveClan(List<string> args)
        {
            if (!ValidateCampaignMode(out string error))
                return error;

            if (args == null || args.Count < 1)
            {
                return "Error: Missing argument.\n" +
                       "Usage: gm.hero.remove_clan <hero>\n" +
                       "Removes a hero from their current clan.\n" +
                       "Example: gm.hero.remove_clan lord_1_1\n";
            }

            var (hero, heroError) = FindSingleHero(args[0]);
            if (heroError != null) return heroError;

            if (hero.Clan == null)
                return $"Error: {hero.Name} is not a member of any clan.\n";

            try
            {
                string previousClanName = hero.Clan.Name.ToString();
                hero.Clan = null;

                return $"Success: {hero.Name} (ID: {hero.StringId}) removed from clan '{previousClanName}'.\n";
            }
            catch (Exception ex)
            {
                return $"Error: Failed to remove hero from clan.\n{ex.Message}\n";
            }
        }

        #endregion

        #region Hero State Management

        /// <summary>
        /// Kill a hero
        /// Usage: gm.hero.kill [hero] [optional: show_death_log]
        /// </summary>
        [CommandLineFunctionality.CommandLineArgumentFunction("kill", "gm.hero")]
        public static string KillHero(List<string> args)
        {
            if (!ValidateCampaignMode(out string error))
                return error;

            if (args == null || args.Count < 1)
            {
                return "Error: Missing argument.\n" +
                       "Usage: gm.hero.kill <hero> [show_death_log]\n" +
                       "Kills the specified hero.\n" +
                       "Example: gm.hero.kill lord_1_1\n" +
                       "Example: gm.hero.kill lord_1_1 true (shows death log)\n";
            }

            var (hero, heroError) = FindSingleHero(args[0]);
            if (heroError != null) return heroError;

            if (!hero.IsAlive)
                return $"Error: {hero.Name} is already dead.\n";

            bool showDeathLog = args.Count > 1 && bool.TryParse(args[1], out bool show) && show;

            try
            {
                KillCharacterAction.ApplyByMurder(hero, null, showDeathLog);
                return $"Success: {hero.Name} (ID: {hero.StringId}) has been killed.\n";
            }
            catch (Exception ex)
            {
                return $"Error: Failed to kill hero.\n{ex.Message}\n";
            }
        }

        /// <summary>
        /// Imprison a hero
        /// Usage: gm.hero.imprison [prisoner] [captor]
        /// </summary>
        [CommandLineFunctionality.CommandLineArgumentFunction("imprison", "gm.hero")]
        public static string ImprisonHero(List<string> args)
        {
            if (!ValidateCampaignMode(out string error))
                return error;

            if (args == null || args.Count < 2)
            {
                return "Error: Missing arguments.\n" +
                       "Usage: gm.hero.imprison <prisoner> <captor>\n" +
                       "Imprisons a hero by another hero/party.\n" +
                       "Example: gm.hero.imprison lord_1_1 lord_2_1\n";
            }

            var (prisoner, prisonerError) = FindSingleHero(args[0]);
            if (prisonerError != null) return prisonerError;

            var (captor, captorError) = FindSingleHero(args[1]);
            if (captorError != null) return captorError;

            if (prisoner.IsPrisoner)
                return $"Error: {prisoner.Name} is already a prisoner.\n";

            try
            {
                // Get the captor's party base
                PartyBase captorParty = captor.PartyBelongedTo?.Party
                                        ?? captor.Clan?.Kingdom?.Leader?.PartyBelongedTo?.Party
                                        ?? Settlement.FindFirst(s => s.OwnerClan == captor.Clan)?.Party;

                if (captorParty == null)
                    return $"Error: {captor.Name} has no valid party or settlement to hold prisoners.\n";

                TakePrisonerAction.Apply(captorParty, prisoner);
                return $"Success: {prisoner.Name} (ID: {prisoner.StringId}) is now imprisoned by {captor.Name}.\n";
            }
            catch (Exception ex)
            {
                return $"Error: Failed to imprison hero.\n{ex.Message}\n";
            }
        }

        /// <summary>
        /// Release a hero from prison
        /// Usage: gm.hero.release [hero]
        /// </summary>
        [CommandLineFunctionality.CommandLineArgumentFunction("release", "gm.hero")]
        public static string ReleaseHero(List<string> args)
        {
            if (!ValidateCampaignMode(out string error))
                return error;

            if (args == null || args.Count < 1)
            {
                return "Error: Missing argument.\n" +
                       "Usage: gm.hero.release <hero>\n" +
                       "Releases a hero from prison.\n" +
                       "Example: gm.hero.release lord_1_1\n";
            }

            var (hero, heroError) = FindSingleHero(args[0]);
            if (heroError != null) return heroError;

            if (!hero.IsPrisoner)
                return $"Error: {hero.Name} is not a prisoner.\n";

            try
            {
                EndCaptivityAction.ApplyByReleasedAfterBattle(hero);
                return $"Success: {hero.Name} (ID: {hero.StringId}) has been released from captivity.\n";
            }
            catch (Exception ex)
            {
                return $"Error: Failed to release hero.\n{ex.Message}\n";
            }
        }

        #endregion

        #region Hero Attributes

        /// <summary>
        /// Change hero's age
        /// Usage: gm.hero.set_age [hero] [age]
        /// </summary>
        [CommandLineFunctionality.CommandLineArgumentFunction("set_age", "gm.hero")]
        public static string SetAge(List<string> args)
        {
            if (!ValidateCampaignMode(out string error))
                return error;

            if (args == null || args.Count < 2)
            {
                return "Error: Missing arguments.\n" +
                       "Usage: gm.hero.set_age <hero> <age>\n" +
                       "Sets the hero's age.\n" +
                       "Example: gm.hero.set_age lord_1_1 30\n";
            }

            var (hero, heroError) = FindSingleHero(args[0]);
            if (heroError != null) return heroError;

            if (!float.TryParse(args[1], out float age) || age < 0 || age > 128)
                return "Error: Invalid age. Must be between 0 and 128.\n";

            try
            {
                float previousAge = hero.Age;
                hero.SetBirthDay(CampaignTime.YearsFromNow(-age));
                return $"Success: {hero.Name}'s age changed from {previousAge:F0} to {hero.Age:F0}.\n";
            }
            catch (Exception ex)
            {
                return $"Error: Failed to set age.\n{ex.Message}\n";
            }
        }

        /// <summary>
        /// Change hero's gold
        /// Usage: gm.hero.set_gold [hero] [amount]
        /// </summary>
        [CommandLineFunctionality.CommandLineArgumentFunction("set_gold", "gm.hero")]
        public static string SetGold(List<string> args)
        {
            if (!ValidateCampaignMode(out string error))
                return error;

            if (args == null || args.Count < 2)
            {
                return "Error: Missing arguments.\n" +
                       "Usage: gm.hero.set_gold <hero> <amount>\n" +
                       "Sets the hero's gold amount.\n" +
                       "Example: gm.hero.set_gold lord_1_1 10000\n";
            }

            var (hero, heroError) = FindSingleHero(args[0]);
            if (heroError != null) return heroError;

            if (!int.TryParse(args[1], out int amount))
                return "Error: Invalid gold amount. Must be a number.\n";

            try
            {
                int previousGold = hero.Gold;
                hero.ChangeHeroGold(amount - previousGold);
                return $"Success: {hero.Name}'s gold changed from {previousGold} to {hero.Gold}.\n";
            }
            catch (Exception ex)
            {
                return $"Error: Failed to set gold.\n{ex.Message}\n";
            }
        }

        /// <summary>
        /// Add gold to hero
        /// Usage: gm.hero.add_gold [hero] [amount]
        /// </summary>
        [CommandLineFunctionality.CommandLineArgumentFunction("add_gold", "gm.hero")]
        public static string AddGold(List<string> args)
        {
            if (!ValidateCampaignMode(out string error))
                return error;

            if (args == null || args.Count < 2)
            {
                return "Error: Missing arguments.\n" +
                       "Usage: gm.hero.add_gold <hero> <amount>\n" +
                       "Adds gold to the hero (use negative to subtract).\n" +
                       "Example: gm.hero.add_gold lord_1_1 5000\n";
            }

            var (hero, heroError) = FindSingleHero(args[0]);
            if (heroError != null) return heroError;

            if (!int.TryParse(args[1], out int amount))
                return "Error: Invalid gold amount. Must be a number.\n";

            try
            {
                int previousGold = hero.Gold;
                hero.ChangeHeroGold(amount);
                return $"Success: {hero.Name}'s gold changed from {previousGold} to {hero.Gold} ({(amount >= 0 ? "+" : "")}{amount}).\n";
            }
            catch (Exception ex)
            {
                return $"Error: Failed to add gold.\n{ex.Message}\n";
            }
        }

        /// <summary>
        /// Heal a hero to full health
        /// Usage: gm.hero.heal [hero]
        /// </summary>
        [CommandLineFunctionality.CommandLineArgumentFunction("heal", "gm.hero")]
        public static string HealHero(List<string> args)
        {
            if (!ValidateCampaignMode(out string error))
                return error;

            if (args == null || args.Count < 1)
            {
                return "Error: Missing argument.\n" +
                       "Usage: gm.hero.heal <hero>\n" +
                       "Heals a hero to full health.\n" +
                       "Example: gm.hero.heal lord_1_1\n";
            }

            var (hero, heroError) = FindSingleHero(args[0]);
            if (heroError != null) return heroError;

            if (!hero.IsAlive)
                return $"Error: Cannot heal {hero.Name} - hero is dead.\n";

            try
            {
                hero.HitPoints = hero.CharacterObject.MaxHitPoints();
                return $"Success: {hero.Name} has been healed to full health ({hero.HitPoints}/{hero.CharacterObject.MaxHitPoints()}).\n";
            }
            catch (Exception ex)
            {
                return $"Error: Failed to heal hero.\n{ex.Message}\n";
            }
        }

        #endregion

        #region Relationships

        /// <summary>
        /// Set relation between two heroes
        /// Usage: gm.hero.set_relation [hero1] [hero2] [value]
        /// </summary>
        [CommandLineFunctionality.CommandLineArgumentFunction("set_relation", "gm.hero")]
        public static string SetRelation(List<string> args)
        {
            if (!ValidateCampaignMode(out string error))
                return error;

            if (args == null || args.Count < 3)
            {
                return "Error: Missing arguments.\n" +
                       "Usage: gm.hero.set_relation <hero1> <hero2> <value>\n" +
                       "Sets the relationship value between two heroes (-100 to 100).\n" +
                       "Example: gm.hero.set_relation lord_1_1 lord_2_1 50\n";
            }

            var (hero1, hero1Error) = FindSingleHero(args[0]);
            if (hero1Error != null) return hero1Error;

            var (hero2, hero2Error) = FindSingleHero(args[1]);
            if (hero2Error != null) return hero2Error;

            if (!int.TryParse(args[2], out int value) || value < -100 || value > 100)
                return "Error: Invalid relation value. Must be between -100 and 100.\n";

            try
            {
                int previousRelation = hero1.GetRelation(hero2);
                int change = value - previousRelation;
                ChangeRelationAction.ApplyRelationChangeBetweenHeroes(hero1, hero2, change, true);

                return $"Success: Relation between {hero1.Name} and {hero2.Name} changed from {previousRelation} to {value}.\n";
            }
            catch (Exception ex)
            {
                return $"Error: Failed to set relation.\n{ex.Message}\n";
            }
        }

        #endregion
    }
}