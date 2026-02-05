using System.Collections.Generic;
using Bannerlord.GameMaster.Common;
using Bannerlord.GameMaster.Information;
using TaleWorlds.Core;
using TaleWorlds.Library;

namespace Bannerlord.GameMaster.Items
{
    /// <summary>
    /// Singleton manager that maintains categorized pools of game items for equipment generation.
    /// Pools are organized by culture, tier, weapon type, armor type, and other relevant categories.
    /// </summary>
    public sealed class ItemPoolManager
    {
        private static ItemPoolManager _instance;
        private static readonly object _lock = new();

        /// <summary>
        /// Gets the singleton instance of the ItemPoolManager.
        /// </summary>
        public static ItemPoolManager Instance
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

        private ItemPoolManager()
        {
            InitializePoolStructures();
        }

        private bool _initialized;
        private int _totalItemsProcessed;
        private int _validItemsAdded;

        #region Pools

        // Weapon Pools
        // Key: CultureId -> Tier -> WeaponTypeFlags -> Items
        private Dictionary<string, Dictionary<int, Dictionary<WeaponTypeFlags, MBList<ItemObject>>>> _weaponPoolsByCulture;

        // Neutral weapons (no culture restriction)
        // Key: Tier -> WeaponTypeFlags -> Items
        private Dictionary<int, Dictionary<WeaponTypeFlags, MBList<ItemObject>>> _neutralWeaponPools;

        // Armor Pools
        // Key: CultureId -> Tier -> ArmorComponent.ArmorMaterialTypes -> Items
        private Dictionary<string, Dictionary<int, Dictionary<ArmorComponent.ArmorMaterialTypes, MBList<ItemObject>>>> _armorPoolsByCulture;

        // Key: CultureId -> Tier -> EquipmentIndex (head, body, gloves, boots, cape) -> Items
        private Dictionary<string, Dictionary<int, Dictionary<EquipmentIndex, MBList<ItemObject>>>> _armorPoolsBySlot;

        // Neutral armor (no culture restriction)
        // Key: Tier -> EquipmentIndex -> Items
        private Dictionary<int, Dictionary<EquipmentIndex, MBList<ItemObject>>> _neutralArmorPools;

        // Horse Pools
        // Key: CultureId -> Tier -> Items (horses)
        private Dictionary<string, Dictionary<int, MBList<ItemObject>>> _horsePoolsByCulture;

        // Key: CultureId -> Tier -> Items (harnesses)
        private Dictionary<string, Dictionary<int, MBList<ItemObject>>> _harnessPoolsByCulture;

        // Neutral horses and harnesses
        private Dictionary<int, MBList<ItemObject>> _neutralHorsePools;
        private Dictionary<int, MBList<ItemObject>> _neutralHarnessPools;

        // Shield Pools
        // Key: CultureId -> Tier -> Items
        private Dictionary<string, Dictionary<int, MBList<ItemObject>>> _shieldPoolsByCulture;

        // Neutral shields
        private Dictionary<int, MBList<ItemObject>> _neutralShieldPools;

        // Ammunition Pools
        // Key: WeaponTypeFlags (Arrow, Bolt, Bullet) -> Items
        private Dictionary<WeaponTypeFlags, MBList<ItemObject>> _ammoPools;

        // Banner Pools
        // Key: CultureId -> Items
        private Dictionary<string, MBList<ItemObject>> _bannerPoolsByCulture;
        private MBList<ItemObject> _neutralBannerPool;

        #endregion

        #region Properties

        /// <summary>
        /// Gets whether the item pools have been initialized.
        /// </summary>
        public bool IsInitialized => _initialized;

        /// <summary>
        /// Gets the total number of items processed during initialization.
        /// </summary>
        public int TotalItemsProcessed => _totalItemsProcessed;

        /// <summary>
        /// Gets the number of valid items added to pools.
        /// </summary>
        public int ValidItemsAdded => _validItemsAdded;

