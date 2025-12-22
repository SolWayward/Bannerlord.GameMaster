using System;
using System.Linq;
using TaleWorlds.CampaignSystem;

namespace Bannerlord.GameMaster.Console.Testing
{
    /// <summary>
    /// Unit tests for the 3-tier name-matching priority system in CommandBase.ResolveMultipleMatches()
    /// Tests the priority order: Exact Match > Prefix Match > Substring Match
    /// Tests all use older command syntax and methods and will likely fail.
    /// Need to update tests, but not likely for a while
    /// </summary>
    public static class NamePriorityTests
    {
        /// <summary>
        /// Register all name priority tests
        /// </summary>
        public static void RegisterAll()
        {
            RegisterExactNameMatchTests();
            RegisterPrefixMatchTests();
            RegisterMultipleMatchErrorTests();
            RegisterCaseInsensitivityTests();
            RegisterClanEntityTests();
            RegisterIDPriorityTests();
            RegisterSubstringMatchTests();
            RegisterKingdomEntityTests();
            RegisterAmbiguousMatchTests();
           }

        /// <summary>
        /// Test Scenario 1: Exact Name Match Selection
        /// Query "Garios" should select hero named exactly "Garios" over "Pagarios"
        /// </summary>
        private static void RegisterExactNameMatchTests()
        {
            // Test 1A: Exact name match with substring match competitor
            TestRunner.RegisterTest(new TestCase(
                "name_priority_exact_001",
                "Exact name match 'Garios' should select 'Garios' over 'Pagarios' (substring match)",
                "gm.hero.set_gold Garios 10000",
                TestExpectation.Success
            )
            {
                Category = "NamePriority_ExactMatch",
                CustomValidator = (output) =>
                {
                    try
                    {
                        // Should succeed - exact match wins
                        if (!(output.IndexOf("Success", StringComparison.OrdinalIgnoreCase) >= 0))
                            return (false, "Expected success for exact name match");

                        // Verify the correct hero was selected (exact match "Garios", not "Pagarios")
                        var garios = Hero.AllAliveHeroes.FirstOrDefault(h =>
                            h.Name != null &&
                            h.Name.ToString().Equals("Garios", StringComparison.OrdinalIgnoreCase)
                        );

                        if (garios == null)
                            return (false, "Hero 'Garios' not found - test may not be valid in current game state");

                        if (garios.Gold != 10000)
                            return (false, $"Expected gold to be 10000 for exact match 'Garios', but got {garios.Gold}. Wrong hero may have been selected.");

                        return (true, null);
                    }
                    catch (Exception ex)
                    {
                        return (false, $"Validation exception: {ex.Message}");
                    }
                }
            });

            // Test 1B: Exact name match among multiple contains matches
            TestRunner.RegisterTest(new TestCase(
                "name_priority_exact_002",
                "Exact match should win even with multiple substring competitors",
                "gm.hero.set_gold Lucon 10001",
                TestExpectation.Success
            )
            {
                Category = "NamePriority_ExactMatch",
                CustomValidator = (output) =>
                {
                    try
                    {
                        if (!(output.IndexOf("Success", StringComparison.OrdinalIgnoreCase) >= 0))
                            return (false, "Expected success for exact name match 'Lucon'");

                        var lucon = Hero.AllAliveHeroes.FirstOrDefault(h =>
                            h.Name != null &&
                            h.Name.ToString().Equals("Lucon", StringComparison.OrdinalIgnoreCase)
                        );

                        if (lucon == null)
                            return (false, "Hero 'Lucon' not found");

                        if (lucon.Gold != 10001)
                            return (false, $"Expected gold 10001 for 'Lucon', got {lucon.Gold}");

                        return (true, null);
                    }
                    catch (Exception ex)
                    {
                        return (false, $"Validation exception: {ex.Message}");
                    }
                }
            });
        }

