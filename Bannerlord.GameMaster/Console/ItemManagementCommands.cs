using Bannerlord.GameMaster.Console.Common;
using Bannerlord.GameMaster.Items;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Roster;
using TaleWorlds.Core;
using TaleWorlds.Library;

namespace Bannerlord.GameMaster.Console
{
    [CommandLineFunctionality.CommandLineArgumentFunction("item", "gm")]
    public static class ItemManagementCommands
    {
        #region Inventory Management

        /// <summary>
        /// Add item(s) to a hero's party inventory
        /// Usage: gm.item.add [item_query] [count] [hero_query] [modifier]
        /// </summary>
        [CommandLineFunctionality.CommandLineArgumentFunction("add", "gm.item")]
        public static string AddItem(List<string> args)
        {
            return Cmd.Run(args, () =>
            {
                if (!CommandBase.ValidateCampaignMode(out string error))
                    return error;

                var usageMessage = CommandValidator.CreateUsageMessage(
                    "gm.item.add", "<item_query> <count> <hero_query> [modifier]",
                    "Adds item(s) to a hero's party inventory with optional quality modifier.",
                    "gm.item.add imperial_sword 5 player\n" +
                    "gm.item.add sturgia_axe 1 lord_1_1 masterwork\n" +
                    "gm.item.add shield 3 player fine");

                if (!CommandBase.ValidateArgumentCount(args, 3, usageMessage, out error))
                    return error;

                // Find item
                var (item, itemError) = CommandBase.FindSingleItem(args[0]);
                if (itemError != null) return itemError;

                // Validate count
                if (!CommandValidator.ValidateIntegerRange(args[1], 1, 10000, out int count, out string countError))
                    return CommandBase.FormatErrorMessage(countError);

                // Find hero
                var (hero, heroError) = CommandBase.FindSingleHero(args[2]);
                if (heroError != null) return heroError;

                // Parse optional modifier
                ItemModifier modifier = null;
                if (args.Count > 3)
                {
                    string modifierName = string.Join(" ", args.Skip(3));
                    var (parsedModifier, modError) = ItemModifierHelper.ParseModifier(modifierName);
                    
                    if (modError != null)
                        return CommandBase.FormatErrorMessage(modError);
                    
                    if (parsedModifier != null && !ItemModifierHelper.CanHaveModifier(item))
                        return CommandBase.FormatErrorMessage($"{item.Name} cannot have quality modifiers.");
                    
                    modifier = parsedModifier;
                }

                return CommandBase.ExecuteWithErrorHandling(() =>
                {
                    if (hero.PartyBelongedTo == null)
                        return CommandBase.FormatErrorMessage($"{hero.Name} does not belong to a party and cannot hold items.");

                    EquipmentElement equipElement = new EquipmentElement(item, modifier);
                    hero.PartyBelongedTo.ItemRoster.AddToCounts(equipElement, count);

                    string modifierText = modifier != null ? $" ({modifier.Name})" : "";
                    return CommandBase.FormatSuccessMessage(
                        $"Added {count}x {item.Name}{modifierText} to {hero.Name}'s party inventory.");
                }, "Failed to add item");
            });
        }

        /// <summary>
        /// Remove specific item(s) from a hero's party inventory
        /// Usage: gm.item.remove [item_query] [count] [hero_query]
        /// </summary>
        [CommandLineFunctionality.CommandLineArgumentFunction("remove", "gm.item")]
        public static string RemoveItem(List<string> args)
        {
            return Cmd.Run(args, () =>
            {
                if (!CommandBase.ValidateCampaignMode(out string error))
                    return error;

                var usageMessage = CommandValidator.CreateUsageMessage(
                    "gm.item.remove", "<item_query> <count> <hero_query>",
                    "Removes specific item(s) from a hero's party inventory.",
                    "gm.item.remove imperial_sword 5 player");

                if (!CommandBase.ValidateArgumentCount(args, 3, usageMessage, out error))
                    return error;

                var (item, itemError) = CommandBase.FindSingleItem(args[0]);
                if (itemError != null) return itemError;

                if (!CommandValidator.ValidateIntegerRange(args[1], 1, 10000, out int count, out string countError))
                    return CommandBase.FormatErrorMessage(countError);

                var (hero, heroError) = CommandBase.FindSingleHero(args[2]);
                if (heroError != null) return heroError;

                return CommandBase.ExecuteWithErrorHandling(() =>
                {
                    if (hero.PartyBelongedTo == null)
                        return CommandBase.FormatErrorMessage($"{hero.Name} does not belong to a party.");

                    int currentCount = hero.PartyBelongedTo.ItemRoster.GetItemNumber(item);
                    if (currentCount < count)
                        return CommandBase.FormatErrorMessage($"{hero.Name}'s party only has {currentCount}x {item.Name}, cannot remove {count}.");

                    hero.PartyBelongedTo.ItemRoster.AddToCounts(item, -count);
                    return CommandBase.FormatSuccessMessage(
                        $"Removed {count}x {item.Name} from {hero.Name}'s party inventory.");
                }, "Failed to remove item");
            });
        }