        /// <summary>
        /// Gets the weapon pools organized by culture, tier, and weapon type.
        /// </summary>
        public Dictionary<string, Dictionary<int, Dictionary<WeaponTypeFlags, MBList<ItemObject>>>> WeaponPoolsByCulture => _weaponPoolsByCulture;

        /// <summary>
        /// Gets the neutral weapon pools (no culture restriction) organized by tier and weapon type.
        /// </summary>
        public Dictionary<int, Dictionary<WeaponTypeFlags, MBList<ItemObject>>> NeutralWeaponPools => _neutralWeaponPools;

        /// <summary>
        /// Gets the armor pools organized by culture, tier, and material type.
        /// </summary>
        public Dictionary<string, Dictionary<int, Dictionary<ArmorComponent.ArmorMaterialTypes, MBList<ItemObject>>>> ArmorPoolsByCulture => _armorPoolsByCulture;

        /// <summary>
        /// Gets the armor pools organized by culture, tier, and equipment slot.
        /// </summary>
        public Dictionary<string, Dictionary<int, Dictionary<EquipmentIndex, MBList<ItemObject>>>> ArmorPoolsBySlot => _armorPoolsBySlot;

        /// <summary>
        /// Gets the neutral armor pools (no culture restriction) organized by tier and slot.
        /// </summary>
        public Dictionary<int, Dictionary<EquipmentIndex, MBList<ItemObject>>> NeutralArmorPools => _neutralArmorPools;

        /// <summary>
        /// Gets the horse pools organized by culture and tier.
        /// </summary>
        public Dictionary<string, Dictionary<int, MBList<ItemObject>>> HorsePoolsByCulture => _horsePoolsByCulture;

        /// <summary>
        /// Gets the harness pools organized by culture and tier.
        /// </summary>
        public Dictionary<string, Dictionary<int, MBList<ItemObject>>> HarnessPoolsByCulture => _harnessPoolsByCulture;

        /// <summary>
        /// Gets the neutral horse pools organized by tier.
        /// </summary>
        public Dictionary<int, MBList<ItemObject>> NeutralHorsePools => _neutralHorsePools;

        /// <summary>
        /// Gets the neutral harness pools organized by tier.
        /// </summary>
        public Dictionary<int, MBList<ItemObject>> NeutralHarnessPools => _neutralHarnessPools;

        /// <summary>
        /// Gets the shield pools organized by culture and tier.
        /// </summary>
        public Dictionary<string, Dictionary<int, MBList<ItemObject>>> ShieldPoolsByCulture => _shieldPoolsByCulture;

        /// <summary>
        /// Gets the neutral shield pools organized by tier.
        /// </summary>
        public Dictionary<int, MBList<ItemObject>> NeutralShieldPools => _neutralShieldPools;

        /// <summary>
        /// Gets the ammunition pools organized by ammo type.
        /// </summary>
        public Dictionary<WeaponTypeFlags, MBList<ItemObject>> AmmoPools => _ammoPools;

        /// <summary>
        /// Gets the banner pools organized by culture.
        /// </summary>
        public Dictionary<string, MBList<ItemObject>> BannerPoolsByCulture => _bannerPoolsByCulture;

        /// <summary>
        /// Gets the neutral banner pool.
        /// </summary>
        public MBList<ItemObject> NeutralBannerPool => _neutralBannerPool;

        #endregion

        /// MARK: Initialize
        /// <summary>
        /// Initializes or reinitializes all item pools by scanning game items.
        /// Call this after game data is fully loaded.
        /// </summary>
        public void Initialize()
        {
            lock (_lock)
            {
                if (_initialized)
                    return;

                // Reset counters
                _totalItemsProcessed = 0;
                _validItemsAdded = 0;

                // Get all items from the game's object manager
                MBReadOnlyList<ItemObject> allItems = Game.Current?.ObjectManager?.GetObjectTypeList<ItemObject>();
                if (allItems == null || allItems.Count == 0)
                {
                    BLGMResult.Error("Initialize() failed: No items found in ObjectManager").Log();
                    return;
                }

                // Process each item
                for (int i = 0; i < allItems.Count; i++)
                {
                    ItemObject item = allItems[i];
                    _totalItemsProcessed++;

                    // Filter out invalid items using validation
                    if (!ItemValidation.IsValidItem(item))
                        continue;

                    // Categorize and add the item to appropriate pools
                    CategorizeAndAddItem(item);
                    _validItemsAdded++;
                }

                _initialized = true;
                BLGMResult.Success($"ItemPoolManager initialized: {_validItemsAdded} valid items from {_totalItemsProcessed} total items").Log();
            }
        }