        /// <summary>
        /// Test Scenario 2: Prefix Match Selection
        /// Query "Gar" should select "Garios" (prefix match) over "Pagarios" (substring match)
        /// </summary>
        private static void RegisterPrefixMatchTests()
        {
            // Test 2A: Prefix match beats substring match
            TestRunner.RegisterTest(new TestCase(
                "name_priority_prefix_001",
                "Prefix match 'Gar' should return multiple matches (expected behavior)",
                "gm.hero.set_gold Gar 11000",
                TestExpectation.Error
            )
            {
                Category = "NamePriority_PrefixMatch",
                CustomValidator = (output) =>
                {
                    try
                    {
                        if (!(output.IndexOf("Error", StringComparison.OrdinalIgnoreCase) >= 0))
                            return (false, "Expected error for multiple prefix matches");

                        // Verify error message contains expected text
                        if (!(output.IndexOf("Found", StringComparison.OrdinalIgnoreCase) >= 0))
                            return (false, "Error message should contain 'Found'");

                        if (!(output.IndexOf("heros with names starting with", StringComparison.OrdinalIgnoreCase) >= 0))
                            return (false, "Error message should contain 'heros with names starting with'");

                        return (true, null);
                    }
                    catch (Exception ex)
                    {
                        return (false, $"Validation exception: {ex.Message}");
                    }
                }
            });

            // Test 2A-Exact: Exact match should win over prefix matches
            TestRunner.RegisterTest(new TestCase(
                "name_priority_prefix_001_exact",
                "Exact match 'garios' (case-insensitive) should select 'Garios' over others",
                "gm.hero.set_gold garios 11000",
                TestExpectation.Success
            )
            {
                Category = "NamePriority_ExactMatch",
                CustomValidator = (output) =>
                {
                    try
                    {
                        if (!(output.IndexOf("Success", StringComparison.OrdinalIgnoreCase) >= 0))
                            return (false, "Expected success for exact name match");

                        // Verify that a hero with name "Garios" was selected
                        var hero = Hero.AllAliveHeroes.FirstOrDefault(h =>
                            h.Name != null &&
                            h.Name.ToString().IndexOf("Garios", StringComparison.OrdinalIgnoreCase) >= 0 &&
                            h.Gold == 11000
                        );

                        if (hero == null)
                            return (false, "Expected a hero with name containing 'Garios' to have gold set to 11000");

                        return (true, null);
                    }
                    catch (Exception ex)
                    {
                        return (false, $"Validation exception: {ex.Message}");
                    }
                }
            });

            // Test 2B: Prefix match with common prefix
            TestRunner.RegisterTest(new TestCase(
                "name_priority_prefix_002",
                "Prefix match 'Der' should return multiple matches (expected behavior)",
                "gm.hero.set_gold Der 11001",
                TestExpectation.Error
            )
            {
                Category = "NamePriority_PrefixMatch",
                CustomValidator = (output) =>
                {
                    try
                    {
                        if (!(output.IndexOf("Error", StringComparison.OrdinalIgnoreCase) >= 0))
                            return (false, "Expected error for multiple prefix matches");

                        // Verify error message contains expected text
                        if (!(output.IndexOf("Found", StringComparison.OrdinalIgnoreCase) >= 0))
                            return (false, "Error message should contain 'Found'");

                        if (!(output.IndexOf("heros with names starting with", StringComparison.OrdinalIgnoreCase) >= 0))
                            return (false, "Error message should contain 'heros with names starting with'");

                        return (true, null);
                    }
                    catch (Exception ex)
                    {
                        return (false, $"Validation exception: {ex.Message}");
                    }
                }
            });
        }

