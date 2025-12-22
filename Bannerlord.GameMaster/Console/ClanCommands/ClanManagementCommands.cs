using Bannerlord.GameMaster.Console.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.Library;
using System.Reflection;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Party.PartyComponents;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.CampaignSystem.Settlements.Workshops;
using TaleWorlds.Core;
using TaleWorlds.ObjectSystem;
using Bannerlord.GameMaster.Clans;

namespace Bannerlord.GameMaster.Console.ClanCommands
{
    [CommandLineFunctionality.CommandLineArgumentFunction("clan", "gm")]
    public static class ClanManagementCommands
    {
        //MARK: gm.clan.add_hero
        /// <summary>
        /// Transfer a hero to another clan
        /// Usage: gm.clan.add_hero [clan] [hero]
        /// </summary>
        [CommandLineFunctionality.CommandLineArgumentFunction("add_hero", "gm.clan")]
        public static string AddHeroToClan(List<string> args)
        {
            return Cmd.Run(args, () =>
            {
                if (!CommandBase.ValidateCampaignMode(out string error))
                    return error;

                var usageMessage = CommandValidator.CreateUsageMessage(
                    "gm.clan.add_hero", "<clan> <hero>",
                    "Adds a hero to the specified clan.\n" +
                    "Supports named arguments: clan:empire_south hero:lord_1_1",
                    "gm.clan.add_hero empire_south lord_1_1");

                // Parse arguments
                var parsedArgs = CommandBase.ParseArguments(args);

                // Define valid arguments
                parsedArgs.SetValidArguments(
                    new CommandBase.ArgumentDefinition("clan", true),
                    new CommandBase.ArgumentDefinition("hero", true)
                );

                // Validate
                string validationError = parsedArgs.GetValidationError();
                if (validationError != null)
                    return CommandBase.FormatErrorMessage(validationError);

                if (parsedArgs.TotalCount < 2)
                    return usageMessage;

                // Parse clan
                string clanArg = parsedArgs.GetArgument("clan", 0);
                if (clanArg == null)
                    return CommandBase.FormatErrorMessage("Missing required argument 'clan'.");

                var (clan, clanError) = CommandBase.FindSingleClan(clanArg);
                if (clanError != null) return clanError;

                // Parse hero
                string heroArg = parsedArgs.GetArgument("hero", 1);
                if (heroArg == null)
                    return CommandBase.FormatErrorMessage("Missing required argument 'hero'.");

                var (hero, heroError) = CommandBase.FindSingleHero(heroArg);
                if (heroError != null) return heroError;

                // Build display
                var resolvedValues = new Dictionary<string, string>
                {
                    { "clan", clan.Name.ToString() },
                    { "hero", hero.Name.ToString() }
                };
                string argumentDisplay = parsedArgs.FormatArgumentDisplay("add_hero", resolvedValues);

                return CommandBase.ExecuteWithErrorHandling(() =>
                {
                    string previousClanName = hero.Clan?.Name?.ToString() ?? "No Clan";
                    hero.Clan = clan;

                    // Prevents crash if hero is moved to a clan and is not the leader, the game will crash when player choses to be released from his oath in conversation
                    if (hero == Hero.MainHero)
                        clan.SetLeader(Hero.MainHero);

                    return argumentDisplay + CommandBase.FormatSuccessMessage($"{hero.Name} transferred from '{previousClanName}' to '{clan.Name}'.");
                }, "Failed to add hero to clan");
            });
        }

