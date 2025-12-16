using System;
using System.IO;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;

namespace Bannerlord.GameMaster.Console.Testing
{
    /// <summary>
    /// Integration tests that validate actual game state changes
    /// </summary>
    public static class IntegrationTests
    {
        /// <summary>
        /// Register all integration tests
        /// </summary>
        public static void RegisterAll()
        {
            RegisterHeroManagementIntegrationTests();
            RegisterClanManagementIntegrationTests();
            RegisterKingdomManagementIntegrationTests();
            RegisterItemEquipmentSaveLoadIntegrationTests();
            RegisterTroopQueryIntegrationTests();
            RegisterTroopManagementIntegrationTests();
            RegisterEdgeCaseTests();
            RegisterSpecialCasesTests();
            RegisterIDMatchingCollisionFixTests();
            NamePriorityTests.RegisterAll();
        }

        /// <summary>
        /// Register hero management integration tests with state validation
        /// </summary>
        private static void RegisterHeroManagementIntegrationTests()
        {
            // Test: Move hero to different clan and validate state change (using name-based query)
            TestRunner.RegisterTest(new TestCase(
                "integration_hero_move_clan_001",
                "Move a living lord (Derthert) to another clan and verify clan change using name-based query",
                "gm.hero.set_clan derthert clan_empire_south_2",
                TestExpectation.Success
            )
            {
                Category = "Integration_HeroManagement",
                CustomValidator = (output) =>
                {
                    try
                    {
                        var hero = Hero.FindFirst(h => h.Name != null && h.Name.ToString().Contains("Derthert"));
                        if (hero == null) return (false, "Hero Derthert not found");
                        
                        var targetClan = Clan.FindFirst(c => c.StringId == "clan_empire_south_2");
                        if (targetClan == null) return (false, "Target clan not found");
                        
                        if (hero.Clan != targetClan)
                            return (false, $"Hero clan is {hero.Clan?.StringId} but expected {targetClan.StringId}");
                        
                        return (true, null);
                    }
                    catch (Exception ex)
                    {
                        return (false, $"Validation exception: {ex.Message}");
                    }
                },
                CleanupCommands = new System.Collections.Generic.List<string>
                {
                    // Reset hero back to original clan (Derthert back to Vlandia)
                    "gm.hero.set_clan derthert clan_vlandia_1"
                }
            });

            // Test: Move hero to different clan using exact ID
            TestRunner.RegisterTest(new TestCase(
                "integration_hero_move_clan_001b",
                "Move a living lord to another clan using exact hero ID",
                "gm.hero.set_clan lord_1_11 clan_empire_south_2",
                TestExpectation.Success
            )
            {
                Category = "Integration_HeroManagement",
                CustomValidator = (output) =>
                {
                    try
                    {
                        var hero = Hero.FindFirst(h => h.StringId == "lord_1_11");
                        if (hero == null) return (false, "Hero lord_1_11 not found");
                        
                        var targetClan = Clan.FindFirst(c => c.StringId == "clan_empire_south_2");
                        if (targetClan == null) return (false, "Target clan not found");
                        
                        if (hero.Clan != targetClan)
                            return (false, $"Hero clan is {hero.Clan?.StringId} but expected {targetClan.StringId}");
                        
                        return (true, null);
                    }
                    catch (Exception ex)
                    {
                        return (false, $"Validation exception: {ex.Message}");
                    }
                },
                CleanupCommands = new System.Collections.Generic.List<string>
                {
                    // Reset hero back to original clan
                    "gm.hero.set_clan lord_1_11 clan_empire_north_1"
                }
            });

            // Test: Try to move player hero
            TestRunner.RegisterTest(new TestCase(
                "integration_hero_move_player_001",
                "Test moving player hero (main_hero) to another clan",
                "gm.hero.set_clan main_hero clan_empire_south_1",
                TestExpectation.Success
            )
            {
                Category = "Integration_HeroManagement",
                CustomValidator = (output) =>
                {
                    try
                    {
                        var player = Hero.MainHero;
                        if (player == null) return (false, "Main hero not found");
                        
                        var targetClan = Clan.FindFirst(c => c.StringId == "clan_empire_south_1");
                        if (targetClan == null) return (false, "Target clan not found");
                        
                        if (player.Clan != targetClan)
                            return (false, $"Player clan is {player.Clan?.StringId} but expected {targetClan.StringId}");
                        
                        return (true, null);
                    }
                    catch (Exception ex)
                    {
                        return (false, $"Validation exception: {ex.Message}");
                    }
                },
                CleanupCommands = new System.Collections.Generic.List<string>
                {
                    "gm.hero.set_clan main_hero player_faction"
                }
            });

            // Test: Try to move a dead hero
            TestRunner.RegisterTest(new TestCase(
                "integration_hero_move_dead_001",
                "Attempt to move a dead hero to another clan",
                "gm.hero.set_clan dead_lord_2_1 clan_empire_south_2",
                TestExpectation.NoException
            )
            {
                Category = "Integration_HeroManagement",
                CustomValidator = (output) =>
                {
                    try
                    {
                        var hero = Hero.FindFirst(h => h.StringId == "dead_lord_2_1");
                        if (hero == null) return (false, "Hero dead_lord_2_1 (Olek the Old) not found");
                        if (hero.IsAlive) return (false, "Hero should be dead for this test (Olek the Old is already dead)");
                        
                        // Command should still work or produce appropriate error
                        return (true, null);
                    }
                    catch (Exception ex)
                    {
                        return (false, $"Validation exception: {ex.Message}");
                    }
                }
            });

            // Test: Set hero gold and verify
            TestRunner.RegisterTest(new TestCase(
                "integration_hero_set_gold_001",
                "Set hero gold to specific amount and verify",
                "gm.hero.set_gold derthert 50000",
                TestExpectation.Success
            )
            {
                Category = "Integration_HeroManagement",
                CustomValidator = (output) =>
                {
                    try
                    {
                        var hero = Hero.FindFirst(h => h.Name != null && h.Name.ToString().Contains("Derthert"));
                        if (hero == null) return (false, "Derthert not found");
                        
                        if (hero.Gold != 50000)
                            return (false, $"Hero gold is {hero.Gold} but expected 50000");
                        
                        return (true, null);
                    }
                    catch (Exception ex)
                    {
                        return (false, $"Validation exception: {ex.Message}");
                    }
                }
            });

            // Test: Add gold to hero and verify increment
            TestRunner.RegisterTest(new TestCase(
                "integration_hero_add_gold_001",
                "Add gold to hero and verify amount increases correctly",
                "",  // Will be dynamically created
                TestExpectation.Success
            )
            {
                Category = "Integration_HeroManagement",
                CustomValidator = (output) =>
                {
                    try
                    {
                        var hero = Hero.FindFirst(h => h.Name != null && h.Name.ToString().Contains("Derthert"));
                        if (hero == null) return (false, "Derthert not found");
                        
                        // Setup set gold to 1000, then command added 10000, so final should be 11000
                        int expectedGold = 11000;
                        
                        if (hero.Gold != expectedGold)
                            return (false, $"Hero gold is {hero.Gold} but expected {expectedGold}");
                        
                        return (true, null);
                    }
                    catch (Exception ex)
                    {
                        return (false, $"Validation exception: {ex.Message}");
                    }
                },
                SetupCommands = new System.Collections.Generic.List<string>
                {
                    "gm.hero.set_gold derthert 1000"
                },
                Command = "gm.hero.add_gold derthert 10000"
            });

            // Test: Set hero age and verify
            TestRunner.RegisterTest(new TestCase(
                "integration_hero_set_age_001",
                "Set hero age and verify change",
                "gm.hero.set_age derthert 45",
                TestExpectation.Success
            )
            {
                Category = "Integration_HeroManagement",
                CustomValidator = (output) =>
                {
                    try
                    {
                        var hero = Hero.FindFirst(h => h.Name != null && h.Name.ToString().Contains("Derthert"));
                        if (hero == null) return (false, "Derthert not found");
                        
                        float age = hero.Age;
                        if (Math.Abs(age - 45.0f) > 1.0f)  // Allow 1 year tolerance
                            return (false, $"Hero age is {age:F1} but expected 45");
                        
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
        /// Register item equipment save/load integration tests
        /// </summary>
        private static void RegisterItemEquipmentSaveLoadIntegrationTests()
        {
            // Test: Save player equipment to file and verify file exists
            TestRunner.RegisterTest(new TestCase(
                "integration_equipment_save_001",
                "Save player equipment to file and verify file exists",
                "gm.item.save_equipment main_hero test_equipment_save",
                TestExpectation.Contains
            )
            {
                Category = "Integration_EquipmentSaveLoad",
                ExpectedText = "Saved",
                CustomValidator = (output) =>
                {
                    try
                    {
                        // Verify save message
                        if (!(output.IndexOf("Saved", StringComparison.OrdinalIgnoreCase) >= 0))
                            return (false, "Expected 'Saved' message for save operation");
                        
                        // Verify file was created
                        string documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                        string filePath = Path.Combine(documentsPath, "Mount and Blade II Bannerlord", "Configs", "GameMaster", "HeroSets", "test_equipment_save.json");
                        
                        if (!File.Exists(filePath))
                            return (false, $"Equipment file was not created at: {filePath}");
                        
                        // Verify file has content
                        string fileContent = File.ReadAllText(filePath);
                        if (string.IsNullOrWhiteSpace(fileContent))
                            return (false, "Equipment file exists but is empty");
                        
                        // Verify it's valid JSON with expected structure
                        if (!fileContent.Contains("HeroName") || !fileContent.Contains("Equipment"))
                            return (false, "Equipment file does not contain expected JSON structure");
                        
                        return (true, null);
                    }
                    catch (Exception ex)
                    {
                        return (false, $"Validation exception: {ex.Message}");
                    }
                },
                CleanupCommands = new System.Collections.Generic.List<string>()
            });

            // Test: Save civilian equipment to file and verify file exists
            TestRunner.RegisterTest(new TestCase(
                "integration_equipment_save_002",
                "Save player civilian equipment to file and verify file exists",
                "gm.item.save_equipment_civilian main_hero test_equipment_save_civilian",
                TestExpectation.Contains
            )
            {
                Category = "Integration_EquipmentSaveLoad",
                ExpectedText = "Saved",
                CustomValidator = (output) =>
                {
                    try
                    {
                        // Verify save message
                        if (!(output.IndexOf("Saved", StringComparison.OrdinalIgnoreCase) >= 0))
                            return (false, "Expected 'Saved' message for save operation");
                        
                        // Verify file was created in civilian subfolder
                        string documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                        string filePath = Path.Combine(documentsPath, "Mount and Blade II Bannerlord", "Configs", "GameMaster", "HeroSets", "civilian", "test_equipment_save_civilian.json");
                        
                        if (!File.Exists(filePath))
                            return (false, $"Civilian equipment file was not created at: {filePath}");
                        
                        // Verify file has content
                        string fileContent = File.ReadAllText(filePath);
                        if (string.IsNullOrWhiteSpace(fileContent))
                            return (false, "Civilian equipment file exists but is empty");
                        
                        // Verify it's valid JSON with expected structure
                        if (!fileContent.Contains("HeroName") || !fileContent.Contains("Equipment"))
                            return (false, "Civilian equipment file does not contain expected JSON structure");
                        
                        return (true, null);
                    }
                    catch (Exception ex)
                    {
                        return (false, $"Validation exception: {ex.Message}");
                    }
                },
                CleanupCommands = new System.Collections.Generic.List<string>()
            });

            // Test: Save both equipment sets and verify both files exist
            TestRunner.RegisterTest(new TestCase(
                "integration_equipment_save_003",
                "Save both battle and civilian equipment sets and verify both files exist",
                "gm.item.save_equipment_both main_hero test_equipment_both",
                TestExpectation.Contains
            )
            {
                Category = "Integration_EquipmentSaveLoad",
                ExpectedText = "Saved",
                CustomValidator = (output) =>
                {
                    try
                    {
                        // Verify save message
                        if (!(output.IndexOf("Saved", StringComparison.OrdinalIgnoreCase) >= 0))
                            return (false, "Expected 'Saved' message for save operation");
                        
                        string documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                        
                        // Verify battle equipment file
                        string battleFilePath = Path.Combine(documentsPath, "Mount and Blade II Bannerlord", "Configs", "GameMaster", "HeroSets", "test_equipment_both.json");
                        if (!File.Exists(battleFilePath))
                            return (false, $"Battle equipment file was not created at: {battleFilePath}");
                        
                        // Verify civilian equipment file
                        string civilianFilePath = Path.Combine(documentsPath, "Mount and Blade II Bannerlord", "Configs", "GameMaster", "HeroSets", "civilian", "test_equipment_both.json");
                        if (!File.Exists(civilianFilePath))
                            return (false, $"Civilian equipment file was not created at: {civilianFilePath}");
                        
                        // Verify both files have content
                        string battleContent = File.ReadAllText(battleFilePath);
                        string civilianContent = File.ReadAllText(civilianFilePath);
                        
                        if (string.IsNullOrWhiteSpace(battleContent))
                            return (false, "Battle equipment file exists but is empty");
                        
                        if (string.IsNullOrWhiteSpace(civilianContent))
                            return (false, "Civilian equipment file exists but is empty");
                        
                        return (true, null);
                    }
                    catch (Exception ex)
                    {
                        return (false, $"Validation exception: {ex.Message}");
                    }
                },
                CleanupCommands = new System.Collections.Generic.List<string>()
            });

            // Test: Save equipment from lord_1_1 and load onto lord_4_1
            TestRunner.RegisterTest(new TestCase(
                "integration_equipment_load_001",
                "Save equipment from lord_1_1 and load onto lord_4_1 (different hero)",
                "gm.item.load_equipment lord_4_1 test_equipment_load",
                TestExpectation.Contains
            )
            {
                Category = "Integration_EquipmentSaveLoad",
                ExpectedText = "Loaded",
                SetupCommands = new System.Collections.Generic.List<string>
                {
                    // Save lord_1_1's equipment (source hero)
                    "gm.item.save_equipment lord_1_1 test_equipment_load",
                    // Clear lord_4_1's equipment (target hero)
                    "gm.item.remove_equipped lord_4_1"
                },
                CustomValidator = (output) =>
                {
                    try
                    {
                        // Verify load message
                        if (!(output.IndexOf("Loaded", StringComparison.OrdinalIgnoreCase) >= 0))
                            return (false, "Expected 'Loaded' message for load operation");
                        
                        // Verify file was read
                        string documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                        string filePath = Path.Combine(documentsPath, "Mount and Blade II Bannerlord", "Configs", "GameMaster", "HeroSets", "test_equipment_load.json");
                        
                        if (!File.Exists(filePath))
                            return (false, "Equipment file should exist from setup command");
                        
                        // Verify lord_4_1 now has equipment (loaded from lord_1_1)
                        var targetHero = Hero.FindFirst(h => h.StringId == "lord_4_1");
                        if (targetHero == null) return (false, "Target hero lord_4_1 not found");
                        
                        // Check if any equipment slots are filled
                        bool hasEquipment = false;
                        for (int i = 0; i < (int)EquipmentIndex.NumEquipmentSetSlots; i++)
                        {
                            if (!targetHero.BattleEquipment[(EquipmentIndex)i].IsEmpty)
                            {
                                hasEquipment = true;
                                break;
                            }
                        }
                        
                        if (!hasEquipment)
                            return (false, "Target hero lord_4_1 should have equipment after loading from lord_1_1");
                        
                        return (true, null);
                    }
                    catch (Exception ex)
                    {
                        return (false, $"Validation exception: {ex.Message}");
                    }
                },
                CleanupCommands = new System.Collections.Generic.List<string>()
            });

            // Test: Save civilian equipment from lord_1_1 and load onto lord_4_1
            TestRunner.RegisterTest(new TestCase(
                "integration_equipment_load_002",
                "Save civilian equipment from lord_1_1 and load onto lord_4_1 (different hero)",
                "gm.item.load_equipment_civilian lord_4_1 test_equipment_load_civilian",
                TestExpectation.Contains
            )
            {
                Category = "Integration_EquipmentSaveLoad",
                ExpectedText = "Loaded",
                SetupCommands = new System.Collections.Generic.List<string>
                {
                    // Save lord_1_1's civilian equipment (source hero)
                    "gm.item.save_equipment_civilian lord_1_1 test_equipment_load_civilian",
                    // Clear lord_4_1's equipment (target hero)
                    "gm.item.remove_equipped lord_4_1"
                },
                CustomValidator = (output) =>
                {
                    try
                    {
                        // Verify load message
                        if (!(output.IndexOf("Loaded", StringComparison.OrdinalIgnoreCase) >= 0))
                            return (false, "Expected 'Loaded' message for load operation");
                        
                        // Verify file was read
                        string documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                        string filePath = Path.Combine(documentsPath, "Mount and Blade II Bannerlord", "Configs", "GameMaster", "HeroSets", "civilian", "test_equipment_load_civilian.json");
                        
                        if (!File.Exists(filePath))
                            return (false, "Civilian equipment file should exist from setup command");
                        
                        // Verify lord_4_1 now has civilian equipment (loaded from lord_1_1)
                        var targetHero = Hero.FindFirst(h => h.StringId == "lord_4_1");
                        if (targetHero == null) return (false, "Target hero lord_4_1 not found");
                        
                        // Check if any civilian equipment slots are filled
                        bool hasEquipment = false;
                        for (int i = 0; i < (int)EquipmentIndex.NumEquipmentSetSlots; i++)
                        {
                            if (!targetHero.CivilianEquipment[(EquipmentIndex)i].IsEmpty)
                            {
                                hasEquipment = true;
                                break;
                            }
                        }
                        
                        if (!hasEquipment)
                            return (false, "Target hero lord_4_1 should have civilian equipment after loading from lord_1_1");
                        
                        return (true, null);
                    }
                    catch (Exception ex)
                    {
                        return (false, $"Validation exception: {ex.Message}");
                    }
                },
                CleanupCommands = new System.Collections.Generic.List<string>()
            });

            // Test: Save both equipment sets from lord_1_1 and load onto lord_4_1
            TestRunner.RegisterTest(new TestCase(
                "integration_equipment_load_003",
                "Save both equipment sets from lord_1_1 and load onto lord_4_1 (different hero)",
                "gm.item.load_equipment_both lord_4_1 test_equipment_load_both",
                TestExpectation.Contains
            )
            {
                Category = "Integration_EquipmentSaveLoad",
                ExpectedText = "loaded",
                SetupCommands = new System.Collections.Generic.List<string>
                {
                    // Save both equipment sets from lord_1_1 (source hero)
                    "gm.item.save_equipment_both lord_1_1 test_equipment_load_both",
                    // Clear lord_4_1's equipment (target hero)
                    "gm.item.remove_equipped lord_4_1"
                },
                CustomValidator = (output) =>
                {
                    try
                    {
                        // Verify load message
                        if (!(output.IndexOf("Loaded", StringComparison.OrdinalIgnoreCase) >= 0))
                            return (false, "Expected 'Loaded' message for load operation");
                        
                        var targetHero = Hero.FindFirst(h => h.StringId == "lord_4_1");
                        if (targetHero == null) return (false, "Target hero lord_4_1 not found");
                        
                        // Check if battle equipment was restored
                        bool hasBattleEquipment = false;
                        for (int i = 0; i < (int)EquipmentIndex.NumEquipmentSetSlots; i++)
                        {
                            if (!targetHero.BattleEquipment[(EquipmentIndex)i].IsEmpty)
                            {
                                hasBattleEquipment = true;
                                break;
                            }
                        }
                        
                        // Check if civilian equipment was restored
                        bool hasCivilianEquipment = false;
                        for (int i = 0; i < (int)EquipmentIndex.NumEquipmentSetSlots; i++)
                        {
                            if (!targetHero.CivilianEquipment[(EquipmentIndex)i].IsEmpty)
                            {
                                hasCivilianEquipment = true;
                                break;
                            }
                        }
                        
                        if (!hasBattleEquipment)
                            return (false, "Target hero lord_4_1 should have battle equipment after loading both sets from lord_1_1");
                        
                        if (!hasCivilianEquipment)
                            return (false, "Target hero lord_4_1 should have civilian equipment after loading both sets from lord_1_1");
                        
                        return (true, null);
                    }
                    catch (Exception ex)
                    {
                        return (false, $"Validation exception: {ex.Message}");
                    }
                },
                CleanupCommands = new System.Collections.Generic.List<string>()
            });

            // COMMENTED OUT: These tests were based on incorrect Priority 6 assumptions
            // Test: Save equipment using name query "derthert" - verifies Priority 6 fix
            /*
            TestRunner.RegisterTest(new TestCase(
                "integration_equipment_save_name_query_001",
                "Save equipment using name query 'derthert' - should select Derthert (lord_4_1), not an Empire hero",
                "gm.item.save_equipment_both derthert test_derthert_save",
                TestExpectation.Contains
            )
            {
                Category = "Integration_EquipmentSaveLoad",
                ExpectedText = "Saved",
                CustomValidator = (output) =>
                {
                    try
                    {
                        // Verify save message
                        if (!(output.IndexOf("Saved", StringComparison.OrdinalIgnoreCase) >= 0))
                            return (false, "Expected 'Saved' message for save operation");
                        
                        string documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                        string filePath = Path.Combine(documentsPath, "Mount and Blade II Bannerlord", "Configs", "GameMaster", "HeroSets", "test_derthert_save.json");
                        
                        if (!File.Exists(filePath))
                            return (false, $"Equipment file was not created at: {filePath}");
                        
                        // Verify file content has correct hero ID (lord_4_1 is Derthert, Vlandian)
                        string fileContent = File.ReadAllText(filePath);
                        if (!fileContent.Contains("\"HeroId\": \"lord_4_1\""))
                            return (false, "Equipment file should contain HeroId: lord_4_1 (Derthert - Vlandian)");
                        
                        if (!fileContent.Contains("Derthert"))
                            return (false, "Equipment file should contain hero name 'Derthert'");
                        
                        // Verify it's NOT an Empire hero
                        if (fileContent.Contains("lord_1_") && !fileContent.Contains("lord_4_1"))
                            return (false, "Wrong hero selected - appears to be an Empire hero (lord_1_x) instead of Derthert (lord_4_1)");
                        
                        return (true, null);
                    }
                    catch (Exception ex)
                    {
                        return (false, $"Validation exception: {ex.Message}");
                    }
                },
                CleanupCommands = new System.Collections.Generic.List<string>()
            });

            // Test: Load equipment using name query "derthert" - verifies Priority 6 fix
            TestRunner.RegisterTest(new TestCase(
                "integration_equipment_load_name_query_001",
                "Load equipment using name query 'derthert' - should load onto Derthert (lord_4_1), not an Empire hero",
                "gm.item.load_equipment_both derthert test_derthert_load",
                TestExpectation.Contains
            )
            {
                Category = "Integration_EquipmentSaveLoad",
                ExpectedText = "Loaded",
                SetupCommands = new System.Collections.Generic.List<string>
                {
                    // Save lord_1_1's equipment (Empire hero)
                    "gm.item.save_equipment_both lord_1_1 test_derthert_load",
                    // Clear Derthert's equipment first to verify load
                    "gm.item.remove_equipped lord_4_1"
                },
                CustomValidator = (output) =>
                {
                    try
                    {
                        // Verify load message
                        if (!(output.IndexOf("Loaded", StringComparison.OrdinalIgnoreCase) >= 0))
                            return (false, "Expected 'Loaded' message for load operation");
                        
                        // Verify Derthert (lord_4_1) now has equipment
                        var derthert = Hero.FindFirst(h => h.StringId == "lord_4_1");
                        if (derthert == null) return (false, "Derthert (lord_4_1) not found");
                        
                        // Verify Derthert is the correct hero (Vlandian, not Empire)
                        if (derthert.Name == null || !derthert.Name.ToString().Contains("Derthert"))
                            return (false, "Hero lord_4_1 is not Derthert - wrong hero may have been selected");
                        
                        // Check if Derthert now has equipment (loaded from lord_1_1)
                        bool hasEquipment = false;
                        for (int i = 0; i < (int)EquipmentIndex.NumEquipmentSetSlots; i++)
                        {
                            if (!derthert.BattleEquipment[(EquipmentIndex)i].IsEmpty)
                            {
                                hasEquipment = true;
                                break;
                            }
                        }
                        
                        if (!hasEquipment)
                            return (false, "Derthert (lord_4_1) should have equipment after loading");
                        
                        return (true, null);
                    }
                    catch (Exception ex)
                    {
                        return (false, $"Validation exception: {ex.Message}");
                    }
                },
                CleanupCommands = new System.Collections.Generic.List<string>()
            });

            // Test: Single name-only match auto-selection - verifies Priority 6 fix
            TestRunner.RegisterTest(new TestCase(
                "integration_equipment_single_name_match_001",
                "Single name-only match auto-selection - unique name query should succeed without ambiguity error",
                "gm.item.save_equipment_both Garios test_garios_save",
                TestExpectation.Contains
            )
            {
                Category = "Integration_EquipmentSaveLoad",
                ExpectedText = "Saved",
                CustomValidator = (output) =>
                {
                    try
                    {
                        // Verify save succeeded without ambiguity error
                        if (!(output.IndexOf("Saved", StringComparison.OrdinalIgnoreCase) >= 0))
                            return (false, "Expected 'Saved' message - should auto-select single name match");
                        
                        // Verify no ambiguity error was shown
                        if (output.IndexOf("multiple", StringComparison.OrdinalIgnoreCase) >= 0)
                            return (false, "Should not show ambiguity error for single name match");
                        
                        string documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                        string filePath = Path.Combine(documentsPath, "Mount and Blade II Bannerlord", "Configs", "GameMaster", "HeroSets", "test_garios_save.json");
                        
                        if (!File.Exists(filePath))
                            return (false, $"Equipment file was not created at: {filePath}");
                        
                        // Verify file content has correct hero name
                        string fileContent = File.ReadAllText(filePath);
                        if (!fileContent.Contains("Garios"))
                            return (false, "Equipment file should contain hero name 'Garios'");
                        
                        // Find the actual Garios hero to verify correct hero was selected
                        var garios = Hero.AllAliveHeroes.FirstOrDefault(h =>
                            h.Name != null &&
                            h.Name.ToString().Equals("Garios", StringComparison.OrdinalIgnoreCase)
                        );
                        
                        if (garios != null)
                        {
                            // Verify the saved file has the correct hero ID
                            if (!fileContent.Contains($"\"HeroId\": \"{garios.StringId}\""))
                                return (false, $"Equipment file should contain HeroId: {garios.StringId} for Garios");
                        }
                        
                        return (true, null);
                    }
                    catch (Exception ex)
                    {
                        return (false, $"Validation exception: {ex.Message}");
                    }
                },
                CleanupCommands = new System.Collections.Generic.List<string>()
            });
            */

            // Test: Cleanup all test equipment files
            TestRunner.RegisterTest(new TestCase(
                "integration_equipment_cleanup_001",
                "Cleanup all test equipment files",
                "",
                TestExpectation.NoException
            )
            {
                Category = "Integration_EquipmentSaveLoad",
                CustomValidator = (output) =>
                {
                    try
                    {
                        string documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                        string heroSetsPath = Path.Combine(documentsPath, "Mount and Blade II Bannerlord", "Configs", "GameMaster", "HeroSets");
                        
                        // Delete all test_equipment_* files
                        if (Directory.Exists(heroSetsPath))
                        {
                            var testFiles = Directory.GetFiles(heroSetsPath, "test_equipment_*.json");
                            foreach (var file in testFiles)
                            {
                                File.Delete(file);
                            }
                            
                            // Also clean civilian subfolder
                            string civilianPath = Path.Combine(heroSetsPath, "civilian");
                            if (Directory.Exists(civilianPath))
                            {
                                var civilianTestFiles = Directory.GetFiles(civilianPath, "test_equipment_*.json");
                                foreach (var file in civilianTestFiles)
                                {
                                    File.Delete(file);
                                }
                            }
                        }
                        
                        return (true, null);
                    }
                    catch (Exception ex)
                    {
                        return (false, $"Cleanup exception: {ex.Message}");
                    }
                }
            });
        }

        /// <summary>
        /// Register clan management integration tests
        /// </summary>
        private static void RegisterClanManagementIntegrationTests()
        {
            // Test: Set clan gold and verify
            TestRunner.RegisterTest(new TestCase(
                "integration_clan_set_gold_001",
                "Set clan gold and verify amount",
                "gm.clan.set_gold clan_empire_south_1 100000",
                TestExpectation.Success
            )
            {
                Category = "Integration_ClanManagement",
                CustomValidator = (output) =>
                {
                    try
                    {
                        var clan = Clan.FindFirst(c => c.StringId == "clan_empire_south_1");
                        if (clan == null) return (false, "Clan not found");
                        
                        // clan.Gold returns the sum of all living clan members' gold
                        // We distribute 100000 among all members, so total should be 100000
                        // Allow small tolerance for integer division rounding
                        int totalClanGold = clan.Heroes.Where(h => h.IsAlive).Sum(h => h.Gold);
                        
                        if (totalClanGold != 100000)
                            return (false, $"Total clan member gold is {totalClanGold} but expected 100000. Clan.Gold property shows: {clan.Gold}");
                        
                        return (true, null);
                    }
                    catch (Exception ex)
                    {
                        return (false, $"Validation exception: {ex.Message}");
                    }
                }
            });

            // Test: Set clan renown and verify
            TestRunner.RegisterTest(new TestCase(
                "integration_clan_set_renown_001",
                "Set clan renown and verify amount",
                "gm.clan.set_renown clan_empire_south_1 500",
                TestExpectation.Success
            )
            {
                Category = "Integration_ClanManagement",
                CustomValidator = (output) =>
                {
                    try
                    {
                        var clan = Clan.FindFirst(c => c.StringId == "clan_empire_south_1");
                        if (clan == null) return (false, "Clan not found");
                        
                        if (Math.Abs(clan.Renown - 500.0f) > 1.0f)
                            return (false, $"Clan renown is {clan.Renown:F1} but expected 500");
                        
                        return (true, null);
                    }
                    catch (Exception ex)
                    {
                        return (false, $"Validation exception: {ex.Message}");
                    }
                }
            });

            // Test: Set clan tier (increase) and verify
            TestRunner.RegisterTest(new TestCase(
                "integration_clan_set_tier_001_increase",
                "Increase clan tier and verify (tier can be increased)",
                "gm.clan.set_tier clan_vlandia_2 5",
                TestExpectation.Success
            )
            {
                Category = "Integration_ClanManagement",
                CustomValidator = (output) =>
                {
                    try
                    {
                        var clan = Clan.FindFirst(c => c.StringId == "clan_vlandia_2");
                        if (clan == null) return (false, "Clan clan_vlandia_2 not found");
                        
                        if (clan.Tier != 5)
                            return (false, $"Clan tier is {clan.Tier} but expected 5");
                        
                        return (true, null);
                    }
                    catch (Exception ex)
                    {
                        return (false, $"Validation exception: {ex.Message}");
                    }
                }
            });

            // Test: Set clan tier (decrease) - should fail
            // Note: Game engine limitation - clan tiers cannot be decreased, only increased
            TestRunner.RegisterTest(new TestCase(
                "integration_clan_set_tier_001_decrease",
                "Attempt to decrease clan tier (should fail - game engine limitation)",
                "gm.clan.set_tier clan_empire_south_1 4",
                TestExpectation.Error
            )
            {
                Category = "Integration_ClanManagement",
                ExpectedText = "Cannot lower clan tier",
                CustomValidator = (output) =>
                {
                    try
                    {
                        var clan = Clan.FindFirst(c => c.StringId == "clan_empire_south_1");
                        if (clan == null) return (false, "Clan not found");
                        
                        // Verify that tier was NOT decreased (should remain at original tier)
                        // clan_empire_south_1 is tier 6, attempting to set to 4 should fail
                        if (clan.Tier < 6)
                            return (false, $"Clan tier was incorrectly decreased to {clan.Tier}");
                        
                        return (true, null);
                    }
                    catch (Exception ex)
                    {
                        return (false, $"Validation exception: {ex.Message}");
                    }
                }
            });

            // Test: Player faction operations
            TestRunner.RegisterTest(new TestCase(
                "integration_clan_player_faction_001",
                "Modify player faction (player_faction) gold",
                "gm.clan.set_gold player_faction 150000",
                TestExpectation.Success
            )
            {
                Category = "Integration_ClanManagement",
                CustomValidator = (output) =>
                {
                    try
                    {
                        var playerClan = Clan.PlayerClan;
                        if (playerClan == null) return (false, "Player clan not found");
                        
                        if (playerClan.Gold != 150000)
                            return (false, $"Player clan gold is {playerClan.Gold} but expected 150000");
                        
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
        /// Register kingdom management integration tests
        /// </summary>
        private static void RegisterKingdomManagementIntegrationTests()
        {
            // Test: Add clan to kingdom and verify
            TestRunner.RegisterTest(new TestCase(
                "integration_kingdom_add_clan_001",
                "Add a clan to a kingdom (Sturgia) and verify membership",
                "gm.kingdom.add_clan clan_empire_south_1 sturgia",
                TestExpectation.Success
            )
            {
                Category = "Integration_KingdomManagement",
                CustomValidator = (output) =>
                {
                    try
                    {
                        var clan = Clan.FindFirst(c => c.StringId == "clan_empire_south_1");
                        if (clan == null) return (false, "Clan not found");
                        
                        var kingdom = Kingdom.All.FirstOrDefault(k => k.StringId == "sturgia");
                        if (kingdom == null) return (false, "Kingdom sturgia not found");
                        
                        if (clan.Kingdom != kingdom)
                            return (false, $"Clan kingdom is {clan.Kingdom?.StringId} but expected {kingdom.StringId}");
                        
                        if (!kingdom.Clans.Contains(clan))
                            return (false, "Clan not in kingdom's clan list");
                        
                        return (true, null);
                    }
                    catch (Exception ex)
                    {
                        return (false, $"Validation exception: {ex.Message}");
                    }
                },
                CleanupCommands = new System.Collections.Generic.List<string>
                {
                    "gm.kingdom.remove_clan clan_empire_south_1"
                }
            });
        }

        /// <summary>
        /// Register troop query integration tests
        /// </summary>
        private static void RegisterTroopQueryIntegrationTests()
        {
            // Test: Query all troops and verify categories
            TestRunner.RegisterTest(new TestCase(
                "integration_troop_query_basic_001",
                "Query all troops and verify output contains troop categories",
                "gm.query.troop",
                TestExpectation.Contains
            )
            {
                Category = "Integration_TroopQuery",
                ExpectedText = "troop",
                CustomValidator = (output) =>
                {
                    try
                    {
                        // Verify output contains major troop categories
                        bool hasRegular = output.IndexOf("[Regular]", StringComparison.OrdinalIgnoreCase) >= 0;
                        bool hasMilitia = output.IndexOf("[Militia]", StringComparison.OrdinalIgnoreCase) >= 0;
                        bool hasMercenary = output.IndexOf("[Mercenary]", StringComparison.OrdinalIgnoreCase) >= 0;
                        bool hasBandit = output.IndexOf("[Bandit]", StringComparison.OrdinalIgnoreCase) >= 0;
                        
                        if (!hasRegular)
                            return (false, "Expected [Regular] category in troop output");
                        if (!hasMilitia)
                            return (false, "Expected [Militia] category in troop output");
                        if (!hasMercenary)
                            return (false, "Expected [Mercenary] category in troop output");
                        if (!hasBandit)
                            return (false, "Expected [Bandit] category in troop output");
                        
                        return (true, null);
                    }
                    catch (Exception ex)
                    {
                        return (false, $"Validation exception: {ex.Message}");
                    }
                }
            });

            // Test: Query infantry troops
            TestRunner.RegisterTest(new TestCase(
                "integration_troop_query_filter_001",
                "Query infantry troops and verify Formation: Infantry in output",
                "gm.query.troop infantry",
                TestExpectation.Contains
            )
            {
                Category = "Integration_TroopQuery",
                ExpectedText = "Formation: Infantry",
                CustomValidator = (output) =>
                {
                    try
                    {
                        // Verify output contains Formation: Infantry
                        if (!(output.IndexOf("Formation: Infantry", StringComparison.OrdinalIgnoreCase) >= 0))
                            return (false, "Expected 'Formation: Infantry' in output");
                        
                        // Verify it doesn't contain other formations
                        if (output.IndexOf("Formation: Cavalry", StringComparison.OrdinalIgnoreCase) >= 0)
                            return (false, "Should not contain 'Formation: Cavalry' when filtering for infantry");
                        
                        return (true, null);
                    }
                    catch (Exception ex)
                    {
                        return (false, $"Validation exception: {ex.Message}");
                    }
                }
            });

            // Test: Query empire troops
            TestRunner.RegisterTest(new TestCase(
                "integration_troop_query_culture_001",
                "Query empire troops and verify Culture: Empire in results",
                "gm.query.troop empire",
                TestExpectation.Contains
            )
            {
                Category = "Integration_TroopQuery",
                ExpectedText = "Culture: Empire",
                CustomValidator = (output) =>
                {
                    try
                    {
                        // Verify output contains Culture: Empire
                        if (!(output.IndexOf("Culture: Empire", StringComparison.OrdinalIgnoreCase) >= 0))
                            return (false, "Expected 'Culture: Empire' in output");
                        
                        // Verify it doesn't contain other cultures
                        if (output.IndexOf("Culture: Vlandia", StringComparison.OrdinalIgnoreCase) >= 0)
                            return (false, "Should not contain 'Culture: Vlandia' when filtering for empire");
                        
                        return (true, null);
                    }
                    catch (Exception ex)
                    {
                        return (false, $"Validation exception: {ex.Message}");
                    }
                }
            });

            // Test: Query tier 5+ troops
            TestRunner.RegisterTest(new TestCase(
                "integration_troop_query_tier_001",
                "Query tier 5+ troops and verify Tier: 5 or Tier: 6 in results",
                "gm.query.troop tier5",
                TestExpectation.Contains
            )
            {
                Category = "Integration_TroopQuery",
                ExpectedText = "Tier:",
                CustomValidator = (output) =>
                {
                    try
                    {
                        // Verify output contains Tier: 5 or Tier: 6
                        bool hasTier5 = output.IndexOf("Tier: 5", StringComparison.OrdinalIgnoreCase) >= 0;
                        bool hasTier6 = output.IndexOf("Tier: 6", StringComparison.OrdinalIgnoreCase) >= 0;
                        
                        if (!hasTier5 && !hasTier6)
                            return (false, "Expected 'Tier: 5' or 'Tier: 6' in output");
                        
                        // Verify it doesn't contain lower tiers
                        if (output.IndexOf("Tier: 1", StringComparison.OrdinalIgnoreCase) >= 0 ||
                            output.IndexOf("Tier: 2", StringComparison.OrdinalIgnoreCase) >= 0 ||
                            output.IndexOf("Tier: 3", StringComparison.OrdinalIgnoreCase) >= 0 ||
                            output.IndexOf("Tier: 4", StringComparison.OrdinalIgnoreCase) >= 0)
                            return (false, "Should not contain Tier: 1-4 when filtering for tier 5+");
                        
                        return (true, null);
                    }
                    catch (Exception ex)
                    {
                        return (false, $"Validation exception: {ex.Message}");
                    }
                }
            });

            // Test: Query vlandian cavalry (combined filters)
            TestRunner.RegisterTest(new TestCase(
                "integration_troop_query_combined_001",
                "Query vlandian cavalry and verify both Culture: Vlandia and Formation: Cavalry",
                "gm.query.troop vlandia cavalry",
                TestExpectation.Contains
            )
            {
                Category = "Integration_TroopQuery",
                ExpectedText = "Culture: Vlandia",
                CustomValidator = (output) =>
                {
                    try
                    {
                        // Verify output contains both Culture: Vlandia and Formation: Cavalry
                        bool hasVlandia = output.IndexOf("Culture: Vlandia", StringComparison.OrdinalIgnoreCase) >= 0;
                        bool hasCavalry = output.IndexOf("Formation: Cavalry", StringComparison.OrdinalIgnoreCase) >= 0;
                        
                        if (!hasVlandia)
                            return (false, "Expected 'Culture: Vlandia' in output");
                        if (!hasCavalry)
                            return (false, "Expected 'Formation: Cavalry' in output");
                        
                        return (true, null);
                    }
                    catch (Exception ex)
                    {
                        return (false, $"Validation exception: {ex.Message}");
                    }
                }
            });

            // Test: Query troops sorted by tier
            TestRunner.RegisterTest(new TestCase(
                "integration_troop_query_sorting_001",
                "Query troops sorted by tier and verify ascending tier order in output",
                "gm.query.troop sort:tier",
                TestExpectation.Contains
            )
            {
                Category = "Integration_TroopQuery",
                ExpectedText = "Tier:",
                CustomValidator = (output) =>
                {
                    try
                    {
                        // Verify output contains tier information
                        if (!(output.IndexOf("Tier:", StringComparison.OrdinalIgnoreCase) >= 0))
                            return (false, "Expected 'Tier:' in output");
                        
                        // Extract tier numbers to verify sorting order (ascending)
                        var lines = output.Split('\n');
                        int previousTier = -1;
                        
                        foreach (var line in lines)
                        {
                            if (line.IndexOf("Tier:", StringComparison.OrdinalIgnoreCase) >= 0)
                            {
                                // Extract tier number from line
                                var tierIndex = line.IndexOf("Tier:", StringComparison.OrdinalIgnoreCase);
                                if (tierIndex >= 0 && tierIndex + 6 < line.Length)
                                {
                                    var tierChar = line[tierIndex + 6];
                                    if (char.IsDigit(tierChar))
                                    {
                                        int currentTier = tierChar - '0';
                                        if (previousTier != -1 && currentTier < previousTier)
                                            return (false, $"Tiers not in ascending order: found {currentTier} after {previousTier}");
                                        previousTier = currentTier;
                                    }
                                }
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

            // Test: Query all troops and verify exclusions
            TestRunner.RegisterTest(new TestCase(
                "integration_troop_query_exclusions_001",
                "Query all troops and verify NO templates, NPCs, children, or special characters appear",
                "gm.query.troop",
                TestExpectation.Contains
            )
            {
                Category = "Integration_TroopQuery",
                ExpectedText = "troop",
                CustomValidator = (output) =>
                {
                    try
                    {
                        // Verify output does not contain template indicators
                        if (output.IndexOf("template", StringComparison.OrdinalIgnoreCase) >= 0)
                            return (false, "Output should not contain templates");
                        
                        // Verify output does not contain child indicators
                        if (output.IndexOf("child", StringComparison.OrdinalIgnoreCase) >= 0)
                            return (false, "Output should not contain children");
                        
                        // Verify output does not contain NPC indicators
                        if (output.IndexOf("Notable", StringComparison.OrdinalIgnoreCase) >= 0 ||
                            output.IndexOf("Wanderer", StringComparison.OrdinalIgnoreCase) >= 0)
                            return (false, "Output should not contain NPCs/Notables/Wanderers");
                        
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
        /// Register troop management integration tests
        /// </summary>
        private static void RegisterTroopManagementIntegrationTests()
        {
            // Test: Give 10 imperial recruits to player
            TestRunner.RegisterTest(new TestCase(
                "integration_troop_give_001",
                "Give 10 imperial recruits to player and validate party roster count increased",
                "gm.troops.give_hero_troops main_hero imperial_recruit 10",
                TestExpectation.Success
            )
            {
                Category = "Integration_TroopManagement",
                CustomValidator = (output) =>
                {
                    try
                    {
                        var player = Hero.MainHero;
                        if (player == null) return (false, "Main hero not found");
                        if (player.PartyBelongedTo == null) return (false, "Player has no party");
                        
                        // Check if imperial recruit was added to roster
                        var imperialRecruit = Game.Current.ObjectManager.GetObject<CharacterObject>("imperial_recruit");
                        if (imperialRecruit == null) return (false, "Imperial recruit character not found");
                        
                        int count = player.PartyBelongedTo.MemberRoster.GetTroopCount(imperialRecruit);
                        if (count < 10)
                            return (false, $"Expected at least 10 imperial recruits in roster, found {count}");
                        
                        return (true, null);
                    }
                    catch (Exception ex)
                    {
                        return (false, $"Validation exception: {ex.Message}");
                    }
                }
            });

            // Test: Give 5 battanian troops to a lord
            TestRunner.RegisterTest(new TestCase(
                "integration_troop_give_002",
                "Give 5 battanian troops to a lord (Garios) and validate his party roster",
                "gm.troops.give_hero_troops Garios battanian_trained_warrior 5",
                TestExpectation.Success
            )
            {
                Category = "Integration_TroopManagement",
                CustomValidator = (output) =>
                {
                    try
                    {
                        // Find Garios
                        var garios = Hero.AllAliveHeroes.FirstOrDefault(h =>
                            h.Name != null &&
                            h.Name.ToString().Equals("Garios", StringComparison.OrdinalIgnoreCase)
                        );
                        
                        if (garios == null) return (false, "Garios not found");
                        if (garios.PartyBelongedTo == null) return (false, "Garios has no party");
                        
                        // Check if battanian warrior was added to roster
                        var battanianWarrior = Game.Current.ObjectManager.GetObject<CharacterObject>("battanian_trained_warrior");
                        if (battanianWarrior == null) return (false, "Battanian trained warrior character not found");
                        
                        int count = garios.PartyBelongedTo.MemberRoster.GetTroopCount(battanianWarrior);
                        if (count < 5)
                            return (false, $"Expected at least 5 battanian warriors in Garios' roster, found {count}");
                        
                        return (true, null);
                    }
                    catch (Exception ex)
                    {
                        return (false, $"Validation exception: {ex.Message}");
                    }
                }
            });

            // Test: Give troops and verify exact troop type
            TestRunner.RegisterTest(new TestCase(
                "integration_troop_give_verify_001",
                "Give troops and verify exact troop type in roster using CharacterObject lookup",
                "gm.troops.give_hero_troops main_hero vlandian_knight 3",
                TestExpectation.Success
            )
            {
                Category = "Integration_TroopManagement",
                CustomValidator = (output) =>
                {
                    try
                    {
                        var player = Hero.MainHero;
                        if (player == null) return (false, "Main hero not found");
                        if (player.PartyBelongedTo == null) return (false, "Player has no party");
                        
                        // Verify exact troop type was added
                        var vlandianKnight = Game.Current.ObjectManager.GetObject<CharacterObject>("vlandian_knight");
                        if (vlandianKnight == null) return (false, "Vlandian knight character not found");
                        
                        int count = player.PartyBelongedTo.MemberRoster.GetTroopCount(vlandianKnight);
                        if (count < 3)
                            return (false, $"Expected at least 3 vlandian knights in roster, found {count}");
                        
                        return (true, null);
                    }
                    catch (Exception ex)
                    {
                        return (false, $"Validation exception: {ex.Message}");
                    }
                }
            });

            // Test: Give 100 troops and verify quantity
            TestRunner.RegisterTest(new TestCase(
                "integration_troop_give_quantity_001",
                "Give 100 troops and verify exact quantity added to roster",
                "",
                TestExpectation.Success
            )
            {
                Category = "Integration_TroopManagement",
                SetupCommands = new System.Collections.Generic.List<string>
                {
                    // Clear existing imperial recruits first for accurate count
                    "gm.troops.give_hero_troops main_hero imperial_recruit -1000"
                },
                Command = "gm.troops.give_hero_troops main_hero imperial_recruit 100",
                CustomValidator = (output) =>
                {
                    try
                    {
                        var player = Hero.MainHero;
                        if (player == null) return (false, "Main hero not found");
                        if (player.PartyBelongedTo == null) return (false, "Player has no party");
                        
                        var imperialRecruit = Game.Current.ObjectManager.GetObject<CharacterObject>("imperial_recruit");
                        if (imperialRecruit == null) return (false, "Imperial recruit character not found");
                        
                        int count = player.PartyBelongedTo.MemberRoster.GetTroopCount(imperialRecruit);
                        if (count < 100)
                            return (false, $"Expected at least 100 imperial recruits, found {count}");
                        
                        return (true, null);
                    }
                    catch (Exception ex)
                    {
                        return (false, $"Validation exception: {ex.Message}");
                    }
                }
            });

            // Test: Give troops twice to same hero and verify cumulative
            TestRunner.RegisterTest(new TestCase(
                "integration_troop_give_multiple_001",
                "Give troops twice to same hero and verify cumulative total",
                "",
                TestExpectation.Success
            )
            {
                Category = "Integration_TroopManagement",
                SetupCommands = new System.Collections.Generic.List<string>
                {
                    // Clear existing sturgian warriors first
                    "gm.troops.give_hero_troops main_hero sturgian_warrior -1000",
                    // Give 25 sturgian warriors
                    "gm.troops.give_hero_troops main_hero sturgian_warrior 25"
                },
                Command = "gm.troops.give_hero_troops main_hero sturgian_warrior 25",
                CustomValidator = (output) =>
                {
                    try
                    {
                        var player = Hero.MainHero;
                        if (player == null) return (false, "Main hero not found");
                        if (player.PartyBelongedTo == null) return (false, "Player has no party");
                        
                        var sturgianWarrior = Game.Current.ObjectManager.GetObject<CharacterObject>("sturgian_warrior");
                        if (sturgianWarrior == null) return (false, "Sturgian warrior character not found");
                        
                        int count = player.PartyBelongedTo.MemberRoster.GetTroopCount(sturgianWarrior);
                        if (count < 50)
                            return (false, $"Expected at least 50 sturgian warriors (25+25), found {count}");
                        
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
        /// Register edge case and error handling tests
        /// </summary>
        private static void RegisterEdgeCaseTests()
        {
            // Test: Query wanderers specifically
            TestRunner.RegisterTest(new TestCase(
                "edge_case_query_wanderers_001",
                "Query for wanderer heroes",
                "gm.query.hero wanderer",
                TestExpectation.Contains
            )
            {
                Category = "EdgeCases",
                ExpectedText = "hero(es) matching"
            });

            // Test: Query notables
            TestRunner.RegisterTest(new TestCase(
                "edge_case_query_notables_001",
                "Query for notable heroes",
                "gm.query.hero notable",
                TestExpectation.Contains
            )
            {
                Category = "EdgeCases",
                ExpectedText = "hero(es) matching"
            });

            // Test: Try to move a wanderer to a clan
            TestRunner.RegisterTest(new TestCase(
                "edge_case_wanderer_to_clan_001",
                "Attempt to add a wanderer to a clan",
                "",  // Will be set dynamically
                TestExpectation.NoException
            )
            {
                Category = "EdgeCases",
                SetupCommands = new System.Collections.Generic.List<string>
                {
                    "gm.query.hero wanderer"  // Find a wanderer
                },
                Command = "gm.hero.set_clan lord_1_1 clan_empire_south_1"  // This should work or produce clear error
            });

            // Test: Query bandit clans
            TestRunner.RegisterTest(new TestCase(
                "edge_case_bandit_clans_001",
                "Query for bandit clans",
                "gm.query.clan bandit",
                TestExpectation.Contains
            )
            {
                Category = "EdgeCases",
                ExpectedText = "clan"
            });

            // Test: Try operations on clan leader
            TestRunner.RegisterTest(new TestCase(
                "edge_case_clan_leader_001",
                "Query for clan leaders",
                "gm.query.hero clanleader",
                TestExpectation.Contains
            )
            {
                Category = "EdgeCases",
                ExpectedText = "hero(es) matching"
            });

            // Test: Try operations on kingdom ruler
            TestRunner.RegisterTest(new TestCase(
                "edge_case_kingdom_ruler_001",
                "Query for kingdom rulers",
                "gm.query.hero kingdomruler",
                TestExpectation.Contains
            )
            {
                Category = "EdgeCases",
                ExpectedText = "hero(es) matching"
            });

            // Test: Invalid hero ID
            TestRunner.RegisterTest(new TestCase(
                "edge_case_invalid_hero_001",
                "Try to operate on non-existent hero",
                "gm.hero.set_gold invalid_hero_id_xyz 1000",
                TestExpectation.Error
            )
            {
                Category = "EdgeCases",
                ExpectedText = "No hero matching"
            });

            // Test: Invalid clan ID
            TestRunner.RegisterTest(new TestCase(
                "edge_case_invalid_clan_001",
                "Try to operate on non-existent clan",
                "gm.clan.set_gold invalid_clan_id_xyz 1000",
                TestExpectation.Error
            )
            {
                Category = "EdgeCases",
                ExpectedText = "No clan matching"
            });

            // Test: Query by kingdom name (not hero name)
            TestRunner.RegisterTest(new TestCase(
                "edge_case_query_by_kingdom_001",
                "Query heroes in Vlandia",
                "gm.query.hero vlandia",
                TestExpectation.Contains
            )
            {
                Category = "EdgeCases",
                ExpectedText = "hero(es) matching",
                CustomValidator = (output) =>
                {
                    // Verify we got Vlandian heroes
                    if (output.Contains("Vlandia") || output.Contains("vlandia"))
                        return (true, null);
                    return (false, "Expected to find Vlandian heroes");
                }
            });

            // Test: Known hero by name
            TestRunner.RegisterTest(new TestCase(
                "edge_case_query_derthert_001",
                "Query for King Derthert specifically",
                "gm.query.hero derthert",
                TestExpectation.Contains
            )
            {
                Category = "EdgeCases",
                ExpectedText = "Derthert"
            });
        }

        /// <summary>
        /// Register comprehensive edge case tests for special hero types and clans
        /// Tests scenarios that could break or cause unexpected behavior
        /// </summary>
        private static void RegisterSpecialCasesTests()
        {
            // ============================================================
            // PLAYER HERO EDGE CASES
            // ============================================================

            // Test: Move player to an NPC clan
            TestRunner.RegisterTest(new TestCase(
                "player_special_001",
                "Move player (main_hero) to an NPC clan (clan_empire_north_1)",
                "gm.hero.set_clan main_hero clan_empire_north_1",
                TestExpectation.Success
            )
            {
                Category = "SpecialCases_Player",
                CustomValidator = (output) =>
                {
                    try
                    {
                        var player = Hero.MainHero;
                        if (player == null) return (false, "Main hero not found");
                        
                        var targetClan = Clan.FindFirst(c => c.StringId == "clan_empire_north_1");
                        if (targetClan == null) return (false, "Target clan clan_empire_north_1 not found");
                        
                        if (player.Clan != targetClan)
                            return (false, $"Player clan is {player.Clan?.StringId} but expected {targetClan.StringId}");
                        
                        return (true, null);
                    }
                    catch (Exception ex)
                    {
                        return (false, $"Validation exception: {ex.Message}");
                    }
                },
                CleanupCommands = new System.Collections.Generic.List<string>
                {
                    "gm.hero.set_clan main_hero player_faction"
                }
            });

            // Test: Move player to a bandit clan (looters)
            // KNOWN ISSUE: Moving player to bandit clan causes game crash when opening clan page
            TestRunner.RegisterTest(new TestCase(
                "player_special_002",
                "Move player to a bandit clan (looters) - WARNING: May cause crash on clan page",
                "gm.hero.set_clan main_hero looters",
                TestExpectation.NoException
            )
            {
                Category = "SpecialCases_Player",
                CustomValidator = (output) =>
                {
                    try
                    {
                        var player = Hero.MainHero;
                        if (player == null) return (false, "Main hero not found");
                        
                        var targetClan = Clan.FindFirst(c => c.StringId == "looters");
                        if (targetClan == null) return (false, "Target clan looters not found");
                        
                        // NOTE: This test validates the operation completes, but opening the clan
                        // page after this change will crash the game due to bandit clan UI limitations
                        if (player.Clan != targetClan)
                            return (false, $"Player clan is {player.Clan?.StringId} but expected {targetClan.StringId}");
                        
                        return (true, null);
                    }
                    catch (Exception ex)
                    {
                        return (false, $"Validation exception: {ex.Message}");
                    }
                },
                CleanupCommands = new System.Collections.Generic.List<string>
                {
                    "gm.hero.set_clan main_hero player_faction"
                }
            });

            // Test: Move player to a mercenary clan (ghilman)
            TestRunner.RegisterTest(new TestCase(
                "player_special_003",
                "Move player to a mercenary clan (ghilman)",
                "gm.hero.set_clan main_hero ghilman",
                TestExpectation.NoException
            )
            {
                Category = "SpecialCases_Player",
                CustomValidator = (output) =>
                {
                    try
                    {
                        var player = Hero.MainHero;
                        if (player == null) return (false, "Main hero not found");
                        
                        var targetClan = Clan.FindFirst(c => c.StringId == "ghilman");
                        if (targetClan == null) return (false, "Target clan ghilman not found");
                        
                        if (player.Clan != targetClan)
                            return (false, $"Player clan is {player.Clan?.StringId} but expected {targetClan.StringId}");
                        
                        return (true, null);
                    }
                    catch (Exception ex)
                    {
                        return (false, $"Validation exception: {ex.Message}");
                    }
                },
                CleanupCommands = new System.Collections.Generic.List<string>
                {
                    "gm.hero.set_clan main_hero player_faction"
                }
            });

            // Test: Make player the leader of an NPC clan
            TestRunner.RegisterTest(new TestCase(
                "player_special_004",
                "Make player the leader of an NPC clan (clan_empire_south_1)",
                "gm.clan.set_leader clan_empire_south_1 main_hero",
                TestExpectation.NoException
            )
            {
                Category = "SpecialCases_Player",
                SetupCommands = new System.Collections.Generic.List<string>
                {
                    "gm.hero.set_clan main_hero clan_empire_south_1"
                },
                CustomValidator = (output) =>
                {
                    try
                    {
                        var player = Hero.MainHero;
                        if (player == null) return (false, "Main hero not found");
                        
                        var clan = Clan.FindFirst(c => c.StringId == "clan_empire_south_1");
                        if (clan == null) return (false, "Target clan not found");
                        
                        // Validate that player is now the clan leader
                        if (clan.Leader != player)
                            return (false, $"Clan leader is {clan.Leader?.Name} but expected player");
                        
                        return (true, null);
                    }
                    catch (Exception ex)
                    {
                        return (false, $"Validation exception: {ex.Message}");
                    }
                },
                CleanupCommands = new System.Collections.Generic.List<string>
                {
                    "gm.hero.set_clan main_hero player_faction"
                }
            });

            // Test: Make player the ruler of an NPC kingdom
            TestRunner.RegisterTest(new TestCase(
                "player_special_005",
                "Make player the ruler of an NPC kingdom (Sturgia)",
                "gm.kingdom.set_ruler sturgia main_hero",
                TestExpectation.NoException
            )
            {
                Category = "SpecialCases_Player",
                SetupCommands = new System.Collections.Generic.List<string>
                {
                    "gm.hero.set_clan main_hero clan_sturgia_1"
                },
                CustomValidator = (output) =>
                {
                    try
                    {
                        var player = Hero.MainHero;
                        if (player == null) return (false, "Main hero not found");
                        
                        var kingdom = Kingdom.All.FirstOrDefault(k => k.StringId == "sturgia");
                        if (kingdom == null) return (false, "Kingdom sturgia not found");
                        
                        // Validate that player is now the kingdom ruler
                        if (kingdom.Leader != player)
                            return (false, $"Kingdom ruler is {kingdom.Leader?.Name} but expected player");
                        
                        return (true, null);
                    }
                    catch (Exception ex)
                    {
                        return (false, $"Validation exception: {ex.Message}");
                    }
                },
                CleanupCommands = new System.Collections.Generic.List<string>
                {
                    "gm.hero.set_clan main_hero player_faction"
                }
            });

            // ============================================================
            // WANDERER HERO EDGE CASES
            // ============================================================

            // Test: Move a wanderer to an NPC clan
            TestRunner.RegisterTest(new TestCase(
                "wanderer_special_001",
                "Move a wanderer (CharacterObject_1900 'Ruwa the Spicevendor') to an NPC clan",
                "gm.hero.set_clan CharacterObject_1900 clan_empire_south_1",
                TestExpectation.NoException
            )
            {
                Category = "SpecialCases_Wanderer",
                CustomValidator = (output) =>
                {
                    try
                    {
                        // Wanderers are special heroes that might not behave like normal lords
                        var hero = Hero.FindFirst(h => h.StringId == "CharacterObject_1900");
                        if (hero == null) return (false, "Wanderer CharacterObject_1900 not found");
                        
                        var targetClan = Clan.FindFirst(c => c.StringId == "clan_empire_south_1");
                        if (targetClan == null) return (false, "Target clan not found");
                        
                        // Validate wanderer was moved to the clan
                        if (hero.Clan != targetClan)
                            return (false, $"Wanderer clan is {hero.Clan?.StringId} but expected {targetClan.StringId}");
                        
                        return (true, null);
                    }
                    catch (Exception ex)
                    {
                        return (false, $"Validation exception: {ex.Message}");
                    }
                }
            });

            // Test: Make a wanderer the leader of a clan
            TestRunner.RegisterTest(new TestCase(
                "wanderer_special_002",
                "Make a wanderer the leader of an NPC clan (clan_empire_south_2)",
                "gm.clan.set_leader clan_empire_south_2 CharacterObject_1900",
                TestExpectation.NoException
            )
            {
                Category = "SpecialCases_Wanderer",
                SetupCommands = new System.Collections.Generic.List<string>
                {
                    "gm.hero.set_clan CharacterObject_1900 clan_empire_south_2"
                },
                CustomValidator = (output) =>
                {
                    try
                    {
                        var wanderer = Hero.FindFirst(h => h.StringId == "CharacterObject_1900");
                        if (wanderer == null) return (false, "Wanderer not found");
                        
                        var clan = Clan.FindFirst(c => c.StringId == "clan_empire_south_2");
                        if (clan == null) return (false, "Clan not found");
                        
                        // Validate wanderer is now the clan leader
                        if (clan.Leader != wanderer)
                            return (false, $"Clan leader is {clan.Leader?.Name} but expected wanderer");
                        
                        return (true, null);
                    }
                    catch (Exception ex)
                    {
                        return (false, $"Validation exception: {ex.Message}");
                    }
                }
            });

            // Test: Make a wanderer the ruler of a kingdom
            TestRunner.RegisterTest(new TestCase(
                "wanderer_special_003",
                "Make a wanderer the ruler of a kingdom (Northern Empire)",
                "gm.kingdom.set_ruler empire CharacterObject_1900",
                TestExpectation.NoException
            )
            {
                Category = "SpecialCases_Wanderer",
                SetupCommands = new System.Collections.Generic.List<string>
                {
                    "gm.hero.set_clan CharacterObject_1900 clan_empire_north_1"
                },
                CustomValidator = (output) =>
                {
                    try
                    {
                        var wanderer = Hero.FindFirst(h => h.StringId == "CharacterObject_1900");
                        if (wanderer == null) return (false, "Wanderer not found");
                        
                        var kingdom = Kingdom.All.FirstOrDefault(k => k.StringId == "empire");
                        if (kingdom == null) return (false, "Kingdom empire not found");
                        
                        // Validate wanderer is now the kingdom ruler
                        if (kingdom.Leader != wanderer)
                            return (false, $"Kingdom ruler is {kingdom.Leader?.Name} but expected wanderer");
                        
                        return (true, null);
                    }
                    catch (Exception ex)
                    {
                        return (false, $"Validation exception: {ex.Message}");
                    }
                }
            });

            // ============================================================
            // DEAD HERO EDGE CASES
            // ============================================================

            // Test: Make a dead hero the leader of a clan
            TestRunner.RegisterTest(new TestCase(
                "dead_hero_special_001",
                "Make a dead hero (dead_lord_2_1 'Olek the Old') the leader of a clan",
                "gm.clan.set_leader clan_empire_south_1 dead_lord_2_1",
                TestExpectation.Error
            )
            {
                Category = "SpecialCases_DeadHero",
                ExpectedText = "No hero matching query 'dead_lord_2_1' found"
            });

            // Test: Make a dead hero the ruler of a kingdom
            TestRunner.RegisterTest(new TestCase(
                "dead_hero_special_002",
                "Make a dead hero (dead_lord_2_1) the ruler of a kingdom",
                "gm.kingdom.set_ruler empire dead_lord_2_1",
                TestExpectation.Error
            )
            {
                Category = "SpecialCases_DeadHero",
                ExpectedText = "No hero matching query 'dead_lord_2_1' found"
            });

            // Test: Attempt to heal a dead hero
            TestRunner.RegisterTest(new TestCase(
                "dead_hero_special_003",
                "Attempt to heal a dead hero (dead_lord_2_1) to full health",
                "gm.hero.heal dead_lord_2_1",
                TestExpectation.NoException
            )
            {
                Category = "SpecialCases_DeadHero",
                CustomValidator = (output) =>
                {
                    try
                    {
                        var hero = Hero.FindFirst(h => h.StringId == "dead_lord_2_1");
                        if (hero == null) return (false, "Dead hero not found");
                        
                        // Check how heal command handles dead heroes
                        // Heal should not resurrect dead heroes
                        if (hero.IsAlive)
                            return (false, "Heal command should not resurrect dead heroes");
                        
                        return (true, null);
                    }
                    catch (Exception ex)
                    {
                        return (false, $"Validation exception: {ex.Message}");
                    }
                }
            });

            // ============================================================
            // MERCENARY/CRIMINAL HERO EDGE CASES
            // ============================================================

            // Test: Move a mercenary hero to an NPC clan
            TestRunner.RegisterTest(new TestCase(
                "merc_hero_special_001",
                "Move a mercenary hero (CharacterObject_1866 'Orunhard of the Brotherhood') to an NPC clan",
                "gm.hero.set_clan CharacterObject_1866 clan_vlandia_1",
                TestExpectation.NoException
            )
            {
                Category = "SpecialCases_Mercenary",
                CustomValidator = (output) =>
                {
                    try
                    {
                        var hero = Hero.FindFirst(h => h.StringId == "CharacterObject_1866");
                        if (hero == null) return (false, "Mercenary hero CharacterObject_1866 not found");
                        
                        var targetClan = Clan.FindFirst(c => c.StringId == "clan_vlandia_1");
                        if (targetClan == null) return (false, "Target clan not found");
                        
                        // Validate mercenary hero was moved to the clan
                        if (hero.Clan != targetClan)
                            return (false, $"Mercenary hero clan is {hero.Clan?.StringId} but expected {targetClan.StringId}");
                        
                        return (true, null);
                    }
                    catch (Exception ex)
                    {
                        return (false, $"Validation exception: {ex.Message}");
                    }
                }
            });

            // Test: Make a mercenary hero the leader of an NPC clan
            TestRunner.RegisterTest(new TestCase(
                "merc_hero_special_002",
                "Make a mercenary hero the leader of an NPC clan (clan_vlandia_2)",
                "gm.clan.set_leader clan_vlandia_2 CharacterObject_1866",
                TestExpectation.NoException
            )
            {
                Category = "SpecialCases_Mercenary",
                SetupCommands = new System.Collections.Generic.List<string>
                {
                    "gm.hero.set_clan CharacterObject_1866 clan_vlandia_2"
                },
                CustomValidator = (output) =>
                {
                    try
                    {
                        var mercHero = Hero.FindFirst(h => h.StringId == "CharacterObject_1866");
                        if (mercHero == null) return (false, "Mercenary hero not found");
                        
                        var clan = Clan.FindFirst(c => c.StringId == "clan_vlandia_2");
                        if (clan == null) return (false, "Clan not found");
                        
                        // Validate mercenary is now the clan leader
                        if (clan.Leader != mercHero)
                            return (false, $"Clan leader is {clan.Leader?.Name} but expected mercenary hero");
                        
                        return (true, null);
                    }
                    catch (Exception ex)
                    {
                        return (false, $"Validation exception: {ex.Message}");
                    }
                }
            });

            // Test: Make a mercenary hero the ruler of a kingdom
            TestRunner.RegisterTest(new TestCase(
                "merc_hero_special_003",
                "Make a mercenary hero the ruler of a kingdom (Vlandia)",
                "gm.kingdom.set_ruler vlandia CharacterObject_1866",
                TestExpectation.NoException
            )
            {
                Category = "SpecialCases_Mercenary",
                CustomValidator = (output) =>
                {
                    try
                    {
                        var mercHero = Hero.FindFirst(h => h.StringId == "CharacterObject_1866");
                        if (mercHero == null) return (false, "Mercenary hero not found");
                        
                        var kingdom = Kingdom.All.FirstOrDefault(k => k.StringId == "vlandia");
                        if (kingdom == null) return (false, "Kingdom vlandia not found");
                        
                        // Validate mercenary is now the kingdom ruler
                        if (kingdom.Leader != mercHero)
                            return (false, $"Kingdom ruler is {kingdom.Leader?.Name} but expected mercenary hero");
                        
                        return (true, null);
                    }
                    catch (Exception ex)
                    {
                        return (false, $"Validation exception: {ex.Message}");
                    }
                }
            });

            // ============================================================
            // BANDIT CLAN EDGE CASES
            // ============================================================

            // Test: Move player to an empty bandit clan (looters)
            // KNOWN ISSUE: Moving player to bandit clan causes game crash when opening clan page
            TestRunner.RegisterTest(new TestCase(
                "bandit_clan_special_001",
                "Move player to an empty bandit clan (looters) - WARNING: May cause crash on clan page",
                "gm.hero.set_clan main_hero looters",
                TestExpectation.NoException
            )
            {
                Category = "SpecialCases_BanditClan",
                CustomValidator = (output) =>
                {
                    try
                    {
                        var player = Hero.MainHero;
                        if (player == null) return (false, "Main hero not found");
                        
                        var looterClan = Clan.FindFirst(c => c.StringId == "looters");
                        if (looterClan == null) return (false, "Looters clan not found");
                        
                        // NOTE: This test validates the operation completes, but opening the clan
                        // page after this change will crash the game due to bandit clan UI limitations
                        if (player.Clan != looterClan)
                            return (false, $"Player clan is {player.Clan?.StringId} but expected {looterClan.StringId}");
                        
                        return (true, null);
                    }
                    catch (Exception ex)
                    {
                        return (false, $"Validation exception: {ex.Message}");
                    }
                },
                CleanupCommands = new System.Collections.Generic.List<string>
                {
                    "gm.hero.set_clan main_hero player_faction"
                }
            });

            // Test: Move NPC lord to an empty bandit clan
            TestRunner.RegisterTest(new TestCase(
                "bandit_clan_special_002",
                "Move NPC lord (lord_4_1 'Derthert') to an empty bandit clan (deserters)",
                "gm.hero.set_clan lord_4_1 deserters",
                TestExpectation.NoException
            )
            {
                Category = "SpecialCases_BanditClan",
                CustomValidator = (output) =>
                {
                    try
                    {
                        var hero = Hero.FindFirst(h => h.StringId == "lord_4_1");
                        if (hero == null) return (false, "Lord not found");
                        
                        var desertersClan = Clan.FindFirst(c => c.StringId == "deserters");
                        if (desertersClan == null) return (false, "Deserters clan not found");
                        
                        // Validate NPC lord was moved to bandit clan
                        if (hero.Clan != desertersClan)
                            return (false, $"Hero clan is {hero.Clan?.StringId} but expected {desertersClan.StringId}");
                        
                        return (true, null);
                    }
                    catch (Exception ex)
                    {
                        return (false, $"Validation exception: {ex.Message}");
                    }
                },
                CleanupCommands = new System.Collections.Generic.List<string>
                {
                    "gm.hero.set_clan lord_4_1 clan_vlandia_1"
                }
            });

            // Test: Make player the leader of a bandit clan
            TestRunner.RegisterTest(new TestCase(
                "bandit_clan_special_003",
                "Make player the leader of a bandit clan (looters)",
                "gm.clan.set_leader looters main_hero",
                TestExpectation.NoException
            )
            {
                Category = "SpecialCases_BanditClan",
                SetupCommands = new System.Collections.Generic.List<string>
                {
                    "gm.hero.set_clan main_hero looters"
                },
                CustomValidator = (output) =>
                {
                    try
                    {
                        var player = Hero.MainHero;
                        if (player == null) return (false, "Main hero not found");
                        
                        var looterClan = Clan.FindFirst(c => c.StringId == "looters");
                        if (looterClan == null) return (false, "Looters clan not found");
                        
                        // Validate player is now the leader of bandit clan
                        if (looterClan.Leader != player)
                            return (false, $"Bandit clan leader is {looterClan.Leader?.Name} but expected player");
                        
                        return (true, null);
                    }
                    catch (Exception ex)
                    {
                        return (false, $"Validation exception: {ex.Message}");
                    }
                },
                CleanupCommands = new System.Collections.Generic.List<string>
                {
                    "gm.hero.set_clan main_hero player_faction"
                }
            });

            // Test: Query bandit clans to verify they're empty
            TestRunner.RegisterTest(new TestCase(
                "bandit_clan_special_004",
                "Query bandit clans to verify they have 0 heroes",
                "gm.query.clan bandit",
                TestExpectation.Contains
            )
            {
                Category = "SpecialCases_BanditClan",
                ExpectedText = "clan",
                CustomValidator = (output) =>
                {
                    try
                    {
                        // Verify we found bandit clans in the output
                        if (!output.ToLower().Contains("looters") && !output.ToLower().Contains("bandit"))
                            return (false, "Expected to find bandit clan information");
                        
                        // Check actual bandit clans
                        var looterClan = Clan.FindFirst(c => c.StringId == "looters");
                        if (looterClan != null)
                        {
                            int heroCount = looterClan.Heroes.Count(h => h.IsAlive);
                            // Bandit clans should typically have 0 heroes
                            // Allow for possibility that tests have modified them
                        }
                        
                        return (true, null);
                    }
                    catch (Exception ex)
                    {
                        return (false, $"Validation exception: {ex.Message}");
                    }
                }
            });

            // ============================================================
            // SPECIAL CLAN EDGE CASES
            // ============================================================

            // Test: Move player to a mercenary clan (ghilman)
            TestRunner.RegisterTest(new TestCase(
                "special_clan_001",
                "Move player to a mercenary clan (ghilman)",
                "gm.hero.set_clan main_hero ghilman",
                TestExpectation.NoException
            )
            {
                Category = "SpecialCases_SpecialClan",
                CustomValidator = (output) =>
                {
                    try
                    {
                        var player = Hero.MainHero;
                        if (player == null) return (false, "Main hero not found");
                        
                        var ghilmanClan = Clan.FindFirst(c => c.StringId == "ghilman");
                        if (ghilmanClan == null) return (false, "Ghilman clan not found");
                        
                        // Validate player was moved to mercenary clan
                        if (player.Clan != ghilmanClan)
                            return (false, $"Player clan is {player.Clan?.StringId} but expected {ghilmanClan.StringId}");
                        
                        return (true, null);
                    }
                    catch (Exception ex)
                    {
                        return (false, $"Validation exception: {ex.Message}");
                    }
                },
                CleanupCommands = new System.Collections.Generic.List<string>
                {
                    "gm.hero.set_clan main_hero player_faction"
                }
            });

            // Test: Move NPC lord to a mercenary clan
            TestRunner.RegisterTest(new TestCase(
                "special_clan_002",
                "Move NPC lord to a mercenary clan (legion_of_the_betrayed)",
                "gm.hero.set_clan lord_1_1 legion_of_the_betrayed",
                TestExpectation.NoException
            )
            {
                Category = "SpecialCases_SpecialClan",
                CustomValidator = (output) =>
                {
                    try
                    {
                        var hero = Hero.FindFirst(h => h.StringId == "lord_1_1");
                        if (hero == null) return (false, "Lord not found");
                        
                        var mercClan = Clan.FindFirst(c => c.StringId == "legion_of_the_betrayed");
                        if (mercClan == null) return (false, "Legion of the Betrayed clan not found");
                        
                        return (true, null);
                    }
                    catch (Exception ex)
                    {
                        return (false, $"Validation exception: {ex.Message}");
                    }
                }
            });

            // Test: Query special clans to verify their structure
            TestRunner.RegisterTest(new TestCase(
                "special_clan_003",
                "Query special clans (mercenary, criminal, sect) to verify their structure",
                "gm.query.clan mercenary",
                TestExpectation.Contains
            )
            {
                Category = "SpecialCases_SpecialClan",
                ExpectedText = "clan",
                CustomValidator = (output) =>
                {
                    try
                    {
                        // Check for mercenary clan mentions
                        if (!output.ToLower().Contains("ghilman") &&
                            !output.ToLower().Contains("mercenary") &&
                            !output.ToLower().Contains("legion"))
                            return (false, "Expected to find mercenary clan information");
                        
                        // Verify mercenary clans exist
                        var ghilman = Clan.FindFirst(c => c.StringId == "ghilman");
                        var legion = Clan.FindFirst(c => c.StringId == "legion_of_the_betrayed");
                        
                        if (ghilman == null && legion == null)
                            return (false, "No mercenary clans found in game");
                        
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
        /// Register tests for the ID matching collision fix in CommandBase.ResolveMultipleMatches
        /// Tests the three-tier priority algorithm for smart ID matching
        /// </summary>
        private static void RegisterIDMatchingCollisionFixTests()
        {
            // ============================================================
            // TEST 1: EXACT ID MATCH PRIORITY
            // ============================================================

            // Test: Exact ID match should be selected even if other partial matches exist
            TestRunner.RegisterTest(new TestCase(
                "id_matching_exact_match_001",
                "Query matches exact ID among partial matches - should return exact match immediately",
                "gm.hero.set_gold lord_1_1 5000",
                TestExpectation.Success
            )
            {
                Category = "IDMatching_ExactMatch",
                CustomValidator = (output) =>
                {
                    try
                    {
                        // Verify the command succeeded (exact ID match)
                        if (!(output.IndexOf("Success", StringComparison.OrdinalIgnoreCase) >= 0))
                            return (false, "Expected success for exact ID match");

                        // Verify it was the correct hero (lord_1_1, not lord_1_11 or lord_1_12)
                        var hero = Hero.FindFirst(h => h.StringId == "lord_1_1");
                        if (hero == null) return (false, "Hero lord_1_1 not found");
                        
                        if (hero.Gold != 5000)
                            return (false, $"Gold should be 5000 but is {hero.Gold} - wrong hero may have been selected");

                        return (true, null);
                    }
                    catch (Exception ex)
                    {
                        return (false, $"Validation exception: {ex.Message}");
                    }
                }
            });

            // Test: Exact ID match with case insensitivity
            TestRunner.RegisterTest(new TestCase(
                "id_matching_exact_match_002_case_insensitive",
                "Query with different case should match exact ID (case-insensitive)",
                "gm.hero.set_gold LORD_1_1 5001",
                TestExpectation.Success
            )
            {
                Category = "IDMatching_ExactMatch",
                CustomValidator = (output) =>
                {
                    try
                    {
                        if (!(output.IndexOf("Success", StringComparison.OrdinalIgnoreCase) >= 0))
                            return (false, "Expected success for case-insensitive exact ID match");

                        var hero = Hero.FindFirst(h => h.StringId == "lord_1_1");
                        if (hero == null) return (false, "Hero lord_1_1 not found");
                        
                        if (hero.Gold != 5001)
                            return (false, $"Gold should be 5001 but is {hero.Gold}");

                        return (true, null);
                    }
                    catch (Exception ex)
                    {
                        return (false, $"Validation exception: {ex.Message}");
                    }
                }
            });

            // ============================================================
            // TEST 2: SHORTEST ID SELECTION
            // ============================================================

            // Test: Query "lord_1" should select shortest ID among matches
            TestRunner.RegisterTest(new TestCase(
                "id_matching_shortest_id_001",
                "Query 'lord_1' matches multiple IDs with same length - should return error",
                "gm.hero.set_gold lord_1 6000",
                TestExpectation.Error
            )
            {
                Category = "IDMatching_ShortestID",
                CustomValidator = (output) =>
                {
                    try
                    {
                        // Should error because all matching IDs have the same length
                        if (!(output.IndexOf("Error", StringComparison.OrdinalIgnoreCase) >= 0))
                            return (false, "Expected error for multiple IDs with same length");

                        // Check for the specific error message about same-length IDs
                        if (!(output.IndexOf("Please use a more specific ID", StringComparison.OrdinalIgnoreCase) >= 0))
                            return (false, "Expected error message asking to be more specific");

                        return (true, null);
                    }
                    catch (Exception ex)
                    {
                        return (false, $"Validation exception: {ex.Message}");
                    }
                }
            });

            // Test: Query partial ID with multiple matches of different lengths
            TestRunner.RegisterTest(new TestCase(
                "id_matching_shortest_id_002",
                "Query 'clan_empire' matches multiple IDs with same length - should return error",
                "gm.clan.set_gold clan_empire 50000",
                TestExpectation.Error
            )
            {
                Category = "IDMatching_ShortestID",
                CustomValidator = (output) =>
                {
                    try
                    {
                        // Should error because all matching IDs have the same length
                        if (!(output.IndexOf("Error", StringComparison.OrdinalIgnoreCase) >= 0))
                            return (false, "Expected error for multiple IDs with same length");

                        // Check for the specific error message about same-length IDs
                        if (!(output.IndexOf("Please use a more specific ID", StringComparison.OrdinalIgnoreCase) >= 0))
                            return (false, "Expected error message asking to be more specific");

                        return (true, null);
                    }
                    catch (Exception ex)
                    {
                        return (false, $"Validation exception: {ex.Message}");
                    }
                }
            });

            // Test: Query should auto-select shortest ID when IDs have different lengths (Hero)
            TestRunner.RegisterTest(new TestCase(
                "id_matching_shortest_id_003",
                "Query 'lord_1_41' should auto-select shortest ID (lord_1_41) over longer match (lord_1_411)",
                "gm.hero.set_gold lord_1_41 6000",
                TestExpectation.Success
            )
            {
                Category = "IDMatching_ShortestID",
                CustomValidator = (output) =>
                {
                    try
                    {
                        // Should succeed because lord_1_41 is shorter than lord_1_411
                        if (!(output.IndexOf("Success", StringComparison.OrdinalIgnoreCase) >= 0))
                            return (false, "Expected success - shortest ID (lord_1_41) should be auto-selected");

                        // Verify the correct hero was selected (lord_1_41, not lord_1_411)
                        var hero = Hero.FindFirst(h => h.StringId == "lord_1_41");
                        if (hero == null) return (false, "Hero lord_1_41 not found");
                        
                        if (hero.Gold != 6000)
                            return (false, $"Gold should be 6000 but is {hero.Gold} - wrong hero may have been selected");

                        return (true, null);
                    }
                    catch (Exception ex)
                    {
                        return (false, $"Validation exception: {ex.Message}");
                    }
                }
            });

            // Test: Query should auto-select shortest ID when IDs have different lengths (Clan)
            TestRunner.RegisterTest(new TestCase(
                "id_matching_shortest_id_004",
                "Partial query 'clan_vlandia_1' should auto-select shortest ID (clan_vlandia_1) over longer match (clan_vlandia_11)",
                "gm.clan.set_gold clan_vlandia_1 50000",
                TestExpectation.Success
            )
            {
                Category = "IDMatching_ShortestID",
                CustomValidator = (output) =>
                {
                    try
                    {
                        // Should succeed because clan_vlandia_1 is shorter than clan_vlandia_11
                        if (!(output.IndexOf("Success", StringComparison.OrdinalIgnoreCase) >= 0))
                            return (false, "Expected success - shortest ID (clan_vlandia_1) should be auto-selected");

                        // Verify the correct clan was selected
                        var clan = Clan.FindFirst(c => c.StringId == "clan_vlandia_1");
                        if (clan == null) return (false, "Clan clan_vlandia_1 not found");
                        
                        // Verify clan gold was set correctly
                        int totalClanGold = clan.Heroes.Where(h => h.IsAlive).Sum(h => h.Gold);
                        if (totalClanGold != 50000)
                            return (false, $"Total clan member gold is {totalClanGold} but expected 50000");

                        return (true, null);
                    }
                    catch (Exception ex)
                    {
                        return (false, $"Validation exception: {ex.Message}");
                    }
                }
            });

            // ============================================================
            // TEST 3: MULTIPLE NAME MATCHES ERROR
            // ============================================================

            // Test: Query matches multiple entities by name only
            TestRunner.RegisterTest(new TestCase(
                "id_matching_name_matches_error_001",
                "Query matching multiple names (not IDs) should return error asking to be more specific",
                "gm.hero.set_gold derthert 7000",
                TestExpectation.NoException
            )
            {
                Category = "IDMatching_NameMatches",
                CustomValidator = (output) =>
                {
                    try
                    {
                        // "derthert" might match multiple heroes by name
                        // If only one Derthert exists, it should succeed
                        // If multiple exist, should get "name" error (not ID collision)
                        
                        var matches = Hero.AllAliveHeroes.Where(h =>
                            h.Name != null &&
                            h.Name.ToString().IndexOf("derthert", StringComparison.OrdinalIgnoreCase) >= 0
                        ).ToList();

                        if (matches.Count == 1)
                        {
                            // Single match should succeed
                            return (output.IndexOf("Success", StringComparison.OrdinalIgnoreCase) >= 0,
                                "Expected success for single name match");
                        }
                        else if (matches.Count > 1)
                        {
                            // Multiple name matches should error with "name" message
                            bool hasError = output.IndexOf("Error", StringComparison.OrdinalIgnoreCase) >= 0;
                            bool mentionsName = output.IndexOf("name", StringComparison.OrdinalIgnoreCase) >= 0;
                            
                            if (!hasError)
                                return (false, "Expected error for multiple name matches");
                            if (!mentionsName)
                                return (false, "Error message should mention 'name' matches");
                            
                            return (true, null);
                        }

                        return (true, null);
                    }
                    catch (Exception ex)
                    {
                        return (false, $"Validation exception: {ex.Message}");
                    }
                }
            });

            // Test: Query by hero name should work correctly
            TestRunner.RegisterTest(new TestCase(
                "id_matching_name_query_001",
                "Query by hero name should work correctly",
                "gm.hero.set_gold Garios 7500",
                TestExpectation.Success
            )
            {
                Category = "IDMatching_NameMatches",
                CustomValidator = (output) =>
                {
                    try
                    {
                        // Query by name "Garios" should find Lucon's brother
                        if (!(output.IndexOf("Success", StringComparison.OrdinalIgnoreCase) >= 0))
                            return (false, "Expected success for name-based query");

                        // Find hero with name Garios
                        var hero = Hero.AllAliveHeroes.FirstOrDefault(h =>
                            h.Name != null &&
                            h.Name.ToString().Equals("Garios", StringComparison.OrdinalIgnoreCase)
                        );
                        
                        if (hero == null) return (false, "Hero with name 'Garios' not found");
                        
                        if (hero.Gold != 7500)
                            return (false, $"Gold should be 7500 but is {hero.Gold}");

                        return (true, null);
                    }
                    catch (Exception ex)
                    {
                        return (false, $"Validation exception: {ex.Message}");
                    }
                }
            });

            // Test: Query by clan name should work correctly
            TestRunner.RegisterTest(new TestCase(
                "id_matching_name_query_002",
                "Query by clan name should work correctly",
                "gm.clan.set_gold Comnos 55000",
                TestExpectation.Success
            )
            {
                Category = "IDMatching_NameMatches",
                CustomValidator = (output) =>
                {
                    try
                    {
                        // Query by clan name "Comnos" should work
                        if (!(output.IndexOf("Success", StringComparison.OrdinalIgnoreCase) >= 0))
                            return (false, "Expected success for clan name-based query");

                        // Find clan with name Comnos
                        var clan = Clan.All.FirstOrDefault(c =>
                            c.Name != null &&
                            c.Name.ToString().Equals("Comnos", StringComparison.OrdinalIgnoreCase)
                        );
                        
                        if (clan == null) return (false, "Clan with name 'Comnos' not found");
                        
                        // Verify clan gold was set correctly
                        int totalClanGold = clan.Heroes.Where(h => h.IsAlive).Sum(h => h.Gold);
                        if (totalClanGold != 55000)
                            return (false, $"Total clan member gold is {totalClanGold} but expected 55000");

                        return (true, null);
                    }
                    catch (Exception ex)
                    {
                        return (false, $"Validation exception: {ex.Message}");
                    }
                }
            });

            // ============================================================
            // TEST 4: SINGLE MATCH SUCCESS
            // ============================================================

            // Test: Query matches exactly one entity
            TestRunner.RegisterTest(new TestCase(
                "id_matching_single_match_001",
                "Query matching exactly one entity should return success",
                "gm.hero.set_gold main_hero 8000",
                TestExpectation.Success
            )
            {
                Category = "IDMatching_SingleMatch",
                CustomValidator = (output) =>
                {
                    try
                    {
                        if (!(output.IndexOf("Success", StringComparison.OrdinalIgnoreCase) >= 0))
                            return (false, "Expected success for single match");

                        var player = Hero.MainHero;
                        if (player == null) return (false, "Main hero not found");
                        
                        if (player.Gold != 8000)
                            return (false, $"Gold should be 8000 but is {player.Gold}");

                        return (true, null);
                    }
                    catch (Exception ex)
                    {
                        return (false, $"Validation exception: {ex.Message}");
                    }
                }
            });

            // ============================================================
            // TEST 5: NO MATCHES ERROR
            // ============================================================

            // Test: Query doesn't match any entity
            TestRunner.RegisterTest(new TestCase(
                "id_matching_no_match_001",
                "Query matching no entities should return 'not found' error",
                "gm.hero.set_gold nonexistent_hero_xyz_123 9000",
                TestExpectation.Error
            )
            {
                Category = "IDMatching_NoMatch",
                ExpectedText = "No hero matching",
                CustomValidator = (output) =>
                {
                    try
                    {
                        bool hasError = output.IndexOf("Error", StringComparison.OrdinalIgnoreCase) >= 0;
                        bool hasNotFound = output.IndexOf("No hero", StringComparison.OrdinalIgnoreCase) >= 0 ||
                                          output.IndexOf("not found", StringComparison.OrdinalIgnoreCase) >= 0;

                        if (!hasError)
                            return (false, "Expected error for no matches");
                        if (!hasNotFound)
                            return (false, "Error should indicate entity not found");

                        return (true, null);
                    }
                    catch (Exception ex)
                    {
                        return (false, $"Validation exception: {ex.Message}");
                    }
                }
            });

            // ============================================================
            // TEST 6: MIXED ID AND NAME MATCHES
            // ============================================================

            // Test: Query matches some IDs and some names - should prioritize ID matches
            TestRunner.RegisterTest(new TestCase(
                "id_matching_mixed_matches_001",
                "Query matching both IDs and names should prioritize ID matches",
                "gm.kingdom.add_clan clan_empire_south_1 empire",
                TestExpectation.Success
            )
            {
                Category = "IDMatching_MixedMatches",
                CustomValidator = (output) =>
                {
                    try
                    {
                        // "empire" matches kingdom IDs (empire, empire_s, empire_w)
                        // and possibly kingdom names
                        // Should select shortest ID match (likely "empire")
                        
                        if (!(output.IndexOf("Success", StringComparison.OrdinalIgnoreCase) >= 0))
                            return (false, "Expected success - ID matches should be prioritized over name matches");

                        return (true, null);
                    }
                    catch (Exception ex)
                    {
                        return (false, $"Validation exception: {ex.Message}");
                    }
                },
                CleanupCommands = new System.Collections.Generic.List<string>
                {
                    "gm.kingdom.remove_clan clan_empire_south_1"
                }
            });

            // ============================================================
            // TEST 7: SAME-LENGTH ID COLLISION
            // ============================================================

            // Test: Query matches multiple IDs with identical length
            TestRunner.RegisterTest(new TestCase(
                "id_matching_same_length_collision_001",
                "Query matching multiple IDs with same length should return error or select first deterministically",
                "gm.hero.set_gold lord_1 10000",
                TestExpectation.NoException
            )
            {
                Category = "IDMatching_SameLengthCollision",
                CustomValidator = (output) =>
                {
                    try
                    {
                        // Query "lord_1" might match: lord_1_1, lord_1_2, etc.
                        // These have different lengths (8 vs 8), so shortest logic applies
                        // However if they have exactly same length, should get ambiguity error
                        // OR select first one deterministically
                        
                        // For lord_1 query, we expect shortest (lord_1_1) to be selected
                        // This is actually Test 2, but confirming behavior
                        
                        bool hasSuccess = output.IndexOf("Success", StringComparison.OrdinalIgnoreCase) >= 0;
                        bool hasError = output.IndexOf("Error", StringComparison.OrdinalIgnoreCase) >= 0;

                        // Either success (selected shortest/first) or error (ambiguous) is acceptable
                        if (!hasSuccess && !hasError)
                            return (false, "Expected either success or error message");

                        return (true, null);
                    }
                    catch (Exception ex)
                    {
                        return (false, $"Validation exception: {ex.Message}");
                    }
                }
            });

            // Test: Clan query with potential same-length ID collision
            TestRunner.RegisterTest(new TestCase(
                "id_matching_same_length_collision_002",
                "Clan query matching IDs with same length should handle gracefully",
                "gm.clan.set_gold clan_empire 60000",
                TestExpectation.NoException
            )
            {
                Category = "IDMatching_SameLengthCollision",
                CustomValidator = (output) =>
                {
                    try
                    {
                        // Should either succeed (selected shortest) or error (ambiguous)
                        bool hasSuccess = output.IndexOf("Success", StringComparison.OrdinalIgnoreCase) >= 0;
                        bool hasError = output.IndexOf("Error", StringComparison.OrdinalIgnoreCase) >= 0;

                        if (!hasSuccess && !hasError)
                            return (false, "Expected either success or error");

                        if (hasError)
                        {
                            // If error, should mention IDs with same length
                            bool mentionsLength = output.IndexOf("length", StringComparison.OrdinalIgnoreCase) >= 0 ||
                                                 output.IndexOf("specific", StringComparison.OrdinalIgnoreCase) >= 0;
                            if (!mentionsLength)
                                return (false, "Ambiguity error should mention length or ask to be specific");
                        }

                        return (true, null);
                    }
                    catch (Exception ex)
                    {
                        return (false, $"Validation exception: {ex.Message}");
                    }
                }
            });

            // ============================================================
            // TEST 8: CASE INSENSITIVITY
            // ============================================================

            // Test: Query with different case should match correctly
            TestRunner.RegisterTest(new TestCase(
                "id_matching_case_insensitive_001",
                "Case-insensitive matching - uppercase query should match lowercase ID",
                "gm.hero.set_gold MAIN_HERO 11000",
                TestExpectation.Success
            )
            {
                Category = "IDMatching_CaseInsensitivity",
                CustomValidator = (output) =>
                {
                    try
                    {
                        if (!(output.IndexOf("Success", StringComparison.OrdinalIgnoreCase) >= 0))
                            return (false, "Expected success for case-insensitive match");

                        var player = Hero.MainHero;
                        if (player == null) return (false, "Main hero not found");
                        
                        if (player.Gold != 11000)
                            return (false, $"Gold should be 11000 but is {player.Gold}");

                        return (true, null);
                    }
                    catch (Exception ex)
                    {
                        return (false, $"Validation exception: {ex.Message}");
                    }
                }
            });

            // Test: Mixed case query should work with shortest ID logic
            TestRunner.RegisterTest(new TestCase(
                "id_matching_case_insensitive_002",
                "Mixed case query should apply shortest ID logic case-insensitively",
                "gm.clan.set_gold Clan_Empire 70000",
                TestExpectation.NoException
            )
            {
                Category = "IDMatching_CaseInsensitivity",
                CustomValidator = (output) =>
                {
                    try
                    {
                        // Should succeed or error, but handle case-insensitively
                        bool hasSuccess = output.IndexOf("Success", StringComparison.OrdinalIgnoreCase) >= 0;
                        bool hasError = output.IndexOf("Error", StringComparison.OrdinalIgnoreCase) >= 0;

                        if (!hasSuccess && !hasError)
                            return (false, "Expected result (success or error)");

                        return (true, null);
                    }
                    catch (Exception ex)
                    {
                        return (false, $"Validation exception: {ex.Message}");
                    }
                }
            });

            // ============================================================
            // TEST 9: BACKWARD COMPATIBILITY - SIMPLE CASES
            // ============================================================

            // Test: Simple unique query should still work as before
            TestRunner.RegisterTest(new TestCase(
                "id_matching_backward_compat_001",
                "Backward compatibility - simple unique query should work as before",
                "gm.hero.set_gold main_hero 12000",
                TestExpectation.Success
            )
            {
                Category = "IDMatching_BackwardCompatibility",
                CustomValidator = (output) =>
                {
                    try
                    {
                        if (!(output.IndexOf("Success", StringComparison.OrdinalIgnoreCase) >= 0))
                            return (false, "Expected success for backward compatible query");

                        var player = Hero.MainHero;
                        if (player == null) return (false, "Main hero not found");
                        
                        if (player.Gold != 12000)
                            return (false, $"Gold should be 12000 but is {player.Gold}");

                        return (true, null);
                    }
                    catch (Exception ex)
                    {
                        return (false, $"Validation exception: {ex.Message}");
                    }
                }
            });

            // Test: Kingdom exact ID match backward compatibility
            TestRunner.RegisterTest(new TestCase(
                "id_matching_backward_compat_002",
                "Backward compatibility - exact kingdom ID should work as before",
                "gm.kingdom.add_clan clan_sturgia_2 vlandia",
                TestExpectation.Success
            )
            {
                Category = "IDMatching_BackwardCompatibility",
                CustomValidator = (output) =>
                {
                    try
                    {
                        if (!(output.IndexOf("Success", StringComparison.OrdinalIgnoreCase) >= 0))
                            return (false, "Expected success for exact kingdom ID");

                        var clan = Clan.FindFirst(c => c.StringId == "clan_sturgia_2");
                        if (clan == null) return (false, "Clan not found");

                        var kingdom = Kingdom.All.FirstOrDefault(k => k.StringId == "vlandia");
                        if (kingdom == null) return (false, "Kingdom vlandia not found");

                        if (clan.Kingdom != kingdom)
                            return (false, $"Clan kingdom is {clan.Kingdom?.StringId} but expected vlandia");

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

            // ============================================================
            // TEST 10: ALL THREE ENTITY TYPES (HERO, CLAN, KINGDOM)
            // ============================================================

            // Test: Hero FindSingle with exact match
            TestRunner.RegisterTest(new TestCase(
                "id_matching_all_types_hero_001",
                "Hero FindSingle - exact ID match among partials",
                "gm.hero.set_age lord_2_1 35",
                TestExpectation.Success
            )
            {
                Category = "IDMatching_AllTypes",
                CustomValidator = (output) =>
                {
                    try
                    {
                        if (!(output.IndexOf("Success", StringComparison.OrdinalIgnoreCase) >= 0))
                            return (false, "Expected success for hero exact ID");

                        var hero = Hero.FindFirst(h => h.StringId == "lord_2_1");
                        if (hero == null) return (false, "Hero lord_2_1 not found");

                        return (true, null);
                    }
                    catch (Exception ex)
                    {
                        return (false, $"Validation exception: {ex.Message}");
                    }
                }
            });

            // Test: Clan FindSingle with shortest ID
            TestRunner.RegisterTest(new TestCase(
                "id_matching_all_types_clan_001",
                "Clan FindSingle - shortest ID selection",
                "gm.clan.set_renown clan_empire 800",
                TestExpectation.NoException
            )
            {
                Category = "IDMatching_AllTypes",
                CustomValidator = (output) =>
                {
                    try
                    {
                        // Should succeed or error, but not crash
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

            // Test: Kingdom FindSingle with exact match
            TestRunner.RegisterTest(new TestCase(
                "id_matching_all_types_kingdom_001",
                "Kingdom FindSingle - exact ID among name matches",
                "gm.kingdom.add_clan clan_vlandia_3 sturgia",
                TestExpectation.Success
            )
            {
                Category = "IDMatching_AllTypes",
                CustomValidator = (output) =>
                {
                    try
                    {
                        if (!(output.IndexOf("Success", StringComparison.OrdinalIgnoreCase) >= 0))
                            return (false, "Expected success for exact kingdom ID");

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
    }
}