        /// MARK: Clear
        /// <summary>
        /// Clears all item pools and resets initialization state.
        /// </summary>
        public void Clear()
        {
            lock (_lock)
            {
                InitializePoolStructures();
                _initialized = false;
                _totalItemsProcessed = 0;
                _validItemsAdded = 0;
            }
        }

        /// MARK: Reinitialize
        /// <summary>
        /// Reinitializes the item pools by clearing and rebuilding them.
        /// </summary>
        public void Reinitialize()
        {
            Clear();
            Initialize();
        }

        /// MARK: InitializePoolStructs
        private void InitializePoolStructures()
        {
            // Initialize weapon pools
            _weaponPoolsByCulture = new();
            _neutralWeaponPools = new();

            // Initialize armor pools
            _armorPoolsByCulture = new();
            _armorPoolsBySlot = new();
            _neutralArmorPools = new();

            // Initialize horse pools
            _horsePoolsByCulture = new();
            _harnessPoolsByCulture = new();
            _neutralHorsePools = new();
            _neutralHarnessPools = new();

            // Initialize shield pools
            _shieldPoolsByCulture = new();
            _neutralShieldPools = new();

            // Initialize ammo pools
            _ammoPools = new();

            // Initialize banner pools
            _bannerPoolsByCulture = new();
            _neutralBannerPool = new();

            _initialized = false;
        }

        /// MARK: CategorizeAndAdd
        /// <summary>
        /// Categorizes an item and adds it to the appropriate pool(s).
        /// </summary>
        private void CategorizeAndAddItem(ItemObject item)
        {
            if (item == null)
                return;

            // Determine if item has a culture
            bool hasCulture = item.Culture != null;
            string cultureId = hasCulture ? item.Culture.StringId : null;

            // Categorize by item type
            switch (item.ItemType)
            {
                // Weapons
                case ItemObject.ItemTypeEnum.OneHandedWeapon:
                case ItemObject.ItemTypeEnum.TwoHandedWeapon:
                case ItemObject.ItemTypeEnum.Polearm:
                case ItemObject.ItemTypeEnum.Bow:
                case ItemObject.ItemTypeEnum.Crossbow:
                case ItemObject.ItemTypeEnum.Thrown:
                case ItemObject.ItemTypeEnum.Pistol:
                case ItemObject.ItemTypeEnum.Musket:
                    if (hasCulture)
                        AddWeaponToPool(item, cultureId);
                    else
                        AddWeaponToNeutralPool(item);
                    break;

                // Shields
                case ItemObject.ItemTypeEnum.Shield:
                    if (hasCulture)
                        AddShieldToPool(item, cultureId);
                    else
                        AddShieldToNeutralPool(item);
                    break;

                // Ammunition
                case ItemObject.ItemTypeEnum.Arrows:
                case ItemObject.ItemTypeEnum.Bolts:
                case ItemObject.ItemTypeEnum.Bullets:
                    AddAmmoToPool(item);
                    break;

                // Armor
                case ItemObject.ItemTypeEnum.HeadArmor:
                case ItemObject.ItemTypeEnum.BodyArmor:
                case ItemObject.ItemTypeEnum.LegArmor:
                case ItemObject.ItemTypeEnum.HandArmor:
                case ItemObject.ItemTypeEnum.Cape:
                    if (hasCulture)
                        AddArmorToPool(item, cultureId);
                    else
                        AddArmorToNeutralPool(item);
                    break;

                // Horses
                case ItemObject.ItemTypeEnum.Horse:
                    if (hasCulture)
                        AddHorseToPool(item, cultureId);
                    else
                        AddHorseToNeutralPool(item);
                    break;

                // Horse Harnesses
                case ItemObject.ItemTypeEnum.HorseHarness:
                    if (hasCulture)
                        AddHarnessToPool(item, cultureId);
                    else
                        AddHarnessToNeutralPool(item);
                    break;

                // Banners
                case ItemObject.ItemTypeEnum.Banner:
                    if (hasCulture)
                        AddBannerToPool(item, cultureId);
                    else
                        _neutralBannerPool.Add(item);
                    break;

                // Items that don't fit equipment categories (goods, books, animals, etc.)
                default:
                    // These items are not equipment and are intentionally not added to pools
                    break;
            }
        }

