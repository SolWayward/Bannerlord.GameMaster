# Item Management System - Complete Example

**Navigation:** [← Back: Implementation Workflow](workflow.md) | [Back to Index](../README.md) | [Next: Extensions Template →](../templates/extensions.md)

---

## Overview

This document demonstrates implementing a complete item management system from scratch, serving as a practical example of the [Implementation Workflow](workflow.md).

We'll implement:
- Item type categorization (weapon, armor, food, etc.)
- Item searching and filtering
- Console commands to query and manage items
- Comprehensive tests

---

## Step 1: Create ItemExtensions.cs

**File:** `Bannerlord.GameMaster/Items/ItemExtensions.cs`

```csharp
using System;
using TaleWorlds.Core;

namespace Bannerlord.GameMaster.Items
{
    [Flags]
    public enum ItemTypes
    {
        None = 0,
        Weapon = 1,
        Armor = 2,
        Mount = 4,
        Food = 8,
        Trade = 16,
        OneHanded = 32,
        TwoHanded = 64,
        Ranged = 128,
        Shield = 256
    }

    public static class ItemExtensions
    {
        public static ItemTypes GetItemTypes(this ItemObject item)
        {
            ItemTypes types = ItemTypes.None;

            if (item.IsFood) types |= ItemTypes.Food;
            if (item.IsTrade) types |= ItemTypes.Trade;
            
            if (item.ItemType == ItemObject.ItemTypeEnum.OneHandedWeapon)
                types |= ItemTypes.Weapon | ItemTypes.OneHanded;
            if (item.ItemType == ItemObject.ItemTypeEnum.TwoHandedWeapon)
                types |= ItemTypes.Weapon | ItemTypes.TwoHanded;
            if (item.ItemType == ItemObject.ItemTypeEnum.Bow)
                types |= ItemTypes.Weapon | ItemTypes.Ranged;
            if (item.ItemType == ItemObject.ItemTypeEnum.Shield)
                types |= ItemTypes.Shield;
            if (item.ArmorComponent != null)
                types |= ItemTypes.Armor;
            if (item.ItemType == ItemObject.ItemTypeEnum.Horse)
                types |= ItemTypes.Mount;

            return types;
        }

        public static bool HasAllTypes(this ItemObject item, ItemTypes types)
        {
            if (types == ItemTypes.None) return true;
            return (item.GetItemTypes() & types) == types;
        }

        public static bool HasAnyType(this ItemObject item, ItemTypes types)
        {
            if (types == ItemTypes.None) return true;
            return (item.GetItemTypes() & types) != ItemTypes.None;
        }

        public static string FormattedDetails(this ItemObject item)
        {
            return $"{item.StringId}\t{item.Name}\tValue: {item.Value}";
        }
    }
}
```

---

## Step 2: Create ItemQueries.cs

**File:** `Bannerlord.GameMaster/Items/ItemQueries.cs`

```csharp
using System;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.Core;

namespace Bannerlord.GameMaster.Items
{
    public static class ItemQueries
    {
        public static ItemObject GetItemById(string itemId)
        {
            return Items.All.FirstOrDefault(i => 
                i.StringId.Equals(itemId, StringComparison.OrdinalIgnoreCase));
        }

        public static List<ItemObject> QueryItems(
            string query = "",
            ItemTypes requiredTypes = ItemTypes.None,
            bool matchAll = true)
        {
            IEnumerable<ItemObject> items = Items.All;

            if (!string.IsNullOrEmpty(query))
            {
                string lowerFilter = query.ToLower();
                items = items.Where(i =>
                    i.Name.ToString().ToLower().Contains(lowerFilter) ||
                    i.StringId.ToLower().Contains(lowerFilter));
            }

            if (requiredTypes != ItemTypes.None)
            {
                items = items.Where(i => 
                    matchAll ? i.HasAllTypes(requiredTypes) : i.HasAnyType(requiredTypes));
            }

            return items.ToList();
        }

        public static ItemTypes ParseItemType(string typeString)
        {
            var normalized = typeString.ToLower() switch
            {
                "1h" => "OneHanded",
                "2h" => "TwoHanded",
                _ => typeString
            };

            return Enum.TryParse<ItemTypes>(normalized, true, out var result) 
                ? result : ItemTypes.None;
        }

        public static ItemTypes ParseItemTypes(IEnumerable<string> typeStrings)
        {
            ItemTypes combined = ItemTypes.None;
            foreach (var typeString in typeStrings)
            {
                combined |= ParseItemType(typeString);
            }
            return combined;
        }

        public static string GetFormattedDetails(List<ItemObject> items)
        {
            if (items.Count == 0) return "";
            return string.Join("\n", items.Select(i => i.FormattedDetails())) + "\n";
        }
    }
}
```

