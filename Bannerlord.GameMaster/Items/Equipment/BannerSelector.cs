using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.ObjectSystem;

namespace Bannerlord.GameMaster.Items
{
    /// <summary>
    /// Selects appropriate banners for heroes based on level and combat skills.
    /// Banners are only assigned to heroes level 10+.
    /// </summary>
    public static class BannerSelector
    {
        #region Banner String IDs by Category

        // Tier mapping: in-game item tiers are 2, 4, 6 which correspond to t1, t2, t3 suffixes
        // Level 10-14: Tier 2 (t1 suffix)
        // Level 15-19: Tier 4 (t2 suffix)
        // Level 20+: Tier 6 (t3 suffix)

        /// <summary>Horseman banners (+charge damage) - for mounted heroes with polearm.</summary>
        private static readonly string[] HorsemanBanners = new[]
        {
            "banner_of_the_horseman_t1",  // Tier 2
            "banner_of_the_squire_t2",    // Tier 4
            "banner_of_the_knight_t3"     // Tier 6
        };

        /// <summary>Roaming/Boundless Horde banners (+mount speed) - for mounted heroes without polearm.</summary>
        private static readonly string[] MountSpeedBanners = new[]
        {
            "tug_of_the_roaming_horse_t1",     // Tier 2
            "tug_of_the_boundless_horde_t2",   // Tier 4
            "tug_of_the_endless_steppe_t3"     // Tier 6
        };

        /// <summary>Falcon/Hawk/Eagle banners (-range accuracy penalty) - for bow/crossbow heroes.</summary>
        private static readonly string[] RangedBanners = new[]
        {
            "banner_of_faris_falcon_t1",  // Tier 2
            "banner_of_emir_hawk_t2",     // Tier 4
            "banner_of_sultan_eagle_t3"   // Tier 6
        };

        /// <summary>Shield banners (-shield damage) - for one-handed + shield heroes.</summary>
        private static readonly string[] ShieldBanners = new[]
        {
            "banner_of_oaken_shields_t1",  // Tier 2
            "banner_of_iron_shields_t2",   // Tier 4
            "banner_of_steel_shields_t3"   // Tier 6
        };

        /// <summary>Morale banners (-morale shock) - fallback for shield heroes.</summary>
        private static readonly string[] MoraleBanners = new[]
        {
            "standard_of_duty_t1",        // Tier 2
            "standard_of_courage_t2",     // Tier 4
            "standard_of_discipline_t3"   // Tier 6
        };

        /// <summary>Spear wall banners (+damage vs mounted) - for polearm heroes non-mounted.</summary>
        private static readonly string[] SpearWallBanners = new[]
        {
            "spear_bracing_banner_t1",  // Tier 2
            "spear_wall_banner_t2",     // Tier 4
            "pike_wall_banner_t3"       // Tier 6
        };

        /// <summary>Fury/Rage/Wrath banners (+melee damage) - for two-handed heroes.</summary>
        private static readonly string[] MeleeDamageBanners = new[]
        {
            "standard_of_fury_t1",   // Tier 2
            "standard_of_rage_t2",   // Tier 4
            "standard_of_wrath_t3"   // Tier 6
        };

        /// <summary>Stone/Iron/Steel banners (-melee damage taken) - for general melee heroes.</summary>
        private static readonly string[] MeleeDefenseBanners = new[]
        {
            "stone_banner_t1",  // Tier 2
            "iron_banner_t2",   // Tier 4
            "steel_banner_t3"   // Tier 6
        };

        /// <summary>Campaign banners (basic morale) - generic fallback.</summary>
        private static readonly string[] CampaignBanners = new[]
        {
            "campaign_banner_small",      // Tier 2
            "campaign_banner_small",      // Tier 4 (no t2 version)
            "campaign_banner_small"       // Tier 6 (no t3 version)
        };

        /// <summary>All valid banner IDs that should NEVER be equipped on non-mounted heroes.</summary>
        private static readonly HashSet<string> MountedOnlyBanners = new()
        {
            "banner_of_the_horseman_t1",
            "banner_of_the_squire_t2",
            "banner_of_the_knight_t3",
            "tug_of_the_roaming_horse_t1",
            "tug_of_the_boundless_horde_t2",
            "tug_of_the_endless_steppe_t3"
        };

        /// <summary>All valid banner IDs that should NEVER be equipped on non-bow/crossbow heroes.</summary>
        private static readonly HashSet<string> RangedOnlyBanners = new()
        {
            "banner_of_faris_falcon_t1",
            "banner_of_emir_hawk_t2",
            "banner_of_sultan_eagle_t3"
        };

