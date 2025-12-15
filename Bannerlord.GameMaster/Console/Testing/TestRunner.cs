using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Library;

namespace Bannerlord.GameMaster.Console.Testing
{
    /// <summary>
    /// Executes and validates console command tests
    /// </summary>
    public static class TestRunner
    {
        private static List<TestCase> _registeredTests = new List<TestCase>();
        private static List<TestResult> _lastRunResults = new List<TestResult>();

        /// <summary>
        /// Register a test case to be run
        /// </summary>
        public static void RegisterTest(TestCase testCase)
        {
            _registeredTests.Add(testCase);
        }

        /// <summary>
        /// Register multiple test cases
        /// </summary>
        public static void RegisterTests(IEnumerable<TestCase> testCases)
        {
            _registeredTests.AddRange(testCases);
        }

        /// <summary>
        /// Get all registered tests
        /// </summary>
        public static List<TestCase> GetRegisteredTests()
        {
            return _registeredTests;
        }

        /// <summary>
        /// Get results from last test run
        /// </summary>
        public static List<TestResult> GetLastResults()
        {
            return _lastRunResults;
        }

        /// <summary>
        /// Clear all registered tests
        /// </summary>
        public static void ClearTests()
        {
            _registeredTests.Clear();
        }

        /// <summary>
        /// Run all registered tests
        /// </summary>
        public static List<TestResult> RunAllTests()
        {
            return RunTests(_registeredTests);
        }

        /// <summary>
        /// Run tests in a specific category
        /// </summary>
        public static List<TestResult> RunTestsByCategory(string category)
        {
            var tests = _registeredTests.Where(t => 
                string.Equals(t.Category, category, StringComparison.OrdinalIgnoreCase)).ToList();
            return RunTests(tests);
        }

        /// <summary>
        /// Run a specific test by ID
        /// </summary>
        public static TestResult RunTestById(string testId)
        {
            var test = _registeredTests.FirstOrDefault(t => 
                string.Equals(t.TestId, testId, StringComparison.OrdinalIgnoreCase));
            
            if (test == null)
                return null;

            var results = RunTests(new List<TestCase> { test });
            return results.FirstOrDefault();
        }

        /// <summary>
        /// Run a collection of tests
        /// </summary>
        public static List<TestResult> RunTests(List<TestCase> tests)
        {
            _lastRunResults.Clear();

            foreach (var test in tests)
            {
                var result = ExecuteTest(test);
                _lastRunResults.Add(result);
            }

            // Automatically save test results to file
            SaveTestResultsToFile();

            return _lastRunResults;
        }

        /// <summary>
        /// Execute a single test case
        /// </summary>
        private static TestResult ExecuteTest(TestCase test)
        {
            var result = new TestResult(test);
            var stopwatch = Stopwatch.StartNew();

            try
            {
                // Validate campaign requirement
                if (test.RequiresCampaign && Campaign.Current == null)
                {
                    result.Passed = false;
                    result.ErrorMessage = "Test requires campaign mode but no campaign is active";
                    result.ExecutionTimeMs = stopwatch.ElapsedMilliseconds;
                    return result;
                }

                // Run setup commands
                foreach (var setupCmd in test.SetupCommands)
                {
                    ExecuteConsoleCommand(setupCmd);
                }

                // Execute the test command
                result.ActualOutput = ExecuteConsoleCommand(test.Command);

                // Validate result
                result.Passed = ValidateTestResult(test, result.ActualOutput, out string validationError);
                result.ErrorMessage = validationError;

                // Run cleanup commands
                foreach (var cleanupCmd in test.CleanupCommands)
                {
                    ExecuteConsoleCommand(cleanupCmd);
                }
            }
            catch (Exception ex)
            {
                result.Passed = false;
                result.Exception = ex;
                result.ErrorMessage = $"Exception during test execution: {ex.Message}";
            }

            stopwatch.Stop();
            result.ExecutionTimeMs = stopwatch.ElapsedMilliseconds;
            return result;
        }

