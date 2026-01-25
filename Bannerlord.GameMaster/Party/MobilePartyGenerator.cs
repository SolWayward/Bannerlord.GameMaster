using System;
using System.Linq;
using Bannerlord.GameMaster.Characters;
using Bannerlord.GameMaster.Common;
using Bannerlord.GameMaster.Cultures;
using Bannerlord.GameMaster.Heroes;
using Bannerlord.GameMaster.Information;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Party.PartyComponents;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Library;

namespace Bannerlord.GameMaster.Party
{
    /// <summary>
    /// Centralized party generation system with layered architecture.
    /// Layer 1: Base party initialization methods
    /// Layer 2: Type-specific party methods (Lord, Bandit, Settlement)
    /// Layer 3: Convenience methods for specific use cases
    /// </summary>
    public static class MobilePartyGenerator
    {
        #region Layer 1: Base Methods

        /// <summary>
        /// Initializes a party with common settings after creation.
        /// Called by party creation methods to apply consistent configuration.
        /// </summary>
        private static void InitializePartySettings(
            MobileParty party,
            Settlement targetSettlement = null,
            float aggressiveness = 0.5f,
            int partyTradeGold = 0,
            bool enableAi = true)
        {
            MobileParty.NavigationType navType = party.IsCurrentlyAtSea
                ? MobileParty.NavigationType.Naval
                : MobileParty.NavigationType.Default;

            party.DesiredAiNavigationType = navType;
            party.Aggressiveness = aggressiveness;

            if (partyTradeGold > 0)
                party.PartyTradeGold = partyTradeGold;

            if (enableAi)
            {
                party.Ai.EnableAi();

                if (targetSettlement != null)
                    party.SetMoveGoToSettlement(targetSettlement, navType, false);

                party.Ai.CheckPartyNeedsUpdate();
                party.RecalculateShortTermBehavior();
            }
            else
            {
                party.Ai.DisableAi();
            }
        }

        #endregion

        #region Layer 2: Type-Specific Methods

        // MARK: CreateLordParty
        /// <summary>
        /// Creates a lord party for a hero using native LordPartyComponent.
        /// This is the standard way to create noble/lord parties.
        /// </summary>
        /// <param name="hero">The hero who will own and lead the party</param>
        /// <param name="spawnSettlement">Settlement to spawn at (uses hero's home if null)</param>
        /// <param name="spawnRadius">Radius around settlement to spawn within</param>
        /// <param name="partyTradeGold">Initial trade gold (default: 20000)</param>
        /// <param name="enableAi">Whether to enable AI control</param>
        /// <returns>The created MobileParty</returns>
        public static MobileParty CreateLordParty(
            Hero hero,
            Settlement spawnSettlement = null,
            float spawnRadius = 0.5f,
            int partyTradeGold = 20000,
            bool enableAi = true)
        {
            if (hero == null)
            {
                BLGMResult.Error("CreateLordParty() failed, hero cannot be null", new ArgumentNullException(nameof(hero))).Log();
                return null;
            }

            Settlement settlement = spawnSettlement ?? hero.HomeSettlement ?? hero.GetHomeOrAlternativeSettlement();

            if (settlement == null)
            {
                BLGMResult.Error("CreateLordParty() Cannot create party: No valid settlement found for spawning", new InvalidOperationException("Cannot create party: No valid settlement found for spawning")).Log();
                return null;
            }

            MobileParty party = LordPartyComponent.CreateLordParty(
                stringId: $"lord_{hero.StringId}_temp",
                hero: hero,
                position: settlement.GatePosition,
                spawnRadius: spawnRadius,
                spawnSettlement: settlement,
                partyLeader: hero
            );

            // Register with BLGMObjectManager and set proper StringId
            BLGMObjectManager.RegisterParty(party, BLGMObjectManager.PartyTypeNames.Lord, hero.StringId);

            float aggressiveness = Math.Max(0.3f, RandomNumberGen.Instance.NextRandomFloat());
            InitializePartySettings(party, settlement, aggressiveness, partyTradeGold, enableAi);

            return party;
        }

