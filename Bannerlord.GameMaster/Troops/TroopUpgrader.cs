using System;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Roster;
using TaleWorlds.Core;

namespace Bannerlord.GameMaster.Troops
{
    public class TroopUpgrader
    {
        // MARK: UpgradeTroops
        /// <summary>
        /// Upgrades all troops in the roster to the specified tier while maintaining desired composition ratios.
        /// When troops have multiple upgrade paths, intelligently splits them to achieve target ratios.
        /// Pre-analyzes all upgrade paths to account for troops with limited options.
        /// </summary>
        /// <param name="memberRoster">The troop roster to upgrade</param>
        /// <param name="targetTier">Maximum tier to upgrade to (default: max tier)</param>
        /// <param name="targetRangedRatio">Desired ratio of ranged troops (0.0-1.0, null for auto)</param>
        /// <param name="targetCavalryRatio">Desired ratio of cavalry troops (0.0-1.0, null for auto)</param>
        /// <param name="targetInfantryRatio">Desired ratio of infantry troops (0.0-1.0, null for auto)</param>
        public static void UpgradeTroops(TroopRoster memberRoster,
            int targetTier = 7,
            float? targetRangedRatio = null,
            float? targetCavalryRatio = null,
            float? targetInfantryRatio = null)
        {
            // Normalize and validate ratios
            var (rangedRatio, cavalryRatio, infantryRatio) = NormalizeRatios(
                targetRangedRatio, targetCavalryRatio, targetInfantryRatio);
    
            // Dictionary to accumulate all roster changes
            Dictionary<CharacterObject, int> rosterChanges = new Dictionary<CharacterObject, int>();
    
            // Pre-analyze all troops to understand final upgrade possibilities
            var troopAnalysis = AnalyzeUpgradePaths(memberRoster, targetTier);
            
            // Calculate adjusted ratios to compensate for locked-in troops
            var adjustedRatios = CalculateAdjustedRatios(
                troopAnalysis, rangedRatio, cavalryRatio, infantryRatio);
    
            // Process each troop type in the roster
            foreach (var analysis in troopAnalysis)
            {
                if (analysis.troop.Character.IsHero)
                    continue;
    
                ProcessTroopUpgradeWithAnalysis(analysis, targetTier, rosterChanges, adjustedRatios);
            }
    
            // Apply all roster changes
            foreach (var change in rosterChanges)
            {
                memberRoster.AddToCounts(change.Key, change.Value);
            }
        }

        // MARK: NormalizeRatios
        /// <summary>
        /// Normalize ratios of troop types
        /// </summary>
        public static (float ranged, float cavalry, float infantry) NormalizeRatios(
            float? rangedRatio, float? cavalryRatio, float? infantryRatio)
        {
            // Count how many ratios were specified
            int specifiedCount = (rangedRatio.HasValue ? 1 : 0) +
                                 (cavalryRatio.HasValue ? 1 : 0) +
                                 (infantryRatio.HasValue ? 1 : 0);

            // Case 1: No ratios specified - use defaults
            if (specifiedCount == 0)
            {
                return (0.30f, 0.20f, 0.50f); // 30% ranged, 20% cavalry, 50% infantry
            }

            // Case 2: All three specified - validate they sum to 1.0
            if (specifiedCount == 3)
            {
                float sum = rangedRatio.Value + cavalryRatio.Value + infantryRatio.Value;
                if (Math.Abs(sum - 1.0f) > 0.01f) // Allow small floating point errors
                {
                    // Normalize to sum to 1.0
                    return (rangedRatio.Value / sum, cavalryRatio.Value / sum, infantryRatio.Value / sum);
                }
                return (rangedRatio.Value, cavalryRatio.Value, infantryRatio.Value);
            }

            // Case 3: Partial specification - calculate unspecified from remaining
            float specifiedSum = (rangedRatio ?? 0) + (cavalryRatio ?? 0) + (infantryRatio ?? 0);

            if (specifiedSum >= 1.0f)
            {
                // Specified ratios already exceed or equal 100% - normalize them
                float sum = specifiedSum;
                return (
                    (rangedRatio ?? 0) / sum,
                    (cavalryRatio ?? 0) / sum,
                    (infantryRatio ?? 0) / sum
                );
            }

            float remaining = 1.0f - specifiedSum;
            int unspecifiedCount = 3 - specifiedCount;
            float autoRatio = remaining / unspecifiedCount;

            return (
                rangedRatio ?? autoRatio,
                cavalryRatio ?? autoRatio,
                infantryRatio ?? autoRatio
            );
        }

