using System;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;
using Bannerlord.GameMaster.Heroes;
using Bannerlord.GameMaster.Clans;
using Bannerlord.GameMaster.Kingdoms;
using Bannerlord.GameMaster.Items;
using Bannerlord.GameMaster.Troops;
using Bannerlord.GameMaster.Settlements;

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

            // DEBUG: Log all matched heroes
            if (CommandLogger.IsEnabled && matchedHeroes != null && matchedHeroes.Count > 0)
            {
                System.Text.StringBuilder debugInfo = new System.Text.StringBuilder();
                debugInfo.AppendLine($"[DEBUG] FindSingleHero query '{query}' found {matchedHeroes.Count} matches:");
                foreach (var hero in matchedHeroes)
                {
                    debugInfo.AppendLine($"  - Name: '{hero.Name}' | ID: '{hero.StringId}' | Culture: {hero.Culture?.Name}");
                }
                CommandLogger.Log(debugInfo.ToString());
            }

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
        /// Helper method to find a single troop from a query (filtered combat troops only)
        /// </summary>
        public static (CharacterObject troop, string error) FindSingleTroop(string query)
        {
            List<CharacterObject> matchedTroops = TroopQueries.QueryTroops(query);

            if (matchedTroops == null || matchedTroops.Count == 0)
                return (null, $"Error: No troop matching query '{query}' found.\n");

            if (matchedTroops.Count == 1)
                return (matchedTroops[0], null);

            // Use smart matching for multiple results
            return ResolveMultipleMatches(
                matches: matchedTroops,
                query: query,
                getStringId: t => t.StringId,
                getName: t => t.Name?.ToString() ?? "",
                formatDetails: TroopQueries.GetFormattedDetails,
                entityType: "troop");
        }

        /// <summary>
        /// Helper method to find a single CharacterObject from a query (unfiltered - includes all non-hero CharacterObjects)
        /// Use this when you need to accept any CharacterObject including dancers, refugees, etc.
        /// </summary>
        public static (CharacterObject character, string error) FindSingleCharacterObject(string query)
        {
            List<CharacterObject> matchedCharacters = TroopQueries.QueryCharacterObjects(query);

            if (matchedCharacters == null || matchedCharacters.Count == 0)
                return (null, $"Error: No character matching query '{query}' found.\n");

            if (matchedCharacters.Count == 1)
                return (matchedCharacters[0], null);

            // Use smart matching for multiple results
            return ResolveMultipleMatches(
                matches: matchedCharacters,
                query: query,
                getStringId: c => c.StringId,
                getName: c => c.Name?.ToString() ?? "",
                formatDetails: TroopQueries.GetFormattedDetails,
                entityType: "character");
        }

        /// <summary>
        /// Helper method to find a single settlement from a query
        /// </summary>
        public static (Settlement settlement, string error) FindSingleSettlement(string query)
        {
            List<Settlement> matchedSettlements = SettlementQueries.QuerySettlements(query);

            if (matchedSettlements == null || matchedSettlements.Count == 0)
                return (null, $"Error: No settlement matching query '{query}' found.\n");

            if (matchedSettlements.Count == 1)
                return (matchedSettlements[0], null);

            // Use smart matching for multiple results
            return ResolveMultipleMatches(
                matches: matchedSettlements,
                query: query,
                getStringId: s => s.StringId,
                getName: s => s.Name?.ToString() ?? "",
                formatDetails: SettlementQueries.GetFormattedDetails,
                entityType: "settlement");
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

            // DEBUG: Log resolution process
            if (CommandLogger.IsEnabled)
            {
                CommandLogger.Log($"[DEBUG] ResolveMultipleMatches for query '{query}' with {matches.Count} matches");
            }

            foreach (var entity in matches)
            {
                string entityId = getStringId(entity) ?? "";
                string entityName = getName(entity) ?? "";

                bool matchesId = entityId.IndexOf(query, StringComparison.OrdinalIgnoreCase) >= 0;
                bool matchesName = entityName.IndexOf(query, StringComparison.OrdinalIgnoreCase) >= 0;

                // DEBUG: Log match details
                if (CommandLogger.IsEnabled)
                {
                    CommandLogger.Log($"[DEBUG]   Entity: Name='{entityName}' ID='{entityId}' | MatchesName={matchesName} MatchesID={matchesId}");
                }

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

            // DEBUG: Log categorization
            if (CommandLogger.IsEnabled)
            {
                CommandLogger.Log($"[DEBUG] Categorization: allMatches={allMatches.Count}, idMatches={idMatches.Count}, nameMatches={nameMatches.Count}");
            }

            // Priority 1: Check for exact name match across ALL matches
            var exactNameMatches = allMatches.Where(e => getName(e).Equals(query, StringComparison.OrdinalIgnoreCase)).ToList();
            
            if (CommandLogger.IsEnabled)
            {
                CommandLogger.Log($"[DEBUG] Priority 1 - Exact name matches: {exactNameMatches.Count}");
            }
            
            if (exactNameMatches.Count == 1)
            {
                if (CommandLogger.IsEnabled)
                {
                    CommandLogger.Log($"[DEBUG] SELECTED by Priority 1: Name='{getName(exactNameMatches[0])}' ID='{getStringId(exactNameMatches[0])}'");
                }
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
            
            if (CommandLogger.IsEnabled)
            {
                CommandLogger.Log($"[DEBUG] Priority 2 - Prefix name matches: {prefixMatches.Count}, Total matches: {allMatches.Count}");
            }
            
            // Only auto-select if the prefix match is the ONLY match overall
            if (prefixMatches.Count == 1 && allMatches.Count == 1)
            {
                if (CommandLogger.IsEnabled)
                {
                    CommandLogger.Log($"[DEBUG] SELECTED by Priority 2: Name='{getName(prefixMatches[0])}' ID='{getStringId(prefixMatches[0])}'");
                }
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

        #region Argument Parsing Helpers

        /// <summary>
        /// Parses arguments to properly handle quoted strings using SINGLE QUOTES.
        /// TaleWorlds removes double quotes but preserves single quotes.
        /// Use 'text with spaces' for multi-word arguments.
        ///
        /// Examples:
        ///   Input:  ["'vladiv", "castle'", "'castle", "of", "stone'", "example"]
        ///   Output: ["vladiv castle", "castle of stone", "example"]
        ///
        ///   Input:  ["'Castle", "of", "Rocks'"]
        ///   Output: ["Castle of Rocks"]
        /// </summary>
        public static List<string> ParseQuotedArguments(List<string> args)
        {
            if (args == null || args.Count == 0)
                return args ?? new List<string>();

            var result = new List<string>();
            int i = 0;

            while (i < args.Count)
            {
                string arg = args[i];

                // Check if this argument contains a colon with a quote after it (named argument with quoted value)
                int colonIndex = arg.IndexOf(':');
                if (colonIndex > 0 && colonIndex < arg.Length - 1)
                {
                    string afterColon = arg.Substring(colonIndex + 1);
                    if (afterColon.StartsWith("'"))
                    {
                        // This is a named argument with a quoted value
                        string name = arg.Substring(0, colonIndex + 1); // Keep the colon
                        string firstPart = afterColon.Substring(1); // Remove leading quote
                        
                        // Check if the quote closes in this same arg
                        if (firstPart.EndsWith("'"))
                        {
                            // Single-word quoted value
                            result.Add(name + firstPart.Substring(0, firstPart.Length - 1));
                            i++;
                            continue;
                        }
                        
                        // Multi-word quoted value - collect remaining parts
                        var quotedParts = new List<string> { firstPart };
                        i++;
                        
                        while (i < args.Count)
                        {
                            string part = args[i];
                            if (part.EndsWith("'"))
                            {
                                // Found closing quote
                                quotedParts.Add(part.Substring(0, part.Length - 1));
                                i++;
                                break;
                            }
                            else
                            {
                                quotedParts.Add(part);
                                i++;
                            }
                        }
                        
                        result.Add(name + string.Join(" ", quotedParts));
                        continue;
                    }
                }

                // Check if this argument starts with a single quote (regular quoted argument)
                if (arg.StartsWith("'"))
                {
                    // Start collecting parts of the quoted string
                    var quotedParts = new List<string>();
                    
                    // Remove leading quote from first part
                    string firstPart = arg.Substring(1);
                    
                    // Check if the quote also ends in this same arg (e.g., 'word')
                    if (firstPart.EndsWith("'"))
                    {
                        // Single-word quoted arg, remove trailing quote
                        result.Add(firstPart.Substring(0, firstPart.Length - 1));
                        i++;
                        continue;
                    }
                    
                    // Add first part (without leading quote)
                    quotedParts.Add(firstPart);
                    i++;
                    
                    // Continue collecting until we find the closing quote
                    while (i < args.Count)
                    {
                        string part = args[i];
                        
                        if (part.EndsWith("'"))
                        {
                            // Found closing quote, remove it and add final part
                            quotedParts.Add(part.Substring(0, part.Length - 1));
                            i++;
                            break;
                        }
                        else
                        {
                            // Middle part of quoted string
                            quotedParts.Add(part);
                            i++;
                        }
                    }
                    
                    // Combine all parts with spaces
                    result.Add(string.Join(" ", quotedParts));
                    
                    // If no closing quote was found, we've consumed all remaining args
                    // which is fine - treat as one long quoted string
                }
                else
                {
                    // Regular unquoted argument
                    result.Add(arg);
                    i++;
                }
            }

            return result;
        }

        /// <summary>
        /// Joins remaining arguments starting from the specified index into a single string.
        /// Useful for commands that accept multi-word text as the last parameter.
        /// Example: JoinRemainingArgs(args, 2) for "gm.cmd arg1 arg2 word1 word2 word3" returns "word1 word2 word3"
        /// </summary>
        public static string JoinRemainingArgs(List<string> args, int startIndex)
        {
            if (args == null || startIndex >= args.Count)
                return string.Empty;

            return string.Join(" ", args.Skip(startIndex));
        }

        /// <summary>
        /// Gets an argument at the specified index, or returns all remaining arguments joined if consumeRemaining is true.
        /// This is useful for parameters that should accept multi-word input.
        /// </summary>
        public static string GetArgument(List<string> args, int index, bool consumeRemaining = false)
        {
            if (args == null || index >= args.Count)
                return string.Empty;

            if (consumeRemaining)
                return JoinRemainingArgs(args, index);

            return args[index];
        }

        #endregion

        #region Named Argument Parser

        /// <summary>
        /// Represents a command argument definition for validation and display
        /// </summary>
        public class ArgumentDefinition
        {
            public string Name { get; set; }
            public bool IsRequired { get; set; }
            public string DefaultDisplay { get; set; }
            public List<string> Aliases { get; set; } = new List<string>();

            public ArgumentDefinition(string name, bool isRequired, string defaultDisplay = null, params string[] aliases)
            {
                Name = name;
                IsRequired = isRequired;
                DefaultDisplay = defaultDisplay;
                if (aliases != null)
                    Aliases.AddRange(aliases);
            }
        }

        /// <summary>
        /// Parses command arguments into a structure supporting both positional and named arguments.
        /// Named arguments use format argName:argContent (no spaces around colon).
        /// Example: count:5 name:'Sir Galahad' culture:vlandia
        /// </summary>
        public class ParsedArguments
        {
            private readonly Dictionary<string, string> _namedArgs = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            private readonly List<string> _positionalArgs = new List<string>();
            private readonly List<string> _allArgs = new List<string>();
            private readonly List<string> _unknownNamedArgs = new List<string>();
            private List<ArgumentDefinition> _validArguments;

            public ParsedArguments(List<string> args)
            {
                if (args == null || args.Count == 0)
                    return;

                // Process each argument
                foreach (string arg in args)
                {
                    _allArgs.Add(arg);

                    // Check if this is a named argument (contains : without spaces)
                    int colonIndex = arg.IndexOf(':');
                    if (colonIndex > 0 && colonIndex < arg.Length - 1)
                    {
                        string name = arg.Substring(0, colonIndex).Trim();
                        string value = arg.Substring(colonIndex + 1);
                        
                        // Only treat as named argument if name doesn't contain spaces
                        if (!name.Contains(" "))
                        {
                            _namedArgs[name] = value;
                            continue;
                        }
                    }

                    // Not a named argument, treat as positional
                    _positionalArgs.Add(arg);
                }
            }

            /// <summary>
            /// Sets valid argument definitions for this command and validates
            /// </summary>
            public void SetValidArguments(params ArgumentDefinition[] definitions)
            {
                _validArguments = new List<ArgumentDefinition>(definitions);
                ValidateNamedArguments();
            }

            /// <summary>
            /// Validates that all named arguments match defined argument names (case-insensitive)
            /// </summary>
            private void ValidateNamedArguments()
            {
                if (_validArguments == null || _validArguments.Count == 0)
                    return;

                _unknownNamedArgs.Clear();

                foreach (var namedArgKey in _namedArgs.Keys)
                {
                    bool found = false;
                    foreach (var def in _validArguments)
                    {
                        if (def.Name.Equals(namedArgKey, StringComparison.OrdinalIgnoreCase))
                        {
                            found = true;
                            break;
                        }
                        // Check aliases
                        foreach (var alias in def.Aliases)
                        {
                            if (alias.Equals(namedArgKey, StringComparison.OrdinalIgnoreCase))
                            {
                                found = true;
                                break;
                            }
                        }
                        if (found) break;
                    }

                    if (!found)
                        _unknownNamedArgs.Add(namedArgKey);
                }
            }

            /// <summary>
            /// Gets validation error if unknown named arguments were found
            /// </summary>
            public string GetValidationError()
            {
                if (_unknownNamedArgs.Count == 0)
                    return null;

                string validNames = _validArguments != null
                    ? string.Join(", ", _validArguments.Select(a => a.Name + (a.Aliases.Count > 0 ? "/" + string.Join("/", a.Aliases) : "")))
                    : "none defined";

                return $"Unknown named argument(s): {string.Join(", ", _unknownNamedArgs)}\nValid argument names: {validNames}";
            }

            /// <summary>
            /// Formats the argument display header showing all argument values
            /// </summary>
            public string FormatArgumentDisplay(string commandName, Dictionary<string, string> resolvedValues)
            {
                if (_validArguments == null || _validArguments.Count == 0)
                    return string.Empty;

                var parts = new List<string>();
                
                foreach (var def in _validArguments)
                {
                    string displayValue = resolvedValues.ContainsKey(def.Name)
                        ? resolvedValues[def.Name]
                        : def.DefaultDisplay ?? "Not specified";

                    if (def.IsRequired)
                        parts.Add($"<{def.Name}: {displayValue}>");
                    else
                        parts.Add($"[{def.Name}: {displayValue}]");
                }

                return $"{commandName} {string.Join(" ", parts)}\n";
            }

            /// <summary>
            /// Gets argument by name, returns null if not found
            /// </summary>
            public string GetNamed(string name)
            {
                return _namedArgs.TryGetValue(name, out string value) ? value : null;
            }

            /// <summary>
            /// Gets argument by name or falls back to positional index
            /// </summary>
            public string GetArgument(string name, int positionalIndex)
            {
                // Try named first
                if (_namedArgs.TryGetValue(name, out string value))
                    return value;

                // Fall back to positional
                if (positionalIndex >= 0 && positionalIndex < _positionalArgs.Count)
                    return _positionalArgs[positionalIndex];

                return null;
            }

            /// <summary>
            /// Gets positional argument at index
            /// </summary>
            public string GetPositional(int index)
            {
                return index >= 0 && index < _positionalArgs.Count ? _positionalArgs[index] : null;
            }

            /// <summary>
            /// Checks if a named argument exists
            /// </summary>
            public bool HasNamed(string name)
            {
                return _namedArgs.ContainsKey(name);
            }

            /// <summary>
            /// Gets the count of positional arguments
            /// </summary>
            public int PositionalCount => _positionalArgs.Count;

            /// <summary>
            /// Gets the count of named arguments
            /// </summary>
            public int NamedCount => _namedArgs.Count;

            /// <summary>
            /// Gets the total count of all arguments
            /// </summary>
            public int TotalCount => _allArgs.Count;

            /// <summary>
            /// Gets all positional arguments as a list
            /// </summary>
            public List<string> GetAllPositional() => new List<string>(_positionalArgs);

            /// <summary>
            /// Gets all named argument names
            /// </summary>
            public IEnumerable<string> GetNamedArgumentNames() => _namedArgs.Keys;

            /// <summary>
            /// Gets all valid argument definitions
            /// </summary>
            public List<ArgumentDefinition> GetValidArguments() => _validArguments;
        }

        /// <summary>
        /// Parses arguments with support for both quoted strings and named arguments.
        /// First handles quoted arguments, then parses named arguments.
        /// </summary>
        public static ParsedArguments ParseArguments(List<string> args)
        {
            // First, parse quoted arguments to handle multi-word strings
            var quotedParsed = ParseQuotedArguments(args);
            
            // Then create ParsedArguments which will identify named vs positional
            return new ParsedArguments(quotedParsed);
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
        /// Execute command with automatic logging and quote parsing.
        /// Automatically parses quoted arguments before passing to command logic.
        /// </summary>
        public static string Run(List<string> args, Func<string> action)
        {
            // Make a copy of original args for logging
            var originalArgs = args != null ? new List<string>(args) : new List<string>();
            
            // Parse quoted arguments BEFORE passing to action
            // This reconstructs 'multi word' arguments that were split by TaleWorlds
            var parsedArgs = CommandBase.ParseQuotedArguments(args);
            
            // Replace the args list contents with parsed args
            if (args != null && parsedArgs != null)
            {
                args.Clear();
                args.AddRange(parsedArgs);
            }

            // Get command name using parsed args for cleaner logging
            string commandName = GetCallingCommandName(parsedArgs);

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
        /// Execute command with automatic logging and quote parsing using CommandResult.
        /// Automatically parses quoted arguments before passing to command logic.
        /// </summary>
        public static CommandResult Run(List<string> args, Func<CommandResult> action)
        {
            // Make a copy of original args for logging
            var originalArgs = args != null ? new List<string>(args) : new List<string>();
            
            // Parse quoted arguments BEFORE passing to action
            // This reconstructs 'multi word' arguments that were split by TaleWorlds
            var parsedArgs = CommandBase.ParseQuotedArguments(args);
            
            // Replace the args list contents with parsed args
            if (args != null && parsedArgs != null)
            {
                args.Clear();
                args.AddRange(parsedArgs);
            }
            
            // Get command name using parsed args for cleaner logging
            string commandName = GetCallingCommandName(parsedArgs);
            
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
            catch (Exception ex)
            {
                // If reflection fails, log the error for debugging
                System.Diagnostics.Debug.WriteLine($"[CommandLogger] Failed to get calling command name: {ex.Message}");
            }
            
            // Fallback: Use generic name with arguments if available
            if (args != null && args.Count > 0)
            {
                return "gm.command " + string.Join(" ", args);
            }
            
            return "gm.command";
        }
    }
}