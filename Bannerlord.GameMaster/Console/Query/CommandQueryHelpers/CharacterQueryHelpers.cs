using Bannerlord.GameMaster.Characters;
using Bannerlord.GameMaster.Cultures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.ObjectSystem;

namespace Bannerlord.GameMaster.Console.Query.CommandQueryHelpers;

/// <summary>
/// Helper methods for CharacterObject query commands.
/// Parses arguments for querying ALL CharacterObjects (heroes, troops, templates, NPCs).
/// </summary>
public static class CharacterQueryHelpers
{
    // MARK: Character Type Keywords
    private static readonly HashSet<string> TypeKeywords = new(StringComparer.OrdinalIgnoreCase)
    {
        // Character Classification
        "hero", "troop", "template", "npc",
        // Hero-Specific
        "lord", "wanderer", "notable", "child",
        // Gender
        "female", "male",
        // Character State
        "original", "blgm"
    };

    // MARK: Tier Keywords
    private static readonly HashSet<string> TierKeywords = new(StringComparer.OrdinalIgnoreCase)
    {
        "tier0", "tier1", "tier2", "tier3", "tier4", "tier5", "tier6", "tier6plus"
    };

    // MARK: Culture Keywords (individual cultures)
    private static readonly HashSet<string> CultureKeywords = new(StringComparer.OrdinalIgnoreCase)
    {
        "empire", "vlandia", "sturgia", "aserai", "khuzait", "battania", "nord", "bandit"
    };

    // MARK: Culture Group Keywords
    private static readonly HashSet<string> CultureGroupKeywords = new(StringComparer.OrdinalIgnoreCase)
    {
        "all_cultures", "main_cultures", "bandit_cultures"
    };

    /// <summary>
    /// Parse command arguments into CharacterQueryArguments struct.
    /// Supports keywords: hero, troop, template, npc, lord, wanderer, notable, 
    ///                    child, female, male, original, blgm, tier0-6
    /// Supports culture keywords: empire, vlandia, sturgia, etc.
    /// Supports culture groups: all_cultures, main_cultures, bandit_cultures
    /// </summary>
    public static CharacterQueryArguments ParseCharacterQueryArguments(List<string> args)
    {
        if (args == null || args.Count == 0)
            return new("", CharacterTypes.None, null, -1, "id", false);

        List<string> searchTerms = new();
        List<string> typeTerms = new();
        List<CultureObject> cultures = new();
        int tier = -1;
        string sortBy = "id";
        bool sortDesc = false;

        foreach (string arg in args)
        {
            // Check for sort parameters
            if (arg.StartsWith("sort:", StringComparison.OrdinalIgnoreCase))
            {
                CommonQueryHelpers.ParseSortParameter(arg, ref sortBy, ref sortDesc);
            }
            // Check for tier keywords
            else if (TierKeywords.Contains(arg))
            {
                int parsedTier = ParseCharacterTierKeyword(arg);
                if (parsedTier >= 0)
                {
                    tier = parsedTier;
                    // Also add to type terms for type flag filtering
                    typeTerms.Add(arg);
                }
            }
            // Check for culture group keywords
            else if (CultureGroupKeywords.Contains(arg))
            {
                List<CultureObject> groupCultures = GetCultureGroup(arg);
                foreach (CultureObject culture in groupCultures)
                {
                    if (!cultures.Contains(culture))
                        cultures.Add(culture);
                }
            }
            // Check for individual culture keywords
            else if (CultureKeywords.Contains(arg))
            {
                CultureObject culture = GetCultureByKeyword(arg);
                if (culture != null && !cultures.Contains(culture))
                    cultures.Add(culture);
            }
            // Check for comma-separated cultures (e.g., "vlandia,battania,empire")
            else if (arg.Contains(",") && !arg.StartsWith("sort:"))
            {
                List<CultureObject> parsedCultures = ParseCultureList(arg);
                foreach (CultureObject culture in parsedCultures)
                {
                    if (!cultures.Contains(culture))
                        cultures.Add(culture);
                }
            }
            // Check for type keywords
            else if (TypeKeywords.Contains(arg))
            {
                typeTerms.Add(arg);
            }
            // Otherwise treat as search term
            else
            {
                searchTerms.Add(arg);
            }
        }

        string query = string.Join(" ", searchTerms).Trim();
        CharacterTypes types = ParseCharacterTypes(typeTerms);

        return new(query, types, cultures.Count > 0 ? cultures : null, tier, sortBy, sortDesc);
    }

    /// <summary>
    /// Parse character tier keyword into tier number
    /// </summary>
    public static int ParseCharacterTierKeyword(string tierKeyword)
    {
        return tierKeyword.ToLower() switch
        {
            "tier0" => 0,
            "tier1" => 1,
            "tier2" => 2,
            "tier3" => 3,
            "tier4" => 4,
            "tier5" => 5,
            "tier6" => 6,
            "tier6plus" => 6,
            _ => -1
        };
    }

