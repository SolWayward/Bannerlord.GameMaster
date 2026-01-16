using System;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Party;
using Bannerlord.GameMaster.Common.Interfaces;
using Bannerlord.GameMaster.Console.Common;
using Bannerlord.GameMaster.Console.Common.Formatting;

namespace Bannerlord.GameMaster.Kingdoms
{
	public static class KingdomQueries
    {
        /// <summary>
        /// Finds a kingdom with the specified kingdomId, using a case-insensitive comparison.
        /// </summary>
        public static Kingdom GetKingdomById(string kingdomId)
        {
            return Kingdom.All.FirstOrDefault(k => k.StringId.Equals(kingdomId, StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// Main unified method to find kingdoms by search string and type flags
        /// </summary>
        /// <param name="query">Optional case-insensitive substring to filter by name or ID</param>
        /// <param name="requiredTypes">Kingdom type flags to match</param>
        /// <param name="matchAll">If true, kingdom must have ALL flags. If false, kingdom must have ANY flag</param>
        /// <param name="sortBy">Sort field (id, name, clans, heroes, fiefs, strength, or any KingdomType flag)</param>
        /// <param name="sortDescending">True for descending, false for ascending</param>
        /// <returns>List of kingdoms matching all criteria</returns>
        public static List<Kingdom> QueryKingdoms(
            string query = "",
            KingdomTypes requiredTypes = KingdomTypes.None,
            bool matchAll = true,
            string sortBy = "id",
            bool sortDescending = false)
        {
            IEnumerable<Kingdom> kingdoms = Kingdom.All;

            // Filter by name/ID if provided
            if (!string.IsNullOrEmpty(query))
            {
                string lowerFilter = query.ToLower();
                kingdoms = kingdoms.Where(k =>
                    k.Name.ToString().ToLower().Contains(lowerFilter) ||
                    k.StringId.ToLower().Contains(lowerFilter));
            }

            // Filter by kingdom types
            if (requiredTypes != KingdomTypes.None)
            {
                kingdoms = kingdoms.Where(k => matchAll ? k.HasAllTypes(requiredTypes) : k.HasAnyType(requiredTypes));
            }

            // Apply sorting
            kingdoms = ApplySorting(kingdoms, sortBy, sortDescending);

            return kingdoms.ToList();
        }

        /// <summary>
        /// Apply sorting to kingdoms collection
        /// </summary>
        private static IEnumerable<Kingdom> ApplySorting(
            IEnumerable<Kingdom> kingdoms,
            string sortBy,
            bool descending)
        {
            sortBy = sortBy.ToLower();

            // Check if sortBy matches a KingdomType flag
            if (Enum.TryParse<KingdomTypes>(sortBy, true, out var kingdomType) && kingdomType != KingdomTypes.None)
            {
                // Sort by whether kingdom has this type flag
                return descending
                    ? kingdoms.OrderByDescending(k => k.GetKingdomTypes().HasFlag(kingdomType))
                    : kingdoms.OrderBy(k => k.GetKingdomTypes().HasFlag(kingdomType));
            }

            // Sort by standard fields
            IOrderedEnumerable<Kingdom> orderedKingdoms = sortBy switch
            {
                "name" => descending
                    ? kingdoms.OrderByDescending(k => k.Name.ToString())
                    : kingdoms.OrderBy(k => k.Name.ToString()),
                "clans" => descending
                    ? kingdoms.OrderByDescending(k => k.Clans.Count)
                    : kingdoms.OrderBy(k => k.Clans.Count),
                "heroes" => descending
                    ? kingdoms.OrderByDescending(k => k.Heroes.Count())
                    : kingdoms.OrderBy(k => k.Heroes.Count()),
                "fiefs" => descending
                    ? kingdoms.OrderByDescending(k => k.Fiefs.Count)
                    : kingdoms.OrderBy(k => k.Fiefs.Count),
                "strength" => descending
                    ? kingdoms.OrderByDescending(k => k.CurrentTotalStrength)
                    : kingdoms.OrderBy(k => k.CurrentTotalStrength),
                "ruler" => descending
                    ? kingdoms.OrderByDescending(k => k.Leader?.Name?.ToString() ?? "")
                    : kingdoms.OrderBy(k => k.Leader?.Name?.ToString() ?? ""),
                _ => descending  // default to id
                    ? kingdoms.OrderByDescending(k => k.StringId)
                    : kingdoms.OrderBy(k => k.StringId)
            };

            return orderedKingdoms;
        }

        /// <summary>
        /// Parse a string into KingdomTypes enum value
        /// </summary>
        public static KingdomTypes ParseKingdomType(string typeString)
        {
            var normalizedType = typeString.ToLower() switch
            {
                "active" => "Active",
                "eliminated" => "Eliminated",
                "empty" => "Empty",
                "player" => "PlayerKingdom",
                "playerkingdom" => "PlayerKingdom",
                "atwar" => "AtWar",
                "war" => "AtWar",
                "hasallies" => "HasAllies",
                "allies" => "HasAllies",
                "allied" => "HasAllies",
                "hasenemies" => "HasEnemies",
                "enemies" => "HasEnemies",
                _ => typeString
            };

            if (Enum.TryParse<KingdomTypes>(normalizedType, true, out var result))
                return result;
            return KingdomTypes.None;
        }

        /// <summary>
        /// Parse multiple strings and combine into KingdomTypes flags
        /// </summary>
        public static KingdomTypes ParseKingdomTypes(IEnumerable<string> typeStrings)
        {
            KingdomTypes combined = KingdomTypes.None;
            foreach (var typeString in typeStrings)
            {
                var parsed = ParseKingdomType(typeString);
                if (parsed != KingdomTypes.None)
                    combined |= parsed;
            }
            return combined;
        }

        /// <summary>
        /// Returns a formatted string listing kingdom details with aligned columns
        /// </summary>
        public static string GetFormattedDetails(List<Kingdom> kingdoms)
        {
            if (kingdoms.Count == 0)
                return "";

            return ColumnFormatter<Kingdom>.FormatList(
                kingdoms,
                k => k.StringId,
                k => k.Name.ToString(),
                k => $"Clans: {k.Clans.Count}",
                k => $"Heroes: {k.Heroes.Count()}",
                k => $"RulingClan: {k.RulingClan?.Name?.ToString() ?? "None"}",
                k => $"Ruler: {k.Leader?.Name?.ToString() ?? "None"}"
            );
        }

        /// <summary>
        /// Get all clan leaders for a specific kingdom
        /// </summary>
        public static List<Hero> GetClanLeaders(Kingdom kingdom)
        {
            if (kingdom == null)
                return new List<Hero>();

            return kingdom.Clans
                .Where(c => c.Leader != null)
                .Select(c => c.Leader)
                .ToList();
        }

        /// <summary>
        /// Get all party leaders for a specific kingdom
        /// </summary>
        public static List<Hero> GetPartyLeaders(Kingdom kingdom)
        {
            if (kingdom == null)
                return new List<Hero>();

            return kingdom.AllParties
                .Where(p => p.LeaderHero != null)
                .Select(p => p.LeaderHero)
                .ToList();
        }

        /// <summary>
        /// Get all heroes in a specific kingdom
        /// </summary>
        public static List<Hero> GetHeroes(Kingdom kingdom)
        {
            if (kingdom == null)
                return new List<Hero>();

            return kingdom.Heroes.ToList();
        }
 }

 /// <summary>
 /// Wrapper class implementing IEntityQueries interface for Kingdom entities
 /// </summary>
 public class KingdomQueriesWrapper : IEntityQueries<Kingdom, KingdomTypes>
 {
  public Kingdom GetById(string id) => KingdomQueries.GetKingdomById(id);
  public List<Kingdom> Query(string query, KingdomTypes types, bool matchAll) => KingdomQueries.QueryKingdoms(query, types, matchAll);
  public KingdomTypes ParseType(string typeString) => KingdomQueries.ParseKingdomType(typeString);
  public KingdomTypes ParseTypes(IEnumerable<string> typeStrings) => KingdomQueries.ParseKingdomTypes(typeStrings);
  public string GetFormattedDetails(List<Kingdom> entities) => KingdomQueries.GetFormattedDetails(entities);
 }
}