---

## Step 3: Add FindSingleItem to CommandBase

**Update:** `Bannerlord.GameMaster/Console/Common/CommandBase.cs`

Add this method to the Entity Finder Methods region:

```csharp
/// <summary>
/// Helper method to find a single item from a query
/// </summary>
public static (ItemObject item, string error) FindSingleItem(string query)
{
    List<ItemObject> matches = ItemQueries.QueryItems(query);

    if (matches == null || matches.Count == 0)
        return (null, $"Error: No item matching query '{query}' found.\n");

    if (matches.Count == 1)
        return (matches[0], null);

    return ResolveMultipleMatches(
        matches: matches,
        query: query,
        getStringId: i => i.StringId,
        getName: i => i.Name?.ToString() ?? "",
        formatDetails: ItemQueries.GetFormattedDetails,
        entityType: "item");
}
```

---

## Step 4: Create ItemQueryCommands.cs

**File:** `Bannerlord.GameMaster/Console/Query/ItemQueryCommands.cs`

```csharp
using Bannerlord.GameMaster.Items;
using Bannerlord.GameMaster.Console.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Library;

namespace Bannerlord.GameMaster.Console.Query
{
    [CommandLineFunctionality.CommandLineArgumentFunction("query", "gm")]
    public static class ItemQueryCommands
    {
        private static (string query, ItemTypes types) ParseArguments(List<string> args)
        {
            if (args == null || args.Count == 0)
                return ("", ItemTypes.None);

            var typeKeywords = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "weapon", "armor", "mount", "food", "trade", 
                "1h", "onehanded", "2h", "twohanded", "ranged", "shield"
            };

            List<string> searchTerms = new();
            List<string> typeTerms = new();

            foreach (var arg in args)
            {
                if (typeKeywords.Contains(arg.ToLower()))
                    typeTerms.Add(arg);
                else
                    searchTerms.Add(arg);
            }

            return (string.Join(" ", searchTerms).Trim(), 
                    ItemQueries.ParseItemTypes(typeTerms));
        }

        [CommandLineFunctionality.CommandLineArgumentFunction("item", "gm.query")]
        public static string QueryItems(List<string> args)
        {
            return Cmd.Run(args, () =>
            {
                if (!CommandBase.ValidateCampaignMode(out string error))
                    return error;

                var (query, types) = ParseArguments(args);
                List<ItemObject> matches = ItemQueries.QueryItems(query, types, matchAll: true);

                if (matches.Count == 0)
                {
                    return $"Found 0 item(s)\n" +
                           "Usage: gm.query.item [search] [type keywords]\n" +
                           "Example: gm.query.item sword weapon 1h\n";
                }

                return $"Found {matches.Count} item(s):\n" +
                       ItemQueries.GetFormattedDetails(matches);
            });
        }

        [CommandLineFunctionality.CommandLineArgumentFunction("item_info", "gm.query")]
        public static string QueryItemInfo(List<string> args)
        {
            return Cmd.Run(args, () =>
            {
                if (!CommandBase.ValidateCampaignMode(out string error))
                    return error;

                if (args == null || args.Count == 0)
                    return "Error: Please provide an item ID.\n";

                ItemObject item = ItemQueries.GetItemById(args[0]);
                if (item == null)
                    return $"Error: Item '{args[0]}' not found.\n";

                return $"Item Information:\n" +
                       $"ID: {item.StringId}\n" +
                       $"Name: {item.Name}\n" +
                       $"Type: {item.ItemType}\n" +
                       $"Value: {item.Value}\n" +
                       $"Types: {item.GetItemTypes()}\n";
            });
        }
    }
}
```

---

## Step 5: Create ItemManagementCommands.cs

**File:** `Bannerlord.GameMaster/Console/ItemManagementCommands.cs`

