using Bannerlord.GameMaster.Console.Common.Execution;
using Bannerlord.GameMaster.Console.Common.Validation;
using Bannerlord.GameMaster.Console.Query.CommandQueryHelpers;
using Bannerlord.GameMaster.Troops;
using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Library;

namespace Bannerlord.GameMaster.Console.Query.TroopQueryCommands;

/// <summary>
/// Get detailed info about a specific CharacterObject by ID (UNFILTERED)
/// WARNING: Can return info on NPCs, templates, children, etc. - not just combat troops
/// Usage: gm.query.character_objects_info [characterId]
/// Example: gm.query.character_objects_info imperial_legionary
/// Note: Use gm.query.troop_info for filtered combat troops only
/// </summary>
public static class QueryCharacterObjectsInfoCommand
{
    [CommandLineFunctionality.CommandLineArgumentFunction("character_objects_info", "gm.query")]
    public static string Execute(List<string> args)
    {
        return Cmd.Run(args, () =>
        {
            // MARK: Validation
            if (!CommandValidator.ValidateCampaignState(out string error))
                return error;

            if (args == null || args.Count == 0)
                return "Error: Please provide a character ID.\nUsage: gm.query.character_objects_info <characterId>\n";

            // MARK: Parse Arguments
            string characterId = args[0];

            // MARK: Execute Logic
            CharacterObject character = TroopQueries.GetTroopById(characterId);

            if (character == null)
                return $"Error: Character with ID '{characterId}' not found.\n";

            if (character.IsHero)
                return $"Error: '{characterId}' is a hero/lord. Use gm.query.hero_info instead.\n";

            string headerNote = "[WARNING] UNFILTERED - Showing info for ANY CharacterObject (may be NPC, template, child, etc.)\n";

            // Check if it's an actual troop
            string troopStatus = character.IsActualTroop()
                ? "[OK] This is a valid combat troop"
                : "[WARNING] This is NOT a combat troop (NPC, template, child, etc.)";

            TroopTypes types = character.GetTroopTypes();
            string cultureName = character.Culture?.Name?.ToString() ?? "None";
            string equipmentInfo = TroopQueryHelpers.BuildEquipmentInfo(character);
            string upgradeInfo = TroopQueryHelpers.BuildUpgradeInfo(character);
            string category = character.GetTroopCategory();

            return headerNote + "\n" +
                   $"Character Information:\n" +
                   $"ID: {character.StringId}\n" +
                   $"Name: {character.Name}\n" +
                   $"Status: {troopStatus}\n" +
                   $"Category: {category}\n" +
                   $"Tier: {character.GetBattleTier()}\n" +
                   $"Level: {character.Level}\n" +
                   $"Culture: {cultureName}\n" +
                   $"Formation: {character.DefaultFormationClass}\n" +
                   $"Types: {types}\n" +
                   equipmentInfo +
                   upgradeInfo;
        });
    }
}