        /// <summary>
        /// Remove all items from a hero's party inventory
        /// Usage: gm.item.remove_all [hero_query]
        /// </summary>
        [CommandLineFunctionality.CommandLineArgumentFunction("remove_all", "gm.item")]
        public static string RemoveAllItems(List<string> args)
        {
            return Cmd.Run(args, () =>
            {
                if (!CommandBase.ValidateCampaignMode(out string error))
                    return error;

                var usageMessage = CommandValidator.CreateUsageMessage(
                    "gm.item.remove_all", "<hero_query>",
                    "Removes all items from a hero's party inventory.",
                    "gm.item.remove_all player");

                if (!CommandBase.ValidateArgumentCount(args, 1, usageMessage, out error))
                    return error;

                var (hero, heroError) = CommandBase.FindSingleHero(args[0]);
                if (heroError != null) return heroError;

                return CommandBase.ExecuteWithErrorHandling(() =>
                {
                    if (hero.PartyBelongedTo == null)
                        return CommandBase.FormatErrorMessage($"{hero.Name} does not belong to a party.");

                    int itemCount = hero.PartyBelongedTo.ItemRoster.Count;
                    hero.PartyBelongedTo.ItemRoster.Clear();
                    
                    return CommandBase.FormatSuccessMessage(
                        $"Removed all items ({itemCount} types) from {hero.Name}'s party inventory.");
                }, "Failed to remove all items");
            });
        }

        /// <summary>
        /// Transfer item from one hero's party to another
        /// Usage: gm.item.transfer [item_query] [count] [from_hero] [to_hero]
        /// </summary>
        [CommandLineFunctionality.CommandLineArgumentFunction("transfer", "gm.item")]
        public static string TransferItem(List<string> args)
        {
            return Cmd.Run(args, () =>
            {
                if (!CommandBase.ValidateCampaignMode(out string error))
                    return error;

                var usageMessage = CommandValidator.CreateUsageMessage(
                    "gm.item.transfer", "<item_query> <count> <from_hero> <to_hero>",
                    "Transfers item(s) from one hero's party to another.",
                    "gm.item.transfer imperial_sword 5 lord_1_1 player");

                if (!CommandBase.ValidateArgumentCount(args, 4, usageMessage, out error))
                    return error;

                var (item, itemError) = CommandBase.FindSingleItem(args[0]);
                if (itemError != null) return itemError;

                if (!CommandValidator.ValidateIntegerRange(args[1], 1, 10000, out int count, out string countError))
                    return CommandBase.FormatErrorMessage(countError);

                var (fromHero, fromError) = CommandBase.FindSingleHero(args[2]);
                if (fromError != null) return fromError;

                var (toHero, toError) = CommandBase.FindSingleHero(args[3]);
                if (toError != null) return toError;

                return CommandBase.ExecuteWithErrorHandling(() =>
                {
                    if (fromHero.PartyBelongedTo == null)
                        return CommandBase.FormatErrorMessage($"{fromHero.Name} does not belong to a party.");
                    if (toHero.PartyBelongedTo == null)
                        return CommandBase.FormatErrorMessage($"{toHero.Name} does not belong to a party.");

                    int currentCount = fromHero.PartyBelongedTo.ItemRoster.GetItemNumber(item);
                    if (currentCount < count)
                        return CommandBase.FormatErrorMessage($"{fromHero.Name}'s party only has {currentCount}x {item.Name}, cannot transfer {count}.");

                    fromHero.PartyBelongedTo.ItemRoster.AddToCounts(item, -count);
                    toHero.PartyBelongedTo.ItemRoster.AddToCounts(item, count);

                    return CommandBase.FormatSuccessMessage(
                        $"Transferred {count}x {item.Name} from {fromHero.Name} to {toHero.Name}.");
                }, "Failed to transfer item");
            });
        }

        #endregion

        #region Equipment Management

