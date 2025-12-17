using System;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TaleWorlds.ObjectSystem;

namespace Bannerlord.GameMaster.Heroes
{
    public class HeroGenerator
    {
        private readonly Random _random;

        public HeroGenerator()
        {
            int seed = (int)DateTime.UtcNow.Ticks;
            _random = new Random(seed);
        }

        public HeroGenerator(int seed)
        {
            _random = new Random(seed);
        }

        /// <summary>
        /// Result of lord generation operation
        /// </summary>
        public class HeroGenerationResult
        {
            public bool Success { get; set; }
            public string ErrorMessage { get; set; }
            public List<(Hero hero, Clan clan)> CreatedLords { get; set; } = new List<(Hero, Clan)>();
        }

        /// <summary>
        /// Configuration for generating lords
        /// </summary>
        public class HeroGenerationConfig
        {
            public int Count { get; set; } = 1;
            public Clan TargetClan { get; set; }
            public int MinAge { get; set; } = 30;
            public int MaxAge { get; set; } = 40;
            public int MinLevel { get; set; } = 15;
            public int MaxLevel { get; set; } = 25;
            public ItemObject.ItemTiers MinArmorTier { get; set; } = ItemObject.ItemTiers.Tier4;
            public ItemObject.ItemTiers MinWeaponTier { get; set; } = ItemObject.ItemTiers.Tier3;
        }

        /// <summary>
        /// Configuration for creating a fresh lord
        /// </summary>
        public class HeroCreationConfig
        {
            public bool IsFemale { get; set; }
            public string Name { get; set; }
            public Clan TargetClan { get; set; }
            public int MinAge { get; set; } = 20;
            public int MaxAge { get; set; } = 24;
            public bool RandomizeAppearance { get; set; } = true;
            public bool AddCivilianClothes { get; set; } = true;
        }

        /// <summary>
        /// Configuration for spawning a wanderer
        /// </summary>
        public class WandererSpawnConfig
        {
            public Settlement TargetSettlement { get; set; }
            public int MinAge { get; set; } = 25;
            public int MaxAge { get; set; } = 35;
        }

        /// <summary>
        /// Generate multiple lords with random templates and good equipment
        /// </summary>
        public HeroGenerationResult GenerateHeroes(HeroGenerationConfig config)
        {
            var result = new HeroGenerationResult();

            // Get available noble/warrior templates
            var lordTemplates = CharacterObject.All
                .Where(c => !c.IsHero && c.Occupation == Occupation.Lord && c.Culture != null)
                .ToList();

            if (lordTemplates.Count == 0)
            {
                result.ErrorMessage = "No lord templates found in game data.";
                return result;
            }

            // Get available clans for random assignment
            var availableClans = Clan.All
                .Where(c => !c.IsEliminated && !c.IsBanditFaction && c.Leader != null)
                .ToList();

            if (availableClans.Count == 0)
            {
                result.ErrorMessage = "No available clans found.";
                return result;
            }

            var usedClans = new HashSet<Clan>();

            for (int i = 0; i < config.Count; i++)
            {
                // Select random template with random gender
                var genderFilteredTemplates = lordTemplates
                    .Where(t => t.IsFemale == (_random.Next(2) == 0))
                    .ToList();

                // Fall back to all templates if no matching gender found
                if (genderFilteredTemplates.Count == 0)
                    genderFilteredTemplates = lordTemplates;

                var template = genderFilteredTemplates[_random.Next(genderFilteredTemplates.Count)];

                // Determine clan for this lord
                Clan assignedClan;
                if (config.TargetClan != null)
                {
                    assignedClan = config.TargetClan;
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
                    assignedClan = unusedClans[_random.Next(unusedClans.Count)];
                    usedClans.Add(assignedClan);
                }

                // Create the hero
                var hero = CreateHero(template, assignedClan, config.MinAge, config.MaxAge);
                if (hero == null)
                    continue;

                // Give decent stats
                ApplyLevelAndStats(hero, config.MinLevel, config.MaxLevel);

                // Give good equipment
                EquipHeroWithGear(hero, template.Culture, config.MinArmorTier, config.MinWeaponTier);

                result.CreatedLords.Add((hero, assignedClan));
            }

            if (result.CreatedLords.Count == 0)
            {
                result.ErrorMessage = "Failed to create any lords.";
                return result;
            }

            result.Success = true;
            return result;
        }

