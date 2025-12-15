using System.Collections.Generic;

namespace Bannerlord.GameMaster.Console.Testing
{
    /// <summary>
    /// Contains standard test definitions for validating console commands
    /// </summary>
    public static class StandardTests
    {
        /// <summary>
        /// Register all standard tests
        /// </summary>
        public static void RegisterAll()
        {
        	TestRunner.ClearTests();
        	
        	RegisterHeroQueryTests();
        	RegisterClanQueryTests();
        	RegisterKingdomQueryTests();
        	RegisterItemQueryTests();
        	RegisterHeroManagementTests();
        	RegisterClanManagementTests();
        	RegisterKingdomManagementTests();
        	RegisterItemManagementTests();
        	RegisterItemEquipmentSaveTests();
        	RegisterItemEquipmentLoadTests();
        	RegisterSuccessPathTests();
        	RegisterSortingTests();
        }

        /// <summary>
        /// Register hero query command tests
        /// </summary>
        private static void RegisterHeroQueryTests()
        {
            // Test basic hero query
            TestRunner.RegisterTest(new TestCase(
                "hero_query_001",
                "Query heroes without parameters should return all living heroes",
                "gm.query.hero",
                TestExpectation.Contains
            )
            {
                Category = "HeroQuery",
                ExpectedText = "hero(es) matching"
            });

            // Test hero query with type filter
            TestRunner.RegisterTest(new TestCase(
                "hero_query_002",
                "Query heroes with 'lord' filter",
                "gm.query.hero lord",
                TestExpectation.Contains
            )
            {
                Category = "HeroQuery",
                ExpectedText = "hero(es) matching"
            });

            // Test hero query with name search
            TestRunner.RegisterTest(new TestCase(
                "hero_query_003",
                "Query heroes with specific name",
                "gm.query.hero aserai",
                TestExpectation.Contains
            )
            {
                Category = "HeroQuery",
                ExpectedText = "hero(es) matching"
            });

            // Test hero_any query
            TestRunner.RegisterTest(new TestCase(
                "hero_query_004",
                "Query heroes matching ANY criteria (lord OR wanderer)",
                "gm.query.hero_any lord wanderer",
                TestExpectation.Contains
            )
            {
                Category = "HeroQuery",
                ExpectedText = "hero(es) matching ANY"
            });

            // Test hero info - should error without ID
            TestRunner.RegisterTest(new TestCase(
                "hero_query_005",
                "Hero info without ID should return error",
                "gm.query.hero_info",
                TestExpectation.Error
            )
            {
                Category = "HeroQuery",
                ExpectedText = "Please provide a hero ID"
            });

            // Test invalid hero ID
            TestRunner.RegisterTest(new TestCase(
                "hero_query_006",
                "Hero info with invalid ID should return error",
                "gm.query.hero_info invalid_hero_id_xyz",
                TestExpectation.Error
            )
            {
                Category = "HeroQuery",
                ExpectedText = "not found"
            });
        }

        /// <summary>
        /// Register clan query command tests
        /// </summary>
        private static void RegisterClanQueryTests()
        {
            // Test basic clan query
            TestRunner.RegisterTest(new TestCase(
                "clan_query_001",
                "Query clans without parameters should return all clans",
                "gm.query.clan",
                TestExpectation.Contains
            )
            {
                Category = "ClanQuery",
                ExpectedText = "clan(s) matching"
            });

            // Test clan query with name search
            TestRunner.RegisterTest(new TestCase(
                "clan_query_002",
                "Query clans with specific name",
                "gm.query.clan empire",
                TestExpectation.Contains
            )
            {
                Category = "ClanQuery",
                ExpectedText = "clan(s) matching"
            });

            // Test clan_any query
            TestRunner.RegisterTest(new TestCase(
                "clan_query_003",
                "Query clans matching ANY criteria",
                "gm.query.clan_any empire battania",
                TestExpectation.Contains
            )
            {
                Category = "ClanQuery",
                ExpectedText = "clan(s) matching ANY"
            });

            // Test clan info without ID - should error
            TestRunner.RegisterTest(new TestCase(
                "clan_query_004",
                "Clan info without ID should return error",
                "gm.query.clan_info",
                TestExpectation.Error
            )
            {
                Category = "ClanQuery",
                ExpectedText = "Please provide a clan ID"
            });
        }

        /// <summary>
        /// Register kingdom query command tests
        /// </summary>
        private static void RegisterKingdomQueryTests()
        {
            // Test basic kingdom query
            TestRunner.RegisterTest(new TestCase(
                "kingdom_query_001",
                "Query kingdoms without parameters should return all kingdoms",
                "gm.query.kingdom",
                TestExpectation.Contains
            )
            {
                Category = "KingdomQuery",
                ExpectedText = "kingdom(s) matching"
            });

            // Test kingdom query with name search
            TestRunner.RegisterTest(new TestCase(
                "kingdom_query_002",
                "Query kingdoms with specific name",
                "gm.query.kingdom empire",
                TestExpectation.Contains
            )
            {
                Category = "KingdomQuery",
                ExpectedText = "kingdom(s) matching"
            });

            // Test kingdom_any query
            TestRunner.RegisterTest(new TestCase(
                "kingdom_query_003",
                "Query kingdoms matching ANY criteria",
                "gm.query.kingdom_any empire battania",
                TestExpectation.Contains
            )
            {
                Category = "KingdomQuery",
                ExpectedText = "kingdom(s) matching ANY"
            });

            // Test kingdom info without ID - should error
            TestRunner.RegisterTest(new TestCase(
                "kingdom_query_004",
                "Kingdom info without ID should return error",
                "gm.query.kingdom_info",
                TestExpectation.Error
            )
            {
                Category = "KingdomQuery",
                ExpectedText = "Please provide a kingdom ID"
            });
        }

