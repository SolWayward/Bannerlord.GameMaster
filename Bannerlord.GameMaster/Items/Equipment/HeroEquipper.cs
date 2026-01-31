using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Bannerlord.GameMaster.Common;
using Helpers;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.CharacterDevelopment;
using TaleWorlds.Core;
using TaleWorlds.Library;

namespace Bannerlord.GameMaster.Items
{
    /// <summary>
    /// Provides methods for equipping heroes with dynamically generated equipment
    /// based on their stats, culture, and weapon preferences.
    /// Analyzes hero combat skills to determine appropriate weapon loadouts.
    /// </summary>
    public class HeroEquipper
    {
        #region Fields

        private readonly EquipmentBuilder _equipmentBuilder;

        /// <summary>
        /// Minimum skill threshold for a skill to be considered significant.
        /// Skills below this value are ignored when determining weapon preferences.
        /// </summary>
        private const int MinimumSkillThreshold = 20;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the HeroEquipper class.
        /// </summary>
        public HeroEquipper()
        {
            _equipmentBuilder = new();
        }

        /// <summary>
        /// Initializes a new instance of the HeroEquipper class with a specific equipment builder.
        /// </summary>
        /// <param name="equipmentBuilder">The equipment builder to use for generating equipment.</param>
        public HeroEquipper(EquipmentBuilder equipmentBuilder)
        {
            _equipmentBuilder = equipmentBuilder;
        }

        #endregion


        /// MARK: EquipHeroByStats
        /// <summary>
        /// Equips a hero with equipment generated based on their stats and culture.
        /// This method analyzes the hero's skills to determine optimal weapon preferences
        /// and selects appropriate tier equipment.
        /// </summary>
        /// <param name="hero">The hero to equip.</param>
        /// <param name="tier">The target tier for equipment (0+). Native items are tier 0-6, mods may add higher. If -1, tier is calculated from hero level.</param>
        /// <param name="weaponPreferences">Flags indicating preferred weapon types. If None, preferences are derived from hero skills.</param>
        /// <param name="replaceBattleEquipment">Whether to replace the hero's battle equipment.</param>
        /// <param name="replaceCivilianEquipment">Whether to replace the hero's civilian equipment.</param>
        /// <returns>BLGMResult indicating success or failure with details.</returns>
        public BLGMResult EquipHeroByStats(
            Hero hero,
            int tier = -1,
            WeaponTypeFlags weaponPreferences = WeaponTypeFlags.None,
            bool replaceBattleEquipment = true,
            bool replaceCivilianEquipment = false)
        {
            // Validation
            if (hero == null)
            {
                return BLGMResult.Error("EquipHeroByStats() failed: hero cannot be null");
            }

            // Calculate tier from hero level if not specified
            int effectiveTier = tier >= 0 ? tier : CalculateTierFromLevel(hero.Level);
            int heroLevel = CalculateLevelFromTier(effectiveTier);

            // Derive weapon preferences from skills if not specified
            WeaponTypeFlags effectivePreferences = weaponPreferences;
            if (weaponPreferences == WeaponTypeFlags.None)
            {
                effectivePreferences = DeriveWeaponPreferencesFromSkills(hero);
            }

            // Determine if hero should be mounted based on riding skill
            bool isMounted = hero.GetSkillValue(DefaultSkills.Riding) >= 50;

            // Determine if hero should get a banner (level 10+)
            bool withBanner = heroLevel >= 10;

            // Generate and apply battle equipment
            if (replaceBattleEquipment)
            {
                Equipment battleEquipment = _equipmentBuilder.GetEquipmentSet(
                    hero,
                    hero.Culture,
                    heroLevel,
                    effectivePreferences,
                    withHorse: isMounted,
                    withBanner: withBanner,
                    isFemale: hero.IsFemale,
                    includeNeutralItems: true);

                EquipmentHelper.AssignHeroEquipmentFromEquipment(hero, battleEquipment);

                // Set formation class based on equipped items
                SetFormationClassFromEquipment(hero);
            }

            // Generate and apply civilian equipment
            if (replaceCivilianEquipment)
            {
                Equipment civilianEquipment = _equipmentBuilder.GetCivilianEquipmentSet(
                    hero,
                    hero.Culture,
                    heroLevel,
                    hero.IsFemale,
                    includeNeutralItems: true);

                EquipmentHelper.AssignHeroEquipmentFromEquipment(hero, civilianEquipment);
            }

            string mountInfo = isMounted ? " (mounted)" : "";
            return BLGMResult.Success($"Equipped {hero.Name} with tier {effectiveTier} equipment{mountInfo}");
        }