        /// <summary>
        /// Unequip all items from a hero and add them back to inventory (both battle and civilian equipment)
        /// Usage: gm.item.unequip_all [hero_query]
        /// </summary>
        [CommandLineFunctionality.CommandLineArgumentFunction("unequip_all", "gm.item")]
        public static string UnequipAll(List<string> args)
        {
            return Cmd.Run(args, () =>
            {
                if (!CommandBase.ValidateCampaignMode(out string error))
                    return error;

                var usageMessage = CommandValidator.CreateUsageMessage(
                    "gm.item.unequip_all", "<hero_query>",
                    "Unequips all items from a hero and adds them to party inventory (battle and civilian equipment).",
                    "gm.item.unequip_all player");

                if (!CommandBase.ValidateArgumentCount(args, 1, usageMessage, out error))
                    return error;

                var (hero, heroError) = CommandBase.FindSingleHero(args[0]);
                if (heroError != null) return heroError;

                return CommandBase.ExecuteWithErrorHandling(() =>
                {
                    if (hero.PartyBelongedTo == null)
                        return CommandBase.FormatErrorMessage($"{hero.Name} does not belong to a party and cannot hold items in inventory.");

                    int itemsUnequipped = 0;
                    List<string> unequippedItems = new List<string>();

                    // Unequip battle equipment
                    for (int i = 0; i < (int)EquipmentIndex.NumEquipmentSetSlots; i++)
                    {
                        EquipmentIndex slot = (EquipmentIndex)i;
                        var element = hero.BattleEquipment[slot];
                        if (!element.IsEmpty)
                        {
                            // Add to party inventory
                            hero.PartyBelongedTo.ItemRoster.AddToCounts(element, 1);
                            unequippedItems.Add($"{element.Item.Name} (battle:{slot})");
                            itemsUnequipped++;
                            
                            // Remove from equipment
                            hero.BattleEquipment[slot] = EquipmentElement.Invalid;
                        }
                    }

                    // Unequip civilian equipment
                    for (int i = 0; i < (int)EquipmentIndex.NumEquipmentSetSlots; i++)
                    {
                        EquipmentIndex slot = (EquipmentIndex)i;
                        var element = hero.CivilianEquipment[slot];
                        if (!element.IsEmpty)
                        {
                            // Add to party inventory
                            hero.PartyBelongedTo.ItemRoster.AddToCounts(element, 1);
                            unequippedItems.Add($"{element.Item.Name} (civilian:{slot})");
                            itemsUnequipped++;
                            
                            // Remove from equipment
                            hero.CivilianEquipment[slot] = EquipmentElement.Invalid;
                        }
                    }

                    if (itemsUnequipped == 0)
                        return CommandBase.FormatSuccessMessage($"{hero.Name} has no items equipped.");

                    StringBuilder result = new StringBuilder();
                    result.AppendLine($"Unequipped {itemsUnequipped} items from {hero.Name} and added them to party inventory:");
                    foreach (var item in unequippedItems)
                    {
                        result.AppendLine($"  - {item}");
                    }

                    return result.ToString();
                }, "Failed to unequip all items");
            });
        }

        /// <summary>
        /// Remove all equipped items from a hero (both battle and civilian equipment) - items are deleted
        /// Usage: gm.item.remove_equipped [hero_query]
        /// </summary>
        [CommandLineFunctionality.CommandLineArgumentFunction("remove_equipped", "gm.item")]
        public static string RemoveEquipped(List<string> args)
        {
            return Cmd.Run(args, () =>
            {
                if (!CommandBase.ValidateCampaignMode(out string error))
                    return error;

                var usageMessage = CommandValidator.CreateUsageMessage(
                    "gm.item.remove_equipped", "<hero_query>",
                    "Removes all equipped items from a hero (battle and civilian equipment). Items are deleted, not moved to inventory.",
                    "gm.item.remove_equipped player");

                if (!CommandBase.ValidateArgumentCount(args, 1, usageMessage, out error))
                    return error;

                var (hero, heroError) = CommandBase.FindSingleHero(args[0]);
                if (heroError != null) return heroError;

                return CommandBase.ExecuteWithErrorHandling(() =>
                {
                    int itemsRemoved = 0;

                    // Count items before clearing
                    for (int i = 0; i < (int)EquipmentIndex.NumEquipmentSetSlots; i++)
                    {
                        EquipmentIndex slot = (EquipmentIndex)i;
                        if (!hero.BattleEquipment[slot].IsEmpty) itemsRemoved++;
                        if (!hero.CivilianEquipment[slot].IsEmpty) itemsRemoved++;
                    }

                    Equipment battleEquipment = new Equipment();
                    Equipment civilianEquipment = new Equipment();
                    
                    hero.BattleEquipment.FillFrom(battleEquipment);
                    hero.CivilianEquipment.FillFrom(civilianEquipment);

                    if (itemsRemoved == 0)
                        return CommandBase.FormatSuccessMessage($"{hero.Name} has no items equipped.");

                    return CommandBase.FormatSuccessMessage(
                        $"Removed {itemsRemoved} equipped items from {hero.Name} (battle and civilian equipment cleared).");
                }, "Failed to remove equipped items");
            });
        }

