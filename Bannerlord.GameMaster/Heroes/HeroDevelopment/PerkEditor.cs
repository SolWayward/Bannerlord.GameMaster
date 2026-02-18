using System;
using System.Reflection;
using Bannerlord.GameMaster.Common;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.CharacterDevelopment;
using TaleWorlds.Core;

namespace Bannerlord.GameMaster.Heroes.HeroDevelopment
{
    /// <summary>
    /// Provides unrestricted perk editing operations for any hero.
    /// Uses native HeroDeveloper.AddPerk (public) for selecting perks, and
    /// Hero.SetPerkValueInternal (internal, requires reflection) for deselecting.
    /// Handles permanent bonus reversal when deselecting perks that grant permanent bonuses.
    /// </summary>
    public static class PerkEditor
    {
        #region Cached Reflection

        /// <summary>
        /// Cached MethodInfo for Hero.SetPerkValueInternal(PerkObject, bool).
        /// This is internal in native code but used by PerkResetCampaignBehavior for perk resets.
        /// </summary>
        private static readonly MethodInfo SetPerkValueInternalMethod = typeof(Hero)
            .GetMethod("SetPerkValueInternal", BindingFlags.NonPublic | BindingFlags.Instance);

        #endregion

        /// MARK: SelectPerk
        /// <summary>
        /// Selects (activates) a perk for a hero. Uses native HeroDeveloper.AddPerk which is public.
        /// Does not check skill level requirements.
        /// </summary>
        /// <param name="hero">The hero to modify</param>
        /// <param name="perk">The perk to select</param>
        /// <returns>BLGMResult indicating success or failure</returns>
        public static BLGMResult SelectPerk(Hero hero, PerkObject perk)
        {
            if (hero == null)
            {
                return BLGMResult.Error("SelectPerk() failed, hero cannot be null",
                    new ArgumentNullException(nameof(hero))).Log();
            }

            if (perk == null)
            {
                return BLGMResult.Error("SelectPerk() failed, perk cannot be null",
                    new ArgumentNullException(nameof(perk))).Log();
            }

            if (hero.GetPerkValue(perk))
            {
                return BLGMResult.Success($"{hero.Name} already has perk '{perk.Name}'");
            }

            hero.HeroDeveloper.AddPerk(perk);

            return BLGMResult.Success($"Selected perk '{perk.Name}' for {hero.Name}");
        }

        /// MARK: DeselectPerk
        /// <summary>
        /// Deselects (deactivates) a perk for a hero.
        /// Uses reflection to call Hero.SetPerkValueInternal(perk, false) since it is internal.
        /// Also handles permanent bonus reversal for perks that grant permanent attribute/focus bonuses.
        /// </summary>
        /// <param name="hero">The hero to modify</param>
        /// <param name="perk">The perk to deselect</param>
        /// <returns>BLGMResult indicating success or failure</returns>
        public static BLGMResult DeselectPerk(Hero hero, PerkObject perk)
        {
            if (hero == null)
            {
                return BLGMResult.Error("DeselectPerk() failed, hero cannot be null",
                    new ArgumentNullException(nameof(hero))).Log();
            }

            if (perk == null)
            {
                return BLGMResult.Error("DeselectPerk() failed, perk cannot be null",
                    new ArgumentNullException(nameof(perk))).Log();
            }

            if (!hero.GetPerkValue(perk))
            {
                return BLGMResult.Success($"{hero.Name} does not have perk '{perk.Name}'");
            }

            // Reverse permanent bonuses before deselecting
            ClearPermanentBonusIfExists(hero, perk);

            // Use reflection to call internal SetPerkValueInternal(perk, false)
            BLGMResult reflectionResult = InvokeSetPerkValueInternal(hero, perk, false);

            if (!reflectionResult.IsSuccess)
            {
                return reflectionResult;
            }

            return BLGMResult.Success($"Deselected perk '{perk.Name}' for {hero.Name}");
        }

