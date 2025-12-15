using Bannerlord.GameMaster.Heroes;
using Bannerlord.GameMaster.Console.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Library;

namespace Bannerlord.GameMaster.Console.Query
{
    [CommandLineFunctionality.CommandLineArgumentFunction("query", "gm")]
    public static class HeroQueryCommands
    {
        /// <summary>
        /// Parse command arguments into search filter and hero type flags
        /// </summary>
        private static (string query, HeroTypes types, bool includeDead, string sortBy, bool sortDesc) ParseArguments(List<string> args)
        {
            if (args == null || args.Count == 0)
                return ("", HeroTypes.Alive, false, "id", false);

            var typeKeywords = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "hero", "lord", "wanderer", "notable", "merchant", "children", "child",
                "female", "male", "clanleader", "kingdomruler", "partyleader",
                "fugitive", "alive", "dead", "prisoner", "withoutclan", "withoutkingdom", "married"
            };

            List<string> searchTerms = new();
            List<string> typeTerms = new();
            string sortBy = "id";
            bool sortDesc = false;

            // Check if "dead" keyword is present
            bool includeDead = args.Any(arg => arg.Equals("dead", StringComparison.OrdinalIgnoreCase));

            foreach (var arg in args)
            {
                // Check for sort parameters
                if (arg.StartsWith("sort:", StringComparison.OrdinalIgnoreCase))
                {
                    ParseSortParameter(arg, ref sortBy, ref sortDesc);
                }
                // Check for type keywords
                else if (typeKeywords.Contains(arg, StringComparer.OrdinalIgnoreCase))
                {
                    typeTerms.Add(arg);
                }
                // Otherwise treat as search term
                else
                {
                    searchTerms.Add(arg);
                }
            }

            string query = string.Join(" ", searchTerms).Trim();
            HeroTypes types = HeroQueries.ParseHeroTypes(typeTerms);

            // Default to Alive if no life status specified and not searching dead
            if (!includeDead && !types.HasFlag(HeroTypes.Dead) && !types.HasFlag(HeroTypes.Alive))
            {
                types |= HeroTypes.Alive;
            }

            return (query, types, includeDead, sortBy, sortDesc);
        }

        /// <summary>
        /// Parse sort parameter (e.g., "sort:name:desc" or "sort:age")
        /// </summary>
        private static void ParseSortParameter(string sortParam, ref string sortBy, ref bool sortDesc)
        {
            var parts = sortParam.Split(':');
            if (parts.Length >= 2)
            {
                sortBy = parts[1].ToLower();
            }
            if (parts.Length >= 3)
            {
                sortDesc = parts[2].Equals("desc", StringComparison.OrdinalIgnoreCase);
            }
        }

        /// <summary>
        /// Unified hero listing command
        /// Usage: gm.query.hero [search terms] [type keywords] [sort parameters]
        /// Example: gm.query.hero john lord female clanleader
        /// Example: gm.query.hero aserai wanderer sort:name
        /// Example: gm.query.hero dead kingdomruler sort:age:desc
        /// Example: gm.query.hero sort:wanderer (sorts by wanderer flag)
        /// </summary>
        [CommandLineFunctionality.CommandLineArgumentFunction("hero", "gm.query")]
        public static string QueryHeroes(List<string> args)
        {
            return Cmd.Run(args, () =>
            {
                if (Campaign.Current == null)
                    return "Error: Must be in campaign mode.\n";

                var (query, types, includeDead, sortBy, sortDesc) = ParseArguments(args);

                List<Hero> matchedHeroes = HeroQueries.QueryHeroes(query, types, matchAll: true, includeDead: includeDead, sortBy, sortDesc);

                string criteriaDesc = BuildCriteriaString(query, types, sortBy, sortDesc);
                
                if (matchedHeroes.Count == 0)
                {
                    return $"Found 0 hero(es) matching {criteriaDesc}\n" +
                           "Usage: gm.query.hero [search] [type keywords] [sort]\n" +
                           "Type keywords: lord, wanderer, notable, female, male, clanleader, kingdomruler, dead, etc.\n" +
                           "Sort: sort:name, sort:age, sort:clan, sort:kingdom, sort:<type> (add :desc for descending)\n" +
                           "Example: gm.query.hero john lord female sort:name\n";
                }

                return $"Found {matchedHeroes.Count} hero(es) matching {criteriaDesc}:\n" +
                       $"{HeroQueries.GetFormattedDetails(matchedHeroes)}";
            });
        }

