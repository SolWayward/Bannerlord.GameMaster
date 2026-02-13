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
    }
}