        /// MARK: TogglePerk
        /// <summary>
        /// Toggles a perk on or off. If currently selected, deselects it. If not selected, selects it.
        /// </summary>
        /// <param name="hero">The hero to modify</param>
        /// <param name="perk">The perk to toggle</param>
        /// <returns>BLGMResult indicating success or failure</returns>
        public static BLGMResult TogglePerk(Hero hero, PerkObject perk)
        {
            if (hero == null)
            {
                return BLGMResult.Error("TogglePerk() failed, hero cannot be null",
                    new ArgumentNullException(nameof(hero))).Log();
            }

            if (perk == null)
            {
                return BLGMResult.Error("TogglePerk() failed, perk cannot be null",
                    new ArgumentNullException(nameof(perk))).Log();
            }

            if (hero.GetPerkValue(perk))
            {
                return DeselectPerk(hero, perk);
            }

            else
            {
                return SelectPerk(hero, perk);
            }
        }

        /// MARK: ClearPerksForSkill
        /// <summary>
        /// Clears all perks for a specific skill. Handles permanent bonus reversal for each perk.
        /// Follows the native PerkResetCampaignBehavior.ClearPerksForSkill pattern.
        /// </summary>
        /// <param name="hero">The hero to modify</param>
        /// <param name="skill">The skill whose perks to clear</param>
        /// <returns>BLGMResult indicating success or failure</returns>
        public static BLGMResult ClearPerksForSkill(Hero hero, SkillObject skill)
        {
            if (hero == null)
            {
                return BLGMResult.Error("ClearPerksForSkill() failed, hero cannot be null",
                    new ArgumentNullException(nameof(hero))).Log();
            }

            if (skill == null)
            {
                return BLGMResult.Error("ClearPerksForSkill() failed, skill cannot be null",
                    new ArgumentNullException(nameof(skill))).Log();
            }

            if (SetPerkValueInternalMethod == null)
            {
                return BLGMResult.Error(
                    "ClearPerksForSkill() failed: SetPerkValueInternal method not found via reflection. Game version may be incompatible.",
                    new MissingMethodException(nameof(Hero), "SetPerkValueInternal")).Log();
            }

            int clearedCount = 0;

            foreach (PerkObject perk in PerkObject.All)
            {
                if (perk.Skill == skill && hero.GetPerkValue(perk))
                {
                    ClearPermanentBonusIfExists(hero, perk);
                    SetPerkValueInternalMethod.Invoke(hero, new object[] { perk, false });
                    clearedCount++;
                }
            }

            // Clamp hit points after clearing perks (some perks affect max HP)
            hero.HitPoints = TaleWorlds.Library.MathF.Min(hero.HitPoints, hero.MaxHitPoints);

            return BLGMResult.Success(
                $"Cleared {clearedCount} perks for {hero.Name}'s {skill.Name}");
        }

        /// MARK: ClearAllPerks
        /// <summary>
        /// Clears all perks for a hero. Handles permanent bonus cleanup first,
        /// then uses native Hero.ClearPerks() which also adjusts HitPoints.
        /// </summary>
        /// <param name="hero">The hero whose perks to clear</param>
        /// <returns>BLGMResult indicating success or failure</returns>
        public static BLGMResult ClearAllPerks(Hero hero)
        {
            if (hero == null)
            {
                return BLGMResult.Error("ClearAllPerks() failed, hero cannot be null",
                    new ArgumentNullException(nameof(hero))).Log();
            }

            // Clear permanent bonuses before wiping all perks
            foreach (PerkObject perk in PerkObject.All)
            {
                ClearPermanentBonusIfExists(hero, perk);
            }

            // Native ClearPerks also clamps HitPoints
            hero.ClearPerks();

            return BLGMResult.Success($"Cleared all perks for {hero.Name}");
        }

