using System;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Settlements;
using Bannerlord.GameMaster.Common;
using Bannerlord.GameMaster.Common.Interfaces;

namespace Bannerlord.GameMaster.Settlements
{
    [Flags]
    public enum SettlementTypes : long
    {
        None = 0,
        Settlement = 1,         // 2^0  - General town (castle or city)
        Castle = 2,            // 2^1  - Castle specifically
        City = 4,              // 2^2  - City specifically
        Village = 8,           // 2^3  - Village
        Hideout = 16,          // 2^4  - Bandit hideout
        PlayerOwned = 32,      // 2^5  - Owned by player clan
        Besieged = 64,         // 2^6  - Currently under siege
        Raided = 128,          // 2^7  - Village is being raided
        Empire = 256,          // 2^8  - Empire culture
        Vlandia = 512,         // 2^9  - Vlandia culture
        Sturgia = 1024,        // 2^10 - Sturgia culture
        Aserai = 2048,         // 2^11 - Aserai culture
        Khuzait = 4096,        // 2^12 - Khuzait culture
        Battania = 8192,       // 2^13 - Battania culture
        Nord = 16384,          // 2^14 - Nord culture (Warsails DLC)
        LowProsperity = 32768, // 2^15 - Prosperity < 3000
        MediumProsperity = 65536, // 2^16 - Prosperity 3000-6000
        HighProsperity = 131072,  // 2^17 - Prosperity > 6000
    }

    public static class SettlementExtensions
    {
        #region Actions

        /// <inheritdoc cref="SettlementManager.ChangeSettlementOwner"/>
        public static void ChangeOwner(this Settlement settlement, Hero hero) => SettlementManager.ChangeSettlementOwner(settlement, hero);
        
        /// <summary>
        /// Renames a settlement with persistence. The name change will persist through save/load cycles.
        /// </summary>
        /// <param name="settlement">The settlement to rename</param>
        /// <param name="newName">The new name for the settlement</param>
        /// <returns>BLGMResult indicating success or failure with a message</returns>
        public static BLGMResult Rename(this Settlement settlement, string newName) => SettlementManager.RenameSettlement(settlement, newName);

        /// <summary>
        /// Resets a settlement's name to its original value.
        /// </summary>
        /// <param name="settlement">The settlement to reset</param>
        /// <returns>BLGMResult indicating success or failure with a message</returns>
        public static BLGMResult ResetName(this Settlement settlement) => SettlementManager.ResetSettlementName(settlement);

        /// <summary>
        /// Gets settlment original name if renamed, Other Null if not renamed
        /// </summary>
        public static string GetOriginalName(this Settlement settlement) => SettlementManager.GetOriginalSettlementName(settlement);

        /// <summary>
        /// Returns true if the settlement was renamed
        /// </summary>
        public static bool IsRenamed(this Settlement settlement) => SettlementManager.IsSettlementRenamed(settlement);
        
        #endregion

