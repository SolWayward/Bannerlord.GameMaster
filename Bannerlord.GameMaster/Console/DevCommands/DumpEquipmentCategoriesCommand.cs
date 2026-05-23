using Bannerlord.GameMaster.Console.Common;
using Bannerlord.GameMaster.Console.Common.Execution;
using Bannerlord.GameMaster.Console.Common.Formatting;
using Bannerlord.GameMaster.Console.Common.Validation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.ObjectSystem;

namespace Bannerlord.GameMaster.Console.DevCommands;

/// <summary>
/// Command to dump all equipment rosters matching specified EquipmentCategories to a text file.
/// Useful for analyzing what items are available for different category combinations like
/// IsLordTemplate, IsFemaleTemplate, IsKingdomRulerTemplate, etc.
/// </summary>
public static class DumpEquipmentCategoriesCommand
{
    private static readonly Dictionary<string, EquipmentCategories> CategoryMapping = new(StringComparer.OrdinalIgnoreCase)
    {
        { "female", EquipmentCategories.IsFemaleTemplate },
        { "lord", EquipmentCategories.IsLordTemplate },
        { "child", EquipmentCategories.IsChildEquipmentTemplate },
        { "teenager", EquipmentCategories.IsTeenagerEquipmentTemplate },
        { "ruler", EquipmentCategories.IsKingdomRulerTemplate },
        { "all", EquipmentCategories.None } // Special case: dump all rosters
    };

