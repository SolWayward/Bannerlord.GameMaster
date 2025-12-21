using Bannerlord.GameMaster.Console.Common;
using Bannerlord.GameMaster.Heroes;
using Bannerlord.GameMaster.Party;
using System;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.ObjectSystem;

namespace Bannerlord.GameMaster.Console.HeroCommands
{
    [CommandLineFunctionality.CommandLineArgumentFunction("hero", "gm")]
    public static class HeroManagementCommands
    {
        //MARK: set_clan
        /// <summary>
        /// Transfer a hero to another clan
        /// Usage: gm.hero.set_clan [hero] [clan]
        /// </summary>
        [CommandLineFunctionality.CommandLineArgumentFunction("set_clan", "gm.hero")]
        public static string SetClan(List<string> args)
        {
            return Cmd.Run(args, () =>
            {
                if (!CommandBase.ValidateCampaignMode(out string error))
                    return error;

                var usageMessage = CommandValidator.CreateUsageMessage(
                    "gm.hero.set_clan", "<hero> <clan>",
                    "Transfers a hero to another clan.",
                    "gm.hero.set_clan lord_1_1 clan_empire_south_1");

                if (!CommandBase.ValidateArgumentCount(args, 2, usageMessage, out error))
                    return error;

                var (hero, heroError) = CommandBase.FindSingleHero(args[0]);
                if (heroError != null) return heroError;

                var (clan, clanError) = CommandBase.FindSingleClan(args[1]);
                if (clanError != null) return clanError;

                return CommandBase.ExecuteWithErrorHandling(() =>
                {
                    string previousClanName = hero.Clan?.Name?.ToString() ?? "No Clan";
                    hero.Clan = clan;

                    return CommandBase.FormatSuccessMessage(
                        $"{hero.Name} (ID: {hero.StringId}) transferred from '{previousClanName}' to '{clan.Name}'.\n" +
                        $"Updated details: {hero.FormattedDetails()}");
                }, "Failed to transfer hero");
            });
        }

        /// MARK: remove_clan 
        /// <summary>
        /// Remove a hero from their clan
        /// Usage: gm.hero.remove_clan [hero]
        /// </summary>
        [CommandLineFunctionality.CommandLineArgumentFunction("remove_clan", "gm.hero")]
        public static string RemoveClan(List<string> args)
        {
            return Cmd.Run(args, () =>
            {
                if (!CommandBase.ValidateCampaignMode(out string error))
                    return error;

                var usageMessage = CommandValidator.CreateUsageMessage(
                    "gm.hero.remove_clan", "<hero>",
                    "Removes a hero from their current clan.",
                    "gm.hero.remove_clan lord_1_1");

                if (!CommandBase.ValidateArgumentCount(args, 1, usageMessage, out error))
                    return error;

                var (hero, heroError) = CommandBase.FindSingleHero(args[0]);
                if (heroError != null) return heroError;

                if (hero.Clan == null)
                    return CommandBase.FormatErrorMessage($"{hero.Name} is not a member of any clan.");

                return CommandBase.ExecuteWithErrorHandling(() =>
                {
                    string previousClanName = hero.Clan.Name.ToString();
                    hero.Clan = null;

                    return CommandBase.FormatSuccessMessage(
                        $"{hero.Name} (ID: {hero.StringId}) removed from clan '{previousClanName}'.");
                }, "Failed to remove hero from clan");
            });
        }