        /// <summary>
        /// Equip a specific item to a hero (auto-detects appropriate slot)
        /// Usage: gm.item.equip [item_query] [hero_query] [civilian]
        /// </summary>
        [CommandLineFunctionality.CommandLineArgumentFunction("equip", "gm.item")]
        public static string EquipItem(List<string> args)
        {
            return Cmd.Run(args, () =>
            {
                if (!CommandBase.ValidateCampaignMode(out string error))
                    return error;

                var usageMessage = CommandValidator.CreateUsageMessage(
                    "gm.item.equip", "<item_query> <hero_query> [civilian]",
                    "Equips an item to a hero's first available slot. Add 'civilian' for civilian equipment.",
                    "gm.item.equip imperial_sword player\n" +
                    "gm.item.equip robe player civilian");

                if (!CommandBase.ValidateArgumentCount(args, 2, usageMessage, out error))
                    return error;

                var (item, itemError) = CommandBase.FindSingleItem(args[0]);
                if (itemError != null) return itemError;

                var (hero, heroError) = CommandBase.FindSingleHero(args[1]);
                if (heroError != null) return heroError;

                bool isCivilian = args.Count > 2 && args[2].Equals("civilian", StringComparison.OrdinalIgnoreCase);

                return CommandBase.ExecuteWithErrorHandling(() =>
                {
                    EquipmentIndex slot = GetAppropriateSlotForItem(item);
                    if (slot == EquipmentIndex.None)
                        return CommandBase.FormatErrorMessage($"{item.Name} cannot be equipped (no appropriate slot).");

                    Equipment equipment = isCivilian ? hero.CivilianEquipment : hero.BattleEquipment;
                    equipment[slot] = new EquipmentElement(item);

                    string equipmentType = isCivilian ? "civilian" : "battle";
                    return CommandBase.FormatSuccessMessage(
                        $"Equipped {item.Name} to {hero.Name}'s {equipmentType} equipment (slot: {slot}).");
                }, "Failed to equip item");
            });
        }

        /// <summary>
        /// Unequip a specific item if it's currently equipped
        /// Usage: gm.item.unequip [item_query] [hero_query]
        /// </summary>
        [CommandLineFunctionality.CommandLineArgumentFunction("unequip", "gm.item")]
        public static string UnequipItem(List<string> args)
        {
            return Cmd.Run(args, () =>
            {
                if (!CommandBase.ValidateCampaignMode(out string error))
                    return error;

                var usageMessage = CommandValidator.CreateUsageMessage(
                    "gm.item.unequip", "<item_query> <hero_query>",
                    "Unequips a specific item if currently equipped (checks both battle and civilian).",
                    "gm.item.unequip imperial_sword player");

                if (!CommandBase.ValidateArgumentCount(args, 2, usageMessage, out error))
                    return error;

                var (item, itemError) = CommandBase.FindSingleItem(args[0]);
                if (itemError != null) return itemError;

                var (hero, heroError) = CommandBase.FindSingleHero(args[1]);
                if (heroError != null) return heroError;

                return CommandBase.ExecuteWithErrorHandling(() =>
                {
                    bool foundInBattle = false;
                    bool foundInCivilian = false;
                    List<string> unequippedSlots = new List<string>();

                    // Check battle equipment
                    for (int i = 0; i < (int)EquipmentIndex.NumEquipmentSetSlots; i++)
                    {
                        EquipmentIndex slot = (EquipmentIndex)i;
                        if (hero.BattleEquipment[slot].Item == item)
                        {
                            hero.BattleEquipment[slot] = EquipmentElement.Invalid;
                            foundInBattle = true;
                            unequippedSlots.Add($"battle:{slot}");
                        }
                    }

                    // Check civilian equipment
                    for (int i = 0; i < (int)EquipmentIndex.NumEquipmentSetSlots; i++)
                    {
                        EquipmentIndex slot = (EquipmentIndex)i;
                        if (hero.CivilianEquipment[slot].Item == item)
                        {
                            hero.CivilianEquipment[slot] = EquipmentElement.Invalid;
                            foundInCivilian = true;
                            unequippedSlots.Add($"civilian:{slot}");
                        }
                    }

                    if (!foundInBattle && !foundInCivilian)
                        return CommandBase.FormatErrorMessage($"{item.Name} is not currently equipped by {hero.Name}.");

                    return CommandBase.FormatSuccessMessage(
                        $"Unequipped {item.Name} from {hero.Name} (removed from: {string.Join(", ", unequippedSlots)}).");
                }, "Failed to unequip item");
            });
        }

