using System.Collections.Generic;
using Bannerlord.GameMaster.Common;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Library;

namespace Bannerlord.GameMaster.Items
{
    /// <summary>
    /// Builds equipment sets for heroes by selecting items from categorized pools
    /// based on culture, tier, stats, and weapon preferences.
    /// </summary>
    public class EquipmentBuilder
    {
        #region Fields

        private readonly ItemPoolManager _poolManager;
        private readonly RandomNumberGen _random;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the EquipmentBuilder class.
        /// </summary>
        public EquipmentBuilder()
        {
            _poolManager = ItemPoolManager.Instance;
            _random = RandomNumberGen.Instance;
        }

        /// <summary>
        /// Initializes a new instance of the EquipmentBuilder class with a specific pool manager.
        /// </summary>
        /// <param name="poolManager">The item pool manager to use for equipment selection.</param>
        public EquipmentBuilder(ItemPoolManager poolManager)
        {
            _poolManager = poolManager;
            _random = RandomNumberGen.Instance;
        }

        #endregion
        
        /// MARK: WeaponLoadout
        /// <summary>
        /// Represents a weapon loadout plan for equipment generation.
        /// Determines what types of weapons go in which slots based on allowed flags.
        /// </summary>
        public struct WeaponLoadout
        {
            /// <summary>Primary melee weapon type flags (one-handed, two-handed, or polearm).</summary>
            public WeaponTypeFlags PrimaryMeleeType;

            /// <summary>Ranged weapon type (Bow, Crossbow, or None). Mutually exclusive.</summary>
            public WeaponTypeFlags RangedType;

            /// <summary>Throwing weapon type flags if throwing is allowed.</summary>
            public WeaponTypeFlags ThrowingType;

            /// <summary>Whether a shield should be included.</summary>
            public bool IncludeShield;

            /// <summary>Whether the primary weapon is two-handed (prevents shield).</summary>
            public bool IsTwoHanded;

            /// <summary>Whether ranged is the primary skill (goes in slot 0).</summary>
            public bool RangedIsPrimary;

            /// <summary>Number of throwing weapon slots needed (0-2).</summary>
            public int ThrowingSlots;

            /// <summary>Number of ammo slots needed for ranged weapons (1-2).</summary>
            public int AmmoSlots;
        }

        /// MARK: GetEquipmentSet
        /// <summary>
        /// Generates a complete equipment set based on the specified parameters.
        /// </summary>
        /// <param name="culture">The culture to use for item selection. If null, uses neutral items.</param>
        /// <param name="heroLevel">The hero's level used to determine tier range.</param>
        /// <param name="weaponPreferences">Flags indicating preferred weapon types.</param>
        /// <param name="withHorse">Whether to include horse and harness equipment.</param>
        /// <param name="withBanner">Whether to include a banner in slot 4.</param>
        /// <param name="isFemale">Whether the hero is female (affects armor filtering).</param>
        /// <param name="includeNeutralItems">Whether to also check neutral item pools.</param>
        /// <returns>A new Equipment object populated with selected items.</returns>
        public Equipment GetEquipmentSet(
            CultureObject culture,
            int heroLevel,
            WeaponTypeFlags weaponPreferences,
            bool withHorse = false,
            bool withBanner = false,
            bool isFemale = false,
            bool includeNeutralItems = true)
        {
            // Ensure pool manager is initialized
            if (!_poolManager.IsInitialized)
            {
                _poolManager.Initialize();
            }

            // Create new Equipment object (battle type)
            Equipment equipment = new(Equipment.EquipmentType.Battle);

            // Get tier range using ItemValidation
            (ItemObject.ItemTiers minTier, ItemObject.ItemTiers maxTier) = ItemValidation.GetTierRangeForLevel(heroLevel);

            // Determine weapon loadout based on allowed weapon flags (default: ranged takes priority)
            WeaponLoadout loadout = DetermineWeaponLoadout(weaponPreferences, rangedTakesPriority: true);

            // Fill weapon slots (0-3)
            FillWeaponSlots(equipment, culture, minTier, maxTier, loadout, withHorse, includeNeutralItems);

            // Fill banner slot (4) if requested
            if (withBanner)
            {
                FillBannerSlot(equipment, culture);
            }

            // Fill armor slots (5-9: Head, Cape, Body, Gloves, Boots)
            FillArmorSlots(equipment, culture, heroLevel, minTier, maxTier, isFemale, includeNeutralItems, null);

            // Fill horse slots (10-11) if requested
            if (withHorse)
            {
                FillHorseSlots(equipment, culture, minTier, maxTier, includeNeutralItems);
            }

            return equipment;
        }

        /// MARK: GetEquipmentSet (Hero overload)
        /// <summary>
        /// Generates a complete equipment set based on the specified parameters.
        /// Uses the hero's skills to determine ranged vs throwing priority when both are specified.
        /// For heroes level 10+, selects skill-appropriate banners when withBanner is true.
        /// </summary>
        /// <param name="hero">The hero to use for skill-based priority determination.</param>
        /// <param name="culture">The culture to use for item selection. If null, uses neutral items.</param>
        /// <param name="heroLevel">The hero's level used to determine tier range.</param>
        /// <param name="weaponPreferences">Flags indicating preferred weapon types.</param>
        /// <param name="withHorse">Whether to include horse and harness equipment.</param>
        /// <param name="withBanner">Whether to include a banner in slot 4 (only for level 10+).</param>
        /// <param name="isFemale">Whether the hero is female (affects armor filtering).</param>
        /// <param name="includeNeutralItems">Whether to also check neutral item pools.</param>
        /// <returns>A new Equipment object populated with selected items.</returns>
        public Equipment GetEquipmentSet(
            Hero hero,
            CultureObject culture,
            int heroLevel,
            WeaponTypeFlags weaponPreferences,
            bool withHorse = false,
            bool withBanner = false,
            bool isFemale = false,
            bool includeNeutralItems = true)
        {
            // Ensure pool manager is initialized
            if (!_poolManager.IsInitialized)
            {
                _poolManager.Initialize();
            }

            // Create new Equipment object (battle type)
            Equipment equipment = new(Equipment.EquipmentType.Battle);

            // Get tier range using ItemValidation
            (ItemObject.ItemTiers minTier, ItemObject.ItemTiers maxTier) = ItemValidation.GetTierRangeForLevel(heroLevel);

            // Determine ranged priority based on hero skills
            bool rangedTakesPriority = DetermineRangedPriority(hero, weaponPreferences);

            // Determine weapon loadout based on allowed weapon flags with skill-based priority
            WeaponLoadout loadout = DetermineWeaponLoadout(weaponPreferences, rangedTakesPriority);

            // Fill weapon slots (0-3)
            FillWeaponSlots(equipment, culture, minTier, maxTier, loadout, withHorse, includeNeutralItems);

            // Fill armor slots (5-9: Head, Cape, Body, Gloves, Boots) - BEFORE banner so we know equipment state
            FillArmorSlots(equipment, culture, heroLevel, minTier, maxTier, isFemale, includeNeutralItems, hero);

            // Fill horse slots (10-11) if requested - BEFORE banner so we know mount state
            if (withHorse)
            {
                FillHorseSlots(equipment, culture, minTier, maxTier, includeNeutralItems);
            }

            // Fill banner slot (4) if requested AND hero is level 10+
            // Banner selection uses skill-based logic from BannerSelector
            if (withBanner && heroLevel >= 10)
            {
                FillBannerSlotForHero(equipment, hero, heroLevel);
            }

            return equipment;
        }

        /// MARK: GetEquipmentForHero
        /// <summary>
        /// Generates a complete equipment set for a hero based on their stats and culture.
        /// </summary>
        /// <param name="hero">The hero to generate equipment for.</param>
        /// <param name="tier">The target tier for equipment (0-6). If -1, tier is calculated from hero level.</param>
        /// <param name="weaponPreferences">Flags indicating preferred weapon types. If None, preferences are derived from hero skills.</param>
        /// <param name="isCivilian">Whether to generate civilian equipment (no weapons/armor).</param>
        /// <returns>A new Equipment object populated with selected items.</returns>
        public Equipment GetEquipmentSetForHero(
            Hero hero,
            int tier = -1,
            WeaponTypeFlags weaponPreferences = WeaponTypeFlags.None,
            bool isCivilian = false)
        {
            if (hero == null)
            {
                BLGMResult.Error("GetEquipmentSetForHero() failed: hero cannot be null").Log();
                return new Equipment(isCivilian ? Equipment.EquipmentType.Civilian : Equipment.EquipmentType.Battle);
            }

            // Use hero's culture, level, and determine if mounted based on riding skill threshold
            CultureObject culture = hero.Culture;
            int heroLevel = hero.Level;
            bool withHorse = !isCivilian && hero.GetSkillValue(DefaultSkills.Riding) >= 50;
            bool isFemale = hero.IsFemale;

            // If no weapon preferences specified, derive from hero's combat skills
            if (weaponPreferences == WeaponTypeFlags.None)
            {
                weaponPreferences = HeroEquipper.DeriveWeaponPreferencesFromSkills(hero);
            }

            return GetEquipmentSet(hero, culture, heroLevel, weaponPreferences, withHorse, withBanner: false, isFemale);
        }

        /// MARK: DetermineRangedPriority
        /// <summary>
        /// Determines whether ranged weapons (bow/crossbow) should take priority over throwing weapons
        /// based on the hero's skill levels. When both ranged and throwing are specified in weapon preferences,
        /// compares the hero's bow/crossbow skill against throwing skill.
        /// </summary>
        /// <param name="hero">The hero whose skills determine priority. If null, returns true (ranged priority).</param>
        /// <param name="weaponPreferences">The weapon type flags to check for ranged/throwing conflict.</param>
        /// <returns>True if ranged skill >= throwing skill (tie goes to ranged), or if hero is null.</returns>
        private static bool DetermineRangedPriority(Hero hero, WeaponTypeFlags weaponPreferences)
        {
            // Default to ranged priority if no hero provided
            if (hero == null)
                return true;

            // Check if both ranged and throwing are specified
            bool hasRanged = (weaponPreferences & (WeaponTypeFlags.Bow | WeaponTypeFlags.Crossbow)) != 0;
            bool hasThrowing = (weaponPreferences & WeaponTypeFlags.AllThrowing) != 0;

            // If not both present, no conflict to resolve - default to ranged priority
            if (!hasRanged || !hasThrowing)
                return true;

            // Get skill values for comparison
            int bowSkill = hero.GetSkillValue(DefaultSkills.Bow);
            int crossbowSkill = hero.GetSkillValue(DefaultSkills.Crossbow);
            int throwingSkill = hero.GetSkillValue(DefaultSkills.Throwing);

            // Use max of bow/crossbow for ranged skill
            int rangedSkill = System.Math.Max(bowSkill, crossbowSkill);

            // Ranged takes priority if skill >= throwing (tie goes to ranged)
            return rangedSkill >= throwingSkill;
        }