        /// MARK: EquipHero
        /// <summary>
        /// Equips a hero with equipment generated based on specified parameters.
        /// </summary>
        /// <param name="hero">The hero to equip.</param>
        /// <param name="culture">The culture to use for item selection. If null, uses hero's culture.</param>
        /// <param name="tier">The target tier for equipment (0+). Native items are tier 0-6, mods may add higher.</param>
        /// <param name="weaponPreferences">Flags indicating preferred weapon types.</param>
        /// <param name="isMounted">Whether to include horse and harness equipment.</param>
        /// <param name="replaceBattleEquipment">Whether to replace the hero's battle equipment.</param>
        /// <param name="replaceCivilianEquipment">Whether to replace the hero's civilian equipment.</param>
        /// <returns>BLGMResult indicating success or failure with details.</returns>
        public BLGMResult EquipHero(
            Hero hero,
            CultureObject culture,
            int tier,
            WeaponTypeFlags weaponPreferences,
            bool isMounted = false,
            bool replaceBattleEquipment = true,
            bool replaceCivilianEquipment = false)
        {
            // Validation
            if (hero == null)
            {
                return BLGMResult.Error("EquipHero() failed: hero cannot be null");
            }

            // Use hero's culture if none specified
            CultureObject effectiveCulture = culture ?? hero.Culture;

            // Ensure tier is non-negative (no upper limit - mods may add higher tier items)
            int effectiveTier = tier < 0 ? 0 : tier;
            int heroLevel = CalculateLevelFromTier(effectiveTier);

            // Ensure at least some weapon preference is set
            WeaponTypeFlags effectivePreferences = weaponPreferences;
            if (effectivePreferences == WeaponTypeFlags.None)
            {
                effectivePreferences = WeaponTypeFlags.AllOneHanded | WeaponTypeFlags.Shield;
            }

            // Determine if hero should get a banner (level 10+)
            bool withBanner = heroLevel >= 10;

            // Generate and apply battle equipment
            if (replaceBattleEquipment)
            {
                Equipment battleEquipment = _equipmentBuilder.GetEquipmentSet(
                    hero,
                    effectiveCulture,
                    heroLevel,
                    effectivePreferences,
                    withHorse: isMounted,
                    withBanner: withBanner,
                    isFemale: hero.IsFemale,
                    includeNeutralItems: true);

                EquipmentHelper.AssignHeroEquipmentFromEquipment(hero, battleEquipment);

                // Set formation class based on equipped items
                SetFormationClassFromEquipment(hero);
            }

            // Generate and apply civilian equipment
            if (replaceCivilianEquipment)
            {
                Equipment civilianEquipment = _equipmentBuilder.GetCivilianEquipmentSet(
                    hero,
                    effectiveCulture,
                    heroLevel,
                    hero.IsFemale,
                    includeNeutralItems: true);

                EquipmentHelper.AssignHeroEquipmentFromEquipment(hero, civilianEquipment);
            }

            string mountInfo = isMounted ? " (mounted)" : "";
            string cultureInfo = effectiveCulture != null ? $" [{effectiveCulture.StringId}]" : "";
            return BLGMResult.Success($"Equipped {hero.Name} with tier {effectiveTier}{cultureInfo} equipment{mountInfo}");
        }

