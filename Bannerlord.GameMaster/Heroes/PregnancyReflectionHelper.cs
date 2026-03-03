using System;
using System.Collections;
using System.Reflection;
using Bannerlord.GameMaster.Common;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.CampaignBehaviors;

namespace Bannerlord.GameMaster.Heroes
{
    /// <summary>
    /// Provides cached reflection access to PregnancyCampaignBehavior internals
    /// for manipulating pregnancy records (replacing father on existing pregnancies).
    /// </summary>
    public static class PregnancyReflectionHelper
    {
        #region Cached Reflection Fields

        private static readonly FieldInfo HeroPregnanciesField = typeof(PregnancyCampaignBehavior)
            .GetField("_heroPregnancies", BindingFlags.NonPublic | BindingFlags.Instance);

        private static readonly Type PregnancyType = typeof(PregnancyCampaignBehavior)
            .GetNestedType("Pregnancy", BindingFlags.NonPublic | BindingFlags.Public);

        private static readonly ConstructorInfo PregnancyConstructor = PregnancyType?.GetConstructor(
            new[] { typeof(Hero), typeof(Hero), typeof(CampaignTime) });

        private static readonly FieldInfo MotherField = PregnancyType?.GetField("Mother");

        private static readonly FieldInfo FatherField = PregnancyType?.GetField("Father");

        private static readonly FieldInfo DueDateField = PregnancyType?.GetField("DueDate");

        #endregion

        /// MARK: ReplacePregnancyFather
        /// <summary>
        /// Replaces the father on an existing pregnancy record for the given mother.
        /// Must be called after MakePregnantAction.Apply() has created the initial record.
        /// </summary>
        /// <param name="mother">The pregnant hero whose pregnancy record to modify</param>
        /// <param name="father">The hero to set as the father</param>
        /// <returns>BLGMResult indicating success or failure with details</returns>
        public static BLGMResult ReplacePregnancyFather(Hero mother, Hero father)
        {
            try
            {
                if (HeroPregnanciesField == null)
                {
                    return BLGMResult.Error(
                        "ReplacePregnancyFather() failed: _heroPregnancies field not found via reflection. Game version may be incompatible.",
                        new MissingFieldException(nameof(PregnancyCampaignBehavior), "_heroPregnancies")).Log();
                }

                if (PregnancyType == null)
                {
                    return BLGMResult.Error(
                        "ReplacePregnancyFather() failed: Pregnancy nested type not found via reflection. Game version may be incompatible.",
                        new MissingMemberException(nameof(PregnancyCampaignBehavior), "Pregnancy")).Log();
                }

                if (PregnancyConstructor == null)
                {
                    return BLGMResult.Error(
                        "ReplacePregnancyFather() failed: Pregnancy constructor not found via reflection. Game version may be incompatible.",
                        new MissingMethodException("PregnancyCampaignBehavior.Pregnancy", ".ctor(Hero, Hero, CampaignTime)")).Log();
                }

                if (MotherField == null || DueDateField == null)
                {
                    return BLGMResult.Error(
                        "ReplacePregnancyFather() failed: Mother or DueDate field not found on Pregnancy type. Game version may be incompatible.",
                        new MissingFieldException("PregnancyCampaignBehavior.Pregnancy", "Mother/DueDate")).Log();
                }

                PregnancyCampaignBehavior behavior = Campaign.Current.GetCampaignBehavior<PregnancyCampaignBehavior>();
                if (behavior == null)
                {
                    return BLGMResult.Error(
                        "ReplacePregnancyFather() failed: PregnancyCampaignBehavior not found in current campaign.",
                        new InvalidOperationException("PregnancyCampaignBehavior not found")).Log();
                }

                IList pregnancyList = HeroPregnanciesField.GetValue(behavior) as IList;
                if (pregnancyList == null)
                {
                    return BLGMResult.Error(
                        "ReplacePregnancyFather() failed: _heroPregnancies list is null or could not be cast to IList.",
                        new InvalidOperationException("_heroPregnancies is null or not IList")).Log();
                }

                // Find the pregnancy record for this mother
                int foundIndex = -1;
                object foundPregnancy = null;

                for (int i = 0; i < pregnancyList.Count; i++)
                {
                    object pregnancy = pregnancyList[i];
                    Hero pregnancyMother = MotherField.GetValue(pregnancy) as Hero;

                    if (pregnancyMother == mother)
                    {
                        foundIndex = i;
                        foundPregnancy = pregnancy;
                        break;
                    }
                }

                if (foundIndex < 0 || foundPregnancy == null)
                {
                    return BLGMResult.Error(
                        $"ReplacePregnancyFather() failed: No pregnancy record found for {mother.Name}. MakePregnantAction.Apply() may not have created one.",
                        new InvalidOperationException($"No pregnancy record for {mother.Name}")).Log();
                }

                // Read the DueDate from the existing pregnancy to preserve it
                CampaignTime dueDate = (CampaignTime)DueDateField.GetValue(foundPregnancy);

                // Remove the old pregnancy
                pregnancyList.RemoveAt(foundIndex);

                // Create a new pregnancy with the correct father
                object newPregnancy = PregnancyConstructor.Invoke(new object[] { mother, father, dueDate });
                pregnancyList.Add(newPregnancy);

                return BLGMResult.Success($"Replaced pregnancy father for {mother.Name} with {father.Name}");
            }
            catch (Exception ex)
            {
                return BLGMResult.Error(
                    $"ReplacePregnancyFather() failed with unexpected exception for {mother?.Name}: {ex.Message}", ex).Log();
            }
        }

