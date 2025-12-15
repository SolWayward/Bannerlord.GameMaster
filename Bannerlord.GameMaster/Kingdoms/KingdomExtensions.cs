using System;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using Bannerlord.GameMaster.Common.Interfaces;

namespace Bannerlord.GameMaster.Kingdoms
{
	[Flags]
	public enum KingdomTypes
    {
        None = 0,
        Active = 1,
        Eliminated = 2,
        Empty = 4,
        PlayerKingdom = 8,
        AtWar = 16,
    }

    public static class KingdomExtensions
    {
        /// <summary>
        /// Gets all kingdom type flags for this kingdom
        /// </summary>
        public static KingdomTypes GetKingdomTypes(this Kingdom kingdom)
        {
            KingdomTypes types = KingdomTypes.None;

            if (kingdom.IsEliminated)
                types |= KingdomTypes.Eliminated;
            else
                types |= KingdomTypes.Active;

            // Check if kingdom has no clans or heroes
            if (kingdom.Clans.Count == 0 || kingdom.Heroes.Count() == 0)
                types |= KingdomTypes.Empty;

            if (kingdom == Hero.MainHero.MapFaction as Kingdom)
                types |= KingdomTypes.PlayerKingdom;

            // Check if at war with anyone
            bool atWar = false;
            foreach (var otherKingdom in Kingdom.All)
            {
                if (otherKingdom != kingdom && FactionManager.IsAtWarAgainstFaction(kingdom, otherKingdom))
                {
                    atWar = true;
                    break;
                }
            }
            if (atWar)
                types |= KingdomTypes.AtWar;

            return types;
        }

        /// <summary>
        /// Checks if kingdom has ALL specified flags
        /// </summary>
        public static bool HasAllTypes(this Kingdom kingdom, KingdomTypes types)
        {
            if (types == KingdomTypes.None) return true;
            var kingdomTypes = kingdom.GetKingdomTypes();
            return (kingdomTypes & types) == types;
        }

        /// <summary>
        /// Checks if kingdom has ANY of the specified flags
        /// </summary>
        public static bool HasAnyType(this Kingdom kingdom, KingdomTypes types)
        {
            if (types == KingdomTypes.None) return true;
            var kingdomTypes = kingdom.GetKingdomTypes();
            return (kingdomTypes & types) != KingdomTypes.None;
        }

        /// <summary>
        /// Returns a formatted string containing the kingdom's details
        /// </summary>
        public static string FormattedDetails(this Kingdom kingdom)
        {
            int heroCount = kingdom.Heroes.Count();
            return $"{kingdom.StringId}\t{kingdom.Name}\tClans: {kingdom.Clans.Count}\tHeroes: {heroCount}\t" +
                   $"RulingClan: {kingdom.RulingClan?.Name}\tRuler: {kingdom.Leader?.Name}";
        }

        /// <summary>
        /// Alias for GetKingdomTypes to match IEntityExtensions interface
        /// </summary>
        public static KingdomTypes GetTypes(this Kingdom kingdom) => kingdom.GetKingdomTypes();
 }

 /// <summary>
 /// Wrapper class implementing IEntityExtensions interface for Kingdom entities
 /// </summary>
 public class KingdomExtensionsWrapper : IEntityExtensions<Kingdom, KingdomTypes>
 {
  public KingdomTypes GetTypes(Kingdom entity) => entity.GetKingdomTypes();
  public bool HasAllTypes(Kingdom entity, KingdomTypes types) => entity.HasAllTypes(types);
  public bool HasAnyType(Kingdom entity, KingdomTypes types) => entity.HasAnyType(types);
  public string FormattedDetails(Kingdom entity) => entity.FormattedDetails();
 }
}