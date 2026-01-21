using System;
using System.Collections.Concurrent;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Bannerlord.GameMaster.Common;
using Bannerlord.GameMaster.Console.Common;
using Bannerlord.GameMaster.Information;
using TaleWorlds.Library;

namespace Bannerlord.GameMaster.Console.Common.Execution
{
	/// <summary>
	/// Logs console command outputs to a file with log rotation and exception handling
	/// </summary>
	public static class CommandLogger
	{
		#region Constants
		private const string LOG_FILE_PREFIX = "command_log";
		private const string LOG_FILE_EXTENSION = ".txt";
		private const int MAX_LOG_FILES = 5;
		private const int FLUSH_INTERVAL_MS = 1000;
		private const string SEPARATOR_EQUALS = "================================================================================";
		private const string SEPARATOR_DASHES = "--------------------------------------------------------------------------------";
		#endregion

		#region Fields
		private static readonly object _lockObject = new();
		private static readonly ConcurrentQueue<string> _writeQueue = new();
		private static bool _isEnabled = false;
		private static string _logFilePath;
		private static string _logDirectory;
		private static Timer _flushTimer;
		private static bool _isFlushingQueue = false;
		#endregion

		#region Properties
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
		#endregion

		#region Initialization
		/// <summary>
		/// Initialize the logger with default or custom log file path.
		/// Creates a new timestamped log file and performs log rotation.
		/// </summary>
		/// <param name="customPath">Optional custom directory path for log files</param>
		public static void Initialize(string customPath = null)
		{
			try
			{
				if (string.IsNullOrEmpty(customPath))
				{
					// Default log path in Documents/Mount and Blade II Bannerlord/Configs/GameMaster
					string documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
					_logDirectory = Path.Combine(documentsPath, "Mount and Blade II Bannerlord", "Configs", "GameMaster");
				}
				else
				{
					_logDirectory = customPath;
				}

				// Create directory if it doesn't exist
				Directory.CreateDirectory(_logDirectory);

				// Create new timestamped log file
				string timestamp = DateTime.Now.ToString("yyyy-MM-dd_HHmmss");
				string fileName = new StringBuilder()
					.Append(LOG_FILE_PREFIX)
					.Append('_')
					.Append(timestamp)
					.Append(LOG_FILE_EXTENSION)
					.ToString();

				_logFilePath = Path.Combine(_logDirectory, fileName);

				// Perform log rotation - keep only MAX_LOG_FILES most recent
				PerformLogRotation();

				// Start the flush timer for buffered writes
				_flushTimer = new(FlushQueueCallback, null, FLUSH_INTERVAL_MS, FLUSH_INTERVAL_MS);

				_isEnabled = true;
			}
			catch (Exception ex)
			{
				BLGMResult.Error($"CommandLogger failed to initialize: {ex.Message}", ex).Log();
			}
		}

		/// <summary>
		/// Shutdown the logger and flush remaining entries
		/// </summary>
		public static void Shutdown()
		{
			_isEnabled = false;
			_flushTimer?.Dispose();
			_flushTimer = null;
			FlushQueue();
		}
		#endregion

		#region Log Rotation
		/// <summary>
		/// Performs log rotation by keeping only the most recent log files
		/// </summary>
		private static void PerformLogRotation()
		{
			try
			{
				string searchPattern = new StringBuilder()
					.Append(LOG_FILE_PREFIX)
					.Append('_')
					.Append('*')
					.Append(LOG_FILE_EXTENSION)
					.ToString();

				string[] existingFiles = Directory.GetFiles(_logDirectory, searchPattern);

				if (existingFiles.Length <= MAX_LOG_FILES)
					return;

				// Sort files by creation time (oldest first) - avoid LINQ in hot path
				Array.Sort(existingFiles, (a, b) => File.GetCreationTime(a).CompareTo(File.GetCreationTime(b)));

				// Delete oldest files, keeping only MAX_LOG_FILES
				int filesToDelete = existingFiles.Length - MAX_LOG_FILES;
				for (int i = 0; i < filesToDelete; i++)
				{
					try
					{
						File.Delete(existingFiles[i]);
					}
					catch (Exception ex)
					{
						BLGMResult.Error($"Failed to delete old log file: {ex.Message}", ex).Log();
					}
				}
			}
			catch (Exception ex)
			{
				BLGMResult.Error($"Failed to perform log rotation: {ex.Message}", ex).Log();
			}
		}
		#endregion

