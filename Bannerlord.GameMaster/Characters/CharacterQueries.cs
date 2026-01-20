using System;
using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using Bannerlord.GameMaster.Console.Common.Formatting;
using TaleWorlds.Library;

namespace Bannerlord.GameMaster.Characters
{
    /// <summary>
    /// Provides query methods for searching, finding, filtering, or sorting CharacterObject entities.
    /// This is a COMPREHENSIVE query - returns ALL CharacterObjects by default (heroes, troops, templates, NPCs).
    /// Use CharacterTypes flags to filter specific types. Use TroopQueries for filtered combat troops only.
    /// </summary>
    public static class CharacterQueries
    {
        #region Query

        /// <summary>
        /// Finds a character with the specified characterId, Fast but case sensitive (all string ids SHOULD be lower case) <br />
        /// Use QueryCharacterObjects().FirstOrDefault() to find character with case insensitive partial name or partial stringIds <br />
        /// Example: QueryCharacterObjects("imperial").FirstOrDefault()
        /// </summary>
        public static CharacterObject GetCharacterById(string characterId) => CharacterObject.Find(characterId);

        /// <summary>
        /// Performance focused method to find CharacterObjects matching multiple parameters. All parameters are optional and can be used with none, one or a combination of any parameters<br />
        /// By default returns EVERYTHING (heroes, troops, templates, NPCs, etc.)
        /// Use CharacterTypes flags to filter specific types.<br />
        /// Note: <paramref name="query"/> parameter is a string used that will match partial character names or partial stringIds
        /// </summary>
        /// <param name="query">Optional case-insensitive substring to filter by name or ID</param>
        /// <param name="requiredTypes">CharacterTypes flags to filter by</param>
        /// <param name="matchAll">If true, character must have ALL flags. If false, character must have ANY flag</param>
        /// <param name="cultures">Optional list of CultureObjects to filter by</param>
        /// <param name="tierFilter">Optional tier filter (0-6+, -1 for no filter)</param>
        /// <param name="sortBy">Sort field (id, name, tier, level, culture, formation, or any CharacterTypes flag)</param>
        /// <param name="sortDescending">True for descending, false for ascending</param>
        /// <returns>List of CharacterObjects matching all criteria</returns>
        public static MBReadOnlyList<CharacterObject> QueryCharacterObjects(
            string query = "",
            CharacterTypes requiredTypes = CharacterTypes.None,
            bool matchAll = true,
            List<CultureObject> cultures = null,
            int tierFilter = -1,
            string sortBy = "id",
            bool sortDescending = false)
        {
            // Get all character objects
            MBReadOnlyList<CharacterObject> allCharacters = CharacterObject.All;

            int estimatedCapacity = allCharacters.Count;
            MBReadOnlyList<CharacterObject> results = new(estimatedCapacity);

            bool hasQuery = !string.IsNullOrEmpty(query);
            bool hasTypes = requiredTypes != CharacterTypes.None;
            bool hasCultureFilter = cultures != null && cultures.Count > 0;
            bool hasTierFilter = tierFilter >= 0;

            // MARK: Filter characters (NO EXCLUSIONS - includes everything by default)
            for (int i = 0; i < allCharacters.Count; i++)
            {
                CharacterObject character = allCharacters[i];

                if (MatchesFilters(character, query, hasQuery, requiredTypes, hasTypes, matchAll, cultures, hasCultureFilter, tierFilter, hasTierFilter))
                {
                    results.Add(character);
                }
            }

            // MARK: Sorting - only if needed
            if (results.Count > 1)
            {
                IComparer<CharacterObject> comparer = GetCharacterComparer(sortBy, sortDescending);
                results.Sort(comparer);
            }

            return results;
        }