        /// MARK: GenerateEquipment
        /// <summary>
        /// Generates equipment for a hero without applying it.
        /// Useful for previewing or modifying equipment before assignment.
        /// </summary>
        /// <param name="hero">The hero to generate equipment for.</param>
        /// <param name="tier">The target tier for equipment (0+). Native items are tier 0-6, mods may add higher. If -1, tier is calculated from hero level.</param>
        /// <param name="weaponPreferences">Flags indicating preferred weapon types. If None, preferences are derived from hero skills.</param>
        /// <param name="isCivilian">Whether to generate civilian equipment.</param>
        /// <returns>A new Equipment object populated with selected items, or null if hero is null.</returns>
        public Equipment GenerateEquipmentForHero(
            Hero hero,
            int tier = -1,
            WeaponTypeFlags weaponPreferences = WeaponTypeFlags.None,
            bool isCivilian = false)
        {
            // Validation
            if (hero == null)
            {
                BLGMResult.Error("GenerateEquipmentForHero() failed: hero cannot be null").Log();
                return null;
            }

            // Calculate effective tier
            int effectiveTier = tier >= 0 ? tier : CalculateTierFromLevel(hero.Level);
            int heroLevel = CalculateLevelFromTier(effectiveTier);

            // For civilian equipment, use the dedicated civilian method
            if (isCivilian)
            {
                return _equipmentBuilder.GetCivilianEquipmentSet(
                    hero,
                    hero.Culture,
                    heroLevel,
                    hero.IsFemale,
                    includeNeutralItems: true);
            }

            // Derive weapon preferences if not specified
            WeaponTypeFlags effectivePreferences = weaponPreferences;
            if (weaponPreferences == WeaponTypeFlags.None)
            {
                effectivePreferences = DeriveWeaponPreferencesFromSkills(hero);
            }

            // Determine if mounted
            bool isMounted = hero.GetSkillValue(DefaultSkills.Riding) >= 50;

            // Determine if hero should get a banner (level 10+)
            bool withBanner = heroLevel >= 10;

            // Generate battle equipment
            return _equipmentBuilder.GetEquipmentSet(
                hero,
                hero.Culture,
                heroLevel,
                effectivePreferences,
                withHorse: isMounted,
                withBanner: withBanner,
                isFemale: hero.IsFemale,
                includeNeutralItems: true);
        }


        #region Static Methods
        
        /// MARK: CalcTierFromLevel
        /// <summary>
        /// Calculates the appropriate equipment tier based on hero level.
        /// Returns tier 6 for high-level heroes (level 31+). For higher tiers from mods,
        /// specify the tier explicitly when calling equip methods.
        /// </summary>
        /// <param name="heroLevel">The hero's level.</param>
        /// <returns>An equipment tier value (0-6 for native levels, higher levels return 6 as base).</returns>
        public static int CalculateTierFromLevel(int heroLevel)
        {
            // Mapping: Level 1-5 = Tier 0, Level 6-10 = Tier 1, etc.
            // High-level heroes (31+) default to tier 6; specify higher tiers explicitly if needed
            if (heroLevel <= 5) return 0;
            if (heroLevel <= 10) return 1;
            if (heroLevel <= 15) return 2;
            if (heroLevel <= 20) return 3;
            if (heroLevel <= 25) return 4;
            if (heroLevel <= 30) return 5;
            return 6;
        }

        /// MARK: CalcLevelFromTier
        /// <summary>
        /// Calculates a representative hero level for a given tier.
        /// Used when tier is explicitly specified to get appropriate equipment.
        /// </summary>
        /// <param name="tier">The equipment tier (0+ - native items are 0-6, mods may add higher).</param>
        /// <returns>A hero level appropriate for the tier.</returns>
        public static int CalculateLevelFromTier(int tier)
        {
            // Return middle level for each tier range
            // For tiers 6+, return level 35 (high enough for any equipment)
            return tier switch
            {
                0 => 3,   // Level 1-5
                1 => 8,   // Level 6-10
                2 => 13,  // Level 11-15
                3 => 18,  // Level 16-20
                4 => 23,  // Level 21-25
                5 => 28,  // Level 26-30
                _ => 35   // Level 31+ (tier 6 or higher)
            };
        }

