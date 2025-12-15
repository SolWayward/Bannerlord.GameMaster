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