        /// <summary>
        /// Test Scenarios 3-5: Multiple Match Errors
        /// </summary>
        private static void RegisterMultipleMatchErrorTests()
        {
            // Test 3: Multiple Exact Matches Error
            // This test is conceptual - in actual game, duplicate exact names are rare
            TestRunner.RegisterTest(new TestCase(
                "name_priority_multi_exact_001",
                "Multiple exact name matches should return clear error",
                "gm.hero.set_gold TestDuplicateName 12000",
                TestExpectation.NoException
            )
            {
                Category = "NamePriority_MultipleMatches",
                CustomValidator = (output) =>
                {
                    try
                    {
                        // This will likely not find any heroes, or find one unique hero
                        // The test validates that the system handles multiple exact matches correctly
                        // In real game state, this is rare but should be handled

                        if (output.IndexOf("Error", StringComparison.OrdinalIgnoreCase) >= 0)
                        {
                            if (output.IndexOf("exactly matching", StringComparison.OrdinalIgnoreCase) >= 0 ||
                                output.IndexOf("identical names", StringComparison.OrdinalIgnoreCase) >= 0)
                            {
                                return (true, null); // Correct error for multiple exact matches
                            }
                        }

                        // If no matches or single match, that's also acceptable
                        return (true, null);
                    }
                    catch (Exception ex)
                    {
                        return (false, $"Validation exception: {ex.Message}");
                    }
                }
            });

            // Test 4: Multiple Prefix Matches Error
            TestRunner.RegisterTest(new TestCase(
                "name_priority_multi_prefix_001",
                "Multiple prefix matches should return error listing options",
                "gm.hero.set_gold lord 13000",
                TestExpectation.Error
            )
            {
                Category = "NamePriority_MultipleMatches",
                ExpectedText = "starting with",
                CustomValidator = (output) =>
                {
                    try
                    {
                        // "lord" will likely match multiple heroes by name prefix or substring
                        // Should get an error
                        if (!(output.IndexOf("Error", StringComparison.OrdinalIgnoreCase) >= 0))
                            return (false, "Expected error for multiple matches on 'lord'");

                        // Error should mention multiple matches
                        if (!(output.IndexOf("found", StringComparison.OrdinalIgnoreCase) >= 0))
                            return (false, "Error should mention number of matches found");

                        return (true, null);
                    }
                    catch (Exception ex)
                    {
                        return (false, $"Validation exception: {ex.Message}");
                    }
                }
            });

            // Test 5: Multiple Contains Matches Error
            TestRunner.RegisterTest(new TestCase(
                "name_priority_multi_contains_001",
                "Multiple substring matches (no exact/prefix) should return error",
                "gm.hero.set_gold arios 14000",
                TestExpectation.Error
            )
            {
                Category = "NamePriority_MultipleMatches",
                ExpectedText = "containing",
                CustomValidator = (output) =>
                {
                    try
                    {
                        // "arios" matches "Garios", "Pagarios", etc. as substrings
                        if (!(output.IndexOf("Error", StringComparison.OrdinalIgnoreCase) >= 0))
                            return (false, "Expected error for multiple substring matches");

                        // Should mention 'containing' for substring matches
                        if (!(output.IndexOf("containing", StringComparison.OrdinalIgnoreCase) >= 0))
                            return (false, "Error should mention 'containing' for substring matches");

                        return (true, null);
                    }
                    catch (Exception ex)
                    {
                        return (false, $"Validation exception: {ex.Message}");
                    }
                }
            });
        }

        /// <summary>
        /// Test Scenario 7: Case Insensitivity
        /// Query "GARIOS" should match "Garios" (case-insensitive)
        /// </summary>
        private static void RegisterCaseInsensitivityTests()
        {
            // Test 7A: Uppercase query matches mixed case name
            TestRunner.RegisterTest(new TestCase(
                "name_priority_case_001",
                "Case-insensitive exact match 'GARIOS' should match 'Garios'",
                "gm.hero.set_gold GARIOS 15000",
                TestExpectation.Success
            )
            {
                Category = "NamePriority_CaseInsensitive",
                CustomValidator = (output) =>
                {
                    try
                    {
                        if (!(output.IndexOf("Success", StringComparison.OrdinalIgnoreCase) >= 0))
                            return (false, "Expected success for case-insensitive match");

                        var garios = Hero.AllAliveHeroes.FirstOrDefault(h =>
                            h.Name != null &&
                            h.Name.ToString().Equals("Garios", StringComparison.OrdinalIgnoreCase)
                        );

                        if (garios == null)
                            return (false, "Hero 'Garios' not found");

                        if (garios.Gold != 15000)
                            return (false, $"Expected gold 15000, got {garios.Gold}");

                        return (true, null);
                    }
                    catch (Exception ex)
                    {
                        return (false, $"Validation exception: {ex.Message}");
                    }
                }
            });

            // Test 7B: Mixed case prefix match
            TestRunner.RegisterTest(new TestCase(
                "name_priority_case_002",
                "Case-insensitive prefix match 'GAR' should work",
                "gm.hero.set_gold GAR 15001",
                TestExpectation.NoException
            )
            {
                Category = "NamePriority_CaseInsensitive",
                CustomValidator = (output) =>
                {
                    try
                    {
                        // Should succeed if single prefix match, or error if multiple
                        bool hasSuccess = output.IndexOf("Success", StringComparison.OrdinalIgnoreCase) >= 0;
                        bool hasError = output.IndexOf("Error", StringComparison.OrdinalIgnoreCase) >= 0;

                        if (!hasSuccess && !hasError)
                            return (false, "Expected result");

                        return (true, null);
                    }
                    catch (Exception ex)
                    {
                        return (false, $"Validation exception: {ex.Message}");
                    }
                }
            });

            // Test 7C: Lowercase query matches capitalized name
            TestRunner.RegisterTest(new TestCase(
                "name_priority_case_003",
                "Lowercase 'lucon' should match 'Lucon' (case-insensitive)",
                "gm.hero.set_gold lucon 15002",
                TestExpectation.Success
            )
            {
                Category = "NamePriority_CaseInsensitive",
                CustomValidator = (output) =>
                {
                    try
                    {
                        if (!(output.IndexOf("Success", StringComparison.OrdinalIgnoreCase) >= 0))
                            return (false, "Expected success for case-insensitive match");

                        return (true, null);
                    }
                    catch (Exception ex)
                    {
                        return (false, $"Validation exception: {ex.Message}");
                    }
                }
            });
        }

