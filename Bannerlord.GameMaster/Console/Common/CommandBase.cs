using System;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using Bannerlord.GameMaster.Heroes;
using Bannerlord.GameMaster.Clans;
using Bannerlord.GameMaster.Kingdoms;
using Bannerlord.GameMaster.Items;

namespace Bannerlord.GameMaster.Console.Common
{
    /// <summary>
    /// Base class providing common functionality for all command classes
    /// </summary>
    public static class CommandBase
    {
        #region Entity Finder Methods

        /// <summary>
        /// Helper method to find a single hero from a query
        /// </summary>
        public static (Hero hero, string error) FindSingleHero(string query)
        {
            List<Hero> matchedHeroes = HeroQueries.QueryHeroes(query);

            if (matchedHeroes == null || matchedHeroes.Count == 0)
                return (null, $"Error: No hero matching query '{query}' found.\n");

            if (matchedHeroes.Count == 1)
                return (matchedHeroes[0], null);

            // Use smart matching for multiple results
            return ResolveMultipleMatches(
                matches: matchedHeroes,
                query: query,
                getStringId: h => h.StringId,
                getName: h => h.Name?.ToString() ?? "",
                formatDetails: HeroQueries.GetFormattedDetails,
                entityType: "hero");
        }

        /// <summary>
        /// Helper method to find a single clan from a query
        /// </summary>
        public static (Clan clan, string error) FindSingleClan(string query)
        {
            List<Clan> matchedClans = ClanQueries.QueryClans(query);

            if (matchedClans == null || matchedClans.Count == 0)
                return (null, $"Error: No clan matching query '{query}' found.\n");

            if (matchedClans.Count == 1)
                return (matchedClans[0], null);

            // Use smart matching for multiple results
            return ResolveMultipleMatches(
                matches: matchedClans,
                query: query,
                getStringId: c => c.StringId,
                getName: c => c.Name?.ToString() ?? "",
                formatDetails: ClanQueries.GetFormattedDetails,
                entityType: "clan");
        }

        /// <summary>
        /// Helper method to find a single kingdom from a query
        /// </summary>
        public static (Kingdom kingdom, string error) FindSingleKingdom(string query)
        {
            List<Kingdom> matchedKingdoms = KingdomQueries.QueryKingdoms(query);

            if (matchedKingdoms == null || matchedKingdoms.Count == 0)
                return (null, $"Error: No kingdom matching query '{query}' found.\n");

            if (matchedKingdoms.Count == 1)
                return (matchedKingdoms[0], null);

            // Use smart matching for multiple results
            return ResolveMultipleMatches(
                matches: matchedKingdoms,
                query: query,
                getStringId: k => k.StringId,
                getName: k => k.Name?.ToString() ?? "",
                formatDetails: KingdomQueries.GetFormattedDetails,
                entityType: "kingdom");
        }

        /// <summary>
        /// Helper method to find a single item from a query
        /// </summary>
        public static (ItemObject item, string error) FindSingleItem(string query)
        {
            List<ItemObject> matchedItems = ItemQueries.QueryItems(query);

            if (matchedItems == null || matchedItems.Count == 0)
                return (null, $"Error: No item matching query '{query}' found.\n");

            if (matchedItems.Count == 1)
                return (matchedItems[0], null);

            // Use smart matching for multiple results
            return ResolveMultipleMatches(
                matches: matchedItems,
                query: query,
                getStringId: i => i.StringId,
                getName: i => i.Name?.ToString() ?? "",
                formatDetails: ItemQueries.GetFormattedDetails,
                entityType: "item");
        }

