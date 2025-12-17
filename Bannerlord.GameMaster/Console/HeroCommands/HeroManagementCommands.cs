using Bannerlord.GameMaster.Console.Common;
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
using TaleWorlds.ObjectSystem;

namespace Bannerlord.GameMaster.Console.HeroCommands
{
    [CommandLineFunctionality.CommandLineArgumentFunction("hero", "gm")]
    public static class HeroManagementCommands
    {
        #region Clan Management

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

        #endregion

        #region Hero State Management

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

        #endregion

        #region Hero Attributes

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

        #endregion

        #region Relationships

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

        #endregion

        #region Hero Generation

        /// <summary>
        /// Generate new lords with random templates and good equipment
        /// Usage: gm.hero.generate_lords [count] [clan]
        /// </summary>
        [CommandLineFunctionality.CommandLineArgumentFunction("generate_lords", "gm.hero")]
        public static string GenerateLords(List<string> args)
        {
            return Cmd.Run(args, () =>
            {
                if (!CommandBase.ValidateCampaignMode(out string error))
                    return error;

                var usageMessage = CommandValidator.CreateUsageMessage(
                    "gm.hero.generate_lords", "[count=1] [clan=random]",
                    "Creates lords from random templates with good gear and decent stats. Age 30-40. If clan not specified, each lord goes to a different random clan.",
                    "gm.hero.generate_lords 3\ngm.hero.generate_lords 5 empire_south");

                // Parse count (optional, default 1)
                int count = 1;
                if (args.Count > 0)
                {
                    if (!CommandValidator.ValidateIntegerRange(args[0], 1, 20, out count, out string countError))
                        return CommandBase.FormatErrorMessage(countError);
                }

                // Parse clan (optional)
                Clan targetClan = null;
                if (args.Count > 1)
                {
                    var (clan, clanError) = CommandBase.FindSingleClan(args[1]);
                    if (clanError != null) return clanError;
                    targetClan = clan;
                }

                return CommandBase.ExecuteWithErrorHandling(() =>
                {
                    // Get available noble/warrior templates
                    var lordTemplates = CharacterObject.All
                        .Where(c => !c.IsHero && c.Occupation == Occupation.Lord && c.Culture != null)
                        .ToList();

                    if (lordTemplates.Count == 0)
                        return CommandBase.FormatErrorMessage("No lord templates found in game data.");

                    // Get available clans for random assignment
                    var availableClans = Clan.All
                        .Where(c => !c.IsEliminated && !c.IsBanditFaction && c.Leader != null)
                        .ToList();

                    if (availableClans.Count == 0)
                        return CommandBase.FormatErrorMessage("No available clans found.");

                    var random = new Random();
                    var createdLords = new List<(Hero hero, Clan clan)>();
                    var usedClans = new HashSet<Clan>();

                    for (int i = 0; i < count; i++)
                    {
                        // Select random template with random gender
                       var genderFilteredTemplates = lordTemplates
                           .Where(t => t.IsFemale == (random.Next(2) == 0))
                           .ToList();
                       
                       // Fall back to all templates if no matching gender found
                       if (genderFilteredTemplates.Count == 0)
                           genderFilteredTemplates = lordTemplates;
                           
                       var template = genderFilteredTemplates[random.Next(genderFilteredTemplates.Count)];

                        // Determine clan for this lord
                        Clan assignedClan;
                        if (targetClan != null)
                        {
                            assignedClan = targetClan;
                        }
                        else
                        {
                            // Find a clan not yet used
                            var unusedClans = availableClans.Where(c => !usedClans.Contains(c)).ToList();
                            if (unusedClans.Count == 0)
                            {
                                // All clans used, reset
                                usedClans.Clear();
                                unusedClans = availableClans.ToList();
                            }
                            assignedClan = unusedClans[random.Next(unusedClans.Count)];
                            usedClans.Add(assignedClan);
                        }

                        // Generate unique ID
                        int randomId = random.Next(10000, 99999);
                        string lordId = $"gm_lord_{assignedClan.StringId}_{CampaignTime.Now.GetYear}_{randomId}";

                        // Create hero with age 30-40
                        int age = random.Next(30, 41);
                        Hero newLord = HeroCreator.CreateSpecialHero(
                            template,
                            assignedClan.Leader?.CurrentSettlement ?? Settlement.All.FirstOrDefault(s => s.OwnerClan == assignedClan),
                            assignedClan,
                            null,
                            age
                        );

                        if (newLord == null)
                            continue;

                        // Set as active lord
                        newLord.ChangeState(Hero.CharacterStates.Active);
                        newLord.SetNewOccupation(Occupation.Lord);

                        // Give decent stats (level 15-25)
                        int targetLevel = random.Next(15, 26);
                        for (int level = 1; level < targetLevel; level++)
                        {
                            newLord.HeroDeveloper.AddFocus(
                                DefaultSkills.OneHanded,
                                1,
                                false
                            );
                        }

                        // Give good equipment
                        var equipment = newLord.BattleEquipment;
                        
                        // Find and equip armor based on culture
                        var armorItems = MBObjectManager.Instance.GetObjectTypeList<ItemObject>()
                            .Where(item => item.Culture == template.Culture &&
                                         (item.Type == ItemObject.ItemTypeEnum.BodyArmor ||
                                          item.Type == ItemObject.ItemTypeEnum.HeadArmor ||
                                          item.Type == ItemObject.ItemTypeEnum.LegArmor ||
                                          item.Type == ItemObject.ItemTypeEnum.HandArmor ||
                                          item.Type == ItemObject.ItemTypeEnum.Cape) &&
                                         item.Tier >= ItemObject.ItemTiers.Tier4)
                            .ToList();

                        if (armorItems.Count > 0)
                        {
                            foreach (var armorPiece in armorItems.Take(5))
                            {
                                EquipmentIndex slot = EquipmentIndex.None;
                                if (armorPiece.Type == ItemObject.ItemTypeEnum.BodyArmor)
                                    slot = EquipmentIndex.Body;
                                else if (armorPiece.Type == ItemObject.ItemTypeEnum.HeadArmor)
                                    slot = EquipmentIndex.Head;
                                else if (armorPiece.Type == ItemObject.ItemTypeEnum.LegArmor)
                                    slot = EquipmentIndex.Leg;
                                else if (armorPiece.Type == ItemObject.ItemTypeEnum.HandArmor)
                                    slot = EquipmentIndex.Gloves;
                                else if (armorPiece.Type == ItemObject.ItemTypeEnum.Cape)
                                    slot = EquipmentIndex.Cape;

                                if (slot != EquipmentIndex.None && equipment[slot].IsEmpty)
                                {
                                    equipment[slot] = new EquipmentElement(armorPiece);
                                }
                            }
                        }

                        // Find and equip weapon
                        var weapons = MBObjectManager.Instance.GetObjectTypeList<ItemObject>()
                            .Where(item => item.Culture == template.Culture &&
                                         item.Type == ItemObject.ItemTypeEnum.OneHandedWeapon &&
                                         item.Tier >= ItemObject.ItemTiers.Tier3)
                            .ToList();

                        if (weapons.Count > 0)
                        {
                            var weapon = weapons[random.Next(weapons.Count)];
                            if (equipment[EquipmentIndex.Weapon0].IsEmpty)
                            {
                                equipment[EquipmentIndex.Weapon0] = new EquipmentElement(weapon);
                            }
                        }

                        createdLords.Add((newLord, assignedClan));
                    }

                    if (createdLords.Count == 0)
                        return CommandBase.FormatErrorMessage("Failed to create any lords.");

                    var result = new System.Text.StringBuilder();
                    result.AppendLine($"Successfully created {createdLords.Count} lord(s):");
                    foreach (var (lord, clan) in createdLords)
                    {
                        result.AppendLine($"  - {lord.Name} (ID: {lord.StringId}, Age: {(int)lord.Age}, Clan: {clan.Name})");
                    }

                    return CommandBase.FormatSuccessMessage(result.ToString());
                }, "Failed to generate lords");
            });
        }