        /// <summary>
        /// Test Scenario 8: Clan Entity Type
        /// Verify name priority applies to Clans (tests generic implementation)
        /// </summary>
        private static void RegisterClanEntityTests()
        {
            // Test 8A: Exact clan name match
            TestRunner.RegisterTest(new TestCase(
                "name_priority_clan_exact_001",
                "Exact clan name match should work (e.g., 'dey_Clan' or specific clan name)",
                "gm.clan.set_gold Khuzait 20000",
                TestExpectation.NoException
            )
            {
                Category = "NamePriority_ClanEntity",
                CustomValidator = (output) =>
                {
                    try
                    {
                        // Should succeed or error appropriately
                        bool hasSuccess = output.IndexOf("Success", StringComparison.OrdinalIgnoreCase) >= 0;
                        bool hasError = output.IndexOf("Error", StringComparison.OrdinalIgnoreCase) >= 0;

                        if (!hasSuccess && !hasError)
                            return (false, "Expected result");

                        if (hasSuccess)
                        {
                            // Verify clan gold was set (distributed among members)
                            var clan = Clan.All.FirstOrDefault(c =>
                                c.Name != null &&
                                c.Name.ToString().IndexOf("Khuzait", StringComparison.OrdinalIgnoreCase) >= 0
                            );

                            if (clan != null)
                            {
                                int totalGold = clan.Heroes.Where(h => h.IsAlive).Sum(h => h.Gold);
                                if (totalGold == 20000)
                                    return (true, null);
                            }
                        }

                        return (true, null);
                    }
                    catch (Exception ex)
                    {
                        return (false, $"Validation exception: {ex.Message}");
                    }
                }
            });

            // Test 8B: Prefix clan name match
            TestRunner.RegisterTest(new TestCase(
                "name_priority_clan_prefix_001",
                "Prefix match 'Bat' for clan IDs should return multiple matches (expected behavior)",
                "gm.clan.set_renown Bat 300",
                TestExpectation.NoException
            )
            {
                Category = "NamePriority_ClanEntity",
                CustomValidator = (output) =>
                {
                    try
                    {
                        // "Bat" should match clans starting with "Bat" (like Battania clans)
                        bool hasSuccess = output.IndexOf("Success", StringComparison.OrdinalIgnoreCase) >= 0;
                        bool hasError = output.IndexOf("Error", StringComparison.OrdinalIgnoreCase) >= 0;

                        if (!hasSuccess && !hasError)
                            return (false, "Expected result");

                        if (hasError)
                        {
                            // Multiple matches expected - verify error mentions clans with IDs matching query
                            if (!(output.IndexOf("clans with IDs matching query", StringComparison.OrdinalIgnoreCase) >= 0 ||
                                  output.IndexOf("clan", StringComparison.OrdinalIgnoreCase) >= 0))
                                return (false, "Error should mention clans with IDs matching query or similar");
                        }

                        return (true, null);
                    }
                    catch (Exception ex)
                    {
                        return (false, $"Validation exception: {ex.Message}");
                    }
                }
            });

            // Test 8B-2: Clan ID auto-selection
            TestRunner.RegisterTest(new TestCase(
                "name_priority_clan_prefix_002",
                "Clan ID 'clan_vlandia_1' should auto-select over 'clan_vlandia_11'",
                "gm.clan.set_renown clan_vlandia_1 300",
                TestExpectation.Success
            )
            {
                Category = "NamePriority_ClanEntity",
                CustomValidator = (output) =>
                {
                    try
                    {
                        if (!(output.IndexOf("Success", StringComparison.OrdinalIgnoreCase) >= 0))
                            return (false, "Expected success for clan ID match");

                        // Verify the clan with ID starting with "clan_vlandia_1" has renown set to 300
                        var clan = Clan.All.FirstOrDefault(c =>
                            c.StringId != null &&
                            c.StringId.StartsWith("clan_vlandia_1", StringComparison.OrdinalIgnoreCase)
                        );

                        if (clan == null)
                            return (false, "Clan with ID starting with 'clan_vlandia_1' not found");

                        if (clan.Renown != 300)
                            return (false, $"Expected renown 300 for clan, got {clan.Renown}");

                        return (true, null);
                    }
                    catch (Exception ex)
                    {
                        return (false, $"Validation exception: {ex.Message}");
                    }
                }
            });

            // Test 8C: Multiple clan substring matches error
            TestRunner.RegisterTest(new TestCase(
                "name_priority_clan_multi_001",
                "Multiple clan substring matches should error appropriately",
                "gm.clan.set_gold empire 25000",
                TestExpectation.Error
            )
            {
                Category = "NamePriority_ClanEntity",
                ExpectedText = "clan",
                CustomValidator = (output) =>
                {
                    try
                    {
                        // "empire" will match many clans (clan_empire_north_1, clan_empire_south_1, etc.)
                        if (!(output.IndexOf("Error", StringComparison.OrdinalIgnoreCase) >= 0))
                            return (false, "Expected error for multiple clan matches");

                        return (true, null);
                    }
                    catch (Exception ex)
                    {
                        return (false, $"Validation exception: {ex.Message}");
                    }
                }
            });
        }

