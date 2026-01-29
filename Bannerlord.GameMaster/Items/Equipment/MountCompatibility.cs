using TaleWorlds.Core;

namespace Bannerlord.GameMaster.Items
{
    /// <summary>
    /// Provides static methods for determining weapon and item compatibility with mounted combat.
    /// Used by the HeroOutfitter system when generating equipment for mounted heroes.
    /// </summary>
    public static class MountCompatibility
    {
        /// MARK: IsWepUsableOnMount
        /// <summary>
        /// Determines if a weapon can be used while mounted on a horse.
        /// Excludes weapons that cannot be reloaded on horseback or require two hands.
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

            // Check if weapon cannot be reloaded on horseback (e.g., crossbows)
            // WeaponFlags.CantReloadOnHorseback = 262144UL
            if (HasWeaponFlag(primaryWeapon, WeaponFlags.CantReloadOnHorseback))
                return false;

            // Check if weapon cannot be used with one hand (two-handed weapons)
            // Two-handed weapons are generally not usable on mounts
            // WeaponFlags.NotUsableWithOneHand = 16UL
            if (HasWeaponFlag(primaryWeapon, WeaponFlags.NotUsableWithOneHand))
                return false;

            return true;
        }

        /// MARK: IsWepUsableOnMount
        /// <summary>
        /// Determines if a weapon is usable on mount considering weapon type-specific exceptions.
        /// Some two-handed weapons like polearms are designed for mounted combat despite
        /// having the NotUsableWithOneHand flag.
        /// </summary>
        /// <param name="item">The weapon item to check.</param>
        /// <param name="allowPolearms">Whether to allow two-handed polearms (lances, etc.).</param>
        /// <returns>True if the weapon can be used on a mount; false otherwise.</returns>
        public static bool IsWeaponUsableOnMount(ItemObject item, bool allowPolearms)
        {
            if (item == null)
                return false;

            // Non-weapons are always "usable" on mount
            if (!item.HasWeaponComponent)
                return true;

            WeaponComponentData primaryWeapon = item.PrimaryWeapon;
            if (primaryWeapon == null)
                return true;

            // Check if weapon cannot be reloaded on horseback
            if (HasWeaponFlag(primaryWeapon, WeaponFlags.CantReloadOnHorseback))
                return false;

            // Check two-handed restriction
            if (HasWeaponFlag(primaryWeapon, WeaponFlags.NotUsableWithOneHand))
            {
                // Allow polearms if specified (they are designed for mounted use)
                if (allowPolearms && item.ItemType == ItemObject.ItemTypeEnum.Polearm)
                    return true;

                return false;
            }

            return true;
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