        //MARK: add_gold
        /// <summary>
        /// Add gold to all clan members
        /// Usage: gm.clan.add_gold [clan] [amount]
        /// </summary>
        [CommandLineFunctionality.CommandLineArgumentFunction("add_gold", "gm.clan")]
        public static string AddClanGold(List<string> args)
        {
            return Cmd.Run(args, () =>
            {
                if (!CommandBase.ValidateCampaignMode(out string error))
                    return error;

                var usageMessage = CommandValidator.CreateUsageMessage(
                    "gm.clan.add_gold", "<clan> <amount>",
                    "Adds gold to all members of the clan.\n" +
                    "Supports named arguments: clan:empire_south amount:50000",
                    "gm.clan.add_gold empire_south 50000");

                // Parse arguments
                var parsedArgs = CommandBase.ParseArguments(args);

                // Define valid arguments
                parsedArgs.SetValidArguments(
                    new CommandBase.ArgumentDefinition("clan", true),
                    new CommandBase.ArgumentDefinition("amount", true)
                );

                // Validate
                string validationError = parsedArgs.GetValidationError();
                if (validationError != null)
                    return CommandBase.FormatErrorMessage(validationError);

                if (parsedArgs.TotalCount < 2)
                    return usageMessage;

                // Parse clan
                string clanArg = parsedArgs.GetArgument("clan", 0);
                if (clanArg == null)
                    return CommandBase.FormatErrorMessage("Missing required argument 'clan'.");

                var (clan, clanError) = CommandBase.FindSingleClan(clanArg);
                if (clanError != null) return clanError;

                // Parse amount
                string amountArg = parsedArgs.GetArgument("amount", 1);
                if (amountArg == null)
                    return CommandBase.FormatErrorMessage("Missing required argument 'amount'.");

                if (!CommandValidator.ValidateIntegerRange(amountArg, int.MinValue, int.MaxValue, out int amount, out string goldError))
                    return CommandBase.FormatErrorMessage(goldError);

                // Build display
                var resolvedValues = new Dictionary<string, string>
                {
                    { "clan", clan.Name.ToString() },
                    { "amount", amount.ToString() }
                };
                string argumentDisplay = parsedArgs.FormatArgumentDisplay("add_gold", resolvedValues);

                return CommandBase.ExecuteWithErrorHandling(() =>
                {
                    int previousGold = clan.Gold;
                    int membersCount = clan.Heroes.Count(h => h.IsAlive);

                    if (membersCount == 0)
                        return argumentDisplay + CommandBase.FormatErrorMessage($"{clan.Name} has no living heroes to receive gold.");

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

                    return argumentDisplay + CommandBase.FormatSuccessMessage(
                        $"Added {amount} gold to {clan.Name} (distributed among {membersCount} members).\n" +
                        $"Clan gold changed from {previousGold} to {clan.Gold}.");
                }, "Failed to add gold");
            });
        }

        //MARK: set_gold
        /// <summary>
        /// Set total clan gold by distributing to members
        /// Usage: gm.clan.set_gold [clan] [amount]
        /// </summary>
        [CommandLineFunctionality.CommandLineArgumentFunction("set_gold", "gm.clan")]
        public static string SetClanGold(List<string> args)
        {
            return Cmd.Run(args, () =>
            {
                if (!CommandBase.ValidateCampaignMode(out string error))
                    return error;

                var usageMessage = CommandValidator.CreateUsageMessage(
                    "gm.clan.set_gold", "<clan> <amount>",
                    "Sets the clan's total gold by distributing to all members.\n" +
                    "Supports named arguments: clan:empire_south amount:100000",
                    "gm.clan.set_gold empire_south 100000");

                // Parse arguments
                var parsedArgs = CommandBase.ParseArguments(args);

                // Define valid arguments
                parsedArgs.SetValidArguments(
                    new CommandBase.ArgumentDefinition("clan", true),
                    new CommandBase.ArgumentDefinition("amount", true)
                );

                // Validate
                string validationError = parsedArgs.GetValidationError();
                if (validationError != null)
                    return CommandBase.FormatErrorMessage(validationError);

                if (parsedArgs.TotalCount < 2)
                    return usageMessage;

                // Parse clan
                string clanArg = parsedArgs.GetArgument("clan", 0);
                if (clanArg == null)
                    return CommandBase.FormatErrorMessage("Missing required argument 'clan'.");

                var (clan, clanError) = CommandBase.FindSingleClan(clanArg);
                if (clanError != null) return clanError;

                // Parse amount
                string amountArg = parsedArgs.GetArgument("amount", 1);
                if (amountArg == null)
                    return CommandBase.FormatErrorMessage("Missing required argument 'amount'.");

                if (!CommandValidator.ValidateIntegerRange(amountArg, 0, int.MaxValue, out int targetAmount, out string goldError))
                    return CommandBase.FormatErrorMessage(goldError);

                // Build display
                var resolvedValues = new Dictionary<string, string>
                {
                    { "clan", clan.Name.ToString() },
                    { "amount", targetAmount.ToString() }
                };
                string argumentDisplay = parsedArgs.FormatArgumentDisplay("set_gold", resolvedValues);

                return CommandBase.ExecuteWithErrorHandling(() =>
                {
                    int previousGold = clan.Gold;
                    int membersCount = clan.Heroes.Count(h => h.IsAlive);

                    if (membersCount == 0)
                        return argumentDisplay + CommandBase.FormatErrorMessage($"{clan.Name} has no living heroes to receive gold.");

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

                    return argumentDisplay + CommandBase.FormatSuccessMessage(
                        $"Set {clan.Name}'s gold to {targetAmount} (distributed among {membersCount} members).\n" +
                        $"Previous clan gold: {previousGold}, New clan gold: {clan.Gold}.");
                }, "Failed to set gold");
            });
        }