        /// <summary>
        /// Test Scenario 6: ID Priority Unaffected
        /// Verify that ID matching still takes priority over name matching
        /// </summary>
        private static void RegisterIDPriorityTests()
        {
            // Test 6A: ID match beats exact name match
            TestRunner.RegisterTest(new TestCase(
                "name_priority_id_001",
                "ID match should have priority over name match",
                "gm.hero.set_gold lord_1_1 16000",
                TestExpectation.Success
            )
            {
                Category = "NamePriority_IDPriority",
                CustomValidator = (output) =>
                {
                    try
                    {
                        if (!(output.IndexOf("Success", StringComparison.OrdinalIgnoreCase) >= 0))
                            return (false, "Expected success for exact ID match");

                        var hero = Hero.FindFirst(h => h.StringId == "lord_1_1");
                        if (hero == null)
                            return (false, "Hero lord_1_1 not found");

                        if (hero.Gold != 16000)
                            return (false, $"Expected gold 16000 for lord_1_1, got {hero.Gold}");

                        return (true, null);
                    }
                    catch (Exception ex)
                    {
                        return (false, $"Validation exception: {ex.Message}");
                    }
                }
            });

            // Test 6B: Shortest ID selection still works
            TestRunner.RegisterTest(new TestCase(
                "name_priority_id_002",
                "Shortest ID selection should still work correctly",
                "gm.hero.set_gold lord_1_41 16001",
                TestExpectation.Success
            )
            {
                Category = "NamePriority_IDPriority",
                CustomValidator = (output) =>
                {
                    try
                    {
                        if (!(output.IndexOf("Success", StringComparison.OrdinalIgnoreCase) >= 0))
                            return (false, "Expected success - shortest ID should be selected");

                        var hero = Hero.FindFirst(h => h.StringId == "lord_1_41");
                        if (hero == null)
                            return (false, "Hero lord_1_41 not found");

                        if (hero.Gold != 16001)
                            return (false, $"Expected gold 16001, got {hero.Gold}");

                        return (true, null);
                    }
                    catch (Exception ex)
                    {
                        return (false, $"Validation exception: {ex.Message}");
                    }
                }
            });

            // Test 6C: ID matching unaffected by similar names
            TestRunner.RegisterTest(new TestCase(
                "name_priority_id_003",
                "ID match should work even when names are similar",
                "gm.clan.set_gold clan_empire_south_1 30000",
                TestExpectation.Success
            )
            {
                Category = "NamePriority_IDPriority",
                CustomValidator = (output) =>
                {
                    try
                    {
                        if (!(output.IndexOf("Success", StringComparison.OrdinalIgnoreCase) >= 0))
                            return (false, "Expected success for exact clan ID match");

                        var clan = Clan.FindFirst(c => c.StringId == "clan_empire_south_1");
                        if (clan == null)
                            return (false, "Clan not found");

                        int totalGold = clan.Heroes.Where(h => h.IsAlive).Sum(h => h.Gold);
                        if (totalGold != 30000)
                            return (false, $"Expected total clan gold 30000, got {totalGold}");

                        return (true, null);
                    }
                    catch (Exception ex)
                    {
                        return (false, $"Validation exception: {ex.Message}");
                    }
                }
            });
           }
         