        /// MARK: kill 
        /// <summary>
        /// Kill a hero
        /// Usage: gm.hero.kill [hero] [optional: show_death_log]
        /// </summary>
        [CommandLineFunctionality.CommandLineArgumentFunction("kill", "gm.hero")]
        public static string KillHero(List<string> args)
        {
            return Cmd.Run(args, () =>
            {
                if (!CommandBase.ValidateCampaignMode(out string error))
                    return error;

                var usageMessage = CommandValidator.CreateUsageMessage(
                    "gm.hero.kill", "<hero> [show_death_log]",
                    "Kills the specified hero.",
                    "gm.hero.kill lord_1_1 true (shows death log)");

                if (!CommandBase.ValidateArgumentCount(args, 1, usageMessage, out error))
                    return error;

                var (hero, heroError) = CommandBase.FindSingleHero(args[0]);
                if (heroError != null) return heroError;

                if (!hero.IsAlive)
                    return CommandBase.FormatErrorMessage($"{hero.Name} is already dead.");

                bool showDeathLog = false;
                if (args.Count > 1)
                {
                    if (!CommandValidator.ValidateBoolean(args[1], out showDeathLog, out string boolError))
                        return CommandBase.FormatErrorMessage(boolError);
                }

                return CommandBase.ExecuteWithErrorHandling(() =>
                {
                    KillCharacterAction.ApplyByMurder(hero, null, showDeathLog);
                    return CommandBase.FormatSuccessMessage($"{hero.Name} (ID: {hero.StringId}) has been killed.");
                }, "Failed to kill hero");
            });
        }

        /// MARK: imprison 
        /// <summary>
        /// Imprison a hero
        /// Usage: gm.hero.imprison [prisoner] [captor]
        /// </summary>
        [CommandLineFunctionality.CommandLineArgumentFunction("imprison", "gm.hero")]
        public static string ImprisonHero(List<string> args)
        {
            return Cmd.Run(args, () =>
            {
                if (!CommandBase.ValidateCampaignMode(out string error))
                    return error;

                var usageMessage = CommandValidator.CreateUsageMessage(
                    "gm.hero.imprison", "<prisoner> <captor>",
                    "Imprisons a hero by another hero/party.",
                    "gm.hero.imprison lord_1_1 lord_2_1");

                if (!CommandBase.ValidateArgumentCount(args, 2, usageMessage, out error))
                    return error;

                var (prisoner, prisonerError) = CommandBase.FindSingleHero(args[0]);
                if (prisonerError != null) return prisonerError;

                var (captor, captorError) = CommandBase.FindSingleHero(args[1]);
                if (captorError != null) return captorError;

                if (prisoner.IsPrisoner)
                    return CommandBase.FormatErrorMessage($"{prisoner.Name} is already a prisoner.");

                return CommandBase.ExecuteWithErrorHandling(() =>
                {
                    // Get the captor's party base
                    PartyBase captorParty = captor.PartyBelongedTo?.Party
                                            ?? captor.Clan?.Kingdom?.Leader?.PartyBelongedTo?.Party
                                            ?? Settlement.FindFirst(s => s.OwnerClan == captor.Clan)?.Party;

                    if (captorParty == null)
                        return CommandBase.FormatErrorMessage($"{captor.Name} has no valid party or settlement to hold prisoners.");

                    TakePrisonerAction.Apply(captorParty, prisoner);
                    return CommandBase.FormatSuccessMessage($"{prisoner.Name} (ID: {prisoner.StringId}) is now imprisoned by {captor.Name}.");
                }, "Failed to imprison hero");
            });
        }

        /// MARK: release
        /// <summary>
        /// Release a hero from prison
        /// Usage: gm.hero.release [hero]
        /// </summary>
        [CommandLineFunctionality.CommandLineArgumentFunction("release", "gm.hero")]
        public static string ReleaseHero(List<string> args)
        {
            return Cmd.Run(args, () =>
            {
                if (!CommandBase.ValidateCampaignMode(out string error))
                    return error;

                var usageMessage = CommandValidator.CreateUsageMessage(
                    "gm.hero.release", "<hero>",
                    "Releases a hero from prison.",
                    "gm.hero.release lord_1_1");

                if (!CommandBase.ValidateArgumentCount(args, 1, usageMessage, out error))
                    return error;

                var (hero, heroError) = CommandBase.FindSingleHero(args[0]);
                if (heroError != null) return heroError;

                if (!hero.IsPrisoner)
                    return CommandBase.FormatErrorMessage($"{hero.Name} is not a prisoner.");

                return CommandBase.ExecuteWithErrorHandling(() =>
                {
                    EndCaptivityAction.ApplyByReleasedAfterBattle(hero);
                    return CommandBase.FormatSuccessMessage($"{hero.Name} (ID: {hero.StringId}) has been released from captivity.");
                }, "Failed to release hero");
            });
        }

