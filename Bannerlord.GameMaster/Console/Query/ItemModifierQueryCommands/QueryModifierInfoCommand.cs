using Bannerlord.GameMaster.Console.Common;
using Bannerlord.GameMaster.Console.Common.Execution;
using Bannerlord.GameMaster.Console.Common.Formatting;
using Bannerlord.GameMaster.Console.Common.Validation;
using Bannerlord.GameMaster.Items;
using System.Collections.Generic;
using TaleWorlds.Core;
using TaleWorlds.Library;

namespace Bannerlord.GameMaster.Console.Query.ItemModifierQueryCommands;

/// <summary>
/// Get detailed information about a specific modifier
/// Usage: gm.query.modifier_info &lt;modifier_name&gt;
/// Example: gm.query.modifier_info fine
/// </summary>
public static class QueryModifierInfoCommand
{
    [CommandLineFunctionality.CommandLineArgumentFunction("modifier_info", "gm.query")]
    public static string Execute(List<string> args)
    {
        return Cmd.Run(args, () =>
        {
            // MARK: Validation
            if (!CommandValidator.ValidateCampaignState(out string error))
                return CommandResult.Error(error).Message;

            if (args == null || args.Count == 0)
                return CommandResult.Success("Please provide a modifier name.\n" +
                       "Usage: gm.query.modifier_info <modifier_name>\n" +
                       "Example: gm.query.modifier_info fine\n").Message;

            // MARK: Parse Arguments
            string modifierName = string.Join(" ", args);
            
            // MARK: Execute Logic
            ItemModifier modifier;
            string parseError;
            (modifier, parseError) = ItemModifierHelper.ParseModifier(modifierName);

            if (parseError != null)
                return CommandResult.Error(parseError).Message;

            if (modifier == null)
                return CommandResult.Error($"Modifier '{modifierName}' not found.").Message;

            return CommandResult.Success($"Modifier Information:\n" +
                   $"StringId: {modifier.StringId}\n" +
                   $"Name: {modifier.Name}\n" +
                   $"Price Multiplier: x{modifier.PriceMultiplier:F2}\n" +
                   $"Damage Modifier: {(modifier.Damage >= 0 ? "+" : "")}{modifier.Damage}\n" +
                   $"Speed Modifier: {(modifier.Speed >= 0 ? "+" : "")}{modifier.Speed}\n" +
                   $"Missile Speed Modifier: {(modifier.MissileSpeed >= 0 ? "+" : "")}{modifier.MissileSpeed}\n" +
                   $"Armor Modifier: {(modifier.Armor >= 0 ? "+" : "")}{modifier.Armor}\n" +
                   $"Hit Points Modifier: {(modifier.HitPoints >= 0 ? "+" : "")}{modifier.HitPoints}\n" +
                   $"Stack Count Modifier: {(modifier.StackCount >= 0 ? "+" : "")}{modifier.StackCount}\n").Message;
        });
    }
}