        /// MARK: AddWeaponToPool
        /// <summary>
        /// Adds a weapon to the cultural weapon pool.
        /// </summary>
        private void AddWeaponToPool(ItemObject item, string cultureId)
        {
            if (item.PrimaryWeapon == null)
                return;

            WeaponTypeFlags weaponType = WeaponClassToWeaponTypeFlags(item.PrimaryWeapon.WeaponClass);
            if (weaponType == WeaponTypeFlags.None)
                return;

            int tier = (int)item.Tier;

            // Ensure culture dictionary exists
            if (!_weaponPoolsByCulture.TryGetValue(cultureId, out Dictionary<int, Dictionary<WeaponTypeFlags, MBList<ItemObject>>> tierDict))
            {
                tierDict = new();
                _weaponPoolsByCulture[cultureId] = tierDict;
            }

            // Ensure tier dictionary exists
            if (!tierDict.TryGetValue(tier, out Dictionary<WeaponTypeFlags, MBList<ItemObject>> weaponDict))
            {
                weaponDict = new();
                tierDict[tier] = weaponDict;
            }

            // Ensure weapon type list exists
            if (!weaponDict.TryGetValue(weaponType, out MBList<ItemObject> itemList))
            {
                itemList = new();
                weaponDict[weaponType] = itemList;
            }

            itemList.Add(item);
        }

        /// MARK: AddWeaponNeutral
        /// <summary>
        /// Adds a weapon to the neutral weapon pool (no culture).
        /// </summary>
        private void AddWeaponToNeutralPool(ItemObject item)
        {
            if (item.PrimaryWeapon == null)
                return;

            WeaponTypeFlags weaponType = WeaponClassToWeaponTypeFlags(item.PrimaryWeapon.WeaponClass);
            if (weaponType == WeaponTypeFlags.None)
                return;

            int tier = (int)item.Tier;

            // Ensure tier dictionary exists
            if (!_neutralWeaponPools.TryGetValue(tier, out Dictionary<WeaponTypeFlags, MBList<ItemObject>> weaponDict))
            {
                weaponDict = new();
                _neutralWeaponPools[tier] = weaponDict;
            }

            // Ensure weapon type list exists
            if (!weaponDict.TryGetValue(weaponType, out MBList<ItemObject> itemList))
            {
                itemList = new();
                weaponDict[weaponType] = itemList;
            }

            itemList.Add(item);
        }

        /// MARK: AddShieldToPool
        /// <summary>
        /// Adds a shield to the cultural shield pool.
        /// </summary>
        private void AddShieldToPool(ItemObject item, string cultureId)
        {
            int tier = (int)item.Tier;

            // Ensure culture dictionary exists
            if (!_shieldPoolsByCulture.TryGetValue(cultureId, out Dictionary<int, MBList<ItemObject>> tierDict))
            {
                tierDict = new();
                _shieldPoolsByCulture[cultureId] = tierDict;
            }

            // Ensure tier list exists
            if (!tierDict.TryGetValue(tier, out MBList<ItemObject> itemList))
            {
                itemList = new();
                tierDict[tier] = itemList;
            }

            itemList.Add(item);
        }

