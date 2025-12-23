using System;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.CampaignBehaviors;
using TaleWorlds.CampaignSystem.Election;

namespace Bannerlord.GameMaster.Kingdoms
{
    public static class KingdomTradeAgreementExtensions
    {
        /// <summary>
		/// Forms a trade agreement with specified kingdom <br/>
        /// It doesn't matter which kingdom is the proposer
		/// </summary>
		public static void MakeTradeAgreement(this Kingdom proposingKindom, Kingdom receivingkingdom)
		{
			TradeAgreementDecision tradeDecision = new(proposingKindom.RulingClan, receivingkingdom);
			
            // receivingkingdom.AddDecision(tradeDecision, ignoreInfluenceCost: true); //Not needed to add to queue if auto accepting (suppresses notification)
			
            TradeAgreementDecision.TradeAgreementDecisionOutcome outcome = new(true, proposingKindom, receivingkingdom);
			tradeDecision.ApplyChosenOutcome(outcome);
		}
    }
}