        /// MARK: DetermineWeaponSet
        /// <summary>
        /// Analyzes weapon type flags to determine the weapon loadout plan.
        /// </summary>
        /// <param name="allowedWeapons">The allowed weapon type flags.</param>
        /// <param name="rangedTakesPriority">When both ranged and throwing are specified, if true ranged is primary; if false throwing is primary.</param>
        /// <returns>A WeaponLoadout structure containing the loadout plan.</returns>
        private WeaponLoadout DetermineWeaponLoadout(WeaponTypeFlags allowedWeapons, bool rangedTakesPriority = true)
        {
            WeaponLoadout loadout = new();

            // Determine primary melee weapon type
            if ((allowedWeapons & WeaponTypeFlags.AllTwoHanded) != 0)
            {
                loadout.PrimaryMeleeType = allowedWeapons & WeaponTypeFlags.AllTwoHanded;
                loadout.IsTwoHanded = true;
            }
            
            else if ((allowedWeapons & WeaponTypeFlags.AllPolearms) != 0)
            {
                loadout.PrimaryMeleeType = allowedWeapons & WeaponTypeFlags.AllPolearms;
                
                // Check if specifically two-handed polearm
                loadout.IsTwoHanded = (allowedWeapons & WeaponTypeFlags.TwoHandedPolearm) != 0;
            }
            
            else if ((allowedWeapons & WeaponTypeFlags.AllOneHanded) != 0)
            {
                // Check if any of the one-handed weapons include OneHandedPolearm
                // One-handed polearms still need sidearms, so treat them as polearms
                if ((allowedWeapons & WeaponTypeFlags.OneHandedPolearm) != 0)
                {
                    // Include polearm in primary type but also other one-handed for variety
                    loadout.PrimaryMeleeType = allowedWeapons & WeaponTypeFlags.AllOneHanded;
                    loadout.IsTwoHanded = false;
                    // NOTE: isPolearmPrimary check in FillMeleeOnlyLoadout will handle sidearm requirement
                }
                else
                {
                    loadout.PrimaryMeleeType = allowedWeapons & WeaponTypeFlags.AllOneHanded;
                    loadout.IsTwoHanded = false;
                }
            }
            
            else
            {
                // Default to one-handed sword if nothing specified
                loadout.PrimaryMeleeType = WeaponTypeFlags.OneHandedSword;
                loadout.IsTwoHanded = false;
            }

            // Determine ranged weapon type (mutually exclusive)
            if ((allowedWeapons & WeaponTypeFlags.Bow) != 0)
            {
                loadout.RangedType = WeaponTypeFlags.Bow;
                loadout.AmmoSlots = 2; // Bow typically gets 2 arrow stacks
            }
            
            else if ((allowedWeapons & WeaponTypeFlags.Crossbow) != 0)
            {
                loadout.RangedType = WeaponTypeFlags.Crossbow;
                loadout.AmmoSlots = 1; // Crossbow gets 1 bolt stack
            }
            
            else
            {
                loadout.RangedType = WeaponTypeFlags.None;
                loadout.AmmoSlots = 0;
            }

            // Determine throwing weapon type
            if ((allowedWeapons & WeaponTypeFlags.AllThrowing) != 0)
            {
                loadout.ThrowingType = allowedWeapons & WeaponTypeFlags.AllThrowing;
                loadout.ThrowingSlots = 2; // Typically 2 throwing weapon stacks
            }
            
            else
            {
                loadout.ThrowingType = WeaponTypeFlags.None;
                loadout.ThrowingSlots = 0;
            }

            // Determine if ranged is primary (goes in slot 0)
            // Ranged is primary if bow/crossbow is specified and either:
            // - No throwing is specified, OR
            // - Both are specified but ranged takes priority based on skills
            loadout.RangedIsPrimary = loadout.RangedType != WeaponTypeFlags.None &&
                (loadout.ThrowingType == WeaponTypeFlags.None || rangedTakesPriority);

            // Conflict resolution: When both ranged and throwing are present, keep one based on priority
            if (loadout.RangedType != WeaponTypeFlags.None && loadout.ThrowingType != WeaponTypeFlags.None)
            {
                if (rangedTakesPriority)
                {
                    // Remove throwing from loadout - ranged is primary
                    loadout.ThrowingType = WeaponTypeFlags.None;
                    loadout.ThrowingSlots = 0;
                }
                else
                {
                    // Remove ranged from loadout - throwing is primary
                    loadout.RangedType = WeaponTypeFlags.None;
                    loadout.AmmoSlots = 0;
                    loadout.RangedIsPrimary = false;
                }
            }

            // Determine if shield should be included
            // Shield only with one-handed weapons, never with two-handed
            loadout.IncludeShield = !loadout.IsTwoHanded && (allowedWeapons & WeaponTypeFlags.Shield) != 0;

            // If ranged is primary with bow, typically no shield (slot 1 is arrows)
            // But crossbow users can have shield in slot 3
            if (loadout.RangedIsPrimary && loadout.RangedType == WeaponTypeFlags.Bow)
            {
                loadout.IncludeShield = false;
            }

            return loadout;
        }

        /// MARK: FillWeaponSlots
        /// <summary>
        /// Fills weapon slots 0-3 based on the loadout plan.
        /// Slot 0: Primary weapon (NEVER throwing/shields/ammo)
        /// Slots 1-3: Secondary weapons, ammo, shields, throwing
        ///
        /// Key loadout rules enforced:
        /// - Polearms ALWAYS require a sidearm (sword/axe/mace)
        /// - Bows/Crossbows ALWAYS require matching ammo (only if weapon exists)
        /// - Multiple throwing weapons must be the same type
        /// - Bow + Polearm: bow, 1 arrow, polearm, sidearm
        /// - Bow + No Polearm: bow, 2 arrows, sidearm
        /// </summary>
        private void FillWeaponSlots(
            Equipment equipment,
            CultureObject culture,
            ItemObject.ItemTiers minTier,
            ItemObject.ItemTiers maxTier,
            WeaponLoadout loadout,
            bool isMounted,
            bool includeNeutralItems)
        {
            string cultureId = culture?.StringId;

            // MARK: Ranged
            // Slot 0 - Primary weapon (never throwing, shield, or ammo)
            if (loadout.RangedIsPrimary)
            {
                // Ranged weapon in slot 0 (Bow or Crossbow)
                ItemObject rangedWeapon = SelectWeaponByType(cultureId, minTier, maxTier, loadout.RangedType, isMounted, includeNeutralItems);
                if (rangedWeapon != null)
                {
                    equipment[EquipmentIndex.Weapon0] = new EquipmentElement(rangedWeapon);
                    
                    // ONLY add ammo if ranged weapon was successfully equipped
                    WeaponTypeFlags ammoType = loadout.RangedType == WeaponTypeFlags.Bow ? WeaponTypeFlags.Arrow : WeaponTypeFlags.Bolt;
                    ItemObject ammo1 = SelectAmmo(ammoType);
                    if (ammo1 != null)
                    {
                        equipment[EquipmentIndex.Weapon1] = new EquipmentElement(ammo1);
                    }
                    
                    // Check if primary melee is a polearm - affects slot layout
                    bool hasPolearm = (loadout.PrimaryMeleeType & WeaponTypeFlags.AllPolearms) != 0;
                    
                    if (hasPolearm)
                    {
                        // Bow + Polearm layout: bow, arrow, polearm, sidearm
                        // Slot 2 - Polearm (use specialized selection for mounted vs infantry)
                        ItemObject polearm = isMounted
                            ? SelectPolearmForMounted(cultureId, minTier, maxTier, includeNeutralItems)
                            : SelectPolearmForInfantry(cultureId, minTier, maxTier, includeNeutralItems);
                        if (polearm != null)
                        {
                            equipment[EquipmentIndex.Weapon2] = new EquipmentElement(polearm);
                        }
                        
                        // Slot 3 - SIDEARM (required with polearm)
                        ItemObject sidearm = SelectSidearmWeapon(cultureId, minTier, maxTier, isMounted, includeNeutralItems);
                        if (sidearm != null)
                        {
                            equipment[EquipmentIndex.Weapon3] = new EquipmentElement(sidearm);
                        }
                    }
                    else
                    {
                        // Bow + No Polearm layout: bow, arrow, sidearm, arrow OR shield
                        // Slot 2 - Melee weapon (sidearm)
                        ItemObject meleeWeapon = SelectWeaponByType(cultureId, minTier, maxTier, loadout.PrimaryMeleeType, isMounted, includeNeutralItems);
                        if (meleeWeapon != null)
                        {
                            equipment[EquipmentIndex.Weapon2] = new EquipmentElement(meleeWeapon);
                        }

                        // Slot 3 - Second arrow stack for bow, or shield for crossbow
                        if (loadout.RangedType == WeaponTypeFlags.Bow && loadout.AmmoSlots >= 2)
                        {
                            ItemObject ammo2 = SelectAmmo(WeaponTypeFlags.Arrow);
                            if (ammo2 != null)
                            {
                                equipment[EquipmentIndex.Weapon3] = new EquipmentElement(ammo2);
                            }
                        }
                        else if (loadout.RangedType == WeaponTypeFlags.Crossbow && loadout.IncludeShield)
                        {
                            ItemObject shield = SelectShield(cultureId, minTier, maxTier, includeNeutralItems);
                            if (shield != null)
                            {
                                equipment[EquipmentIndex.Weapon3] = new EquipmentElement(shield);
                            }
                        }
                    }
                }
                else
                {
                    // No ranged weapon found - fall back to melee loadout without ammo
                    // DON'T add ammo without a ranged weapon
                    FillMeleeOnlyLoadout(equipment, cultureId, minTier, maxTier, loadout, isMounted, includeNeutralItems);
                }
            }
            
            // MARK: Throwing
            else if (loadout.ThrowingType != WeaponTypeFlags.None)
            {
                // Check if primary melee is a polearm - affects slot layout (requires sidearm)
                bool hasPolearm = (loadout.PrimaryMeleeType & WeaponTypeFlags.AllPolearms) != 0;

                // Slot 0 - Primary melee weapon (use specialized polearm selection if polearm is primary)
                ItemObject meleeWeapon;
                if (hasPolearm)
                {
                    meleeWeapon = isMounted
                        ? SelectPolearmForMounted(cultureId, minTier, maxTier, includeNeutralItems)
                        : SelectPolearmForInfantry(cultureId, minTier, maxTier, includeNeutralItems);
                }
                else
                {
                    meleeWeapon = SelectWeaponByType(cultureId, minTier, maxTier, loadout.PrimaryMeleeType, isMounted, includeNeutralItems);
                }

                if (meleeWeapon != null)
                {
                    equipment[EquipmentIndex.Weapon0] = new EquipmentElement(meleeWeapon);
                }

                if (hasPolearm)
                {
                    // Polearm + Throwing layout: polearm, shield/sidearm, throwing, sidearm/throwing
                    if (loadout.IncludeShield)
                    {
                        // Slot 1 - Shield
                        ItemObject shield = SelectShield(cultureId, minTier, maxTier, includeNeutralItems);
                        if (shield != null)
                        {
                            equipment[EquipmentIndex.Weapon1] = new EquipmentElement(shield);
                        }
                        
                        // Slot 2 - Throwing weapon
                        ItemObject throwing1 = SelectWeaponByType(cultureId, minTier, maxTier, loadout.ThrowingType, isMounted, includeNeutralItems);
                        if (throwing1 != null)
                        {
                            equipment[EquipmentIndex.Weapon2] = new EquipmentElement(throwing1);
                        }
                        
                        // Slot 3 - SIDEARM (required with polearm)
                        ItemObject sidearm = SelectSidearmWeapon(cultureId, minTier, maxTier, isMounted, includeNeutralItems);
                        if (sidearm != null)
                        {
                            equipment[EquipmentIndex.Weapon3] = new EquipmentElement(sidearm);
                        }
                    }
                    else
                    {
                        // Slot 1 - SIDEARM (required with polearm, no shield)
                        ItemObject sidearm = SelectSidearmWeapon(cultureId, minTier, maxTier, isMounted, includeNeutralItems);
                        if (sidearm != null)
                        {
                            equipment[EquipmentIndex.Weapon1] = new EquipmentElement(sidearm);
                        }
                        
                        // Slot 2-3 - Throwing weapons (MUST be same type for both slots)
                        ItemObject throwing1 = SelectWeaponByType(cultureId, minTier, maxTier, loadout.ThrowingType, isMounted, includeNeutralItems);
                        if (throwing1 != null)
                        {
                            equipment[EquipmentIndex.Weapon2] = new EquipmentElement(throwing1);
                            equipment[EquipmentIndex.Weapon3] = new EquipmentElement(throwing1);
                        }
                    }
                }
                else
                {
                    // Non-polearm + Throwing layout: melee, shield, throwing, throwing
                    // Slot 1 - Shield if allowed
                    if (loadout.IncludeShield)
                    {
                        ItemObject shield = SelectShield(cultureId, minTier, maxTier, includeNeutralItems);
                        if (shield != null)
                        {
                            equipment[EquipmentIndex.Weapon1] = new EquipmentElement(shield);
                        }
                    }

                    // Slot 2-3 - Throwing weapons (MUST be same type for both slots)
                    ItemObject throwing1 = SelectWeaponByType(cultureId, minTier, maxTier, loadout.ThrowingType, isMounted, includeNeutralItems);
                    if (throwing1 != null)
                    {
                        equipment[EquipmentIndex.Weapon2] = new EquipmentElement(throwing1);
                        
                        // Use the SAME item for second throwing slot to ensure matching types
                        equipment[EquipmentIndex.Weapon3] = new EquipmentElement(throwing1);
                    }
                }
            }
            
            else
            {
                // MARK: Pure Melee
                FillMeleeOnlyLoadout(equipment, cultureId, minTier, maxTier, loadout, isMounted, includeNeutralItems);
            }
        }