        /// <summary>
        /// Equip item to specific equipment slot
        /// Usage: gm.item.equip_slot [item_query] [hero_query] [slot] [civilian]
        /// </summary>
        [CommandLineFunctionality.CommandLineArgumentFunction("equip_slot", "gm.item")]
        public static string EquipItemToSlot(List<string> args)
        {
            return Cmd.Run(args, () =>
            {
                if (!CommandBase.ValidateCampaignMode(out string error))
                    return error;

                var usageMessage = CommandValidator.CreateUsageMessage(
                    "gm.item.equip_slot", "<item_query> <hero_query> <slot> [civilian]",
                    "Equips an item to a specific equipment slot. Add 'civilian' for civilian equipment.\n" +
                    "Valid slots: Head, Body, Leg, Gloves, Cape, Horse, HorseHarness, Weapon0-3",
                    "gm.item.equip_slot imperial_sword player Weapon0\n" +
                    "gm.item.equip_slot fine_robe player Body civilian");

                if (!CommandBase.ValidateArgumentCount(args, 3, usageMessage, out error))
                    return error;

                var (item, itemError) = CommandBase.FindSingleItem(args[0]);
                if (itemError != null) return itemError;

                var (hero, heroError) = CommandBase.FindSingleHero(args[1]);
                if (heroError != null) return heroError;

                if (!TryParseEquipmentSlot(args[2], out EquipmentIndex slot))
                    return CommandBase.FormatErrorMessage($"Invalid equipment slot: '{args[2]}'. Valid slots: Head, Body, Leg, Gloves, Cape, Horse, HorseHarness, Weapon0, Weapon1, Weapon2, Weapon3.");

                bool isCivilian = args.Count > 3 && args[3].Equals("civilian", StringComparison.OrdinalIgnoreCase);

                return CommandBase.ExecuteWithErrorHandling(() =>
                {
                    Equipment equipment = isCivilian ? hero.CivilianEquipment : hero.BattleEquipment;
                    equipment[slot] = new EquipmentElement(item);

                    string equipmentType = isCivilian ? "civilian" : "battle";
                    return CommandBase.FormatSuccessMessage(
                        $"Equipped {item.Name} to {hero.Name}'s {equipmentType} equipment slot {slot}.");
                }, "Failed to equip item to slot");
            });
        }

        /// <summary>
        /// Unequip item from specific equipment slot
        /// Usage: gm.item.unequip_slot [hero_query] [slot] [civilian]
        /// </summary>
        [CommandLineFunctionality.CommandLineArgumentFunction("unequip_slot", "gm.item")]
        public static string UnequipSlot(List<string> args)
        {
            return Cmd.Run(args, () =>
            {
                if (!CommandBase.ValidateCampaignMode(out string error))
                    return error;

                var usageMessage = CommandValidator.CreateUsageMessage(
                    "gm.item.unequip_slot", "<hero_query> <slot> [civilian]",
                    "Unequips item from a specific equipment slot. Add 'civilian' for civilian equipment.\n" +
                    "Valid slots: Head, Body, Leg, Gloves, Cape, Horse, HorseHarness, Weapon0-3",
                    "gm.item.unequip_slot player Weapon0\n" +
                    "gm.item.unequip_slot player Body civilian");

                if (!CommandBase.ValidateArgumentCount(args, 2, usageMessage, out error))
                    return error;

                var (hero, heroError) = CommandBase.FindSingleHero(args[0]);
                if (heroError != null) return heroError;

                if (!TryParseEquipmentSlot(args[1], out EquipmentIndex slot))
                    return CommandBase.FormatErrorMessage($"Invalid equipment slot: '{args[1]}'. Valid slots: Head, Body, Leg, Gloves, Cape, Horse, HorseHarness, Weapon0, Weapon1, Weapon2, Weapon3.");

                bool isCivilian = args.Count > 2 && args[2].Equals("civilian", StringComparison.OrdinalIgnoreCase);

                return CommandBase.ExecuteWithErrorHandling(() =>
                {
                    Equipment equipment = isCivilian ? hero.CivilianEquipment : hero.BattleEquipment;
                    ItemObject previousItem = equipment[slot].Item;
                    
                    if (previousItem == null)
                    {
                        string equipmentType = isCivilian ? "civilian" : "battle";
                        return CommandBase.FormatErrorMessage($"No item equipped in {hero.Name}'s {equipmentType} equipment slot {slot}.");
                    }

                    equipment[slot] = EquipmentElement.Invalid;
                    
                    string eqType = isCivilian ? "civilian" : "battle";
                    return CommandBase.FormatSuccessMessage(
                        $"Unequipped {previousItem.Name} from {hero.Name}'s {eqType} equipment slot {slot}.");
                }, "Failed to unequip slot");
            });
        }

