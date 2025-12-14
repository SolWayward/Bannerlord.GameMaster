using System;
using System.Collections.Generic;
using System.Linq;
using Bannerlord.GameMaster.Console.Common;
using TaleWorlds.Library;

namespace Bannerlord.GameMaster.Console.Testing
{
    /// <summary>
    /// Console commands for running automated tests
    /// </summary>
    [CommandLineFunctionality.CommandLineArgumentFunction("test", "gm")]
    public static class TestCommands
    {
        /// <summary>
        /// Run all registered tests
        /// Usage: gm.test.run_all
        /// </summary>
        [CommandLineFunctionality.CommandLineArgumentFunction("run_all", "gm.test")]
        public static string RunAllTests(List<string> args)
        {
            return Cmd.Run(args, () =>
            {
                try
                {
                    var tests = TestRunner.GetRegisteredTests();
                    
                    if (tests.Count == 0)
                    {
                        return "No tests are registered. Use gm.test.list to see available tests.\n";
                    }

                    InformationManager.DisplayMessage(new InformationMessage($"Running {tests.Count} tests..."));
                    
                    var results = TestRunner.RunAllTests();
                    string report = TestRunner.GenerateReport(results);
                    
                    return report;
                }
                catch (Exception ex)
                {
                    return $"Error running tests: {ex.Message}\n";
                }
            });
        }

        /// <summary>
        /// Run tests in a specific category
        /// Usage: gm.test.run_category HeroQuery
        /// </summary>
        [CommandLineFunctionality.CommandLineArgumentFunction("run_category", "gm.test")]
        public static string RunTestsByCategory(List<string> args)
        {
            if (args == null || args.Count == 0)
            {
                return "Error: Please specify a category.\nUsage: gm.test.run_category <category>\n" +
                       "Use gm.test.list to see available categories.\n";
            }

            try
            {
                string category = args[0];
                var results = TestRunner.RunTestsByCategory(category);
                
                if (results.Count == 0)
                {
                    return $"No tests found in category '{category}'.\nUse gm.test.list to see available categories.\n";
                }

                string report = TestRunner.GenerateReport(results);
                return report;
            }
            catch (Exception ex)
            {
                return $"Error running tests: {ex.Message}\n";
            }
        }

        /// <summary>
        /// Run a specific test by ID
        /// Usage: gm.test.run_single test_hero_query_001
        /// </summary>
        [CommandLineFunctionality.CommandLineArgumentFunction("run_single", "gm.test")]
        public static string RunSingleTest(List<string> args)
        {
            if (args == null || args.Count == 0)
            {
                return "Error: Please specify a test ID.\nUsage: gm.test.run_single <test_id>\n" +
                       "Use gm.test.list to see available test IDs.\n";
            }

            try
            {
                string testId = args[0];
                var result = TestRunner.RunTestById(testId);
                
                if (result == null)
                {
                    return $"Test '{testId}' not found.\nUse gm.test.list to see available tests.\n";
                }

                return result.GetDetails() + "\n";
            }
            catch (Exception ex)
            {
                return $"Error running test: {ex.Message}\n";
            }
        }

        /// <summary>
        /// List all registered tests
        /// Usage: gm.test.list [category]
        /// </summary>
        [CommandLineFunctionality.CommandLineArgumentFunction("list", "gm.test")]
        public static string ListTests(List<string> args)
        {
            return Cmd.Run(args, () =>
            {
                try
                {
                    var tests = TestRunner.GetRegisteredTests();
                    
                    if (tests.Count == 0)
                    {
                        return "No tests are registered.\n";
                    }

                    string filterCategory = args != null && args.Count > 0 ? args[0] : null;
                    
                    if (!string.IsNullOrEmpty(filterCategory))
                    {
                        tests = tests.Where(t => string.Equals(t.Category, filterCategory, StringComparison.OrdinalIgnoreCase)).ToList();
                        
                        if (tests.Count == 0)
                        {
                            return $"No tests found in category '{filterCategory}'.\n";
                        }
                    }

                    string output = $"=== REGISTERED TESTS ({tests.Count}) ===\n\n";

                    // Group by category
                    var byCategory = tests.GroupBy(t => t.Category ?? "Uncategorized");
                    
                    foreach (var category in byCategory.OrderBy(g => g.Key))
                    {
                        output += $"Category: {category.Key} ({category.Count()} tests)\n";
                        output += new string('-', 50) + "\n";
                        
                        foreach (var test in category.OrderBy(t => t.TestId))
                        {
                            output += $"  [{test.TestId}] {test.Description}\n";
                            output += $"    Command: {test.Command}\n";
                            output += $"    Expects: {test.Expectation}";
                            
                            if (!string.IsNullOrEmpty(test.ExpectedText))
                            {
                                output += $" (contains: '{test.ExpectedText}')";
                            }
                            
                            output += "\n\n";
                        }
                    }

                    return output;
                }
                catch (Exception ex)
                {
                    return $"Error listing tests: {ex.Message}\n";
                }
            });
        }

        /// <summary>
        /// Show results from last test run
        /// Usage: gm.test.last_results [verbose]
        /// </summary>
        [CommandLineFunctionality.CommandLineArgumentFunction("last_results", "gm.test")]
        public static string ShowLastResults(List<string> args)
        {
            try
            {
                var results = TestRunner.GetLastResults();
                
                if (results.Count == 0)
                {
                    return "No test results available. Run tests first with gm.test.run_all\n";
                }

                bool verbose = args != null && args.Count > 0 && 
                              (args[0].Equals("verbose", StringComparison.OrdinalIgnoreCase) ||
                               args[0].Equals("true", StringComparison.OrdinalIgnoreCase));

                if (verbose)
                {
                    return TestRunner.GenerateReport(results);
                }
                else
                {
                    int passed = results.Count(r => r.Passed);
                    int failed = results.Count(r => !r.Passed);
                    string summary = $"Last Test Run Summary:\n";
                    summary += $"Total: {results.Count}, Passed: {passed}, Failed: {failed}\n";
                    summary += $"Success Rate: {(passed * 100.0 / results.Count):F1}%\n";
                    summary += "\nUse 'gm.test.last_results verbose' for detailed report.\n";
                    return summary;
                }
            }
            catch (Exception ex)
            {
                return $"Error showing results: {ex.Message}\n";
            }
        }

