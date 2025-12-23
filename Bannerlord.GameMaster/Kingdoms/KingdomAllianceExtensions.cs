using System;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.CampaignBehaviors;
using TaleWorlds.CampaignSystem.Election;

namespace Bannerlord.GameMaster.Kingdoms
{
    public static class KingdomAllianceExtensions
    {
        /// <summary>
		/// Forms an alliance with specified kingdom <br/>
        /// callToWar will only auto call the ally to war for the player if the player is the proposing kingdom
		/// </summary>
		public static void DeclareAlliance(this Kingdom proposingKindom, Kingdom receivingkingdom, bool callToWar = true)
		{
			FactionManager.SetNeutral(receivingkingdom, proposingKindom);	
			StartAllianceDecision allianceDecision = new(proposingKindom.RulingClan, receivingkingdom);
			
            // receivingkingdom.AddDecision(allianceDecision, ignoreInfluenceCost: true); //Not needed to add to queue if auto accepting (suppresses notification)
			
            StartAllianceDecision.StartAllianceDecisionOutcome outcome = new(true, proposingKindom, receivingkingdom);
			allianceDecision.ApplyChosenOutcome(outcome);

			if (callToWar)
            {   
                // Clears auto generated call to war notification (doesnt work, notification not cleared)
                //ClearCallToWarProposalDecisions(proposingKindom);
                //ClearCallToWarProposalDecisions(receivingkingdom);
     
				ProposeCallAllyToWarForceAccept(proposingKindom, receivingkingdom);
            }
		}

		/// <summary>
		/// Proposes to ally to join war against target kingdom and forces ally to accept
		/// </summary>
		public static void ProposeCallAllyToWarForceAccept(this Kingdom proposer, Kingdom ally, Kingdom enemy)
		{
			ProposeCallAllyToWar(proposer, ally, enemy);
			AcceptCallAllyToWar(proposer, ally, enemy);  // Fixed parameter order
		}

		/// <summary>
		/// Proposes to ally to join war against all kingdoms the proposer is at war with and forces ally to accept
		/// </summary>
		public static void ProposeCallAllyToWarForceAccept(this Kingdom proposer, Kingdom ally)
		{
			ProposeCallAllyToWar(proposer, ally);
			AcceptCallAllyToWar(proposer, ally);  // Fixed parameter order
		}

		/// <summary>
		/// Proposes to ally to join war against target enemy kingdom
		/// </summary>
		public static void ProposeCallAllyToWar(this Kingdom proposer, Kingdom ally, Kingdom enemy)
		{			
			ProposeCallToWarAgreementDecision decision = new(proposer.RulingClan, ally, enemy);
			
            //proposer.AddDecision(decision, ignoreInfluenceCost: true); // Not needed to add to queue if auto accepting (suppresses notification)
			
			ProposeCallToWarAgreementDecision.ProposeCallToWarAgreementDecisionOutcome outcome = new(true, proposer, ally, enemy);
			decision.ApplyChosenOutcome(outcome);
		}

		/// <summary>
		/// Proposes to ally to join war against all kingdoms the proposer is at war with
		/// </summary>
		public static void ProposeCallAllyToWar(this Kingdom proposer, Kingdom ally)
		{			
			foreach(IFaction enemy in proposer.FactionsAtWarWith)
			{
				if (enemy.IsKingdomFaction)
					ProposeCallAllyToWar(proposer, ally, (Kingdom)enemy); // Fixed: was AcceptCallAllyToWar
			}	
		}

		/// <summary>
		/// Forces ally to accept any proposals to join war against target enemy kingdom
		/// </summary>
		public static void AcceptCallAllyToWar(this Kingdom proposer, Kingdom ally, Kingdom enemy)
		{			
			AcceptCallToWarAgreementDecision decision = new(proposer.RulingClan, ally, enemy);
			
            //ally.AddDecision(decision, ignoreInfluenceCost: true); //Not needed to add to queue if auto accepting (suppresses notification)
			
			AcceptCallToWarAgreementDecision.AcceptCallToWarAgreementDecisionOutcome outcome = 
				new(true, proposer, ally, enemy);
			decision.ApplyChosenOutcome(outcome);
		}

		/// <summary>
		/// Forces ally to accept any proposals to join war against all kingdoms proposer is at war with
		/// </summary>
		public static void AcceptCallAllyToWar(this Kingdom proposer, Kingdom ally)
		{			
			foreach(IFaction targetKingdom in proposer.FactionsAtWarWith)
			{
				if (targetKingdom.IsKingdomFaction)
					AcceptCallAllyToWar(proposer, ally, (Kingdom)targetKingdom);
			}	
		}

        /// <summary>
        /// Used to clear CallToWar notification on alliance formed, if call to war is true on DeclareAlliance method
        /// </summary>
        /// <param name="kingdom"></param>
        private static void ClearCallToWarProposalDecisions(Kingdom kingdom)
        {
            // Convert MBReadOnlyList to IEnumerable first, then use LINQ
            var decisionsToRemove = kingdom.UnresolvedDecisions
                .Where(d => d is ProposeCallToWarAgreementDecision)
                .Cast<ProposeCallToWarAgreementDecision>()
                .ToList();
            
            foreach (var decision in decisionsToRemove)
                kingdom.UnresolvedDecisions.Remove(decision);
        }
    }
}