        /// MARK: AddShieldNeutral
        /// <summary>
        /// Adds a shield to the neutral shield pool.
        /// </summary>
        private void AddShieldToNeutralPool(ItemObject item)
        {
            int tier = (int)item.Tier;

            // Ensure tier list exists
            if (!_neutralShieldPools.TryGetValue(tier, out MBList<ItemObject> itemList))
            {
                itemList = new();
                _neutralShieldPools[tier] = itemList;
            }

            itemList.Add(item);
        }

        /// MARK: AddAmmoToPool
        /// <summary>
        /// Adds ammunition to the ammo pool based on type.
        /// </summary>
        private void AddAmmoToPool(ItemObject item)
        {
            WeaponTypeFlags ammoType = item.ItemType switch
            {
                ItemObject.ItemTypeEnum.Arrows => WeaponTypeFlags.Arrow,
                ItemObject.ItemTypeEnum.Bolts => WeaponTypeFlags.Bolt,
                ItemObject.ItemTypeEnum.Bullets => WeaponTypeFlags.Bullet,
                _ => WeaponTypeFlags.None
            };

            if (ammoType == WeaponTypeFlags.None)
                return;

            // Ensure ammo list exists
            if (!_ammoPools.TryGetValue(ammoType, out MBList<ItemObject> itemList))
            {
                itemList = new();
                _ammoPools[ammoType] = itemList;
            }

            itemList.Add(item);
        }

        /// MARK: AddArmorToPool
        /// <summary>
        /// Adds armor to the cultural armor pools (by slot and by material).
        /// </summary>
        private void AddArmorToPool(ItemObject item, string cultureId)
        {
            EquipmentIndex slot = GetArmorSlot(item);
            if (slot == EquipmentIndex.None)
                return;

            int tier = (int)item.Tier;

            // Add to slot-based pool
            AddArmorToSlotPool(item, cultureId, tier, slot);

            // Add to material-based pool
            if (item.ArmorComponent != null)
            {
                AddArmorToMaterialPool(item, cultureId, tier, item.ArmorComponent.MaterialType);
            }
        }

        /// MARK: AddArmorToSlotPool
        /// <summary>
        /// Adds armor to the cultural armor pool organized by slot.
        /// </summary>
        private void AddArmorToSlotPool(ItemObject item, string cultureId, int tier, EquipmentIndex slot)
        {
            // Ensure culture dictionary exists
            if (!_armorPoolsBySlot.TryGetValue(cultureId, out Dictionary<int, Dictionary<EquipmentIndex, MBList<ItemObject>>> tierDict))
            {
                tierDict = new();
                _armorPoolsBySlot[cultureId] = tierDict;
            }

            // Ensure tier dictionary exists
            if (!tierDict.TryGetValue(tier, out Dictionary<EquipmentIndex, MBList<ItemObject>> slotDict))
            {
                slotDict = new();
                tierDict[tier] = slotDict;
            }

            // Ensure slot list exists
            if (!slotDict.TryGetValue(slot, out MBList<ItemObject> itemList))
            {
                itemList = new();
                slotDict[slot] = itemList;
            }

            itemList.Add(item);
        }

        /// MARK: AddArmorToMatPool
        /// <summary>
        /// Adds armor to the cultural armor pool organized by material type.
        /// </summary>
        private void AddArmorToMaterialPool(ItemObject item, string cultureId, int tier, ArmorComponent.ArmorMaterialTypes materialType)
        {
            // Ensure culture dictionary exists
            if (!_armorPoolsByCulture.TryGetValue(cultureId, out Dictionary<int, Dictionary<ArmorComponent.ArmorMaterialTypes, MBList<ItemObject>>> tierDict))
            {
                tierDict = new();
                _armorPoolsByCulture[cultureId] = tierDict;
            }

            // Ensure tier dictionary exists
            if (!tierDict.TryGetValue(tier, out Dictionary<ArmorComponent.ArmorMaterialTypes, MBList<ItemObject>> materialDict))
            {
                materialDict = new();
                tierDict[tier] = materialDict;
            }

            // Ensure material list exists
            if (!materialDict.TryGetValue(materialType, out MBList<ItemObject> itemList))
            {
                itemList = new();
                materialDict[materialType] = itemList;
            }

            itemList.Add(item);
        }

