using System;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using Bannerlord.GameMaster.Common.Interfaces;

namespace Bannerlord.GameMaster.Clans
{
	[Flags]
	public enum ClanTypes
    {
        None = 0,
        Active = 1,
        Eliminated = 2,
        Bandit = 4,
        NonBandit = 8,
        MapFaction = 16,
        Noble = 32,
        MinorFaction = 64,
        Rebel = 128,
        Mercenary = 256,
        UnderMercenaryService = 512,
        Mafia = 1024,
        Outlaw = 2048,
        Nomad = 4096,
        Sect = 8192,
        WithoutKingdom = 16384,
        Empty = 32768,
        PlayerClan = 65536,
    }

    public static class ClanExtensions
    {
        /// <summary>
        /// Gets all clan type flags for this clan
        /// </summary>
        public static ClanTypes GetClanTypes(this Clan clan)
        {
            ClanTypes types = ClanTypes.None;

            if (clan.IsEliminated)
                types |= ClanTypes.Eliminated;
            else
                types |= ClanTypes.Active;

            if (clan.IsBanditFaction) types |= ClanTypes.Bandit;
            if (!clan.IsBanditFaction) types |= ClanTypes.NonBandit;
            if (clan.IsMapFaction) types |= ClanTypes.MapFaction;
            if (clan.IsNoble) types |= ClanTypes.Noble;
            if (clan.IsMinorFaction) types |= ClanTypes.MinorFaction;
            if (clan.IsRebelClan) types |= ClanTypes.Rebel;
            if (clan.IsClanTypeMercenary) types |= ClanTypes.Mercenary;
            if (clan.IsUnderMercenaryService) types |= ClanTypes.UnderMercenaryService;
            if (clan.IsMafia) types |= ClanTypes.Mafia;
            if (clan.IsOutlaw) types |= ClanTypes.Outlaw;
            if (clan.IsNomad) types |= ClanTypes.Nomad;
            if (clan.IsSect) types |= ClanTypes.Sect;
            if (clan.Kingdom == null) types |= ClanTypes.WithoutKingdom;
            if (clan.Heroes.Count == 0) types |= ClanTypes.Empty;
            if (clan == Clan.PlayerClan) types |= ClanTypes.PlayerClan;

            return types;
        }

        /// <summary>
        /// Checks if clan has ALL specified flags
        /// </summary>
        public static bool HasAllTypes(this Clan clan, ClanTypes types)
        {
            if (types == ClanTypes.None) return true;
            var clanTypes = clan.GetClanTypes();
            return (clanTypes & types) == types;
        }

        /// <summary>
        /// Checks if clan has ANY of the specified flags
        /// </summary>
        public static bool HasAnyType(this Clan clan, ClanTypes types)
        {
            if (types == ClanTypes.None) return true;
            var clanTypes = clan.GetClanTypes();
            return (clanTypes & types) != ClanTypes.None;
        }

        /// <summary>
        /// Returns a formatted string containing the clan's details
        /// </summary>
        public static string FormattedDetails(this Clan clan)
        {
            return $"{clan.StringId}\t{clan.Name}\tHeroes: {clan.Heroes.Count()}\tLeader: {clan.Leader?.Name}\tKingdom: {clan.Kingdom?.Name}";
        }

        /// <summary>
        /// Alias for GetClanTypes to match IEntityExtensions interface
        /// </summary>
        public static ClanTypes GetTypes(this Clan clan) => clan.GetClanTypes();
 }

 /// <summary>
 /// Wrapper class implementing IEntityExtensions interface for Clan entities
 /// </summary>
 public class ClanExtensionsWrapper : IEntityExtensions<Clan, ClanTypes>
 {
  public ClanTypes GetTypes(Clan entity) => entity.GetClanTypes();
  public bool HasAllTypes(Clan entity, ClanTypes types) => entity.HasAllTypes(types);
  public bool HasAnyType(Clan entity, ClanTypes types) => entity.HasAnyType(types);
  public string FormattedDetails(Clan entity) => entity.FormattedDetails();
 }
}