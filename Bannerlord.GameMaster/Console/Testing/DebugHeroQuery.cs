using Bannerlord.GameMaster.Console.Common;
using Bannerlord.GameMaster.Heroes;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Library;

namespace Bannerlord.GameMaster.Console.Testing
{
    /// <summary>
    /// Debug test for hero query issues
    /// </summary>
    [CommandLineFunctionality.CommandLineArgumentFunction("debug_hero_query", "gm.test")]
    public static class DebugHeroQuery
    {
        /// <summary>
        /// Test hero query for a specific string to debug resolution issues
        /// Usage: gm.test.debug_hero_query [query]
        /// </summary>
        [CommandLineFunctionality.CommandLineArgumentFunction("run", "gm.test.debug_hero_query")]
        public static string Run(List<string> args)
        {
            return Cmd.Run(args, () =>
            {
                if (!CommandBase.ValidateCampaignMode(out string error))
                    return error;

                if (args == null || args.Count < 1)
                    return "Error: Missing query argument. Usage: gm.test.debug_hero_query.run <query>\n";

                string query = args[0];

                // Enable logging temporarily
                bool wasEnabled = CommandLogger.IsEnabled;
                if (!wasEnabled)
                {
                    CommandLogger.Initialize();
                    CommandLogger.IsEnabled = true;
                }

                StringBuilder result = new StringBuilder();
                result.AppendLine($"=== DEBUG HERO QUERY: '{query}' ===\n");

                // Get all heroes that match the query
                List<Hero> matchedHeroes = HeroQueries.QueryHeroes(query);

                result.AppendLine($"Total heroes found: {matchedHeroes.Count}\n");

                if (matchedHeroes.Count > 0)
                {
                    result.AppendLine("All matched heroes:");
                    foreach (var hero in matchedHeroes)
                    {
                        string culture = hero.Culture?.Name?.ToString() ?? "Unknown";
                        string clan = hero.Clan?.Name?.ToString() ?? "None";
                        string kingdom = hero.Clan?.Kingdom?.Name?.ToString() ?? "None";
                        
                        result.AppendLine($"  Name: '{hero.Name}' | ID: '{hero.StringId}'");
                        result.AppendLine($"    Culture: {culture} | Clan: {clan} | Kingdom: {kingdom}");
                        result.AppendLine($"    Age: {hero.Age} | IsAlive: {hero.IsAlive}");
                        
                        // Check name/ID matching
                        bool nameContains = hero.Name.ToString().IndexOf(query, System.StringComparison.OrdinalIgnoreCase) >= 0;
                        bool idContains = hero.StringId.IndexOf(query, System.StringComparison.OrdinalIgnoreCase) >= 0;
                        bool nameExact = hero.Name.ToString().Equals(query, System.StringComparison.OrdinalIgnoreCase);
                        bool idExact = hero.StringId.Equals(query, System.StringComparison.OrdinalIgnoreCase);
                        bool namePrefix = hero.Name.ToString().StartsWith(query, System.StringComparison.OrdinalIgnoreCase);
                        bool idPrefix = hero.StringId.StartsWith(query, System.StringComparison.OrdinalIgnoreCase);
                        
                        result.AppendLine($"    Match Type: NameContains={nameContains}, IDContains={idContains}");
                        result.AppendLine($"                NameExact={nameExact}, IDExact={idExact}");
                        result.AppendLine($"                NamePrefix={namePrefix}, IDPrefix={idPrefix}");
                        result.AppendLine();
                    }
                }

                // Now test FindSingleHero to see which one it selects
                result.AppendLine("Testing FindSingleHero resolution:");
                var (selectedHero, heroError) = CommandBase.FindSingleHero(query);
                
                if (heroError != null)
                {
                    result.AppendLine($"ERROR: {heroError}");
                }
                else if (selectedHero != null)
                {
                    result.AppendLine($"SELECTED: Name: '{selectedHero.Name}' | ID: '{selectedHero.StringId}'");
                    result.AppendLine($"          Culture: {selectedHero.Culture?.Name} | Clan: {selectedHero.Clan?.Name}");
                }

                // Restore logging state
                if (!wasEnabled)
                {
                    CommandLogger.IsEnabled = false;
                }

                return result.ToString();
            });
        }
    }
}