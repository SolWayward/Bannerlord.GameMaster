using System;

namespace Bannerlord.GameMaster.Console.Testing
{
    /// <summary>
    /// Represents the result of a test execution
    /// </summary>
    public class TestResult
    {
        /// <summary>
        /// The test case that was executed
        /// </summary>
        public TestCase TestCase { get; set; }

        /// <summary>
        /// Whether the test passed
        /// </summary>
        public bool Passed { get; set; }

        /// <summary>
        /// The actual output from executing the command
        /// </summary>
        public string ActualOutput { get; set; }

        /// <summary>
        /// Error message if test failed
        /// </summary>
        public string ErrorMessage { get; set; }

        /// <summary>
        /// Exception if one occurred during test execution
        /// </summary>
        public Exception Exception { get; set; }

        /// <summary>
        /// Time taken to execute the test in milliseconds
        /// </summary>
        public long ExecutionTimeMs { get; set; }

        /// <summary>
        /// Timestamp when test was executed
        /// </summary>
        public DateTime ExecutedAt { get; set; }

        public TestResult(TestCase testCase)
        {
            TestCase = testCase;
            ExecutedAt = DateTime.Now;
        }

        /// <summary>
        /// Get a formatted summary of the test result
        /// </summary>
        public string GetSummary()
        {
            string status = Passed ? "[PASS]" : "[FAIL]";
            string message = $"{status} {TestCase.TestId}: {TestCase.Description}";
            
            if (!Passed && !string.IsNullOrEmpty(ErrorMessage))
            {
                message += $"\n  Error: {ErrorMessage}";
            }
            
            if (Exception != null)
            {
                message += $"\n  Exception: {Exception.Message}";
            }
            
            message += $"\n  Execution Time: {ExecutionTimeMs}ms";
            
            return message;
        }

        /// <summary>
        /// Get detailed test result information
        /// </summary>
        public string GetDetails()
        {
            string details = GetSummary();
            details += $"\n  Command: {TestCase.Command}";
            details += $"\n  Expected: {TestCase.Expectation}";
            
            if (!string.IsNullOrEmpty(TestCase.ExpectedText))
            {
                details += $"\n  Expected Text: {TestCase.ExpectedText}";
            }
            
            if (!string.IsNullOrEmpty(ActualOutput))
            {
                details += $"\n  Actual Output: {ActualOutput.Trim()}";
            }
            
            return details;
        }
    }
}