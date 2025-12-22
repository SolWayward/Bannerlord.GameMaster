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
                    "Transfers a hero to another clan.\n" +
                    "Supports named arguments: hero:lord_1_1 clan:clan_empire_south_1",
                    "gm.hero.set_clan lord_1_1 clan_empire_south_1");

                // Parse arguments
                var parsedArgs = CommandBase.ParseArguments(args);

                // Define valid arguments
                parsedArgs.SetValidArguments(
                    new CommandBase.ArgumentDefinition("hero", true),
                    new CommandBase.ArgumentDefinition("clan", true)
                );

                // Validate
                string validationError = parsedArgs.GetValidationError();
                if (validationError != null)
                    return CommandBase.FormatErrorMessage(validationError);

                if (parsedArgs.TotalCount < 2)
                    return usageMessage;

                // Parse hero
                string heroArg = parsedArgs.GetArgument("hero", 0);
                if (heroArg == null)
                    return CommandBase.FormatErrorMessage("Missing required argument 'hero'.");

                var (hero, heroError) = CommandBase.FindSingleHero(heroArg);
                if (heroError != null) return heroError;

                // Parse clan
                string clanArg = parsedArgs.GetArgument("clan", 1);
                if (clanArg == null)
                    return CommandBase.FormatErrorMessage("Missing required argument 'clan'.");

                var (clan, clanError) = CommandBase.FindSingleClan(clanArg);
                if (clanError != null) return clanError;

                // Build display
                var resolvedValues = new Dictionary<string, string>
                {
                    { "hero", hero.Name.ToString() },
                    { "clan", clan.Name.ToString() }
                };
                string argumentDisplay = parsedArgs.FormatArgumentDisplay("set_clan", resolvedValues);

                return CommandBase.ExecuteWithErrorHandling(() =>
                {
                    string previousClanName = hero.Clan?.Name?.ToString() ?? "No Clan";
                    hero.Clan = clan;

                    return argumentDisplay + CommandBase.FormatSuccessMessage(
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
                    "Removes a hero from their current clan.\n" +
                    "Supports named arguments: hero:lord_1_1",
                    "gm.hero.remove_clan lord_1_1");

                // Parse arguments
                var parsedArgs = CommandBase.ParseArguments(args);

                // Define valid arguments
                parsedArgs.SetValidArguments(
                    new CommandBase.ArgumentDefinition("hero", true)
                );

                // Validate
                string validationError = parsedArgs.GetValidationError();
                if (validationError != null)
                    return CommandBase.FormatErrorMessage(validationError);

                if (parsedArgs.TotalCount < 1)
                    return usageMessage;

                // Parse hero
                string heroArg = parsedArgs.GetArgument("hero", 0);
                if (heroArg == null)
                    return CommandBase.FormatErrorMessage("Missing required argument 'hero'.");

                var (hero, heroError) = CommandBase.FindSingleHero(heroArg);
                if (heroError != null) return heroError;

                if (hero.Clan == null)
                    return CommandBase.FormatErrorMessage($"{hero.Name} is not a member of any clan.");

                // Build display
                var resolvedValues = new Dictionary<string, string>
                {
                    { "hero", hero.Name.ToString() }
                };
                string argumentDisplay = parsedArgs.FormatArgumentDisplay("remove_clan", resolvedValues);

                return CommandBase.ExecuteWithErrorHandling(() =>
                {
                    string previousClanName = hero.Clan.Name.ToString();
                    hero.Clan = null;

                    return argumentDisplay + CommandBase.FormatSuccessMessage(
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
                    "Kills the specified hero.\n" +
                    "Supports named arguments: hero:lord_1_1 showDeathLog:true",
                    "gm.hero.kill lord_1_1 true (shows death log)");

                // Parse arguments
                var parsedArgs = CommandBase.ParseArguments(args);

                // Define valid arguments
                parsedArgs.SetValidArguments(
                    new CommandBase.ArgumentDefinition("hero", true),
                    new CommandBase.ArgumentDefinition("showDeathLog", false, null, "show_death_log")
                );

                // Validate
                string validationError = parsedArgs.GetValidationError();
                if (validationError != null)
                    return CommandBase.FormatErrorMessage(validationError);

                if (parsedArgs.TotalCount < 1)
                    return usageMessage;

                // Parse hero
                string heroArg = parsedArgs.GetArgument("hero", 0);
                if (heroArg == null)
                    return CommandBase.FormatErrorMessage("Missing required argument 'hero'.");

                var (hero, heroError) = CommandBase.FindSingleHero(heroArg);
                if (heroError != null) return heroError;

                if (!hero.IsAlive)
                    return CommandBase.FormatErrorMessage($"{hero.Name} is already dead.");

                // Parse optional showDeathLog
                bool showDeathLog = false;
                string showDeathLogArg = parsedArgs.GetArgument("showDeathLog", 1) ?? parsedArgs.GetNamed("show_death_log");
                if (showDeathLogArg != null)
                {
                    if (!CommandValidator.ValidateBoolean(showDeathLogArg, out showDeathLog, out string boolError))
                        return CommandBase.FormatErrorMessage(boolError);
                }

                // Build display
                var resolvedValues = new Dictionary<string, string>
                {
                    { "hero", hero.Name.ToString() },
                    { "showDeathLog", showDeathLog.ToString() }
                };
                string argumentDisplay = parsedArgs.FormatArgumentDisplay("kill", resolvedValues);

                return CommandBase.ExecuteWithErrorHandling(() =>
                {
                    KillCharacterAction.ApplyByMurder(hero, null, showDeathLog);
                    return argumentDisplay + CommandBase.FormatSuccessMessage($"{hero.Name} (ID: {hero.StringId}) has been killed.");
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
                    "Imprisons a hero by another hero/party.\n" +
                    "Supports named arguments: prisoner:lord_1_1 captor:lord_2_1",
                    "gm.hero.imprison lord_1_1 lord_2_1");

                // Parse arguments
                var parsedArgs = CommandBase.ParseArguments(args);

                // Define valid arguments
                parsedArgs.SetValidArguments(
                    new CommandBase.ArgumentDefinition("prisoner", true),
                    new CommandBase.ArgumentDefinition("captor", true)
                );

                // Validate
                string validationError = parsedArgs.GetValidationError();
                if (validationError != null)
                    return CommandBase.FormatErrorMessage(validationError);

                if (parsedArgs.TotalCount < 2)
                    return usageMessage;

                // Parse prisoner
                string prisonerArg = parsedArgs.GetArgument("prisoner", 0);
                if (prisonerArg == null)
                    return CommandBase.FormatErrorMessage("Missing required argument 'prisoner'.");

                var (prisoner, prisonerError) = CommandBase.FindSingleHero(prisonerArg);
                if (prisonerError != null) return prisonerError;

                // Parse captor
                string captorArg = parsedArgs.GetArgument("captor", 1);
                if (captorArg == null)
                    return CommandBase.FormatErrorMessage("Missing required argument 'captor'.");

                var (captor, captorError) = CommandBase.FindSingleHero(captorArg);
                if (captorError != null) return captorError;

                if (prisoner.IsPrisoner)
                    return CommandBase.FormatErrorMessage($"{prisoner.Name} is already a prisoner.");

                // Build display
                var resolvedValues = new Dictionary<string, string>
                {
                    { "prisoner", prisoner.Name.ToString() },
                    { "captor", captor.Name.ToString() }
                };
                string argumentDisplay = parsedArgs.FormatArgumentDisplay("imprison", resolvedValues);

                return CommandBase.ExecuteWithErrorHandling(() =>
                {
                    // Get the captor's party base
                    PartyBase captorParty = captor.PartyBelongedTo?.Party
                                            ?? captor.Clan?.Kingdom?.Leader?.PartyBelongedTo?.Party
                                            ?? Settlement.FindFirst(s => s.OwnerClan == captor.Clan)?.Party;

                    if (captorParty == null)
                        return argumentDisplay + CommandBase.FormatErrorMessage($"{captor.Name} has no valid party or settlement to hold prisoners.");

                    TakePrisonerAction.Apply(captorParty, prisoner);
                    return argumentDisplay + CommandBase.FormatSuccessMessage($"{prisoner.Name} (ID: {prisoner.StringId}) is now imprisoned by {captor.Name}.");
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
                    "Releases a hero from prison.\n" +
                    "Supports named arguments: hero:lord_1_1",
                    "gm.hero.release lord_1_1");

                // Parse arguments
                var parsedArgs = CommandBase.ParseArguments(args);

                // Define valid arguments
                parsedArgs.SetValidArguments(
                    new CommandBase.ArgumentDefinition("hero", true)
                );

                // Validate
                string validationError = parsedArgs.GetValidationError();
                if (validationError != null)
                    return CommandBase.FormatErrorMessage(validationError);

                if (parsedArgs.TotalCount < 1)
                    return usageMessage;

                // Parse hero
                string heroArg = parsedArgs.GetArgument("hero", 0);
                if (heroArg == null)
                    return CommandBase.FormatErrorMessage("Missing required argument 'hero'.");

                var (hero, heroError) = CommandBase.FindSingleHero(heroArg);
                if (heroError != null) return heroError;

                if (!hero.IsPrisoner)
                    return CommandBase.FormatErrorMessage($"{hero.Name} is not a prisoner.");

                // Build display
                var resolvedValues = new Dictionary<string, string>
                {
                    { "hero", hero.Name.ToString() }
                };
                string argumentDisplay = parsedArgs.FormatArgumentDisplay("release", resolvedValues);

                return CommandBase.ExecuteWithErrorHandling(() =>
                {
                    EndCaptivityAction.ApplyByReleasedAfterBattle(hero);
                    return argumentDisplay + CommandBase.FormatSuccessMessage($"{hero.Name} (ID: {hero.StringId}) has been released from captivity.");
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
                    "Sets the hero's age.\n" +
                    "Supports named arguments: hero:lord_1_1 age:30",
                    "gm.hero.set_age lord_1_1 30");

                // Parse arguments
                var parsedArgs = CommandBase.ParseArguments(args);

                // Define valid arguments
                parsedArgs.SetValidArguments(
                    new CommandBase.ArgumentDefinition("hero", true),
                    new CommandBase.ArgumentDefinition("age", true)
                );

                // Validate
                string validationError = parsedArgs.GetValidationError();
                if (validationError != null)
                    return CommandBase.FormatErrorMessage(validationError);

                if (parsedArgs.TotalCount < 2)
                    return usageMessage;

                // Parse hero
                string heroArg = parsedArgs.GetArgument("hero", 0);
                if (heroArg == null)
                    return CommandBase.FormatErrorMessage("Missing required argument 'hero'.");

                var (hero, heroError) = CommandBase.FindSingleHero(heroArg);
                if (heroError != null) return heroError;

                // Parse age
                string ageArg = parsedArgs.GetArgument("age", 1);
                if (ageArg == null)
                    return CommandBase.FormatErrorMessage("Missing required argument 'age'.");

                if (!CommandValidator.ValidateFloatRange(ageArg, 0, 128, out float age, out string ageError))
                    return CommandBase.FormatErrorMessage(ageError);

                // Build display
                var resolvedValues = new Dictionary<string, string>
                {
                    { "hero", hero.Name.ToString() },
                    { "age", age.ToString("F0") }
                };
                string argumentDisplay = parsedArgs.FormatArgumentDisplay("set_age", resolvedValues);

                return CommandBase.ExecuteWithErrorHandling(() =>
                {
                    float previousAge = hero.Age;
                    hero.SetBirthDay(CampaignTime.YearsFromNow(-age));
                    return argumentDisplay + CommandBase.FormatSuccessMessage($"{hero.Name}'s age changed from {previousAge:F0} to {hero.Age:F0}.");
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
                    "Sets the hero's gold amount.\n" +
                    "Supports named arguments: hero:lord_1_1 amount:10000",
                    "gm.hero.set_gold lord_1_1 10000");

                // Parse arguments
                var parsedArgs = CommandBase.ParseArguments(args);

                // Define valid arguments
                parsedArgs.SetValidArguments(
                    new CommandBase.ArgumentDefinition("hero", true),
                    new CommandBase.ArgumentDefinition("amount", true)
                );

                // Validate
                string validationError = parsedArgs.GetValidationError();
                if (validationError != null)
                    return CommandBase.FormatErrorMessage(validationError);

                if (parsedArgs.TotalCount < 2)
                    return usageMessage;

                // Parse hero
                string heroArg = parsedArgs.GetArgument("hero", 0);
                if (heroArg == null)
                    return CommandBase.FormatErrorMessage("Missing required argument 'hero'.");

                var (hero, heroError) = CommandBase.FindSingleHero(heroArg);
                if (heroError != null) return heroError;

                // Parse amount
                string amountArg = parsedArgs.GetArgument("amount", 1);
                if (amountArg == null)
                    return CommandBase.FormatErrorMessage("Missing required argument 'amount'.");

                if (!CommandValidator.ValidateIntegerRange(amountArg, int.MinValue, int.MaxValue, out int amount, out string goldError))
                    return CommandBase.FormatErrorMessage(goldError);

                // Build display
                var resolvedValues = new Dictionary<string, string>
                {
                    { "hero", hero.Name.ToString() },
                    { "amount", amount.ToString() }
                };
                string argumentDisplay = parsedArgs.FormatArgumentDisplay("set_gold", resolvedValues);

                return CommandBase.ExecuteWithErrorHandling(() =>
                {
                    int previousGold = hero.Gold;
                    hero.ChangeHeroGold(amount - previousGold);
                    return argumentDisplay + CommandBase.FormatSuccessMessage($"{hero.Name}'s gold changed from {previousGold} to {hero.Gold}.");
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
                    "Adds gold to the hero (use negative to subtract).\n" +
                    "Supports named arguments: hero:lord_1_1 amount:5000",
                    "gm.hero.add_gold lord_1_1 5000");

                // Parse arguments
                var parsedArgs = CommandBase.ParseArguments(args);

                // Define valid arguments
                parsedArgs.SetValidArguments(
                    new CommandBase.ArgumentDefinition("hero", true),
                    new CommandBase.ArgumentDefinition("amount", true)
                );

                // Validate
                string validationError = parsedArgs.GetValidationError();
                if (validationError != null)
                    return CommandBase.FormatErrorMessage(validationError);

                if (parsedArgs.TotalCount < 2)
                    return usageMessage;

                // Parse hero
                string heroArg = parsedArgs.GetArgument("hero", 0);
                if (heroArg == null)
                    return CommandBase.FormatErrorMessage("Missing required argument 'hero'.");

                var (hero, heroError) = CommandBase.FindSingleHero(heroArg);
                if (heroError != null) return heroError;

                // Parse amount
                string amountArg = parsedArgs.GetArgument("amount", 1);
                if (amountArg == null)
                    return CommandBase.FormatErrorMessage("Missing required argument 'amount'.");

                if (!CommandValidator.ValidateIntegerRange(amountArg, int.MinValue, int.MaxValue, out int amount, out string goldError))
                    return CommandBase.FormatErrorMessage(goldError);

                // Build display
                var resolvedValues = new Dictionary<string, string>
                {
                    { "hero", hero.Name.ToString() },
                    { "amount", amount.ToString() }
                };
                string argumentDisplay = parsedArgs.FormatArgumentDisplay("add_gold", resolvedValues);

                return CommandBase.ExecuteWithErrorHandling(() =>
                {
                    int previousGold = hero.Gold;
                    hero.ChangeHeroGold(amount);
                    return argumentDisplay + CommandBase.FormatSuccessMessage(
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
                    "Heals a hero to full health.\n" +
                    "Supports named arguments: hero:lord_1_1",
                    "gm.hero.heal lord_1_1");

                // Parse arguments
                var parsedArgs = CommandBase.ParseArguments(args);

                // Define valid arguments
                parsedArgs.SetValidArguments(
                    new CommandBase.ArgumentDefinition("hero", true)
                );

                // Validate
                string validationError = parsedArgs.GetValidationError();
                if (validationError != null)
                    return CommandBase.FormatErrorMessage(validationError);

                if (parsedArgs.TotalCount < 1)
                    return usageMessage;

                // Parse hero
                string heroArg = parsedArgs.GetArgument("hero", 0);
                if (heroArg == null)
                    return CommandBase.FormatErrorMessage("Missing required argument 'hero'.");

                var (hero, heroError) = CommandBase.FindSingleHero(heroArg);
                if (heroError != null) return heroError;

                if (!hero.IsAlive)
                    return CommandBase.FormatErrorMessage($"Cannot heal {hero.Name} - hero is dead.");

                // Build display
                var resolvedValues = new Dictionary<string, string>
                {
                    { "hero", hero.Name.ToString() }
                };
                string argumentDisplay = parsedArgs.FormatArgumentDisplay("heal", resolvedValues);

                return CommandBase.ExecuteWithErrorHandling(() =>
                {
                    hero.HitPoints = hero.CharacterObject.MaxHitPoints();
                    return argumentDisplay + CommandBase.FormatSuccessMessage(
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
                    "Sets the relationship value between two heroes (-100 to 100).\n" +
                    "Supports named arguments: hero1:lord_1_1 hero2:lord_2_1 value:50",
                    "gm.hero.set_relation lord_1_1 lord_2_1 50");

                // Parse arguments
                var parsedArgs = CommandBase.ParseArguments(args);

                // Define valid arguments
                parsedArgs.SetValidArguments(
                    new CommandBase.ArgumentDefinition("hero1", true),
                    new CommandBase.ArgumentDefinition("hero2", true),
                    new CommandBase.ArgumentDefinition("value", true)
                );

                // Validate
                string validationError = parsedArgs.GetValidationError();
                if (validationError != null)
                    return CommandBase.FormatErrorMessage(validationError);

                if (parsedArgs.TotalCount < 3)
                    return usageMessage;

                // Parse hero1
                string hero1Arg = parsedArgs.GetArgument("hero1", 0);
                if (hero1Arg == null)
                    return CommandBase.FormatErrorMessage("Missing required argument 'hero1'.");

                var (hero1, hero1Error) = CommandBase.FindSingleHero(hero1Arg);
                if (hero1Error != null) return hero1Error;

                // Parse hero2
                string hero2Arg = parsedArgs.GetArgument("hero2", 1);
                if (hero2Arg == null)
                    return CommandBase.FormatErrorMessage("Missing required argument 'hero2'.");

                var (hero2, hero2Error) = CommandBase.FindSingleHero(hero2Arg);
                if (hero2Error != null) return hero2Error;

                // Parse value
                string valueArg = parsedArgs.GetArgument("value", 2);
                if (valueArg == null)
                    return CommandBase.FormatErrorMessage("Missing required argument 'value'.");

                if (!CommandValidator.ValidateIntegerRange(valueArg, -100, 100, out int value, out string relationError))
                    return CommandBase.FormatErrorMessage(relationError);

                // Build display
                var resolvedValues = new Dictionary<string, string>
                {
                    { "hero1", hero1.Name.ToString() },
                    { "hero2", hero2.Name.ToString() },
                    { "value", value.ToString() }
                };
                string argumentDisplay = parsedArgs.FormatArgumentDisplay("set_relation", resolvedValues);

                return CommandBase.ExecuteWithErrorHandling(() =>
                {
                    int previousRelation = hero1.GetRelation(hero2);
                    int change = value - previousRelation;
                    ChangeRelationAction.ApplyRelationChangeBetweenHeroes(hero1, hero2, change, true);

                    return argumentDisplay + CommandBase.FormatSuccessMessage(
                        $"Relation between {hero1.Name} and {hero2.Name} changed from {previousRelation} to {value}.");
                }, "Failed to set relation");
            });
        }

        /// MARK: add_hero_to_party
        /// <summary>
        /// Add a hero to another hero's party. Hero leaves their current party if already in one.
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
                    "The hero's clan will be changed to match the party leader's clan.\n" +
                    "Supports named arguments: hero:companion_1 partyLeader:player",
                    "gm.hero.add_hero_to_party companion_1 player\n" +
                    "gm.hero.add_hero_to_party wanderer_1 derthert");

                // Parse arguments
                var parsedArgs = CommandBase.ParseArguments(args);

                // Define valid arguments
                parsedArgs.SetValidArguments(
                    new CommandBase.ArgumentDefinition("hero", true),
                    new CommandBase.ArgumentDefinition("partyLeader", true, null, "leader")
                );

                // Validate
                string validationError = parsedArgs.GetValidationError();
                if (validationError != null)
                    return CommandBase.FormatErrorMessage(validationError);

                if (parsedArgs.TotalCount < 2)
                    return usageMessage;

                // Parse hero
                string heroArg = parsedArgs.GetArgument("hero", 0);
                if (heroArg == null)
                    return CommandBase.FormatErrorMessage("Missing required argument 'hero'.");

                var (hero, heroError) = CommandBase.FindSingleHero(heroArg);
                if (heroError != null) return heroError;

                // Parse partyLeader
                string leaderArg = parsedArgs.GetArgument("partyLeader", 1) ?? parsedArgs.GetNamed("leader");
                if (leaderArg == null)
                    return CommandBase.FormatErrorMessage("Missing required argument 'partyLeader'.");

                var (partyLeader, leaderError) = CommandBase.FindSingleHero(leaderArg);
                if (leaderError != null) return leaderError;

                // Build display
                var resolvedValues = new Dictionary<string, string>
                {
                    { "hero", hero.Name.ToString() },
                    { "partyLeader", partyLeader.Name.ToString() }
                };
                string argumentDisplay = parsedArgs.FormatArgumentDisplay("add_hero_to_party", resolvedValues);

                return CommandBase.ExecuteWithErrorHandling(() =>
                {
                    // Validate party leader has a party
                    if (partyLeader.PartyBelongedTo == null)
                        return argumentDisplay + CommandBase.FormatErrorMessage($"{partyLeader.Name} does not have a party.");

                    if (partyLeader.PartyBelongedTo.LeaderHero != partyLeader)
                        return argumentDisplay + CommandBase.FormatErrorMessage($"{partyLeader.Name} is not a party leader.");

                    // Check if hero is trying to join their own party
                    if (hero == partyLeader)
                        return argumentDisplay + CommandBase.FormatErrorMessage("Cannot add a hero to their own party.");

                    string previousPartyInfo = "None";
                    
                    // Remove hero from current party if they are in one
                    if (hero.PartyBelongedTo != null)
                    {
                        previousPartyInfo = hero.PartyBelongedTo.Name?.ToString() ?? "Unknown";
                        
                        // If hero is a party leader, we need to disband their party first
                        if (hero.PartyBelongedTo.LeaderHero == hero)
                        {
                            // Disband the party (this is complex, so we'll just prevent it for now)
                            return argumentDisplay + CommandBase.FormatErrorMessage(
                                $"{hero.Name} is currently leading their own party ({hero.PartyBelongedTo.Name}). " +
                                "Party leaders must disband their party before joining another. This is not yet implemented.");
                        }
                        
                        // Remove hero from their current party roster
                        hero.PartyBelongedTo.MemberRoster.RemoveTroop(hero.CharacterObject);
                    }

                    if (hero.Occupation == Occupation.Wanderer)
                        partyLeader.PartyBelongedTo.AddCompanionToParty(hero);
                    else
                        partyLeader.PartyBelongedTo.AddLordToParty(hero);

                    return argumentDisplay + CommandBase.FormatSuccessMessage(
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
                    "The party is initialized with 10 basic troops and 20000 trade gold.\n" +
                    "Supports named arguments: hero:lord_1_1",
                    "gm.hero.create_party lord_1_1\n" +
                    "gm.hero.create_party wanderer_1");

                // Parse arguments
                var parsedArgs = CommandBase.ParseArguments(args);

                // Define valid arguments
                parsedArgs.SetValidArguments(
                    new CommandBase.ArgumentDefinition("hero", true)
                );

                // Validate
                string validationError = parsedArgs.GetValidationError();
                if (validationError != null)
                    return CommandBase.FormatErrorMessage(validationError);

                if (parsedArgs.TotalCount < 1)
                    return usageMessage;

                // Parse hero
                string heroArg = parsedArgs.GetArgument("hero", 0);
                if (heroArg == null)
                    return CommandBase.FormatErrorMessage("Missing required argument 'hero'.");

                var (hero, heroError) = CommandBase.FindSingleHero(heroArg);
                if (heroError != null) return heroError;

                // Build display
                var resolvedValues = new Dictionary<string, string>
                {
                    { "hero", hero.Name.ToString() }
                };
                string argumentDisplay = parsedArgs.FormatArgumentDisplay("create_party", resolvedValues);

                return CommandBase.ExecuteWithErrorHandling(() =>
                {
                    // Check if hero already has a party
                    if (hero.PartyBelongedTo != null && hero.PartyBelongedTo.LeaderHero == hero)
                        return argumentDisplay + CommandBase.FormatErrorMessage($"{hero.Name} already leads a party: {hero.PartyBelongedTo.Name}");

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
                        return argumentDisplay + CommandBase.FormatErrorMessage($"Could not find a suitable settlement to spawn {hero.Name}'s party.");

                    // Create the party using the extension method
                    MobileParty newParty = hero.CreateParty(spawnSettlement);

                    if (newParty == null)
                        return argumentDisplay + CommandBase.FormatErrorMessage($"Failed to create party for {hero.Name}.");

                    return argumentDisplay + CommandBase.FormatSuccessMessage(
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
                    "Changes the hero's culture. Note: This does not change the hero's equipment or appearance, only the culture property.\n" +
                    "Supports named arguments: hero:lord_1_1 culture:vlandia",
                    "gm.hero.set_culture lord_1_1 vlandia\n" +
                    "gm.hero.set_culture companion_1 battania");

                // Parse arguments
                var parsedArgs = CommandBase.ParseArguments(args);

                // Define valid arguments
                parsedArgs.SetValidArguments(
                    new CommandBase.ArgumentDefinition("hero", true),
                    new CommandBase.ArgumentDefinition("culture", true)
                );

                // Validate
                string validationError = parsedArgs.GetValidationError();
                if (validationError != null)
                    return CommandBase.FormatErrorMessage(validationError);

                if (parsedArgs.TotalCount < 2)
                    return usageMessage;

                // Parse hero
                string heroArg = parsedArgs.GetArgument("hero", 0);
                if (heroArg == null)
                    return CommandBase.FormatErrorMessage("Missing required argument 'hero'.");

                var (hero, heroError) = CommandBase.FindSingleHero(heroArg);
                if (heroError != null) return heroError;

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
                    { "hero", hero.Name.ToString() },
                    { "culture", newCulture.Name.ToString() }
                };
                string argumentDisplay = parsedArgs.FormatArgumentDisplay("set_culture", resolvedValues);

                return CommandBase.ExecuteWithErrorHandling(() =>
                {
                    string previousCulture = hero.Culture?.Name?.ToString() ?? "None";
                    hero.Culture = newCulture;

                    return argumentDisplay + CommandBase.FormatSuccessMessage(
                        $"{hero.Name}'s culture changed from '{previousCulture}' to '{hero.Culture.Name}'.");
                }, "Failed to set culture");
            });
        }
    }
}