		#region Command Logging
		/// <summary>
		/// Log a command execution (basic)
		/// </summary>
		/// <param name="command">The command that was executed</param>
		/// <param name="output">The output from the command</param>
		public static void LogCommand(string command, string output)
		{
			if (!_isEnabled || string.IsNullOrEmpty(_logFilePath))
				return;

			StringBuilder logEntry = new();
			logEntry.AppendLine(SEPARATOR_EQUALS);
			logEntry.Append("Timestamp: ").AppendLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
			logEntry.Append("Command: ").AppendLine(command);
			logEntry.AppendLine(SEPARATOR_DASHES);
			logEntry.AppendLine(output);
			logEntry.AppendLine(SEPARATOR_EQUALS);
			logEntry.AppendLine();

			EnqueueWrite(logEntry.ToString());
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

			StringBuilder logEntry = new();
			logEntry.AppendLine(SEPARATOR_EQUALS);
			logEntry.Append("Timestamp: ").AppendLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
			logEntry.Append("Command: ").AppendLine(command);
			logEntry.Append("Status: ").AppendLine(isSuccess ? "SUCCESS" : "FAILED");
			logEntry.AppendLine(SEPARATOR_DASHES);
			logEntry.AppendLine(output);
			logEntry.AppendLine(SEPARATOR_EQUALS);
			logEntry.AppendLine();

			EnqueueWrite(logEntry.ToString());
		}

		/// <summary>
		/// Log a command execution with comprehensive details including parsed arguments
		/// </summary>
		/// <param name="rawCommand">The raw command as typed by the user</param>
		/// <param name="parsedArguments">Formatted string showing parsed argument results</param>
		/// <param name="output">The output from the command</param>
		/// <param name="isSuccess">Whether the command succeeded</param>
		public static void LogCommand(string rawCommand, string parsedArguments, string output, bool isSuccess)
		{
			if (!_isEnabled || string.IsNullOrEmpty(_logFilePath))
				return;

			StringBuilder logEntry = new();
			logEntry.AppendLine(SEPARATOR_EQUALS);
			logEntry.Append("Timestamp: ").AppendLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
			logEntry.Append("Command: ").AppendLine(rawCommand);

			if (!string.IsNullOrEmpty(parsedArguments))
			{
				logEntry.AppendLine("Parsed Arguments:");
				// Indent each line of parsed arguments
				string[] argLines = parsedArguments.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
				for (int i = 0; i < argLines.Length; i++)
				{
					logEntry.Append("  ").AppendLine(argLines[i]);
				}
			}

			logEntry.Append("Status: ").AppendLine(isSuccess ? "SUCCESS" : "FAILED");
			logEntry.AppendLine(SEPARATOR_DASHES);
			logEntry.AppendLine(output);
			logEntry.AppendLine(SEPARATOR_EQUALS);
			logEntry.AppendLine();

			EnqueueWrite(logEntry.ToString());
		}
	
		/// <summary>
		/// Log a CommandResult to the custom log file.
		/// Uses the result's Message and status for formatting.
		/// Only writes if logging is enabled.
		/// </summary>
		/// <param name="result">The CommandResult to log</param>
		public static void LogCommandResult(CommandResult result)
		{
			if (!_isEnabled || string.IsNullOrEmpty(_logFilePath))
				return;
	
			StringBuilder logEntry = new();
			logEntry.AppendLine(SEPARATOR_EQUALS);
			logEntry.Append("Timestamp: ").AppendLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
			logEntry.Append("Status: ").AppendLine(result.IsSuccess ? "SUCCESS" : "FAILED");
			logEntry.AppendLine(SEPARATOR_DASHES);
			logEntry.AppendLine(result.Message);
			
			if (!result.IsSuccess && result.Exception != null)
			{
				logEntry.AppendLine("Exception Details:");
				AppendExceptionDetailsForLog(logEntry, result.Exception, 0);
			}
			
			logEntry.AppendLine(SEPARATOR_EQUALS);
			logEntry.AppendLine();
	
			EnqueueWrite(logEntry.ToString());
		}
		#endregion

