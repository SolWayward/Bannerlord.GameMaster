using System;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Settlements;
using Bannerlord.GameMaster.Common.Interfaces;
using Bannerlord.GameMaster.Console.Common;
using Bannerlord.GameMaster.Console.Common.Formatting;

namespace Bannerlord.GameMaster.Settlements
{
    /// <summary>
    /// Provides utility methods for querying Settlement entities
    /// </summary>
    public static class SettlementQueries
    {
        /// <summary>
        /// Finds a settlement with the specified ID, using case-insensitive comparison
        /// </summary>
        /// <param name="settlementId">The string ID of the settlement to find</param>
        /// <returns>The matching Settlement, or null if not found</returns>
        public static Settlement GetSettlementById(string settlementId)
        {
            return Settlement.All.FirstOrDefault(s => 
                s.StringId.Equals(settlementId, StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// Main unified method to find settlements by search string and type flags
        /// </summary>
        /// <param name="query">Optional case-insensitive substring to filter by name or ID</param>
        /// <param name="requiredTypes">Settlement type flags (AND logic by default)</param>
        /// <param name="matchAll">If true, settlement must have ALL flags. If false, ANY flag</param>
        /// <param name="sortBy">Sort field (id, name, prosperity, owner, kingdom, culture, or any SettlementType flag)</param>
        /// <param name="sortDescending">True for descending, false for ascending</param>
        /// <returns>List of settlements matching all criteria</returns>
        public static List<Settlement> QuerySettlements(
            string query = "",
            SettlementTypes requiredTypes = SettlementTypes.None,
            bool matchAll = true,
            string sortBy = "id",
            bool sortDescending = false)
        {
            IEnumerable<Settlement> settlements = Settlement.All;

            // Filter by name/ID if provided
            if (!string.IsNullOrEmpty(query))
            {
                string lowerFilter = query.ToLower();
                settlements = settlements.Where(s =>
                    s.Name.ToString().ToLower().Contains(lowerFilter) ||
                    s.StringId.ToLower().Contains(lowerFilter));
            }

            // Filter by types
            if (requiredTypes != SettlementTypes.None)
            {
                settlements = settlements.Where(s =>
                    matchAll ? s.HasAllTypes(requiredTypes) : s.HasAnyType(requiredTypes));
            }

            // Apply sorting
            settlements = ApplySorting(settlements, sortBy, sortDescending);

            return settlements.ToList();
        }

        /// <summary>
        /// Apply sorting to settlements collection
        /// </summary>
        private static IEnumerable<Settlement> ApplySorting(
            IEnumerable<Settlement> settlements,
            string sortBy,
            bool descending)
        {
            sortBy = sortBy.ToLower();

            // Check if sortBy matches a SettlementTypes flag
            if (Enum.TryParse<SettlementTypes>(sortBy, true, out var settlementType) && 
                settlementType != SettlementTypes.None)
            {
                return descending
                    ? settlements.OrderByDescending(s => s.GetSettlementTypes().HasFlag(settlementType))
                    : settlements.OrderBy(s => s.GetSettlementTypes().HasFlag(settlementType));
            }

            // Sort by standard fields
            IOrderedEnumerable<Settlement> orderedSettlements = sortBy switch
            {
                "name" => descending
                    ? settlements.OrderByDescending(s => s.Name.ToString())
                    : settlements.OrderBy(s => s.Name.ToString()),
                "prosperity" => descending
                    ? settlements.OrderByDescending(s => GetProsperityValue(s))
                    : settlements.OrderBy(s => GetProsperityValue(s)),
                "owner" => descending
                    ? settlements.OrderByDescending(s => s.OwnerClan?.Name?.ToString() ?? "")
                    : settlements.OrderBy(s => s.OwnerClan?.Name?.ToString() ?? ""),
                "kingdom" => descending
                    ? settlements.OrderByDescending(s => s.MapFaction?.Name?.ToString() ?? "")
                    : settlements.OrderBy(s => s.MapFaction?.Name?.ToString() ?? ""),
                "culture" => descending
                    ? settlements.OrderByDescending(s => s.Culture?.Name?.ToString() ?? "")
                    : settlements.OrderBy(s => s.Culture?.Name?.ToString() ?? ""),
                _ => descending  // default to id
                    ? settlements.OrderByDescending(s => s.StringId)
                    : settlements.OrderBy(s => s.StringId)
            };

            return orderedSettlements;
        }

        /// <summary>
        /// Helper method to get prosperity value for sorting
        /// </summary>
        private static float GetProsperityValue(Settlement settlement)
        {
            if (settlement.IsTown && settlement.Town != null)
                return settlement.Town.Prosperity;
            if (settlement.IsVillage && settlement.Village != null)
                return settlement.Village.Hearth;
            return 0f;
        }

        /// <summary>
        /// Parse a string into SettlementTypes enum value
        /// </summary>
        public static SettlementTypes ParseSettlementType(string typeString)
        {
            // Handle common aliases
            var normalized = typeString.ToLower() switch
            {
                "town" => "Town",
                "castle" => "Castle",
                "city" => "City",
                "village" => "Village",
                "hideout" => "Hideout",
                "player" => "PlayerOwned",
                "playerowned" => "PlayerOwned",
                "besieged" => "Besieged",
                "siege" => "Besieged",
                "raided" => "Raided",
                "empire" => "Empire",
                "vlandia" => "Vlandia",
                "sturgia" => "Sturgia",
                "aserai" => "Aserai",
                "khuzait" => "Khuzait",
                "battania" => "Battania",
                "nord" => "Nord",
                "lowprosperity" => "LowProsperity",
                "mediumprosperity" => "MediumProsperity",
                "highprosperity" => "HighProsperity",
                "low" => "LowProsperity",
                "medium" => "MediumProsperity",
                "high" => "HighProsperity",
                _ => typeString
            };

            return Enum.TryParse<SettlementTypes>(normalized, true, out var result)
                ? result : SettlementTypes.None;
        }

        /// <summary>
        /// Parse multiple strings and combine into SettlementTypes flags
        /// </summary>
        public static SettlementTypes ParseSettlementTypes(IEnumerable<string> typeStrings)
        {
            SettlementTypes combined = SettlementTypes.None;
            foreach (var typeString in typeStrings)
            {
                var parsed = ParseSettlementType(typeString);
                if (parsed != SettlementTypes.None)
                    combined |= parsed;
            }
            return combined;
        }

        /// <summary>
        /// Returns a formatted string listing settlement details with aligned columns
        /// </summary>
        public static string GetFormattedDetails(List<Settlement> settlements)
        {
            if (settlements.Count == 0)
                return "";

            return ColumnFormatter<Settlement>.FormatList(
                settlements,
                s => s.StringId,
                s => s.Name.ToString(),
                s => {
                    string type = s.IsTown ? "City"
                        : s.IsCastle ? "Castle"
                        : s.IsVillage ? "Village"
                        : s.IsHideout ? "Hideout"
                        : "Unknown";
                    return $"[{type}]";
                },
                s => $"Owner: {s.OwnerClan?.Name?.ToString() ?? "None"}",
                s => $"Kingdom: {s.MapFaction?.Name?.ToString() ?? "None"}",
                s => $"Culture: {s.Culture?.Name?.ToString() ?? "None"}",
                s => {
                    if ((s.IsTown | s.IsCastle) && s.Town != null)
                        return $"Prosperity: {s.Town.Prosperity:F0}";
                    else if (s.IsVillage && s.Village != null)
                        return $"Hearth: {s.Village.Hearth:F0}";
                    return "";
                }
            );
        }
    }

    /// <summary>
    /// Wrapper class implementing IEntityQueries interface for Settlement entities
    /// </summary>
    public class SettlementQueriesWrapper : IEntityQueries<Settlement, SettlementTypes>
    {
        public Settlement GetById(string id) => SettlementQueries.GetSettlementById(id);
        public List<Settlement> Query(string query, SettlementTypes types, bool matchAll)
            => SettlementQueries.QuerySettlements(query, types, matchAll);
        public SettlementTypes ParseType(string typeString) => SettlementQueries.ParseSettlementType(typeString);
        public SettlementTypes ParseTypes(IEnumerable<string> typeStrings)
            => SettlementQueries.ParseSettlementTypes(typeStrings);
        public string GetFormattedDetails(List<Settlement> entities)
            => SettlementQueries.GetFormattedDetails(entities);
    }
}