        /// <summary>
        /// Create a fresh lord with minimal stats and equipment
        /// </summary>
        public HeroGenerationResult CreateFreshHero(HeroCreationConfig config)
        {
            var result = new HeroGenerationResult();

            if (string.IsNullOrWhiteSpace(config.Name))
            {
                result.ErrorMessage = "Name cannot be empty.";
                return result;
            }

            if (config.TargetClan == null)
            {
                result.ErrorMessage = "Target clan cannot be null.";
                return result;
            }

            // Get all cultures for random selection
            var allCultures = MBObjectManager.Instance.GetObjectTypeList<TaleWorlds.Core.BasicCultureObject>()
                .Where(c => c != null)
                .ToList();

            if (allCultures.Count == 0)
            {
                result.ErrorMessage = "No cultures found in game data.";
                return result;
            }

            // Select random culture
            var randomCulture = allCultures[_random.Next(allCultures.Count)];

            // Get lord templates matching gender and the random culture
            var lordTemplates = CharacterObject.All
                .Where(c => !c.IsHero &&
                          c.Occupation == Occupation.Lord &&
                          c.IsFemale == config.IsFemale &&
                          c.Culture == randomCulture)
                .ToList();

            // If no templates for this culture, try any culture
            if (lordTemplates.Count == 0)
            {
                lordTemplates = CharacterObject.All
                    .Where(c => !c.IsHero &&
                              c.Occupation == Occupation.Lord &&
                              c.IsFemale == config.IsFemale &&
                              c.Culture != null)
                    .ToList();
            }

            if (lordTemplates.Count == 0)
            {
                result.ErrorMessage = $"No {(config.IsFemale ? "female" : "male")} lord templates found.";
                return result;
            }

            var template = lordTemplates[_random.Next(lordTemplates.Count)];

            // Create hero
            var hero = CreateHero(template, config.TargetClan, config.MinAge, config.MaxAge);
            if (hero == null)
            {
                result.ErrorMessage = "Failed to create hero.";
                return result;
            }

            // Set name
            hero.SetName(new TaleWorlds.Localization.TextObject(config.Name), new TaleWorlds.Localization.TextObject(config.Name));

            // Randomize appearance if requested
            if (config.RandomizeAppearance)
            {
                RandomizeHeroAppearance(hero, config.IsFemale);
            }

            // Clear all equipment
            ClearEquipment(hero);

            // Add basic civilian clothes if requested
            if (config.AddCivilianClothes)
            {
                AddCivilianClothes(hero, template.Culture);
            }

            result.CreatedLords.Add((hero, config.TargetClan));
            result.Success = true;
            return result;
        }

        /// <summary>
        /// Spawn a wanderer hero in a settlement
        /// </summary>
        public HeroGenerationResult SpawnWanderer(WandererSpawnConfig config)
        {
            var result = new HeroGenerationResult();

            if (config.TargetSettlement == null)
            {
                result.ErrorMessage = "Target settlement cannot be null.";
                return result;
            }

            if (!config.TargetSettlement.IsTown && !config.TargetSettlement.IsCastle)
            {
                result.ErrorMessage = $"Settlement '{config.TargetSettlement.Name}' must be a city or castle to spawn wanderers.";
                return result;
            }

            // Get all wanderer templates and select a random one
            var wandererTemplates = CharacterObject.All
                .Where(c => c.Occupation == Occupation.Wanderer && !c.IsHero)
                .ToList();

            if (wandererTemplates.Count == 0)
            {
                result.ErrorMessage = "No wanderer templates found in game data.";
                return result;
            }

            // Select a random wanderer template
            var wandererTemplate = wandererTemplates[_random.Next(wandererTemplates.Count)];

            // Create unique ID for the wanderer
            int randomId = _random.Next(10000, 99999);
            string wandererId = $"gm_wanderer_{config.TargetSettlement.StringId}_{CampaignTime.Now.GetYear}_{randomId}";

            // Determine age
            int age = _random.Next(config.MinAge, config.MaxAge + 1);

            // Create the hero using the proper creation method
            Hero wanderer = HeroCreator.CreateSpecialHero(
                wandererTemplate,
                config.TargetSettlement,
                null,  // clan
                null,  // supporterOf
                age
            );

            if (wanderer == null)
            {
                result.ErrorMessage = "Failed to create wanderer hero.";
                return result;
            }

            // Ensure wanderer has proper initialization
            wanderer.ChangeState(Hero.CharacterStates.Active);
            wanderer.SetNewOccupation(Occupation.Wanderer);

            // Make sure wanderer stays in settlement
            TaleWorlds.CampaignSystem.Actions.EnterSettlementAction.ApplyForCharacterOnly(wanderer, config.TargetSettlement);

            result.CreatedLords.Add((wanderer, null));
            result.Success = true;
            return result;
        }