        /// MARK: HasPerk
        /// <summary>
        /// Checks if a hero has a specific perk selected.
        /// </summary>
        /// <param name="hero">The hero to check</param>
        /// <param name="perk">The perk to check for</param>
        /// <returns>True if the hero has the perk, false otherwise or if parameters are null</returns>
        public static bool HasPerk(Hero hero, PerkObject perk)
        {
            if (hero == null || perk == null)
            {
                return false;
            }

            return hero.GetPerkValue(perk);
        }

        #region Permanent Bonus Handling

        /// MARK: ClearPermanentBonus
        /// <summary>
        /// Reverses permanent attribute/focus bonuses granted by specific perks.
        /// Replicates the logic from native PerkResetCampaignBehavior.ClearPermanentBonusesIfExists().
        /// Only acts if the hero currently has the perk selected.
        /// </summary>
        /// <param name="hero">The hero to remove permanent bonuses from</param>
        /// <param name="perk">The perk whose permanent bonuses to reverse</param>
        private static void ClearPermanentBonusIfExists(Hero hero, PerkObject perk)
        {
            if (!hero.GetPerkValue(perk))
            {
                return;
            }

            // Crafting perks that grant permanent attribute bonuses
            if (perk == DefaultPerks.Crafting.VigorousSmith)
            {
                hero.HeroDeveloper.RemoveAttribute(DefaultCharacterAttributes.Vigor, 1);
                return;
            }

            if (perk == DefaultPerks.Crafting.StrongSmith)
            {
                hero.HeroDeveloper.RemoveAttribute(DefaultCharacterAttributes.Control, 1);
                return;
            }

            if (perk == DefaultPerks.Crafting.EnduringSmith)
            {
                hero.HeroDeveloper.RemoveAttribute(DefaultCharacterAttributes.Endurance, 1);
                return;
            }

            // Crafting perk that grants permanent focus bonuses
            if (perk == DefaultPerks.Crafting.WeaponMasterSmith)
            {
                hero.HeroDeveloper.RemoveFocus(DefaultSkills.OneHanded, 1);
                hero.HeroDeveloper.RemoveFocus(DefaultSkills.TwoHanded, 1);
                return;
            }

            // Athletics perks that grant permanent attribute bonuses
            if (perk == DefaultPerks.Athletics.Durable)
            {
                hero.HeroDeveloper.RemoveAttribute(DefaultCharacterAttributes.Endurance, 1);
                return;
            }

            if (perk == DefaultPerks.Athletics.Steady)
            {
                hero.HeroDeveloper.RemoveAttribute(DefaultCharacterAttributes.Control, 1);
                return;
            }

            if (perk == DefaultPerks.Athletics.Strong)
            {
                hero.HeroDeveloper.RemoveAttribute(DefaultCharacterAttributes.Vigor, 1);
            }
        }

        #endregion

        #region Reflection Helpers

        /// <summary>
        /// Invokes Hero.SetPerkValueInternal via cached reflection.
        /// </summary>
        /// <param name="hero">The hero instance to invoke on</param>
        /// <param name="perk">The perk to set</param>
        /// <param name="value">True to select, false to deselect</param>
        /// <returns>BLGMResult indicating success or failure</returns>
        private static BLGMResult InvokeSetPerkValueInternal(Hero hero, PerkObject perk, bool value)
        {
            if (SetPerkValueInternalMethod == null)
            {
                return BLGMResult.Error(
                    "InvokeSetPerkValueInternal() failed: SetPerkValueInternal method not found via reflection. Game version may be incompatible.",
                    new MissingMethodException(nameof(Hero), "SetPerkValueInternal")).Log();
            }

            try
            {
                SetPerkValueInternalMethod.Invoke(hero, new object[] { perk, value });
                return BLGMResult.Success($"SetPerkValueInternal({perk.Name}, {value}) invoked for {hero.Name}");
            }
            catch (Exception ex)
            {
                return BLGMResult.Error(
                    $"InvokeSetPerkValueInternal() failed for {hero.Name}, perk '{perk.Name}': {ex.Message}", ex).Log();
            }
        }

        #endregion
    }
}