        /// MARK: AddArmorNeutral
        /// <summary>
        /// Adds armor to the neutral armor pool (no culture).
        /// </summary>
        private void AddArmorToNeutralPool(ItemObject item)
        {
            EquipmentIndex slot = GetArmorSlot(item);
            if (slot == EquipmentIndex.None)
                return;

            int tier = (int)item.Tier;

            // Ensure tier dictionary exists
            if (!_neutralArmorPools.TryGetValue(tier, out Dictionary<EquipmentIndex, MBList<ItemObject>> slotDict))
            {
                slotDict = new();
                _neutralArmorPools[tier] = slotDict;
            }

            // Ensure slot list exists
            if (!slotDict.TryGetValue(slot, out MBList<ItemObject> itemList))
            {
                itemList = new();
                slotDict[slot] = itemList;
            }

            itemList.Add(item);
        }

        /// MARK: AddHorseToPool
        /// <summary>
        /// Adds a horse to the cultural horse pool.
        /// </summary>
        private void AddHorseToPool(ItemObject item, string cultureId)
        {
            int tier = (int)item.Tier;

            // Ensure culture dictionary exists
            if (!_horsePoolsByCulture.TryGetValue(cultureId, out Dictionary<int, MBList<ItemObject>> tierDict))
            {
                tierDict = new();
                _horsePoolsByCulture[cultureId] = tierDict;
            }

            // Ensure tier list exists
            if (!tierDict.TryGetValue(tier, out MBList<ItemObject> itemList))
            {
                itemList = new();
                tierDict[tier] = itemList;
            }

            itemList.Add(item);
        }

        /// MARK: AddHorseNeutral
        /// <summary>
        /// Adds a horse to the neutral horse pool.
        /// </summary>
        private void AddHorseToNeutralPool(ItemObject item)
        {
            int tier = (int)item.Tier;

            // Ensure tier list exists
            if (!_neutralHorsePools.TryGetValue(tier, out MBList<ItemObject> itemList))
            {
                itemList = new();
                _neutralHorsePools[tier] = itemList;
            }

            itemList.Add(item);
        }

        /// MARK: AddHarnessToPool
        /// <summary>
        /// Adds a harness to the cultural harness pool.
        /// Filters out pack animal harnesses (mule harnesses, bags, etc.) that are inappropriate for combat.
        /// </summary>
        private void AddHarnessToPool(ItemObject item, string cultureId)
        {
            // Filter out mule harnesses and pack animal harnesses
            if (!MountCompatibility.IsCombatHarness(item))
                return;

            int tier = (int)item.Tier;

            // Ensure culture dictionary exists
            if (!_harnessPoolsByCulture.TryGetValue(cultureId, out Dictionary<int, MBList<ItemObject>> tierDict))
            {
                tierDict = new();
                _harnessPoolsByCulture[cultureId] = tierDict;
            }

            // Ensure tier list exists
            if (!tierDict.TryGetValue(tier, out MBList<ItemObject> itemList))
            {
                itemList = new();
                tierDict[tier] = itemList;
            }

            itemList.Add(item);
        }

        /// MARK: AddHarnessNeutral
        /// <summary>
        /// Adds a harness to the neutral harness pool.
        /// Filters out pack animal harnesses (mule harnesses, bags, etc.) that are inappropriate for combat.
        /// </summary>
        private void AddHarnessToNeutralPool(ItemObject item)
        {
            // Filter out mule harnesses and pack animal harnesses
            if (!MountCompatibility.IsCombatHarness(item))
                return;

            int tier = (int)item.Tier;

            // Ensure tier list exists
            if (!_neutralHarnessPools.TryGetValue(tier, out MBList<ItemObject> itemList))
            {
                itemList = new();
                _neutralHarnessPools[tier] = itemList;
            }

            itemList.Add(item);
        }

