using System;
using Bannerlord.GameMaster.Common;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.CharacterDevelopment;
using TaleWorlds.Core;
using TaleWorlds.Library;

namespace Bannerlord.GameMaster.Heroes.HeroDevelopment
{
    /// <summary>
    /// Provides unrestricted skill level and XP editing operations for any hero.
    /// Uses native HeroDeveloper APIs: SetInitialSkillLevel for bidirectional level changes,
    /// AddSkillXp for positive XP injection (bypasses focus factor).
    /// </summary>
    public static class SkillEditor
    {
        /// MARK: SetSkillLevel
        /// <summary>
        /// Sets a hero's skill to an exact level by computing and applying the required XP.
        /// Works for both increasing and decreasing skill levels.
        /// Uses native SetInitialSkillLevel which directly sets XP and skill value.
        /// </summary>
        /// <param name="hero">The hero to modify</param>
        /// <param name="skill">The skill to set</param>
        /// <param name="level">The target skill level (clamped to 0-1023)</param>
        /// <returns>BLGMResult indicating success or failure</returns>
        public static BLGMResult SetSkillLevel(Hero hero, SkillObject skill, int level)
        {
            if (hero == null)
            {
                return BLGMResult.Error("SetSkillLevel() failed, hero cannot be null",
                    new ArgumentNullException(nameof(hero))).Log();
            }

            if (skill == null)
            {
                return BLGMResult.Error("SetSkillLevel() failed, skill cannot be null",
                    new ArgumentNullException(nameof(skill))).Log();
            }

            int clampedLevel = MBMath.ClampInt(level, 0, 1023);
            int previousLevel = hero.GetSkillValue(skill);

            hero.HeroDeveloper.SetInitialSkillLevel(skill, clampedLevel);

            return BLGMResult.Success(
                $"Set {hero.Name}'s {skill.Name} from {previousLevel} to {clampedLevel}");
        }

        /// MARK: ChangeSkillLevel
        /// <summary>
        /// Changes a hero's skill level by a delta amount (positive or negative).
        /// For positive deltas, uses native ChangeSkillLevel which adds XP.
        /// For negative deltas, uses SetInitialSkillLevel since native AddSkillXp rejects negative XP.
        /// </summary>
        /// <param name="hero">The hero to modify</param>
        /// <param name="skill">The skill to change</param>
        /// <param name="delta">The amount to change (positive to increase, negative to decrease)</param>
        /// <returns>BLGMResult indicating success or failure</returns>
        public static BLGMResult ChangeSkillLevel(Hero hero, SkillObject skill, int delta)
        {
            if (hero == null)
            {
                return BLGMResult.Error("ChangeSkillLevel() failed, hero cannot be null",
                    new ArgumentNullException(nameof(hero))).Log();
            }

            if (skill == null)
            {
                return BLGMResult.Error("ChangeSkillLevel() failed, skill cannot be null",
                    new ArgumentNullException(nameof(skill))).Log();
            }

            if (delta == 0)
            {
                return BLGMResult.Success($"{hero.Name}'s {skill.Name} unchanged (delta is 0)");
            }

            int previousLevel = hero.GetSkillValue(skill);
            int targetLevel = MBMath.ClampInt(previousLevel + delta, 0, 1023);

            if (delta > 0)
            {
                // Native ChangeSkillLevel works for positive deltas (computes XP and calls AddSkillXp)
                hero.HeroDeveloper.ChangeSkillLevel(skill, targetLevel - previousLevel, true);
            }

            else
            {
                // For negative deltas, SetInitialSkillLevel handles both directions cleanly
                hero.HeroDeveloper.SetInitialSkillLevel(skill, targetLevel);
            }

            int actualLevel = hero.GetSkillValue(skill);

            return BLGMResult.Success(
                $"Changed {hero.Name}'s {skill.Name} from {previousLevel} to {actualLevel} (delta: {delta})");
        }

