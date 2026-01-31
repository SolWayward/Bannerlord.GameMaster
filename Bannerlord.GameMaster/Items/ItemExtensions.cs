using System;
using TaleWorlds.Core;
using Bannerlord.GameMaster.Common.Interfaces;

namespace Bannerlord.GameMaster.Items
{
    [Flags]
    public enum ItemTypes
    {
        None = 0,
        Weapon = 1,
        Armor = 2,
        Mount = 4,
        Food = 8,
        Trade = 16,
        OneHanded = 32,
        TwoHanded = 64,
        Ranged = 128,
        Shield = 256,
        HeadArmor = 512,
        BodyArmor = 1024,
        LegArmor = 2048,
        HandArmor = 4096,
        Cape = 8192,
        Thrown = 16384,
        Arrows = 32768,
        Bolts = 65536,
        Polearm = 131072,
        Banner = 262144,
        Goods = 524288,
        Bow = 1048576,
        Crossbow = 2097152,
        Civilian = 4194304,
        Combat = 8388608,
        HorseArmor = 16777216
    }

    /// <summary>
    /// Extension methods for ItemObject entities
    /// </summary>
    public static class ItemExtensions
    {
        /// <summary>
        /// Gets all item type flags for this item
        /// </summary>
        public static ItemTypes GetItemTypes(this ItemObject item)
        {
            ItemTypes types = ItemTypes.None;

            if (item.IsFood) types |= ItemTypes.Food;
            if (item.IsBannerItem) types |= ItemTypes.Banner;

            // Check native civilian flag (ItemFlags.Civilian = 4194304)
            // This is the same flag the game UI uses to show the civilian equipment icon
            bool hasNativeCivilianFlag = (item.ItemFlags & ItemFlags.Civilian) == ItemFlags.Civilian;

            // Item type categorization
            switch (item.ItemType)
            {
                case ItemObject.ItemTypeEnum.OneHandedWeapon:
                    types |= ItemTypes.Weapon | ItemTypes.OneHanded | ItemTypes.Combat;
                    break;
                case ItemObject.ItemTypeEnum.TwoHandedWeapon:
                    types |= ItemTypes.Weapon | ItemTypes.TwoHanded | ItemTypes.Combat;
                    break;
                case ItemObject.ItemTypeEnum.Polearm:
                    types |= ItemTypes.Weapon | ItemTypes.Polearm | ItemTypes.Combat;
                    break;
                case ItemObject.ItemTypeEnum.Bow:
                    types |= ItemTypes.Weapon | ItemTypes.Ranged | ItemTypes.Bow | ItemTypes.Combat;
                    break;
                case ItemObject.ItemTypeEnum.Crossbow:
                    types |= ItemTypes.Weapon | ItemTypes.Ranged | ItemTypes.Crossbow | ItemTypes.Combat;
                    break;
                case ItemObject.ItemTypeEnum.Thrown:
                    types |= ItemTypes.Weapon | ItemTypes.Thrown | ItemTypes.Combat;
                    break;
                case ItemObject.ItemTypeEnum.Arrows:
                    types |= ItemTypes.Arrows | ItemTypes.Combat;
                    break;
                case ItemObject.ItemTypeEnum.Bolts:
                    types |= ItemTypes.Bolts | ItemTypes.Combat;
                    break;
                case ItemObject.ItemTypeEnum.Shield:
                    types |= ItemTypes.Shield | ItemTypes.Combat;
                    break;
                case ItemObject.ItemTypeEnum.Horse:
                    types |= ItemTypes.Mount;
                    break;
                case ItemObject.ItemTypeEnum.HorseHarness:
                    types |= ItemTypes.HorseArmor;
                    break;
                case ItemObject.ItemTypeEnum.Goods:
                    types |= ItemTypes.Goods | ItemTypes.Trade;
                    break;
                case ItemObject.ItemTypeEnum.HeadArmor:
                    types |= ItemTypes.Armor | ItemTypes.HeadArmor;
                    break;
                case ItemObject.ItemTypeEnum.BodyArmor:
                    types |= ItemTypes.Armor | ItemTypes.BodyArmor;
                    break;
                case ItemObject.ItemTypeEnum.LegArmor:
                    types |= ItemTypes.Armor | ItemTypes.LegArmor;
                    break;
                case ItemObject.ItemTypeEnum.HandArmor:
                    types |= ItemTypes.Armor | ItemTypes.HandArmor;
                    break;
                case ItemObject.ItemTypeEnum.Cape:
                    types |= ItemTypes.Armor | ItemTypes.Cape;
                    break;
            }

            // Apply civilian flag based on native ItemFlags.Civilian
            if (hasNativeCivilianFlag)
                types |= ItemTypes.Civilian;

            // Food items are also considered civilian
            if (item.IsFood)
                types |= ItemTypes.Civilian;

            return types;
        }

        /// <summary>
        /// Checks if item has ALL specified flags
        /// </summary>
        public static bool HasAllTypes(this ItemObject item, ItemTypes types)
        {
            if (types == ItemTypes.None) return true;
            var itemTypes = item.GetItemTypes();
            return (itemTypes & types) == types;
        }

        /// <summary>
        /// Checks if item has ANY of the specified flags
        /// </summary>
        public static bool HasAnyType(this ItemObject item, ItemTypes types)
        {
            if (types == ItemTypes.None) return true;
            ItemTypes itemTypes = item.GetItemTypes();
            return (itemTypes & types) != ItemTypes.None;
        }

        /// <summary>
        /// Checks if item can be used in civilian loadout using the native ItemFlags.Civilian flag.
        /// This is the same flag the game UI uses to show the civilian equipment icon.
        /// </summary>
        public static bool IsCivilian(this ItemObject item)
        {
            // Use native ItemFlags.Civilian (value 4194304) - same check as game UI
            return (item.ItemFlags & ItemFlags.Civilian) == ItemFlags.Civilian;
        }

        /// <summary>
        /// Returns a formatted string containing the item's details
        /// </summary>
        public static string FormattedDetails(this ItemObject item)
        {
            // Note: ItemTiers enum values are offset by 1 (Tier0=-1, Tier1=0, Tier2=1, etc.)
            // So we add 1 to display the user-friendly tier number
            string tier = (int)item.Tier >= -1 ? $"Tier: {(int)item.Tier + 1}" : "Tier: N/A";
            string culture = item.Culture?.Name?.ToString() ?? "None";
            string loadout = item.IsCivilian() ? "Civilian" : "Battle";
            return $"{item.StringId}\t{item.Name}\t{culture}\tType: {item.ItemType}\t{tier}\tValue: {item.Value}\t{loadout}";
        }

        /// <summary>
        /// Alias for GetItemTypes to match IEntityExtensions interface
        /// </summary>
        public static ItemTypes GetTypes(this ItemObject item) => item.GetItemTypes();
    }

    /// <summary>
    /// Wrapper class implementing IEntityExtensions interface for ItemObject entities
    /// </summary>
    public class ItemExtensionsWrapper : IEntityExtensions<ItemObject, ItemTypes>
    {
        public ItemTypes GetTypes(ItemObject entity) => entity.GetItemTypes();
        public bool HasAllTypes(ItemObject entity, ItemTypes types) => entity.HasAllTypes(types);
        public bool HasAnyType(ItemObject entity, ItemTypes types) => entity.HasAnyType(types);
        public string FormattedDetails(ItemObject entity) => entity.FormattedDetails();
    }
}