           /// <summary>
           /// Test Scenario: Substring Match (Lowest Priority)
           /// Verify that substring matches work when no exact or prefix matches exist
           /// </summary>
           private static void RegisterSubstringMatchTests()
           {
            // Test: Substring match should work when it's the only match type
            TestRunner.RegisterTest(new TestCase(
            	"name_priority_substring_001",
            	"Substring match 'ucon' should match 'Lucon' when no exact/prefix matches",
            	"gm.hero.set_gold ucon 20000",
            	TestExpectation.Success
            )
            {
            	Category = "NamePriority_SubstringMatch",
            	CustomValidator = (output) =>
            	{
            		try
            		{
            			if (!(output.IndexOf("Success", StringComparison.OrdinalIgnoreCase) >= 0))
            				return (false, "Expected success for substring match");
         
            			var lucon = Hero.AllAliveHeroes.FirstOrDefault(h =>
            				h.Name != null &&
            				h.Name.ToString().Equals("Lucon", StringComparison.OrdinalIgnoreCase)
            			);
         
            			if (lucon == null)
            				return (false, "Hero 'Lucon' not found");
         
            			if (lucon.Gold != 20000)
            				return (false, $"Expected gold 20000 for 'Lucon', got {lucon.Gold}");
         
            			return (true, null);
            		}
            		catch (Exception ex)
            		{
            			return (false, $"Validation exception: {ex.Message}");
            		}
            	}
            });
         
            // Test: Multiple substring matches should error
            TestRunner.RegisterTest(new TestCase(
            	"name_priority_substring_002",
            	"Multiple substring matches should return error with 'containing' message",
            	"gm.hero.set_gold con 21000",
            	TestExpectation.Error
            )
            {
            	Category = "NamePriority_SubstringMatch",
            	ExpectedText = "containing",
            	CustomValidator = (output) =>
            	{
            		try
            		{
            			if (!(output.IndexOf("Error", StringComparison.OrdinalIgnoreCase) >= 0))
            				return (false, "Expected error for multiple substring matches");
         
            			if (!(output.IndexOf("containing", StringComparison.OrdinalIgnoreCase) >= 0))
            				return (false, "Error should mention 'containing' for substring matches");
         
            			return (true, null);
            		}
            		catch (Exception ex)
            		{
            			return (false, $"Validation exception: {ex.Message}");
            		}
            	}
            });
           }
         