        /// <summary>
        /// Resolves a single entity from multiple matches using smart matching logic.
        /// Prioritizes: 1) Exact name match, 2) Name prefix match, 3) Exact ID match, 4) ID prefix match, 5) Shortest ID match, 6) Error on name-only matches
        /// </summary>
        /// <typeparam name="T">Entity type (Hero, Clan, or Kingdom)</typeparam>
        /// <param name="matches">List of matched entities</param>
        /// <param name="query">Original query string</param>
        /// <param name="getStringId">Function to extract StringId from entity</param>
        /// <param name="getName">Function to extract Name from entity</param>
        /// <param name="formatDetails">Function to format entity list for error messages</param>
        /// <param name="entityType">Name of entity type for error messages (e.g., "hero")</param>
        /// <returns>Tuple of (selected entity or null, error message or null)</returns>
        private static (T entity, string error) ResolveMultipleMatches<T>(
            List<T> matches,
            string query,
            Func<T, string> getStringId,
            Func<T, string> getName,
            Func<List<T>, string> formatDetails,
            string entityType) where T : class
        {
            // Collect all matches for name priority checking
            var allMatches = new List<T>();
            var idMatches = new List<T>();
            var nameMatches = new List<T>();

            foreach (var entity in matches)
            {
                string entityId = getStringId(entity) ?? "";
                string entityName = getName(entity) ?? "";

                bool matchesId = entityId.IndexOf(query, StringComparison.OrdinalIgnoreCase) >= 0;
                bool matchesName = entityName.IndexOf(query, StringComparison.OrdinalIgnoreCase) >= 0;

                if (matchesId || matchesName)
                {
                    allMatches.Add(entity);
                }
                
                if (matchesId)
                {
                    idMatches.Add(entity);
                }
                if (matchesName && !matchesId)
                {
                    nameMatches.Add(entity);
                }
            }

            // Priority 1: Check for exact name match across ALL matches
            var exactNameMatches = allMatches.Where(e => getName(e).Equals(query, StringComparison.OrdinalIgnoreCase)).ToList();
            
            if (exactNameMatches.Count == 1)
            {
                return (exactNameMatches[0], null); // Exact name match wins immediately
            }
            else if (exactNameMatches.Count > 1)
            {
                return (null, $"Error: Found {exactNameMatches.Count} {entityType}s with names exactly matching '{query}':\n" +
                    $"{formatDetails(exactNameMatches)}" +
                    $"Multiple entities have identical names. Please use their IDs.\n");
            }
            
            // Priority 2: Check for prefix matches across ALL matches
            var prefixMatches = allMatches.Where(e => getName(e).StartsWith(query, StringComparison.OrdinalIgnoreCase)).ToList();
            
            // Only auto-select if the prefix match is the ONLY match overall
            if (prefixMatches.Count == 1 && allMatches.Count == 1)
            {
                return (prefixMatches[0], null); // Single prefix match AND single overall match wins
            }
            else if (prefixMatches.Count > 1)
            {
                return (null, $"Error: Found {prefixMatches.Count} {entityType}s with names starting with '{query}':\n" +
                    $"{formatDetails(prefixMatches)}" +
                    $"Please use a more specific name or use their IDs.\n");
            }
            else if (prefixMatches.Count == 1 && allMatches.Count > 1)
            {
                // There's a prefix match but also other substring matches - ambiguous
                return (null, $"Error: Found {allMatches.Count} {entityType}s with names containing '{query}':\n" +
                    $"{formatDetails(allMatches)}" +
                    $"Please use a more specific name or use their IDs.\n");
            }

            // Priority 3: Check for exact ID match
            foreach (var entity in idMatches)
            {
                string entityId = getStringId(entity) ?? "";
                if (entityId.Equals(query, StringComparison.OrdinalIgnoreCase))
                {
                    return (entity, null); // Exact ID match wins
                }
            }

            // Priority 4: Check for ID prefix matches
            var idPrefixMatches = allMatches.Where(e => getStringId(e).StartsWith(query, StringComparison.OrdinalIgnoreCase)).ToList();
            
            if (idPrefixMatches.Count == 1)
            {
                return (idPrefixMatches[0], null); // Single ID prefix match wins
            }
            else if (idPrefixMatches.Count > 1)
            {
                return (null, $"Error: Found {idPrefixMatches.Count} {entityType}s with IDs starting with '{query}':\n" +
                    $"{formatDetails(idPrefixMatches)}" +
                    $"Please use a more specific ID.\n");
            }

            // Priority 5: Use shortest ID match if available
            if (idMatches.Count > 0)
            {
                // Find the shortest ID
                var shortestMatch = idMatches.OrderBy(e => getStringId(e)?.Length ?? int.MaxValue).First();
                int shortestLength = getStringId(shortestMatch)?.Length ?? 0;

                // Check if there are multiple IDs with the same shortest length
                var allShortestMatches = idMatches.Where(e => (getStringId(e)?.Length ?? 0) == shortestLength).ToList();

                if (allShortestMatches.Count == 1)
                {
                    return (shortestMatch, null);
                }
                else
                {
                    // Multiple IDs with same length - still ambiguous
                    return (null, $"Error: Found {allShortestMatches.Count} {entityType}s with IDs matching query '{query}':\n" +
                        $"{formatDetails(allShortestMatches)}" +
                        $"These IDs have the same length and cannot be automatically selected.\n" +
                        $"Please use a more specific ID.\n");
                }
            }

            // Priority 6: Only name matches remain
            if (nameMatches.Count > 0)
            {
                // Multiple substring matches (no exact or prefix matches)
                return (null, $"Error: Found {nameMatches.Count} {entityType}s with names containing '{query}':\n" +
                    $"{formatDetails(nameMatches)}" +
                    $"Please use a more specific name or use their IDs.\n");
            }

            // Should never reach here, but safety fallback
            return (null, $"Error: Found {matches.Count} {entityType}s matching query '{query}':\n" +
                $"{formatDetails(matches)}" +
                $"Please use a more specific name or ID.\n");
        }

