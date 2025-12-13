using Bannerlord.GameMaster.Heroes;
using System;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Library;

namespace Bannerlord.GameMaster.Console
{
    [CommandLineFunctionality.CommandLineArgumentFunction("hero", "gm")]
    public static class ListHeroesCommands
    {
        /// <summary>
        /// Parse command arguments into search filter and hero type flags
        /// </summary>
        private static (string searchFilter, HeroTypes types, bool includeDead) ParseArguments(List<string> args)
        {
            if (args == null || args.Count == 0)
                return ("", HeroTypes.Alive, false);

            var typeKeywords = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "hero", "lord", "wanderer", "notable", "merchant", "children", "child",
                "female", "male", "clanleader", "kingdomruler", "partyleader",
                "fugitive", "alive", "dead", "prisoner", "withoutclan", "withoutkingdom", "married"
            };

            List<string> searchTerms = new();
            List<string> typeTerms = new();
            bool includeDead = false;

            foreach (var arg in args)
            {
                var lower = arg.ToLower();

                if (lower == "dead")
                {
                    includeDead = true;
                    typeTerms.Add(arg);
                }
                else if (typeKeywords.Contains(lower))
                {
                    typeTerms.Add(arg);
                }
                else
                {
                    searchTerms.Add(arg);
                }
            }

            string searchFilter = string.Join(" ", searchTerms).Trim();
            HeroTypes types = HeroQueries.ParseHeroTypes(typeTerms);

            // Default to Alive if no life status specified and not searching dead
            if (!includeDead && !types.HasFlag(HeroTypes.Dead) && !types.HasFlag(HeroTypes.Alive))
            {
                types |= HeroTypes.Alive;
            }

            return (searchFilter, types, includeDead);
        }

        /// <summary>
        /// Unified hero listing command
        /// Usage: gm.hero.find [search terms] [type keywords]
        /// Example: gm.hero.find john lord female clanleader
        /// Example: gm.hero.find aserai wanderer
        /// Example: gm.hero.find dead kingdomruler
        /// </summary>
        [CommandLineFunctionality.CommandLineArgumentFunction("find", "gm.hero")]
        public static string FindHeroes(List<string> args)
        {
            if (Campaign.Current == null)
                return "Error: Must be in campaign mode.\n";

            var (searchFilter, types, includeDead) = ParseArguments(args);

            List<Hero> matchedHeroes = HeroQueries.FindHeroes(searchFilter, types, matchAll: true, includeDead: includeDead);

            if (matchedHeroes.Count == 0)
            {
                string criteria = BuildCriteriaString(searchFilter, types);
                return $"No heroes found matching criteria: {criteria}\n" +
                       "Usage: gm.hero.find [search] [type keywords]\n" +
                       "Type keywords: lord, wanderer, notable, female, male, clanleader, kingdomruler, dead, etc.\n" +
                       "Example: gm.hero.find john lord female\n";
            }

            string criteriaDesc = BuildCriteriaString(searchFilter, types);
            return $"Found {matchedHeroes.Count} hero(es) matching {criteriaDesc}:\n" +
                   $"{HeroQueries.GetFormattedDetails(matchedHeroes)}";
        }

        /// <summary>
        /// Find heroes matching ANY of the specified types (OR logic)
        /// Usage: gm.hero.find_any [search terms] [type keywords]
        /// Example: gm.hero.find_any lord wanderer (finds anyone who is lord OR wanderer)
        /// </summary>
        [CommandLineFunctionality.CommandLineArgumentFunction("find_any", "gm.hero")]
        public static string FindHeroesAny(List<string> args)
        {
            if (Campaign.Current == null)
                return "Error: Must be in campaign mode.\n";

            var (searchFilter, types, includeDead) = ParseArguments(args);

            List<Hero> matchedHeroes = HeroQueries.FindHeroes(searchFilter, types, matchAll: false, includeDead: includeDead);

            if (matchedHeroes.Count == 0)
            {
                string criteria = BuildCriteriaString(searchFilter, types);
                return $"No heroes found matching ANY of: {criteria}\n" +
                       "Usage: gm.hero.find_any [search] [type keywords]\n" +
                       "Example: gm.hero.find_any lord wanderer (finds lords OR wanderers)\n";
            }

            string criteriaDesc = BuildCriteriaString(searchFilter, types);
            return $"Found {matchedHeroes.Count} hero(es) matching ANY of {criteriaDesc}:\n" +
                   $"{HeroQueries.GetFormattedDetails(matchedHeroes)}";
        }

        /// <summary>
        /// Get detailed info about a specific hero by ID
        /// </summary>
        [CommandLineFunctionality.CommandLineArgumentFunction("info", "gm.hero")]
        public static string GetHeroInfo(List<string> args)
        {
            if (Campaign.Current == null)
                return "Error: Must be in campaign mode.\n";

            if (args == null || args.Count == 0)
                return "Error: Please provide a hero ID.\nUsage: gm.hero.info <heroId>\n";

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
        }

        /// <summary>
        /// Helper to build a readable criteria string
        /// </summary>
        private static string BuildCriteriaString(string searchFilter, HeroTypes types)
        {
            List<string> parts = new();

            if (!string.IsNullOrEmpty(searchFilter))
                parts.Add($"search: '{searchFilter}'");

            if (types != HeroTypes.None)
            {
                var typeList = Enum.GetValues(typeof(HeroTypes))
                    .Cast<HeroTypes>()
                    .Where(t => t != HeroTypes.None && types.HasFlag(t))
                    .Select(t => t.ToString().ToLower());
                parts.Add($"types: {string.Join(", ", typeList)}");
            }

            return parts.Count > 0 ? string.Join(", ", parts) : "all heroes";
        }
    }
}