using Bannerlord.GameMaster.Console.Common;
using Bannerlord.GameMaster.Console.Common.Execution;
using Bannerlord.GameMaster.Console.Common.Validation;
using Bannerlord.GameMaster.Console.Query.CommandQueryHelpers;
using Bannerlord.GameMaster.Troops;
using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Library;

namespace Bannerlord.GameMaster.Console.Query.TroopQueryCommands;

/// <summary>
/// Get detailed info about a specific troop by ID
/// Usage: gm.query.troop_info [troopId]
/// Example: gm.query.troop_info imperial_legionary
/// </summary>
public static class QueryTroopInfoCommand
{
    [CommandLineFunctionality.CommandLineArgumentFunction("troop_info", "gm.query")]
    public static string Execute(List<string> args)
    {
        return Cmd.Run(args, () =>
        {
            // MARK: Validation
            if (!CommandValidator.ValidateCampaignState(out string error))
                return CommandResult.Error(error).Message;

            if (args == null || args.Count == 0)
                return CommandResult.Error("Please provide a troop ID.\nUsage: gm.query.troop_info <troopId>\n").Message;

            // MARK: Parse Arguments
            string troopId = args[0];

            // MARK: Execute Logic
            CharacterObject troop = TroopQueries.GetTroopById(troopId);

            if (troop == null)
                return CommandResult.Error($"Troop with ID '{troopId}' not found.\n").Message;

            if (troop.IsHero)
                return CommandResult.Error($"'{troopId}' is a hero/lord, not a troop. Use gm.query.hero_info instead.\n").Message;

            if (!troop.IsActualTroop())
                return CommandResult.Error($"'{troopId}' is not an actual troop (may be NPC, child, template, etc.).\n").Message;

            TroopTypes types = troop.GetTroopTypes();
            string cultureName = troop.Culture?.Name?.ToString() ?? "None";
            string equipmentInfo = TroopQueryHelpers.BuildEquipmentInfo(troop);
            string upgradeInfo = TroopQueryHelpers.BuildUpgradeInfo(troop);
            string category = troop.GetTroopCategory();

            return CommandResult.Success($"Troop Information:\n" +
                   $"ID: {troop.StringId}\n" +
                   $"Name: {troop.Name}\n" +
                   $"Category: {category}\n" +
                   $"Tier: {troop.GetBattleTier()}\n" +
                   $"Level: {troop.Level}\n" +
                   $"Culture: {cultureName}\n" +
                   $"Formation: {troop.DefaultFormationClass}\n" +
                   $"Types: {types}\n" +
                   equipmentInfo +
                   upgradeInfo).Message;
        });
    }
}
