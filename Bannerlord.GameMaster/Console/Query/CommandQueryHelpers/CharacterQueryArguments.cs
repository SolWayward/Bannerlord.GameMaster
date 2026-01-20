using Bannerlord.GameMaster.Characters;
using System;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;

namespace Bannerlord.GameMaster.Console.Query.CommandQueryHelpers;

/// <summary>
/// Query arguments specific to character object queries (including heroes, troops, templates, NPCs)
/// </summary>
public struct CharacterQueryArguments
{
    public QueryArguments QueryArgs;
    public CharacterTypes Types;
    public List<CultureObject> Cultures;
    public int Tier;

    public CharacterQueryArguments(
        string query,
        CharacterTypes types,
        List<CultureObject> cultures,
        int tier,
        string sortBy,
        bool sortDesc)
    {
        QueryArgs = new(query, sortBy, sortDesc);
        Types = types;
        Cultures = cultures ?? new();
        Tier = tier;
    }

    /// <summary>
    /// Build a readable criteria string for character queries
    /// </summary>
    public readonly string GetCriteriaString()
    {
        List<string> parts = new();

        if (!string.IsNullOrEmpty(QueryArgs.Query))
            parts.Add($"search: '{QueryArgs.Query}'");

        if (Types != CharacterTypes.None)
        {
            CharacterTypes types = Types;
            IEnumerable<string> typeList = Enum.GetValues(typeof(CharacterTypes))
                .Cast<CharacterTypes>()
                .Where(t => t != CharacterTypes.None && types.HasFlag(t))
                .Select(t => t.ToString().ToLower());
            parts.Add($"types: {string.Join(", ", typeList)}");
        }

        if (Cultures != null && Cultures.Count > 0)
        {
            IEnumerable<string> cultureNames = Cultures.Select(c => c.Name.ToString());
            parts.Add($"cultures: {string.Join(", ", cultureNames)}");
        }

        if (Tier >= 0)
            parts.Add($"tier: {Tier}");

        if (!string.IsNullOrEmpty(QueryArgs.SortBy) && QueryArgs.SortBy != "id")
            parts.Add($"sort: {QueryArgs.SortBy}{(QueryArgs.SortDesc ? " (desc)" : " (asc)")}");

        return parts.Count > 0 ? string.Join(", ", parts) : "all characters";
    }
}
