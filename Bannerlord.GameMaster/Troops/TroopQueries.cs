using System;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.Core;
using TaleWorlds.CampaignSystem;
using TaleWorlds.ObjectSystem;
using Bannerlord.GameMaster.Common.Interfaces;
using Bannerlord.GameMaster.Console.Common;

namespace Bannerlord.GameMaster.Troops
{
    /// <summary>
    /// Provides utility methods for querying troop entities
    /// </summary>
    public static class TroopQueries
    {
        /// <summary>
        /// Finds a troop with the specified troopId
        /// </summary>
        public static CharacterObject GetTroopById(string troopId)
        {
            return MBObjectManager.Instance.GetObject<CharacterObject>(troopId);
        }

        /// <summary>
        /// Main unified method to find troops by search string and type flags
        /// CRITICAL: Heroes/Lords are NEVER troops - they are automatically excluded
        /// </summary>
        /// <param name="query">Optional case-insensitive substring to filter by name or ID</param>
        /// <param name="requiredTypes">Troop type flags that ALL must match (AND logic)</param>
        /// <param name="matchAll">If true, troop must have ALL flags. If false, troop must have ANY flag</param>
        /// <param name="tierFilter">Optional tier filter (0-6+, -1 for no filter)</param>
        /// <param name="sortBy">Sort field (id, name, tier, level, culture, occupation, formation, or any TroopTypes flag)</param>
        /// <param name="sortDescending">True for descending, false for ascending</param>
        /// <returns>List of troops matching all criteria</returns>
        public static List<CharacterObject> QueryTroops(
            string query = "",
            TroopTypes requiredTypes = TroopTypes.None,
            bool matchAll = true,
            int tierFilter = -1,
            string sortBy = "id",
            bool sortDescending = false)
        {
            IEnumerable<CharacterObject> troops =
                MBObjectManager.Instance.GetObjectTypeList<CharacterObject>();
            
            // CRITICAL: Filter out non-troops (heroes, NPCs, children, templates, etc.)
            troops = troops.Where(t => t.IsActualTroop());
            
            // Filter by name/ID
            if (!string.IsNullOrEmpty(query))
            {
                string lowerFilter = query.ToLower();
                troops = troops.Where(t =>
                    t.Name.ToString().ToLower().Contains(lowerFilter) ||
                    t.StringId.ToLower().Contains(lowerFilter));
            }
            
            // Filter by tier (exact match using GetBattleTier())
            if (tierFilter >= 0)
            {
                troops = troops.Where(t => t.GetBattleTier() == tierFilter);
            }
            
            // Filter by types
            if (requiredTypes != TroopTypes.None)
            {
                troops = troops.Where(t =>
                    matchAll ? t.HasAllTypes(requiredTypes) : t.HasAnyType(requiredTypes));
            }
            
            // Apply sorting
            troops = ApplySorting(troops, sortBy, sortDescending);
            
            return troops.ToList();
        }

        /// <summary>
        /// UNFILTERED query method to find ALL CharacterObjects (except heroes) by search string and type flags
        /// WARNING: This returns ALL character objects including NPCs, children, templates, etc.
        /// Use this for debugging/comparison purposes to see what QueryTroops() filters out.
        /// For normal gameplay, use QueryTroops() which filters to actual combat troops only.
        /// </summary>
        /// <param name="query">Optional case-insensitive substring to filter by name or ID</param>
        /// <param name="requiredTypes">Troop type flags that ALL must match (AND logic)</param>
        /// <param name="matchAll">If true, character must have ALL flags. If false, character must have ANY flag</param>
        /// <param name="tierFilter">Optional tier filter (0-6+, -1 for no filter)</param>
        /// <param name="sortBy">Sort field (id, name, tier, level, culture, occupation, formation, or any TroopTypes flag)</param>
        /// <param name="sortDescending">True for descending, false for ascending</param>
        /// <returns>List of ALL CharacterObjects (except heroes) matching criteria</returns>
        public static List<CharacterObject> QueryCharacterObjects(
            string query = "",
            TroopTypes requiredTypes = TroopTypes.None,
            bool matchAll = true,
            int tierFilter = -1,
            string sortBy = "id",
            bool sortDescending = false)
        {
            IEnumerable<CharacterObject> characters =
                MBObjectManager.Instance.GetObjectTypeList<CharacterObject>();
            
            // ONLY exclude heroes - include EVERYTHING else (NPCs, templates, children, etc.)
            characters = characters.Where(t => !t.IsHero);
            
            // Filter by name/ID
            if (!string.IsNullOrEmpty(query))
            {
                string lowerFilter = query.ToLower();
                characters = characters.Where(t =>
                    t.Name.ToString().ToLower().Contains(lowerFilter) ||
                    t.StringId.ToLower().Contains(lowerFilter));
            }
            
            // Filter by tier (exact match using GetBattleTier())
            if (tierFilter >= 0)
            {
                characters = characters.Where(t => t.GetBattleTier() == tierFilter);
            }
            
            // Filter by types
            if (requiredTypes != TroopTypes.None)
            {
                characters = characters.Where(t =>
                    matchAll ? t.HasAllTypes(requiredTypes) : t.HasAnyType(requiredTypes));
            }
            
            // Apply sorting
            characters = ApplySorting(characters, sortBy, sortDescending);
            
            return characters.ToList();
        }

