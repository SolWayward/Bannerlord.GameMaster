using Bannerlord.GameMaster.Console.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Library;
using TaleWorlds.ObjectSystem;
using TaleWorlds.ObjectSystem;

namespace Bannerlord.GameMaster.Console.Query
{
    [CommandLineFunctionality.CommandLineArgumentFunction("query", "gm")]
    public static class CultureQueryCommands
    {
        /// <summary>
        /// Parse command arguments into search filter and culture type flags
        /// </summary>
        private static (string query, bool mainOnly, bool banditOnly, string sortBy, bool sortDesc) ParseArguments(List<string> args)
        {
            if (args == null || args.Count == 0)
                return ("", false, false, "id", false);

            var typeKeywords = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "main", "bandit", "minor"
            };

            List<string> searchTerms = new();
            bool mainOnly = false;
            bool banditOnly = false;
            string sortBy = "id";
            bool sortDesc = false;

            foreach (var arg in args)
            {
                // Check for sort parameters
                if (arg.StartsWith("sort:", StringComparison.OrdinalIgnoreCase))
                {
                    ParseSortParameter(arg, ref sortBy, ref sortDesc);
                }
                // Check for type keywords
                else if (arg.Equals("main", StringComparison.OrdinalIgnoreCase))
                {
                    mainOnly = true;
                }
                else if (arg.Equals("bandit", StringComparison.OrdinalIgnoreCase))
                {
                    banditOnly = true;
                }
                // Otherwise treat as search term
                else
                {
                    searchTerms.Add(arg);
                }
            }

            string query = string.Join(" ", searchTerms).Trim();

            return (query, mainOnly, banditOnly, sortBy, sortDesc);
        }

        /// <summary>
        /// Parse sort parameter (e.g., "sort:name:desc" or "sort:id")
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
        /// List all cultures with optional filtering
        /// Usage: gm.query.culture [search terms] [main|bandit] [sort parameters]
        /// Example: gm.query.culture
        /// Example: gm.query.culture main
        /// Example: gm.query.culture empire sort:name
        /// Example: gm.query.culture bandit sort:name:desc
        /// </summary>
        [CommandLineFunctionality.CommandLineArgumentFunction("culture", "gm.query")]
        public static string QueryCultures(List<string> args)
        {
            return Cmd.Run(args, () =>
            {
                if (!CommandBase.ValidateCampaignMode(out string error))
                    return error;

                var (query, mainOnly, banditOnly, sortBy, sortDesc) = ParseArguments(args);

                // Get all cultures as IEnumerable
                IEnumerable<CultureObject> cultures = MBObjectManager.Instance.GetObjectTypeList<CultureObject>();

                // Apply filters
                if (mainOnly)
                {
                    cultures = cultures.Where(c => c.IsMainCulture);
                }
                else if (banditOnly)
                {
                    cultures = cultures.Where(c => c.IsBandit);
                }

                // Apply search filter
                if (!string.IsNullOrEmpty(query))
                {
                    string lowerQuery = query.ToLower();
                    cultures = cultures.Where(c =>
                        c.StringId.ToLower().Contains(lowerQuery) ||
                        c.Name.ToString().ToLower().Contains(lowerQuery));
                }

                // Convert to list and apply sorting
                List<CultureObject> cultureList = SortCultures(cultures.ToList(), sortBy, sortDesc);

                string criteriaDesc = BuildCriteriaString(query, mainOnly, banditOnly, sortBy, sortDesc);

                if (cultureList.Count == 0)
                {
                    return $"Found 0 culture(s) matching {criteriaDesc}\n" +
                           "Usage: gm.query.culture [search] [main|bandit] [sort]\n" +
                           "Example: gm.query.culture empire\n" +
                           "Example: gm.query.culture main sort:name\n" +
                           "Example: gm.query.culture bandit\n";
                }

                return $"Found {cultureList.Count} culture(s) matching {criteriaDesc}:\n" +
                       $"{GetFormattedCultureList(cultureList)}";
            });
        }

        /// <summary>
        /// Get detailed info about a specific culture by ID
        /// Usage: gm.query.culture_info <cultureId>
        /// </summary>
        [CommandLineFunctionality.CommandLineArgumentFunction("culture_info", "gm.query")]
        public static string QueryCultureInfo(List<string> args)
        {
            return Cmd.Run(args, () =>
            {
                if (!CommandBase.ValidateCampaignMode(out string error))
                    return error;

                if (args == null || args.Count == 0)
                    return "Error: Please provide a culture ID.\n" +
                           "Usage: gm.query.culture_info <cultureId>\n" +
                           "Example: gm.query.culture_info empire\n";

                string cultureId = args[0];
                var culture = MBObjectManager.Instance.GetObject<CultureObject>(cultureId);

                if (culture == null)
                    return $"Error: Culture with ID '{cultureId}' not found.\n";

                return $"Culture Information:\n" +
                       $"ID: {culture.StringId}\n" +
                       $"Name: {culture.Name}\n" +
                       $"Is Main Culture: {culture.IsMainCulture}\n" +
                       $"Is Bandit: {culture.IsBandit}\n" +
                       $"Color: {culture.Color}\n" +
                       $"Color2: {culture.Color2}\n" +
                       $"Male Names: {culture.MaleNameList?.Count ?? 0}\n" +
                       $"Female Names: {culture.FemaleNameList?.Count ?? 0}\n" +
                       $"Clan Names: {culture.ClanNameList?.Count ?? 0}\n";
            });
        }

        /// <summary>
        /// Sort cultures based on the specified criteria
        /// </summary>
        private static List<CultureObject> SortCultures(List<CultureObject> cultures, string sortBy, bool descending)
        {
            IOrderedEnumerable<CultureObject> sorted = sortBy.ToLower() switch
            {
                "name" => descending
                    ? cultures.OrderByDescending(c => c.Name.ToString())
                    : cultures.OrderBy(c => c.Name.ToString()),
                "id" => descending
                    ? cultures.OrderByDescending(c => c.StringId)
                    : cultures.OrderBy(c => c.StringId),
                _ => descending
                    ? cultures.OrderByDescending(c => c.StringId)
                    : cultures.OrderBy(c => c.StringId)
            };

            return sorted.ToList();
        }

        /// <summary>
        /// Format culture list for display
        /// </summary>
        private static string GetFormattedCultureList(List<CultureObject> cultures)
        {
            if (cultures.Count == 0)
                return "";

            return ColumnFormatter<CultureObject>.FormatList(
                cultures,
                c => c.StringId,
                c => c.Name.ToString(),
                c => c.IsMainCulture ? "Main" : c.IsBandit ? "Bandit" : "Minor"
            );
        }

        /// <summary>
        /// Helper to build a readable criteria string
        /// </summary>
        private static string BuildCriteriaString(string query, bool mainOnly, bool banditOnly, string sortBy, bool sortDesc)
        {
            List<string> parts = new();

            if (!string.IsNullOrEmpty(query))
                parts.Add($"search: '{query}'");

            if (mainOnly)
                parts.Add("type: main");
            else if (banditOnly)
                parts.Add("type: bandit");

            if (!string.IsNullOrEmpty(sortBy) && sortBy != "id")
                parts.Add($"sort: {sortBy}{(sortDesc ? " (desc)" : " (asc)")}");

            return parts.Count > 0 ? string.Join(", ", parts) : "all cultures";
        }
    }
}