        /// MARK: FillMeleeOnlyLoadout
        /// <summary>
        /// Fills weapon slots with melee-only loadout.
        /// Enforces sidearm requirement when polearm is primary.
        /// Uses specialized polearm selection to ensure mounted heroes get couchable polearms
        /// and infantry heroes get braceable polearms.
        /// </summary>
        private void FillMeleeOnlyLoadout(
            Equipment equipment,
            string cultureId,
            ItemObject.ItemTiers minTier,
            ItemObject.ItemTiers maxTier,
            WeaponLoadout loadout,
            bool isMounted,
            bool includeNeutralItems)
        {
            // Check if primary is a polearm - includes OneHandedPolearm which is in AllOneHanded
            // One-handed polearms still need sidearms just like two-handed polearms
            bool isPolearmPrimary = (loadout.PrimaryMeleeType & WeaponTypeFlags.AllPolearms) != 0;

            // Slot 0 - Primary melee weapon
            // For polearms, use specialized selection based on mounted vs infantry
            ItemObject primaryMelee;
            if (isPolearmPrimary)
            {
                primaryMelee = isMounted
                    ? SelectPolearmForMounted(cultureId, minTier, maxTier, includeNeutralItems)
                    : SelectPolearmForInfantry(cultureId, minTier, maxTier, includeNeutralItems);
            }
            else
            {
                primaryMelee = SelectWeaponByType(cultureId, minTier, maxTier, loadout.PrimaryMeleeType, isMounted, includeNeutralItems);
            }

            bool hasPrimaryWeapon = false;

            if (primaryMelee != null)
            {
                equipment[EquipmentIndex.Weapon0] = new EquipmentElement(primaryMelee);
                hasPrimaryWeapon = true;

                // Re-check if the actually selected weapon is a polearm
                // (in case PrimaryMeleeType had multiple options and we got a non-polearm)
                isPolearmPrimary = ItemValidation.IsPolearmWeapon(primaryMelee);
            }
            else
            {
                // FALLBACK: If no primary weapon found, try to get ANY one-handed weapon as fallback
                ItemObject fallbackWeapon = SelectSidearmWeapon(cultureId, minTier, maxTier, isMounted, includeNeutralItems);
                if (fallbackWeapon != null)
                {
                    equipment[EquipmentIndex.Weapon0] = new EquipmentElement(fallbackWeapon);
                    hasPrimaryWeapon = true;
                    // Since we used a sidearm as primary, treat this as non-polearm loadout
                    isPolearmPrimary = false;
                }
            }

            // Slot 1 - Shield if allowed and not two-handed
            // Sidearm is handled separately - shield and sidearm are NOT mutually exclusive for polearm users
            if (hasPrimaryWeapon && loadout.IncludeShield && !loadout.IsTwoHanded)
            {
                ItemObject shield = SelectShield(cultureId, minTier, maxTier, includeNeutralItems);
                if (shield != null)
                {
                    equipment[EquipmentIndex.Weapon1] = new EquipmentElement(shield);
                }
            }

            // Slot 2 - SIDEARM for polearm users (REQUIRED - polearms always need a backup weapon)
            // Or if no shield was added in slot 1, put sidearm there instead
            if (hasPrimaryWeapon && isPolearmPrimary)
            {
                ItemObject sidearm = SelectSidearmWeapon(cultureId, minTier, maxTier, isMounted, includeNeutralItems);
                if (sidearm != null)
                {
                    // Put sidearm in slot 2 if we have a shield in slot 1, otherwise slot 1
                    if (equipment[EquipmentIndex.Weapon1].Item != null)
                    {
                        equipment[EquipmentIndex.Weapon2] = new EquipmentElement(sidearm);
                    }
                    else
                    {
                        equipment[EquipmentIndex.Weapon1] = new EquipmentElement(sidearm);
                    }
                }
            }
            else if (hasPrimaryWeapon && !isPolearmPrimary)
            {
                // Non-polearm primary - add a polearm as backup in slot 2
                // Use specialized polearm selection based on mounted vs infantry
                ItemObject polearm = isMounted
                    ? SelectPolearmForMounted(cultureId, minTier, maxTier, includeNeutralItems)
                    : SelectPolearmForInfantry(cultureId, minTier, maxTier, includeNeutralItems);

                if (polearm != null)
                {
                    equipment[EquipmentIndex.Weapon2] = new EquipmentElement(polearm);

                    // Slot 3 - SIDEARM REQUIRED since we added a polearm
                    ItemObject sidearm = SelectSidearmWeapon(cultureId, minTier, maxTier, isMounted, includeNeutralItems);
                    if (sidearm != null)
                    {
                        equipment[EquipmentIndex.Weapon3] = new EquipmentElement(sidearm);
                    }
                }
            }
        }

        /// MARK: SelectSidearmWeapon
        /// <summary>
        /// Selects a sidearm weapon (one-handed sword, axe, or mace).
        /// Used when polearm is equipped to ensure hero has a backup melee weapon.
        /// Implements tier fallback to guarantee a sidearm is found.
        /// </summary>
        private ItemObject SelectSidearmWeapon(
            string cultureId,
            ItemObject.ItemTiers minTier,
            ItemObject.ItemTiers maxTier,
            bool isMounted,
            bool includeNeutralItems)
        {
            // Sidearms are one-handed weapons: sword, axe, or mace
            WeaponTypeFlags sidearmTypes = WeaponTypeFlags.OneHandedSword |
                                           WeaponTypeFlags.OneHandedAxe |
                                           WeaponTypeFlags.OneHandedMace;
            
            // First attempt: Try requested tier range
            ItemObject sidearm = SelectWeaponByType(cultureId, minTier, maxTier, sidearmTypes, isMounted, includeNeutralItems);
            if (sidearm != null)
                return sidearm;

            // TIER FALLBACK: Expand search to find ANY sidearm
            // Try lower tiers first
            for (int tier = (int)minTier - 1; tier >= 0; tier--)
            {
                sidearm = SelectWeaponByType(cultureId, (ItemObject.ItemTiers)tier, (ItemObject.ItemTiers)tier, sidearmTypes, isMounted, includeNeutralItems);
                if (sidearm != null)
                    return sidearm;
            }

            // Try higher tiers
            for (int tier = (int)maxTier + 1; tier <= (int)ItemObject.ItemTiers.Tier6; tier++)
            {
                sidearm = SelectWeaponByType(cultureId, (ItemObject.ItemTiers)tier, (ItemObject.ItemTiers)tier, sidearmTypes, isMounted, includeNeutralItems);
                if (sidearm != null)
                    return sidearm;
            }

            // Last resort: Try ALL one-handed weapons (including daggers, etc.)
            sidearm = SelectWeaponByType(cultureId, ItemObject.ItemTiers.Tier1, ItemObject.ItemTiers.Tier6, WeaponTypeFlags.AllOneHanded, isMounted, includeNeutralItems: true);
            if (sidearm != null)
                return sidearm;

            // Ultimate fallback: Try without mount compatibility filter
            return SelectWeaponByType(cultureId, ItemObject.ItemTiers.Tier1, ItemObject.ItemTiers.Tier6, WeaponTypeFlags.AllOneHanded, isMounted: false, includeNeutralItems: true);
        }

        #region Specialized Polearm Selection

        /// MARK: SelectPolearmForMounted
        /// <summary>
        /// Selects a polearm for mounted heroes with proper priority:
        /// 1. Couchable polearms in tier range (preferred for mounted)
        /// 2. Any mounted-usable polearm in tier range (non-braceable)
        /// 3. Tier fallback only if NO mounted-usable polearm found
        /// NEVER returns a braceable-only polearm for mounted heroes.
        /// </summary>
        private ItemObject SelectPolearmForMounted(
            string cultureId,
            ItemObject.ItemTiers minTier,
            ItemObject.ItemTiers maxTier,
            bool includeNeutralItems)
        {
            MBList<ItemObject> couchable = new();
            MBList<ItemObject> otherMountedUsable = new();

            // Collect from tier range
            CollectMountedPolearmsFromTierRange(cultureId, minTier, maxTier, includeNeutralItems,
                couchable, otherMountedUsable);

            // Priority 1: Couchable polearms in tier range
            if (couchable.Count > 0)
                return SelectRandomItem(couchable);

            // Priority 2: Any mounted-usable polearm in tier range
            if (otherMountedUsable.Count > 0)
                return SelectRandomItem(otherMountedUsable);

            // Priority 3: Tier fallback - expand search
            return SelectPolearmForMountedWithTierFallback(cultureId, minTier, maxTier, includeNeutralItems);
        }

        /// MARK: CollectMountedPolearmsFromTierRange
        /// <summary>
        /// Collects mounted-usable polearms from a tier range, separating couchable from other types.
        /// </summary>
        private void CollectMountedPolearmsFromTierRange(
            string cultureId,
            ItemObject.ItemTiers minTier,
            ItemObject.ItemTiers maxTier,
            bool includeNeutralItems,
            MBList<ItemObject> couchable,
            MBList<ItemObject> otherMountedUsable)
        {
            for (int tier = (int)minTier; tier <= (int)maxTier; tier++)
            {
                // Culture pool
                if (cultureId != null &&
                    _poolManager.WeaponPoolsByCulture.TryGetValue(cultureId,
                        out Dictionary<int, Dictionary<WeaponTypeFlags, MBList<ItemObject>>> cultureTiers) &&
                    cultureTiers.TryGetValue(tier, out Dictionary<WeaponTypeFlags, MBList<ItemObject>> cultureWeapons))
                {
                    CollectMountedPolearmsFromPool(cultureWeapons, couchable, otherMountedUsable);
                }

                // Neutral pool
                if (includeNeutralItems &&
                    _poolManager.NeutralWeaponPools.TryGetValue(tier,
                        out Dictionary<WeaponTypeFlags, MBList<ItemObject>> neutralWeapons))
                {
                    CollectMountedPolearmsFromPool(neutralWeapons, couchable, otherMountedUsable);
                }
            }
        }

