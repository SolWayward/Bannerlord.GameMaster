using System.Text;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using Bannerlord.GameMaster.Common;

namespace Bannerlord.GameMaster.RemovalHelpers;

/// <summary>
/// Handles removal of BLGM-generated clans from the game.
/// </summary>
public static class ClanRemover
{
    /// <summary>
    /// Removes a single clan if it meets the criteria for removal.
    /// </summary>
    /// <param name="clan">The clan to remove.</param>
    /// <returns>BLGMResult indicating success or failure with details.</returns>
    public static BLGMResult RemoveSingleClan(Clan clan)
    {
        if (clan == null)
        {
            return new(false, "Clan is null");
        }

        if (!clan.StringId.StartsWith("blgm_"))
        {
            return new(false, $"Clan {clan.Name} is not a BLGM-generated clan");
        }

        // Skip if player clan
        if (clan == Clan.PlayerClan)
        {
            return new(false, $"Clan {clan.Name} is the player's clan and cannot be removed");
        }

        // Skip if ruling clan of a kingdom (only if not a mercenary)
        if (clan.IsUnderMercenaryService == false && clan.Kingdom?.RulingClan == clan)
        {
            return new(false, $"Clan {clan.Name} is the ruling clan of {clan.Kingdom.Name} and cannot be removed directly");
        }

        // Skip if clan leader is kingdom ruler
        if (clan.Leader?.Clan?.Kingdom?.Leader == clan.Leader)
        {
            return new(false, $"Clan {clan.Name} leader is a kingdom ruler and cannot be removed directly");
        }

        string clanName = clan.Name?.ToString() ?? "Unknown";

        // Remove the clan (automatically handles heroes)
        DestroyClanAction.Apply(clan);

        return new(true, $"Removed clan: {clanName}");
    }

    /// <summary>
    /// Removes multiple BLGM clans in batch, up to the specified count.
    /// </summary>
    /// <param name="count">Maximum number of clans to remove. If null, removes all eligible clans.</param>
    /// <returns>Tuple containing the number of clans removed and a detailed summary.</returns>
    public static (int removed, string details) BatchRemoveClans(int? count)
    {
        int removedCount = 0;
        int skippedCount = 0;
        StringBuilder details = new();

        // Get BLGM clans from manager
        System.Collections.Generic.List<Clan> blgmClans = new(BLGMObjectManager.BlgmClans);

        int targetCount = count ?? blgmClans.Count;
        int processed = 0;

        foreach (Clan clan in blgmClans)
        {
            if (processed >= targetCount)
            {
                break;
            }

            BLGMResult result = RemoveSingleClan(clan);

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
        summary.AppendLine($"Batch Clan Removal Complete:");
        summary.AppendLine($"  Removed: {removedCount}");
        summary.AppendLine($"  Skipped: {skippedCount}");
        summary.AppendLine();
        summary.Append(details);

        return (removedCount, summary.ToString());
    }
}