        /// <summary>
        /// Register item query command tests
        /// </summary>
        private static void RegisterItemQueryTests()
        {
            // Test basic item query
            TestRunner.RegisterTest(new TestCase(
                "item_query_001",
                "Query items without parameters should return all items",
                "gm.query.item",
                TestExpectation.Contains
            )
            {
                Category = "ItemQuery",
                ExpectedText = "item(s) matching"
            });

            // Test item query with type filter
            TestRunner.RegisterTest(new TestCase(
                "item_query_002",
                "Query items with 'weapon' filter",
                "gm.query.item weapon",
                TestExpectation.Contains
            )
            {
                Category = "ItemQuery",
                ExpectedText = "item(s) matching"
            });

            // Test item query with name search
            TestRunner.RegisterTest(new TestCase(
                "item_query_003",
                "Query items with specific name",
                "gm.query.item sword",
                TestExpectation.Contains
            )
            {
                Category = "ItemQuery",
                ExpectedText = "item(s) matching"
            });

            // Test item_any query
            TestRunner.RegisterTest(new TestCase(
                "item_query_004",
                "Query items matching ANY criteria (weapon OR armor)",
                "gm.query.item_any weapon armor",
                TestExpectation.Contains
            )
            {
                Category = "ItemQuery",
                ExpectedText = "item(s) matching ANY"
            });

            // Test item info - should error without ID
            TestRunner.RegisterTest(new TestCase(
                "item_query_005",
                "Item info without ID should return error",
                "gm.query.item_info",
                TestExpectation.Error
            )
            {
                Category = "ItemQuery",
                ExpectedText = "Please provide an item ID"
            });

            // Test invalid item ID
            TestRunner.RegisterTest(new TestCase(
                "item_query_006",
                "Item info with invalid ID should return error",
                "gm.query.item_info invalid_item_id_xyz",
                TestExpectation.Error
            )
            {
                Category = "ItemQuery",
                ExpectedText = "not found"
            });

            // Test item query with specific armor type
            TestRunner.RegisterTest(new TestCase(
                "item_query_007",
                "Query items with specific armor type",
                "gm.query.item armor head",
                TestExpectation.Contains
            )
            {
                Category = "ItemQuery",
                ExpectedText = "item(s) matching"
            });

            // Test item query with weapon subtype
            TestRunner.RegisterTest(new TestCase(
                "item_query_008",
                "Query items for one-handed weapons",
                "gm.query.item weapon 1h",
                TestExpectation.Contains
            )
            {
                Category = "ItemQuery",
                ExpectedText = "item(s) matching"
            });

            // Test bow filter
            TestRunner.RegisterTest(new TestCase(
                "item_query_009",
                "Query items with 'bow' filter",
                "gm.query.item bow",
                TestExpectation.Contains
            )
            {
                Category = "ItemQuery",
                ExpectedText = "item(s) matching"
            });

            // Test crossbow filter
            TestRunner.RegisterTest(new TestCase(
                "item_query_010",
                "Query items with 'crossbow' filter",
                "gm.query.item crossbow",
                TestExpectation.Contains
            )
            {
                Category = "ItemQuery",
                ExpectedText = "item(s) matching"
            });

            // Test civilian filter
            TestRunner.RegisterTest(new TestCase(
                "item_query_011",
                "Query items with 'civilian' filter",
                "gm.query.item civilian",
                TestExpectation.Contains
            )
            {
                Category = "ItemQuery",
                ExpectedText = "item(s) matching"
            });

            // Test combat filter
            TestRunner.RegisterTest(new TestCase(
                "item_query_012",
                "Query items with 'combat' filter",
                "gm.query.item combat",
                TestExpectation.Contains
            )
            {
                Category = "ItemQuery",
                ExpectedText = "item(s) matching"
            });

            // Test horsearmor filter
            TestRunner.RegisterTest(new TestCase(
                "item_query_013",
                "Query items with 'horsearmor' filter",
                "gm.query.item horsearmor",
                TestExpectation.Contains
            )
            {
                Category = "ItemQuery",
                ExpectedText = "item(s) matching"
            });

            // Test tier filtering - tier3
            TestRunner.RegisterTest(new TestCase(
                "item_query_014",
                "Query items with tier3 filter",
                "gm.query.item tier3",
                TestExpectation.Contains
            )
            {
                Category = "ItemQuery",
                ExpectedText = "item(s) matching"
            });

            // Test tier filtering - tier5
            TestRunner.RegisterTest(new TestCase(
                "item_query_015",
                "Query items with tier5 filter",
                "gm.query.item tier5",
                TestExpectation.Contains
            )
            {
                Category = "ItemQuery",
                ExpectedText = "item(s) matching"
            });

            // Test sorting by name
            TestRunner.RegisterTest(new TestCase(
                "item_query_016",
                "Query items sorted by name",
                "gm.query.item sort:name",
                TestExpectation.Contains
            )
            {
                Category = "ItemQuery",
                ExpectedText = "item(s) matching"
            });

            // Test sorting by value descending
            TestRunner.RegisterTest(new TestCase(
                "item_query_017",
                "Query items sorted by value descending",
                "gm.query.item sort:value:desc",
                TestExpectation.Contains
            )
            {
                Category = "ItemQuery",
                ExpectedText = "item(s) matching"
            });

            // Test sorting by tier ascending
            TestRunner.RegisterTest(new TestCase(
                "item_query_018",
                "Query items sorted by tier ascending",
                "gm.query.item sort:tier:asc",
                TestExpectation.Contains
            )
            {
                Category = "ItemQuery",
                ExpectedText = "item(s) matching"
            });

            // Test combined: bow + tier5
            TestRunner.RegisterTest(new TestCase(
                "item_query_019",
                "Query tier 5 bows",
                "gm.query.item bow tier5",
                TestExpectation.Contains
            )
            {
                Category = "ItemQuery",
                ExpectedText = "item(s) matching"
            });

            // Test combined: armor + combat + tier4
            TestRunner.RegisterTest(new TestCase(
                "item_query_020",
                "Query tier 4 combat armor",
                "gm.query.item armor combat tier4",
                TestExpectation.Contains
            )
            {
                Category = "ItemQuery",
                ExpectedText = "item(s) matching"
            });

            // Test combined: bow + tier5 + sort
            TestRunner.RegisterTest(new TestCase(
                "item_query_021",
                "Query tier 5 bows sorted by value descending",
                "gm.query.item bow tier5 sort:value:desc",
                TestExpectation.Contains
            )
            {
                Category = "ItemQuery",
                ExpectedText = "item(s) matching"
            });

            // Test item_any with bow and crossbow
            TestRunner.RegisterTest(new TestCase(
                "item_query_022",
                "Query items matching bow OR crossbow",
                "gm.query.item_any bow crossbow",
                TestExpectation.Contains
            )
            {
                Category = "ItemQuery",
                ExpectedText = "item(s) matching ANY"
            });

            // Test item_any with tier filtering
            TestRunner.RegisterTest(new TestCase(
                "item_query_023",
                "Query tier 4 items matching 1h OR 2h weapons",
                "gm.query.item_any 1h 2h tier4",
                TestExpectation.Contains
            )
            {
                Category = "ItemQuery",
                ExpectedText = "item(s) matching ANY"
            });

            // Test sorting by type
            TestRunner.RegisterTest(new TestCase(
                "item_query_024",
                "Query items sorted by type",
                "gm.query.item weapon sort:type",
                TestExpectation.Contains
            )
            {
                Category = "ItemQuery",
                ExpectedText = "item(s) matching"
            });

            // Test civilian armor
            TestRunner.RegisterTest(new TestCase(
                "item_query_025",
                "Query civilian armor items",
                "gm.query.item armor civilian",
                TestExpectation.Contains
            )
            {
                Category = "ItemQuery",
                ExpectedText = "item(s) matching"
            });
        }

