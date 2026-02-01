using Bannerlord.GameMaster.Console.Common;
using Bannerlord.GameMaster.Console.Common.Execution;
using Bannerlord.GameMaster.Console.Common.Formatting;
using Bannerlord.GameMaster.Console.Common.Parsing;
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
/// Command to dump all equipment rosters matching specified EquipmentFlags to a text file.
/// Useful for analyzing what items are available for different flag combinations like
/// IsFlamboyantTemplate, IsNobleTemplate, IsFemaleTemplate, etc.
/// </summary>
public static class DumpEquipmentForFlagsCommand
{
    private static readonly Dictionary<string, EquipmentFlags> FlagMapping = new(StringComparer.OrdinalIgnoreCase)
    {
        { "wanderer", EquipmentFlags.IsWandererEquipment },
        { "gentry", EquipmentFlags.IsGentryEquipment },
        { "rebel", EquipmentFlags.IsRebelHeroEquipment },
        { "noncombatant", EquipmentFlags.IsNoncombatantTemplate },
        { "combatant", EquipmentFlags.IsCombatantTemplate },
        { "civilian", EquipmentFlags.IsCivilianTemplate },
        { "noble", EquipmentFlags.IsNobleTemplate },
        { "female", EquipmentFlags.IsFemaleTemplate },
        { "medium", EquipmentFlags.IsMediumTemplate },
        { "heavy", EquipmentFlags.IsHeavyTemplate },
        { "flamboyant", EquipmentFlags.IsFlamboyantTemplate },
        { "stoic", EquipmentFlags.IsStoicTemplate },
        { "nomad", EquipmentFlags.IsNomadTemplate },
        { "woodland", EquipmentFlags.IsWoodlandTemplate },
        { "child", EquipmentFlags.IsChildEquipmentTemplate },
        { "teenager", EquipmentFlags.IsTeenagerEquipmentTemplate },
        { "all", EquipmentFlags.None } // Special case: dump all rosters
    };