        /// <summary>
        /// Create a fresh lord with minimal stats and equipment
        /// Usage: gm.hero.create_lord [gender] [name] [clan]
        /// </summary>
        [CommandLineFunctionality.CommandLineArgumentFunction("create_lord", "gm.hero")]
        public static string CreateLord(List<string> args)
        {
            return Cmd.Run(args, () =>
            {
                if (!CommandBase.ValidateCampaignMode(out string error))
                    return error;

                var usageMessage = CommandValidator.CreateUsageMessage(
                    "gm.hero.create_lord", "<gender> <name> <clan>",
                    "Creates a fresh lord age 20-24 with minimal stats and only clothes. Gender must be 'male' or 'female'.",
                    "gm.hero.create_lord male NewLord empire_south");

                if (!CommandBase.ValidateArgumentCount(args, 3, usageMessage, out error))
                    return error;

                // Parse gender
                bool isFemale;
                string genderArg = args[0].ToLower();
                if (genderArg == "male" || genderArg == "m")
                    isFemale = false;
                else if (genderArg == "female" || genderArg == "f")
                    isFemale = true;
                else
                    return CommandBase.FormatErrorMessage("Gender must be 'male' or 'female'.");

                string name = args[1];
                if (string.IsNullOrWhiteSpace(name))
                    return CommandBase.FormatErrorMessage("Name cannot be empty.");

                var (clan, clanError) = CommandBase.FindSingleClan(args[2]);
                if (clanError != null) return clanError;

                return CommandBase.ExecuteWithErrorHandling(() =>
                {
                    var random = new Random();
                    
                    // Get all cultures for random selection
                    var allCultures = MBObjectManager.Instance.GetObjectTypeList<TaleWorlds.Core.BasicCultureObject>()
                        .Where(c => c != null)
                        .ToList();
                    
                    if (allCultures.Count == 0)
                        return CommandBase.FormatErrorMessage("No cultures found in game data.");
                    
                    // Select random culture
                    var randomCulture = allCultures[random.Next(allCultures.Count)];
                    
                    // Get lord templates matching gender and the random culture
                    var lordTemplates = CharacterObject.All
                        .Where(c => !c.IsHero &&
                                  c.Occupation == Occupation.Lord &&
                                  c.IsFemale == isFemale &&
                                  c.Culture == randomCulture)
                        .ToList();

                    // If no templates for this culture, try any culture
                    if (lordTemplates.Count == 0)
                    {
                        lordTemplates = CharacterObject.All
                            .Where(c => !c.IsHero &&
                                      c.Occupation == Occupation.Lord &&
                                      c.IsFemale == isFemale &&
                                      c.Culture != null)
                            .ToList();
                    }

                    if (lordTemplates.Count == 0)
                        return CommandBase.FormatErrorMessage($"No {(isFemale ? "female" : "male")} lord templates found.");

                    var template = lordTemplates[random.Next(lordTemplates.Count)];

                    // Generate unique ID
                    int randomId = random.Next(10000, 99999);
                    string lordId = $"gm_lord_{clan.StringId}_{name.ToLower()}_{randomId}";

                    // Create hero age 20-24
                    int age = random.Next(20, 25);
                    Hero newLord = HeroCreator.CreateSpecialHero(
                        template,
                        clan.Leader?.CurrentSettlement ?? Settlement.All.FirstOrDefault(s => s.OwnerClan == clan),
                        clan,
                        null,
                        age
                    );

                    if (newLord == null)
                        return CommandBase.FormatErrorMessage("Failed to create hero.");

                    // Set name
                    newLord.SetName(new TaleWorlds.Localization.TextObject(name), new TaleWorlds.Localization.TextObject(name));

                    // Set as active lord
                    newLord.ChangeState(Hero.CharacterStates.Active);
                    newLord.SetNewOccupation(Occupation.Lord);
                    
                    // Randomize body properties for unique appearance (face and hair)
                    var bodyPropertiesMin = newLord.CharacterObject.GetBodyPropertiesMin();
                    var bodyPropertiesMax = newLord.CharacterObject.GetBodyPropertiesMax();
                    
                    // Generate random body properties within the character's min/max range
                    var randomBodyProperties = BodyProperties.GetRandomBodyProperties(
                        newLord.CharacterObject.Race,
                        isFemale,
                        bodyPropertiesMin,
                        bodyPropertiesMax,
                        (int)newLord.Age,
                        random.Next(),
                        null,  // Hair tags - use template defaults
                        null,  // Beard tags - automatic for males
                        null   // Tattoo tags - no tattoos
                    );
                    
                    // Apply randomized appearance using reflection - convert to StaticBodyProperties
                    var staticBodyProp = typeof(Hero).GetProperty("StaticBodyProperties");
                    if (staticBodyProp != null)
                    {
                        // Create StaticBodyProperties from BodyProperties using key parts
                        var staticBody = new StaticBodyProperties(
                            randomBodyProperties.KeyPart1,
                            randomBodyProperties.KeyPart2,
                            randomBodyProperties.KeyPart3,
                            randomBodyProperties.KeyPart4,
                            randomBodyProperties.KeyPart5,
                            randomBodyProperties.KeyPart6,
                            randomBodyProperties.KeyPart7,
                            randomBodyProperties.KeyPart8
                        );
                        staticBodyProp.SetValue(newLord, staticBody);
                    }

                    // Clear all equipment except civilian clothes
                    var equipment = newLord.BattleEquipment;
                    for (int i = 0; i < (int)EquipmentIndex.NumEquipmentSetSlots; i++)
                    {
                        equipment[(EquipmentIndex)i] = new EquipmentElement();
                    }

                    // Add basic civilian clothes
                    var civilianClothes = MBObjectManager.Instance.GetObjectTypeList<ItemObject>()
                        .Where(item => item.Culture == template.Culture &&
                                     item.Type == ItemObject.ItemTypeEnum.BodyArmor &&
                                     item.Tier == ItemObject.ItemTiers.Tier1 &&
                                     item.IsCivilian)
                        .FirstOrDefault();

                    if (civilianClothes != null)
                    {
                        equipment[EquipmentIndex.Body] = new EquipmentElement(civilianClothes);
                    }

                    return CommandBase.FormatSuccessMessage(
                        $"Created fresh lord '{newLord.Name}' (ID: {newLord.StringId}):\n" +
                        $"Age: {(int)newLord.Age} | Gender: {(isFemale ? "Female" : "Male")} | Clan: {clan.Name}\n" +
                        $"Level: 1 | Equipment: Minimal");
                }, "Failed to create lord");
            });
        }

        #endregion
    }
}