        /// MARK: WeaponPrefFromSkill
        /// <summary>
        /// Derives weapon type preferences from a hero's combat skills.
        /// Analyzes the hero's skill values to determine optimal weapon loadout.
        /// </summary>
        /// <param name="hero">The hero to analyze.</param>
        /// <returns>WeaponTypeFlags representing the hero's preferred weapon types.</returns>
        public static WeaponTypeFlags DeriveWeaponPreferencesFromSkills(Hero hero)
        {
            if (hero == null)
            {
                return WeaponTypeFlags.AllOneHanded | WeaponTypeFlags.Shield;
            }

            // Get all combat skill values
            List<KeyValuePair<SkillObject, int>> skillValues = GetHeroCombatSkillValues(hero);

            // Sort by skill value descending
            List<KeyValuePair<SkillObject, int>> sortedSkills = skillValues
                .Where(kvp => kvp.Value >= MinimumSkillThreshold)
                .OrderByDescending(kvp => kvp.Value)
                .ToList();

            // If no significant skills, default to one-handed + shield
            if (sortedSkills.Count == 0)
            {
                return WeaponTypeFlags.AllOneHanded | WeaponTypeFlags.Shield;
            }

            // Determine weapon flags from sorted skills
            bool isMounted = hero.GetSkillValue(DefaultSkills.Riding) >= 50;
            return DetermineWeaponFlagsFromSkills(sortedSkills, isMounted);
        }

        /// MARK: WeaponFlagFromSkill
        /// <summary>
        /// Determines weapon type flags based on sorted combat skills.
        /// </summary>
        /// <param name="sortedSkills">List of skill/value pairs sorted by descending value.</param>
        /// <param name="isMounted">Whether the hero is primarily mounted (affects polearm shield compatibility).</param>
        /// <returns>WeaponTypeFlags combining primary and secondary weapon preferences.</returns>
        public static WeaponTypeFlags DetermineWeaponFlagsFromSkills(
            List<KeyValuePair<SkillObject, int>> sortedSkills,
            bool isMounted = false)
        {
            if (sortedSkills == null || sortedSkills.Count == 0)
            {
                return WeaponTypeFlags.AllOneHanded | WeaponTypeFlags.Shield;
            }

            WeaponTypeFlags resultFlags = WeaponTypeFlags.None;

            // Get top skill (primary weapon)
            SkillObject topSkill = sortedSkills[0].Key;

            // Get second skill (secondary weapon) if available
            SkillObject secondSkill = sortedSkills.Count > 1 ? sortedSkills[1].Key : null;

            // Determine primary weapon loadout based on top skill
            if (topSkill == DefaultSkills.TwoHanded)
            {
                // Two-handed focus: No shield (two-handed weapons incompatible)
                resultFlags |= WeaponTypeFlags.AllTwoHanded;

                // Add secondary ranged if second skill is ranged
                if (secondSkill != null && IsRangedSkill(secondSkill))
                {
                    resultFlags |= GetRangedFlagsForSkill(secondSkill);
                }
            }
            
            else if (topSkill == DefaultSkills.OneHanded)
            {
                // One-handed focus: Include shield
                resultFlags |= WeaponTypeFlags.AllOneHanded | WeaponTypeFlags.Shield;

                // Add secondary ranged/throwing if applicable
                if (secondSkill != null)
                {
                    if (secondSkill == DefaultSkills.Throwing)
                    {
                        resultFlags |= WeaponTypeFlags.AllThrowing;
                    }
                    else if (IsRangedSkill(secondSkill))
                    {
                        resultFlags |= GetRangedFlagsForSkill(secondSkill);
                    }
                }
            }

            else if (topSkill == DefaultSkills.Polearm)
            {
                // Polearm focus: Shield if not mounted (one-handed polearms can use shield)
                resultFlags |= WeaponTypeFlags.AllPolearms;

                if (!isMounted)
                {
                    resultFlags |= WeaponTypeFlags.Shield;
                }

                // Add secondary ranged if applicable
                if (secondSkill != null && IsRangedSkill(secondSkill))
                {
                    resultFlags |= GetRangedFlagsForSkill(secondSkill);
                }
            }

            else if (topSkill == DefaultSkills.Bow)
            {
                // Bow focus: No shield (bow requires two hands)
                resultFlags |= WeaponTypeFlags.Bow;

                // Always need melee backup
                if (secondSkill != null && IsMeleeSkill(secondSkill))
                {
                    resultFlags |= GetMeleeFlagsForSkill(secondSkill);
                }
                else
                {
                    // Default to one-handed if no melee skill
                    resultFlags |= WeaponTypeFlags.AllOneHanded;
                }
            }

            else if (topSkill == DefaultSkills.Crossbow)
            {
                // Crossbow focus: Can have shield if secondary is one-handed
                resultFlags |= WeaponTypeFlags.Crossbow;

                if (secondSkill == DefaultSkills.OneHanded)
                {
                    resultFlags |= WeaponTypeFlags.AllOneHanded | WeaponTypeFlags.Shield;
                }
                else if (secondSkill != null && IsMeleeSkill(secondSkill))
                {
                    resultFlags |= GetMeleeFlagsForSkill(secondSkill);
                }
                else
                {
                    // Default melee backup with potential shield
                    resultFlags |= WeaponTypeFlags.AllOneHanded | WeaponTypeFlags.Shield;
                }
            }

            else if (topSkill == DefaultSkills.Throwing)
            {
                // Throwing focus: Always needs melee backup + shield
                resultFlags |= WeaponTypeFlags.AllThrowing | WeaponTypeFlags.AllOneHanded | WeaponTypeFlags.Shield;
            }

            // Ensure at least one melee weapon is always included
            if ((resultFlags & WeaponTypeFlags.AllMelee) == WeaponTypeFlags.None)
            {
                resultFlags |= WeaponTypeFlags.AllOneHanded;
            }

            return resultFlags;
        }

