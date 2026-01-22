using Bannerlord.GameMaster.Console.Common;
using Bannerlord.GameMaster.Console.Common.Execution;
using Bannerlord.GameMaster.Console.Common.Validation;
using Bannerlord.GameMaster.Items;
using System.Collections.Generic;
using TaleWorlds.Core;
using TaleWorlds.Library;

namespace Bannerlord.GameMaster.Console.Query.ItemQueryCommands;

/// <summary>
/// Get detailed info about a specific item by ID
/// Usage: gm.query.item_info itemId
/// </summary>
public static class QueryItemInfoCommand
{
    [CommandLineFunctionality.CommandLineArgumentFunction("item_info", "gm.query")]
    public static string Execute(List<string> args)
    {
        return Cmd.Run(args, () =>
        {
            // MARK: Validation
            if (!CommandValidator.ValidateCampaignState(out string error))
                return CommandResult.Error(error).Log().Message;

            if (args == null || args.Count == 0)
                return CommandResult.Error("Please provide an item ID.\nUsage: gm.query.item_info <itemId>\n").Log().Message;

            // MARK: Parse Arguments
            string itemId = args[0];
            ItemObject item = ItemQueries.GetItemById(itemId);

            if (item == null)
                return CommandResult.Error($"Item with ID '{itemId}' not found.\n").Log().Message;

            // MARK: Execute Logic
            ItemTypes types = item.GetItemTypes();
            // Note: ItemTiers enum values are offset by 1 (Tier0=-1, Tier1=0, Tier2=1, etc.)
            // So we add 1 to display the user-friendly tier number
            string tier = (int)item.Tier >= -1 ? ((int)item.Tier + 1).ToString() : "N/A";

            // Build detailed stats based on item type
            string additionalInfo = "";

            // Weapon stats
            if (item.WeaponComponent != null)
            {
                WeaponComponentData weapon = item.WeaponComponent.PrimaryWeapon;
                additionalInfo += $"Weapon Class: {weapon.WeaponClass}\n" +
                                 $"Damage: {weapon.SwingDamage} (Swing), {weapon.ThrustDamage} (Thrust)\n" +
                                 $"Speed: {weapon.SwingSpeed} (Swing), {weapon.ThrustSpeed} (Thrust)\n" +
                                 $"Handling: {weapon.Handling}\n";
            }

            // Armor stats
            if (item.ArmorComponent != null)
            {
                ArmorComponent armor = item.ArmorComponent;
                additionalInfo += $"Head Armor: {armor.HeadArmor}\n" +
                                 $"Body Armor: {armor.BodyArmor}\n" +
                                 $"Leg Armor: {armor.LegArmor}\n" +
                                 $"Arm Armor: {armor.ArmArmor}\n";
            }

            // Mount stats
            if (item.HorseComponent != null)
            {
                HorseComponent horse = item.HorseComponent;
                additionalInfo += $"Charge Damage: {horse.ChargeDamage}\n" +
                                 $"Speed: {horse.Speed}\n" +
                                 $"Maneuver: {horse.Maneuver}\n" +
                                 $"Hit Points: {horse.HitPoints}\n";
            }

            return CommandResult.Success($"Item Information:\n" +
                   $"ID: {item.StringId}\n" +
                   $"Name: {item.Name}\n" +
                   $"Type: {item.ItemType}\n" +
                   $"Value: {item.Value}\n" +
                   $"Weight: {item.Weight}\n" +
                   $"Tier: {tier}\n" +
                   $"Types: {types}\n" +
                   additionalInfo).Log().Message;
        });
    }
}