        /// MARK: CollectMountedPolearmsFromPool
        /// <summary>
        /// Collects mounted-usable polearms from a weapon pool, categorizing by couchable vs other.
        /// </summary>
        private void CollectMountedPolearmsFromPool(
            Dictionary<WeaponTypeFlags, MBList<ItemObject>> weaponPools,
            MBList<ItemObject> couchable,
            MBList<ItemObject> otherMountedUsable)
        {
            foreach (KeyValuePair<WeaponTypeFlags, MBList<ItemObject>> kvp in weaponPools)
            {
                if ((kvp.Key & WeaponTypeFlags.AllPolearms) == 0)
                    continue;

                for (int i = 0; i < kvp.Value.Count; i++)
                {
                    ItemObject item = kvp.Value[i];

                    // Skip polearms that cannot be used on mount (braceable-only polearms)
                    if (!MountCompatibility.IsPolearmUsableOnMount(item))
                        continue;

                    if (MountCompatibility.IsCouchablePolearm(item))
                        couchable.Add(item);
                    else
                        otherMountedUsable.Add(item);
                }
            }
        }

        /// MARK: SelectPolearmForMountedWithTierFallback
        /// <summary>
        /// Tier fallback for mounted polearm selection - expands search to other tiers.
        /// </summary>
        private ItemObject SelectPolearmForMountedWithTierFallback(
            string cultureId,
            ItemObject.ItemTiers minTier,
            ItemObject.ItemTiers maxTier,
            bool includeNeutralItems)
        {
            MBList<ItemObject> couchable = new();
            MBList<ItemObject> otherMountedUsable = new();

            // Try lower tiers
            for (int tier = (int)minTier - 1; tier >= 0; tier--)
            {
                CollectMountedPolearmsFromTierRange(cultureId, (ItemObject.ItemTiers)tier,
                    (ItemObject.ItemTiers)tier, includeNeutralItems, couchable, otherMountedUsable);

                if (couchable.Count > 0)
                    return SelectRandomItem(couchable);
                if (otherMountedUsable.Count > 0)
                    return SelectRandomItem(otherMountedUsable);
            }

            // Try higher tiers
            for (int tier = (int)maxTier + 1; tier <= (int)ItemObject.ItemTiers.Tier6; tier++)
            {
                CollectMountedPolearmsFromTierRange(cultureId, (ItemObject.ItemTiers)tier,
                    (ItemObject.ItemTiers)tier, includeNeutralItems, couchable, otherMountedUsable);

                if (couchable.Count > 0)
                    return SelectRandomItem(couchable);
                if (otherMountedUsable.Count > 0)
                    return SelectRandomItem(otherMountedUsable);
            }

            return null;
        }

        /// MARK: SelectPolearmForInfantry
        /// <summary>
        /// Selects a polearm for infantry heroes with proper priority:
        /// 1. Braceable polearms in tier range (best for infantry anti-cavalry)
        /// 2. Any polearm in tier range (couchable is fine on foot too)
        /// 3. Tier fallback only if NO polearm found at all
        /// </summary>
        private ItemObject SelectPolearmForInfantry(
            string cultureId,
            ItemObject.ItemTiers minTier,
            ItemObject.ItemTiers maxTier,
            bool includeNeutralItems)
        {
            MBList<ItemObject> braceable = new();
            MBList<ItemObject> allPolearms = new();

            // Collect from tier range
            CollectInfantryPolearmsFromTierRange(cultureId, minTier, maxTier, includeNeutralItems,
                braceable, allPolearms);

            // Priority 1: Braceable polearms in tier range
            if (braceable.Count > 0)
                return SelectRandomItem(braceable);

            // Priority 2: Any polearm in tier range
            if (allPolearms.Count > 0)
                return SelectRandomItem(allPolearms);

            // Priority 3: Standard tier fallback
            return SelectWeaponByType(cultureId, minTier, maxTier, WeaponTypeFlags.AllPolearms,
                isMounted: false, includeNeutralItems);
        }

        /// MARK: CollectInfantryPolearmsFromTierRange
        /// <summary>
        /// Collects polearms for infantry from a tier range, separating braceable from other types.
        /// </summary>
        private void CollectInfantryPolearmsFromTierRange(
            string cultureId,
            ItemObject.ItemTiers minTier,
            ItemObject.ItemTiers maxTier,
            bool includeNeutralItems,
            MBList<ItemObject> braceable,
            MBList<ItemObject> allPolearms)
        {
            for (int tier = (int)minTier; tier <= (int)maxTier; tier++)
            {
                // Culture pool
                if (cultureId != null &&
                    _poolManager.WeaponPoolsByCulture.TryGetValue(cultureId,
                        out Dictionary<int, Dictionary<WeaponTypeFlags, MBList<ItemObject>>> cultureTiers) &&
                    cultureTiers.TryGetValue(tier, out Dictionary<WeaponTypeFlags, MBList<ItemObject>> cultureWeapons))
                {
                    CollectInfantryPolearmsFromPool(cultureWeapons, braceable, allPolearms);
                }

                // Neutral pool
                if (includeNeutralItems &&
                    _poolManager.NeutralWeaponPools.TryGetValue(tier,
                        out Dictionary<WeaponTypeFlags, MBList<ItemObject>> neutralWeapons))
                {
                    CollectInfantryPolearmsFromPool(neutralWeapons, braceable, allPolearms);
                }
            }
        }

        /// MARK: CollectInfantryPolearmsFromPool
        /// <summary>
        /// Collects polearms for infantry from a weapon pool, categorizing by braceable vs other.
        /// </summary>
        private void CollectInfantryPolearmsFromPool(
            Dictionary<WeaponTypeFlags, MBList<ItemObject>> weaponPools,
            MBList<ItemObject> braceable,
            MBList<ItemObject> allPolearms)
        {
            foreach (KeyValuePair<WeaponTypeFlags, MBList<ItemObject>> kvp in weaponPools)
            {
                if ((kvp.Key & WeaponTypeFlags.AllPolearms) == 0)
                    continue;

                for (int i = 0; i < kvp.Value.Count; i++)
                {
                    ItemObject item = kvp.Value[i];
                    allPolearms.Add(item);

                    if (MountCompatibility.IsBraceablePolearm(item))
                        braceable.Add(item);
                }
            }
        }

        #endregion

        /// MARK: FillArmorSlots
        /// <summary>
        /// Fills armor slots 5-9 (Head, Cape, Body, Gloves, Boots).
        /// For the Head slot, applies crown restrictions based on ruling clan membership.
        /// </summary>
        /// <param name="equipment">The equipment to fill.</param>
        /// <param name="culture">The culture to use for item selection.</param>
        /// <param name="heroLevel">The hero's level used for tier calculations.</param>
        /// <param name="minTier">The minimum item tier.</param>
        /// <param name="maxTier">The maximum item tier.</param>
        /// <param name="isFemale">Whether the hero is female.</param>
        /// <param name="includeNeutralItems">Whether to include neutral items.</param>
        /// <param name="hero">The hero to check for ruling clan membership (can be null).</param>
        private void FillArmorSlots(
            Equipment equipment,
            CultureObject culture,
            int heroLevel,
            ItemObject.ItemTiers minTier,
            ItemObject.ItemTiers maxTier,
            bool isFemale,
            bool includeNeutralItems,
            Hero hero)
        {
            string cultureId = culture?.StringId;
            bool isRulingClanMember = ItemValidation.IsRulingClanMember(hero);

            // Fill each armor slot
            EquipmentIndex[] armorSlots = new[]
            {
                EquipmentIndex.Head,    // 5
                EquipmentIndex.Cape,    // 6
                EquipmentIndex.Body,    // 7
                EquipmentIndex.Gloves,  // 8
                EquipmentIndex.Leg      // 9 (Boots)
            };

            foreach (EquipmentIndex slot in armorSlots)
            {
                ItemObject armor;
                
                if (slot == EquipmentIndex.Head)
                {
                    // Use specialized head armor selection with crown restrictions
                    armor = SelectBattleHeadArmor(cultureId, minTier, maxTier, heroLevel, isFemale, isRulingClanMember, includeNeutralItems);
                }
                else
                {
                    armor = SelectArmorBySlot(cultureId, minTier, maxTier, slot, heroLevel, isFemale, includeNeutralItems);
                }
                
                if (armor != null)
                {
                    equipment[slot] = new EquipmentElement(armor);
                }
            }
        }

        /// MARK: FillHorseSlots
        /// <summary>
        /// Fills horse slots 10-11 (Horse, HorseHarness).
        /// Horse is ALWAYS equipped if requested, harness MUST be found with tier fallback.
        /// </summary>
        private void FillHorseSlots(
            Equipment equipment,
            CultureObject culture,
            ItemObject.ItemTiers minTier,
            ItemObject.ItemTiers maxTier,
            bool includeNeutralItems)
        {
            string cultureId = culture?.StringId;

            // Slot 10 - Horse (use tier fallback to guarantee horse is found)
            ItemObject horse = SelectHorseWithFallback(cultureId, minTier, maxTier, includeNeutralItems);
            if (horse != null)
            {
                equipment[EquipmentIndex.Horse] = new EquipmentElement(horse);
                
                // Slot 11 - Horse Harness (MUST be equipped when horse is equipped)
                // Use tier fallback to GUARANTEE harness is found
                ItemObject harness = SelectHarnessWithFallback(cultureId, minTier, maxTier, includeNeutralItems);
                if (harness != null)
                {
                    equipment[EquipmentIndex.HorseHarness] = new EquipmentElement(harness);
                }
            }
        }



        /// <summary>
        /// Banner slot index (slot 4 in equipment).
        /// </summary>
        private const EquipmentIndex BannerSlotIndex = (EquipmentIndex)4;

        /// MARK: FillBannerSlot
        /// <summary>
        /// Fills banner slot 4 with a culture-appropriate banner.
        /// </summary>
        private void FillBannerSlot(Equipment equipment, CultureObject culture)
        {
            ItemObject banner = SelectBanner(culture?.StringId);
            if (banner != null)
            {
                equipment[BannerSlotIndex] = new EquipmentElement(banner);
            }
        }

        /// MARK: FillBannerSlotForHero
        /// <summary>
        /// Fills banner slot 4 with a skill-appropriate banner for the hero.
        /// Uses BannerSelector to determine the best banner based on hero's level and combat skills.
        /// Only called for heroes level 10+.
        /// </summary>
        /// <param name="equipment">The equipment to fill (should already have weapons/armor/horse populated).</param>
        /// <param name="hero">The hero to select a banner for.</param>
        /// <param name="heroLevel">The hero's level for tier calculation.</param>
        private void FillBannerSlotForHero(Equipment equipment, Hero hero, int heroLevel)
        {
            // Use BannerSelector to get skill-appropriate banner
            ItemObject banner = BannerSelector.SelectBannerForHero(hero, heroLevel, equipment);
            if (banner != null)
            {
                equipment[BannerSlotIndex] = new EquipmentElement(banner);
            }
        }