        /// <summary>
        /// Register hero management command tests
        /// </summary>
        private static void RegisterHeroManagementTests()
        {
            // Test set_clan without arguments - should error
            TestRunner.RegisterTest(new TestCase(
                "hero_mgmt_001",
                "Set clan without arguments should return usage error",
                "gm.hero.set_clan",
                TestExpectation.Error
            )
            {
                Category = "HeroManagement",
                ExpectedText = "Missing arguments"
            });

            // Test set_age without arguments - should error
            TestRunner.RegisterTest(new TestCase(
                "hero_mgmt_002",
                "Set age without arguments should return usage error",
                "gm.hero.set_age",
                TestExpectation.Error
            )
            {
                Category = "HeroManagement",
                ExpectedText = "Missing arguments"
            });

            // Test set_gold without arguments - should error
            TestRunner.RegisterTest(new TestCase(
                "hero_mgmt_003",
                "Set gold without arguments should return usage error",
                "gm.hero.set_gold",
                TestExpectation.Error
            )
            {
                Category = "HeroManagement",
                ExpectedText = "Missing arguments"
            });

            // Test kill without arguments - should error
            TestRunner.RegisterTest(new TestCase(
                "hero_mgmt_004",
                "Kill hero without arguments should return usage error",
                "gm.hero.kill",
                TestExpectation.Error
            )
            {
                Category = "HeroManagement",
                ExpectedText = "Missing arguments"
            });

            // Test heal without arguments - should error
            TestRunner.RegisterTest(new TestCase(
                "hero_mgmt_005",
                "Heal hero without arguments should return usage error",
                "gm.hero.heal",
                TestExpectation.Error
            )
            {
                Category = "HeroManagement",
                ExpectedText = "Missing arguments"
            });

            // Test set_relation without arguments - should error
            TestRunner.RegisterTest(new TestCase(
                "hero_mgmt_006",
                "Set relation without arguments should return usage error",
                "gm.hero.set_relation",
                TestExpectation.Error
            )
            {
                Category = "HeroManagement",
                ExpectedText = "Missing arguments"
            });
        }

        /// <summary>
        /// Register clan management command tests
        /// </summary>
        private static void RegisterClanManagementTests()
        {
            // Test add_hero without arguments - should error
            TestRunner.RegisterTest(new TestCase(
                "clan_mgmt_001",
                "Add hero to clan without arguments should return usage error",
                "gm.clan.add_hero",
                TestExpectation.Error
            )
            {
                Category = "ClanManagement",
                ExpectedText = "Missing arguments"
            });

            // Test set_gold without arguments - should error
            TestRunner.RegisterTest(new TestCase(
                "clan_mgmt_002",
                "Set clan gold without arguments should return usage error",
                "gm.clan.set_gold",
                TestExpectation.Error
            )
            {
                Category = "ClanManagement",
                ExpectedText = "Missing arguments"
            });

            // Test add_gold without arguments - should error
            TestRunner.RegisterTest(new TestCase(
                "clan_mgmt_003",
                "Add clan gold without arguments should return usage error",
                "gm.clan.add_gold",
                TestExpectation.Error
            )
            {
                Category = "ClanManagement",
                ExpectedText = "Missing arguments"
            });

            // Test set_renown without arguments - should error
            TestRunner.RegisterTest(new TestCase(
                "clan_mgmt_004",
                "Set clan renown without arguments should return usage error",
                "gm.clan.set_renown",
                TestExpectation.Error
            )
            {
                Category = "ClanManagement",
                ExpectedText = "Missing arguments"
            });

            // Test set_tier without arguments - should error
            TestRunner.RegisterTest(new TestCase(
                "clan_mgmt_005",
                "Set clan tier without arguments should return usage error",
                "gm.clan.set_tier",
                TestExpectation.Error
            )
            {
                Category = "ClanManagement",
                ExpectedText = "Missing arguments"
            });
        }

        /// <summary>
        /// Register kingdom management command tests
        /// </summary>
        private static void RegisterKingdomManagementTests()
        {
            // Test add_clan without arguments - should error
            TestRunner.RegisterTest(new TestCase(
                "kingdom_mgmt_001",
                "Add clan to kingdom without arguments should return usage error",
                "gm.kingdom.add_clan",
                TestExpectation.Error
            )
            {
                Category = "KingdomManagement",
                ExpectedText = "Missing arguments"
            });

            // Test remove_clan without arguments - should error
            TestRunner.RegisterTest(new TestCase(
                "kingdom_mgmt_002",
                "Remove clan from kingdom without arguments should return usage error",
                "gm.kingdom.remove_clan",
                TestExpectation.Error
            )
            {
                Category = "KingdomManagement",
                ExpectedText = "Missing arguments"
            });

            // Test declare_war without arguments - should error
            TestRunner.RegisterTest(new TestCase(
                "kingdom_mgmt_003",
                "Declare war without arguments should return usage error",
                "gm.kingdom.declare_war",
                TestExpectation.Error
            )
            {
                Category = "KingdomManagement",
                ExpectedText = "Missing arguments"
            });

            // Test make_peace without arguments - should error
            TestRunner.RegisterTest(new TestCase(
                "kingdom_mgmt_004",
                "Make peace without arguments should return usage error",
                "gm.kingdom.make_peace",
                TestExpectation.Error
            )
            {
                Category = "KingdomManagement",
                ExpectedText = "Missing arguments"
            });

            // Test set_ruler without arguments - should error
            TestRunner.RegisterTest(new TestCase(
                "kingdom_mgmt_005",
                "Set kingdom ruler without arguments should return usage error",
                "gm.kingdom.set_ruler",
                TestExpectation.Error
            )
            {
                Category = "KingdomManagement",
                ExpectedText = "Missing arguments"
            });
        }
      