        /// <summary>
        /// List all equipped items on a hero
        /// Usage: gm.item.list_equipped [hero_query]
        /// </summary>
        [CommandLineFunctionality.CommandLineArgumentFunction("list_equipped", "gm.item")]
        public static string ListEquippedItems(List<string> args)
        {
            return Cmd.Run(args, () =>
            {
                if (!CommandBase.ValidateCampaignMode(out string error))
                    return error;

                var usageMessage = CommandValidator.CreateUsageMessage(
                    "gm.item.list_equipped", "<hero_query>",
                    "Lists all equipped items on a hero (battle and civilian equipment).",
                    "gm.item.list_equipped player");

                if (!CommandBase.ValidateArgumentCount(args, 1, usageMessage, out error))
                    return error;

                var (hero, heroError) = CommandBase.FindSingleHero(args[0]);
                if (heroError != null) return heroError;

                return CommandBase.ExecuteWithErrorHandling(() =>
                {
                    StringBuilder result = new StringBuilder();
                    result.AppendLine($"Equipped items for {hero.Name}:\n");

                    // Battle Equipment
                    result.AppendLine("=== BATTLE EQUIPMENT ===");
                    bool hasBattleItems = false;
                    for (int i = 0; i < (int)EquipmentIndex.NumEquipmentSetSlots; i++)
                    {
                        EquipmentIndex slot = (EquipmentIndex)i;
                        var element = hero.BattleEquipment[slot];
                        if (!element.IsEmpty)
                        {
                            hasBattleItems = true;
                            string modifierText = element.ItemModifier != null ? $" ({element.ItemModifier.Name})" : "";
                            result.AppendLine($"  {slot,-15} {element.Item.Name}{modifierText}");
                        }
                    }
                    if (!hasBattleItems)
                        result.AppendLine("  (No battle equipment)");

                    result.AppendLine();

                    // Civilian Equipment
                    result.AppendLine("=== CIVILIAN EQUIPMENT ===");
                    bool hasCivilianItems = false;
                    for (int i = 0; i < (int)EquipmentIndex.NumEquipmentSetSlots; i++)
                    {
                        EquipmentIndex slot = (EquipmentIndex)i;
                        var element = hero.CivilianEquipment[slot];
                        if (!element.IsEmpty)
                        {
                        hasCivilianItems = true;
                            string modifierText = element.ItemModifier != null ? $" ({element.ItemModifier.Name})" : "";
                            result.AppendLine($"  {slot,-15} {element.Item.Name}{modifierText}");
                        }
                    }
                    if (!hasCivilianItems)
                        result.AppendLine("  (No civilian equipment)");

                    return result.ToString();
                }, "Failed to list equipped items");
            });
        }

        #endregion

        #region Modifier Management

        /// <summary>
        /// Change modifier on all equipped items for a hero (battle and civilian)
        /// Usage: gm.item.set_equipped_modifier [hero_query] [modifier]
        /// </summary>
        [CommandLineFunctionality.CommandLineArgumentFunction("set_equipped_modifier", "gm.item")]
        public static string SetEquippedModifier(List<string> args)
        {
            return Cmd.Run(args, () =>
            {
                if (!CommandBase.ValidateCampaignMode(out string error))
                    return error;

                var usageMessage = CommandValidator.CreateUsageMessage(
                    "gm.item.set_equipped_modifier", "<hero_query> <modifier>",
                    "Sets modifier on all equipped items for a hero (battle and civilian equipment).",
                    "gm.item.set_equipped_modifier player masterwork\n" +
                    "gm.item.set_equipped_modifier lord_1_1 fine");

                if (!CommandBase.ValidateArgumentCount(args, 2, usageMessage, out error))
                    return error;

                var (hero, heroError) = CommandBase.FindSingleHero(args[0]);
                if (heroError != null) return heroError;

                string modifierName = string.Join(" ", args.Skip(1));
                var (modifier, modError) = ItemModifierHelper.ParseModifier(modifierName);
                
                if (modError != null)
                    return CommandBase.FormatErrorMessage(modError);

                if (modifier == null)
                    return CommandBase.FormatErrorMessage($"Modifier '{modifierName}' not found.");

                return CommandBase.ExecuteWithErrorHandling(() =>
                {
                    int itemsChanged = 0;
                    List<string> changedItems = new List<string>();

                    // Process battle equipment
                    for (int i = 0; i < (int)EquipmentIndex.NumEquipmentSetSlots; i++)
                    {
                        EquipmentIndex slot = (EquipmentIndex)i;
                        var element = hero.BattleEquipment[slot];
                        if (!element.IsEmpty && ItemModifierHelper.CanHaveModifier(element.Item))
                        {
                            hero.BattleEquipment[slot] = new EquipmentElement(element.Item, modifier);
                            changedItems.Add($"{element.Item.Name} (battle:{slot})");
                            itemsChanged++;
                        }
                    }

                    // Process civilian equipment
                    for (int i = 0; i < (int)EquipmentIndex.NumEquipmentSetSlots; i++)
                    {
                        EquipmentIndex slot = (EquipmentIndex)i;
                        var element = hero.CivilianEquipment[slot];
                        if (!element.IsEmpty && ItemModifierHelper.CanHaveModifier(element.Item))
                        {
                            hero.CivilianEquipment[slot] = new EquipmentElement(element.Item, modifier);
                            changedItems.Add($"{element.Item.Name} (civilian:{slot})");
                            itemsChanged++;
                        }
                    }

                    if (itemsChanged == 0)
                        return CommandBase.FormatSuccessMessage($"{hero.Name} has no equipped items that can have modifiers.");

                    StringBuilder result = new StringBuilder();
                    result.AppendLine($"Set modifier '{modifier.Name}' on {itemsChanged} equipped items for {hero.Name}:");
                    foreach (var item in changedItems)
                    {
                        result.AppendLine($"  - {item}");
                    }

                    return result.ToString();
                }, "Failed to set equipped modifiers");
            });
        }

