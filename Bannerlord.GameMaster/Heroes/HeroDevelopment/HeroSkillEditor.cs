using System;
using System.Collections.Generic;
using System.Text;
using Bannerlord.GameMaster.Common;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.CharacterDevelopment;
using TaleWorlds.CampaignSystem.Extensions;
using TaleWorlds.Core;

namespace Bannerlord.GameMaster.Heroes.HeroDevelopment
{
    /// <summary>
    /// Primary entry point for unrestricted hero character development editing.
    /// Composes SkillEditor, AttributeEditor, FocusEditor, and PerkEditor behind a single facade.
    /// Manages snapshot lifecycle for reset/cancel operations.
    /// All operations work on any hero and bypass native point restrictions.
    /// Both console commands and Commander's UI should call through this class.
    /// </summary>
    public class HeroSkillEditor
    {
        /// <summary>The hero being edited</summary>
        public Hero TargetHero { get; private set; }

        /// <summary>Snapshot captured at creation time for full reset support</summary>
        public HeroSkillSnapshot OriginalSnapshot { get; private set; }

        /// <summary>
        /// Creates a new HeroSkillEditor for the specified hero and captures an initial snapshot.
        /// </summary>
        /// <param name="hero">The hero to edit. Cannot be null.</param>
        public HeroSkillEditor(Hero hero)
        {
            if (hero == null)
            {
                BLGMResult.Error("HeroSkillEditor() failed, hero cannot be null",
                    new ArgumentNullException(nameof(hero))).Log();
                return;
            }

            TargetHero = hero;
            OriginalSnapshot = HeroSkillSnapshot.Capture(hero);
        }

        #region Skill Operations

        /// MARK: SetSkillLevel
        /// <summary>
        /// Sets the target hero's skill to an exact level.
        /// </summary>
        public BLGMResult SetSkillLevel(SkillObject skill, int level)
        {
            return SkillEditor.SetSkillLevel(TargetHero, skill, level);
        }

        /// MARK: ChangeSkillLevel
        /// <summary>
        /// Changes the target hero's skill level by a delta amount.
        /// </summary>
        public BLGMResult ChangeSkillLevel(SkillObject skill, int delta)
        {
            return SkillEditor.ChangeSkillLevel(TargetHero, skill, delta);
        }

        /// MARK: AddSkillXp
        /// <summary>
        /// Injects XP directly into a skill for the target hero.
        /// </summary>
        public BLGMResult AddSkillXp(SkillObject skill, float xp)
        {
            return SkillEditor.AddSkillXp(TargetHero, skill, xp);
        }

        /// MARK: GetSkillInfo
        /// <summary>
        /// Gets detailed skill information for the target hero.
        /// </summary>
        public SkillInfo GetSkillInfo(SkillObject skill)
        {
            return SkillEditor.GetSkillInfo(TargetHero, skill);
        }

        #endregion

        #region Attribute Operations

        /// MARK: SetAttribute
        /// <summary>
        /// Sets the target hero's attribute to an exact value.
        /// </summary>
        public BLGMResult SetAttribute(CharacterAttribute attribute, int value, bool respectMaxCap = true)
        {
            return AttributeEditor.SetAttribute(TargetHero, attribute, value, respectMaxCap);
        }

        /// MARK: AddAttribute
        /// <summary>
        /// Adds or removes attribute points for the target hero by a delta amount.
        /// </summary>
        public BLGMResult AddAttribute(CharacterAttribute attribute, int delta)
        {
            return AttributeEditor.AddAttribute(TargetHero, attribute, delta);
        }

        /// MARK: GetAttributeValue
        /// <summary>
        /// Gets the current attribute value for the target hero.
        /// </summary>
        public int GetAttributeValue(CharacterAttribute attribute)
        {
            return AttributeEditor.GetAttributeValue(TargetHero, attribute);
        }

        #endregion

        #region Focus Operations

        /// MARK: SetFocus
        /// <summary>
        /// Sets the target hero's focus for a skill to an exact value.
        /// </summary>
        public BLGMResult SetFocus(SkillObject skill, int value)
        {
            return FocusEditor.SetFocus(TargetHero, skill, value);
        }

        /// MARK: AddFocus
        /// <summary>
        /// Adds or removes focus points for the target hero by a delta amount.
        /// </summary>
        public BLGMResult AddFocus(SkillObject skill, int delta)
        {
            return FocusEditor.AddFocus(TargetHero, skill, delta);
        }