        //MARK: add_gold_leader
        /// <summary>
        /// Add gold to clan leader only
        /// Usage: gm.clan.add_gold_leader [clan] [amount]
        /// </summary>
        [CommandLineFunctionality.CommandLineArgumentFunction("add_gold_leader", "gm.clan")]
        public static string AddGoldToLeader(List<string> args)
        {
            return Cmd.Run(args, () =>
            {
                if (!CommandBase.ValidateCampaignMode(out string error))
                    return error;

                var usageMessage = CommandValidator.CreateUsageMessage(
                    "gm.clan.add_gold_leader", "<clan> <amount>",
                    "Adds gold to the clan leader only.\n" +
                    "Supports named arguments: clan:empire_south amount:50000",
                    "gm.clan.add_gold_leader empire_south 50000");

                // Parse arguments
                var parsedArgs = CommandBase.ParseArguments(args);

                // Define valid arguments
                parsedArgs.SetValidArguments(
                    new CommandBase.ArgumentDefinition("clan", true),
                    new CommandBase.ArgumentDefinition("amount", true)
                );

                // Validate
                string validationError = parsedArgs.GetValidationError();
                if (validationError != null)
                    return CommandBase.FormatErrorMessage(validationError);

                if (parsedArgs.TotalCount < 2)
                    return usageMessage;

                // Parse clan
                string clanArg = parsedArgs.GetArgument("clan", 0);
                if (clanArg == null)
                    return CommandBase.FormatErrorMessage("Missing required argument 'clan'.");

                var (clan, clanError) = CommandBase.FindSingleClan(clanArg);
                if (clanError != null) return clanError;

                if (clan.Leader == null)
                    return CommandBase.FormatErrorMessage($"{clan.Name} has no leader.");

                // Parse amount
                string amountArg = parsedArgs.GetArgument("amount", 1);
                if (amountArg == null)
                    return CommandBase.FormatErrorMessage("Missing required argument 'amount'.");

                if (!CommandValidator.ValidateIntegerRange(amountArg, int.MinValue, int.MaxValue, out int amount, out string goldError))
                    return CommandBase.FormatErrorMessage(goldError);

                // Build display
                var resolvedValues = new Dictionary<string, string>
                {
                    { "clan", clan.Name.ToString() },
                    { "amount", amount.ToString() }
                };
                string argumentDisplay = parsedArgs.FormatArgumentDisplay("add_gold_leader", resolvedValues);

                return CommandBase.ExecuteWithErrorHandling(() =>
                {
                    int previousClanGold = clan.Gold;
                    int previousLeaderGold = clan.Leader.Gold;

                    clan.Leader.ChangeHeroGold(amount);

                    return argumentDisplay + CommandBase.FormatSuccessMessage(
                        $"Added {amount} gold to {clan.Leader.Name} (leader of {clan.Name}).\n" +
                        $"Leader gold: {previousLeaderGold} → {clan.Leader.Gold}\n" +
                        $"Clan total gold: {previousClanGold} → {clan.Gold}");
                }, "Failed to add gold to leader");
            });
        }