        // MARK: CreateBanditParty
        /// <summary>
        /// Creates a bandit party at a hideout using native BanditPartyComponent.
        /// </summary>
        /// <param name="hideout">The hideout this bandit party belongs to</param>
        /// <param name="banditClan">The bandit clan (if null, uses hideout's owning clan)</param>
        /// <param name="isBossParty">Whether this is a boss party</param>
        /// <param name="partyTemplate">Party template to use (if null, uses culture default)</param>
        /// <returns>The created MobileParty</returns>
        public static MobileParty CreateBanditParty(
            Hideout hideout,
            Clan banditClan = null,
            bool isBossParty = false,
            PartyTemplateObject partyTemplate = null)
        {
            if (hideout == null)
            {
                BLGMResult.Error("CreateBanditParty() failed, hideout cannot be null", new ArgumentNullException(nameof(hideout))).Log();
                return null;
            }

            Clan clan = banditClan ?? hideout.Settlement.OwnerClan;

            if (clan == null)
            {
                BLGMResult.Error("CreateBanditParty() failed, Cannot create bandit party: No valid clan found", new InvalidOperationException("Cannot create bandit party: No valid clan found")).Log();
                return null;
            }

            PartyTemplateObject template = partyTemplate
                ?? clan.Culture.BanditBossPartyTemplate
                ?? clan.DefaultPartyTemplate;

            MobileParty party = BanditPartyComponent.CreateBanditParty(
                $"bandit_{hideout.StringId}_temp",
                clan,
                hideout,
                isBossParty,
                template,
                hideout.Settlement.GatePosition
            );

            // Register with BLGMObjectManager and set proper StringId
            BLGMObjectManager.RegisterParty(party, BLGMObjectManager.PartyTypeNames.Bandit, hideout.StringId);

            return party;
        }

        // MARK: CreateLooterParty
        /// <summary>
        /// Creates a looter party near a settlement.
        /// Looters are bandits without a hideout.
        /// </summary>
        /// <param name="relatedSettlement">Settlement the looters spawn near</param>
        /// <param name="partyTemplate">Party template (if null, uses looters template)</param>
        /// <returns>The created MobileParty</returns>
        public static MobileParty CreateLooterParty(
            Settlement relatedSettlement,
            PartyTemplateObject partyTemplate = null)
        {
            if (relatedSettlement == null)
            {
                BLGMResult.Error("CreateLooterParty() failed, relatedSettlement cannot be null", new ArgumentNullException(nameof(relatedSettlement))).Log();
                return null;
            }

            Clan lootersClan = Clan.BanditFactions.FirstOrDefault(c => c.Culture == CultureLookup.Looters)
                              ?? Clan.BanditFactions.FirstOrDefault();

            if (lootersClan == null)
            {
                BLGMResult.Error("CreateLooterParty() failed, Cannot create looter party: No bandit clan found", new InvalidOperationException("Cannot create looter party: No bandit clan found")).Log();
                return null;
            }

            PartyTemplateObject template = partyTemplate ?? lootersClan.DefaultPartyTemplate;

            MobileParty party = BanditPartyComponent.CreateLooterParty(
                $"looters_{relatedSettlement.StringId}_temp",
                lootersClan,
                relatedSettlement,
                false,
                template,
                relatedSettlement.GatePosition
            );

            // Register with BLGMObjectManager and set proper StringId
            BLGMObjectManager.RegisterParty(party, BLGMObjectManager.PartyTypeNames.Looter, relatedSettlement.StringId);

            return party;
        }

        // MARK: CreateSettlementParty
        /// <summary>
        /// Creates a settlement-based party (villagers, patrol, militia).
        /// </summary>
        /// <param name="settlement">The settlement this party belongs to</param>
        /// <param name="partyType">Type of settlement party to create</param>
        /// <param name="partyTemplate">Optional custom party template</param>
        /// <returns>The created MobileParty</returns>
        public static MobileParty CreateSettlementParty(
            Settlement settlement,
            SettlementPartyType partyType,
            PartyTemplateObject partyTemplate = null)
        {
            if (settlement == null)
            {
                BLGMResult.Error("CreateSettlementParty() failed, settlement cannot be null", new ArgumentNullException(nameof(settlement))).Log();
                return null;
            }

            switch (partyType)
            {
                case SettlementPartyType.Villager:
                    return CreateVillagerPartyInternal(settlement);
                case SettlementPartyType.Patrol:
                    return CreatePatrolPartyInternal(settlement, partyTemplate);
                case SettlementPartyType.Militia:
                    return CreateMilitiaPartyInternal(settlement);
                default:
                    BLGMResult.Error($"CreateSettlementParty() failed: Unknown settlement party type: {partyType}",
                        new ArgumentException($"Unknown settlement party type: {partyType}")).Log();
                    return null;
            }
        }