        /// <summary>
        /// Register item management command tests
        /// </summary>
        private static void RegisterItemManagementTests()
        {
            // Test add item without arguments - should error
            TestRunner.RegisterTest(new TestCase(
                "item_mgmt_001",
                "Add item without arguments should return usage error",
                "gm.item.add",
                TestExpectation.Error
            )
            {
                Category = "ItemManagement",
                ExpectedText = "Missing arguments"
            });

            // Test remove item without arguments - should error
            TestRunner.RegisterTest(new TestCase(
                "item_mgmt_002",
                "Remove item without arguments should return usage error",
                "gm.item.remove",
                TestExpectation.Error
            )
            {
                Category = "ItemManagement",
                ExpectedText = "Missing arguments"
            });

            // Test remove_all without arguments - should error
            TestRunner.RegisterTest(new TestCase(
                "item_mgmt_003",
                "Remove all items without arguments should return usage error",
                "gm.item.remove_all",
                TestExpectation.Error
            )
            {
                Category = "ItemManagement",
                ExpectedText = "Missing arguments"
            });

            // Test transfer item without arguments - should error
            TestRunner.RegisterTest(new TestCase(
                "item_mgmt_004",
                "Transfer item without arguments should return usage error",
                "gm.item.transfer",
                TestExpectation.Error
            )
            {
                Category = "ItemManagement",
                ExpectedText = "Missing arguments"
            });

            // Test unequip_all without arguments - should error
            TestRunner.RegisterTest(new TestCase(
                "item_mgmt_005",
                "Unequip all items without arguments should return usage error",
                "gm.item.unequip_all",
                TestExpectation.Error
            )
            {
                Category = "ItemManagement",
                ExpectedText = "Missing arguments"
            });

            // Test equip without arguments - should error
            TestRunner.RegisterTest(new TestCase(
                "item_mgmt_006",
                "Equip item without arguments should return usage error",
                "gm.item.equip",
                TestExpectation.Error
            )
            {
                Category = "ItemManagement",
                ExpectedText = "Missing arguments"
            });

            // Test unequip without arguments - should error
            TestRunner.RegisterTest(new TestCase(
                "item_mgmt_007",
                "Unequip item without arguments should return usage error",
                "gm.item.unequip",
                TestExpectation.Error
            )
            {
                Category = "ItemManagement",
                ExpectedText = "Missing arguments"
            });

            // Test equip_slot without arguments - should error
            TestRunner.RegisterTest(new TestCase(
                "item_mgmt_008",
                "Equip item to slot without arguments should return usage error",
                "gm.item.equip_slot",
                TestExpectation.Error
            )
            {
                Category = "ItemManagement",
                ExpectedText = "Missing arguments"
            });

            // Test unequip_slot without arguments - should error
            TestRunner.RegisterTest(new TestCase(
                "item_mgmt_009",
                "Unequip slot without arguments should return usage error",
                "gm.item.unequip_slot",
                TestExpectation.Error
            )
            {
                Category = "ItemManagement",
                ExpectedText = "Missing arguments"
            });

            // Test list_equipped without arguments - should error
            TestRunner.RegisterTest(new TestCase(
                "item_mgmt_010",
                "List equipped items without arguments should return usage error",
                "gm.item.list_equipped",
                TestExpectation.Error
            )
            {
                Category = "ItemManagement",
                ExpectedText = "Missing arguments"
            });

            // Test remove_equipped without arguments - should error
            TestRunner.RegisterTest(new TestCase(
                "item_mgmt_011",
                "Remove equipped items without arguments should return usage error",
                "gm.item.remove_equipped",
                TestExpectation.Error
            )
            {
                Category = "ItemManagement",
                ExpectedText = "Missing arguments"
            });

            // Test list_inventory without arguments - should error
            TestRunner.RegisterTest(new TestCase(
                "item_mgmt_012",
                "List inventory without arguments should return usage error",
                "gm.item.list_inventory",
                TestExpectation.Error
            )
            {
                Category = "ItemManagement",
                ExpectedText = "Missing arguments"
            });

            // Test add item with invalid item ID
            TestRunner.RegisterTest(new TestCase(
                "item_mgmt_013",
                "Add item with invalid item ID should return error",
                "gm.item.add invalid_item_xyz 1 player",
                TestExpectation.Error
            )
            {
                Category = "ItemManagement",
                ExpectedText = "No item matching"
            });

            // Test add item with invalid hero
            TestRunner.RegisterTest(new TestCase(
                "item_mgmt_014",
                "Add item with invalid hero should return error",
                "gm.item.add sword 1 invalid_hero_xyz",
                TestExpectation.Error
            )
            {
                Category = "ItemManagement",
                ExpectedText = "No hero matching"
            });

            // Test add item with invalid count (negative)
            TestRunner.RegisterTest(new TestCase(
                "item_mgmt_015",
                "Add item with negative count should return error",
                "gm.item.add sword -5 player",
                TestExpectation.Error
            )
            {
                Category = "ItemManagement",
                ExpectedText = "Error"
            });

            // Test equip_slot with invalid slot name
            TestRunner.RegisterTest(new TestCase(
                "item_mgmt_016",
                "Equip to invalid slot should return error",
                "gm.item.equip_slot sword player InvalidSlot",
                TestExpectation.Error
            )
            {
                Category = "ItemManagement",
                ExpectedText = "Invalid equipment slot"
            });

            // Test list_inventory with invalid hero
            TestRunner.RegisterTest(new TestCase(
                "item_mgmt_017",
                "List inventory with invalid hero should return error",
                "gm.item.list_inventory invalid_hero_xyz",
                TestExpectation.Error
            )
            {
                Category = "ItemManagement",
                ExpectedText = "No hero matching"
            });

            // Test remove_equipped with invalid hero
            TestRunner.RegisterTest(new TestCase(
                "item_mgmt_018",
                "Remove equipped with invalid hero should return error",
                "gm.item.remove_equipped invalid_hero_xyz",
                TestExpectation.Error
            )
            {
                Category = "ItemManagement",
                ExpectedText = "No hero matching"
            });
        }