        /// MARK: HeroCombatSkillLvl
        /// <summary>
        /// Gets all combat skill values for a hero.
        /// </summary>
        /// <param name="hero">The hero to analyze.</param>
        /// <returns>List of skill/value pairs for all combat skills.</returns>
        public static List<KeyValuePair<SkillObject, int>> GetHeroCombatSkillValues(Hero hero)
        {
            List<KeyValuePair<SkillObject, int>> skills = new();

            if (hero == null)
            {
                return skills;
            }

            // Collect all combat skill values
            skills.Add(new(DefaultSkills.OneHanded, hero.GetSkillValue(DefaultSkills.OneHanded)));
            skills.Add(new(DefaultSkills.TwoHanded, hero.GetSkillValue(DefaultSkills.TwoHanded)));
            skills.Add(new(DefaultSkills.Polearm, hero.GetSkillValue(DefaultSkills.Polearm)));
            skills.Add(new(DefaultSkills.Bow, hero.GetSkillValue(DefaultSkills.Bow)));
            skills.Add(new(DefaultSkills.Crossbow, hero.GetSkillValue(DefaultSkills.Crossbow)));
            skills.Add(new(DefaultSkills.Throwing, hero.GetSkillValue(DefaultSkills.Throwing)));

            return skills;
        }

        /// MARK: IsRangedSkill
        /// <summary>
        /// Determines if a skill is a ranged combat skill.
        /// </summary>
        /// <param name="skill">The skill to check.</param>
        /// <returns>True if the skill is Bow, Crossbow, or Throwing; false otherwise.</returns>
        public static bool IsRangedSkill(SkillObject skill)
        {
            return skill == DefaultSkills.Bow ||
                   skill == DefaultSkills.Crossbow ||
                   skill == DefaultSkills.Throwing;
        }

        /// MARK: IsMeleeSkill
        /// <summary>
        /// Determines if a skill is a melee combat skill.
        /// </summary>
        /// <param name="skill">The skill to check.</param>
        /// <returns>True if the skill is OneHanded, TwoHanded, or Polearm; false otherwise.</returns>
        public static bool IsMeleeSkill(SkillObject skill)
        {
            return skill == DefaultSkills.OneHanded ||
                   skill == DefaultSkills.TwoHanded ||
                   skill == DefaultSkills.Polearm;
        }

