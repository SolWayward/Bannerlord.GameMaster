using System;
using Bannerlord.GameMaster.Common;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.CharacterDevelopment;
using TaleWorlds.Core;
using TaleWorlds.Library;

namespace Bannerlord.GameMaster.Heroes.HeroDevelopment
{
    /// <summary>
    /// Provides unrestricted focus point editing operations for any hero.
    /// Uses native HeroDeveloper.AddFocus (with checkUnspentFocusPoints: false) and
    /// RemoveFocus, both of which are public. No reflection needed.
    /// </summary>
    public static class FocusEditor
    {
        /// MARK: SetFocus
        /// <summary>
        /// Sets a hero's focus for a skill to an exact value. Computes the delta from current
        /// and calls AddFocus or RemoveFocus accordingly.
        /// Bypasses unspent focus point restrictions by passing checkUnspentFocusPoints: false.
        /// </summary>
        /// <param name="hero">The hero to modify</param>
        /// <param name="skill">The skill to set focus for</param>
        /// <param name="value">The target focus value (clamped to 0-MaxFocusPerSkill)</param>
        /// <returns>BLGMResult indicating success or failure</returns>
        public static BLGMResult SetFocus(Hero hero, SkillObject skill, int value)
        {
            if (hero == null)
            {
                return BLGMResult.Error("SetFocus() failed, hero cannot be null",
                    new ArgumentNullException(nameof(hero))).Log();
            }

            if (skill == null)
            {
                return BLGMResult.Error("SetFocus() failed, skill cannot be null",
                    new ArgumentNullException(nameof(skill))).Log();
            }

            int maxFocus = Campaign.Current.Models.CharacterDevelopmentModel.MaxFocusPerSkill;
            int targetValue = MBMath.ClampInt(value, 0, maxFocus);
            int currentFocus = hero.HeroDeveloper.GetFocus(skill);
            int delta = targetValue - currentFocus;

            if (delta == 0)
            {
                return BLGMResult.Success(
                    $"{hero.Name}'s {skill.Name} focus is already {currentFocus}");
            }

            if (delta > 0)
            {
                hero.HeroDeveloper.AddFocus(skill, delta, false);
            }

            else
            {
                hero.HeroDeveloper.RemoveFocus(skill, -delta);
            }

            int actualFocus = hero.HeroDeveloper.GetFocus(skill);

            return BLGMResult.Success(
                $"Set {hero.Name}'s {skill.Name} focus from {currentFocus} to {actualFocus}");
        }

        /// MARK: AddFocus
        /// <summary>
        /// Adds or removes focus points by a delta amount.
        /// Positive delta increases, negative delta decreases.
        /// Bypasses unspent focus point cost.
        /// </summary>
        /// <param name="hero">The hero to modify</param>
        /// <param name="skill">The skill to change focus for</param>
        /// <param name="delta">The amount to change (positive to add, negative to remove)</param>
        /// <returns>BLGMResult indicating success or failure</returns>
        public static BLGMResult AddFocus(Hero hero, SkillObject skill, int delta)
        {
            if (hero == null)
            {
                return BLGMResult.Error("AddFocus() failed, hero cannot be null",
                    new ArgumentNullException(nameof(hero))).Log();
            }

            if (skill == null)
            {
                return BLGMResult.Error("AddFocus() failed, skill cannot be null",
                    new ArgumentNullException(nameof(skill))).Log();
            }

            if (delta == 0)
            {
                return BLGMResult.Success($"{hero.Name}'s {skill.Name} focus unchanged (delta is 0)");
            }

            int previousFocus = hero.HeroDeveloper.GetFocus(skill);

            if (delta > 0)
            {
                hero.HeroDeveloper.AddFocus(skill, delta, false);
            }

            else
            {
                hero.HeroDeveloper.RemoveFocus(skill, -delta);
            }

            int actualFocus = hero.HeroDeveloper.GetFocus(skill);

            return BLGMResult.Success(
                $"Changed {hero.Name}'s {skill.Name} focus from {previousFocus} to {actualFocus} (delta: {delta})");
        }

        /// MARK: RemoveFocus
        /// <summary>
        /// Removes focus points from a skill. Convenience wrapper around native RemoveFocus.
        /// </summary>
        /// <param name="hero">The hero to modify</param>
        /// <param name="skill">The skill to remove focus from</param>
        /// <param name="amount">The amount of focus points to remove (must be positive)</param>
        /// <returns>BLGMResult indicating success or failure</returns>
        public static BLGMResult RemoveFocus(Hero hero, SkillObject skill, int amount)
        {
            if (hero == null)
            {
                return BLGMResult.Error("RemoveFocus() failed, hero cannot be null",
                    new ArgumentNullException(nameof(hero))).Log();
            }

            if (skill == null)
            {
                return BLGMResult.Error("RemoveFocus() failed, skill cannot be null",
                    new ArgumentNullException(nameof(skill))).Log();
            }

            if (amount <= 0)
            {
                return BLGMResult.Error("RemoveFocus() failed, amount must be positive").Log();
            }

            int previousFocus = hero.HeroDeveloper.GetFocus(skill);
            hero.HeroDeveloper.RemoveFocus(skill, amount);
            int actualFocus = hero.HeroDeveloper.GetFocus(skill);

            return BLGMResult.Success(
                $"Removed {amount} focus from {hero.Name}'s {skill.Name}. Focus: {previousFocus} -> {actualFocus}");
        }

        /// MARK: GetFocusValue
        /// <summary>
        /// Gets the current focus value for a skill.
        /// </summary>
        /// <param name="hero">The hero to query</param>
        /// <param name="skill">The skill to query focus for</param>
        /// <returns>The current focus value, or 0 if parameters are null</returns>
        public static int GetFocusValue(Hero hero, SkillObject skill)
        {
            if (hero == null || skill == null)
            {
                return 0;
            }

            return hero.HeroDeveloper.GetFocus(skill);
        }
    }
}