        /// <summary>
        /// Register item modifier management command tests
        /// </summary>
        private static void RegisterItemModifierManagementTests()
        {
            // Test set_equipped_modifier without arguments - should error
            TestRunner.RegisterTest(new TestCase(
                "modifier_mgmt_001",
                "Set equipped modifier without arguments should return usage error",
                "gm.item.set_equipped_modifier",
                TestExpectation.Error
            )
            {
                Category = "ModifierManagement",
                ExpectedText = "Missing arguments"
            });

            // Test set_inventory_modifier without arguments - should error
            TestRunner.RegisterTest(new TestCase(
                "modifier_mgmt_002",
                "Set inventory modifier without arguments should return usage error",
                "gm.item.set_inventory_modifier",
                TestExpectation.Error
            )
            {
                Category = "ModifierManagement",
                ExpectedText = "Missing arguments"
            });

            // Test remove_equipped_modifier without arguments - should error
            TestRunner.RegisterTest(new TestCase(
                "modifier_mgmt_003",
                "Remove equipped modifier without arguments should return usage error",
                "gm.item.remove_equipped_modifier",
                TestExpectation.Error
            )
            {
                Category = "ModifierManagement",
                ExpectedText = "Missing arguments"
            });

            // Test set_equipped_modifier with invalid hero
            TestRunner.RegisterTest(new TestCase(
                "modifier_mgmt_004",
                "Set equipped modifier with invalid hero should return error",
                "gm.item.set_equipped_modifier invalid_hero_xyz masterwork",
                TestExpectation.Error
            )
            {
                Category = "ModifierManagement",
                ExpectedText = "No hero matching"
            });

            // Test set_equipped_modifier with invalid modifier
            TestRunner.RegisterTest(new TestCase(
                "modifier_mgmt_005",
                "Set equipped modifier with invalid modifier should return error",
                "gm.item.set_equipped_modifier player invalid_modifier_xyz",
                TestExpectation.Error
            )
            {
                Category = "ModifierManagement",
                ExpectedText = "not found"
            });

            // Test set_inventory_modifier with invalid hero
            TestRunner.RegisterTest(new TestCase(
                "modifier_mgmt_006",
                "Set inventory modifier with invalid hero should return error",
                "gm.item.set_inventory_modifier invalid_hero_xyz fine",
                TestExpectation.Error
            )
            {
                Category = "ModifierManagement",
                ExpectedText = "No hero matching"
            });

            // Test set_inventory_modifier with invalid modifier
            TestRunner.RegisterTest(new TestCase(
                "modifier_mgmt_007",
                "Set inventory modifier with invalid modifier should return error",
                "gm.item.set_inventory_modifier player invalid_modifier_xyz",
                TestExpectation.Error
            )
            {
                Category = "ModifierManagement",
                ExpectedText = "not found"
            });

            // Test remove_equipped_modifier with invalid hero
            TestRunner.RegisterTest(new TestCase(
                "modifier_mgmt_008",
                "Remove equipped modifier with invalid hero should return error",
                "gm.item.remove_equipped_modifier invalid_hero_xyz",
                TestExpectation.Error
            )
            {
                Category = "ModifierManagement",
                ExpectedText = "No hero matching"
            });

            // Test add item with modifier
            TestRunner.RegisterTest(new TestCase(
                "modifier_mgmt_009",
                "Add item with valid modifier should succeed or give appropriate feedback",
                "gm.item.add sword 1 player fine",
                TestExpectation.NoException
            )
            {
                Category = "ModifierManagement"
            });

            // Test add item with invalid modifier
            TestRunner.RegisterTest(new TestCase(
                "modifier_mgmt_010",
                "Add item with invalid modifier should return error",
                "gm.item.add sword 1 player invalid_modifier_xyz",
                TestExpectation.Error
            )
            {
                Category = "ModifierManagement",
                ExpectedText = "not found"
            });
        }

        /// <summary>
        /// Register equipment save command tests
        /// Tests for: save_equipment, save_equipment_civilian, save_equipment_both
        /// </summary>
        private static void RegisterItemEquipmentSaveTests()
        {
            // ===== SAVE_EQUIPMENT TESTS =====
            
            // Test save_equipment without arguments - should error
            TestRunner.RegisterTest(new TestCase(
                "equipment_save_001",
                "Save equipment without arguments should return usage error",
                "gm.item.save_equipment",
                TestExpectation.Error
            )
            {
                Category = "ItemEquipmentSave",
                ExpectedText = "Missing arguments"
            });

            // Test save_equipment with only hero (missing filename) - should error
            TestRunner.RegisterTest(new TestCase(
                "equipment_save_002",
                "Save equipment with only hero argument should return usage error",
                "gm.item.save_equipment player",
                TestExpectation.Error
            )
            {
                Category = "ItemEquipmentSave",
                ExpectedText = "Missing arguments"
            });

            // Test save_equipment with invalid hero - should error
            TestRunner.RegisterTest(new TestCase(
                "equipment_save_003",
                "Save equipment with invalid hero should return error",
                "gm.item.save_equipment invalid_hero_xyz my_loadout",
                TestExpectation.Error
            )
            {
                Category = "ItemEquipmentSave",
                ExpectedText = "No hero matching"
            });

            // ===== SAVE_EQUIPMENT_CIVILIAN TESTS =====
            
            // Test save_equipment_civilian without arguments - should error
            TestRunner.RegisterTest(new TestCase(
                "equipment_save_004",
                "Save civilian equipment without arguments should return usage error",
                "gm.item.save_equipment_civilian",
                TestExpectation.Error
            )
            {
                Category = "ItemEquipmentSave",
                ExpectedText = "Missing arguments"
            });

            // Test save_equipment_civilian with only hero (missing filename) - should error
            TestRunner.RegisterTest(new TestCase(
                "equipment_save_005",
                "Save civilian equipment with only hero argument should return usage error",
                "gm.item.save_equipment_civilian player",
                TestExpectation.Error
            )
            {
                Category = "ItemEquipmentSave",
                ExpectedText = "Missing arguments"
            });

            // Test save_equipment_civilian with invalid hero - should error
            TestRunner.RegisterTest(new TestCase(
                "equipment_save_006",
                "Save civilian equipment with invalid hero should return error",
                "gm.item.save_equipment_civilian invalid_hero_xyz my_civilian",
                TestExpectation.Error
            )
            {
                Category = "ItemEquipmentSave",
                ExpectedText = "No hero matching"
            });

            // ===== SAVE_EQUIPMENT_BOTH TESTS =====
            
            // Test save_equipment_both without arguments - should error
            TestRunner.RegisterTest(new TestCase(
                "equipment_save_007",
                "Save both equipment sets without arguments should return usage error",
                "gm.item.save_equipment_both",
                TestExpectation.Error
            )
            {
                Category = "ItemEquipmentSave",
                ExpectedText = "Missing arguments"
            });

            // Test save_equipment_both with only hero (missing filename) - should error
            TestRunner.RegisterTest(new TestCase(
                "equipment_save_008",
                "Save both equipment sets with only hero argument should return usage error",
                "gm.item.save_equipment_both player",
                TestExpectation.Error
            )
            {
                Category = "ItemEquipmentSave",
                ExpectedText = "Missing arguments"
            });

            // Test save_equipment_both with invalid hero - should error
            TestRunner.RegisterTest(new TestCase(
                "equipment_save_009",
                "Save both equipment sets with invalid hero should return error",
                "gm.item.save_equipment_both invalid_hero_xyz my_complete",
                TestExpectation.Error
            )
            {
                Category = "ItemEquipmentSave",
                ExpectedText = "No hero matching"
            });

            // Note: Success path tests for save commands are difficult to validate without campaign mode
            // Expected success messages would be:
            // - save_equipment: "Saved {hero}'s battle equipment to: {filename}"
            // - save_equipment_civilian: "Saved {hero}'s civilian equipment to: {filename}"
            // - save_equipment_both: "Saved {hero}'s equipment sets:"
        }