        /// MARK: SelectRandomItem
        /// <summary>
        /// Selects a random item from the given list.
        /// </summary>
        /// <param name="items">The list of items to select from.</param>
        /// <returns>A random item from the list, or null if the list is empty or null.</returns>
        private ItemObject SelectRandomItem(MBList<ItemObject> items)
        {
            if (items == null || items.Count == 0)
                return null;

            int index = _random.NextRandomInt(items.Count);
            return items[index];
        }

        /// MARK: SelectWeightedRandomItem
        /// <summary>
        /// Selects a random item from the given list with weighted probability favoring higher tiers.
        /// Higher tier items have a greater chance of being selected.
        /// Weight formula: tier + 1 (so Tier1=2, Tier2=3, ... Tier6=7)
        /// This gives roughly 3x more likely to pick Tier6 over Tier1.
        /// </summary>
        /// <param name="items">The list of items to select from.</param>
        /// <returns>A random item from the list (favoring higher tiers), or null if empty.</returns>
        private ItemObject SelectWeightedRandomItem(MBList<ItemObject> items)
        {
            if (items == null || items.Count == 0)
                return null;

            // If only one item, return it
            if (items.Count == 1)
                return items[0];

            // Calculate total weight
            int totalWeight = 0;
            for (int i = 0; i < items.Count; i++)
            {
                totalWeight += GetItemWeight(items[i]);
            }

            // Select based on weighted random
            int randomValue = _random.NextRandomInt(totalWeight);
            int cumulativeWeight = 0;

            for (int i = 0; i < items.Count; i++)
            {
                cumulativeWeight += GetItemWeight(items[i]);
                if (randomValue < cumulativeWeight)
                {
                    return items[i];
                }
            }

            // Fallback (shouldn't reach here)
            return items[items.Count - 1];
        }

        /// MARK: GetItemWeight
        /// <summary>
        /// Gets the selection weight for an item based on its tier.
        /// Higher tiers have higher weights for increased selection probability.
        /// </summary>
        private static int GetItemWeight(ItemObject item)
        {
            // Weight = tier value + 2
            // This makes Tier1 weight 3, Tier6 weight 8
            // Higher tiers are approximately 2.5x more likely than lowest tier
            return (int)item.Tier + 2;
        }

        /// MARK: SelectWeaponByType
        /// <summary>
        /// Selects a weapon matching the specified type from pools.
        /// Uses WEIGHTED selection favoring higher tiers within the range.
        /// </summary>
        private ItemObject SelectWeaponByType(
            string cultureId,
            ItemObject.ItemTiers minTier,
            ItemObject.ItemTiers maxTier,
            WeaponTypeFlags weaponType,
            bool isMounted,
            bool includeNeutralItems)
        {
            MBList<ItemObject> candidates = new();

            // Collect items from appropriate tiers
            for (int tier = (int)minTier; tier <= (int)maxTier; tier++)
            {
                // Try culture-specific pool first
                if (cultureId != null &&
                    _poolManager.WeaponPoolsByCulture.TryGetValue(cultureId, out Dictionary<int, Dictionary<WeaponTypeFlags, MBList<ItemObject>>> cultureTiers) &&
                    cultureTiers.TryGetValue(tier, out Dictionary<WeaponTypeFlags, MBList<ItemObject>> cultureWeapons))
                {
                    CollectMatchingWeapons(cultureWeapons, weaponType, isMounted, candidates);
                }

                // Also check neutral pool if enabled
                if (includeNeutralItems &&
                    _poolManager.NeutralWeaponPools.TryGetValue(tier, out Dictionary<WeaponTypeFlags, MBList<ItemObject>> neutralWeapons))
                {
                    CollectMatchingWeapons(neutralWeapons, weaponType, isMounted, candidates);
                }
            }

            // Use equal probability selection - all items in tier range have equal chance
            return SelectRandomItem(candidates);
        }

        /// MARK: CollectMatchWeapons
        /// <summary>
        /// Collects weapons matching the specified type flags into the candidates list.
        /// Uses IsPolearmUsableOnMount() for polearms to properly exclude braceable-only polearms
        /// from mounted heroes.
        /// </summary>
        private void CollectMatchingWeapons(
            Dictionary<WeaponTypeFlags, MBList<ItemObject>> weaponPools,
            WeaponTypeFlags targetType,
            bool isMounted,
            MBList<ItemObject> candidates)
        {
            foreach (KeyValuePair<WeaponTypeFlags, MBList<ItemObject>> kvp in weaponPools)
            {
                // Check if this weapon type matches any of the target flags
                if ((kvp.Key & targetType) != 0)
                {
                    for (int i = 0; i < kvp.Value.Count; i++)
                    {
                        ItemObject item = kvp.Value[i];

                        if (isMounted)
                        {
                            // For polearms, use specific mount usability check that excludes braceable-only polearms
                            if (item.ItemType == ItemObject.ItemTypeEnum.Polearm)
                            {
                                if (MountCompatibility.IsPolearmUsableOnMount(item))
                                    candidates.Add(item);
                            }
                            else
                            {
                                // For non-polearms, use standard mount check
                                if (MountCompatibility.IsWeaponUsableOnMount(item, allowPolearms: true))
                                    candidates.Add(item);
                            }
                        }
                        else
                        {
                            candidates.Add(item);
                        }
                    }
                }
            }
        }

        /// MARK: SelectArmorBySlot
        /// <summary>
        /// Selects armor for a specific slot matching culture and tier criteria.
        /// Uses WEIGHTED selection favoring higher tiers within the range.
        /// Implements tier fallback logic: if no items found in requested tier range,
        /// tries lower tiers first, then higher tiers to ensure slot gets filled.
        /// </summary>
        private ItemObject SelectArmorBySlot(
            string cultureId,
            ItemObject.ItemTiers minTier,
            ItemObject.ItemTiers maxTier,
            EquipmentIndex slot,
            int heroLevel,
            bool isFemale,
            bool includeNeutralItems)
        {
            MBList<ItemObject> candidates = new();
            bool excludeCloth = ItemValidation.ShouldExcludeClothArmor(heroLevel, slot);

            // First attempt: Try requested tier range
            CollectArmorFromTierRange(cultureId, (int)minTier, (int)maxTier, slot, isFemale, excludeCloth, includeNeutralItems, candidates);

            // If items found, return random selection (equal probability for all items in tier range)
            if (candidates.Count > 0)
            {
                return SelectRandomItem(candidates);
            }

            // TIER FALLBACK: No items found in requested range
            // First, try 1 tier lower (minTier-1), then 1 tier higher (maxTier+1)
            // This prevents jumping from expected Tier3-4 to Tier6 when Tier2 items exist
            
            // Try one tier lower first
            int oneTierLower = (int)minTier - 1;
            if (oneTierLower >= 0)
            {
                CollectArmorFromTierRange(cultureId, oneTierLower, oneTierLower, slot, isFemale, excludeCloth, includeNeutralItems, candidates);
                if (candidates.Count > 0)
                {
                    return SelectRandomItem(candidates);
                }
            }

            // Try one tier higher
            int oneTierHigher = (int)maxTier + 1;
            if (oneTierHigher <= (int)ItemObject.ItemTiers.Tier6)
            {
                CollectArmorFromTierRange(cultureId, oneTierHigher, oneTierHigher, slot, isFemale, excludeCloth, includeNeutralItems, candidates);
                if (candidates.Count > 0)
                {
                    return SelectRandomItem(candidates);
                }
            }

            // If still nothing found, expand search further - try remaining lower tiers
            for (int tier = oneTierLower - 1; tier >= 0; tier--)
            {
                CollectArmorFromTierRange(cultureId, tier, tier, slot, isFemale, excludeCloth, includeNeutralItems, candidates);
                if (candidates.Count > 0)
                {
                    return SelectRandomItem(candidates);
                }
            }

            // Finally try remaining higher tiers
            for (int tier = oneTierHigher + 1; tier <= (int)ItemObject.ItemTiers.Tier6; tier++)
            {
                CollectArmorFromTierRange(cultureId, tier, tier, slot, isFemale, excludeCloth, includeNeutralItems, candidates);
                if (candidates.Count > 0)
                {
                    return SelectRandomItem(candidates);
                }
            }

            // Last resort: Try without cloth exclusion if we had it enabled
            if (excludeCloth)
            {
                // Try all tiers without cloth exclusion
                for (int tier = 0; tier <= (int)ItemObject.ItemTiers.Tier6; tier++)
                {
                    CollectArmorFromTierRange(cultureId, tier, tier, slot, isFemale, excludeCloth: false, includeNeutralItems, candidates);
                    if (candidates.Count > 0)
                    {
                        return SelectRandomItem(candidates);
                    }
                }
            }

            return null;
        }

        /// MARK: CollectArmorFromTierRange
        /// <summary>
        /// Helper method to collect armor from a specific tier range into candidates list.
        /// </summary>
        private void CollectArmorFromTierRange(
            string cultureId,
            int minTier,
            int maxTier,
            EquipmentIndex slot,
            bool isFemale,
            bool excludeCloth,
            bool includeNeutralItems,
            MBList<ItemObject> candidates)
        {
            for (int tier = minTier; tier <= maxTier; tier++)
            {
                // Try culture-specific pool
                if (cultureId != null &&
                    _poolManager.ArmorPoolsBySlot.TryGetValue(cultureId, out Dictionary<int, Dictionary<EquipmentIndex, MBList<ItemObject>>> cultureTiers) &&
                    cultureTiers.TryGetValue(tier, out Dictionary<EquipmentIndex, MBList<ItemObject>> cultureArmor) &&
                    cultureArmor.TryGetValue(slot, out MBList<ItemObject> cultureItems))
                {
                    CollectValidArmor(cultureItems, isFemale, excludeCloth, candidates);
                }

                // Also check neutral pool if enabled
                if (includeNeutralItems &&
                    _poolManager.NeutralArmorPools.TryGetValue(tier, out Dictionary<EquipmentIndex, MBList<ItemObject>> neutralArmor) &&
                    neutralArmor.TryGetValue(slot, out MBList<ItemObject> neutralItems))
                {
                    CollectValidArmor(neutralItems, isFemale, excludeCloth, candidates);
                }
            }
        }