        private static MobileParty CreateVillagerPartyInternal(Settlement settlement)
        {
            if (!settlement.IsVillage)
            {
                BLGMResult.Error("CreateVillagerPartyInternal() failed, Villager parties can only be created for villages", new InvalidOperationException("Villager parties can only be created for villages")).Log();
                return null;
            }

            MobileParty party = VillagerPartyComponent.CreateVillagerParty($"villager_{settlement.StringId}_temp", settlement.Village);
            
            // Register with BLGMObjectManager and set proper StringId
            BLGMObjectManager.RegisterParty(party, BLGMObjectManager.PartyTypeNames.Villager, settlement.StringId);
            
            return party;
        }

        private static MobileParty CreatePatrolPartyInternal(Settlement settlement, PartyTemplateObject template)
        {
            if (!settlement.IsFortification)
            {
                BLGMResult.Error("CreatePatrolPartyInternal() failed, Patrol parties can only be created for fortifications (towns/castles)", new InvalidOperationException("Patrol parties can only be created for fortifications (towns/castles)")).Log();
                return null;
            }

            PartyTemplateObject partyTemplate = template
                ?? settlement.Culture.SettlementPatrolPartyTemplateStrong
                ?? settlement.Culture.DefaultPartyTemplate;

            MobileParty party = PatrolPartyComponent.CreatePatrolParty(
                $"patrol_{settlement.StringId}_temp",
                settlement.GatePosition,
                1.0f,
                settlement,
                partyTemplate
            );

            // Register with BLGMObjectManager and set proper StringId
            BLGMObjectManager.RegisterParty(party, BLGMObjectManager.PartyTypeNames.Patrol, settlement.StringId);

            return party;
        }

        private static MobileParty CreateMilitiaPartyInternal(Settlement settlement)
        {
            if (!settlement.IsFortification)
            {
                BLGMResult.Error("CreateMilitiaPartyInternal() failed, Militia parties can only be created for fortifications (towns/castles)", new InvalidOperationException("Militia parties can only be created for fortifications (towns/castles)")).Log();
                return null;
            }

            MobileParty party = MilitiaPartyComponent.CreateMilitiaParty($"militia_{settlement.StringId}_temp", settlement);
            
            // Register with BLGMObjectManager and set proper StringId
            BLGMObjectManager.RegisterParty(party, BLGMObjectManager.PartyTypeNames.Militia, settlement.StringId);
            
            return party;
        }

        #endregion

        #region Layer 3: Convenience Methods

        // MARK: CreateClanParty
        /// <summary>
        /// Creates a party for a clan member (convenience wrapper for CreateLordParty).
        /// If the hero doesn't have a clan, throws an exception.
        /// </summary>
        public static MobileParty CreateClanParty(Hero hero, Settlement spawnSettlement = null)
        {
            if (hero == null)
            {
                BLGMResult.Error("CreateClanParty() failed, hero cannot be null", new ArgumentNullException(nameof(hero))).Log();
                return null;
            }

            if (hero.Clan == null)
            {
                BLGMResult.Error("CreateClanParty() Cannot create clan party: Hero has no clan", new InvalidOperationException("Cannot create clan party: Hero has no clan")).Log();
                return null;
            }

            return CreateLordParty(hero, spawnSettlement ?? hero.Clan.HomeSettlement);
        }

        /// <summary>
        /// Creates a party for a new hero, generating a leader if none specified.
        /// </summary>
        /// <param name="clan">The clan this party belongs to</param>
        /// <param name="leader">Optional leader hero. If null, generates a new one.</param>
        /// <param name="spawnSettlement">Settlement to spawn at</param>
        /// <returns>The created MobileParty</returns>
        public static MobileParty CreateClanPartyWithNewLeader(
            Clan clan,
            Hero leader = null,
            Settlement spawnSettlement = null)
        {
            if (clan == null)
            {
                BLGMResult.Error("CreateClanPartyWithNewLeader() failed, clan cannot be null", new ArgumentNullException(nameof(clan))).Log();
                return null;
            }

            Settlement settlement = spawnSettlement ?? clan.HomeSettlement;

            if (settlement == null)
            {
                BLGMResult.Error("CreateClanPartyWithNewLeader() failed, Cannot create party: No valid settlement found", new InvalidOperationException("Cannot create party: No valid settlement found")).Log();
                return null;
            }

            Hero partyLeader = leader;
            if (partyLeader == null)
            {
                // Generate a new hero for this party using HeroGenerator
                CultureFlags cultureFlag = CultureLookup.GetCultureFlag(clan.Culture);
                bool isFemale = RandomNumberGen.Instance.NextRandomInt(2) == 0;
                string heroName = CultureLookup.GetUniqueRandomHeroName(clan.Culture, isFemale);

                partyLeader = HeroGenerator.CreateLord(
                    heroName,
                    cultureFlag,
                    isFemale ? GenderFlags.Female : GenderFlags.Male,
                    clan,
                    withParty: false,
                    settlement: settlement
                );
            }

            return CreateLordParty(partyLeader, settlement);
        }

