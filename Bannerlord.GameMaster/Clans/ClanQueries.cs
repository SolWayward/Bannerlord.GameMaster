using System;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Party;
using Bannerlord.GameMaster.Common.Interfaces;

namespace Bannerlord.GameMaster.Clans
{
	public static class ClanQueries
    {
        /// <summary>
        /// Finds a clan with the specified clanId, using a case-insensitive comparison.
        /// </summary>
        public static Clan GetClanById(string clanId)
        {
            return Clan.FindFirst(c => c.StringId.Equals(clanId, StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// Main unified method to find clans by search string and type flags
        /// </summary>
        /// <param name="query">Optional case-insensitive substring to filter by name or ID</param>
        /// <param name="requiredTypes">Clan type flags to match</param>
        /// <param name="matchAll">If true, clan must have ALL flags. If false, clan must have ANY flag</param>
        /// <returns>List of clans matching all criteria</returns>
        public static List<Clan> QueryClans(
            string query = "",
            ClanTypes requiredTypes = ClanTypes.None,
            bool matchAll = true)
        {
            IEnumerable<Clan> clans = Clan.All;

            // Filter by name/ID if provided
            if (!string.IsNullOrEmpty(query))
            {
                string lowerFilter = query.ToLower();
                clans = clans.Where(c =>
                    c.Name.ToString().ToLower().Contains(lowerFilter) ||
                    c.StringId.ToLower().Contains(lowerFilter));
            }

            // Filter by clan types
            if (requiredTypes != ClanTypes.None)
            {
                clans = clans.Where(c => matchAll ? c.HasAllTypes(requiredTypes) : c.HasAnyType(requiredTypes));
            }

            return clans.ToList();
        }

        /// <summary>
        /// Parse a string into ClanTypes enum value
        /// </summary>
        public static ClanTypes ParseClanType(string typeString)
        {
            // Handle special cases and aliases
            var normalizedType = typeString.ToLower() switch
            {
                "active" => "Active",
                "eliminated" => "Eliminated",
                "bandit" => "Bandit",
                "nonbandit" => "NonBandit",
                "mapfaction" => "MapFaction",
                "noble" => "Noble",
                "minor" => "MinorFaction",
                "minorfaction" => "MinorFaction",
                "rebel" => "Rebel",
                "mercenary" => "Mercenary",
                "merc" => "Mercenary",
                "undermercenaryservice" => "UnderMercenaryService",
                "mafia" => "Mafia",
                "outlaw" => "Outlaw",
                "nomad" => "Nomad",
                "sect" => "Sect",
                "withoutkingdom" => "WithoutKingdom",
                "empty" => "Empty",
                "player" => "PlayerClan",
                "playerclan" => "PlayerClan",
                _ => typeString
            };

            if (Enum.TryParse<ClanTypes>(normalizedType, true, out var result))
                return result;
            return ClanTypes.None;
        }

        /// <summary>
        /// Parse multiple strings and combine into ClanTypes flags
        /// </summary>
        public static ClanTypes ParseClanTypes(IEnumerable<string> typeStrings)
        {
            ClanTypes combined = ClanTypes.None;
            foreach (var typeString in typeStrings)
            {
                var parsed = ParseClanType(typeString);
                if (parsed != ClanTypes.None)
                    combined |= parsed;
            }
            return combined;
        }

        /// <summary>
        /// Returns a formatted string listing clan details
        /// </summary>
        public static string GetFormattedDetails(List<Clan> clans)
        {
            if (clans.Count == 0)
                return "";
            return string.Join("\n", clans.Select(c => c.FormattedDetails())) + "\n";
        }

        /// <summary>
        /// Get all party leaders for a specific clan
        /// </summary>
        public static List<Hero> GetPartyLeaders(Clan clan)
        {
            return MobileParty.All
                .Where(p => p.LeaderHero != null && p.LeaderHero.Clan == clan)
                .Select(p => p.LeaderHero)
                .ToList();
        }
 }

 /// <summary>
 /// Wrapper class implementing IEntityQueries interface for Clan entities
 /// </summary>
 public class ClanQueriesWrapper : IEntityQueries<Clan, ClanTypes>
 {
  public Clan GetById(string id) => ClanQueries.GetClanById(id);
  public List<Clan> Query(string query, ClanTypes types, bool matchAll) => ClanQueries.QueryClans(query, types, matchAll);
  public ClanTypes ParseType(string typeString) => ClanQueries.ParseClanType(typeString);
  public ClanTypes ParseTypes(IEnumerable<string> typeStrings) => ClanQueries.ParseClanTypes(typeStrings);
  public string GetFormattedDetails(List<Clan> entities) => ClanQueries.GetFormattedDetails(entities);
 }
}