		#region Exception Logging
		/// <summary>
		/// Log an exception with full stack trace to the custom log file
		/// </summary>
		/// <param name="commandName">The name of the command that caused the exception</param>
		/// <param name="ex">The exception to log</param>
		public static void LogException(string commandName, Exception ex)
		{
			if (ex == null)
				return;

			// Build the RGL log message (single [BLGM] prefix at start)
			StringBuilder rglLog = new();
			rglLog.AppendLine("[BLGM] Error:");
			rglLog.Append("Command: ").AppendLine(commandName);
			AppendExceptionDetailsForLog(rglLog, ex, 0);

			// ALWAYS write to RGL log (unconditional)
			Debug.Print(rglLog.ToString());

			// Write to custom log file (only if enabled)
			if (_isEnabled && !string.IsNullOrEmpty(_logFilePath))
			{
				StringBuilder logEntry = new();
				logEntry.AppendLine(SEPARATOR_EQUALS);
				logEntry.AppendLine("[BLGM] Error:");
				logEntry.Append("Timestamp: ").AppendLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
				logEntry.Append("Command: ").AppendLine(commandName);
				AppendExceptionDetailsForLog(logEntry, ex, 0);
				logEntry.AppendLine(SEPARATOR_DASHES);
				logEntry.AppendLine();

				EnqueueWrite(logEntry.ToString());
			}
		}

		/// <summary>
		/// Handles and logs an exception, returning a simplified error message for console display.
		/// Writes full details to both RGL log (always) and custom log file (if enabled).
		/// </summary>
		/// <param name="commandName">The name of the command that caused the exception</param>
		/// <param name="ex">The exception to handle</param>
		/// <returns>A simplified error message suitable for console display (no stack trace)</returns>
		public static string HandleAndLogException(string commandName, Exception ex)
		{
			if (ex == null)
				return "[BLGM] Error: Unknown error\n";

			// Build the log message (single [BLGM] prefix at start)
			StringBuilder logBuilder = new();
			logBuilder.AppendLine("[BLGM] Command Error:");
			logBuilder.Append("Command: ").AppendLine(commandName);
			AppendExceptionDetailsForLog(logBuilder, ex, 0);

			string fullLogMessage = logBuilder.ToString();

			// ALWAYS write to RGL log (unconditional)
			Debug.Print(fullLogMessage);
			SystemConsoleManager.WriteLog(fullLogMessage);

			// CONDITIONALLY write to custom file (only if enabled)
			if (_isEnabled && !string.IsNullOrEmpty(_logFilePath))
			{
				StringBuilder fileEntry = new();
				fileEntry.AppendLine(SEPARATOR_EQUALS);
				fileEntry.AppendLine("[BLGM] Error:");
				fileEntry.Append("Timestamp: ").AppendLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
				fileEntry.Append("Command: ").AppendLine(commandName);
				AppendExceptionDetailsForLog(fileEntry, ex, 0);
				fileEntry.AppendLine(SEPARATOR_DASHES);
				fileEntry.AppendLine();

				EnqueueWrite(fileEntry.ToString());
			}

			// Return simplified message for console display (no stack trace)
			return $"[BLGM] Error: {ex.Message}\n";
		}

		/// <summary>
		/// Appends exception details for log entries (no [BLGM] prefix - added at entry level)
		/// </summary>
		private static void AppendExceptionDetailsForLog(StringBuilder sb, Exception ex, int level)
		{
			string indent = level > 0 ? new string(' ', level * 2) : string.Empty;

			if (level == 0)
			{
				sb.Append("Exception: ").AppendLine(ex.Message);
			}
			else
			{
				sb.Append(indent).Append("Inner Exception ").Append(level).Append(": ").AppendLine(ex.Message);
			}

			sb.Append(indent).AppendLine("Stack Trace:");

			if (!string.IsNullOrEmpty(ex.StackTrace))
			{
				string[] stackLines = ex.StackTrace.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
				for (int i = 0; i < stackLines.Length; i++)
				{
					sb.Append(indent).Append("   ").AppendLine(stackLines[i].TrimStart());
				}
			}
			else
			{
				sb.Append(indent).AppendLine("   (No stack trace available)");
			}

			// Recursively handle inner exceptions
			if (ex.InnerException != null)
			{
				sb.AppendLine();
				AppendExceptionDetailsForLog(sb, ex.InnerException, level + 1);
			}
		}
		#endregion

		#region Buffered Writing
		/// <summary>
		/// Enqueue a log entry for buffered writing
		/// </summary>
		private static void EnqueueWrite(string logEntry)
		{
			_writeQueue.Enqueue(logEntry);
		}

