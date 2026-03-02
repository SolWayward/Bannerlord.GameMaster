using System.Text;
using Bannerlord.GameMaster.Common;
using Bannerlord.GameMaster.Items;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.Core;
using TaleWorlds.Library;

namespace Bannerlord.GameMaster.Behaviours
{
    /// <summary>
    /// Detects BLGM heroes with trash or missing equipment after save load
    /// and re-equips them using HeroEquipper.
    /// <br/>
    /// Fires on OnGameLoadFinishedEvent which runs after the native
    /// CheckInvalidEquipmentsAndReplaceIfNeeded() has already replaced
    /// !IsReady items with DefaultItems.Trash or cleared slots.
    /// </summary>
    internal class EquipmentRepairBehavior : CampaignBehaviorBase
    {
        public override void RegisterEvents()
        {
            CampaignEvents.OnGameLoadFinishedEvent.AddNonSerializedListener(this, OnGameLoadFinished);
        }

        public override void SyncData(IDataStore dataStore)
        {
            // No persistent state needed
        }

        /// MARK: OnGameLoadFinished
        /// <summary>
        /// Scans all BLGM heroes for damaged equipment and re-equips them.
        /// Runs once after save load is fully complete.
        /// </summary>
        private void OnGameLoadFinished()
        {
            RepairBlgmHeroEquipment();
        }

        /// MARK: RepairBlgmHeroEquipment
        /// <summary>
        /// Iterates all BLGM-tracked heroes, detects equipment damage from save load,
        /// and re-equips affected heroes using HeroEquipper.EquipHeroByStats().
        /// Also removes DefaultItems.Trash from party inventories.
        /// </summary>
        private static void RepairBlgmHeroEquipment()
        {
            MBList<Hero> blgmHeroes = BLGMObjectManager.BlgmHeroes;
            if (blgmHeroes == null || blgmHeroes.Count == 0)
                return;

            // Guard against extremely unlikely null trash item (would indicate broken game state)
            if (DefaultItems.Trash == null)
                return;

            HeroEquipper equipper = null;
            int repairedCount = 0;
            StringBuilder repairLog = new();

            for (int i = 0; i < blgmHeroes.Count; i++)
            {
                Hero hero = blgmHeroes[i];

                if (!HeroNeedsEquipmentRepair(hero))
                    continue;

                // Lazy-initialize equipper only when first hero actually needs repair
                if (equipper == null)
                    equipper = new();

                // Re-equip battle and civilian equipment using hero's stats
                BLGMResult result = equipper.EquipHeroByStats(
                    hero,
                    tier: -1,
                    weaponPreferences: WeaponTypeFlags.None,
                    replaceBattleEquipment: true,
                    replaceCivilianEquipment: true);

                if (result.IsSuccess)
                {
                    repairedCount++;
                    repairLog.AppendLine($"  Repaired: {hero.Name} (ID: {hero.StringId})");
                }

                // Remove trash items from party inventory
                RemoveTrashFromPartyInventory(hero);
            }

            if (repairedCount > 0)
            {
                BLGMResult.Success(
                    $"Equipment repair: Re-equipped {repairedCount} BLGM heroes with damaged equipment after save load.\n{repairLog}")
                    .DisplayAndLog();
            }
        }

        /// MARK: HeroNeedsEquipmentRepair
        /// <summary>
        /// Determines if a hero's equipment was damaged by the native trash replacement.
        /// Checks for DefaultItems.Trash and !IsReady items only.
        /// Empty slots are intentional and should not trigger repair.
        /// </summary>
        /// <param name="hero">The hero to check.</param>
        /// <returns>True if equipment needs repair; false otherwise.</returns>
        private static bool HeroNeedsEquipmentRepair(Hero hero)
        {
            if (hero == null || hero.IsDead)
                return false;

            // Only repair if trash or !IsReady items are present
            // (the actual signature of native trash replacement damage)
            if (HasDamagedEquipment(hero.BattleEquipment))
                return true;

            if (HasDamagedEquipment(hero.CivilianEquipment))
                return true;

            return false;
        }

        /// MARK: HasDamagedEquipment
        /// <summary>
        /// Checks an equipment set for trash items or items with IsReady = false.
        /// </summary>
        /// <param name="equipment">The equipment to check.</param>
        /// <returns>True if any slot contains damaged equipment.</returns>
        private static bool HasDamagedEquipment(Equipment equipment)
        {
            if (equipment == null)
                return false;

            for (int i = 0; i < (int)EquipmentIndex.NumEquipmentSetSlots; i++)
            {
                EquipmentElement element = equipment[(EquipmentIndex)i];

                if (element.Item == DefaultItems.Trash)
                    return true;

                if (element.Item != null && !element.Item.IsReady)
                    return true;
            }

            return false;
        }

        /// MARK: RemoveTrashFromInventory
        /// <summary>
        /// Removes all DefaultItems.Trash from a hero's party inventory.
        /// The native trash replacement adds trash items to the party ItemRoster.
        /// </summary>
        /// <param name="hero">The hero whose party inventory to clean.</param>
        private static void RemoveTrashFromPartyInventory(Hero hero)
        {
            MobileParty party = hero.PartyBelongedTo;
            if (party == null)
                return;

            int trashCount = party.ItemRoster.GetItemNumber(DefaultItems.Trash);
            if (trashCount > 0)
            {
                party.ItemRoster.AddToCounts(DefaultItems.Trash, -trashCount);
            }
        }
    }
}
