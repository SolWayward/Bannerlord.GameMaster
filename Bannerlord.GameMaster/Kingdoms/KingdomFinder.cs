using System;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Party;

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
        /// <param name="searchFilter">Optional case-insensitive substring to filter by name or ID</param>
        /// <param name="requiredTypes">Kingdom type flags to match</param>
        /// <param name="matchAll">If true, kingdom must have ALL flags. If false, kingdom must have ANY flag</param>
        /// <returns>List of kingdoms matching all criteria</returns>
        public static List<Kingdom> FindKingdoms(
            string searchFilter = "",
            KingdomTypes requiredTypes = KingdomTypes.None,
            bool matchAll = true)
        {
            IEnumerable<Kingdom> kingdoms = Kingdom.All;

            // Filter by name/ID if provided
            if (!string.IsNullOrEmpty(searchFilter))
            {
                string lowerFilter = searchFilter.ToLower();
                kingdoms = kingdoms.Where(k =>
                    k.Name.ToString().ToLower().Contains(lowerFilter) ||
                    k.StringId.ToLower().Contains(lowerFilter));
            }

            // Filter by kingdom types
            if (requiredTypes != KingdomTypes.None)
            {
                kingdoms = kingdoms.Where(k => matchAll ? k.HasAllTypes(requiredTypes) : k.HasAnyType(requiredTypes));
            }

            return kingdoms.ToList();
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
        /// Returns a formatted string listing kingdom details
        /// </summary>
        public static string GetFormattedDetails(List<Kingdom> kingdoms)
        {
            if (kingdoms.Count == 0)
                return "";
            return string.Join("\n", kingdoms.Select(k => k.FormattedDetails())) + "\n";
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
}