		/// <summary>
		/// Timer callback for flushing the write queue
		/// </summary>
		private static void FlushQueueCallback(object state)
		{
			FlushQueue();
		}

		/// <summary>
		/// Flush all queued log entries to file
		/// </summary>
		private static void FlushQueue()
		{
			if (_isFlushingQueue || _writeQueue.IsEmpty || string.IsNullOrEmpty(_logFilePath))
				return;

			lock (_lockObject)
			{
				if (_isFlushingQueue || _writeQueue.IsEmpty)
					return;

				_isFlushingQueue = true;

				try
				{
					StringBuilder batchContent = new();
					while (_writeQueue.TryDequeue(out string entry))
					{
						batchContent.Append(entry);
					}

					if (batchContent.Length > 0)
					{
						File.AppendAllText(_logFilePath, batchContent.ToString());
					}
				}
				
				catch (Exception ex)
				{
					BLGMResult.Error($"Failed to flush log queue: {ex.Message}", ex).Log();
				}

				finally
				{
					_isFlushingQueue = false;
				}
			}
		}

		/// <summary>
		/// Force an immediate flush of all queued entries (synchronous)
		/// </summary>
		public static void ForceFlush()
		{
			FlushQueue();
		}
		#endregion

		#region Utility Methods
		/// <summary>
		/// Clear the current log file
		/// </summary>
		public static void ClearLog()
		{
			if (string.IsNullOrEmpty(_logFilePath))
				return;

			try
			{
				lock (_lockObject)
				{
					// Clear the queue first
					while (_writeQueue.TryDequeue(out _)) { }

					if (File.Exists(_logFilePath))
					{
						File.WriteAllText(_logFilePath, string.Empty);
					}
				}
			}
			catch (Exception ex)
			{
				throw new InvalidOperationException(
					new StringBuilder()
						.Append("Failed to clear log file: ")
						.Append(ex.Message)
						.ToString(),
					ex);
			}
		}

		/// <summary>
		/// Get the size of the current log file in bytes
		/// </summary>
		/// <returns>The size of the log file in bytes, or 0 if the file does not exist</returns>
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
		/// Get the number of entries in the current log file
		/// </summary>
		/// <returns>The count of log entries in the file</returns>
		public static int GetLogEntryCount()
		{
			if (string.IsNullOrEmpty(_logFilePath) || !File.Exists(_logFilePath))
				return 0;

			try
			{
				// Flush pending entries first
				FlushQueue();

				string content = File.ReadAllText(_logFilePath);
				// Count separator lines as proxy for entries
				int count = 0;
				int index = 0;
				while ((index = content.IndexOf(SEPARATOR_EQUALS, index, StringComparison.Ordinal)) != -1)
				{
					count++;
					index += SEPARATOR_EQUALS.Length;
				}
				// Each entry has two separators (start and end), so divide by 2
				return count / 2;
			}
			catch
			{
				return 0;
			}
		}

		/// <summary>
		/// Get the count of existing log files in the log directory
		/// </summary>
		/// <returns>The number of log files</returns>
		public static int GetLogFileCount()
		{
			if (string.IsNullOrEmpty(_logDirectory) || !Directory.Exists(_logDirectory))
				return 0;

			try
			{
				string searchPattern = new StringBuilder()
					.Append(LOG_FILE_PREFIX)
					.Append('_')
					.Append('*')
					.Append(LOG_FILE_EXTENSION)
					.ToString();

				return Directory.GetFiles(_logDirectory, searchPattern).Length;
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

			StringBuilder header = new();
			header.AppendLine();
			header.Append('#', 80).AppendLine();
			header.Append("# NEW SESSION STARTED: ").AppendLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
			header.Append('#', 80).AppendLine();
			header.AppendLine();

			EnqueueWrite(header.ToString());
		}

		/// <summary>
		/// Log a debug message directly (for debugging purposes)
		/// </summary>
		/// <param name="message">The message to log</param>
		public static void Log(string message)
		{
			if (!_isEnabled || string.IsNullOrEmpty(_logFilePath))
				return;

			StringBuilder entry = new();
			entry.Append('[').Append(DateTime.Now.ToString("HH:mm:ss")).Append("] ").AppendLine(message);

			EnqueueWrite(entry.ToString());
		}
		#endregion
	}
}