        /// MARK: RangedFlagsForSkill
        /// <summary>
        /// Gets the weapon type flags corresponding to a ranged skill.
        /// </summary>
        /// <param name="skill">The ranged skill.</param>
        /// <returns>Appropriate WeaponTypeFlags for the ranged skill.</returns>
        private static WeaponTypeFlags GetRangedFlagsForSkill(SkillObject skill)
        {
            if (skill == DefaultSkills.Bow)
                return WeaponTypeFlags.Bow;
            if (skill == DefaultSkills.Crossbow)
                return WeaponTypeFlags.Crossbow;
            if (skill == DefaultSkills.Throwing)
                return WeaponTypeFlags.AllThrowing;

            return WeaponTypeFlags.None;
        }

        /// MARK: MeleeFlagsForSkill
        /// <summary>
        /// Gets the weapon type flags corresponding to a melee skill.
        /// </summary>
        /// <param name="skill">The melee skill.</param>
        /// <returns>Appropriate WeaponTypeFlags for the melee skill.</returns>
        private static WeaponTypeFlags GetMeleeFlagsForSkill(SkillObject skill)
        {
            if (skill == DefaultSkills.OneHanded)
                return WeaponTypeFlags.AllOneHanded;
            if (skill == DefaultSkills.TwoHanded)
                return WeaponTypeFlags.AllTwoHanded;
            if (skill == DefaultSkills.Polearm)
                return WeaponTypeFlags.AllPolearms;

            return WeaponTypeFlags.AllOneHanded;
        }

        #endregion

        #region Formation Class Assignment

        /// MARK: SetFormationClass
        /// <summary>
        /// Sets the hero's DefaultFormationClass based on their equipped items.
        /// Rules:
        /// - Mounted + melee primary -> Cavalry (HeavyCavalry if couching polearm + plate armor tier 5+)
        /// - Mounted + ranged primary -> HorseArcher
        /// - Unmounted + ranged primary -> Ranged
        /// - Unmounted + melee primary -> Infantry (HeavyInfantry if plate armor tier 5+)
        /// - 2 throwing stacks + no plate + unmounted -> Skirmisher
        /// - Fallback -> General
        /// </summary>
        /// <param name="hero">The hero whose formation class to set.</param>
        /// <returns>The assigned FormationClass.</returns>
        public static FormationClass SetFormationClassFromEquipment(Hero hero)
        {
            if (hero == null || hero.CharacterObject == null)
                return FormationClass.General;

            Equipment equipment = hero.BattleEquipment;
            if (equipment == null)
                return FormationClass.General;

            // Determine mounted status (actual equipment, not skill)
            bool isMounted = HasMountEquipped(equipment);

            // Determine primary weapon type from highest skill that has matching equipment
            SkillObject primarySkill = GetPrimaryWeaponSkillWithEquipment(hero, equipment);

            // Check armor conditions
            bool hasPlateArmor = HasPlateBodyArmor(equipment);
            bool hasCouchingPolearm = HasCouchingPolearmEquipped(equipment);
            int throwingStacks = CountThrowingWeaponStacks(equipment);

            FormationClass result;

            // Rule 5: Skirmisher (check first - specific condition)
            // 2 throwing stacks + no plate armor + not mounted
            if (throwingStacks >= 2 && !hasPlateArmor && !isMounted)
            {
                result = FormationClass.Skirmisher;
            }
            // Rule 1 & 2: Mounted formations
            else if (isMounted)
            {
                if (IsRangedSkill(primarySkill))
                {
                    // Rule 2: Mounted + ranged -> HorseArcher
                    result = FormationClass.HorseArcher;
                }
                else
                {
                    // Rule 1: Mounted + melee -> Cavalry or HeavyCavalry
                    if (hasCouchingPolearm && hasPlateArmor)
                        result = FormationClass.HeavyCavalry;
                    else
                        result = FormationClass.Cavalry;
                }
            }
            // Rule 3 & 4: Unmounted formations
            else
            {
                if (IsRangedSkill(primarySkill))
                {
                    // Rule 3: Unmounted + ranged -> Ranged
                    result = FormationClass.Ranged;
                }
                else
                {
                    // Rule 4: Unmounted + melee -> Infantry or HeavyInfantry
                    if (hasPlateArmor)
                        result = FormationClass.HeavyInfantry;
                    else
                        result = FormationClass.Infantry;
                }
            }

            // Apply to character using reflection (setter is protected)
            SetDefaultFormationClass(hero.CharacterObject, result);
            return result;
        }