        //MARK: give_gold
        /// <summary>
        /// Distribute gold to specific clan member
        /// Usage: gm.clan.give_gold [clan] [hero] [amount]
        /// </summary>
        [CommandLineFunctionality.CommandLineArgumentFunction("give_gold", "gm.clan")]
        public static string GiveGoldToMember(List<string> args)
        {
            return Cmd.Run(args, () =>
            {
                if (!CommandBase.ValidateCampaignMode(out string error))
                    return error;

                var usageMessage = CommandValidator.CreateUsageMessage(
                    "gm.clan.give_gold", "<clan> <hero> <amount>",
                    "Gives gold to a specific clan member.\n" +
                    "Supports named arguments: clan:empire_south hero:lord_1_1 amount:10000",
                    "gm.clan.give_gold empire_south lord_1_1 10000");

                // Parse arguments
                var parsedArgs = CommandBase.ParseArguments(args);

                // Define valid arguments
                parsedArgs.SetValidArguments(
                    new CommandBase.ArgumentDefinition("clan", true),
                    new CommandBase.ArgumentDefinition("hero", true),
                    new CommandBase.ArgumentDefinition("amount", true)
                );

                // Validate
                string validationError = parsedArgs.GetValidationError();
                if (validationError != null)
                    return CommandBase.FormatErrorMessage(validationError);

                if (parsedArgs.TotalCount < 3)
                    return usageMessage;

                // Parse clan
                string clanArg = parsedArgs.GetArgument("clan", 0);
                if (clanArg == null)
                    return CommandBase.FormatErrorMessage("Missing required argument 'clan'.");

                var (clan, clanError) = CommandBase.FindSingleClan(clanArg);
                if (clanError != null) return clanError;

                // Parse hero
                string heroArg = parsedArgs.GetArgument("hero", 1);
                if (heroArg == null)
                    return CommandBase.FormatErrorMessage("Missing required argument 'hero'.");

                var (hero, heroError) = CommandBase.FindSingleHero(heroArg);
                if (heroError != null) return heroError;

                if (hero.Clan != clan)
                    return CommandBase.FormatErrorMessage($"{hero.Name} is not a member of {clan.Name}.");

                // Parse amount
                string amountArg = parsedArgs.GetArgument("amount", 2);
                if (amountArg == null)
                    return CommandBase.FormatErrorMessage("Missing required argument 'amount'.");

                if (!CommandValidator.ValidateIntegerRange(amountArg, int.MinValue, int.MaxValue, out int amount, out string goldError))
                    return CommandBase.FormatErrorMessage(goldError);

                // Build display
                var resolvedValues = new Dictionary<string, string>
                {
                    { "clan", clan.Name.ToString() },
                    { "hero", hero.Name.ToString() },
                    { "amount", amount.ToString() }
                };
                string argumentDisplay = parsedArgs.FormatArgumentDisplay("give_gold", resolvedValues);

                return CommandBase.ExecuteWithErrorHandling(() =>
                {
                    int previousClanGold = clan.Gold;
                    int previousHeroGold = hero.Gold;

                    hero.ChangeHeroGold(amount);

                    return argumentDisplay + CommandBase.FormatSuccessMessage(
                        $"Added {amount} gold to {hero.Name} (member of {clan.Name}).\n" +
                        $"Hero gold: {previousHeroGold} → {hero.Gold}\n" +
                        $"Clan total gold: {previousClanGold} → {clan.Gold}");
                }, "Failed to give gold to member");
            });
        }

        //MARK: set_renown
        /// <summary>
        /// Set clan renown
        /// Usage: gm.clan.set_renown [clan] [amount]
        /// </summary>
        [CommandLineFunctionality.CommandLineArgumentFunction("set_renown", "gm.clan")]
        public static string SetClanRenown(List<string> args)
        {
            return Cmd.Run(args, () =>
            {
                if (!CommandBase.ValidateCampaignMode(out string error))
                    return error;

                var usageMessage = CommandValidator.CreateUsageMessage(
                    "gm.clan.set_renown", "<clan> <amount>",
                    "Sets the clan's renown.\n" +
                    "Supports named arguments: clan:empire_south amount:500",
                    "gm.clan.set_renown empire_south 500");

                // Parse arguments
                var parsedArgs = CommandBase.ParseArguments(args);

                // Define valid arguments
                parsedArgs.SetValidArguments(
                    new CommandBase.ArgumentDefinition("clan", true),
                    new CommandBase.ArgumentDefinition("amount", true)
                );

                // Validate
                string validationError = parsedArgs.GetValidationError();
                if (validationError != null)
                    return CommandBase.FormatErrorMessage(validationError);

                if (parsedArgs.TotalCount < 2)
                    return usageMessage;

                // Parse clan
                string clanArg = parsedArgs.GetArgument("clan", 0);
                if (clanArg == null)
                    return CommandBase.FormatErrorMessage("Missing required argument 'clan'.");

                var (clan, clanError) = CommandBase.FindSingleClan(clanArg);
                if (clanError != null) return clanError;

                // Parse amount
                string amountArg = parsedArgs.GetArgument("amount", 1);
                if (amountArg == null)
                    return CommandBase.FormatErrorMessage("Missing required argument 'amount'.");

                if (!CommandValidator.ValidateFloatRange(amountArg, 0, float.MaxValue, out float amount, out string renownError))
                    return CommandBase.FormatErrorMessage(renownError);

                // Build display
                var resolvedValues = new Dictionary<string, string>
                {
                    { "clan", clan.Name.ToString() },
                    { "amount", amount.ToString("F0") }
                };
                string argumentDisplay = parsedArgs.FormatArgumentDisplay("set_renown", resolvedValues);

                return CommandBase.ExecuteWithErrorHandling(() =>
                {
                    float previousRenown = clan.Renown;
                    clan.Renown = amount;
                    return argumentDisplay + CommandBase.FormatSuccessMessage($"{clan.Name}'s renown changed from {previousRenown:F0} to {clan.Renown:F0}.");
                }, "Failed to set renown");
            });
        }