        /// <summary>
        /// Execute a console command and return its output
        /// </summary>
        private static string ExecuteConsoleCommand(string fullCommand)
        {
            if (string.IsNullOrWhiteSpace(fullCommand))
                return string.Empty;

            // Parse command into parts (e.g., "gm.query.hero aserai lord")
            var parts = fullCommand.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 0)
                return string.Empty;

            // Parse the command path (e.g., "gm.query.hero" -> ["gm", "query", "hero"])
            var commandPath = parts[0].Split('.');
            if (commandPath.Length < 2)
                return "Error: Invalid command format. Expected format: module.command or module.group.command\n";

            // Get arguments
            var args = parts.Skip(1).ToList();

            // Find and execute the command using reflection
            return FindAndExecuteCommand(commandPath, args);
        }

        /// <summary>
        /// Find and execute a command using reflection
        /// </summary>
        private static string FindAndExecuteCommand(string[] commandPath, List<string> args)
        {
            try
            {
                // Get all types in the current assembly
                var assembly = Assembly.GetExecutingAssembly();
                var types = assembly.GetTypes();

                // Look for command classes with CommandLineArgumentFunction attributes
                foreach (var type in types)
                {
                    var methods = type.GetMethods(BindingFlags.Public | BindingFlags.Static);
                    
                    foreach (var method in methods)
                    {
                        // Get all attributes - we'll check the type name dynamically
                        var attributes = method.GetCustomAttributes(false);
                        
                        foreach (var attribute in attributes)
                        {
                            var attrType = attribute.GetType();
                            if (attrType.Name != "CommandLineArgumentFunction") continue;
                            
                            // Check if this matches our command path
                            if (MatchesCommandPath(attribute, commandPath))
                            {
                                // Invoke the method
                                var result = method.Invoke(null, new object[] { args });
                                return result?.ToString() ?? string.Empty;
                            }
                        }
                    }
                }

                return $"Error: Command '{string.Join(".", commandPath)}' not found\n";
            }
            catch (Exception ex)
            {
                return $"Error executing command: {ex.InnerException?.Message ?? ex.Message}\n";
            }
        }