        /// MARK: set_age
        /// <summary>
        /// Change hero's age
        /// Usage: gm.hero.set_age [hero] [age]
        /// </summary>
        [CommandLineFunctionality.CommandLineArgumentFunction("set_age", "gm.hero")]
        public static string SetAge(List<string> args)
        {
            return Cmd.Run(args, () =>
            {
                if (!CommandBase.ValidateCampaignMode(out string error))
                    return error;

                var usageMessage = CommandValidator.CreateUsageMessage(
                    "gm.hero.set_age", "<hero> <age>",
                    "Sets the hero's age.",
                    "gm.hero.set_age lord_1_1 30");

                if (!CommandBase.ValidateArgumentCount(args, 2, usageMessage, out error))
                    return error;

                var (hero, heroError) = CommandBase.FindSingleHero(args[0]);
                if (heroError != null) return heroError;

                if (!CommandValidator.ValidateFloatRange(args[1], 0, 128, out float age, out string ageError))
                    return CommandBase.FormatErrorMessage(ageError);

                return CommandBase.ExecuteWithErrorHandling(() =>
                {
                    float previousAge = hero.Age;
                    hero.SetBirthDay(CampaignTime.YearsFromNow(-age));
                    return CommandBase.FormatSuccessMessage($"{hero.Name}'s age changed from {previousAge:F0} to {hero.Age:F0}.");
                }, "Failed to set age");
            });
        }

        /// MARK: set_gold
        /// <summary>
        /// Change hero's gold
        /// Usage: gm.hero.set_gold [hero] [amount]
        /// </summary>
        [CommandLineFunctionality.CommandLineArgumentFunction("set_gold", "gm.hero")]
        public static string SetGold(List<string> args)
        {
            return Cmd.Run(args, () =>
            {
                if (!CommandBase.ValidateCampaignMode(out string error))
                    return error;

                var usageMessage = CommandValidator.CreateUsageMessage(
                    "gm.hero.set_gold", "<hero> <amount>",
                    "Sets the hero's gold amount.",
                    "gm.hero.set_gold lord_1_1 10000");

                if (!CommandBase.ValidateArgumentCount(args, 2, usageMessage, out error))
                    return error;

                var (hero, heroError) = CommandBase.FindSingleHero(args[0]);
                if (heroError != null) return heroError;

                if (!CommandValidator.ValidateIntegerRange(args[1], int.MinValue, int.MaxValue, out int amount, out string goldError))
                    return CommandBase.FormatErrorMessage(goldError);

                return CommandBase.ExecuteWithErrorHandling(() =>
                {
                    int previousGold = hero.Gold;
                    hero.ChangeHeroGold(amount - previousGold);
                    return CommandBase.FormatSuccessMessage($"{hero.Name}'s gold changed from {previousGold} to {hero.Gold}.");
                }, "Failed to set gold");
            });
        }

