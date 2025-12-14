using System;
using System.Collections.Generic;

namespace Bannerlord.GameMaster.Console.Testing
{
    /// <summary>
    /// Represents a single test case for a console command
    /// </summary>
    public class TestCase
    {
        /// <summary>
        /// Unique identifier for the test
        /// </summary>
        public string TestId { get; set; }

        /// <summary>
        /// Human-readable description of what this test validates
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// The full command to execute (e.g., "gm.query.hero aserai lord")
        /// </summary>
        public string Command { get; set; }

        /// <summary>
        /// Expected result type (Success, Error, or Contains)
        /// </summary>
        public TestExpectation Expectation { get; set; }

        /// <summary>
        /// Expected text to be present in result (for Contains expectation)
        /// </summary>
        public string ExpectedText { get; set; }

        /// <summary>
        /// Text that should NOT be in result (optional validation)
        /// </summary>
        public string UnexpectedText { get; set; }

        /// <summary>
        /// Category for grouping tests (e.g., "HeroQuery", "ClanManagement")
        /// </summary>
        public string Category { get; set; }

        /// <summary>
        /// Whether this test requires a campaign to be active
        /// </summary>
        public bool RequiresCampaign { get; set; } = true;

        /// <summary>
        /// Optional setup commands to run before this test
        /// </summary>
        public List<string> SetupCommands { get; set; } = new List<string>();

        /// <summary>
        /// Optional cleanup commands to run after this test
        /// </summary>
        public List<string> CleanupCommands { get; set; } = new List<string>();

        /// <summary>
        /// Optional custom validation function that receives the command output and returns (passed, errorMessage)
        /// </summary>
        public Func<string, (bool passed, string errorMessage)> CustomValidator { get; set; }

        public TestCase(string testId, string description, string command, TestExpectation expectation)
        {
            TestId = testId;
            Description = description;
            Command = command;
            Expectation = expectation;
        }
    }

    /// <summary>
    /// Defines what to expect from a test execution
    /// </summary>
    public enum TestExpectation
    {
        /// <summary>
        /// Command should execute successfully (starts with "Success:")
        /// </summary>
        Success,

        /// <summary>
        /// Command should fail with an error (starts with "Error:")
        /// </summary>
        Error,

        /// <summary>
        /// Result should contain specific text (ExpectedText property)
        /// </summary>
        Contains,

        /// <summary>
        /// Result should not contain specific text (UnexpectedText property)
        /// </summary>
        NotContains,

        /// <summary>
        /// Just verify command runs without crashing
        /// </summary>
        NoException
    }
}