        /// <summary>
        /// Check if command attribute matches the command path
        /// </summary>
        private static bool MatchesCommandPath(object attr, string[] commandPath)
        {
            // The attribute stores the command name and group name
            // We need to use reflection to get these values
            var nameField = attr.GetType().GetField("Name", BindingFlags.Public | BindingFlags.Instance);
            var groupField = attr.GetType().GetField("GroupName", BindingFlags.Public | BindingFlags.Instance);
            
            string commandName = nameField?.GetValue(attr) as string;
            string groupName = groupField?.GetValue(attr) as string;
            
            // Build the full command string from attribute
            string fullCommand = string.IsNullOrEmpty(groupName)
                ? commandName
                : $"{groupName}.{commandName}";

            // Compare with the command path
            string searchCommand = string.Join(".", commandPath);
            return string.Equals(fullCommand, searchCommand, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Validate test result against expectations
        /// </summary>
        private static bool ValidateTestResult(TestCase test, string actualOutput, out string errorMessage)
        {
            errorMessage = null;

            // Allow empty output if test has CustomValidator and expects NoException
            // (e.g., cleanup tests that only run in CustomValidator)
            bool allowEmptyOutput = test.CustomValidator != null && test.Expectation == TestExpectation.NoException;

            if (string.IsNullOrEmpty(actualOutput) && !allowEmptyOutput)
            {
                errorMessage = "Command produced no output";
                return false;
            }

            // First run custom validator if provided
            if (test.CustomValidator != null)
            {
                try
                {
                    var (passed, customError) = test.CustomValidator(actualOutput);
                    if (!passed)
                    {
                        errorMessage = customError ?? "Custom validation failed";
                        return false;
                    }
                }
                catch (Exception ex)
                {
                    errorMessage = $"Custom validator threw exception: {ex.Message}";
                    return false;
                }
            }

            // Then run standard validation
            switch (test.Expectation)
            {
                case TestExpectation.Success:
                    if (!actualOutput.StartsWith("Success:", StringComparison.OrdinalIgnoreCase))
                    {
                        errorMessage = "Expected success but got: " + actualOutput.Substring(0, Math.Min(100, actualOutput.Length));
                        return false;
                    }
                    break;

                case TestExpectation.Error:
                    if (!actualOutput.StartsWith("Error:", StringComparison.OrdinalIgnoreCase))
                    {
                        errorMessage = "Expected error but got: " + actualOutput.Substring(0, Math.Min(100, actualOutput.Length));
                        return false;
                    }
                    break;

                case TestExpectation.Contains:
                    if (string.IsNullOrEmpty(test.ExpectedText))
                    {
                        errorMessage = "ExpectedText is required for Contains expectation";
                        return false;
                    }
                    if (!actualOutput.Contains(test.ExpectedText))
                    {
                        errorMessage = $"Expected output to contain '{test.ExpectedText}' but it didn't";
                        return false;
                    }
                    break;

                case TestExpectation.NotContains:
                    if (string.IsNullOrEmpty(test.UnexpectedText))
                    {
                        errorMessage = "UnexpectedText is required for NotContains expectation";
                        return false;
                    }
                    if (actualOutput.Contains(test.UnexpectedText))
                    {
                        errorMessage = $"Output should not contain '{test.UnexpectedText}' but it did";
                        return false;
                    }
                    break;

                case TestExpectation.NoException:
                    // If we got here, no exception occurred
                    break;
            }

            // Check for unexpected text (optional additional validation)
            if (!string.IsNullOrEmpty(test.UnexpectedText) && actualOutput.Contains(test.UnexpectedText))
            {
                errorMessage = $"Output contains unexpected text: '{test.UnexpectedText}'";
                return false;
            }

            return true;
        }

        /// <summary>
        /// Save test results to file in the GameMaster configs directory
        /// </summary>
        private static void SaveTestResultsToFile()
        {
            try
            {
                // Generate the verbose report
                string report = GenerateReport(_lastRunResults);

                // Build the file path
                string documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                string directoryPath = Path.Combine(documentsPath, "Mount and Blade II Bannerlord", "Configs", "GameMaster");
                string filePath = Path.Combine(directoryPath, "test-results.txt");

                // Create directory if it doesn't exist
                Directory.CreateDirectory(directoryPath);

                // Create timestamp header
                string timestamp = $"=== Test Results Generated: {DateTime.Now:yyyy-MM-dd HH:mm:ss} ==={Environment.NewLine}{Environment.NewLine}";

                // Write to file with timestamp header
                File.WriteAllText(filePath, timestamp + report);
            }
            catch (Exception ex)
            {
                // Log error but don't throw - we don't want to break test execution
                TaleWorlds.Library.Debug.Print($"[TestRunner] Failed to save test results to file: {ex.Message}");
            }
        }

        /// <summary>
        /// Generate a summary report of test results
        /// </summary>
        public static string GenerateReport(List<TestResult> results)
        {
            if (results == null || results.Count == 0)
                return "No test results to report.\n";

            int passed = results.Count(r => r.Passed);
            int failed = results.Count(r => !r.Passed);
            double totalTime = results.Sum(r => r.ExecutionTimeMs);

            string report = "=== TEST RESULTS ===\n";
            report += $"Total Tests: {results.Count}\n";
            report += $"Passed: {passed}\n";
            report += $"Failed: {failed}\n";
            report += $"Success Rate: {(passed * 100.0 / results.Count):F1}%\n";
            report += $"Total Time: {totalTime}ms\n\n";

            // Group by category
            var byCategory = results.GroupBy(r => r.TestCase.Category ?? "Uncategorized");
            foreach (var category in byCategory)
            {
                report += $"--- {category.Key} ---\n";
                foreach (var result in category)
                {
                    report += result.GetSummary() + "\n";
                }
                report += "\n";
            }

            // Show failed tests details
            var failedTests = results.Where(r => !r.Passed).ToList();
            if (failedTests.Count > 0)
            {
                report += "=== FAILED TESTS DETAILS ===\n";
                foreach (var result in failedTests)
                {
                    report += result.GetDetails() + "\n\n";
                }
            }

            return report;
        }
    }
}