        /// <summary>All valid banner IDs that should NEVER be equipped on heroes without shields.</summary>
        private static readonly HashSet<string> ShieldOnlyBanners = new()
        {
            "banner_of_oaken_shields_t1",
            "banner_of_iron_shields_t2",
            "banner_of_steel_shields_t3"
        };

        #endregion

        #region Public Methods

        /// MARK: SelectBannerForHero
        /// <summary>
        /// Selects an appropriate banner for a hero based on their level and combat skills.
        /// Only heroes level 10+ receive banners.
        /// </summary>
        /// <param name="hero">The hero to select a banner for.</param>
        /// <param name="heroLevel">The hero's level (used for tier calculation).</param>
        /// <param name="equipment">The hero's current equipment (used to check for shield, mount, weapons).</param>
        /// <returns>An appropriate banner ItemObject, or null if hero is under level 10.</returns>
        public static ItemObject SelectBannerForHero(Hero hero, int heroLevel, Equipment equipment)
        {
            // Only heroes level 10+ get banners
            if (heroLevel < 10)
                return null;

            // Determine banner tier from level
            int tierIndex = GetTierIndexFromLevel(heroLevel);

            // Analyze hero's equipment and skills
            bool isMounted = equipment != null && !equipment[EquipmentIndex.Horse].IsEmpty;
            bool hasShield = HasShieldEquipped(equipment);
            bool hasPolearm = HasPolearmEquipped(equipment);
            bool hasBowOrCrossbow = HasBowOrCrossbowEquipped(equipment);

            // Get hero's combat skills
            int ridingSkill = hero?.GetSkillValue(DefaultSkills.Riding) ?? 0;
            int bowSkill = hero?.GetSkillValue(DefaultSkills.Bow) ?? 0;
            int crossbowSkill = hero?.GetSkillValue(DefaultSkills.Crossbow) ?? 0;
            int oneHandedSkill = hero?.GetSkillValue(DefaultSkills.OneHanded) ?? 0;
            int twoHandedSkill = hero?.GetSkillValue(DefaultSkills.TwoHanded) ?? 0;
            int polearmSkill = hero?.GetSkillValue(DefaultSkills.Polearm) ?? 0;

            // Determine highest skill
            int maxRangedSkill = System.Math.Max(bowSkill, crossbowSkill);
            int maxMeleeSkill = System.Math.Max(oneHandedSkill, System.Math.Max(twoHandedSkill, polearmSkill));

            // Select banner based on priority rules
            string[] selectedBannerArray = null;

            // Priority 1: Mounted heroes (riding skill highest)
            if (isMounted && ridingSkill >= maxMeleeSkill && ridingSkill >= maxRangedSkill)
            {
                if (hasPolearm)
                {
                    // Mounted + polearm = Horseman banner (+charge damage)
                    selectedBannerArray = HorsemanBanners;
                }
                else
                {
                    // Mounted without polearm = Mount speed banner
                    selectedBannerArray = MountSpeedBanners;
                }
            }
            // Priority 2: Bow/Crossbow heroes
            else if (hasBowOrCrossbow && maxRangedSkill >= maxMeleeSkill)
            {
                selectedBannerArray = RangedBanners;
            }
            // Priority 3: One-handed + shield heroes
            else if (hasShield && oneHandedSkill >= twoHandedSkill && oneHandedSkill >= polearmSkill)
            {
                selectedBannerArray = ShieldBanners;
            }
            // Priority 4: Polearm heroes non-mounted
            else if (!isMounted && polearmSkill >= oneHandedSkill && polearmSkill >= twoHandedSkill)
            {
                selectedBannerArray = SpearWallBanners;
            }
            // Priority 5: Two-handed heroes
            else if (twoHandedSkill >= oneHandedSkill && twoHandedSkill >= polearmSkill)
            {
                selectedBannerArray = MeleeDamageBanners;
            }
            // Priority 6: General melee heroes
            else if (maxMeleeSkill > 0)
            {
                selectedBannerArray = MeleeDefenseBanners;
            }
            // Fallback: Campaign banner
            else
            {
                selectedBannerArray = CampaignBanners;
            }

            // Get banner from selected array
            if (selectedBannerArray != null && tierIndex < selectedBannerArray.Length)
            {
                ItemObject banner = GetBannerByStringId(selectedBannerArray[tierIndex]);
                if (banner != null)
                    return banner;
            }

            // Fallback to a safe banner if selection failed
            return GetFallbackBanner(tierIndex, isMounted, hasBowOrCrossbow, hasShield);
        }

