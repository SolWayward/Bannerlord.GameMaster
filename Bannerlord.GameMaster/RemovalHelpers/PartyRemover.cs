using System.Text;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.Party;

namespace Bannerlord.GameMaster.RemovalHelpers;

/// <summary>
/// Handles removal of mobile parties led by BLGM-generated heroes.
/// </summary>
public static class PartyRemover
{
    /// <summary>
    /// Removes multiple mobile parties led by BLGM heroes in batch, up to the specified count.
    /// </summary>
    /// <param name="count">Maximum number of parties to remove. If null, removes all eligible parties.</param>
    /// <returns>Tuple containing the number of parties removed and a detailed summary.</returns>
    public static (int removed, string details) BatchRemoveParties(int? count)
    {
        int removedCount = 0;
        int skippedCount = 0;
        StringBuilder details = new();

        // Get all mobile parties
        System.Collections.Generic.List<MobileParty> allParties = new();
        foreach (MobileParty party in Campaign.Current.CampaignObjectManager.MobileParties)
        {
            allParties.Add(party);
        }

        int targetCount = count ?? allParties.Count;
        int processed = 0;

        foreach (MobileParty party in allParties)
        {
            if (removedCount >= targetCount)
            {
                break;
            }

            // Check if party leader is a BLGM hero
            if (party.LeaderHero?.StringId.StartsWith("blgm_") != true)
            {
                continue;
            }

            // Skip player party
            if (party == MobileParty.MainParty)
            {
                skippedCount++;
                details.AppendLine($"  - Skipped: Party {party.Name} is the player's party");
                processed++;
                continue;
            }

            // Skip if leader is clan leader
            if (party.LeaderHero.Clan?.Leader == party.LeaderHero)
            {
                skippedCount++;
                details.AppendLine($"  - Skipped: Party {party.Name} leader is a clan leader");
                processed++;
                continue;
            }

            // Skip if leader is kingdom ruler
            if (party.LeaderHero.Clan?.Kingdom?.Leader == party.LeaderHero)
            {
                skippedCount++;
                details.AppendLine($"  - Skipped: Party {party.Name} leader is a kingdom ruler");
                processed++;
                continue;
            }

            string partyName = party.Name?.ToString() ?? "Unknown";

            // Remove the party
            DestroyPartyAction.Apply(null, party);

            removedCount++;
            details.AppendLine($"  ✓ Removed party: {partyName}");
            processed++;
        }

        // Add summary at the beginning
        StringBuilder summary = new();
        summary.AppendLine($"Batch Party Removal Complete:");
        summary.AppendLine($"  Removed: {removedCount}");
        summary.AppendLine($"  Skipped: {skippedCount}");
        summary.AppendLine();
        summary.Append(details);

        return (removedCount, summary.ToString());
    }
}
