using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;

namespace Bannerlord.GameMaster.Items
{
    /// <summary>
    /// Provides static methods for validating items and determining their suitability
    /// for equipment pools and hero assignment based on various criteria.
    /// </summary>
    public static class ItemValidation
    {
        /// <summary>
        /// StringIds that should be except from gender check and is compatibile for all genders
        /// </summary>
        private static readonly string[] genderExceptionStringIds = new[]
        {
            "desert_headdress",
        };

        /// <summary>
        /// Keywords that identify female-only items (case insensitive).
        /// Males cannot wear items containing these keywords in Name or StringId.
        /// </summary>
        private static readonly string[] FemaleOnlyKeywords = new[]
        {
            "lady",
            "ladies",
            "dress",
            "woman",
            "female"
        };

        /// <summary>
        /// Item names that should be completely excluded from equipment pools.
        /// </summary>
        private static readonly string[] BlacklistedItemNames = new[]
        {
            "Ballista Arrows",
            "Stone"
        };

        /// <summary>
        /// Keywords that identify items to exclude from equipment pools.
        /// </summary>
        private static readonly string[] BlacklistedKeywords = new[]
        {
            "sling",
            "practice",
            "tournament",
            "tourney",
            "dummy"
        };

        /// <summary>
        /// Specific weapon StringIds to exclude from equipment pools.
        /// </summary>
        private static readonly string[] BlacklistedWeaponStringIds = new[]
        {
            "push_fork",
            "hook"
        };

        /// <summary>
        /// Keywords that identify crown items.
        /// </summary>
        private static readonly string[] CrownKeywords = new[]
        {
            "crown"
        };

        /// <summary>
        /// Special crown items identified by StringId that may not contain crown keyword.
        /// </summary>
        private static readonly string[] SpecialCrownItems = new[]
        {
            "imperial_jeweled_band"
        };

        #region Appearance Thresholds

        /// <summary>
        /// Minimum appearance value for standard civilian items.
        /// Items with Appearance > this value are considered suitable.
        /// </summary>
        public const float MinimumCivilianAppearance = 0.99f;

        /// <summary>
        /// Minimum appearance value for ruling clan members' civilian items.
        /// Items with Appearance > this value are considered suitable for royalty.
        /// </summary>
        public const float MinimumRoyalAppearance = 1.99f;

        /// <summary>
        /// Minimum appearance value for battle armor items.
        /// Items with Appearance > this value are considered suitable for battle.
        /// </summary>
        public const float MinimumBattleArmorAppearance = 0.49f;

        #endregion

        #region Blacklist Methods

        /// MARK: IsBlacklistedItem
        /// <summary>
        /// Determines if an item is blacklisted and should be excluded from equipment pools.
        /// Checks both exact name matches and keyword matches.
        /// </summary>
        /// <param name="item">The item to check.</param>
        /// <returns>True if the item is blacklisted; false otherwise.</returns>
        public static bool IsBlacklistedItem(ItemObject item)
        {
            if (item == null)
                return false;

            string itemName = item.Name?.ToString() ?? string.Empty;
            string itemStringId = item.StringId ?? string.Empty;

            // Check exact name matches
            foreach (string blacklistedName in BlacklistedItemNames)
            {
                if (itemName.Equals(blacklistedName, System.StringComparison.OrdinalIgnoreCase))
                    return true;
            }

            // Check keyword matches
            foreach (string keyword in BlacklistedKeywords)
            {
                if (itemName.IndexOf(keyword, System.StringComparison.OrdinalIgnoreCase) >= 0)
                    return true;

                if (itemStringId.IndexOf(keyword, System.StringComparison.OrdinalIgnoreCase) >= 0)
                    return true;
            }

            return false;
        }

