using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;

namespace Bannerlord.GameMaster.Items
{
	/// <summary>
	/// Provides equipment transfer and validation methods for Hero inventory management.
	/// Used by Commander's Hero Editor for popup inventory system.
	/// </summary>
	public static class EquipmentTransfer
	{
		#region Public Methods

		/// <summary>
		/// Removes an item from the specified equipment slot and returns it for temporary storage.
		/// </summary>
		/// <param name="hero">The hero whose equipment is being modified</param>
		/// <param name="equipment">The equipment set (hero.BattleEquipment or hero.CivilianEquipment)</param>
		/// <param name="slot">The slot to remove the item from</param>
		/// <returns>The EquipmentElement containing item and modifier, or EquipmentElement.Invalid if slot is empty</returns>
		/// <example>
		/// var removedItem = EquipmentTransfer.TransferItemToTemporaryStorage(hero, hero.BattleEquipment, EquipmentIndex.Head);
		/// if (!removedItem.IsEmpty)
		/// {
		///     // Store in temporary inventory
		///     temporaryStorage.Add(removedItem);
		/// }
		/// </example>
		public static EquipmentElement TransferItemToTemporaryStorage(Hero hero, TaleWorlds.Core.Equipment equipment, EquipmentIndex slot)
		{
			// Validate parameters
			if (hero == null || equipment == null)
			{
				return EquipmentElement.Invalid;
			}

			// Validate slot is within valid range
			if (slot < EquipmentIndex.WeaponItemBeginSlot || slot >= EquipmentIndex.NumEquipmentSetSlots)
			{
				return EquipmentElement.Invalid;
			}

			// Get the equipment element from the slot (preserves item and modifier)
			EquipmentElement element = equipment[slot];

			// Clear the slot
			equipment[slot] = EquipmentElement.Invalid;

			// Return the element (may be Invalid if slot was empty)
			return element;
		}

		/// <summary>
		/// Equips an item from temporary storage to the specified slot with validation.
		/// </summary>
		/// <param name="hero">The hero whose equipment is being modified</param>
		/// <param name="equipment">The equipment set to modify</param>
		/// <param name="item">The item to equip</param>
		/// <param name="slot">The target equipment slot</param>
		/// <param name="modifier">Optional item modifier (masterwork, legendary, etc.)</param>
		/// <returns>True if successfully equipped, false if validation failed</returns>
		/// <example>
		/// bool success = EquipmentTransfer.TransferItemFromTemporaryStorage(
		///     hero,
		///     hero.BattleEquipment,
		///     selectedItem,
		///     EquipmentIndex.Head,
		///     itemModifier
		/// );
		/// </example>
		public static bool TransferItemFromTemporaryStorage(Hero hero, TaleWorlds.Core.Equipment equipment, ItemObject item, EquipmentIndex slot, ItemModifier modifier = null)
		{
			// Validate parameters
			if (hero == null || equipment == null || item == null)
			{
				return false;
			}

			// Validate slot is within valid range
			if (slot < EquipmentIndex.WeaponItemBeginSlot || slot >= EquipmentIndex.NumEquipmentSetSlots)
			{
				return false;
			}

			// Validate item can go in this slot
			if (!ValidateEquipmentSlot(item, slot))
			{
				return false;
			}

			// Create equipment element with item and modifier
			EquipmentElement element = new(item, modifier);

			// Assign to slot
			equipment[slot] = element;

			return true;
		}

		/// <summary>
		/// Atomically swaps items between two equipment slots.
		/// Handles empty slots gracefully.
		/// </summary>
		/// <param name="hero">The hero whose equipment is being modified</param>
		/// <param name="equipment">The equipment set to modify</param>
		/// <param name="slot1">First equipment slot</param>
		/// <param name="slot2">Second equipment slot</param>
		/// <returns>True if swap was successful</returns>
		/// <example>
		/// // Swap weapon slots via drag-drop
		/// bool swapped = EquipmentTransfer.SwapEquipment(
		///     hero,
		///     hero.BattleEquipment,
		///     EquipmentIndex.Weapon0,
		///     EquipmentIndex.Weapon1
		/// );
		/// </example>
		public static bool SwapEquipment(Hero hero, TaleWorlds.Core.Equipment equipment, EquipmentIndex slot1, EquipmentIndex slot2)
		{
			// Validate parameters
			if (hero == null || equipment == null)
			{
				return false;
			}

			// Validate both slots are within valid range
			if (slot1 < EquipmentIndex.WeaponItemBeginSlot || slot1 >= EquipmentIndex.NumEquipmentSetSlots ||
			    slot2 < EquipmentIndex.WeaponItemBeginSlot || slot2 >= EquipmentIndex.NumEquipmentSetSlots)
			{
				return false;
			}

			// Get elements from both slots
			EquipmentElement element1 = equipment[slot1];
			EquipmentElement element2 = equipment[slot2];

			// Perform atomic swap
			equipment[slot1] = element2;
			equipment[slot2] = element1;

			return true;
		}