    /// <summary>
    /// Dumps equipment rosters matching specified categories to a text file for analysis.
    /// Usage: gm.dev.dump_equipment_categories [categories]
    /// Categories can be combined with commas: gm.dev.dump_equipment_categories lord,female
    /// Use 'all' to dump all equipment rosters grouped by their categories.
    /// </summary>
    [CommandLineFunctionality.CommandLineArgumentFunction("dump_equipment_categories", "gm.dev")]
    public static string DumpEquipmentCategories(List<string> args)
    {
        return Cmd.Run(args, () =>
        {
            // MARK: Validation
            if (!CommandValidator.ValidateCampaignState(out string error))
                return CommandResult.Error(error);

            string usageMessage = CreateUsageMessage();

            // MARK: Parse Arguments
            bool dumpAll = args.Count == 0;
            EquipmentCategories targetCategories = EquipmentCategories.None;
            List<string> requestedCategoryNames = new();

            if (args.Count > 0)
            {
                string categoryArg = args[0].ToLowerInvariant();

                if (categoryArg == "all")
                {
                    dumpAll = true;
                }
                else
                {
                    string[] categoryParts = categoryArg.Split(',');
                    foreach (string part in categoryParts)
                    {
                        string trimmed = part.Trim();
                        if (CategoryMapping.TryGetValue(trimmed, out EquipmentCategories category))
                        {
                            targetCategories |= category;
                            requestedCategoryNames.Add(trimmed);
                        }
                        else
                        {
                            return CommandResult.Error(MessageFormatter.FormatErrorMessage(
                                $"Unknown category '{trimmed}'.\n{usageMessage}"));
                        }
                    }
                }
            }

            // MARK: Execute Logic
            MBReadOnlyList<MBEquipmentRoster> allRosters = MBObjectManager.Instance.GetObjectTypeList<MBEquipmentRoster>();

            if (allRosters == null || allRosters.Count == 0)
            {
                return CommandResult.Error(MessageFormatter.FormatErrorMessage("No equipment rosters found in game."));
            }

            string fileName = dumpAll ? "EquipmentCategories_All_Dump.txt" : $"EquipmentCategories_{string.Join("_", requestedCategoryNames)}_Dump.txt";
            string outputPath = DevCommandHelpers.GetDumpFilePath(fileName);

            StringBuilder sb = new();
            int matchingRosterCount = 0;
            int totalItemsFound = 0;

            if (dumpAll)
            {
                sb.AppendLine("Equipment Rosters Dump - ALL CATEGORIES");
                sb.AppendLine($"Total Rosters in Game: {allRosters.Count}");
                sb.AppendLine(new string('=', 100));
                sb.AppendLine();

                // Group rosters by their categories
                Dictionary<EquipmentCategories, List<MBEquipmentRoster>> groupedByCategories = new();

                for (int i = 0; i < allRosters.Count; i++)
                {
                    MBEquipmentRoster roster = allRosters[i];
                    if (roster == null) continue;

                    if (!groupedByCategories.ContainsKey(roster.EquipmentCategories))
                    {
                        groupedByCategories[roster.EquipmentCategories] = new List<MBEquipmentRoster>();
                    }

                    groupedByCategories[roster.EquipmentCategories].Add(roster);
                }

                foreach (KeyValuePair<EquipmentCategories, List<MBEquipmentRoster>> group in groupedByCategories)
                {
                    sb.AppendLine($"CATEGORIES: {FormatCategories(group.Key)} ({(uint)group.Key})");
                    sb.AppendLine($"Rosters with this category combination: {group.Value.Count}");
                    sb.AppendLine(new string('-', 80));

                    foreach (MBEquipmentRoster roster in group.Value)
                    {
                        matchingRosterCount++;
                        totalItemsFound += AppendRosterDetails(sb, roster);
                    }

                    sb.AppendLine();
                }
            }
            else
            {
                sb.AppendLine($"Equipment Rosters Dump - Categories: {FormatCategories(targetCategories)}");
                sb.AppendLine($"Requested Categories Value: {(uint)targetCategories}");
                sb.AppendLine(new string('=', 100));
                sb.AppendLine();

                for (int i = 0; i < allRosters.Count; i++)
                {
                    MBEquipmentRoster roster = allRosters[i];
                    if (roster == null) continue;

                    // Check if roster has ALL the requested categories
                    if ((roster.EquipmentCategories & targetCategories) == targetCategories)
                    {
                        matchingRosterCount++;
                        sb.AppendLine($"Roster Categories: {FormatCategories(roster.EquipmentCategories)} ({(uint)roster.EquipmentCategories})");
                        sb.AppendLine(new string('-', 80));
                        totalItemsFound += AppendRosterDetails(sb, roster);
                        sb.AppendLine();
                    }
                }
            }

            sb.AppendLine(new string('=', 100));
            sb.AppendLine($"Summary: {matchingRosterCount} rosters, {totalItemsFound} total item slots");

            // Add category reference at the end
            sb.AppendLine();
            sb.AppendLine("EQUIPMENT CATEGORIES REFERENCE:");
            sb.AppendLine(new string('-', 50));
            foreach (KeyValuePair<string, EquipmentCategories> kvp in CategoryMapping)
            {
                if (kvp.Key != "all")
                {
                    sb.AppendLine($"  {kvp.Key,-15} = {kvp.Value,-30} ({(uint)kvp.Value})");
                }
            }

            File.WriteAllText(outputPath, sb.ToString());

            return CommandResult.Success(MessageFormatter.FormatSuccessMessage(
                $"Equipment categories dump complete!\n" +
                $"Matching rosters: {matchingRosterCount}\n" +
                $"Total item slots: {totalItemsFound}\n" +
                $"Output: {outputPath}"));
        }).Message;
    }

    // MARK: Helper Methods

    private static string CreateUsageMessage()
    {
        StringBuilder sb = new();
        sb.AppendLine("Usage: gm.dev.dump_equipment_categories [categories]");
        sb.AppendLine();
        sb.AppendLine("Examples:");
        sb.AppendLine("  gm.dev.dump_equipment_categories all           - Dump all rosters grouped by categories");
        sb.AppendLine("  gm.dev.dump_equipment_categories lord          - Dump rosters with IsLordTemplate");
        sb.AppendLine("  gm.dev.dump_equipment_categories lord,female   - Dump rosters with both categories");
        sb.AppendLine("  gm.dev.dump_equipment_categories ruler         - Dump kingdom ruler equipment");
        sb.AppendLine();
        sb.AppendLine("Available categories:");
        sb.AppendLine("  female, lord, child, teenager, ruler, all");
        return sb.ToString();
    }

