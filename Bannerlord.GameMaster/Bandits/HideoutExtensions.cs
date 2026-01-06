using System;
using System.Collections.Generic;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Library;

namespace Bannerlord.GameMaster.Bandits
{
    public static class HideoutExtensions
    {
        /// <summary>
        /// Destroys a hideout by removing all bandit parties inside it.
        /// The hideout will naturally deactivate when empty (as per native game behavior).
        /// </summary>
        public static bool DestroyHideout(this Hideout hideout)
        {
            if (hideout == null || hideout.Settlement == null)
            {
                return false;
            }

            List<MobileParty> parties = hideout.Settlement.Parties;
            bool anyDestroyed = false;

            // Create a copy of the list to avoid modification during iteration
            MBList<MobileParty> partiesToDestroy = new();
            for (int i = 0; i < parties.Count; i++)
            {
                MobileParty party = parties[i];
                if (party != null && (party.IsBandit || party.IsBanditBossParty))
                {
                    partiesToDestroy.Add(party);
                }
            }

            // Destroy all bandit parties
            for (int i = 0; i < partiesToDestroy.Count; i++)
            {
                MobileParty party = partiesToDestroy[i];
                if (party.IsActive)
                {
                    DestroyPartyAction.Apply(null, party);
                    anyDestroyed = true;
                }
            }

            return anyDestroyed;
        }
    }
}