        // MARK: AnalyzeUpgradePaths
        /// <summary>
        /// Pre-analyzes all troops to understand what final upgrade options are available
        /// </summary>
        private static List<TroopUpgradeAnalysis> AnalyzeUpgradePaths(TroopRoster roster, int targetTier)
        {
            var results = new List<TroopUpgradeAnalysis>();
            
            foreach (TroopRosterElement troop in roster.GetTroopRoster())
            {
                if (troop.Character.IsHero)
                    continue;
                    
                var finalOptions = GetFinalUpgradeOptions(troop.Character, targetTier);
                results.Add(new TroopUpgradeAnalysis
                {
                    troop = troop,
                    finalOptions = finalOptions,
                    isFlexible = finalOptions.Count > 1
                });
            }
            
            return results;
        }
        
        // MARK: GetFinalUpgradeOptions
        /// <summary>
        /// Recursively finds all possible final FormationClass options for a troop
        /// </summary>
        private static HashSet<FormationClass> GetFinalUpgradeOptions(CharacterObject troop, int targetTier)
        {
            var options = new HashSet<FormationClass>();
            
            if (troop.Tier >= targetTier || troop.UpgradeTargets.Length == 0)
            {
                // This is a final state
                options.Add(troop.DefaultFormationClass);
                return options;
            }
            
            // Recursively check all upgrade paths
            foreach (var upgrade in troop.UpgradeTargets)
            {
                var upgradeOptions = GetFinalUpgradeOptions(upgrade, targetTier);
                foreach (var option in upgradeOptions)
                {
                    options.Add(option);
                }
            }
            
            return options;
        }
        
        // MARK: CalculateAdjustedRatios
        /// <summary>
        /// Adjusts target ratios to account for troops with limited upgrade options
        /// </summary>
        private static AdjustedRatios CalculateAdjustedRatios(
            List<TroopUpgradeAnalysis> analyses,
            float targetRangedRatio,
            float targetCavalryRatio,
            float targetInfantryRatio)
        {
            int totalTroops = 0;
            int lockedRanged = 0, lockedCavalry = 0, lockedInfantry = 0;
            int flexibleTroops = 0;
            
            foreach (var analysis in analyses)
            {
                int count = analysis.troop.Number;
                totalTroops += count;
                
                if (!analysis.isFlexible)
                {
                    // Only one option - this is locked in
                    var finalClass = analysis.finalOptions.First();
                    switch (finalClass)
                    {
                        case FormationClass.Ranged:
                            lockedRanged += count;
                            break;
                        case FormationClass.Cavalry:
                        case FormationClass.HorseArcher:
                            lockedCavalry += count;
                            break;
                        case FormationClass.Infantry:
                            lockedInfantry += count;
                            break;
                    }
                }
                else
                {
                    flexibleTroops += count;
                }
            }
            
            // Calculate target counts
            int targetRanged = (int)(totalTroops * targetRangedRatio);
            int targetCavalry = (int)(totalTroops * targetCavalryRatio);
            int targetInfantry = (int)(totalTroops * targetInfantryRatio);
            
            // Calculate deficits after accounting for locked troops
            int rangedDeficit = targetRanged - lockedRanged;
            int cavalryDeficit = targetCavalry - lockedCavalry;
            int infantryDeficit = targetInfantry - lockedInfantry;
            
            // If we have flexible troops, calculate adjusted ratios for them
            float adjustedRanged = 0.33f, adjustedCavalry = 0.33f, adjustedInfantry = 0.34f;
            
            if (flexibleTroops > 0)
            {
                // Ensure deficits are non-negative (can't remove locked troops)
                rangedDeficit = Math.Max(0, rangedDeficit);
                cavalryDeficit = Math.Max(0, cavalryDeficit);
                infantryDeficit = Math.Max(0, infantryDeficit);
                
                int totalDeficit = rangedDeficit + cavalryDeficit + infantryDeficit;
                
                if (totalDeficit > 0 && totalDeficit <= flexibleTroops)
                {
                    // We can achieve the target - allocate proportionally
                    adjustedRanged = (float)rangedDeficit / flexibleTroops;
                    adjustedCavalry = (float)cavalryDeficit / flexibleTroops;
                    adjustedInfantry = (float)infantryDeficit / flexibleTroops;
                }
                else if (totalDeficit > flexibleTroops)
                {
                    // Not enough flexible troops - scale down proportionally
                    adjustedRanged = (float)rangedDeficit / totalDeficit;
                    adjustedCavalry = (float)cavalryDeficit / totalDeficit;
                    adjustedInfantry = (float)infantryDeficit / totalDeficit;
                }
                else
                {
                    // No deficit needed, distribute evenly
                    adjustedRanged = adjustedCavalry = adjustedInfantry = 1f / 3f;
                }
            }
            
            return new AdjustedRatios
            {
                rangedRatio = adjustedRanged,
                cavalryRatio = adjustedCavalry,
                infantryRatio = adjustedInfantry
            };
        }
        
