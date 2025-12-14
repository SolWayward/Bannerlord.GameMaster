using Bannerlord.GameMaster.Clans;
using Bannerlord.GameMaster.Console.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Library;

namespace Bannerlord.GameMaster.Console.Query
{
    [CommandLineFunctionality.CommandLineArgumentFunction("query", "gm")]
    public static class ClanQueryCommands
    {
        /// <summary>
        /// Parse command arguments into search filter and clan type flags
        /// </summary>
        private static (string query, ClanTypes types) ParseArguments(List<string> args)
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

            string query = string.Join(" ", searchTerms).Trim();
            ClanTypes types = ClanQueries.ParseClanTypes(typeTerms);

            // Default to Active if no status specified
            if (!types.HasFlag(ClanTypes.Active) && !types.HasFlag(ClanTypes.Eliminated))
            {
                types |= ClanTypes.Active;
            }

            return (query, types);
        }

        /// <summary>
        /// Helper to build a readable criteria string
        /// </summary>
        private static string BuildCriteriaString(string query, ClanTypes types)
        {
            List<string> parts = new();

            if (!string.IsNullOrEmpty(query))
                parts.Add($"search: '{query}'");

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
        /// Usage: gm.query.clan [search terms] [type keywords]
        /// Example: gm.query.clan empire noble
        /// Example: gm.query.clan bandit
        /// Example: gm.query.clan eliminated empty
        /// </summary>
        [CommandLineFunctionality.CommandLineArgumentFunction("clan", "gm.query")]
        public static string QueryClans(List<string> args)
        {
            return Cmd.Run(args, () =>
            {
                if (Campaign.Current == null)
                    return "Error: Must be in campaign mode.\n";

                var (query, types) = ParseArguments(args);
                List<Clan> matchedClans = ClanQueries.QueryClans(query, types, matchAll: true);

                string criteriaDesc = BuildCriteriaString(query, types);
                
                if (matchedClans.Count == 0)
                {
                    return $"Found 0 clan(s) matching {criteriaDesc}\n" +
                           "Usage: gm.query.clan [search] [type keywords]\n" +
                           "Type keywords: noble, minor, bandit, mercenary, eliminated, empty, etc.\n" +
                           "Example: gm.query.clan empire noble\n";
                }

                return $"Found {matchedClans.Count} clan(s) matching {criteriaDesc}:\n" +
                       $"{ClanQueries.GetFormattedDetails(matchedClans)}";
            });
        }

        /// <summary>
        /// Find clans matching ANY of the specified types (OR logic)
        /// Usage: gm.query.clan_any [search terms] [type keywords]
        /// </summary>
        [CommandLineFunctionality.CommandLineArgumentFunction("clan_any", "gm.query")]
        public static string QueryClansAny(List<string> args)
        {
            return Cmd.Run(args, () =>
            {
                if (Campaign.Current == null)
                    return "Error: Must be in campaign mode.\n";

                var (query, types) = ParseArguments(args);
                List<Clan> matchedClans = ClanQueries.QueryClans(query, types, matchAll: false);

                string criteriaDesc = BuildCriteriaString(query, types);
                
                if (matchedClans.Count == 0)
                {
                    return $"Found 0 clan(s) matching ANY of {criteriaDesc}\n" +
                           "Usage: gm.query.clan_any [search] [type keywords]\n" +
                           "Example: gm.query.clan_any bandit outlaw (finds bandits OR outlaws)\n";
                }

                return $"Found {matchedClans.Count} clan(s) matching ANY of {criteriaDesc}:\n" +
                       $"{ClanQueries.GetFormattedDetails(matchedClans)}";
            });
        }

        /// <summary>
        /// Get detailed info about a specific clan by ID
        /// </summary>
        [CommandLineFunctionality.CommandLineArgumentFunction("clan_info", "gm.query")]
        public static string QueryClanInfo(List<string> args)
        {
            return Cmd.Run(args, () =>
            {
                if (Campaign.Current == null)
                    return "Error: Must be in campaign mode.\n";

                if (args == null || args.Count == 0)
                    return "Error: Please provide a clan ID.\nUsage: gm.query.clan_info <clanId>\n";

                string clanId = args[0];
                Clan clan = ClanQueries.GetClanById(clanId);

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
            });
        }
    }
}