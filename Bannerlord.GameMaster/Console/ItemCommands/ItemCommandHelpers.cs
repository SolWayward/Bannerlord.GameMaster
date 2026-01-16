using System;
using System.Collections.Generic;
using TaleWorlds.Core;

namespace Bannerlord.GameMaster.Console.ItemCommands
{
    /// <summary>
    /// Helper methods for item commands - equipment slot mapping and parsing
    /// </summary>
    public static class ItemCommandHelpers
    {
        #region Slot Mapping

        /// <summary>
        /// Determines the appropriate equipment slot for an item based on its type
        /// </summary>
        /// <param name="item">The item to find a slot for</param>
        /// <returns>The appropriate EquipmentIndex, or EquipmentIndex.None if no slot is available</returns>
        public static EquipmentIndex GetAppropriateSlotForItem(ItemObject item)
        {
            switch (item.ItemType)
            {
                case ItemObject.ItemTypeEnum.HeadArmor:
                    return EquipmentIndex.Head;
                case ItemObject.ItemTypeEnum.BodyArmor:
                    return EquipmentIndex.Body;
                case ItemObject.ItemTypeEnum.LegArmor:
                    return EquipmentIndex.Leg;
                case ItemObject.ItemTypeEnum.HandArmor:
                    return EquipmentIndex.Gloves;
                case ItemObject.ItemTypeEnum.Cape:
                    return EquipmentIndex.Cape;
                case ItemObject.ItemTypeEnum.Horse:
                    return EquipmentIndex.Horse;
                case ItemObject.ItemTypeEnum.HorseHarness:
                    return EquipmentIndex.HorseHarness;
                case ItemObject.ItemTypeEnum.OneHandedWeapon:
                case ItemObject.ItemTypeEnum.TwoHandedWeapon:
                case ItemObject.ItemTypeEnum.Polearm:
                case ItemObject.ItemTypeEnum.Bow:
                case ItemObject.ItemTypeEnum.Crossbow:
                case ItemObject.ItemTypeEnum.Thrown:
                case ItemObject.ItemTypeEnum.Arrows:
                case ItemObject.ItemTypeEnum.Bolts:
                case ItemObject.ItemTypeEnum.Shield:
                    return EquipmentIndex.Weapon0; // Use first weapon slot by default
                default:
                    return EquipmentIndex.None;
            }
        }

        /// <summary>
        /// Parses equipment slot name to EquipmentIndex enum
        /// </summary>
        /// <param name="slotName">The slot name string (e.g., "Head", "Weapon0", "Body")</param>
        /// <param name="slot">The parsed EquipmentIndex output</param>
        /// <returns>True if parsing was successful, false otherwise</returns>
        public static bool TryParseEquipmentSlot(string slotName, out EquipmentIndex slot)
        {
            // Handle numeric weapon slots (Weapon0-3)
            if (slotName.StartsWith("Weapon", StringComparison.OrdinalIgnoreCase))
            {
                if (slotName.Length == 7 && char.IsDigit(slotName[6]))
                {
                    int weaponNum = int.Parse(slotName.Substring(6));
                    if (weaponNum >= 0 && weaponNum <= 3)
                    {
                        slot = EquipmentIndex.Weapon0 + weaponNum;
                        return true;
                    }
                }
            }

            // Try to parse as enum
            if (Enum.TryParse<EquipmentIndex>(slotName, true, out slot))
            {
                // Validate it's within equipment slot range
                if (slot >= EquipmentIndex.WeaponItemBeginSlot && slot < EquipmentIndex.NumEquipmentSetSlots)
                    return true;
            }

            slot = EquipmentIndex.None;
            return false;
        }

        #endregion

        #region Equipment Display

        /// <summary>
        /// Gets a list of equipment items for display purposes
        /// </summary>
        /// <param name="equipment">The equipment set to list</param>
        /// <returns>List of EquipmentItemInfo for display</returns>
        public static List<EquipmentItemInfo> GetEquipmentList(Equipment equipment)
        {
            List<EquipmentItemInfo> items = new();

            for (int i = 0; i < (int)EquipmentIndex.NumEquipmentSetSlots; i++)
            {
                EquipmentIndex slot = (EquipmentIndex)i;
                EquipmentElement element = equipment[slot];

                if (!element.IsEmpty)
                {
                    string modifierText = element.ItemModifier != null ? $" ({element.ItemModifier.Name})" : "";
                    items.Add(new EquipmentItemInfo
                    {
                        Slot = slot.ToString(),
                        ItemName = element.Item.Name?.ToString() ?? "",
                        ModifierText = modifierText
                    });
                }
            }

            return items;
        }

        #endregion
    }
}
