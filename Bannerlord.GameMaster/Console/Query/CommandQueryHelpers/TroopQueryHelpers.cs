using Bannerlord.GameMaster.Troops;
using System;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;

namespace Bannerlord.GameMaster.Console.Query.CommandQueryHelpers;

/// <summary>
/// Helper methods for troop/CharacterObject query commands
/// </summary>
public static class TroopQueryHelpers
{
    /// <summary>
    /// Parse command arguments into TroopQueryArguments struct
    /// </summary>
    public static TroopQueryArguments ParseTroopQueryArguments(List<string> args)
    {
        if (args == null || args.Count == 0)
            return new("", TroopTypes.None, -1, "id", false);

        HashSet<string> typeKeywords = new(StringComparer.OrdinalIgnoreCase)
        {
            // Formation types
            "infantry", "ranged", "cavalry", "horsearcher", "mounted",
            // Troop line types
            "regular", "noble", "militia", "mercenary", "caravan", "peasant", "minorfaction",
            // Equipment types
            "shield", "twohanded", "2h", "polearm", "bow", "crossbow", "throwing", "throwingweapon",
            // Cultures
            "empire", "vlandia", "sturgia", "aserai", "khuzait", "battania", "nord", "bandit",
            // Gender types
            "female", "male"
        };

        HashSet<string> tierKeywords = new(StringComparer.OrdinalIgnoreCase)
        {
            "tier0", "tier1", "tier2", "tier3", "tier4", "tier5", "tier6", "tier6plus"
        };

        List<string> searchTerms = new();
        List<string> typeTerms = new();
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
            else if (tierKeywords.Contains(arg))
            {
                int parsedTier = ParseTroopTierKeyword(arg);
                if (parsedTier >= 0)
                {
                    tier = parsedTier;
                    // Also add to type terms for type flag filtering
                    typeTerms.Add(arg);
                }
            }
            // Check for type keywords
            else if (typeKeywords.Contains(arg))
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
        TroopTypes types = TroopQueries.ParseTroopTypes(typeTerms);

        return new(query, types, tier, sortBy, sortDesc);
    }

    /// <summary>
    /// Parse troop tier keyword into tier number
    /// </summary>
    public static int ParseTroopTierKeyword(string tierKeyword)
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
    /// Build equipment info string for a CharacterObject
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
    /// Build upgrade paths info string for a CharacterObject
    /// </summary>
    public static string BuildUpgradeInfo(CharacterObject character)
    {
        if (character.UpgradeTargets == null || character.UpgradeTargets.Length == 0)
            return "Upgrades: None\n";

        IEnumerable<string> upgrades = character.UpgradeTargets.Select(u => u.Name.ToString());
        return "Upgrades: " + string.Join(", ", upgrades) + "\n";
    }
}