        /// MARK: HasMountEquipped
        /// <summary>
        /// Checks if equipment has a mount (horse) equipped.
        /// </summary>
        /// <param name="equipment">The equipment to check.</param>
        /// <returns>True if a mount is equipped; false otherwise.</returns>
        private static bool HasMountEquipped(Equipment equipment)
        {
            EquipmentElement horseElement = equipment[EquipmentIndex.Horse];
            return !horseElement.IsEmpty && horseElement.Item != null && horseElement.Item.HasHorseComponent;
        }

        /// MARK: GetPrimarySkillWithEquip
        /// <summary>
        /// Gets the hero's primary weapon skill that has matching equipment.
        /// Returns the highest skill where the hero actually has a weapon of that type equipped.
        /// </summary>
        /// <param name="hero">The hero to analyze.</param>
        /// <param name="equipment">The equipment to check.</param>
        /// <returns>The skill object for the hero's primary weapon type.</returns>
        private static SkillObject GetPrimaryWeaponSkillWithEquipment(Hero hero, Equipment equipment)
        {
            List<KeyValuePair<SkillObject, int>> skills = GetHeroCombatSkillValues(hero)
                .Where(kvp => kvp.Value >= MinimumSkillThreshold)
                .OrderByDescending(kvp => kvp.Value)
                .ToList();

            foreach (KeyValuePair<SkillObject, int> skillPair in skills)
            {
                if (HasWeaponForSkill(equipment, skillPair.Key))
                    return skillPair.Key;
            }

            // Fallback to highest skill even without equipment
            return skills.Count > 0 ? skills[0].Key : DefaultSkills.OneHanded;
        }

        /// MARK: HasWeaponForSkill
        /// <summary>
        /// Checks if equipment contains a weapon matching the given skill.
        /// </summary>
        /// <param name="equipment">The equipment to check.</param>
        /// <param name="skill">The skill to match against.</param>
        /// <returns>True if a matching weapon is equipped; false otherwise.</returns>
        private static bool HasWeaponForSkill(Equipment equipment, SkillObject skill)
        {
            for (int i = 0; i <= (int)EquipmentIndex.Weapon3; i++)
            {
                EquipmentElement element = equipment[(EquipmentIndex)i];
                if (element.IsEmpty || element.Item == null || !element.Item.HasWeaponComponent)
                    continue;

                ItemObject.ItemTypeEnum itemType = element.Item.ItemType;

                if (skill == DefaultSkills.Bow && itemType == ItemObject.ItemTypeEnum.Bow)
                    return true;
                if (skill == DefaultSkills.Crossbow && itemType == ItemObject.ItemTypeEnum.Crossbow)
                    return true;
                if (skill == DefaultSkills.Throwing && itemType == ItemObject.ItemTypeEnum.Thrown)
                    return true;
                if (skill == DefaultSkills.OneHanded && IsOneHandedMeleeWeapon(element.Item))
                    return true;
                if (skill == DefaultSkills.TwoHanded && IsTwoHandedMeleeWeapon(element.Item))
                    return true;
                if (skill == DefaultSkills.Polearm && itemType == ItemObject.ItemTypeEnum.Polearm)
                    return true;
            }
            return false;
        }

        /// MARK: HasPlateBodyArmor
        /// <summary>
        /// Checks if body armor is plate material AND tier 5+.
        /// </summary>
        /// <param name="equipment">The equipment to check.</param>
        /// <returns>True if plate armor tier 5+ is equipped; false otherwise.</returns>
        private static bool HasPlateBodyArmor(Equipment equipment)
        {
            EquipmentElement bodyElement = equipment[EquipmentIndex.Body];
            if (bodyElement.IsEmpty || bodyElement.Item == null)
                return false;

            ItemObject bodyArmor = bodyElement.Item;

            // Must have armor component
            if (!bodyArmor.HasArmorComponent)
                return false;

            ArmorComponent armorComponent = bodyArmor.ArmorComponent;
            if (armorComponent == null)
                return false;

            // Check material type is Plate
            if (armorComponent.MaterialType != ArmorComponent.ArmorMaterialTypes.Plate)
                return false;

            // Check tier is 5 or higher
            if ((int)bodyArmor.Tier < 5)
                return false;

            return true;
        }