        /// MARK: GetTierIndexFromLevel
        /// <summary>
        /// Gets the tier index (0, 1, or 2) based on hero level.
        /// Level 10-14: Index 0 (Tier 2 items with t1 suffix)
        /// Level 15-19: Index 1 (Tier 4 items with t2 suffix)
        /// Level 20+: Index 2 (Tier 6 items with t3 suffix)
        /// </summary>
        public static int GetTierIndexFromLevel(int heroLevel)
        {
            if (heroLevel < 15)
                return 0; // Tier 2
            if (heroLevel < 20)
                return 1; // Tier 4
            return 2;     // Tier 6
        }

        /// MARK: IsBannerValidForHero
        /// <summary>
        /// Validates that a banner is appropriate for a hero based on their equipment.
        /// Prevents equipping charge banners to non-mounted, range banners to non-ranged, etc.
        /// </summary>
        public static bool IsBannerValidForHero(ItemObject banner, bool isMounted, bool hasBowOrCrossbow, bool hasShield)
        {
            if (banner == null)
                return false;

            string stringId = banner.StringId ?? string.Empty;

            // Check mounted-only restrictions
            if (!isMounted && MountedOnlyBanners.Contains(stringId))
                return false;

            // Check ranged-only restrictions
            if (!hasBowOrCrossbow && RangedOnlyBanners.Contains(stringId))
                return false;

            // Check shield-only restrictions
            if (!hasShield && ShieldOnlyBanners.Contains(stringId))
                return false;

            return true;
        }

        #endregion

        #region Private Methods

        /// MARK: GetBannerByStringId
        /// <summary>
        /// Gets a banner ItemObject by its string ID.
        /// </summary>
        private static ItemObject GetBannerByStringId(string stringId)
        {
            if (string.IsNullOrEmpty(stringId))
                return null;

            return MBObjectManager.Instance?.GetObject<ItemObject>(stringId);
        }

        /// MARK: GetFallbackBanner
        /// <summary>
        /// Gets a safe fallback banner that respects hero equipment restrictions.
        /// </summary>
        private static ItemObject GetFallbackBanner(int tierIndex, bool isMounted, bool hasBowOrCrossbow, bool hasShield)
        {
            // Try defense banners first (safe for everyone)
            string[] defenseBanners = MeleeDefenseBanners;
            if (tierIndex < defenseBanners.Length)
            {
                ItemObject banner = GetBannerByStringId(defenseBanners[tierIndex]);
                if (banner != null)
                    return banner;
            }

            // Try morale banners
            string[] moraleBanners = MoraleBanners;
            if (tierIndex < moraleBanners.Length)
            {
                ItemObject banner = GetBannerByStringId(moraleBanners[tierIndex]);
                if (banner != null)
                    return banner;
            }

            // Last resort: campaign banner
            return GetBannerByStringId("campaign_banner_small");
        }

        /// MARK: HasShieldEquipped
        /// <summary>
        /// Checks if the equipment has a shield in any weapon slot.
        /// </summary>
        private static bool HasShieldEquipped(Equipment equipment)
        {
            if (equipment == null)
                return false;

            for (int i = 0; i <= 3; i++)
            {
                EquipmentElement element = equipment[(EquipmentIndex)i];
                if (!element.IsEmpty && element.Item != null && element.Item.ItemType == ItemObject.ItemTypeEnum.Shield)
                    return true;
            }

            return false;
        }

        /// MARK: HasPolearmEquipped
        /// <summary>
        /// Checks if the equipment has a polearm in any weapon slot.
        /// </summary>
        private static bool HasPolearmEquipped(Equipment equipment)
        {
            if (equipment == null)
                return false;

            for (int i = 0; i <= 3; i++)
            {
                EquipmentElement element = equipment[(EquipmentIndex)i];
                if (!element.IsEmpty && element.Item != null && element.Item.ItemType == ItemObject.ItemTypeEnum.Polearm)
                    return true;
            }

            return false;
        }

        /// MARK: HasBowOrCrossbowEquipped
        /// <summary>
        /// Checks if the equipment has a bow or crossbow in any weapon slot.
        /// </summary>
        private static bool HasBowOrCrossbowEquipped(Equipment equipment)
        {
            if (equipment == null)
                return false;

            for (int i = 0; i <= 3; i++)
            {
                EquipmentElement element = equipment[(EquipmentIndex)i];
                if (!element.IsEmpty && element.Item != null)
                {
                    if (element.Item.ItemType == ItemObject.ItemTypeEnum.Bow ||
                        element.Item.ItemType == ItemObject.ItemTypeEnum.Crossbow)
                        return true;
                }
            }

            return false;
        }

        #endregion
    }
}