        /// <summary>
        /// Change modifier on all compatible items in a hero's party inventory
        /// Usage: gm.item.set_inventory_modifier [hero_query] [modifier]
        /// </summary>
        [CommandLineFunctionality.CommandLineArgumentFunction("set_inventory_modifier", "gm.item")]
        public static string SetInventoryModifier(List<string> args)
        {
            return Cmd.Run(args, () =>
            {
                if (!CommandBase.ValidateCampaignMode(out string error))
                    return error;

                var usageMessage = CommandValidator.CreateUsageMessage(
                    "gm.item.set_inventory_modifier", "<hero_query> <modifier>",
                    "Sets modifier on all compatible items in a hero's party inventory.",
                    "gm.item.set_inventory_modifier player legendary\n" +
                    "gm.item.set_inventory_modifier lord_1_1 fine");

                if (!CommandBase.ValidateArgumentCount(args, 2, usageMessage, out error))
                    return error;

                var (hero, heroError) = CommandBase.FindSingleHero(args[0]);
                if (heroError != null) return heroError;

                string modifierName = string.Join(" ", args.Skip(1));
                var (modifier, modError) = ItemModifierHelper.ParseModifier(modifierName);
                
                if (modError != null)
                    return CommandBase.FormatErrorMessage(modError);

                if (modifier == null)
                    return CommandBase.FormatErrorMessage($"Modifier '{modifierName}' not found.");

                return CommandBase.ExecuteWithErrorHandling(() =>
                {
                    if (hero.PartyBelongedTo == null)
                        return CommandBase.FormatErrorMessage($"{hero.Name} does not belong to a party.");

                    var roster = hero.PartyBelongedTo.ItemRoster;
                    int itemTypesChanged = 0;
                    int totalItemsChanged = 0;
                    List<string> changedItems = new List<string>();

                    // Create a new roster with modified items
                    List<(ItemObject item, int count, ItemModifier oldModifier)> itemsToModify = new List<(ItemObject, int, ItemModifier)>();

                    for (int i = 0; i < roster.Count; i++)
                    {
                        var element = roster.GetElementCopyAtIndex(i);
                        if (ItemModifierHelper.CanHaveModifier(element.EquipmentElement.Item))
                        {
                            itemsToModify.Add((element.EquipmentElement.Item, element.Amount, element.EquipmentElement.ItemModifier));
                        }
                    }

                    // Remove old items and add with new modifier
                    foreach (var (item, count, oldModifier) in itemsToModify)
                    {
                        // Remove old version
                        roster.AddToCounts(new EquipmentElement(item, oldModifier), -count);
                        
                        // Add new version with modifier
                        roster.AddToCounts(new EquipmentElement(item, modifier), count);
                        
                        string oldModText = oldModifier != null ? $" ({oldModifier.Name})" : "";
                        changedItems.Add($"{count}x {item.Name}{oldModText}");
                        itemTypesChanged++;
                        totalItemsChanged += count;
                    }

                    if (itemTypesChanged == 0)
                        return CommandBase.FormatSuccessMessage($"{hero.Name}'s party has no items that can have modifiers.");

                    StringBuilder result = new StringBuilder();
                    result.AppendLine($"Set modifier '{modifier.Name}' on {totalItemsChanged} items ({itemTypesChanged} types) in {hero.Name}'s party:");
                    foreach (var item in changedItems.Take(10)) // Limit display to first 10
                    {
                        result.AppendLine($"  - {item}");
                    }
                    if (changedItems.Count > 10)
                    {
                        result.AppendLine($"  ... and {changedItems.Count - 10} more item types");
                    }

                    return result.ToString();
                }, "Failed to set inventory modifiers");
            });
        }