        /// <summary>
        /// Check if character matches all filter criteria
        /// </summary>
        public static bool MatchesFilters(
            CharacterObject character,
            string query,
            bool hasQuery,
            CharacterTypes requiredTypes,
            bool hasTypes,
            bool matchAll,
            List<CultureObject> cultures,
            bool hasCultureFilter,
            int tierFilter,
            bool hasTierFilter)
        {
            // Name/ID filter using OrdinalIgnoreCase (no string allocation)
            if (hasQuery)
            {
                bool nameMatch = character.Name.ToString().IndexOf(query, StringComparison.OrdinalIgnoreCase) >= 0;
                bool idMatch = character.StringId.IndexOf(query, StringComparison.OrdinalIgnoreCase) >= 0;
                if (!nameMatch && !idMatch)
                    return false;
            }

            // Culture filter
            if (hasCultureFilter)
            {
                if (character.Culture == null)
                    return false;

                bool cultureMatch = false;
                for (int i = 0; i < cultures.Count; i++)
                {
                    if (cultures[i] == character.Culture)
                    {
                        cultureMatch = true;
                        break;
                    }
                }
                if (!cultureMatch)
                    return false;
            }

            // Tier filter (exact match using GetBattleTier() - only for non-heroes)
            if (hasTierFilter)
            {
                // Heroes don't have tiers in the same sense, skip tier filter for heroes
                if (!character.IsHero && character.GetBattleTier() != tierFilter)
                    return false;
            }

            // Type filter
            if (hasTypes)
            {
                bool matches = matchAll
                    ? character.HasAllCharacterTypes(requiredTypes)
                    : character.HasAnyCharacterType(requiredTypes);
                if (!matches)
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Get comparer for character sorting
        /// </summary>
        public static IComparer<CharacterObject> GetCharacterComparer(string sortBy, bool descending)
        {
            sortBy = sortBy.ToLowerInvariant();

            // Check if sortBy matches a CharacterTypes flag
            if (Enum.TryParse<CharacterTypes>(sortBy, true, out CharacterTypes characterType) && characterType != CharacterTypes.None)
            {
                return Comparer<CharacterObject>.Create((a, b) =>
                {
                    bool aHas = a.GetCharacterTypes().HasFlag(characterType);
                    bool bHas = b.GetCharacterTypes().HasFlag(characterType);
                    int result = aHas.CompareTo(bHas);
                    return descending ? -result : result;
                });
            }

            // Standard field comparers
            return sortBy switch
            {
                "name" => Comparer<CharacterObject>.Create((a, b) =>
                {
                    int result = string.Compare(a.Name.ToString(), b.Name.ToString(), StringComparison.Ordinal);
                    return descending ? -result : result;
                }),
                "tier" => Comparer<CharacterObject>.Create((a, b) =>
                {
                    // Heroes get tier -1 for sorting purposes
                    int aTier = a.IsHero ? -1 : a.Tier;
                    int bTier = b.IsHero ? -1 : b.Tier;
                    int result = aTier.CompareTo(bTier);
                    return descending ? -result : result;
                }),
                "level" => Comparer<CharacterObject>.Create((a, b) =>
                {
                    int result = a.Level.CompareTo(b.Level);
                    return descending ? -result : result;
                }),
                "culture" => Comparer<CharacterObject>.Create((a, b) =>
                {
                    int result = string.Compare(
                        a.Culture?.Name?.ToString() ?? "",
                        b.Culture?.Name?.ToString() ?? "",
                        StringComparison.Ordinal);
                    return descending ? -result : result;
                }),
                "formation" => Comparer<CharacterObject>.Create((a, b) =>
                {
                    int result = a.DefaultFormationClass.CompareTo(b.DefaultFormationClass);
                    return descending ? -result : result;
                }),
                "classification" => Comparer<CharacterObject>.Create((a, b) =>
                {
                    int result = string.Compare(
                        a.GetCharacterClassification(),
                        b.GetCharacterClassification(),
                        StringComparison.Ordinal);
                    return descending ? -result : result;
                }),
                _ => Comparer<CharacterObject>.Create((a, b) =>  // default: id
                {
                    int result = string.Compare(a.StringId, b.StringId, StringComparison.Ordinal);
                    return descending ? -result : result;
                })
            };
        }

        #endregion

        #region Parsing / Formatting

        /// <summary>
        /// Parse a string into CharacterTypes enum value
        /// </summary>
        public static CharacterTypes ParseCharacterType(string typeString)
        {
            if (Enum.TryParse<CharacterTypes>(typeString, true, out CharacterTypes result))
                return result;
            return CharacterTypes.None;
        }

        /// <summary>
        /// Parse multiple strings and combine into CharacterTypes flags
        /// </summary>
        public static CharacterTypes ParseCharacterTypes(IEnumerable<string> typeStrings)
        {
            CharacterTypes combined = CharacterTypes.None;
            foreach (string typeString in typeStrings)
            {
                CharacterTypes parsed = ParseCharacterType(typeString);
                if (parsed != CharacterTypes.None)
                    combined |= parsed;
            }

            return combined;
        }

        /// <summary>
        /// Returns a formatted string listing character details with aligned columns
        /// </summary>
        public static string GetFormattedDetails(List<CharacterObject> characters)
        {
            if (characters.Count == 0)
                return "";

            return ColumnFormatter<CharacterObject>.FormatList(
                characters,
                c => c.StringId,
                c => c.Name.ToString(),
                c => $"[{c.GetCharacterClassification()}]",
                c => $"Gender: {(c.IsFemale ? "Female" : "Male")}",
                c => c.IsHero ? "Tier: N/A" : $"Tier: {c.GetBattleTier()}",
                c => $"Level: {c.Level}",
                c => $"Culture: {c.Culture?.Name?.ToString() ?? "None"}"
            );
        }

        #endregion
    }
}
