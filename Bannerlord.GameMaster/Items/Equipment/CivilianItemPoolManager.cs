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
    /// based on their EquipmentFlags. This provides more authentic and culture-appropriate civilian
    /// outfits compared to heuristic-based item selection.
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

        #region Equipment Flags Constants

        // From EquipmentFlags.cs:
        // IsCombatantTemplate = 16        // For combatant characters
        // IsCivilianTemplate = 32         // Civilian equipment set
        // IsNobleTemplate = 64            // Noble-quality equipment
        // IsFemaleTemplate = 128          // Female-specific equipment

        private const EquipmentFlags CombatantFlag = (EquipmentFlags)16;
        private const EquipmentFlags CivilianFlag = (EquipmentFlags)32;
        private const EquipmentFlags NobleFlag = (EquipmentFlags)64;
        private const EquipmentFlags FemaleFlag = (EquipmentFlags)128;

        #endregion

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

        // Female civilian item pools
        // Key: CultureId -> EquipmentIndex -> Items
        private Dictionary<string, Dictionary<EquipmentIndex, MBList<ItemObject>>> _femaleCivilianPools;

        // Male civilian item pools
        // Key: CultureId -> EquipmentIndex -> Items
        private Dictionary<string, Dictionary<EquipmentIndex, MBList<ItemObject>>> _maleCivilianPools;

        // Crown pools for ruling clan members
        // Key: CultureId -> Items
        private Dictionary<string, MBList<ItemObject>> _femaleCrownPools;
        private Dictionary<string, MBList<ItemObject>> _maleCrownPools;

        // Civilian weapon pools (one-handed melee for males)
        // Key: CultureId -> Items
        private Dictionary<string, MBList<ItemObject>> _civilianWeaponPools;

        // Fallback pools for cultures without specific items
        private Dictionary<EquipmentIndex, MBList<ItemObject>> _fallbackFemalePools;
        private Dictionary<EquipmentIndex, MBList<ItemObject>> _fallbackMalePools;
        private MBList<ItemObject> _fallbackFemaleCrowns;
        private MBList<ItemObject> _fallbackMaleCrowns;
        private MBList<ItemObject> _fallbackWeapons;

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
            _initialized = false;
        }

        /// MARK: ProcessRoster
        /// <summary>
        /// Processes a single equipment roster and extracts civilian items.
        /// </summary>
        private void ProcessRoster(MBEquipmentRoster roster)
        {
            if (roster == null)
                return;

            // Must be an equipment template
            if (!roster.IsEquipmentTemplate())
                return;

            // Exclude peasant rosters (they have noble flag but contain peasant items)
            if (IsPeasantRoster(roster))
                return;

            // Resolve culture (handles lord_ rosters with null culture)
            string cultureId = ResolveCultureId(roster);
            if (cultureId == null)
                return;

            bool isFemaleRoster = roster.HasEquipmentFlags(FemaleFlag);
            bool isNoble = roster.HasEquipmentFlags(NobleFlag);
            bool isCivilian = roster.HasEquipmentFlags(CivilianFlag);

            // Female civilian rosters: female + noble + civilian
            bool isFemaleCivilianRoster = isFemaleRoster && isNoble && isCivilian;

            // Male civilian rosters: noble + civilian WITHOUT female flag
            bool isMaleCivilianRoster = !isFemaleRoster && isNoble && isCivilian;

            if (isFemaleCivilianRoster)
            {
                ExtractCivilianItems(roster, cultureId, isFemale: true);
            }
            else if (isMaleCivilianRoster)
            {
                ExtractCivilianItems(roster, cultureId, isFemale: false);
            }
        }

        /// MARK: IsPeasantRoster
        /// <summary>
        /// Checks if the roster is a peasant roster that should be excluded.
        /// These rosters have the noble flag but contain peasant items.
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
        /// </summary>
        private void ExtractCivilianItems(MBEquipmentRoster roster, string cultureId, bool isFemale)
        {
            MBReadOnlyList<Equipment> allEquipments = roster.AllEquipments;
            if (allEquipments == null || allEquipments.Count == 0)
                return;

            // Get target pools
            Dictionary<string, Dictionary<EquipmentIndex, MBList<ItemObject>>> targetPools = 
                isFemale ? _femaleCivilianPools : _maleCivilianPools;
            Dictionary<string, MBList<ItemObject>> targetCrownPools = 
                isFemale ? _femaleCrownPools : _maleCrownPools;
            Dictionary<EquipmentIndex, MBList<ItemObject>> fallbackPools = 
                isFemale ? _fallbackFemalePools : _fallbackMalePools;
            MBList<ItemObject> fallbackCrowns = 
                isFemale ? _fallbackFemaleCrowns : _fallbackMaleCrowns;

            // Ensure culture dictionaries exist
            EnsureCulturePoolsExist(cultureId, targetPools);
            if (!targetCrownPools.ContainsKey(cultureId))
            {
                targetCrownPools[cultureId] = new();
            }
            if (!_civilianWeaponPools.ContainsKey(cultureId))
            {
                _civilianWeaponPools[cultureId] = new();
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
                    ExtractWeaponsFromEquipment(equipment, cultureId);
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
        private void ExtractWeaponsFromEquipment(Equipment equipment, string cultureId)
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
                    if (!_civilianWeaponPools[cultureId].Contains(item))
                    {
                        _civilianWeaponPools[cultureId].Add(item);
                        _itemsExtracted++;
                    }
                    // Also add to fallback
                    if (!_fallbackWeapons.Contains(item))
                    {
                        _fallbackWeapons.Add(item);
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
            EnsureInitialized();

            Dictionary<string, Dictionary<EquipmentIndex, MBList<ItemObject>>> pools =
                isFemale ? _femaleCivilianPools : _maleCivilianPools;

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
                isFemale ? _fallbackFemalePools : _fallbackMalePools;
            
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
        /// </summary>
        public ItemObject GetCrown(string cultureId, bool isFemale)
        {
            EnsureInitialized();

            Dictionary<string, MBList<ItemObject>> crownPools = 
                isFemale ? _femaleCrownPools : _maleCrownPools;

            // Try culture-specific crown pool
            if (cultureId != null && 
                crownPools.TryGetValue(cultureId, out MBList<ItemObject> crowns) &&
                crowns.Count > 0)
            {
                return SelectRandomItem(crowns);
            }

            // Fallback to any crown
            MBList<ItemObject> fallbackCrowns = isFemale ? _fallbackFemaleCrowns : _fallbackMaleCrowns;
            if (fallbackCrowns.Count > 0)
            {
                return SelectRandomItem(fallbackCrowns);
            }

            return null;
        }

        /// MARK: GetCivilianWeapon
        /// <summary>
        /// Gets a one-handed civilian weapon for male characters.
        /// </summary>
        public ItemObject GetCivilianWeapon(string cultureId)
        {
            EnsureInitialized();

            // Try culture-specific weapon pool
            if (cultureId != null && 
                _civilianWeaponPools.TryGetValue(cultureId, out MBList<ItemObject> weapons) &&
                weapons.Count > 0)
            {
                return SelectRandomItem(weapons);
            }

            // Fallback to any civilian weapon
            if (_fallbackWeapons.Count > 0)
            {
                return SelectRandomItem(_fallbackWeapons);
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