        /// <summary>
        /// Remove modifiers from all equipped items for a hero
        /// Usage: gm.item.remove_equipped_modifier [hero_query]
        /// </summary>
        [CommandLineFunctionality.CommandLineArgumentFunction("remove_equipped_modifier", "gm.item")]
        public static string RemoveEquippedModifier(List<string> args)
        {
            return Cmd.Run(args, () =>
            {
                if (!CommandBase.ValidateCampaignMode(out string error))
                    return error;

                var usageMessage = CommandValidator.CreateUsageMessage(
                    "gm.item.remove_equipped_modifier", "<hero_query>",
                    "Removes modifiers from all equipped items for a hero.",
                    "gm.item.remove_equipped_modifier player");

                if (!CommandBase.ValidateArgumentCount(args, 1, usageMessage, out error))
                    return error;

                var (hero, heroError) = CommandBase.FindSingleHero(args[0]);
                if (heroError != null) return heroError;

                return CommandBase.ExecuteWithErrorHandling(() =>
                {
                    int itemsChanged = 0;

                    // Process battle equipment
                    for (int i = 0; i < (int)EquipmentIndex.NumEquipmentSetSlots; i++)
                    {
                        EquipmentIndex slot = (EquipmentIndex)i;
                        var element = hero.BattleEquipment[slot];
                        if (!element.IsEmpty && element.ItemModifier != null)
                        {
                            hero.BattleEquipment[slot] = new EquipmentElement(element.Item);
                            itemsChanged++;
                        }
                    }

                    // Process civilian equipment
                    for (int i = 0; i < (int)EquipmentIndex.NumEquipmentSetSlots; i++)
                    {
                        EquipmentIndex slot = (EquipmentIndex)i;
                        var element = hero.CivilianEquipment[slot];
                        if (!element.IsEmpty && element.ItemModifier != null)
                        {
                            hero.CivilianEquipment[slot] = new EquipmentElement(element.Item);
                            itemsChanged++;
                        }
                    }

                    if (itemsChanged == 0)
                        return CommandBase.FormatSuccessMessage($"{hero.Name} has no equipped items with modifiers.");

                    return CommandBase.FormatSuccessMessage(
                        $"Removed modifiers from {itemsChanged} equipped items for {hero.Name}.");
                }, "Failed to remove equipped modifiers");
            });
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Determines the appropriate equipment slot for an item based on its type
        /// </summary>
        private static EquipmentIndex GetAppropriateSlotForItem(ItemObject item)
        {
            switch (item.ItemType)
            {
                case ItemObject.ItemTypeEnum.HeadArmor:
                    return EquipmentIndex.Head;
                case ItemObject.ItemTypeEnum.BodyArmor:
                    return EquipmentIndex.Body;
                case ItemObject.ItemTypeEnum.LegArmor:
                    return EquipmentIndex.Leg;
                case ItemObject.ItemTypeEnum.HandArmor:
                    return EquipmentIndex.Gloves;
                case ItemObject.ItemTypeEnum.Cape:
                    return EquipmentIndex.Cape;
                case ItemObject.ItemTypeEnum.Horse:
                    return EquipmentIndex.Horse;
                case ItemObject.ItemTypeEnum.HorseHarness:
                    return EquipmentIndex.HorseHarness;
                case ItemObject.ItemTypeEnum.OneHandedWeapon:
                case ItemObject.ItemTypeEnum.TwoHandedWeapon:
                case ItemObject.ItemTypeEnum.Polearm:
                case ItemObject.ItemTypeEnum.Bow:
                case ItemObject.ItemTypeEnum.Crossbow:
                case ItemObject.ItemTypeEnum.Thrown:
                case ItemObject.ItemTypeEnum.Arrows:
                case ItemObject.ItemTypeEnum.Bolts:
                case ItemObject.ItemTypeEnum.Shield:
                    return EquipmentIndex.Weapon0; // Use first weapon slot by default
                default:
                    return EquipmentIndex.None;
            }
        }

        /// <summary>
        /// Parses equipment slot name to EquipmentIndex enum
        /// </summary>
        private static bool TryParseEquipmentSlot(string slotName, out EquipmentIndex slot)
        {
            // Handle numeric weapon slots (Weapon0-3)
            if (slotName.StartsWith("Weapon", StringComparison.OrdinalIgnoreCase))
            {
                if (slotName.Length == 7 && char.IsDigit(slotName[6]))
                {
                    int weaponNum = int.Parse(slotName.Substring(6));
                    if (weaponNum >= 0 && weaponNum <= 3)
                    {
                        slot = EquipmentIndex.Weapon0 + weaponNum;
                        return true;
                    }
                }
            }

            // Try to parse as enum
            if (Enum.TryParse<EquipmentIndex>(slotName, true, out slot))
            {
                // Validate it's within equipment slot range
                if (slot >= EquipmentIndex.WeaponItemBeginSlot && slot < EquipmentIndex.NumEquipmentSetSlots)
                    return true;
            }

            slot = EquipmentIndex.None;
            return false;
        }

        #endregion
    }
}