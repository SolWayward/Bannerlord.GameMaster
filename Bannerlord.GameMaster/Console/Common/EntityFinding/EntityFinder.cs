using System;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.ObjectSystem;
using Bannerlord.GameMaster.Console.Common.Execution;

namespace Bannerlord.GameMaster.Console.Common.EntityFinding
{
    /// <summary>
    /// Provides core resolution logic for entity finder operations.
    /// Type-specific finders are located in separate classes (HeroFinder, ClanFinder, etc.).
    /// </summary>
    public static class EntityFinder
    {
        // MARK: ResolveMultiMatches
        //  Core resolution logic for handling multiple entity matches

        /// <summary>
        /// Resolves a single entity from multiple matches using smart matching logic.
        /// Prioritizes In Order Exact name match, Name prefix match, Exact ID match, ID prefix match, Shortest ID match, Error on name-only matches
        /// </summary>
        /// <typeparam name="T">Entity type (must inherit from MBObjectBase)</typeparam>
        /// <param name="matches">List of matched entities</param>
        /// <param name="query">Original query string</param>
        /// <param name="getStringId">Function to extract StringId from entity</param>
        /// <param name="getName">Function to extract Name from entity</param>
        /// <param name="formatDetails">Function to format entity list for error messages</param>
        /// <param name="entityType">Name of entity type for error messages (e.g., "hero")</param>
        /// <returns>EntityFinderResult containing selected entity or error message</returns>
        internal static EntityFinderResult<T> ResolveMultipleMatches<T>(
            List<T> matches,
            string query,
            Func<T, string> getStringId,
            Func<T, string> getName,
            Func<List<T>, string> formatDetails,
            string entityType) where T : MBObjectBase
        {
            // Collect all matches for name priority checking
            List<T> allMatches = new();
            List<T> idMatches = new();
            List<T> nameMatches = new();

            // DEBUG: Log resolution process
            if (CommandLogger.IsEnabled)
            {
                CommandLogger.Log($"[BLGM_DEBUG] ResolveMultipleMatches for query '{query}' with {matches.Count} matches");
            }

            foreach (T entity in matches)
            {
                string entityId = getStringId(entity) ?? "";
                string entityName = getName(entity) ?? "";

                bool matchesId = entityId.IndexOf(query, StringComparison.OrdinalIgnoreCase) >= 0;
                bool matchesName = entityName.IndexOf(query, StringComparison.OrdinalIgnoreCase) >= 0;

                // DEBUG: Log match details
                if (CommandLogger.IsEnabled)
                {
                    CommandLogger.Log($"[BLGM_DEBUG]   Entity: Name='{entityName}' ID='{entityId}' | MatchesName={matchesName} MatchesID={matchesId}");
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
                CommandLogger.Log($"[BLGM_DEBUG] Categorization: allMatches={allMatches.Count}, idMatches={idMatches.Count}, nameMatches={nameMatches.Count}");
            }

            // MARK: Priority1 Exact Name
            // Priority 1: Check for exact name match across ALL matches
            List<T> exactNameMatches = allMatches.Where(e => getName(e).Equals(query, StringComparison.OrdinalIgnoreCase)).ToList();
            
            if (CommandLogger.IsEnabled)
            {
                CommandLogger.Log($"[BLGM_DEBUG] Priority 1 - Exact name matches: {exactNameMatches.Count}");
            }
            
            if (exactNameMatches.Count == 1)
            {
                if (CommandLogger.IsEnabled)
                {
                    CommandLogger.Log($"[BLGM_DEBUG] SELECTED by Priority 1: Name='{getName(exactNameMatches[0])}' ID='{getStringId(exactNameMatches[0])}'");
                }
                return EntityFinderResult<T>.Success(exactNameMatches[0]); // Exact name match wins immediately
            }
            else if (exactNameMatches.Count > 1)
            {
                return EntityFinderResult<T>.Error($"Error: Found {exactNameMatches.Count} {entityType}s with names exactly matching '{query}':\n" +
                    $"{formatDetails(exactNameMatches)}" +
                    $"Multiple entities have identical names. Please use their IDs.\n");
            }

            // MARK: Priority2 All prefix
            // Priority 2: Check for prefix matches across ALL matches
            List<T> prefixMatches = allMatches.Where(e => getName(e).StartsWith(query, StringComparison.OrdinalIgnoreCase)).ToList();
            
            if (CommandLogger.IsEnabled)
            {
                CommandLogger.Log($"[BLGM_DEBUG] Priority 2 - Prefix name matches: {prefixMatches.Count}, Total matches: {allMatches.Count}");
            }
            
            // Only auto-select if the prefix match is the ONLY match overall
            if (prefixMatches.Count == 1 && allMatches.Count == 1)
            {
                if (CommandLogger.IsEnabled)
                {
                    CommandLogger.Log($"[BLGM_DEBUG] SELECTED by Priority 2: Name='{getName(prefixMatches[0])}' ID='{getStringId(prefixMatches[0])}'");
                }
                return EntityFinderResult<T>.Success(prefixMatches[0]); // Single prefix match AND single overall match wins
            }
            else if (prefixMatches.Count > 1)
            {
                return EntityFinderResult<T>.Error($"Error: Found {prefixMatches.Count} {entityType}s with names starting with '{query}':\n" +
                    $"{formatDetails(prefixMatches)}" +
                    $"Please use a more specific name or use their IDs.\n");
            }
            else if (prefixMatches.Count == 1 && allMatches.Count > 1)
            {
                // There's a prefix match but also other substring matches - ambiguous
                return EntityFinderResult<T>.Error($"Error: Found {allMatches.Count} {entityType}s with names containing '{query}':\n" +
                    $"{formatDetails(allMatches)}" +
                    $"Please use a more specific name or use their IDs.\n");
            }
            
            // MARK: Priority3 Exact ID
            // Priority 3: Check for exact ID match
            foreach (T entity in idMatches)
            {
                string entityId = getStringId(entity) ?? "";
                if (entityId.Equals(query, StringComparison.OrdinalIgnoreCase))
                {
                    return EntityFinderResult<T>.Success(entity); // Exact ID match wins
                }
            }

            // Priority 4: Check for ID prefix matches
            // MARK: Priority4 ID prefix
            List<T> idPrefixMatches = allMatches.Where(e => getStringId(e).StartsWith(query, StringComparison.OrdinalIgnoreCase)).ToList();
            
            if (idPrefixMatches.Count == 1)
            {
                return EntityFinderResult<T>.Success(idPrefixMatches[0]); // Single ID prefix match wins
            }
            else if (idPrefixMatches.Count > 1)
            {
                return EntityFinderResult<T>.Error($"Error: Found {idPrefixMatches.Count} {entityType}s with IDs starting with '{query}':\n" +
                    $"{formatDetails(idPrefixMatches)}" +
                    $"Please use a more specific ID.\n");
            }

            // MARK: Priority5 Shortest ID
            // Priority 5: Use shortest ID match if available
            if (idMatches.Count > 0)
            {
                // Find the shortest ID
                T shortestMatch = idMatches.OrderBy(e => getStringId(e)?.Length ?? int.MaxValue).First();
                int shortestLength = getStringId(shortestMatch)?.Length ?? 0;

                // Check if there are multiple IDs with the same shortest length
                List<T> allShortestMatches = idMatches.Where(e => (getStringId(e)?.Length ?? 0) == shortestLength).ToList();

                if (allShortestMatches.Count == 1)
                {
                    return EntityFinderResult<T>.Success(shortestMatch);
                }
                else
                {
                    // Multiple IDs with same length - still ambiguous
                    return EntityFinderResult<T>.Error($"Error: Found {allShortestMatches.Count} {entityType}s with IDs matching query '{query}':\n" +
                        $"{formatDetails(allShortestMatches)}" +
                        $"These IDs have the same length and cannot be automatically selected.\n" +
                        $"Please use a more specific ID.\n");
                }
            }

            // MARK: Priority6: Multi Name
            // Priority 6: Only name matches remain
            if (nameMatches.Count > 0)
            {
                // Multiple substring matches (no exact or prefix matches)
                return EntityFinderResult<T>.Error($"Error: Found {nameMatches.Count} {entityType}s with names containing '{query}':\n" +
                    $"{formatDetails(nameMatches)}" +
                    $"Please use a more specific name or use their IDs.\n");
            }

            // Should never reach here, but safety fallback
            return EntityFinderResult<T>.Error($"Error: Found {matches.Count} {entityType}s matching query '{query}':\n" +
                $"{formatDetails(matches)}" +
                $"Please use a more specific name or ID.\n");
        }
    }
}