        /// <summary>
        /// Register equipment load command tests
        /// Tests for: load_equipment, load_equipment_civilian, load_equipment_both
        /// </summary>
        private static void RegisterItemEquipmentLoadTests()
        {
            // ===== LOAD_EQUIPMENT TESTS =====
            
            // Test load_equipment without arguments - should error
            TestRunner.RegisterTest(new TestCase(
                "equipment_load_001",
                "Load equipment without arguments should return usage error",
                "gm.item.load_equipment",
                TestExpectation.Error
            )
            {
                Category = "ItemEquipmentLoad",
                ExpectedText = "Missing arguments"
            });

            // Test load_equipment with only hero (missing filename) - should error
            TestRunner.RegisterTest(new TestCase(
                "equipment_load_002",
                "Load equipment with only hero argument should return usage error",
                "gm.item.load_equipment player",
                TestExpectation.Error
            )
            {
                Category = "ItemEquipmentLoad",
                ExpectedText = "Missing arguments"
            });

            // Test load_equipment with invalid hero - should error
            TestRunner.RegisterTest(new TestCase(
                "equipment_load_003",
                "Load equipment with invalid hero should return error",
                "gm.item.load_equipment invalid_hero_xyz my_loadout",
                TestExpectation.Error
            )
            {
                Category = "ItemEquipmentLoad",
                ExpectedText = "No hero matching"
            });

            // Test load_equipment with non-existent file - should error
            TestRunner.RegisterTest(new TestCase(
                "equipment_load_004",
                "Load equipment with non-existent file should return error",
                "gm.item.load_equipment player nonexistent_file_xyz_12345",
                TestExpectation.Error
            )
            {
                Category = "ItemEquipmentLoad",
                ExpectedText = "not found"
            });

            // ===== LOAD_EQUIPMENT_CIVILIAN TESTS =====
            
            // Test load_equipment_civilian without arguments - should error
            TestRunner.RegisterTest(new TestCase(
                "equipment_load_005",
                "Load civilian equipment without arguments should return usage error",
                "gm.item.load_equipment_civilian",
                TestExpectation.Error
            )
            {
                Category = "ItemEquipmentLoad",
                ExpectedText = "Missing arguments"
            });

            // Test load_equipment_civilian with only hero (missing filename) - should error
            TestRunner.RegisterTest(new TestCase(
                "equipment_load_006",
                "Load civilian equipment with only hero argument should return usage error",
                "gm.item.load_equipment_civilian player",
                TestExpectation.Error
            )
            {
                Category = "ItemEquipmentLoad",
                ExpectedText = "Missing arguments"
            });

            // Test load_equipment_civilian with invalid hero - should error
            TestRunner.RegisterTest(new TestCase(
                "equipment_load_007",
                "Load civilian equipment with invalid hero should return error",
                "gm.item.load_equipment_civilian invalid_hero_xyz my_civilian",
                TestExpectation.Error
            )
            {
                Category = "ItemEquipmentLoad",
                ExpectedText = "No hero matching"
            });

            // Test load_equipment_civilian with non-existent file - should error
            TestRunner.RegisterTest(new TestCase(
                "equipment_load_008",
                "Load civilian equipment with non-existent file should return error",
                "gm.item.load_equipment_civilian player nonexistent_civilian_xyz_12345",
                TestExpectation.Error
            )
            {
                Category = "ItemEquipmentLoad",
                ExpectedText = "not found"
            });

            // ===== LOAD_EQUIPMENT_BOTH TESTS =====
            
            // Test load_equipment_both without arguments - should error
            TestRunner.RegisterTest(new TestCase(
                "equipment_load_009",
                "Load both equipment sets without arguments should return usage error",
                "gm.item.load_equipment_both",
                TestExpectation.Error
            )
            {
                Category = "ItemEquipmentLoad",
                ExpectedText = "Missing arguments"
            });

            // Test load_equipment_both with only hero (missing filename) - should error
            TestRunner.RegisterTest(new TestCase(
                "equipment_load_010",
                "Load both equipment sets with only hero argument should return usage error",
                "gm.item.load_equipment_both player",
                TestExpectation.Error
            )
            {
                Category = "ItemEquipmentLoad",
                ExpectedText = "Missing arguments"
            });

            // Test load_equipment_both with invalid hero - should error
            TestRunner.RegisterTest(new TestCase(
                "equipment_load_011",
                "Load both equipment sets with invalid hero should return error",
                "gm.item.load_equipment_both invalid_hero_xyz my_complete",
                TestExpectation.Error
            )
            {
                Category = "ItemEquipmentLoad",
                ExpectedText = "No hero matching"
            });

            // Test load_equipment_both with non-existent files
            // Note: This command handles missing files gracefully and reports which were found/not found
            TestRunner.RegisterTest(new TestCase(
                "equipment_load_012",
                "Load both equipment sets with non-existent files should report appropriately",
                "gm.item.load_equipment_both player nonexistent_both_xyz_12345",
                TestExpectation.Error
            )
            {
                Category = "ItemEquipmentLoad",
                ExpectedText = "not found"
            });

            // Note: Success path tests for load commands are difficult to validate without pre-existing save files
            // Expected success messages would be:
            // - load_equipment: "Loaded {hero}'s battle equipment from: {filename}"
            // - load_equipment_civilian: "Loaded {hero}'s civilian equipment from: {filename}"
            // - load_equipment_both: "Loading equipment sets for {hero}:"
            //   (gracefully handles missing files by reporting which were found/loaded)
        }