        /// MARK: add_gold 
        /// <summary>
        /// Add gold to hero
        /// Usage: gm.hero.add_gold [hero] [amount]
        /// </summary>
        [CommandLineFunctionality.CommandLineArgumentFunction("add_gold", "gm.hero")]
        public static string AddGold(List<string> args)
        {
            return Cmd.Run(args, () =>
            {
                if (!CommandBase.ValidateCampaignMode(out string error))
                    return error;

                var usageMessage = CommandValidator.CreateUsageMessage(
                    "gm.hero.add_gold", "<hero> <amount>",
                    "Adds gold to the hero (use negative to subtract).",
                    "gm.hero.add_gold lord_1_1 5000");

                if (!CommandBase.ValidateArgumentCount(args, 2, usageMessage, out error))
                    return error;

                var (hero, heroError) = CommandBase.FindSingleHero(args[0]);
                if (heroError != null) return heroError;

                if (!CommandValidator.ValidateIntegerRange(args[1], int.MinValue, int.MaxValue, out int amount, out string goldError))
                    return CommandBase.FormatErrorMessage(goldError);

                return CommandBase.ExecuteWithErrorHandling(() =>
                {
                    int previousGold = hero.Gold;
                    hero.ChangeHeroGold(amount);
                    return CommandBase.FormatSuccessMessage(
                        $"{hero.Name}'s gold changed from {previousGold} to {hero.Gold} ({(amount >= 0 ? "+" : "")}{amount}).");
                }, "Failed to add gold");
            });
        }

        /// MARK: heal
        /// <summary>
        /// Heal a hero to full health
        /// Usage: gm.hero.heal [hero]
        /// </summary>
        [CommandLineFunctionality.CommandLineArgumentFunction("heal", "gm.hero")]
        public static string HealHero(List<string> args)
        {
            return Cmd.Run(args, () =>
            {
                if (!CommandBase.ValidateCampaignMode(out string error))
                    return error;

                var usageMessage = CommandValidator.CreateUsageMessage(
                    "gm.hero.heal", "<hero>",
                    "Heals a hero to full health.",
                    "gm.hero.heal lord_1_1");

                if (!CommandBase.ValidateArgumentCount(args, 1, usageMessage, out error))
                    return error;

                var (hero, heroError) = CommandBase.FindSingleHero(args[0]);
                if (heroError != null) return heroError;

                if (!hero.IsAlive)
                    return CommandBase.FormatErrorMessage($"Cannot heal {hero.Name} - hero is dead.");

                return CommandBase.ExecuteWithErrorHandling(() =>
                {
                    hero.HitPoints = hero.CharacterObject.MaxHitPoints();
                    return CommandBase.FormatSuccessMessage(
                        $"{hero.Name} has been healed to full health ({hero.HitPoints}/{hero.CharacterObject.MaxHitPoints()}).");
                }, "Failed to heal hero");
            });
        }

        /// MARK: set_relation
        /// <summary>
        /// Set relation between two heroes
        /// Usage: gm.hero.set_relation [hero1] [hero2] [value]
        /// </summary>
        [CommandLineFunctionality.CommandLineArgumentFunction("set_relation", "gm.hero")]
        public static string SetRelation(List<string> args)
        {
            return Cmd.Run(args, () =>
            {
                if (!CommandBase.ValidateCampaignMode(out string error))
                    return error;

                var usageMessage = CommandValidator.CreateUsageMessage(
                    "gm.hero.set_relation", "<hero1> <hero2> <value>",
                    "Sets the relationship value between two heroes (-100 to 100).",
                    "gm.hero.set_relation lord_1_1 lord_2_1 50");

                if (!CommandBase.ValidateArgumentCount(args, 3, usageMessage, out error))
                    return error;

                var (hero1, hero1Error) = CommandBase.FindSingleHero(args[0]);
                if (hero1Error != null) return hero1Error;

                var (hero2, hero2Error) = CommandBase.FindSingleHero(args[1]);
                if (hero2Error != null) return hero2Error;

                if (!CommandValidator.ValidateIntegerRange(args[2], -100, 100, out int value, out string relationError))
                    return CommandBase.FormatErrorMessage(relationError);

                return CommandBase.ExecuteWithErrorHandling(() =>
                {
                    int previousRelation = hero1.GetRelation(hero2);
                    int change = value - previousRelation;
                    ChangeRelationAction.ApplyRelationChangeBetweenHeroes(hero1, hero2, change, true);

                    return CommandBase.FormatSuccessMessage(
                        $"Relation between {hero1.Name} and {hero2.Name} changed from {previousRelation} to {value}.");
                }, "Failed to set relation");
            });
        }