        /// MARK: AddSkillXp
        /// <summary>
        /// Injects XP directly into a skill. Always positive in native API.
        /// Bypasses focus factor by passing isAffectedByFocusFactor: false.
        /// </summary>
        /// <param name="hero">The hero to modify</param>
        /// <param name="skill">The skill to add XP to</param>
        /// <param name="xp">The amount of XP to add (must be positive)</param>
        /// <returns>BLGMResult indicating success or failure</returns>
        public static BLGMResult AddSkillXp(Hero hero, SkillObject skill, float xp)
        {
            if (hero == null)
            {
                return BLGMResult.Error("AddSkillXp() failed, hero cannot be null",
                    new ArgumentNullException(nameof(hero))).Log();
            }

            if (skill == null)
            {
                return BLGMResult.Error("AddSkillXp() failed, skill cannot be null",
                    new ArgumentNullException(nameof(skill))).Log();
            }

            if (xp <= 0f)
            {
                return BLGMResult.Error("AddSkillXp() failed, XP must be positive (native rejects <= 0)").Log();
            }

            int previousLevel = hero.GetSkillValue(skill);
            float previousXp = hero.HeroDeveloper.GetSkillXp(skill);

            // Pass isAffectedByFocusFactor: false to bypass focus/attribute multipliers
            hero.HeroDeveloper.AddSkillXp(skill, xp, false, true);

            int newLevel = hero.GetSkillValue(skill);
            float newXp = hero.HeroDeveloper.GetSkillXp(skill);

            return BLGMResult.Success(
                $"Added {xp:F0} XP to {hero.Name}'s {skill.Name}. " +
                $"Level: {previousLevel} -> {newLevel}, XP: {previousXp:F0} -> {newXp:F0}");
        }

        /// MARK: GetSkillInfo
        /// <summary>
        /// Returns information about a hero's skill including current level, total XP,
        /// XP progress toward next level, and XP required for next level.
        /// </summary>
        /// <param name="hero">The hero to query</param>
        /// <param name="skill">The skill to get info for</param>
        /// <returns>A SkillInfo struct with skill details, or default if parameters are null</returns>
        public static SkillInfo GetSkillInfo(Hero hero, SkillObject skill)
        {
            if (hero == null || skill == null)
            {
                return default;
            }

            HeroDeveloper developer = hero.HeroDeveloper;
            int level = hero.GetSkillValue(skill);
            float totalXp = developer.GetSkillXp(skill);
            int xpProgress = developer.GetSkillXpProgress(skill);
            int xpForCurrentLevel = Campaign.Current.Models.CharacterDevelopmentModel.GetXpRequiredForSkillLevel(level);
            int xpForNextLevel = Campaign.Current.Models.CharacterDevelopmentModel.GetXpRequiredForSkillLevel(level + 1);
            int xpNeededForNext = xpForNextLevel - xpForCurrentLevel;
            int focus = developer.GetFocus(skill);
            float learningRate = developer.GetFocusFactor(skill);

            return new SkillInfo(
                skill,
                level,
                totalXp,
                xpProgress,
                xpNeededForNext,
                focus,
                learningRate);
        }
    }

    /// <summary>
    /// Immutable struct containing detailed information about a hero's skill state.
    /// </summary>
    public readonly struct SkillInfo
    {
        public SkillObject Skill { get; }
        public int Level { get; }
        public float TotalXp { get; }
        public int XpProgress { get; }
        public int XpNeededForNextLevel { get; }
        public int Focus { get; }
        public float LearningRate { get; }

        public SkillInfo(SkillObject skill, int level, float totalXp, int xpProgress,
            int xpNeededForNextLevel, int focus, float learningRate)
        {
            Skill = skill;
            Level = level;
            TotalXp = totalXp;
            XpProgress = xpProgress;
            XpNeededForNextLevel = xpNeededForNextLevel;
            Focus = focus;
            LearningRate = learningRate;
        }

        public override string ToString()
        {
            return $"{Skill?.Name}: Level {Level}, XP: {TotalXp:F0} ({XpProgress}/{XpNeededForNextLevel}), " +
                   $"Focus: {Focus}, Learning Rate: {LearningRate:F2}";
        }
    }
}