        #region Helpers
        /// <summary>
        /// Gets all settlement type flags for this settlement
        /// </summary>
        public static SettlementTypes GetSettlementTypes(this Settlement settlement)
        {
            SettlementTypes types = SettlementTypes.None;

            // Settlement type categories
            if (settlement.IsCastle)
            {
                types |= SettlementTypes.Castle;
                types |= SettlementTypes.Settlement;
            }
            else if (settlement.IsTown)
            {
                types |= SettlementTypes.City;
                types |= SettlementTypes.Settlement;
            }

            else if (settlement.IsHideout)
            {
                types |= SettlementTypes.Hideout;
            }

            if (settlement.IsVillage)
            {
                types |= SettlementTypes.Village;
            }

            // Ownership
            if (settlement.OwnerClan == Hero.MainHero.Clan)
            {
                types |= SettlementTypes.PlayerOwned;
            }

            // State flags
            if (settlement.IsUnderSiege)
            {
                types |= SettlementTypes.Besieged;
            }

            if (settlement.IsVillage && settlement.Village != null && settlement.Village.VillageState == Village.VillageStates.BeingRaided)
            {
                types |= SettlementTypes.Raided;
            }

            // Culture flags
            if (settlement.Culture != null)
            {
                string cultureId = settlement.Culture.StringId.ToLower();
                if (cultureId.Contains("empire"))
                    types |= SettlementTypes.Empire;
                else if (cultureId.Contains("vlandia"))
                    types |= SettlementTypes.Vlandia;
                else if (cultureId.Contains("sturgia"))
                    types |= SettlementTypes.Sturgia;
                else if (cultureId.Contains("aserai"))
                    types |= SettlementTypes.Aserai;
                else if (cultureId.Contains("khuzait"))
                    types |= SettlementTypes.Khuzait;
                else if (cultureId.Contains("battania"))
                    types |= SettlementTypes.Battania;
                else if (cultureId.Contains("nord"))
                    types |= SettlementTypes.Nord;
            }

            // Prosperity levels (for towns and villages)
            if ((settlement.IsTown || settlement.IsCastle) && settlement.Town != null)
            {
                float prosperity = settlement.Town.Prosperity;
                if (prosperity < 3000)
                    types |= SettlementTypes.LowProsperity;
                else if (prosperity <= 6000)
                    types |= SettlementTypes.MediumProsperity;
                else
                    types |= SettlementTypes.HighProsperity;
            }
            else if (settlement.IsVillage && settlement.Village != null)
            {
                float prosperity = settlement.Village.Hearth; // Villages use Hearth instead of Prosperity
                if (prosperity < 300)
                    types |= SettlementTypes.LowProsperity;
                else if (prosperity <= 600)
                    types |= SettlementTypes.MediumProsperity;
                else
                    types |= SettlementTypes.HighProsperity;
            }

            return types;
        }

        /// <summary>
        /// Checks if settlement has ALL specified flags
        /// </summary>
        public static bool HasAllTypes(this Settlement settlement, SettlementTypes types)
        {
            if (types == SettlementTypes.None) return true;
            var settlementTypes = settlement.GetSettlementTypes();
            return (settlementTypes & types) == types;
        }

        /// <summary>
        /// Checks if settlement has ANY of the specified flags
        /// </summary>
        public static bool HasAnyType(this Settlement settlement, SettlementTypes types)
        {
            if (types == SettlementTypes.None) return true;
            var settlementTypes = settlement.GetSettlementTypes();
            return (settlementTypes & types) != SettlementTypes.None;
        }

        /// <summary>
        /// Returns a formatted string containing the settlement's details
        /// </summary>
        public static string FormattedDetails(this Settlement settlement)
        {
            string settlementType = settlement.IsTown ? "City"
                : settlement.IsCastle ? "Castle"
                : settlement.IsVillage ? "Village"
                : settlement.IsHideout ? "Hideout"
                : "Unknown";

            string ownerName = settlement.OwnerClan?.Name?.ToString() ?? "None";
            string kingdomName = settlement.MapFaction?.Name?.ToString() ?? "None";
            string cultureName = settlement.Culture?.Name?.ToString() ?? "None";

            string prosperityInfo = "";
            if ((settlement.IsTown | settlement.IsCastle) && settlement.Town != null)
            {
                prosperityInfo = $"Prosperity: {settlement.Town.Prosperity:F0}";
            }
            else if (settlement.IsVillage && settlement.Village != null)
            {
                prosperityInfo = $"Hearth: {settlement.Village.Hearth:F0}";
            }

            return $"{settlement.StringId}\t{settlement.Name}\t[{settlementType}]\tOwner: {ownerName}\t" +
                   $"Kingdom: {kingdomName}\tCulture: {cultureName}\t{prosperityInfo}";
        }

        /// <summary>
        /// Alias for GetSettlementTypes to match IEntityExtensions interface
        /// </summary>
        public static SettlementTypes GetTypes(this Settlement settlement) => settlement.GetSettlementTypes();
    }

    #endregion

    #region Wrapper

    /// <summary>
    /// Wrapper class implementing IEntityExtensions interface for Settlement entities
    /// </summary>
    public class SettlementExtensionsWrapper : IEntityExtensions<Settlement, SettlementTypes>
    {
        public SettlementTypes GetTypes(Settlement entity) => entity.GetSettlementTypes();
        public bool HasAllTypes(Settlement entity, SettlementTypes types) => entity.HasAllTypes(types);
        public bool HasAnyType(Settlement entity, SettlementTypes types) => entity.HasAnyType(types);
        public string FormattedDetails(Settlement entity) => entity.FormattedDetails();
    }

    #endregion
}