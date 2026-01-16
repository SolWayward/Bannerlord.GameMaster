using System;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Party;
using Bannerlord.GameMaster.Common.Interfaces;
using Bannerlord.GameMaster.Console.Common;
using Bannerlord.GameMaster.Console.Common.Formatting;

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
        /// <param name="sortBy">Sort field (id, name, tier, gold, renown, kingdom, or any ClanType flag)</param>
        /// <param name="sortDescending">True for descending, false for ascending</param>
        /// <returns>List of clans matching all criteria</returns>
        public static List<Clan> QueryClans(
            string query = "",
            ClanTypes requiredTypes = ClanTypes.None,
            bool matchAll = true,
            string sortBy = "id",
            bool sortDescending = false)
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

            // Apply sorting
            clans = ApplySorting(clans, sortBy, sortDescending);

            return clans.ToList();
        }

        /// <summary>
        /// Apply sorting to clans collection
        /// </summary>
        private static IEnumerable<Clan> ApplySorting(
            IEnumerable<Clan> clans,
            string sortBy,
            bool descending)
        {
            sortBy = sortBy.ToLower();

            // Check if sortBy matches a ClanType flag
            if (Enum.TryParse<ClanTypes>(sortBy, true, out var clanType) && clanType != ClanTypes.None)
            {
                // Sort by whether clan has this type flag
                return descending
                    ? clans.OrderByDescending(c => c.GetClanTypes().HasFlag(clanType))
                    : clans.OrderBy(c => c.GetClanTypes().HasFlag(clanType));
            }

            // Sort by standard fields
            IOrderedEnumerable<Clan> orderedClans = sortBy switch
            {
                "name" => descending
                    ? clans.OrderByDescending(c => c.Name.ToString())
                    : clans.OrderBy(c => c.Name.ToString()),
                "tier" => descending
                    ? clans.OrderByDescending(c => c.Tier)
                    : clans.OrderBy(c => c.Tier),
                "gold" => descending
                    ? clans.OrderByDescending(c => c.Gold)
                    : clans.OrderBy(c => c.Gold),
                "renown" => descending
                    ? clans.OrderByDescending(c => c.Renown)
                    : clans.OrderBy(c => c.Renown),
                "kingdom" => descending
                    ? clans.OrderByDescending(c => c.Kingdom?.Name?.ToString() ?? "")
                    : clans.OrderBy(c => c.Kingdom?.Name?.ToString() ?? ""),
                "heroes" => descending
                    ? clans.OrderByDescending(c => c.Heroes.Count)
                    : clans.OrderBy(c => c.Heroes.Count),
                _ => descending  // default to id
                    ? clans.OrderByDescending(c => c.StringId)
                    : clans.OrderBy(c => c.StringId)
            };

            return orderedClans;
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
        /// Returns a formatted string listing clan details with aligned columns
        /// </summary>
        public static string GetFormattedDetails(List<Clan> clans)
        {
        	if (clans.Count == 0)
        		return "";
      
        	return ColumnFormatter<Clan>.FormatList(
        		clans,
        		c => c.StringId,
        		c => c.Name.ToString(),
        		c => $"Culture: {c.Culture?.Name?.ToString() ?? "None"}",
        		c => $"Heroes: {c.Heroes.Count()}",
        		c => $"Leader: {c.Leader?.Name?.ToString() ?? "None"}",
        		c => $"Kingdom: {c.Kingdom?.Name?.ToString() ?? "None"}"
        	);
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