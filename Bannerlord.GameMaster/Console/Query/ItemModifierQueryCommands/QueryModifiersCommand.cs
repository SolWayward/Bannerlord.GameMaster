using Bannerlord.GameMaster.Console.Common;
using Bannerlord.GameMaster.Console.Common.Execution;
using Bannerlord.GameMaster.Console.Common.Formatting;
using Bannerlord.GameMaster.Console.Common.Validation;
using Bannerlord.GameMaster.Items;
using System;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.Core;
using TaleWorlds.Library;

namespace Bannerlord.GameMaster.Console.Query.ItemModifierQueryCommands;

/// <summary>
/// List all available item modifiers
/// Usage: gm.query.modifiers [search]
/// Example: gm.query.modifiers fine
/// </summary>
public static class QueryModifiersCommand
{
    [CommandLineFunctionality.CommandLineArgumentFunction("modifiers", "gm.query")]
    public static string Execute(List<string> args)
    {
        return Cmd.Run(args, () =>
        {
            // MARK: Validation
            if (!CommandValidator.ValidateCampaignState(out string error))
                return CommandResult.Error(error).Log().Message;

            // MARK: Execute Logic
            List<ItemModifier> allModifiers = ItemModifierHelper.GetAllModifiers();

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
                return CommandResult.Success($"Found 0 modifiers{searchMsg}.\n" +
                       "Usage: gm.query.modifiers [search]\n" +
                       "Example: gm.query.modifiers fine\n").Log().Message;
            }

            // Sort by name for better readability
            allModifiers = allModifiers.OrderBy(m => m.Name.ToString()).ToList();

            // Calculate column widths for proper header alignment
            int idWidth = Math.Max("StringId".Length, allModifiers.Max(m => m.StringId.Length));
            int nameWidth = Math.Max("Name".Length, allModifiers.Max(m => m.Name.ToString().Length));
            int priceWidth = "Price Factor".Length;

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

            return CommandResult.Success(result).Log().Message;
        });
    }
}
