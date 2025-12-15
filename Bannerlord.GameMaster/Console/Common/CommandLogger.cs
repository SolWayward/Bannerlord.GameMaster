using System;
using System.IO;
using System.Text;
using TaleWorlds.Library;

namespace Bannerlord.GameMaster.Console.Common
{
    /// <summary>
    /// Logs console command outputs to a file
    /// </summary>
    public static class CommandLogger
    {
        private static readonly object _lockObject = new object();
        private static bool _isEnabled = false;
        private static string _logFilePath;
        
        /// <summary>
        /// Gets or sets whether logging is enabled
        /// </summary>
        public static bool IsEnabled
        {
            get => _isEnabled;
            set => _isEnabled = value;
        }

        /// <summary>
        /// Gets the current log file path
        /// </summary>
        public static string LogFilePath => _logFilePath;

        /// <summary>
        /// Initialize the logger with default or custom log file path
        /// </summary>
        /// <param name="customPath">Optional custom path for the log file</param>
        public static void Initialize(string customPath = null)
        {
            try
            {
                if (string.IsNullOrEmpty(customPath))
                {
                    // Default log path in Documents/Mount and Blade II Bannerlord/Configs/GameMaster
                    string documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                    string bannerlordPath = Path.Combine(documentsPath, "Mount and Blade II Bannerlord", "Configs", "GameMaster");
                    
                    // Create directory if it doesn't exist
                    Directory.CreateDirectory(bannerlordPath);
                    
                    _logFilePath = Path.Combine(bannerlordPath, "command_log.txt");
                }
                else
                {
                    // Ensure directory exists for custom path
                    string directory = Path.GetDirectoryName(customPath);
                    if (!string.IsNullOrEmpty(directory))
                    {
                        Directory.CreateDirectory(directory);
                    }
                    _logFilePath = customPath;
                }
            }
            catch (Exception ex)
            {
                InformationManager.DisplayMessage(new InformationMessage(
                    $"[CommandLogger] Failed to initialize: {ex.Message}",
                    new Color(1.0f, 0.2f, 0.2f)));
            }
        }

        /// <summary>
        /// Log a command execution
        /// </summary>
        /// <param name="command">The command that was executed</param>
        /// <param name="output">The output from the command</param>
        public static void LogCommand(string command, string output)
        {
            if (!_isEnabled || string.IsNullOrEmpty(_logFilePath))
                return;

            try
            {
                lock (_lockObject)
                {
                    var logEntry = new StringBuilder();
                    logEntry.AppendLine("=".PadRight(80, '='));
                    logEntry.AppendLine($"Timestamp: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
                    logEntry.AppendLine($"Command: {command}");
                    logEntry.AppendLine("-".PadRight(80, '-'));
                    logEntry.AppendLine(output);
                    logEntry.AppendLine();

                    File.AppendAllText(_logFilePath, logEntry.ToString());
                }
            }
            catch (Exception ex)
            {
                // Silently fail to avoid disrupting command execution
                // Could optionally log to game's debug log
                Debug.Print($"[CommandLogger] Failed to write log: {ex.Message}");
            }
        }

        /// <summary>
        /// Log a command execution with result status
        /// </summary>
        /// <param name="command">The command that was executed</param>
        /// <param name="output">The output from the command</param>
        /// <param name="isSuccess">Whether the command succeeded</param>
        public static void LogCommand(string command, string output, bool isSuccess)
        {
            if (!_isEnabled || string.IsNullOrEmpty(_logFilePath))
                return;

            try
            {
                lock (_lockObject)
                {
                    var logEntry = new StringBuilder();
                    logEntry.AppendLine("=".PadRight(80, '='));
                    logEntry.AppendLine($"Timestamp: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
                    logEntry.AppendLine($"Command: {command}");
                    logEntry.AppendLine($"Status: {(isSuccess ? "SUCCESS" : "FAILED")}");
                    logEntry.AppendLine("-".PadRight(80, '-'));
                    logEntry.AppendLine(output);
                    logEntry.AppendLine();

                    File.AppendAllText(_logFilePath, logEntry.ToString());
                }
            }
            catch (Exception ex)
            {
                Debug.Print($"[CommandLogger] Failed to write log: {ex.Message}");
            }
        }

        /// <summary>
        /// Clear the log file
        /// </summary>
        public static void ClearLog()
        {
            if (string.IsNullOrEmpty(_logFilePath))
                return;

            try
            {
                lock (_lockObject)
                {
                    if (File.Exists(_logFilePath))
                    {
                        File.WriteAllText(_logFilePath, string.Empty);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to clear log file: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Get the size of the log file in bytes
        /// </summary>
        public static long GetLogFileSize()
        {
            if (string.IsNullOrEmpty(_logFilePath) || !File.Exists(_logFilePath))
                return 0;

            try
            {
                return new FileInfo(_logFilePath).Length;
            }
            catch
            {
                return 0;
            }
        }

        /// <summary>
        /// Get the number of entries in the log file
        /// </summary>
        public static int GetLogEntryCount()
        {
            if (string.IsNullOrEmpty(_logFilePath) || !File.Exists(_logFilePath))
                return 0;

            try
            {
                string content = File.ReadAllText(_logFilePath);
                // Count separator lines as proxy for entries
                int count = 0;
                int index = 0;
                string separator = "=".PadRight(80, '=');
                while ((index = content.IndexOf(separator, index)) != -1)
                {
                    count++;
                    index += separator.Length;
                }
                return count;
            }
            catch
            {
                return 0;
            }
        }

        /// <summary>
        /// Write a session header to the log
        /// </summary>
        public static void LogSessionStart()
        {
            if (!_isEnabled || string.IsNullOrEmpty(_logFilePath))
                return;

            try
            {
                lock (_lockObject)
                {
                    var header = new StringBuilder();
                    header.AppendLine();
                    header.AppendLine("#".PadRight(80, '#'));
                    header.AppendLine($"# NEW SESSION STARTED: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
                    header.AppendLine("#".PadRight(80, '#'));
                    header.AppendLine();

                    File.AppendAllText(_logFilePath, header.ToString());
                }
            }
            catch (Exception ex)
            {
                Debug.Print($"[CommandLogger] Failed to write session header: {ex.Message}");
            }
        }

        /// <summary>
        /// Log a debug message directly (for debugging purposes)
        /// </summary>
        /// <param name="message">The message to log</param>
        public static void Log(string message)
        {
            if (!_isEnabled || string.IsNullOrEmpty(_logFilePath))
                return;

            try
            {
                lock (_lockObject)
                {
                    File.AppendAllText(_logFilePath, message + Environment.NewLine);
                }
            }
            catch (Exception ex)
            {
                Debug.Print($"[CommandLogger] Failed to write debug log: {ex.Message}");
            }
        }
    }
}