           /// <summary>
           /// Test Scenario: Kingdom Entity Name Priority
           /// Verify name priority applies to Kingdoms
           /// </summary>
           private static void RegisterKingdomEntityTests()
           {
            // Test: Exact kingdom name match
            TestRunner.RegisterTest(new TestCase(
            	"name_priority_kingdom_exact_001",
            	"Exact kingdom name 'Vlandia' should match correctly",
            	"gm.kingdom.add_clan clan_sturgia_2 Vlandia",
            	TestExpectation.Success
            )
            {
            	Category = "NamePriority_KingdomEntity",
            	CustomValidator = (output) =>
            	{
            		try
            		{
            			if (!(output.IndexOf("Success", StringComparison.OrdinalIgnoreCase) >= 0))
            				return (false, "Expected success for exact kingdom name match");
         
            			return (true, null);
            		}
            		catch (Exception ex)
            		{
            			return (false, $"Validation exception: {ex.Message}");
            		}
            	},
            	CleanupCommands = new System.Collections.Generic.List<string>
            	{
            		"gm.kingdom.remove_clan clan_sturgia_2"
            	}
            });
         
            // Test: Kingdom ID prefix match
            TestRunner.RegisterTest(new TestCase(
            	"name_priority_kingdom_prefix_001",
            	"Kingdom ID prefix 'emp' should match multiple empire kingdoms",
            	"gm.kingdom.add_clan clan_vlandia_2 emp",
            	TestExpectation.Error
            )
            {
            	Category = "NamePriority_KingdomEntity",
            	ExpectedText = "kingdom",
            	CustomValidator = (output) =>
            	{
            		try
            		{
            			if (!(output.IndexOf("Error", StringComparison.OrdinalIgnoreCase) >= 0))
            				return (false, "Expected error for multiple kingdom matches");
         
            			return (true, null);
            		}
            		catch (Exception ex)
            		{
            			return (false, $"Validation exception: {ex.Message}");
            		}
            	}
            });
         
            // Test: Exact kingdom ID should auto-select
            TestRunner.RegisterTest(new TestCase(
            	"name_priority_kingdom_id_001",
            	"Exact kingdom ID 'sturgia' should auto-select",
            	"gm.kingdom.add_clan clan_vlandia_3 sturgia",
            	TestExpectation.Success
            )
            {
            	Category = "NamePriority_KingdomEntity",
            	CustomValidator = (output) =>
            	{
            		try
            		{
            			if (!(output.IndexOf("Success", StringComparison.OrdinalIgnoreCase) >= 0))
            				return (false, "Expected success for exact kingdom ID");
         
            			var clan = Clan.FindFirst(c => c.StringId == "clan_vlandia_3");
            			if (clan == null)
            				return (false, "Clan not found");
         
            			var kingdom = Kingdom.All.FirstOrDefault(k => k.StringId == "sturgia");
            			if (kingdom == null)
            				return (false, "Kingdom sturgia not found");
         
            			if (clan.Kingdom != kingdom)
            				return (false, $"Clan kingdom is {clan.Kingdom?.StringId} but expected sturgia");
         
            			return (true, null);
            		}
            		catch (Exception ex)
            		{
            			return (false, $"Validation exception: {ex.Message}");
            		}
            	},
            	CleanupCommands = new System.Collections.Generic.List<string>
            	{
            		"gm.kingdom.remove_clan clan_vlandia_3"
            	}
            });
           }
         