        /// MARK: AddBannerToPool
        /// <summary>
        /// Adds a banner to the cultural banner pool.
        /// </summary>
        private void AddBannerToPool(ItemObject item, string cultureId)
        {
            // Ensure culture list exists
            if (!_bannerPoolsByCulture.TryGetValue(cultureId, out MBList<ItemObject> itemList))
            {
                itemList = new();
                _bannerPoolsByCulture[cultureId] = itemList;
            }

            itemList.Add(item);
        }


        /// MARK: WeaponClassToFlag
        /// <summary>
        /// Converts a native WeaponClass enum value to our WeaponTypeFlags enum.
        /// </summary>
        private WeaponTypeFlags WeaponClassToWeaponTypeFlags(WeaponClass weaponClass)
        {
            return weaponClass switch
            {
                // Melee weapons
                WeaponClass.Dagger => WeaponTypeFlags.Dagger,
                WeaponClass.OneHandedSword => WeaponTypeFlags.OneHandedSword,
                WeaponClass.TwoHandedSword => WeaponTypeFlags.TwoHandedSword,
                WeaponClass.OneHandedAxe => WeaponTypeFlags.OneHandedAxe,
                WeaponClass.TwoHandedAxe => WeaponTypeFlags.TwoHandedAxe,
                WeaponClass.Mace => WeaponTypeFlags.OneHandedMace,
                WeaponClass.Pick => WeaponTypeFlags.OneHandedMace, // Picks treated as maces
                WeaponClass.TwoHandedMace => WeaponTypeFlags.TwoHandedMace,
                WeaponClass.OneHandedPolearm => WeaponTypeFlags.OneHandedPolearm,
                WeaponClass.TwoHandedPolearm => WeaponTypeFlags.TwoHandedPolearm,
                WeaponClass.LowGripPolearm => WeaponTypeFlags.TwoHandedPolearm, // Low grip polearms treated as two-handed

                // Ranged weapons
                WeaponClass.Bow => WeaponTypeFlags.Bow,
                WeaponClass.Crossbow => WeaponTypeFlags.Crossbow,
                WeaponClass.Pistol => WeaponTypeFlags.Pistol,
                WeaponClass.Musket => WeaponTypeFlags.Musket,

                // Throwing weapons
                WeaponClass.ThrowingAxe => WeaponTypeFlags.ThrowingAxe,
                WeaponClass.ThrowingKnife => WeaponTypeFlags.ThrowingKnife,
                WeaponClass.Javelin => WeaponTypeFlags.Javelin,
                WeaponClass.Stone => WeaponTypeFlags.Stone,

                // Ammunition
                WeaponClass.Arrow => WeaponTypeFlags.Arrow,
                WeaponClass.Bolt => WeaponTypeFlags.Bolt,
                WeaponClass.Cartridge => WeaponTypeFlags.Bullet,

                // Shields
                WeaponClass.SmallShield => WeaponTypeFlags.Shield,
                WeaponClass.LargeShield => WeaponTypeFlags.Shield,
                
                // Banner
                WeaponClass.Banner => WeaponTypeFlags.Banner,

                // Unmapped types
                _ => WeaponTypeFlags.None
            };
        }

        /// MARK: GetArmorSlot
        /// <summary>
        /// Gets the equipment slot for an armor item based on its item type.
        /// </summary>
        private EquipmentIndex GetArmorSlot(ItemObject item)
        {
            return item.ItemType switch
            {
                ItemObject.ItemTypeEnum.HeadArmor => EquipmentIndex.Head,
                ItemObject.ItemTypeEnum.Cape => EquipmentIndex.Cape,
                ItemObject.ItemTypeEnum.BodyArmor => EquipmentIndex.Body,
                ItemObject.ItemTypeEnum.HandArmor => EquipmentIndex.Gloves,
                ItemObject.ItemTypeEnum.LegArmor => EquipmentIndex.Leg,
                _ => EquipmentIndex.None
            };
        }
    }
}