        // MARK: ProcessTroopUpgradeWithAnalysis
        /// <summary>
        /// Upgrades troops using pre-computed analysis and adjusted ratios
        /// </summary>
        private static void ProcessTroopUpgradeWithAnalysis(
            TroopUpgradeAnalysis analysis,
            int targetTier,
            Dictionary<CharacterObject, int> rosterChanges,
            AdjustedRatios adjustedRatios)
        {
            // Use a queue to process troops that need further upgrading after splits
            Queue<(CharacterObject troop, int count)> upgradeQueue = new Queue<(CharacterObject, int)>();
            upgradeQueue.Enqueue((analysis.troop.Character, analysis.troop.Number));
    
            while (upgradeQueue.Count > 0)
            {
                var (currentTroop, remainingCount) = upgradeQueue.Dequeue();
    
                // Upgrade until target tier or no more upgrades available
                while (currentTroop.Tier < targetTier && currentTroop.UpgradeTargets.Length > 0)
                {
                    if (currentTroop.UpgradeTargets.Length == 1)
                    {
                        // Single path - upgrade all
                        CharacterObject upgraded = currentTroop.UpgradeTargets[0];
                        RecordRosterChange(rosterChanges, currentTroop, -remainingCount);
                        RecordRosterChange(rosterChanges, upgraded, remainingCount);
                        currentTroop = upgraded;
                    }
                    else
                    {
                        // Multiple paths - split based on adjusted ratios
                        var splits = SplitTroopsWithAdjustedRatios(
                            currentTroop, remainingCount, adjustedRatios, analysis.finalOptions);
    
                        // Remove original troops
                        RecordRosterChange(rosterChanges, currentTroop, -remainingCount);
    
                        // Add upgraded troops and queue for further upgrading
                        foreach (var split in splits)
                        {
                            RecordRosterChange(rosterChanges, split.upgradedTroop, split.count);
                            
                            // Queue split troops for continued upgrading if they haven't reached target tier
                            if (split.upgradedTroop.Tier < targetTier && split.upgradedTroop.UpgradeTargets.Length > 0)
                            {
                                upgradeQueue.Enqueue((split.upgradedTroop, split.count));
                            }
                        }
    
                        // Break from inner loop after split
                        break;
                    }
                }
            }
        }
        
