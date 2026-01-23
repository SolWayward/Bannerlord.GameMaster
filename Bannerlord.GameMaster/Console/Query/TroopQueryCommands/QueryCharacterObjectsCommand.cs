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
/// Query ALL CharacterObjects with AND logic.
/// Returns EVERYTHING by default (heroes, troops, templates, NPCs).
/// Use type keywords to filter specific categories.
/// Usage: gm.query.character_objects [search terms] [type keywords] [culture keywords] [tier] [sort parameters]
/// Example: gm.query.character_objects imperial hero
/// Example: gm.query.character_objects troop vlandia sort:name
/// Example: gm.query.character_objects lord female empire,vlandia
/// </summary>
public static class QueryCharacterObjectsCommand
{
    [CommandLineFunctionality.CommandLineArgumentFunction("character_objects", "gm.query")]
    public static string Execute(List<string> args)
    {
        return Cmd.Run(args, () =>
        {
            // MARK: Validation
            if (!CommandValidator.ValidateCampaignState(out string error))
                return CommandResult.Error(error).Message;

            // MARK: Parse Arguments
            CharacterQueryArguments queryArgs = CharacterQueryHelpers.ParseCharacterQueryArguments(args);

            // MARK: Execute Logic
            MBReadOnlyList<CharacterObject> matchedCharacters = CharacterQueries.QueryCharacterObjects(
                queryArgs.QueryArgs.Query,
                queryArgs.Types,
                matchAll: true,
                queryArgs.Cultures,
                queryArgs.Tier,
                queryArgs.QueryArgs.SortBy,
                queryArgs.QueryArgs.SortDesc);

            string criteriaDesc = queryArgs.GetCriteriaString();

            if (matchedCharacters.Count == 0)
            {
                return CommandResult.Success($"Found 0 character(s) matching {criteriaDesc}\n" +
                       "Usage: gm.query.character_objects [search] [type keywords] [cultures] [tier] [sort]\n" +
                       "Type keywords: hero, troop, template, npc, lord, wanderer, notable, child, female, male, original, blgm\n" +
                       "Culture keywords: empire, vlandia, sturgia, aserai, khuzait, battania, nord, bandit\n" +
                       "Culture groups: all_cultures, main_cultures, bandit_cultures (or comma-separated: vlandia,battania)\n" +
                       "Tier keywords: tier0, tier1, tier2, tier3, tier4, tier5, tier6, tier6plus\n" +
                       "Sort: sort:name, sort:tier, sort:level, sort:culture, sort:classification (add :desc for descending)\n" +
                       "Example: gm.query.character_objects imperial hero sort:name\n" +
                       "Example: gm.query.character_objects troop vlandia,empire tier3\n").Message;
            }

            List<CharacterObject> characterList = new(matchedCharacters);
            return CommandResult.Success($"Found {matchedCharacters.Count} character(s) matching {criteriaDesc}:\n" +
                   $"{CharacterQueries.GetFormattedDetails(characterList)}").Message;
        });
    }
}