        //MARK: add_renown
        /// <summary>
        /// Add renown to clan
        /// Usage: gm.clan.add_renown [clan] [amount]
        /// </summary>
        [CommandLineFunctionality.CommandLineArgumentFunction("add_renown", "gm.clan")]
        public static string AddClanRenown(List<string> args)
        {
            return Cmd.Run(args, () =>
            {
                if (!CommandBase.ValidateCampaignMode(out string error))
                    return error;

                var usageMessage = CommandValidator.CreateUsageMessage(
                    "gm.clan.add_renown", "<clan> <amount>",
                    "Adds renown to the clan.\n" +
                    "Supports named arguments: clan:empire_south amount:100",
                    "gm.clan.add_renown empire_south 100");

                // Parse arguments
                var parsedArgs = CommandBase.ParseArguments(args);

                // Define valid arguments
                parsedArgs.SetValidArguments(
                    new CommandBase.ArgumentDefinition("clan", true),
                    new CommandBase.ArgumentDefinition("amount", true)
                );

                // Validate
                string validationError = parsedArgs.GetValidationError();
                if (validationError != null)
                    return CommandBase.FormatErrorMessage(validationError);

                if (parsedArgs.TotalCount < 2)
                    return usageMessage;

                // Parse clan
                string clanArg = parsedArgs.GetArgument("clan", 0);
                if (clanArg == null)
                    return CommandBase.FormatErrorMessage("Missing required argument 'clan'.");

                var (clan, clanError) = CommandBase.FindSingleClan(clanArg);
                if (clanError != null) return clanError;

                // Parse amount
                string amountArg = parsedArgs.GetArgument("amount", 1);
                if (amountArg == null)
                    return CommandBase.FormatErrorMessage("Missing required argument 'amount'.");

                if (!CommandValidator.ValidateFloatRange(amountArg, float.MinValue, float.MaxValue, out float amount, out string renownError))
                    return CommandBase.FormatErrorMessage(renownError);

                // Build display
                var resolvedValues = new Dictionary<string, string>
                {
                    { "clan", clan.Name.ToString() },
                    { "amount", amount.ToString("F0") }
                };
                string argumentDisplay = parsedArgs.FormatArgumentDisplay("add_renown", resolvedValues);

                return CommandBase.ExecuteWithErrorHandling(() =>
                {
                    float previousRenown = clan.Renown;
                    clan.AddRenown(amount, true);
                    return argumentDisplay + CommandBase.FormatSuccessMessage(
                        $"{clan.Name}'s renown changed from {previousRenown:F0} to {clan.Renown:F0} ({(amount >= 0 ? "+" : "")}{amount:F0}).");
                }, "Failed to add renown");
            });
        }