        /// MARK: GetPregnancyFather
        /// <summary>
        /// Reads the father from an existing pregnancy record for a given mother.
        /// </summary>
        /// <param name="mother">The pregnant hero whose pregnancy father to retrieve</param>
        /// <returns>The father Hero, or null if no record found or reflection fails</returns>
        public static Hero GetPregnancyFather(Hero mother)
        {
            try
            {
                if (mother == null)
                {
                    BLGMResult.Error("GetPregnancyFather() failed, mother cannot be null",
                        new ArgumentNullException(nameof(mother))).Log();
                    return null;
                }

                if (HeroPregnanciesField == null || MotherField == null || FatherField == null)
                {
                    BLGMResult.Error(
                        "GetPregnancyFather() failed: Required reflection fields not found. Game version may be incompatible.",
                        new MissingFieldException(nameof(PregnancyCampaignBehavior), "_heroPregnancies/Mother/Father")).Log();
                    return null;
                }

                PregnancyCampaignBehavior behavior = Campaign.Current.GetCampaignBehavior<PregnancyCampaignBehavior>();
                if (behavior == null)
                {
                    BLGMResult.Error(
                        "GetPregnancyFather() failed: PregnancyCampaignBehavior not found in current campaign.",
                        new InvalidOperationException("PregnancyCampaignBehavior not found")).Log();
                    return null;
                }

                IList pregnancyList = HeroPregnanciesField.GetValue(behavior) as IList;
                if (pregnancyList == null)
                {
                    BLGMResult.Error(
                        "GetPregnancyFather() failed: _heroPregnancies list is null or could not be cast to IList.",
                        new InvalidOperationException("_heroPregnancies is null or not IList")).Log();
                    return null;
                }

                // Find the pregnancy record for this mother
                for (int i = 0; i < pregnancyList.Count; i++)
                {
                    object pregnancy = pregnancyList[i];
                    Hero pregnancyMother = MotherField.GetValue(pregnancy) as Hero;

                    if (pregnancyMother == mother)
                    {
                        Hero father = FatherField.GetValue(pregnancy) as Hero;
                        return father;
                    }
                }

                BLGMResult.Error(
                    $"GetPregnancyFather() failed: No pregnancy record found for {mother.Name}.",
                    new InvalidOperationException($"No pregnancy record for {mother.Name}")).Log();
                return null;
            }
            catch (Exception ex)
            {
                BLGMResult.Error(
                    $"GetPregnancyFather() failed with unexpected exception for {mother?.Name}: {ex.Message}", ex).Log();
                return null;
            }
        }