        // MARK: Caravan Methods
        /// <summary>
        /// Creates a caravan party for the specified owner.
        /// </summary>
        /// <param name="owner">The hero who owns the caravan</param>
        /// <param name="homeSettlement">Settlement the caravan is based at (must be a town)</param>
        /// <param name="caravanLeader">Optional leader hero</param>
        /// <param name="isElite">Whether this is an armed/elite caravan</param>
        /// <returns>The created MobileParty</returns>
        public static MobileParty CreateCaravanParty(
            Hero owner,
            Settlement homeSettlement,
            Hero caravanLeader = null,
            bool isElite = false)
        {
            if (owner == null)
            {
                BLGMResult.Error("CreateCaravanParty() failed, Owner cannot be null", new ArgumentNullException(nameof(owner))).Log();
                return null;
            }

            if (homeSettlement == null || !homeSettlement.IsTown)
            {
                BLGMResult.Error("CreateCaravanParty() failed, Caravan home settlement must be a town", new ArgumentException("Caravan home settlement must be a town")).Log();
                return null;
            }

            // Determine if this should be a land or naval caravan based on settlement
            bool isLand = !homeSettlement.HasPort;

            PartyTemplateObject template = Helpers.CaravanHelper.GetRandomCaravanTemplate(
                owner.Culture,
                isElite,
                isLand);

            MobileParty party = CaravanPartyComponent.CreateCaravanParty(
                owner,
                homeSettlement,
                template,
                false,
                caravanLeader,
                null,
                isElite
            );

            // Register with BLGMObjectManager and set proper StringId
            BLGMObjectManager.RegisterParty(party, BLGMObjectManager.PartyTypeNames.Caravan, owner.StringId);

            return party;
        }


        private static PartyTemplateObject GetCaravanTemplate(CultureObject culture, bool isElite, bool isLand = true)
        {
            // Use native helper which properly handles elite/non-elite and land/naval
            return Helpers.CaravanHelper.GetRandomCaravanTemplate(culture, isElite, isLand);
        }

        // MARK: Villager Methods
        /// <summary>
        /// Creates a villager party from a village.
        /// </summary>
        public static MobileParty CreateVillagerParty(Village village)
        {
            if (village == null)
            {
                BLGMResult.Error("CreateVillagerParty() failed, village cannot be null", new ArgumentNullException(nameof(village))).Log();
                return null;
            }

            return CreateSettlementParty(village.Settlement, SettlementPartyType.Villager);
        }

        /// <summary>
        /// Creates a villager party from a settlement (must be a village).
        /// </summary>
        public static MobileParty CreateVillagerParty(Settlement settlement)
        {
            if (settlement == null)
            {
                BLGMResult.Error("CreateVillagerParty() failed, settlement cannot be null", new ArgumentNullException(nameof(settlement))).Log();
                return null;
            }

            return CreateSettlementParty(settlement, SettlementPartyType.Villager);
        }

        // MARK: Patrol/Guard Methods
        /// <summary>
        /// Creates a patrol party (guard party) for a fortification.
        /// Uses strong patrol party template by default if no template is specified
        /// </summary>
        /// <param name="settlement">Town or Castle</param>
        /// <param name="template">Template to use for patrol party, defaults to strong patrol party template of settlement culture if none specified</param>
        public static MobileParty CreatePatrolParty(Settlement settlement, PartyTemplateObject template = null)
        {
            if (settlement == null)
            {
                BLGMResult.Error("CreatePatrolParty() failed, settlement cannot be null", new ArgumentNullException(nameof(settlement))).Log();
                return null;
            }

            return CreateSettlementParty(settlement, SettlementPartyType.Patrol, template);
        }