        /// MARK: SelectBattleHeadArmor
        /// <summary>
        /// Selects head armor for battle equipment with crown restrictions.
        /// Only ruling clan members can wear crowns in battle, and battle crowns must have
        /// "battle" or "helmet" in name (or be Jeweled Crown which works in both contexts).
        /// </summary>
        /// <param name="cultureId">The culture ID for item selection.</param>
        /// <param name="minTier">The minimum item tier.</param>
        /// <param name="maxTier">The maximum item tier.</param>
        /// <param name="heroLevel">The hero's level used for cloth armor exclusion.</param>
        /// <param name="isFemale">Whether the hero is female.</param>
        /// <param name="isRulingClan">Whether the hero is a member of the ruling clan.</param>
        /// <param name="includeNeutralItems">Whether to include neutral items.</param>
        /// <returns>A suitable head armor item, or null if none found.</returns>
        private ItemObject SelectBattleHeadArmor(
            string cultureId,
            ItemObject.ItemTiers minTier,
            ItemObject.ItemTiers maxTier,
            int heroLevel,
            bool isFemale,
            bool isRulingClan,
            bool includeNeutralItems)
        {
            MBList<ItemObject> candidates = new();
            bool excludeCloth = ItemValidation.ShouldExcludeClothArmor(heroLevel, EquipmentIndex.Head);

            // Collect items from appropriate tiers
            for (int tier = (int)minTier; tier <= (int)maxTier; tier++)
            {
                // Try culture-specific pool
                if (cultureId != null &&
                    _poolManager.ArmorPoolsBySlot.TryGetValue(cultureId, out Dictionary<int, Dictionary<EquipmentIndex, MBList<ItemObject>>> cultureTiers) &&
                    cultureTiers.TryGetValue(tier, out Dictionary<EquipmentIndex, MBList<ItemObject>> cultureArmor) &&
                    cultureArmor.TryGetValue(EquipmentIndex.Head, out MBList<ItemObject> cultureItems))
                {
                    CollectBattleHeadArmor(cultureItems, isFemale, excludeCloth, isRulingClan, candidates);
                }

                // Also check neutral pool if enabled
                if (includeNeutralItems &&
                    _poolManager.NeutralArmorPools.TryGetValue(tier, out Dictionary<EquipmentIndex, MBList<ItemObject>> neutralArmor) &&
                    neutralArmor.TryGetValue(EquipmentIndex.Head, out MBList<ItemObject> neutralItems))
                {
                    CollectBattleHeadArmor(neutralItems, isFemale, excludeCloth, isRulingClan, candidates);
                }
            }

            return SelectRandomItem(candidates);
        }

        /// MARK: CollectBattleHeadArmor
        /// <summary>
        /// Collects valid head armor items for battle equipment applying gender, cloth, and crown filters.
        /// Crown handling:
        /// - Non-ruling clan members cannot wear crowns
        /// - Ruling clan members can only wear battle crowns (with "battle" or "helmet" in name) or Jeweled Crown
        /// - Jeweled Crown is female-only
        /// </summary>
        /// <param name="items">The source list of head armor items.</param>
        /// <param name="isFemale">Whether the hero is female.</param>
        /// <param name="excludeCloth">Whether to exclude cloth armor.</param>
        /// <param name="isRulingClan">Whether the hero is a member of the ruling clan.</param>
        /// <param name="candidates">The output list to add valid items to.</param>
        private void CollectBattleHeadArmor(
            MBList<ItemObject> items,
            bool isFemale,
            bool excludeCloth,
            bool isRulingClan,
            MBList<ItemObject> candidates)
        {
            for (int i = 0; i < items.Count; i++)
            {
                ItemObject item = items[i];

                // Apply gender filter
                if (!ItemValidation.IsArmorSuitableForGender(item, isFemale))
                    continue;

                // Apply cloth filter
                if (excludeCloth && ItemValidation.IsClothArmor(item))
                    continue;

                // Handle crown items specially
                if (ItemValidation.IsCrownItem(item))
                {
                    // Non-ruling clan members cannot wear crowns in battle
                    if (!isRulingClan)
                        continue;

                    // For battle, must be a battle crown OR Jeweled Crown (exception for Jeweled Crown which works in both)
                    if (!ItemValidation.IsBattleCrown(item) && !ItemValidation.IsJeweledCrown(item))
                        continue;

                    // Jeweled Crown is female-only
                    if (ItemValidation.IsJeweledCrown(item) && !isFemale)
                        continue;
                }

                candidates.Add(item);
            }
        }

        /// MARK: CollectValidArmor
        /// <summary>
        /// Collects valid armor items applying gender and cloth filters.
        /// </summary>
        private void CollectValidArmor(
            MBList<ItemObject> items,
            bool isFemale,
            bool excludeCloth,
            MBList<ItemObject> candidates)
        {
            for (int i = 0; i < items.Count; i++)
            {
                ItemObject item = items[i];

                // Apply gender filter
                if (!ItemValidation.IsArmorSuitableForGender(item, isFemale))
                    continue;

                // Apply cloth filter
                if (excludeCloth && ItemValidation.IsClothArmor(item))
                    continue;

                candidates.Add(item);
            }
        }

        /// MARK: SelectShield
        /// <summary>
        /// Selects a shield matching culture and tier criteria.
        /// </summary>
        private ItemObject SelectShield(
            string cultureId,
            ItemObject.ItemTiers minTier,
            ItemObject.ItemTiers maxTier,
            bool includeNeutralItems)
        {
            MBList<ItemObject> candidates = new();

            for (int tier = (int)minTier; tier <= (int)maxTier; tier++)
            {
                // Culture-specific shields
                if (cultureId != null &&
                    _poolManager.ShieldPoolsByCulture.TryGetValue(cultureId, out Dictionary<int, MBList<ItemObject>> cultureTiers) &&
                    cultureTiers.TryGetValue(tier, out MBList<ItemObject> cultureShields))
                {
                    for (int i = 0; i < cultureShields.Count; i++)
                    {
                        candidates.Add(cultureShields[i]);
                    }
                }

                // Neutral shields
                if (includeNeutralItems &&
                    _poolManager.NeutralShieldPools.TryGetValue(tier, out MBList<ItemObject> neutralShields))
                {
                    for (int i = 0; i < neutralShields.Count; i++)
                    {
                        candidates.Add(neutralShields[i]);
                    }
                }
            }

            return SelectRandomItem(candidates);
        }

        /// MARK: SelectAmmo
        /// <summary>
        /// Selects ammunition of the specified type.
        /// </summary>
        private ItemObject SelectAmmo(WeaponTypeFlags ammoType)
        {
            if (_poolManager.AmmoPools.TryGetValue(ammoType, out MBList<ItemObject> ammoItems))
            {
                return SelectRandomItem(ammoItems);
            }
            return null;
        }

        /// MARK: SelectHorse
        /// <summary>
        /// Selects a horse matching culture and tier criteria.
        /// </summary>
        private ItemObject SelectHorse(
            string cultureId,
            ItemObject.ItemTiers minTier,
            ItemObject.ItemTiers maxTier,
            bool includeNeutralItems)
        {
            MBList<ItemObject> candidates = new();

            for (int tier = (int)minTier; tier <= (int)maxTier; tier++)
            {
                // Culture-specific horses
                if (cultureId != null &&
                    _poolManager.HorsePoolsByCulture.TryGetValue(cultureId, out Dictionary<int, MBList<ItemObject>> cultureTiers) &&
                    cultureTiers.TryGetValue(tier, out MBList<ItemObject> cultureHorses))
                {
                    for (int i = 0; i < cultureHorses.Count; i++)
                    {
                        candidates.Add(cultureHorses[i]);
                    }
                }

                // Neutral horses
                if (includeNeutralItems &&
                    _poolManager.NeutralHorsePools.TryGetValue(tier, out MBList<ItemObject> neutralHorses))
                {
                    for (int i = 0; i < neutralHorses.Count; i++)
                    {
                        candidates.Add(neutralHorses[i]);
                    }
                }
            }

            return SelectRandomItem(candidates);
        }

        /// MARK: SelectHarness
        /// <summary>
        /// Selects a horse harness matching culture and tier criteria.
        /// </summary>
        private ItemObject SelectHarness(
            string cultureId,
            ItemObject.ItemTiers minTier,
            ItemObject.ItemTiers maxTier,
            bool includeNeutralItems)
        {
            MBList<ItemObject> candidates = new();

            for (int tier = (int)minTier; tier <= (int)maxTier; tier++)
            {
                // Culture-specific harnesses
                if (cultureId != null &&
                    _poolManager.HarnessPoolsByCulture.TryGetValue(cultureId, out Dictionary<int, MBList<ItemObject>> cultureTiers) &&
                    cultureTiers.TryGetValue(tier, out MBList<ItemObject> cultureHarnesses))
                {
                    for (int i = 0; i < cultureHarnesses.Count; i++)
                    {
                        candidates.Add(cultureHarnesses[i]);
                    }
                }

                // Neutral harnesses
                if (includeNeutralItems &&
                    _poolManager.NeutralHarnessPools.TryGetValue(tier, out MBList<ItemObject> neutralHarnesses))
                {
                    for (int i = 0; i < neutralHarnesses.Count; i++)
                    {
                        candidates.Add(neutralHarnesses[i]);
                    }
                }
            }

            return SelectRandomItem(candidates);
        }

        /// MARK: SelectHorseWithFallback
        /// <summary>
        /// Selects a horse with tier fallback to guarantee a horse is found.
        /// </summary>
        private ItemObject SelectHorseWithFallback(
            string cultureId,
            ItemObject.ItemTiers minTier,
            ItemObject.ItemTiers maxTier,
            bool includeNeutralItems)
        {
            // First attempt: Try requested tier range
            ItemObject horse = SelectHorse(cultureId, minTier, maxTier, includeNeutralItems);
            if (horse != null)
                return horse;

            // TIER FALLBACK: Expand search to find ANY horse
            // Try lower tiers first
            for (int tier = (int)minTier - 1; tier >= 0; tier--)
            {
                horse = SelectHorse(cultureId, (ItemObject.ItemTiers)tier, (ItemObject.ItemTiers)tier, includeNeutralItems);
                if (horse != null)
                    return horse;
            }

            // Try higher tiers
            for (int tier = (int)maxTier + 1; tier <= (int)ItemObject.ItemTiers.Tier6; tier++)
            {
                horse = SelectHorse(cultureId, (ItemObject.ItemTiers)tier, (ItemObject.ItemTiers)tier, includeNeutralItems);
                if (horse != null)
                    return horse;
            }

            // Last resort: Try ALL tiers with neutral items forced
            return SelectHorse(cultureId, ItemObject.ItemTiers.Tier1, ItemObject.ItemTiers.Tier6, includeNeutralItems: true);
        }

        /// MARK: SelectHarnessWithFallback
        /// <summary>
        /// Selects a horse harness with tier fallback to GUARANTEE a harness is found.
        /// Horses ALWAYS require harness/armor/saddle.
        /// </summary>
        private ItemObject SelectHarnessWithFallback(
            string cultureId,
            ItemObject.ItemTiers minTier,
            ItemObject.ItemTiers maxTier,
            bool includeNeutralItems)
        {
            // First attempt: Try requested tier range
            ItemObject harness = SelectHarness(cultureId, minTier, maxTier, includeNeutralItems);
            if (harness != null)
                return harness;

            // TIER FALLBACK: Expand search to find ANY harness
            // Try lower tiers first
            for (int tier = (int)minTier - 1; tier >= 0; tier--)
            {
                harness = SelectHarness(cultureId, (ItemObject.ItemTiers)tier, (ItemObject.ItemTiers)tier, includeNeutralItems);
                if (harness != null)
                    return harness;
            }

            // Try higher tiers
            for (int tier = (int)maxTier + 1; tier <= (int)ItemObject.ItemTiers.Tier6; tier++)
            {
                harness = SelectHarness(cultureId, (ItemObject.ItemTiers)tier, (ItemObject.ItemTiers)tier, includeNeutralItems);
                if (harness != null)
                    return harness;
            }

            // Last resort: Try ALL tiers with neutral items forced
            return SelectHarness(cultureId, ItemObject.ItemTiers.Tier1, ItemObject.ItemTiers.Tier6, includeNeutralItems: true);
        }

        /// MARK: SelectBanner
        /// <summary>
        /// Selects a banner for the specified culture.
        /// </summary>
        private ItemObject SelectBanner(string cultureId)
        {
            // Try culture-specific banner first
            if (cultureId != null &&
                _poolManager.BannerPoolsByCulture.TryGetValue(cultureId, out MBList<ItemObject> cultureBanners) &&
                cultureBanners.Count > 0)
            {
                return SelectRandomItem(cultureBanners);
            }

            // Fall back to neutral banner pool
            if (_poolManager.NeutralBannerPool != null && _poolManager.NeutralBannerPool.Count > 0)
            {
                return SelectRandomItem(_poolManager.NeutralBannerPool);
            }

            return null;
        }