```csharp
using Bannerlord.GameMaster.Console.Common;
using Bannerlord.GameMaster.Items;
using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Library;

namespace Bannerlord.GameMaster.Console
{
    [CommandLineFunctionality.CommandLineArgumentFunction("item", "gm")]
    public static class ItemManagementCommands
    {
        #region Item Transfer

        [CommandLineFunctionality.CommandLineArgumentFunction("give", "gm.item")]
        public static string GiveItem(List<string> args)
        {
            return Cmd.Run(args, () =>
            {
                if (!CommandBase.ValidateCampaignMode(out string error))
                    return error;

                var usageMessage = CommandValidator.CreateUsageMessage(
                    "gm.item.give", "<hero> <item_id> <quantity>",
                    "Gives items to hero's inventory.",
                    "gm.item.give lord_1_1 grain 50");

                if (!CommandBase.ValidateArgumentCount(args, 3, usageMessage, out error))
                    return error;

                var (hero, heroError) = CommandBase.FindSingleHero(args[0]);
                if (heroError != null) return heroError;

                var (item, itemError) = CommandBase.FindSingleItem(args[1]);
                if (itemError != null) return itemError;

                if (!CommandValidator.ValidateIntegerRange(args[2], 1, 10000, 
                    out int quantity, out string qtyError))
                    return CommandBase.FormatErrorMessage(qtyError);

                return CommandBase.ExecuteWithErrorHandling(() =>
                {
                    hero.PartyBelongedTo?.ItemRoster?.AddToCounts(item, quantity);
                    return CommandBase.FormatSuccessMessage(
                        $"Gave {quantity}x {item.Name} to {hero.Name}.");
                }, "Failed to give item");
            });
        }

        #endregion
    }
}
```

---

## Step 6: Create ItemTests.cs

**File:** `Bannerlord.GameMaster/Console/Testing/ItemTests.cs`

```csharp
using Bannerlord.GameMaster.Console.Testing;

namespace Bannerlord.GameMaster.Console.Testing
{
    public static class ItemTests
    {
        public static void RegisterAll()
        {
            TestRunner.RegisterTest(new TestCase(
                "item_query_001",
                "Query items should return results",
                "gm.query.item",
                TestExpectation.Contains
            )
            {
                Category = "ItemQuery",
                ExpectedText = "item(s)"
            });

            TestRunner.RegisterTest(new TestCase(
                "item_query_002",
                "Item info without args should error",
                "gm.query.item_info",
                TestExpectation.Error
            )
            {
                Category = "ItemQuery",
                ExpectedText = "Please provide"
            });

            TestRunner.RegisterTest(new TestCase(
                "item_mgmt_001",
                "Give item without args should error",
                "gm.item.give",
                TestExpectation.Error
            )
            {
                Category = "ItemManagement",
                ExpectedText = "Missing arguments"
            });
        }
    }
}
```

---

## Component Interaction Flow

Here's how the components work together:

```
User Input: "gm.query.item sword weapon"
    ↓
ItemQueryCommands.QueryItems()
    ↓
ParseArguments() → query="sword", types=Weapon
    ↓
ItemQueries.QueryItems("sword", Weapon, true)
    ↓
Filter Items.All by name/ID containing "sword"
    ↓
Filter by HasAllTypes(Weapon)
    ↓
ItemExtensions.GetItemTypes() for each item
    ↓
Return filtered list
    ↓
Format output with ItemQueries.GetFormattedDetails()
    ↓
Return to user: "Found 15 item(s):\n..."
```

---

## Testing the Implementation

### Run Tests

```bash
# Register the tests (add to StandardTests.RegisterAll())
ItemTests.RegisterAll();

# Run all item tests
gm.test.run_category ItemQuery
gm.test.run_category ItemManagement

# Or run all tests
gm.test.run_all
```

### Manual Testing

```bash
# Query all items
gm.query.item

# Query weapons
gm.query.item weapon

# Query one-handed swords
gm.query.item sword 1h

# Get item info
gm.query.item_info grain

# Give items to hero
gm.item.give lord_1_1 grain 50
```

---

## Summary

This example demonstrates:

✅ **Complete implementation** of all five layers  
✅ **Following established patterns** from Heroes/Clans/Kingdoms  
✅ **Proper validation** and error handling  
✅ **Comprehensive tests** for all commands  
✅ **Smart entity resolution** via CommandBase  
✅ **Consistent formatting** and naming  

---

## Next Steps

Now that you've seen a complete example:

1. **Use** [Templates](../templates/extensions.md) for your own implementations
2. **Follow** [Best Practices](../guides/best-practices.md) for consistency
3. **Check** [Code Quality Checklist](../reference/code-quality-checklist.md) before committing

---

**Navigation:** [← Back: Implementation Workflow](workflow.md) | [Back to Index](../README.md) | [Next: Extensions Template →](../templates/extensions.md)