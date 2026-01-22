using Bannerlord.GameMaster.Characters;
using Bannerlord.GameMaster.Console.Common;
using Bannerlord.GameMaster.Console.Common.Execution;
using Bannerlord.GameMaster.Console.Common.Validation;
using Bannerlord.GameMaster.Console.Query.CommandQueryHelpers;
using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Library;

namespace Bannerlord.GameMaster.Console.Query.TroopQueryCommands;

/// <summary>
/// Get detailed info about a specific CharacterObject by ID.
/// Works for ALL character types: heroes, troops, templates, NPCs.
/// Usage: gm.query.character_objects_info [characterId]
/// Example: gm.query.character_objects_info imperial_legionary
/// Example: gm.query.character_objects_info lord_1_1
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
                return CommandResult.Error(error).Log().Message;

            if (args == null || args.Count == 0)
                return CommandResult.Error("Please provide a character ID.\nUsage: gm.query.character_objects_info <characterId>\n" +
                       "Example: gm.query.character_objects_info imperial_legionary\n" +
                       "Example: gm.query.character_objects_info lord_1_1 (hero character)\n").Log().Message;

            // MARK: Parse Arguments
            string characterId = args[0];

            // MARK: Execute Logic
            CharacterObject character = CharacterQueries.GetCharacterById(characterId);

            if (character == null)
                return CommandResult.Error($"Character with ID '{characterId}' not found.\n").Log().Message;

            // Build detailed info using CharacterExtensions
            CharacterTypes types = character.GetCharacterTypes();
            string classification = character.GetCharacterClassification();
            string cultureName = character.Culture?.Name?.ToString() ?? "None";
            string equipmentInfo = CharacterQueryHelpers.BuildEquipmentInfo(character);
            string upgradeInfo = CharacterQueryHelpers.BuildUpgradeInfo(character);

            // Build the result string
            string result = $"Character Information:\n" +
                            $"ID: {character.StringId}\n" +
                            $"Name: {character.Name}\n" +
                            $"Classification: {classification}\n" +
                            $"Gender: {(character.IsFemale ? "Female" : "Male")}\n" +
                            $"Culture: {cultureName}\n";

            // Add tier/level for non-heroes
            if (!character.IsHero)
            {
                result += $"Tier: {character.GetBattleTier()}\n" +
                          $"Level: {character.Level}\n" +
                          $"Formation: {character.DefaultFormationClass}\n";
            }
            else if (character.HeroObject != null)
            {
                // Add hero-specific info
                Hero hero = character.HeroObject;
                result += $"Age: {(int)hero.Age}\n" +
                          $"Clan: {hero.Clan?.Name?.ToString() ?? "None"}\n";

                if (hero.CurrentSettlement != null)
                    result += $"Location: {hero.CurrentSettlement.Name}\n";

                if (hero.IsPrisoner)
                    result += $"Status: Prisoner\n";
                else if (hero.IsDead)
                    result += $"Status: Dead\n";
                else
                    result += $"Status: Alive\n";
            }

            result += $"CharacterTypes: {types}\n" +
                      equipmentInfo +
                      upgradeInfo;

            return CommandResult.Success(result).Log().Message;
        });
    }
}