        /// MARK: RemoveFocus
        /// <summary>
        /// Removes focus points from a skill for the target hero.
        /// </summary>
        public BLGMResult RemoveFocus(SkillObject skill, int amount)
        {
            return FocusEditor.RemoveFocus(TargetHero, skill, amount);
        }

        /// MARK: GetFocusValue
        /// <summary>
        /// Gets the current focus value for a skill for the target hero.
        /// </summary>
        public int GetFocusValue(SkillObject skill)
        {
            return FocusEditor.GetFocusValue(TargetHero, skill);
        }

        #endregion

        #region Perk Operations

        /// MARK: SelectPerk
        /// <summary>
        /// Selects a perk for the target hero.
        /// </summary>
        public BLGMResult SelectPerk(PerkObject perk)
        {
            return PerkEditor.SelectPerk(TargetHero, perk);
        }

        /// MARK: DeselectPerk
        /// <summary>
        /// Deselects a perk for the target hero. Handles permanent bonus reversal.
        /// </summary>
        public BLGMResult DeselectPerk(PerkObject perk)
        {
            return PerkEditor.DeselectPerk(TargetHero, perk);
        }

        /// MARK: TogglePerk
        /// <summary>
        /// Toggles a perk on or off for the target hero.
        /// </summary>
        public BLGMResult TogglePerk(PerkObject perk)
        {
            return PerkEditor.TogglePerk(TargetHero, perk);
        }

        /// MARK: ClearPerksForSkill
        /// <summary>
        /// Clears all perks for a specific skill for the target hero.
        /// </summary>
        public BLGMResult ClearPerksForSkill(SkillObject skill)
        {
            return PerkEditor.ClearPerksForSkill(TargetHero, skill);
        }

        /// MARK: ClearAllPerks
        /// <summary>
        /// Clears all perks for the target hero.
        /// </summary>
        public BLGMResult ClearAllPerks()
        {
            return PerkEditor.ClearAllPerks(TargetHero);
        }

        /// MARK: HasPerk
        /// <summary>
        /// Checks if the target hero has a specific perk.
        /// </summary>
        public bool HasPerk(PerkObject perk)
        {
            return PerkEditor.HasPerk(TargetHero, perk);
        }

        #endregion

        #region Unspent Points

        /// MARK: SetUnspentFocusPoints
        /// <summary>
        /// Directly sets the number of unspent focus points for the target hero.
        /// </summary>
        /// <param name="value">The number of unspent focus points to set</param>
        /// <returns>BLGMResult indicating success or failure</returns>
        public BLGMResult SetUnspentFocusPoints(int value)
        {
            if (TargetHero == null)
            {
                return BLGMResult.Error("SetUnspentFocusPoints() failed, TargetHero is null",
                    new InvalidOperationException("TargetHero is null")).Log();
            }

            int previousValue = TargetHero.HeroDeveloper.UnspentFocusPoints;
            TargetHero.HeroDeveloper.UnspentFocusPoints = value;

            return BLGMResult.Success(
                $"Set {TargetHero.Name}'s unspent focus points from {previousValue} to {value}");
        }

        /// MARK: SetUnspentAttributePoints
        /// <summary>
        /// Directly sets the number of unspent attribute points for the target hero.
        /// </summary>
        /// <param name="value">The number of unspent attribute points to set</param>
        /// <returns>BLGMResult indicating success or failure</returns>
        public BLGMResult SetUnspentAttributePoints(int value)
        {
            if (TargetHero == null)
            {
                return BLGMResult.Error("SetUnspentAttributePoints() failed, TargetHero is null",
                    new InvalidOperationException("TargetHero is null")).Log();
            }

            int previousValue = TargetHero.HeroDeveloper.UnspentAttributePoints;
            TargetHero.HeroDeveloper.UnspentAttributePoints = value;

            return BLGMResult.Success(
                $"Set {TargetHero.Name}'s unspent attribute points from {previousValue} to {value}");
        }

        #endregion

        #region Reset Operations

        /// MARK: ResetAll
        /// <summary>
        /// Restores the target hero to the state captured when this editor was created.
        /// </summary>
        /// <returns>BLGMResult indicating success or failure</returns>
        public BLGMResult ResetAll()
        {
            if (OriginalSnapshot == null)
            {
                return BLGMResult.Error("ResetAll() failed, no original snapshot available",
                    new InvalidOperationException("OriginalSnapshot is null")).Log();
            }

            return OriginalSnapshot.RestoreTo(TargetHero);
        }