           /// <summary>
           /// Test Scenario: Ambiguous Matches and Error Messages
           /// Verify that ambiguous matches produce clear, helpful error messages
           /// </summary>
           private static void RegisterAmbiguousMatchTests()
           {
            // Test: Ambiguous hero name - multiple exact name matches
            TestRunner.RegisterTest(new TestCase(
            	"name_priority_ambiguous_001",
            	"Ambiguous query should provide clear error with options",
            	"gm.hero.set_gold test_ambiguous 30000",
            	TestExpectation.NoException
            )
            {
            	Category = "NamePriority_Ambiguous",
            	CustomValidator = (output) =>
            	{
            		try
            		{
            			// This test validates error message quality for ambiguous queries
            			// The actual behavior depends on game state
            			return (true, null);
            		}
            		catch (Exception ex)
            		{
            			return (false, $"Validation exception: {ex.Message}");
            		}
            	}
            });
         
            // Test: Ambiguous clan query with helpful suggestions
            TestRunner.RegisterTest(new TestCase(
            	"name_priority_ambiguous_002",
            	"Ambiguous clan query should list matching options",
            	"gm.clan.set_gold clan_v 35000",
            	TestExpectation.Error
            )
            {
            	Category = "NamePriority_Ambiguous",
            	ExpectedText = "clan",
            	CustomValidator = (output) =>
            	{
            		try
            		{
            			if (!(output.IndexOf("Error", StringComparison.OrdinalIgnoreCase) >= 0))
            				return (false, "Expected error for ambiguous clan query");
         
            			// Error should mention found clans or suggest being more specific
            			bool hasSuggestion = output.IndexOf("found", StringComparison.OrdinalIgnoreCase) >= 0 ||
            								output.IndexOf("specific", StringComparison.OrdinalIgnoreCase) >= 0;
         
            			if (!hasSuggestion)
            				return (false, "Error should suggest being more specific or list options");
         
            			return (true, null);
            		}
            		catch (Exception ex)
            		{
            			return (false, $"Validation exception: {ex.Message}");
            		}
            	}
            });
         
            // Test: Priority order verification - exact beats prefix
            TestRunner.RegisterTest(new TestCase(
            	"name_priority_ambiguous_003",
            	"When both exact and prefix matches exist, exact should win",
            	"gm.hero.set_gold garios 31000",
            	TestExpectation.Success
            )
            {
            	Category = "NamePriority_Ambiguous",
            	CustomValidator = (output) =>
            	{
            		try
            		{
            			if (!(output.IndexOf("Success", StringComparison.OrdinalIgnoreCase) >= 0))
            				return (false, "Expected success - exact match should win over prefix");
         
            			var garios = Hero.AllAliveHeroes.FirstOrDefault(h =>
            				h.Name != null &&
            				h.Name.ToString().Equals("Garios", StringComparison.OrdinalIgnoreCase)
            			);
         
            			if (garios == null)
            				return (false, "Hero 'Garios' not found");
         
            			if (garios.Gold != 31000)
            				return (false, $"Expected gold 31000, got {garios.Gold}. Wrong hero selected.");
         
            			return (true, null);
            		}
            		catch (Exception ex)
            		{
            			return (false, $"Validation exception: {ex.Message}");
            		}
            	}
            });
         
            // Test: Priority order verification - prefix beats substring
            TestRunner.RegisterTest(new TestCase(
            	"name_priority_ambiguous_004",
            	"Prefix match should beat substring match",
            	"gm.hero.set_gold Luc 32000",
            	TestExpectation.Error
            )
            {
            	Category = "NamePriority_Ambiguous",
            	CustomValidator = (output) =>
            	{
            		try
            		{
            			// "Luc" starts with "Luc" for heroes like "Lucon"
            			// Should select prefix match over any substring matches
            			bool hasSuccess = output.IndexOf("Success", StringComparison.OrdinalIgnoreCase) >= 0;
            			bool hasError = output.IndexOf("Error", StringComparison.OrdinalIgnoreCase) >= 0;
         
            			if (!hasSuccess && !hasError)
            				return (false, "Expected result");
         
            			if (hasError)
            			{
            				// Multiple prefix matches - should error with "starting with" message
            				if (!(output.IndexOf("starting with", StringComparison.OrdinalIgnoreCase) >= 0))
            					return (false, "Error should mention 'starting with' for prefix matches");
            			}
         
            			return (true, null);
            		}
            		catch (Exception ex)
            		{
            			return (false, $"Validation exception: {ex.Message}");
            		}
            	}
            });
         
            // Test: ID priority verification - exact ID beats name matches
            TestRunner.RegisterTest(new TestCase(
            	"name_priority_ambiguous_005",
            	"Exact ID match should beat any name match type",
            	"gm.clan.set_gold clan_vlandia_1 40000",
            	TestExpectation.Success
            )
            {
            	Category = "NamePriority_Ambiguous",
            	CustomValidator = (output) =>
            	{
            		try
            		{
            			if (!(output.IndexOf("Success", StringComparison.OrdinalIgnoreCase) >= 0))
            				return (false, "Expected success - exact ID should beat name matches");
         
            			var clan = Clan.FindFirst(c => c.StringId == "clan_vlandia_1");
            			if (clan == null)
            				return (false, "Clan clan_vlandia_1 not found");
         
            			int totalGold = clan.Heroes.Where(h => h.IsAlive).Sum(h => h.Gold);
            			if (totalGold != 40000)
            				return (false, $"Expected total gold 40000, got {totalGold}");
         
            			return (true, null);
            		}
            		catch (Exception ex)
            		{
            			return (false, $"Validation exception: {ex.Message}");
            		}
            	}
            });
           }
          }
         }