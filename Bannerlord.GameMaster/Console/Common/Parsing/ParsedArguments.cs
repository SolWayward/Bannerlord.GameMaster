using System;
using System.Collections.Generic;
using System.Linq;

namespace Bannerlord.GameMaster.Console.Common.Parsing
{
    /// <summary>
    /// Parses and stores command arguments, supporting both positional and named arguments.
    /// Named arguments use format argName:argContent (no spaces around colon).
    /// Example: count:5 name:'Sir Galahad' culture:vlandia
    /// </summary>
    public class ParsedArguments
    {
        private readonly Dictionary<string, string> _namedArgs = new(StringComparer.OrdinalIgnoreCase);
        private readonly List<string> _positionalArgs = new();
        private readonly List<string> _allArgs = new();
        private readonly List<string> _unknownNamedArgs = new();
        private List<ArgumentDefinition> _validArguments;

        /// <summary>
        /// Creates a new ParsedArguments instance from a list of string arguments.
        /// Automatically separates named arguments (containing ':') from positional arguments.
        /// </summary>
        /// <param name="args">Raw argument list after quote parsing</param>
        public ParsedArguments(List<string> args)
        {
            if (args == null || args.Count == 0)
                return;

            // MARK: Process each argument
            foreach (string arg in args)
            {
                _allArgs.Add(arg);

                // Check if this is a named argument (contains : without spaces)
                int colonIndex = arg.IndexOf(':');
                if (colonIndex > 0 && colonIndex < arg.Length - 1)
                {
                    string name = arg.Substring(0, colonIndex).Trim();
                    string value = arg.Substring(colonIndex + 1);

                    // Only treat as named argument if name doesn't contain spaces
                    if (!name.Contains(" "))
                    {
                        _namedArgs[name] = value;
                        continue;
                    }
                }

                // Not a named argument, treat as positional
                _positionalArgs.Add(arg);
            }
        }

        #region Argument Definition and Validation

        /// <summary>
        /// Sets valid argument definitions for this command and validates all named arguments.
        /// Call this after creating ParsedArguments to enable validation.
        /// </summary>
        /// <param name="definitions">Array of ArgumentDefinition objects defining valid arguments</param>
        public void SetValidArguments(params ArgumentDefinition[] definitions)
        {
            _validArguments = new List<ArgumentDefinition>(definitions);
            ValidateNamedArguments();
        }

        /// <summary>
        /// Validates that all named arguments match defined argument names (case-insensitive).
        /// Populates _unknownNamedArgs with any unrecognized argument names.
        /// </summary>
        private void ValidateNamedArguments()
        {
            if (_validArguments == null || _validArguments.Count == 0)
                return;

            _unknownNamedArgs.Clear();

            foreach (string namedArgKey in _namedArgs.Keys)
            {
                bool found = false;
                foreach (ArgumentDefinition def in _validArguments)
                {
                    if (def.Name.Equals(namedArgKey, StringComparison.OrdinalIgnoreCase))
                    {
                        found = true;
                        break;
                    }
                    // Check aliases
                    foreach (string alias in def.Aliases)
                    {
                        if (alias.Equals(namedArgKey, StringComparison.OrdinalIgnoreCase))
                        {
                            found = true;
                            break;
                        }
                    }
                    if (found) break;
                }

                if (!found)
                    _unknownNamedArgs.Add(namedArgKey);
            }
        }

        /// <summary>
        /// Gets validation error if unknown named arguments were found.
        /// Returns null if all named arguments are valid.
        /// </summary>
        /// <returns>Error message string or null if validation passed</returns>
        public string GetValidationError()
        {
            if (_unknownNamedArgs.Count == 0)
                return null;

            string validNames = _validArguments != null
                ? string.Join(", ", _validArguments.Select(a => a.Name + (a.Aliases.Count > 0 ? "/" + string.Join("/", a.Aliases) : "")))
                : "none defined";

            return $"Unknown named argument(s): {string.Join(", ", _unknownNamedArgs)}\nValid argument names: {validNames}";
        }

        #endregion

        #region Argument Display

        /// <summary>
        /// Formats the argument display header showing all argument values.
        /// Used for command output to show what arguments were parsed.
        /// </summary>
        /// <param name="commandName">Name of the command being executed</param>
        /// <param name="resolvedValues">Dictionary of argument names to their resolved display values</param>
        /// <returns>Formatted string showing command with all argument values</returns>
        public string FormatArgumentDisplay(string commandName, Dictionary<string, string> resolvedValues)
        {
            if (_validArguments == null || _validArguments.Count == 0)
                return string.Empty;

            List<string> parts = new();

            foreach (ArgumentDefinition def in _validArguments)
            {
                string displayValue = resolvedValues.ContainsKey(def.Name)
                    ? resolvedValues[def.Name]
                    : def.DefaultDisplay ?? "Not specified";

                if (def.IsRequired)
                    parts.Add($"<{def.Name}: {displayValue}>");
                else
                    parts.Add($"[{def.Name}: {displayValue}]");
            }

            return $"{commandName} {string.Join(" ", parts)}\n";
        }

        #endregion

        #region Named Argument Getters

        /// <summary>
        /// Gets argument by name only, returns null if not found.
        /// </summary>
        /// <param name="name">Argument name (case-insensitive)</param>
        /// <returns>Argument value or null</returns>
        public string GetNamed(string name)
        {
            return _namedArgs.TryGetValue(name, out string value) ? value : null;
        }

