using System;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace Bannerlord.GameMaster.Items
{
    /// <summary>
    /// Provides static methods for determining weapon and item compatibility with mounted combat.
    /// Used by the HeroOutfitter system when generating equipment for mounted heroes.
    /// </summary>
    public static class MountCompatibility
    {
        /// MARK: IsCouchablePolearm
        /// <summary>
        /// Determines if a polearm can be couched while mounted.
        /// Uses native detection: WeaponDescriptionId contains "couch".
        /// Couchable polearms are designed for mounted combat.
        /// </summary>
        /// <param name="item">The polearm item to check.</param>
        /// <returns>True if the polearm can be couched; false otherwise.</returns>
        public static bool IsCouchablePolearm(ItemObject item)
        {
            if (item == null || !item.HasWeaponComponent)
                return false;

            // Check all weapon components (native pattern from MissionAgentStatusVM.IsWeaponCouchable())
            MBReadOnlyList<WeaponComponentData> weapons = item.Weapons;
            if (weapons == null)
                return false;

            for (int i = 0; i < weapons.Count; i++)
            {
                string weaponDescriptionId = weapons[i].WeaponDescriptionId;
                if (weaponDescriptionId != null &&
                    weaponDescriptionId.IndexOf("couch", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    return true;
                }
            }
            return false;
        }

        /// MARK: IsBraceablePolearm
        /// <summary>
        /// Determines if a polearm can be braced (infantry anti-cavalry).
        /// Uses native detection: WeaponDescriptionId contains "bracing".
        /// Braceable polearms (pikes) are infantry-only weapons.
        /// </summary>
        /// <param name="item">The polearm item to check.</param>
        /// <returns>True if the polearm can be braced; false otherwise.</returns>
        public static bool IsBraceablePolearm(ItemObject item)
        {
            if (item == null || !item.HasWeaponComponent)
                return false;

            // Check all weapon components (native pattern from MissionAgentStatusVM.IsWeaponBracable())
            MBReadOnlyList<WeaponComponentData> weapons = item.Weapons;
            if (weapons == null)
                return false;

            for (int i = 0; i < weapons.Count; i++)
            {
                string weaponDescriptionId = weapons[i].WeaponDescriptionId;
                if (weaponDescriptionId != null &&
                    weaponDescriptionId.IndexOf("bracing", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    return true;
                }
            }
            return false;
        }

        /// MARK: IsPolearmUsableOnMount
        /// <summary>
        /// Determines if a polearm can be used effectively while mounted.
        /// Braceable polearms (pikes) are infantry-only and should NOT be given to mounted heroes.
        /// Couchable polearms are explicitly designed for mounted combat.
        /// </summary>
        /// <param name="item">The polearm item to check.</param>
        /// <returns>True if the polearm can be used on a mount; false otherwise.</returns>
        public static bool IsPolearmUsableOnMount(ItemObject item)
        {
            if (item == null)
                return false;

            if (item.ItemType != ItemObject.ItemTypeEnum.Polearm)
                return false;

            // Braceable polearms are infantry weapons - NOT for mounted use
            // Exception: If polearm is BOTH couchable AND braceable, it can be used mounted
            if (IsBraceablePolearm(item) && !IsCouchablePolearm(item))
                return false;

            // Couchable polearms are explicitly for mounted combat
            if (IsCouchablePolearm(item))
                return true;

            // For other polearms, use standard mount usability flags
            return IsWeaponUsableOnMount(item);
        }

        /// MARK: IsWepUsableOnMount
        /// <summary>
        /// Determines if a weapon can be used while mounted on a horse.
        /// Uses native ItemUsageSetFlags check via MBItem.GetItemUsageSetFlags().
        /// Also respects CantReloadOnHorseback flag for crossbows.
        /// </summary>
        /// <param name="item">The weapon item to check.</param>
        /// <returns>True if the weapon can be used on a mount; false otherwise.</returns>
        public static bool IsWeaponUsableOnMount(ItemObject item)
        {
            if (item == null)
                return false;

            // Non-weapons are always "usable" on mount (armor, etc.)
            if (!item.HasWeaponComponent)
                return true;

            WeaponComponentData primaryWeapon = item.PrimaryWeapon;
            if (primaryWeapon == null)
                return true;

            // Check ItemUsageSetFlags
            if (!string.IsNullOrEmpty(primaryWeapon.ItemUsage))
            {
                ItemObject.ItemUsageSetFlags usageFlags = MBItem.GetItemUsageSetFlags(primaryWeapon.ItemUsage);

                // RequiresNoMount = 2 - this is the definitive "cannot use on horseback" indicator
                if ((usageFlags & ItemObject.ItemUsageSetFlags.RequiresNoMount) != 0)
                    return false;
            }

            // Also check WeaponFlags for crossbow reload restriction
            // WeaponFlags.CantReloadOnHorseback = 262144UL
            if (HasWeaponFlag(primaryWeapon, WeaponFlags.CantReloadOnHorseback))
                return false;

            return true;
        }

        /// MARK: IsWepUsableOnMount Pole
        /// <summary>
        /// Determines if a weapon is usable on mount considering weapon type-specific exceptions.
        /// Uses native ItemUsageSetFlags and polearm-specific checks.
        /// </summary>
        /// <param name="item">The weapon item to check.</param>
        /// <param name="allowPolearms">Whether to allow polearms (uses IsPolearmUsableOnMount for filtering).</param>
        /// <returns>True if the weapon can be used on a mount; false otherwise.</returns>
        public static bool IsWeaponUsableOnMount(ItemObject item, bool allowPolearms)
        {
            if (item == null)
                return false;

            // Non-weapons are always "usable" on mount
            if (!item.HasWeaponComponent)
                return true;

            // For polearms, use specialized check that excludes braceable-only polearms
            if (allowPolearms && item.ItemType == ItemObject.ItemTypeEnum.Polearm)
                return IsPolearmUsableOnMount(item);

            // Use standard mount compatibility check
            return IsWeaponUsableOnMount(item);
        }

        /// MARK: IsMountOrHarness
        /// <summary>
        /// Determines if an item is a mount (horse) or harness (horse armor).
        /// </summary>
        /// <param name="item">The item to check.</param>
        /// <returns>True if the item is a horse or horse harness; false otherwise.</returns>
        public static bool IsItemMountOrHarness(ItemObject item)
        {
            if (item == null)
                return false;

            // Check if item has horse component (is a mount)
            if (item.HasHorseComponent)
                return true;

            // Check if item type is horse harness
            if (item.ItemType == ItemObject.ItemTypeEnum.HorseHarness)
                return true;

            return false;
        }

        /// MARK: IsMount
        /// <summary>
        /// Determines if an item is specifically a mount (horse, not harness).
        /// </summary>
        /// <param name="item">The item to check.</param>
        /// <returns>True if the item is a horse; false otherwise.</returns>
        public static bool IsMount(ItemObject item)
        {
            if (item == null)
                return false;

            return item.HasHorseComponent;
        }

        /// MARK: IsHarness
        /// <summary>
        /// Determines if an item is specifically a harness (horse armor, not horse).
        /// </summary>
        /// <param name="item">The item to check.</param>
        /// <returns>True if the item is a horse harness; false otherwise.</returns>
        public static bool IsHarness(ItemObject item)
        {
            if (item == null)
                return false;

            // Must be harness type but not have horse component (to exclude horses)
            return item.ItemType == ItemObject.ItemTypeEnum.HorseHarness && !item.HasHorseComponent;
        }

        /// MARK: IsCombatHarness
        /// <summary>
        /// Determines if a harness is appropriate for combat use (not a pack animal harness).
        /// Filters out items designed for mules and pack animals like "Mule Harness with Bags".
        /// </summary>
        /// <param name="item">The harness item to check.</param>
        /// <returns>True if the harness is combat-appropriate; false if it's a pack animal harness.</returns>
        public static bool IsCombatHarness(ItemObject item)
        {
            if (item == null)
                return false;

            // Must be a harness first
            if (!IsHarness(item))
                return false;

            // Check name and StringId for pack animal indicators (case-insensitive)
            string nameLower = item.Name?.ToString()?.ToLowerInvariant() ?? "";
            string idLower = item.StringId?.ToLowerInvariant() ?? "";

            // Filter out mule-related items
            if (nameLower.Contains("mule") || idLower.Contains("mule"))
                return false;

            // Filter out bag-related items (pack harnesses)
            if (nameLower.Contains("bags") || idLower.Contains("bags"))
                return false;

            // Filter out pack animal harnesses
            if (nameLower.Contains("pack") || idLower.Contains("pack"))
                return false;

            return true;
        }

        /// MARK: HasWeaponFlag
        /// <summary>
        /// Checks if a weapon component has a specific weapon flag.
        /// Uses HasAnyFlag extension method pattern for compatibility.
        /// </summary>
        /// <param name="weapon">The weapon component data to check.</param>
        /// <param name="flag">The weapon flag to check for.</param>
        /// <returns>True if the weapon has the specified flag; false otherwise.</returns>
        private static bool HasWeaponFlag(WeaponComponentData weapon, WeaponFlags flag)
        {
            if (weapon == null)
                return false;

            return (weapon.WeaponFlags & flag) == flag;
        }
    }
}
