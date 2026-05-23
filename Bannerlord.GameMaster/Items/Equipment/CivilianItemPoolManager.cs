using System.Collections.Generic;
using Bannerlord.GameMaster.Common;
using Bannerlord.GameMaster.Information;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Extensions;
using TaleWorlds.Core;
using TaleWorlds.Library;

namespace Bannerlord.GameMaster.Items
{
    /// <summary>
    /// Singleton manager that extracts civilian equipment items directly from game equipment rosters
    /// based on their EquipmentCategories. This provides more authentic and culture-appropriate civilian
    /// outfits compared to heuristic-based item selection.
    ///
    /// Items from lord rosters (IsLordTemplate) are extracted into both the regular pools AND separate
    /// lord-specific pools. At selection time, heroes level 15+ prefer the lord-only pool for higher
    /// quality civilian gear, while lower level heroes draw from the combined pool.
    /// </summary>
    public sealed class CivilianItemPoolManager
    {
        private static CivilianItemPoolManager _instance;
        private static readonly object _lock = new();

        /// <summary>
        /// Gets the singleton instance of the CivilianItemPoolManager.
        /// </summary>
        public static CivilianItemPoolManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_lock)
                    {
                        if (_instance == null)
                        {
                            _instance = new();
                        }
                    }
                }
                return _instance;
            }
        }

        private CivilianItemPoolManager()
        {
            InitializePoolStructures();
        }

        private bool _initialized;
        private int _rostersProcessed;
        private int _itemsExtracted;

        /// <summary>
        /// Minimum hero level to prefer lord-only civilian item pools.
        /// Heroes at or above this level draw from lord pools first for higher quality gear.
        /// </summary>
        private const int LordPoolMinLevel = 15;

        #region Peasant Roster Prefixes

        // Roster prefixes to exclude (peasant/commoner equipment)
        private static readonly string[] PeasantRosterPrefixes = new[]
        {
            "townswoman_",
            "villager_",
            "townsman_",
            "spc_brotherhood_of_woods_"
        };

        #endregion

        #region Lord Culture Mapping

        // Culture mapping for lord_X_ prefixed rosters
        // Based on Bannerlord's internal numbering system
        private static readonly Dictionary<string, string> LordCultureMapping = new()
        {
            { "lord_1_", "empire" },
            { "lord_2_", "sturgia" },
            { "lord_3_", "aserai" },
            { "lord_4_", "vlandia" },
            { "lord_5_", "battania" },
            { "lord_6_", "khuzait" }
        };

        #endregion

        #region Pools

        // Female civilian item pools (combined: all rosters including lord)
        // Key: CultureId -> EquipmentIndex -> Items
        private Dictionary<string, Dictionary<EquipmentIndex, MBList<ItemObject>>> _femaleCivilianPools;

        // Male civilian item pools (combined: all rosters including lord)
        // Key: CultureId -> EquipmentIndex -> Items
        private Dictionary<string, Dictionary<EquipmentIndex, MBList<ItemObject>>> _maleCivilianPools;

        // Female lord-specific civilian pools (IsLordTemplate rosters only)
        // Key: CultureId -> EquipmentIndex -> Items
        private Dictionary<string, Dictionary<EquipmentIndex, MBList<ItemObject>>> _femaleLordCivilianPools;

        // Male lord-specific civilian pools (IsLordTemplate rosters only)
        // Key: CultureId -> EquipmentIndex -> Items
        private Dictionary<string, Dictionary<EquipmentIndex, MBList<ItemObject>>> _maleLordCivilianPools;

        // Crown pools for ruling clan members (combined)
        // Key: CultureId -> Items
        private Dictionary<string, MBList<ItemObject>> _femaleCrownPools;
        private Dictionary<string, MBList<ItemObject>> _maleCrownPools;

        // Lord-specific crown pools
        private Dictionary<string, MBList<ItemObject>> _femaleLordCrownPools;
        private Dictionary<string, MBList<ItemObject>> _maleLordCrownPools;

        // Civilian weapon pools (one-handed melee for males, combined)
        // Key: CultureId -> Items
        private Dictionary<string, MBList<ItemObject>> _civilianWeaponPools;

        // Lord-specific civilian weapon pools
        private Dictionary<string, MBList<ItemObject>> _civilianLordWeaponPools;

        // Fallback pools for cultures without specific items (combined)
        private Dictionary<EquipmentIndex, MBList<ItemObject>> _fallbackFemalePools;
        private Dictionary<EquipmentIndex, MBList<ItemObject>> _fallbackMalePools;
        private MBList<ItemObject> _fallbackFemaleCrowns;
        private MBList<ItemObject> _fallbackMaleCrowns;
        private MBList<ItemObject> _fallbackWeapons;

        // Lord-specific fallback pools
        private Dictionary<EquipmentIndex, MBList<ItemObject>> _fallbackFemaleLordPools;
        private Dictionary<EquipmentIndex, MBList<ItemObject>> _fallbackMaleLordPools;
        private MBList<ItemObject> _fallbackFemaleLordCrowns;
        private MBList<ItemObject> _fallbackMaleLordCrowns;
        private MBList<ItemObject> _fallbackLordWeapons;

        #endregion

        #region Properties

        /// <summary>
        /// Gets whether the civilian pools have been initialized.
        /// </summary>
        public bool IsInitialized => _initialized;

        /// <summary>
        /// Gets the total number of rosters processed during initialization.
        /// </summary>
        public int RostersProcessed => _rostersProcessed;

        /// <summary>
        /// Gets the number of items extracted to pools.
        /// </summary>
        public int ItemsExtracted => _itemsExtracted;

        #endregion

        /// MARK: Initialize
        /// <summary>
        /// Initializes or reinitializes civilian item pools by scanning equipment rosters.
        /// Call this after game data is fully loaded.
        /// </summary>
        public void Initialize()
        {
            lock (_lock)
            {
                if (_initialized)
                    return;

                // Reset counters
                _rostersProcessed = 0;
                _itemsExtracted = 0;

                // Get all equipment rosters
                MBReadOnlyList<MBEquipmentRoster> allRosters = MBEquipmentRosterExtensions.All;
                if (allRosters == null || allRosters.Count == 0)
                {
                    BLGMResult.Error("CivilianItemPoolManager.Initialize() failed: No equipment rosters found").Log();
                    return;
                }

                // Process each roster
                for (int i = 0; i < allRosters.Count; i++)
                {
                    MBEquipmentRoster roster = allRosters[i];
                    ProcessRoster(roster);
                    _rostersProcessed++;
                }

                _initialized = true;
                BLGMResult.Success($"CivilianItemPoolManager initialized: {_itemsExtracted} items from {_rostersProcessed} rosters").Log();
            }
        }

        /// MARK: Clear
        /// <summary>
        /// Clears all civilian pools and resets initialization state.
        /// </summary>
        public void Clear()
        {
            lock (_lock)
            {
                InitializePoolStructures();
                _initialized = false;
                _rostersProcessed = 0;
                _itemsExtracted = 0;
            }
        }

        /// MARK: Reinitialize
        /// <summary>
        /// Reinitializes the civilian pools by clearing and rebuilding them.
        /// </summary>
        public void Reinitialize()
        {
            Clear();
            Initialize();
        }

        /// MARK: InitializePoolStructures
        private void InitializePoolStructures()
        {
            // Combined pools (all rosters)
            _femaleCivilianPools = new();
            _maleCivilianPools = new();
            _femaleCrownPools = new();
            _maleCrownPools = new();
            _civilianWeaponPools = new();
            _fallbackFemalePools = new();
            _fallbackMalePools = new();
            _fallbackFemaleCrowns = new();
            _fallbackMaleCrowns = new();
            _fallbackWeapons = new();

            // Lord-specific pools
            _femaleLordCivilianPools = new();
            _maleLordCivilianPools = new();
            _femaleLordCrownPools = new();
            _maleLordCrownPools = new();
            _civilianLordWeaponPools = new();
            _fallbackFemaleLordPools = new();
            _fallbackMaleLordPools = new();
            _fallbackFemaleLordCrowns = new();
            _fallbackMaleLordCrowns = new();
            _fallbackLordWeapons = new();

            _initialized = false;
        }

        /// MARK: ProcessRoster
        /// <summary>
        /// Processes a single equipment roster and extracts civilian items.
        /// All non-peasant rosters with a resolvable culture are processed.
        /// Gender is determined by the IsFemaleTemplate category flag.
        /// Items from lord rosters are extracted into both the regular pools and lord-specific pools.
        /// Civilian filtering is done at the individual equipment set level via equipment.IsCivilian.
        /// </summary>
        private void ProcessRoster(MBEquipmentRoster roster)
        {
            if (roster == null)
                return;

            // Exclude peasant rosters (they contain peasant items)
            if (IsPeasantRoster(roster))
                return;

            // Resolve culture (handles lord_ rosters with null culture)
            string cultureId = ResolveCultureId(roster);
            if (cultureId == null)
                return;

            bool isFemale = (roster.EquipmentCategories & EquipmentCategories.IsFemaleTemplate) != 0;
            bool isLord = (roster.EquipmentCategories & EquipmentCategories.IsLordTemplate) != 0;

            // Always extract to combined (regular) pools
            ExtractCivilianItems(roster, cultureId, isFemale, isLordPool: false);

            // Additionally extract to lord-specific pools for lord rosters
            if (isLord)
            {
                ExtractCivilianItems(roster, cultureId, isFemale, isLordPool: true);
            }
        }

        /// MARK: IsPeasantRoster
        /// <summary>
        /// Checks if the roster is a peasant roster that should be excluded.
        /// These rosters contain peasant items that are not suitable for noble/lord civilian equipment.
        /// </summary>
        private bool IsPeasantRoster(MBEquipmentRoster roster)
        {
            string rosterId = roster.StringId;
            if (string.IsNullOrEmpty(rosterId))
                return false;

            for (int i = 0; i < PeasantRosterPrefixes.Length; i++)
            {
                if (rosterId.StartsWith(PeasantRosterPrefixes[i]))
                    return true;
            }

            return false;
        }

        /// MARK: ResolveCultureId
        /// <summary>
        /// Resolves the culture ID for a roster. For lord_ rosters with null culture,
        /// maps them to the correct culture based on the numeric prefix.
        /// </summary>
        private string ResolveCultureId(MBEquipmentRoster roster)
        {
            // If roster has a culture, use it
            if (roster.EquipmentCulture != null)
                return roster.EquipmentCulture.StringId;

            // For lord_ rosters with null culture, map by prefix
            string rosterId = roster.StringId;
            if (string.IsNullOrEmpty(rosterId) || !rosterId.StartsWith("lord_"))
                return null;

            // Culture mapping based on lord_X_ prefix
            foreach (KeyValuePair<string, string> mapping in LordCultureMapping)
            {
                if (rosterId.StartsWith(mapping.Key))
                    return mapping.Value;
            }

            return null;
        }

        /// MARK: ExtractCivilianItems
        /// <summary>
        /// Extracts civilian equipment items from a roster into the appropriate pools.
        /// When isLordPool is false, items go to the combined pools (regular + lord items together).
        /// When isLordPool is true, items go to the lord-specific pools (for level 15+ selection).
        /// </summary>
        private void ExtractCivilianItems(MBEquipmentRoster roster, string cultureId, bool isFemale, bool isLordPool)
        {
            MBReadOnlyList<Equipment> allEquipments = roster.AllEquipments;
            if (allEquipments == null || allEquipments.Count == 0)
                return;

            // Select target pools based on isLordPool flag
            Dictionary<string, Dictionary<EquipmentIndex, MBList<ItemObject>>> targetPools =
                isLordPool
                    ? (isFemale ? _femaleLordCivilianPools : _maleLordCivilianPools)
                    : (isFemale ? _femaleCivilianPools : _maleCivilianPools);
            Dictionary<string, MBList<ItemObject>> targetCrownPools =
                isLordPool
                    ? (isFemale ? _femaleLordCrownPools : _maleLordCrownPools)
                    : (isFemale ? _femaleCrownPools : _maleCrownPools);
            Dictionary<EquipmentIndex, MBList<ItemObject>> fallbackPools =
                isLordPool
                    ? (isFemale ? _fallbackFemaleLordPools : _fallbackMaleLordPools)
                    : (isFemale ? _fallbackFemalePools : _fallbackMalePools);
            MBList<ItemObject> fallbackCrowns =
                isLordPool
                    ? (isFemale ? _fallbackFemaleLordCrowns : _fallbackMaleLordCrowns)
                    : (isFemale ? _fallbackFemaleCrowns : _fallbackMaleCrowns);
            Dictionary<string, MBList<ItemObject>> targetWeaponPools =
                isLordPool ? _civilianLordWeaponPools : _civilianWeaponPools;
            MBList<ItemObject> fallbackWeaponPool =
                isLordPool ? _fallbackLordWeapons : _fallbackWeapons;

            // Ensure culture dictionaries exist
            EnsureCulturePoolsExist(cultureId, targetPools);
            if (!targetCrownPools.ContainsKey(cultureId))
            {
                targetCrownPools[cultureId] = new();
            }

            if (!targetWeaponPools.ContainsKey(cultureId))
            {
                targetWeaponPools[cultureId] = new();
            }

            // Process each equipment set in the roster
            for (int equipIdx = 0; equipIdx < allEquipments.Count; equipIdx++)
            {
                Equipment equipment = allEquipments[equipIdx];

                // Only extract items from civilian equipment sets
                if (!equipment.IsCivilian)
                    continue;

                // Extract armor items
                ExtractArmorFromEquipment(equipment, cultureId, isFemale, targetPools, targetCrownPools, fallbackPools, fallbackCrowns);

                // Extract weapons (only for male pools since females don't carry civilian weapons)
                if (!isFemale)
                {
                    ExtractWeaponsFromEquipment(equipment, cultureId, targetWeaponPools, fallbackWeaponPool);
                }
            }
        }

        /// MARK: ExtractArmorFromEquipment
        /// <summary>
        /// Extracts armor items from a single equipment set.
        /// Filters items by gender suitability to prevent female-only items (e.g., "Ladies Shoes")
        /// from being added to male pools.
        /// </summary>
        private void ExtractArmorFromEquipment(
            Equipment equipment,
            string cultureId,
            bool isFemale,
            Dictionary<string, Dictionary<EquipmentIndex, MBList<ItemObject>>> targetPools,
            Dictionary<string, MBList<ItemObject>> crownPools,
            Dictionary<EquipmentIndex, MBList<ItemObject>> fallbackPools,
            MBList<ItemObject> fallbackCrowns)
        {
            // Armor slots to extract
            EquipmentIndex[] armorSlots = new[]
            {
                EquipmentIndex.Head,
                EquipmentIndex.Cape,
                EquipmentIndex.Body,
                EquipmentIndex.Gloves,
                EquipmentIndex.Leg
            };

            foreach (EquipmentIndex slot in armorSlots)
            {
                EquipmentElement element = equipment[slot];
                if (element.IsEmpty)
                    continue;

                ItemObject item = element.Item;
                if (item == null)
                    continue;

                // Filter out gender-inappropriate items (e.g., "Ladies Shoes" from male pools)
                if (!ItemValidation.IsArmorSuitableForGender(item, isFemale))
                    continue;

                // Check if this is a crown
                bool isCrown = ItemValidation.IsCrownItem(item);

                if (isCrown && slot == EquipmentIndex.Head)
                {
                    // Add to crown pool
                    if (!crownPools[cultureId].Contains(item))
                    {
                        crownPools[cultureId].Add(item);
                        _itemsExtracted++;
                    }

                    // Also add to fallback
                    if (!fallbackCrowns.Contains(item))
                    {
                        fallbackCrowns.Add(item);
                    }
                }
                else
                {
                    // Add to regular pool (but exclude crowns from head slot for non-ruling clan)
                    // Crowns should only be in crown pools
                    if (!isCrown || slot != EquipmentIndex.Head)
                    {
                        if (!targetPools[cultureId][slot].Contains(item))
                        {
                            targetPools[cultureId][slot].Add(item);
                            _itemsExtracted++;
                        }

                        // Also add to fallback
                        EnsureSlotPoolExists(slot, fallbackPools);
                        if (!fallbackPools[slot].Contains(item))
                        {
                            fallbackPools[slot].Add(item);
                        }
                    }
                }
            }
        }

        /// MARK: ExtractWeaponsFromEquipment
        /// <summary>
        /// Extracts one-handed civilian weapons from equipment for male characters.
        /// </summary>
        private void ExtractWeaponsFromEquipment(
            Equipment equipment,
            string cultureId,
            Dictionary<string, MBList<ItemObject>> targetWeaponPools,
            MBList<ItemObject> fallbackWeaponPool)
        {
            // Check weapon slots
            for (int i = 0; i < 4; i++)
            {
                EquipmentIndex slot = (EquipmentIndex)i;
                EquipmentElement element = equipment[slot];
                if (element.IsEmpty)
                    continue;

                ItemObject item = element.Item;
                if (item == null)
                    continue;

                // Only one-handed melee weapons for civilian equipment
                if (!item.HasWeaponComponent)
                    continue;

                WeaponComponentData primaryWeapon = item.PrimaryWeapon;
                if (primaryWeapon == null)
                    continue;

                // Check if one-handed melee
                WeaponClass weaponClass = primaryWeapon.WeaponClass;
                bool isOneHandedMelee = weaponClass == WeaponClass.OneHandedSword ||
                                        weaponClass == WeaponClass.OneHandedAxe ||
                                        weaponClass == WeaponClass.Mace ||
                                        weaponClass == WeaponClass.Dagger;

                if (isOneHandedMelee)
                {
                    if (!targetWeaponPools[cultureId].Contains(item))
                    {
                        targetWeaponPools[cultureId].Add(item);
                        _itemsExtracted++;
                    }

                    // Also add to fallback
                    if (!fallbackWeaponPool.Contains(item))
                    {
                        fallbackWeaponPool.Add(item);
                    }
                }
            }
        }

        #region Pool Access Methods

        /// MARK: GetRandomItem
        /// <summary>
        /// Gets a random item from the appropriate civilian pool for the specified slot.
        /// Applies appearance filtering based on hero status (ruling clan members get higher appearance items).
        /// Falls back to other cultures if the specific culture has no items.
        /// </summary>
        /// <param name="cultureId">The culture ID to select from.</param>
        /// <param name="isFemale">Whether the hero is female.</param>
        /// <param name="slot">The equipment slot.</param>
        /// <param name="isRulingClanMember">Whether the hero is a member of a ruling clan.</param>
        /// <param name="appearanceBonus">Additional appearance requirement (0 or 1) for higher quality items.</param>
        /// <returns>A random item from the pool that meets appearance requirements, or null if none available.</returns>
        public ItemObject GetRandomItem(string cultureId, bool isFemale, EquipmentIndex slot, bool isRulingClanMember = false, int appearanceBonus = 0)
        {
            return GetRandomItem(cultureId, isFemale, slot, isRulingClanMember, appearanceBonus, heroLevel: -1);
        }

        /// MARK: GetRandomItem (level-gated)
        /// <summary>
        /// Gets a random item from the appropriate civilian pool for the specified slot.
        /// For heroes at level 15+, prefers lord-specific pools for higher quality gear.
        /// Falls back to combined pools if lord pool has no suitable items.
        /// </summary>
        /// <param name="cultureId">The culture ID to select from.</param>
        /// <param name="isFemale">Whether the hero is female.</param>
        /// <param name="slot">The equipment slot.</param>
        /// <param name="isRulingClanMember">Whether the hero is a member of a ruling clan.</param>
        /// <param name="appearanceBonus">Additional appearance requirement (0 or 1) for higher quality items.</param>
        /// <param name="heroLevel">The hero's level. At 15+, lord-specific pools are preferred. Use -1 to skip level gating.</param>
        /// <returns>A random item from the pool that meets appearance requirements, or null if none available.</returns>
        public ItemObject GetRandomItem(string cultureId, bool isFemale, EquipmentIndex slot, bool isRulingClanMember, int appearanceBonus, int heroLevel)
        {
            EnsureInitialized();

            // Level 15+: try lord-specific pools first
            if (heroLevel >= LordPoolMinLevel)
            {
                ItemObject lordItem = SelectFromPools(cultureId, isFemale, slot, isRulingClanMember, appearanceBonus, useLordPools: true);
                if (lordItem != null)
                    return lordItem;
            }

            // Use combined pools (all items including lord items)
            return SelectFromPools(cultureId, isFemale, slot, isRulingClanMember, appearanceBonus, useLordPools: false);
        }

        /// MARK: SelectFromPools
        /// <summary>
        /// Selects a random item from the specified pool type (lord or combined) with appearance filtering.
        /// </summary>
        private ItemObject SelectFromPools(string cultureId, bool isFemale, EquipmentIndex slot, bool isRulingClanMember, int appearanceBonus, bool useLordPools)
        {
            Dictionary<string, Dictionary<EquipmentIndex, MBList<ItemObject>>> pools =
                useLordPools
                    ? (isFemale ? _femaleLordCivilianPools : _maleLordCivilianPools)
                    : (isFemale ? _femaleCivilianPools : _maleCivilianPools);

            // Try culture-specific pool first
            if (cultureId != null &&
                pools.TryGetValue(cultureId, out Dictionary<EquipmentIndex, MBList<ItemObject>> culturePools) &&
                culturePools.TryGetValue(slot, out MBList<ItemObject> items) &&
                items.Count > 0)
            {
                // Filter by appearance with optional bonus
                MBList<ItemObject> filteredItems = appearanceBonus > 0
                    ? FilterByAppearanceWithBonus(items, isRulingClanMember, appearanceBonus)
                    : FilterByAppearance(items, isRulingClanMember);
                if (filteredItems.Count > 0)
                    return SelectRandomItem(filteredItems);

                // Fallback: standard appearance filter (without bonus)
                if (appearanceBonus > 0)
                {
                    filteredItems = FilterByAppearance(items, isRulingClanMember);
                    if (filteredItems.Count > 0)
                        return SelectRandomItem(filteredItems);
                }

                // Fallback: no appearance filter
                return SelectRandomItem(items);
            }

            // Fallback to generic pool
            Dictionary<EquipmentIndex, MBList<ItemObject>> fallbackPools =
                useLordPools
                    ? (isFemale ? _fallbackFemaleLordPools : _fallbackMaleLordPools)
                    : (isFemale ? _fallbackFemalePools : _fallbackMalePools);

            if (fallbackPools.TryGetValue(slot, out MBList<ItemObject> fallbackItems) &&
                fallbackItems.Count > 0)
            {
                // Filter by appearance with optional bonus
                MBList<ItemObject> filteredFallback = appearanceBonus > 0
                    ? FilterByAppearanceWithBonus(fallbackItems, isRulingClanMember, appearanceBonus)
                    : FilterByAppearance(fallbackItems, isRulingClanMember);
                if (filteredFallback.Count > 0)
                    return SelectRandomItem(filteredFallback);

                // Fallback: standard appearance filter (without bonus)
                if (appearanceBonus > 0)
                {
                    filteredFallback = FilterByAppearance(fallbackItems, isRulingClanMember);
                    if (filteredFallback.Count > 0)
                        return SelectRandomItem(filteredFallback);
                }

                // Fallback: no appearance filter
                return SelectRandomItem(fallbackItems);
            }

            return null;
        }

        /// MARK: GetRandomNonCrownHeadItem
        /// <summary>
        /// Gets a random head item that is NOT a crown.
        /// Used for non-ruling clan members who should not wear crowns.
        /// </summary>
        /// <param name="cultureId">The culture ID to select from.</param>
        /// <param name="isFemale">Whether the hero is female.</param>
        /// <param name="isRulingClanMember">Whether the hero is a member of a ruling clan.</param>
        /// <returns>A random non-crown head item meeting appearance requirements, or null if none available.</returns>
        public ItemObject GetRandomNonCrownHeadItem(string cultureId, bool isFemale, bool isRulingClanMember = false)
        {
            // Regular head items are already filtered to exclude crowns
            return GetRandomItem(cultureId, isFemale, EquipmentIndex.Head, isRulingClanMember);
        }

        /// MARK: GetCrown
        /// <summary>
        /// Gets a crown for ruling clan members.
        /// For heroes level 15+, prefers lord-specific crown pools.
        /// </summary>
        public ItemObject GetCrown(string cultureId, bool isFemale, int heroLevel = -1)
        {
            EnsureInitialized();

            // Level 15+: try lord-specific crown pool first
            if (heroLevel >= LordPoolMinLevel)
            {
                ItemObject lordCrown = SelectCrownFromPools(cultureId, isFemale, useLordPools: true);
                if (lordCrown != null)
                    return lordCrown;
            }

            return SelectCrownFromPools(cultureId, isFemale, useLordPools: false);
        }

        /// MARK: SelectCrownFromPools
        /// <summary>
        /// Selects a crown from the specified pool type (lord or combined).
        /// </summary>
        private ItemObject SelectCrownFromPools(string cultureId, bool isFemale, bool useLordPools)
        {
            Dictionary<string, MBList<ItemObject>> crownPools =
                useLordPools
                    ? (isFemale ? _femaleLordCrownPools : _maleLordCrownPools)
                    : (isFemale ? _femaleCrownPools : _maleCrownPools);

            // Try culture-specific crown pool
            if (cultureId != null &&
                crownPools.TryGetValue(cultureId, out MBList<ItemObject> crowns) &&
                crowns.Count > 0)
            {
                return SelectRandomItem(crowns);
            }

            // Fallback to any crown
            MBList<ItemObject> fallbackCrowns =
                useLordPools
                    ? (isFemale ? _fallbackFemaleLordCrowns : _fallbackMaleLordCrowns)
                    : (isFemale ? _fallbackFemaleCrowns : _fallbackMaleCrowns);

            if (fallbackCrowns.Count > 0)
            {
                return SelectRandomItem(fallbackCrowns);
            }

            return null;
        }

        /// MARK: GetCivilianWeapon
        /// <summary>
        /// Gets a one-handed civilian weapon for male characters.
        /// For heroes level 15+, prefers lord-specific weapon pools.
        /// </summary>
        public ItemObject GetCivilianWeapon(string cultureId, int heroLevel = -1)
        {
            EnsureInitialized();

            // Level 15+: try lord-specific weapon pool first
            if (heroLevel >= LordPoolMinLevel)
            {
                ItemObject lordWeapon = SelectWeaponFromPools(cultureId, useLordPools: true);
                if (lordWeapon != null)
                    return lordWeapon;
            }

            return SelectWeaponFromPools(cultureId, useLordPools: false);
        }

        /// MARK: SelectWeaponFromPools
        /// <summary>
        /// Selects a civilian weapon from the specified pool type (lord or combined).
        /// </summary>
        private ItemObject SelectWeaponFromPools(string cultureId, bool useLordPools)
        {
            Dictionary<string, MBList<ItemObject>> weaponPools =
                useLordPools ? _civilianLordWeaponPools : _civilianWeaponPools;

            // Try culture-specific weapon pool
            if (cultureId != null &&
                weaponPools.TryGetValue(cultureId, out MBList<ItemObject> weapons) &&
                weapons.Count > 0)
            {
                return SelectRandomItem(weapons);
            }

            // Fallback to any civilian weapon
            MBList<ItemObject> fallbackWeaponPool = useLordPools ? _fallbackLordWeapons : _fallbackWeapons;
            if (fallbackWeaponPool.Count > 0)
            {
                return SelectRandomItem(fallbackWeaponPool);
            }

            return null;
        }

        /// MARK: HasItemsForCulture
        /// <summary>
        /// Checks if there are civilian items available for a specific culture and gender.
        /// </summary>
        public bool HasItemsForCulture(string cultureId, bool isFemale)
        {
            EnsureInitialized();

            Dictionary<string, Dictionary<EquipmentIndex, MBList<ItemObject>>> pools =
                isFemale ? _femaleCivilianPools : _maleCivilianPools;

            if (cultureId == null || !pools.TryGetValue(cultureId, out Dictionary<EquipmentIndex, MBList<ItemObject>> culturePools))
                return false;

            // Check if we have at least body items
            return culturePools.TryGetValue(EquipmentIndex.Body, out MBList<ItemObject> bodyItems) &&
                   bodyItems.Count > 0;
        }

        #endregion

        #region Helper Methods

        /// MARK: EnsureInitialized
        private void EnsureInitialized()
        {
            if (!_initialized)
            {
                Initialize();
            }
        }

        /// MARK: EnsureCulturePoolsExist
        private void EnsureCulturePoolsExist(
            string cultureId,
            Dictionary<string, Dictionary<EquipmentIndex, MBList<ItemObject>>> pools)
        {
            if (!pools.ContainsKey(cultureId))
            {
                pools[cultureId] = new();
            }

            Dictionary<EquipmentIndex, MBList<ItemObject>> culturePools = pools[cultureId];

            // Ensure all armor slots exist
            EquipmentIndex[] armorSlots = new[]
            {
                EquipmentIndex.Head,
                EquipmentIndex.Cape,
                EquipmentIndex.Body,
                EquipmentIndex.Gloves,
                EquipmentIndex.Leg
            };

            foreach (EquipmentIndex slot in armorSlots)
            {
                if (!culturePools.ContainsKey(slot))
                {
                    culturePools[slot] = new();
                }
            }
        }

        /// MARK: EnsureSlotPoolExists
        private void EnsureSlotPoolExists(
            EquipmentIndex slot,
            Dictionary<EquipmentIndex, MBList<ItemObject>> pools)
        {
            if (!pools.ContainsKey(slot))
            {
                pools[slot] = new();
            }
        }

        /// MARK: SelectRandomItem
        /// <summary>
        /// Selects a random item from a list using the shared random number generator.
        /// </summary>
        private ItemObject SelectRandomItem(MBList<ItemObject> items)
        {
            if (items == null || items.Count == 0)
                return null;

            int index = RandomNumberGen.Instance.NextRandomInt(items.Count);
            return items[index];
        }

        /// MARK: FilterByAppearance
        /// <summary>
        /// Filters items by civilian appearance requirements.
        /// </summary>
        /// <param name="items">The list of items to filter.</param>
        /// <param name="isRulingClanMember">Whether the hero is a member of a ruling clan.</param>
        /// <returns>A new list containing only items meeting appearance requirements.</returns>
        private MBList<ItemObject> FilterByAppearance(MBList<ItemObject> items, bool isRulingClanMember)
        {
            MBList<ItemObject> filtered = new();
            for (int i = 0; i < items.Count; i++)
            {
                if (ItemValidation.MeetsCivilianAppearanceRequirement(items[i], isRulingClanMember))
                    filtered.Add(items[i]);
            }
            return filtered;
        }

        /// MARK: FilterByAppearanceWithBonus
        /// <summary>
        /// Filters items by civilian appearance requirements with an optional appearance bonus.
        /// Used to select higher quality items (20% chance per slot in civilian equipment).
        /// </summary>
        /// <param name="items">The list of items to filter.</param>
        /// <param name="isRulingClanMember">Whether the hero is a member of a ruling clan.</param>
        /// <param name="appearanceBonus">Additional appearance requirement (0 or 1).</param>
        /// <returns>A new list containing only items meeting the boosted appearance requirements.</returns>
        private MBList<ItemObject> FilterByAppearanceWithBonus(MBList<ItemObject> items, bool isRulingClanMember, int appearanceBonus)
        {
            float baseThreshold = isRulingClanMember ? ItemValidation.MinimumRoyalAppearance : ItemValidation.MinimumCivilianAppearance;
            float effectiveThreshold = baseThreshold + appearanceBonus;

            MBList<ItemObject> filtered = new();
            for (int i = 0; i < items.Count; i++)
            {
                if (items[i].Appearance > effectiveThreshold)
                    filtered.Add(items[i]);
            }
            return filtered;
        }

        #endregion
    }
}