        /// MARK: IsBlacklistedWeaponStringId
        /// <summary>
        /// Determines if a weapon item is blacklisted by StringId.
        /// Checks for specific weapon StringIds that should be excluded.
        /// </summary>
        /// <param name="item">The item to check.</param>
        /// <returns>True if the weapon StringId is blacklisted; false otherwise.</returns>
        public static bool IsBlacklistedWeaponStringId(ItemObject item)
        {
            if (item == null || !item.HasWeaponComponent)
                return false;

            string itemStringId = item.StringId ?? string.Empty;

            for (int i = 0; i < BlacklistedWeaponStringIds.Length; i++)
            {
                if (itemStringId.Equals(BlacklistedWeaponStringIds[i], System.StringComparison.OrdinalIgnoreCase))
                    return true;
            }

            return false;
        }

        #endregion

        /// MARK: IsValidItem
        /// <summary>
        /// Determines if an item is valid for inclusion in equipment pools.
        /// Excludes blacklisted items, placeholder items (DP prefix), items with no functional stats,
        /// and other invalid items.
        /// </summary>
        /// <param name="item">The item to validate.</param>
        /// <returns>True if the item is valid for equipment pools; false otherwise.</returns>
        public static bool IsValidItem(ItemObject item)
        {
            if (item == null)
                return false;

            // Check blacklist first
            if (IsBlacklistedItem(item))
                return false;

            // Exclude items with StringId starting with "DP" (case insensitive)
            // These are typically placeholder or debug items
            if (!string.IsNullOrEmpty(item.StringId) &&
                item.StringId.StartsWith("DP", System.StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            // Validate weapons - exclude those with no damage stats (unless shield)
            if (item.HasWeaponComponent)
            {
                if (!IsValidWeapon(item))
                    return false;

                // Check weapon-specific StringId blacklist
                if (IsBlacklistedWeaponStringId(item))
                    return false;
            }

            // Validate armor - exclude those with no armor stats
            if (item.HasArmorComponent)
            {
                if (!IsValidArmor(item))
                    return false;
            }

            // Validate horses - exclude those with no stats
            if (item.HasHorseComponent)
            {
                if (!IsValidHorse(item))
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Determines if an armor item is suitable for a hero based on gender.
        /// Females can wear anything; males cannot wear items with female-only keywords.
        /// </summary>
        /// <param name="item">The armor item to check.</param>
        /// <param name="isFemale">Whether the hero is female.</param>
        /// <returns>True if the item is suitable for the specified gender; false otherwise.</returns>
        public static bool IsArmorSuitableForGender(ItemObject item, bool isFemale)
        {
            if (item == null)
                return false;

            // Check gender exception item list
            foreach (string genderException in genderExceptionStringIds)
            {
                if (item.StringId == genderException)
                    return true;
            }

            // Females can wear anything
            if (isFemale)
                return true;

            // Check for female-only keywords in Name
            string itemName = item.Name?.ToString() ?? string.Empty;
            string itemStringId = item.StringId ?? string.Empty;

            foreach (string keyword in FemaleOnlyKeywords)
            {
                // Check Name
                if (itemName.IndexOf(keyword, System.StringComparison.OrdinalIgnoreCase) >= 0)
                    return false;

                // Check StringId
                if (itemStringId.IndexOf(keyword, System.StringComparison.OrdinalIgnoreCase) >= 0)
                    return false;
            }

            return true;
        }

        /// MARK: GetTierRangeForLevel
        /// <summary>
        /// Gets the valid tier range for equipment based on hero level.
        /// Higher level heroes use higher tier equipment with appropriate minimum tiers.
        /// Tier ranges are calibrated so heroes get level-appropriate gear:
        /// - Level 0-9: Tier 1-3 (basic equipment)
        /// - Level 10-14: Tier 3-4 (improved equipment)
        /// - Level 15-19: Tier 4-5 (quality equipment)
        /// - Level 20-24: Tier 5-6 (elite equipment)
        /// - Level 25+: Tier 6 (legendary equipment)
        /// </summary>
        /// <param name="heroLevel">The hero's level.</param>
        /// <returns>A tuple containing the minimum and maximum allowed item tiers.</returns>
        public static (ItemObject.ItemTiers minTier, ItemObject.ItemTiers maxTier) GetTierRangeForLevel(int heroLevel)
        {
            // Level 0-9: Tier 1-3 (basic equipment)
            if (heroLevel < 10)
                return (ItemObject.ItemTiers.Tier1, ItemObject.ItemTiers.Tier3);

            // Level 10-14: Tier 3-4 (improved equipment)
            if (heroLevel < 15)
                return (ItemObject.ItemTiers.Tier3, ItemObject.ItemTiers.Tier4);

            // Level 15-19: Tier 4-5 (quality equipment)
            if (heroLevel < 20)
                return (ItemObject.ItemTiers.Tier4, ItemObject.ItemTiers.Tier5);

            // Level 20-24: Tier 5-6 (elite equipment)
            if (heroLevel < 25)
                return (ItemObject.ItemTiers.Tier5, ItemObject.ItemTiers.Tier6);

            // Level 25+: Tier 6 (legendary equipment)
            return (ItemObject.ItemTiers.Tier6, ItemObject.ItemTiers.Tier6);
        }

        /// MARK: IsItemInTierRange
        /// <summary>
        /// Determines if an item's tier falls within the specified tier range.
        /// </summary>
        /// <param name="item">The item to check.</param>
        /// <param name="minTier">The minimum allowed tier.</param>
        /// <param name="maxTier">The maximum allowed tier.</param>
        /// <returns>True if the item's tier is within the range; false otherwise.</returns>
        public static bool IsItemInTierRange(ItemObject item, ItemObject.ItemTiers minTier, ItemObject.ItemTiers maxTier)
        {
            if (item == null)
                return false;

            return item.Tier >= minTier && item.Tier <= maxTier;
        }

        /// MARK: ShouldExcludeCloth
        /// <summary>
        /// Determines if cloth armor should be excluded for a hero based on level and equipment slot.
        /// Low-level heroes (under 10) can wear cloth armor. Capes are always allowed to be cloth.
        /// </summary>
        /// <param name="heroLevel">The hero's level.</param>
        /// <param name="slot">The equipment slot being filled.</param>
        /// <returns>True if cloth armor should be excluded; false if cloth is allowed.</returns>
        public static bool ShouldExcludeClothArmor(int heroLevel, EquipmentIndex slot)
        {
            // Allow cloth for low level heroes
            if (heroLevel < 10)
                return false;

            // Always allow cloth capes
            if (slot == EquipmentIndex.Cape)
                return false;

            // Exclude cloth armor for level 10+ heroes (except capes)
            return true;
        }

        /// MARK: IsClothArmor
        /// <summary>
        /// Determines if an item is cloth armor based on its armor component material type.
        /// </summary>
        /// <param name="item">The item to check.</param>
        /// <returns>True if the item is cloth armor; false otherwise.</returns>
        public static bool IsClothArmor(ItemObject item)
        {
            if (item == null)
                return false;

            if (!item.HasArmorComponent)
                return false;

            ArmorComponent armorComponent = item.ArmorComponent;
            if (armorComponent == null)
                return false;

            return armorComponent.MaterialType == ArmorComponent.ArmorMaterialTypes.Cloth;
        }

        #region Civilian Item Methods

        /// <summary>
        /// Keywords that identify items as civilian appropriate (even without IsCivilian flag).
        /// </summary>
        private static readonly string[] CivilianKeywords = new[]
        {
            "tunic",
            "shirt",
            "robe",
            "gown",
            "pants",
            "trousers",
            "sandals",
            "shoes",
            "slippers",
            "boots",
            "cloak",
            "cape",
            "hood",
            "veil",
            "headscarf",
            "gloves",
            "wrappings",
            "bracers"
        };

        /// <summary>
        /// Keywords that identify items as combat-specific (NOT civilian appropriate).
        /// </summary>
        private static readonly string[] CombatOnlyKeywords = new[]
        {
            "armor",
            "mail",
            "scale",
            "plate",
            "lamellar",
            "brigandine",
            "chainmail",
            "hauberk",
            "coif",
            "helm",
            "helmet",
            "visor",
            "gauntlet",
            "greaves",
            "pauldron",
            "cuirass",
            "vambrace"
        };

        /// MARK: IsCivilianAppropriateItem
        /// <summary>
        /// Determines if an item is suitable for civilian equipment.
        /// Civilian appropriate items include:
        /// - Items explicitly marked as IsCivilian by the game
        /// - Dresses, ladies shoes, civilian crowns
        /// - Cloth and leather items without combat keywords
        /// - Items with civilian keywords (tunic, shirt, pants, etc.)
        /// Excludes items with combat keywords (armor, mail, helm, etc.)
        /// </summary>
        /// <param name="item">The item to check.</param>
        /// <returns>True if the item is suitable for civilian equipment; false otherwise.</returns>
        public static bool IsCivilianAppropriateItem(ItemObject item)
        {
            if (item == null)
                return false;

            // Items marked as civilian by the game are always appropriate
            if (item.IsCivilian)
                return true;

            // Dresses are civilian appropriate
            if (IsDressItem(item))
                return true;

            // Ladies shoes are civilian appropriate
            if (IsLadiesShoes(item))
                return true;

            // Civilian crowns are appropriate
            if (IsCivilianCrown(item))
                return true;

            string itemName = item.Name?.ToString() ?? string.Empty;
            string itemStringId = item.StringId ?? string.Empty;

            // Check for combat-only keywords - these are NOT civilian appropriate
            foreach (string keyword in CombatOnlyKeywords)
            {
                if (itemName.IndexOf(keyword, System.StringComparison.OrdinalIgnoreCase) >= 0)
                    return false;
                if (itemStringId.IndexOf(keyword, System.StringComparison.OrdinalIgnoreCase) >= 0)
                    return false;
            }

            // Check for civilian keywords - these ARE civilian appropriate
            foreach (string keyword in CivilianKeywords)
            {
                if (itemName.IndexOf(keyword, System.StringComparison.OrdinalIgnoreCase) >= 0)
                    return true;
                if (itemStringId.IndexOf(keyword, System.StringComparison.OrdinalIgnoreCase) >= 0)
                    return true;
            }

            // Cloth armor without combat keywords is civilian appropriate
            if (IsClothArmor(item))
                return true;

            // Leather armor at low tiers (0-2) without combat keywords is often civilian
            if (item.HasArmorComponent && item.ArmorComponent != null)
            {
                if (item.ArmorComponent.MaterialType == ArmorComponent.ArmorMaterialTypes.Leather)
                {
                    if (item.Tier <= ItemObject.ItemTiers.Tier2)
                        return true;
                }
            }

            return false;
        }

        /// MARK: IsDressItem
        /// <summary>
        /// Determines if an item is a dress based on name or StringId containing "dress".
        /// </summary>
        /// <param name="item">The item to check.</param>
        /// <returns>True if the item is a dress; false otherwise.</returns>
        public static bool IsDressItem(ItemObject item)
        {
            if (item == null)
                return false;

            string itemName = item.Name?.ToString() ?? string.Empty;
            string itemStringId = item.StringId ?? string.Empty;

            if (itemName.IndexOf("dress", System.StringComparison.OrdinalIgnoreCase) >= 0)
                return true;

            if (itemStringId.IndexOf("dress", System.StringComparison.OrdinalIgnoreCase) >= 0)
                return true;

            return false;
        }

        /// MARK: IsLadiesShoes
        /// <summary>
        /// Determines if an item is ladies shoes based on name containing "Ladies Shoes" or "Lady Shoes".
        /// </summary>
        /// <param name="item">The item to check.</param>
        /// <returns>True if the item is ladies shoes; false otherwise.</returns>
        public static bool IsLadiesShoes(ItemObject item)
        {
            if (item == null)
                return false;

            string itemName = item.Name?.ToString() ?? string.Empty;

            if (itemName.IndexOf("Ladies Shoes", System.StringComparison.OrdinalIgnoreCase) >= 0)
                return true;

            if (itemName.IndexOf("Lady Shoes", System.StringComparison.OrdinalIgnoreCase) >= 0)
                return true;

            return false;
        }

        #endregion

        #region Crown Detection Methods

        /// MARK: IsCrownItem
        /// <summary>
        /// Determines if an item is a crown based on crown keywords or special crown item identifiers.
        /// </summary>
        /// <param name="item">The item to check.</param>
        /// <returns>True if the item is a crown; false otherwise.</returns>
        public static bool IsCrownItem(ItemObject item)
        {
            if (item == null)
                return false;

            string itemName = item.Name?.ToString() ?? string.Empty;
            string itemStringId = item.StringId ?? string.Empty;

            // Check crown keywords
            foreach (string keyword in CrownKeywords)
            {
                if (itemName.IndexOf(keyword, System.StringComparison.OrdinalIgnoreCase) >= 0)
                    return true;

                if (itemStringId.IndexOf(keyword, System.StringComparison.OrdinalIgnoreCase) >= 0)
                    return true;
            }

            // Check special crown items
            foreach (string specialItem in SpecialCrownItems)
            {
                if (itemStringId.Equals(specialItem, System.StringComparison.OrdinalIgnoreCase))
                    return true;
            }

            return false;
        }

        /// MARK: IsBattleCrown
        /// <summary>
        /// Determines if an item is a battle crown (crown with "battle" or "helmet" in name).
        /// Battle crowns are suitable for combat equipment.
        /// </summary>
        /// <param name="item">The item to check.</param>
        /// <returns>True if the item is a battle crown; false otherwise.</returns>
        public static bool IsBattleCrown(ItemObject item)
        {
            if (item == null)
                return false;

            if (!IsCrownItem(item))
                return false;

            string itemName = item.Name?.ToString() ?? string.Empty;
            string itemStringId = item.StringId ?? string.Empty;

            // Check for battle keyword
            if (itemName.IndexOf("battle", System.StringComparison.OrdinalIgnoreCase) >= 0)
                return true;

            if (itemStringId.IndexOf("battle", System.StringComparison.OrdinalIgnoreCase) >= 0)
                return true;

            // Check for helmet keyword
            if (itemName.IndexOf("helmet", System.StringComparison.OrdinalIgnoreCase) >= 0)
                return true;

            if (itemStringId.IndexOf("helmet", System.StringComparison.OrdinalIgnoreCase) >= 0)
                return true;

            return false;
        }

        /// MARK: IsCivilianCrown
        /// <summary>
        /// Determines if an item is a civilian crown (crown without "battle" or "helm" in name).
        /// Civilian crowns are suitable for non-combat equipment.
        /// </summary>
        /// <param name="item">The item to check.</param>
        /// <returns>True if the item is a civilian crown; false otherwise.</returns>
        public static bool IsCivilianCrown(ItemObject item)
        {
            if (item == null)
                return false;

            if (!IsCrownItem(item))
                return false;

            string itemName = item.Name?.ToString() ?? string.Empty;
            string itemStringId = item.StringId ?? string.Empty;

            // If contains "battle" it's not civilian
            if (itemName.IndexOf("battle", System.StringComparison.OrdinalIgnoreCase) >= 0)
                return false;

            if (itemStringId.IndexOf("battle", System.StringComparison.OrdinalIgnoreCase) >= 0)
                return false;

            // If contains "helm" it's not civilian
            if (itemName.IndexOf("helm", System.StringComparison.OrdinalIgnoreCase) >= 0)
                return false;

            if (itemStringId.IndexOf("helm", System.StringComparison.OrdinalIgnoreCase) >= 0)
                return false;

            return true;
        }

        /// MARK: IsJeweledCrown
        /// <summary>
        /// Determines if an item is a jeweled crown based on name containing "Jeweled Crown".
        /// </summary>
        /// <param name="item">The item to check.</param>
        /// <returns>True if the item is a jeweled crown; false otherwise.</returns>
        public static bool IsJeweledCrown(ItemObject item)
        {
            if (item == null)
                return false;

            string itemName = item.Name?.ToString() ?? string.Empty;

            if (itemName.IndexOf("Jeweled Crown", System.StringComparison.OrdinalIgnoreCase) >= 0)
                return true;

            return false;
        }

        #endregion

        #region Weapon Type Detection Methods

        /// MARK: IsPolearmWeapon
        /// <summary>
        /// Determines if a weapon item is a polearm (including one-handed polearms).
        /// Polearms always require a sidearm weapon for backup melee combat.
        /// </summary>
        /// <param name="item">The weapon item to check.</param>
        /// <returns>True if the item is a polearm; false otherwise.</returns>
        public static bool IsPolearmWeapon(ItemObject item)
        {
            if (item == null)
                return false;

            if (!item.HasWeaponComponent)
                return false;

            WeaponComponentData primaryWeapon = item.PrimaryWeapon;
            if (primaryWeapon == null)
                return false;

            // Check weapon class for polearm types
            WeaponClass weaponClass = primaryWeapon.WeaponClass;
            
            // Polearm weapon classes (NOT throwing weapons like Javelin, ThrowingAxe, ThrowingKnife)
            return weaponClass == WeaponClass.OneHandedPolearm ||
                   weaponClass == WeaponClass.TwoHandedPolearm ||
                   weaponClass == WeaponClass.LowGripPolearm;
        }

        /// MARK: IsSidearmWeapon
        /// <summary>
        /// Determines if a weapon item is a sidearm (one-handed sword, axe, mace, or dagger).
        /// Sidearms are backup weapons used when polearm is not effective (close combat).
        /// </summary>
        /// <param name="item">The weapon item to check.</param>
        /// <returns>True if the item is a sidearm; false otherwise.</returns>
        public static bool IsSidearmWeapon(ItemObject item)
        {
            if (item == null)
                return false;

            if (!item.HasWeaponComponent)
                return false;

            WeaponComponentData primaryWeapon = item.PrimaryWeapon;
            if (primaryWeapon == null)
                return false;

            // Check weapon class for sidearm types
            WeaponClass weaponClass = primaryWeapon.WeaponClass;
            
            // One-handed sidearm weapon classes (not polearms, not two-handed, not ranged)
            return weaponClass == WeaponClass.OneHandedSword ||
                   weaponClass == WeaponClass.OneHandedAxe ||
                   weaponClass == WeaponClass.Mace ||
                   weaponClass == WeaponClass.Dagger;
        }

        #endregion

        #region Hero Validation Methods

        /// MARK: IsRulingClanMember
        /// <summary>
        /// Determines if a hero is a member of the ruling clan of their kingdom.
        /// </summary>
        /// <param name="hero">The hero to check.</param>
        /// <returns>True if the hero is in the ruling clan of their kingdom; false otherwise.</returns>
        public static bool IsRulingClanMember(Hero hero)
        {
            if (hero == null)
                return false;

            Clan herosClan = hero.Clan;
            if (herosClan == null)
                return false;

            Kingdom kingdom = herosClan.Kingdom;
            if (kingdom == null)
                return false;

            Clan rulingClan = kingdom.RulingClan;
            if (rulingClan == null)
                return false;

            return herosClan == rulingClan;
        }

        #endregion

        #region Appearance Validation Methods

        /// MARK: MeetsAppearanceThreshold
        /// <summary>
        /// Checks if an item meets the specified appearance threshold.
        /// </summary>
        /// <param name="item">The item to check.</param>
        /// <param name="minAppearance">The minimum appearance value (exclusive).</param>
        /// <returns>True if item.Appearance > minAppearance; false otherwise.</returns>
        public static bool MeetsAppearanceThreshold(ItemObject item, float minAppearance)
        {
            if (item == null)
                return false;

            return item.Appearance > minAppearance;
        }

        /// MARK: MeetsCivilianAppearance
        /// <summary>
        /// Checks if an item meets civilian appearance requirements based on hero status.
        /// </summary>
        /// <param name="item">The item to check.</param>
        /// <param name="isRulingClanMember">Whether the hero is a member of a ruling clan.</param>
        /// <returns>True if item meets appearance requirements; false otherwise.</returns>
        public static bool MeetsCivilianAppearanceRequirement(ItemObject item, bool isRulingClanMember)
        {
            if (item == null)
                return false;

            float threshold = isRulingClanMember ? MinimumRoyalAppearance : MinimumCivilianAppearance;
            return item.Appearance > threshold;
        }

        /// MARK: MeetsBattleArmorAppearance
        /// <summary>
        /// Checks if an armor item meets battle appearance requirements.
        /// </summary>
        /// <param name="item">The item to check.</param>
        /// <returns>True if item.Appearance > MinimumBattleArmorAppearance; false otherwise.</returns>
        public static bool MeetsBattleArmorAppearanceRequirement(ItemObject item)
        {
            if (item == null)
                return false;

            return item.Appearance > MinimumBattleArmorAppearance;
        }

        #endregion

        #region Private Validation Methods

        /// MARK: IsValidWeapon
        /// <summary>
        /// Validates a weapon item by checking for damage stats.
        /// Shields use BodyArmor stat instead of damage stats.
        /// </summary>
        /// <param name="item">The weapon item to validate.</param>
        /// <returns>True if the weapon has valid stats; false otherwise.</returns>
        private static bool IsValidWeapon(ItemObject item)
        {
            WeaponComponentData primaryWeapon = item.PrimaryWeapon;
            if (primaryWeapon == null)
                return false;

            // Shields use BodyArmor stat, not damage stats
            if (item.ItemType == ItemObject.ItemTypeEnum.Shield)
            {
                return primaryWeapon.BodyArmor > 0;
            }

            // Regular weapons need at least one damage type
            bool hasSwingDamage = primaryWeapon.SwingDamage > 0;
            bool hasThrustDamage = primaryWeapon.ThrustDamage > 0;
            bool hasMissileDamage = primaryWeapon.MissileDamage > 0;

            return hasSwingDamage || hasThrustDamage || hasMissileDamage;
        }

        /// MARK: IsValidArmor
        /// <summary>
        /// Validates an armor item by checking for armor stats.
        /// At least one armor value must be greater than zero.
        /// </summary>
        /// <param name="item">The armor item to validate.</param>
        /// <returns>True if the armor has valid stats; false otherwise.</returns>
        private static bool IsValidArmor(ItemObject item)
        {
            ArmorComponent armorComponent = item.ArmorComponent;
            if (armorComponent == null)
                return false;

            // Check if any armor stat is greater than zero
            bool hasHeadArmor = armorComponent.HeadArmor > 0;
            bool hasBodyArmor = armorComponent.BodyArmor > 0;
            bool hasArmArmor = armorComponent.ArmArmor > 0;
            bool hasLegArmor = armorComponent.LegArmor > 0;

            return hasHeadArmor || hasBodyArmor || hasArmArmor || hasLegArmor;
        }

        /// MARK: IsValidHorse
        /// <summary>
        /// Validates a horse item by checking for horse stats.
        /// At least one stat (Speed, Maneuver, ChargeDamage) must be greater than zero.
        /// </summary>
        /// <param name="item">The horse item to validate.</param>
        /// <returns>True if the horse has valid stats; false otherwise.</returns>
        private static bool IsValidHorse(ItemObject item)
        {
            HorseComponent horseComponent = item.HorseComponent;
            if (horseComponent == null)
                return false;

            // Check if any horse stat is greater than zero
            bool hasSpeed = horseComponent.Speed > 0;
            bool hasManeuver = horseComponent.Maneuver > 0;
            bool hasChargeDamage = horseComponent.ChargeDamage > 0;

            return hasSpeed || hasManeuver || hasChargeDamage;
        }

        #endregion
    }
}