        /// <summary>
        /// Creates a naval party at the specified port town. If no template is provided, the default culture Naval patrol template is used.
        /// Requires settlement to be a port town and also requires War Sails to be loaded, returns null if neither condition is met.
        /// </summary>
        /// <param name="settlement">Port Town</param>
        /// <param name="template">Template to use for party creation, recommended to leave as default null, which will use the Naval patrol template</param>
        /// <returns></returns>
        public static MobileParty CreateNavalPatrolParty(Settlement settlement, PartyTemplateObject template = null)
        {
            if (!GameEnvironment.IsWarsailsDlcLoaded)
            {
                BLGMResult.Error("CreateNavalPatrolParty() failed: War Sails DLC is not loaded",
                    new InvalidOperationException("Naval patrol requires War Sails DLC to be installed and loaded")).Log();
                return null;
            }

            if (!settlement.IsFortification || !settlement.HasPort)
            {
                BLGMResult.Error("CreateNavalPatrolParty() failed: Naval patrol requires a port town/castle",
                    new InvalidOperationException("Naval patrol requires a port town/castle")).Log();
                return null;
            }

            PartyTemplateObject partyTemplate = template
                ?? settlement.Culture.SettlementPatrolPartyTemplateNaval
                ?? settlement.Culture.DefaultPartyTemplate;

            MobileParty party = PatrolPartyComponent.CreatePatrolParty(
                $"naval_patrol_{settlement.StringId}_temp",
                settlement.PortPosition,  // Spawn at port, not gate
                1.0f,
                settlement,
                partyTemplate
            );

            // Register with BLGMObjectManager and set proper StringId
            BLGMObjectManager.RegisterParty(party, BLGMObjectManager.PartyTypeNames.NavalPatrol, settlement.StringId);

            return party;
        }

        // MARK: Militia Methods
        /// <summary>
        /// Creates a militia party for a fortification.
        /// </summary>
        public static MobileParty CreateMilitiaParty(Settlement settlement)
        {
            if (settlement == null)
            {
                BLGMResult.Error("CreateMilitiaParty() failed, settlement cannot be null", new ArgumentNullException(nameof(settlement))).Log();
                return null;
            }

            return CreateSettlementParty(settlement, SettlementPartyType.Militia);
        }

        // MARK: Bandit Convenience Methods
        /// <summary>
        /// Creates a desert bandit party at a hideout.
        /// </summary>
        public static MobileParty CreateDesertBanditParty(Hideout hideout, bool isBossParty = false)
        {
            return CreateBanditPartyForCulture(hideout, CultureLookup.Deserters, isBossParty);
        }

        /// <summary>
        /// Creates a forest bandit party at a hideout.
        /// </summary>
        public static MobileParty CreateForestBanditParty(Hideout hideout, bool isBossParty = false)
        {
            return CreateBanditPartyForCulture(hideout, CultureLookup.ForestBandits, isBossParty);
        }

        /// <summary>
        /// Creates a mountain bandit party at a hideout.
        /// </summary>
        public static MobileParty CreateMountainBanditParty(Hideout hideout, bool isBossParty = false)
        {
            return CreateBanditPartyForCulture(hideout, CultureLookup.MountainBandits, isBossParty);
        }

        /// <summary>
        /// Creates a sea raider party at a hideout.
        /// </summary>
        public static MobileParty CreateSeaRaiderParty(Hideout hideout, bool isBossParty = false)
        {
            return CreateBanditPartyForCulture(hideout, CultureLookup.SeaRaiders, isBossParty);
        }

        /// <summary>
        /// Creates a steppe bandit party at a hideout.
        /// </summary>
        public static MobileParty CreateSteppeBanditParty(Hideout hideout, bool isBossParty = false)
        {
            return CreateBanditPartyForCulture(hideout, CultureLookup.SteppeBandits, isBossParty);
        }

        private static MobileParty CreateBanditPartyForCulture(Hideout hideout, CultureObject banditCulture, bool isBossParty)
        {
            if (hideout == null)
            {
                BLGMResult.Error("CreateBanditPartyForCulture() failed, hideout cannot be null", new ArgumentNullException(nameof(hideout))).Log();
                return null;
            }

            // Find the bandit clan for this culture
            Clan banditClan = Clan.BanditFactions.FirstOrDefault(c => c.Culture == banditCulture);

            // Fall back to hideout's owning clan
            banditClan ??= hideout.Settlement.OwnerClan;

            return CreateBanditParty(hideout, banditClan, isBossParty);
        }

        #endregion
    }

    /// <summary>
    /// Types of parties that can be created from settlements.
    /// </summary>
    public enum SettlementPartyType
    {
        /// <summary>Villager party from a village</summary>
        Villager,

        /// <summary>Patrol/guard party from a fortification</summary>
        Patrol,

        /// <summary>Militia party from a fortification</summary>
        Militia
    }
}