        #endregion

        #region Validation Methods

        /// <summary>
        /// Validates campaign mode
        /// </summary>
        public static bool ValidateCampaignMode(out string error)
        {
            if (Campaign.Current == null)
            {
                error = "Error: Must be in campaign mode.\n";
                return false;
            }
            error = null;
            return true;
        }

        /// <summary>
        /// Validates minimum argument count
        /// </summary>
        public static bool ValidateArgumentCount(List<string> args, int requiredCount, string usageMessage, out string error)
        {
            if (args == null || args.Count < requiredCount)
            {
                error = $"Error: Missing arguments.\n{usageMessage}";
                return false;
            }
            error = null;
            return true;
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Executes an action safely with consistent error handling
        /// </summary>
        public static string ExecuteWithErrorHandling(Func<string> action, string errorPrefix = "Error")
        {
            try
            {
                return action();
            }
            catch (Exception ex)
            {
                return $"{errorPrefix}: {ex.Message}\n";
            }
        }

        /// <summary>
        /// Formats a success message with consistent styling
        /// </summary>
        public static string FormatSuccessMessage(string message)
        {
            return $"Success: {message}\n";
        }

        /// <summary>
        /// Formats an error message with consistent styling
        /// </summary>
        public static string FormatErrorMessage(string message)
        {
            return $"Error: {message}\n";
        }

        #endregion
    }

    /// <summary>
    /// Short alias for command execution with automatic logging
    /// Usage in any command: return Cmd.Run(args, () => { /* your logic */ });
    /// </summary>
    public static class Cmd
    {
        /// <summary>
        /// Execute command with automatic logging
        /// </summary>
        public static string Run(List<string> args, Func<string> action)
        {
            string commandName = GetCallingCommandName(args);
            
            try
            {
                string result = action();
                
                if (CommandLogger.IsEnabled)
                {
                    bool isSuccess = !result.StartsWith("Error:", StringComparison.OrdinalIgnoreCase);
                    CommandLogger.LogCommand(commandName, result, isSuccess);
                }
                
                return result;
            }
            catch (Exception ex)
            {
                string errorResult = $"Error: {ex.Message}\n";
                
                if (CommandLogger.IsEnabled)
                {
                    CommandLogger.LogCommand(commandName, errorResult, false);
                }
                
                return errorResult;
            }
        }

        /// <summary>
        /// Execute command with automatic logging using CommandResult
        /// </summary>
        public static CommandResult Run(List<string> args, Func<CommandResult> action)
        {
            string commandName = GetCallingCommandName(args);
            
            try
            {
                CommandResult result = action();
                
                if (CommandLogger.IsEnabled)
                {
                    CommandLogger.LogCommand(commandName, result.Message, result.IsSuccess);
                }
                
                return result;
            }
            catch (Exception ex)
            {
                var errorResult = CommandResult.Error(ex.Message);
                
                if (CommandLogger.IsEnabled)
                {
                    CommandLogger.LogCommand(commandName, errorResult.Message, false);
                }
                
                return errorResult;
            }
        }

        /// <summary>
        /// Automatically determine command name from calling method using reflection
        /// </summary>
        private static string GetCallingCommandName(List<string> args)
        {
            try
            {
                var stackTrace = new System.Diagnostics.StackTrace();
                // Frame 0 = GetCallingCommandName, Frame 1 = Run, Frame 2 = actual command
                var callingMethod = stackTrace.GetFrame(2)?.GetMethod();
                
                if (callingMethod != null)
                {
                    // Get the CommandLineArgumentFunction attribute  
                    var attributes = callingMethod.GetCustomAttributes(false);
                    foreach (var attr in attributes)
                    {
                        var attrType = attr.GetType();
                        if (attrType.Name == "CommandLineArgumentFunctionAttribute")
                        {
                            // Use reflection to get attribute properties
                            var nameProperty = attrType.GetProperty("Name");
                            var parentProperty = attrType.GetProperty("ParentCommandName");
                            
                            if (nameProperty != null && parentProperty != null)
                            {
                                string name = nameProperty.GetValue(attr) as string;
                                string parent = parentProperty.GetValue(attr) as string;
                                
                                // Build command name
                                string commandName = string.IsNullOrEmpty(parent) ? name : $"{parent}.{name}";
                                
                                // Add arguments if present
                                if (args != null && args.Count > 0)
                                {
                                    commandName += " " + string.Join(" ", args);
                                }
                                
                                return commandName;
                            }
                        }
                    }
                }
            }
            catch
            {
                // If reflection fails, fall back to generic name
            }
            
            return "gm.command";
        }
    }
}