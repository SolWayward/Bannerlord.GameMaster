using System.Collections.Generic;

namespace Bannerlord.GameMaster.Console.Common.Parsing
{
    /// <summary>
    /// Represents a command argument definition for validation and display.
    /// Used to define expected arguments for console commands with support for
    /// required/optional status, default values, and aliases.
    /// </summary>
    public class ArgumentDefinition
    {
        /// <summary>
        /// The primary name of the argument used in named argument syntax (e.g., "count" for count:5)
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Whether this argument is required for the command to execute
        /// </summary>
        public bool IsRequired { get; set; }

        /// <summary>
        /// Display text shown when the argument is not provided (for usage/help messages)
        /// </summary>
        public string DefaultDisplay { get; set; }

        /// <summary>
        /// Alternative names that can be used for this argument (case-insensitive matching)
        /// </summary>
        public List<string> Aliases { get; set; } = new();

        /// <summary>
        /// Creates a new argument definition with the specified properties
        /// </summary>
        /// <param name="name">Primary name of the argument</param>
        /// <param name="isRequired">Whether the argument is required</param>
        /// <param name="defaultDisplay">Display text for default value (optional)</param>
        /// <param name="aliases">Alternative names for the argument (optional)</param>
        public ArgumentDefinition(string name, bool isRequired, string defaultDisplay = null, params string[] aliases)
        {
            Name = name;
            IsRequired = isRequired;
            DefaultDisplay = defaultDisplay;
            if (aliases != null)
                Aliases.AddRange(aliases);
        }
    }
}