        /// <summary>
        /// Create a lord hero with base settings
        /// </summary>
        private Hero CreateHero(CharacterObject template, Clan clan, int minAge, int maxAge)
        {
            // Generate unique ID
            int randomId = _random.Next(10000, 99999);
            string lordId = $"gm_lord_{clan.StringId}_{CampaignTime.Now.GetYear}_{randomId}";

            // Create hero with specified age range
            int age = _random.Next(minAge, maxAge + 1);
            Hero newLord = HeroCreator.CreateSpecialHero(
                template,
                clan.Leader?.CurrentSettlement ?? Settlement.All.FirstOrDefault(s => s.OwnerClan == clan),
                clan,
                null,
                age
            );

            if (newLord == null)
                return null;

            // Set as active lord
            newLord.ChangeState(Hero.CharacterStates.Active);
            newLord.SetNewOccupation(Occupation.Lord);

            return newLord;
        }

        /// <summary>
        /// Apply levels and stats to a hero
        /// </summary>
        private void ApplyLevelAndStats(Hero hero, int minLevel, int maxLevel)
        {
            int targetLevel = _random.Next(minLevel, maxLevel + 1);
            for (int level = 1; level < targetLevel; level++)
            {
                hero.HeroDeveloper.AddFocus(
                    DefaultSkills.OneHanded,
                    1,
                    false
                );
            }
        }

        /// <summary>
        /// Equip a lord with good armor and weapons
        /// </summary>
        private void EquipHeroWithGear(Hero hero, BasicCultureObject culture, ItemObject.ItemTiers minArmorTier, ItemObject.ItemTiers minWeaponTier)
        {
            var equipment = hero.BattleEquipment;

            // Find and equip armor based on culture
            var armorItems = MBObjectManager.Instance.GetObjectTypeList<ItemObject>()
                .Where(item => item.Culture == culture &&
                             (item.Type == ItemObject.ItemTypeEnum.BodyArmor ||
                              item.Type == ItemObject.ItemTypeEnum.HeadArmor ||
                              item.Type == ItemObject.ItemTypeEnum.LegArmor ||
                              item.Type == ItemObject.ItemTypeEnum.HandArmor ||
                              item.Type == ItemObject.ItemTypeEnum.Cape) &&
                             item.Tier >= minArmorTier)
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
                .Where(item => item.Culture == culture &&
                             item.Type == ItemObject.ItemTypeEnum.OneHandedWeapon &&
                             item.Tier >= minWeaponTier)
                .ToList();

            if (weapons.Count > 0)
            {
                var weapon = weapons[_random.Next(weapons.Count)];
                if (equipment[EquipmentIndex.Weapon0].IsEmpty)
                {
                    equipment[EquipmentIndex.Weapon0] = new EquipmentElement(weapon);
                }
            }
        }

        /// <summary>
        /// Randomize hero appearance
        /// </summary>
        private void RandomizeHeroAppearance(Hero hero, bool isFemale)
        {
            var bodyPropertiesMin = hero.CharacterObject.GetBodyPropertiesMin();
            var bodyPropertiesMax = hero.CharacterObject.GetBodyPropertiesMax();

            // Generate random body properties within the character's min/max range
            var randomBodyProperties = BodyProperties.GetRandomBodyProperties(
                hero.CharacterObject.Race,
                isFemale,
                bodyPropertiesMin,
                bodyPropertiesMax,
                (int)hero.Age,
                _random.Next(),
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
                staticBodyProp.SetValue(hero, staticBody);
            }
        }

        /// <summary>
        /// Clear all equipment from a hero
        /// </summary>
        private void ClearEquipment(Hero hero)
        {
            var equipment = hero.BattleEquipment;
            for (int i = 0; i < (int)EquipmentIndex.NumEquipmentSetSlots; i++)
            {
                equipment[(EquipmentIndex)i] = new EquipmentElement();
            }
        }

        /// <summary>
        /// Add basic civilian clothes to a hero
        /// </summary>
        private void AddCivilianClothes(Hero hero, BasicCultureObject culture)
        {
            var equipment = hero.BattleEquipment;
            var civilianClothes = MBObjectManager.Instance.GetObjectTypeList<ItemObject>()
                .Where(item => item.Culture == culture &&
                             item.Type == ItemObject.ItemTypeEnum.BodyArmor &&
                             item.Tier == ItemObject.ItemTiers.Tier1 &&
                             item.IsCivilian)
                .FirstOrDefault();

            if (civilianClothes != null)
            {
                equipment[EquipmentIndex.Body] = new EquipmentElement(civilianClothes);
            }
        }
    }
}
