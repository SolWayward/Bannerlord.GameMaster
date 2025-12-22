using Bannerlord.GameMaster.Console.Common;
using Bannerlord.GameMaster.Items;
using System;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.Core;
using TaleWorlds.Library;

namespace Bannerlord.GameMaster.Console.Query
{
    [CommandLineFunctionality.CommandLineArgumentFunction("query", "gm")]
    public static class ItemModifierQueryCommands
    {
        /// <summary>
        /// List all available item modifiers
        /// Usage: gm.query.modifiers [search]
        /// </summary>
        [CommandLineFunctionality.CommandLineArgumentFunction("modifiers", "gm.query")]
        public static string QueryModifiers(List<string> args)
        {
            return Cmd.Run(args, () =>
            {
                if (!CommandBase.ValidateCampaignMode(out string error))
                    return error;

                var allModifiers = ItemModifierHelper.GetAllModifiers();

                // Filter by search term if provided
                if (args != null && args.Count > 0)
                {
                    string searchTerm = string.Join(" ", args).ToLower();
                    allModifiers = allModifiers
                        .Where(m => m.Name.ToString().ToLower().Contains(searchTerm) ||
                                   m.StringId.ToLower().Contains(searchTerm))
                        .ToList();
                }

                if (allModifiers.Count == 0)
                {
                    string searchMsg = args != null && args.Count > 0 
                        ? $" matching '{string.Join(" ", args)}'" 
                        : "";
                    return $"Found 0 modifiers{searchMsg}.\n" +
                           "Usage: gm.query.modifiers [search]\n" +
                           "Example: gm.query.modifiers fine\n";
                }

                // Sort by name for better readability
                allModifiers = allModifiers.OrderBy(m => m.Name.ToString()).ToList();

                // Calculate column widths for proper header alignment
                int idWidth = Math.Max("StringId".Length, allModifiers.Max(m => m.StringId.Length));
                int nameWidth = Math.Max("Name".Length, allModifiers.Max(m => m.Name.ToString().Length));
                int priceWidth = "Price Factor".Length; // Fixed label width

                string result = $"Found {allModifiers.Count} modifier(s):\n\n";
                
                // Add headers with proper spacing
                result += "StringId".PadRight(idWidth + 2);
                result += "Name".PadRight(nameWidth + 2);
                result += "Price Factor\n";
                
                // Add separator line
                result += new string('-', idWidth).PadRight(idWidth + 2);
                result += new string('-', nameWidth).PadRight(nameWidth + 2);
                result += new string('-', priceWidth) + "\n";
                
                // Add formatted data
                result += ColumnFormatter<ItemModifier>.FormatList(
                    allModifiers,
                    m => m.StringId,
                    m => m.Name.ToString(),
                    m => $"x{m.PriceMultiplier:F2}"
                );

                return result;
            });
        }

        /// <summary>
        /// Get detailed information about a specific modifier
        /// Usage: gm.query.modifier_info &lt;modifier_name&gt;
        /// </summary>
        [CommandLineFunctionality.CommandLineArgumentFunction("modifier_info", "gm.query")]
        public static string QueryModifierInfo(List<string> args)
        {
            return Cmd.Run(args, () =>
            {
                if (!CommandBase.ValidateCampaignMode(out string error))
                    return error;

                if (args == null || args.Count == 0)
                    return "Error: Please provide a modifier name.\n" +
                           "Usage: gm.query.modifier_info <modifier_name>\n" +
                           "Example: gm.query.modifier_info fine\n";

                string modifierName = string.Join(" ", args);
                var (modifier, parseError) = ItemModifierHelper.ParseModifier(modifierName);

                if (parseError != null)
                    return CommandBase.FormatErrorMessage(parseError);

                if (modifier == null)
                    return CommandBase.FormatErrorMessage($"Modifier '{modifierName}' not found.");

                return $"Modifier Information:\n" +
                       $"StringId: {modifier.StringId}\n" +
                       $"Name: {modifier.Name}\n" +
                       $"Price Multiplier: x{modifier.PriceMultiplier:F2}\n" +
                       $"Damage Modifier: {(modifier.Damage >= 0 ? "+" : "")}{modifier.Damage}\n" +
                       $"Speed Modifier: {(modifier.Speed >= 0 ? "+" : "")}{modifier.Speed}\n" +
                       $"Missile Speed Modifier: {(modifier.MissileSpeed >= 0 ? "+" : "")}{modifier.MissileSpeed}\n" +
                       $"Armor Modifier: {(modifier.Armor >= 0 ? "+" : "")}{modifier.Armor}\n" +
                       $"Hit Points Modifier: {(modifier.HitPoints >= 0 ? "+" : "")}{modifier.HitPoints}\n" +
                       $"Stack Count Modifier: {(modifier.StackCount >= 0 ? "+" : "")}{modifier.StackCount}\n";
            });
        }
    }
}