        /// <summary>
        /// Gets argument by name or falls back to positional index.
        /// Useful when supporting both named and positional argument styles.
        /// </summary>
        /// <param name="name">Argument name to try first (case-insensitive)</param>
        /// <param name="positionalIndex">Positional index to fall back to</param>
        /// <returns>Argument value or null if not found by either method</returns>
        public string GetArgument(string name, int positionalIndex)
        {
            // Try named first
            if (_namedArgs.TryGetValue(name, out string value))
                return value;

            // Fall back to positional
            if (positionalIndex >= 0 && positionalIndex < _positionalArgs.Count)
                return _positionalArgs[positionalIndex];

            return null;
        }

        /// <summary>
        /// Gets a string argument by name or positional index with optional default.
        /// </summary>
        /// <param name="name">Argument name (case-insensitive)</param>
        /// <param name="positionalIndex">Positional index fallback</param>
        /// <param name="defaultValue">Default value if not found</param>
        /// <returns>Argument value or default</returns>
        public string GetString(string name, int positionalIndex, string defaultValue = null)
        {
            string value = GetArgument(name, positionalIndex);
            return value ?? defaultValue;
        }

        /// <summary>
        /// Gets an integer argument by name or positional index.
        /// </summary>
        /// <param name="name">Argument name (case-insensitive)</param>
        /// <param name="positionalIndex">Positional index fallback</param>
        /// <param name="defaultValue">Default value if not found or invalid</param>
        /// <returns>Parsed integer value or default</returns>
        public int GetInt(string name, int positionalIndex, int defaultValue = 0)
        {
            string value = GetArgument(name, positionalIndex);
            if (string.IsNullOrEmpty(value))
                return defaultValue;

            return int.TryParse(value, out int result) ? result : defaultValue;
        }

        /// <summary>
        /// Gets a float argument by name or positional index.
        /// </summary>
        /// <param name="name">Argument name (case-insensitive)</param>
        /// <param name="positionalIndex">Positional index fallback</param>
        /// <param name="defaultValue">Default value if not found or invalid</param>
        /// <returns>Parsed float value or default</returns>
        public float GetFloat(string name, int positionalIndex, float defaultValue = 0f)
        {
            string value = GetArgument(name, positionalIndex);
            if (string.IsNullOrEmpty(value))
                return defaultValue;

            return float.TryParse(value, out float result) ? result : defaultValue;
        }

        /// <summary>
        /// Gets a boolean argument by name or positional index.
        /// Accepts: true/false, yes/no, 1/0, on/off
        /// </summary>
        /// <param name="name">Argument name (case-insensitive)</param>
        /// <param name="positionalIndex">Positional index fallback</param>
        /// <param name="defaultValue">Default value if not found or invalid</param>
        /// <returns>Parsed boolean value or default</returns>
        public bool GetBool(string name, int positionalIndex, bool defaultValue = false)
        {
            string value = GetArgument(name, positionalIndex);
            if (string.IsNullOrEmpty(value))
                return defaultValue;

            string lowerValue = value.ToLowerInvariant();
            if (lowerValue == "true" || lowerValue == "yes" || lowerValue == "1" || lowerValue == "on")
                return true;
            if (lowerValue == "false" || lowerValue == "no" || lowerValue == "0" || lowerValue == "off")
                return false;

            return defaultValue;
        }

        /// <summary>
        /// Checks if a named argument exists (regardless of value).
        /// </summary>
        /// <param name="name">Argument name (case-insensitive)</param>
        /// <returns>True if the named argument was provided</returns>
        public bool HasNamed(string name)
        {
            return _namedArgs.ContainsKey(name);
        }

        /// <summary>
        /// Checks if an argument exists by name or at the specified positional index.
        /// </summary>
        /// <param name="name">Argument name (case-insensitive)</param>
        /// <param name="positionalIndex">Positional index fallback</param>
        /// <returns>True if the argument exists</returns>
        public bool HasArgument(string name, int positionalIndex)
        {
            return GetArgument(name, positionalIndex) != null;
        }

        #endregion

        #region Positional Argument Getters

        /// <summary>
        /// Gets positional argument at index.
        /// </summary>
        /// <param name="index">Zero-based index in positional argument list</param>
        /// <returns>Argument value or null if index out of range</returns>
        public string GetPositional(int index)
        {
            return index >= 0 && index < _positionalArgs.Count ? _positionalArgs[index] : null;
        }

        /// <summary>
        /// Gets all positional arguments as a new list.
        /// </summary>
        /// <returns>Copy of the positional arguments list</returns>
        public List<string> GetAllPositional() => new(_positionalArgs);

        #endregion

        #region Count Properties

        /// <summary>
        /// Gets the count of positional arguments.
        /// </summary>
        public int PositionalCount => _positionalArgs.Count;

        /// <summary>
        /// Gets the count of named arguments.
        /// </summary>
        public int NamedCount => _namedArgs.Count;

        /// <summary>
        /// Gets the total count of all arguments (both named and positional).
        /// </summary>
        public int TotalCount => _allArgs.Count;

        #endregion

        #region Collection Accessors

        /// <summary>
        /// Gets all named argument names.
        /// </summary>
        /// <returns>Enumerable of argument names</returns>
        public IEnumerable<string> GetNamedArgumentNames() => _namedArgs.Keys;

        /// <summary>
        /// Gets all valid argument definitions (set via SetValidArguments).
        /// </summary>
        /// <returns>List of ArgumentDefinition objects or null if not set</returns>
        public List<ArgumentDefinition> GetValidArguments() => _validArguments;

        #endregion
    }
}
