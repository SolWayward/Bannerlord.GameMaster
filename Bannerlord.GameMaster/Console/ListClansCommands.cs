using Bannerlord.GameMaster.Clans;
using System;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Library;

namespace Bannerlord.GameMaster.Console
{
    [CommandLineFunctionality.CommandLineArgumentFunction("clan", "gm")]
    public static class ListClansCommands
    {
        /// <summary>
        /// Parse command arguments into search filter and clan type flags
        /// </summary>
        private static (string searchFilter, ClanTypes types) ParseArguments(List<string> args)
        {
            if (args == null || args.Count == 0)
                return ("", ClanTypes.Active);

            var typeKeywords = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "active", "eliminated", "bandit", "nonbandit", "mapfaction", "noble",
                "minor", "minorfaction", "rebel", "mercenary", "merc", "undermercenaryservice",
                "mafia", "outlaw", "nomad", "sect", "withoutkingdom", "empty", "player", "playerclan"
            };

            List<string> searchTerms = new();
            List<string> typeTerms = new();

            foreach (var arg in args)
            {
                if (typeKeywords.Contains(arg.ToLower()))
                    typeTerms.Add(arg);
                else
                    searchTerms.Add(arg);
            }

            string searchFilter = string.Join(" ", searchTerms).Trim();
            ClanTypes types = Clans.ClanFinder.ParseClanTypes(typeTerms);

            // Default to Active if no status specified
            if (!types.HasFlag(ClanTypes.Active) && !types.HasFlag(ClanTypes.Eliminated))
            {
                types |= ClanTypes.Active;
            }

            return (searchFilter, types);
        }

        /// <summary>
        /// Helper to build a readable criteria string
        /// </summary>
        private static string BuildCriteriaString(string searchFilter, ClanTypes types)
        {
            List<string> parts = new();

            if (!string.IsNullOrEmpty(searchFilter))
                parts.Add($"search: '{searchFilter}'");

            if (types != ClanTypes.None)
            {
                var typeList = Enum.GetValues(typeof(ClanTypes))
                    .Cast<ClanTypes>()
                    .Where(t => t != ClanTypes.None && types.HasFlag(t))
                    .Select(t => t.ToString().ToLower());
                parts.Add($"types: {string.Join(", ", typeList)}");
            }

            return parts.Count > 0 ? string.Join(", ", parts) : "all clans";
        }

        /// <summary>
        /// Unified clan finding command
        /// Usage: gm.clan.find [search terms] [type keywords]
        /// Example: gm.clan.find empire noble
        /// Example: gm.clan.find bandit
        /// Example: gm.clan.find eliminated empty
        /// </summary>
        [CommandLineFunctionality.CommandLineArgumentFunction("find", "gm.clan")]
        public static string FindClans(List<string> args)
        {
            if (Campaign.Current == null)
                return "Error: Must be in campaign mode.\n";

            var (searchFilter, types) = ParseArguments(args);
            List<Clan> matchedClans = Clans.ClanFinder.FindClans(searchFilter, types, matchAll: true);

            if (matchedClans.Count == 0)
            {
                string criteria = BuildCriteriaString(searchFilter, types);
                return $"No clans found matching criteria: {criteria}\n" +
                       "Usage: gm.clan.find [search] [type keywords]\n" +
                       "Type keywords: noble, minor, bandit, mercenary, eliminated, empty, etc.\n" +
                       "Example: gm.clan.find empire noble\n";
            }

            string criteriaDesc = BuildCriteriaString(searchFilter, types);
            return $"Found {matchedClans.Count} clan(s) matching {criteriaDesc}:\n" +
                   $"{Clans.ClanFinder.GetFormattedDetails(matchedClans)}";
        }

        /// <summary>
        /// Find clans matching ANY of the specified types (OR logic)
        /// Usage: gm.clan.find_any [search terms] [type keywords]
        /// </summary>
        [CommandLineFunctionality.CommandLineArgumentFunction("find_any", "gm.clan")]
        public static string FindClansAny(List<string> args)
        {
            if (Campaign.Current == null)
                return "Error: Must be in campaign mode.\n";

            var (searchFilter, types) = ParseArguments(args);
            List<Clan> matchedClans = Clans.ClanFinder.FindClans(searchFilter, types, matchAll: false);

            if (matchedClans.Count == 0)
            {
                string criteria = BuildCriteriaString(searchFilter, types);
                return $"No clans found matching ANY of: {criteria}\n" +
                       "Usage: gm.clan.find_any [search] [type keywords]\n" +
                       "Example: gm.clan.find_any bandit outlaw (finds bandits OR outlaws)\n";
            }

            string criteriaDesc = BuildCriteriaString(searchFilter, types);
            return $"Found {matchedClans.Count} clan(s) matching ANY of {criteriaDesc}:\n" +
                   $"{Clans.ClanFinder.GetFormattedDetails(matchedClans)}";
        }

        /// <summary>
        /// Simple list all clans with optional search filter
        /// </summary>
        [CommandLineFunctionality.CommandLineArgumentFunction("list", "gm.clan")]
        public static string ListAllClans(List<string> args)
        {
            if (Campaign.Current == null)
                return "Error: Must be in campaign mode.\n";

            string searchFilter = args != null && args.Count > 0 ? string.Join(" ", args) : "";
            List<Clan> matchedClans = Clans.ClanFinder.FindClans(searchFilter, ClanTypes.Active);

            if (matchedClans.Count == 0)
            {
                return string.IsNullOrEmpty(searchFilter)
                    ? "No clans found.\n"
                    : $"No clans found matching '{searchFilter}'.\n";
            }

            return $"Found {matchedClans.Count} clan(s)" +
                   (string.IsNullOrEmpty(searchFilter) ? "" : $" matching '{searchFilter}'") + ":\n" +
                   $"{Clans.ClanFinder.GetFormattedDetails(matchedClans)}";
        }

        /// <summary>
        /// Get detailed info about a specific clan by ID
        /// </summary>
        [CommandLineFunctionality.CommandLineArgumentFunction("info", "gm.clan")]
        public static string GetClanInfo(List<string> args)
        {
            if (Campaign.Current == null)
                return "Error: Must be in campaign mode.\n";

            if (args == null || args.Count == 0)
                return "Error: Please provide a clan ID.\nUsage: gm.clan.info <clanId>\n";

            string clanId = args[0];
            Clan clan = Clans.ClanFinder.GetClanById(clanId);

            if (clan == null)
                return $"Error: Clan with ID '{clanId}' not found.\n";

            var types = clan.GetClanTypes();
            string kingdomName = clan.Kingdom?.Name?.ToString() ?? "None";
            string leaderName = clan.Leader?.Name?.ToString() ?? "None";

            return $"Clan Information:\n" +
                   $"ID: {clan.StringId}\n" +
                   $"Name: {clan.Name}\n" +
                   $"Leader: {leaderName}\n" +
                   $"Kingdom: {kingdomName}\n" +
                   $"Total Heroes: {clan.Heroes.Count}\n" +
                   $"Lords: {clan.Heroes.Count}\n" +
                   $"Companions: {clan.Companions.Count()}\n" +
                   $"Gold: {clan.Gold}\n" +
                   $"Tier: {clan.Tier}\n" +
                   $"Renown: {clan.Renown:F0}\n" +
                   $"Types: {types}\n" +
                   $"Is Eliminated: {clan.IsEliminated}\n";
        }
    }
}