    /// <summary>
    /// Parse a single type string into CharacterTypes flag
    /// </summary>
    public static CharacterTypes ParseCharacterType(string typeString)
    {
        return typeString.ToLower() switch
        {
            // Character Classification
            "hero" => CharacterTypes.IsHero,
            "troop" => CharacterTypes.IsTroop,
            "template" => CharacterTypes.IsTemplate,
            "npc" => CharacterTypes.IsNPC,

            // Hero-Specific
            "lord" => CharacterTypes.IsLord,
            "wanderer" => CharacterTypes.IsWanderer,
            "notable" => CharacterTypes.IsNotable,
            "child" => CharacterTypes.IsChild,

            // Gender
            "female" => CharacterTypes.IsFemale,
            "male" => CharacterTypes.IsMale,

            // Character State
            "original" => CharacterTypes.IsOriginalCharacter,
            "blgm" => CharacterTypes.IsBlgmCreated,

            // Tier (via keywords)
            "tier0" => CharacterTypes.Tier0,
            "tier1" => CharacterTypes.Tier1,
            "tier2" => CharacterTypes.Tier2,
            "tier3" => CharacterTypes.Tier3,
            "tier4" => CharacterTypes.Tier4,
            "tier5" => CharacterTypes.Tier5,
            "tier6" => CharacterTypes.Tier6Plus,
            "tier6plus" => CharacterTypes.Tier6Plus,

            // Culture (via keywords)
            "empire" => CharacterTypes.Empire,
            "vlandia" => CharacterTypes.Vlandia,
            "sturgia" => CharacterTypes.Sturgia,
            "aserai" => CharacterTypes.Aserai,
            "khuzait" => CharacterTypes.Khuzait,
            "battania" => CharacterTypes.Battania,
            "nord" => CharacterTypes.Nord,
            "bandit" => CharacterTypes.Bandit,

            _ => CharacterTypes.None
        };
    }

    /// <summary>
    /// Parse multiple type strings and combine into CharacterTypes flags
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
    /// Get culture object by keyword
    /// </summary>
    public static CultureObject GetCultureByKeyword(string keyword)
    {
        return keyword.ToLower() switch
        {
            "empire" => CultureLookup.Empire,
            "vlandia" => CultureLookup.Vlandia,
            "sturgia" => CultureLookup.Sturgia,
            "aserai" => CultureLookup.Aserai,
            "khuzait" => CultureLookup.Khuzait,
            "battania" => CultureLookup.Battania,
            "nord" => CultureLookup.Nord,
            "bandit" => CultureLookup.Looters, // Default bandit culture
            _ => null
        };
    }

    /// <summary>
    /// Get list of cultures for a culture group keyword
    /// </summary>
    public static List<CultureObject> GetCultureGroup(string groupKeyword)
    {
        return groupKeyword.ToLower() switch
        {
            "all_cultures" => CultureLookup.AllCultures,
            "main_cultures" => CultureLookup.MainCultures,
            "bandit_cultures" => CultureLookup.BanditCultures,
            _ => new()
        };
    }

    /// <summary>
    /// Parse comma-separated culture list (e.g., "vlandia,battania,empire")
    /// </summary>
    public static List<CultureObject> ParseCultureList(string cultureListArg)
    {
        List<CultureObject> cultures = new();
        string[] parts = cultureListArg.Split(',');

        foreach (string part in parts)
        {
            string trimmed = part.Trim().ToLower();

            // Check for culture group first
            if (CultureGroupKeywords.Contains(trimmed))
            {
                List<CultureObject> groupCultures = GetCultureGroup(trimmed);
                foreach (CultureObject culture in groupCultures)
                {
                    if (!cultures.Contains(culture))
                        cultures.Add(culture);
                }
            }
            else
            {
                // Try as individual culture
                CultureObject culture = GetCultureByKeyword(trimmed);
                if (culture != null && !cultures.Contains(culture))
                    cultures.Add(culture);
            }
        }

        return cultures;
    }

    /// <summary>
    /// Build detailed info string for a CharacterObject
    /// </summary>
    public static string BuildCharacterInfo(CharacterObject character)
    {
        StringBuilder sb = new();

        sb.AppendLine($"ID: {character.StringId}");
        sb.AppendLine($"Name: {character.Name}");
        sb.AppendLine($"Classification: {character.GetCharacterClassification()}");
        sb.AppendLine($"Gender: {(character.IsFemale ? "Female" : "Male")}");
        sb.AppendLine($"Culture: {character.Culture?.Name?.ToString() ?? "None"}");

        if (!character.IsHero)
        {
            sb.AppendLine($"Tier: {character.GetBattleTier()}");
            sb.AppendLine($"Level: {character.Level}");
            sb.AppendLine($"Formation: {character.DefaultFormationClass}");
        }
        else if (character.HeroObject != null)
        {
            Hero hero = character.HeroObject;
            sb.AppendLine($"Age: {(int)hero.Age}");
            sb.AppendLine($"Clan: {hero.Clan?.Name?.ToString() ?? "None"}");
            if (hero.CurrentSettlement != null)
                sb.AppendLine($"Location: {hero.CurrentSettlement.Name}");
        }

        // Character types
        CharacterTypes types = character.GetCharacterTypes();
        sb.AppendLine($"Types: {types}");

        return sb.ToString();
    }

    /// <summary>
    /// Build equipment info string for a CharacterObject.
    /// Reuses pattern from TroopQueryHelpers.
    /// </summary>
    public static string BuildEquipmentInfo(CharacterObject character)
    {
        if (character.FirstBattleEquipment == null)
            return "Equipment: None\n";

        Equipment equipment = character.FirstBattleEquipment;
        List<string> items = new();
        for (int i = 0; i < 12; i++)
        {
            ItemObject item = equipment[i].Item;
            if (item != null)
            {
                items.Add(item.Name.ToString());
            }
        }
        return items.Count > 0
            ? "Equipment: " + string.Join(", ", items) + "\n"
            : "Equipment: None\n";
    }

    /// <summary>
    /// Build upgrade paths info string for a CharacterObject.
    /// Reuses pattern from TroopQueryHelpers.
    /// </summary>
    public static string BuildUpgradeInfo(CharacterObject character)
    {
        if (character.UpgradeTargets == null || character.UpgradeTargets.Length == 0)
            return "Upgrades: None\n";

        IEnumerable<string> upgrades = character.UpgradeTargets.Select(u => u.Name.ToString());
        return "Upgrades: " + string.Join(", ", upgrades) + "\n";
    }
}