        /// <summary>
        /// Apply sorting to troops collection
        /// </summary>
        private static IEnumerable<CharacterObject> ApplySorting(
            IEnumerable<CharacterObject> troops,
            string sortBy,
            bool descending)
        {
            sortBy = sortBy.ToLower();
            
            // Check if sortBy matches a TroopTypes flag
            if (Enum.TryParse<TroopTypes>(sortBy, true, out var troopType) && 
                troopType != TroopTypes.None)
            {
                return descending
                    ? troops.OrderByDescending(t => t.GetTroopTypes().HasFlag(troopType))
                    : troops.OrderBy(t => t.GetTroopTypes().HasFlag(troopType));
            }
            
            // Sort by standard fields
            IOrderedEnumerable<CharacterObject> orderedTroops = sortBy switch
            {
                "name" => descending
                    ? troops.OrderByDescending(t => t.Name.ToString())
                    : troops.OrderBy(t => t.Name.ToString()),
                "tier" => descending
                    ? troops.OrderByDescending(t => t.Tier)
                    : troops.OrderBy(t => t.Tier),
                "level" => descending
                    ? troops.OrderByDescending(t => t.Level)
                    : troops.OrderBy(t => t.Level),
                "culture" => descending
                    ? troops.OrderByDescending(t => t.Culture?.Name?.ToString() ?? "")
                    : troops.OrderBy(t => t.Culture?.Name?.ToString() ?? ""),
                // NOTE: Occupation removed - it's for HEROES not TROOPS
                "formation" => descending
                    ? troops.OrderByDescending(t => t.DefaultFormationClass)
                    : troops.OrderBy(t => t.DefaultFormationClass),
                _ => descending  // default to id
                    ? troops.OrderByDescending(t => t.StringId)
                    : troops.OrderBy(t => t.StringId)
            };
            
            return orderedTroops;
        }

        /// <summary>
        /// Parse a string into TroopTypes enum value with alias support
        /// </summary>
        public static TroopTypes ParseTroopType(string typeString)
        {
            // Handle common aliases
            var normalized = typeString.ToLower() switch
            {
                "2h" => "TwoHanded",
                "mounted" => "Mounted",
                "cav" => "Cavalry",
                "ha" => "HorseArcher",
                _ => typeString
            };
            
            return Enum.TryParse<TroopTypes>(normalized, true, out var result)
                ? result : TroopTypes.None;
        }

        /// <summary>
        /// Parse multiple strings and combine into TroopTypes flags
        /// </summary>
        public static TroopTypes ParseTroopTypes(IEnumerable<string> typeStrings)
        {
            TroopTypes combined = TroopTypes.None;
            foreach (var typeString in typeStrings)
            {
                var parsed = ParseTroopType(typeString);
                if (parsed != TroopTypes.None)
                    combined |= parsed;
            }
            return combined;
        }

        /// <summary>
        /// Returns a formatted string listing troop details with aligned columns
        /// </summary>
        public static string GetFormattedDetails(List<CharacterObject> troops)
        {
            if (troops.Count == 0)
                return "";

            return ColumnFormatter<CharacterObject>.FormatList(
                troops,
                t => t.StringId,
                t => t.Name.ToString(),
                t => $"[{t.GetTroopCategory()}]",
                t => $"Tier: {t.GetBattleTier()}",
                t => $"Level: {t.Level}",
                t => $"Culture: {t.Culture?.Name?.ToString() ?? "None"}",
                t => $"Formation: {t.DefaultFormationClass}"
            );
        }
    }

    /// <summary>
    /// Wrapper class implementing IEntityQueries interface for CharacterObject entities
    /// </summary>
    public class TroopQueriesWrapper : IEntityQueries<CharacterObject, TroopTypes>
    {
        public CharacterObject GetById(string id) => TroopQueries.GetTroopById(id);
        public List<CharacterObject> Query(string query, TroopTypes types, bool matchAll) => 
            TroopQueries.QueryTroops(query, types, matchAll);
        public TroopTypes ParseType(string typeString) => TroopQueries.ParseTroopType(typeString);
        public TroopTypes ParseTypes(IEnumerable<string> typeStrings) => 
            TroopQueries.ParseTroopTypes(typeStrings);
        public string GetFormattedDetails(List<CharacterObject> entities) => 
            TroopQueries.GetFormattedDetails(entities);
    }
}