        /// MARK: ResetSkills
        /// <summary>
        /// Restores only skill levels from the original snapshot.
        /// </summary>
        /// <returns>BLGMResult indicating success or failure</returns>
        public BLGMResult ResetSkills()
        {
            if (TargetHero == null)
            {
                return BLGMResult.Error("ResetSkills() failed, TargetHero is null",
                    new InvalidOperationException("TargetHero is null")).Log();
            }

            if (OriginalSnapshot == null)
            {
                return BLGMResult.Error("ResetSkills() failed, no original snapshot available",
                    new InvalidOperationException("OriginalSnapshot is null")).Log();
            }

            foreach (KeyValuePair<SkillObject, int> kvp in OriginalSnapshot.SkillLevels)
            {
                int currentLevel = TargetHero.GetSkillValue(kvp.Key);

                if (currentLevel != kvp.Value)
                {
                    TargetHero.HeroDeveloper.SetInitialSkillLevel(kvp.Key, kvp.Value);
                }
            }

            return BLGMResult.Success($"Reset all skill levels for {TargetHero.Name} to original values");
        }

        /// MARK: ResetAttributes
        /// <summary>
        /// Restores only attribute values from the original snapshot.
        /// </summary>
        /// <returns>BLGMResult indicating success or failure</returns>
        public BLGMResult ResetAttributes()
        {
            if (TargetHero == null)
            {
                return BLGMResult.Error("ResetAttributes() failed, TargetHero is null",
                    new InvalidOperationException("TargetHero is null")).Log();
            }

            if (OriginalSnapshot == null)
            {
                return BLGMResult.Error("ResetAttributes() failed, no original snapshot available",
                    new InvalidOperationException("OriginalSnapshot is null")).Log();
            }

            foreach (KeyValuePair<CharacterAttribute, int> kvp in OriginalSnapshot.AttributeValues)
            {
                int currentValue = TargetHero.GetAttributeValue(kvp.Key);
                int delta = kvp.Value - currentValue;

                if (delta > 0)
                {
                    TargetHero.HeroDeveloper.AddAttribute(kvp.Key, delta, false);
                }

                else if (delta < 0)
                {
                    TargetHero.HeroDeveloper.RemoveAttribute(kvp.Key, -delta);
                }
            }

            return BLGMResult.Success($"Reset all attributes for {TargetHero.Name} to original values");
        }

        /// MARK: ResetFocus
        /// <summary>
        /// Restores only focus values from the original snapshot.
        /// </summary>
        /// <returns>BLGMResult indicating success or failure</returns>
        public BLGMResult ResetFocus()
        {
            if (TargetHero == null)
            {
                return BLGMResult.Error("ResetFocus() failed, TargetHero is null",
                    new InvalidOperationException("TargetHero is null")).Log();
            }

            if (OriginalSnapshot == null)
            {
                return BLGMResult.Error("ResetFocus() failed, no original snapshot available",
                    new InvalidOperationException("OriginalSnapshot is null")).Log();
            }

            foreach (KeyValuePair<SkillObject, int> kvp in OriginalSnapshot.FocusValues)
            {
                int currentFocus = TargetHero.HeroDeveloper.GetFocus(kvp.Key);
                int delta = kvp.Value - currentFocus;

                if (delta > 0)
                {
                    TargetHero.HeroDeveloper.AddFocus(kvp.Key, delta, false);
                }

                else if (delta < 0)
                {
                    TargetHero.HeroDeveloper.RemoveFocus(kvp.Key, -delta);
                }
            }

            return BLGMResult.Success($"Reset all focus values for {TargetHero.Name} to original values");
        }

        /// MARK: ResetPerks
        /// <summary>
        /// Restores only perk selections from the original snapshot.
        /// Clears all perks first (with permanent bonus handling), then re-selects original perks.
        /// </summary>
        /// <returns>BLGMResult indicating success or failure</returns>
        public BLGMResult ResetPerks()
        {
            if (TargetHero == null)
            {
                return BLGMResult.Error("ResetPerks() failed, TargetHero is null",
                    new InvalidOperationException("TargetHero is null")).Log();
            }

            if (OriginalSnapshot == null)
            {
                return BLGMResult.Error("ResetPerks() failed, no original snapshot available",
                    new InvalidOperationException("OriginalSnapshot is null")).Log();
            }

            // Clear all current perks with permanent bonus handling
            PerkEditor.ClearAllPerks(TargetHero);

            // Re-select original perks
            foreach (KeyValuePair<PerkObject, bool> kvp in OriginalSnapshot.PerkSelections)
            {
                if (kvp.Value)
                {
                    TargetHero.HeroDeveloper.AddPerk(kvp.Key);
                }
            }

            return BLGMResult.Success($"Reset all perks for {TargetHero.Name} to original values");
        }