        /// <summary>
        /// Clear all registered tests
        /// Usage: gm.test.clear
        /// </summary>
        [CommandLineFunctionality.CommandLineArgumentFunction("clear", "gm.test")]
        public static string ClearTests(List<string> args)
        {
            try
            {
                int count = TestRunner.GetRegisteredTests().Count;
                TestRunner.ClearTests();
                return $"Cleared {count} registered tests.\n";
            }
            catch (Exception ex)
            {
                return $"Error clearing tests: {ex.Message}\n";
            }
        }

        /// <summary>
        /// Register standard tests for validation
        /// Usage: gm.test.register_standard
        /// </summary>
        [CommandLineFunctionality.CommandLineArgumentFunction("register_standard", "gm.test")]
        public static string RegisterStandardTests(List<string> args)
        {
            try
            {
                StandardTests.RegisterAll();
                int count = TestRunner.GetRegisteredTests().Count;
                return $"Successfully registered {count} standard tests.\nUse 'gm.test.list' to see them.\n";
            }
            catch (Exception ex)
            {
                return $"Error registering standard tests: {ex.Message}\n";
            }
        }

        /// <summary>
        /// Register integration tests that validate game state changes
        /// Usage: gm.test.register_integration
        /// </summary>
        [CommandLineFunctionality.CommandLineArgumentFunction("register_integration", "gm.test")]
        public static string RegisterIntegrationTests(List<string> args)
        {
            try
            {
                IntegrationTests.RegisterAll();
                int count = TestRunner.GetRegisteredTests().Count;
                return $"Successfully registered {count} integration tests.\nUse 'gm.test.list' to see them.\n";
            }
            catch (Exception ex)
            {
                return $"Error registering integration tests: {ex.Message}\n";
            }
        }

        /// <summary>
        /// Register all available tests (standard + integration)
        /// Usage: gm.test.register_all
        /// </summary>
        [CommandLineFunctionality.CommandLineArgumentFunction("register_all", "gm.test")]
        public static string RegisterAllTests(List<string> args)
        {
            try
            {
                TestRunner.ClearTests();
                StandardTests.RegisterAll();
                IntegrationTests.RegisterAll();
                int count = TestRunner.GetRegisteredTests().Count;
                return $"Successfully registered {count} tests (standard + integration).\nUse 'gm.test.list' to see them.\n";
            }
            catch (Exception ex)
            {
                return $"Error registering tests: {ex.Message}\n";
            }
        }

        /// <summary>
        /// Register all tests and run them
        /// Usage: gm.test.register_all_run
        /// </summary>
        [CommandLineFunctionality.CommandLineArgumentFunction("register_all_run", "gm.test")]
        public static string RegisterAllAndRun(List<string> args)
        {
            return Cmd.Run(args, () =>
            {
                try
                {
                    // First, register all tests
                    TestRunner.ClearTests();
                    StandardTests.RegisterAll();
                    IntegrationTests.RegisterAll();
                    int testCount = TestRunner.GetRegisteredTests().Count;

                    string result = $"Successfully registered {testCount} tests (standard + integration).\n\n";

                    // Then run all tests
                    var tests = TestRunner.GetRegisteredTests();
                    if (tests.Count == 0)
                    {
                        return result + "No tests are registered.\n";
                    }

                    InformationManager.DisplayMessage(new InformationMessage($"Running {tests.Count} tests..."));
                    
                    var results = TestRunner.RunAllTests();
                    string report = TestRunner.GenerateReport(results);
                    
                    return result + report;
                }
                catch (Exception ex)
                {
                    return $"Error registering and running tests: {ex.Message}\n";
                }
            });
        }

        /// <summary>
        /// Show help for test commands
        /// Usage: gm.test.help
        /// </summary>
        [CommandLineFunctionality.CommandLineArgumentFunction("help", "gm.test")]
        public static string ShowHelp(List<string> args)
        {
            return "=== GAME MASTER TEST COMMANDS ===\n\n" +
                   "Test Management:\n" +
                   "  gm.test.register_standard      - Register standard validation tests\n" +
                   "  gm.test.register_integration   - Register integration tests with state validation\n" +
                   "  gm.test.register_all           - Register all tests (standard + integration)\n" +
                   "  gm.test.register_all_run       - Register all tests and run them\n" +
                   "  gm.test.list [category]        - List all registered tests (optionally filter by category)\n" +
                   "  gm.test.clear                  - Clear all registered tests\n\n" +
                   "Running Tests:\n" +
                   "  gm.test.run_all                - Run all registered tests\n" +
                   "  gm.test.run_category <cat>     - Run tests in a specific category\n" +
                   "  gm.test.run_single <id>        - Run a specific test by ID\n\n" +
                   "Results:\n" +
                   "  gm.test.last_results           - Show summary of last test run\n" +
                   "  gm.test.last_results verbose   - Show detailed results of last test run\n\n" +
                   "Usage Examples:\n" +
                   "  Quick: gm.test.register_all_run\n" +
                   "  Basic: gm.test.register_standard → gm.test.run_all → gm.test.last_results verbose\n" +
                   "  Full:  gm.test.register_all → gm.test.run_all → gm.test.last_results verbose\n";
        }
    }
}