        /// MARK: add_hero_to_party
        /// <summary>
        /// Add a hero to another hero's party as a companion. Hero leaves their current party if already in one.
        /// Usage: gm.hero.add_hero_to_party [hero] [partyLeader]
        /// </summary>
        [CommandLineFunctionality.CommandLineArgumentFunction("add_hero_to_party", "gm.hero")]
        public static string AddHeroToParty(List<string> args)
        {
            return Cmd.Run(args, () =>
            {
                if (!CommandBase.ValidateCampaignMode(out string error))
                    return error;

                var usageMessage = CommandValidator.CreateUsageMessage(
                    "gm.hero.add_hero_to_party", "<hero> <partyLeader>",
                    "Adds a hero as a companion to another hero's party. The hero will leave their current party if they are already in one.\n" +
                    "The hero's clan will be changed to match the party leader's clan.",
                    "gm.hero.add_hero_to_party companion_1 player\n" +
                    "gm.hero.add_hero_to_party wanderer_1 derthert");

                if (!CommandBase.ValidateArgumentCount(args, 2, usageMessage, out error))
                    return error;

                var (hero, heroError) = CommandBase.FindSingleHero(args[0]);
                if (heroError != null) return heroError;

                var (partyLeader, leaderError) = CommandBase.FindSingleHero(args[1]);
                if (leaderError != null) return leaderError;

                return CommandBase.ExecuteWithErrorHandling(() =>
                {
                    // Validate party leader has a party
                    if (partyLeader.PartyBelongedTo == null)
                        return CommandBase.FormatErrorMessage($"{partyLeader.Name} does not have a party.");

                    if (partyLeader.PartyBelongedTo.LeaderHero != partyLeader)
                        return CommandBase.FormatErrorMessage($"{partyLeader.Name} is not a party leader.");

                    // Check if hero is trying to join their own party
                    if (hero == partyLeader)
                        return CommandBase.FormatErrorMessage("Cannot add a hero to their own party.");

                    string previousPartyInfo = "None";
                    
                    // Remove hero from current party if they are in one
                    if (hero.PartyBelongedTo != null)
                    {
                        previousPartyInfo = hero.PartyBelongedTo.Name?.ToString() ?? "Unknown";
                        
                        // If hero is a party leader, we need to disband their party first
                        if (hero.PartyBelongedTo.LeaderHero == hero)
                        {
                            // Disband the party (this is complex, so we'll just prevent it for now)
                            return CommandBase.FormatErrorMessage(
                                $"{hero.Name} is currently leading their own party ({hero.PartyBelongedTo.Name}). " +
                                "Party leaders must disband their party before joining another. This is not yet implemented.");
                        }
                        
                        // Remove hero from their current party roster
                        hero.PartyBelongedTo.MemberRoster.RemoveTroop(hero.CharacterObject);
                    }

                    // Add hero to the target party using the extension method
                    partyLeader.PartyBelongedTo.AddCompanionToParty(hero);

                    return CommandBase.FormatSuccessMessage(
                        $"{hero.Name} has joined {partyLeader.Name}'s party.\n" +
                        $"Previous party: {previousPartyInfo}\n" +
                        $"New party: {partyLeader.PartyBelongedTo.Name}\n" +
                        $"Clan updated to: {hero.Clan?.Name}");
                }, "Failed to add hero to party");
            });
        }