    private static string FormatCategories(EquipmentCategories categories)
    {
        if (categories == EquipmentCategories.None)
            return "None";

        List<string> names = new();
        if ((categories & EquipmentCategories.IsFemaleTemplate) != 0) names.Add("Female");
        if ((categories & EquipmentCategories.IsLordTemplate) != 0) names.Add("Lord");
        if ((categories & EquipmentCategories.IsChildEquipmentTemplate) != 0) names.Add("Child");
        if ((categories & EquipmentCategories.IsTeenagerEquipmentTemplate) != 0) names.Add("Teenager");
        if ((categories & EquipmentCategories.IsKingdomRulerTemplate) != 0) names.Add("KingdomRuler");
        return string.Join(" | ", names);
    }

    private static int AppendRosterDetails(StringBuilder sb, MBEquipmentRoster roster)
    {
        int itemCount = 0;

        sb.AppendLine($"  Roster ID: {roster.StringId}");

        if (roster.EquipmentCulture != null)
        {
            sb.AppendLine($"  Culture: {roster.EquipmentCulture.StringId}");
        }

        MBReadOnlyList<Equipment> equipments = roster.AllEquipments;
        sb.AppendLine($"  Equipment Sets: {equipments.Count}");

        for (int setIndex = 0; setIndex < equipments.Count; setIndex++)
        {
            Equipment equipment = equipments[setIndex];
            if (equipment == null) continue;

            string equipType = equipment.IsCivilian ? "Civilian" : (equipment.IsStealth ? "Stealth" : "Battle");
            sb.AppendLine($"    Set {setIndex + 1} ({equipType}):");

            // Iterate through all equipment slots
            for (int slotIndex = 0; slotIndex < (int)EquipmentIndex.NumEquipmentSetSlots; slotIndex++)
            {
                EquipmentElement element = equipment[(EquipmentIndex)slotIndex];
                if (!element.IsEmpty && element.Item != null)
                {
                    ItemObject item = element.Item;
                    string slotName = GetSlotName((EquipmentIndex)slotIndex);

                    sb.AppendLine($"      [{slotName}] {item.Name} (ID: {item.StringId})");
                    sb.AppendLine($"          Type: {item.ItemType}, Tier: {item.Tier}, Value: {item.Value}");

                    if (item.Culture != null)
                    {
                        sb.AppendLine($"          Culture: {item.Culture.StringId}");
                    }

                    // Add armor info if applicable
                    if (item.HasArmorComponent)
                    {
                        ArmorComponent armor = item.ArmorComponent;
                        sb.AppendLine($"          Armor - Head: {armor.HeadArmor}, Body: {armor.BodyArmor}, Arm: {armor.ArmArmor}, Leg: {armor.LegArmor}");
                    }

                    itemCount++;
                }
            }
        }

        sb.AppendLine();
        return itemCount;
    }

    private static string GetSlotName(EquipmentIndex index)
    {
        return index switch
        {
            EquipmentIndex.Weapon0 => "Weapon0",
            EquipmentIndex.Weapon1 => "Weapon1",
            EquipmentIndex.Weapon2 => "Weapon2",
            EquipmentIndex.Weapon3 => "Weapon3",
            EquipmentIndex.Head => "Head",
            EquipmentIndex.Body => "Body",
            EquipmentIndex.Leg => "Leg",
            EquipmentIndex.Gloves => "Gloves",
            EquipmentIndex.Cape => "Cape",
            EquipmentIndex.Horse => "Horse",
            EquipmentIndex.HorseHarness => "Harness",
            _ => index.ToString()
        };
    }
}