        //MARK: set_tier
        /// <summary>
        /// Change clan tier
        /// Usage: gm.clan.set_tier [clan] [tier]
        /// </summary>
        [CommandLineFunctionality.CommandLineArgumentFunction("set_tier", "gm.clan")]
        public static string SetClanTier(List<string> args)
        {
            return Cmd.Run(args, () =>
            {
                if (!CommandBase.ValidateCampaignMode(out string error))
                    return error;

                var usageMessage = CommandValidator.CreateUsageMessage(
                    "gm.clan.set_tier", "<clan> <tier>",
                    "Sets the clan's tier (0-6).\n" +
                    "Supports named arguments: clan:empire_south tier:5",
                    "gm.clan.set_tier empire_south 5");

                // Parse arguments
                var parsedArgs = CommandBase.ParseArguments(args);

                // Define valid arguments
                parsedArgs.SetValidArguments(
                    new CommandBase.ArgumentDefinition("clan", true),
                    new CommandBase.ArgumentDefinition("tier", true)
                );

                // Validate
                string validationError = parsedArgs.GetValidationError();
                if (validationError != null)
                    return CommandBase.FormatErrorMessage(validationError);

                if (parsedArgs.TotalCount < 2)
                    return usageMessage;

                // Parse clan
                string clanArg = parsedArgs.GetArgument("clan", 0);
                if (clanArg == null)
                    return CommandBase.FormatErrorMessage("Missing required argument 'clan'.");

                var (clan, clanError) = CommandBase.FindSingleClan(clanArg);
                if (clanError != null) return clanError;

                // Parse tier
                string tierArg = parsedArgs.GetArgument("tier", 1);
                if (tierArg == null)
                    return CommandBase.FormatErrorMessage("Missing required argument 'tier'.");

                if (!CommandValidator.ValidateIntegerRange(tierArg, 0, 6, out int tier, out string tierError))
                    return CommandBase.FormatErrorMessage(tierError);

                if (clan.Tier == tier)
                    return CommandBase.FormatErrorMessage($"Clan is already {clan.Tier}");

                // Build display
                var resolvedValues = new Dictionary<string, string>
                {
                    { "clan", clan.Name.ToString() },
                    { "tier", tier.ToString() }
                };
                string argumentDisplay = parsedArgs.FormatArgumentDisplay("set_tier", resolvedValues);

                return CommandBase.ExecuteWithErrorHandling(() =>
                {
                    int previousTier = clan.Tier;
                    clan.SetClanTier(tier);

                    return argumentDisplay + CommandBase.FormatSuccessMessage($"{clan.Name}'s tier changed from {previousTier} to {clan.Tier}.");
                }, "Failed to set tier");
            });
        }

        //MARK: destroy
        /// <summary>
        /// Destroy/Eliminate a clan
        /// Usage: gm.clan.destroy [clan]
        /// </summary>
        [CommandLineFunctionality.CommandLineArgumentFunction("destroy", "gm.clan")]
        public static string DestroyClan(List<string> args)
        {
            return Cmd.Run(args, () =>
            {
                if (!CommandBase.ValidateCampaignMode(out string error))
                    return error;

                var usageMessage = CommandValidator.CreateUsageMessage(
                    "gm.clan.destroy", "<clan>",
                    "Destroys/eliminates the specified clan.\n" +
                    "Supports named arguments: clan:rebel_clan_1",
                    "gm.clan.destroy rebel_clan_1");

                // Parse arguments
                var parsedArgs = CommandBase.ParseArguments(args);

                // Define valid arguments
                parsedArgs.SetValidArguments(
                    new CommandBase.ArgumentDefinition("clan", true)
                );

                // Validate
                string validationError = parsedArgs.GetValidationError();
                if (validationError != null)
                    return CommandBase.FormatErrorMessage(validationError);

                if (parsedArgs.TotalCount < 1)
                    return usageMessage;

                // Parse clan
                string clanArg = parsedArgs.GetArgument("clan", 0);
                if (clanArg == null)
                    return CommandBase.FormatErrorMessage("Missing required argument 'clan'.");

                var (clan, clanError) = CommandBase.FindSingleClan(clanArg);
                if (clanError != null) return clanError;

                if (clan.IsEliminated)
                    return CommandBase.FormatErrorMessage($"{clan.Name} is already eliminated.");

                if (clan == Clan.PlayerClan)
                    return CommandBase.FormatErrorMessage("Cannot destroy the player's clan.");

                // Build display
                var resolvedValues = new Dictionary<string, string>
                {
                    { "clan", clan.Name.ToString() }
                };
                string argumentDisplay = parsedArgs.FormatArgumentDisplay("destroy", resolvedValues);

                return CommandBase.ExecuteWithErrorHandling(() =>
                {
                    DestroyClanAction.Apply(clan);
                    return argumentDisplay + CommandBase.FormatSuccessMessage($"{clan.Name} has been destroyed/eliminated.");
                }, "Failed to destroy clan");
            });
        }