        /// <summary>
        /// Find heroes matching ANY of the specified types (OR logic)
        /// Usage: gm.query.hero_any [search terms] [type keywords] [sort parameters]
        /// Example: gm.query.hero_any lord wanderer (finds anyone who is lord OR wanderer)
        /// Example: gm.query.hero_any lord wanderer sort:name:desc
        /// </summary>
        [CommandLineFunctionality.CommandLineArgumentFunction("hero_any", "gm.query")]
        public static string QueryHeroesAny(List<string> args)
        {
            return Cmd.Run(args, () =>
            {
                if (Campaign.Current == null)
                    return "Error: Must be in campaign mode.\n";

                var (query, types, includeDead, sortBy, sortDesc) = ParseArguments(args);

                List<Hero> matchedHeroes = HeroQueries.QueryHeroes(query, types, matchAll: false, includeDead: includeDead, sortBy, sortDesc);

                string criteriaDesc = BuildCriteriaString(query, types, sortBy, sortDesc);
                
                if (matchedHeroes.Count == 0)
                {
                    return $"Found 0 hero(es) matching ANY of {criteriaDesc}\n" +
                           "Usage: gm.query.hero_any [search] [type keywords] [sort]\n" +
                           "Example: gm.query.hero_any lord wanderer sort:name\n";
                }

                return $"Found {matchedHeroes.Count} hero(es) matching ANY of {criteriaDesc}:\n" +
                       $"{HeroQueries.GetFormattedDetails(matchedHeroes)}";
            });
        }

        /// <summary>
        /// Get detailed info about a specific hero by ID
        /// </summary>
        [CommandLineFunctionality.CommandLineArgumentFunction("hero_info", "gm.query")]
        public static string QueryHeroInfo(List<string> args)
        {
            return Cmd.Run(args, () =>
            {
                if (Campaign.Current == null)
                    return "Error: Must be in campaign mode.\n";

                if (args == null || args.Count == 0)
                    return "Error: Please provide a hero ID.\nUsage: gm.query.hero_info <heroId>\n";

                string heroId = args[0];
                Hero hero = HeroQueries.GetHeroById(heroId);

                if (hero == null)
                    return $"Error: Hero with ID '{heroId}' not found.\n";

                var types = hero.GetHeroTypes();
                string clanName = hero.Clan?.Name?.ToString() ?? "None";
                string kingdomName = hero.Clan?.Kingdom?.Name?.ToString() ?? "None";

                return $"Hero Information:\n" +
                       $"ID: {hero.StringId}\n" +
                       $"Name: {hero.Name}\n" +
                       $"Clan: {clanName}\n" +
                       $"Kingdom: {kingdomName}\n" +
                       $"Age: {hero.Age:F0}\n" +
                       $"Types: {types}\n" +
                       $"Is Alive: {hero.IsAlive}\n" +
                       $"Is Prisoner: {hero.IsPrisoner}\n";
            });
        }

        /// <summary>
        /// Helper to build a readable criteria string
        /// </summary>
        private static string BuildCriteriaString(string query, HeroTypes types, string sortBy, bool sortDesc)
        {
            List<string> parts = new();

            if (!string.IsNullOrEmpty(query))
                parts.Add($"search: '{query}'");

            if (types != HeroTypes.None)
            {
                var typeList = Enum.GetValues(typeof(HeroTypes))
                    .Cast<HeroTypes>()
                    .Where(t => t != HeroTypes.None && types.HasFlag(t))
                    .Select(t => t.ToString().ToLower());
                parts.Add($"types: {string.Join(", ", typeList)}");
            }

            if (!string.IsNullOrEmpty(sortBy) && sortBy != "id")
                parts.Add($"sort: {sortBy}{(sortDesc ? " (desc)" : " (asc)")}");

            return parts.Count > 0 ? string.Join(", ", parts) : "all heroes";
        }
    }
}