        // MARK: SplitTroopsWithAdjustedRatios
        /// <summary>
        /// Splits troops based on adjusted ratios that compensate for locked-in troops.
        /// Calculates desirability score for each upgrade based on its eventual outcomes.
        /// </summary>
        private static List<(CharacterObject upgradedTroop, int count)> SplitTroopsWithAdjustedRatios(
            CharacterObject baseTroop,
            int totalCount,
            AdjustedRatios ratios,
            HashSet<FormationClass> finalOptions)
        {
            var result = new List<(CharacterObject, int)>();
            int targetTier = 7;

            // Map target ratios for easy lookup
            var targetRatios = new Dictionary<FormationClass, float>
            {
                [FormationClass.Ranged] = ratios.rangedRatio,
                [FormationClass.Cavalry] = ratios.cavalryRatio,
                [FormationClass.HorseArcher] = (ratios.cavalryRatio + ratios.rangedRatio) * 0.5f,
                [FormationClass.Infantry] = ratios.infantryRatio
            };

            // Calculate desirability score for each immediate upgrade option
            var upgradeScores = new Dictionary<CharacterObject, float>();
            
            foreach (var upgrade in baseTroop.UpgradeTargets)
            {
                // Find what formation classes this upgrade can eventually lead to
                var upgradeFinalOptions = GetFinalUpgradeOptions(upgrade, targetTier);
                
                // Calculate score based on how much we need the roles this upgrade can become
                float score = 0f;
                int optionCount = upgradeFinalOptions.Count;
                
                if (optionCount > 0)
                {
                    foreach (var finalClass in upgradeFinalOptions)
                    {
                        // Average the target ratios of all possible outcomes
                        float ratio = targetRatios.ContainsKey(finalClass) ? targetRatios[finalClass] : 0.2f;
                        score += ratio;
                    }
                    // Average across all possible outcomes
                    score /= optionCount;
                }
                else
                {
                    // Fallback: use immediate class
                    score = targetRatios.ContainsKey(upgrade.DefaultFormationClass)
                        ? targetRatios[upgrade.DefaultFormationClass]
                        : 0.2f;
                }
                
                // Ensure minimum score so all upgrades can be chosen
                score = Math.Max(score, 0.05f);
                upgradeScores[upgrade] = score;
            }

            // Calculate total score
            float totalScore = upgradeScores.Values.Sum();
            
            // Distribute troops based on scores
            int remainingTroops = totalCount;
            var upgradeList = upgradeScores.Keys.ToList();

            for (int i = 0; i < upgradeList.Count; i++)
            {
                var upgrade = upgradeList[i];
                float score = upgradeScores[upgrade];

                int troopsForThisUpgrade;
                if (i == upgradeList.Count - 1)
                {
                    // Last upgrade gets all remaining troops
                    troopsForThisUpgrade = remainingTroops;
                }
                else
                {
                    // Allocate proportionally based on score
                    troopsForThisUpgrade = (int)Math.Round(totalCount * (score / totalScore));
                    troopsForThisUpgrade = Math.Max(0, Math.Min(troopsForThisUpgrade, remainingTroops));
                }

                if (troopsForThisUpgrade > 0)
                {
                    result.Add((upgrade, troopsForThisUpgrade));
                    remainingTroops -= troopsForThisUpgrade;
                }
            }

            return result;
        }
    
        // MARK: RecordRosterChange
        /// <summary>
        /// Helper for UpdateTroops()
        /// </summary>
        private static void RecordRosterChange(Dictionary<CharacterObject, int> changes, CharacterObject troop, int count)
        {
            if (changes.ContainsKey(troop))
                changes[troop] += count;
            else
                changes[troop] = count;
        }
        
        // Helper classes for analysis
        private class TroopUpgradeAnalysis
        {
            public TroopRosterElement troop;
            public HashSet<FormationClass> finalOptions;
            public bool isFlexible;
        }
        
        private class AdjustedRatios
        {
            public float rangedRatio;
            public float cavalryRatio;
            public float infantryRatio;
        }
    }
}