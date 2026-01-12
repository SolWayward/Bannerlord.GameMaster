using System.Text;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using Bannerlord.GameMaster.Common;

namespace Bannerlord.GameMaster.RemovalHelpers;

/// <summary>
/// Handles removal of BLGM-generated kingdoms from the game.
/// </summary>
public static class KingdomRemover
{
    /// <summary>
    /// Removes a single kingdom if it meets the criteria for removal.
    /// </summary>
    /// <param name="kingdom">The kingdom to remove.</param>
    /// <returns>BLGMResult indicating success or failure with details.</returns>
    public static BLGMResult RemoveSingleKingdom(Kingdom kingdom)
    {
        if (kingdom == null)
        {
            return new(false, "Kingdom is null");
        }

        if (!kingdom.StringId.StartsWith("blgm_"))
        {
            return new(false, $"Kingdom {kingdom.Name} is not a BLGM-generated kingdom");
        }

        // Skip if player is part of this kingdom
        if (Hero.MainHero?.MapFaction == kingdom)
        {
            return new(false, $"Kingdom {kingdom.Name} contains the player and cannot be removed");
        }

        string kingdomName = kingdom.Name?.ToString() ?? "Unknown";

        // Remove the kingdom (automatically handles clans and heroes)
        DestroyKingdomAction.Apply(kingdom);

        return new(true, $"Removed kingdom: {kingdomName}");
    }

    /// <summary>
    /// Removes multiple BLGM kingdoms in batch, up to the specified count.
    /// </summary>
    /// <param name="count">Maximum number of kingdoms to remove. If null, removes all eligible kingdoms.</param>
    /// <returns>Tuple containing the number of kingdoms removed and a detailed summary.</returns>
    public static (int removed, string details) BatchRemoveKingdoms(int? count)
    {
        int removedCount = 0;
        int skippedCount = 0;
        StringBuilder details = new();

        // Get BLGM kingdoms from manager
        System.Collections.Generic.List<Kingdom> blgmKingdoms = new(BLGMObjectManager.BlgmKingdoms);

        int targetCount = count ?? blgmKingdoms.Count;
        int processed = 0;

        foreach (Kingdom kingdom in blgmKingdoms)
        {
            if (processed >= targetCount)
            {
                break;
            }

            BLGMResult result = RemoveSingleKingdom(kingdom);

            if (result.wasSuccessful)
            {
                removedCount++;
                details.AppendLine($"  ✓ {result.message}");
            }
            else
            {
                skippedCount++;
                details.AppendLine($"  - Skipped: {result.message}");
            }

            processed++;
        }

        // Add summary at the beginning
        StringBuilder summary = new();
        summary.AppendLine($"Batch Kingdom Removal Complete:");
        summary.AppendLine($"  Removed: {removedCount}");
        summary.AppendLine($"  Skipped: {skippedCount}");
        summary.AppendLine();
        summary.Append(details);

        return (removedCount, summary.ToString());
    }

    /// <summary>
    /// Removes all BLGM-generated kingdoms from the game.
    /// </summary>
    /// <returns>BLGMResult indicating success with details about removed kingdoms.</returns>
    public static BLGMResult RemoveAllBlgmKingdoms()
    {
        (int removed, string details) = BatchRemoveKingdoms(null);
        return new(removed > 0, details);
    }
}
