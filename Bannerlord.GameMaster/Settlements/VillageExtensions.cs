using System;
using System.Reflection;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.CampaignSystem.Party.PartyComponents;
using Bannerlord.GameMaster.Common;
using TaleWorlds.CampaignSystem;
using Bannerlord.GameMaster.Behaviours;

namespace Bannerlord.GameMaster.Settlements
{
    public static class VillageExtensions
    {
        private static PropertyInfo _boundProperty; //Reflection cached field

        /// <summary>
        /// Finds the best trade-bound town for a village using the game's native algorithm.
        /// Prefers same-faction towns, falls back to neutral/allied towns within travel distance.
        /// </summary>
        /// <param name="village">Village to find trade town for</param>
        /// <returns>Best town for trade, or null if none found within reasonable distance</returns>
        public static Settlement GetRecommendedTradeBound(this Village village)
        {
            if (village == null)
                return null;

            // Use the game's built-in logic
            return Campaign.Current.Models.VillageTradeModel.GetTradeBoundToAssignForVillage(village);
        }

        /// <summary>
        /// Attempts to Change which settlement (castle/town) a village is bound to. <br />
        /// If the bound settlement is a town, the village is also tradebound to the town as the game will ignore the tradebound anyway for town bound villages<br />
        /// If the bound settlment is a castle, the village will automatically be trade bound to a town recommened by native bannerlord, as castles dont have trade markets<br />
        /// Changes persist automatically due to [SaveableField] attributes.
        /// </summary>
        /// <returns>BLGMResult containing a bool wasSuccessful, and a string message</returns>
        public static BLGMResult SetBoundSettlement(this Village village, Settlement newBoundSettlement)
        {
            if (village == null)
                return new BLGMResult(false, "Cannot change village binding: village is null");

            if (newBoundSettlement == null)
                return new BLGMResult(false, "Cannot change village binding: settlement is null");

            if (!newBoundSettlement.IsFortification)
                return new BLGMResult(false, $"Cannot bind {village.Settlement.Name} to {newBoundSettlement.Name}: Must be a castle or town");

            if (village.Bound == newBoundSettlement)
                return new BLGMResult(false, $"{village.Settlement.Name} is already bound to {newBoundSettlement.Name}");

            try
            {
                // Cache reflection info for performance
                if (_boundProperty == null)
                {
                    _boundProperty = typeof(Village).GetProperty("Bound",
                        BindingFlags.Public | BindingFlags.Instance);

                    if (_boundProperty == null)
                        return new BLGMResult(false, "CRITICAL: Could not find Village.Bound property via reflection");
                }

                // Store old bound for logging
                Settlement oldBound = village.Bound;

                // Invoke private setter - handles all sync automatically
                _boundProperty.SetValue(village, newBoundSettlement);

                // Update party visuals for ownership change
                village.Settlement.Party.SetVisualAsDirty();

                VillagerPartyComponent villagerParty = village.VillagerPartyComponent;
                if (villagerParty != null && villagerParty.MobileParty != null)
                    villagerParty.MobileParty.Party.SetVisualAsDirty();

                string tradeBoundInfo = "";

                // Attempt to autoset tradebound if regular bound to a castle
                if (newBoundSettlement.IsCastle)
                {
                    Settlement tradeBound = village.GetRecommendedTradeBound();
                    
                    tradeBoundInfo = "\n\nVillages Bound to castles are required to be trade bound to to a town\nAuto setting tradebound town: ";

                    if (tradeBound != null)
                    {
                        tradeBoundInfo += $"{village.SetTradeBoundSettlement(tradeBound).Message}";
                    }
                    else
                    {
                        tradeBoundInfo += $"Warning: Could not automatically find a valid town to use for trade bound settlement, please manually set the tradebound settlement otherwise village may not be able to trade";
                    }
                }
                else
                {
                    // Game will automatically set tradebound to the bound settlment if it is a town
                    tradeBoundInfo = "\n\nVillages bound to a town ignore their trade bound, only castle bound villages use trade bound settlement since castles have no trade market\nAuto setting Tradebound to regular bound: ";
                    tradeBoundInfo += $"{village.TradeBound.Name}";
                }

                // Success
                return new BLGMResult(true, $"{village.Settlement.Name} rebound from {(oldBound != null ? oldBound.Name.ToString() : "none")} to {newBoundSettlement.Name}{tradeBoundInfo}");
            }

            catch (Exception ex)
            {
                // Catch and log any reflection or runtime errors
                return new BLGMResult(false, $"ERROR changing village binding: {ex.Message}").Log();
            }
        }

        /// <summary>
        /// Attempts to change which settlement (must be town not castle) the village is TradeBound to<br />
        /// If the village is already regular bound to a town, then it ignores the trade bound settlement and should be set the same as the regular binding<br />
        /// If the village is regular bound to a castle, then it needs to be trade bound to a town as castes dont have trade markets. (SetBoundSettlment() tries to do this automatically)
        /// </summary>
        /// <returns>BLGMResult containing a bool wasSuccessful, and a string message</returns>
        public static BLGMResult SetTradeBoundSettlement(this Village village, Settlement newTradeBound)
        {
            if (village == null)
                return new BLGMResult(false, "Cannot change trade bound: village is null");

            if (newTradeBound == null)
                return new BLGMResult(false, "Cannot change trade bound: settlement is null");

            if (!newTradeBound.IsTown)
                return new BLGMResult(false, $"Cannot trade bind {village.Settlement.Name} to {newTradeBound.Name}: Must be a town");

            if (village.TradeBound == newTradeBound)
                return new BLGMResult(false, $"{village.Settlement.Name} is already trade-bound to {newTradeBound.Name}");

            try
            {
                Settlement oldTradeBound = village.TradeBound; // Used to display previous tradeBound
                Settlement currentRegularBound = village.Bound; // Used to display warning

                // This setter handles all synchronization automatically
                village.TradeBound = newTradeBound;

                // Track the change for save/load persistence
                VillageTradeBoundBehavior behavior = Campaign.Current?.GetCampaignBehavior<VillageTradeBoundBehavior>();
                if (behavior != null)
                {
                    behavior.TrackTradeBoundChange(village, newTradeBound);
                }

                string tradeBoundWarning = "";
                if (currentRegularBound != null && currentRegularBound.IsTown) // Null check required, as previous tradebound will be null if the village was just bound to a castle
                    tradeBoundWarning = "\n\nWarning: Game might ignore the new trade bound settlement since village is already regular bound to a town, usually villages are only trade bound if regular bound to a castle, since castles don't have markets.";

                return new BLGMResult(true, $"{village.Settlement.Name} trade bound changed from {(oldTradeBound != null ? oldTradeBound.Name.ToString() : "none")} to {newTradeBound.Name}{tradeBoundWarning}");
            }
            
            catch (Exception ex)
            {
                return new BLGMResult(false, $"ERROR changing trade bound: {ex.Message}").Log();
            }
        }
    }
}