        #region Civilian Equipment Methods

        /// MARK: GetCivilianEquipmentSet
        /// <summary>
        /// Generates a complete civilian equipment set for a hero.
        /// Includes dress preference for females, Ladies Shoes pairing when wearing dress,
        /// and crown restrictions for ruling clan members only.
        /// </summary>
        /// <param name="hero">The hero to generate equipment for (used for crown eligibility).</param>
        /// <param name="culture">The culture to use for item selection. If null, uses neutral items.</param>
        /// <param name="heroLevel">The hero's level used to determine tier range.</param>
        /// <param name="isFemale">Whether the hero is female (affects dress preference).</param>
        /// <param name="includeNeutralItems">Whether to also check neutral item pools.</param>
        /// <returns>A new Equipment object with civilian equipment.</returns>
        public Equipment GetCivilianEquipmentSet(
            Hero hero,
            CultureObject culture,
            int heroLevel,
            bool isFemale,
            bool includeNeutralItems = true)
        {
            // Ensure pool manager is initialized
            if (!_poolManager.IsInitialized)
            {
                _poolManager.Initialize();
            }

            // Create new Equipment object (civilian type)
            Equipment equipment = new(Equipment.EquipmentType.Civilian);

            // Get tier range using ItemValidation
            (ItemObject.ItemTiers minTier, ItemObject.ItemTiers maxTier) = ItemValidation.GetTierRangeForLevel(heroLevel);

            string cultureId = culture?.StringId;

            // Check if hero is in ruling clan (affects crown eligibility)
            bool isRulingClanMember = ItemValidation.IsRulingClanMember(hero);

            // Track if wearing dress (affects gloves and shoes selection)
            bool wearingDress = false;

            // For females: 80% chance to select dress if available
            if (isFemale && HasDressesForCulture(cultureId, minTier, maxTier, includeNeutralItems))
            {
                int dressChance = _random.NextRandomInt(100);
                if (dressChance < 80)
                {
                    ItemObject dress = SelectDress(cultureId, minTier, maxTier, includeNeutralItems);
                    if (dress != null)
                    {
                        equipment[EquipmentIndex.Body] = new EquipmentElement(dress);
                        wearingDress = true;
                    }
                }
            }

            // If not wearing dress, select regular civilian body armor
            if (!wearingDress)
            {
                ItemObject bodyArmor = SelectCivilianArmorBySlot(cultureId, minTier, maxTier, EquipmentIndex.Body, isFemale, includeNeutralItems);
                if (bodyArmor != null)
                {
                    equipment[EquipmentIndex.Body] = new EquipmentElement(bodyArmor);
                }
            }

            // Head slot: ONLY for ruling clan members with civilian crowns
            if (isRulingClanMember)
            {
                ItemObject crown = SelectCivilianCrown(cultureId, minTier, maxTier, isFemale, includeNeutralItems);
                if (crown != null)
                {
                    equipment[EquipmentIndex.Head] = new EquipmentElement(crown);
                }
            }

            // Cape slot: civilian cape
            ItemObject cape = SelectCivilianArmorBySlot(cultureId, minTier, maxTier, EquipmentIndex.Cape, isFemale, includeNeutralItems);
            if (cape != null)
            {
                equipment[EquipmentIndex.Cape] = new EquipmentElement(cape);
            }

            // Gloves: NO gloves if wearing dress
            if (!wearingDress)
            {
                ItemObject gloves = SelectCivilianArmorBySlot(cultureId, minTier, maxTier, EquipmentIndex.Gloves, isFemale, includeNeutralItems);
                if (gloves != null)
                {
                    equipment[EquipmentIndex.Gloves] = new EquipmentElement(gloves);
                }
            }

            // Boots: Ladies Shoes if wearing dress, otherwise regular civilian boots
            if (wearingDress)
            {
                ItemObject ladiesShoes = SelectLadiesShoes(cultureId, minTier, maxTier, includeNeutralItems);
                if (ladiesShoes != null)
                {
                    equipment[EquipmentIndex.Leg] = new EquipmentElement(ladiesShoes);
                }
                else
                {
                    // Fallback to regular civilian boots if no ladies shoes found
                    ItemObject boots = SelectCivilianArmorBySlot(cultureId, minTier, maxTier, EquipmentIndex.Leg, isFemale, includeNeutralItems);
                    if (boots != null)
                    {
                        equipment[EquipmentIndex.Leg] = new EquipmentElement(boots);
                    }
                }
            }
            else
            {
                ItemObject boots = SelectCivilianArmorBySlot(cultureId, minTier, maxTier, EquipmentIndex.Leg, isFemale, includeNeutralItems);
                if (boots != null)
                {
                    equipment[EquipmentIndex.Leg] = new EquipmentElement(boots);
                }
            }

            // Weapon slot 0: civilian one-handed weapon
            ItemObject weapon = SelectCivilianWeapon(cultureId, minTier, maxTier, includeNeutralItems);
            if (weapon != null)
            {
                equipment[EquipmentIndex.Weapon0] = new EquipmentElement(weapon);
            }

            return equipment;
        }

        #region Civilian Weapon Selection

        /// MARK: SelectCivilianWeapon
        /// <summary>
        /// Selects a one-handed civilian weapon from pools.
        /// </summary>
        private ItemObject SelectCivilianWeapon(
            string cultureId,
            ItemObject.ItemTiers minTier,
            ItemObject.ItemTiers maxTier,
            bool includeNeutralItems)
        {
            MBList<ItemObject> candidates = new();

            // Collect civilian weapons from appropriate tiers
            for (int tier = (int)minTier; tier <= (int)maxTier; tier++)
            {
                // Try culture-specific pool first
                if (cultureId != null &&
                    _poolManager.WeaponPoolsByCulture.TryGetValue(cultureId, out Dictionary<int, Dictionary<WeaponTypeFlags, MBList<ItemObject>>> cultureTiers) &&
                    cultureTiers.TryGetValue(tier, out Dictionary<WeaponTypeFlags, MBList<ItemObject>> cultureWeapons))
                {
                    CollectCivilianWeapons(cultureWeapons, candidates);
                }

                // Also check neutral pool if enabled
                if (includeNeutralItems &&
                    _poolManager.NeutralWeaponPools.TryGetValue(tier, out Dictionary<WeaponTypeFlags, MBList<ItemObject>> neutralWeapons))
                {
                    CollectCivilianWeapons(neutralWeapons, candidates);
                }
            }

            return SelectRandomItem(candidates);
        }

        /// MARK: CollectCivilianWeapons
        /// <summary>
        /// Collects one-handed civilian appropriate weapons into the candidates list.
        /// </summary>
        private void CollectCivilianWeapons(
            Dictionary<WeaponTypeFlags, MBList<ItemObject>> weaponPools,
            MBList<ItemObject> candidates)
        {
            // Only collect one-handed weapons that are civilian appropriate
            foreach (KeyValuePair<WeaponTypeFlags, MBList<ItemObject>> kvp in weaponPools)
            {
                // Only one-handed weapon types
                if ((kvp.Key & WeaponTypeFlags.AllOneHanded) != 0)
                {
                    for (int i = 0; i < kvp.Value.Count; i++)
                    {
                        ItemObject item = kvp.Value[i];
                        if (ItemValidation.IsCivilianAppropriateItem(item))
                        {
                            candidates.Add(item);
                        }
                    }
                }
            }
        }

        #endregion

        #region Dress Selection

        /// MARK: HasDressesForCulture
        /// <summary>
        /// Checks if dresses are available for the specified culture and tier range.
        /// </summary>
        private bool HasDressesForCulture(
            string cultureId,
            ItemObject.ItemTiers minTier,
            ItemObject.ItemTiers maxTier,
            bool includeNeutralItems)
        {
            for (int tier = (int)minTier; tier <= (int)maxTier; tier++)
            {
                // Check culture-specific pool
                if (cultureId != null &&
                    _poolManager.ArmorPoolsBySlot.TryGetValue(cultureId, out Dictionary<int, Dictionary<EquipmentIndex, MBList<ItemObject>>> cultureTiers) &&
                    cultureTiers.TryGetValue(tier, out Dictionary<EquipmentIndex, MBList<ItemObject>> cultureArmor) &&
                    cultureArmor.TryGetValue(EquipmentIndex.Body, out MBList<ItemObject> cultureBodyItems))
                {
                    for (int i = 0; i < cultureBodyItems.Count; i++)
                    {
                        if (ItemValidation.IsDressItem(cultureBodyItems[i]))
                            return true;
                    }
                }

                // Check neutral pool if enabled
                if (includeNeutralItems &&
                    _poolManager.NeutralArmorPools.TryGetValue(tier, out Dictionary<EquipmentIndex, MBList<ItemObject>> neutralArmor) &&
                    neutralArmor.TryGetValue(EquipmentIndex.Body, out MBList<ItemObject> neutralBodyItems))
                {
                    for (int i = 0; i < neutralBodyItems.Count; i++)
                    {
                        if (ItemValidation.IsDressItem(neutralBodyItems[i]))
                            return true;
                    }
                }
            }

            return false;
        }

        /// MARK: SelectDress
        /// <summary>
        /// Selects a dress from armor pools for the specified culture and tier range.
        /// </summary>
        private ItemObject SelectDress(
            string cultureId,
            ItemObject.ItemTiers minTier,
            ItemObject.ItemTiers maxTier,
            bool includeNeutralItems)
        {
            MBList<ItemObject> candidates = new();

            // First attempt: Try requested tier range
            CollectDressesFromTierRange(cultureId, (int)minTier, (int)maxTier, includeNeutralItems, candidates);
            
            if (candidates.Count > 0)
            {
                return SelectRandomItem(candidates);
            }

            // TIER FALLBACK: No dresses found in requested range
            // Try one tier lower first
            int oneTierLower = (int)minTier - 1;
            if (oneTierLower >= 0)
            {
                CollectDressesFromTierRange(cultureId, oneTierLower, oneTierLower, includeNeutralItems, candidates);
                if (candidates.Count > 0)
                {
                    return SelectRandomItem(candidates);
                }
            }

            // Try one tier higher
            int oneTierHigher = (int)maxTier + 1;
            if (oneTierHigher <= (int)ItemObject.ItemTiers.Tier6)
            {
                CollectDressesFromTierRange(cultureId, oneTierHigher, oneTierHigher, includeNeutralItems, candidates);
                if (candidates.Count > 0)
                {
                    return SelectRandomItem(candidates);
                }
            }

            // Expand search to remaining lower tiers
            for (int tier = oneTierLower - 1; tier >= 0; tier--)
            {
                CollectDressesFromTierRange(cultureId, tier, tier, includeNeutralItems, candidates);
                if (candidates.Count > 0)
                {
                    return SelectRandomItem(candidates);
                }
            }

            // Finally try remaining higher tiers
            for (int tier = oneTierHigher + 1; tier <= (int)ItemObject.ItemTiers.Tier6; tier++)
            {
                CollectDressesFromTierRange(cultureId, tier, tier, includeNeutralItems, candidates);
                if (candidates.Count > 0)
                {
                    return SelectRandomItem(candidates);
                }
            }

            return null;
        }