        /// MARK: create_party
        /// <summary>
        /// Create a party for a hero at their last known location or home settlement
        /// Usage: gm.hero.create_party [hero]
        /// </summary>
        [CommandLineFunctionality.CommandLineArgumentFunction("create_party", "gm.hero")]
        public static string CreateParty(List<string> args)
        {
            return Cmd.Run(args, () =>
            {
                if (!CommandBase.ValidateCampaignMode(out string error))
                    return error;

                var usageMessage = CommandValidator.CreateUsageMessage(
                    "gm.hero.create_party", "<hero>",
                    "Creates a party for the specified hero. The party will spawn at the hero's last known location if available,\n" +
                    "otherwise at their home settlement or an alternative settlement.\n" +
                    "The party is initialized with 10 basic troops and 20000 trade gold.",
                    "gm.hero.create_party lord_1_1\n" +
                    "gm.hero.create_party wanderer_1");

                if (!CommandBase.ValidateArgumentCount(args, 1, usageMessage, out error))
                    return error;

                var (hero, heroError) = CommandBase.FindSingleHero(args[0]);
                if (heroError != null) return heroError;

                return CommandBase.ExecuteWithErrorHandling(() =>
                {
                    // Check if hero already has a party
                    if (hero.PartyBelongedTo != null && hero.PartyBelongedTo.LeaderHero == hero)
                        return CommandBase.FormatErrorMessage($"{hero.Name} already leads a party: {hero.PartyBelongedTo.Name}");

                    // Determine spawn settlement
                    Settlement spawnSettlement = null;
                    
                    // Try to use last seen place if it's a settlement
                    if (hero.LastKnownClosestSettlement != null)
                    {
                        spawnSettlement = hero.LastKnownClosestSettlement;
                    }
                    
                    // Fallback to home or alternative settlement
                    if (spawnSettlement == null)
                    {
                        spawnSettlement = hero.GetHomeOrAlternativeSettlement();
                    }

                    if (spawnSettlement == null)
                        return CommandBase.FormatErrorMessage($"Could not find a suitable settlement to spawn {hero.Name}'s party.");

                    // Create the party using the extension method
                    MobileParty newParty = hero.CreateParty(spawnSettlement);

                    if (newParty == null)
                        return CommandBase.FormatErrorMessage($"Failed to create party for {hero.Name}.");

                    return CommandBase.FormatSuccessMessage(
                        $"Created party for {hero.Name}.\n" +
                        $"Party: {newParty.Name}\n" +
                        $"Location: {spawnSettlement.Name}\n" +
                        $"Initial roster: {newParty.MemberRoster.TotalManCount} troops\n" +
                        $"Trade gold: {newParty.PartyTradeGold}");
                }, "Failed to create party");
            });
        }

        /// MARK: set_culture
        /// <summary>
        /// Change a hero's culture
        /// Usage: gm.hero.set_culture [hero] [culture]
        /// </summary>
        [CommandLineFunctionality.CommandLineArgumentFunction("set_culture", "gm.hero")]
        public static string SetCulture(List<string> args)
        {
            return Cmd.Run(args, () =>
            {
                if (!CommandBase.ValidateCampaignMode(out string error))
                    return error;

                var usageMessage = CommandValidator.CreateUsageMessage(
                    "gm.hero.set_culture", "<hero> <culture>",
                    "Changes the hero's culture. Note: This does not change the hero's equipment or appearance, only the culture property.",
                    "gm.hero.set_culture lord_1_1 vlandia\n" +
                    "gm.hero.set_culture companion_1 battania");

                if (!CommandBase.ValidateArgumentCount(args, 2, usageMessage, out error))
                    return error;

                var (hero, heroError) = CommandBase.FindSingleHero(args[0]);
                if (heroError != null) return heroError;

                // Parse culture
                CultureObject newCulture = MBObjectManager.Instance.GetObject<CultureObject>(args[1]);
                if (newCulture == null)
                    return CommandBase.FormatErrorMessage($"Culture '{args[1]}' not found. Valid cultures: aserai, battania, empire, khuzait, nord, sturgia, vlandia");

                return CommandBase.ExecuteWithErrorHandling(() =>
                {
                    string previousCulture = hero.Culture?.Name?.ToString() ?? "None";
                    hero.Culture = newCulture;

                    return CommandBase.FormatSuccessMessage(
                        $"{hero.Name}'s culture changed from '{previousCulture}' to '{hero.Culture.Name}'.");
                }, "Failed to set culture");
            });
        }
    }
}