        /// MARK: HasCouchingPolearm
        /// <summary>
        /// Checks if equipment contains a couching polearm.
        /// </summary>
        /// <param name="equipment">The equipment to check.</param>
        /// <returns>True if a couching polearm is equipped; false otherwise.</returns>
        private static bool HasCouchingPolearmEquipped(Equipment equipment)
        {
            for (int i = 0; i <= (int)EquipmentIndex.Weapon3; i++)
            {
                EquipmentElement element = equipment[(EquipmentIndex)i];
                if (element.IsEmpty || element.Item == null)
                    continue;

                if (MountCompatibility.IsCouchablePolearm(element.Item))
                    return true;
            }
            return false;
        }

        /// MARK: CountThrowingStacks
        /// <summary>
        /// Counts the number of throwing weapon stacks in equipment.
        /// </summary>
        /// <param name="equipment">The equipment to check.</param>
        /// <returns>The number of throwing weapon stacks equipped.</returns>
        private static int CountThrowingWeaponStacks(Equipment equipment)
        {
            int count = 0;
            for (int i = 0; i <= (int)EquipmentIndex.Weapon3; i++)
            {
                EquipmentElement element = equipment[(EquipmentIndex)i];
                if (element.IsEmpty || element.Item == null)
                    continue;

                if (element.Item.ItemType == ItemObject.ItemTypeEnum.Thrown)
                    count++;
            }
            return count;
        }

        /// MARK: IsOneHandedMelee
        /// <summary>
        /// Checks if item is a one-handed melee weapon (not polearm, not ranged).
        /// </summary>
        /// <param name="item">The item to check.</param>
        /// <returns>True if one-handed melee weapon; false otherwise.</returns>
        private static bool IsOneHandedMeleeWeapon(ItemObject item)
        {
            if (item == null || !item.HasWeaponComponent)
                return false;

            ItemObject.ItemTypeEnum itemType = item.ItemType;
            return itemType == ItemObject.ItemTypeEnum.OneHandedWeapon;
        }

        /// MARK: IsTwoHandedMelee
        /// <summary>
        /// Checks if item is a two-handed melee weapon (not polearm, not ranged).
        /// </summary>
        /// <param name="item">The item to check.</param>
        /// <returns>True if two-handed melee weapon; false otherwise.</returns>
        private static bool IsTwoHandedMeleeWeapon(ItemObject item)
        {
            if (item == null || !item.HasWeaponComponent)
                return false;

            ItemObject.ItemTypeEnum itemType = item.ItemType;
            return itemType == ItemObject.ItemTypeEnum.TwoHandedWeapon;
        }

        /// MARK: SetDefaultFormationClass
        /// <summary>
        /// Sets the DefaultFormationClass property on a BasicCharacterObject using reflection.
        /// The setter is protected, so direct access is not possible from external classes.
        /// </summary>
        /// <param name="character">The character to modify.</param>
        /// <param name="formationClass">The formation class to set.</param>
        private static void SetDefaultFormationClass(BasicCharacterObject character, FormationClass formationClass)
        {
            if (character == null)
                return;

            PropertyInfo propertyInfo = typeof(BasicCharacterObject).GetProperty(
                "DefaultFormationClass",
                BindingFlags.Public | BindingFlags.Instance);

            if (propertyInfo != null && propertyInfo.CanWrite)
            {
                propertyInfo.SetValue(character, formationClass);
            }
            else
            {
                // Fallback: try to set the backing field directly
                FieldInfo fieldInfo = typeof(BasicCharacterObject).GetField(
                    "<DefaultFormationClass>k__BackingField",
                    BindingFlags.NonPublic | BindingFlags.Instance);

                if (fieldInfo != null)
                {
                    fieldInfo.SetValue(character, formationClass);
                }
                else
                {
                    BLGMResult.Error(
                        "SetDefaultFormationClass() failed: Could not find property or backing field",
                        new MissingFieldException("DefaultFormationClass")).Log();
                }
            }
        }

        #endregion
    }
}