		/// <summary>
		/// Validates if an item type is compatible with an equipment slot.
		/// Performs basic type matching only (HeadArmor → Head slot, etc.).
		/// Weapon slots (Weapon0-3) accept any weapon or shield type.
		/// </summary>
		/// <param name="item">The item to validate</param>
		/// <param name="slot">The target equipment slot</param>
		/// <returns>True if item type matches slot type, false otherwise</returns>
		/// <example>
		/// if (EquipmentTransfer.ValidateEquipmentSlot(helmItem, EquipmentIndex.Head))
		/// {
		///     // Can equip helm to head slot
		/// }
		/// </example>
		public static bool ValidateEquipmentSlot(ItemObject item, EquipmentIndex slot)
		{
			// Validate item
			if (item == null)
			{
				return false;
			}

			// Get expected item type for slot
			ItemObject.ItemTypeEnum? expectedType = GetItemTypeForSlot(slot);

			// Weapon slots accept multiple types (any weapon or shield)
			if (expectedType == null)
			{
				// Weapon slots - accept weapons and shields
				return item.ItemType == ItemObject.ItemTypeEnum.OneHandedWeapon ||
				       item.ItemType == ItemObject.ItemTypeEnum.TwoHandedWeapon ||
				       item.ItemType == ItemObject.ItemTypeEnum.Polearm ||
				       item.ItemType == ItemObject.ItemTypeEnum.Bow ||
				       item.ItemType == ItemObject.ItemTypeEnum.Crossbow ||
				       item.ItemType == ItemObject.ItemTypeEnum.Thrown ||
				       item.ItemType == ItemObject.ItemTypeEnum.Shield ||
				       item.ItemType == ItemObject.ItemTypeEnum.Musket ||
				       item.ItemType == ItemObject.ItemTypeEnum.Pistol ||
				       item.ItemType == ItemObject.ItemTypeEnum.Banner;
			}

			// For non-weapon slots, item type must match exactly
			return item.ItemType == expectedType.Value;
		}

		#endregion

		#region Private Helper Methods

		/// <summary>
		/// Maps an equipment slot to its expected item type.
		/// Returns null for weapon slots (which accept multiple types).
		/// </summary>
		/// <param name="slot">The equipment slot</param>
		/// <returns>Expected ItemTypeEnum, or null for weapon slots</returns>
		private static ItemObject.ItemTypeEnum? GetItemTypeForSlot(EquipmentIndex slot)
		{
			switch (slot)
			{
				case EquipmentIndex.Head:
					return ItemObject.ItemTypeEnum.HeadArmor;
				case EquipmentIndex.Body:
					return ItemObject.ItemTypeEnum.BodyArmor;
				case EquipmentIndex.Leg:
					return ItemObject.ItemTypeEnum.LegArmor;
				case EquipmentIndex.Gloves:
					return ItemObject.ItemTypeEnum.HandArmor;
				case EquipmentIndex.Cape:
					return ItemObject.ItemTypeEnum.Cape;
				case EquipmentIndex.Horse:
					return ItemObject.ItemTypeEnum.Horse;
				case EquipmentIndex.HorseHarness:
					return ItemObject.ItemTypeEnum.HorseHarness;
				case EquipmentIndex.Weapon0:
				case EquipmentIndex.Weapon1:
				case EquipmentIndex.Weapon2:
				case EquipmentIndex.Weapon3:
					// Weapon slots accept multiple types
					return null;
				default:
					return null;
			}
		}

		#endregion
	}
}
