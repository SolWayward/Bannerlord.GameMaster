using System.Collections.Generic;
using Bannerlord.GameMaster.Common;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.CharacterDevelopment;
using TaleWorlds.CampaignSystem.Extensions;
using TaleWorlds.Core;

namespace Bannerlord.GameMaster.Heroes.HeroDevelopment
{
    /// <summary>
    /// Captures the full character development state of a hero at a point in time
    /// so it can be restored on cancel/reset. Analogous to how native SkillVM tracks
    /// original focus, but captures everything: skills, XP, focus, attributes, perks,
    /// unspent points, level, and total XP.
    /// </summary>
    public class HeroSkillSnapshot
    {
        public Dictionary<SkillObject, int> SkillLevels { get; private set; }
        public Dictionary<SkillObject, float> SkillXps { get; private set; }
        public Dictionary<SkillObject, int> FocusValues { get; private set; }
        public Dictionary<CharacterAttribute, int> AttributeValues { get; private set; }
        public Dictionary<PerkObject, bool> PerkSelections { get; private set; }
        public int UnspentFocusPoints { get; private set; }
        public int UnspentAttributePoints { get; private set; }
        public int TotalXp { get; private set; }
        public int Level { get; private set; }

        private HeroSkillSnapshot()
        {
            SkillLevels = new Dictionary<SkillObject, int>();
            SkillXps = new Dictionary<SkillObject, float>();
            FocusValues = new Dictionary<SkillObject, int>();
            AttributeValues = new Dictionary<CharacterAttribute, int>();
            PerkSelections = new Dictionary<PerkObject, bool>();
        }

        /// MARK: Capture
        /// <summary>
        /// Captures the full character development state of the specified hero.
        /// Iterates Skills.All, Attributes.All, and PerkObject.All to record current values.
        /// </summary>
        /// <param name="hero">The hero whose state to capture</param>
        /// <returns>A new snapshot containing the hero's current development state, or null if hero is null</returns>
        public static HeroSkillSnapshot Capture(Hero hero)
        {
            if (hero == null)
            {
                BLGMResult.Error("HeroSkillSnapshot.Capture() failed, hero cannot be null",
                    new System.ArgumentNullException(nameof(hero))).Log();
                return null;
            }

            HeroSkillSnapshot snapshot = new();
            HeroDeveloper developer = hero.HeroDeveloper;

            // Capture skill levels and XP
            foreach (SkillObject skill in Skills.All)
            {
                snapshot.SkillLevels[skill] = hero.GetSkillValue(skill);
                snapshot.SkillXps[skill] = developer.GetSkillXp(skill);
                snapshot.FocusValues[skill] = developer.GetFocus(skill);
            }

            // Capture attribute values
            foreach (CharacterAttribute attribute in Attributes.All)
            {
                snapshot.AttributeValues[attribute] = hero.GetAttributeValue(attribute);
            }

            // Capture perk selections
            foreach (PerkObject perk in PerkObject.All)
            {
                snapshot.PerkSelections[perk] = hero.GetPerkValue(perk);
            }

            // Capture unspent points and level
            snapshot.UnspentFocusPoints = developer.UnspentFocusPoints;
            snapshot.UnspentAttributePoints = developer.UnspentAttributePoints;
            snapshot.TotalXp = developer.TotalXp;
            snapshot.Level = hero.Level;

            return snapshot;
        }

        /// MARK: RestoreTo
        /// <summary>
        /// Restores the captured state back to the specified hero.
        /// Uses SetInitialSkillLevel for skills, direct attribute/focus APIs, and perk toggling.
        /// </summary>
        /// <param name="hero">The hero to restore state to</param>
        /// <returns>BLGMResult indicating success or failure</returns>
        public BLGMResult RestoreTo(Hero hero)
        {
            if (hero == null)
            {
                return BLGMResult.Error("HeroSkillSnapshot.RestoreTo() failed, hero cannot be null",
                    new System.ArgumentNullException(nameof(hero))).Log();
            }

            HeroDeveloper developer = hero.HeroDeveloper;

            // Restore attributes first (some perks grant permanent attribute bonuses)
            foreach (KeyValuePair<CharacterAttribute, int> kvp in AttributeValues)
            {
                int currentValue = hero.GetAttributeValue(kvp.Key);
                int delta = kvp.Value - currentValue;

                if (delta > 0)
                {
                    developer.AddAttribute(kvp.Key, delta, false);
                }

                else if (delta < 0)
                {
                    developer.RemoveAttribute(kvp.Key, -delta);
                }
            }

            // Restore focus values
            foreach (KeyValuePair<SkillObject, int> kvp in FocusValues)
            {
                int currentFocus = developer.GetFocus(kvp.Key);
                int delta = kvp.Value - currentFocus;

                if (delta > 0)
                {
                    developer.AddFocus(kvp.Key, delta, false);
                }

                else if (delta < 0)
                {
                    developer.RemoveFocus(kvp.Key, -delta);
                }
            }

            // Restore skill levels (uses SetInitialSkillLevel which handles XP correctly)
            foreach (KeyValuePair<SkillObject, int> kvp in SkillLevels)
            {
                int currentLevel = hero.GetSkillValue(kvp.Key);

                if (currentLevel != kvp.Value)
                {
                    developer.SetInitialSkillLevel(kvp.Key, kvp.Value);
                }
            }

            // Restore perks - clear all first then re-select captured ones
            hero.ClearPerks();

            foreach (KeyValuePair<PerkObject, bool> kvp in PerkSelections)
            {
                if (kvp.Value)
                {
                    developer.AddPerk(kvp.Key);
                }
            }

            // Restore unspent points
            developer.UnspentFocusPoints = UnspentFocusPoints;
            developer.UnspentAttributePoints = UnspentAttributePoints;

            // Restore level
            hero.Level = Level;

            return BLGMResult.Success($"Restored development snapshot for {hero.Name}");
        }
    }
}