        /// MARK: RemovePregnancyRecord
        /// <summary>
        /// Removes the pregnancy record for a given mother from the _heroPregnancies list.
        /// </summary>
        /// <param name="mother">The hero whose pregnancy record to remove</param>
        /// <returns>BLGMResult indicating success or failure with details</returns>
        public static BLGMResult RemovePregnancyRecord(Hero mother)
        {
            try
            {
                if (mother == null)
                {
                    return BLGMResult.Error("RemovePregnancyRecord() failed, mother cannot be null",
                        new ArgumentNullException(nameof(mother))).Log();
                }

                if (HeroPregnanciesField == null || MotherField == null)
                {
                    return BLGMResult.Error(
                        "RemovePregnancyRecord() failed: Required reflection fields not found. Game version may be incompatible.",
                        new MissingFieldException(nameof(PregnancyCampaignBehavior), "_heroPregnancies/Mother")).Log();
                }

                PregnancyCampaignBehavior behavior = Campaign.Current.GetCampaignBehavior<PregnancyCampaignBehavior>();
                if (behavior == null)
                {
                    return BLGMResult.Error(
                        "RemovePregnancyRecord() failed: PregnancyCampaignBehavior not found in current campaign.",
                        new InvalidOperationException("PregnancyCampaignBehavior not found")).Log();
                }

                IList pregnancyList = HeroPregnanciesField.GetValue(behavior) as IList;
                if (pregnancyList == null)
                {
                    return BLGMResult.Error(
                        "RemovePregnancyRecord() failed: _heroPregnancies list is null or could not be cast to IList.",
                        new InvalidOperationException("_heroPregnancies is null or not IList")).Log();
                }

                // Find the pregnancy record for this mother
                int foundIndex = -1;

                for (int i = 0; i < pregnancyList.Count; i++)
                {
                    object pregnancy = pregnancyList[i];
                    Hero pregnancyMother = MotherField.GetValue(pregnancy) as Hero;

                    if (pregnancyMother == mother)
                    {
                        foundIndex = i;
                        break;
                    }
                }

                if (foundIndex < 0)
                {
                    return BLGMResult.Success($"No pregnancy record found for {mother.Name} (already cleaned up or never created)");
                }

                pregnancyList.RemoveAt(foundIndex);
                return BLGMResult.Success($"Removed pregnancy record for {mother.Name}");
            }
            catch (Exception ex)
            {
                return BLGMResult.Error(
                    $"RemovePregnancyRecord() failed with unexpected exception for {mother?.Name}: {ex.Message}", ex).Log();
            }
        }

        /// MARK: AddPregnancyRecord
        /// <summary>
        /// Creates a new pregnancy record and adds it to _heroPregnancies via reflection.
        /// Used for clanless mothers where MakePregnantAction.Apply() is bypassed to avoid
        /// NRE in PregnancyLogEntry.IsVisibleNotification (mother.Clan.Equals() crashes when Clan is null).
        /// </summary>
        /// <param name="mother">The hero to create a pregnancy record for</param>
        /// <param name="father">The hero to set as the father</param>
        /// <returns>BLGMResult indicating success or failure with details</returns>
        public static BLGMResult AddPregnancyRecord(Hero mother, Hero father)
        {
            try
            {
                if (HeroPregnanciesField == null)
                {
                    return BLGMResult.Error(
                        "AddPregnancyRecord() failed: _heroPregnancies field not found via reflection. Game version may be incompatible.",
                        new MissingFieldException(nameof(PregnancyCampaignBehavior), "_heroPregnancies")).Log();
                }

                if (PregnancyType == null)
                {
                    return BLGMResult.Error(
                        "AddPregnancyRecord() failed: Pregnancy nested type not found via reflection. Game version may be incompatible.",
                        new MissingMemberException(nameof(PregnancyCampaignBehavior), "Pregnancy")).Log();
                }

                if (PregnancyConstructor == null)
                {
                    return BLGMResult.Error(
                        "AddPregnancyRecord() failed: Pregnancy constructor not found via reflection. Game version may be incompatible.",
                        new MissingMethodException("PregnancyCampaignBehavior.Pregnancy", ".ctor(Hero, Hero, CampaignTime)")).Log();
                }

                PregnancyCampaignBehavior behavior = Campaign.Current.GetCampaignBehavior<PregnancyCampaignBehavior>();
                if (behavior == null)
                {
                    return BLGMResult.Error(
                        "AddPregnancyRecord() failed: PregnancyCampaignBehavior not found in current campaign.",
                        new InvalidOperationException("PregnancyCampaignBehavior not found")).Log();
                }

                IList pregnancyList = HeroPregnanciesField.GetValue(behavior) as IList;
                if (pregnancyList == null)
                {
                    return BLGMResult.Error(
                        "AddPregnancyRecord() failed: _heroPregnancies list is null or could not be cast to IList.",
                        new InvalidOperationException("_heroPregnancies is null or not IList")).Log();
                }

                // Calculate due date the same way native ChildConceived() does
                CampaignTime dueDate = CampaignTime.DaysFromNow(Campaign.Current.Models.PregnancyModel.PregnancyDurationInDays);

                // Create new Pregnancy record with the correct father from the start
                object newPregnancy = PregnancyConstructor.Invoke(new object[] { mother, father, dueDate });
                pregnancyList.Add(newPregnancy);

                return BLGMResult.Success($"Added pregnancy record for {mother.Name} with father {father.Name}");
            }
            catch (Exception ex)
            {
                return BLGMResult.Error(
                    $"AddPregnancyRecord() failed with unexpected exception for {mother?.Name}: {ex.Message}", ex).Log();
            }
        }
    }
}