        //MARK: set_leader
        /// <summary>
        /// Change clan leader
        /// Usage: gm.clan.set_leader [clan] [hero]
        /// </summary>
        [CommandLineFunctionality.CommandLineArgumentFunction("set_leader", "gm.clan")]
        public static string SetClanLeader(List<string> args)
        {
            return Cmd.Run(args, () =>
            {
                if (!CommandBase.ValidateCampaignMode(out string error))
                    return error;

                var usageMessage = CommandValidator.CreateUsageMessage(
                    "gm.clan.set_leader", "<clan> <hero>",
                    "Changes the clan leader to the specified hero.\n" +
                    "Supports named arguments: clan:empire_south hero:lord_1_1",
                    "gm.clan.set_leader empire_south lord_1_1");

                // Parse arguments
                var parsedArgs = CommandBase.ParseArguments(args);

                // Define valid arguments
                parsedArgs.SetValidArguments(
                    new CommandBase.ArgumentDefinition("clan", true),
                    new CommandBase.ArgumentDefinition("hero", true)
                );

                // Validate
                string validationError = parsedArgs.GetValidationError();
                if (validationError != null)
                    return CommandBase.FormatErrorMessage(validationError);

                if (parsedArgs.TotalCount < 2)
                    return usageMessage;

                // Parse clan
                string clanArg = parsedArgs.GetArgument("clan", 0);
                if (clanArg == null)
                    return CommandBase.FormatErrorMessage("Missing required argument 'clan'.");

                var (clan, clanError) = CommandBase.FindSingleClan(clanArg);
                if (clanError != null) return clanError;

                // Parse hero
                string heroArg = parsedArgs.GetArgument("hero", 1);
                if (heroArg == null)
                    return CommandBase.FormatErrorMessage("Missing required argument 'hero'.");

                var (hero, heroError) = CommandBase.FindSingleHero(heroArg);
                if (heroError != null) return heroError;

                if (hero.Clan != clan)
                    return CommandBase.FormatErrorMessage($"{hero.Name} is not a member of {clan.Name}.");

                // Build display
                var resolvedValues = new Dictionary<string, string>
                {
                    { "clan", clan.Name.ToString() },
                    { "hero", hero.Name.ToString() }
                };
                string argumentDisplay = parsedArgs.FormatArgumentDisplay("set_leader", resolvedValues);

                return CommandBase.ExecuteWithErrorHandling(() =>
                {
                    string previousLeader = clan.Leader?.Name?.ToString() ?? "None";
                    clan.SetLeader(hero);
                    return argumentDisplay + CommandBase.FormatSuccessMessage($"{clan.Name}'s leader changed from {previousLeader} to {hero.Name}.");
                }, "Failed to set clan leader");
            });
        }

        //MARK: rename
        /// <summary>
        /// Rename a clan
        /// Usage: gm.clan.rename [clan] [newName]
        /// </summary>
        [CommandLineFunctionality.CommandLineArgumentFunction("rename", "gm.clan")]
        public static string RenameClan(List<string> args)
        {
            return Cmd.Run(args, () =>
            {
                if (!CommandBase.ValidateCampaignMode(out string error))
                    return error;

                var usageMessage = CommandValidator.CreateUsageMessage(
                    "gm.clan.rename", "<clan> <newName>",
                    "Renames the specified clan. Use SINGLE QUOTES for multi-word names.\n" +
                    "Supports named arguments: clan:empire_south newName:'Southern Empire Lords'",
                    "gm.clan.rename empire_south 'Southern Empire Lords'\n" +
                    "gm.clan.rename clan_1 NewClanName");

                // Parse arguments
                var parsedArgs = CommandBase.ParseArguments(args);

                // Define valid arguments
                parsedArgs.SetValidArguments(
                    new CommandBase.ArgumentDefinition("clan", true),
                    new CommandBase.ArgumentDefinition("newName", true, null, "name")
                );

                // Validate
                string validationError = parsedArgs.GetValidationError();
                if (validationError != null)
                    return CommandBase.FormatErrorMessage(validationError);

                if (parsedArgs.TotalCount < 2)
                    return usageMessage;

                // Parse clan
                string clanArg = parsedArgs.GetArgument("clan", 0);
                if (clanArg == null)
                    return CommandBase.FormatErrorMessage("Missing required argument 'clan'.");

                var (clan, clanError) = CommandBase.FindSingleClan(clanArg);
                if (clanError != null) return clanError;

                // Parse newName
                string newName = parsedArgs.GetArgument("newName", 1) ?? parsedArgs.GetNamed("name");
                if (newName == null)
                    return CommandBase.FormatErrorMessage("Missing required argument 'newName'.");

                if (string.IsNullOrWhiteSpace(newName))
                    return CommandBase.FormatErrorMessage("New name cannot be empty.");

                // Build display
                var resolvedValues = new Dictionary<string, string>
                {
                    { "clan", clan.Name.ToString() },
                    { "newName", newName }
                };
                string argumentDisplay = parsedArgs.FormatArgumentDisplay("rename", resolvedValues);

                return CommandBase.ExecuteWithErrorHandling(() =>
                {
                    string previousName = clan.Name.ToString();
                    clan.SetStringName(newName);

                    return argumentDisplay + CommandBase.FormatSuccessMessage(
                        $"Clan renamed from '{previousName}' to '{clan.Name}' (ID: {clan.StringId})");
                }, "Failed to rename clan");
            });
        }

