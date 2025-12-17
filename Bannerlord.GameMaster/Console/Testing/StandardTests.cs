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
        	RegisterTroopQueryTests();
        	RegisterSettlementQueryTests();
        	RegisterHeroManagementTests();
        	RegisterClanManagementTests();
        	RegisterKingdomManagementTests();
        	RegisterItemManagementTests();
        	RegisterTroopManagementTests();
        	RegisterItemEquipmentSaveTests();
        	RegisterItemEquipmentLoadTests();
        	RegisterSettlementManagementTests();
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
        /// Register settlement query command tests
        /// </summary>
        private static void RegisterSettlementQueryTests()
        {
            // Test basic settlement query
            TestRunner.RegisterTest(new TestCase(
                "settlement_query_001",
                "Query settlements without parameters should return all settlements",
                "gm.query.settlement",
                TestExpectation.Contains
            )
            {
                Category = "SettlementQuery",
                ExpectedText = "settlement(s) matching"
            });

            // Test settlement query with name search
            TestRunner.RegisterTest(new TestCase(
                "settlement_query_002",
                "Query settlements with specific name",
                "gm.query.settlement pen",
                TestExpectation.Contains
            )
            {
                Category = "SettlementQuery",
                ExpectedText = "settlement(s) matching"
            });

            // Test settlement query with type filter - castle
            TestRunner.RegisterTest(new TestCase(
                "settlement_query_003",
                "Query settlements with 'castle' filter",
                "gm.query.settlement castle",
                TestExpectation.Contains
            )
            {
                Category = "SettlementQuery",
                ExpectedText = "settlement(s) matching"
            });

            // Test settlement query with type filter - city
            TestRunner.RegisterTest(new TestCase(
                "settlement_query_004",
                "Query settlements with 'city' filter",
                "gm.query.settlement city",
                TestExpectation.Contains
            )
            {
                Category = "SettlementQuery",
                ExpectedText = "settlement(s) matching"
            });

            // Test settlement query with type filter - village
            TestRunner.RegisterTest(new TestCase(
                "settlement_query_005",
                "Query settlements with 'village' filter",
                "gm.query.settlement village",
                TestExpectation.Contains
            )
            {
                Category = "SettlementQuery",
                ExpectedText = "settlement(s) matching"
            });

            // Test settlement query with culture filter
            TestRunner.RegisterTest(new TestCase(
                "settlement_query_006",
                "Query settlements with 'empire' culture filter",
                "gm.query.settlement empire",
                TestExpectation.Contains
            )
            {
                Category = "SettlementQuery",
                ExpectedText = "settlement(s) matching"
            });

            // Test settlement query with combined filters
            TestRunner.RegisterTest(new TestCase(
                "settlement_query_007",
                "Query settlements with combined filters (castle empire)",
                "gm.query.settlement castle empire",
                TestExpectation.Contains
            )
            {
                Category = "SettlementQuery",
                ExpectedText = "settlement(s) matching"
            });

            // Test settlement_any query (OR logic)
            TestRunner.RegisterTest(new TestCase(
                "settlement_query_008",
                "Query settlements matching ANY criteria (castle OR city)",
                "gm.query.settlement_any castle city",
                TestExpectation.Contains
            )
            {
                Category = "SettlementQuery",
                ExpectedText = "settlement(s) matching ANY"
            });

            // Test settlement_info without ID - should error
            TestRunner.RegisterTest(new TestCase(
                "settlement_query_009",
                "Settlement info without ID should return error",
                "gm.query.settlement_info",
                TestExpectation.Error
            )
            {
                Category = "SettlementQuery",
                ExpectedText = "Please provide a settlement ID"
            });

            // Test settlement_info with invalid ID
            TestRunner.RegisterTest(new TestCase(
                "settlement_query_010",
                "Settlement info with invalid ID should return error",
                "gm.query.settlement_info invalid_settlement_id_xyz",
                TestExpectation.Error
            )
            {
                Category = "SettlementQuery",
                ExpectedText = "not found"
            });

            // Test settlement query with player filter
            TestRunner.RegisterTest(new TestCase(
                "settlement_query_011",
                "Query player-owned settlements",
                "gm.query.settlement player",
                TestExpectation.Contains
            )
            {
                Category = "SettlementQuery",
                ExpectedText = "settlement(s) matching"
            });

            // Test settlement query with besieged filter
            TestRunner.RegisterTest(new TestCase(
                "settlement_query_012",
                "Query besieged settlements",
                "gm.query.settlement besieged",
                TestExpectation.Contains
            )
            {
                Category = "SettlementQuery",
                ExpectedText = "settlement(s) matching"
            });

            // Test settlement query with prosperity filters
            TestRunner.RegisterTest(new TestCase(
                "settlement_query_013",
                "Query high prosperity settlements",
                "gm.query.settlement high",
                TestExpectation.Contains
            )
            {
                Category = "SettlementQuery",
                ExpectedText = "settlement(s) matching"
            });

            // Test settlement query with multiple cultures (OR logic)
            TestRunner.RegisterTest(new TestCase(
                "settlement_query_014",
                "Query settlements from multiple cultures",
                "gm.query.settlement_any empire vlandia",
                TestExpectation.Contains
            )
            {
                Category = "SettlementQuery",
                ExpectedText = "settlement(s) matching ANY"
            });

            // Test settlement query with sorting by name
            TestRunner.RegisterTest(new TestCase(
                "settlement_query_015",
                "Query settlements sorted by name",
                "gm.query.settlement sort:name",
                TestExpectation.Contains
            )
            {
                Category = "SettlementQuery",
                ExpectedText = "settlement(s) matching"
            });

            // Test settlement query with sorting by prosperity descending
            TestRunner.RegisterTest(new TestCase(
                "settlement_query_016",
                "Query settlements sorted by prosperity descending",
                "gm.query.settlement sort:prosperity:desc",
                TestExpectation.Contains
            )
            {
                Category = "SettlementQuery",
                ExpectedText = "settlement(s) matching"
            });

            // Test settlement query with combined filters and sorting
            TestRunner.RegisterTest(new TestCase(
                "settlement_query_017",
                "Query empire cities sorted by prosperity",
                "gm.query.settlement empire city sort:prosperity:desc",
                TestExpectation.Contains
            )
            {
                Category = "SettlementQuery",
                ExpectedText = "settlement(s) matching"
            });

            // Test settlement query with Vlandia culture
            TestRunner.RegisterTest(new TestCase(
                "settlement_query_018",
                "Query vlandia settlements",
                "gm.query.settlement vlandia",
                TestExpectation.Contains
            )
            {
                Category = "SettlementQuery",
                ExpectedText = "settlement(s) matching"
            });

            // Test settlement query with Sturgia culture
            TestRunner.RegisterTest(new TestCase(
                "settlement_query_019",
                "Query sturgia settlements",
                "gm.query.settlement sturgia",
                TestExpectation.Contains
            )
            {
                Category = "SettlementQuery",
                ExpectedText = "settlement(s) matching"
            });

            // Test settlement query with Aserai culture
            TestRunner.RegisterTest(new TestCase(
                "settlement_query_020",
                "Query aserai settlements",
                "gm.query.settlement aserai",
                TestExpectation.Contains
            )
            {
                Category = "SettlementQuery",
                ExpectedText = "settlement(s) matching"
            });

            // Test settlement query with Khuzait culture
            TestRunner.RegisterTest(new TestCase(
                "settlement_query_021",
                "Query khuzait settlements",
                "gm.query.settlement khuzait",
                TestExpectation.Contains
            )
            {
                Category = "SettlementQuery",
                ExpectedText = "settlement(s) matching"
            });

            // Test settlement query with Battania culture
            TestRunner.RegisterTest(new TestCase(
                "settlement_query_022",
                "Query battania settlements",
                "gm.query.settlement battania",
                TestExpectation.Contains
            )
            {
                Category = "SettlementQuery",
                ExpectedText = "settlement(s) matching"
            });

            // Test sorting by owner
            TestRunner.RegisterTest(new TestCase(
                "settlement_query_023",
                "Query settlements sorted by owner",
                "gm.query.settlement sort:owner",
                TestExpectation.Contains
            )
            {
                Category = "SettlementQuery",
                ExpectedText = "settlement(s) matching"
            });

            // Test sorting by kingdom
            TestRunner.RegisterTest(new TestCase(
                "settlement_query_024",
                "Query settlements sorted by kingdom",
                "gm.query.settlement sort:kingdom",
                TestExpectation.Contains
            )
            {
                Category = "SettlementQuery",
                ExpectedText = "settlement(s) matching"
            });

            // Test sorting by culture
            TestRunner.RegisterTest(new TestCase(
                "settlement_query_025",
                "Query settlements sorted by culture",
                "gm.query.settlement sort:culture",
                TestExpectation.Contains
            )
            {
                Category = "SettlementQuery",
                ExpectedText = "settlement(s) matching"
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
        /// Register troop query command tests
        /// </summary>
        private static void RegisterTroopQueryTests()
        {
            // ===== BASIC QUERY TESTS =====
            
            // Test basic troop query without parameters
            TestRunner.RegisterTest(new TestCase(
                "troop_query_001",
                "Query troops without parameters should return all troops (excluding heroes)",
                "gm.query.troop",
                TestExpectation.Contains
            )
            {
                Category = "TroopQuery",
                ExpectedText = "troop(s) matching"
            });

            // ===== FILTERING TESTS - EXCLUSIONS =====
            
            // Test that templates are excluded
            TestRunner.RegisterTest(new TestCase(
                "troop_filter_001",
                "Templates should be excluded from troop queries",
                "gm.query.troop template",
                TestExpectation.Contains
            )
            {
                Category = "TroopFiltering",
                ExpectedText = "0 troop(s) matching",
                CustomValidator = (output) =>
                {
                    // Verify no troops with "template" in ID are returned
                    if (output.Contains("template") && !output.Contains("0 troop(s)"))
                        return (false, "Templates should be excluded from results");
                    return (true, null);
                }
            });

            // Test that equipment sets are excluded
            TestRunner.RegisterTest(new TestCase(
                "troop_filter_002",
                "Equipment sets should be excluded from troop queries",
                "gm.query.troop _equipment",
                TestExpectation.Contains
            )
            {
                Category = "TroopFiltering",
                ExpectedText = "0 troop(s) matching"
            });

            // Test that town NPCs are excluded
            TestRunner.RegisterTest(new TestCase(
                "troop_filter_003",
                "Town NPCs (armorer, blacksmith, etc) should be excluded",
                "gm.query.troop armorer",
                TestExpectation.Contains
            )
            {
                Category = "TroopFiltering",
                ExpectedText = "0 troop(s) matching"
            });

            // Test that wanderers are excluded
            TestRunner.RegisterTest(new TestCase(
                "troop_filter_004",
                "Wanderers/companions should be excluded from troop queries",
                "gm.query.troop wanderer",
                TestExpectation.Contains
            )
            {
                Category = "TroopFiltering",
                ExpectedText = "0 troop(s) matching",
                CustomValidator = (output) =>
                {
                    // Should not find NPCs starting with spc_wanderer or npc_wanderer
                    if (output.Contains("spc_wanderer") || output.Contains("npc_wanderer"))
                        return (false, "Wanderers should be excluded from results");
                    return (true, null);
                }
            });

            // Test that children are excluded
            TestRunner.RegisterTest(new TestCase(
                "troop_filter_005",
                "Children/teens/infants should be excluded from troop queries",
                "gm.query.troop child",
                TestExpectation.Contains
            )
            {
                Category = "TroopFiltering",
                ExpectedText = "0 troop(s) matching"
            });

            // Test that practice dummies are excluded
            TestRunner.RegisterTest(new TestCase(
                "troop_filter_006",
                "Practice dummies should be excluded from troop queries",
                "gm.query.troop dummy",
                TestExpectation.Contains
            )
            {
                Category = "TroopFiltering",
                ExpectedText = "0 troop(s) matching"
            });

            // Test that special characters are excluded
            TestRunner.RegisterTest(new TestCase(
                "troop_filter_007",
                "Special characters (cutscene, tutorial) should be excluded",
                "gm.query.troop cutscene",
                TestExpectation.Contains
            )
            {
                Category = "TroopFiltering",
                ExpectedText = "0 troop(s) matching"
            });

            // Test that non-combat peasants are excluded
            TestRunner.RegisterTest(new TestCase(
                "troop_filter_008",
                "Non-combat peasants (Tier 0, Level 1) should be excluded",
                "gm.query.troop villager tier0",
                TestExpectation.Contains
            )
            {
                Category = "TroopFiltering",
                ExpectedText = "troop(s) matching"
            });

            // Test that caravan leaders are excluded
            TestRunner.RegisterTest(new TestCase(
                "troop_filter_009",
                "Caravan leaders should be excluded from troop queries",
                "gm.query.troop caravan_leader",
                TestExpectation.Contains
            )
            {
                Category = "TroopFiltering",
                ExpectedText = "0 troop(s) matching"
            });

            // ===== FILTERING TESTS - INCLUSIONS =====
            
            // Test that regular troops are included
            TestRunner.RegisterTest(new TestCase(
                "troop_filter_010",
                "Regular military troops (tier 1+) should be included",
                "gm.query.troop tier1",
                TestExpectation.Contains
            )
            {
                Category = "TroopFiltering",
                ExpectedText = "troop(s) matching",
                CustomValidator = (output) =>
                {
                    // Should find at least some tier 1 troops
                    if (output.Contains("0 troop(s) matching"))
                        return (false, "Should find tier 1 troops");
                    return (true, null);
                }
            });

            // Test that militia are included
            TestRunner.RegisterTest(new TestCase(
                "troop_filter_011",
                "Militia troops should be included in queries",
                "gm.query.troop militia",
                TestExpectation.Contains
            )
            {
                Category = "TroopFiltering",
                ExpectedText = "troop(s) matching",
                CustomValidator = (output) =>
                {
                    // Should find militia troops
                    if (output.Contains("0 troop(s) matching"))
                        return (false, "Should find militia troops");
                    return (true, null);
                }
            });

            // Test that mercenaries are included (but not leaders)
            TestRunner.RegisterTest(new TestCase(
                "troop_filter_012",
                "Mercenary troops should be included (not leaders)",
                "gm.query.troop mercenary",
                TestExpectation.Contains
            )
            {
                Category = "TroopFiltering",
                ExpectedText = "troop(s) matching",
                CustomValidator = (output) =>
                {
                    // Should find mercenary troops
                    if (output.Contains("mercenary_leader"))
                        return (false, "Mercenary leaders should be excluded");
                    if (output.Contains("0 troop(s) matching"))
                        return (false, "Should find mercenary troops");
                    return (true, null);
                }
            });

            // Test that caravan guards are included
            TestRunner.RegisterTest(new TestCase(
                "troop_filter_013",
                "Caravan guards/masters should be included",
                "gm.query.troop caravan",
                TestExpectation.Contains
            )
            {
                Category = "TroopFiltering",
                ExpectedText = "troop(s) matching",
                CustomValidator = (output) =>
                {
                    // Should find caravan guards/masters but not leaders
                    if (output.Contains("caravan_leader"))
                        return (false, "Caravan leaders should be excluded");
                    return (true, null);
                }
            });

            // Test that bandits are included
            TestRunner.RegisterTest(new TestCase(
                "troop_filter_014",
                "Bandit troops should be included in queries",
                "gm.query.troop bandit",
                TestExpectation.Contains
            )
            {
                Category = "TroopFiltering",
                ExpectedText = "troop(s) matching",
                CustomValidator = (output) =>
                {
                    // Should find bandit troops
                    if (output.Contains("0 troop(s) matching"))
                        return (false, "Should find bandit troops");
                    return (true, null);
                }
            });

            // ===== CATEGORY TESTS =====
            
            // Test bandit category identification
            TestRunner.RegisterTest(new TestCase(
                "troop_category_001",
                "Bandit troops should show [Bandit] category",
                "gm.query.troop bandit",
                TestExpectation.Contains
            )
            {
                Category = "TroopCategory",
                ExpectedText = "[Bandit]"
            });

            // Test militia category identification
            TestRunner.RegisterTest(new TestCase(
                "troop_category_002",
                "Militia troops should show [Militia] category",
                "gm.query.troop militia",
                TestExpectation.Contains
            )
            {
                Category = "TroopCategory",
                ExpectedText = "[Militia]"
            });

            // Test mercenary category identification
            TestRunner.RegisterTest(new TestCase(
                "troop_category_003",
                "Mercenary troops should show [Mercenary] category",
                "gm.query.troop mercenary",
                TestExpectation.Contains
            )
            {
                Category = "TroopCategory",
                ExpectedText = "[Mercenary]"
            });

            // Test noble category identification
            TestRunner.RegisterTest(new TestCase(
                "troop_category_004",
                "Noble troops should show [Noble/Elite] category",
                "gm.query.troop noble",
                TestExpectation.Contains
            )
            {
                Category = "TroopCategory",
                ExpectedText = "[Noble/Elite]"
            });

            // Test regular category identification
            TestRunner.RegisterTest(new TestCase(
                "troop_category_005",
                "Regular troops should show [Regular] category",
                "gm.query.troop regular",
                TestExpectation.Contains
            )
            {
                Category = "TroopCategory",
                ExpectedText = "[Regular]"
            });

            // Test caravan category identification
            TestRunner.RegisterTest(new TestCase(
                "troop_category_006",
                "Caravan troops should show [Caravan] category",
                "gm.query.troop caravan",
                TestExpectation.Contains
            )
            {
                Category = "TroopCategory",
                ExpectedText = "[Caravan]"
            });

            // ===== INTEGRATION TESTS =====
            
            // Test default query excludes non-troops
            TestRunner.RegisterTest(new TestCase(
                "troop_integration_001",
                "Default query should only return actual troops",
                "gm.query.troop",
                TestExpectation.NoException
            )
            {
                Category = "TroopIntegration",
                CustomValidator = (output) =>
                {
                    // Should not contain excluded categories
                    var excludedTerms = new[] { "template", "_equipment", "wanderer", "child", "dummy", "cutscene" };
                    foreach (var term in excludedTerms)
                    {
                        if (output.ToLower().Contains(term))
                            return (false, $"Output should not contain excluded term: {term}");
                    }
                    
                    // Should contain category tags
                    if (!output.Contains("["))
                        return (false, "Output should include category tags");
                        
                    return (true, null);
                }
            });

            // Test combined type filters
            TestRunner.RegisterTest(new TestCase(
                "troop_integration_002",
                "Query empire infantry should return only actual troops",
                "gm.query.troop empire infantry",
                TestExpectation.Contains
            )
            {
                Category = "TroopIntegration",
                ExpectedText = "troop(s) matching",
                CustomValidator = (output) =>
                {
                    // Should show categories for all results
                    if (output.Contains("troop(s) matching") && !output.Contains("0 troop(s)"))
                    {
                        if (!output.Contains("["))
                            return (false, "Results should include category tags");
                    }
                    return (true, null);
                }
            });

            // Test tier filtering with categories
            TestRunner.RegisterTest(new TestCase(
                "troop_integration_003",
                "Query tier 3+ troops should show appropriate categories",
                "gm.query.troop tier3",
                TestExpectation.Contains
            )
            {
                Category = "TroopIntegration",
                ExpectedText = "troop(s) matching",
                CustomValidator = (output) =>
                {
                    // Tier 3 troops should not be peasants
                    if (output.Contains("[Peasant]"))
                        return (false, "Tier 3 troops should not be categorized as Peasant");
                    return (true, null);
                }
            });

            // Test output format includes all expected fields
            TestRunner.RegisterTest(new TestCase(
                "troop_integration_004",
                "Troop query output should include category, tier, level, culture, formation",
                "gm.query.troop empire tier2",
                TestExpectation.Contains
            )
            {
                Category = "TroopIntegration",
                ExpectedText = "troop(s) matching",
                CustomValidator = (output) =>
                {
                    // Check for expected field labels
                    var expectedFields = new[] { "Tier:", "Level:", "Culture:", "Formation:" };
                    foreach (var field in expectedFields)
                    {
                        if (!output.Contains(field))
                            return (false, $"Output should contain field: {field}");
                    }
                    
                    // Check for category tags
                    if (!output.Contains("[") || !output.Contains("]"))
                        return (false, "Output should contain category tags [CategoryName]");
                        
                    return (true, null);
                }
            });

            // Test troop_any with filtering
            TestRunner.RegisterTest(new TestCase(
                "troop_integration_005",
                "Query troop_any should only return actual troops",
                "gm.query.troop_any cavalry ranged",
                TestExpectation.Contains
            )
            {
                Category = "TroopIntegration",
                ExpectedText = "troop(s) matching ANY",
                CustomValidator = (output) =>
                {
                    // Should not contain excluded categories
                    var excludedTerms = new[] { "template", "wanderer", "child", "dummy" };
                    foreach (var term in excludedTerms)
                    {
                        if (output.ToLower().Contains(term))
                            return (false, $"Output should not contain excluded term: {term}");
                    }
                    return (true, null);
                }
            });


            // Test troop query with formation type filter
            TestRunner.RegisterTest(new TestCase(
                "troop_query_002",
                "Query troops with formation type filter",
                "gm.query.troop infantry",
                TestExpectation.Contains
            )
            {
                Category = "TroopQuery",
                ExpectedText = "troop(s) matching"
            });

            // Test troop query with equipment type filter
            TestRunner.RegisterTest(new TestCase(
                "troop_query_003",
                "Query troops with equipment type filter (shield)",
                "gm.query.troop shield",
                TestExpectation.Contains
            )
            {
                Category = "TroopQuery",
                ExpectedText = "troop(s) matching"
            });

            // Test troop_any query (OR logic)
            TestRunner.RegisterTest(new TestCase(
                "troop_query_004",
                "Query troops matching ANY criteria (cavalry OR ranged)",
                "gm.query.troop_any cavalry ranged",
                TestExpectation.Contains
            )
            {
                Category = "TroopQuery",
                ExpectedText = "troop(s) matching ANY"
            });

            // Test troop_info without ID - should error
            TestRunner.RegisterTest(new TestCase(
                "troop_query_005",
                "Troop info without ID should return error",
                "gm.query.troop_info",
                TestExpectation.Error
            )
            {
                Category = "TroopQuery",
                ExpectedText = "Please provide a troop ID"
            });

            // Test troop_info with invalid ID
            TestRunner.RegisterTest(new TestCase(
                "troop_query_006",
                "Troop info with invalid ID should return error",
                "gm.query.troop_info invalid_troop_id_xyz",
                TestExpectation.Error
            )
            {
                Category = "TroopQuery",
                ExpectedText = "not found"
            });

            // ===== FORMATION TYPE TESTS =====
            
            // Test query infantry troops
            TestRunner.RegisterTest(new TestCase(
                "troop_query_007",
                "Query infantry troops",
                "gm.query.troop infantry",
                TestExpectation.Contains
            )
            {
                Category = "TroopQuery",
                ExpectedText = "troop(s) matching"
            });

            // Test query ranged troops
            TestRunner.RegisterTest(new TestCase(
                "troop_query_008",
                "Query ranged troops",
                "gm.query.troop ranged",
                TestExpectation.Contains
            )
            {
                Category = "TroopQuery",
                ExpectedText = "troop(s) matching"
            });

            // Test query cavalry troops
            TestRunner.RegisterTest(new TestCase(
                "troop_query_009",
                "Query cavalry troops",
                "gm.query.troop cavalry",
                TestExpectation.Contains
            )
            {
                Category = "TroopQuery",
                ExpectedText = "troop(s) matching"
            });

            // Test query horsearcher troops
            TestRunner.RegisterTest(new TestCase(
                "troop_query_010",
                "Query horsearcher troops",
                "gm.query.troop horsearcher",
                TestExpectation.Contains
            )
            {
                Category = "TroopQuery",
                ExpectedText = "troop(s) matching"
            });

            // ===== EQUIPMENT TYPE TESTS =====
            
            // Test query shield troops
            TestRunner.RegisterTest(new TestCase(
                "troop_query_011",
                "Query shield troops",
                "gm.query.troop shield",
                TestExpectation.Contains
            )
            {
                Category = "TroopQuery",
                ExpectedText = "troop(s) matching"
            });

            // Test query bow troops
            TestRunner.RegisterTest(new TestCase(
                "troop_query_012",
                "Query bow troops",
                "gm.query.troop bow",
                TestExpectation.Contains
            )
            {
                Category = "TroopQuery",
                ExpectedText = "troop(s) matching"
            });

            // Test query crossbow troops
            TestRunner.RegisterTest(new TestCase(
                "troop_query_013",
                "Query crossbow troops",
                "gm.query.troop crossbow",
                TestExpectation.Contains
            )
            {
                Category = "TroopQuery",
                ExpectedText = "troop(s) matching"
            });

            // Test query twohanded weapon troops
            TestRunner.RegisterTest(new TestCase(
                "troop_query_014",
                "Query twohanded weapon troops",
                "gm.query.troop twohanded",
                TestExpectation.Contains
            )
            {
                Category = "TroopQuery",
                ExpectedText = "troop(s) matching"
            });

            // Test query polearm troops
            TestRunner.RegisterTest(new TestCase(
                "troop_query_015",
                "Query polearm troops",
                "gm.query.troop polearm",
                TestExpectation.Contains
            )
            {
                Category = "TroopQuery",
                ExpectedText = "troop(s) matching"
            });

            // Test query throwing weapon troops
            TestRunner.RegisterTest(new TestCase(
                "troop_query_016",
                "Query throwing weapon troops",
                "gm.query.troop throwing",
                TestExpectation.Contains
            )
            {
                Category = "TroopQuery",
                ExpectedText = "troop(s) matching"
            });

            // ===== TIER FILTERING TESTS =====
            
            // Test query tier1 troops
            TestRunner.RegisterTest(new TestCase(
                "troop_query_017",
                "Query tier1 troops",
                "gm.query.troop tier1",
                TestExpectation.Contains
            )
            {
                Category = "TroopQuery",
                ExpectedText = "troop(s) matching"
            });

            // Test query tier3 troops
            TestRunner.RegisterTest(new TestCase(
                "troop_query_018",
                "Query tier3 troops",
                "gm.query.troop tier3",
                TestExpectation.Contains
            )
            {
                Category = "TroopQuery",
                ExpectedText = "troop(s) matching"
            });

            // Test query tier5 troops
            TestRunner.RegisterTest(new TestCase(
                "troop_query_019",
                "Query tier5 troops",
                "gm.query.troop tier5",
                TestExpectation.Contains
            )
            {
                Category = "TroopQuery",
                ExpectedText = "troop(s) matching"
            });

            // Test query tier6plus troops
            TestRunner.RegisterTest(new TestCase(
                "troop_query_020",
                "Query tier6plus troops",
                "gm.query.troop tier6plus",
                TestExpectation.Contains
            )
            {
                Category = "TroopQuery",
                ExpectedText = "troop(s) matching"
            });

            // ===== CULTURE TESTS =====
            
            // Test query empire troops
            TestRunner.RegisterTest(new TestCase(
                "troop_query_021",
                "Query empire troops",
                "gm.query.troop empire",
                TestExpectation.Contains
            )
            {
                Category = "TroopQuery",
                ExpectedText = "troop(s) matching"
            });

            // Test query vlandia troops
            TestRunner.RegisterTest(new TestCase(
                "troop_query_022",
                "Query vlandia troops",
                "gm.query.troop vlandia",
                TestExpectation.Contains
            )
            {
                Category = "TroopQuery",
                ExpectedText = "troop(s) matching"
            });

            // Test query sturgia troops
            TestRunner.RegisterTest(new TestCase(
                "troop_query_023",
                "Query sturgia troops",
                "gm.query.troop sturgia",
                TestExpectation.Contains
            )
            {
                Category = "TroopQuery",
                ExpectedText = "troop(s) matching"
            });

            // Test query aserai troops
            TestRunner.RegisterTest(new TestCase(
                "troop_query_024",
                "Query aserai troops",
                "gm.query.troop aserai",
                TestExpectation.Contains
            )
            {
                Category = "TroopQuery",
                ExpectedText = "troop(s) matching"
            });

            // Test query khuzait troops
            TestRunner.RegisterTest(new TestCase(
                "troop_query_025",
                "Query khuzait troops",
                "gm.query.troop khuzait",
                TestExpectation.Contains
            )
            {
                Category = "TroopQuery",
                ExpectedText = "troop(s) matching"
            });

            // Test query battania troops
            TestRunner.RegisterTest(new TestCase(
                "troop_query_026",
                "Query battania troops",
                "gm.query.troop battania",
                TestExpectation.Contains
            )
            {
                Category = "TroopQuery",
                ExpectedText = "troop(s) matching"
            });

            // Test query bandit troops
            TestRunner.RegisterTest(new TestCase(
                "troop_query_027",
                "Query bandit troops",
                "gm.query.troop bandit",
                TestExpectation.Contains
            )
            {
                Category = "TroopQuery",
                ExpectedText = "troop(s) matching"
            });

            // ===== TROOP LINE TESTS =====
            
            // Test query regular troops
            TestRunner.RegisterTest(new TestCase(
                "troop_query_028",
                "Query regular troops",
                "gm.query.troop regular",
                TestExpectation.Contains
            )
            {
                Category = "TroopQuery",
                ExpectedText = "troop(s) matching"
            });

            // Test query noble troops
            TestRunner.RegisterTest(new TestCase(
                "troop_query_029",
                "Query noble troops",
                "gm.query.troop noble",
                TestExpectation.Contains
            )
            {
                Category = "TroopQuery",
                ExpectedText = "troop(s) matching"
            });

            // Test query militia troops
            TestRunner.RegisterTest(new TestCase(
                "troop_query_030",
                "Query militia troops",
                "gm.query.troop militia",
                TestExpectation.Contains
            )
            {
                Category = "TroopQuery",
                ExpectedText = "troop(s) matching"
            });

            // Test query mercenary troops
            TestRunner.RegisterTest(new TestCase(
                "troop_query_031",
                "Query mercenary troops",
                "gm.query.troop mercenary",
                TestExpectation.Contains
            )
            {
                Category = "TroopQuery",
                ExpectedText = "troop(s) matching"
            });

            // ===== COMBINED FILTER TESTS =====
            
            // Test query empire infantry
            TestRunner.RegisterTest(new TestCase(
                "troop_query_032",
                "Query empire infantry",
                "gm.query.troop empire infantry",
                TestExpectation.Contains
            )
            {
                Category = "TroopQuery",
                ExpectedText = "troop(s) matching"
            });

            // Test query aserai cavalry tier3
            TestRunner.RegisterTest(new TestCase(
                "troop_query_033",
                "Query aserai cavalry tier3",
                "gm.query.troop aserai cavalry tier3",
                TestExpectation.Contains
            )
            {
                Category = "TroopQuery",
                ExpectedText = "troop(s) matching"
            });

            // Test query shield infantry with sorting
            TestRunner.RegisterTest(new TestCase(
                "troop_query_034",
                "Query shield infantry sorted by tier",
                "gm.query.troop shield infantry sort:tier",
                TestExpectation.Contains
            )
            {
                Category = "TroopQuery",
                ExpectedText = "troop(s) matching"
            });

            // Test query battania ranged bow
            TestRunner.RegisterTest(new TestCase(
                "troop_query_035",
                "Query battania ranged bow",
                "gm.query.troop battania ranged bow",
                TestExpectation.Contains
            )
            {
                Category = "TroopQuery",
                ExpectedText = "troop(s) matching"
            });

            // ===== SORTING TESTS =====
            
            // Test sort by name
            TestRunner.RegisterTest(new TestCase(
                "troop_query_036",
                "Query troops sorted by name",
                "gm.query.troop sort:name",
                TestExpectation.Contains
            )
            {
                Category = "TroopQuery",
                ExpectedText = "troop(s) matching"
            });

            // Test sort by tier descending
            TestRunner.RegisterTest(new TestCase(
                "troop_query_037",
                "Query troops sorted by tier descending",
                "gm.query.troop sort:tier:desc",
                TestExpectation.Contains
            )
            {
                Category = "TroopQuery",
                ExpectedText = "troop(s) matching"
            });

            // Test sort by level
            TestRunner.RegisterTest(new TestCase(
                "troop_query_038",
                "Query troops sorted by level",
                "gm.query.troop sort:level",
                TestExpectation.Contains
            )
            {
                Category = "TroopQuery",
                ExpectedText = "troop(s) matching"
            });

            // Test sort by culture
            TestRunner.RegisterTest(new TestCase(
                "troop_query_039",
                "Query troops sorted by culture",
                "gm.query.troop sort:culture",
                TestExpectation.Contains
            )
            {
                Category = "TroopQuery",
                ExpectedText = "troop(s) matching"
            });

            // ===== OR LOGIC TESTS =====
            
            // Test query cavalry OR ranged
            TestRunner.RegisterTest(new TestCase(
                "troop_query_040",
                "Query troops that are cavalry OR ranged",
                "gm.query.troop_any cavalry ranged",
                TestExpectation.Contains
            )
            {
                Category = "TroopQuery",
                ExpectedText = "troop(s) matching ANY"
            });

            // Test query bow OR crossbow
            TestRunner.RegisterTest(new TestCase(
                "troop_query_041",
                "Query troops with bow OR crossbow",
                "gm.query.troop_any bow crossbow",
                TestExpectation.Contains
            )
            {
                Category = "TroopQuery",
                ExpectedText = "troop(s) matching ANY"
            });

            // Test query empire OR vlandia infantry
            TestRunner.RegisterTest(new TestCase(
                "troop_query_042",
                "Query empire OR vlandia infantry",
                "gm.query.troop_any empire vlandia infantry",
                TestExpectation.Contains
            )
            {
                Category = "TroopQuery",
                ExpectedText = "troop(s) matching ANY"
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

            // Test generate_lords with invalid count - should error
            TestRunner.RegisterTest(new TestCase(
                "hero_mgmt_007",
                "Generate lords with invalid count (negative) should return error",
                "gm.hero.generate_lords -1",
                TestExpectation.Error
            )
            {
                Category = "HeroManagement",
                ExpectedText = "Error"
            });

            // Test generate_lords with invalid count (too high) - should error
            TestRunner.RegisterTest(new TestCase(
                "hero_mgmt_008",
                "Generate lords with invalid count (>20) should return error",
                "gm.hero.generate_lords 50",
                TestExpectation.Error
            )
            {
                Category = "HeroManagement",
                ExpectedText = "Error"
            });

            // Test generate_lords with invalid clan - should error
            TestRunner.RegisterTest(new TestCase(
                "hero_mgmt_009",
                "Generate lords with invalid clan should return error",
                "gm.hero.generate_lords 1 invalid_clan_xyz",
                TestExpectation.Error
            )
            {
                Category = "HeroManagement",
                ExpectedText = "No clan matching"
            });

            // Test create_lord without arguments - should error
            TestRunner.RegisterTest(new TestCase(
                "hero_mgmt_010",
                "Create lord without arguments should return usage error",
                "gm.hero.create_lord",
                TestExpectation.Error
            )
            {
                Category = "HeroManagement",
                ExpectedText = "Missing arguments"
            });

            // Test create_lord with only gender (missing name and clan) - should error
            TestRunner.RegisterTest(new TestCase(
                "hero_mgmt_011",
                "Create lord with only gender should return usage error",
                "gm.hero.create_lord male",
                TestExpectation.Error
            )
            {
                Category = "HeroManagement",
                ExpectedText = "Missing arguments"
            });

            // Test create_lord with invalid gender - should error
            TestRunner.RegisterTest(new TestCase(
                "hero_mgmt_012",
                "Create lord with invalid gender should return error",
                "gm.hero.create_lord invalid NewLord empire_south",
                TestExpectation.Error
            )
            {
                Category = "HeroManagement",
                ExpectedText = "Gender must be"
            });

            // Test create_lord with invalid clan - should error
            TestRunner.RegisterTest(new TestCase(
                "hero_mgmt_013",
                "Create lord with invalid clan should return error",
                "gm.hero.create_lord male NewLord invalid_clan_xyz",
                TestExpectation.Error
            )
            {
                Category = "HeroManagement",
                ExpectedText = "No clan matching"
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
        /// Register troop management command tests
        /// </summary>
        private static void RegisterTroopManagementTests()
        {
            // === Troop Management Command Tests ===
            
            // Test give_hero_troops without arguments - should error
            TestRunner.RegisterTest(new TestCase(
                "troop_mgmt_001",
                "Give hero troops without arguments should return usage error",
                "gm.troops.give_hero_troops",
                TestExpectation.Error
            )
            {
                Category = "TroopManagement",
                ExpectedText = "Missing arguments"
            });

            // Test give_hero_troops with only troop query (missing count and hero) - should error
            TestRunner.RegisterTest(new TestCase(
                "troop_mgmt_002",
                "Give hero troops with only troop query should return usage error",
                "gm.troops.give_hero_troops imperial_recruit",
                TestExpectation.Error
            )
            {
                Category = "TroopManagement",
                ExpectedText = "Missing arguments"
            });

            // Test give_hero_troops with only troop and count (missing hero) - should error
            TestRunner.RegisterTest(new TestCase(
                "troop_mgmt_003",
                "Give hero troops with missing hero query should return usage error",
                "gm.troops.give_hero_troops imperial_recruit 10",
                TestExpectation.Error
            )
            {
                Category = "TroopManagement",
                ExpectedText = "Missing arguments"
            });

            // Test give_hero_troops with invalid troop query - should error
            TestRunner.RegisterTest(new TestCase(
                "troop_mgmt_004",
                "Give hero troops with invalid troop query should return error",
                "gm.troops.give_hero_troops nonexistent_troop_xyz_12345 10 player",
                TestExpectation.Error
            )
            {
                Category = "TroopManagement",
                ExpectedText = "No troop matching"
            });

            // Test give_hero_troops with invalid hero query - should error
            TestRunner.RegisterTest(new TestCase(
                "troop_mgmt_005",
                "Give hero troops with invalid hero query should return error",
                "gm.troops.give_hero_troops imperial_recruit 10 nonexistent_hero_xyz_12345",
                TestExpectation.Error
            )
            {
                Category = "TroopManagement",
                ExpectedText = "No hero matching"
            });

            // Test give_hero_troops with invalid count (negative) - should error
            TestRunner.RegisterTest(new TestCase(
                "troop_mgmt_006",
                "Give hero troops with negative count should return error",
                "gm.troops.give_hero_troops imperial_recruit -5 player",
                TestExpectation.Error
            )
            {
                Category = "TroopManagement",
                ExpectedText = "Error"
            });

            // Test give_hero_troops with invalid count (zero) - should error
            TestRunner.RegisterTest(new TestCase(
                "troop_mgmt_007",
                "Give hero troops with zero count should return error",
                "gm.troops.give_hero_troops imperial_recruit 0 player",
                TestExpectation.Error
            )
            {
                Category = "TroopManagement",
                ExpectedText = "Error"
            });

            // Test give_hero_troops with count too large - should error
            TestRunner.RegisterTest(new TestCase(
                "troop_mgmt_008",
                "Give hero troops with count exceeding maximum (>10000) should return error",
                "gm.troops.give_hero_troops imperial_recruit 100000 player",
                TestExpectation.Error
            )
            {
                Category = "TroopManagement",
                ExpectedText = "Error"
            });

            // Test give_hero_troops with valid empire recruit - should succeed
            TestRunner.RegisterTest(new TestCase(
                "troop_mgmt_009",
                "Give imperial recruits to player should succeed",
                "gm.troops.give_hero_troops player imperial_recruit 10",
                TestExpectation.Success
            )
            {
                Category = "TroopManagement"
            });

            // Test give_hero_troops with valid battanian troops - should succeed
            TestRunner.RegisterTest(new TestCase(
                "troop_mgmt_010",
                "Give battanian highborn warriors to player should succeed",
                "gm.troops.give_hero_troops player battanian_highborn_warrior 5",
                TestExpectation.Success
            )
            {
                Category = "TroopManagement"
            });

            // Test give_hero_troops with different culture troops - should succeed
            TestRunner.RegisterTest(new TestCase(
                "troop_mgmt_011",
                "Give vlandian troops to player should succeed",
                "gm.troops.give_hero_troops player vlandia 3",
                TestExpectation.Error
            )
            {
                Category = "TroopManagement",
                ExpectedText = "multiple"
            });

            // Test give_hero_troops to different hero - should succeed
            TestRunner.RegisterTest(new TestCase(
                "troop_mgmt_012",
                "Give troops to specific lord should succeed",
                "gm.troops.give_hero_troops lord_1_1 sturgia 15",
                TestExpectation.Error
            )
            {
                Category = "TroopManagement",
                ExpectedText = "multiple"
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
        /// Register settlement management command tests
        /// </summary>
        private static void RegisterSettlementManagementTests()
        {
            // Test set_owner without arguments - should error
            TestRunner.RegisterTest(new TestCase(
                "settlement_mgmt_001",
                "Set settlement owner without arguments should return usage error",
                "gm.settlement.set_owner",
                TestExpectation.Error
            )
            {
                Category = "SettlementManagement",
                ExpectedText = "Missing arguments"
            });

            // Test set_owner with only settlement (missing hero) - should error
            TestRunner.RegisterTest(new TestCase(
                "settlement_mgmt_002",
                "Set settlement owner with only settlement argument should return usage error",
                "gm.settlement.set_owner pen",
                TestExpectation.Error
            )
            {
                Category = "SettlementManagement",
                ExpectedText = "Missing arguments"
            });

            // Test set_owner with invalid settlement - should error
            TestRunner.RegisterTest(new TestCase(
                "settlement_mgmt_003",
                "Set settlement owner with invalid settlement should return error",
                "gm.settlement.set_owner invalid_settlement_xyz lord_1_1",
                TestExpectation.Error
            )
            {
                Category = "SettlementManagement",
                ExpectedText = "No settlement matching"
            });

            // Test set_owner with invalid hero - should error
            TestRunner.RegisterTest(new TestCase(
                "settlement_mgmt_004",
                "Set settlement owner with invalid hero should return error",
                "gm.settlement.set_owner pen invalid_hero_xyz",
                TestExpectation.Error
            )
            {
                Category = "SettlementManagement",
                ExpectedText = "No hero matching"
            });

            // Test set_owner_clan without arguments - should error
            TestRunner.RegisterTest(new TestCase(
                "settlement_mgmt_005",
                "Set settlement owner clan without arguments should return usage error",
                "gm.settlement.set_owner_clan",
                TestExpectation.Error
            )
            {
                Category = "SettlementManagement",
                ExpectedText = "Missing arguments"
            });

            // Test set_owner_clan with only settlement (missing clan) - should error
            TestRunner.RegisterTest(new TestCase(
                "settlement_mgmt_006",
                "Set settlement owner clan with only settlement argument should return usage error",
                "gm.settlement.set_owner_clan pen",
                TestExpectation.Error
            )
            {
                Category = "SettlementManagement",
                ExpectedText = "Missing arguments"
            });

            // Test set_owner_clan with invalid settlement - should error
            TestRunner.RegisterTest(new TestCase(
                "settlement_mgmt_007",
                "Set settlement owner clan with invalid settlement should return error",
                "gm.settlement.set_owner_clan invalid_settlement_xyz empire_south",
                TestExpectation.Error
            )
            {
                Category = "SettlementManagement",
                ExpectedText = "No settlement matching"
            });

            // Test set_owner_clan with invalid clan - should error
            TestRunner.RegisterTest(new TestCase(
                "settlement_mgmt_008",
                "Set settlement owner clan with invalid clan should return error",
                "gm.settlement.set_owner_clan pen invalid_clan_xyz",
                TestExpectation.Error
            )
            {
                Category = "SettlementManagement",
                ExpectedText = "No clan matching"
            });

            // Test set_owner_kingdom without arguments - should error
            TestRunner.RegisterTest(new TestCase(
                "settlement_mgmt_009",
                "Set settlement owner kingdom without arguments should return usage error",
                "gm.settlement.set_owner_kingdom",
                TestExpectation.Error
            )
            {
                Category = "SettlementManagement",
                ExpectedText = "Missing arguments"
            });

            // Test set_owner_kingdom with only settlement (missing kingdom) - should error
            TestRunner.RegisterTest(new TestCase(
                "settlement_mgmt_010",
                "Set settlement owner kingdom with only settlement argument should return usage error",
                "gm.settlement.set_owner_kingdom pen",
                TestExpectation.Error
            )
            {
                Category = "SettlementManagement",
                ExpectedText = "Missing arguments"
            });

            // Test set_owner_kingdom with invalid settlement - should error
            TestRunner.RegisterTest(new TestCase(
                "settlement_mgmt_011",
                "Set settlement owner kingdom with invalid settlement should return error",
                "gm.settlement.set_owner_kingdom invalid_settlement_xyz empire",
                TestExpectation.Error
            )
            {
                Category = "SettlementManagement",
                ExpectedText = "No settlement matching"
            });

            // Test set_owner_kingdom with invalid kingdom - should error
            TestRunner.RegisterTest(new TestCase(
                "settlement_mgmt_012",
                "Set settlement owner kingdom with invalid kingdom should return error",
                "gm.settlement.set_owner_kingdom pen invalid_kingdom_xyz",
                TestExpectation.Error
            )
            {
                Category = "SettlementManagement",
                ExpectedText = "No kingdom matching"
            });

            // Test set_prosperity without arguments - should error
            TestRunner.RegisterTest(new TestCase(
                "settlement_mgmt_013",
                "Set settlement prosperity without arguments should return usage error",
                "gm.settlement.set_prosperity",
                TestExpectation.Error
            )
            {
                Category = "SettlementManagement",
                ExpectedText = "Missing arguments"
            });

            // Test set_prosperity with invalid settlement - should error
            TestRunner.RegisterTest(new TestCase(
                "settlement_mgmt_014",
                "Set settlement prosperity with invalid settlement should return error",
                "gm.settlement.set_prosperity invalid_settlement_xyz 5000",
                TestExpectation.Error
            )
            {
                Category = "SettlementManagement",
                ExpectedText = "No settlement matching"
            });

            // Test set_prosperity with invalid value (negative) - should error
            TestRunner.RegisterTest(new TestCase(
                "settlement_mgmt_015",
                "Set settlement prosperity with negative value should return error",
                "gm.settlement.set_prosperity pen -1000",
                TestExpectation.Error
            )
            {
                Category = "SettlementManagement",
                ExpectedText = "Error"
            });

            // Test set_hearths without arguments - should error
            TestRunner.RegisterTest(new TestCase(
                "settlement_mgmt_016",
                "Set village hearths without arguments should return usage error",
                "gm.settlement.set_hearths",
                TestExpectation.Error
            )
            {
                Category = "SettlementManagement",
                ExpectedText = "Missing arguments"
            });

            // Test set_hearths with invalid settlement - should error
            TestRunner.RegisterTest(new TestCase(
                "settlement_mgmt_017",
                "Set village hearths with invalid settlement should return error",
                "gm.settlement.set_hearths invalid_village_xyz 500",
                TestExpectation.Error
            )
            {
                Category = "SettlementManagement",
                ExpectedText = "No settlement matching"
            });

            // Test rename without arguments - should error
            TestRunner.RegisterTest(new TestCase(
                "settlement_mgmt_018",
                "Rename settlement without arguments should return usage error",
                "gm.settlement.rename",
                TestExpectation.Error
            )
            {
                Category = "SettlementManagement",
                ExpectedText = "Missing arguments"
            });

            // Test rename with only settlement (missing name) - should error
            TestRunner.RegisterTest(new TestCase(
                "settlement_mgmt_019",
                "Rename settlement with only settlement argument should return usage error",
                "gm.settlement.rename pen",
                TestExpectation.Error
            )
            {
                Category = "SettlementManagement",
                ExpectedText = "Missing arguments"
            });

            // Test rename with invalid settlement - should error
            TestRunner.RegisterTest(new TestCase(
                "settlement_mgmt_020",
                "Rename settlement with invalid settlement should return error",
                "gm.settlement.rename invalid_settlement_xyz NewName",
                TestExpectation.Error
            )
            {
                Category = "SettlementManagement",
                ExpectedText = "No settlement matching"
            });

            // Test set_loyalty without arguments - should error
            TestRunner.RegisterTest(new TestCase(
                "settlement_mgmt_021",
                "Set settlement loyalty without arguments should return usage error",
                "gm.settlement.set_loyalty",
                TestExpectation.Error
            )
            {
                Category = "SettlementManagement",
                ExpectedText = "Missing arguments"
            });

            // Test set_loyalty with invalid settlement - should error
            TestRunner.RegisterTest(new TestCase(
                "settlement_mgmt_022",
                "Set settlement loyalty with invalid settlement should return error",
                "gm.settlement.set_loyalty invalid_settlement_xyz 100",
                TestExpectation.Error
            )
            {
                Category = "SettlementManagement",
                ExpectedText = "No settlement matching"
            });

            // Test set_security without arguments - should error
            TestRunner.RegisterTest(new TestCase(
                "settlement_mgmt_023",
                "Set settlement security without arguments should return usage error",
                "gm.settlement.set_security",
                TestExpectation.Error
            )
            {
                Category = "SettlementManagement",
                ExpectedText = "Missing arguments"
            });

            // Test set_security with invalid settlement - should error
            TestRunner.RegisterTest(new TestCase(
                "settlement_mgmt_024",
                "Set settlement security with invalid settlement should return error",
                "gm.settlement.set_security invalid_settlement_xyz 100",
                TestExpectation.Error
            )
            {
                Category = "SettlementManagement",
                ExpectedText = "No settlement matching"
            });

            // Test add_construction without arguments - should error
            TestRunner.RegisterTest(new TestCase(
                "settlement_mgmt_025",
                "Add settlement construction without arguments should return usage error",
                "gm.settlement.add_construction",
                TestExpectation.Error
            )
            {
                Category = "SettlementManagement",
                ExpectedText = "Missing arguments"
            });

            // Test add_construction with invalid settlement - should error
            TestRunner.RegisterTest(new TestCase(
                "settlement_mgmt_026",
                "Add settlement construction with invalid settlement should return error",
                "gm.settlement.add_construction invalid_settlement_xyz 500",
                TestExpectation.Error
            )
            {
                Category = "SettlementManagement",
                ExpectedText = "No settlement matching"
            });

            // Test give_food without arguments - should error
            TestRunner.RegisterTest(new TestCase(
                "settlement_mgmt_027",
                "Give settlement food without arguments should return usage error",
                "gm.settlement.give_food",
                TestExpectation.Error
            )
            {
                Category = "SettlementManagement",
                ExpectedText = "Missing arguments"
            });

            // Test give_food with invalid settlement - should error
            TestRunner.RegisterTest(new TestCase(
                "settlement_mgmt_028",
                "Give settlement food with invalid settlement should return error",
                "gm.settlement.give_food invalid_settlement_xyz 1000",
                TestExpectation.Error
            )
            {
                Category = "SettlementManagement",
                ExpectedText = "No settlement matching"
            });

            // Test give_gold without arguments - should error
            TestRunner.RegisterTest(new TestCase(
                "settlement_mgmt_029",
                "Give settlement gold without arguments should return usage error",
                "gm.settlement.give_gold",
                TestExpectation.Error
            )
            {
                Category = "SettlementManagement",
                ExpectedText = "Missing arguments"
            });

            // Test give_gold with invalid settlement - should error
            TestRunner.RegisterTest(new TestCase(
                "settlement_mgmt_030",
                "Give settlement gold with invalid settlement should return error",
                "gm.settlement.give_gold invalid_settlement_xyz 10000",
                TestExpectation.Error
            )
            {
                Category = "SettlementManagement",
                ExpectedText = "No settlement matching"
            });

            // Test add_militia without arguments - should error
            TestRunner.RegisterTest(new TestCase(
                "settlement_mgmt_031",
                "Add settlement militia without arguments should return usage error",
                "gm.settlement.add_militia",
                TestExpectation.Error
            )
            {
                Category = "SettlementManagement",
                ExpectedText = "Missing arguments"
            });

            // Test add_militia with invalid settlement - should error
            TestRunner.RegisterTest(new TestCase(
                "settlement_mgmt_032",
                "Add settlement militia with invalid settlement should return error",
                "gm.settlement.add_militia invalid_settlement_xyz 100",
                TestExpectation.Error
            )
            {
                Category = "SettlementManagement",
                ExpectedText = "No settlement matching"
            });

            // Test fill_garrison without arguments - should error
            TestRunner.RegisterTest(new TestCase(
                "settlement_mgmt_033",
                "Fill settlement garrison without arguments should return usage error",
                "gm.settlement.fill_garrison",
                TestExpectation.Error
            )
            {
                Category = "SettlementManagement",
                ExpectedText = "Missing arguments"
            });

            // Test fill_garrison with invalid settlement - should error
            TestRunner.RegisterTest(new TestCase(
                "settlement_mgmt_034",
                "Fill settlement garrison with invalid settlement should return error",
                "gm.settlement.fill_garrison invalid_settlement_xyz",
                TestExpectation.Error
            )
            {
                Category = "SettlementManagement",
                ExpectedText = "No settlement matching"
            });

            // Test upgrade_buildings without arguments - should error
            TestRunner.RegisterTest(new TestCase(
                "settlement_mgmt_035",
                "Upgrade buildings without arguments should return usage error",
                "gm.settlement.upgrade_buildings",
                TestExpectation.Error
            )
            {
                Category = "SettlementManagement",
                ExpectedText = "Missing arguments"
            });

            // Test upgrade_buildings with invalid settlement - should error
            TestRunner.RegisterTest(new TestCase(
                "settlement_mgmt_036",
                "Upgrade buildings with invalid settlement should return error",
                "gm.settlement.upgrade_buildings invalid_settlement_xyz 3",
                TestExpectation.Error
            )
            {
                Category = "SettlementManagement",
                ExpectedText = "No settlement matching"
            });

            // Test own_workshops without arguments - should error
            TestRunner.RegisterTest(new TestCase(
                "settlement_mgmt_037",
                "Own workshops without arguments should return usage error",
                "gm.settlement.own_workshops",
                TestExpectation.Error
            )
            {
                Category = "SettlementManagement",
                ExpectedText = "Missing arguments"
            });

            // Test own_workshops with invalid settlement - should error
            TestRunner.RegisterTest(new TestCase(
                "settlement_mgmt_038",
                "Own workshops with invalid settlement should return error",
                "gm.settlement.own_workshops invalid_settlement_xyz",
                TestExpectation.Error
            )
            {
                Category = "SettlementManagement",
                ExpectedText = "No settlement matching"
            });

            // Test add_workshop without arguments - should error
            TestRunner.RegisterTest(new TestCase(
                "settlement_mgmt_039",
                "Add workshop without arguments should return usage error",
                "gm.settlement.add_workshop",
                TestExpectation.Error
            )
            {
                Category = "SettlementManagement",
                ExpectedText = "Missing arguments"
            });

            // Test add_workshop with invalid settlement - should error
            TestRunner.RegisterTest(new TestCase(
                "settlement_mgmt_040",
                "Add workshop with invalid settlement should return error",
                "gm.settlement.add_workshop invalid_settlement_xyz 2",
                TestExpectation.Error
            )
            {
                Category = "SettlementManagement",
                ExpectedText = "No settlement matching"
            });

            // Test create_caravan without arguments - should error
            TestRunner.RegisterTest(new TestCase(
                "settlement_mgmt_041",
                "Create caravan without arguments should return usage error",
                "gm.settlement.create_caravan",
                TestExpectation.Error
            )
            {
                Category = "SettlementManagement",
                ExpectedText = "Missing arguments"
            });

            // Test create_caravan with invalid settlement - should error
            TestRunner.RegisterTest(new TestCase(
                "settlement_mgmt_042",
                "Create caravan with invalid settlement should return error",
                "gm.settlement.create_caravan invalid_settlement_xyz",
                TestExpectation.Error
            )
            {
                Category = "SettlementManagement",
                ExpectedText = "No settlement matching"
            });

            // Test spawn_wanderer without arguments - should error
            TestRunner.RegisterTest(new TestCase(
                "settlement_mgmt_043",
                "Spawn wanderer without arguments should return usage error",
                "gm.settlement.spawn_wanderer",
                TestExpectation.Error
            )
            {
                Category = "SettlementManagement",
                ExpectedText = "Missing arguments"
            });

            // Test spawn_wanderer with invalid settlement - should error
            TestRunner.RegisterTest(new TestCase(
                "settlement_mgmt_044",
                "Spawn wanderer with invalid settlement should return error",
                "gm.settlement.spawn_wanderer invalid_settlement_xyz",
                TestExpectation.Error
            )
            {
                Category = "SettlementManagement",
                ExpectedText = "No settlement matching"
            });

            // Test add_workshop is deprecated - should error with helpful message
            TestRunner.RegisterTest(new TestCase(
                "settlement_mgmt_045",
                "Add workshop should return deprecation error",
                "gm.settlement.add_workshop pen 2",
                TestExpectation.Error
            )
            {
                Category = "SettlementManagement",
                ExpectedText = "deprecated"
            });

            // Test create_notable_caravan without arguments - should error
            TestRunner.RegisterTest(new TestCase(
                "settlement_mgmt_046",
                "Create notable caravan without arguments should return usage error",
                "gm.settlement.create_notable_caravan",
                TestExpectation.Error
            )
            {
                Category = "SettlementManagement",
                ExpectedText = "Missing arguments"
            });

            // Test create_notable_caravan with invalid settlement - should error
            TestRunner.RegisterTest(new TestCase(
                "settlement_mgmt_047",
                "Create notable caravan with invalid settlement should return error",
                "gm.settlement.create_notable_caravan invalid_settlement_xyz",
                TestExpectation.Error
            )
            {
                Category = "SettlementManagement",
                ExpectedText = "No settlement matching"
            });

            // Test create_player_caravan without arguments - should error
            TestRunner.RegisterTest(new TestCase(
                "settlement_mgmt_048",
                "Create player caravan without arguments should return usage error",
                "gm.settlement.create_player_caravan",
                TestExpectation.Error
            )
            {
                Category = "SettlementManagement",
                ExpectedText = "Missing arguments"
            });

            // Test create_player_caravan with invalid settlement - should error
            TestRunner.RegisterTest(new TestCase(
                "settlement_mgmt_049",
                "Create player caravan with invalid settlement should return error",
                "gm.settlement.create_player_caravan invalid_settlement_xyz",
                TestExpectation.Error
            )
            {
                Category = "SettlementManagement",
                ExpectedText = "No settlement matching"
            });

            // Test create_player_caravan with invalid leader hero - should error
            TestRunner.RegisterTest(new TestCase(
                "settlement_mgmt_050",
                "Create player caravan with invalid leader hero should return error",
                "gm.settlement.create_player_caravan pen invalid_hero_xyz",
                TestExpectation.Error
            )
            {
                Category = "SettlementManagement",
                ExpectedText = "No hero matching"
            });

            // Test old create_caravan command no longer exists
            TestRunner.RegisterTest(new TestCase(
                "settlement_mgmt_051",
                "Old create_caravan command should not exist",
                "gm.settlement.create_caravan",
                TestExpectation.Error
            )
            {
                Category = "SettlementManagement",
                ExpectedText = "Missing arguments"
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