        /// <summary>
        /// Register success path tests - validate successful command execution
        /// </summary>
        private static void RegisterSuccessPathTests()
        {
        	// Test: Successfully transfer hero to new clan
        	TestRunner.RegisterTest(new TestCase(
        		"hero_mgmt_success_001",
        		"Successfully transfer hero lord_1_1 to clan_empire_south_1",
        		"gm.hero.set_clan lord_1_1 clan_empire_south_1",
        		TestExpectation.Success
        	)
        	{
        		Category = "SuccessPaths_HeroManagement"
        	});
      
        	// Test: Successfully set hero age
        	TestRunner.RegisterTest(new TestCase(
        		"hero_mgmt_success_002",
        		"Successfully set hero age to 30",
        		"gm.hero.set_age lord_1_1 30",
        		TestExpectation.Success
        	)
        	{
        		Category = "SuccessPaths_HeroManagement"
        	});
      
        	// Test: Successfully set hero gold
        	TestRunner.RegisterTest(new TestCase(
        		"hero_mgmt_success_003",
        		"Successfully set hero gold to 5000",
        		"gm.hero.set_gold lord_1_1 5000",
        		TestExpectation.Success
        	)
        	{
        		Category = "SuccessPaths_HeroManagement"
        	});
      
        	// Test: Successfully add gold to hero
        	TestRunner.RegisterTest(new TestCase(
        		"hero_mgmt_success_004",
        		"Successfully add 1000 gold to hero",
        		"gm.hero.add_gold lord_1_1 1000",
        		TestExpectation.Success
        	)
        	{
        		Category = "SuccessPaths_HeroManagement"
        	});
      
        	// Test: Successfully heal hero
        	TestRunner.RegisterTest(new TestCase(
        		"hero_mgmt_success_005",
        		"Successfully heal hero to full health",
        		"gm.hero.heal lord_1_1",
        		TestExpectation.Success
        	)
        	{
        		Category = "SuccessPaths_HeroManagement"
        	});
      
        	// Test: Successfully set hero relation
        	TestRunner.RegisterTest(new TestCase(
        		"hero_mgmt_success_006",
        		"Successfully set relation between two heroes to 50",
        		"gm.hero.set_relation lord_1_1 lord_2_1 50",
        		TestExpectation.Success
        	)
        	{
        		Category = "SuccessPaths_HeroManagement"
        	});
      
        	// Test: Successfully add clan to kingdom
        	TestRunner.RegisterTest(new TestCase(
        		"kingdom_mgmt_success_001",
        		"Successfully add clan to kingdom",
        		"gm.kingdom.add_clan clan_sturgia_2 vlandia",
        		TestExpectation.Success
        	)
        	{
        		Category = "SuccessPaths_KingdomManagement",
        		CleanupCommands = new System.Collections.Generic.List<string>
        		{
        			"gm.kingdom.remove_clan clan_sturgia_2"
        		}
        	});
      
        	// Test: Successfully remove clan from kingdom
        	TestRunner.RegisterTest(new TestCase(
        		"kingdom_mgmt_success_002",
        		"Successfully remove clan from kingdom",
        		"gm.kingdom.remove_clan clan_vlandia_2",
        		TestExpectation.Success
        	)
        	{
        		Category = "SuccessPaths_KingdomManagement",
        		CleanupCommands = new System.Collections.Generic.List<string>
        		{
        			"gm.kingdom.add_clan clan_vlandia_2 vlandia"
        		}
        	});
      
        	// Test: Successfully set kingdom ruler
        	TestRunner.RegisterTest(new TestCase(
        		"kingdom_mgmt_success_003",
        		"Successfully set kingdom ruler",
        		"gm.kingdom.set_ruler vlandia lord_4_2",
        		TestExpectation.Success
        	)
        	{
        		Category = "SuccessPaths_KingdomManagement",
        		CleanupCommands = new System.Collections.Generic.List<string>
        		{
        			"gm.kingdom.set_ruler vlandia lord_4_1"
        		}
        	});
      
        	// Test: Successfully set clan gold
        	TestRunner.RegisterTest(new TestCase(
        		"clan_mgmt_success_001",
        		"Successfully set clan gold to 10000",
        		"gm.clan.set_gold clan_empire_south_1 10000",
        		TestExpectation.Success
        	)
        	{
        		Category = "SuccessPaths_ClanManagement"
        	});
      
        	// Test: Successfully add gold to clan
        	TestRunner.RegisterTest(new TestCase(
        		"clan_mgmt_success_002",
        		"Successfully add 5000 gold to clan",
        		"gm.clan.add_gold clan_empire_south_1 5000",
        		TestExpectation.Success
        	)
        	{
        		Category = "SuccessPaths_ClanManagement"
        	});
      
        	// Test: Successfully set clan renown
        	TestRunner.RegisterTest(new TestCase(
        		"clan_mgmt_success_003",
        		"Successfully set clan renown to 500",
        		"gm.clan.set_renown clan_empire_south_1 500",
        		TestExpectation.Success
        	)
        	{
        		Category = "SuccessPaths_ClanManagement"
        	});
      
        	// Test: Successfully increase clan tier
        	TestRunner.RegisterTest(new TestCase(
        		"clan_mgmt_success_004",
        		"Successfully increase clan tier to 5",
        		"gm.clan.set_tier clan_vlandia_2 5",
        		TestExpectation.Success
        	)
        	{
        		Category = "SuccessPaths_ClanManagement"
        	});
      
        	// Test: Successfully query heroes with results
        	TestRunner.RegisterTest(new TestCase(
        		"query_success_001",
        		"Successfully query for living lords",
        		"gm.query.hero lord alive",
        		TestExpectation.Contains
        	)
        	{
        		Category = "SuccessPaths_Query",
        		ExpectedText = "hero(es) matching"
        	});
      
        	// Test: Successfully query clans with results
        	TestRunner.RegisterTest(new TestCase(
        		"query_success_002",
        		"Successfully query for empire clans",
        		"gm.query.clan empire",
        		TestExpectation.Contains
        	)
        	{
        		Category = "SuccessPaths_Query",
        		ExpectedText = "clan(s) matching"
        	});
      
        	// Test: Successfully query kingdoms with results
        	TestRunner.RegisterTest(new TestCase(
        		"query_success_003",
        		"Successfully query for active kingdoms",
        		"gm.query.kingdom active",
        		TestExpectation.Contains
        	)
        	{
        		Category = "SuccessPaths_Query",
        		ExpectedText = "kingdom(s) matching"
        	});

        	// Test: Successfully query items with results
        	TestRunner.RegisterTest(new TestCase(
        		"query_success_004",
        		"Successfully query for weapons",
        		"gm.query.item weapon",
        		TestExpectation.Contains
        	)
        	{
        		Category = "SuccessPaths_Query",
        		ExpectedText = "item(s) matching"
        	});
        }