        #endregion

        #region Level Operations

        /// MARK: RecalculateLevel
        /// <summary>
        /// Recalculates the hero's level from current skill XP totals.
        /// Uses the native formula: TotalXp = sum(2 * skillLevel^2.2) - 2000, then checks level from XP.
        /// Calls SetInitialLevelFromSkills pattern followed by CheckLevel.
        /// </summary>
        /// <returns>BLGMResult indicating success or failure</returns>
        public BLGMResult RecalculateLevel()
        {
            if (TargetHero == null)
            {
                return BLGMResult.Error("RecalculateLevel() failed, TargetHero is null",
                    new InvalidOperationException("TargetHero is null")).Log();
            }

            int previousLevel = TargetHero.Level;

            // Recalculate total XP from skill levels using native formula
            float totalXp = 0f;

            foreach (SkillObject skill in Skills.All)
            {
                float skillLevel = (float)TargetHero.GetSkillValue(skill);
                totalXp += 2f * TaleWorlds.Library.MathF.Pow(skillLevel, 2.2f);
            }

            int calculatedTotalXp = TaleWorlds.Library.MathF.Max(1, (int)totalXp - 2000);

            // Reset level and recalculate from XP
            TargetHero.Level = 0;
            TargetHero.HeroDeveloper.SetInitialLevel(1);

            // Use native CheckLevel by setting XP via SetInitialLevel approach
            // SetInitialLevel sets TotalXp and calls CheckLevel
            HeroDeveloper developer = TargetHero.HeroDeveloper;

            // Walk the level calculation manually to determine correct level
            int level = 0;
            bool done = false;

            while (!done)
            {
                int xpRequired = developer.GetXpRequiredForLevel(level + 1);
                int maxSkillPoint = Campaign.Current.Models.CharacterDevelopmentModel.GetMaxSkillPoint();

                if (xpRequired != maxSkillPoint && calculatedTotalXp >= xpRequired)
                {
                    level++;
                }

                else
                {
                    done = true;
                }
            }

            TargetHero.Level = level;

            return BLGMResult.Success(
                $"Recalculated {TargetHero.Name}'s level from {previousLevel} to {level} (TotalXP: {calculatedTotalXp})");
        }

        #endregion

        #region Summary

        /// MARK: GetSummary
        /// <summary>
        /// Returns a formatted string containing the current character development state of the target hero.
        /// Includes all skills, attributes, focus, unspent points, and level.
        /// </summary>
        /// <returns>Formatted summary string</returns>
        public string GetSummary()
        {
            if (TargetHero == null)
            {
                return "No target hero set";
            }

            HeroDeveloper developer = TargetHero.HeroDeveloper;
            StringBuilder sb = new();

            sb.AppendLine($"=== Character Development: {TargetHero.Name} ===");
            sb.AppendLine($"Level: {TargetHero.Level} | Total XP: {developer.TotalXp}");
            sb.AppendLine($"Unspent Attribute Points: {developer.UnspentAttributePoints}");
            sb.AppendLine($"Unspent Focus Points: {developer.UnspentFocusPoints}");

            // Attributes
            sb.AppendLine("\n--- Attributes ---");

            foreach (CharacterAttribute attribute in Attributes.All)
            {
                sb.AppendLine($"  {attribute.Name}: {TargetHero.GetAttributeValue(attribute)}");
            }

            // Skills grouped by attribute
            sb.AppendLine("\n--- Skills ---");

            foreach (SkillObject skill in Skills.All)
            {
                SkillInfo info = SkillEditor.GetSkillInfo(TargetHero, skill);
                sb.AppendLine($"  {info}");
            }

            // Perk count summary
            int totalPerks = 0;
            int selectedPerks = 0;

            foreach (PerkObject perk in PerkObject.All)
            {
                totalPerks++;

                if (TargetHero.GetPerkValue(perk))
                {
                    selectedPerks++;
                }
            }

            sb.AppendLine($"\n--- Perks ---");
            sb.AppendLine($"  Selected: {selectedPerks} / {totalPerks}");

            return sb.ToString();
        }

        #endregion
    }
}
