using System;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Party;

namespace Bannerlord.GameMaster.Clans
{
    public static class ClanFinder
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
        /// <param name="searchFilter">Optional case-insensitive substring to filter by name or ID</param>
        /// <param name="requiredTypes">Clan type flags to match</param>
        /// <param name="matchAll">If true, clan must have ALL flags. If false, clan must have ANY flag</param>
        /// <returns>List of clans matching all criteria</returns>
        public static List<Clan> FindClans(
            string searchFilter = "",
            ClanTypes requiredTypes = ClanTypes.None,
            bool matchAll = true)
        {
            IEnumerable<Clan> clans = Clan.All;

            // Filter by name/ID if provided
            if (!string.IsNullOrEmpty(searchFilter))
            {
                string lowerFilter = searchFilter.ToLower();
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

        #region Legacy Methods (for backward compatibility)

        public static List<Clan> GetAllClans(string filter = "", bool includeEliminated = false)
        {
            var types = includeEliminated ? ClanTypes.None : ClanTypes.Active;
            return FindClans(filter, types);
        }

        public static List<Clan> GetNonBanditClans(string filter = "", bool includeEliminated = false)
        {
            var types = ClanTypes.NonBandit | (includeEliminated ? ClanTypes.None : ClanTypes.Active);
            return FindClans(filter, types);
        }

        public static List<Clan> GetBanditClans(string filter = "", bool includeEliminated = false)
        {
            var types = ClanTypes.Bandit | (includeEliminated ? ClanTypes.None : ClanTypes.Active);
            return FindClans(filter, types);
        }

        public static List<Clan> GetNobleClans(bool includeEliminated = false)
            => FindClans("", ClanTypes.Noble | (includeEliminated ? ClanTypes.None : ClanTypes.Active));

        public static List<Clan> GetMinorClans(bool includeEliminated = false)
            => FindClans("", ClanTypes.MinorFaction | (includeEliminated ? ClanTypes.None : ClanTypes.Active));

        public static List<Clan> GetRebelClans(bool includeEliminated = false)
            => FindClans("", ClanTypes.Rebel | (includeEliminated ? ClanTypes.None : ClanTypes.Active));

        public static List<Clan> GetMercenaryTypeClans(bool includeEliminated = false)
            => FindClans("", ClanTypes.Mercenary | (includeEliminated ? ClanTypes.None : ClanTypes.Active));

        public static List<Clan> GetClansUnderMercenaryService(bool includeEliminated = false)
            => FindClans("", ClanTypes.UnderMercenaryService | (includeEliminated ? ClanTypes.None : ClanTypes.Active));

        public static List<Clan> GetMafiaClans(bool includeEliminated = false)
            => FindClans("", ClanTypes.Mafia | (includeEliminated ? ClanTypes.None : ClanTypes.Active));

        public static List<Clan> GetOutlawClans(bool includeEliminated = false)
            => FindClans("", ClanTypes.Outlaw | (includeEliminated ? ClanTypes.None : ClanTypes.Active));

        public static List<Clan> GetNomadClans(bool includeEliminated = false)
            => FindClans("", ClanTypes.Nomad | (includeEliminated ? ClanTypes.None : ClanTypes.Active));

        public static List<Clan> GetSectClans(bool includeEliminated = false)
            => FindClans("", ClanTypes.Sect | (includeEliminated ? ClanTypes.None : ClanTypes.Active));

        public static List<Clan> GetClansWithoutKingdom(bool includeEliminated = false)
            => FindClans("", ClanTypes.WithoutKingdom | (includeEliminated ? ClanTypes.None : ClanTypes.Active));

        public static List<Clan> GetEliminatedClans()
            => FindClans("", ClanTypes.Eliminated);

        public static List<Clan> GetEmptyClans(bool includeEliminated = false)
            => FindClans("", ClanTypes.Empty | (includeEliminated ? ClanTypes.None : ClanTypes.Active));

        public static List<Clan> MapFactionClans(bool includeEliminated = false)
            => FindClans("", ClanTypes.MapFaction | (includeEliminated ? ClanTypes.None : ClanTypes.Active));

        #endregion
    }
}