        /// MARK: CollectDressesFromTierRange
        /// <summary>
        /// Helper method to collect dresses from a specific tier range.
        /// </summary>
        private void CollectDressesFromTierRange(
            string cultureId,
            int minTier,
            int maxTier,
            bool includeNeutralItems,
            MBList<ItemObject> candidates)
        {
            for (int tier = minTier; tier <= maxTier; tier++)
            {
                // Check culture-specific pool
                if (cultureId != null &&
                    _poolManager.ArmorPoolsBySlot.TryGetValue(cultureId, out Dictionary<int, Dictionary<EquipmentIndex, MBList<ItemObject>>> cultureTiers) &&
                    cultureTiers.TryGetValue(tier, out Dictionary<EquipmentIndex, MBList<ItemObject>> cultureArmor) &&
                    cultureArmor.TryGetValue(EquipmentIndex.Body, out MBList<ItemObject> cultureBodyItems))
                {
                    for (int i = 0; i < cultureBodyItems.Count; i++)
                    {
                        ItemObject item = cultureBodyItems[i];
                        if (ItemValidation.IsDressItem(item) && ItemValidation.IsCivilianAppropriateItem(item))
                        {
                            candidates.Add(item);
                        }
                    }
                }

                // Check neutral pool if enabled
                if (includeNeutralItems &&
                    _poolManager.NeutralArmorPools.TryGetValue(tier, out Dictionary<EquipmentIndex, MBList<ItemObject>> neutralArmor) &&
                    neutralArmor.TryGetValue(EquipmentIndex.Body, out MBList<ItemObject> neutralBodyItems))
                {
                    for (int i = 0; i < neutralBodyItems.Count; i++)
                    {
                        ItemObject item = neutralBodyItems[i];
                        if (ItemValidation.IsDressItem(item) && ItemValidation.IsCivilianAppropriateItem(item))
                        {
                            candidates.Add(item);
                        }
                    }
                }
            }
        }

        /// MARK: SelectLadiesShoes
        /// <summary>
        /// Selects ladies shoes from armor pools for the specified culture and tier range.
        /// </summary>
        private ItemObject SelectLadiesShoes(
            string cultureId,
            ItemObject.ItemTiers minTier,
            ItemObject.ItemTiers maxTier,
            bool includeNeutralItems)
        {
            MBList<ItemObject> candidates = new();

            for (int tier = (int)minTier; tier <= (int)maxTier; tier++)
            {
                // Check culture-specific pool
                if (cultureId != null &&
                    _poolManager.ArmorPoolsBySlot.TryGetValue(cultureId, out Dictionary<int, Dictionary<EquipmentIndex, MBList<ItemObject>>> cultureTiers) &&
                    cultureTiers.TryGetValue(tier, out Dictionary<EquipmentIndex, MBList<ItemObject>> cultureArmor) &&
                    cultureArmor.TryGetValue(EquipmentIndex.Leg, out MBList<ItemObject> cultureLegItems))
                {
                    for (int i = 0; i < cultureLegItems.Count; i++)
                    {
                        ItemObject item = cultureLegItems[i];
                        if (ItemValidation.IsLadiesShoes(item))
                        {
                            candidates.Add(item);
                        }
                    }
                }

                // Check neutral pool if enabled
                if (includeNeutralItems &&
                    _poolManager.NeutralArmorPools.TryGetValue(tier, out Dictionary<EquipmentIndex, MBList<ItemObject>> neutralArmor) &&
                    neutralArmor.TryGetValue(EquipmentIndex.Leg, out MBList<ItemObject> neutralLegItems))
                {
                    for (int i = 0; i < neutralLegItems.Count; i++)
                    {
                        ItemObject item = neutralLegItems[i];
                        if (ItemValidation.IsLadiesShoes(item))
                        {
                            candidates.Add(item);
                        }
                    }
                }
            }

            return SelectRandomItem(candidates);
        }

        #endregion

        #region Civilian Armor Selection

        /// MARK: SelectCivilianArmorBySlot
        /// <summary>
        /// Selects civilian armor for a specific slot matching culture and tier criteria.
        /// Implements tier fallback for required slots (Body, Leg) to ensure they get filled.
        /// </summary>
        private ItemObject SelectCivilianArmorBySlot(
            string cultureId,
            ItemObject.ItemTiers minTier,
            ItemObject.ItemTiers maxTier,
            EquipmentIndex slot,
            bool isFemale,
            bool includeNeutralItems)
        {
            MBList<ItemObject> candidates = new();

            // First attempt: Try requested tier range
            CollectCivilianArmorFromTierRange(cultureId, (int)minTier, (int)maxTier, slot, isFemale, includeNeutralItems, candidates);

            if (candidates.Count > 0)
            {
                return SelectRandomItem(candidates);
            }

            // TIER FALLBACK for required slots (Body and Leg/Boots are required for civilian)
            bool isRequiredSlot = (slot == EquipmentIndex.Body || slot == EquipmentIndex.Leg);
            
            if (isRequiredSlot)
            {
                // Try lower tiers first (minTier-1 down to 0)
                for (int tier = (int)minTier - 1; tier >= 0; tier--)
                {
                    CollectCivilianArmorFromTierRange(cultureId, tier, tier, slot, isFemale, includeNeutralItems, candidates);
                    if (candidates.Count > 0)
                    {
                        return SelectRandomItem(candidates);
                    }
                }

                // Try higher tiers (maxTier+1 up to Tier6)
                for (int tier = (int)maxTier + 1; tier <= (int)ItemObject.ItemTiers.Tier6; tier++)
                {
                    CollectCivilianArmorFromTierRange(cultureId, tier, tier, slot, isFemale, includeNeutralItems, candidates);
                    if (candidates.Count > 0)
                    {
                        return SelectRandomItem(candidates);
                    }
                }

                // Last resort for required slots: Try ALL armor (not just civilian) to fill the slot
                for (int tier = 0; tier <= (int)ItemObject.ItemTiers.Tier6; tier++)
                {
                    CollectArmorFromTierRange(cultureId, tier, tier, slot, isFemale, excludeCloth: false, includeNeutralItems, candidates);
                    if (candidates.Count > 0)
                    {
                        return SelectRandomItem(candidates);
                    }
                }
            }

            return SelectRandomItem(candidates);
        }

        /// MARK: CollectCivilianArmorFromTierRange
        /// <summary>
        /// Helper method to collect civilian armor from a specific tier range.
        /// </summary>
        private void CollectCivilianArmorFromTierRange(
            string cultureId,
            int minTier,
            int maxTier,
            EquipmentIndex slot,
            bool isFemale,
            bool includeNeutralItems,
            MBList<ItemObject> candidates)
        {
            for (int tier = minTier; tier <= maxTier; tier++)
            {
                // Try culture-specific pool
                if (cultureId != null &&
                    _poolManager.ArmorPoolsBySlot.TryGetValue(cultureId, out Dictionary<int, Dictionary<EquipmentIndex, MBList<ItemObject>>> cultureTiers) &&
                    cultureTiers.TryGetValue(tier, out Dictionary<EquipmentIndex, MBList<ItemObject>> cultureArmor) &&
                    cultureArmor.TryGetValue(slot, out MBList<ItemObject> cultureItems))
                {
                    CollectCivilianArmor(cultureItems, isFemale, candidates);
                }

                // Also check neutral pool if enabled
                if (includeNeutralItems &&
                    _poolManager.NeutralArmorPools.TryGetValue(tier, out Dictionary<EquipmentIndex, MBList<ItemObject>> neutralArmor) &&
                    neutralArmor.TryGetValue(slot, out MBList<ItemObject> neutralItems))
                {
                    CollectCivilianArmor(neutralItems, isFemale, candidates);
                }
            }
        }

        /// MARK: CollectCivilianArmor
        /// <summary>
        /// Collects civilian armor items applying gender check and civilian filter.
        /// Uses the native IsCivilian flag as primary filter for accurate civilian item detection.
        /// Note: Cloth armor is NOT excluded for civilian equipment - most civilian clothes are cloth.
        /// </summary>
        private void CollectCivilianArmor(
            MBList<ItemObject> items,
            bool isFemale,
            MBList<ItemObject> candidates)
        {
            for (int i = 0; i < items.Count; i++)
            {
                ItemObject item = items[i];

                // Apply gender filter
                if (!ItemValidation.IsArmorSuitableForGender(item, isFemale))
                    continue;

                // NO cloth exclusion for civilian - civilian clothes are often cloth
                // This was causing Vlandia heroes to get leather armor instead of tunics

                // Primary filter: Use native IsCivilian flag as source of truth
                // This is the most reliable way to determine civilian appropriateness
                if (item.IsCivilian)
                {
                    candidates.Add(item);
                    continue;
                }

                // Secondary filter: Also include items identified as civilian by heuristics
                // (dresses, ladies shoes, civilian crowns, cloth items, etc.)
                // This catches items that may not have the IsCivilian flag but are clearly civilian
                if (ItemValidation.IsCivilianAppropriateItem(item))
                {
                    candidates.Add(item);
                }
            }
        }

        #endregion

        #region Civilian Crown Selection

        /// MARK: SelectCivilianCrown
        /// <summary>
        /// Selects a civilian crown for ruling clan members.
        /// </summary>
        private ItemObject SelectCivilianCrown(
            string cultureId,
            ItemObject.ItemTiers minTier,
            ItemObject.ItemTiers maxTier,
            bool isFemale,
            bool includeNeutralItems)
        {
            MBList<ItemObject> candidates = new();

            for (int tier = (int)minTier; tier <= (int)maxTier; tier++)
            {
                // Try culture-specific pool
                if (cultureId != null &&
                    _poolManager.ArmorPoolsBySlot.TryGetValue(cultureId, out Dictionary<int, Dictionary<EquipmentIndex, MBList<ItemObject>>> cultureTiers) &&
                    cultureTiers.TryGetValue(tier, out Dictionary<EquipmentIndex, MBList<ItemObject>> cultureArmor) &&
                    cultureArmor.TryGetValue(EquipmentIndex.Head, out MBList<ItemObject> cultureItems))
                {
                    CollectCivilianCrowns(cultureItems, isFemale, candidates);
                }

                // Also check neutral pool if enabled
                if (includeNeutralItems &&
                    _poolManager.NeutralArmorPools.TryGetValue(tier, out Dictionary<EquipmentIndex, MBList<ItemObject>> neutralArmor) &&
                    neutralArmor.TryGetValue(EquipmentIndex.Head, out MBList<ItemObject> neutralItems))
                {
                    CollectCivilianCrowns(neutralItems, isFemale, candidates);
                }
            }

            return SelectRandomItem(candidates);
        }

        /// MARK: CollectCivilianCrowns
        /// <summary>
        /// Collects civilian crown items applying gender check and Jeweled Crown female-only check.
        /// </summary>
        private void CollectCivilianCrowns(
            MBList<ItemObject> items,
            bool isFemale,
            MBList<ItemObject> candidates)
        {
            for (int i = 0; i < items.Count; i++)
            {
                ItemObject item = items[i];

                // Must be a civilian crown
                if (!ItemValidation.IsCivilianCrown(item))
                    continue;

                // Apply gender filter
                if (!ItemValidation.IsArmorSuitableForGender(item, isFemale))
                    continue;

                // Jeweled Crown is female-only
                if (!isFemale && ItemValidation.IsJeweledCrown(item))
                    continue;

                candidates.Add(item);
            }
        }

        #endregion

        #endregion
    }
}