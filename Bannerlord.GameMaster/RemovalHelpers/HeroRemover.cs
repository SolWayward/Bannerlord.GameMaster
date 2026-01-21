using System.Text;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using Bannerlord.GameMaster.Common;

namespace Bannerlord.GameMaster.RemovalHelpers;

/// <summary>
/// Handles removal of BLGM-generated heroes from the game.
/// </summary>
public static class HeroRemover
{
    /// <summary>
    /// Removes a single hero if it meets the criteria for removal.
    /// </summary>
    /// <param name="hero">The hero to remove.</param>
    /// <returns>BLGMResult indicating success or failure with details.</returns>
    public static BLGMResult RemoveSingleHero(Hero hero)
    {
        if (hero == null)
        {
            return new(false, "Hero is null");
        }

        if (!hero.StringId.StartsWith("blgm_"))
        {
            return new(false, $"Hero {hero.Name} is not a BLGM-generated hero");
        }

        // Skip if hero is clan leader
        if (hero.Clan?.Leader == hero)
        {
            return new(false, $"Hero {hero.Name} is a clan leader and cannot be removed directly");
        }

        // Skip if hero is kingdom ruler
        if (hero.Clan?.Kingdom?.Leader == hero)
        {
            return new(false, $"Hero {hero.Name} is a kingdom ruler and cannot be removed directly");
        }

        string heroName = hero.Name?.ToString() ?? "Unknown";

        // Remove the hero
        KillCharacterAction.ApplyByRemove(hero, showNotification: false, isForced: true);

        return new(true, $"Removed hero: {heroName}");
    }

    /// <summary>
    /// Removes multiple BLGM heroes in batch, up to the specified count.
    /// </summary>
    /// <param name="count">Maximum number of heroes to remove. If null, removes all eligible heroes.</param>
    /// <returns>Tuple containing the number of heroes removed and a detailed summary.</returns>
    public static (int removed, string details) BatchRemoveHeroes(int? count)
    {
        int removedCount = 0;
        int skippedCount = 0;
        StringBuilder details = new();

        // Get BLGM heroes from manager
        System.Collections.Generic.List<Hero> blgmHeroes = new(BLGMObjectManager.BlgmHeroes);

        int targetCount = count ?? blgmHeroes.Count;
        int processed = 0;

        foreach (Hero hero in blgmHeroes)
        {
            if (processed >= targetCount)
            {
                break;
            }

            BLGMResult result = RemoveSingleHero(hero);

            if (result.IsSuccess)
            {
                removedCount++;
                details.AppendLine($"  ✓ {result.Message}");
            }
            else
            {
                skippedCount++;
                details.AppendLine($"  - Skipped: {result.Message}");
            }

            processed++;
        }

        // Add summary at the beginning
        StringBuilder summary = new();
        summary.AppendLine($"Batch Hero Removal Complete:");
        summary.AppendLine($"  Removed: {removedCount}");
        summary.AppendLine($"  Skipped: {skippedCount}");
        summary.AppendLine();
        summary.Append(details);

        return (removedCount, summary.ToString());
    }

    /// <summary>
    /// Removes all BLGM-generated heroes from the game.
    /// </summary>
    /// <returns>BLGMResult indicating success with details about removed heroes.</returns>
    public static BLGMResult RemoveAllBlgmHeroes()
    {
        (int removed, string details) = BatchRemoveHeroes(null);
        return new(removed > 0, details);
    }
}