        //MARK: set_culture
        /// <summary>
        /// Change a clan's culture
        /// Usage: gm.clan.set_culture [clan] [culture]
        /// </summary>
        [CommandLineFunctionality.CommandLineArgumentFunction("set_culture", "gm.clan")]
        public static string SetCulture(List<string> args)
        {
            return Cmd.Run(args, () =>
            {
                if (!CommandBase.ValidateCampaignMode(out string error))
                    return error;

                var usageMessage = CommandValidator.CreateUsageMessage(
                    "gm.clan.set_culture", "<clan> <culture>",
                    "Changes the clan's culture. Also updates the clan's basic troop to match the new culture.\n" +
                    "Supports named arguments: clan:empire_south culture:vlandia",
                    "gm.clan.set_culture empire_south vlandia\n" +
                    "gm.clan.set_culture my_clan battania");

                // Parse arguments
                var parsedArgs = CommandBase.ParseArguments(args);

                // Define valid arguments
                parsedArgs.SetValidArguments(
                    new CommandBase.ArgumentDefinition("clan", true),
                    new CommandBase.ArgumentDefinition("culture", true)
                );

                // Validate
                string validationError = parsedArgs.GetValidationError();
                if (validationError != null)
                    return CommandBase.FormatErrorMessage(validationError);

                if (parsedArgs.TotalCount < 2)
                    return usageMessage;

                // Parse clan
                string clanArg = parsedArgs.GetArgument("clan", 0);
                if (clanArg == null)
                    return CommandBase.FormatErrorMessage("Missing required argument 'clan'.");

                var (clan, clanError) = CommandBase.FindSingleClan(clanArg);
                if (clanError != null) return clanError;

                // Parse culture
                string cultureArg = parsedArgs.GetArgument("culture", 1);
                if (cultureArg == null)
                    return CommandBase.FormatErrorMessage("Missing required argument 'culture'.");

                CultureObject newCulture = MBObjectManager.Instance.GetObject<CultureObject>(cultureArg);
                if (newCulture == null)
                    return CommandBase.FormatErrorMessage($"Culture '{cultureArg}' not found. Valid cultures: aserai, battania, empire, khuzait, nord, sturgia, vlandia");

                // Build display
                var resolvedValues = new Dictionary<string, string>
                {
                    { "clan", clan.Name.ToString() },
                    { "culture", newCulture.Name.ToString() }
                };
                string argumentDisplay = parsedArgs.FormatArgumentDisplay("set_culture", resolvedValues);

                return CommandBase.ExecuteWithErrorHandling(() =>
                {
                    string previousCulture = clan.Culture?.Name?.ToString() ?? "None";
                    clan.Culture = newCulture;
                    clan.BasicTroop = newCulture.BasicTroop;

                    return argumentDisplay + CommandBase.FormatSuccessMessage(
                        $"{clan.Name}'s culture changed from '{previousCulture}' to '{clan.Culture.Name}'.\n" +
                        $"Basic troop updated to: {clan.BasicTroop?.Name}");
                }, "Failed to set culture");
            });
        }
    }
}