        /// <summary>
        /// Register sorting tests for hero, clan, and kingdom queries
        /// </summary>
        private static void RegisterSortingTests()
        {
        	   // Hero sorting tests
        	   TestRunner.RegisterTest(new TestCase(
        	       "hero_sort_001",
        	       "Query heroes sorted by name",
        	       "gm.query.hero sort:name",
        	       TestExpectation.Contains
        	   )
        	   {
        	       Category = "Sorting",
        	       ExpectedText = "hero(es) matching"
        	   });

        	   TestRunner.RegisterTest(new TestCase(
        	       "hero_sort_002",
        	       "Query heroes sorted by age descending",
        	       "gm.query.hero sort:age:desc",
        	       TestExpectation.Contains
        	   )
        	   {
        	       Category = "Sorting",
        	       ExpectedText = "hero(es) matching"
        	   });

        	   TestRunner.RegisterTest(new TestCase(
        	       "hero_sort_003",
        	       "Query lords sorted by clan",
        	       "gm.query.hero lord sort:clan",
        	       TestExpectation.Contains
        	   )
        	   {
        	       Category = "Sorting",
        	       ExpectedText = "hero(es) matching"
        	   });

        	   TestRunner.RegisterTest(new TestCase(
        	       "hero_sort_004",
        	       "Query heroes sorted by wanderer type flag",
        	       "gm.query.hero sort:wanderer",
        	       TestExpectation.Contains
        	   )
        	   {
        	       Category = "Sorting",
        	       ExpectedText = "hero(es) matching"
        	   });

        	   // Clan sorting tests
        	   TestRunner.RegisterTest(new TestCase(
        	       "clan_sort_001",
        	       "Query clans sorted by name",
        	       "gm.query.clan sort:name",
        	       TestExpectation.Contains
        	   )
        	   {
        	       Category = "Sorting",
        	       ExpectedText = "clan(s) matching"
        	   });

        	   TestRunner.RegisterTest(new TestCase(
        	       "clan_sort_002",
        	       "Query clans sorted by gold descending",
        	       "gm.query.clan sort:gold:desc",
        	       TestExpectation.Contains
        	   )
        	   {
        	       Category = "Sorting",
        	       ExpectedText = "clan(s) matching"
        	   });

        	   TestRunner.RegisterTest(new TestCase(
        	       "clan_sort_003",
        	       "Query noble clans sorted by renown",
        	       "gm.query.clan noble sort:renown:desc",
        	       TestExpectation.Contains
        	   )
        	   {
        	       Category = "Sorting",
        	       ExpectedText = "clan(s) matching"
        	   });

        	   TestRunner.RegisterTest(new TestCase(
        	       "clan_sort_004",
        	       "Query clans sorted by mercenary type flag",
        	       "gm.query.clan sort:mercenary",
        	       TestExpectation.Contains
        	   )
        	   {
        	       Category = "Sorting",
        	       ExpectedText = "clan(s) matching"
        	   });

        	   // Kingdom sorting tests
        	   TestRunner.RegisterTest(new TestCase(
        	       "kingdom_sort_001",
        	       "Query kingdoms sorted by name",
        	       "gm.query.kingdom sort:name",
        	       TestExpectation.Contains
        	   )
        	   {
        	       Category = "Sorting",
        	       ExpectedText = "kingdom(s) matching"
        	   });

        	   TestRunner.RegisterTest(new TestCase(
        	       "kingdom_sort_002",
        	       "Query kingdoms sorted by strength descending",
        	       "gm.query.kingdom sort:strength:desc",
        	       TestExpectation.Contains
        	   )
        	   {
        	       Category = "Sorting",
        	       ExpectedText = "kingdom(s) matching"
        	   });

        	   TestRunner.RegisterTest(new TestCase(
        	       "kingdom_sort_003",
        	       "Query active kingdoms sorted by clans",
        	       "gm.query.kingdom active sort:clans:desc",
        	       TestExpectation.Contains
        	   )
        	   {
        	       Category = "Sorting",
        	       ExpectedText = "kingdom(s) matching"
        	   });

        	   TestRunner.RegisterTest(new TestCase(
        	       "kingdom_sort_004",
        	       "Query kingdoms sorted by atwar type flag",
        	       "gm.query.kingdom sort:atwar",
        	       TestExpectation.Contains
        	   )
        	   {
        	       Category = "Sorting",
        	       ExpectedText = "kingdom(s) matching"
        	   });

        	   // Combined filter and sort tests
        	   TestRunner.RegisterTest(new TestCase(
        	       "combined_sort_001",
        	       "Query female lords sorted by age",
        	       "gm.query.hero female lord sort:age",
        	       TestExpectation.Contains
        	   )
        	   {
        	       Category = "Sorting",
        	       ExpectedText = "hero(es) matching"
        	   });

        	   TestRunner.RegisterTest(new TestCase(
        	       "combined_sort_002",
        	       "Query empire clans sorted by tier descending",
        	       "gm.query.clan empire sort:tier:desc",
        	       TestExpectation.Contains
        	   )
        	   {
        	       Category = "Sorting",
        	       ExpectedText = "clan(s) matching"
        	   });

        	   TestRunner.RegisterTest(new TestCase(
        	       "combined_sort_003",
        	       "Query kingdoms at war sorted by fiefs",
        	       "gm.query.kingdom atwar sort:fiefs:desc",
        	       TestExpectation.Contains
        	   )
        	   {
        	       Category = "Sorting",
        	       ExpectedText = "kingdom(s) matching"
        	   });
        }
       }
      }