    /// <summary>
    /// Dumps equipment rosters matching specified flags to a text file for analysis.
    /// Usage: gm.dev.dump_equipment_flags [flags]
    /// Flags can be combined with commas: gm.dev.dump_equipment_flags noble,female,flamboyant
    /// Use 'all' to dump all equipment rosters grouped by their flags.
    /// </summary>
    [CommandLineFunctionality.CommandLineArgumentFunction("dump_equipment_flags", "gm.dev")]
    public static string DumpEquipmentFlags(List<string> args)
    {
        return Cmd.Run(args, () =>
        {
            // MARK: Validation
            if (!CommandValidator.ValidateCampaignState(out string error))
                return CommandResult.Error(error);

            string usageMessage = CreateUsageMessage();

            // MARK: Parse Arguments
            bool dumpAll = args.Count == 0;
            EquipmentFlags targetFlags = EquipmentFlags.None;
            List<string> requestedFlagNames = new();

            if (args.Count > 0)
            {
                string flagArg = args[0].ToLowerInvariant();
                
                if (flagArg == "all")
                {
                    dumpAll = true;
                }
                else
                {
                    string[] flagParts = flagArg.Split(',');
                    foreach (string part in flagParts)
                    {
                        string trimmed = part.Trim();
                        if (FlagMapping.TryGetValue(trimmed, out EquipmentFlags flag))
                        {
                            targetFlags |= flag;
                            requestedFlagNames.Add(trimmed);
                        }
                        else
                        {
                            return CommandResult.Error(MessageFormatter.FormatErrorMessage(
                                $"Unknown flag '{trimmed}'.\n{usageMessage}"));
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

            string fileName = dumpAll ? "EquipmentFlags_All_Dump.txt" : $"EquipmentFlags_{string.Join("_", requestedFlagNames)}_Dump.txt";
            string outputPath = DevCommandHelpers.GetDumpFilePath(fileName);

            StringBuilder sb = new();
            int matchingRosterCount = 0;
            int totalItemsFound = 0;

            if (dumpAll)
            {
                sb.AppendLine("Equipment Rosters Dump - ALL FLAGS");
                sb.AppendLine($"Total Rosters in Game: {allRosters.Count}");
                sb.AppendLine(new string('=', 100));
                sb.AppendLine();

                // Group rosters by their flags
                Dictionary<EquipmentFlags, List<MBEquipmentRoster>> groupedByFlags = new();
                
                for (int i = 0; i < allRosters.Count; i++)
                {
                    MBEquipmentRoster roster = allRosters[i];
                    if (roster == null) continue;

                    if (!groupedByFlags.ContainsKey(roster.EquipmentFlags))
                    {
                        groupedByFlags[roster.EquipmentFlags] = new List<MBEquipmentRoster>();
                    }
                    groupedByFlags[roster.EquipmentFlags].Add(roster);
                }

                foreach (KeyValuePair<EquipmentFlags, List<MBEquipmentRoster>> group in groupedByFlags)
                {
                    sb.AppendLine($"FLAGS: {FormatFlags(group.Key)} ({(uint)group.Key})");
                    sb.AppendLine($"Rosters with this flag combination: {group.Value.Count}");
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
                sb.AppendLine($"Equipment Rosters Dump - Flags: {FormatFlags(targetFlags)}");
                sb.AppendLine($"Requested Flags Value: {(uint)targetFlags}");
                sb.AppendLine(new string('=', 100));
                sb.AppendLine();

                for (int i = 0; i < allRosters.Count; i++)
                {
                    MBEquipmentRoster roster = allRosters[i];
                    if (roster == null) continue;

                    // Check if roster has ALL the requested flags
                    if (roster.HasEquipmentFlags(targetFlags))
                    {
                        matchingRosterCount++;
                        sb.AppendLine($"Roster Flags: {FormatFlags(roster.EquipmentFlags)} ({(uint)roster.EquipmentFlags})");
                        sb.AppendLine(new string('-', 80));
                        totalItemsFound += AppendRosterDetails(sb, roster);
                        sb.AppendLine();
                    }
                }
            }

            sb.AppendLine(new string('=', 100));
            sb.AppendLine($"Summary: {matchingRosterCount} rosters, {totalItemsFound} total item slots");

            // Add flag reference at the end
            sb.AppendLine();
            sb.AppendLine("EQUIPMENT FLAGS REFERENCE:");
            sb.AppendLine(new string('-', 50));
            foreach (KeyValuePair<string, EquipmentFlags> kvp in FlagMapping)
            {
                if (kvp.Key != "all")
                {
                    sb.AppendLine($"  {kvp.Key,-15} = {kvp.Value,-30} ({(uint)kvp.Value})");
                }
            }

            File.WriteAllText(outputPath, sb.ToString());

            return CommandResult.Success(MessageFormatter.FormatSuccessMessage(
                $"Equipment flags dump complete!\n" +
                $"Matching rosters: {matchingRosterCount}\n" +
                $"Total item slots: {totalItemsFound}\n" +
                $"Output: {outputPath}"));
        }).Message;
    }

    // MARK: Helper Methods
    
    private static string CreateUsageMessage()
    {
        StringBuilder sb = new();
        sb.AppendLine("Usage: gm.dev.dump_equipment_flags [flags]");
        sb.AppendLine();
        sb.AppendLine("Examples:");
        sb.AppendLine("  gm.dev.dump_equipment_flags all              - Dump all rosters grouped by flags");
        sb.AppendLine("  gm.dev.dump_equipment_flags flamboyant       - Dump rosters with IsFlamboyantTemplate");
        sb.AppendLine("  gm.dev.dump_equipment_flags noble,female     - Dump rosters with both flags");
        sb.AppendLine("  gm.dev.dump_equipment_flags gentry,civilian  - Dump gentry civilian equipment");
        sb.AppendLine();
        sb.AppendLine("Available flags:");
        sb.AppendLine("  wanderer, gentry, rebel, noncombatant, combatant, civilian,");
        sb.AppendLine("  noble, female, medium, heavy, flamboyant, stoic, nomad,");
        sb.AppendLine("  woodland, child, teenager, all");
        return sb.ToString();
    }

    private static string FormatFlags(EquipmentFlags flags)
    {
        if (flags == EquipmentFlags.None)
            return "None";

        List<string> flagNames = new();
        
        if ((flags & EquipmentFlags.IsWandererEquipment) != 0) flagNames.Add("Wanderer");
        if ((flags & EquipmentFlags.IsGentryEquipment) != 0) flagNames.Add("Gentry");
        if ((flags & EquipmentFlags.IsRebelHeroEquipment) != 0) flagNames.Add("Rebel");
        if ((flags & EquipmentFlags.IsNoncombatantTemplate) != 0) flagNames.Add("Noncombatant");
        if ((flags & EquipmentFlags.IsCombatantTemplate) != 0) flagNames.Add("Combatant");
        if ((flags & EquipmentFlags.IsCivilianTemplate) != 0) flagNames.Add("Civilian");
        if ((flags & EquipmentFlags.IsNobleTemplate) != 0) flagNames.Add("Noble");
        if ((flags & EquipmentFlags.IsFemaleTemplate) != 0) flagNames.Add("Female");
        if ((flags & EquipmentFlags.IsMediumTemplate) != 0) flagNames.Add("Medium");
        if ((flags & EquipmentFlags.IsHeavyTemplate) != 0) flagNames.Add("Heavy");
        if ((flags & EquipmentFlags.IsFlamboyantTemplate) != 0) flagNames.Add("Flamboyant");
        if ((flags & EquipmentFlags.IsStoicTemplate) != 0) flagNames.Add("Stoic");
        if ((flags & EquipmentFlags.IsNomadTemplate) != 0) flagNames.Add("Nomad");
        if ((flags & EquipmentFlags.IsWoodlandTemplate) != 0) flagNames.Add("Woodland");
        if ((flags & EquipmentFlags.IsChildEquipmentTemplate) != 0) flagNames.Add("Child");
        if ((flags & EquipmentFlags.IsTeenagerEquipmentTemplate) != 0) flagNames.Add("Teenager");

        return string.Join(" | ", flagNames);
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
